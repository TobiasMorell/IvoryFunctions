using System.Reflection;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Decorations;

[AttributeUsage(AttributeTargets.Parameter)]
public class TimerTriggerAttribute : TriggerAttribute
{
    public string CronExpression { get; init; }
    public bool RunOnStartup { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="TimerTriggerAttribute"/>.
    /// </summary>
    /// <param name="cronExpression">See https://www.quartz-scheduler.net/documentation/quartz-3.x/how-tos/crontrigger.html for examples.</param>
    public TimerTriggerAttribute(string cronExpression)
    {
        CronExpression = cronExpression;
    }

    public override Function ToFunction(
        FunctionAttribute function,
        MethodInfo method,
        Type parameterType,
        SingletonAttribute? singletonAttribute,
        QueueOutputAttribute? outputQueueAttribute
    )
    {
        return new TimerTriggeredFunction(
            function,
            method,
            parameterType,
            this,
            singletonAttribute is not null,
            outputQueueAttribute?.QueueName
        );
    }
}
