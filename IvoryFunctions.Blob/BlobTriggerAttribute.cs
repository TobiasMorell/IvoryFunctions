using System.Reflection;
using IvoryFunctions.Decorations;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Blob;

[AttributeUsage(AttributeTargets.Parameter)]
public class BlobTriggerAttribute : TriggerAttribute
{
    public string BlobPath { get; set; }

    /// <summary>
    /// The interval to poll for file changes. The default is 10 seconds.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);

    public BlobTriggerAttribute(string blobPath)
    {
        BlobPath = blobPath;
    }

    public override Function ToFunction(
        FunctionAttribute function,
        MethodInfo method,
        Type parameterType,
        SingletonAttribute? singletonAttribute,
        QueueOutputAttribute? outputQueueAttribute
    )
    {
        return new BlobTriggeredFunction(
            function,
            method,
            parameterType,
            this,
            singletonAttribute is not null,
            outputQueueAttribute?.QueueName
        );
    }
}
