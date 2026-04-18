# AGENT HANDOFF — GameTracker

Bu dosya, projeyi sürdürecek AI ajanlar içindir. Her yeni oturumda **önce bu dosyayı baştan sona oku**, sonra [ROAD_MAP.md](./ROAD_MAP.md)'e bak.

> Sahibi: hobi projesi, **hızlı bitirmek** öncelik. Aşırı mühendislik yapma. Kullanıcı Türkçe cevap bekler.

---

## 1. Kısa mimari

| Katman | Teknoloji | Konum |
|--------|-----------|-------|
| Backend | .NET 9 Web API + MSSQL (Somee) | `backend/` |
| Frontend | React 18 + Vite 7 + Tailwind + React Router + TanStack Query | `frontend/` |
| Desktop kabuğu | Electron 34 (Vite SPA'yı yükler) | `desktop/` |
| CI | GitHub Actions: `dotnet build/test` + frontend `npm run lint/build` | `.github/workflows/ci.yml` |

**Veri akışı:** Desktop / Web → .NET API (JWT) → MSSQL. IGDB + RAWG hibrit oyun verisi. Gemini → AI oyun önerisi.

**Auth:** 15 dk'lık access JWT + rotasyonlu refresh token (30 / 90 gün `RememberMe`'ye göre). 401 alan her istek bir kez otomatik refresh dener; refresh de patlarsa login'e yönlenir.

**Desktop özel:** Electron main process tanınan `.exe` process'lerini izler, 30 sn'de bir `POST /api/Library/user/{id}/playtime/{gameId}` ile dakika delta'sı gönderir. Tray + autostart desteği var.

---

## 2. Ortam / çalıştırma komutları

### PowerShell kritik kuralı

`&&` **ÇALIŞMAZ**. Ardışık komut için:

```powershell
npm run lint; if ($LASTEXITCODE -eq 0) { npm run build }
```

### Backend

```powershell
cd backend
dotnet build --nologo -clp:ErrorsOnly   # hızlı doğrulama
dotnet test  --nologo                    # testler
dotnet run                                # localhost:5000 / :5001
```

Eğer `dotnet build` "file is locked" (MSB3027) verirse: arka planda `dotnet run` vardır. Önce şu komutla bul:

```powershell
Get-Process -Name GameTracker.Api -ErrorAction SilentlyContinue | Stop-Process -Force
```

### Frontend

```powershell
cd frontend
npm install    # yalnızca lockfile değişince
npm run dev    # http://localhost:5173
npm run lint; if ($LASTEXITCODE -eq 0) { npm run build }
```

### Desktop

```powershell
# Birinci terminal:
cd frontend; npm run dev

# İkinci terminal:
cd desktop
npm install   # yalnızca ilk kez
npm run dev   # Vite dev server'a bağlanır
```

Paketli build: `cd desktop; npm run build` → `desktop/dist-desktop/` altında NSIS installer.

`.env` dosyaları:
- `frontend/.env` → `VITE_API_BASE_URL`, örn. `http://localhost:5000/api`
- `desktop/.env` → geliştirmede `GAMETRACKER_DEV_SERVER_URL=http://localhost:5173`

---

## 3. Proje durumu — özet

**Tamam:** Tüm ROAD_MAP §1–§5. Güvenlik (refresh token), veri modeli, yüklü oyun algılama (Steam/Epic/GOG), playtime tracker, tray, autostart, manuel oyun ekleme, IGDB arama ile eşleme. **Arayüz dili:** Türkçe (varsayılan) + İngilizce — `PreferencesContext.locale` + `frontend/src/i18n/*.json`, seçim **Ayarlar → Tercihler → Arayüz dili**; `localStorage` `gt_preferences` içinde kalıcı.

**Açık:** Yalnızca §6 **NSIS installer** — `.exe` üretip paylaşılabilir hale getirmek. Kod imzalama / auto-update / sanal liste / 2FA **kapsam dışı** (i18n artık uygulamada; bkz. yukarı).

Detay için [ROAD_MAP.md](./ROAD_MAP.md).

---

## 4. Bilinen bug'lar / dikkat edilecekler

### 4.1. Playtime rozeti görünmüyor

**Olasılıklar (en yaygından):**

