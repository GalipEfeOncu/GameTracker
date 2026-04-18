using Microsoft.Extensions.Configuration;

namespace GameTracker.Api
{
    public static class AppConfig
    {
        public static IConfiguration Configuration { get; set; }

        public static string? ConnectionString => Configuration?.GetConnectionString("GameTrackerDB");

        /// <summary>Boş veya yalnızca boşluk connection string ile SQL çağrısı yapılmaması için.</summary>
        public static bool IsDatabaseConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
        public static string RawgApiKey => Configuration?["ApiKeys:RawgApiKey"];
        public static string? IgdbClientId => Configuration?["Igdb:ClientId"];
        public static string? IgdbClientSecret => Configuration?["Igdb:ClientSecret"];

        /// <summary>IGDB (Twitch) client id + secret tanımlı mı.</summary>
        public static bool IsIgdbConfigured =>
            !string.IsNullOrWhiteSpace(IgdbClientId) && !string.IsNullOrWhiteSpace(IgdbClientSecret);

        public static string GeminiApiKey => Configuration?["ApiKeys:GeminiApiKey"];

        /// <summary>Gemini model id (generateContent). Kota model bazlıdır — env: ApiKeys__GeminiModel.</summary>
        /// <remarks>Varsayılan: gemini-2.5-flash-lite (yüksek hacim / düşük gecikme; preview modeller sık 503 verebilir).</remarks>
        public static string GeminiModel
        {
            get
            {
                var m = Configuration?["ApiKeys:GeminiModel"];
                return string.IsNullOrWhiteSpace(m) ? "gemini-2.5-flash-lite" : m.Trim();
            }
        }

        public static bool IsGeminiConfigured => !string.IsNullOrWhiteSpace(GeminiApiKey);
        public static string MailAddress => Configuration?["EmailSettings:MailAddress"];
        public static string MailPassword => Configuration?["EmailSettings:MailPassword"];

        /// <summary>SMTP sunucusu; boşsa Gmail (smtp.gmail.com).</summary>
        public static string SmtpHost
        {
            get
            {
                var h = Configuration?["EmailSettings:SmtpHost"];
                return string.IsNullOrWhiteSpace(h) ? "smtp.gmail.com" : h.Trim();
            }
        }

        /// <summary>SMTP portu; varsayılan 587.</summary>
        public static int SmtpPort
        {
            get
            {
                var raw = Configuration?["EmailSettings:SmtpPort"];
                return int.TryParse(raw, out var p) && p > 0 ? p : 587;
            }
        }

        /// <summary>E-posta göndermek için adres + şifre tanımlı mı (Render üretim kontrolü).</summary>
        public static bool IsEmailConfigured =>
            !string.IsNullOrWhiteSpace(MailAddress) && !string.IsNullOrWhiteSpace(MailPassword);
    }
}
