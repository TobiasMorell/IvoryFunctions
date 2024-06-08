using IvoryFunctions.Abstractions;
using IvoryFunctions.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace IvoryFunctions.Setup;

internal class TimerFunctionsRegistrator : IFunctionRegistrator<TimerTriggeredFunction>
{
    public void Register(
        IEnumerable<TimerTriggeredFunction> functions,
        IServiceCollection serviceCollection
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
}
