-- UserLibrary'ye oynama süresi sütunu. Desktop istemci arka planda oyun process'ini
-- izleyip her 30 sn'de heartbeat ile backend'e ekleme (delta) gönderir; web'de de
-- "X saat oynandı" rozeti olarak görünür.
-- Çalıştırma: Somee Run Scripts / SSMS / sqlcmd. İdempotent.

USE GameTrackerDB;
GO

IF NOT EXISTS (
    SELECT * FROM sys.columns
     WHERE object_id = OBJECT_ID('dbo.UserLibrary')
       AND name = 'playtime_minutes'
)
BEGIN
    ALTER TABLE UserLibrary
      ADD playtime_minutes INT NOT NULL CONSTRAINT DF_UserLibrary_Playtime DEFAULT 0;
    PRINT 'UserLibrary.playtime_minutes sutunu eklendi.';
END
ELSE
BEGIN
    PRINT 'UserLibrary.playtime_minutes sutunu zaten mevcut; atlandi.';
END
GO
