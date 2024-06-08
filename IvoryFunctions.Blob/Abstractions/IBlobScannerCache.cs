namespace IvoryFunctions.Blob.Abstractions;

public interface IBlobScannerCache
{
    Task<DateTime> GetLastScanTimeAsync(string path, CancellationToken cancellationToken = default);
    Task SetLastScanTimeAsync(
        string path,
        DateTime lastScanTime,
        CancellationToken cancellationToken = default
    );
}
