using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GameTracker;
using GameTracker.Models;
using GameTracker.Services;

namespace GameTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibraryController : ControllerBase
    {
        private readonly RawgApiService _rawgApi;
        private readonly GeminiService _geminiService;

        public LibraryController(RawgApiService rawgApi, GeminiService geminiService)
        {
            _rawgApi = rawgApi;
            _geminiService = geminiService;
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetUserLibrary(int userId, [FromQuery] string status = null)
        {
            var library = LibraryManager.GetUserLibrary(userId, status);
            return Ok(library);
        }

        [HttpPost("user/{userId}/add")]
        public IActionResult AddGameToLibrary(int userId, [FromBody] Game game, [FromQuery] string status = "PlanToPlay")
        {
            bool added = LibraryManager.AddGameToLibrary(userId, game, status);
            if (added) return Ok(new { message = "Game added to library." });
            return BadRequest("Game could not be added or already exists in library.");
        }

        [HttpDelete("user/{userId}/remove/{gameId}")]
        public IActionResult RemoveGameFromLibrary(int userId, int gameId)
        {
            bool removed = LibraryManager.RemoveGame(userId, gameId);
            if (removed) return Ok(new { message = "Game removed." });
            return BadRequest("Could not remove game.");
        }

        [HttpPut("user/{userId}/status/{gameId}")]
        public IActionResult UpdateGameStatus(int userId, int gameId, [FromBody] UpdateStatusRequest req)
        {
            bool updated = LibraryManager.UpdateGameStatus(userId, gameId, req.NewStatus);
            if (updated) return Ok(new { message = "Status updated." });
            return BadRequest("Status could not be updated.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string query, [FromQuery] bool nsfw = false, [FromQuery] int size = 20)
        {
            var results = await _rawgApi.GetGamesBySearchAsync(query, nsfw, size);
            return Ok(results);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularGames([FromQuery] int offset = 0, [FromQuery] bool nsfw = false)
        {
            const int rawgPageSize = 40;   // RAWG max page size
            const int pagesToFetch = 3;    // 3 concurrent pages = 120 raw games -> ~80 after filter

            // Her istekte hangi RAWG sayfalarını çekeceğimizi hesapla
            int startPage = (offset / rawgPageSize) + 1;

            // 3 RAWG sayfasını paralel çek
            var tasks = Enumerable.Range(startPage, pagesToFetch)
                .Select(p => _rawgApi.GetPopularGamesAsync(p, nsfw, rawgPageSize));

            var results = await Task.WhenAll(tasks);
            var games = results
                .SelectMany(r => r)
                .Where(g => g.Added >= 3) // Sıfır ilgi gören oyunları at
                .ToList();

            // Eğer 3 sayfanın hepsinden de oyun geldiyse daha fazla sayfa muhtemelen var
            bool hasMore = results.Any(r => r.Count > 0);
            int nextOffset = offset + (rawgPageSize * pagesToFetch);

            return Ok(new { items = games, nextOffset, hasMore });
        }

        [HttpGet("game/{gameId}")]
        public async Task<IActionResult> GetGameDetails(int gameId)
        {
            var game = await _rawgApi.GetGameDetailsAsync(gameId);
            if (game == null) return NotFound();

            return Ok(game);
        }

        [HttpGet("game/{gameId}/screenshots")]
        public async Task<IActionResult> GetGameScreenshots(int gameId)
        {
            var screenshots = await _rawgApi.GetGameScreenshotsAsync(gameId);
            return Ok(screenshots ?? new List<Screenshot>());
        }

        [HttpGet("discover")]
        public async Task<IActionResult> GetDiscoverGames(
            [FromQuery] int? genre = null,
            [FromQuery] string mode = "trending",
            [FromQuery] int page = 1,
            [FromQuery] bool nsfw = false)
        {
            const int pageSize = 20;
            var games = await _rawgApi.GetDiscoverGamesAsync(genre, mode, page, nsfw, pageSize);
            bool hasMore = games.Count == pageSize;
            return Ok(new { items = games, hasMore, nextPage = page + 1 });
        }


        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendations([FromBody] List<string> userGames)
        {
            try
            {
                // 1. AI'dan isimleri al
                var recommendedNames = await _geminiService.GetRecommendationsAsync(userGames);
                if (recommendedNames == null || recommendedNames.Count == 0) return Ok(new List<Game>());

                // 2. Her isim için RAWG'dan oyun verisini paralel çek (hız için)
                // İlk sonucu almak yeterli, genelde AI tam ismi verir.
                var tasks = recommendedNames.Take(15).Select(async name => 
                {
                    var searchResult = await _rawgApi.GetGamesBySearchAsync(name, false, 1);
                    return searchResult.FirstOrDefault();
                });

                var results = await Task.WhenAll(tasks);
                var games = results.Where(g => g != null).ToList();

                return Ok(games);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; }
    }
}
