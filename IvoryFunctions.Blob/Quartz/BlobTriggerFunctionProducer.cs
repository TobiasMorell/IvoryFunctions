using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IvoryFunctions.Blob.Quartz;

// TODO: Can this be replaced with https://masstransit.io/documentation/configuration/scheduling#quartznet
internal class BlobTriggerFunctionProducer : BackgroundService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IEnumerable<BlobTriggeredFunction> _functions;
    private readonly ILogger? _logger;

    public BlobTriggerFunctionProducer(
        IEnumerable<BlobTriggeredFunction> timerFunctions,
        ISchedulerFactory schedulerFactory,
        ILoggerFactory? logger
    )
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger?.CreateLogger<BlobTriggerFunctionProducer>();
        _functions = timerFunctions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var function in _functions)
        {
            await ScheduleFunction(function, stoppingToken);
        }
    }

    private async Task ScheduleFunction(
        BlobTriggeredFunction function,
        CancellationToken stoppingToken
    )
    {
        var jobType = typeof(BlobScannerJob<>).MakeGenericType(function.Method.DeclaringType!);

        var job = JobBuilder
            .Create(jobType)
            .WithIdentity(function.FunctionAttribute.Name, function.Method.DeclaringType!.Name)
            .WithDescription(
                $"Job for {function.FunctionAttribute.Name} ({function.BlobTriggerAttribute.BlobPath}) with interval {function.BlobTriggerAttribute.PollingInterval} seconds"
            )
            .Build();

        job.JobDataMap.Put("Function", function);

        var trigger = TriggerBuilder
            .Create()
            .WithDescription(
                $"Interval trigger for {function.FunctionAttribute.Name} ({function.BlobTriggerAttribute.PollingInterval})"
            )
            .ForJob(job)
            .StartNow()
            .WithSimpleSchedule(
                s => s.WithInterval(function.BlobTriggerAttribute.PollingInterval).RepeatForever()
            )
            .Build();

        _logger?.LogInformation(
            new(
                $"[{function.FunctionAttribute.Name}] started on {trigger.StartTimeUtc} with interval '{function.BlobTriggerAttribute.PollingInterval}'"
            )
        );

        var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);
        await scheduler.ScheduleJob(job, trigger, stoppingToken);
    }
}
