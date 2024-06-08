using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Blob.Extensions;
using IvoryFunctions.Blob.Quartz;
using IvoryFunctions.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace IvoryFunctions.Blob.Implementations;

internal class BlobFunctionsRegistrator : IFunctionRegistrator<BlobTriggeredFunction>
{
    public void Register(
        IEnumerable<BlobTriggeredFunction> functions,
        IServiceCollection serviceCollection
    )
    {
        var configurator = new FunctionsBlobConfigurator();
        // TODO: Allow custom configuration
        // configure(configurator);
        configurator.UseStaticFiles();

        if (configurator.ScannerType is null)
        {
            throw new InvalidOperationException(
                $"Invalid configuration of BlobTriggers - no scanner type added. Please add one using {nameof(IFunctionsBlobConfigurator.UseScanner)} or one of the extension methods."
            );
        }

        if (functions.Any(f => f is BlobTriggeredFunction))
        {
            serviceCollection.AddSingleton(
                typeof(IBlobScannerCache),
                configurator.ScannerCacheType
            );
            serviceCollection.AddSingleton(typeof(IBlobScanner), configurator.ScannerType);
            serviceCollection.AddSingleton(typeof(IBlobMatcher), configurator.MatcherType);
            serviceCollection.AddHostedService<BlobTriggerFunctionProducer>();
        }
    }
}
