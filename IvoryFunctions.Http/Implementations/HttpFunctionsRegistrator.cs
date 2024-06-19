using System.Reflection;
using IvoryFunctions.Configuration;
using IvoryFunctions.Http.Utils;
using IvoryFunctions.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IvoryFunctions.Http.Implementations;

internal class HttpFunctionsRegistrator : IFunctionRegistrator<HttpTriggeredFunction>
{
    public void Register(
        IEnumerable<HttpTriggeredFunction> functions,
        IServiceCollection serviceCollection,
        IIvoryFunctionsConfigurator configurator
    )
    {
        serviceCollection.AddControllers().AddApplicationPart(Assembly.GetExecutingAssembly()).AddControllersAsServices();
        serviceCollection.AddSingleton<IHttpPathMatcher, HttpPathMatcher>();
    }

    public void Prepare(IHost host)
    {
        if (host is not IEndpointRouteBuilder endpointRouteBuilder)
        {
            throw new InvalidOperationException("Host is not an IEndpointRouteBuilder. You must use an ASP.NET Core host that implements IEndpointRouteBuilder to use HttpTrigger, e.g. WebApplication.");
        }

        endpointRouteBuilder.MapControllers();
    }
}
