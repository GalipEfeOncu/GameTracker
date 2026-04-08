# Plan: IGDB ana kaynak + RAWG tamamlayıcı (detay / gereksinimler)

Bu belge, mevcut kod tabanına göre (**`RawgApiService`**, **`LibraryController`**, **`Game` modeli**, **`GameDetailsPage`**) uygulanacak geçişin sırasını ve teknik kararları tanımlar. Uygulama adımlarında bu dosyayı referans al.

**İlgili:** [ROAD_MAP.md](./ROAD_MAP.md) (üst seviye iş listesi), [DEPLOY.md](./DEPLOY.md) (Twitch Client ID / secret eklenecek).

---

## 1. Hedef özet

| Alan | Kaynak | Not |
|------|--------|-----|
| Popüler sıralama, keşfet modları, arama sonuçları (liste kartları) | **IGDB** | Ana ürün deneyimi; sıralama IGDB alanlarına göre tanımlanacak. |
| Oyun detayında **platformlar, mağazalar, satın alma linkleri** | **RAWG** (şimdilik) | Şu an çalışan akış korunur; IGDB `external_games` ile RAWG `id` eşlemesi gerekir. |
| **Sistem gereksinimleri** (min / önerilen) | **IGDB → yoksa RAWG** | IGDB’de oyun bazlı spec yok; yine de önce IGDB’de gelecek bir alan var mı kontrol, sonra RAWG `platforms[].requirements` birleştirilir. |
| Kapak, açıklama, geliştirici, yayıncı, SS, puanlar (detay + listeler) | **Ağırlık IGDB** | Listeler ve detayın “IGDB omurgası” ile doldurulması; UI alanları ortak DTO’ya map edilir. |

**Prensip:** Kullanıcıya tek bir `gameId` (tercihen **IGDB numeric id**) gösterilir; backend RAWG çağrıları için gerekirse eşlemeyi çözer.

---

## 2. En kritik karar: oyun kimliği (ID)

Bugün:

- Kütüphane ve route **`/game/:id`** RAWG `games.id` ile çalışıyor.
- `AddToLibrary` gövdesi `body.Id` = RAWG id.

IGDB ana kaynak olunca **önerilen hedef:**

