using IvoryFunctions.Blob.Abstractions;

namespace IvoryFunctions.Blob.Implementations.File;

internal class FileBlobScanner : IBlobScanner
{
    private readonly IBlobScannerCache _scannerCache;
    private readonly IBlobMatcher _blobMatcher;

    public FileBlobScanner(IBlobScannerCache scannerCache, IBlobMatcher blobMatcher)
    {
        _scannerCache = scannerCache;
        _blobMatcher = blobMatcher;
    }

    public async Task<IBlob[]> ScanAsync(string path, CancellationToken cancellationToken = default)
    {
        var lastScanTime = await _scannerCache.GetLastScanTimeAsync(path, cancellationToken);

        IBlob[] files = [];
        if (_blobMatcher.IsDynamicPath(path))
        {
            var pathUpToDynamicSegment = path.Substring(0, path.IndexOf('{'));
            var directory = new DirectoryInfo(pathUpToDynamicSegment);
            if (!directory.Exists)
            {
                return [];
            }

            files = RecursiveScanForFilesInDirectory(directory, path, lastScanTime);
        }
        else if (System.IO.File.Exists(path))
        {
            files = [new FileBlob(new FileInfo(path))];
        }

        await _scannerCache.SetLastScanTimeAsync(path, DateTime.UtcNow, cancellationToken);

        return files;
    }

    private IBlob[] RecursiveScanForFilesInDirectory(
        DirectoryInfo directory,
        string path,
        DateTime lastScanTime
    )
    {
        var blobs = new List<IBlob>();
        foreach (var file in directory.EnumerateFiles())
        {
            if (
                file.LastWriteTimeUtc >= lastScanTime
                && _blobMatcher.IsMatchingPath(file.FullName, path)
            )
            {
                blobs.Add(new FileBlob(file));
            }
        }

        foreach (var subDirectory in directory.EnumerateDirectories())
        {
            if (_blobMatcher.IsMatchingPath(subDirectory.FullName, path))
            {
                blobs.AddRange(RecursiveScanForFilesInDirectory(subDirectory, path, lastScanTime));
            }
        }

        return blobs.ToArray();
    }
}
