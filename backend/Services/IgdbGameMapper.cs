using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using GameTracker.Models;
using Newtonsoft.Json.Linq;

namespace GameTracker.Services;

internal static class IgdbGameMapper
{
    private const string IgdbUploadBase = "https://images.igdb.com/igdb/image/upload";
    private const string CoverCardTransform = "t_cover_big";
    private const string CoverHeroTransform = "t_1080p";
    private const string ScreenshotTransform = "t_screenshot_big";
    private const string ScreenshotHeroTransform = "t_1080p";

    public static Game FromSummaryToken(JToken g)
    {
        var coverId = g["cover"]?["image_id"]?.Value<string>();
        var shotId = FirstScreenshotImageId(g);
        var listImage = !string.IsNullOrEmpty(shotId)
            ? BuildScreenshotUrl(shotId, ScreenshotTransform)
            : BuildCoverUrl(coverId, CoverCardTransform);
        var game = new Game
        {
            Id = ToIntId(g["id"]),
            Name = g.Value<string>("name") ?? "Unknown",
            BackgroundImage = listImage,
        };

        MapGenres(game, g["genres"]);
        var agg = g["aggregated_rating"]?.Value<double?>();
        var aggCount = g["aggregated_rating_count"]?.Value<int?>();
        var total = g["total_rating"]?.Value<double?>();
        GameDisplayScoreHelper.Apply(game, null, agg, aggCount, total);
        return game;
    }

    public static Game FromDetailToken(JToken g)
    {
        var game = FromSummaryToken(g);
        var coverId = g["cover"]?["image_id"]?.Value<string>();
        var shotId = FirstScreenshotImageId(g);
        game.BackgroundImageAdditional = !string.IsNullOrEmpty(shotId)
            ? BuildScreenshotUrl(shotId, ScreenshotHeroTransform)
            : BuildCoverUrl(coverId, CoverHeroTransform);

        var summary = g.Value<string>("summary");
        var storyline = g.Value<string>("storyline");
        var text = !string.IsNullOrWhiteSpace(storyline) ? storyline : summary;
        if (!string.IsNullOrWhiteSpace(text))
        {
            game.DescriptionRaw = text;
            var safe = WebUtility.HtmlEncode(text);
            game.Description = $"<p>{safe.Replace("\n", "</p><p>", StringComparison.Ordinal)}</p>";
        }

        var releaseUnix = g["first_release_date"]?.Value<long?>();
        if (releaseUnix is > 0)
        {
            try
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(releaseUnix.Value).UtcDateTime;
                game.Released = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch
            {
                game.Released = "";
            }
        }

        game.Website = PickOfficialWebsite(g["websites"]);
        MapInvolvedCompanies(game, g["involved_companies"]);
        MapAgeRatings(game, g["age_ratings"]);
        return game;
    }

    public static List<Screenshot> ScreenshotsFromToken(JToken g)
    {
        var list = new List<Screenshot>();
        var shots = g["screenshots"] as JArray;
        if (shots == null) return list;
        var i = 0;
        foreach (var s in shots)
        {
            var imageId = s["image_id"]?.Value<string>();
            if (string.IsNullOrEmpty(imageId)) continue;
            var url = BuildScreenshotUrl(imageId, ScreenshotTransform);
            list.Add(new Screenshot { Id = i++, ImageUrl = url });
        }

        return list;
    }

    private static int ToIntId(JToken? idToken)
    {
        if (idToken == null) return 0;
        var v = idToken.Value<long?>();
        if (v is null or <= 0 or > int.MaxValue) return 0;
        return (int)v.Value;
    }

    private static string BuildCoverUrl(string? imageId, string transform) =>
        string.IsNullOrEmpty(imageId) ? "" : $"{IgdbUploadBase}/{transform}/{imageId}.jpg";

    private static string BuildScreenshotUrl(string? imageId, string transform) =>
        string.IsNullOrEmpty(imageId) ? "" : $"{IgdbUploadBase}/{transform}/{imageId}.jpg";

    private static string? FirstScreenshotImageId(JToken g)
    {
        if (g["screenshots"] is not JArray shots || shots.Count == 0) return null;
        return shots[0]?["image_id"]?.Value<string>();
    }

