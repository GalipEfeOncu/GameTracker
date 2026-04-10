# Deploy ve yapılandırma (kişisel notlar)

API ve SPA’yı canlıya alırken ve sırları yönetirken kullanacağın özet. Detaylı özellik listesi: [FEATURES.md](./FEATURES.md).

---

## Repoda tutulmaması gerekenler

Connection string, RAWG/Gemini, **IGDB (Twitch Client ID + secret)**, SMTP ve JWT imza anahtarı commit edilmemeli. Şema için `backend/appsettings.Example.json`; gerçek değerler yalnızca user-secrets (lokal) veya barındırma panelindeki env (üretim).

**IGDB:** [Twitch Developer Console](https://dev.twitch.tv/console/apps) üzerinde uygulama oluştur; **Client ID** ve **Client Secret** al. Bu kimlikler IGDB API v4 için OAuth `client_credentials` token üretiminde kullanılır (oyun listeleri / arama). Detay sayfasında platform ve mağaza için **RAWG anahtarı** ayrıca gerekir.

---

## Lokal — User Secrets

`backend` klasöründe:

```bash
cd backend

dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "BURAYA_CONNECTION_STRING"
dotnet user-secrets set "ApiKeys:RawgApiKey" "BURAYA_RAWG_KEY"
dotnet user-secrets set "Igdb:ClientId" "TWITCH_CLIENT_ID"
dotnet user-secrets set "Igdb:ClientSecret" "TWITCH_CLIENT_SECRET"
dotnet user-secrets set "ApiKeys:GeminiApiKey" "BURAYA_GEMINI_KEY"
dotnet user-secrets set "EmailSettings:MailAddress" "ornek@mail.com"
dotnet user-secrets set "EmailSettings:MailPassword" "UYGULAMA_SIFRESI"
dotnet user-secrets set "Jwt:SigningKey" "EN_AZ_32_KARAKTER_UZUNLUGUNDA_GUVENLI_BIR_ANAHTAR"
```

- `Jwt:SigningKey` HS256 için en az 32 karakter; üretimde güçlü rastgele değer.
- Geliştirmede boş JWT anahtarı geçici anahtar üretir (konsolda uyarı).
- **Beni hatırla:** `RememberMe: true` ile token süresi `Jwt:RememberMeAccessTokenMinutes` (refresh token yok).

Listelemek: `dotnet user-secrets list`

---

## Üretim — ortam değişkenleri

`__` iç içe yapılandırma anahtarlarını ayırır.

| Ortam değişkeni | Karşılığı |
|-----------------|-----------|
| `ConnectionStrings__GameTrackerDB` | SQL connection string |
| `ApiKeys__RawgApiKey` | RAWG (detay tamamlayıcı) |
| `Igdb__ClientId` | Twitch uygulama Client ID (IGDB) |
| `Igdb__ClientSecret` | Twitch Client Secret (IGDB) |
| `ApiKeys__GeminiApiKey` | Gemini (isteğe bağlı) |
| `EmailSettings__MailAddress` | SMTP |
| `EmailSettings__MailPassword` | SMTP şifresi |
| `Jwt__SigningKey` | JWT (≥32 karakter) |
| `Cors__AllowedOrigins` | SPA kökenleri; noktalı virgülle veya `Cors__AllowedOrigins__0`, `__1`, … |

---

## CORS

- **Development:** `Cors:AllowedOrigins` boşsa varsayılan olarak `localhost:5173` (ve birkaç Vite portu) kabul edilir.
- **Production:** izinli kökenler **zorunlu**; boşsa API başlamaz.

Örnek env:

```bash
Cors__AllowedOrigins=https://senin-spa-adresin.com
```

---

## Frontend build — `VITE_API_BASE_URL`

Üretim build’inde API kökünü ver; **sonunda `/api` olmalı** (örn. `https://senin-api.onrender.com/api`). Boş bırakırsan yalnızca Vite dev + proxy ile `/api` çalışır. Şablon: `frontend/.env.example`.

**Vercel (React Router):** `/popular` gibi yollarda sayfa yenilenince 404 almamak için `frontend/vercel.json` içinde tüm istekler `index.html`’e yönlendirilir (SPA rewrite).

---

## Barındırma

Backend ve frontend ayrı servislerde; panelde yukarıdaki env’leri tanımla, frontend için build komutu `npm run build`, çıktı `dist`. Uyku modu olan ücretsiz planlarda ilk istek gecikebilir — beklenen.

### Render.com — API (Docker)

Render’da “Language” listesinde .NET yok; **Docker** seçilir. Repoda `backend/Dockerfile` kullanılır.

| Alan | Değer |
|------|--------|
| **Runtime** | Docker |
| **Root Directory** | `backend` |
| **Dockerfile Path** | `Dockerfile` |
| **Build / Start** | Boş bırakılabilir (Dockerfile `ENTRYPOINT` ile çalışır) |

Ortam değişkenleri yine `ConnectionStrings__GameTrackerDB`, `Igdb__*`, `Cors__AllowedOrigins` vb. Üretimde **CORS** dolu olmalı (bkz. yukarı).

---

## E-posta doğrulama / şifre sıfırlama / hesap silme kodları

Şu an **bellek içi**; süre yaklaşık **15 dakika**, API **yeniden başlayınca** kodlar gider. Trafiğin artarsa Redis veya küçük bir DB tablosu düşünürsün.

---

## `appsettings.Development.json`

Git’e genelde girmez; yine de sırları buraya yazma — user-secrets kullan.
