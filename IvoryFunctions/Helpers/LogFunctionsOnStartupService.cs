using Microsoft.Extensions.Hosting;

namespace IvoryFunctions.Helpers;

public class LogFunctionsOnStartupService : IHostedService
{
    private readonly IEnumerable<Function> _functions;

    public LogFunctionsOnStartupService(IEnumerable<Function> functions)
    {
        _functions = functions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("Functions:");
        Console.WriteLine();

        foreach (var function in _functions)
        {
            Console.ForegroundColor = function.Color;
            Console.WriteLine("\t{0}", function.ConsoleListing);

            Console.WriteLine();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
