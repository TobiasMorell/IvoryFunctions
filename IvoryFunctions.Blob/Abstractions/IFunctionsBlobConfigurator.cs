namespace IvoryFunctions.Blob.Abstractions;

public interface IFunctionsBlobConfigurator
{
    IFunctionsBlobConfigurator UseScanner(Type scannerImplementation);
    IFunctionsBlobConfigurator UseScannerCache(Type scannerCacheImplementation);
    IFunctionsBlobConfigurator UseBlobMatcher(Type blobMatcherType);
}
