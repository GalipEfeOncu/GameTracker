using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameTracker.Models;
using Newtonsoft.Json.Linq;

namespace GameTracker.Services;

/// <summary>IGDB detay omurgası + RAWG platform/mağaza/gereksinim/Metacritic tamamlayıcı.</summary>
public sealed class HybridGameDetailService
{
    private readonly IgdbApiService _igdb;
    private readonly RawgApiService _rawg;

    public HybridGameDetailService(IgdbApiService igdb, RawgApiService rawg)
    {
        _igdb = igdb;
        _rawg = rawg;
    }

    public async Task<Game?> GetDetailAsync(int igdbGameId, CancellationToken ct = default)
    {
        var token = await _igdb.GetGameJsonAsync(igdbGameId, ct).ConfigureAwait(false);
        if (token == null) return null;

        if (IgdbGameMapper.IsLikelyNsfw(token, showNsfw: false))
            return null;

        var game = IgdbGameMapper.FromDetailToken(token);
        var agg = token["aggregated_rating"]?.Value<double?>();
        var aggCount = token["aggregated_rating_count"]?.Value<int?>();
        var total = token["total_rating"]?.Value<double?>();

        var ttb = await _igdb.GetGameTimeToBeatJsonAsync(igdbGameId, ct).ConfigureAwait(false);
        IgdbGameMapper.ApplyTimeToBeat(game, ttb);

        var externalArr = await _igdb.GetExternalGamesForGameAsync(igdbGameId, ct).ConfigureAwait(false);
        var igdbStores = IgdbGameMapper.StoresFromExternalGames(externalArr);

        var (rawgId, rawgStoreMatchTrust) = await TryResolveRawgIdAsync(game.Name, token, ct).ConfigureAwait(false);
        game.RawgId = rawgId;

        int? rawgMeta = null;
        if (rawgId is > 0)
        {
            var rawgGame = await _rawg.GetGameDetailsAsync(rawgId.Value).ConfigureAwait(false);
            if (rawgGame != null)
            {
                rawgMeta = rawgGame.Metacritic;
                game.Platforms = rawgGame.Platforms;
                var allowRawgStoreLinks = rawgStoreMatchTrust || igdbStores.Count == 0;
                game.Stores = allowRawgStoreLinks
                    ? IgdbGameMapper.MergeStoreListsPreferIgdb(igdbStores, rawgGame.Stores)
                    : igdbStores;
                if (string.IsNullOrWhiteSpace(game.Website) && !string.IsNullOrWhiteSpace(rawgGame.Website))
                    game.Website = rawgGame.Website;
                if (string.IsNullOrWhiteSpace(game.Description) && !string.IsNullOrWhiteSpace(rawgGame.Description))
                {
                    game.Description = rawgGame.Description;
                    game.DescriptionRaw = rawgGame.DescriptionRaw ?? rawgGame.Description;
                }

                if (string.IsNullOrWhiteSpace(game.Released) && !string.IsNullOrWhiteSpace(rawgGame.Released))
                    game.Released = rawgGame.Released;

                if ((game.AgeRatingsDisplay == null || game.AgeRatingsDisplay.Count == 0)
                    && rawgGame.EsrbRating != null
                    && !string.IsNullOrWhiteSpace(rawgGame.EsrbRating.Name))
                {
                    game.AgeRatingsDisplay = new List<AgeRatingDisplayItem>
                    {
                        new() { Organization = "ESRB", Label = rawgGame.EsrbRating.Name.Trim() },
                    };
                }
            }
            else if (igdbStores.Count > 0)
                game.Stores = igdbStores;
        }
        else if (igdbStores.Count > 0)
            game.Stores = igdbStores;

        game.Stores = DigitalGameStoreFilter.KeepMajorDigitalOnly(game.Stores);

        GameDisplayScoreHelper.Apply(game, rawgMeta, agg, aggCount, total);
        return game;
    }

