using System.Text;
using System.Text.Json;
using IvoryFunctions.Http.Models;

namespace IvoryFunctions.Http.Extensions;

public static class HttpResponseDataExtensions
{
    public static Task WriteAsync(
        this HttpResponseData response,
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var buffer = Encoding.UTF8.GetBytes(content);
        return response.Body.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
    }

    public static Task WriteAsync(
        this HttpResponseData response,
        byte[] content,
        CancellationToken cancellationToken = default
    )
    {
        return response.Body.WriteAsync(content, 0, content.Length, cancellationToken);
    }

    public static Task WriteAsync(
        this HttpResponseData response,
        Stream content,
        CancellationToken cancellationToken = default
    )
    {
        return content.CopyToAsync(response.Body, cancellationToken);
    }

    public static Task WriteAsJsonAsync<T>(
        this HttpResponseData response,
        T content,
        CancellationToken cancellationToken = default
    )
    {
        var json = JsonSerializer.Serialize(content);
        return response.WriteAsync(json, cancellationToken);
    }
}
