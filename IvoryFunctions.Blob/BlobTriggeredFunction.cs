using System.Reflection;
using IvoryFunctions.Decorations;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Blob;

internal record BlobTriggeredFunction(
    FunctionAttribute FunctionAttribute,
    MethodInfo Method,
    Type MessageType,
    BlobTriggerAttribute BlobTriggerAttribute,
    bool IsSingleton,
    string? OutputQueueName
) : Function(FunctionAttribute, Method, MessageType, IsSingleton, OutputQueueName)
{
    public override ConsoleColor Color => ConsoleColor.DarkCyan;
    public override string ConsoleListing =>
        string.Format(
            "{0}: blobTrigger - {1}",
            FunctionAttribute.Name,
            BlobTriggerAttribute.BlobPath
        );
}
