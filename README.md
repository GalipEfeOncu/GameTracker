# GameTracker

**GameTracker** is a full-stack game library, discovery, and (optionally) playtime-tracking application. Manage your backlog, browse catalog data from **IGDB** and **RAWG**, and—with the Windows desktop client—surface installed titles and sync playtime to your cloud library.

[![CI](https://github.com/GalipEfeOncu/GameTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/GalipEfeOncu/GameTracker/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-7-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)

<table>
<tr>
<td><strong>Live web app</strong></td>
<td><a href="https://game-tracker-virid.vercel.app/">game-tracker-virid.vercel.app</a> · <a href="https://game-tracker-virid.vercel.app/popular">Popular games</a></td>
</tr>
<tr>
<td><strong>Source</strong></td>
<td><a href="https://github.com/GalipEfeOncu/GameTracker">github.com/GalipEfeOncu/GameTracker</a></td>
</tr>
<tr>
<td><strong>Windows installer</strong></td>
<td><a href="https://github.com/GalipEfeOncu/GameTracker/releases">GitHub Releases</a> (<code>GameTracker Setup *.exe</code>)</td>
</tr>
</table>

<p align="center">
  <img src="https://github.com/user-attachments/assets/19471807-5380-4fd6-b81a-91de8cb21183" alt="GameTracker — library view" width="780" />
</p>

---

## Highlights

- **Discover & search** — Hybrid IGDB-first + RAWG enrichment for platforms, stores, and requirements  
- **Personal library** — Track status, ratings, notes, and metadata backed by JWT-authenticated REST APIs  
- **Optional AI hints** — Gemini-powered recommendations when configured  
- **Desktop (Windows)** — Electron shell loads the same SPA; optional process-based playtime sync to the API  
- **Production-ready auth** — Short-lived access tokens, rotating refresh tokens, email verification hooks  

---

## Repository layout

| Layer | Stack | Path |
|--------|--------|------|
| Web UI | React 19, Vite 7, Tailwind, TanStack Query, React Router | [`frontend/`](./frontend/) |
| API | ASP.NET Core (.NET 9), JWT, rate limiting, MSSQL | [`backend/`](./backend/) |
| Desktop | Electron (bundles SPA build), NSIS installer | [`desktop/`](./desktop/) |
| Tests | xUnit API tests | [`tests/GameTracker.Api.Tests`](./tests/GameTracker.Api.Tests/) |

---

## Quickstart (local development)

### Prerequisites

- [.NET SDK 9.x](https://dotnet.microsoft.com/download)
- Node.js (**LTS** recommended) and npm  
- SQL Server (LocalDB or remote) for persisted auth and library data  

### 1. Backend secrets (`backend/`)

Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:

```bash
cd backend

dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "Igdb:ClientId" "TWITCH_CLIENT_ID"
dotnet user-secrets set "Igdb:ClientSecret" "TWITCH_CLIENT_SECRET"

# Recommended — richer metadata
dotnet user-secrets set "ApiKeys:RawgApiKey" "YOUR_RAWG_KEY"

# Optional — AI suggestions
dotnet user-secrets set "ApiKeys:GeminiApiKey" "YOUR_GEMINI_KEY"

# Optional — registration / password flows
dotnet user-secrets set "EmailSettings:MailAddress" "example@mail.com"
dotnet user-secrets set "EmailSettings:MailPassword" "APP_PASSWORD"

# Production-like JWT (local)
dotnet user-secrets set "Jwt:SigningKey" "A_SECURE_RANDOM_KEY_AT_LEAST_32_CHARS"
```

In **Development**, Swagger is available. In **Production**, CORS origins must be configured or the API will not start — see [`docs/DEPLOY.md`](./docs/DEPLOY.md).

### 2. Run the API

```bash
cd backend
dotnet run
```

Default URL (may vary): `http://localhost:5118` · Swagger (dev): `/swagger`

### 3. Run the frontend

```bash
cd frontend
npm install
npm run dev
```

For production builds, set `VITE_API_BASE_URL` (must end with `/api`). Copy [`frontend/.env.production.example`](./frontend/.env.production.example) → `frontend/.env.production` (gitignored).

---

## Ship checklist

1. **API** — Deploy backend; DB reachable; env vars set ([`docs/DEPLOY.md`](./docs/DEPLOY.md)). Include your Vercel origin in **`Cors__AllowedOrigins`**.  
2. **Web** — `frontend/.env.production` with live API URL → `npm run build` → deploy `frontend/dist/`.  
3. **Desktop** — Same SPA env as step 2, then `cd desktop && npm install && npm run build`. Artifact: `desktop/dist-desktop/GameTracker Setup x.y.z.exe`.  
4. **Smoke test** — Login, library, catalog search; desktop: installed games + playtime if applicable.  
5. **Release** — Tag and attach the `.exe` on [GitHub Releases](https://github.com/GalipEfeOncu/GameTracker/releases).  

---

## Configuration reference

Values can come from `appsettings.json`, User Secrets, or environment variables (production nesting uses `__`).

| Area | Keys / notes |
|------|----------------|
| Database | `ConnectionStrings:GameTrackerDB` |
| IGDB | `Igdb:ClientId`, `Igdb:ClientSecret` (Twitch app) |
| RAWG (detail completer) | `ApiKeys:RawgApiKey` |
| Email (SMTP) | `EmailSettings:MailAddress`, `EmailSettings:MailPassword`; optional `EmailSettings:SmtpHost`, `EmailSettings:SmtpPort` |
| Production | `Cors:AllowedOrigins`, `Jwt:SigningKey` (≥32 chars recommended) |

Full template: [`backend/appsettings.Example.json`](./backend/appsettings.Example.json)

---

## Tests

```bash
dotnet test
```

Project: [`tests/GameTracker.Api.Tests`](./tests/GameTracker.Api.Tests/)

---

## Documentation

| Doc | Purpose |
|-----|---------|
| [`docs/DEPLOY.md`](./docs/DEPLOY.md) | Hosting, secrets, CORS, `VITE_API_BASE_URL` |
| [`docs/DATABASE.md`](./docs/DATABASE.md) | SQL Server setup |
| [`docs/FEATURES.md`](./docs/FEATURES.md) | Feature overview |
| [`docs/ROAD_MAP.md`](./docs/ROAD_MAP.md) | Roadmap |

---

## Legacy WinForms

The legacy WinForms client is **not** in this repository. The active solution targets the web API + SPA + Electron desktop shell.

---

## Screenshots (legacy WinForms)

| Library | AI recommendations |
| :---: | :---: |
| ![](https://github.com/user-attachments/assets/84f92c88-92f8-449f-9201-c3bc6b2be749) | ![](https://github.com/user-attachments/assets/0b30c609-e406-4a14-8c24-010946b8a462) |

---

## License

[MIT License](./LICENSE) — Copyright (c) 2025 Galip Efe Öncü
