namespace IvoryFunctions.Decorations;

[AttributeUsage(AttributeTargets.Method)]
public class QueueOutputAttribute : Attribute
{
    public string QueueName { get; init; }

    public QueueOutputAttribute(string queueName)
    {
        QueueName = queueName;
    }
}
