-- Kalıcı geçici kod deposu (e-posta doğrulama, şifre sıfırlama, hesap silme).
-- API restart'ında aktif kodların kaybolmaması için bellek yerine DB tablosu.
-- Kod düz değil SHA-256 hash olarak tutulur.
-- Çalıştırma: mevcut GameTrackerDB üzerinde (Somee Run Scripts / SSMS / sqlcmd).
-- İdempotent: birden fazla çalıştırılabilir.

USE GameTrackerDB;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TempVerificationCodes')
BEGIN
    CREATE TABLE TempVerificationCodes (
        id              BIGINT IDENTITY(1,1) PRIMARY KEY,
        purpose         NVARCHAR(32)  NOT NULL,   -- 'EmailVerification' | 'PasswordReset' | 'DeleteAccount'
        subject_key     NVARCHAR(320) NOT NULL,   -- normalize edilmiş e-posta veya user_id (string)
        code_hash       NVARCHAR(128) NOT NULL,   -- SHA-256(code) hex
        expires_at_utc  DATETIME2     NOT NULL,
        created_at_utc  DATETIME2     NOT NULL CONSTRAINT DF_TempVerificationCodes_Created DEFAULT SYSUTCDATETIME(),
        consumed_at_utc DATETIME2     NULL
    );

    CREATE INDEX IX_TempVerificationCodes_Lookup
        ON TempVerificationCodes (purpose, subject_key, consumed_at_utc, expires_at_utc);

    PRINT 'TempVerificationCodes tablosu olusturuldu.';
END
ELSE
BEGIN
    PRINT 'TempVerificationCodes tablosu zaten mevcut; atlandi.';
END
GO
