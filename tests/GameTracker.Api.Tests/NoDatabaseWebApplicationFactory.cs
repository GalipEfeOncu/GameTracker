using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace GameTracker.Api.Tests;

/// <summary>
/// Development + user-secrets ile gelen gerçek connection string'i devre dışı bırakmak için
/// <see cref="Environments.Production"/> kullanır (CreateDefaultBuilder yalnızca Development'ta user-secrets ekler).
/// <see cref="IWebHostBuilder.UseSetting"/> değerleri <c>Program.cs</c> içindeki CORS/JWT okumasından önce uygulanır.
/// </summary>
public class NoDatabaseWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);
        builder.UseSetting("ConnectionStrings:GameTrackerDB", string.Empty);
        builder.UseSetting("Jwt:SigningKey", "TEST_FACTORY_JWT_SIGNING_KEY_32_CHARS_MIN!");
        builder.UseSetting("Cors:AllowedOrigins", "http://localhost");
    }
}
