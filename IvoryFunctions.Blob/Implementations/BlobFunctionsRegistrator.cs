using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Extensions;
using IvoryFunctions.Blob.Quartz;
using IvoryFunctions.Configuration;
using IvoryFunctions.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IvoryFunctions.Blob.Implementations;

internal class BlobFunctionsRegistrator : IFunctionRegistrator<BlobTriggeredFunction>
{
    public void Register(
        IEnumerable<BlobTriggeredFunction> functions,
        IServiceCollection serviceCollection,
        IIvoryFunctionsConfigurator configurator
    )
    {
        var blobConfigurator = configurator.GetInternalConfiguration<IvoryFunctionsBlobConfigurator>();
        if (blobConfigurator is null)
        {
            blobConfigurator = new IvoryFunctionsBlobConfigurator();
            blobConfigurator.UseStaticFiles();   
        }

        if (blobConfigurator.ScannerType is null)
        {
            throw new InvalidOperationException(
                $"Invalid configuration of BlobTriggers - no scanner type added. Please add one using {nameof(IIvoryFunctionsBlobConfigurator.UseScanner)} or one of the extension methods."
            );
        }

        if (functions.Any())
        {
            serviceCollection.AddSingleton(
                typeof(IBlobScannerCache),
                blobConfigurator.ScannerCacheType
            );
            serviceCollection.AddSingleton(typeof(IBlobScanner), blobConfigurator.ScannerType);
            serviceCollection.AddSingleton(typeof(IBlobMatcher), blobConfigurator.MatcherType);
            serviceCollection.AddHostedService<BlobTriggerFunctionProducer>();
        }
    }

    public void Prepare(IHost host)
    {
        
    }
}
