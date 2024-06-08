using Functions.MassTransit.Tests.TestFunctions;
using IvoryFunctions.Extensions;
using MassTransit;
using MassTransit.Internals;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RareWineLibraryBackend.Testing;
using Xunit.Abstractions;

namespace Functions.MassTransit.Tests.Abstractions;

public class QueueTriggerFunctionConsumerTests : TestWithSetup
{
    private ITestHarness Harness => Services.GetRequiredService<ITestHarness>();

    [Fact]
    public async Task Can_execute_function_that_does_not_throw()
    {
        // Arrange
        await Harness.Start();
        var endpoint = await Harness.Bus.GetSendEndpoint(new Uri("queue:example-function"));

        // Act
        await endpoint.Send(new ExampleFunctionMessage("Hello, world!"));

        // Assert
        var sent = await Harness.Sent.Any();
        sent.ShouldBeTrue();
        var consumed = await Harness.Consumed.Any();
        consumed.ShouldBeTrue();
    }

    [Fact]
    public async Task Singleton_functions_are_executed_one_at_a_time()
    {
        // Arrange
        var messageDuration = TimeSpan.FromMilliseconds(300);
        await Harness.Start();
        var endpoint = await Harness.Bus.GetSendEndpoint(new Uri("queue:singleton-function"));

        // Act
        await endpoint.SendBatch(
            [
                new ExampleFunctionMessage("Hello, world!")
                {
                    SingletonWaitDuration = messageDuration,
                },
                new ExampleFunctionMessage("Hello, world 2!")
                {
                    SingletonWaitDuration = messageDuration,
                }
            ]
        );

        // Assert
        var sent = await Harness.Sent.Any();
        sent.ShouldBeTrue();

        await Harness.InactivityTask;

        var consumed = await Harness.Consumed.SelectAsync<ExampleFunctionMessage>().ToListAsync();
        consumed.Count.ShouldBe(2);

        var firstMessage = consumed[0];
        firstMessage.Context.Message.Message.ShouldBe("Hello, world!");
        firstMessage.ElapsedTime.ShouldBe(
            messageDuration,
            tolerance: TimeSpan.FromMilliseconds(50)
        );

        var secondMessage = consumed[1];
        secondMessage.Context.Message.Message.ShouldBe("Hello, world 2!");
        secondMessage.ElapsedTime.ShouldBe(
            messageDuration,
            tolerance: TimeSpan.FromMilliseconds(50)
        );
        secondMessage.StartTime.ShouldBe(
            firstMessage.StartTime + messageDuration,
            tolerance: TimeSpan.FromMilliseconds(50)
        );
    }

    [Fact]
    public async Task Output_from_function_with_QueueOutput_attribute_is_queued_on_specified_queue()
    {
        // Arrange
        await Harness.Start();
        var endpoint = await Harness.Bus.GetSendEndpoint(new Uri("queue:function-with-output"));

        // Act
        await endpoint.Send(new ExampleFunctionMessage("Hello, world!"));

        // Assert
        var sent = await Harness.Sent.Any();
        sent.ShouldBeTrue();

        await Harness.InactivityTask;

        var consumed = await Harness.Consumed.SelectAsync<ExampleFunctionMessage>().ToListAsync();
        consumed.Count.ShouldBe(1);

        var firstMessage = consumed[0];
        firstMessage.Context.Message.Message.ShouldBe("Hello, world!");

        var output = await Harness.Sent.SelectAsync<ExampleFunctionMessage>().ToListAsync();
        output.ShouldContain(
            m => m.Context.DestinationAddress.AbsolutePath.Contains("example-output")
        );
    }

    [Fact]
    public async Task Functions_that_throw_an_exception_are_retried()
    {
        // Arrange
        await Harness.Start();
        var endpoint = await Harness.Bus.GetSendEndpoint(new Uri("queue:function-that-throws"));

        // Act
        await endpoint.Send(new ExampleFunctionMessage("Hello, world!"));

        // Assert
        var sent = await Harness.Sent.Any();
        sent.ShouldBeTrue();

        await Harness.InactivityTask;

        var consumed = await Harness.Consumed.SelectAsync<ExampleFunctionMessage>().ToListAsync();
        consumed.Count.ShouldBe(1);

        var firstMessage = consumed[0];
        firstMessage.Exception.ShouldNotBeNull();
        var testFunctions = Harness.Provider.GetRequiredService<TestFunctions.TestFunctions>();
        testFunctions.TestFunctionInvocations.ShouldBe(5);

        var faults = await Harness
            .Published.SelectAsync<Fault<ExampleFunctionMessage>>()
            .ToListAsync();
        faults.Count.ShouldBe(1);
    }

    public QueueTriggerFunctionConsumerTests(ITestOutputHelper testOutputHelper)
        : base(services =>
        {
            services.AddLogging(x => x.AddXUnit(testOutputHelper));

            services.AddMassTransitTestHarness(cfg =>
            {
                var functions = cfg.AddIvoryFunctions(typeof(TestFunctions.TestFunctions));

                cfg.UsingInMemory(
                    (context, config) =>
                    {
                        context.SetupIvoryFunctionsQueueTriggers(config, functions);
                    }
                );
            });
        }) { }
}
