using Microsoft.AspNetCore.Mvc;
using GameTracker.Api;

namespace GameTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>Canlılık + veritabanı yapılandırması (bağlantı testi yapmaz; yalnızca connection string varlığı).</summary>
        [HttpGet]
        public IActionResult Get() =>
            Ok(new
            {
                status = "ok",
                database = new { configured = AppConfig.IsDatabaseConfigured }
            });
    }
}
