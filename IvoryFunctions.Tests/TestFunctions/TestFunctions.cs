using IvoryFunctions.Abstractions;
using IvoryFunctions.Blob;
using IvoryFunctions.Decorations;
using IvoryFunctions.Http;
using IvoryFunctions.Http.Models;
using Quartz;

namespace Functions.MassTransit.Tests.TestFunctions;

public record ExampleFunctionMessage(string Message)
{
    public TimeSpan SingletonWaitDuration { get; set; }
}

public class TestFunctions
{
    public int TestFunctionInvocations { get; private set; }

    [Function(nameof(TestFunction))]
    public Task TestFunction([QueueTrigger("example-function")] ExampleFunctionMessage message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }

    [Singleton]
    [Function(nameof(SingletonFunction))]
    public async Task SingletonFunction(
        [QueueTrigger("singleton-function")] ExampleFunctionMessage message
    )
    {
        if (message.SingletonWaitDuration > TimeSpan.Zero)
        {
            await Task.Delay(message.SingletonWaitDuration);
        }
    }

    [Function(nameof(FunctionWithOutput))]
    [QueueOutput("example-output")]
    public Task<ExampleFunctionMessage> FunctionWithOutput(
        [QueueTrigger("function-with-output")] ExampleFunctionMessage message
    )
    {
        var reversedMessage = new string(message.Message.Reverse().ToArray());
        return Task.FromResult(new ExampleFunctionMessage(reversedMessage));
    }

    [Function(nameof(FunctionThatThrows))]
    public Task FunctionThatThrows(
        [QueueTrigger("function-that-throws")] ExampleFunctionMessage message
    )
    {
        TestFunctionInvocations++;
        throw new InvalidOperationException("This function throws an exception");
    }
}

public class FunctionWithoutParameters
{
    [Function(nameof(ExampleFunction))]
    public async Task ExampleFunction()
    {
        await Task.CompletedTask;
    }
}

public class FunctionWithoutTriggerDecoration
{
    [Function(nameof(ExampleFunction))]
    public async Task ExampleFunction(ExampleFunctionMessage parameter)
    {
        await Task.CompletedTask;
    }
}

public class FunctionWithTimerTrigger
{
    private readonly List<DateTime> _functionInvocations = new();
    public IReadOnlyList<DateTime> FunctionInvocations => _functionInvocations;

    [Function(nameof(ExampleTimerFunction))]
    public async Task ExampleTimerFunction(
        [TimerTrigger("0/1 * * * * ?")] TimerInfo timer,
        CancellationToken cancellationToken = default
    )
    {
        _functionInvocations.Add(DateTime.UtcNow);
        await Task.CompletedTask;
    }

    [Function(nameof(TimerWithOutput))]
    [QueueOutput("example-output")]
    public Task<ExampleFunctionMessage> TimerWithOutput(
        [TimerTrigger("0/1 * * * * ?")] TimerInfo timer,
        CancellationToken cancellationToken = default
    )
    {
        _functionInvocations.Add(DateTime.UtcNow);
        return Task.FromResult(new ExampleFunctionMessage(DateTime.UtcNow.ToString()));
    }

    [Function(nameof(TimerWithContextArgument))]
    public Task TimerWithContextArgument(
        [TimerTrigger("0/1 * * * * ?")] TimerInfo timer,
        IJobExecutionContext context
    )
    {
        _functionInvocations.Add(context.FireTimeUtc.DateTime);
        return Task.CompletedTask;
    }
}

public class FunctionWithBlobTrigger
{
    private readonly List<byte[]> _blobs = new();
    public IReadOnlyList<byte[]> Blobs => _blobs;

    [Function(nameof(ExampleBlobFunction))]
    public Task ExampleBlobFunction(
        [BlobTrigger("example-blob-container/{name}.txt")] byte[] blob,
        string name
    )
    {
        _blobs.Add(blob);
        return Task.CompletedTask;
    }
}

public class FunctionWithHttpTrigger
{
    [Function(nameof(HttpFunction))]
    public Task HttpFunction(
        [HttpTrigger(AuthorizationLevel.AllowAnonymous, "GET", Route = "example-http-function")]
            HttpRequestData request
    )
    {
        Console.WriteLine(request.Body);
        return Task.CompletedTask;
    }
}
