# Web + masaüstü dağıtım planı

**Amaç:** Aynı React (Vite) arayüzünü **web** olarak yayınlamaya devam etmek; **masaüstü** için tek bir “kabuk” ile paketlemek ve güncelleme yolunu tanımlamak. Backend (.NET API) ortak kalır; desktop istemci uzaktan API’ye bağlanır (yerel gömülü API bu planda yok).

**Referans:** [DEPLOY.md](./DEPLOY.md) · [ROAD_MAP.md](./ROAD_MAP.md) §2

---

## 1. Mevcut durum

| Parça | Durum |
|--------|--------|
| SPA | Vite 7 + React 19, `npm run build` → `dist/` |
| API kökü | `VITE_API_BASE_URL` (sonu `/api`); geliştirmede Vite proxy `/api` → `localhost:5118` |
| Üretim web | DEPLOY: ayrı barındırma, CORS `Cors__AllowedOrigins` |
| Masaüstü | Yok; kabuk seçilmedi |

---

## 2. Web tarafı (netleştirme, “dağıtım olgunluğu”)

Web zaten hedefleniyor; bu aşamada yapılacaklar **süreç ve tekrarlanabilirlik**:

1. **Build pipeline** — PR veya `main` için: `frontend` lint + `npm run build`; isteğe bağlı `backend` `dotnet test`.
2. **Ortamlar** — En az: geliştirme + üretim; isteğe bağlı “staging” API kökü ile önizleme.
3. **Env şablonları** — `frontend/.env.example` ile `VITE_API_BASE_URL` ve `VITE_DEV_PROXY_TARGET` dokümante (DEPLOY ile uyumlu).
4. **Sağlık kontrolü** — API `/health` (veya mevcut endpoint) ile deploy sonrası duman testi notu.

Bunlar desktop’tan bağımsız; web “tamam” sayılırken CI + env netliği yeterli.

---

## 3. Masaüstü stratejisi (yüksek seviye)

- **UI tek kaynak:** `frontend` build çıktısı (`dist`) kabuğa gömülür veya `file://` / `custom://` ile yüklenir; mümkün olduğunca **aynı axios tabanı** `VITE_API_BASE_URL` ile uzak API’ye gider.
- **Yerel dosya + uzak API:** `file` origin’de CORS tarayıcı gibi işlemez; Electron/Tauri **HTTP istemcisi** yine origin’den bağımsız çalıştığı için genelde sorun olmaz — yine de üretimde HTTPS ve doğru API URL şart.
- **Gömülü lokal API** (isteğe bağlı, bu planın dışı): Kullanıcıya “tek tık, internetsiz” senaryo; paket boyutu ve güvenlik güncellemesi maliyeti yüksek; **ilk sürümde önerilmez**.

---

## 4. Kabuk seçenekleri (karar tablosu)

| Seçenek | Artı | Eksi |
|---------|------|------|
| **Tauri 2** | Küçük paket, WebView2 (Windows’ta gömülü), Rust tarafı ince | Windows dışı hedef için ek test; Rust toolchain |
| **Electron** | Olgun ekosistem, `electron-builder`, auto-update hikâyesi net | Daha büyük indirme, Chromium maliyeti |
| **.NET MAUI Blazor WebView / WebView2** | Backend ekibi C# odaklıysa tek dil | SPA’yı “sadece WebView” ile taşımak kolay; cross-platform UI/mağaza karmaşıklığı |

**Öneri (taslak):** Önce **Tauri 2** veya **Electron** ile **Windows-only PoC**; macOS/Linux talebe göre ikinci adım. Resmi mağaza (Microsoft Store) hedefleniyorsa paketleme ve imzalama gereksinimleri ayrıca ele alınır.

### Karar — Electron (Faz A)

Faz A için **Electron + electron-builder** seçildi. Gerekçeler:

- **Toolchain maliyeti sıfır:** Repo zaten Node ekosistemini kullanıyor; ek Rust toolchain kurulumu gerekmiyor. Tauri 2'nin daha küçük paket boyutu avantajı PoC için belirleyici değil.
- **Olgun güncelleme hikâyesi:** `electron-updater` + GitHub Releases, Faz D ("auto-update") için kısa yol sunar.
- **Paketleme:** `electron-builder` ile Windows NSIS / MSI üretimi hazır; Authenticode imzalama adımı (Faz B) standart.
- **Kabuk ↔ renderer sınırı:** `contextBridge` + `preload.js` ile ileride §3 "yüklü oyun algılama" için sınırlı, denetlenebilir IPC yüzeyi açılabilir.

