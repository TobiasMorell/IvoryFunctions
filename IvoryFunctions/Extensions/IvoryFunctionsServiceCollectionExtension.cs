using System.Reflection;
using IvoryFunctions.Helpers;
using IvoryFunctions.Setup;
using IvoryFunctions.Topology;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IvoryFunctions.Extensions;

public static class IvoryFunctionsServiceCollectionExtension
{
    public static IEnumerable<Function> AddIvoryFunctions(
        this IBusRegistrationConfigurator cfg,
        params Type[] types
    )
    {
        var assembly = Assembly.GetCallingAssembly();

        var functions = TypeHelper.DiscoverFunctions(types.Any() ? types : assembly.GetTypes());
        var functionClasses = functions.Select(c => c.Method.DeclaringType).ToHashSet();
        foreach (var functionClass in functionClasses)
        {
            cfg.AddSingleton(functionClass);
        }
        foreach (var fun in functions)
        {
            cfg.AddSingleton(fun);
            cfg.AddSingleton(fun.GetType(), fun);
        }

        RegisterFunctions(cfg, functions, assembly);

        cfg.AddHostedService<LogFunctionsOnStartupService>();

        return functions;
    }

    private static void RegisterFunctions(
        IBusRegistrationConfigurator cfg,
        Function[] functions,
        Assembly assembly
    )
    {
        var registrators = TypeHelper.DiscoverFunctionRegistrators(assembly);

        var functionsByType = functions
            .GroupBy(f => f.GetType())
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var (functionType, fns) in functionsByType)
        {
            var registratorType = registrators.GetValueOrDefault(functionType);
            if (registratorType is null)
            {
                throw new InvalidOperationException(
                    $"No registrator found for function type '{functionType.Name}'"
                );
            }

            var registrator = Activator.CreateInstance(registratorType);
            var registerMethod = registratorType.GetMethod(
                nameof(IFunctionRegistrator<Function>.Register),
                [typeof(IEnumerable<>).MakeGenericType(functionType), typeof(IServiceCollection)]
            );
            var typedFns = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.Cast))!
                .MakeGenericMethod(functionType)
                .Invoke(null, [fns]);
            registerMethod.Invoke(registrator, [typedFns, cfg]);
        }
    }

    public static void SetupIvoryFunctionsQueueTriggers<TConfig>(
        this IBusRegistrationContext context,
        IBusFactoryConfigurator<TConfig> busConfigurator,
        IEnumerable<Function> functions
    )
        where TConfig : IReceiveEndpointConfigurator
    {
        var consumers = TypeHelper.BuildMassTransitQueueConsumers(
            functions.Where(f => f is QueueTriggeredFunction).Cast<QueueTriggeredFunction>()
        );

        busConfigurator.SendTopology.ErrorQueueNameFormatter = new PoisonQueueNameFormatter();
        busConfigurator.SendTopology.DeadLetterQueueNameFormatter = new PoisonQueueNameFormatter();

        foreach (var (consumer, fn) in consumers)
        {
            ConfigureQueueConsumer(busConfigurator, fn, consumer, context);
        }

        busConfigurator.ConfigureEndpoints(context);
    }

    private static void ConfigureQueueConsumer<TConfig>(
        IReceiveConfigurator<TConfig> receiveConfigurator,
        QueueTriggeredFunction queueFunction,
        Type consumer,
        IBusRegistrationContext context
    )
        where TConfig : IReceiveEndpointConfigurator
    {
        receiveConfigurator.ReceiveEndpoint(
            queueFunction.QueueTriggerAttribute.QueueName,
            e =>
            {
                e.UseMessageRetry(
                    retry =>
                        retry.Interval(
                            queueFunction.FunctionAttribute.Retries,
                            queueFunction.FunctionAttribute.RetryDelay
                        )
                );

                e.ConcurrentMessageLimit = queueFunction.IsSingleton ? 1 : null;

                e.Consumer(
                    consumer,
                    c =>
                        Activator.CreateInstance(
                            c,
                            [
                                queueFunction,
                                context.GetRequiredService(queueFunction.Method.DeclaringType!),
                                context.GetService<ILoggerFactory>()
                            ]
                        )
                );
            }
        );
    }
}
