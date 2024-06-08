using System.Reflection;
using IvoryFunctions.Decorations;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Http;

/// <summary>
/// DO NOT INSTANTIATE THIS MANUALLY! The IvoryFunctions library will handle this for you. Please refer to the
/// <see cref="HttpTriggerAttribute"/> if you need a new HTTP-triggered function.
/// </summary>
public record HttpTriggeredFunction(
    FunctionAttribute FunctionAttribute,
    MethodInfo Method,
    Type MessageType,
    HttpTriggerAttribute HttpTriggerAttribute,
    bool IsSingleton,
    string? OutputQueueName
) : Function(FunctionAttribute, Method, MessageType, IsSingleton, OutputQueueName)
{
    public override ConsoleColor Color => ConsoleColor.DarkYellow;

    public override string ConsoleListing =>
        string.Format(
            "{0}: {1}, {2}",
            FunctionAttribute.Name,
            string.Join(", ", HttpTriggerAttribute.Methods),
            $"http://localhost:7071/api/{HttpTriggerAttribute.Route}"
        );
}
