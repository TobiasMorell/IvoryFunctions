using Functions.MassTransit.Tests.Fakes;
using Functions.MassTransit.Tests.TestFunctions;
using IvoryFunctions.Abstractions;
using IvoryFunctions.Extensions;
using IvoryFunctions.Helpers;
using MassTransit;
using MassTransit.Internals;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RareWineLibraryBackend.Testing;
using Xunit.Abstractions;

namespace Functions.MassTransit.Tests.Abstractions;

public class InvokeTimerTriggerJobTests : TestWithSetup
{
    private IEnumerable<Function> Functions => Services.GetRequiredService<IEnumerable<Function>>();
    private ITestHarness Harness => Services.GetRequiredService<ITestHarness>();
    private FunctionWithTimerTrigger Function =>
        Services.GetRequiredService<FunctionWithTimerTrigger>();
    private ILoggerFactory LoggerFactory => Services.GetRequiredService<ILoggerFactory>();

    public InvokeTimerTriggerJobTests(ITestOutputHelper testOutputHelper)
        : base(services =>
        {
            services.AddLogging(x => x.AddXUnit(testOutputHelper));
            services.AddMassTransitTestHarness(mt =>
            {
                mt.AddIvoryFunctions(cfg =>
                {
                    cfg.ConfigureMassTransit((x, setupFunctions) =>
                    {
                        x.UsingInMemory(setupFunctions);
                    });
                }, typeof(FunctionWithTimerTrigger));
            });
        }) { }

    [Fact]
    public async Task Executing_job_without_function_throws()
    {
        // Arrange
        var job = CreateSampleJob();
        var context = new FakeJobExecutionContext();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => job.Execute(context));
    }

    [Fact]
    public async Task Executing_job_with_function_in_job_data_map_invokes_function()
    {
        // Arrange
        var job = CreateSampleJob();
        var context = new FakeJobExecutionContext();
        var timerFunction = Functions.First(
            f => f.FunctionAttribute.Name == nameof(FunctionWithTimerTrigger.ExampleTimerFunction)
        );
        context.Put("Function", timerFunction);

        // Act
        await job.Execute(context);

        // Assert
        var invokedOn = Function.FunctionInvocations.ShouldHaveSingleItem();
        invokedOn.ShouldBe(DateTime.UtcNow, TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task Executing_a_timer_trigger_with_QueueOutput_attribute_queues_the_resulting_message()
    {
        // Arrange
        var job = CreateSampleJob();
        var context = new FakeJobExecutionContext();
        var timerWithOutput = Functions.First(
            f => f.FunctionAttribute.Name == nameof(FunctionWithTimerTrigger.TimerWithOutput)
        );
        context.Put("Function", timerWithOutput);

        // Act
        await job.Execute(context);

        // Assert
        Function.FunctionInvocations.ShouldHaveSingleItem();

        var sent = await Harness.Sent.SelectAsync<ExampleFunctionMessage>().ToListAsync();
        var msg = sent.ShouldHaveSingleItem();
        msg.Context.DestinationAddress.AbsolutePath.ShouldContain(timerWithOutput.OutputQueueName);
    }

    [Fact]
    public async Task Can_provide_execution_context_as_function_argument()
    {
        // Arrange
        var fireTime = DateTimeOffset.UtcNow;

        var job = CreateSampleJob();
        var context = new FakeJobExecutionContext() { FireTimeUtc = fireTime, };
        var timerWithOutput = Functions.First(
            f =>
                f.FunctionAttribute.Name
                == nameof(FunctionWithTimerTrigger.TimerWithContextArgument)
        );
        context.Put("Function", timerWithOutput);

        // Act
        await job.Execute(context);

        // Assert
        var firedOn = Function.FunctionInvocations.ShouldHaveSingleItem();
        firedOn.ShouldBe(fireTime.DateTime);
    }

    private InvokeTimerTriggerJob<FunctionWithTimerTrigger> CreateSampleJob()
    {
        return new InvokeTimerTriggerJob<FunctionWithTimerTrigger>(
            Function,
            Harness.Bus,
            LoggerFactory
        );
    }
}
