using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameTracker;
using GameTracker.Api;
using GameTracker.Api.Auth;
using GameTracker.Models;
using GameTracker.Services;

namespace GameTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LibraryController : ControllerBase
    {
        private const int PopularBatchSize = 120;

        private readonly IgdbApiService _igdb;
        private readonly RawgApiService _rawg;
        private readonly GeminiService _geminiService;
        private readonly HybridGameDetailService _hybridDetail;

        public LibraryController(
            IgdbApiService igdb,
            RawgApiService rawg,
            GeminiService geminiService,
            HybridGameDetailService hybridDetail)
        {
            _igdb = igdb;
            _rawg = rawg;
            _geminiService = geminiService;
            _hybridDetail = hybridDetail;
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public IActionResult GetUserLibrary(int userId, [FromQuery] string status = null)
        {
            if (!User.TryGetUserId(out var authedId) || authedId != userId)
                return Forbid();

            var library = LibraryManager.GetUserLibrary(userId, status);
            return Ok(library);
        }

        [Authorize]
        [HttpPost("user/{userId}/add")]
        public IActionResult AddGameToLibrary(int userId, [FromBody] AddToLibraryGameDto body, [FromQuery] string status = "PlanToPlay")
        {
            if (!User.TryGetUserId(out var authedId) || authedId != userId)
                return Forbid();
            if (body == null || body.Id <= 0)
                return BadRequest(new { message = "Geçerli oyun bilgisi gerekli (id)." });
            if (!LibraryStatuses.IsAllowed(status))
                return BadRequest(new { message = "Invalid library status." });

            var game = new Game
            {
                Id = body.Id,
                Name = body.Name ?? "Unknown",
                BackgroundImage = body.BackgroundImage ?? "",
            };

            bool added = LibraryManager.AddGameToLibrary(userId, game, status);
            if (added) return Ok(new { message = "Game added to library." });
            return BadRequest("Game could not be added or already exists in library.");
        }

        [Authorize]
        [HttpDelete("user/{userId}/remove/{gameId}")]
        public IActionResult RemoveGameFromLibrary(int userId, int gameId)
        {
            if (!User.TryGetUserId(out var authedId) || authedId != userId)
                return Forbid();

            bool removed = LibraryManager.RemoveGame(userId, gameId);
            if (removed) return Ok(new { message = "Game removed." });
            return BadRequest("Could not remove game.");
        }

        [Authorize]
        [HttpPut("user/{userId}/status/{gameId}")]
        public IActionResult UpdateGameStatus(int userId, int gameId, [FromBody] UpdateStatusRequest req)
        {
            if (!User.TryGetUserId(out var authedId) || authedId != userId)
                return Forbid();
            if (req == null || string.IsNullOrWhiteSpace(req.NewStatus))
                return BadRequest(new { message = "NewStatus is required." });
            if (!LibraryStatuses.IsAllowed(req.NewStatus))
                return BadRequest(new { message = "Invalid library status." });

            bool updated = LibraryManager.UpdateGameStatus(userId, gameId, req.NewStatus);
            if (updated) return Ok(new { message = "Status updated." });
            return BadRequest("Status could not be updated.");
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGames(
            [FromQuery] string query,
            [FromQuery] bool nsfw = false,
            [FromQuery] int size = 40,
            CancellationToken cancellationToken = default)
        {
            var results = await _igdb.SearchAsync(query, size, nsfw, cancellationToken).ConfigureAwait(false);
            return Ok(results);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularGames(
            [FromQuery] int offset = 0,
            [FromQuery] bool nsfw = false,
            CancellationToken cancellationToken = default)
        {
            var games = await _igdb.GetPopularAsync(offset, PopularBatchSize, nsfw, cancellationToken).ConfigureAwait(false);
            bool hasMore = games.Count >= PopularBatchSize;
            int nextOffset = offset + games.Count;

            bool igdbConfigured = AppConfig.IsIgdbConfigured;
            bool rawgConfigured = !string.IsNullOrWhiteSpace(AppConfig.RawgApiKey);
            return Ok(new { items = games, nextOffset, hasMore, igdbConfigured, rawgConfigured });
        }

        [HttpGet("game/{gameId}")]
        public async Task<IActionResult> GetGameDetails(int gameId, CancellationToken cancellationToken = default)
        {
            var game = await _hybridDetail.GetDetailAsync(gameId, cancellationToken).ConfigureAwait(false);
            if (game == null) return NotFound();
            return Ok(game);
        }

        [HttpGet("game/{gameId}/screenshots")]
        public async Task<IActionResult> GetGameScreenshots(int gameId, CancellationToken cancellationToken = default)
        {
            var screenshots = await _igdb.GetScreenshotsAsync(gameId, cancellationToken).ConfigureAwait(false);
            return Ok(screenshots ?? new List<Screenshot>());
        }

        [HttpGet("discover")]
        public async Task<IActionResult> GetDiscoverGames(
            [FromQuery] int? genre = null,
            [FromQuery] string mode = "trending",
            [FromQuery] int page = 1,
            [FromQuery] bool nsfw = false,
            CancellationToken cancellationToken = default)
        {
            const int pageSize = 20;
            var games = await _igdb.GetDiscoverAsync(genre, mode, page, pageSize, nsfw, cancellationToken).ConfigureAwait(false);
            bool hasMore = games.Count == pageSize;
            bool igdbConfigured = AppConfig.IsIgdbConfigured;
            bool rawgConfigured = !string.IsNullOrWhiteSpace(AppConfig.RawgApiKey);
            return Ok(new { items = games, hasMore, nextPage = page + 1, igdbConfigured, rawgConfigured });
        }

        [HttpPost("recommendations")]
        public async Task<IActionResult> GetRecommendations(
            [FromBody] List<string> userGames,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var recommendedNames = await _geminiService.GetRecommendationsAsync(userGames).ConfigureAwait(false);
                if (recommendedNames == null || recommendedNames.Count == 0) return Ok(new List<Game>());

                var tasks = recommendedNames.Take(15).Select(async name =>
                {
                    var found = await _igdb.SearchAsync(name, 1, showNsfw: false, cancellationToken).ConfigureAwait(false);
                    return found.FirstOrDefault();
                });

                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
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
