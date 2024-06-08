using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Implementations.File;

namespace IvoryFunctions.Blob.Extensions;

public static class IFunctionsBlobConfiguratorExtensions
{
    public static IFunctionsBlobConfigurator UseScanner<TScanner>(
        this IFunctionsBlobConfigurator configurator
    )
    {
        configurator.UseScanner(typeof(TScanner));
        return configurator;
    }

    public static IFunctionsBlobConfigurator UseStaticFiles(
        this IFunctionsBlobConfigurator configurator
    )
    {
        return configurator.UseScanner<FileBlobScanner>();
    }

    public static IFunctionsBlobConfigurator UseScannerCache<TScannerCache>(
        this IFunctionsBlobConfigurator configurator
    )
    {
        configurator.UseScannerCache(typeof(TScannerCache));
        return configurator;
    }

    public static IFunctionsBlobConfigurator UseBlobMatcher<TBlobMatcher>(
        this IFunctionsBlobConfigurator configurator
    )
    {
        configurator.UseBlobMatcher(typeof(TBlobMatcher));
        return configurator;
    }
}
