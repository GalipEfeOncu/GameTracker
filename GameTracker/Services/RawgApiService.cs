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
        catch (TaskCanceledException)
        {
            // İstek zaman aşımına uğradıysa
            System.Diagnostics.Debug.WriteLine("Hata: İstek zaman aşımına uğradı (Timeout). API cevap vermedi.");
            return new List<Game>(); // Uygulama çökmesin, boş liste dönsün.
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
    /// Belirli bir oyunun detaylı bilgilerini getirir
    /// </summary>
    /// <param name="gameId">Oyun ID'si</param>
    public async Task<GameDetails> GetGameDetailsAsync(int gameId)
    {
        try
        {
            string url = $"{_baseUrl}/games/{gameId}?key={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var gameDetails = JsonConvert.DeserializeObject<GameDetails>(json);

            return gameDetails;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetGameDetailsAsync Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Oyunun ekran görüntülerini getirir
    /// </summary>
    /// <param name="gameId">Oyun ID'si</param>
    public async Task<List<Screenshot>> GetGameScreenshotsAsync(int gameId)
    {
        try
        {
            string url = $"{_baseUrl}/games/{gameId}/screenshots?key={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
                return new List<Screenshot>();
            }

            string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var screenshotResponse = JsonConvert.DeserializeObject<ScreenshotResponse>(json);

            return screenshotResponse?.Results ?? new List<Screenshot>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetGameScreenshotsAsync Error: {ex.Message}");
            return new List<Screenshot>();
        }
    }
}