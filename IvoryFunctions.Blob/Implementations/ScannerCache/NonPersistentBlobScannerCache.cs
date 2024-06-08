using System.Collections.Concurrent;
using IvoryFunctions.Blob.Abstractions;

namespace IvoryFunctions.Blob.Implementations.ScannerCache;

internal class NonPersistentBlobScannerCache : IBlobScannerCache
{
    private static readonly DateTime _startTime = DateTime.UtcNow;
    private readonly ConcurrentDictionary<string, DateTime> _lastScanTimes = new();

    public Task<DateTime> GetLastScanTimeAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(_lastScanTimes.GetValueOrDefault(path, _startTime));
    }

    public Task SetLastScanTimeAsync(
        string path,
        DateTime lastScanTime,
        CancellationToken cancellationToken = default
    )
    {
        _lastScanTimes[path] = lastScanTime;
        return Task.CompletedTask;
    }
}
