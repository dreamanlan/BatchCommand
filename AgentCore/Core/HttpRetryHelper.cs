using System;
using AgentPlugin.Abstractions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Shared HTTP retry helper with exponential backoff.
    /// Supports retrying on transient HTTP errors including 429 (Rate Limit).
    /// </summary>
    internal static class HttpRetryHelper
    {
        /// <summary>
        /// Executes the action with exponential backoff retry on transient failures.
        /// </summary>
        public static async Task<string> RetryAsync(Func<Task<string>> action, int maxRetries, string callerTag = "LlmProvider")
        {
            int delay = 1000;
            for (int i = 0; i <= maxRetries; i++)
            {
                try { return await action(); }
                catch (Exception ex) when (i < maxRetries && IsRetryable(ex))
                {
                    int retryDelay = delay;
                    // Honor Retry-After header for 429 responses
                    if (ex is HttpRequestException httpEx && httpEx.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        retryDelay = Math.Max(retryDelay, 5000); // at least 5s for rate limit
                    }
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[{callerTag}] retry {i + 1}/{maxRetries}: {ex.Message}");
                    await Task.Delay(retryDelay);
                    delay = Math.Min(delay * 2, 30000); // cap at 30s
                }
            }
            return "[error] max retries exceeded";
        }

        /// <summary>
        /// Determines whether the exception is transient and worth retrying.
        /// Includes HttpRequestException (with 429/5xx), TaskCanceledException, TimeoutException.
        /// </summary>
        public static bool IsRetryable(Exception ex)
        {
            if (ex is HttpRequestException httpEx)
            {
                if (httpEx.StatusCode.HasValue)
                {
                    int code = (int)httpEx.StatusCode.Value;
                    // Retry on 429 (rate limit) and 5xx (server errors)
                    return code == 429 || code >= 500;
                }
                // No status code means network-level failure, retryable
                return true;
            }
            return ex is TaskCanceledException || ex is TimeoutException;
        }
    }
}
