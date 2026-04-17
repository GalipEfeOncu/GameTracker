-- Legacy 'Played' durumunu web UI'daki 'Completed' ile hizalar.
-- Eski WinForms istemcisi "Played" yazıyordu; web yalnızca "Completed" kullanır.
-- Çalıştırma: Somee Run Scripts / SSMS / sqlcmd. İdempotent — birden fazla çalıştırılabilir.

USE GameTrackerDB;
GO

DECLARE @affected INT;

UPDATE UserLibrary
   SET status = 'Completed'
 WHERE status = 'Played';

SET @affected = @@ROWCOUNT;
PRINT CONCAT('Played -> Completed normalize edildi. Etkilenen satir: ', @affected);
GO
