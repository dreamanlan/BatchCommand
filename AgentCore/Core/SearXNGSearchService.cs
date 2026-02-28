using System;
using System.Collections.Generic;
using System.Text;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Service for SearXNG search integration.
    /// Uses HttpClientOperations for HTTP calls.
    /// </summary>
    public class SearXNGSearchService
    {
        private const int DefaultCount = 5;
        private const int MaxCount = 20;

        private string _instanceUrl = string.Empty;
        private readonly HttpClientOperations _httpClient;

        public bool IsConfigured => !string.IsNullOrEmpty(_instanceUrl);

        public SearXNGSearchService(HttpClientOperations httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Set the SearXNG instance URL (e.g. http://localhost:8080).
        /// </summary>
        public string SetUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "error: url is empty";
            _instanceUrl = url.TrimEnd('/');
            return "ok";
        }

        /// <summary>
        /// Perform a web search via SearXNG.
        /// Returns formatted text results suitable for LLM consumption.
        /// </summary>
        public string Search(string query, int count = DefaultCount)
        {
            if (!IsConfigured)
                return "[error] SearXNG instance URL not set. Use searxng_set_url() first.";
            if (string.IsNullOrWhiteSpace(query))
                return "[error] Search query is empty.";

            count = Math.Max(1, Math.Min(count, MaxCount));

            try {
                string encodedQuery = Uri.EscapeDataString(query);
                string url = $"{_instanceUrl}/search?q={encodedQuery}&format=json";

                var headers = new Dictionary<string, string> {
                    { "Accept", "application/json" }
                };

                string json = _httpClient.Get(url, headers);
                return FormatResults(json, query, count);
            }
            catch (Exception ex) {
                return $"[error] SearXNG search failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Perform a web search and return raw JSON response.
        /// </summary>
        public string SearchRaw(string query, int count = DefaultCount)
        {
            if (!IsConfigured)
                return "[error] SearXNG instance URL not set. Use searxng_set_url() first.";
            if (string.IsNullOrWhiteSpace(query))
                return "[error] Search query is empty.";

            count = Math.Max(1, Math.Min(count, MaxCount));

            try {
                string encodedQuery = Uri.EscapeDataString(query);
                string url = $"{_instanceUrl}/search?q={encodedQuery}&format=json";

                var headers = new Dictionary<string, string> {
                    { "Accept", "application/json" }
                };

                return _httpClient.Get(url, headers);
            }
            catch (Exception ex) {
                return $"[error] SearXNG search failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Parse SearXNG JSON response and format as LLM-friendly text.
        /// </summary>
        private string FormatResults(string json, string query, int count)
        {
            try {
                object? parsed = Utils.JsonHelper.FromJson(json);
                if (parsed == null)
                    return "[error] Failed to parse search response.";

                var root = parsed as Dictionary<string, object?>;
                if (root == null)
                    return "[error] Unexpected response format.";

                var sb = new StringBuilder();
                sb.AppendLine($"Search results for: {query}");
                sb.AppendLine(new string('-', 40));

                // SearXNG returns results in a top-level "results" array
                if (root.TryGetValue("results", out var resultsObj) && resultsObj is List<object?> results) {
                    int idx = 1;
                    foreach (var item in results) {
                        if (idx > count) break;
                        if (item is Dictionary<string, object?> r) {
                            string title = GetStringValue(r, "title");
                            string url = GetStringValue(r, "url");
                            string content = GetStringValue(r, "content");

                            sb.AppendLine($"[{idx}] {title}");
                            sb.AppendLine($"    URL: {url}");
                            if (!string.IsNullOrEmpty(content))
                                sb.AppendLine($"    {content}");
                            sb.AppendLine();
                            idx++;
                        }
                    }
                    if (idx == 1)
                        sb.AppendLine("No results found.");
                }
                else {
                    sb.AppendLine("No results found.");
                }

                return sb.ToString().TrimEnd();
            }
            catch (Exception ex) {
                return $"[error] Failed to format results: {ex.Message}";
            }
        }

        private static string GetStringValue(Dictionary<string, object?> dict, string key)
        {
            if (dict.TryGetValue(key, out var val) && val != null)
                return val.ToString() ?? string.Empty;
            return string.Empty;
        }
    }
}
