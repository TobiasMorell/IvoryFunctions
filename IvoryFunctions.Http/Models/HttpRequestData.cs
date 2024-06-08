using System.Collections.Specialized;
using System.Security.Claims;
using System.Web;

namespace IvoryFunctions.Http.Models;

public abstract class HttpRequestData
{
    private NameValueCollection? _query;

    /// <summary>
    /// A <see cref="Stream"/> containing the HTTP body data.
    /// </summary>
    public abstract Stream Body { get; }

    /// <summary>
    /// Gets a <see cref="HttpHeadersCollection"/> containing the request headers.
    /// </summary>
    public abstract HttpHeadersCollection Headers { get; }

    /// <summary>
    /// Gets an <see cref="IReadOnlyCollection{IHttpCookie}"/> containing the request cookies.
    /// </summary>
    public abstract IReadOnlyCollection<IHttpCookie> Cookies { get; }

    /// <summary>
    /// Gets the <see cref="Uri"/> for this request.
    /// </summary>
    public abstract Uri Url { get; }

    /// <summary>
    /// Gets an <see cref="IEnumerable{ClaimsIdentity}"/> containing the request identities.
    /// </summary>
    public abstract IEnumerable<ClaimsIdentity> Identities { get; }

    /// <summary>
    /// Gets the HTTP method for this request.
    /// </summary>
    public abstract string Method { get; }

    /// <summary>
    /// Creates a response for this request.
    /// </summary>
    /// <returns>The response instance.</returns>
    public abstract HttpResponseData CreateResponse();

    /// <summary>
    /// Gets the <see cref="NameValueCollection"/> containing the request query.
    /// </summary>
    public virtual NameValueCollection Query => _query ??= HttpUtility.ParseQueryString(Url.Query);
}
