using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Implementations;
using IvoryFunctions.Configuration;

namespace IvoryFunctions.Blob.Extensions;

public static class IvoryFunctionsConfiguratorExtensions
{
    public static IIvoryFunctionsConfigurator ConfigureBlobTriggers(
        this IIvoryFunctionsConfigurator configurator,
        Action<IIvoryFunctionsBlobConfigurator> configure
    )
    {
        var blobConfigurator = new IvoryFunctionsBlobConfigurator();
        configure(blobConfigurator);
        
        configurator.AddInternalConfiguration(blobConfigurator);

        return configurator;
    }
}