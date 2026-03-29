# Somee’den Connection String Alma ve Backend’te Kullanma

## 1. Somee panelinden connection bilgilerini alın

1. **Somee’ye giriş yapın:** https://somee.com  
2. **Control Panel**’e gidin.  
3. Soldaki menüden **MS SQL Databases** bölümüne tıklayın.  
4. Listeden **veritabanınızın adına** (örn. GameTrackerDB) tıklayın.  
5. Açılan **MS SQL Database Dashboard (Summary)** sayfasında şunları bulun:
   - **Connection string** satırı (bazen “Copy to clipboard” linkiyle)
   - veya ayrı ayrı: **Server name**, **Database**, **Login / User**, **Password**

Panelde “Connection string” hazır veriliyorsa **Copy to clipboard** ile kopyalayın.  
Bilgiler ayrı ayrıysa (Server, Database, Login, Password) aşağıdaki formata göre siz birleştirin.

---

## 2. .NET 9 backend için doğru format

Bizim backend **Microsoft.Data.SqlClient** kullanıyor. Somee’nin bazen verdiği eski format (“workstation id”, “data source” vb.) veya **“Network Library”** parametresi bu sürücüde hataya yol açabiliyor. Aşağıdaki format kullanılmalı:

```
Server=tcp:SUNUCU_ADI,1433;Database=VERITABANI_ADI;User Id=KULLANICI;Password=SIFRE;TrustServerCertificate=True;Encrypt=False
```

**Sizin doldurmanız gerekenler:**

| Yer       | Somee panelinde nerede?        | Örnek                          |
|----------|--------------------------------|--------------------------------|
| SUNUCU_ADI   | Server name / connection string içindeki sunucu | `YOUR_SERVER_NAME.mssql.somee.com` |
| VERITABANI_ADI | Database / initial catalog       | `YOUR_DATABASE_NAME`           |
| KULLANICI    | Login / User id                 | `YOUR_SQL_LOGIN`               |
| SIFRE        | Password                        | (Somee’de gördüğünüz şifre)    |

**Tamamen örnek, gerçek bilgi içermeyen format:**

```
Server=tcp:YOUR_SERVER_NAME.mssql.somee.com,1433;Database=YOUR_DATABASE_NAME;User Id=YOUR_SQL_LOGIN;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False
```

---

## 3. Backend’te nereye yazılacak?

**Önerilen:** `backend` klasöründe User Secrets (repoda ve proje klasöründe düz metin olarak kalmaz):

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "Server=tcp:...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;Encrypt=False"
```

Connection string’i panelden **olduğu gibi** tek parça yapıştırmanız yeterlidir (format yukarıdaki gibi olmalı).

---

## 4. Sık yapılan hatalar

- **Sunucu adında `tcp:` ve port:** `Server=tcp:xxx.mssql.somee.com,1433` olmalı (virgül ve 1433).
- **Şifrede özel karakter:** Somee şifresinde `;` veya `"` varsa tırnak içinde olduğundan sorun çıkmaz; yine de şifreyi panelden yeniden kopyalayıp yapıştırın, başında/sonunda **boşluk** kalmadığından emin olun.
- **Kullanıcı adı:** Panelde “Login” veya “User” bazen `KullaniciAdi_SQLLogin_1` gibi bir formatta olur; aynen kopyalayın.
- **Database adı:** Bazı panellerde “Initial catalog” veya sadece veritabanı adı yazar; büyük/küçük harf paneldeki ile aynı olsun.

---

## 5. Değişiklikten sonra

`user-secrets set` sonrası veya yapılandırma değişince **backend’i yeniden başlatın** (`Ctrl+C`, sonra `dotnet run`).

---

**Resmi Somee yardımı:** [MS SQL Database connection string](https://somee.com/DOKA/Help/Article/103)
