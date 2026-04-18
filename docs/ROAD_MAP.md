# GameTracker — Yol haritası

> **AI ajansları:** Bu dosyaya bakmadan önce [AGENT_HANDOFF.md](./AGENT_HANDOFF.md)'i oku — komutlar, bilinen bug'lar ve kapsam dışı liste orada.

Henüz yapılmamış veya netleştirilmesi gereken işler. Tamamlanan özellikler: [FEATURES.md](./FEATURES.md).

**Sıradaki odak (MVP sonrası, isteğe bağlı):** GitHub’da sürüm etiketi + elde `desktop` build’i Release’e ekleme; CI’de Windows runner artefaktı ve Authenticode **kapsam dışı** (hobi). **§6 imzasız NSIS** — tamamlandı (`assets/`, `npm run build` → `dist-desktop/`). **§7 UI dili (TR + EN)** — tamamlandı. Proje MVP: sanal liste, kod imzalama, auto-update **kapsam dışı**.

**Referans:** [DEPLOY.md](./DEPLOY.md) · [DATABASE.md](./DATABASE.md) · [PLAN_WEB_DESKTOP.md](./PLAN_WEB_DESKTOP.md) · `backend/README.md`

---

## Nasıl kullanılır?

- `[ ]` **Açık** — yapılacak iş.
- `[x]` **Kapalı** — tamamlandı veya karar verilip bırakıldı.
- Bölümler **yapılma sırasına** göre numaralandırıldı; önce kalan PoC borcu, sonra ürün kritik (güvenlik, veri modeli), sonra masaüstünün varlık nedeni (desktop özellikleri), sonra dağıtım altyapısı, en sonda web UX ve kalite cilası.

