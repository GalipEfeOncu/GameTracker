using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace GameTracker.Services
{
    /// <summary>
    /// Rotasyonlu refresh token üretimi/doğrulaması.
    /// Token string'i yalnızca istemciye döner; DB'de SHA-256 hex hash saklanır.
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>Yeni refresh token üretir ve DB'ye kaydeder; düz token döner.</summary>
        string Issue(int userId, bool rememberMe);

        /// <summary>
        /// Refresh token rotasyonu: geçerliyse eski token revoke edilir, yeni token üretilir.
        /// Kullanılmış (revoked) bir token yeniden kullanılırsa kullanıcının TÜM aktif refresh'leri iptal edilir ve null döner.
        /// </summary>
        RefreshRotationResult? Rotate(string rawToken);

        /// <summary>Tek oturumu (bu refresh'i) iptal eder.</summary>
        void Revoke(string rawToken);

        /// <summary>Kullanıcının tüm aktif refresh'lerini iptal eder (şifre değişimi / hesap silme).</summary>
        void RevokeAllForUser(int userId);
    }

    public sealed class RefreshRotationResult
    {
        public int UserId { get; init; }
        public string NewRawToken { get; init; } = string.Empty;
        public DateTime ExpiresAtUtc { get; init; }
    }

    public sealed class SqlRefreshTokenService : IRefreshTokenService
    {
        private readonly int _defaultDays;
        private readonly int _rememberMeDays;

        public SqlRefreshTokenService(IConfiguration configuration)
        {
            _defaultDays = int.TryParse(configuration["Jwt:RefreshTokenDays"], out var d) && d > 0 ? d : 30;
            _rememberMeDays = int.TryParse(configuration["Jwt:RefreshTokenRememberMeDays"], out var rd) && rd > 0 ? rd : 90;
        }

        public string Issue(int userId, bool rememberMe)
        {
            var raw = GenerateRaw();
            var hash = Hash(raw);
            var days = rememberMe ? _rememberMeDays : _defaultDays;
            var expires = DateTime.UtcNow.AddDays(days);

            const string sql = @"
INSERT INTO RefreshTokens (user_id, token_hash, expires_at_utc)
VALUES (@uid, @hash, @exp);";

            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@uid",  SqlDbType.Int)             { Value = userId },
                new SqlParameter("@hash", SqlDbType.NVarChar, 128)   { Value = hash },
                new SqlParameter("@exp",  SqlDbType.DateTime2)       { Value = expires },
            });

            return raw;
        }

        public RefreshRotationResult? Rotate(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken)) return null;

            var hash = Hash(rawToken);

            // Önce token kaydını yükle (revoked olsa bile — ihlal tespiti için).
            const string selectSql = @"
SELECT TOP 1 user_id, revoked_at_utc, expires_at_utc
  FROM RefreshTokens
 WHERE token_hash = @hash;";

            var table = DatabaseHelper.ExecuteQuery(selectSql, new[]
            {
                new SqlParameter("@hash", SqlDbType.NVarChar, 128) { Value = hash },
            });

            if (table.Rows.Count == 0) return null;

            var row = table.Rows[0];
            var userId = Convert.ToInt32(row["user_id"]);
            var revoked = row["revoked_at_utc"] != DBNull.Value;
            var expires = Convert.ToDateTime(row["expires_at_utc"]);

            if (revoked)
            {
                // İhlal: revoke edilmiş bir token tekrar kullanıldı → kullanıcının tüm aktif refresh'lerini iptal et.
                RevokeAllForUser(userId);
                return null;
            }

            if (expires <= DateTime.UtcNow) return null;

            var newRaw = GenerateRaw();
            var newHash = Hash(newRaw);
            // Not: RememberMe bilgisi token kaydında tutulmuyor; rotasyonda default TTL kullanılır.
            // Gerçek "uzun oturum" ilk issue'da belirlenen ömür içinde rotasyonla yenilenir — bu yüzden
            // refresh TTL'i kayıtların oluşturulduğu andan itibaren sabit ilerler; rotasyonda da
            // aynı uzunluğu korumak için mevcut expires'a dokunmuyoruz:
            var newExpires = expires;

            const string rotateSql = @"
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
END";

            var affected = DatabaseHelper.ExecuteNonQuery(rotateSql, new[]
            {
                new SqlParameter("@oldHash", SqlDbType.NVarChar, 128) { Value = hash },
                new SqlParameter("@newHash", SqlDbType.NVarChar, 128) { Value = newHash },
                new SqlParameter("@uid",     SqlDbType.Int)           { Value = userId },
                new SqlParameter("@exp",     SqlDbType.DateTime2)     { Value = newExpires },
            });

            // affected: update + insert birleşik sayı. Update başarısızsa insert atlanır.
            if (affected < 1) return null;

            return new RefreshRotationResult
            {
                UserId = userId,
                NewRawToken = newRaw,
                ExpiresAtUtc = newExpires,
            };
        }

        public void Revoke(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken)) return;

            var hash = Hash(rawToken);
            const string sql = @"
UPDATE RefreshTokens
   SET revoked_at_utc = SYSUTCDATETIME()
 WHERE token_hash = @hash
   AND revoked_at_utc IS NULL;";

            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@hash", SqlDbType.NVarChar, 128) { Value = hash },
            });
        }

        public void RevokeAllForUser(int userId)
        {
            const string sql = @"
UPDATE RefreshTokens
   SET revoked_at_utc = SYSUTCDATETIME()
 WHERE user_id = @uid
   AND revoked_at_utc IS NULL;";

            DatabaseHelper.ExecuteNonQuery(sql, new[]
            {
                new SqlParameter("@uid", SqlDbType.Int) { Value = userId },
            });
        }

        private static string GenerateRaw()
        {
            // 32 bayt (256-bit) kriptografik rastgele; hex olarak 64 karakter.
            Span<byte> bytes = stackalloc byte[32];
            RandomNumberGenerator.Fill(bytes);
            var sb = new StringBuilder(64);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static string Hash(string raw)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw.Trim()));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
