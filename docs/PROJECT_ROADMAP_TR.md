# GameTracker — Web yol haritası (Legacy karşılaştırmalı)

Bu belge **React + .NET API** yığını ile **WinForms (DevExpress) legacy** uygulamasının (`GameTracker/` — repoda yok, geliştirici makinesinde `.gitignore` ile kalabilir) özellik karşılaştırmasını, riskleri ve önerilen iş sırasını içerir.

**İlgili:** `backend/README.md`, `docs/SECURITY.md`, `docs/FREE_STACK.md`, `docs/LOCAL_DATABASE.md`

---

## 1. Özet tablo: Legacy → Web

| Alan | Legacy (WinForms) | Web (şu an) |
|------|-------------------|-------------|
| Oturum | Bellekte `Session` + giriş zorunlu ana akış | `localStorage` (`gt_user` + `accessToken`); **JWT** ile korumalı kütüphane ve profil uçları |
| Beni hatırla | DPAPI ile şifreli şifre + e-posta (`Settings`) | İsteğe bağlı; **uzun ömürlü access JWT** (giriş checkbox’ı) |
| Başlangıç sayfası | Home / Library (`StartPage`) | Ayarlarda var (`PreferencesContext` + `localStorage`) |
| NSFW içerik | `ShowNSFW` ayarı | Ayarlarda var; API `nsfw` query ile |
| Popüler / ana sayfa | RAWG, sayfalı “daha fazla” | `PopularPage`, sonsuz kaydırma |
| Arama | Ayrı sayfa, sayfalama | `DiscoverPage` + header `?q=` senkronu |
| Keşfet | Arama ile birleşik yapı | Tür + mod (trend, rating, metacritic…) |
| Kütüphane | Filtre: PlanToPlay, Playing, **Played**, Dropped | Sekmeler + **Completed**, **Wishlist** ekleri; isimler DB ile uyumlu olmalı |
| Kart etkileşimi | Sağ tık menü (kütüphane) | `GameCard` üç nokta menüsü |
| Oyun detayı | Form içi panel, geri | `/game/:id`; ekran görüntüleri, mağaza linkleri, gereksinimler |
| AI öneri | Gemini + RAWG eşleme | `AiSuggestionPage` + aynı backend |
| Ayarlar | Kullanıcı adı, şifre, başlangıç, NSFW, **önbellek temizle**, çıkış | Kullanıcı adı, şifre, başlangıç, NSFW, **hesap silme** (e-posta kodu) |
| Kayıt / e-posta | Legacy akışı basit | Kayıt + **e-posta doğrulama** + API |
| Şifre sıfırlama | — | `ForgotPasswordPage` + API |
| Görsel önbellek | `ImageManager` (RAM) | Tarayıcı / HTTP önbelleği |
| Düzen | `LayoutCalculator` pencere boyutuna göre kart sayısı | CSS grid (breakpoint) |
| Dağıtım | Tek .exe | Statik frontend + ayrı API host |

---

## 2. Legacy’de olup web’de olmayan veya zayıf olanlar

### 2.1 Oturum ve güvenlik

- **Kütüphane ve profil uçları JWT ile korunuyor;** giriş yanıtındaki `AccessToken` ile `Authorization: Bearer` gönderilir. Hâlâ iyileştirilebilir: refresh token, kısa ömürlü access token, şifre/username değişiminde token iptali.
- **Beni hatırla (DPAPI):** Web’de karşılığı yok; “oturumu açık tut” için refresh token veya uzun ömürlü cookie stratejisi düşünülebilir (güvenlik tasarımı şart).

### 2.2 Ürün özellikleri

