using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Implementations.ScannerCache;

namespace IvoryFunctions.Blob.Implementations;

internal class IvoryFunctionsBlobConfigurator : IIvoryFunctionsBlobConfigurator
{
    public Type? ScannerType { get; private set; }
    public Type ScannerCacheType { get; private set; } = typeof(NonPersistentBlobScannerCache);
    public Type MatcherType { get; private set; } = typeof(BlobFileMatcher);

    public IIvoryFunctionsBlobConfigurator UseScanner(Type scannerImplementation)
    {
        ScannerType = scannerImplementation;
        return this;
    }

    public IIvoryFunctionsBlobConfigurator UseScannerCache(Type scannerCacheImplementation)
    {
        ScannerCacheType = scannerCacheImplementation;
        return this;
    }

    public IIvoryFunctionsBlobConfigurator UseBlobMatcher(Type blobMatcherType)
    {
        MatcherType = blobMatcherType;
        return this;
    }
}
