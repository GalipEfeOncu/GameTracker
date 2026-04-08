# GameTracker — Özellikler (web)

React + .NET API ile oyun kütüphanesi, RAWG verisi ve isteğe bağlı Gemini önerileri.

> **Not:** WinForms / DevExpress **legacy** sürüm bu repoda yok. Aşağıdaki maddeler yalnızca web uygulamasına aittir.

**İçindekiler:** [Yığın](#teknoloji-yığını) · [Hesap](#kimlik-doğrulama-ve-hesap) · [Kütüphane](#kütüphane) · [RAWG](#oyun-keşfi-ve-içerik-rawg) · [AI](#ai-önerileri) · [Ayarlar](#ayarlar-ve-tercihler) · [API](#api-ve-güvenlik-sunucu) · [UI](#ön-yüz-kalitesi) · [CI](#ci) · [Deploy](#dağıtım) · [Legacy](#legacy-karşılaştırması)

---

## Teknoloji yığını

| Katman | Teknoloji |
|--------|-----------|
| **API** | .NET 9 Web API, Newtonsoft.Json, Swagger (geliştirme), Problem Details, merkezi exception handler |
| **Veri** | Microsoft SQL Server · `LibraryManager` / `UserManager` |
| **Harici API** | RAWG (liste, detay, ekran görüntüleri, keşfet) · Google Gemini (öneri — isteğe bağlı anahtar) |
| **Ön yüz** | React 19, Vite, axios, React Router |
| **Kimlik** | JWT Bearer (HS256) · `Authorization: Bearer` |

---

## Kimlik doğrulama ve hesap

| Özellik | Açıklama |
|---------|----------|
| Kayıt | E-posta, kullanıcı adı, parola · sonrasında **e-posta doğrulama kodu** |
| Giriş | **E-posta veya kullanıcı adı** + parola · yanıtta **access token** · **Beni hatırla** → daha uzun ömürlü JWT |
| Şifre | **Şifremi unuttum** · e-posta kodu + yeni parola |
| Oturum | `localStorage` (`gt_user`, `accessToken`) · 401 → temizleme ve girişe yönlendirme |
| Profil | Kullanıcı adı ve parola güncelleme (JWT) |
| Hesap silme | E-posta kodu ile onay · JWT + rate limit |

---

## Kütüphane

- Durumlar: **PlanToPlay**, **Playing**, **Completed**, **Dropped**, **Wishlist** (backend whitelist).
- CRUD: ekleme, kaldırma, durum güncelleme — yalnızca token’daki kullanıcı kendi `userId` yoluna erişir.
- Arayüz: sekmeler · kartlarda üç nokta menüsü (legacy sağ tık menüsünün karşılığı).

---

## Oyun keşfi ve içerik (RAWG)

- **Popüler** — Sonsuz kaydırma veya **sayfa sayfa** (`PreferencesContext`).
- **Keşfet** — Tür + sıralama (trend, rating, metacritic vb.).
- **Arama** — Üst çubuk sorgusu · URL `?q=` ile senkron.
- **Oyun detayı** — `/game/:id`: açıklama, görseller, mağazalar, gereksinimler; kütüphane ekleme / güncelleme / silme.
- **NSFW** — Ayar + API `nsfw` query.
- RAWG yoksa veya hata: kullanıcıya boş / hata durumları.

---

## AI önerileri

- Sayfa: **AiSuggestionPage** — kütüphaneye göre Gemini + RAWG eşlemesi (`POST .../recommendations`).
- Kütüphane boşken: API ve UI davranışı uyumlu.

---

## Ayarlar ve tercihler

- Başlangıç sayfası (Home / Library).
- NSFW gösterimi.
- Popüler liste modu: sonsuz / sayfalı.
- Performans metni (tarayıcı önbelleği, yenileme önerisi).
- Hesap silme akışı.

---

## API ve güvenlik (sunucu)

| Konu | Davranış |
|------|----------|
| CORS | `GameTrackerCors` · üretimde izinli kökenler zorunlu |
| Rate limit | Auth formları ve yıkıcı işlemler · 429 JSON |
| Sıkıştırma | Gzip (HTTPS ile) |
| Health | `GET /api/health` |
| DB yok | Connection string yoksa yapılandırılmış hata (exception handler) |
| Testler | xUnit · `WebApplicationFactory` · health ve DB senaryoları |

---

## Ön yüz kalitesi

- **Toast** — kütüphane, ayarlar, 429 vb.
- **Skeleton** — kart grid, detay sayfası.
- **Error Boundary** — kök sarmalayıcı.
- **Erişilebilirlik** — `focus-visible`, atla bağlantısı, `aria-label` / `role`, Escape ile menü kapatma.

---

## CI

GitHub Actions: `dotnet build` · `dotnet test` · `npm ci` · `npm run build` · `npm run lint`.

---

## Dağıtım

- Üretim: `VITE_API_BASE_URL` (köke **`/api` dahil**) · geliştirme: Vite proxy.
- Sırlar, CORS, env → [DEPLOY.md](./DEPLOY.md) · veritabanı → [DATABASE.md](./DATABASE.md).

---

## Legacy karşılaştırması

| Alan | Legacy (WinForms) | Web |
|------|-------------------|-----|
| Oturum | Bellek + zorunlu giriş | JWT + `localStorage` |
| Dağıtım | Tek `.exe` | Statik SPA + ayrı API |
| Liste | Sayfalama | Popüler: sonsuz veya sayfalı · kütüphane grid |

Planlanan işler: [ROAD_MAP.md](./ROAD_MAP.md).
