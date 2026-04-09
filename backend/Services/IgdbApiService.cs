using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameTracker.Models;
using Newtonsoft.Json.Linq;

namespace GameTracker.Services;

/// <summary>IGDB v4 (Twitch OAuth). Listeler ve arama burada; detayda RAWG ile birleştirme ayrı serviste.</summary>
public sealed class IgdbApiService
{
    private const string IgdbBase = "https://api.igdb.com/v4/";
    private const int Unix2019 = 1546300800; // 2019-01-01 — popüler/trend için alt tarih sınırı

    private static readonly string ListFields =
        "id,name,cover.image_id,screenshots.image_id,first_release_date,genres.id,genres.name,genres.slug," +
        "aggregated_rating,total_rating,aggregated_rating_count,hypes,themes.slug";

    private static readonly string DetailFields =
        ListFields + ",summary,storyline,involved_companies.developer,involved_companies.publisher," +
        "involved_companies.company.name,websites.url,websites.trusted," +
        "age_ratings.rating_category.rating,age_ratings.rating_category.organization.name";

    private readonly IHttpClientFactory _httpFactory;
    private readonly TwitchIgdbTokenProvider _tokens;
    private readonly IgdbRateGate _rateGate;

    public IgdbApiService(
        IHttpClientFactory httpFactory,
        TwitchIgdbTokenProvider tokens,
        IgdbRateGate rateGate)
    {
        _httpFactory = httpFactory;
        _tokens = tokens;
        _rateGate = rateGate;
    }

    public bool IsConfigured => _tokens.IsConfigured;

    public async Task<List<Game>> GetPopularAsync(int offset, int limit, bool showNsfw, CancellationToken ct = default)
    {
        var unixEnd = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds();
        var body =
            $"fields {ListFields}; " +
            $"where game_type = 0 & version_parent = null & first_release_date >= {Unix2019} & first_release_date <= {unixEnd}; " +
            "sort hypes desc; " +
            $"limit {limit}; offset {offset};";
        return await QueryGamesAsync(body, showNsfw, ct).ConfigureAwait(false);
    }

    public async Task<List<Game>> GetDiscoverAsync(
        int? genreIgdbId,
        string mode,
        int page,
        int pageSize,
        bool showNsfw,
        CancellationToken ct = default)
    {
        var offset = Math.Max(0, (page - 1) * pageSize);
        var unixEnd = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds();
        var baseWhere = "game_type = 0 & version_parent = null";
        var genreClause = genreIgdbId is > 0 ? $" & genres = {genreIgdbId.Value}" : "";

        string where;
        string sort;
        switch (mode)
        {
            case "top_rated":
                where = $"{baseWhere}{genreClause} & total_rating != null";
                sort = "sort total_rating desc;";
                break;
            case "new":
                where = $"{baseWhere}{genreClause} & first_release_date != null & first_release_date <= {unixEnd}";
                sort = "sort first_release_date desc;";
                break;
            case "metacritic_top":
                where = $"{baseWhere}{genreClause} & aggregated_rating >= 75 & aggregated_rating_count > 0";
                sort = "sort aggregated_rating desc;";
                break;
            case "trending":
            default:
                where = $"{baseWhere}{genreClause} & first_release_date >= {Unix2019} & first_release_date <= {unixEnd}";
                sort = "sort hypes desc;";
                break;
        }

        var body =
            $"fields {ListFields}; " +
            $"where {where}; " +
            sort +
            $"limit {pageSize}; offset {offset};";
        return await QueryGamesAsync(body, showNsfw, ct).ConfigureAwait(false);
    }

    public async Task<List<Game>> SearchAsync(string term, int limit, bool showNsfw, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term)) return new List<Game>();
        var q = term.Trim().Replace("\"", "\\\"", StringComparison.Ordinal);
        var body =
            $"search \"{q}\"; " +
            $"fields {ListFields}; " +
            "where game_type = 0 & version_parent = null; " +
            $"limit {limit};";
        return await QueryGamesAsync(body, showNsfw, ct).ConfigureAwait(false);
    }

    public async Task<JObject?> GetGameJsonAsync(int igdbGameId, CancellationToken ct = default)
    {
        if (igdbGameId <= 0) return null;
        var body = $"fields {DetailFields}; where id = {igdbGameId}; limit 1;";
        var arr = await PostArrayAsync("games", body, ct).ConfigureAwait(false);
        return arr?.FirstOrDefault() as JObject;
    }

    public async Task<List<Screenshot>> GetScreenshotsAsync(int igdbGameId, CancellationToken ct = default)
    {
        var g = await GetGameJsonAsync(igdbGameId, ct).ConfigureAwait(false);
        return g == null ? new List<Screenshot>() : IgdbGameMapper.ScreenshotsFromToken(g);
    }

    private async Task<List<Game>> QueryGamesAsync(string apicalypseBody, bool showNsfw, CancellationToken ct)
    {
        var arr = await PostArrayAsync("games", apicalypseBody, ct).ConfigureAwait(false);
        if (arr == null) return new List<Game>();
        var list = new List<Game>();
        foreach (var item in arr)
        {
            if (item is not JObject jo) continue;
            if (IgdbGameMapper.IsLikelyNsfw(jo, showNsfw)) continue;
            list.Add(IgdbGameMapper.FromSummaryToken(jo));
        }

        return list;
    }

    private async Task<JArray?> PostArrayAsync(string endpoint, string apicalypseBody, CancellationToken ct)
    {
        if (!_tokens.IsConfigured) return null;
        await _rateGate.WaitTurnAsync(ct).ConfigureAwait(false);
        var token = await _tokens.GetAccessTokenAsync(ct).ConfigureAwait(false);
        if (string.IsNullOrEmpty(token)) return null;

        var client = _httpFactory.CreateClient(nameof(IgdbApiService));
        using var req = new HttpRequestMessage(HttpMethod.Post, IgdbBase + endpoint)
        {
            Content = new StringContent(apicalypseBody, Encoding.UTF8, "text/plain"),
        };
        req.Headers.TryAddWithoutValidation("Client-ID", GameTracker.Api.AppConfig.IgdbClientId!.Trim());
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var res = await client.SendAsync(req, ct).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        try
        {
            return JArray.Parse(json);
        }
        catch
        {
            return null;
        }
    }
}
