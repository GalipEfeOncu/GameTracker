-- Refresh token rotasyonu için kalıcı token deposu.
-- Token yalnızca istemciye gönderilir; DB'de SHA-256 hash olarak saklanır.
-- Rotasyon: her refresh'te yeni token üretilir, eski revoke edilir ve replaced_by_hash ile bağlanır.
-- İhlal tespiti: revoked_at_utc dolu bir token tekrar kullanılırsa kullanıcının tüm aktif refresh'leri iptal edilir.
-- Çalıştırma: Somee Run Scripts / SSMS / sqlcmd. İdempotent.

USE GameTrackerDB;
GO

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

    CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash
        ON RefreshTokens (token_hash);

    CREATE INDEX IX_RefreshTokens_User_Active
        ON RefreshTokens (user_id, revoked_at_utc, expires_at_utc);

    PRINT 'RefreshTokens tablosu olusturuldu.';
END
ELSE
BEGIN
    PRINT 'RefreshTokens tablosu zaten mevcut; atlandi.';
END
GO
