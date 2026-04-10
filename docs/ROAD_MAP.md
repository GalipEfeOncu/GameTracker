# GameTracker — Yol haritası

Henüz yapılmamış veya netleştirilmesi gereken işler. Tamamlanan özellikler: [FEATURES.md](./FEATURES.md).

**Referans:** [DEPLOY.md](./DEPLOY.md) · [DATABASE.md](./DATABASE.md) · `backend/README.md`

---

## Nasıl kullanılır?

- `[ ]` **Açık** — yapılacak veya takip edilen iş (GitHub’da tıklanınca işaretlenebilir).
- Aşağıdaki bölümler **önerilen yapılma sırasına** göre numaralandırıldı: önce veri kaynağı ve mimari kararlar, sonra platform ve desktop özellikleri, ardından mevcut web borçları ve ince UX.

**İçindekiler:** [Özet sıra](#önerilen-yapılma-sırası-özet) · [1. API](#1-oyun-veri-kaynağı-api) · [2. Platform](#2-web--desktop-dağıtım) · [3. Desktop oyun](#3-desktop-yüklü-oyun--oynama-süresi) · [4. Güvenlik](#4-güvenlik--oturum) · [5. Veri modeli](#5-veri-modeli--uyumluluk) · [6. Web UX](#6-web-ürün--ux) · [Dil (EN / TR)](#dil-english-and-turkish-i18n) · [7. Kalite](#7-kalite--operasyon) · [Web+Desktop plan](./PLAN_WEB_DESKTOP.md)

---

## Önerilen yapılma sırası (özet)

1. ~~**Oyun veri API’si**~~ **Tamamlandı** — IGDB + RAWG **hibrit** üretimde; araştırma özeti ve alan haritası §1 tabloda. *(Tek API ideali seçilmedi.)*
2. ~~**RAWG’den geçiş / yeni sağlayıcı**~~ **Kapatıldı** — Tam RAWG kaldırma yok; IGDB omurga + RAWG tamamlayıcı (detay, Metacritic, gereksinimler) backend ve UI’da mevcut.
3. **Web + desktop dağıtım** — İki çıktı için mimari ve CI (shell / framework seçimi).
4. **Desktop: yüklü oyun algılama** — Mağaza kütüphaneleri + geniş kurulum kapsamı (aşağıda).
5. **Desktop: yüklü oyunlar sekmesi** — Liste + metadata eşlemesi.
6. **Desktop: oynama süresi (kalıcı)** — DB şeması; oyun silinse bile sürelerin korunması (launcher tarzı kimlik/eşleme kuralları).
7. **Güvenlik** — Refresh token / token iptali; kalıcı e-posta-kod deposu.
8. **Veri modeli** — Played ↔ Completed; Gemini öneri adedi netliği.
9. **Dil / yerelleştirme (EN + TR)** — Uygulama dili seçeneği; **öncelik: Web UX paketinin içinde yüksek** (çekirdek akışlar oturduktan sonra, kütüphane arama / büyük liste optimizasyonu ile aynı dönem veya hemen önce). İstenen diller: **İngilizce**, **Türkçe**.
10. **Web UX** — Kütüphane arama, AI eşiği, büyük listeler, görseller, mikro etkileşim, şema genişlemesi, önbellek temizleme (düşük öncelik).
11. **Kalite** — ESLint sıkılaştırma; deploy checklist güncelliği.

---

## 1. Oyun veri kaynağı (API)

**Durum:** Üretimde **hibrit IGDB + RAWG** kullanılıyor. Aşağıdaki plan maddeleri uygulandı; **tek API** ideali bilinçli olarak bırakıldı (maliyet/kota/eksik alan dengesi).

- [x] **Uygulama planı** — Hibrit strateji (IGDB ana liste/detay omurgası; detayda platform / tamamlayıcı mağaza RAWG; mağaza linklerinde IGDB `external_games` önceliği; sistem gereksinimleri RAWG; süre tahmini IGDB `game_time_to_beats`): **[PLAN_IGDB_RAWG_HYBRID.md](./PLAN_IGDB_RAWG_HYBRID.md)**.
- [x] **Araştırma: IGDB ve alternatifler** — RAWG sınırları, IGDB alan haritası, Twitch OAuth / kota özeti §1 tabloda ve planda netleştirildi; ürün kararı hibrit.
- [x] **Hedef veri modeli (işlevsel)** — Kapak, ekran görüntüleri, açıklama, platformlar, dijital mağaza linkleri (IGDB + filtre + RAWG birleşimi), sistem gereksinimleri ve Metacritic (RAWG), skor özeti (IGDB + Metacritic) sağlanıyor. *(“Tek kaynak” ideali uygulanmadı.)*
- [x] **RAWG’den geçiş veya strateji** — Kalıcı **hibrit** seçildi; backend (`HybridGameDetailService`, `IgdbApiService`, …) ve frontend tüketimi buna göre yapılandırıldı. Tam RAWG kaldırma veya yalnız IGDB’ye geçiş **yapılmadı** (IGDB’de oyun bazlı PC min/önerilen ve resmi Metacritic yok).

### IGDB araştırma özeti (kaynak: resmi `igdbapi.proto` + IGDB dokümantasyonu)

| İstenen | IGDB’de durum | Not |
|--------|----------------|-----|
| Kaliteli kapak | **Var** | `Cover` + `image_id` → `images.igdb.com` üzerinden boyut seçilebilir görseller. |
| Ekran görüntüleri | **Var** | `screenshots` (aynı image CDN). |
| About / açıklama | **Var** | `summary`, `storyline`; yerelleştirme için `game_localizations`. |
| **Sistem gereksinimleri (min/önerilen PC)** | **Yok (oyun bazlı)** | Şemada oyun için “minimum RAM / GPU” alanı yok. `PlatformVersion` altındaki cpu/ram/storage vb. **konsol/donanım platformu** tanımı; Steam sayfasındaki gibi **oyun başı PC gereksinimi** değil. |
| **Metacritic skoru (resmi sayı)** | **Yok** | Ayrı `metacritic` alanı yok. `aggregated_rating` / `aggregated_rating_count` = **dış eleştiri kaynaklarından türetilmiş özet puan** (Metacritic ile aynı sayı garanti değil). `total_rating` kullanıcı+eleştiri karışımına yakın. |
| Hangi platformlarda | **Var** | `platforms`, `release_dates`; aile/tip bilgisi. |
| Mağazalar + linkler | **Güçlü** | `websites` + `WebsiteType` (Steam, Epic, GOG, itch, Discord, …); `external_games` ile mağaza kimlikleri (ör. Steam `appid`). |

**Operasyonel:** API Twitch geliştirici hesabı + Client ID / OAuth access token ister. **Hız limiti** tipik olarak **saniyede ~4 istek** (eşzamanlı istek sınırı var) — toplu senkron veya popüler liste için önbellek / kuyruk tasarımı önemli.

**Sonuç (uygulama):** IGDB liste/keşfet/popülerlik ve detay omurgası; **mağaza linkleri** öncelikle IGDB `external_games`, tamamlayıcı ve düşük güven RAWG’de birleştirme + dijital mağaza filtresi. **Platform ve PC gereksinimleri** ile **Metacritic** RAWG’den. **HowLongToBeat** IGDB `game_time_to_beats`. Ayrıntı plan: [PLAN_IGDB_RAWG_HYBRID.md](./PLAN_IGDB_RAWG_HYBRID.md).

**Açık teknik borç (API ile ilgili, §1 dışı veya operasyon):** IGDB hız limiti için sunucu tarafı agresif önbellek/kuyruk henüz yol haritası maddesi değil; `game_localizations` ile içerik dili §6 i18n altında.

---

## 2. Web + desktop dağıtım

**Uygulama planı (fazlı):** [PLAN_WEB_DESKTOP.md](./PLAN_WEB_DESKTOP.md) — mevcut Vite SPA + uzak .NET API; masaüstü için kabuk (Tauri / Electron önerilen karşılaştırma), CI, imzalama ve auto-update.

### Özet fazlar

| Faz | İçerik |
|-----|--------|
| **A** | Kabuk seçimi + PoC (Windows); `dist` yükleme; `VITE_API_BASE_URL` ile canlı API |
| **B** | Windows installer + kod imzalama (üretim) |
| **C** | CI’de frontend build + desktop artefakt + Release |
| **D** | Otomatik / bildirimli güncelleme (Tauri updater veya electron-updater) |

### Görevler

- [ ] **Faz A — Kabuk + PoC** — [PLAN_WEB_DESKTOP.md](./PLAN_WEB_DESKTOP.md) §5 Faz A maddeleri (Tauri veya Electron net seçim, minimal masaüstü proje).
- [ ] **Faz B — Paket ve imza** — Kurulum dosyası; Authenticode / platform imzası ve sırların CI’de yönetimi.
- [ ] **Faz C — CI** — Web build doğrulaması + desktop sürüm artefaktı (etiketli release ile hizalı).
- [ ] **Web dağıtım olgunluğu** — Frontend için net CI (lint + build); `.env.example` / `VITE_*` ile DEPLOY uyumu (plan §2).
- [ ] **Faz D — Sürümleme ve güncelleme** — Desktop auto-update kanalı + kullanıcıya güncelleme davranışı (plan §5 Faz D).

- [ ] **Çift dağıtım (üst seviye)** — Yukarıdaki fazlar tamamlanınca tiklenecek: web üretimde + masaüstü kurulumu dağıtılabilir durumda.

---

## 3. Desktop: yüklü oyun & oynama süresi

- [ ] **Yüklü oyun algılama** — Steam, Epic, GOG, Xbox, Battle.net ve diğer yaygın mağaza/kütüphane kaynakları; ayrıca **mağaza dışı kurulumlar** (yaygın dizinler, kullanıcı tanımlı yollar, “her türlü” yüklü başlığın kapsanması hedefi). Kapsam, gizlilik ve yasal kullanım için kullanıcıya **açık rıza / ayar** düşünülmeli.
- [ ] **Yüklü oyunlar sekmesi** — Algılanan oyunların ayrı sekmede tutulması; mümkün olduğunca metadata API ile eşleştirme (isim, exe, mağaza kimliği).
- [ ] **Oynama süresi istatistikleri (DB)** — Sürelerin veritabanında saklanması; **oyun diskten silinse bile** ilgili başlık için toplam sürenin **korunması** (sabit oyun kimliği: API slug, hash, kullanıcı birleştirme kuralları — tasarım netleştirilecek). Amaç: mağaza veya kaynak fark etmeksizin **launcher benzeri** kullanım özeti.

---

## 4. Güvenlik & oturum

- [ ] **Refresh token** + kısa ömürlü access token; çıkış veya şifre değişiminde token iptali / iptal listesi (şu an uzun ömürlü JWT + “beni hatırla”).
- [ ] **Kalıcı geçici kod deposu** — E-posta doğrulama, şifre sıfırlama, hesap silme kodları için Redis veya DB tablosu (şu an bellek, restart’ta sıfırlanır; TTL korunmalı).

---

## 5. Veri modeli & uyumluluk

- [ ] **Played ↔ Completed** — Legacy DB’de `Played` varsa web’deki **Completed** ile uyum: tek enum, migrasyon veya eşleme + gerekirse tek seferlik SQL.
- [ ] **Gemini öneri sayısı** — Servis ~20 isim beklerken controller’da ~15 RAWG araması: kasıtlı mı, netleştir (API değişince yeniden değerlendirilir).

---

## 6. Web ürün & UX

### Dil: English and Turkish (i18n)

**Öncelik (bu bölüm içinde):** **Yüksek** — Güvenlik (§4) ve veri modeli uyumu (§5) netleştikten sonra; “mikro etkileşim / önbellek temizle” gibi düşük öncelikli UX maddelerinden **önce** planlanmalı.

| Dil | Rol |
|-----|-----|
| **Türkçe** | Varsayılan veya birincil yerel dil (ürün hedef kitlesine göre netleştirilir). |
| **İngilizce** | İkinci dil; seçenek olarak her zaman sunulur. |

- [ ] **Dil seçeneği** — Kullanıcı arayüzünde **İngilizce** ve **Türkçe** arasında geçiş (ayarlar veya üst menü); tercih kalıcılığı: `localStorage` ve/veya oturumlu kullanıcı için backend profil alanı (tercih hangisi netleştirilecek).
- [ ] **i18n altyapısı** — Tüm kullanıcıya dönük sabit metinlerin çeviri anahtarlarına taşınması (ör. `react-i18next` veya eşdeğeri); başlangıç dili: tarayıcı dili + kullanıcı tercihi önceliği.
- [ ] **İçerik dili (isteğe bağlı, sonraki adım)** — Oyun açıklamaları / başlıklar için IGDB `game_localizations` veya API dil parametreleri; MVP’de yalnızca **UI dili** yeterli sayılabilir.

---

- [ ] **Kütüphane arama / sıralama** — İsimle süzme; isteğe bağlı sıralama (eklenme, ad, Metacritic); ilk adım: istemci tarafı filtre.
- [ ] **AI öneri eşiği (UX)** — “En az üç oyun” bilgilendirme; az örnekte Keşfet / Popüler CTA.
- [ ] **Büyük listeler** — Sanal liste (windowing) veya sayfalı mod; çok `GameCard` DOM yükü.
- [x] **Liste / hero görsel + kart oranı (web + API)** — Liste/kartlar IGDB **kapak** (`t_cover_big`); detay hero’da varsa ilk **screenshot** (`t_1080p`), yoksa kapak. Kart **`aspect-[2/3]`** + `object-cover`; hero tam alan `object-cover`.
- [ ] **Görsel optimizasyonu** — Seçilen API görselleri için `resize` / `srcset` / sabit genişlik; LQIP tarzı yer tutucu (veri kaynağı değişince yeniden uyum).
- [ ] **Mikro etkileşim** — Başarı animasyonu / optimistic UI; “Tekrar dene”; klavye (global arama odağı, yardım).
- [ ] **Şema genişlemesi** — Oyun başına not / etiket; tamamlanma tarihi vb. istatistikler.
- [ ] **Önbellek temizle** — Bilinçli “cache / görsel önbelleği temizle” (düşük öncelik).

---

## 7. Kalite & operasyon

- [ ] **ESLint sıkılaştırma** — `eslint-plugin-react` + `react-hooks` veya CI’de zorunlu lint (import / hook hataları).
- [ ] **Deploy checklist** — Yeni ortamlarda `DEPLOY.md` / `DATABASE.md` ile yapılandırma adımlarını güncel tut (CORS, env, connection string).

---

*Kaynak: Eski planlama belgeleri, kod incelemesi ve ürün hedefleri.*
