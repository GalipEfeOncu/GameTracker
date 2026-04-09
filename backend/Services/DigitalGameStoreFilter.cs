using System;
using System.Collections.Generic;
using System.Linq;
using GameTracker.Models;

namespace GameTracker.Services;

/// <summary>
/// Satın al listesinden fiziksel perakende ve küçük/yan kanalları çıkarır; yalnızca büyük dijital mağaza URL’lerini bırakır.
/// </summary>
internal static class DigitalGameStoreFilter
{
    /// <summary>Filtre sonrası liste boşsa <c>null</c> döner (API’de &quot;Satın Al&quot; bölümünü gizlemek için).</summary>
    public static List<StoreWrapper>? KeepMajorDigitalOnly(List<StoreWrapper>? stores)
    {
        if (stores == null || stores.Count == 0) return null;
        var kept = stores
            .Where(s => s != null && !string.IsNullOrWhiteSpace(s.Url) && IsApprovedDigitalUrl(s.Url!.Trim()))
            .ToList();
        return kept.Count > 0 ? kept : null;
    }

    private static bool IsApprovedDigitalUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var u)) return false;
        if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps) return false;

        var h = u.Host.ToLowerInvariant();
        var path = u.AbsolutePath.ToLowerInvariant();

        foreach (var b in BlockedHostSnippets)
        {
            if (h.Contains(b, StringComparison.Ordinal)) return false;
        }

        if (h.Contains("steampowered", StringComparison.Ordinal)) return true;
        if (h.Contains("epicgames.com", StringComparison.Ordinal)) return true;
        if (h.Contains("gog.com", StringComparison.Ordinal)) return true;
        if (h.Contains("itch.io", StringComparison.Ordinal)) return true;

        if (h.Contains("playstation.com", StringComparison.Ordinal)) return true;
        if (h.Contains("sonyentertainmentnetwork", StringComparison.Ordinal)) return true;

        if (h.Contains("xbox.com", StringComparison.Ordinal)) return true;
        if (h.StartsWith("store.microsoft.", StringComparison.Ordinal) || h.Contains("store.microsoft.com", StringComparison.Ordinal))
            return true;
        if (h.Contains("microsoft.com", StringComparison.Ordinal)
            && (path.Contains("/p/", StringComparison.Ordinal) || path.Contains("/store/", StringComparison.Ordinal)))
            return true;

        if (h.Contains("nintendo.com", StringComparison.Ordinal)) return true;

        // Ubisoft (Connect / mağaza)
        if (h.Contains("ubisoftconnect.com", StringComparison.Ordinal)) return true;
        if (h.Contains("store.ubi.com", StringComparison.Ordinal)) return true;
        if (h.Contains("ubi.com", StringComparison.Ordinal) && (path.Contains("/game/", StringComparison.Ordinal) || path.Contains("/store", StringComparison.Ordinal)))
            return true;
        if (h.Contains("ubisoft.com", StringComparison.Ordinal) && (path.Contains("/game/", StringComparison.Ordinal) || path.Contains("/store", StringComparison.Ordinal)))
            return true;

        // Battle.net / Blizzard mağaza
        if (h.Contains("battle.net", StringComparison.Ordinal)) return true;
        if (h.Contains("shop.blizzard.com", StringComparison.Ordinal)) return true;
        if (h.Contains("blizzard.com", StringComparison.Ordinal) && path.Contains("/shop", StringComparison.Ordinal)) return true;

        // EA / Origin / EA App
        if (h.Contains("origin.com", StringComparison.Ordinal)) return true;
        if (h.EndsWith("ea.com", StringComparison.Ordinal) || h.Contains(".ea.com", StringComparison.Ordinal))
        {
            if (path.Contains("/games/", StringComparison.Ordinal) || path.Contains("/game/", StringComparison.Ordinal)
                || path.Contains("/store", StringComparison.Ordinal) || path.Contains("/play/", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    /// <summary>Fiziksel veya karma perakende; dijital mağaza değil.</summary>
    private static readonly string[] BlockedHostSnippets =
    {
        "amazon.", "amzn.", "amzn.to", "a.co",
        "ebay.", "walmart.", "target.", "bestbuy.", "gamestop.", "newegg.",
        "costco.", "bjs.com", "argos.", "fnac.", "mediamarkt.", "saturn.",
        "currys.", "jbhifi.", "ebgames.", "videogamesplus.",
    };
}
