using MassTransit;

namespace IvoryFunctions.Topology;

public class PoisonQueueNameFormatter : IErrorQueueNameFormatter, IDeadLetterQueueNameFormatter
{
    public string FormatErrorQueueName(string queueName)
    {
        return queueName + "-poison";
    }

    public string FormatDeadLetterQueueName(string queueName)
    {
        return queueName + "-poison";
    }
}
