namespace IvoryFunctions.Blob.Abstractions;

public interface IIvoryFunctionsBlobConfigurator
{
    IIvoryFunctionsBlobConfigurator UseScanner(Type scannerImplementation);
    IIvoryFunctionsBlobConfigurator UseScannerCache(Type scannerCacheImplementation);
    IIvoryFunctionsBlobConfigurator UseBlobMatcher(Type blobMatcherType);
}
