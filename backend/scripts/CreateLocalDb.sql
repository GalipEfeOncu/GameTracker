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
        added_at  DATETIME2 NOT NULL DEFAULT GETDATE(),
        PRIMARY KEY (user_id, game_id),
        CONSTRAINT FK_UserLibrary_users FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
    );
END
GO

PRINT 'GameTrackerDB hazir.';
GO
