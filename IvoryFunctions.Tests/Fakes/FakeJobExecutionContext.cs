using Quartz;

namespace Functions.MassTransit.Tests.Fakes;

public class FakeJobExecutionContext : IJobExecutionContext
{
    private readonly IDictionary<string, object> _jobData = new Dictionary<string, object>();

    public void Put(object key, object objectValue)
    {
        _jobData[key.ToString()] = objectValue;
    }

    public object? Get(object key)
    {
        return _jobData[key.ToString()];
    }

    public IScheduler Scheduler { get; }
    public ITrigger Trigger { get; }
    public ICalendar? Calendar { get; }
    public bool Recovering { get; }
    public TriggerKey RecoveringTriggerKey { get; }
    public int RefireCount { get; }
    public JobDataMap MergedJobDataMap => new JobDataMap(_jobData);
    public IJobDetail JobDetail { get; }
    public IJob JobInstance { get; }
    public DateTimeOffset FireTimeUtc { get; init; }
    public DateTimeOffset? ScheduledFireTimeUtc { get; }
    public DateTimeOffset? PreviousFireTimeUtc { get; }
    public DateTimeOffset? NextFireTimeUtc { get; }
    public string FireInstanceId { get; }
    public object? Result { get; set; }
    public TimeSpan JobRunTime { get; }
    public CancellationToken CancellationToken { get; }
}