    /// <returns>RAWG oyun id ve mağaza linkleri için bu eşlemenin güvenilir olup olmadığı.</returns>
    private async Task<(int? Id, bool TrustStores)> TryResolveRawgIdAsync(string? gameName, JObject igdbToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return (null, false);
        if (string.IsNullOrWhiteSpace(GameTracker.Api.AppConfig.RawgApiKey)) return (null, false);

        var igdbSlug = igdbToken.Value<string>("slug");
        var releaseUnix = igdbToken["first_release_date"]?.Value<long?>();

        var results = await _rawg.GetGamesBySearchAsync(gameName, showNsfw: false, pageSize: 24).ConfigureAwait(false);
        if (results == null || results.Count == 0) return (null, false);

        Game? best = null;
        var bestScore = int.MinValue;
        foreach (var g in results)
        {
            var s = ScoreRawgCandidate(g, gameName, igdbSlug, releaseUnix);
            if (s > bestScore)
            {
                bestScore = s;
                best = g;
            }
        }

        if (best == null) return (null, false);
        const int minId = 25;
        if (bestScore < minId) return (null, false);
        const int trustStoresThreshold = 52;
        return (best.Id, bestScore >= trustStoresThreshold);
    }

    private static int ScoreRawgCandidate(Game g, string igdbName, string? igdbSlug, long? igdbReleaseUnix)
    {
        int score = 0;
        if (string.Equals(g.Name, igdbName, StringComparison.OrdinalIgnoreCase))
            score += 100;

        var nIgdb = NormalizeGameTitle(igdbName);
        var nRawg = NormalizeGameTitle(g.Name ?? "");
        if (nRawg == nIgdb && nIgdb.Length > 0)
            score += 85;

        var rawgSlug = (g.Slug ?? "").Trim();
        if (!string.IsNullOrEmpty(igdbSlug) && !string.IsNullOrEmpty(rawgSlug))
        {
            if (string.Equals(rawgSlug, igdbSlug, StringComparison.OrdinalIgnoreCase))
                score += 130;
            else if (rawgSlug.Contains(igdbSlug, StringComparison.OrdinalIgnoreCase)
                     || igdbSlug.Contains(rawgSlug, StringComparison.OrdinalIgnoreCase))
                score += 55;
            else
                score -= 45;
        }

        if (igdbReleaseUnix is > 0
            && !string.IsNullOrWhiteSpace(g.Released)
            && DateTime.TryParse(g.Released, CultureInfo.InvariantCulture, DateTimeStyles.None, out var rd))
        {
            var yIgdb = DateTimeOffset.FromUnixTimeSeconds(igdbReleaseUnix.Value).Year;
            if (rd.Year == yIgdb) score += 48;
            else if (Math.Abs(rd.Year - yIgdb) <= 1) score += 28;
            else if (Math.Abs(rd.Year - yIgdb) <= 2) score += 12;
            else if (Math.Abs(rd.Year - yIgdb) > 4) score -= 35;
        }

        var igdbLower = igdbName.ToLowerInvariant();
        var rawgLower = (g.Name ?? "").ToLowerInvariant();
        if (!igdbLower.Contains("dlc", StringComparison.Ordinal) && rawgLower.Contains("dlc", StringComparison.Ordinal))
            score -= 75;
        if (!igdbLower.Contains("expansion", StringComparison.Ordinal) && rawgLower.Contains("expansion", StringComparison.Ordinal))
            score -= 75;
        if (!igdbLower.Contains("phantom", StringComparison.Ordinal) && rawgLower.Contains("phantom liberty", StringComparison.Ordinal))
            score -= 95;

        foreach (var kw in new[] { "season pass", "soundtrack", "artbook", "trilogy", "bundle" })
        {
            if (!igdbLower.Contains(kw, StringComparison.Ordinal) && rawgLower.Contains(kw, StringComparison.Ordinal))
                score -= 45;
        }

        score += Math.Min(g.Added / 400, 35);
        if (g.Metacritic is >= 80) score += 12;
        if (g.Metacritic is >= 90) score += 10;

        return score;
    }

    private static string NormalizeGameTitle(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        var s = name.Trim();
        var paren = s.IndexOf('(');
        if (paren > 0) s = s[..paren].Trim();
        var parts = s.ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts);
    }
}
