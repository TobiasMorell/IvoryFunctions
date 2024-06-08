using System.Reflection;
using IvoryFunctions.Abstractions;
using IvoryFunctions.Decorations;
using IvoryFunctions.Setup;

namespace IvoryFunctions.Helpers;

internal static class TypeHelper
{
    internal static Function[] DiscoverFunctions(params Type[] types)
    {
        var functions = new List<Function>();

        foreach (var type in types)
        {
            var methods = type.GetMethods()
                .Where(f => f.GetCustomAttribute<FunctionAttribute>() is not null);

            foreach (var method in methods)
            {
                var func = method.GetCustomAttribute<FunctionAttribute>();
                var functionArg = method.GetParameters().FirstOrDefault();
                if (functionArg is null)
                {
                    throw new InvalidOperationException(
                        $"Function {method.Name} must have a parameter"
                    );
                }

                var singletonAttribute = method.GetCustomAttribute<SingletonAttribute>();
                var outputQueueAttribute = method.GetCustomAttribute<QueueOutputAttribute>();

                var triggerAttribute = functionArg.GetCustomAttribute<TriggerAttribute>();
                if (triggerAttribute is null)
                {
                    throw new InvalidOperationException(
                        $"Function '{func.Name}' did not declare a trigger attribute. Please add a trigger attribute on the first argument of the function."
                    );
                }

                functions.Add(
                    triggerAttribute.ToFunction(
                        func!,
                        method,
                        functionArg.ParameterType,
                        singletonAttribute,
                        outputQueueAttribute
                    )
                );
            }
        }

        return functions.ToArray();
    }

    internal static (
        Type ConsumerType,
        QueueTriggeredFunction Function
    )[] BuildMassTransitQueueConsumers(IEnumerable<QueueTriggeredFunction> functions)
    {
        var consumerTypes = functions.Select(
            function =>
                (
                    typeof(QueueTriggerFunctionConsumer<,>).MakeGenericType(
                        [function.Method.DeclaringType!, function.MessageType]
                    ),
                    function
                )
        );

        return consumerTypes.ToArray();
    }

    internal static Dictionary<Type, Type> DiscoverFunctionRegistrators(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Where(t => t.Name.Contains("IvoryFunctions"))
            .Select(Assembly.Load)
            .SelectMany(a => a.GetTypes())
            .Where(
                t =>
                    typeof(IFunctionRegistrator).IsAssignableFrom(t)
                    && t is { IsInterface: false, IsAbstract: false }
            )
            .ToDictionary(
                t =>
                    t.GetInterfaces()
                        .First(
                            t => t.IsGenericType && typeof(IFunctionRegistrator).IsAssignableFrom(t)
                        )
                        .GetGenericArguments()[0],
                t => t
            );
    }
}
