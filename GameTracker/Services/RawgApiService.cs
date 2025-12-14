using GameTracker.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GameTracker
{
    public class RawgApiService
    {
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.rawg.io/api";
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public RawgApiService()
        {
            // Anahtarı config'den çekiyoruz
            _apiKey = ConfigurationManager.AppSettings["RawgApiKey"];

            // Anahtar yoksa hata fırlat
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("API Key bulunamadı! Lütfen Secrets.config dosyasını kontrol et.");
        }

        /// <summary>
        /// NSFW filtresine göre dışlanacak etiketlerin listesini döndürür.
        /// </summary>
        private string GetNsfwFilterParam()
        {
            // Eğer kullanıcı "Show NSFW" seçeneğini AÇIK (True) yaptıysa filtreleme yok.
            if (Properties.Settings.Default.ShowNSFW)
                return "";

            string genres = "adult,hentai";
            string tags = "nudity,sexual-content,hentai,erotic,nsfw,romance-sex";

            return $"&exclude_genres={genres}&exclude_tags={tags}";
        }

        /// <summary>
        /// Gelen oyun listesini manuel olarak tarayıp +18 içerikleri temizler.
        /// API filtresinden kaçan (tag girilmemiş) oyunlar için güvenlik kilidi.
        /// </summary>
        private List<Game> FilterNsfwClientSide(List<Game> games)
        {
            // Eğer NSFW açık ise filtreleme yapma, hepsini dön.
            if (Properties.Settings.Default.ShowNSFW)
                return games;

            if (games == null || games.Count == 0)
                return new List<Game>();

            // Listeyi temizle
            games.RemoveAll(g =>
            {
                // Adults Only ise sil.
                if (g.EsrbRating != null && (g.EsrbRating.Slug == "adults-only" || g.EsrbRating.Id == 5))
                    return true;

                // İsminde sakıncalı kelime geçenleri sil 
                string nameLower = g.Name.ToLower();
                if (nameLower.Contains("hentai") || nameLower.Contains("sex ") || nameLower.Contains("porno"))
                    return true;

                // Genre Kontrolü 
                if (g.Genres != null && g.Genres.Any(gen => gen.Slug == "adult" || gen.Slug == "hentai"))
                    return true;

                return false; // Temiz
            });

            return games;
        }

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
                var games = gameResponse?.results ?? new List<Game>();

                return FilterNsfwClientSide(games);
            }
            catch (HttpRequestException httpEx)
            {
                // İnternet kopuksa veya sunucu tamamen çökmüşse
                System.Diagnostics.Debug.WriteLine($"Network hatası: {httpEx.Message}");
                return new List<Game>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                return new List<Game>();
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

            string encodedSearch = Uri.EscapeDataString(searchTerm);
            string url = $"{_baseUrl}/games?key={_apiKey}&search={encodedSearch}&page_size={pageSize}"; // Temel URL
            url += GetNsfwFilterParam(); // NSFW Filtresini ekle 

            return await GetGamesAsync(url);
        }

        /// <summary>
        /// Popüler oyunları getirir
        /// </summary>
        public async Task<List<Game>> GetPopularGamesAsync(int pageNumber, int pageSize = 20)
        {
            string url = $"{_baseUrl}/games?key={_apiKey}&ordering=-added,-rating&dates=2024-01-01,2025-12-31&page_size={pageSize}&exclude_additions=true&page={pageNumber}";
            url += GetNsfwFilterParam();
            return await GetGamesAsync(url);
        }

        /// <summary>
        /// ID'ye göre oyunun TÜM detaylarını getirir 
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

        /// <summary>
        /// Oyunun ekran görüntülerini çeker.
        /// </summary>
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
}