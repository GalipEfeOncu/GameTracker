using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using GameTracker.Models;

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

            // Eğer sunucu hatası (5xx) varsa loglar ve boş döner
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return new List<Game>();
            }

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var gameResponse = JsonConvert.DeserializeObject<GameResponse>(json);
            return gameResponse?.results ?? new List<Game>();
        }
        catch (HttpRequestException httpEx)
        {
            // İnternet kopuksa veya sunucu tamamen göçmüşse
            System.Diagnostics.Debug.WriteLine($"Network hatası: {httpEx.Message}");
            return new List<Game>();
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
    public async Task<List<Game>> GetPopularGamesAsync(int pageNumber, int pageSize = 20)
    {
        string url = $"{_baseUrl}/games?key={_apiKey}&ordering=-added,-rating&dates=2024-01-01,2025-12-31&page_size={pageSize}&exclude_additions=true&page={pageNumber}";
        return await GetGamesAsync(url);
    }

    /// <summary>
    /// ID'ye göre oyunun TÜM detaylarını getirir (Description, System Reqs vs. dahil)
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public async Task<Game> GetGameDetailsAsync(int gameId)
    {
        string url = $"{_baseUrl}/games/{gameId}?key={_apiKey}";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Hata: {response.StatusCode}");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // Tek bir obje döndüğü için direkt Game'e deserialize ediyoruz, List'e değil.
            var game = JsonConvert.DeserializeObject<Game>(json);
            return game;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Detay Hatası: {ex.Message}");
            return null;
        }
    }

    // Oyunun ekran görüntülerini çeker
    public async Task<List<Screenshot>> GetGameScreenshotsAsync(int gameId)
    {
        string url = $"{_baseUrl}/games/{gameId}/screenshots?key={_apiKey}";
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<ScreenshotResponse>(response);
            return result?.Results ?? new List<Screenshot>();
        }
        catch (Exception) { return new List<Screenshot>(); }
    }
}