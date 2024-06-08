using IvoryFunctions.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace IvoryFunctions.Setup;

public interface IFunctionRegistrator { }

public interface IFunctionRegistrator<TFunction> : IFunctionRegistrator
    where TFunction : Function
{
    void Register(IEnumerable<TFunction> functions, IServiceCollection serviceCollection);
}
