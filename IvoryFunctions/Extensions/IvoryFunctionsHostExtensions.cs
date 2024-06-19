using IvoryFunctions.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IvoryFunctions.Extensions;

public static class IvoryFunctionsHostExtensions
{
    public static IHost UseIvoryFunctions(this IHost host)
    {
        var registrators = host.Services.GetServices<IFunctionRegistrator>();

        foreach (var registrator in registrators)
        {
            registrator.Prepare(host);
        }

        return host;
    }
}