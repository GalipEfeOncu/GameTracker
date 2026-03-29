using System.Collections.Immutable;

namespace GameTracker
{
    /// <summary>
    /// Kütüphane durumu — web UI ve legacy WinForms ile uyum. "Played" legacy değer.
    /// </summary>
    public static class LibraryStatuses
    {
        public static readonly ImmutableHashSet<string> Allowed = ImmutableHashSet.Create(
            StringComparer.OrdinalIgnoreCase,
            "PlanToPlay",
            "Playing",
            "Completed",
            "Played",
            "Dropped",
            "Wishlist");

        public static bool IsAllowed(string status) =>
            !string.IsNullOrWhiteSpace(status) && Allowed.Contains(status.Trim());
    }
}
