namespace IvoryFunctions.Decorations;

[AttributeUsage(AttributeTargets.Method)]
public class FunctionAttribute : Attribute
{
    public string Name { get; init; }

    public int Retries { get; init; } = 4;
    public TimeSpan RetryDelay { get; init; }

    public FunctionAttribute(string name)
    {
        Name = name;
    }
}
