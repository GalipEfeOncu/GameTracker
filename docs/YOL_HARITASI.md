# GameTracker – Yol Haritası

**Hedef:** Önce her şeyi **lokalde** bitir, sonra projeyi hem **web uygulaması** hem **masaüstü uygulaması** olarak yayınlamak.

---

## Genel akış

```
1. Lokal geliştirme (şu an buradayız)
   → Backend + Frontend tamamlanır, bilgisayarınızda test edilir.

2. Web uygulaması yayını
   → Frontend (React) + Backend (API) internette bir adreste yayınlanır.
   → Kullanıcılar tarayıcıdan erişir.

3. Masaüstü uygulaması yayını
   → Aynı web arayüzü Electron ile paketlenir VEYA mevcut WinForms uygulaması güncellenir.
   → Kullanıcılar .exe kurup bilgisayarda çalıştırır.
```

---

## Faz 1 (Faz A) – Neler yapıldı?

Faz 1’de **sadece backend** tarafında işlem yapıldı. Frontend’e dokunulmadı.

| Yapılan | Açıklama |
|--------|----------|
| **Güvenlik** | `appsettings.json` içindeki gerçek şifre/API anahtarları kaldırıldı. Şablon için `appsettings.Example.json` eklendi. Gerçek değerler artık `appsettings.Development.json` (lokalde) veya ortam değişkenleri (sunucuda) ile verilecek. |
| **A1 – Kütüphane status** | Kullanıcı kütüphanesi listesi artık her oyun için `status` (Playing, PlanToPlay, Played, Dropped) döndürüyor. |
| **A2 – Screenshots** | Yeni endpoint: `GET api/Library/game/{id}/screenshots` → Oyunun ekran görüntüleri. |
| **A3 – Kullanıcı adı** | Yeni endpoint: `PUT api/User/{userId}/profile/username` → Kullanıcı adı değiştirme. |
| **A4 – Şifre** | Yeni endpoint: `PUT api/User/{userId}/profile/password` → Şifre değiştirme (mevcut şifre doğrulanıyor). |

Bu sayede Faz 2’de frontend bu API’leri kullanabilecek.

---

## Faz 1 sonrası – Sizin yapmanız gereken (tek şey)

Backend’i **lokalde çalıştırabilmeniz** için yapılandırma gerekli. `appsettings.json` artık boş; uygulama gerçek veritabanı ve API anahtarları olmadan açılmaz.

### Adımlar

1. **`backend`** klasörüne gidin.
2. **`appsettings.Example.json`** dosyasını kopyalayıp adını **`appsettings.Development.json`** yapın (aynı klasörde).
3. **`appsettings.Development.json`** dosyasını açın ve placeholder’ları kendi değerlerinizle değiştirin:
   - `ConnectionStrings:GameTrackerDB` → SQL Server bağlantı cümleniz
   - `ApiKeys:RawgApiKey` → RAWG API anahtarınız
   - `ApiKeys:GeminiApiKey` → Gemini API anahtarınız
   - (E-posta kullanacaksanız) `EmailSettings:MailAddress` ve `MailPassword`

Bu dosya `.gitignore`’da olduğu için Git’e **commit edilmez**; güvenli kalır.

4. Backend’i çalıştırın:
   ```bash
   cd backend
   dotnet run
   ```
5. Frontend’i ayrı terminalde çalıştırın:
   ```bash
   cd frontend
   npm run dev
   ```

Bundan sonra Faz 2’ye (frontend API + UI) geçebilirsiniz; lokalde her şey bu yapılandırmayla çalışır.

---

## Faz 2 ve sonrası (kısa özet)

- **Faz 2 (Faz B):** Frontend’e apiClient fonksiyonları (register, kütüphane ekle/çıkar/status, screenshots) ve status eşlemesi eklenir.
- **Faz 3 (Faz C):** Oyun detay sayfasında “Kütüphaneye Ekle” butonu, screenshots alanı; kütüphane sayfasında kaldırma ve status değiştirme UI.
- **Faz 4 (Faz D):** Ayarlar sayfası (kullanıcı adı/şifre değiştirme, başlangıç sayfası vb.).

Tüm bu fazlar **lokalde** tamamlanır; tek gereken yukarıdaki `appsettings.Development.json` kurulumu.

---

## Yayınlama: Web + Masaüstü

### Web uygulaması

- **Frontend:** Vercel / Netlify / GitHub Pages (ücretsiz) ile yayınlanır.
- **Backend:** Render / Railway / Fly.io (ücretsiz tier) ile yayınlanır.
- **Veritabanı:** Mevcut MSSQL (Somee vb. ücretsiz hosting) veya SQL Server Express kendi sunucunuzda.

Önce lokalde bitirirsiniz; bitince `docs/FREE_STACK.md` içindeki adımlarla deploy edebilirsiniz.

### Masaüstü uygulaması

İki mantıklı seçenek:

| Seçenek | Açıklama |
|--------|----------|
| **Electron** | Mevcut React (frontend) uygulaması Electron ile paketlenir. Kullanıcı bir .exe kurar; uygulama içinde aynı arayüz açılır ve **yayınladığınız API adresine** bağlanır. Veri yine buluttaki backend + DB’de kalır. |
| **Mevcut WinForms (GameTracker)** | Repodaki `GameTracker` (C# WinForms) projesi zaten masaüstü uygulaması. İsterseniz bu uygulamayı da aynı backend API’yi kullanacak şekilde güncelleyip hem web hem bu .exe ile aynı veritabanını kullanabilirsiniz. |

Yani:
- **“Tek kod tabanı, hem web hem masaüstü”** derseniz → React + Electron.
- **“Ayrıca eski Windows uygulamasını da sürdüreyim”** derseniz → WinForms’u aynı API’ye bağlayıp iki ürün (web + desktop) sunarsınız.

---

## Özet

| Soru | Cevap |
|------|--------|
| Faz 1’de ne yaptık? | Backend’e kütüphane status, screenshots endpoint’i, kullanıcı adı ve şifre güncelleme endpoint’leri eklendi; hassas bilgiler `appsettings`’ten alınıp şablona taşındı. |
| Ben ne yapmalıyım? | `backend` içinde `appsettings.Example.json` → `appsettings.Development.json` kopyalayıp kendi DB ve API anahtarlarınızı yazın. Sonra backend + frontend’i lokalde çalıştırıp test edin. |
| Faz 2’ye ne zaman geçerim? | Bu tek adımı yaptıktan sonra geçebilirsiniz; Faz 2 frontend tarafında API kullanımı ve UI’dır. |
| Web + desktop nasıl yayınlarım? | Önce tüm fazları lokalde bitirin. Web: frontend + backend’i Vercel/Render vb. ile yayınlayın. Masaüstü: React’ı Electron ile paketleyin veya WinForms’u aynı API’ye bağlayın. |

Detaylı teknik adımlar için: `MIGRATION_PLAN.md`, `docs/SECURITY.md`, `docs/FREE_STACK.md`.
