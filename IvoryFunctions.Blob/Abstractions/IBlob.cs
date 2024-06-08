namespace IvoryFunctions.Blob.Abstractions;

public interface IBlob
{
    Uri BlobUri { get; }
    Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default);
    Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(CancellationToken cancellationToken = default);
}
