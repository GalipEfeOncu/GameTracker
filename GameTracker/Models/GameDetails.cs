using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameTracker.Models
{
    /// <summary>
    /// Oyunun detaylı bilgilerini tutan model.
    /// RAWG API'nin /games/{id} endpoint'inden döner.
    /// </summary>
    public class GameDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description_raw")]
        public string DescriptionRaw { get; set; }

        [JsonProperty("background_image")]
        public string BackgroundImage { get; set; }

        [JsonProperty("background_image_additional")]
        public string BackgroundImageAdditional { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("metacritic")]
        public int? Metacritic { get; set; }

        [JsonProperty("released")]
        public string Released { get; set; }

        [JsonProperty("platforms")]
        public List<PlatformWrapper> Platforms { get; set; }

        [JsonProperty("genres")]
        public List<Genre> Genres { get; set; }

        [JsonProperty("developers")]
        public List<Developer> Developers { get; set; }

        [JsonProperty("publishers")]
        public List<Publisher> Publishers { get; set; }

        [JsonProperty("esrb_rating")]
        public EsrbRating EsrbRating { get; set; }

        [JsonProperty("stores")]
        public List<StoreWrapper> Stores { get; set; }
    }

    // Alt modeller
    public class PlatformInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }

    public class PlatformWrapper
    {
        [JsonProperty("platform")]
        public PlatformInfo Platform { get; set; }
    }

    public class Genre
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Developer
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Publisher
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class EsrbRating
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }  // "Everyone", "Teen", "Mature 17+" vb.
    }

    public class StoreWrapper
    {
        [JsonProperty("store")]
        public Store Store { get; set; }
    }

    public class Store
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }  // "Steam", "PlayStation Store" vb.
    }

    /// <summary>
    /// Screenshot listesi için model
    /// </summary>
    public class Screenshot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class ScreenshotResponse
    {
        [JsonProperty("results")]
        public List<Screenshot> Results { get; set; }
    }
}