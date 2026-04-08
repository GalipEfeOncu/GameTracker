# GameTracker — Yol haritası

Henüz yapılmamış veya netleştirilmesi gereken işler. Tamamlanan özellikler: [FEATURES.md](./FEATURES.md).

**Referans:** [DEPLOY.md](./DEPLOY.md) · [DATABASE.md](./DATABASE.md) · `backend/README.md`

---

## Nasıl kullanılır?

- `[ ]` **Açık** — yapılacak veya takip edilen iş (GitHub’da tıklanınca işaretlenebilir).
- Aşağıdaki bölümler **önerilen yapılma sırasına** göre numaralandırıldı: önce veri kaynağı ve mimari kararlar, sonra platform ve desktop özellikleri, ardından mevcut web borçları ve ince UX.

**İçindekiler:** [Özet sıra](#önerilen-yapılma-sırası-özet) · [1. API](#1-oyun-veri-kaynağı-api) · [2. Platform](#2-web--desktop-dağıtım) · [3. Desktop oyun](#3-desktop-yüklü-oyun--oynama-süresi) · [4. Güvenlik](#4-güvenlik--oturum) · [5. Veri modeli](#5-veri-modeli--uyumluluk) · [6. Web UX](#6-web-ürün--ux) · [7. Kalite](#7-kalite--operasyon)

---

## Önerilen yapılma sırası (özet)

1. **Oyun veri API’si** — IGDB (ve alternatifler) araştırması; RAWG’nin Metacritic, sistem gereksinimleri, mağaza/platform zenginliği gibi eksikleri nedeniyle değişim veya tamamlayıcı kaynak kararı; mümkünse **tek API**, çoklu kaynak son çare.
2. **RAWG’den geçiş / yeni sağlayıcı entegrasyonu** — Backend + UI; gerekirse cache ve eşleme migrasyonu.
3. **Web + desktop dağıtım** — İki çıktı için mimari ve CI (shell / framework seçimi).
4. **Desktop: yüklü oyun algılama** — Mağaza kütüphaneleri + geniş kurulum kapsamı (aşağıda).
5. **Desktop: yüklü oyunlar sekmesi** — Liste + metadata eşlemesi.
6. **Desktop: oynama süresi (kalıcı)** — DB şeması; oyun silinse bile sürelerin korunması (launcher tarzı kimlik/eşleme kuralları).
7. **Güvenlik** — Refresh token / token iptali; kalıcı e-posta-kod deposu.
8. **Veri modeli** — Played ↔ Completed; Gemini öneri adedi netliği.
9. **Web UX** — Kütüphane arama, AI eşiği, büyük listeler, görseller, mikro etkileşim, şema genişlemesi, önbellek temizleme (düşük öncelik).
10. **Kalite** — ESLint sıkılaştırma; deploy checklist güncelliği.

---

## 1. Oyun veri kaynağı (API)

- [ ] **Uygulama planı** — Hibrit strateji (IGDB ana liste/detay omurgası; detayda platform/mağaza/link RAWG; sistem gereksinimleri IGDB→RAWG sırası): **[PLAN_IGDB_RAWG_HYBRID.md](./PLAN_IGDB_RAWG_HYBRID.md)**.
- [ ] **Araştırma: IGDB ve alternatifler** — RAWG’nin yetersiz kaldığı alanlar özellikle **Metacritic skorları**, **sistem gereksinimleri**, tutarlı ve zengin mağaza/platform bilgisi. **IGDB API** ciddi aday; lisans, kota, alan haritası ve Twitch kimlik gereksinimleri netleştirilmeli.
- [ ] **Hedef veri modeli (tek kaynak ideali)** — API’nin karşılaması istenenler: kaliteli **kapak**, **ekran görüntüleri**, **about / açıklama**, **sistem gereksinimleri**, **Metacritic** (veya eşdeğeri), **hangi cihaz/platformlarda** olduğu, **hangi mağazalarda** satıldığı ve **doğrudan linkler**. Tercih: **tek API** ile tümü; ancak **çok gerekliyse** ikinci bir API ile tamamlanabilir — **mümkün olduğunca kaçınılacak** (maliyet, kota, bakım).
- [ ] **RAWG’den geçiş veya strateji** — Yukarıdaki gereksinimler karşılanana kadar RAWG ile kalma / geçiş / geçici hibrit yol haritası; backend servis ve frontend tüketiminin yeniden yapılandırılması.

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

**Sonuç:** IGDB, kapak/SS/açıklama tarafında güçlü; **PC sistem gereksinimleri** ve **bire bir Metacritic** için tek başına yetmez. Ürün kararı: **liste/keşfet/popülerlik IGDB**; detayda **platform + mağaza + linkler şimdilik RAWG**; **gereksinimler** önce IGDB kontrol, yoksa RAWG — ayrıntı plan: [PLAN_IGDB_RAWG_HYBRID.md](./PLAN_IGDB_RAWG_HYBRID.md).

---

## 2. Web + desktop dağıtım

- [ ] **Çift dağıtım** — Proje hem **web uygulaması** hem **masaüstü uygulaması** olarak yayınlanmalı (paylaşılan API + masaüstü kabuk: Electron, Tauri, .NET MAUI + WebView vb. — **teknoloji seçimi** ayrı bir alt görev).
- [ ] **Sürümleme ve güncelleme** — Desktop için otomatik veya bildirimli güncelleme stratejisi (imzalama, kanal).

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

- [ ] **Kütüphane arama / sıralama** — İsimle süzme; isteğe bağlı sıralama (eklenme, ad, Metacritic); ilk adım: istemci tarafı filtre.
- [ ] **AI öneri eşiği (UX)** — “En az üç oyun” bilgilendirme; az örnekte Keşfet / Popüler CTA.
- [ ] **Büyük listeler** — Sanal liste (windowing) veya sayfalı mod; çok `GameCard` DOM yükü.
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
