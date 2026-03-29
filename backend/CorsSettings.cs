using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GameTracker.Api;

/// <summary>
/// CORS origin listesi: appsettings <c>Cors:AllowedOrigins</c> dizi veya noktalı virgül/virgülle ayrılmış tek satır;
/// Development’ta boşsa Vite varsayılan portları kullanılır.
/// </summary>
public static class CorsSettings
{
    private static readonly string[] DefaultDevelopmentOrigins =
    {
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:5174",
        "http://127.0.0.1:5174",
    };

    public static string[] ResolveAllowedOrigins(IConfiguration configuration, IHostEnvironment environment)
    {
        var section = configuration.GetSection("Cors:AllowedOrigins");
        var fromArray = section.Get<string[]>();
        if (fromArray is { Length: > 0 })
            return fromArray;

        var csv = configuration["Cors:AllowedOrigins"];
        if (!string.IsNullOrWhiteSpace(csv))
        {
            return csv.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        if (environment.IsDevelopment())
            return DefaultDevelopmentOrigins;

        return Array.Empty<string>();
    }
}
