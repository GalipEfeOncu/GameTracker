# GameTracker

GameTracker is a full-stack game library and discovery app:

- **Frontend**: React + Vite (`frontend/`)
- **Backend API**: ASP.NET Core (.NET 9) (`backend/`)
- **Data sources**: **IGDB (via Twitch OAuth)** for discovery/search, with **RAWG** used as an optional “detail completer” (platforms/stores/requirements)
- **Optional**: Gemini-powered recommendations

![GameTracker](https://github.com/user-attachments/assets/19471807-5380-4fd6-b81a-91de8cb21183)

## Quickstart (local development)

### Prerequisites

- .NET SDK **9.x**
- Node.js **(recommended: latest LTS)** and npm
- SQL Server (remote or LocalDB) for auth/library endpoints

### 1) Configure secrets (backend)

The backend supports **User Secrets** for local development. From `backend/`:

```bash
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "YOUR_CONNECTION_STRING"
dotnet user-secrets set "Igdb:ClientId" "TWITCH_CLIENT_ID"
dotnet user-secrets set "Igdb:ClientSecret" "TWITCH_CLIENT_SECRET"

# Optional (recommended for richer game details)
dotnet user-secrets set "ApiKeys:RawgApiKey" "YOUR_RAWG_KEY"

# Optional (AI recommendations)
dotnet user-secrets set "ApiKeys:GeminiApiKey" "YOUR_GEMINI_KEY"

# Optional (email workflows)
dotnet user-secrets set "EmailSettings:MailAddress" "example@mail.com"
dotnet user-secrets set "EmailSettings:MailPassword" "APP_PASSWORD"

# Optional (JWT signing key for production-like behavior)
dotnet user-secrets set "Jwt:SigningKey" "A_SECURE_RANDOM_KEY_AT_LEAST_32_CHARS"
```

Notes:
- In **Development**, Swagger is enabled and the API logs which integrations are configured.
- In **Production**, CORS origins are **required** or the API will refuse to start (see docs below).

### 2) Run the backend API

```bash
cd backend
dotnet run
```

Default URL: `http://localhost:5118`  
Swagger (Development): `http://localhost:5118/swagger`

### 3) Run the frontend

```bash
cd frontend
npm install
npm run dev
```

For production builds, set `VITE_API_BASE_URL` (must end with `/api`). See `frontend/.env.example`.

## Configuration reference

All config values can be provided via `appsettings.json`, **User Secrets** (local), or environment variables (production uses `__` as nesting separator).

### Required for core experience (discovery/search)

| Setting | Description |
|---|---|
| `Igdb:ClientId` / `Igdb:ClientSecret` | Twitch app credentials used to obtain IGDB access tokens |

### Recommended (richer game detail)

| Setting | Description |
|---|---|
| `ApiKeys:RawgApiKey` | RAWG API key used as a “detail completer” (platforms/stores/requirements) |

### Required for auth + library persistence

| Setting | Description |
|---|---|
| `ConnectionStrings:GameTrackerDB` | SQL Server connection string |

### Production-only essentials

| Setting | Description |
|---|---|
| `Cors:AllowedOrigins` | Allowed SPA origins. In production this must be set. |
| `Jwt:SigningKey` | HS256 signing key (minimum 32 characters recommended) |

Full example template: `backend/appsettings.Example.json`  
Deployment & secrets guide: `docs/DEPLOY.md`

## Tests

From the repository root:

```bash
dotnet test
```

Test project: `tests/GameTracker.Api.Tests`

## Docs

- `docs/DEPLOY.md`: deployment, secrets, production CORS rules, frontend `VITE_API_BASE_URL`
- `docs/DATABASE.md`: SQL Server setup (Somee notes, LocalDB fallback)
- `docs/FEATURES.md`: feature list
- `docs/ROAD_MAP.md`: roadmap
- `docs/PLAN_IGDB_RAWG_HYBRID.md`: IGDB-as-primary + RAWG-as-complementary implementation plan

## Tech stack

- **Backend**: ASP.NET Core (.NET 9), JWT auth, rate limiting, Swagger (dev), response compression
- **Frontend**: React 19, Vite, React Router, TanStack Query, Tailwind CSS
- **Database**: Microsoft SQL Server
- **Integrations**: IGDB (Twitch OAuth), RAWG, optional Gemini

## Legacy WinForms (historical)

The legacy WinForms app is **not included** in this repository (ignored via `.gitignore`). The current solution (`GameTracker.sln`) focuses on the web API + web frontend.

## Screenshots (legacy WinForms)

| Library | AI recommendations |
| :---: | :---: |
| ![](https://github.com/user-attachments/assets/84f92c88-92f8-449f-9201-c3bc6b2be749) | ![](https://github.com/user-attachments/assets/0b30c609-e406-4a14-8c24-010946b8a462) |

## License

See `LICENSE`.
