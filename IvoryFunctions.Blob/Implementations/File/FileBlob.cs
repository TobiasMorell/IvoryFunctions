using IvoryFunctions.Blob.Abstractions;

namespace IvoryFunctions.Blob.Implementations.File;

internal class FileBlob : IBlob
{
    private readonly FileInfo _fileInfo;
    public Uri BlobUri => new("file://" + _fileInfo.FullName);

    public FileBlob(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }

    public Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
    {
        return System.IO.File.ReadAllTextAsync(_fileInfo.FullName, cancellationToken);
    }

    public Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default)
    {
        return System.IO.File.ReadAllBytesAsync(_fileInfo.FullName, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Stream>(
            new FileStream(
                _fileInfo.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous
            )
        );
    }
}
