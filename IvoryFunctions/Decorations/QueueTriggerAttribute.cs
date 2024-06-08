using System.Reflection;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Decorations;

public class QueueTriggerAttribute : TriggerAttribute
{
    public string QueueName { get; set; }

    public QueueTriggerAttribute(string queueName)
    {
        QueueName = queueName;
    }

    public override Function ToFunction(
        FunctionAttribute function,
        MethodInfo method,
        Type parameterType,
        SingletonAttribute? singletonAttribute,
        QueueOutputAttribute? outputQueueAttribute
    )
    {
        return new QueueTriggeredFunction(
            function,
            method,
            parameterType,
            this,
            singletonAttribute is not null,
            outputQueueAttribute?.QueueName
        );
    }
}