**İçindekiler:** [Özet sıra](#önerilen-yapılma-sırası-özet) · [1. API (tamam)](#1-oyun-veri-kaynağı-api--tamamlandı) · [2. Faz A kapanış](#2-faz-a-kapanış-desktop-poc) · [3. Güvenlik](#3-güvenlik--oturum) · [4. Veri modeli](#4-veri-modeli--uyumluluk) · [5. Desktop özellikleri](#5-desktop-yüklü-oyun--oynama-süresi) · [6. Masaüstü dağıtım](#6-masaüstü-dağıtım-faz-bcd) · [7. Web UX + i18n](#7-web-ürün--ux) · [8. Kalite](#8-kalite--operasyon)

---

## Önerilen yapılma sırası (özet)

| # | Başlık | Durum | Neden bu sırada |
|---|--------|-------|-----------------|
| 1 | Oyun veri API'si (IGDB + RAWG hibrit) | ~~Tamam~~ | Temel. |
| 2 | Faz A kapanış (paketli duman testi, `HashRouter`) | ~~Tamam~~ | Desktop PoC borcu kapatıldı. |
| 3 | Güvenlik (refresh token, kalıcı kod deposu) | ~~Tamam~~ | Access 15 dk + rotasyonlu refresh; üretim kullanıcısı için kritik kırılabilir değişiklik artık oturdu. |
| 4 | Veri modeli (Played ↔ Completed, Gemini sayısı) | ~~Tamam~~ | `LibraryStatuses.Normalize` + migration; prompt ve `Take()` tek sabitten. |
| 5 | Desktop özellikleri (yüklü oyun algılama + oynama süresi) | ~~Tamam~~ | Steam/Epic/GOG algılama + manuel ekleme; arka planda tanınan process için 30 sn'de bir backend'e playtime heartbeat. Tray + autostart çalışıyor. |
| 6 | **Masaüstü dağıtım** (imzasız NSIS installer) | ~~Tamam~~ | `electron-builder` NSIS; `assets/icon.ico` + `tray.png`; kök `README` indirme bölümü. İmza / auto-update kapsam dışı. |
| 7 | Web UX + **i18n (UI)** | ~~Tamam~~ | Türkçe varsayılan + İngilizce; `tr.json`/`en.json`, `PreferencesContext.locale`, Ayarlar. İçerik dili (IGDB localization) hâlâ kapalı; bkz. §7. |
| 8 | ~~Kalite cilası~~ | İptal | Kapsam dışı — mevcut lint + CI yeterli. |

---

## 1. Oyun veri kaynağı (API) — tamamlandı

**Durum:** Üretimde **hibrit IGDB + RAWG**. "Tek API" ideali bilinçli olarak bırakıldı (maliyet/kota/eksik alan dengesi).

- [x] **Uygulama planı** — [PLAN_IGDB_RAWG_HYBRID.md](./PLAN_IGDB_RAWG_HYBRID.md).
- [x] **Araştırma (IGDB + alternatifler)** — alan haritası aşağıdaki tabloda.
- [x] **Hedef veri modeli** — kapak, ekran görüntüleri, açıklama, platformlar, mağaza linkleri, sistem gereksinimleri, Metacritic skoru.
- [x] **RAWG strateji kararı** — kalıcı hibrit.

### IGDB alan özeti

| İstenen | IGDB | Not |
|---------|------|-----|
| Kapak / ekran görüntüleri / açıklama | **Var** | `Cover`, `screenshots`, `summary`, `storyline`. |
| Sistem gereksinimleri (PC min/önerilen) | **Yok** | Oyun bazlı alan yok → RAWG'den. |
| Metacritic skoru (resmi) | **Yok** | `aggregated_rating` farklı bir metrik → RAWG'den. |
| Platform / mağaza linkleri | **Güçlü** | `platforms`, `external_games`, `websites`. |

**Operasyonel:** Twitch OAuth, ~4 req/s hız limiti. **Açık teknik borç:** sunucu tarafı agresif önbellek / kuyruk henüz yol haritası maddesi değil; `game_localizations` ile içerik dili §7 i18n altında.

---

## 2. Faz A kapanış (desktop PoC)

Electron kabuğu ve Vite dev akışı çalışıyor. Kalan işler **küçük**; Faz A'yı gerçekten kapatmak için.

- [x] **Kabuk seçimi + minimal proje** — Electron seçildi; `desktop/` altında `src/main.js`, `preload.js`, `package.json`; Vite `base: './'` `VITE_DESKTOP=1` bayrağıyla; env akışı `desktop/.env.example` + `desktop/README.md`.
- [x] **Dev modda duman testi** — `npm run dev` (Vite) + `desktop/npm run dev` (Electron) başarılı.
- [x] **`HashRouter` + desktop-güvenli 401 redirect** — `VITE_DESKTOP=1` bayrağıyla `App.jsx` `HashRouter`'a, `apiClient.js` 401 handler `window.location.hash`'e geçiş yapar; web build (`BrowserRouter` + path redirect) değişmedi.
- [x] **Paketli (`file://`) duman testi** — `npm run build:dir` başarılı; unpacked `.exe` açılıyor, SPA HashRouter altında geziniyor, `localhost:5118/api` çağrıları geçiyor. Terminaldeki `cache_util_win` uyarıları Electron zararsız loglarıdır.

---

## 3. Güvenlik & oturum

Masaüstü dağıtımından önce oturması gerekenler — token değişimi sahada uygulama varken risklidir.

- [x] **Refresh token + kısa ömürlü access token** — Access JWT 15 dk; `RefreshTokens` tablosunda SHA-256 hash + rotasyon (her refresh yenisini üretir + eskiyi revoke eder); revoke edilmiş token tekrar kullanılırsa kullanıcının tüm oturumları iptal edilir; `POST /User/refresh` + `POST /User/logout`; şifre değişimi / sıfırlama tüm oturumları revoke eder. RememberMe artık refresh ömrünü uzatır (30 → 90 gün). Frontend `apiClient` 401'de tek uçuculu refresh promise ile isteği tekrar dener; migration `backend/scripts/AddRefreshTokens.sql`.
- [x] **Kalıcı geçici kod deposu** — `TempVerificationCodes` tablosu (SHA-256 hash + aynı hedef için yeni kod eskiyi geçersizleştirir); üç static store tek `IVerificationCodeStore` DI servisine konsolide edildi; migration `backend/scripts/AddTempVerificationCodes.sql`.

---

## 4. Veri modeli & uyumluluk

- [x] **Played ↔ Completed** — `LibraryStatuses.Normalize` tek noktadan kanonikleştirme (yazma + okuma + filtre); `Completed` filtresi migration öncesi legacy `Played` satırlarını da kapsar; tek seferlik SQL `backend/scripts/NormalizePlayedToCompleted.sql`.
- [x] **Gemini öneri sayısı** — `GeminiService.RecommendationCount = 15` tek kaynak; prompt + `Take()` aynı sabitten okur. IGDB hız limiti (~4 req/s) dostu; daha önceki tutarsızlık (prompt 20 iste, controller 15 al, örnek 15) giderildi.

---

## 5. Desktop: yüklü oyun & oynama süresi

- [x] **Yüklü oyun algılama** — Steam (`appmanifest_*.acf` + `libraryfolders.vdf`), Epic (`*.item` JSON), GOG (registry). Ek npm bağımlılığı yok; `reg query` + `tasklist` subprocess. Xbox/Battle.net/korsan için **manuel ekleme** (IGDB ara + `.exe` seç).
- [x] **Yüklü Oyunlarım sayfası** — Algılananlar + izlenen eşlemeler; her oyun için IGDB'den arama ile kütüphaneye bağlama. Web build'de gizlidir.
- [x] **Oynama süresi** — `UserLibrary.playtime_minutes` sütunu. Electron main process'te 5 sn tarama + 30 sn heartbeat: tanınan `.exe` çalışırken süre birikir, `POST /api/Library/user/{id}/playtime/{gameId}` ile sunucuya delta gönderilir. Web'de ve desktop'ta kart altında "X saat" rozeti. AFK tespiti yok (kararla).
- [x] **Tray + autostart** — Kapatınca tray'de yaşar; Windows login item toggle'ı; refresh sonrası tracker yeni token'ı otomatik alır.

---

## 6. Masaüstü dağıtım (Faz B/C/D)

**Plan:** [PLAN_WEB_DESKTOP.md](./PLAN_WEB_DESKTOP.md). Faz A iskelesi tamam; Faz B–D dağıtım altyapısı. §5 olmadan kullanıcıya indirme önermek erken (kod imzalama parası ve ilk izlenim riski).

| Faz | İçerik | Durum |
|-----|--------|-------|
| A | Kabuk + PoC | [x] İskelet tamam, kapanış §2'de |
| B1 | Windows NSIS (imzasız) + ikonlar | [x] `npm run build` → `GameTracker Setup x.y.z.exe` |
| B2 | Authenticode imzalı installer | Kapsam dışı |
| C | CI'de desktop artefakt + Release | Kapsam dışı (manuel release yeterli) |
| D | Auto-update (electron-updater) | Kapsam dışı |

- [x] **Web dağıtım olgunluğu** — GitHub Actions: `dotnet build` + test, `frontend` `npm run build` + `npm run lint`; `frontend/.env.example`.
- [x] **MVP paket — NSIS** — `desktop/package.json` `electron-builder`; `assets/icon.ico`, `assets/tray.png`; `generate:icons` ile yeniden üretim; kök `README.md` “Download (Windows desktop)”.
- [ ] **İsteğe bağlı (ürün)** — Authenticode; CI’de Windows build; `electron-updater`; GitHub Releases otomasyonu.

---

## 7. Web ürün & UX

Çekirdek akışlar (§3–§5) oturduktan sonraki cila. Boyutlar: i18n, arama/sanal liste, ince UX.

### i18n (UI — tamamlandı)

| Dil | Rol |
|-----|-----|
| **Türkçe** | Varsayılan / birincil. |
| **İngilizce** | İkinci dil; **Ayarlar → Tercihler → Arayüz dili**. |

- [x] **Dil seçeneği** — TR ↔ EN; tercih `localStorage` anahtarı `gt_preferences` içindeki `locale` (`PreferencesContext`). Backend profil alanı **yok** (bilerek sade tutuldu).
- [x] **i18n altyapısı** — `frontend/src/i18n/t.js` (`translate`, `getStoredLocale`), `useI18n.js`, `tr.json` / `en.json`; bileşenlerde `t('anahtar')`. `document.documentElement.lang` güncellenir. Paket bağımlılığı eklenmedi.
- [ ] **İçerik dili (opsiyonel, sonraki adım)** — IGDB `game_localizations` / API dil parametreleri. Oyun başlığı/açıklaması API dilinde kalır; yalnızca uygulama chrome'u çevrilir.

### UX ve performans

- [ ] **Kütüphane arama / sıralama** — İsimle süzme + sıralama (eklenme, ad, Metacritic); ilk adım istemci tarafı filtre.
- [ ] **Büyük listeler** — Sanal liste (`@tanstack/react-virtual` zaten bağımlılıkta) veya sayfalı mod; çok `GameCard` DOM yükü.
- [ ] **Görsel optimizasyonu** — Seçilen API görselleri için `resize` / `srcset` / sabit genişlik; LQIP yer tutucu.
- [ ] **Mikro etkileşim** — Optimistic UI + başarı animasyonu; "Tekrar dene"; klavye (global arama odağı, yardım).
- [ ] **Şema genişlemesi** — Oyun başına not / etiket; tamamlanma tarihi vb. istatistikler.
- [ ] **Önbellek temizle** — "Cache / görsel önbelleği temizle" aksiyonu (düşük öncelik).
- [x] **AI öneri UX** — Sayfa açılışında çağrı yok; "Önerileri oluştur" + yeniden analiz; düz palet.
- [x] **Liste / hero görsel + kart oranı** — Kapaklar istek anında IGDB'den taze; DB `image_url` fallback. Liste `aspect-[5/7]`; detay hero ilk screenshot `t_1080p`.

---

## 8. Kalite & operasyon

- [ ] **ESLint sıkılaştırma** — `eslint-plugin-react` + `react-hooks` veya CI'de zorunlu lint (import / hook hataları).
- [ ] **Deploy checklist** — `DEPLOY.md` / `DATABASE.md` yeni ortamlarda güncel tut (CORS, env, connection string).

---

*Kaynak: eski planlama belgeleri, kod incelemesi ve ürün hedefleri.*
