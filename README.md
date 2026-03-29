# GameTracker

Oyun kütüphanesi, RAWG verisi ve (isteğe bağlı) Gemini önerileri.

![GameTracker](https://github.com/user-attachments/assets/19471807-5380-4fd6-b81a-91de8cb21183)

## Web uygulaması (React + .NET API)

| | |
|--|--|
| **Frontend** | `frontend/` — `npm install` → `npm run dev` |
| **Backend** | `backend/` — `dotnet run` (varsayılan `http://localhost:5118`) |
| **Sırlar** | `backend/README.md` — `dotnet user-secrets` (connection string, RAWG, Gemini, e-posta, JWT) |
| **Üretim SPA/API** | `VITE_API_BASE_URL` (`frontend/.env.example`), backend `Cors:AllowedOrigins` — `docs/FREE_STACK.md` |
| **Test** | Kökten `dotnet test` — `tests/GameTracker.Api.Tests` (`/api/health` smoke) |
| **Yol haritası** | `docs/PROJECT_ROADMAP_TR.md` (legacy karşılaştırma, riskler, faz planı) |

## Masaüstü (WinForms, legacy — yalnızca lokal)

Klasik **DevExpress** projesi repoda yok; `.gitignore` ile `GameTracker/` hariç tutulur. İsterseniz eski kaynağı kendi makinenizde aynı yola koyup Visual Studio ile açabilirsiniz. Çözüm dosyası `GameTracker.sln` yalnızca **web API** (`backend/GameTracker.Api.csproj`) içerir.

## Teknolojiler (web)

- .NET 9 Web API, Microsoft SQL Server, RAWG API, Gemini (isteğe bağlı), React 19, Vite

## Ekran görüntüleri (WinForms sürümü)

| Kütüphane | AI öneri |
| :---: | :---: |
| ![](https://github.com/user-attachments/assets/84f92c88-92f8-449f-9201-c3bc6b2be749) | ![](https://github.com/user-attachments/assets/0b30c609-e406-4a14-8c24-010946b8a462) |

## İndir (WinForms)

[Releases — v1.0](https://github.com/GalipEfeOncu/GameTracker/releases/tag/v1.0)

## Geliştirici

**Galip Efe Öncü** — eğitim ve hobi amaçlı proje.
