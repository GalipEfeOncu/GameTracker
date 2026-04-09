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
        var game = new Game
        {
            Id = ToIntId(g["id"]),
            Name = g.Value<string>("name") ?? "Unknown",
            Slug = g.Value<string>("slug") ?? "",
            BackgroundImage = BuildCoverUrl(coverId, CoverCardTransform),
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
        MapTrailer(game, g["videos"]);
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

    public static void ApplyTimeToBeat(Game game, JObject? ttb)
    {
        game.TimeToBeat = null;
        if (ttb == null) return;

        static double? SecToHours(int? sec) =>
            sec is > 0 ? Math.Round(sec.Value / 3600.0, 1) : null;

        var h = ttb["hastily"]?.Value<int?>();
        var n = ttb["normally"]?.Value<int?>();
        var c = ttb["completely"]?.Value<int?>();
        if (h is not > 0 && n is not > 0 && c is not > 0) return;

        game.TimeToBeat = new GameTimeToBeatInfo
        {
            MainStoryHours = SecToHours(h),
            MainExtraHours = SecToHours(n),
            CompletionistHours = SecToHours(c),
            SubmissionCount = ttb["count"]?.Value<int?>(),
        };
    }

    private static void MapTrailer(Game game, JToken? videosTok)
    {
        game.TrailerYoutubeId = null;
        if (videosTok is not JArray arr || arr.Count == 0) return;

        JToken? chosen = null;
        foreach (var v in arr)
        {
            var id = NormalizeYoutubeVideoId(v["video_id"]?.Value<string>());
            if (string.IsNullOrEmpty(id)) continue;
            var name = (v.Value<string>("name") ?? "").ToLowerInvariant();
            if (name.Contains("trailer", StringComparison.Ordinal))
            {
                chosen = v;
                break;
            }

            chosen ??= v;
        }

        if (chosen == null) return;
        var finalId = NormalizeYoutubeVideoId(chosen["video_id"]?.Value<string>());
        if (!string.IsNullOrEmpty(finalId))
            game.TrailerYoutubeId = finalId;
    }

    /// <summary>IGDB <c>video_id</c> çoğunlukla ham YouTube id; bazen URL gelebilir.</summary>
    private static string? NormalizeYoutubeVideoId(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();
        const StringComparison o = StringComparison.OrdinalIgnoreCase;
        if (raw.Contains("youtu.be/", o))
        {
            var i = raw.IndexOf("youtu.be/", o) + 9;
            var end = raw.IndexOf('?', i);
            var id = (end < 0 ? raw[i..] : raw[i..end]).Trim();
            return id.Length >= 6 ? id : null;
        }

        if (raw.Contains("youtube.com/watch", o))
        {
            var vIdx = raw.IndexOf("v=", o);
            if (vIdx >= 0)
            {
                var start = vIdx + 2;
                var amp = raw.IndexOf('&', start);
                var id = (amp < 0 ? raw[start..] : raw[start..amp]).Trim();
                return id.Length >= 6 ? id : null;
            }
        }

        if (raw.Contains("embed/", o))
        {
            var i = raw.IndexOf("embed/", o) + 6;
            var end = raw.IndexOf('?', i);
            var id = (end < 0 ? raw[i..] : raw[i..end]).Trim();
            return id.Length >= 6 ? id : null;
        }

        return raw.Length >= 6 && raw.Length <= 32 && !raw.Contains('/') ? raw : null;
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

    /// <summary>
    /// IGDB <c>external_games</c> → satın alma linkleri. Öncelik: API’nin verdiği <c>url</c>;
    /// yoksa yalnızca Steam için sayısal <c>uid</c> ile mağaza URL’si üretilir (itch/epic vb. tahmin edilmez).
    /// </summary>
    public static List<StoreWrapper> StoresFromExternalGames(JArray? arr)
    {
        var built = new List<StoreWrapper>();
        if (arr == null || arr.Count == 0) return built;

        var rows = arr
            .OfType<JObject>()
            .OrderByDescending(t => !string.IsNullOrWhiteSpace(t.Value<string>("url")))
            .ToList();

        foreach (var t in rows)
        {
            var urlRaw = t.Value<string>("url")?.Trim();
            var uid = t.Value<string>("uid")?.Trim();
            var cat = t["category"]?.Value<int?>();
            var srcName = (t["external_game_source"] as JObject)?["name"]?.Value<string>() ?? "";

            string? finalUrl = null;
            if (!string.IsNullOrEmpty(urlRaw) && TryNormalizeHttpUrl(urlRaw, out var nu))
                finalUrl = nu;
            else if (IsSteamExternal(cat, srcName) && !string.IsNullOrEmpty(uid) && uid.All(char.IsAsciiDigit))
                finalUrl = $"https://store.steampowered.com/app/{uid}/";

            if (string.IsNullOrEmpty(finalUrl)) continue;

            var displayName = ResolveStoreDisplayName(finalUrl, cat, srcName);
            var storeId = cat is > 0 ? cat.Value : StableStoreIdFromName(displayName);
            built.Add(new StoreWrapper
            {
                Url = finalUrl,
                Store = new StoreInfo { Id = storeId, Name = displayName },
            });
        }

        return DedupeStoresByCanonicalKey(built);
    }

    private static List<StoreWrapper> DedupeStoresByCanonicalKey(List<StoreWrapper> stores)
    {
        var map = new Dictionary<string, StoreWrapper>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in stores)
        {
            var key = CanonicalStoreKey(s.Store?.Name, s.Url) ?? $"url:{s.Url}";
            if (!map.ContainsKey(key))
                map[key] = s;
        }

        return map.Values.ToList();
    }

    /// <summary>RAWG mağazaları ile birleştir: aynı mağaza anahtarında IGDB (önce eklenen) kazanır.</summary>
    public static List<StoreWrapper> MergeStoreListsPreferIgdb(List<StoreWrapper> igdbStores, List<StoreWrapper>? rawgStores)
    {
        var map = new Dictionary<string, StoreWrapper>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in igdbStores)
        {
            var key = CanonicalStoreKey(s.Store?.Name, s.Url) ?? $"url:{s.Url}";
            if (!map.ContainsKey(key))
                map[key] = s;
        }

        foreach (var s in rawgStores ?? Enumerable.Empty<StoreWrapper>())
        {
            if (string.IsNullOrWhiteSpace(s.Url)) continue;
            var key = CanonicalStoreKey(s.Store?.Name, s.Url) ?? $"url:{s.Url}";
            if (!map.ContainsKey(key))
                map[key] = s;
        }

        return map.Values.ToList();
    }

    private static string? CanonicalStoreKey(string? storeName, string? url)
    {
        var n = (storeName ?? "").ToLowerInvariant();
        if (n.Contains("steam", StringComparison.Ordinal)) return "steam";
        if (n.Contains("gog", StringComparison.Ordinal)) return "gog";
        if (n.Contains("epic", StringComparison.Ordinal)) return "epic";
        if (n.Contains("itch", StringComparison.Ordinal)) return "itch";
        if (n.Contains("playstation", StringComparison.Ordinal) || n.Contains("psn", StringComparison.Ordinal))
            return "playstation";
        if (n.Contains("xbox", StringComparison.Ordinal) || n.Contains("microsoft", StringComparison.Ordinal))
            return "microsoft";
        if (n.Contains("nintendo", StringComparison.Ordinal)) return "nintendo";
        if (n.Contains("amazon", StringComparison.Ordinal)) return "amazon";
        if (n.Contains("ubisoft", StringComparison.Ordinal)) return "ubisoft";

        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var u))
        {
            var h = u.Host.ToLowerInvariant();
            if (h.Contains("steampowered")) return "steam";
            if (h.Contains("gog.com")) return "gog";
            if (h.Contains("epicgames.com")) return "epic";
            if (h.Contains("itch.io")) return "itch";
        }

        return null;
    }

    private static string ResolveStoreDisplayName(string url, int? category, string sourceName)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var u))
        {
            var h = u.Host.ToLowerInvariant();
            if (h.Contains("steampowered")) return "Steam";
            if (h.Contains("gog.com")) return "GOG";
            if (h.Contains("epicgames.com")) return "Epic Games Store";
            if (h.Contains("itch.io")) return "itch.io";
            if (h.Contains("playstation.com")) return "PlayStation Store";
            if (h.Contains("xbox.com") || h.Contains("microsoft.com")) return "Microsoft Store";
            if (h.Contains("nintendo.com")) return "Nintendo eShop";
        }

        if (category is > 0 && ExternalCategoryDisplayNames.TryGetValue(category.Value, out var byCat))
            return byCat;
        if (!string.IsNullOrWhiteSpace(sourceName)) return sourceName.Trim();
        return "Store";
    }

    private static readonly Dictionary<int, string> ExternalCategoryDisplayNames = new()
    {
        [1] = "Steam",
        [5] = "GOG",
        [26] = "Epic Games Store",
        [30] = "itch.io",
        [36] = "PlayStation Store",
        [11] = "Microsoft Store",
    };

    private static int StableStoreIdFromName(string name)
    {
        var key = name.ToLowerInvariant();
        return key switch
        {
            "steam" => 1,
            "gog" => 5,
            "epic games store" => 26,
            "itch.io" => 30,
            "playstation store" => 36,
            "microsoft store" => 11,
            _ => Math.Abs(name.GetHashCode(StringComparison.OrdinalIgnoreCase) % 100_000) + 10_000,
        };
    }

    private static bool IsSteamExternal(int? category, string sourceName)
    {
        if (category == 1) return true;
        return sourceName.Contains("steam", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryNormalizeHttpUrl(string raw, out string normalized)
    {
        normalized = "";
        if (string.IsNullOrWhiteSpace(raw)) return false;
        raw = raw.Trim();
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var u)) return false;
        if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps) return false;
        normalized = u.ToString();
        return true;
    }
}
