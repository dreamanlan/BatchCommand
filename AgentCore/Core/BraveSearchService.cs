using System;
using System.Collections.Generic;
using System.Text;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Service for Brave Search API integration.
    /// Uses HttpClientOperations for HTTP calls.
    /// </summary>
    public class BraveSearchService
    {
        private const string SearchEndpoint = "https://api.search.brave.com/res/v1/web/search";
        private const int DefaultCount = 5;
        private const int MaxCount = 20;

        private string _apiKeyTemplate = string.Empty;
        private readonly HttpClientOperations _httpClient;

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKeyTemplate);

        public BraveSearchService(HttpClientOperations httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Set the Brave Search API Key template (may contain %var% placeholders).
        /// Actual value is resolved at search time.
        /// </summary>
        public string SetApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return "error: api key is empty";
            _apiKeyTemplate = apiKey.Trim();
            return "ok";
        }

        /// <summary>
        /// Perform a web search via Brave Search API.
        /// Returns formatted text results suitable for LLM consumption.
        /// </summary>
        public string Search(string query, int count = DefaultCount)
        {
            if (!IsConfigured)
                return "[error] Brave Search API key not set. Use brave_set_api_key() first.";
            if (string.IsNullOrWhiteSpace(query))
                return "[error] Search query is empty.";

            count = Math.Max(1, Math.Min(count, MaxCount));

            try {
                // Resolve apiKey at request time; plaintext exists only during this call
                string resolvedKey = AgentCore.Instance.ResolveEnvironmentValue("search", "brave", _apiKeyTemplate);
                string encodedQuery = Uri.EscapeDataString(query);
                string url = $"{SearchEndpoint}?q={encodedQuery}&count={count}";

                var headers = new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Accept-Encoding", "identity" },
                    { "X-Subscription-Token", resolvedKey }
                };

                string json = _httpClient.Get(url, headers);
                return FormatResults(json, query);
            }
            catch (Exception ex) {
                return $"[error] Brave search failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Perform a web search and return raw JSON response.
        /// </summary>
        public string SearchRaw(string query, int count = DefaultCount)
        {
            if (!IsConfigured)
                return "[error] Brave Search API key not set. Use brave_set_api_key() first.";
            if (string.IsNullOrWhiteSpace(query))
                return "[error] Search query is empty.";

            count = Math.Max(1, Math.Min(count, MaxCount));

            try {
                // Resolve apiKey at request time; plaintext exists only during this call
                string resolvedKey = AgentCore.Instance.ResolveEnvironmentValue("search", "brave", _apiKeyTemplate);
                string encodedQuery = Uri.EscapeDataString(query);
                string url = $"{SearchEndpoint}?q={encodedQuery}&count={count}";

                var headers = new Dictionary<string, string> {
                    { "Accept", "application/json" },
                    { "Accept-Encoding", "identity" },
                    { "X-Subscription-Token", resolvedKey }
                };

                return _httpClient.Get(url, headers);
            }
            catch (Exception ex) {
                return $"[error] Brave search failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Parse Brave Search JSON response and format as LLM-friendly text.
        /// Uses simple JSON parsing to avoid external dependencies.
        /// </summary>
        private string FormatResults(string json, string query)
        {
            try {
                // Use the project's existing JsonHelper to parse
                object? parsed = Utils.JsonHelper.FromJson(json);
                if (parsed == null)
                    return "[error] Failed to parse search response.";

                var root = parsed as Dictionary<string, object?>;
                if (root == null)
                    return "[error] Unexpected response format.";

                var sb = new StringBuilder();
                sb.AppendLine($"Search results for: {query}");
                sb.AppendLine(new string('-', 40));

                // Extract web results
                if (root.TryGetValue("web", out var webObj) && webObj is Dictionary<string, object?> web) {
                    if (web.TryGetValue("results", out var resultsObj) && resultsObj is List<object?> results) {
                        int idx = 1;
                        foreach (var item in results) {
                            if (item is Dictionary<string, object?> r) {
                                string title = GetStringValue(r, "title");
                                string url = GetStringValue(r, "url");
                                string description = GetStringValue(r, "description");

                                sb.AppendLine($"[{idx}] {title}");
                                sb.AppendLine($"    URL: {url}");
                                if (!string.IsNullOrEmpty(description))
                                    sb.AppendLine($"    {description}");
                                sb.AppendLine();
                                idx++;
                            }
                        }
                        if (idx == 1)
                            sb.AppendLine("No results found.");
                    }
                    else {
                        sb.AppendLine("No web results in response.");
                    }
                }
                else {
                    sb.AppendLine("No web results in response.");
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
