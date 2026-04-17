using System.Collections.Immutable;

namespace GameTracker
{
    /// <summary>
    /// Kütüphane durumu. Kanonik değerler: PlanToPlay, Playing, Completed, Dropped, Wishlist.
    /// <para>
    /// "Played" legacy WinForms değeridir; yazma yolunda (ekleme / durum güncelleme) artık
    /// <see cref="Normalize"/> ile "Completed"e çevrilir. Okuma yolunda eski kayıtlar gelirse
    /// <c>LibraryManager</c> bunları da "Completed" olarak döner. Tek seferlik DB migration:
    /// <c>backend/scripts/NormalizePlayedToCompleted.sql</c>.
    /// </para>
    /// </summary>
    public static class LibraryStatuses
    {
        public const string Completed = "Completed";
        private const string LegacyPlayed = "Played";

        public static readonly ImmutableHashSet<string> Allowed = ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "PlanToPlay",
            "Playing",
            Completed,
            LegacyPlayed,
            "Dropped",
            "Wishlist");

        public static bool IsAllowed(string status) =>
            !string.IsNullOrWhiteSpace(status) && Allowed.Contains(status.Trim());

        /// <summary>
        /// Gelen durumu kanonik forma çevirir (ör. legacy "Played" → "Completed").
        /// Boş / tanınmayan değerlerde olduğu gibi döner; çağıran <see cref="IsAllowed"/> ile doğrulamalıdır.
        /// </summary>
        public static string Normalize(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return status;
            var trimmed = status.Trim();
            if (string.Equals(trimmed, LegacyPlayed, System.StringComparison.OrdinalIgnoreCase))
                return Completed;
            return trimmed;
        }
    }
}
