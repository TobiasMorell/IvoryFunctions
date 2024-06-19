using IvoryFunctions.Configuration;
using IvoryFunctions.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IvoryFunctions.Setup;

public interface IFunctionRegistrator
{
    void Prepare(IHost host);
}

public interface IFunctionRegistrator<TFunction> : IFunctionRegistrator
    where TFunction : Function
{
    void Register(IEnumerable<TFunction> functions, IServiceCollection serviceCollection, IIvoryFunctionsConfigurator configurator);
}
