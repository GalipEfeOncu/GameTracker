# Güvenlik ve Yapılandırma

## Hassas Bilgiler (Secrets)

Aşağıdaki değerler **asla** depoya commit edilmemelidir:

- Veritabanı connection string
- RAWG API anahtarı
- Gemini API anahtarı
- E-posta adresi ve uygulama şifresi (EmailSettings)

## Yapılandırma Yöntemleri

### 1. Ortam değişkenleri (Production önerilir)

Backend, .NET `IConfiguration` ile ortam değişkenlerini okur. Örnek (Render, Railway, Fly.io vb.):

| Ortam Değişkeni | Açıklama |
|-----------------|----------|
| `ConnectionStrings__GameTrackerDB` | SQL Server connection string |
| `ApiKeys__RawgApiKey` | RAWG API key |
| `ApiKeys__GeminiApiKey` | Google Gemini API key |
| `EmailSettings__MailAddress` | SMTP e-posta adresi |
| `EmailSettings__MailPassword` | SMTP uygulama şifresi |

Alt çizgi `__` bölüm (ConnectionStrings, ApiKeys) ile anahtarı (GameTrackerDB, RawgApiKey) ayırır.

### 2. Lokal geliştirme

- `backend/appsettings.Example.json` dosyasını `appsettings.Development.json` olarak kopyalayın.
- Gerçek değerleri `appsettings.Development.json` içine yazın.
- Bu dosya `.gitignore` ile takip dışındadır; commit edilmez.

### 3. .NET User Secrets (isteğe bağlı)

Lokal için alternatif:

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "your_connection_string"
dotnet user-secrets set "ApiKeys:RawgApiKey" "your_key"
```

## Depodaki Dosyalar

- `appsettings.json`: Yapı yalnızca boş/placeholder; gerçek secret **içermemeli**.
- `appsettings.Example.json`: Şablon; gerçek değer yazılmadan commit edilebilir.

## SQL Server bağlantısı (Somee / uzak sunucu)

- **Backend (.NET 9):** `Network Library=DBMSSOCN` **kullanmayın** — Microsoft.Data.SqlClient bu anahtarı desteklemez. Bunun yerine `Server=tcp:SunucuAdi,1433;...` formatını kullanın. Somee’den connection string’i nasıl alıp bu formata çevireceğiniz: **`docs/SOMEE_CONNECTION_STRING.md`**.
- **Masaüstü (WinForms):** Eski .NET Framework için `SecretString.config` içinde `Network Library=DBMSSOCN` kullanılabilir; örnek: `GameTracker/SecretString.config.example`.

Sunucunun erişilebilir olduğundan (internet, firewall, Somee panelinde veritabanı açık) emin olun.
