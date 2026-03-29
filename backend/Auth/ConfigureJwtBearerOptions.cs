using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameTracker.Api.Auth
{
    public class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly JwtTokenService _jwt;

        public ConfigureJwtBearerOptions(JwtTokenService jwt)
        {
            _jwt = jwt;
        }

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            // .NET 8+ varsayılan JsonWebTokenHandler ile claim eşlemesi; "sub" tutarlı kalsın diye kapatıyoruz.
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwt.GetSigningKey(),
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };
        }
    }
}
