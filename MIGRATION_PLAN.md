# GameTracker – Legacy’den Web’e Geçiş Planı

Bu belge, **Legacy WinForms (C# .NET 4.8)** projesindeki tüm özellikleri **Backend API (.NET 9)** + **Frontend (React + Vite)** yığınına adım adım taşımak için kullanılır.

---

## 1. Legacy Projedeki Özellikler (Referans)

| # | Özellik | Legacy Konum | Backend | Frontend |
|---|--------|--------------|---------|----------|
| 1 | **Kayıt (Register)** | SignupForm | ✅ Var | ✅ Var (axios doğrudan) |
| 2 | **E-posta doğrulama (kayıt sırasında)** | SignupForm → EmailService | ❌ Yok | ❌ Yok |
| 3 | **Giriş (Login)** | LoginForm | ✅ Var | ✅ Var |
| 4 | **Beni hatırla (Remember Me)** | LoginForm + Settings | ❌ Yok (API’de) | ❌ Yok |
| 5 | **Ana sayfa (Popüler oyunlar)** | MainForm Home | ✅ Var | ✅ PopularPage |
| 6 | **Kütüphane listesi (status filtreli)** | MainForm Library | ✅ Var | ✅ LibraryPage |
| 7 | **Kütüphane: PlanToPlay / Playing / Played / Dropped** | Legacy 4 tab | ✅ API status | ⚠️ Tab’lar var; “Dropped” yok, “Completed”↔“Played” uyumsuz |
| 8 | **Kütüphaneye oyun ekleme** | Detay → context menu | ✅ POST add | ❌ UI bağlı değil |
| 9 | **Kütüphaneden oyun çıkarma** | Detay / Library | ✅ DELETE remove | ❌ UI yok |
| 10 | **Kütüphanedeki oyunun status’unu güncelleme** | Detay context menu | ✅ PUT status | ❌ UI yok |
| 11 | **Kütüphane listesinde oyun status’u gösterimi** | LibraryManager DB’den status | ⚠️ API status dönmüyor | ❌ Kullanılamıyor |
| 12 | **Arama (Search)** | MainForm Search | ✅ Var | ✅ DiscoverPage (arama var) |
| 13 | **Keşfet (Discover)** | - | ✅ Var | ✅ DiscoverPage |
| 14 | **Oyun detay sayfası** | ShowGameDetails | ✅ GET game/{id} | ✅ GameDetailsPage |
| 15 | **Detayda “Kütüphaneye Ekle” / “In Library” butonu** | btnLibraryAction + context | ✅ add/remove/status | ❌ Buton API’ye bağlı değil |
| 16 | **Detayda ekran görüntüleri (screenshots)** | LoadScreenshots, RawgApiService | ❌ Endpoint yok (servis var) | ❌ Yok |
| 17 | **AI öneri (Gemini)** | pageAI, btnGenerateAI | ✅ POST recommendations | ✅ AiSuggestionPage |
| 18 | **Ayarlar: Kullanıcı adı değiştirme** | Settings, UserManager | ❌ Endpoint yok | ❌ Yok |
| 19 | **Ayarlar: Şifre değiştirme** | Settings, UserManager | ❌ Endpoint yok | ❌ Yok |
| 20 | **Ayarlar: Başlangıç sayfası (Home/Library)** | Settings, StartPage | ❌ (istemci tarafı) | ❌ Yok |
| 21 | **Ayarlar: NSFW içerik göster** | Settings, RawgApiService | ✅ query param | ❌ Yok |
| 22 | **Çıkış (Logout)** | Settings | N/A (token yok) | ✅ Context’te var |
| 23 | **Register’ı apiClient’a taşıma** | - | - | ⚠️ register apiClient’ta yok |

---

## 2. Eksik Kalan Özellikler (Özet)

### Backend’de eksik
- E-posta doğrulama (kayıt sırasında kod gönderme + doğrulama endpoint’i).
- Kullanıcı adı güncelleme endpoint’i (`UpdateUsername`).
- Şifre güncelleme endpoint’i (`VerifyPassword` + `UpdatePassword`).
- Oyun ekran görüntüleri endpoint’i (`GetGameScreenshots` → `/Library/game/{id}/screenshots`).
- Kütüphane listesinde her oyun için **status** alanının dönmesi (şu an API status dönmüyor).

### Frontend’de eksik
- **apiClient**: `register`, `addGameToLibrary`, `removeGameFromLibrary`, `updateGameStatus`.
- **GameDetailsPage**: “Kütüphaneye Ekle” / “Kütüphanede” butonu ve status seçenekleri (dropdown/menu).
- **LibraryPage**: Kütüphaneden kaldırma, status değiştirme (dropdown veya menü); “Dropped” sekmesi; status’un API’den gelmesi.
- **SettingsPage**: Kullanıcı adı değiştirme, şifre değiştirme, başlangıç sayfası (Home/Library), NSFW toggle.
- **Oyun detay**: Ekran görüntüleri (screenshots) bölümü.
- Status uyumu: Backend “Played” kullanıyor, frontend “Completed” kullanıyor → tek tip (örn. “Played”) veya eşleme yapılmalı.

---

## 3. Adım Adım Geçiş Planı

Aşağıdaki sıra, bağımlılıklara göre (önce backend, sonra frontend) kurgulandı. Her adım bitince test edilebilir.

---

### Faz A: Backend Tamamlama ✅ (Tamamlandı)

| Adım | Görev | Durum |
|------|--------|----------|
| **A1** | Kütüphane cevabında `status` dön | `GetUserLibrary` için DTO veya Game’e `Status` ekle; DB’den okunan `status` alanını modele yaz ve JSON’da dön. |
| **A2** | Screenshots endpoint’i ekle | `RawgApiService.GetGameScreenshotsAsync` kullan; `GET api/Library/game/{id}/screenshots` ekle. |
| **A3** | Kullanıcı adı güncelleme | `UserManager.UpdateUsername` için `PUT api/User/profile/username` (body: `{ "newUsername": "..." }`). |
| **A4** | Şifre güncelleme | `VerifyPassword` + `UpdatePassword` için `PUT api/User/profile/password` (body: mevcut şifre, yeni şifre, tekrar). |
| **A5** | (İsteğe bağlı) E-posta doğrulama | Kayıt akışında kod gönder; `POST api/User/verify-email` ile kod doğrulama. EmailService zaten var. |

---

### Faz B: Frontend API ve Veri Katmanı

| Adım | Görev | Açıklama |
|------|--------|----------|
| **B1** | apiClient’a eksik fonksiyonları ekle | `register`, `addGameToLibrary`, `removeGameFromLibrary`, `updateGameStatus`, `getGameScreenshots`. |
| **B2** | Status eşlemesi | Backend “Played” kullanıyorsa frontend’de “Completed” yerine “Played” kullan veya API’de alias kabul et; tek tip isimlendirme seç. |
| **B3** | Kütüphane veri yapısı | API’den gelen `status` alanını kütüphane kartlarında ve filtrelerde kullan. |

---

### Faz C: Oyun Detay ve Kütüphane UI

| Adım | Görev | Açıklama |
|------|--------|----------|
| **C1** | GameDetailsPage – Kütüphane butonu | “Kütüphaneye Ekle” / “Kütüphanede” butonu; tıklanınca status menüsü (PlanToPlay, Playing, Played, Dropped) + “Kütüphaneden çıkar”. `addGameToLibrary`, `updateGameStatus`, `removeGameFromLibrary` ve kütüphanede mi sorgusu (library list veya ayrı endpoint) kullan. |
| **C2** | GameDetailsPage – Screenshots | `getGameScreenshots(id)` ile ekran görüntüleri bölümü; grid veya yatay kaydırmalı liste. |
| **C3** | LibraryPage – Kaldırma ve status | Her kartta “Kaldır” ve “Durum değiştir” (dropdown/menu); `updateGameStatus`, `removeGameFromLibrary` çağır; listeyi invalidate/refetch et. |
| **C4** | LibraryPage – Dropped sekmesi | Tab listesine “Dropped” ekle; backend zaten `status=Dropped` destekliyor. |

---

### Faz D: Ayarlar ve Kayıt

| Adım | Görev | Açıklama |
|------|--------|----------|
| **D1** | Register’ı apiClient’a taşı | Register sayfasında `apiClient.register({ username, email, password })` kullan; tutarlı hata/başarı yönetimi. |
| **D2** | SettingsPage – Profil | Kullanıcı adı değiştirme formu + `PUT api/User/profile/username`. |
| **D3** | SettingsPage – Şifre | Mevcut / yeni / tekrar şifre alanları + `PUT api/User/profile/password`. |
| **D4** | SettingsPage – Tercihler | Başlangıç sayfası (Home/Library) → localStorage; NSFW toggle → localStorage veya backend’de kullanıcı tercihi endpoint’i (varsa kullan). |
| **D5** | (İsteğe bağlı) E-posta doğrulama UI | Kayıt sonrası “Kod gönder” → “Doğrulama kodu gir” ekranı ve `POST api/User/verify-email`. |

---

### Faz E: İyileştirmeler ve Test

| Adım | Görev | Açıklama |
|------|--------|----------|
| **E1** | “Beni hatırla” | İsteğe bağlı: localStorage’da güvenli saklama (ör. email; şifre saklama önerilmez). |
| **E2** | Hata mesajları ve loading | Tüm yeni API çağrılarında hata/loading state’leri. |
| **E3** | Kütüphane invalidate | Oyun ekleme/çıkarma/status değişince ilgili React Query cache’ini invalidate et. |

---

## 4. Önerilen Uygulama Sırası (Teknik)

1. **A1** → **B1, B2, B3** (kütüphane status’u uçtan uca)
2. **A2** → **C2** (screenshots)
3. **C1** (detayda kütüphane butonu; A1/B1 sonrası)
4. **C3**, **C4** (kütüphane sayfası kaldırma, status, Dropped)
5. **A3**, **A4** → **D2**, **D3** (profil ve şifre)
6. **D1**, **D4** (register apiClient, tercihler)
7. **A5**, **D5** (e-posta doğrulama – isteğe bağlı)
8. **E1–E3** (hatırla, hata/loading, cache)

Bu plan ile legacy’deki özellikler yeni yığına adım adım ve uyumlu şekilde taşınabilir.
