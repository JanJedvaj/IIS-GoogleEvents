using System.Diagnostics.CodeAnalysis;

namespace IISGoogleEvents.Infrastructure.Clients.Dhmz;

public class DhmzFeedCache
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    private DhmzWeatherXml? _feed;
    private DateTimeOffset _fetchedAt;

    public async Task<DhmzWeatherXml> GetOrFetchAsync(
        Func<CancellationToken, Task<DhmzWeatherXml>> fetch,
        TimeSpan timeToLive,
        CancellationToken cancellationToken)
    {
        if (TryRead(timeToLive, out var cached))
            return cached;

        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (TryRead(timeToLive, out cached))
                return cached;

            var feed = await fetch(cancellationToken);

            _feed = feed;
            _fetchedAt = DateTimeOffset.UtcNow;

            return feed;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool TryRead(TimeSpan timeToLive, [NotNullWhen(true)] out DhmzWeatherXml? feed)
    {
        feed = _feed;

        return feed is not null && DateTimeOffset.UtcNow - _fetchedAt < timeToLive;
    }
}
