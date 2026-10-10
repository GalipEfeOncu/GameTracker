# Migration: Somee MSSQL → Azure SQL (veya PostgreSQL fallback)

> **Hedef:** Veritabanını Somee'den Azure SQL Free Tier'a taşımak.
> Azure sorun çıkarırsa **PostgreSQL** alternatifi bu belgede §B olarak yer alır.

---

## BÖLÜM A — Azure SQL Free Tier (MSSQL kalır, sıfır kod değişikliği)

### A1. Azure SQL Database Oluşturma

Azure Portal → SQL databases → **Create**

| Alan | Değer |
|------|-------|
| **Subscription** | Free Trial veya Pay-As-You-Go |
| **Resource Group** | `gametracker-rg` (yeni oluştur) |
| **Database name** | `GameTrackerDB` |
| **Server** | Yeni oluştur → `gametracker-sql` |
| **Location** | Projeye en yakın bölge (ör. West Europe) |
| **Want to use SQL elastic pool?** | No |
| **Compute + storage** | **Free** seçeneğini seç (32 GB, 100K vCore-sn/ay) |
| **Backup redundancy** | Locally-redundant (ücretsiz) |

> [!IMPORTANT]
> "Compute + storage" adımında **"Looking for free offer?"** bağlantısını tıklayın veya **Free** tier'ı seçin. Yoksa ücretli plana düşersiniz.

**Server oluştururken:**
- **Server admin login:** `gametracker_admin` (veya tercih ettiğiniz)
- **Password:** Güçlü şifre (kaydedin!)
- **Allow Azure services:** ✅ Evet

### A2. Firewall Kuralları

Azure Portal → SQL Server → Networking:

- [x] **Allow Azure services and resources to access this server** — Evet
- **Client IP:** Kendi IP'nizi ekleyin (lokal geliştirme için)
- **Oracle VM IP'si:** API sunucusunun public IP'sini ekleyin

### A3. Connection String

Azure Portal → SQL Database → Connection strings → **ADO.NET** sekmesi:

```
Server=tcp:gametracker-sql.database.windows.net,1433;Initial Catalog=GameTrackerDB;Persist Security Info=False;User ID=gametracker_admin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### A4. Tabloları Oluştur

Azure Portal → SQL Database → **Query editor** (veya SSMS / Azure Data Studio):

> [!WARNING]
> Azure SQL'de `USE database; GO` çalışmaz — zaten doğru DB'ye bağlısınızdır. Aşağıdaki script'leri `USE` ve `GO` satırları **olmadan** çalıştırın.

Sırasıyla çalıştırın:

**1. Ana tablolar** (aşağıdaki Azure uyumlu versiyonu kullanın):

```sql
-- 1. users tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
BEGIN
    CREATE TABLE users (
        user_id        INT IDENTITY(1,1) PRIMARY KEY,
        username       NVARCHAR(100) NOT NULL,
        email          NVARCHAR(255) NOT NULL,
        password       NVARCHAR(255) NOT NULL,
        email_verified BIT NOT NULL DEFAULT 0,
        created_at     DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_users_username UNIQUE (username),
        CONSTRAINT UQ_users_email UNIQUE (email)
    );
END

-- 2. UserLibrary tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserLibrary')
BEGIN
    CREATE TABLE UserLibrary (
        user_id   INT NOT NULL,
        game_id   INT NOT NULL,
        game_name NVARCHAR(500) NOT NULL,
        image_url NVARCHAR(1000) NULL,
        status    NVARCHAR(50) NOT NULL DEFAULT 'PlanToPlay',
        playtime_minutes INT NOT NULL DEFAULT 0,
        added_at  DATETIME2 NOT NULL DEFAULT GETDATE(),
        PRIMARY KEY (user_id, game_id),
        CONSTRAINT FK_UserLibrary_users FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
    );
END

