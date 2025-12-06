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

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("metacritic")]
        public int? Metacritic { get; set; }

        [JsonProperty("released")]
        public string Released { get; set; }

        [JsonProperty("platforms")]
        public List<Platform> Platforms { get; set; }
    }

    public class Platform
    {
        [JsonProperty("platform")]
        public PlatformInfo platform { get; set; }
    }

    public class GameResponse
    {
        [JsonProperty("results")]
        public List<Game> results { get; set; }
    }
}