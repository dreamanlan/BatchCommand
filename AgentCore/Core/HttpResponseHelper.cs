using System.Net.Http;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Helper to check HTTP response status and throw detailed error info on failure.
    /// Replaces the built-in EnsureSuccessStatusCode() with richer diagnostics.
    /// </summary>
    internal static class HttpResponseHelper
    {
        /// <summary>
        /// Throws HttpRequestException with detailed info (status, URL, body, response headers)
        /// if the response does not indicate success. Safe to call from both sync and async contexts.
        /// </summary>
        public static void EnsureSuccessOrThrowDetailed(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode) return;
            string errBody = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            throw new HttpRequestException(
                $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}" +
                $" | {resp.RequestMessage?.RequestUri}" +
                $" | Body: {errBody}" +
                $" | Headers: {resp.Headers}");
        }

        /// <summary>
        /// Async version of EnsureSuccessOrThrowDetailed.
        /// Preferred in async methods where awaiting is possible.
        /// </summary>
        public static async Task EnsureSuccessOrThrowDetailedAsync(HttpResponseMessage resp)
        {
            if (resp.IsSuccessStatusCode) return;
            string errBody = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}" +
                $" | {resp.RequestMessage?.RequestUri}" +
                $" | Body: {errBody}" +
                $" | Headers: {resp.Headers}");
        }
    }
}
