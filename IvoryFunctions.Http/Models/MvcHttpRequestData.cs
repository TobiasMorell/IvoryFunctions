using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IvoryFunctions.Http.Models;

internal class MvcHttpRequestData : HttpRequestData
{
    private readonly HttpRequest _http;

    public MvcHttpRequestData(HttpRequest http)
    {
        _http = http;
    }

    public override Stream Body => _http.Body;
    public override HttpHeadersCollection Headers => new HttpHeadersCollection(_http.Headers);
    public override IReadOnlyCollection<IHttpCookie> Cookies => throw new NotImplementedException(); // TODO: Implement this!
    public override Uri Url => new Uri(_http.Path);
    public override IEnumerable<ClaimsIdentity> Identities => _http.HttpContext.User.Identities;
    public override string Method => _http.Method;

    public override HttpResponseData CreateResponse() => new MvcHttpResponseData();
}
