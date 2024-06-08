using System.Reflection;
using IvoryFunctions.Decorations;

namespace IvoryFunctions.Helpers;

public abstract record Function(
    FunctionAttribute FunctionAttribute,
    MethodInfo Method,
    Type MessageType,
    bool IsSingleton,
    string? OutputQueueName
)
{
    public abstract ConsoleColor Color { get; }
    public abstract string ConsoleListing { get; }
}

internal record QueueTriggeredFunction(
    FunctionAttribute FunctionAttribute,
    MethodInfo Method,
    Type MessageType,
    QueueTriggerAttribute QueueTriggerAttribute,
    bool IsSingleton,
    string? OutputQueueName
) : Function(FunctionAttribute, Method, MessageType, IsSingleton, OutputQueueName)
{
    public override ConsoleColor Color => ConsoleColor.Yellow;

    public override string ConsoleListing =>
        string.Format(
            "{0}: queueTrigger - {1}",
            FunctionAttribute.Name,
            QueueTriggerAttribute.QueueName
        );
}

internal record TimerTriggeredFunction(
    FunctionAttribute FunctionAttribute,
    MethodInfo Method,
    Type MessageType,
    TimerTriggerAttribute TimerTriggerAttribute,
    bool IsSingleton,
    string? OutputQueueName
) : Function(FunctionAttribute, Method, MessageType, IsSingleton, OutputQueueName)
{
    public override ConsoleColor Color => ConsoleColor.Cyan;
    public override string ConsoleListing =>
        string.Format(
            "{0}: timerTrigger - {1} (RunOnStartup: {2})",
            FunctionAttribute.Name,
            TimerTriggerAttribute.CronExpression,
            TimerTriggerAttribute.RunOnStartup
        );
}
