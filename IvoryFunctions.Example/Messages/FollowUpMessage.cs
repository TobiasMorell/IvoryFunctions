namespace IvoryFunctions.Example.Messages;

public record FollowUpMessage(string Message)
{
    public const string QueueName = "follow-up";
}