1. **Harici kimlik:** API ve frontend için **birincil id = IGDB `game.id`** (ulong; JSON’da string olabilir, C# tarafında `long` / `ulong` + dikkatli serileştirme).
2. **Eşleme:** IGDB `external_games` (kaynak = Steam vb.) veya arama ile RAWG id bulunabildiğinde, sunucuda **RAWG id** sadece “detay tamamlama” için kullanılır; istenirse `GameMetadataLink` benzeri cache tablosu: `igdb_id`, `rawg_id`, `updated_at`.
3. **Migrasyon:** Mevcut kullanıcı verisi RAWG id ile kayıtlıysa, tek seferlik veya arka planda:
   - RAWG id → isim/slug → IGDB arama → en iyi eşleşen `igdb_id` atanır; riskli eşleşmeler loglanır veya kullanıcıya “yeniden eşle” bırakılır.
4. **Geçiş süreci (opsiyonel):** Kısa dönem `?source=rawg` veya çift id endpoint (`/api/library/game/detail?igdb=…&rawg=…`) **önerilmez**; tek id ile gitmek borcu azaltır.

Bu madde **1. sprint’te** netleştirilmeden IGDB listeleri production’a bağlanmamalı; aksi halde kütüphane ile liste/detay uyumsuz kalır.

---

## 3. Konu bazında kaynak matrisi (detaylı)

### 3.1 Listeler: Popüler, Keşfet, Arama

| Özellik | Şu an (RAWG) | Hedef (IGDB) |
|---------|----------------|--------------|
| Popüler | `ordering=-added` + tarih aralığı | IGDB’de “popülerlik” için: `hypes`, `follows`, çıkış tarihi, rating sayıları veya özel sıralı endpoint; **tek bir tanım seç** (ör. son X ay + `total_rating_count` / `hypes` ağırlığı). Dokümante et. |
| Trend | RAWG `-added` + tarih | IGDB’de benzer: yakın `first_release_date` + etkileşim alanları veya sabit sıralama kuralı. |
| En iyi puan | RAWG `-rating` | `total_rating` veya `aggregated_rating` azalan; eksik puanlı oyunları listede sona it veya filtrele. |
| Yeni çıkan | RAWG `-released` | `first_release_date` azalan. |
| Metacritic 75+ | RAWG `metacritic` | IGDB’de doğrudan Metacritic yok → filtre **`aggregated_rating >= 75`** (veya eşik) ile yaklaşık karşılık; liste/kart başlığı **“Metacritic 75+” kullanılmaz** — bkz. §3.4 (ör. “Eleştiri özeti 75+”). |
| Tür filtresi | RAWG `genre` id | IGDB `genre` id’leri; `DiscoverPage` `GENRES` dizisi **IGDB id** ile yeniden doldurulmalı. |
| NSFW | RAWG exclude + istemci süzgeci | IGDB `themes` / `age_ratings` ile süzgeç; mevcut istemci süzgeci IGDB alanlarına uyarlanır. |

### 3.2 Oyun detayı (tek sayfa)

| Blok | Birincil | İkincil / birleştirme |
|------|----------|------------------------|
| Başlık, kapak, özet, hikâye, çıkış | IGDB | — |
| Ekran görüntüleri | IGDB (`screenshots` + image CDN) | İsteğe bağlı: RAWG SS yedek (genelde gereksiz). |
| Türler, geliştirici, yayıncı | IGDB (`involved_companies`, `genres`) | RAWG ile çakışırsa IGDB öncelikli. |
| **Platform listesi (chip)** | **RAWG** | `GetGameDetailsAsync(rawgId)` veya hafif RAWG çağrısı; yanıt `Game.platforms` mevcut UI ile uyumlu kalsın. |
| **Mağazalar + linkler** | **RAWG** | Mevcut `stores` + `/stores` link birleştirme mantığı. |
| **Puan alanı (UI)** | **§3.4 politikası** | Tek sayı göster; kaynak her zaman etiketlenir. |
| Resmi site | IGDB `websites` (official) | RAWG `website` yedek. |
| **Sistem gereksinimleri** | Birleşik (aşağı §4) | — |

### 3.3 AI önerileri (`POST .../recommendations`)

- Gemini’den gelen isimler için arama: önce **IGDB search**, ilk sonuç `igdb_id`; kütüphaneye ekleme ve kartlar IGDB id ile hizalanır.
- Eski akışta RAWG araması kaldırılmadan önce kütüphane id stratejisi (§2) tamamlanmalı.

### 3.4 Puan gösterimi: Metacritic yoksa en iyi mümkün puan + şeffaflık

**Kural:** Kullanıcıya **yanıltıcı “Metacritic”** yazılmaz; gösterilen her sayının **ne olduğu** arayüzde kısa ve net belirtilir (chip, alt satır veya tooltip).

**Backend’in üreteceği mantıksal öncelik (ilk dolu / anlamlı değer kazanır):**

1. **RAWG `metacritic`** (tamamlayıcı RAWG çağrısında varsa) → etiket: **Metacritic** (0–100).
2. **IGDB `aggregated_rating`** (+ tercihen `aggregated_rating_count` &gt; 0) → etiket: **Eleştiri özeti (IGDB)** veya eşdeğer kısa metin; IGDB’nin dış eleştiri kaynaklarından türettiği puan olduğu belirtilir.
3. **IGDB `total_rating`** → etiket: **Toplam puan (IGDB)** (kullanıcı + eleştiri karışımı).
4. Hiçbiri yoksa → **“Puan yok”** / tire; açıklama gerekmez.

**API sözleşmesi önerisi:** Yanıtta ayrı alanlar: `display_score` (nullable number), `display_score_kind` (enum veya string: `metacritic` | `igdb_critic_aggregate` | `igdb_total` | `none`), isteğe bağlı `display_score_label` (Türkçe kısa metin, i18n için). Liste kartlarında da aynı politika (küçük etiket veya ikon + tooltip).

**Keşfet modu adı:** “Metacritic 75+” yerine örn. **“Yüksek eleştiri puanı (75+)”** ve açıklamada IGDB `aggregated_rating` kullanıldığı belirtilir; RAWG eşlemesi olan oyunlarda detay sayfasında yine §3.4 önceliği uygulanır.

---

## 4. Sistem gereksinimleri: IGDB → RAWG birleştirme

**Algoritma (backend, detay endpoint’inde veya ayrı alan hesaplayıcıda):**

1. `requirements` nesnesini üret (`minimum` / `recommended` string veya HTML — mevcut `GameDetailsPage` RAWG şekline yakın tut).
2. **Adım A — IGDB:** Şu an şemada standart oyun bazlı PC spec yok; ileride alan eklenirse buraya bağlanır. Şimdilik çoğu durumda boş kalır.
3. **Adım B — RAWG:** `rawgId` biliniyorsa `GetGameDetailsAsync(rawgId)` veya sadece gereksinim için minimal RAWG çağrısı; `platforms` içinde `slug === 'pc'` veya ilk dolu `requirements` seçilir (mevcut frontend mantığı ile uyumlu).
4. **Birleştirme kuralı:** IGDB doluysa önce IGDB göster; **değilse** RAWG; ikisi de doluysa **RAWG’yi mağaza kaynaklı “resmi spec”** sayıp öncelik ver veya kullanıcıya “Kaynak: RAWG” notu (ürün kararı).

Frontend’deki metin “RAWG üzerinde belirtilmediyse…” → “Veri sağlayıcıda belirtilmediyse…” veya kaynak etiketi ile güncellenir.

---

## 5. Backend uygulama fazları

### Faz 0 — Hazırlık

- [ ] Twitch Developer uygulaması; `Igdb:ClientId`, `Igdb:ClientSecret` (veya token üretim servisi) — [DEPLOY.md](./DEPLOY.md) güncelle.
- [ ] OAuth client credentials ile kısa ömürlü access token önbelleği (singleton servis, 4 req/s limitine uygun `SemaphoreSlim` veya kuyruk).
- [ ] `AppConfig` + `appsettings.Example.json` yeni anahtarlar; RAWG anahtarı **detay için** kalır.

### Faz 1 — `IgdbApiService`

- [ ] POST `https://api.igdb.com/v4/games` vb. sarmalayıcı; hata ve boş cevap davranışı.
- [ ] Popüler / keşfet / arama için gerekli sorgular (Apicalypse): `fields`, `where`, `sort`, `limit`, `offset`.
- [ ] Görsel URL üretimi: `https://images.igdb.com/igdb/image/upload/t_720p/{image_id}.jpg` (boyut politikası tek yerde).

### Faz 2 — Ortak DTO veya mevcut `Game` genişletmesi

- [ ] İstemcinin beklediği şekle **map** (ör. `background_image`, `name`, `id` = igdb).
- [ ] **§3.4 puan alanları:** `display_score`, `display_score_kind` (ve isteğe bağlı `display_score_label`) — liste ve detayda aynı mantık.
- [ ] `rawgId` isteğe bağlı dahili alan (JSON’da `rawg_id` opsiyonel) — frontend mağaza/platform için kullanmaz, sadece debug; veya sadece sunucu içi.

### Faz 3 — Controller geçişi

- [ ] `GET .../popular` → IGDB; `nextOffset` / sayfalama IGDB `offset` ile uyumlu.
- [ ] `GET .../discover` → IGDB; genre id ve mod mapping tablosu.
- [ ] `GET .../search` → IGDB search endpoint.
- [ ] `GET .../game/{id}` → **hibrit:** IGDB detay + RAWG platform/mağaza + §4 gereksinim birleştirme; `GET .../screenshots` IGDB’den veya ana detayda gömülü.
- [ ] `rawgConfigured` bayrağı → `metadataConfigured` veya ayrı `igdbConfigured` / `rawgConfigured` (detay tamamlama için RAWG yoksa uyarı).

### Faz 4 — Veri migrasyonu

- [ ] Kütüphane tablosunda `igdb_id` (ve geçiş sürecinde `legacy_rawg_id`) sütunları; migration script.
- [ ] Arka plan veya admin tek seferlik: RAWG → IGDB eşleme.

### Faz 5 — Temizlik

- [ ] Kullanılmayan RAWG listeleme kodunu kaldır; RAWG çağrıları sadece detay tamamlama + gereksinimde kalsın.
- [ ] Testler: WebApplicationFactory ile mock veya kayıtlı cevap; health dışında IGDB integration test (isteğe bağlı).

---

## 6. Frontend uygulama fazları

- [ ] `GameCard` / hook’lar: `id` artık IGDB; görsel alanları yeni map; **§3.4** puan/rozet (`display_score` + etiket veya tooltip).
- [ ] `DiscoverPage` — `GENRES`; mod listesinde **§3.4** ile uyumlu isimler (örn. “Yüksek eleştiri puanı (75+)”, Metacritic iddiası yok).
- [ ] `PopularPage` — `rawgConfigured` → yeni bayrak isimleri; boş durum mesajları.
- [ ] `GameDetailsPage` — **§3.4:** `display_score` + kullanıcıya görünür kaynak etiketi (Metacritic yalnızca `kind=metacritic` iken); gereksinim metni §4.
- [ ] `apiClient.js` — Endpoint sözleşmesi değişirse tipler/yorumlar.

---

## 7. Riskler ve mitigasyon

| Risk | Mitigasyon |
|------|------------|
| IGDB 4 req/s | **Paralel burst’tan kaçın:** RAWG’daki gibi 3 eşzamanlı IGDB isteği limiti hızlı aşar. Sunucu tarafı **sıralı kuyruk** veya `SemaphoreSlim(1)` ile ~4 req/s altında kal; agresif **önbellek** (memory/SQL, TTL); listelerde `fields` daraltma; mümkünse tek istekte gerekli alanlar. |
| Yanlış RAWG↔IGDB eşlemesi | Steam `app_id` üzerinden çapraz doğrulama; düşük güven skorunda mağaza linki göstermeme veya kullanıcıya uyarı. |
| Mağaza / platform boş kalması | RAWG eşlemesi başarısızsa **IGDB `websites`** (Steam, Epic, GOG tipleri) ile **B planı** link listesi göster; mümkün olduğunca tıklanabilir URL doğrula. |
| Çift API tutarsızlığı (isim, tarih, puan) | **Tek kartta tek “ana” kaynak** (IGDB omurga); RAWG yalnızca tamamlayıcı alanlar. Puan için **§3.4** zorunlu; çelişen tarih/isimde IGDB öncelikli, fark tooltipla açıklanabilir. |
| “Metacritic” beklentisi | Yalnızca RAWG `metacritic` gerçekten geldiğinde bu etiket kullanılır; aksi halde §3.4 etiketleri — **asla** IGDB skorunu Metacritic diye satma. |
| Büyük id ulong | JavaScript güvenli tamsayı: id’leri string taşıma veya JSON bigint stratejisi. |
| Mevcut kullanıcılar RAWG id | §2 migrasyon zorunlu. |

---

## 8. Önerilen uygulama sırası (özet checklist)

1. §2 Kimlik stratejisi + DB şeması taslağı onayı.  
2. Faz 0–1: Twitch token + `IgdbApiService` + bir sorgu kanıtı (ör. tek oyun).  
3. Faz 2: IGDB → mevcut `Game` (veya yeni DTO) map; tek endpoint pilot (`/search` veya `/popular`).  
4. Faz 3: Popüler + keşfet + arama tamamen IGDB; detay hibrit + §4 gereksinimler.  
5. Faz 4: Kütüphane migrasyonu + öneriler akışı.  
6. Faz 6: Frontend tam uyum + metinler.  
7. Faz 5: RAWG sadeleştirme ve test.

---

*Bu plan, uygulama sırasında kod ve API değişikliklerine göre küçük revizyonlarla güncellenmelidir.*
