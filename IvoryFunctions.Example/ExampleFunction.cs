using IvoryFunctions.Abstractions;
using IvoryFunctions.Blob;
using IvoryFunctions.Decorations;
using IvoryFunctions.Example.Messages;
using IvoryFunctions.Http;
using IvoryFunctions.Http.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IvoryFunctions.Example;

public class ExampleFunction
{
    private readonly ILogger _logger;

    public ExampleFunction(ILoggerFactory logger)
    {
        _logger = logger.CreateLogger<ExampleFunction>();
    }

    [Function(nameof(TimerTriggerFunction))]
    [QueueOutput("log-this-to-console")]
    public async Task<ExampleMessage> TimerTriggerFunction(
        [TimerTrigger("0/5 * * * * ?")] TimerInfo timer
    )
    {
        _logger.LogInformation("Timer trigger function invoked!");
        return new ExampleMessage("Timer trigger function invoked!");
    }

    [Function(nameof(LogThisToConsole))]
    [QueueOutput(FollowUpMessage.QueueName)]
    public async Task<FollowUpMessage> LogThisToConsole(
        [QueueTrigger("log-this-to-console")] ExampleMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(message.Message);
        return new FollowUpMessage("Logged to console!");
    }

    [Function(nameof(FollowUp))]
    [QueueOutput("throw-exception")]
    public async Task<ExampleMessage> FollowUp(
        [QueueTrigger(FollowUpMessage.QueueName)] FollowUpMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(message.Message);

        return new ExampleMessage("Followed up successfully!");
    }

    [Function(nameof(LongRunningSingletonFunction))]
    [Singleton]
    public async Task LongRunningSingletonFunction(
        [QueueTrigger("long-running-singleton-function")] ExampleMessage message,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Starting long running singleton function: {0}", message.Message);
        await Task.Delay(10000, cancellationToken);
        _logger.LogInformation("Finished long running singleton function: {0}", message.Message);
    }

    [Function(nameof(LogContentOfBlob))]
    public async Task LogContentOfBlob(
        [BlobTrigger("/home/tobias/dev/RareWineAPI/blobs/{name}.txt")] string blobContent
    )
    {
        _logger.LogInformation("Blob content: {0}", blobContent);
    }

    [Function(nameof(HttpTrigger))]
    public async Task<IActionResult> HttpTrigger(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "example")]
            HttpRequestData request,
        CancellationToken cancellationToken = default
    )
    {
        using var bodyStream = new StreamReader(request.Body);
        var body = await bodyStream.ReadToEndAsync(cancellationToken);

        _logger.LogInformation("HTTP trigger function invoked!\n{0}", body);
        return new OkObjectResult("HTTP trigger function invoked!");
    }
}
