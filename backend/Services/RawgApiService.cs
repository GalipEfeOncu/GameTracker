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
            _apiKey = GameTracker.Api.AppConfig.RawgApiKey;

            // Anahtar yoksa hata fırlat
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("API Key bulunamadı! Lütfen Secrets.config dosyasını kontrol et.");
        }

        /// <summary>
        /// NSFW filtresine göre dışlanacak etiketlerin listesini döndürür.
        /// </summary>
        private string GetNsfwFilterParam(bool showNsfw)
        {
            // Eğer kullanıcı "Show NSFW" seçeneğini AÇIK (True) yaptıysa filtreleme yok.
            if (showNsfw)
                return "";

            string genres = "adult,hentai,erotic";
            string tags = "nudity,sexual-content,nsfw,romance-sex,hentai";

            return $"&exclude_genres={genres}&exclude_tags={tags}";
        }

        /// <summary>
        /// Gelen oyun listesini manuel olarak tarayıp +18 içerikleri temizler.
        /// API filtresinden kaçan (tag girilmemiş) oyunlar için güvenlik kilidi.
        /// </summary>
        private List<Game> FilterNsfwClientSide(List<Game> games, bool showNsfw)
        {
            // Eğer NSFW açık ise filtreleme yapma, hepsini dön.
            if (showNsfw)
                return games;

            if (games == null || games.Count == 0)
                return new List<Game>();


            // Listeyi temizle
            games.RemoveAll(g =>
            {
                if (g == null) return true;

                // Adults Only kontrolü
                if (g.EsrbRating != null && (g.EsrbRating.Slug == "adults-only" || g.EsrbRating.Id == 5))
                    return true;

                // İsim Kontrolü
                if (!string.IsNullOrEmpty(g.Name))
                {
                    string nameLower = g.Name.ToLowerInvariant();
                    string[] badWords = { "hentai", "sex ", "porno", "nude", "erotic", "waifu hunter", "strip" };

                    if (badWords.Any(w => nameLower.Contains(w)))
                        return true;
                }

                // Genre ve Tag kontrolleri
                bool hasBadGenre = g.Genres != null && g.Genres.Any(gen => gen.Slug == "adult" || gen.Slug == "hentai");

                // Tags bazen null gelebilir
                bool hasBadTag = g.Tags != null && g.Tags.Any(t => t.Slug == "nudity" || t.Slug == "sexual-content");

                if (hasBadGenre || hasBadTag) return true;

                return false;
            });

            // To drastically reduce client render memory, we rewrite the original RAWG image urls
            // to cropped lightweight versions using their media server endpoint if available.
            foreach (var g in games) 
            {
                if (!string.IsNullOrEmpty(g.BackgroundImage) && g.BackgroundImage.Contains("media/games"))
                {
                    g.BackgroundImage = g.BackgroundImage.Replace("media/games", "media/crop/600/400/games");
                }
            }

            return games;
        }

        private async Task<List<Game>> GetGamesAsync(string url, bool showNsfw)
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

                return FilterNsfwClientSide(games, showNsfw);
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
        public async Task<List<Game>> GetGamesBySearchAsync(string searchTerm, bool showNsfw = false, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<Game>();

            string encodedSearch = Uri.EscapeDataString(searchTerm);
            // Crop to medium resolution for lists to save client bandwidth
            string url = $"{_baseUrl}/games?key={_apiKey}&search={encodedSearch}&page_size={pageSize}"; // Temel URL
            url += GetNsfwFilterParam(showNsfw); // NSFW Filtresini ekle 

            return await GetGamesAsync(url, showNsfw);
        }

        /// <summary>
        /// Popüler oyunları getirir
        /// </summary>
        public async Task<List<Game>> GetPopularGamesAsync(int pageNumber, bool showNsfw = false, int pageSize = 20)
        {
            var endDate = DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd"); // Gelecek 3 aya kadar
            var startDate = new DateTime(2024, 1, 1).ToString("yyyy-MM-dd"); // 2024 başından itibaren

            // Limit size of results to shrink json payload and download cropped images primarily
            string url = $"{_baseUrl}/games?key={_apiKey}&ordering=-released&dates={startDate},{endDate}&page_size={pageSize}&page={pageNumber}";

            // NSFW filtresini de ekle
            url += GetNsfwFilterParam(showNsfw);

            return await GetGamesAsync(url, showNsfw);
        }

        /// <summary>
        /// Keşfet sayfası için: genre, mode ve sayfalama desteğiyle oyun getirir.
        /// mode: trending | top_rated | new | genre
        /// </summary>
        public async Task<List<Game>> GetDiscoverGamesAsync(int? genreId, string mode, int page, bool showNsfw = false, int pageSize = 20)
        {
            string ordering = mode switch
            {
                "top_rated" => "-rating",
                "new" => "-released",
                "trending" => "-added",
                "metacritic_top" => "-metacritic",
                _ => "-added"
            };

            string url = $"{_baseUrl}/games?key={_apiKey}&ordering={ordering}&page_size={pageSize}&page={page}";

            // Belirli genre seçildiyse filtrele
            if (genreId.HasValue)
                url += $"&genres={genreId.Value}";

            // Trend ve yeni modunda tarih filtresi
            if (mode == "trending" || mode == "new")
            {
                var startDate = new DateTime(2023, 1, 1).ToString("yyyy-MM-dd");
                var endDate = DateTime.Now.AddMonths(4).ToString("yyyy-MM-dd");
                url += $"&dates={startDate},{endDate}";
            }

            // Metacritic modunda sadece yüksek skorluları getir
            if (mode == "metacritic_top")
                url += "&metacritic=75,100";

            url += GetNsfwFilterParam(showNsfw);

            return await GetGamesAsync(url, showNsfw);
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

                // Ekstra olarak oyunun mağaza URL'lerini ayrı bir endpointten çekip birleştiriyoruz
                try
                {
                    string storesUrl = $"{_baseUrl}/games/{gameId}/stores?key={_apiKey}";
                    var storesResponse = await _httpClient.GetStringAsync(storesUrl).ConfigureAwait(false);
                    var linkResult = JsonConvert.DeserializeObject<StoreLinkResponse>(storesResponse);
                    
                    if (linkResult?.Results != null && game.Stores != null)
                    {
                        foreach (var wrapper in game.Stores)
                        {
                            var matchedLink = linkResult.Results.Find(l => l.StoreId == wrapper.Store?.Id);
                            if (matchedLink != null && !string.IsNullOrEmpty(matchedLink.Url))
                            {
                                wrapper.Url = matchedLink.Url;
                            }
                        }
                    }
                }
                catch (Exception) { /* Hata varsa linkler boş kalsın sorun değil */ }

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