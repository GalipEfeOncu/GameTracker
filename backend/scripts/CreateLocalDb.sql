-- GameTracker yerel veritabanı (LocalDB / SQL Server Express)
-- Çalıştırmak: sqlcmd -S "(localdb)\MSSQLLocalDB" -i CreateLocalDb.sql
-- veya SSMS ile (localdb)\MSSQLLocalDB'e bağlanıp bu dosyayı açıp çalıştırın.

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GameTrackerDB')
BEGIN
    CREATE DATABASE GameTrackerDB;
END
GO

USE GameTrackerDB;
GO

-- Kullanıcılar tablosu
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
GO

-- Kullanıcı kütüphanesi (oyunlar)
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
GO

-- Geçici doğrulama kodları (e-posta doğrulama / şifre sıfırlama / hesap silme)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TempVerificationCodes')
BEGIN
    CREATE TABLE TempVerificationCodes (
        id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        purpose         NVARCHAR(32)  NOT NULL,
        subject_key     NVARCHAR(320) NOT NULL,
        code_hash       NVARCHAR(128) NOT NULL,
        expires_at_utc  DATETIME2     NOT NULL,
        created_at_utc  DATETIME2     NOT NULL CONSTRAINT DF_TempVerificationCodes_Created DEFAULT SYSUTCDATETIME(),
        consumed_at_utc DATETIME2     NULL
    );

    CREATE INDEX IX_TempVerificationCodes_Lookup
        ON TempVerificationCodes (purpose, subject_key, consumed_at_utc, expires_at_utc);
END
GO

-- Refresh token rotasyonu (kısa ömürlü access JWT + uzun ömürlü rotasyonlu refresh)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        id                BIGINT IDENTITY(1,1) PRIMARY KEY,
        user_id           INT           NOT NULL,
        token_hash        NVARCHAR(128) NOT NULL,
        issued_at_utc     DATETIME2     NOT NULL CONSTRAINT DF_RefreshTokens_Issued DEFAULT SYSUTCDATETIME(),
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
GO

PRINT 'GameTrackerDB hazir.';
GO
