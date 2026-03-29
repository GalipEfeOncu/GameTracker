# GameTracker API (Backend)

## Hassas yapılandırma (sırlar)

**API anahtarları, veritabanı parolası ve e-posta şifresi repoda ve düz JSON dosyalarında tutulmamalı.** Önerilen yöntemler:

### Lokal geliştirme — .NET User Secrets (önerilen)

Proje zaten bir `UserSecretsId` kullanıyor; sırlar Windows’ta kullanıcı profili altında saklanır, **çalışma klasöründe görünmez** ve Git’e girmez.

`backend` klasöründe:

```bash
cd backend

dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "BURAYA_CONNECTION_STRING"
dotnet user-secrets set "ApiKeys:RawgApiKey" "BURAYA_RAWG_KEY"
dotnet user-secrets set "ApiKeys:GeminiApiKey" "BURAYA_GEMINI_KEY"
dotnet user-secrets set "EmailSettings:MailAddress" "ornek@mail.com"
dotnet user-secrets set "EmailSettings:MailPassword" "UYGULAMA_SIFRESI"
dotnet user-secrets set "Jwt:SigningKey" "EN_AZ_32_KARAKTER_UZUNLUGUNDA_GUVENLI_BIR_ANAHTAR"
```

**JWT:** `Jwt:SigningKey` HS256 için en az 32 karakter olmalıdır. Development’ta boş bırakılırsa API geçici bir anahtarla çalışır (konsolda uyarı). **Üretimde** mutlaka güçlü, rastgele bir anahtar verin; repoya ve düz `appsettings` dosyalarına yazmayın.

**Beni hatırla:** Giriş isteğinde `RememberMe: true` gönderilirse token süresi `Jwt:RememberMeAccessTokenMinutes` ile üretilir (varsayılan ~30 gün). Ayrı refresh token yok; güvenlik için ileride ayrı tasarlanmalıdır.

### CORS (SPA kökenleri)

Tarayıcı, farklı kökende barındırılan frontend’den API’ye istek atarken **CORS** köken listesi gerekir.

- **Development:** `Cors:AllowedOrigins` boşsa varsayılan olarak `http://localhost:5173` (ve birkaç Vite yedeği) kabul edilir.
- **Production:** `Cors:AllowedOrigins` **zorunludur**; boşsa uygulama başlamaz.

`appsettings` örneği (`appsettings.Example.json`):

```json
"Cors": {
  "AllowedOrigins": [ "https://sizin-spa-adresiniz.com" ]
}
```

Tek satırda birden fazla köken (ortam değişkeni için pratik):

```bash
Cors__AllowedOrigins=https://app.vercel.app;https://www.alanadiniz.com
```

Frontend üretim build’inde `VITE_API_BASE_URL` ile API adresini verin; ayrıntı: `frontend/.env.example`, `docs/FREE_STACK.md`.

Kayıtlı sırları listelemek (değerler maskelenir):

```bash
dotnet user-secrets list
```

`ASPNETCORE_ENVIRONMENT=Development` iken (`launchSettings.json` varsayılanı) user secrets otomatik yüklenir.

### Üretim / barındırma — ortam değişkenleri

Sunucuda anahtarları panelden veya secret store’dan **env** olarak verin (`__` iç içe anahtar için):

| Ortam değişkeni | Karşılığı |
|-------------------|-----------|
| `ConnectionStrings__GameTrackerDB` | SQL connection string |
| `ApiKeys__RawgApiKey` | RAWG API key |
| `ApiKeys__GeminiApiKey` | Gemini API key |
| `EmailSettings__MailAddress` | SMTP e-posta |
| `EmailSettings__MailPassword` | SMTP şifresi |
| `Jwt__SigningKey` | JWT imzalama anahtarı (≥32 karakter) |
| `Cors__AllowedOrigins` | İzinli SPA kökenleri (noktalı virgülle ayrılmış) veya `Cors__AllowedOrigins__0`, `__1`, … |

Şema için `appsettings.Example.json` (içine secret yazmayın). Ek: `docs/SECURITY.md`, `docs/FREE_STACK.md`, yerel SQL: `docs/LOCAL_DATABASE.md`.

## Çalıştırma

```bash
dotnet run
```

API varsayılan olarak `http://localhost:5118` adresinde çalışır. Swagger: `http://localhost:5118/swagger`.
