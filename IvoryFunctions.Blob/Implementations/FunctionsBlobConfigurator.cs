using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Implementations.ScannerCache;

namespace IvoryFunctions.Blob.Implementations;

internal class FunctionsBlobConfigurator : IFunctionsBlobConfigurator
{
    public Type? ScannerType { get; private set; }
    public Type ScannerCacheType { get; private set; } = typeof(NonPersistentBlobScannerCache);
    public Type MatcherType { get; private set; } = typeof(BlobFileMatcher);

    public IFunctionsBlobConfigurator UseScanner(Type scannerImplementation)
    {
        ScannerType = scannerImplementation;
        return this;
    }

    public IFunctionsBlobConfigurator UseScannerCache(Type scannerCacheImplementation)
    {
        ScannerCacheType = scannerCacheImplementation;
        return this;
    }

    public IFunctionsBlobConfigurator UseBlobMatcher(Type blobMatcherType)
    {
        MatcherType = blobMatcherType;
        return this;
    }
}
