using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameTracker.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private const string ModelId = "gemini-2.5-flash-lite";
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        private static readonly HttpClient httpClient = new HttpClient();

        public GeminiService()
        {
            _apiKey = System.Configuration.ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("Gemini API Key bulunamadı! Secrets.config dosyasını kontrol et.");
        }

        public async Task<List<string>> GetRecommendationsAsync(List<string> userGames)
        {
            if (userGames == null || userGames.Count == 0) return new List<string>();

            // URL'yi oluşturuyoruz
            string requestUrl = $"{BaseUrl}{ModelId}:generateContent?key={_apiKey}";

            string gamesString = string.Join(", ", userGames);

            // Prompt ayarı
            string prompt = $@"
            I like these games: {gamesString}. 
            Based on this list, recommend 5 similar games that I might like. 
            Rules:
            1. Do not recommend games already in the list.
            2. Return ONLY the game names separated by a comma. 
            3. Do not write any intro or outro text.
            Example Output: The Witcher 3, Cyberpunk 2077, Hades, God of War, Starfield";

            // JSON Gövdesi (Google'ın istediği tam format)
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            // Newtonsoft ile serialize et (Hatasız çevirir)
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(requestUrl, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Hata detayını yakala
                    throw new Exception($"Gemini API Hatası ({response.StatusCode}): {responseString}");
                }

                // Gelen cevabı parçala
                JObject json = JObject.Parse(responseString);

                // İçerikteki metni bul (candidates -> content -> parts -> text)
                string answerText = (string)json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"];

                if (string.IsNullOrEmpty(answerText)) return new List<string>();

                // Temizle ve listeye çevir
                var recommendations = new List<string>();
                foreach (var game in answerText.Split(','))
                {
                    string cleanName = game.Trim().Replace("\n", "").Replace("*", ""); // Markdown karakterlerini temizle
                    if (!string.IsNullOrWhiteSpace(cleanName))
                        recommendations.Add(cleanName);
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                // Hatayı çağıran yere fırlat ki MessageBox ile göstersin
                throw new Exception($"AI Servis Bağlantı Hatası: {ex.Message}");
            }
        }
    }
}