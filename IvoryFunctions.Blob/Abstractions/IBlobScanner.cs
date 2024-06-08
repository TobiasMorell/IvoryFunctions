namespace IvoryFunctions.Blob.Abstractions;

public interface IBlobScanner
{
    Task<IBlob[]> ScanAsync(string path, CancellationToken cancellationToken = default);
}
