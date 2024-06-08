using MassTransit;
using Microsoft.Extensions.Hosting;

namespace IvoryFunctions.Example;

[EntityName("LogThisToConsole")]
public record ExampleMessage(string Message);

public class SendingBackgroundService : BackgroundService
{
    private readonly IBus _bus;

    public SendingBackgroundService(IBus bus)
    {
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messageBatch = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var singletonEndpoint = await _bus.GetSendEndpoint(
                new Uri("queue:long-running-singleton-function")
            );
            await singletonEndpoint.Send(
                new ExampleMessage("First message in batch: " + messageBatch),
                stoppingToken
            );
            await singletonEndpoint.Send(
                new ExampleMessage("Second message in batch: " + messageBatch),
                stoppingToken
            );

            messageBatch++;

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