- **Önbellek temizle:** Legacy’de görsel önbelleği temizleme; web’de bilinçli bir “cache temizle” yok (genelde gerekmez).
- **Sayfa numaralı listeleme:** Legacy’de önceki/sonraki sayfa ve sayfa etiketi; web tamamen infinite scroll (erişilebilirlik / “sayfa 5” ihtiyacı yoksa sorun değil).
- **Kütüphane durum isimleri:** Legacy DB/UI **Played** kullanır; web UI **Completed** ve **Wishlist** gösterir. Veritabanında eski `Played` kayıtları varsa etiket/ filtre uyumsuzluğu olabilir — **tek tip enum + migrasyon veya eşleme** gerekir.
- **Girişte sadece e-posta:** Legacy login e-posta formatı zorunlu tutuyor; web `EmailOrUsername` kabul ediyor (iyi taraf), davranış farkı bilinsin.

### 2.3 Teknik / mimari

- Legacy tek process içinde SQL + RAWG; web **REST** — deploy, CORS, `VITE_` API URL ayrı yönetilir.
- **E-posta doğrulama / şifre sıfırlama kodları** backend’de bellek store (`EmailVerificationStore`, `PasswordResetStore`); API yeniden başlayınca kodlar **sıfırlanır** — üretimde Redis veya DB tablosu düşünülmeli.

---

## 3. Bilinen hatalar ve potansiyel sorunlar

| Konu | Açıklama |
|------|-----------|
| **Boş connection string** | Kütüphane/kullanıcı SQL hataları; Development’ta konsolda uyarı loglanıyor. |
| **CORS** | Yapılandırılmış köken listesi (`Cors:AllowedOrigins`); `AllowAnyOrigin` kaldırıldı. |
| **Prod build** | `VITE_API_BASE_URL` veya aynı host üzerinden reverse proxy; `frontend/.env.example`. |
| **Status doğrulama** | Backend’de whitelist var; geçersiz status reddedilir. |
| **Gemini öneri sayısı** | Servis 20 isim ister; controller ~15 RAWG araması yapıyor — kasıtlı mı netleştirilmeli. |

---

## 4. Web’de tamamlanmış / hizalanmış olanlar (referans)

- Popüler liste mantığı (`-added`, `Added >= 3` kaldırıldı), boş/hata UI, `rawgConfigured`.
- RAWG / Gemini kurucu hatalarının kaldırılması (eksik anahtar 500 zinciri).
- User Secrets öncelikli gizli yapılandırma; `Layout` ↔ `?q=` senkronu.
- Geliştirme ortamında RAWG + DB connection string log özeti.
- Detay sayfası: kütüphane ekleme/güncelleme/silme, ekran görüntüleri, mağazalar, gereksinimler.
- Kayıt, e-posta doğrulama, şifre sıfırlama, profil güncelleme, hesap silme akışları (API + UI).
- `AiSuggestionPage`: `UserId` / `id` birleşik kullanımı.
- **Faz A (tamamlandı):** JWT (HS256, `Jwt:SigningKey`), kütüphane CRUD + kullanıcı profil/silme uçlarında `[Authorize]` ve route `userId` ↔ token `sub` eşlemesi; `LibraryStatuses` whitelist; frontend’de token saklama + axios `Authorization` + 401’de oturum temizleme ve login’e yönlendirme.
- **Faz B (kısmi):** `VITE_API_BASE_URL`, `VITE_DEV_PROXY_TARGET`, CORS `GameTrackerCors` politikası; bellek içi kod store uyarıları dokümante.
- **Faz C (kısmi):** Beni hatırla (uzun JWT), temel a11y, `/api/health` + `dotnet test`.
- **Faz D:** Popüler liste modu (sonsuz / sayfalı), Ayarlar’da performans bilgilendirmesi; kullanıcıya yönelik olmayan UI metinleri (ör. dotnet komutları) Popüler boş durumundan kaldırıldı.
- **Faz D sonrası (1–5):** Toast katmanı, skeleton yükleme, Error Boundary, API rate limiting (auth uçları), GitHub Actions CI.

---

## 5. Önerilen fazlar (öncelik sırası)

### Faz A — Güvenlik ve doğruluk (yüksek öncelik) — uygulandı

1. ~~**JWT** ile `/Library/user/{id}` ve profil uçlarının yalnızca token’daki kullanıcıya açılması.~~
2. ~~**Kütüphane status whitelist** (backend).~~ İsteğe bağlı: **Played ↔ Completed** migrasyon dokümantasyonu / tek seferlik SQL.

