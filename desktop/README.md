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
# Önce frontend'de üretim API kökü (repoda: .env.production.example -> .env.production)
cd frontend
Copy-Item .env.production.example .env.production   # bir kez; dosyayı düzenle: VITE_API_BASE_URL=https://.../api
cd ..\desktop
npm run build              # NSIS: GameTracker Setup x.y.z.exe -> dist-desktop/
# veya sadece unpacked klasör:
npm run build:dir
```

### İkonlar

Kaynak: repoda kök dizin `GameTracker_Icon.png`. Aşağıdaki komut onu okuyup `desktop/assets/tray.png`, `desktop/assets/icon.ico` ve web için `frontend/public/favicon.png` üretir:

```powershell
cd desktop
npm run generate:icons
```

İkon tasarımını güncellediğinde kök PNG’yi değiştir → komutu tekrar çalıştır → `npm run build`.

Yayın öncesi `frontend/.env.production` içinde **tam** `VITE_API_BASE_URL` olmalı (örn. `https://senin-api.com/api`); `file://` ile göreli `/api` çalışmaz.

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
  assets/               # tray.png, icon.ico (kurulum / görev çubuğu)
  scripts/generate-icons.cjs
  .env.example
  src/
    main.js             # pencere, tray, IPC, tracker
    preload.js          # contextBridge
    detectors/          # Steam / Epic / GOG
```
