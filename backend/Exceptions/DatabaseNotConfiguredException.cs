namespace GameTracker.Api.Exceptions;

/// <summary>
/// ConnectionStrings:GameTrackerDB tanımlı değilken veritabanı erişimi denendiğinde fırlatılır.
/// <see cref="GameTracker.Api.ExceptionHandlers.DatabaseNotConfiguredExceptionHandler"/> 503 ve makine-okur kod üretir.
/// </summary>
public sealed class DatabaseNotConfiguredException : InvalidOperationException
{
    public DatabaseNotConfiguredException()
        : base(
            "Veritabanı bağlantı dizesi yapılandırılmamış. ConnectionStrings:GameTrackerDB için ortam değişkeni veya dotnet user-secrets kullanın.")
    {
    }
}
