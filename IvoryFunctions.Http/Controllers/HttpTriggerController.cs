using System.Text.Json;
using IvoryFunctions.Helpers;
using IvoryFunctions.Http.Models;
using IvoryFunctions.Http.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IvoryFunctions.Http.Controllers;

[Route("api")]
[ApiController]
public class HttpTriggerController : ControllerBase
{
    private readonly IReadOnlyDictionary<string, HttpTriggeredFunction> _functions;
    private readonly IHttpPathMatcher _pathMatcher;
    private readonly IServiceProvider _serviceProvider;

    public HttpTriggerController(
        IEnumerable<HttpTriggeredFunction> httpFunctions,
        IHttpPathMatcher pathMatcher,
        IServiceProvider serviceProvider
    )
    {
        _pathMatcher = pathMatcher;
        _serviceProvider = serviceProvider;
        _functions = httpFunctions.ToDictionary(
            f => f.HttpTriggerAttribute.Route ?? f.FunctionAttribute.Name
        );
    }
    
    [Route("test")]
    [HttpGet]
    public string Test()
    {
        return "Hello, World!";
    }

    [Route("{*url}")]
    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpPatch]
    [HttpDelete]
    [HttpHead]
    [HttpOptions]
    public async Task<IActionResult> Index(string url)
    {
        var function = GetFunctionByUrl(url);
        if (function is null)
        {
            return NotFound();
        }

        var functionClass = function.Method.DeclaringType is not null
            ? _serviceProvider.GetService(function.Method.DeclaringType)
            : null;
        var args = new object[function.Method.GetParameters().Length];
        try
        {
            var trigger = ResolveTriggerArgument(function.MessageType);
            args[0] = trigger ?? throw new NotSupportedException();
            FunctionArgumentHelper.ResolveFunctionArgumentsFromContext(function, args, HttpContext);

            var result = function.Method.Invoke(functionClass, args);
            var awaitable = result as Task ?? Task.CompletedTask;
            await awaitable;

            if (
                function.Method.ReturnType == typeof(void)
                || (
                    function.Method.ReturnType == typeof(Task)
                    && function.Method.ReturnType.GetGenericArguments().Length == 0
                )
            )
            {
                return Ok();
            }

            return HandleOutputBinding(function, awaitable);
        }
        catch (JsonException e)
        {
            return BadRequest("Invalid JSON parsed to HttpTrigger: " + e.Message);
        }
        catch (NotSupportedException)
        {
            return StatusCode(
                500,
                $"The HttpTrigger type '{function.MessageType}' could not be deserialized from the request body."
            );
        }
    }

    private IActionResult HandleOutputBinding(HttpTriggeredFunction function, Task awaitable)
    {
        var result = awaitable.GetType().GetProperty("Result")!.GetValue(awaitable);
        if (result is HttpResponseData responseData)
        {
            return StatusCode((int)responseData.StatusCode, responseData.Body);
        }

        if (result is IActionResult actionResult)
        {
            return actionResult;
        }

        throw new NotSupportedException(
            $"Return type of type '{result.GetType().Name}' cannot be handled by HttpTrigger"
        );
    }

    private object? ResolveTriggerArgument(Type type)
    {
        if (type == typeof(HttpRequestData))
        {
            return new MvcHttpRequestData(Request);
        }

        if (type == typeof(HttpRequest))
        {
            return Request;
        }

        var obj = JsonSerializer.Deserialize(Request.Body, type);
        return obj;
    }

    private HttpTriggeredFunction? GetFunctionByUrl(string url)
    {
        if (_functions.TryGetValue(url, out var f))
        {
            return f;
        }

        foreach (var (funcPath, function) in _functions)
        {
            if (!_pathMatcher.IsDynamicPath(funcPath))
            {
                continue;
            }

            if (_pathMatcher.IsMatchingPath(url, funcPath))
            {
                return function;
            }
        }

        return null;
    }
}
