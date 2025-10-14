using System.Collections.Generic;

namespace GameTracker.Models
{
    public class Game
    {
        public int id { get; set; }
        public string name { get; set; }
        public string background_image { get; set; }
        public double rating { get; set; }
        public string released { get; set; }
        public List<Platform> platforms { get; set; }
    }

    public class Platform
    {
        public PlatformInfo platform { get; set; }
    }

    public class PlatformInfo
    {
        public string name { get; set; }
    }

    public class GameResponse
    {
        public List<Game> results { get; set; }
    }
}