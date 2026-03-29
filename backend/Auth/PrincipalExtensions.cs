using System;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GameTracker.Api.Auth
{
    public static class PrincipalExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal? principal, out int userId)
        {
            userId = 0;
            if (principal?.Identity?.IsAuthenticated != true)
                return false;

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase))?.Value;

            return int.TryParse(sub, out userId) && userId > 0;
        }
    }
}
