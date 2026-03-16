# Yerel Veritabanı (LocalDB) ile Çalıştırma

Somee veya uzak SQL Server’a erişilemediğinde (“No such host is known”) localhost’ta test için **LocalDB** kullanabilirsiniz.

## 1. LocalDB’nin yüklü olduğunu kontrol edin

PowerShell’de:

```powershell
SqlLocalDB info
```

Çıktıda `MSSQLLocalDB` (veya başka bir instance) görünüyorsa kurulu demektir. Yoksa [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) indirip kurun.

## 2. Veritabanını oluşturun

`backend/scripts` klasöründeki script’i çalıştırın.

**Seçenek A – sqlcmd (PowerShell/CMD):**

```powershell
cd backend\scripts
sqlcmd -S "(localdb)\MSSQLLocalDB" -i CreateLocalDb.sql
```

**Seçenek B – SSMS:**  
SQL Server Management Studio ile `(localdb)\MSSQLLocalDB` sunucusuna bağlanın, `CreateLocalDb.sql` dosyasını açıp çalıştırın.

## 3. Backend connection string’i

`appsettings.Development.json` içinde `ConnectionStrings:GameTrackerDB` değerini LocalDB için şöyle yapın:

```json
"GameTrackerDB": "Server=(localdb)\\MSSQLLocalDB;Database=GameTrackerDB;Integrated Security=True;TrustServerCertificate=True"
```

(JSON’da tek `\` için `\\` yazılır.)

## 4. Backend’i yeniden başlatın

Backend’i kapatıp (`Ctrl+C`) tekrar `dotnet run` ile başlatın. Kayıt/giriş artık yerel veritabanına gidecektir.

---

**Not:** Somee’ye tekrar bağlanmak istediğinizde `appsettings.Development.json` içindeki connection string’i Somee bilgilerinizle değiştirip backend’i yeniden başlatmanız yeterli.