### Faz B — Üretim hazırlığı — uygulandı (kısmi)

4. ~~**`VITE_API_BASE_URL`** + `apiClient` tabanı; `frontend/.env.example`, `docs/FREE_STACK.md`.~~
5. ~~**CORS:** `Cors:AllowedOrigins` + Development varsayılanları; üretimde boş liste ile başlatma engeli.~~
6. Kalıcı store (Redis/DB) **yapılmadı**; **`docs/SECURITY.md`** ve **FREE_STACK** içinde bellek store + 15 dk + restart uyarısı netleştirildi. İleride Redis/DB.

### Faz C — Ürün iyileştirme — uygulandı (kısmi)

7. ~~**“Beni hatırla”:** daha uzun ömürlü access JWT (`Jwt:RememberMeAccessTokenMinutes`); refresh token / iptal listesi yok (ileride).~~
8. ~~Erişilebilirlik: `focus-visible`, atla bağlantısı, `aria-label` / `role` (Layout, Sidebar, giriş formu, kart menüsü, Escape ile menü kapatma).~~
9. ~~**API smoke:** `GET /api/health`, xUnit + `WebApplicationFactory` (`tests/GameTracker.Api.Tests`).~~

### Faz D — İnce / isteğe bağlı — uygulandı

10. ~~**Önbellek / performans:** Ayarlar’da kullanıcıya yönelik “Performans” metni (tarayıcı önbelleği, yenileme önerisi).~~
11. ~~**Sayfa numaralı mod:** Popüler sayfasında tercih — “Sonsuz kaydırma” veya “Sayfa sayfa” (`PreferencesContext.popularListMode`).~~

### Faz D sonrası — kullanıcı deneyimi ve profesyonellik

**Amaç:** Faz D sonrası 1–5 uygulandı; 6–7 ayrı sprint (oturum + kalıcı kod store).

| # | Konu | Kısa açıklama |
|---|------|----------------|
| 1 | ~~**Toast / bildirim katmanı**~~ | `ToastProvider`, `useToast`, `emitToast` (`gt-toast`); kütüphane detayı, ayarlar, 429 axios. |
| 2 | ~~**Skeleton (iskelet) yükleme**~~ | `GameCardSkeleton` / `GameCardSkeletonGrid`, `GameDetailSkeleton`; Popüler, Keşfet, Kütüphane, oyun detayı. |
| 3 | ~~**React Error Boundary**~~ | `ErrorBoundary` + `main.jsx` kök sarmalayıcı. |
| 4 | ~~**API rate limiting**~~ | `AddRateLimiter` + `UseRateLimiter`; `auth-forms` (30/dk/IP), `auth-destructive` (10/dk/IP); `UserController` kayıt/giriş/doğrulama/şifre + hesap silme. |
| 5 | ~~**CI pipeline**~~ | `.github/workflows/ci.yml`: `dotnet build` + `dotnet test`, `npm ci` + `npm run build` + `npm run lint`. |
| 6 | **Oturum (ileri seviye)** | Refresh token, çıkış / şifre değişiminde token iptal veya kısa ömürlü access JWT + yenileme; şu anki “uzun access JWT”nin üzerine güvenlik katmanı. |
| 7 | **Kalıcı geçici kod store** | E-posta doğrulama, şifre sıfırlama, hesap silme kodları için Redis veya DB tablosu (API restart’ta kod kaybı olmaması). |

---

## 6. Dosya haritası (web)

| Katman | Konum |
|--------|--------|
| API uçları | `backend/Controllers/` |
| RAWG / Gemini | `backend/Services/` |
| Kullanıcı / kütüphane DB | `backend/Managers/` |
| Frontend API | `frontend/src/api/apiClient.js` |
| Sayfalar | `frontend/src/pages/` |
| Navigasyon / shell | `frontend/src/components/Layout.jsx`, `Sidebar.jsx` |

---

*Son güncelleme: 2026-03-29 — Faz D sonrası 1–5 tamamlandı; 6–7 planlı sprint.*