1. **Migration çalıştırılmamış.** `backend/scripts/AddPlaytimeMinutes.sql` Somee veya SSMS'te çalıştırılmadıysa `GetUserLibrary` 500 döner (SELECT "playtime_minutes" yok). Önce SQL'in çalıştığını teyit ettir.
2. **İlk dakika dolmamış.** Delta tam dakika gönderilir (`Math.floor(totalSec / 60)`). 59 sn oyun açık kaldıysa hâlâ 0 yazar.
3. **Tracker oyunu tanımıyor.** Yüklü Oyunlarım sayfasında oyun "İzlenen Oyunlar" bölümünde değilse heartbeat gitmez. Oyunu algılananlardan "Bağla" ile veya manuel ekle.
4. **Session aktarılmamış.** Kullanıcı desktop'ta giriş yapmışken `setDesktopSession` çağrılmadıysa tracker `no_access_token` hatasıyla susar. `frontend/src/context/UserContext.jsx` → `login()` ve `useEffect` mount'ta IPC'yi çağırır. Eski oturumla açık bir desktop'ta bir kez logout → login gerekir.
5. **Gösterilmeyen yerler.** Playtime yalnızca **(a)** `GameCard` (kütüphane grid'i) ve **(b)** `InstalledGamesPage` izlenen oyunlar satırında görünür. `GameDetailsPage` henüz göstermez — eklenmek istenirse `frontend/src/pages/GameDetailsPage.jsx` içinde kullanıcı kütüphanesinden oyunu bul, `playtimeMinutes` alanını oku.

**Hızlı debug:**
- Electron DevTools → Network → `POST /playtime/{id}` 30 sn'de geliyor mu?
- Gelmiyor: Electron main process konsolu (terminal) → `[tracker]` uyarısı var mı?
- Geliyor: Response `{ playtimeMinutes: N }` dönüyor mu? Sonra `GET /Library/user/{id}` cevabındaki oyunun `playtimeMinutes` field'ı N mi?

### 4.2. PowerShell tuzakları

- Çoklu komutta `&&` yok, `;` veya `if ($LASTEXITCODE -eq 0)`.
- Dosya içeriği oluşturmak için `echo > file` yerine `Write` tool'u kullan (Cursor).

### 4.3. `dotnet build` takılması

`GameTracker.Api.exe` arka planda çalışırken build başarısız olur. Fix §2'de yazıldı.

### 4.4. Desktop process adlarında ortak çakışma

Aynı `.exe` adı (ör. `Game.exe`) iki farklı oyunda varsa tracker ikisine birden saniye yazar. Kullanıcı manuel eklemede daha spesifik bir exe seçmeli. Kritik değil; not olarak kalsın.

---

## 5. Kod stil kuralları (koyu cümleler ZORUNLU)

- **Yorumlar narratif olmasın.** "// Import the module", "// Increment counter" gibi obvious yorum ekleme. Sadece **niye** ve non-obvious trade-off açıklaması.
- **Mevcut dosyaları düzenle.** Yeni dosya/belge yaratmadan önce "gerekli mi" diye sor. Doc dosyası (`*.md`) proaktif açma.
- **Tailwind sınıflarını dağıtma.** Mevcut konvansiyon: `rounded-none`, koyu tema (`bg-[#141722]`, `border-[#1f2334]`, `text-gray-*`), mavi aksan (`blue-500/600`). Yeni renk palette'i ekleme.
- **Dil / kopya.** Kullanıcı metinleri `frontend/src/i18n/tr.json` ve `en.json` üzerinden `useI18n().t(anahtar)` ile verilir; varsayılan dil Türkçe. Yeni metin: her iki sözlüğe anahtar ekle. Axios/React dışı toast: `translate(getStoredLocale(), 'api.*')` (`apiClient.js`). Toast mesajları `emitToast(mesaj, 'success'|'error')`.
- **JSON casing.** Backend Newtonsoft kullanıyor; DTO property'lerde `[JsonProperty("camelCase")]` varsa uy. Yoksa default camelCase.
- **Güvenlik:**
  - Yeni endpoint'ler `[Authorize]` + `User.TryGetUserId(out var authedId) && authedId == userId` kontrolü yapmalı (yoksa yatay yetki açığı).
  - Hassas değişikliklerde (parola, hesap silme) `_refresh.RevokeAllForUser` çağır.
- **React Query:** Mutation sonrası `queryClient.invalidateQueries({ queryKey: ['library', userId] })` ile ilgili query'leri tazele.

---

## 6. Dosya haritası (kısayol)

```
backend/
  Controllers/{Library,User}Controller.cs       REST uçları
  Managers/{Library,User}Manager.cs             DB işlemleri (statik)
  Services/{Gemini,Igdb,Rawg,RefreshToken,VerificationCode}Service.cs
  Auth/JwtTokenService.cs                       15 dk access token üretir
  Models/Game.cs                                oyun DTO'su (playtimeMinutes dahil)
  LibraryStatuses.cs                            'Played' → 'Completed' normalize
  scripts/*.sql                                 idempotent migration'lar
  appsettings.json                              Jwt, Cors, ConnectionStrings

frontend/src/
  api/apiClient.js                              Axios + 401 refresh interceptor
  context/{User,Preferences,Toast}Context.jsx  Preferences: locale tr|en + diğer tercihler
  i18n/{t.js,useI18n.js,playtime.js,tr.json,en.json}  Çeviri anahtarları + html lang
  pages/{Popular,Discover,Library,Installed,Ai,Settings,Login,...}Page.jsx
  components/{GameCard,Sidebar,Layout,...}.jsx
  desktop/bridge.js                             window.gameTracker wrapper (web'de no-op)
  constants/libraryStatus.js                    LIBRARY_STATUS + LIBRARY_TABS (etiketler i18n'den)

desktop/src/
  main.js                                       Electron ana süreç, tray, IPC, tracker lifecycle
  preload.js                                    contextBridge IPC yüzeyi
  tracker.js                                    process izleme + heartbeat
  detectors/{steam,epic,gog,index}.js
  lib/{vdf,store,registry,api}.js

docs/
  AGENT_HANDOFF.md                              ← BU DOSYA (her oturum başı oku)
  ROAD_MAP.md                                   kalan işler + neler kapalı
  DATABASE.md                                   migration listesi
  DEPLOY.md                                     prod konfig
  PLAN_WEB_DESKTOP.md                           masaüstü planı
```

---

## 7. Masaüstü dağıtım (NSIS) — MVP tamam

- İkonlar: `desktop/assets/tray.png`, `icon.ico` — `npm run generate:icons` (yer tutucu mavi kare; kendi tasarımınla değiştir).
- Paket: `cd desktop && npm run build` → `dist-desktop/GameTracker Setup x.y.z.exe` (tam kurulum için NSIS).
- Kök `README.md` — “Download (Windows desktop)”; imzasız dağıtım için GitHub Releases’e `.exe` elle eklenir.
- Authenticode / `electron-updater` / CI artefakt: **kapsam dışı** (bkz. §9).

---

## 8. Hızlı smoke test (her büyük değişiklikten sonra)

1. Backend: `dotnet build` → 0 error. `dotnet test` → tüm testler geçer.
2. Frontend: `npm run lint` + `npm run build` → 0 error.
3. Manuel:
   - Login → Popüler / Keşfet / Kütüphane açılıyor mu?
   - Kütüphaneye oyun ekle, durumu "Playing" yap.
   - (Desktop) Yüklü Oyunlarım → algılanan bir oyunu "Bağla" → Settings toggle'ları çalışıyor mu?

Bir şey kırıldığında ilk bakılacak yer: Electron main process terminali + browser DevTools Network.

---

## 9. Yapma listesi (kapsam dışı)

- ❌ Ek diller (TR/EN dışında) veya `react-i18next` seviyesinde kütüphane — mevcut JSON + `translate()` yeterli
- ❌ Sanal liste (`react-virtual`) — mevcut 2×N grid yeterli
- ❌ 2FA, oturum listesi sayfası
- ❌ ESLint kural sıkılaştırma (CI zaten lint çalıştırıyor)
- ❌ Kod imzalama sertifikası
- ❌ Auto-update
- ❌ Xbox / Battle.net / EA App algılayıcıları (Steam+Epic+GOG yeter; gerisi manuel eklenir)
- ❌ AFK tespiti (karar verildi: oyun açıksa sayılır)
