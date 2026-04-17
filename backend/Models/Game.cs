using Newtonsoft.Json;
using System.Collections.Generic;

namespace GameTracker.Models
{
    public class Game
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>IGDB veya RAWG slug (liste/detay); hibrit eşlemede kullanılır.</summary>
        [JsonProperty("slug")]
        public string Slug { get; set; } = "";

        [JsonProperty("background_image")]
        public string BackgroundImage { get; set; }

        [JsonProperty("background_image_additional")]
        public string BackgroundImageAdditional { get; set; } // Ekstra detay resmi

        [JsonProperty("description")]
        public string Description { get; set; } // HTML olan açıklama

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

        [JsonProperty("stores")]
        public List<StoreWrapper>? Stores { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; }

        [JsonProperty("short_screenshots")]
        public List<Screenshot> ShortScreenshots { get; set; }

        [JsonProperty("added")]
        public int Added { get; set; } // Kaç kişi bu oyunu kütüphanesine eklemiş (popülerlik göstergesi)

        /// <summary>§3.4: Kullanıcıya gösterilen tek puan (Metacritic veya IGDB türevi).</summary>
        [JsonProperty("display_score")]
        public double? DisplayScore { get; set; }

        [JsonProperty("display_score_kind")]
        public string DisplayScoreKind { get; set; }

        [JsonProperty("display_score_label")]
        public string DisplayScoreLabel { get; set; }

        /// <summary>IGDB eleştiri özeti (detayda Metacritic ile birlikte gösterilir).</summary>
        [JsonProperty("igdb_aggregated_rating")]
        public double? IgdbAggregatedRating { get; set; }

        [JsonProperty("igdb_aggregated_rating_count")]
        public int? IgdbAggregatedRatingCount { get; set; }

        /// <summary>IGDB toplam kullanıcı+kritik birleşik puan.</summary>
        [JsonProperty("igdb_total_rating")]
        public double? IgdbTotalRating { get; set; }

        /// <summary>IGDB yaş derecelendirmeleri (kuruluş + etiket).</summary>
        [JsonProperty("age_ratings_display")]
        public List<AgeRatingDisplayItem> AgeRatingsDisplay { get; set; }

        /// <summary>IGDB game_videos — YouTube video_id (embed için).</summary>
        [JsonProperty("trailer_youtube_id")]
        public string? TrailerYoutubeId { get; set; }

        /// <summary>IGDB <c>game_time_to_beats</c> (saniye → saat).</summary>
        [JsonProperty("time_to_beat")]
        public GameTimeToBeatInfo? TimeToBeat { get; set; }

        /// <summary>RAWG tamamlayıcı çağrıda kullanılan id (hibrit detay).</summary>
        [JsonProperty("rawg_id")]
        public int? RawgId { get; set; }

        /// <summary>
        /// Kullanıcı kütüphanesindeki oyunun durumu (Playing, PlanToPlay, Played, Dropped).
        /// Sadece GetUserLibrary cevabında dolu; RAWG API cevaplarında null.
        /// </summary>
        [JsonProperty("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Kullanıcının oyundaki toplam oynama süresi (dakika). Desktop istemci tarafından
        /// 30 sn aralıklarla heartbeat ile biriktirilir. RAWG cevaplarında 0.
        /// </summary>
        [JsonProperty("playtimeMinutes")]
        public int PlaytimeMinutes { get; set; }
    }

    // --- Alt Classlar ---

    public class ScreenshotResponse
    {
        [JsonProperty("results")]
        public List<Screenshot> Results { get; set; }
    }

    public class Screenshot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }
    }

    public class PlatformWrapper
    {
        [JsonProperty("platform")]
        public PlatformInfo Platform { get; set; }

        [JsonProperty("released_at")]
        public string ReleasedAt { get; set; }

        [JsonProperty("requirements")]
        public Requirements Requirements { get; set; } // Sistem gereksinimleri

        [JsonProperty("requirements_en")]
        public Requirements RequirementsEn { get; set; } // İngilizce sistem gereksinimleri
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

    public class StoreWrapper
    {
        [JsonProperty("store")]
        public StoreInfo Store { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class StoreInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class StoreLinkResponse
    {
        [JsonProperty("results")]
        public List<StoreLink> Results { get; set; }
    }

    public class StoreLink
    {
        [JsonProperty("store_id")]
        public int StoreId { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Tag
    {
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

    public class GameTimeToBeatInfo
    {
        /// <summary>Hızlı / ana hikaye (IGDB <c>hastily</c>).</summary>
        [JsonProperty("main_story_hours")]
        public double? MainStoryHours { get; set; }

        /// <summary>Normal + biraz ek (IGDB <c>normally</c>).</summary>
        [JsonProperty("main_extra_hours")]
        public double? MainExtraHours { get; set; }

        /// <summary>%100 (IGDB <c>completely</c>).</summary>
        [JsonProperty("completionist_hours")]
        public double? CompletionistHours { get; set; }

        [JsonProperty("submission_count")]
        public int? SubmissionCount { get; set; }
    }

    public class AgeRatingDisplayItem
    {
        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
    }

    public class EsrbRating
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class Genre
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
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