Trade-off olarak paket boyutu Tauri'ye göre büyüktür; ilk sürüm için kabul edildi. macOS / Linux talebi geldiğinde Electron aynı yapıyı sürdürür.

PoC iskeleti: [`desktop/`](../desktop/) (`src/main.js`, `src/preload.js`, `package.json`, `README.md`). Renderer build'i için Vite `base: './'` `VITE_DESKTOP=1` bayrağıyla açıldı (bkz. `frontend/vite.config.js`).

---

## 5. Fazlar (uygulama sırası)

### Faz A — Karar ve PoC (1–2 hafta hedefi, ekip hızına bağlı)

- [x] Kabuk: **Electron** seçildi — gerekçe bu dosyada "Karar" alt başlığında.
- [x] `desktop/` altında minimal proje: `BrowserWindow` → packaged renderer (`resources/renderer/index.html`) veya `GAMETRACKER_DEV_SERVER_URL` yüklüyor (`desktop/src/main.js`).
- [x] `VITE_API_BASE_URL` desktop build'inde mutlak üretim API köküne işaret edecek şekilde `desktop/.env.example` ve `desktop/README.md` ile dokümante; Vite `base: './'` için `VITE_DESKTOP=1` build flag'i eklendi ve `desktop` `build:renderer` scripti bu bayrağı otomatik geçiriyor.

**Açık Faz A takip işleri (gelecek agent):** `desktop/` içinde `npm install` çalıştırıldıktan sonra (1) `npm run dev` ile Vite dev server'a bağlanan pencerenin tam ekran duman testi, (2) `frontend/.env.production` ile canlı API'ye işaretli `npm run build:dir` (NSIS imzasız, unpacked), (3) React Router `file://` uyumluluğu için `HashRouter` geçişi kararı (bkz. `desktop/README.md` sınırlamalar).

### Faz B — Paketleme ve imzalama

- [ ] Windows: `.exe` / kurulum (NSIS / MSI / Tauri bundle) — hangi format seçildiyse.
- [ ] Kod imzalama (Authenticode): üretim için sertifika maliyeti ve CI sırları.
- [ ] Sürüm numarası: `package.json` / `tauri.conf.json` / `electron` ile hizalama.

### Faz C — CI

- [ ] GitHub Actions (veya mevcut CI): `frontend build` → kabuk build → artefakt yükleme (Release).
- [ ] Etiket sürümü (`v1.0.0`) ile sürüm notu şablonu.

### Faz D — Güncelleme (ROAD_MAP “Sürümleme ve güncelleme” maddesi)

- [ ] **Tauri:** `updater` + güncelleme sunucusu veya GitHub Releases.
- [ ] **Electron:** `electron-updater` + aynı.
- [ ] Kullanıcıya “Yeni sürüm var” davranışı (zorunlu / isteğe bağlı) ürün kararı.

---

## 6. Riskler ve bağımlılıklar

- **CORS:** Web SPA için zorunlu; desktop HTTP istemcisi genelde doğrudan API URL kullanır — yine de cookie/JWT ile ileride `credentials` kullanılırsa API tarafı header’ları gözden geçirilmeli.
- **Güvenlik:** Masaüstü içinde gömülü token depolama (keychain / DPAPI) ileride düşünülebilir; ilk aşamada web ile aynı localStorage davranışı kabullenilebilir (risk kabulü).
- **§3 Desktop oyun:** Yüklü oyun tarama **native** kod veya kabuk eklentisi ister; bu plan yalnızca **dağıtım kabuğu**. Oyun listesi özelliği ayrı modül olacaksa Tauri **command** / Electron **preload** API sınırı önceden tasarlanmalı.

---

## 7. Sonraki somut adım

1. Bu belgede **Faz A** için kabuk seçimini işaretle (Tauri önerisi veya Electron).
2. `desktop/` (veya seçilen yapı) altında PoC branch’i aç; `README`de “Desktop geliştirme” 5 satır.
3. ROAD_MAP §2 alt görevlerini fazlara göre tikle.

---

*Son güncelleme: plan taslağı; uygulama ilerledikçe faz kutuları ve karar bölümü güncellenmeli.*
