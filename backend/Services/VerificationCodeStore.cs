using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace GameTracker.Services
{
    /// <summary>
    /// Geçici doğrulama kodu türleri. DB'de <c>purpose</c> sütununa string olarak yazılır.
    /// Yeni bir tür eklerken DB zaten serbest metin tuttuğu için migration gerekmez.
    /// </summary>
    public enum VerificationCodePurpose
    {
        EmailVerification,
        PasswordReset,
        DeleteAccount,
    }

    public interface IVerificationCodeStore
    {
        /// <summary>
        /// Verilen (purpose, subject) için yeni bir kod kaydeder. Aynı çift için önceki
        /// tüketilmemiş kodlar otomatik geçersizleştirilir (yeniden kod iste akışı güvenli).
        /// </summary>
        void Set(VerificationCodePurpose purpose, string subjectKey, string code, TimeSpan? ttl = null);

        /// <summary>
        /// Kodu doğrular ve başarılıysa tek kullanımlık şekilde tüketir (consumed_at_utc işaretlenir).
        /// </summary>
        bool TryConsume(VerificationCodePurpose purpose, string subjectKey, string code);

        /// <summary>
        /// Kayıt iptali gibi durumlarda bekleyen kodları geçersiz kılar (yeniden kayıt için temiz slate).
        /// </summary>
        void RevokePending(VerificationCodePurpose purpose, string subjectKey);
    }

    /// <summary>
    /// SQL tabanlı kalıcı kod deposu. API restart'ında aktif kodlar kaybolmaz.
    /// Kodlar DB'de düz değil SHA-256 hash olarak saklanır; düz kod yalnızca
    /// kullanıcıya e-posta ile gönderilir.
    /// </summary>
    public sealed class SqlVerificationCodeStore : IVerificationCodeStore
    {
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

        public void Set(VerificationCodePurpose purpose, string subjectKey, string code, TimeSpan? ttl = null)
        {
            if (string.IsNullOrWhiteSpace(subjectKey)) throw new ArgumentException("subjectKey required", nameof(subjectKey));
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("code required", nameof(code));

            var purposeStr = purpose.ToString();
            var subject = NormalizeSubject(subjectKey);
            var hash = HashCode(code);
            var expiresAt = DateTime.UtcNow.Add(ttl ?? DefaultTtl);

            // Tek çağrı: (1) bir günden eski çöpü temizle, (2) aynı hedef için aktif kodları geçersizleştir,
            // (3) yeni kodu ekle. Aynı DB bağlantısında sıralı çalışır.
            const string sql = @"
DELETE FROM TempVerificationCodes
 WHERE expires_at_utc < DATEADD(day, -1, SYSUTCDATETIME());

UPDATE TempVerificationCodes
   SET consumed_at_utc = SYSUTCDATETIME()
 WHERE purpose = @purpose
   AND subject_key = @subject
   AND consumed_at_utc IS NULL;

INSERT INTO TempVerificationCodes (purpose, subject_key, code_hash, expires_at_utc)
VALUES (@purpose, @subject, @hash, @expires);";

            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@purpose", SqlDbType.NVarChar, 32) { Value = purposeStr },
                new SqlParameter("@subject", SqlDbType.NVarChar, 320) { Value = subject },
                new SqlParameter("@hash",    SqlDbType.NVarChar, 128) { Value = hash },
                new SqlParameter("@expires", SqlDbType.DateTime2)     { Value = expiresAt },
            });
        }

        public bool TryConsume(VerificationCodePurpose purpose, string subjectKey, string code)
        {
            if (string.IsNullOrWhiteSpace(subjectKey) || string.IsNullOrWhiteSpace(code)) return false;

            var purposeStr = purpose.ToString();
            var subject = NormalizeSubject(subjectKey);
            var hash = HashCode(code);

            // Atomik doğrulama + tüketim: aynı kodu iki kez kullanamayız; yarışmalarda
            // tek bir satır güncellenir. OUTPUT ile güncellenen satırın id'si dönerse başarılı.
            const string sql = @"
UPDATE TempVerificationCodes
   SET consumed_at_utc = SYSUTCDATETIME()
 OUTPUT INSERTED.id
 WHERE purpose = @purpose
   AND subject_key = @subject
   AND code_hash = @hash
   AND consumed_at_utc IS NULL
   AND expires_at_utc > SYSUTCDATETIME();";

            var result = DatabaseHelper.ExecuteScalar(sql, new[]
            {
                new SqlParameter("@purpose", SqlDbType.NVarChar, 32) { Value = purposeStr },
                new SqlParameter("@subject", SqlDbType.NVarChar, 320) { Value = subject },
                new SqlParameter("@hash",    SqlDbType.NVarChar, 128) { Value = hash },
            });

            return result != null && result != DBNull.Value;
        }

        public void RevokePending(VerificationCodePurpose purpose, string subjectKey)
        {
            if (string.IsNullOrWhiteSpace(subjectKey)) return;

            var purposeStr = purpose.ToString();
            var subject = NormalizeSubject(subjectKey);

            const string sql = @"
UPDATE TempVerificationCodes
   SET consumed_at_utc = SYSUTCDATETIME()
 WHERE purpose = @purpose
   AND subject_key = @subject
   AND consumed_at_utc IS NULL;";

            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@purpose", SqlDbType.NVarChar, 32) { Value = purposeStr },
                new SqlParameter("@subject", SqlDbType.NVarChar, 320) { Value = subject },
            });
        }

        /// <summary>
        /// Subject'i eşleşme için normalize eder: trim + lower-invariant.
        /// E-postalar için tam davranış; integer user_id'ler için nötr (sadece trim).
        /// </summary>
        public static string NormalizeSubject(string subject) => subject.Trim().ToLowerInvariant();

        private static string HashCode(string code)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code.Trim()));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
