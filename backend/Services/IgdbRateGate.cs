using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameTracker.Services;

/// <summary>
/// IGDB resmi limiti ~4 istek/saniye; ardışık çağrılar arasında minimum boşluk uygular.
/// </summary>
public sealed class IgdbRateGate
{
    private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(260);
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private DateTime _nextAllowedUtc = DateTime.MinValue;

    public async Task WaitTurnAsync(CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var now = DateTime.UtcNow;
            if (now < _nextAllowedUtc)
                await Task.Delay(_nextAllowedUtc - now, cancellationToken).ConfigureAwait(false);
            _nextAllowedUtc = DateTime.UtcNow.Add(MinInterval);
        }
        finally
        {
            _mutex.Release();
        }
    }
}
