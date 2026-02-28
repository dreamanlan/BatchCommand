using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// OpenAI-compatible provider (DeepSeek, Qwen, GLM, MiniMax V2, etc.)
    /// </summary>
    internal class OpenAiProvider : ILlmProvider
    {
        private const int c_maxHistoryMessages = 50;
        private readonly string _baseUrl;
        private readonly string _apiKeyTemplate;
        private readonly Func<string, string> _apiKeyResolver;
        private readonly string _model;
        private readonly int _maxRetries;
        private readonly ConcurrentDictionary<string, List<object>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private static readonly HttpClient s_http = CreateHttpClient();

        public OpenAiProvider(string baseUrl, string apiKeyTemplate, string model, Func<string, string> apiKeyResolver, int maxRetries = 3)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKeyTemplate = apiKeyTemplate;
            _apiKeyResolver = apiKeyResolver;
            _model = model;
            _maxRetries = maxRetries;
        }

        public void SetOption(string key, string value) { }
        public void SetSystemPrompt(string tag, string prompt) => _systemPrompts[tag] = prompt;
        public bool IsBusy(string tag) => _busySessions.TryGetValue(tag, out var b) && b;

        public void ClearHistory(string tag) => _sessions.TryRemove(tag, out _);

        public async Task<string> ChatAsync(string tag, string topic, string message)
        {
            var messages = _sessions.GetOrAdd(tag, _ => new List<object>());
            lock (messages) { messages.Add(new { role = "user", content = message }); }

            List<object> snapshot;
            lock (messages)
            {
                snapshot = new List<object>(messages);
                if (_systemPrompts.TryGetValue(tag, out var sysPrompt) && !string.IsNullOrEmpty(sysPrompt))
                    snapshot.Insert(0, new { role = "system", content = sysPrompt });
            }

            var body = new { model = _model, messages = snapshot, stream = false };
            string json = System.Text.Json.JsonSerializer.Serialize(body);

            string reply = await HttpRetryHelper.RetryAsync(async () =>
            {
                // Resolve apiKey at request time; plaintext exists only during this call
                string resolvedKey = _apiKeyResolver(_apiKeyTemplate);
                using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/chat/completions");
                req.Headers.Add("Authorization", $"Bearer {resolvedKey}");
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await s_http.SendAsync(req);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(resp);
                string respJson = await resp.Content.ReadAsStringAsync();
                return ParseOpenAiReply(respJson);
            }, _maxRetries, "OpenAiProvider");

            lock (messages)
            {
                messages.Add(new { role = "assistant", content = reply });
                if (messages.Count > c_maxHistoryMessages)
                    messages.RemoveRange(0, messages.Count - c_maxHistoryMessages);
            }
            return reply;
        }

        private static string ParseOpenAiReply(string json)
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("content", out var content))
                    return content.GetString() ?? "";
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
