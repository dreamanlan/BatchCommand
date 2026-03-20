using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// OpenAI-compatible provider (DeepSeek, Qwen, GLM, MiniMax V2, etc.)
    /// </summary>
    internal class OpenAiProvider : ILlmProvider
    {
        private const int c_maxHistoryMessages = 64;
        private readonly string _baseUrl;
        private readonly string _apiKeyTemplate;
        private readonly Func<string, string> _apiKeyResolver;
        private readonly string _model;
        private readonly int _maxRetries;
        private readonly ConcurrentDictionary<string, List<object>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private readonly ConcurrentDictionary<string, int> _sendCounts = new();
        private bool _keepSession = false;
        private int _contextRounds = 12;
        // gateway mode: "" = standard OpenAI, "boxai" = boxai gateway variant
        private string _gatewayMode = "";
        private string _anydevHost = "";
        private string _sessionMode = "";
        private string _requestPath = "";
        private readonly ConcurrentDictionary<string, string> _gatewaySessionIds = new();
        private int _timeoutSeconds = 300;
        private static readonly HttpClient s_http = CreateHttpClient();

        public OpenAiProvider(string baseUrl, string apiKeyTemplate, string model, Func<string, string> apiKeyResolver, int maxRetries = 3)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKeyTemplate = apiKeyTemplate;
            _apiKeyResolver = apiKeyResolver;
            _model = model;
            _maxRetries = maxRetries;
        }

        public void SetOption(string key, string value)
        {
            if (key == "keep_session") _keepSession = value == "true" || value == "1";
            else if (key == "context_rounds" && int.TryParse(value, out var cr) && cr > 0) _contextRounds = cr;
            else if (key == "gateway_mode") _gatewayMode = value ?? "";
            else if (key == "anydev_host") _anydevHost = value ?? "";
            else if (key == "session_mode") _sessionMode = value ?? "";
            else if (key == "request_path") _requestPath = value ?? "";
            else if (key == "timeout" && int.TryParse(value, out var ts) && ts > 0) _timeoutSeconds = ts;
        }
        public void SetSystemPrompt(string tag, string prompt)
        {
            _systemPrompts[tag] = prompt;
            _sendCounts[tag] = 0;
        }
        public bool IsBusy(string tag) => _busySessions.TryGetValue(tag, out var b) && b;

        public void ClearHistory(string tag)
        {
            _sessions.TryRemove(tag, out _);
            _sendCounts.TryRemove(tag, out _);
            _gatewaySessionIds.TryRemove(tag, out _);
        }

        public async Task<string> ChatAsync(string tag, string topic, string message)
        {
            var messages = _sessions.GetOrAdd(tag, _ => new List<object>());
            lock (messages) { messages.Add(new { role = "user", content = message }); }

            List<object> snapshot;
            lock (messages)
            {
                if (_keepSession)
                {
                    // server session mode: only send current message
                    snapshot = new List<object> { new { role = "user", content = message } };
                    // inject system prompt every _contextRounds sends
                    int count = _sendCounts.GetOrAdd(tag, _ => 0);
                    if (count % _contextRounds == 0 &&
                        _systemPrompts.TryGetValue(tag, out var sp1) && !string.IsNullOrEmpty(sp1))
                    {
                        snapshot.Insert(0, new { role = "system", content = sp1 });
                    }
                    _sendCounts[tag] = count + 1;
                }
                else
                {
                    // local history mode: send full history + always prepend system prompt
                    snapshot = new List<object>(messages);
                    if (_systemPrompts.TryGetValue(tag, out var sp2) && !string.IsNullOrEmpty(sp2))
                        snapshot.Insert(0, new { role = "system", content = sp2 });
                }
            }

            string json;
            if (_gatewayMode == "boxai")
                json = BuildBoxAiRequestJson(tag, snapshot);
            else
                json = System.Text.Json.JsonSerializer.Serialize(new { model = _model, messages = snapshot, stream = false });

            string reply = await HttpRetryHelper.RetryAsync(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
                string resolvedKey = _apiKeyResolver(_apiKeyTemplate);
                string path = !string.IsNullOrEmpty(_requestPath) ? _requestPath : "/chat/completions";
                using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}{path}");
                req.Headers.Add("Authorization", $"Bearer {resolvedKey}");
                if (_gatewayMode == "boxai")
                    AddBoxAiHeaders(req, tag);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await s_http.SendAsync(req, cts.Token);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(resp);
                // capture session id from response header
                if (_gatewayMode == "boxai" && _keepSession &&
                    resp.Headers.TryGetValues("X-Session-Id", out var sidValues))
                {
                    foreach (var sid in sidValues)
                    {
                        if (!string.IsNullOrEmpty(sid))
                            _gatewaySessionIds[tag] = sid;
                    }
                }
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

        /// <summary>Build boxai gateway request JSON (OpenAI-compatible with optional model_config).</summary>
        private string BuildBoxAiRequestJson(string tag, List<object> messages)
        {
            // boxai gateway uses standard OpenAI message format but model is optional
            if (!string.IsNullOrEmpty(_model))
                return System.Text.Json.JsonSerializer.Serialize(new { model = _model, messages, stream = false });
            return System.Text.Json.JsonSerializer.Serialize(new { messages, stream = false });
        }

        /// <summary>Add boxai-specific HTTP headers to the request.</summary>
        private void AddBoxAiHeaders(HttpRequestMessage req, string tag)
        {
            if (!string.IsNullOrEmpty(_anydevHost))
                req.Headers.TryAddWithoutValidation("X-Anydev-Host", _anydevHost);
            if (!string.IsNullOrEmpty(_sessionMode))
                req.Headers.TryAddWithoutValidation("X-Session-Mode", _sessionMode);
            if (_keepSession && _gatewaySessionIds.TryGetValue(tag, out var sid) && !string.IsNullOrEmpty(sid))
                req.Headers.TryAddWithoutValidation("X-Session-Id", sid);
        }
    }
}
