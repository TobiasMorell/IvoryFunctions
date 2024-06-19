using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Implementations.File;

namespace IvoryFunctions.Blob.Extensions;

public static class IFunctionsBlobConfiguratorExtensions
{
    public static IIvoryFunctionsBlobConfigurator UseScanner<TScanner>(
        this IIvoryFunctionsBlobConfigurator configurator
    )
    {
        configurator.UseScanner(typeof(TScanner));
        return configurator;
    }

    public static IIvoryFunctionsBlobConfigurator UseStaticFiles(
        this IIvoryFunctionsBlobConfigurator configurator
    )
    {
        return configurator.UseScanner<FileBlobScanner>();
    }

    public static IIvoryFunctionsBlobConfigurator UseScannerCache<TScannerCache>(
        this IIvoryFunctionsBlobConfigurator configurator
    )
    {
        configurator.UseScannerCache(typeof(TScannerCache));
        return configurator;
    }

    public static IIvoryFunctionsBlobConfigurator UseBlobMatcher<TBlobMatcher>(
        this IIvoryFunctionsBlobConfigurator configurator
    )
    {
        configurator.UseBlobMatcher(typeof(TBlobMatcher));
        return configurator;
    }
}
