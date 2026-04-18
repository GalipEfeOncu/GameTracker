using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameTracker.Services
{
    public class GeminiService
    {
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";
        /// <summary>
        /// Gemini'den istenen ve LibraryController'ın IGDB ile eşleştirdiği oyun sayısı.
        /// Tek kaynak: prompt "EXACTLY {N}" + controller Take(N). Daha yüksek değer IGDB hız limitini
        /// (saniyede ~4 istek) zorlar; 15 önerilen yanıt süresiyle iyi dengededir.
        /// </summary>
        public const int RecommendationCount = 15;

        /// <summary>429 kota + 503 yoğunluk için yeniden deneme üst sınırı.</summary>
        private const int MaxGeminiRetries = 6;

        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(3) };

        private readonly string _apiKey;

        public GeminiService()
        {
            _apiKey = GameTracker.Api.AppConfig.GeminiApiKey ?? string.Empty;
        }

        public async Task<List<string>> GetRecommendationsAsync(List<string> userGames, CancellationToken ct = default)
        {
            if (userGames == null || userGames.Count == 0) return new List<string>();
            if (string.IsNullOrEmpty(_apiKey))
                return new List<string>();

            var modelId = GameTracker.Api.AppConfig.GeminiModel;
            string requestUrl = $"{BaseUrl}{modelId}:generateContent?key={_apiKey}";

            string gamesString = string.Join(", ", userGames);

            string prompt = $@"
            I like these games: {gamesString}

            Task:
            Recommend EXACTLY {RecommendationCount} similar games that I might like.

            STRICT RULES (DO NOT BREAK):
            - Recommend EXACTLY {RecommendationCount} games, no more, no less.
            - Do NOT recommend any game already in the list.
            - Return ONLY the full game names.
            - Separate game names with a comma and a single space.
            - Do NOT add numbering, line breaks, explanations, or any extra text.
            - If you cannot find enough games, invent NONE — think harder instead.

            VALID OUTPUT FORMAT EXAMPLE:
            Game A, Game B, Game C, Game D, Game E, Game F, Game G, Game H, Game I, Game J, Game K, Game L, Game M, Game N, Game O";

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

            try
            {
                for (var attempt = 0; attempt < MaxGeminiRetries; attempt++)
                {
                    ct.ThrowIfCancellationRequested();
                    var jsonPayload = JsonConvert.SerializeObject(requestBody);
                    using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    using var response = await HttpClient.PostAsync(requestUrl, httpContent, ct).ConfigureAwait(false);
                    string responseString = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        var friendly = BuildQuotaOrRateLimitMessage(responseString);
                        var waitSec = TryParseRetrySecondsFromGeminiBody(responseString);
                        if (attempt < MaxGeminiRetries - 1 && waitSec.HasValue)
                        {
                            var delayMs = (int)Math.Min((waitSec.Value + 2) * 1000, 120_000);
                            await Task.Delay(delayMs, ct).ConfigureAwait(false);
                            continue;
                        }

                        throw new Exception(friendly);
                    }

                    // 503/502/500 — Google tarafında geçici yoğunluk; kullanıcıya ham JSON göstermeden birkaç kez bekle-yeniden dene.
                    if (IsTransientGeminiOverload(response.StatusCode))
                    {
                        if (attempt < MaxGeminiRetries - 1)
                        {
                            await DelayForOverloadRetryAsync(response, attempt, ct).ConfigureAwait(false);
                            continue;
                        }

                        throw new Exception(
                            "Gemini sunucusu şu an yoğun talep nedeniyle geçici olarak yanıt veremedi (503). Birkaç dakika sonra yeniden deneyin; talep sıklığı genelde kısa sürelidir.");
                    }

                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"Gemini API Hatası ({response.StatusCode}): {TruncateForLog(responseString)}");

                    var json = JObject.Parse(responseString);

                    var blockReason = json["promptFeedback"]?["blockReason"]?.ToString();
                    if (!string.IsNullOrEmpty(blockReason))
                        throw new Exception($"Gemini istek engellendi ({blockReason}). Prompt veya güvenlik ayarlarını kontrol edin.");

                    var candArr = json["candidates"] as JArray;
                    var cand0 = candArr is { Count: > 0 } ? candArr[0] : null;
                    var finishReason = cand0?["finishReason"]?.ToString();
                    if (finishReason is "SAFETY" or "BLOCKLIST" or "PROHIBITED_CONTENT")
                        throw new Exception($"Gemini yanıtı güvenlik nedeniyle tamamlanmadı ({finishReason}). Tekrar deneyin veya kütüphanedeki başlıkları güncelleyin.");

                    var answerText = cand0?["content"]?["parts"]?[0]?["text"]?.ToString();
                    if (string.IsNullOrWhiteSpace(answerText))
                        throw new Exception("Gemini boş yanıt döndü; model çıktısı veya kota limitini kontrol edin.");

                    var parsed = ParseGameNamesFromGeminiText(answerText);
                    return parsed.Count <= RecommendationCount
                        ? parsed
                        : parsed.Take(RecommendationCount).ToList();
                }

                throw new Exception("Gemini çağrısı beklenmedik şekilde tamamlanmadı.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new Exception($"AI Servis Bağlantı Hatası: {ex.Message}");
            }
        }

        private static bool IsTransientGeminiOverload(HttpStatusCode status) =>
            status == HttpStatusCode.ServiceUnavailable // 503
            || status == HttpStatusCode.BadGateway // 502
            || status == HttpStatusCode.InternalServerError; // 500

        /// <summary>Üstel geri çekilme veya Retry-After başlığı (503 için).</summary>
        private static async Task DelayForOverloadRetryAsync(HttpResponseMessage response, int attempt, CancellationToken ct)
        {
            if (response.Headers.RetryAfter?.Delta is { TotalMilliseconds: > 0 } d)
            {
                var ms = Math.Min(d.TotalMilliseconds + 400, 90_000);
                await Task.Delay(TimeSpan.FromMilliseconds(ms), ct).ConfigureAwait(false);
                return;
            }

            var backoffMs = (int)Math.Min(2000 * Math.Pow(2, attempt), 35_000);
            await Task.Delay(backoffMs, ct).ConfigureAwait(false);
        }

        internal static string BuildQuotaOrRateLimitMessage(string geminiJsonBody)
        {
            try
            {
                var jo = JObject.Parse(geminiJsonBody);
                var msg = jo["error"]?["message"]?.ToString();
                var shortHint = "";
                if (!string.IsNullOrEmpty(msg))
                {
                    var m = Regex.Match(msg, @"retry in\s+([\d.]+)\s*s", RegexOptions.IgnoreCase);
                    if (m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var secs))
                        shortHint = $" Yaklaşık {Math.Ceiling(secs)} saniye sonra tekrar deneyebilirsiniz.";
                }

                return "Gemini API kota veya istek limitine takıldınız (429)." + shortHint +
                       " Ücretsiz kotayı aştıysanız Google AI Studio faturalandırmasını kontrol edin veya başka bir model seçin (ApiKeys__GeminiModel). Bilgi: https://ai.google.dev/gemini-api/docs/rate-limits";
            }
            catch
            {
                return "Gemini API kota veya istek limitine takıldınız (429). Bilgi: https://ai.google.dev/gemini-api/docs/rate-limits";
            }
        }

        /// <summary>
        /// Google RPC RetryInfo.retryDelay ("31s") veya mesaj içindeki "retry in Xs".
        /// </summary>
        internal static double? TryParseRetrySecondsFromGeminiBody(string geminiJsonBody)
        {
            double? best = null;
            try
            {
                var jo = JObject.Parse(geminiJsonBody);
                var msg = jo["error"]?["message"]?.ToString();
                if (!string.IsNullOrEmpty(msg))
                {
                    var mm = Regex.Match(msg, @"retry in\s+([\d.]+)\s*s", RegexOptions.IgnoreCase);
                    if (mm.Success && double.TryParse(mm.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture, out var secMsg))
                        best = secMsg;
                }

                if (jo["error"]?["details"] is JArray details)
                {
                    foreach (var d in details)
                    {
                        var rd = d?["retryDelay"]?.ToString();
                        if (!string.IsNullOrEmpty(rd) && rd.EndsWith('s') && rd.Length > 1)
                        {
                            var num = rd[..^1];
                            if (double.TryParse(num, System.Globalization.CultureInfo.InvariantCulture, out var secRd))
                                best = best.HasValue ? Math.Max(best.Value, secRd) : secRd;
                        }
                    }
                }

                return best is > 0 ? best : null;
            }
            catch
            {
                return best;
            }
        }

        private static string TruncateForLog(string s, int maxLen = 400)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var t = s.Trim();
            return t.Length <= maxLen ? t : t[..maxLen] + "…";
        }

        /// <summary>
        /// Virgül / satır sonu / noktalı virgül ile ayrılmış metni parse eder; numara ve markdown kalıntılarını temizler.
        /// </summary>
        internal static List<string> ParseGameNamesFromGeminiText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            var t = text.Replace("\r\n", "\n", StringComparison.Ordinal);
            var segments = t.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<string>();

            foreach (var raw in segments)
            {
                var s = raw.Trim();
                if (string.IsNullOrEmpty(s)) continue;
                s = Regex.Replace(s, @"^\s*\d+\s*[\.\)\-\:]\s*", "").Trim(); // "1. ", "2)", "3:"
                s = Regex.Replace(s, @"^\[\s*\d+\s*\]\s*", "").Trim(); // "[1]"
                s = s.Replace("**", "", StringComparison.Ordinal).Replace("*", "", StringComparison.Ordinal).Trim();
                s = s.Trim().Trim('"', '\'', '\u201C', '\u201D').Trim(); // curly quotes from models

                if (s.Length < 2 || !seen.Add(s))
                    continue;
                list.Add(s);
            }

            return list;
        }
    }
}
