using IvoryFunctions.Abstractions;
using IvoryFunctions.Configuration;
using IvoryFunctions.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace IvoryFunctions.Setup;

internal class TimerFunctionsRegistrator : IFunctionRegistrator<TimerTriggeredFunction>
{
    public void Register(
        IEnumerable<TimerTriggeredFunction> functions,
        IServiceCollection serviceCollection,
        IIvoryFunctionsConfigurator configurator
    )
    {
        if (functions.Any())
        {
            serviceCollection
                .AddQuartz()
                .AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            serviceCollection.AddHostedService<TimerTriggerFunctionProducer>();
        }
    }

    public void Prepare(IHost host)
    {
        
    }
}
