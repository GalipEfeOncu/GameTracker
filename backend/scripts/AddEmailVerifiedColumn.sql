-- Mevcut veritabanına email_verified sütununu ekler (Somee/LocalDB).
-- users tablosu zaten varsa çalıştırın.

USE GameTrackerDB;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('users') AND name = 'email_verified'
)
BEGIN
    ALTER TABLE users ADD email_verified BIT NOT NULL DEFAULT 1;
    -- Mevcut kullanıcılar doğrulanmış kabul edilir (yeni kayıtlarda 0, verify-email ile 1 yapılır)
END
GO
