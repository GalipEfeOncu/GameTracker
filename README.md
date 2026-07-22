# GameTracker

Your game library, organized. Discover titles from IGDB & RAWG, track what you play, and let AI suggest what's next — all from the browser or a lightweight Windows app.

[![CI](https://github.com/GalipEfeOncu/GameTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/GalipEfeOncu/GameTracker/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET 9](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 19](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![Electron 34](https://img.shields.io/badge/Electron-34-47848F?logo=electron&logoColor=white)](https://www.electronjs.org/)

**[Live Demo](https://game-tracker-virid.vercel.app/)** · **[Download for Windows](https://github.com/GalipEfeOncu/GameTracker/releases)**

---

## What it does

- **Library** — Tag every game as *Want to Play*, *Playing*, *Completed*, *Dropped*, or *On Hold*. Rate and review.
- **Catalog** — Browse and search a hybrid IGDB + RAWG catalog: covers, screenshots, platforms, store links, system requirements, Metacritic scores.
- **AI picks** — Hit a button and Gemini analyzes your library to suggest 15 titles you might like.
- **Playtime** — The desktop client watches for running processes (Steam, Epic, GOG, or manually added games) and syncs playtime to your account every 30 seconds.
- **Installed games** — Steam, Epic, and GOG libraries are detected automatically on Windows. Anything else can be linked manually.
- **Bilingual UI** — Turkish (default) and English, switchable from Settings.

### Web vs. Desktop

| | Web | Desktop (Windows) |
|-|-----|-------------------|
| Library, catalog, AI | Yes | Yes |
| Installed game detection | — | Yes |
| Playtime tracking | — | Yes |
| System tray & auto-start | — | Yes |

---

## How it's built

| Layer | Stack |
|-------|-------|
| Frontend | React 19, Vite 7, Tailwind CSS 4, TanStack Query, Framer Motion |
| API | ASP.NET Core (.NET 9), Dapper, MSSQL, JWT auth |
| Desktop | Electron 34, NSIS installer via electron-builder |
| Data | IGDB (Twitch) + RAWG hybrid |
| AI | Google Gemini |
| CI/CD | GitHub Actions, Vercel |

Auth uses short-lived access tokens (15 min) with rotating refresh tokens. 401s are retried silently; failed refreshes redirect to login.

---

# For developers

## Layout

```
backend/         .NET 9 Web API
frontend/        React SPA (Vite)
desktop/         Electron shell + playtime tracker
tests/           xUnit API tests
docs/            Deploy, DB, features, roadmap
```

## Prerequisites

- [.NET SDK 9](https://dotnet.microsoft.com/download)
- [Node.js LTS](https://nodejs.org/) + npm
- SQL Server (LocalDB or remote)

## Getting started

### 1 — Backend secrets

```bash
cd backend

dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "Igdb:ClientId" "TWITCH_CLIENT_ID"
dotnet user-secrets set "Igdb:ClientSecret" "TWITCH_CLIENT_SECRET"

# recommended
dotnet user-secrets set "ApiKeys:RawgApiKey" "YOUR_RAWG_KEY"

# optional — AI recommendations
dotnet user-secrets set "ApiKeys:GeminiApiKey" "YOUR_GEMINI_KEY"

# optional — email verification & password reset
dotnet user-secrets set "EmailSettings:MailAddress" "you@example.com"
dotnet user-secrets set "EmailSettings:MailPassword" "APP_PASSWORD"

# JWT signing (>= 32 chars)
dotnet user-secrets set "Jwt:SigningKey" "SOME_LONG_RANDOM_STRING_HERE_32+"
```

### 2 — Run the API

```bash
cd backend
dotnet run            # http://localhost:5118 — Swagger at /swagger
```

### 3 — Run the frontend

```bash
cd frontend
npm install
npm run dev           # http://localhost:5173
```

Set `VITE_API_BASE_URL` for production builds (must end with `/api`).

### 4 — Run the desktop client

```bash
# terminal 1
cd frontend && npm run dev

# terminal 2
cd desktop
npm install
npm run dev
```

Package an installer: `cd desktop && npm run build` → `dist-desktop/GameTracker Setup x.y.z.exe`

## Config reference

Sourced from `appsettings.json`, User Secrets, or env vars (`__` for nesting in production).

| Area | Keys |
|------|------|
| Database | `ConnectionStrings:GameTrackerDB` |
| IGDB | `Igdb:ClientId`, `Igdb:ClientSecret` |
| RAWG | `ApiKeys:RawgApiKey` |
| Email | `EmailSettings:MailAddress`, `MailPassword`, `SmtpHost`, `SmtpPort` |
| Production | `Cors:AllowedOrigins`, `Jwt:SigningKey` |

Full template: [`backend/appsettings.Example.json`](./backend/appsettings.Example.json)

## Tests

```bash
dotnet test            # runs tests/GameTracker.Api.Tests
```

## Deploy checklist

1. **API** — Deploy backend, ensure DB access, set env vars. Add your Vercel origin to `Cors__AllowedOrigins`.
2. **Web** — Configure `frontend/.env.production` → `npm run build` → deploy `dist/`.
3. **Desktop** — Same frontend env, then `cd desktop && npm install && npm run build`.
4. **Release** — Tag, attach `.exe` to [GitHub Releases](https://github.com/GalipEfeOncu/GameTracker/releases).

See [`docs/DEPLOY.md`](./docs/DEPLOY.md) for the full guide.

## Docs

| File | About |
|------|-------|
| [FEATURES.md](./docs/FEATURES.md) | Completed features |
| [ROAD_MAP.md](./docs/ROAD_MAP.md) | Roadmap |
| [DEPLOY.md](./docs/DEPLOY.md) | Hosting & environment |
| [DATABASE.md](./docs/DATABASE.md) | SQL Server setup |

---

MIT — © 2025 Galip Efe Öncü
