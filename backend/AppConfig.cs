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
        public static string GeminiApiKey => Configuration?["ApiKeys:GeminiApiKey"];
        public static string MailAddress => Configuration?["EmailSettings:MailAddress"];
        public static string MailPassword => Configuration?["EmailSettings:MailPassword"];
    }
}
