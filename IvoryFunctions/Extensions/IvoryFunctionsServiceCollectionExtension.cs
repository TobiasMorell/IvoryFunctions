using System.Diagnostics;
using System.Reflection;
using IvoryFunctions.Configuration;
using IvoryFunctions.Helpers;
using IvoryFunctions.Setup;
using IvoryFunctions.Topology;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IvoryFunctions.Extensions;

public static class IvoryFunctionsServiceCollectionExtension
{
    public static IServiceCollection
        AddIvoryFunctions(this IServiceCollection serviceCollection, params Type[] types) =>
        AddIvoryFunctions(serviceCollection, null, types);

    public static IServiceCollection AddIvoryFunctions(
        this IServiceCollection services,
        Action<IIvoryFunctionsConfigurator>? configure, params Type[] types)
    {
        var assembly = Assembly.GetEntryAssembly();

        var functions = TypeHelper.DiscoverFunctions(types.Any() ? types : assembly.GetTypes());
        var functionClasses = functions.Select(c => c.Method.DeclaringType).ToHashSet();
        foreach (var functionClass in functionClasses)
        {
            services.AddSingleton(functionClass);
        }
        foreach (var fun in functions)
        {
            services.AddSingleton(fun);
            services.AddSingleton(fun.GetType(), fun);
        }
        
        var configurator = new IvoryFunctionsConfigurator();
        services.AddMassTransit(cfg =>
        {
            configure?.Invoke(configurator);
            
            if (configurator.MassTransit is null)
            {
                cfg.UsingInMemory((ctx, busCfg) =>
                {
                    ctx.SetupIvoryFunctionsQueueTriggers(busCfg, functions);
                });
            }
            else
            {
                configurator.MassTransit(cfg, (ctx, busCfg) =>
                {
                    ctx.SetupIvoryFunctionsQueueTriggers(busCfg, functions);
                });
            }
            
            RegisterFunctions(cfg, functions, assembly, configurator);
        });

        services.AddHostedService<LogFunctionsOnStartupService>();

        return services;
    }

    private static void RegisterFunctions(
        IServiceCollection cfg,
        Function[] functions,
        Assembly assembly,
        IIvoryFunctionsConfigurator configurator
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
            cfg.AddSingleton(typeof(IFunctionRegistrator), registrator);
            var registerMethod = registratorType.GetMethod(
                nameof(IFunctionRegistrator<Function>.Register),
                [typeof(IEnumerable<>).MakeGenericType(functionType), typeof(IServiceCollection), typeof(IIvoryFunctionsConfigurator)]
            );
            if (registerMethod is null)
            {
                throw new UnreachableException(
                    "Register method not found on registrator - this is a problem in IvoryFunctions. Please report this to us.");
            }
            
            var typedFns = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.Cast))!
                .MakeGenericMethod(functionType)
                .Invoke(null, [fns]);
            registerMethod.Invoke(registrator, [typedFns, cfg, configurator]);
        }
    }

    internal static void SetupIvoryFunctionsQueueTriggers<TConfig>(
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
