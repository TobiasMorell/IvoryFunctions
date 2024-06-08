using IvoryFunctions.Blob.Abstractions;
using IvoryFunctions.Helpers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Quartz;

namespace IvoryFunctions.Blob.Quartz;

internal class BlobScannerJob<TFunctionsClass> : IJob
{
    private readonly TFunctionsClass _functionsClass;
    private readonly IBlobScanner _blobScanner;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ILogger? _logger;

    public BlobScannerJob(
        TFunctionsClass functionsClass,
        IBlobScanner blobScanner,
        ISendEndpointProvider sendEndpointProvider,
        ILoggerFactory? loggerFactory
    )
    {
        _functionsClass = functionsClass;
        _blobScanner = blobScanner;
        _sendEndpointProvider = sendEndpointProvider;
        _logger = loggerFactory?.CreateLogger<BlobScannerJob<TFunctionsClass>>();
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var function = context.MergedJobDataMap.Get("Function") as BlobTriggeredFunction;
        if (function is null)
        {
            throw new InvalidOperationException(
                $"Expected {nameof(BlobTriggeredFunction)} but received {function?.GetType().Name ?? "<null>"}"
            );
        }

        _logger?.LogDebug(
            "[{name}]: Starting blob scan on path '{path}'.",
            function.FunctionAttribute.Name,
            function.BlobTriggerAttribute.BlobPath
        );

        var blobs = await _blobScanner.ScanAsync(
            function.BlobTriggerAttribute.BlobPath,
            context.CancellationToken
        );
        foreach (var blob in blobs)
        {
            _logger?.LogDebug(
                "[{name}]: Processing blob '{blobName}'.",
                function.FunctionAttribute.Name,
                blob.BlobUri
            );

            var args = new object[function.Method.GetParameters().Length];
            args[0] = await ResolveFunctionArgument(function.MessageType, blob);
            FunctionArgumentHelper.ResolveFunctionArgumentsFromContext(function, args, context);

            var functionResult = function.Method.Invoke(_functionsClass, args);
            var awaitable = functionResult as Task ?? Task.CompletedTask;
            await awaitable;

            await TrySendResultToOutputQueue(function, awaitable, context.CancellationToken);
        }

        _logger?.LogDebug(
            "[{name}]: Finished blob scan on path '{path}'.",
            function.FunctionAttribute.Name,
            function.BlobTriggerAttribute.BlobPath
        );
    }

    private async Task<object> ResolveFunctionArgument(
        Type messageType,
        IBlob blob,
        CancellationToken cancellationToken = default
    )
    {
        if (messageType == typeof(IBlob))
        {
            return blob;
        }

        if (messageType == typeof(Stream))
        {
            return await blob.OpenReadAsync(cancellationToken);
        }

        if (messageType == typeof(string))
        {
            return await blob.ReadAsStringAsync(cancellationToken);
        }

        if (messageType == typeof(byte[]))
        {
            return await blob.ReadAsByteArrayAsync(cancellationToken);
        }

        throw new InvalidOperationException($"Unsupported message type: {messageType.Name}");
    }

    private async Task TrySendResultToOutputQueue(
        BlobTriggeredFunction function,
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
