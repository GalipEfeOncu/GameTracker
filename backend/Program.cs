using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using GameTracker.Api;
using GameTracker.Api.Auth;
using GameTracker.Services;
using GameTracker; // for RawgApiService

var builder = WebApplication.CreateBuilder(args);

var corsOrigins = CorsSettings.ResolveAllowedOrigins(builder.Configuration, builder.Environment);
if (corsOrigins.Length == 0 && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Üretim ortamında CORS kökenleri tanımlanmalıdır. Cors:AllowedOrigins (dizi veya noktalı virgülle ayrılmış liste) veya ortamda Cors__AllowedOrigins kullanın. Ayrıntı: backend/README.md.");
}

// Centralized config
AppConfig.Configuration = builder.Configuration;

// --- Services ---
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await ctx.HttpContext.Response.WriteAsJsonAsync(
            new { message = "Çok fazla istek. Lütfen kısa süre sonra tekrar deneyin." },
            ct);
    };
    options.AddPolicy(RateLimitPolicies.AuthForms, RateLimitPolicies.AuthFormsPartition);
    options.AddPolicy(RateLimitPolicies.AuthDestructive, RateLimitPolicies.AuthDestructivePartition);
});

// DI: Scoped services — her HTTP isteği için yeni instance
builder.Services.AddScoped<RawgApiService>();
builder.Services.AddScoped<GeminiService>();
// EmailService is static, so we don't register it in DI.

// Gzip sıkıştırma — API yanıtları ~70% küçülür, daha hızlı gelir
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("GameTrackerCors", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger (dev ortamında API'yi test etmek için)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameTracker API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT: Authorization başlığına `Bearer {token}` yazın (giriş yanıtındaki AccessToken).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    c.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var rawgOk = !string.IsNullOrWhiteSpace(AppConfig.RawgApiKey);
    app.Logger.LogInformation("RAWG API anahtarı yapılandırılmış: {Configured}", rawgOk);
    if (!rawgOk)
        app.Logger.LogWarning("Popüler/Keşfet için ApiKeys:RawgApiKey tanımlayın (önerilen: dotnet user-secrets; üretim: ortam değişkeni ApiKeys__RawgApiKey).");

    var dbOk = !string.IsNullOrWhiteSpace(AppConfig.ConnectionString);
    app.Logger.LogInformation("Veritabanı connection string tanımlı: {Configured}", dbOk);
    if (!dbOk)
        app.Logger.LogWarning("ConnectionStrings:GameTrackerDB boş. Kayıt, giriş ve kütüphane uçları veritabanına erişemez; user-secrets veya ConnectionStrings__GameTrackerDB env kullanın.");

    app.Logger.LogInformation("CORS izinli kökenler: {Origins}", string.Join(", ", corsOrigins));
}

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("GameTrackerCors");
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Integration testlerde WebApplicationFactory için işaret sınıfı.</summary>
public partial class Program { }
