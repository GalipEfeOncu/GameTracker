# GameTracker API (Backend)

## Yapılandırma (İlk Kurulum)

`appsettings.json` içinde hassas bilgiler boş bırakıldı. Lokal çalıştırmak için:

1. `appsettings.Example.json` dosyasını **kopyalayıp** `appsettings.Development.json` olarak kaydedin.
2. `appsettings.Development.json` içindeki placeholder değerleri kendi connection string ve API anahtarlarınızla doldurun.
3. Bu dosya `.gitignore`'da olduğu için commit edilmez.

Detaylar için kök dizindeki `docs/SECURITY.md` ve `docs/FREE_STACK.md` dosyalarına bakın.

Uzak SQL Server’a bağlanamıyorsanız (örn. "No such host is known") yerel test için LocalDB kullanın: `docs/LOCAL_DATABASE.md`.

## Çalıştırma

```bash
dotnet run
```

API varsayılan olarak `http://localhost:5118` adresinde çalışır. Swagger: `http://localhost:5118/swagger`.
