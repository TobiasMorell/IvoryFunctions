using System.Reflection;
using IvoryFunctions.Decorations;
using IvoryFunctions.Helpers;

namespace IvoryFunctions.Http;

public class HttpTriggerAttribute : TriggerAttribute
{
    public AuthorizationLevel AuthorizationLevel { get; init; }
    public IEnumerable<string> Methods { get; init; }
    public string? Route { get; init; }

    public HttpTriggerAttribute(AuthorizationLevel authorizationLevel, params string[] methods)
    {
        AuthorizationLevel = authorizationLevel;
        Methods = methods;
    }

    public HttpTriggerAttribute(params string[] methods)
    {
        AuthorizationLevel = AuthorizationLevel.Function;
        Methods = methods;
    }

    public override Function ToFunction(
        FunctionAttribute function,
        MethodInfo method,
        Type parameterType,
        SingletonAttribute? singletonAttribute,
        QueueOutputAttribute? outputQueueAttribute
    )
    {
        return new HttpTriggeredFunction(
            function,
            method,
            parameterType,
            this,
            singletonAttribute is not null,
            outputQueueAttribute?.QueueName
        );
    }
}
