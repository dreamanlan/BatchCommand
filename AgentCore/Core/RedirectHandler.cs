using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// DelegatingHandler that manually follows HTTP redirects (301/302/303/307/308),
    /// including HTTPS -> HTTP downgrades that .NET blocks by default.
    /// Wrap the inner SocketsHttpHandler with this handler and set
    /// AllowAutoRedirect = false on the inner handler.
    /// </summary>
    internal class RedirectHandler : DelegatingHandler
    {
        private const int c_maxRedirects = 5;

        public RedirectHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var currentRequest = request;
            for (int i = 0; i <= c_maxRedirects; i++)
            {
                var response = await base.SendAsync(currentRequest, cancellationToken);
                int statusCode = (int)response.StatusCode;
                if (statusCode < 300 || statusCode > 399)
                    return response;
                var location = response.Headers.Location;
                if (location == null)
                    return response; // no Location header, return as-is
                // Resolve relative URIs
                if (!location.IsAbsoluteUri)
                    location = new Uri(currentRequest.RequestUri!, location);
                if (i == c_maxRedirects)
                    return response; // max redirects reached, return last response
                response.Dispose();
                // 307/308 preserve method and body; 301/302/303 switch to GET
                bool preserveMethod = statusCode == 307 || statusCode == 308;
                var newRequest = new HttpRequestMessage
                {
                    RequestUri = location,
                    Method = preserveMethod ? currentRequest.Method : HttpMethod.Get,
                    Version = currentRequest.Version
                };
                // Copy headers from original request
                foreach (var header in request.Headers)
                    newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                // Copy content for 307/308
                if (preserveMethod && currentRequest.Content != null)
                {
                    // Read content bytes and re-create to allow resend
                    var bytes = await currentRequest.Content.ReadAsByteArrayAsync();
                    var newContent = new ByteArrayContent(bytes);
                    if (currentRequest.Content.Headers.ContentType != null)
                        newContent.Headers.ContentType = currentRequest.Content.Headers.ContentType;
                    newRequest.Content = newContent;
                }
                currentRequest = newRequest;
            }
            // Should not reach here, but just in case
            throw new HttpRequestException("Too many redirects");
        }

        /// <summary>
        /// Create a SocketsHttpHandler with AllowAutoRedirect disabled,
        /// wrapped by RedirectHandler for manual redirect handling.
        /// </summary>
        public static RedirectHandler Create(SocketsHttpHandler inner)
        {
            inner.AllowAutoRedirect = false;
            return new RedirectHandler(inner);
        }
    }
}
