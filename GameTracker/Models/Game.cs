using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameTracker.Models
{
    public class Game
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("background_image")]
        public string BackgroundImage { get; set; }

        [JsonProperty("background_image_additional")]
        public string BackgroundImageAdditional { get; set; } // Ekstra detay resmi

        [JsonProperty("description_raw")]
        public string DescriptionRaw { get; set; } // HTML olmayan temiz açıklama

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("rating_top")]
        public int RatingTop { get; set; } // En yüksek puan (örn: 5)

        [JsonProperty("metacritic")]
        public int? Metacritic { get; set; }

        [JsonProperty("released")]
        public string Released { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("playtime")]
        public int Playtime { get; set; } // Ortalama oynanış süresi (saat)

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("esrb_rating")]
        public EsrbRating EsrbRating { get; set; } // Yaş sınırı

        [JsonProperty("platforms")]
        public List<PlatformWrapper> Platforms { get; set; }

        [JsonProperty("genres")]
        public List<Genre> Genres { get; set; }

        [JsonProperty("developers")]
        public List<Developer> Developers { get; set; }

        [JsonProperty("publishers")]
        public List<Publisher> Publishers { get; set; }
    }

    // --- Alt Classlar ---

    public class PlatformWrapper
    {
        [JsonProperty("platform")]
        public PlatformInfo Platform { get; set; }

        [JsonProperty("released_at")]
        public string ReleasedAt { get; set; }

        [JsonProperty("requirements")]
        public Requirements Requirements { get; set; } // Sistem gereksinimleri
    }

    public class PlatformInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class Requirements
    {
        [JsonProperty("minimum")]
        public string Minimum { get; set; }

        [JsonProperty("recommended")]
        public string Recommended { get; set; }
    }

    public class EsrbRating
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } // "Mature", "Everyone" vs.
    }

    public class Genre
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Developer
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Publisher
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class GameResponse
    {
        [JsonProperty("results")]
        public List<Game> results { get; set; }
    }
}