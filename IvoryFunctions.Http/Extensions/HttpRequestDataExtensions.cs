using System.Net;
using IvoryFunctions.Http.Models;

namespace IvoryFunctions.Http.Extensions;

public static class HttpRequestDataExtensions
{
    public static HttpResponseData CreateResponse(
        this HttpRequestData request,
        HttpStatusCode statusCode
    )
    {
        var resp = request.CreateResponse();
        resp.StatusCode = statusCode;

        return resp;
    }
}
