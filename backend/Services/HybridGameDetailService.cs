using System;
using System.Collections.Generic;
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

        int? rawgId = await TryResolveRawgIdAsync(game.Name, ct).ConfigureAwait(false);
        game.RawgId = rawgId;

        int? rawgMeta = null;
        if (rawgId is > 0)
        {
            var rawgGame = await _rawg.GetGameDetailsAsync(rawgId.Value).ConfigureAwait(false);
            if (rawgGame != null)
            {
                rawgMeta = rawgGame.Metacritic;
                game.Platforms = rawgGame.Platforms;
                game.Stores = rawgGame.Stores;
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
        }

        GameDisplayScoreHelper.Apply(game, rawgMeta, agg, aggCount, total);
        return game;
    }

    private async Task<int?> TryResolveRawgIdAsync(string? gameName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return null;
        if (string.IsNullOrWhiteSpace(GameTracker.Api.AppConfig.RawgApiKey)) return null;

        var results = await _rawg.GetGamesBySearchAsync(gameName, showNsfw: false, pageSize: 5).ConfigureAwait(false);
        if (results == null || results.Count == 0) return null;

        var exact = results.FirstOrDefault(g =>
            string.Equals(g.Name, gameName, StringComparison.OrdinalIgnoreCase));
        return exact?.Id ?? results[0].Id;
    }
}
