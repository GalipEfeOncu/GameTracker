# 🎮 GameTracker

**Manage your game library. Discover new titles. Track your playtime.**

GameTracker is an open-source full-stack application that lets you organize your game collection from a single hub. Build your library on the web or with the Windows desktop client, explore a rich catalog powered by IGDB and RAWG, get personalized AI-driven recommendations, and —with the desktop client— automatically track your playtime in the cloud.

[![CI](https://github.com/GalipEfeOncu/GameTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/GalipEfeOncu/GameTracker/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-7-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![Electron](https://img.shields.io/badge/Electron-34-47848F?logo=electron&logoColor=white)](https://www.electronjs.org/)

<table>
<tr>
<td><strong>🌐 Live Demo</strong></td>
<td><a href="https://game-tracker-virid.vercel.app/">game-tracker-virid.vercel.app</a></td>
</tr>
<tr>
<td><strong>⬇️ Download (Windows)</strong></td>
<td><a href="https://github.com/GalipEfeOncu/GameTracker/releases">GitHub Releases</a> — <code>GameTracker Setup x.y.z.exe</code></td>
</tr>
</table>

---

## ✨ Why GameTracker?

<table>
<tr>
<td width="50">📚</td>
<td><strong>Library Management</strong><br>Organize your games by status: <em>Want to Play · Playing · Completed · Dropped · On Hold</em>. Rate them, add notes.</td>
</tr>
<tr>
<td>🔍</td>
<td><strong>Discover & Search</strong><br>Hybrid IGDB + RAWG data source — cover art, screenshots, platforms, store links, system requirements, and Metacritic scores at your fingertips.</td>
</tr>
<tr>
<td>🤖</td>
<td><strong>AI Recommendations</strong><br>Google Gemini analyzes your library and generates 15 personalized suggestions. Completely on-demand — you trigger it.</td>
</tr>
<tr>
<td>⏱️</td>
<td><strong>Playtime Tracking</strong><br>The Windows desktop client automatically detects running games and syncs playtime to the cloud in the background.</td>
</tr>
<tr>
<td>🖥️</td>
<td><strong>Installed Game Detection</strong><br>Steam, Epic Games Store, and GOG libraries are scanned automatically. Other games can be added manually.</td>
</tr>
<tr>
<td>🌍</td>
<td><strong>Multi-Language UI</strong><br>Interface available in Turkish (default) and English. Switch anytime from Settings.</td>
</tr>
</table>

---

## 🏗️ Platform Support

| Platform | Features |
|----------|----------|
| **Web** | Library management, game discovery, AI recommendations, ratings & status tracking |
| **Windows Desktop** | Everything from web + installed game detection, playtime tracking, system tray, auto-start |

---

## 🔐 Security

- **JWT-based authentication** — Short-lived 15-minute access tokens + rotating refresh tokens (30 / 90 days based on "Remember Me")
- **Email verification** — Verification code sent on registration
- **Password reset** — Secure email-based flow
- **Silent token refresh** — Automatic retry on 401; redirect to login on failure

---

## ⚙️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | React 19, Vite 7, Tailwind CSS 4, TanStack Query, React Router, Framer Motion |
| **Backend** | ASP.NET Core (.NET 9), Dapper, JWT, MSSQL |
| **Desktop** | Electron 34, electron-builder (NSIS) |
| **CI/CD** | GitHub Actions (build + test + lint), Vercel (frontend deploy) |
| **Data Sources** | IGDB (Twitch API) + RAWG (hybrid) |
| **AI** | Google Gemini API |

---

# 🛠️ Developer Guide

Everything below is for contributors who want to run and develop the project locally.

## Project Structure

```
GameTracker/
├── backend/          → .NET 9 Web API (JWT, MSSQL, IGDB/RAWG/Gemini integrations)
├── frontend/         → React 19 SPA (Vite 7, Tailwind CSS)
├── desktop/          → Electron shell (installed game detection, playtime tracker)
├── tests/            → xUnit API tests
├── docs/             → Project documentation
└── .github/workflows → CI pipeline
```

## Prerequisites

- [.NET SDK 9.x](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS) and npm
- SQL Server (LocalDB or remote)

## Quick Start

### 1. Backend Secrets

Configure local secrets using [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):

```bash
cd backend

# Required
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "Igdb:ClientId" "TWITCH_CLIENT_ID"
dotnet user-secrets set "Igdb:ClientSecret" "TWITCH_CLIENT_SECRET"

# Recommended — richer metadata
dotnet user-secrets set "ApiKeys:RawgApiKey" "YOUR_RAWG_KEY"

# Optional — AI recommendations
dotnet user-secrets set "ApiKeys:GeminiApiKey" "YOUR_GEMINI_KEY"

# Optional — email verification / password reset
dotnet user-secrets set "EmailSettings:MailAddress" "example@mail.com"
dotnet user-secrets set "EmailSettings:MailPassword" "APP_PASSWORD"

# JWT signing key (≥32 characters)
dotnet user-secrets set "Jwt:SigningKey" "A_SECURE_RANDOM_KEY_AT_LEAST_32_CHARS"
```

### 2. Run the API

```bash
cd backend
dotnet run
```

Default URL: `http://localhost:5118` · Swagger (dev mode): `/swagger`

### 3. Run the Frontend

```bash
cd frontend
npm install
npm run dev          # http://localhost:5173
```

For production builds, set `VITE_API_BASE_URL` (must end with `/api`).

### 4. Run the Desktop Client

```bash
# Terminal 1 — Frontend dev server
cd frontend && npm run dev

# Terminal 2 — Electron
cd desktop
npm install
npm run dev          # Connects to the Vite dev server
```

Packaged build: `cd desktop && npm run build` → `desktop/dist-desktop/GameTracker Setup x.y.z.exe`

## Configuration Reference

Values can be provided via `appsettings.json`, User Secrets, or environment variables (use `__` for nesting in production).

| Area | Keys |
|------|------|
| Database | `ConnectionStrings:GameTrackerDB` |
| IGDB | `Igdb:ClientId`, `Igdb:ClientSecret` |
| RAWG | `ApiKeys:RawgApiKey` |
| Email (SMTP) | `EmailSettings:MailAddress`, `MailPassword`, `SmtpHost`, `SmtpPort` |
| Production | `Cors:AllowedOrigins`, `Jwt:SigningKey` |

Full template: [`backend/appsettings.Example.json`](./backend/appsettings.Example.json)

## Tests

```bash
dotnet test
```

Test project: [`tests/GameTracker.Api.Tests`](./tests/GameTracker.Api.Tests/)

## Deployment

1. **API** — Deploy the backend; ensure DB is reachable and env vars are set. Add your Vercel origin to `Cors__AllowedOrigins`.
2. **Web** — Set the live API URL in `frontend/.env.production` → `npm run build` → deploy `frontend/dist/`.
3. **Desktop** — Same SPA env as step 2, then `cd desktop && npm install && npm run build`. Output: `GameTracker Setup x.y.z.exe`.
4. **Release** — Create a tag and attach the `.exe` to [GitHub Releases](https://github.com/GalipEfeOncu/GameTracker/releases).

Detailed guide: [`docs/DEPLOY.md`](./docs/DEPLOY.md)

## Documentation

| File | Contents |
|------|----------|
| [`docs/FEATURES.md`](./docs/FEATURES.md) | Completed features list |
| [`docs/ROAD_MAP.md`](./docs/ROAD_MAP.md) | Roadmap and planned work |
| [`docs/DEPLOY.md`](./docs/DEPLOY.md) | Hosting, CORS, environment variables |
| [`docs/DATABASE.md`](./docs/DATABASE.md) | SQL Server setup and migrations |

---

## 📄 License

[MIT License](./LICENSE) — Copyright © 2025 Galip Efe Öncü
