using System.Reflection;
using IvoryFunctions.Http.Utils;
using IvoryFunctions.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace IvoryFunctions.Http.Implementations;

internal class HttpFunctionsRegistrator : IFunctionRegistrator<HttpTriggeredFunction>
{
    public void Register(
        IEnumerable<HttpTriggeredFunction> functions,
        IServiceCollection serviceCollection
    )
    {
        serviceCollection.AddControllers().AddApplicationPart(Assembly.GetExecutingAssembly()).AddControllersAsServices();
        serviceCollection.AddSingleton<IHttpPathMatcher, HttpPathMatcher>();
    }
}
