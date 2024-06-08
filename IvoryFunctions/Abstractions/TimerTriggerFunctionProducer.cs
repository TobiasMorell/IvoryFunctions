using IvoryFunctions.Decorations;
using IvoryFunctions.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IvoryFunctions.Abstractions;

// TODO: Can this be replaced with https://masstransit.io/documentation/configuration/scheduling#quartznet
internal class TimerTriggerFunctionProducer : BackgroundService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IEnumerable<TimerTriggeredFunction> _functions;
    private readonly ILogger? _logger;

    public TimerTriggerFunctionProducer(
        IEnumerable<TimerTriggeredFunction> timerFunctions,
        ISchedulerFactory schedulerFactory,
        ILoggerFactory? logger
    )
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger?.CreateLogger<TimerTriggerFunctionProducer>();
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
        TimerTriggeredFunction function,
        CancellationToken stoppingToken
    )
    {
        var jobType = typeof(InvokeTimerTriggerJob<>).MakeGenericType(
            function.Method.DeclaringType!
        );

        var job = JobBuilder
            .Create(jobType)
            .WithIdentity(function.FunctionAttribute.Name, function.Method.DeclaringType!.Name)
            .WithDescription(
                $"Job for {function.FunctionAttribute.Name} ({function.TimerTriggerAttribute.CronExpression})"
            )
            .Build();

        job.JobDataMap.Put("Function", function);

        var trigger = TriggerBuilder
            .Create()
            .WithDescription(
                $"CRON trigger for {function.FunctionAttribute.Name} ({function.TimerTriggerAttribute.CronExpression})"
            )
            .ForJob(job)
            .StartNow()
            .WithCronSchedule(function.TimerTriggerAttribute.CronExpression)
            .Build();

        _logger?.LogInformation(
            new(
                $"[{function.FunctionAttribute.Name}] started on {trigger.StartTimeUtc} with cron expression '{function.TimerTriggerAttribute.CronExpression}'"
            )
        );

        var scheduler = await _schedulerFactory.GetScheduler(stoppingToken);
        await scheduler.ScheduleJob(job, trigger, stoppingToken);

        if (function.TimerTriggerAttribute.RunOnStartup)
        {
            _logger?.LogInformation(
                "[{name}]: Triggering job, as {property} is true",
                function.FunctionAttribute.Name,
                nameof(TimerTriggerAttribute.RunOnStartup)
            );
            await scheduler.TriggerJob(job.Key, stoppingToken);
        }
    }
}
