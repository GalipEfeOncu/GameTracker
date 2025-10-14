using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class RawgApiService
{
    private readonly string _apiKey = "e05bb5b0ad0b4391b17c84790dbcd2e0"; 
    private readonly string _baseUrl = "https://api.rawg.io/api";
    private readonly HttpClient _httpClient;

    public RawgApiService()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Oyunları arayarak getirir
    /// </summary>
    public async Task<List<Game>> GetGamesBySearchAsync(string searchTerm, int pageSize = 20)
    {
        try
        {
            string url = $"{_baseUrl}/games?key={_apiKey}&search={searchTerm}&page_size={pageSize}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            GameResponse gameResponse = JsonConvert.DeserializeObject<GameResponse>(jsonContent);

            return gameResponse?.results ?? new List<Game>();
        }
        catch (Exception ex)
        {
            throw new Exception($"API'dan oyun çekilirken hata oluştu: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Popüler oyunları getirir
    /// </summary>
    public async Task<List<Game>> GetPopularGamesAsync(int pageSize = 20)
    {
        try
        {
            string url = $"{_baseUrl}/games?key={_apiKey}&ordering=-rating&page_size={pageSize}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            GameResponse gameResponse = JsonConvert.DeserializeObject<GameResponse>(jsonContent);

            return gameResponse?.results ?? new List<Game>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Popüler oyunlar çekilirken hata oluştu: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Belirli bir yapımcının oyunlarını getirir
    /// </summary>
    public async Task<List<Game>> GetGamesByGenreAsync(string genre, int pageSize = 20)
    {
        try
        {
            string url = $"{_baseUrl}/games?key={_apiKey}&genres={genre}&page_size={pageSize}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            GameResponse gameResponse = JsonConvert.DeserializeObject<GameResponse>(jsonContent);

            return gameResponse?.results ?? new List<Game>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Oyunlar çekilirken hata oluştu: {ex.Message}", ex);
        }
    }
}

public class Game
{
    public int id { get; set; }
    public string name { get; set; }
    public string background_image { get; set; }
    public double rating { get; set; }
    public string released { get; set; }
    public List<Platform> platforms { get; set; }
}

public class Platform
{
    public PlatformInfo platform { get; set; }
}

public class PlatformInfo
{
    public string name { get; set; }
}

public class GameResponse
{
    public List<Game> results { get; set; }
}