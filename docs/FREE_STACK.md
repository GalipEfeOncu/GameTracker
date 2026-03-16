# GameTracker – Tamamen Ücretsiz Çalıştırma Rehberi

Düşük kullanıcı sayısı için tüm bileşenleri ücretsiz tier / free limitlerle kullanmak.

---

## 1. Veritabanı: MSSQL (Ücretsiz Seçenekler)

| Seçenek | Açıklama | Limit |
|--------|----------|--------|
| **SQL Server Express** | Microsoft’un ücretsiz sürümü. Lokal kurulum veya kendi sunucunuzda/Docker’da çalıştırılır. | DB başına 10 GB, 1 GB RAM (Express 2019+). Kullanıcı sayısı sınırı yok. |
| **Ücretsiz MSSQL hosting (Somee vb.)** | Bazı sağlayıcılar ücretsiz veya çok düşük limitli MSSQL verir. | Genelde 50–200 MB, sınırlı bağlantı. Küçük projeler için yeterli. |
| **Azure SQL Database (Free)** | Azure’da ücretsiz katman. | Çok küçük (ör. 32–100 MB). Sadece deneme / çok hafif kullanım. |

**Öneri (düşük kullanıcı):** Zaten ücretsiz MSSQL hosting kullanıyorsanız aynı yapıyı kullanın. İleride daha kontrol isterseniz kendi makinenizde veya ücretsiz VPS’te **SQL Server Express** (veya Docker ile) çalıştırabilirsiniz.

---

## 2. Backend (.NET API) – Ücretsiz Hosting

| Servis | Ücretsiz limit | Not |
|--------|-----------------|-----|
| **Render** | 750 saat/ay (1 web service), uyku modu (ilk istekte ~30 sn bekleme) | En kolay: GitHub’a push → otomatik deploy. |
| **Railway** | Aylık $5 kredi (yaklaşık sürekli 1 servis için yeter) veya sınırlı free tier | Basit deploy, env variable desteği. |
| **Fly.io** | Küçük VM’ler ücretsiz (pay as you go’da 0’a yakın kalabilir) | Docker ile deploy. |
| **Azure App Service** | Free tier (F1) – sınırlı CPU/saat | .NET için uyumlu. |

**Öneri:** **Render** veya **Railway** ile backend’i deploy edin; connection string ve API anahtarlarını ortam değişkeni (environment variable) olarak verin.

---

## 3. Frontend (React / Vite) – Ücretsiz Hosting

| Servis | Limit | Not |
|--------|--------|-----|
| **Vercel** | Projeler için ücretsiz, bant genişliği limiti | Vite/React için ideal, GitHub entegrasyonu. |
| **Netlify** | Ücretsiz tier, bant limiti | Static site / SPA için uygun. |
| **GitHub Pages** | Ücretsiz | Static; SPA için `base` ve fallback ayarı gerekir. |

**Öneri:** **Vercel** veya **Netlify**; build komutu `npm run build`, output `dist`.

---

## 4. Harici API’ler (Zaten Kullandığınız)

| Servis | Ücretsiz limit |
|--------|-----------------|
| **RAWG** | Ücretsiz plan: günlük istek limiti (düşük trafik için yeterli). |
| **Google Gemini** | Ücretsiz tier: dakika/istek limitleri. |

Limitleri ilgili sitelerden kontrol edin; düşük kullanıcıda genelde yeterli kalır.

---

## 5. Güvenlik – Hassas Bilgileri Repo’da Tutmayın

- **Connection string**, **API key**, **e-posta şifresi** gibi değerleri `appsettings.json` içinde **commit etmeyin**.
- Production’da bu değerleri **environment variable** olarak ayarlayın (Render, Railway, Fly.io hepsi destekler).
- Örnek: `ConnectionStrings__GameTrackerDB`, `ApiKeys__RawgApiKey`, `ApiKeys__GeminiApiKey`, `EmailSettings__MailAddress`, `EmailSettings__MailPassword`.

Backend’de `Program.cs` veya `AppConfig` zaten `IConfiguration` ile bu değişkenleri okuyorsa ekstra kod gerekmez.  
**Önemli:** `appsettings.json` içinde şifre veya API anahtarı commit etmeyin. Production’da sadece env variable kullanın; lokal geliştirme için `appsettings.Development.json` kullanıp bu dosyayı `.gitignore`’a ekleyabilirsiniz.

---

## 6. Özet: Tamamen Ücretsiz Yığın

| Bileşen | Önerilen ücretsiz seçenek |
|---------|---------------------------|
| Veritabanı | Mevcut ücretsiz MSSQL hosting veya SQL Server Express (kendi sunucu/Docker) |
| Backend | Render veya Railway (env variable ile connection string ve API key’ler) |
| Frontend | Vercel veya Netlify |
| RAWG / Gemini | Zaten ücretsiz tier kullanımı |

Bu yapı ile düşük kullanıcılı, tamamen ücretsiz çalışan bir GameTracker kurulumu mümkün.
