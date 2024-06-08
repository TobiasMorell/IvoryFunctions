using IvoryFunctions.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IvoryFunctions.Abstractions;

public class InvokeTimerTriggerJob<TFunctionClass> : IJob
{
    private readonly TFunctionClass _functionClass;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ILogger? _logger;

    public InvokeTimerTriggerJob(
        TFunctionClass functionClass,
        ISendEndpointProvider sendEndpointProvider,
        ILoggerFactory? logger
    )
    {
        _functionClass = functionClass;
        _sendEndpointProvider = sendEndpointProvider;
        _logger = logger?.CreateLogger<InvokeTimerTriggerJob<TFunctionClass>>();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var function = context.MergedJobDataMap.Get("Function") as TimerTriggeredFunction;
        if (function is null)
        {
            throw new InvalidOperationException(
                $"Expected {nameof(TimerTriggeredFunction)} but received {function?.GetType().Name ?? "<null>"}"
            );
        }

        var args = new object[function.Method.GetParameters().Length];
        args[0] = new TimerInfo();
        FunctionArgumentHelper.ResolveFunctionArgumentsFromContext(function, args, context);

        var functionResult = function.Method.Invoke(_functionClass, args);
        var awaitable = functionResult as Task ?? Task.CompletedTask;
        await awaitable;

        await TrySendResultToOutputQueue(function, awaitable, context.CancellationToken);
    }

    private async Task TrySendResultToOutputQueue(
        TimerTriggeredFunction function,
        Task awaitable,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(function.OutputQueueName))
            return;

        var resultProperty = awaitable.GetType().GetProperty("Result");
        var result = resultProperty?.GetValue(awaitable);

        if (result != null)
        {
            var outputEndpoint = await _sendEndpointProvider.GetSendEndpoint(
                new Uri("queue:" + function.OutputQueueName)
            );
            await outputEndpoint.Send(result, cancellationToken);
        }
        else
        {
            _logger?.LogWarning(
                "[{name}]: Function did not return a result. No message will be sent to the output queue: '{outputQueueName}'.",
                function.FunctionAttribute.Name,
                function.OutputQueueName
            );
        }
    }
}
