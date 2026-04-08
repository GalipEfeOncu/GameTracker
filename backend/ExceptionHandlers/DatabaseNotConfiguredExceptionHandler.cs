using GameTracker.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace GameTracker.Api.ExceptionHandlers;

/// <summary>
/// Yapılandırılmamış veritabanı isteklerinde tutarlı 503 ve istemci tarafından ayırt edilebilir gövde döner.
/// </summary>
public sealed class DatabaseNotConfiguredExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DatabaseNotConfiguredException)
            return false;

        httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await httpContext.Response.WriteAsJsonAsync(
            new DatabaseUnconfiguredProblem(
                error: "database_unconfigured",
                message:
                "Veritabanı bağlantısı yapılandırılmamış. ConnectionStrings:GameTrackerDB ortam değişkeni veya user-secrets ile ayarlayın.",
                detail: "Sunucu şu anda kullanıcı ve kütüphane verilerine erişemiyor."),
            cancellationToken);

        return true;
    }

    private sealed record DatabaseUnconfiguredProblem(string error, string message, string detail);
}
