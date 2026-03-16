using System;
using System.Collections.Concurrent;

namespace GameTracker
{
    /// <summary>
    /// Kayıt sonrası e-posta doğrulama kodları (bellek; 15 dk geçerli).
    /// </summary>
    public static class EmailVerificationStore
    {
        private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresAt)> _codes = new();
        private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(15);

        public static void Set(string email, string code)
        {
            var key = email.Trim().ToLowerInvariant();
            _codes[key] = (code, DateTime.UtcNow.Add(Expiry));
        }

        public static bool TryValidate(string email, string code, out string normalizedEmail)
        {
            normalizedEmail = null;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code)) return false;

            var key = email.Trim().ToLowerInvariant();
            if (!_codes.TryGetValue(key, out var entry)) return false;
            if (DateTime.UtcNow > entry.ExpiresAt)
            {
                _codes.TryRemove(key, out _);
                return false;
            }
            if (entry.Code != code.Trim()) return false;
            normalizedEmail = key;
            _codes.TryRemove(key, out _);
            return true;
        }
    }
}
