using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using GameTracker.Api;
using GameTracker.Services;
using GameTracker; // for RawgApiService

var builder = WebApplication.CreateBuilder(args);

// Centralized config
AppConfig.Configuration = builder.Configuration;

// --- Services ---
builder.Services.AddControllers().AddNewtonsoftJson();

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

// CORS — frontend localden erişsin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Swagger (dev ortamında API'yi test etmek için)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
