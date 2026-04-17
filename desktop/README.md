# GameTracker — Desktop (Electron, PoC)

Minimal Electron kabuğu; mevcut Vite SPA (`frontend/`) üretim build'ini yükler ve uzak .NET API'ye `VITE_API_BASE_URL` ile bağlanır. Backend gömülmez.

Kabuk kararı ve fazlar için: [`../docs/PLAN_WEB_DESKTOP.md`](../docs/PLAN_WEB_DESKTOP.md) · Yol haritası için: [`../docs/ROAD_MAP.md`](../docs/ROAD_MAP.md) §2.

## Hızlı geliştirme (5 dakika)

```powershell
# 1) Tek seferlik: kök bağımlılıklar
cd frontend; npm ci; cd ..\desktop; npm install

# 2) Terminal A — Vite dev server
cd frontend; npm run dev          # http://localhost:5173

# 3) Terminal B — Electron kabuğu dev server'a bağlansın
cd desktop; npm run dev           # GAMETRACKER_DEV_SERVER_URL=http://localhost:5173
```

## Üretim benzeri paket çalıştırma (Windows)

```powershell
# frontend build + Electron paketi (NSIS installer -> desktop/dist-desktop/)
cd frontend
# PoC: uzak API köküne işaret eden env (frontend/.env.production veya shell export)
"VITE_API_BASE_URL=https://gametracker-api.example.com/api" | Out-File -Encoding ascii .env.production
cd ..\desktop
npm run build              # .exe installer üretir
# veya sadece unpacked klasör:
npm run build:dir
```

> Renderer (Vite) build'i `file://` protokolünde yükleneceği için Vite `base` `./` olarak ayarlandı (bkz. `frontend/vite.config.js`). SPA içinde **mutlaka** mutlak `VITE_API_BASE_URL` kullanılmalı; aksi halde apiClient `/api`'ye düşer ve `file:///api/...` çağrıları başarısız olur.

## Bilinen PoC sınırlamaları (Faz A kapsamında)

- Kod imzalama yok (Faz B).
- Auto-update yok (Faz D); sürümler elle indirilir.
- React Router altında tam "hard reload" `file://` başarısız olur (MVP için kabul; navigasyon tıklamaları çalışır). İleride `HashRouter` veya `custom://` şeması düşünülebilir.
- Renderer içi `window.location.assign('/login')` çağrıları (apiClient 401 işleyicisi) `file://` kökünde anlamlı değil — ileride `electron` kontrol akışına taşınmalı.
- `ROAD_MAP.md` §3 ("yüklü oyun algılama") bu kabuğun dışında; preload üzerinden sınırlı IPC yüzeyi ileride eklenecek.

## Dosya yapısı

```
desktop/
  package.json          # electron + electron-builder + scripts
  .env.example          # VITE_API_BASE_URL ve GAMETRACKER_DEV_SERVER_URL notu
  src/
    main.js             # BrowserWindow + file:// / dev URL yükleyici
    preload.js          # contextBridge (şimdilik boş)
```
