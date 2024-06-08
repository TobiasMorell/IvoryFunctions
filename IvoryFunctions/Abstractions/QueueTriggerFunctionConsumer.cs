using IvoryFunctions.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace IvoryFunctions.Abstractions;

internal class QueueTriggerFunctionConsumer<TFunctionClass, TMessage> : IConsumer<TMessage>
    where TMessage : class
{
    private readonly TFunctionClass _functionClass;
    private readonly QueueTriggeredFunction _function;
    private readonly ILogger? _logger;

    public QueueTriggerFunctionConsumer(
        QueueTriggeredFunction function,
        TFunctionClass functionClass,
        ILoggerFactory? loggerFactory
    )
    {
        _function = function;
        _functionClass = functionClass;
        _logger = loggerFactory?.CreateLogger<
            QueueTriggerFunctionConsumer<TFunctionClass, TMessage>
        >();
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        if (_function.MessageType != typeof(TMessage))
        {
            throw new InvalidOperationException(
                $"[{_function.FunctionAttribute.Name}: Expected message type of {_function.MessageType}, but received {typeof(TMessage)}"
            );
        }

        var args = new object[_function.Method.GetParameters().Length];
        args[0] = context.Message;
        FunctionArgumentHelper.ResolveFunctionArgumentsFromContext(_function, args, context);

        try
        {
            var functionResult = _function.Method.Invoke(_functionClass, args);
            var awaitable = functionResult as Task ?? Task.CompletedTask;
            await awaitable;

            await TrySendResultToOutputQueue(context, awaitable);

            _logger?.LogInformation(
                "[{name}]: Function invoked successfully",
                _function.FunctionAttribute.Name
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "[{name}]: Error invoking function. Remaining retries: {retries}",
                _function.FunctionAttribute.Name,
                _function.FunctionAttribute.Retries - context.GetRetryAttempt()
            );

            throw;
        }
    }

    private async Task TrySendResultToOutputQueue(ConsumeContext<TMessage> context, Task awaitable)
    {
        if (string.IsNullOrEmpty(_function.OutputQueueName))
            return;

        var resultProperty = awaitable.GetType().GetProperty("Result");
        var result = resultProperty?.GetValue(awaitable);

        if (result != null)
        {
            var outputEndpoint = await context.GetSendEndpoint(
                new Uri("queue:" + _function.OutputQueueName)
            );
            await outputEndpoint.Send(result, context.CancellationToken);
        }
        else
        {
            _logger?.LogWarning(
                "[{name}]: Function did not return a result. No message will be sent to the output queue: '{outputQueueName}'.",
                _function.FunctionAttribute.Name,
                _function.OutputQueueName
            );
        }
    }
}
