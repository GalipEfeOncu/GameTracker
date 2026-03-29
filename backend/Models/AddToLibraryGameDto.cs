using Newtonsoft.Json;

namespace GameTracker.Models
{
    /// <summary>
    /// Kütüphaneye ekleme için minimal gövde — tam <see cref="Game"/> modeli çok alanlı olduğu için
    /// kısmi JSON, nullable reference doğrulamasında hata üretiyordu.
    /// </summary>
    public class AddToLibraryGameDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("background_image")]
        public string? BackgroundImage { get; set; }
    }
}
