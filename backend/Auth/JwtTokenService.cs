using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace GameTracker.Api.Auth
{
    /// <summary>
    /// HS256 access token üretir. Üretimde Jwt:SigningKey user-secrets veya env ile verilmelidir (min. 32 karakter).
    /// </summary>
    public class JwtTokenService
    {
        private readonly string _signingKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenMinutes;

        public JwtTokenService(IConfiguration configuration, IHostEnvironment environment, ILogger<JwtTokenService> logger)
        {
            var key = configuration["Jwt:SigningKey"] ?? configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key))
            {
                if (environment.IsDevelopment())
                {
                    key = "DEV_ONLY_GAMETRACKER_JWT_SIGNING_KEY_32+CHARS!";
                    logger.LogWarning("Jwt:SigningKey tanımlı değil; Development için geçici anahtar kullanılıyor. Üretimde asla bırakmayın.");
                }
                else
                    throw new InvalidOperationException("Jwt:SigningKey (veya Jwt:Key) üretim ortamında zorunludur.");
            }

            if (key.Length < 32)
                throw new InvalidOperationException("Jwt:SigningKey en az 32 karakter olmalıdır (HS256).");

            _signingKey = key;
            _issuer = configuration["Jwt:Issuer"] ?? "GameTracker.Api";
            _audience = configuration["Jwt:Audience"] ?? "GameTracker.Web";
            // Kısa ömürlü access token — uzun oturum için ayrıca refresh token rotasyonu kullanılır.
            _accessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 15;
        }

        public string Issuer => _issuer;
        public string Audience => _audience;
        public SymmetricSecurityKey GetSigningKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));

        /// <summary>Kısa ömürlü access JWT üretir. Uzun oturum refresh token rotasyonu ile sağlanır.</summary>
        public string CreateAccessToken(int userId, string username, string email, out DateTime expiresAtUtc)
        {
            expiresAtUtc = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);
            var credentials = new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username ?? ""),
                new Claim(ClaimTypes.Email, email ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
