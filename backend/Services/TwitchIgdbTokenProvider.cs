using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GameTracker.Services;

/// <summary>
/// Twitch client credentials ile IGDB erişim token'ı alır ve süre dolmadan yeniler.
/// </summary>
public sealed class TwitchIgdbTokenProvider
{
    private const string TokenEndpoint = "https://id.twitch.tv/oauth2/token";
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(5);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly object _sync = new();
    private string? _accessToken;
    private DateTime _expiresAtUtc = DateTime.MinValue;

    public TwitchIgdbTokenProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(GameTracker.Api.AppConfig.IgdbClientId)
        && !string.IsNullOrWhiteSpace(GameTracker.Api.AppConfig.IgdbClientSecret);

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return null;

        lock (_sync)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _expiresAtUtc - RefreshSkew)
                return _accessToken;
        }

        var client = _httpClientFactory.CreateClient(nameof(TwitchIgdbTokenProvider));
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = GameTracker.Api.AppConfig.IgdbClientId!.Trim(),
            ["client_secret"] = GameTracker.Api.AppConfig.IgdbClientSecret!.Trim(),
            ["grant_type"] = "client_credentials",
        });

        using var response = await client.PostAsync(TokenEndpoint, body, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var parsed = JsonConvert.DeserializeObject<TwitchTokenResponse>(json);
        if (string.IsNullOrEmpty(parsed?.AccessToken) || parsed.ExpiresIn <= 0)
            return null;

        lock (_sync)
        {
            _accessToken = parsed.AccessToken;
            _expiresAtUtc = DateTime.UtcNow.AddSeconds(parsed.ExpiresIn);
            return _accessToken;
        }
    }

    private sealed class TwitchTokenResponse
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
