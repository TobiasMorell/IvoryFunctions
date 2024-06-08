using System.Reflection;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Decorations;

[AttributeUsage(AttributeTargets.Parameter)]
public abstract class TriggerAttribute : Attribute
{
    public abstract Function ToFunction(
        FunctionAttribute function,
        MethodInfo method,
        Type parameterType,
        SingletonAttribute? singletonAttribute,
        QueueOutputAttribute? outputQueueAttribute
    );
}
