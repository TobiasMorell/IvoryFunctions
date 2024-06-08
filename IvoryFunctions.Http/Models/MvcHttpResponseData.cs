using System.Net;

namespace IvoryFunctions.Http.Models;

internal class MvcHttpResponseData : HttpResponseData
{
    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
}
