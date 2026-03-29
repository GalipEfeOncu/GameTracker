# Güvenlik ve Yapılandırma

## Hassas Bilgiler (Secrets)

Aşağıdaki değerler **asla** depoya commit edilmemelidir:

- Veritabanı connection string
- RAWG API anahtarı
- Gemini API anahtarı
- E-posta adresi ve uygulama şifresi (EmailSettings)
- JWT imzalama anahtarı (`Jwt:SigningKey` / `Jwt__SigningKey`)

## Yapılandırma Yöntemleri

### 1. Lokal geliştirme — .NET User Secrets (önerilen)

Sırlar repoda ve proje klasöründeki dosyalarda tutulmaz; Windows’ta kullanıcı gizli deposuna yazılır.

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "your_connection_string"
dotnet user-secrets set "ApiKeys:RawgApiKey" "your_rawg_key"
dotnet user-secrets set "ApiKeys:GeminiApiKey" "your_gemini_key"
dotnet user-secrets set "EmailSettings:MailAddress" "your@email"
dotnet user-secrets set "EmailSettings:MailPassword" "your_app_password"
dotnet user-secrets set "Jwt:SigningKey" "your_random_key_at_least_32_chars"
```

HS256 için `Jwt:SigningKey` en az 32 karakter olmalıdır. Development’ta boş bırakılırsa API geçici bir anahtar kullanır; üretimde zorunludur.

`Development` ortamında `dotnet run` bu değerleri otomatik okur (`backend/README.md`).

### 2. Ortam değişkenleri (üretim / CI önerilir)

Backend, .NET `IConfiguration` ile ortam değişkenlerini okur. Örnek (Render, Railway, Fly.io vb.):

| Ortam Değişkeni | Açıklama |
|-----------------|----------|
| `ConnectionStrings__GameTrackerDB` | SQL Server connection string |
| `ApiKeys__RawgApiKey` | RAWG API key |
| `ApiKeys__GeminiApiKey` | Google Gemini API key |
| `EmailSettings__MailAddress` | SMTP e-posta adresi |
| `EmailSettings__MailPassword` | SMTP uygulama şifresi |
| `Jwt__SigningKey` | JWT HS256 imza anahtarı (≥32 karakter) |

Alt çizgi `__` bölüm (ConnectionStrings, ApiKeys) ile anahtarı (GameTrackerDB, RawgApiKey) ayırır.

**CORS:** Üretimde SPA kökenlerini `Cors:AllowedOrigins` veya `Cors__AllowedOrigins` ile verin (`backend/README.md`).

**Geçici doğrulama kodları:** E-posta doğrulama, şifre sıfırlama ve hesap silme kodları şu an **uygulama belleğinde** tutulur; süre **15 dakika**, **yeniden başlatmada sıfırlanır**. Küçük dağıtımlarda kabul edilebilir; kullanıcı artan bir hizmette **Redis**, **SQL tablosu** veya e-posta sağlayıcısının kendi akışı tercih edilir.

### 3. `appsettings.Development.json` (tercih edilmez)

Git’e commit edilmez ancak disk üzerinde düz metindir. **Sırları buraya yazmak yerine User Secrets kullanın.** Yalnızca `Logging` gibi hassas olmayan yerel ayarlar için kullanılabilir. Şema için `appsettings.Example.json` referans alın; gerçek değer yazmayın.

## Depodaki Dosyalar

- `appsettings.json`: Yapı yalnızca boş/placeholder; gerçek secret **içermemeli**.
- `appsettings.Example.json`: Şablon; gerçek değer yazılmadan commit edilebilir.

## SQL Server bağlantısı (Somee / uzak sunucu)

- **Backend (.NET 9):** `Network Library=DBMSSOCN` **kullanmayın** — Microsoft.Data.SqlClient bu anahtarı desteklemez. Bunun yerine `Server=tcp:SunucuAdi,1433;...` formatını kullanın. Somee’den connection string’i nasıl alıp bu formata çevireceğiniz: **`docs/SOMEE_CONNECTION_STRING.md`**.
- **Masaüstü (WinForms):** Eski .NET Framework için `SecretString.config` içinde `Network Library=DBMSSOCN` kullanılabilir; örnek: `GameTracker/SecretString.config.example`.

Sunucunun erişilebilir olduğundan (internet, firewall, Somee panelinde veritabanı açık) emin olun.
