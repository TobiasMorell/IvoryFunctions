namespace IvoryFunctions.Helpers;

public static class FunctionArgumentHelper
{
    public static void ResolveFunctionArgumentsFromContext<TContext>(
        Function function,
        object[] args,
        TContext context
    )
    {
        var arguments = function
            .Method.GetParameters()
            // Skip the trigger argument
            .Skip(1);

        var contextProperties = context
            .GetType()
            .GetProperties()
            .DistinctBy(p => p.PropertyType)
            .ToDictionary(p => p.PropertyType);
        foreach (var argument in arguments)
        {
            var argType = argument.ParameterType;

            if (typeof(TContext).IsAssignableTo(argType))
            {
                args[argument.Position] = context;
            }
            else if (contextProperties.TryGetValue(argType, out var property))
            {
                var value = property.GetValue(context);

                args[argument.Position] =
                    value
                    ?? throw new InvalidOperationException(
                        $"[{function.FunctionAttribute.Name}]: Could not resolve argument of type {argType}"
                    );
            }
            else
            {
                throw new InvalidOperationException(
                    $"[{function.FunctionAttribute.Name}]: Could not resolve argument of type {argType}"
                );
            }
        }
    }
}
