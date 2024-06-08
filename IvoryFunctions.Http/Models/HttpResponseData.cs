using System.Net;

namespace IvoryFunctions.Http.Models;

public abstract class HttpResponseData
{
    /// <summary>
    /// Gets or sets the status code for the response.
    /// </summary>
    public abstract HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="HttpHeadersCollection"/> containing the response headers
    /// </summary>
    public abstract HttpHeadersCollection Headers { get; set; }

    /// <summary>
    /// Gets or sets the response body stream.
    /// </summary>
    public abstract Stream Body { get; set; }

    // public abstract HttpCookies Cookies { get; }

    /// <summary>
    /// Creates an HTTP response for the provided request.
    /// </summary>
    /// <param name="request">The request for which we need to create a response.</param>
    /// <returns>An <see cref="HttpResponseData"/> that represens the response for the provided request.</returns>
    public static HttpResponseData CreateResponse(HttpRequestData request)
    {
        if (request is null)
        {
            throw new System.ArgumentNullException(nameof(request));
        }

        return request.CreateResponse();
    }
}
