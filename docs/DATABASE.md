# Veritabanı (SQL Server)

Şu an **Somee** üzerinde MSSQL kullanıyorsun; başka ücretsiz veya ücretli **SQL Server** host’una geçmek için yapman gereken tek şey: geçerli connection string’i aynı yapılandırma anahtarına yazmak (`ConnectionStrings:GameTrackerDB` / `ConnectionStrings__GameTrackerDB`). Şema ve migration’lar aynı kalır.

**SqlClient kuralı:** `Network Library=DBMSSOCN` kullanma — `Microsoft.Data.SqlClient` bunu desteklemez. Tercihen `Server=tcp:Sunucu,1433;...` formatı (aşağıda Somee örneği).

---

## Somee — connection string

### 1. Panelden bilgiler

1. [Somee](https://somee.com) → Control Panel → **MS SQL Databases** → veritabanı adına tıkla.  
2. **Connection string** veya ayrı: **Server name**, **Database**, **Login**, **Password**.

### 2. .NET 9 backend için format

Somee’nin verdiği eski string veya **Network Library** parametresi hataya yol açabiliyor. Şunu kullan:

```
Server=tcp:SUNUCU_ADI,1433;Database=VERITABANI_ADI;User Id=KULLANICI;Password=SIFRE;TrustServerCertificate=True;Encrypt=False
```

Örnek iskelet (gerçek bilgi yok):

```
Server=tcp:YOUR_SERVER_NAME.mssql.somee.com,1433;Database=YOUR_DATABASE_NAME;User Id=YOUR_SQL_LOGIN;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False
```

### 3. Nereye yazılır

Lokal: `backend` içinde user-secrets:

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "Server=tcp:...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;Encrypt=False"
```

Üretim: barındırma panelinde `ConnectionStrings__GameTrackerDB`.

Değişiklikten sonra API’yi yeniden başlat.

### 4. Sık hatalar

- Sunucu: `Server=tcp:host.mssql.somee.com,1433` (virgül + 1433).
- Login/database adlarını paneldeki gibi aynen kopyala; şifrede baş/son boşluk olmasın.

Resmi yardım: [Somee — MS SQL connection string](https://somee.com/DOKA/Help/Article/103)

---

## Somee dışı MSSQL (geçiş notu)

Azure SQL ücretsiz katman, SQL Server Express (VPS/Docker), başka hosting: panelden aldığın connection string’i yine SqlClient uyumlu (`tcp` + port) tut. Limitler ve firewall kurallarını yeni sağlayıcıya göre ayarla.

---

## LocalDB (Somee’ye erişemediğinde lokal test)

Somee veya uzak SQL yanıt vermiyorsa (`No such host is known` vb.) localhost’ta LocalDB kullanabilirsin.

**Kurulu mu:** PowerShell’de `SqlLocalDB info` — yoksa [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb).

**Veritabanını oluştur:** `backend/scripts/CreateLocalDb.sql`

```powershell
cd backend\scripts
sqlcmd -S "(localdb)\MSSQLLocalDB" -i CreateLocalDb.sql
```

(SSMS ile aynı instance’a bağlanıp script’i çalıştırmak da olur.)

**User secrets:**

```bash
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "Server=(localdb)\MSSQLLocalDB;Database=GameTrackerDB;Integrated Security=True;TrustServerCertificate=True"
```

Backend’i yeniden başlat. Uzak SQL’e dönmek için aynı anahtara Somee (veya yeni host) string’ini tekrar yazman yeterli.
