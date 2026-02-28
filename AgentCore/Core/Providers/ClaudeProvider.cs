using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Claude provider (Anthropic API)
    /// </summary>
    internal class ClaudeProvider : ILlmProvider
    {
        private const int c_maxHistoryMessages = 50;
        private readonly string _url;
        private readonly string _apiKeyTemplate;
        private readonly Func<string, string> _apiKeyResolver;
        private readonly string _model;
        private int _maxTokens = 4096;
        private readonly int _maxRetries;
        private readonly ConcurrentDictionary<string, List<object>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private const string c_defaultUrl = "https://api.anthropic.com/v1/messages";
        private const string c_apiVersion = "2023-06-01";
        private static readonly HttpClient s_http = CreateHttpClient();

        public ClaudeProvider(string url, string apiKeyTemplate, string model, Func<string, string> apiKeyResolver, int maxRetries = 3)
        {
            _url = string.IsNullOrEmpty(url) ? c_defaultUrl : url.TrimEnd('/');
            _apiKeyTemplate = apiKeyTemplate;
            _apiKeyResolver = apiKeyResolver;
            _model = model;
            _maxRetries = maxRetries;
        }

        public void SetOption(string key, string value)
        {
            if (key == "max_tokens" && int.TryParse(value, out var mt))
                _maxTokens = mt;
        }

        public void SetSystemPrompt(string tag, string prompt) => _systemPrompts[tag] = prompt;

        public bool IsBusy(string tag) => _busySessions.TryGetValue(tag, out var b) && b;

        public void ClearHistory(string tag) => _sessions.TryRemove(tag, out _);

        public async Task<string> ChatAsync(string tag, string topic, string message)
        {
            var messages = _sessions.GetOrAdd(tag, _ => new List<object>());
            lock (messages) { messages.Add(new { role = "user", content = message }); }

            List<object> snapshot;
            lock (messages) { snapshot = new List<object>(messages); }

            string? sysPrompt = _systemPrompts.TryGetValue(tag, out var sp) && !string.IsNullOrEmpty(sp) ? sp : null;
            string json;
            if (sysPrompt != null)
            {
                var body = new { model = _model, max_tokens = _maxTokens, system = sysPrompt, messages = snapshot };
                json = System.Text.Json.JsonSerializer.Serialize(body);
            }
            else
            {
                var body = new { model = _model, max_tokens = _maxTokens, messages = snapshot };
                json = System.Text.Json.JsonSerializer.Serialize(body);
            }

            string reply = await HttpRetryHelper.RetryAsync(async () =>
            {
                // Resolve apiKey at request time; plaintext exists only during this call
                string resolvedKey = _apiKeyResolver(_apiKeyTemplate);
                using var req = new HttpRequestMessage(HttpMethod.Post, _url);
                req.Headers.Add("x-api-key", resolvedKey);
                req.Headers.Add("anthropic-version", c_apiVersion);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await s_http.SendAsync(req);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(resp);
                string respJson = await resp.Content.ReadAsStringAsync();
                return ParseClaudeReply(respJson);
            }, _maxRetries, "ClaudeProvider");

            lock (messages)
            {
                messages.Add(new { role = "assistant", content = reply });
                if (messages.Count > c_maxHistoryMessages)
                    messages.RemoveRange(0, messages.Count - c_maxHistoryMessages);
            }
            return reply;
        }

        private static string ParseClaudeReply(string json)
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("content", out var content) && content.GetArrayLength() > 0)
            {
                var sb = new StringBuilder();
                foreach (var block in content.EnumerateArray())
                {
                    if (block.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                        block.TryGetProperty("text", out var text))
                        sb.Append(text.GetString());
                }
                return sb.ToString();
            }
            if (root.TryGetProperty("error", out var err))
                return $"[error] {err.GetRawText()}";
            return "[error] unexpected response format";
        }

        private static HttpClient CreateHttpClient() => new HttpClient(
            RedirectHandler.Create(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            }))
        { Timeout = TimeSpan.FromMinutes(5) };
    }
}
