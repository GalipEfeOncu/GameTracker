using System.Collections.Generic;

namespace GameTracker.Services;

/// <summary>
/// Keşfet tür filtresi: API <c>genre</c> query değeri IGDB genre id olmalıdır.
/// Aşağıdaki id'ler IGDB genre tablosuna göre sabitlenmiştir (Action için yakın alt tür: Hack and slash).
/// </summary>
public static class IgdbGenreCatalog
{
    public static readonly IReadOnlyList<GenreOption> DiscoverGenres = new List<GenreOption>
    {
        new(null, "Tümü"),
        new(25, "Action"),
        new(12, "RPG"),
        new(5, "Shooter"),
        new(31, "Adventure"),
        new(36, "Puzzle"),
        new(15, "Strategy"),
        new(8, "Platformer"),
        new(13, "Simulation"),
        new(10, "Racing"),
        new(14, "Sports"),
        new(4, "Fighting"),
        new(32, "Indie"),
    };

    public sealed record GenreOption(int? IgdbId, string Label);
}
