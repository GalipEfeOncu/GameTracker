using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GameTracker.Models;
using DevExpress.XtraEditors;

public class RawgApiService
{
    private readonly string _apiKey = "e05bb5b0ad0b4391b17c84790dbcd2e0";
    private readonly string _baseUrl = "https://api.rawg.io/api";
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

    private async Task<List<Game>> GetGamesAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var gameResponse = JsonConvert.DeserializeObject<GameResponse>(json);
            return gameResponse?.results ?? new List<Game>();
        }
        catch (Exception ex)
        {
            throw new Exception($"API hatası: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Oyunları arayarak getirir
    /// </summary>
    /// <param name="searchTerm">Aranacak oyun adı</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı (varsayılan: 20)</param>
    /// <returns>Bulunan oyunların listesi</returns>
    public async Task<List<Game>> GetGamesBySearchAsync(string searchTerm, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return new List<Game>();

        string url = $"{_baseUrl}/games?key={_apiKey}&search={searchTerm}&page_size={pageSize}";
        return await GetGamesAsync(url);
    }

    /// <summary>
    /// Popüler oyunları getirir
    /// </summary>
    public async Task<List<Game>> GetPopularGamesAsync(int pageSize = 20)
    {
        string url = $"{_baseUrl}/games?key={_apiKey}&dates=2025-01-01,2025-12-31&ordering=-metacritic&page_size={pageSize}";
        return await GetGamesAsync(url);
    }
}