-- 3. TempVerificationCodes tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TempVerificationCodes')
BEGIN
    CREATE TABLE TempVerificationCodes (
        id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        purpose         NVARCHAR(32)  NOT NULL,
        subject_key     NVARCHAR(320) NOT NULL,
        code_hash       NVARCHAR(128) NOT NULL,
        expires_at_utc  DATETIME2     NOT NULL,
        created_at_utc  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        consumed_at_utc DATETIME2     NULL
    );

    CREATE INDEX IX_TempVerificationCodes_Lookup
        ON TempVerificationCodes (purpose, subject_key, consumed_at_utc, expires_at_utc);
END

-- 4. RefreshTokens tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        id                BIGINT IDENTITY(1,1) PRIMARY KEY,
        user_id           INT           NOT NULL,
        token_hash        NVARCHAR(128) NOT NULL,
        issued_at_utc     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
        expires_at_utc    DATETIME2     NOT NULL,
        last_used_at_utc  DATETIME2     NULL,
        revoked_at_utc    DATETIME2     NULL,
        replaced_by_hash  NVARCHAR(128) NULL,
        CONSTRAINT FK_RefreshTokens_users FOREIGN KEY (user_id)
            REFERENCES users(user_id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash ON RefreshTokens (token_hash);
    CREATE INDEX IX_RefreshTokens_User_Active ON RefreshTokens (user_id, revoked_at_utc, expires_at_utc);
END
```

> [!NOTE]
> Azure SQL'de `CONSTRAINT DF_*` adlı default constraint'ler bazen "name already exists" hatası verebilir. Yukarıdaki script'te constraint adları çıkarılmıştır.

### A5. Veri Taşıma (opsiyonel)

Somee'deki mevcut verileri taşımak istiyorsanız:

**Seçenek 1 — Azure Data Studio / SSMS:**
1. Somee DB'ye bağlan → sağ tık → "Generate Scripts" → INSERT statements olarak dışa aktar
2. Azure SQL'e bağlan → script'i çalıştır

**Seçenek 2 — bacpac:**
1. Somee'den `.bacpac` export et (Somee panelinden veya SSMS)
2. Azure Portal → SQL Database → Import

**Seçenek 3 — Sıfırdan başla:**
Hobi projesi olduğundan, temiz DB ile yeniden kayıt açabilirsiniz.

### A6. Connection String Güncelle

Tek değişiklik bu — **kod değişikliği yok.**

**Lokal geliştirme:**
```bash
cd backend
dotnet user-secrets set "ConnectionStrings:GameTrackerDB" "Server=tcp:gametracker-sql.database.windows.net,1433;Initial Catalog=GameTrackerDB;User ID=gametracker_admin;Password=SIFRE;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Oracle VM üretim (env dosyası):**
```bash
ConnectionStrings__GameTrackerDB=Server=tcp:gametracker-sql.database.windows.net,1433;Initial Catalog=GameTrackerDB;User ID=gametracker_admin;Password=SIFRE;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### A7. Doğrulama

```bash
# Backend'i başlat ve test et
dotnet run

# Login / register çalışıyor mu?
# Kütüphane CRUD çalışıyor mu?
# Swagger'dan endpoint'leri test et
```

### A8. Somee Temizliği

Azure SQL doğrulandıktan sonra:
1. Somee panelinden DB'yi sil veya hesabı kapat
2. Eski connection string'leri temizle

---

## BÖLÜM B — PostgreSQL Fallback (Azure sorun çıkarırsa)

Azure SQL çalışmadıysa, Oracle Cloud VM üzerine PostgreSQL kurulur. Bu bölüm **kod değişiklikleri** gerektirir.

### B1. PostgreSQL Kurulumu (Oracle VM)

```bash
# VM'e SSH ile bağlan
ssh -i ~/.ssh/oracle_key ubuntu@<VM_PUBLIC_IP>

# PostgreSQL 16 kur
sudo apt install -y postgresql-16 postgresql-client-16

# Servisi başlat ve otomatik başlatmayı etkinleştir
sudo systemctl enable postgresql
sudo systemctl start postgresql

# Veritabanı ve kullanıcı oluştur
sudo -u postgres psql << 'SQL'
CREATE USER gametracker WITH PASSWORD 'GÜÇLÜ_ŞİFRE_BURAYA';
CREATE DATABASE gametrackerdb OWNER gametracker;
GRANT ALL PRIVILEGES ON DATABASE gametrackerdb TO gametracker;
SQL
```

### B2. PostgreSQL Tablo Script'leri

`backend/scripts/CreateTables_PostgreSQL.sql` adında yeni dosya oluştur:

```sql
-- PostgreSQL şeması — GameTracker
-- Çalıştırma: psql -U gametracker -d gametrackerdb -f CreateTables_PostgreSQL.sql

-- 1. users
CREATE TABLE IF NOT EXISTS users (
    user_id        SERIAL PRIMARY KEY,
    username       VARCHAR(100) NOT NULL,
    email          VARCHAR(255) NOT NULL,
    password       VARCHAR(255) NOT NULL,
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email)
);

