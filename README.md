# GameTracker

Oyun kütüphanesi, RAWG verisi ve (isteğe bağlı) Gemini önerileri.

![GameTracker](https://github.com/user-attachments/assets/19471807-5380-4fd6-b81a-91de8cb21183)

## Web (React + .NET)

| | |
|--|--|
| **Ön yüz** | `frontend/` → `npm install` → `npm run dev` |
| **API** | `backend/` → `dotnet run` (`http://localhost:5118`) |
| **Deploy / sırlar / CORS** | [docs/DEPLOY.md](docs/DEPLOY.md) |
| **Veritabanı (Somee, geçiş, LocalDB)** | [docs/DATABASE.md](docs/DATABASE.md) |
| **Test** | Kökten `dotnet test` — `tests/GameTracker.Api.Tests` |
| **Özellikler / yol haritası** | [docs/FEATURES.md](docs/FEATURES.md), [docs/ROAD_MAP.md](docs/ROAD_MAP.md) |

## Legacy WinForms

Repoda yok; `.gitignore` ile `GameTracker/` dışarıda. Çözüm `GameTracker.sln` yalnızca web API projesini içerir.

## Teknolojiler (web)

.NET 9 Web API, Microsoft SQL Server, RAWG API, Gemini (isteğe bağlı), React 19, Vite

## Ekran görüntüleri (WinForms sürümü)

| Kütüphane | AI öneri |
| :---: | :---: |
| ![](https://github.com/user-attachments/assets/84f92c88-92f8-449f-9201-c3bc6b2be749) | ![](https://github.com/user-attachments/assets/0b30c609-e406-4a14-8c24-010946b8a462) |

## İndir (WinForms)

[Releases — v1.0](https://github.com/GalipEfeOncu/GameTracker/releases/tag/v1.0)

## Geliştirici

**Galip Efe Öncü** — eğitim ve hobi amaçlı proje.
