using Microsoft.Extensions.Configuration;

namespace GameTracker.Api
{
    public static class AppConfig
    {
        public static IConfiguration Configuration { get; set; }

        public static string ConnectionString => Configuration?.GetConnectionString("GameTrackerDB");
        public static string RawgApiKey => Configuration?["ApiKeys:RawgApiKey"];
        public static string GeminiApiKey => Configuration?["ApiKeys:GeminiApiKey"];
        public static string MailAddress => Configuration?["EmailSettings:MailAddress"];
        public static string MailPassword => Configuration?["EmailSettings:MailPassword"];
    }
}
