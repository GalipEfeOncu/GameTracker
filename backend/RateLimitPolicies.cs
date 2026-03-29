using System.Linq;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace GameTracker.Api;

public static class RateLimitPolicies
{
    public const string AuthForms = "auth-forms";
    public const string AuthDestructive = "auth-destructive";

    public static string PartitionByIp(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(ip))
            return ip;

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            var first = forwarded.Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(first))
                return first;
        }

        return "unknown";
    }

    public static RateLimitPartition<string> AuthFormsPartition(HttpContext httpContext) =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionByIp(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });

    public static RateLimitPartition<string> AuthDestructivePartition(HttpContext httpContext) =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionByIp(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
}