-- 2. UserLibrary
CREATE TABLE IF NOT EXISTS "UserLibrary" (
    user_id          INT NOT NULL,
    game_id          INT NOT NULL,
    game_name        VARCHAR(500) NOT NULL,
    image_url        VARCHAR(1000),
    status           VARCHAR(50) NOT NULL DEFAULT 'PlanToPlay',
    playtime_minutes INT NOT NULL DEFAULT 0,
    added_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    PRIMARY KEY (user_id, game_id),
    CONSTRAINT fk_userlibrary_users FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- 3. TempVerificationCodes
CREATE TABLE IF NOT EXISTS "TempVerificationCodes" (
    id              BIGSERIAL PRIMARY KEY,
    purpose         VARCHAR(32) NOT NULL,
    subject_key     VARCHAR(320) NOT NULL,
    code_hash       VARCHAR(128) NOT NULL,
    expires_at_utc  TIMESTAMPTZ NOT NULL,
    created_at_utc  TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    consumed_at_utc TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_tempverificationcodes_lookup
    ON "TempVerificationCodes" (purpose, subject_key, consumed_at_utc, expires_at_utc);

-- 4. RefreshTokens
CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    id                BIGSERIAL PRIMARY KEY,
    user_id           INT NOT NULL,
    token_hash        VARCHAR(128) NOT NULL,
    issued_at_utc     TIMESTAMPTZ NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    expires_at_utc    TIMESTAMPTZ NOT NULL,
    last_used_at_utc  TIMESTAMPTZ,
    revoked_at_utc    TIMESTAMPTZ,
    replaced_by_hash  VARCHAR(128),
    CONSTRAINT fk_refreshtokens_users FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_refreshtokens_tokenhash ON "RefreshTokens" (token_hash);
CREATE INDEX IF NOT EXISTS ix_refreshtokens_user_active ON "RefreshTokens" (user_id, revoked_at_utc, expires_at_utc);
```

### B3. Kod Değişiklikleri — Özet

PostgreSQL geçişi için değişmesi gereken dosyalar:

#### B3.1. NuGet paket değişikliği

`backend/GameTracker.Api.csproj`:

```diff
-    <PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
+    <PackageReference Include="Npgsql" Version="9.0.3" />
```

#### B3.2. `Data/DatabaseHelper.cs` — Tam yeniden yazım

`SqlConnection` → `NpgsqlConnection`, `SqlCommand` → `NpgsqlCommand`, `SqlParameter` → `NpgsqlParameter`, `SqlDataAdapter` → `NpgsqlDataAdapter`.

**Değişecek yerler:**
| Mevcut (MSSQL) | Yeni (PostgreSQL) |
|-----------------|-------------------|
| `using Microsoft.Data.SqlClient` | `using Npgsql` |
| `SqlConnection` | `NpgsqlConnection` |
| `SqlCommand` | `NpgsqlCommand` |
| `SqlParameter` | `NpgsqlParameter` |
| `SqlDataAdapter` | `NpgsqlDataAdapter` |

#### B3.3. `Managers/UserManager.cs` — SQL syntax değişiklikleri

| Satır | Mevcut (MSSQL) | Yeni (PostgreSQL) |
|-------|----------------|-------------------|
| 19 | `SELECT TOP 1 1 FROM users` | `SELECT 1 FROM users WHERE email = @email LIMIT 1` |
| 42 | `GETDATE()` | `NOW()` |
| 70 | `LOWER(LTRIM(RTRIM(email)))` | `LOWER(TRIM(email))` |
| 122 | `LOWER(LTRIM(RTRIM(email)))` | `LOWER(TRIM(email))` |
| 189 | `LOWER(LTRIM(RTRIM(email)))` | `LOWER(TRIM(email))` |

Ayrıca tüm `SqlParameter` → `NpgsqlParameter`, `SqlException` → `PostgresException` (hata kodları da değişir):

| MSSQL | PostgreSQL |
|-------|-----------|
| `ex.Number == 2627 \|\| ex.Number == 2601` (unique violation) | `ex.SqlState == "23505"` |

#### B3.4. `Managers/LibraryManager.cs` — SQL syntax değişiklikleri

| Satır | Mevcut (MSSQL) | Yeni (PostgreSQL) |
|-------|----------------|-------------------|
| 145 | `OUTPUT INSERTED.playtime_minutes` | `RETURNING playtime_minutes` |

`SqlParameter` → `NpgsqlParameter`, `SqlException` → `PostgresException`.

#### B3.5. `Services/RefreshTokenService.cs` — SQL syntax değişiklikleri

| Satır | Mevcut (MSSQL) | Yeni (PostgreSQL) |
|-------|----------------|-------------------|
| 78 | `SELECT TOP 1 ... FROM RefreshTokens` | `SELECT ... FROM "RefreshTokens" ... LIMIT 1` |
| 113 | `SYSUTCDATETIME()` | `NOW() AT TIME ZONE 'UTC'` |
| 119 | `IF @@ROWCOUNT = 1 BEGIN ... END` | CTE veya `WITH` yaklaşımı (aşağıda) |
| 151 | `SYSUTCDATETIME()` | `NOW() AT TIME ZONE 'UTC'` |
| 165 | `SYSUTCDATETIME()` | `NOW() AT TIME ZONE 'UTC'` |

**`@@ROWCOUNT` pattern'i PostgreSQL karşılığı:**

Mevcut MSSQL (rotate):
```sql
UPDATE RefreshTokens
   SET revoked_at_utc  = SYSUTCDATETIME(),
       replaced_by_hash = @newHash,
       last_used_at_utc = SYSUTCDATETIME()
 WHERE token_hash = @oldHash
   AND revoked_at_utc IS NULL;

IF @@ROWCOUNT = 1
BEGIN
    INSERT INTO RefreshTokens (user_id, token_hash, expires_at_utc)
    VALUES (@uid, @newHash, @exp);
END
```

PostgreSQL karşılığı — **iki ayrı çağrı veya CTE:**
```sql
WITH revoked AS (
    UPDATE "RefreshTokens"
       SET revoked_at_utc   = NOW() AT TIME ZONE 'UTC',
           replaced_by_hash = @newHash,
           last_used_at_utc = NOW() AT TIME ZONE 'UTC'
     WHERE token_hash = @oldHash
       AND revoked_at_utc IS NULL
    RETURNING user_id
)
INSERT INTO "RefreshTokens" (user_id, token_hash, expires_at_utc)
SELECT @uid, @newHash, @exp
 WHERE EXISTS (SELECT 1 FROM revoked);
```

#### B3.6. `Services/VerificationCodeStore.cs` — SQL syntax değişiklikleri

| Satır | Mevcut (MSSQL) | Yeni (PostgreSQL) |
|-------|----------------|-------------------|
| 62 | `DATEADD(day, -1, SYSUTCDATETIME())` | `NOW() AT TIME ZONE 'UTC' - INTERVAL '1 day'` |
| 65 | `SYSUTCDATETIME()` | `NOW() AT TIME ZONE 'UTC'` |
| 94 | `OUTPUT INSERTED.id` | `RETURNING id` |
| 100 | `SYSUTCDATETIME()` | `NOW() AT TIME ZONE 'UTC'` |
| 121 | `SYSUTCDATETIME()` | `NOW() AT TIME ZONE 'UTC'` |

#### B3.7. `SqlDbType` → `NpgsqlDbType` (parametre tipleri)

| Mevcut | Yeni |
|--------|------|
| `SqlDbType.Int` | `NpgsqlDbType.Integer` |
| `SqlDbType.NVarChar` | `NpgsqlDbType.Varchar` |
| `SqlDbType.DateTime2` | `NpgsqlDbType.TimestampTz` |
| `SqlDbType.BigInt` | `NpgsqlDbType.Bigint` |

> [!TIP]
> Npgsql'de parametre tipi belirtmek zorunlu değildir — Npgsql CLR tipinden otomatik çıkarım yapar. Tüm `new NpgsqlParameter("@name", NpgsqlDbType.X) { Value = val }` yerine basitçe `new NpgsqlParameter("@name", val)` kullanılabilir.

### B4. PostgreSQL Connection String

```
Host=localhost;Port=5432;Database=gametrackerdb;Username=gametracker;Password=GÜÇLÜ_ŞİFRE
```

**Oracle VM env:**
```bash
ConnectionStrings__GameTrackerDB=Host=localhost;Port=5432;Database=gametrackerdb;Username=gametracker;Password=GÜÇLÜ_ŞİFRE
```

### B5. Tablo adı büyük/küçük harf uyarısı

PostgreSQL tablo adlarını küçük harfe normalize eder. Mevcut kodda `UserLibrary`, `RefreshTokens`, `TempVerificationCodes` büyük harfle başlıyor. İki seçenek:

1. **Çift tırnak kullan** (mevcut adları koru): SQL sorgularında `"UserLibrary"`, `"RefreshTokens"` şeklinde yazılır
2. **Tablo adlarını küçült** (önerilen): Hem schema hem kodda `userlibrary`, `refreshtokens`, `tempverificationcodes` kullanılır → daha PostgreSQL idiomatik

> [!IMPORTANT]
> Seçenek 2 tercih edilirse, tüm SQL sorgularındaki tablo adları da küçük harfe çevrilmelidir.

---

## Tam değişiklik haritası (B — PostgreSQL)

| Dosya | Değişiklik boyutu | Açıklama |
|-------|-------------------|----------|
| `GameTracker.Api.csproj` | 1 satır | `SqlClient` → `Npgsql` |
| `Data/DatabaseHelper.cs` | ~15 satır | Tüm SQL sınıfları Npgsql'e |
| `Managers/UserManager.cs` | ~20 satır | `TOP 1` → `LIMIT 1`, `GETDATE()` → `NOW()`, exception tipi |
| `Managers/LibraryManager.cs` | ~10 satır | `OUTPUT INSERTED` → `RETURNING`, exception tipi |
| `Services/RefreshTokenService.cs` | ~25 satır | `SYSUTCDATETIME()` → `NOW() AT TIME ZONE 'UTC'`, `@@ROWCOUNT` → CTE |
| `Services/VerificationCodeStore.cs` | ~15 satır | `DATEADD` → `INTERVAL`, `OUTPUT` → `RETURNING` |
| `scripts/CreateTables_PostgreSQL.sql` | Yeni dosya | PostgreSQL schema |
| **Toplam** | **~90 satır** | |

---

## Doğrulama kontrol listesi (her iki seçenek için)

- [ ] `dotnet build` → 0 hata
- [ ] `dotnet test` → tüm testler geçer
- [ ] Register → e-posta doğrulama → login çalışıyor
- [ ] Kütüphaneye oyun ekle / durum güncelle / sil
- [ ] Playtime heartbeat (desktop) çalışıyor
- [ ] Refresh token rotasyonu çalışıyor
- [ ] Şifre değiştir → eski oturumlar revoke edildi
- [ ] Frontend lint + build → 0 hata

---

*Referans: [AGENT_HANDOFF.md](./AGENT_HANDOFF.md) · [DEPLOY.md](./DEPLOY.md) · [DATABASE.md](./DATABASE.md)*
