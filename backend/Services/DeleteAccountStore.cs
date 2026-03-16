using System;
using System.Collections.Concurrent;

namespace GameTracker
{
    /// <summary>
    /// Hesap silme onay kodları: userId -> (code, expiry). 15 dk geçerli.
    /// </summary>
    public static class DeleteAccountStore
    {
        private static readonly ConcurrentDictionary<int, (string Code, DateTime ExpiresAt)> _codes = new();
        private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(15);

        public static void Set(int userId, string code)
        {
            _codes[userId] = (code, DateTime.UtcNow.Add(Expiry));
        }

        public static bool TryValidate(int userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            if (!_codes.TryGetValue(userId, out var entry)) return false;
            if (DateTime.UtcNow > entry.ExpiresAt)
            {
                _codes.TryRemove(userId, out _);
                return false;
            }
            if (entry.Code != code.Trim()) return false;
            _codes.TryRemove(userId, out _);
            return true;
        }
    }
}
