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
| SUNUCU_ADI   | Server name / connection string içindeki sunucu | `GameTrackerDB.mssql.somee.com` |
| VERITABANI_ADI | Database / initial catalog       | `GameTrackerDB`                |
| KULLANICI    | Login / User id                 | `Knover_SQLLogin_1`            |
| SIFRE        | Password                        | (Somee’de gördüğünüz şifre)    |

**Örnek (gerçek bilgileri kendi panelinizden alın):**

```
Server=tcp:GameTrackerDB.mssql.somee.com,1433;Database=GameTrackerDB;User Id=Knover_SQLLogin_1;Password=BURAYA_SOMEE_SIFRENIZI_YAZIN;TrustServerCertificate=True;Encrypt=False
```

---

## 3. Backend’te nereye yazılacak?

- **Dosya:** `backend/appsettings.Development.json`
- **Anahtar:** `ConnectionStrings` → `GameTrackerDB`
- Tüm connection string **tek satırda**, **çift tırnak** içinde olmalı. Şifrede `"` varsa `\"` ile kaçın.

Örnek:

```json
{
  "ConnectionStrings": {
    "GameTrackerDB": "Server=tcp:SUNUCU,1433;Database=DB_ADI;User Id=LOGIN;Password=SIFRE;TrustServerCertificate=True;Encrypt=False"
  }
}
```

---

## 4. Sık yapılan hatalar

- **Sunucu adında `tcp:` ve port:** `Server=tcp:xxx.mssql.somee.com,1433` olmalı (virgül ve 1433).
- **Şifrede özel karakter:** Somee şifresinde `;` veya `"` varsa tırnak içinde olduğundan sorun çıkmaz; yine de şifreyi panelden yeniden kopyalayıp yapıştırın, başında/sonunda **boşluk** kalmadığından emin olun.
- **Kullanıcı adı:** Panelde “Login” veya “User” bazen `KullaniciAdi_SQLLogin_1` gibi bir formatta olur; aynen kopyalayın.
- **Database adı:** Bazı panellerde “Initial catalog” veya sadece veritabanı adı yazar; büyük/küçük harf paneldeki ile aynı olsun.

---

## 5. Değişiklikten sonra

`appsettings.Development.json` kaydettikten sonra **backend’i yeniden başlatın** (terminalde `Ctrl+C`, sonra `dotnet run`). Yeni connection string ancak o zaman okunur.

---

**Resmi Somee yardımı:** [MS SQL Database connection string](https://somee.com/DOKA/Help/Article/103)