    /// <summary>Önce en yaygın / karşılaştırmada güvenilen kuruluş; yoksa sıradaki.</summary>
    private static readonly string[][] AgeRatingOrgPriority =
    {
        new[] { "ESRB", "ENTERTAINMENT SOFTWARE RATING BOARD" },
        new[] { "PEGI", "PAN EUROPEAN" },
        new[] { "USK" },
        new[] { "CERO" },
        new[] { "ACB", "AUSTRALIAN CLASSIFICATION" },
        new[] { "GRAC" },
        new[] { "CLASS_IND", "CLASSIFICAÇÃO INDICATIVA", "CLASSIFICACAO INDICATIVA" },
        new[] { "OFLC" },
        new[] { "BBFC" },
    };

    private static void MapAgeRatings(Game game, JToken? ageTok)
    {
        game.AgeRatingsDisplay = new List<AgeRatingDisplayItem>();
        if (ageTok is not JArray arr || arr.Count == 0) return;

        var candidates = new List<AgeRatingDisplayItem>();
        foreach (var ar in arr)
        {
            var cat = ar["rating_category"];
            var label = cat?.Value<string>("rating");
            if (string.IsNullOrWhiteSpace(label)) continue;
            var org = cat?["organization"]?.Value<string>("name") ?? "";
            candidates.Add(new AgeRatingDisplayItem { Organization = org.Trim(), Label = label.Trim() });
        }

        var primary = PickPrimaryAgeRating(candidates);
        if (primary != null)
            game.AgeRatingsDisplay.Add(primary);
    }

    private static AgeRatingDisplayItem? PickPrimaryAgeRating(List<AgeRatingDisplayItem> candidates)
    {
        if (candidates.Count == 0) return null;
        static string Norm(string? s) => (s ?? "").Trim().ToUpperInvariant();

        foreach (var keys in AgeRatingOrgPriority)
        {
            foreach (var c in candidates)
            {
                var o = Norm(c.Organization);
                if (string.IsNullOrEmpty(o)) continue;
                foreach (var key in keys)
                {
                    var k = key.ToUpperInvariant();
                    if (o.Equals(k, StringComparison.Ordinal) || o.Contains(k, StringComparison.Ordinal))
                        return c;
                }
            }
        }

        return candidates[0];
    }

    private static void MapGenres(Game game, JToken? genresTok)
    {
        if (genresTok is not JArray arr || arr.Count == 0)
        {
            game.Genres = new List<Genre>();
            return;
        }

        game.Genres = arr
            .Select(x => new Genre
            {
                Id = ToIntId(x["id"]),
                Name = x.Value<string>("name") ?? "",
                Slug = x.Value<string>("slug") ?? "",
            })
            .Where(g => !string.IsNullOrEmpty(g.Name))
            .ToList();
    }

    private static void MapInvolvedCompanies(Game game, JToken? involvedTok)
    {
        var devs = new List<Developer>();
        var pubs = new List<Publisher>();
        if (involvedTok is not JArray arr)
        {
            game.Developers = devs;
            game.Publishers = pubs;
            return;
        }

        foreach (var ic in arr)
        {
            var name = ic["company"]?.Value<string>("name");
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (ic.Value<bool?>("developer") == true)
                devs.Add(new Developer { Name = name });
            if (ic.Value<bool?>("publisher") == true)
                pubs.Add(new Publisher { Name = name });
        }

        game.Developers = devs;
        game.Publishers = pubs;
    }

    /// <summary>IGDB teması / isim ile NSFW benzeri içerik süzme (RAWG istemci süzgecine yakın).</summary>
    public static bool IsLikelyNsfw(JToken g, bool showNsfw)
    {
        if (showNsfw) return false;
        var name = (g.Value<string>("name") ?? "").ToLowerInvariant();
        string[] bad = { "hentai", "sex ", "porno", "nude", "erotic", "strip" };
        if (bad.Any(w => name.Contains(w, StringComparison.Ordinal))) return true;

        if (g["themes"] is JArray themes)
        {
            foreach (var t in themes)
            {
                var slug = (t.Value<string>("slug") ?? "").ToLowerInvariant();
                if (slug.Contains("adult", StringComparison.Ordinal) || slug.Contains("erotic", StringComparison.Ordinal))
                    return true;
            }
        }

        return false;
    }

    private static string? PickOfficialWebsite(JToken? websitesTok)
    {
        if (websitesTok is not JArray arr || arr.Count == 0) return null;
        foreach (var w in arr.OrderByDescending(x => x.Value<bool?>("trusted") == true))
        {
            var url = w.Value<string>("url");
            if (!string.IsNullOrWhiteSpace(url)) return url;
        }

        return null;
    }
}
