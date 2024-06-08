using IvoryFunctions.Helpers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace IvoryFunctions.Setup;

internal class QueueFunctionsRegistrator : IFunctionRegistrator<QueueTriggeredFunction>
{
    public void Register(
        IEnumerable<QueueTriggeredFunction> functions,
        IServiceCollection serviceCollection
    )
    {
        var addConsumerMethod = serviceCollection
            .GetType()
            .GetMethods()
            .Single(
                m =>
                    m.Name == nameof(IBusRegistrationConfigurator.AddConsumer)
                    && m.GetParameters().Length == 1
            );

        var consumers = TypeHelper.BuildMassTransitQueueConsumers(functions);

        foreach (var (consumer, fn) in consumers)
        {
            var addConsumerTyped = addConsumerMethod.MakeGenericMethod(consumer);

            // 1st arg is an optional Action<IRegistrationContext,IConsumerConfigurator<T>> configure
            addConsumerTyped.Invoke(serviceCollection, [null]);
        }
    }
}
