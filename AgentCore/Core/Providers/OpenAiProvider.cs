using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
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
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private readonly ConcurrentDictionary<string, int> _sendCounts = new();
        private bool _keepSession = false;
        private int _contextRounds = 16;
        private int _maxContextChars = 128 * 1024; // 128KB default
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
            else if (key == "max_context_chars" && int.TryParse(value, out var mcc) && mcc > 0) _maxContextChars = mcc;
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

        public Task<string> ChatAsync(string tag, string topic, string message)
        {
            return ChatInternalAsync(tag, topic, message, Array.Empty<string>());
        }

        public Task<string> ChatWithImagesAsync(string tag, string topic, string message, string[] imageUrls)
        {
            return ChatInternalAsync(tag, topic, message, imageUrls ?? Array.Empty<string>());
        }

        private async Task<string> ChatInternalAsync(string tag, string topic, string message, string[] imageUrls)
        {
            var messages = _sessions.GetOrAdd(tag, _ => new List<ChatMessage>());
            lock (messages) { messages.Add(new ChatMessage("user", message)); }

            List<ChatMessage> snapshot;
            lock (messages)
            {
                if (_keepSession)
                {
                    // server session mode: only send current message
                    snapshot = new List<ChatMessage> { new ChatMessage("user", message) };
                    // inject system prompt every _contextRounds sends
                    int count = _sendCounts.GetOrAdd(tag, _ => 0);
                    if (count % _contextRounds == 0 &&
                        _systemPrompts.TryGetValue(tag, out var sp1) && !string.IsNullOrEmpty(sp1))
                    {
                        snapshot.Insert(0, new ChatMessage("system", sp1));
                    }
                    _sendCounts[tag] = count + 1;
                }
                else
                {
                    // local history mode: send full history + always prepend system prompt
                    snapshot = new List<ChatMessage>(messages);
                    // Apply maxContextChars limit on history (keep most recent messages that fit)
                    if (_maxContextChars > 0 && snapshot.Count > 1)
                    {
                        int totalChars = 0;
                        int startFrom = 0;
                        for (int i = snapshot.Count - 2; i >= 0; i--) // skip last (current) message
                        {
                            totalChars += snapshot[i].Content.Length;
                            if (totalChars > _maxContextChars)
                            {
                                startFrom = i + 1;
                                break;
                            }
                        }
                        if (startFrom > 0)
                            snapshot.RemoveRange(0, startFrom);
                    }
                    if (_systemPrompts.TryGetValue(tag, out var sp2) && !string.IsNullOrEmpty(sp2))
                        snapshot.Insert(0, new ChatMessage("system", sp2));
                }
            }

            // Build the outbound messages list. When image URLs are attached,
            // the LAST user message is replaced by a multipart-content variant
            // (text part + image_url parts). All other messages keep the
            // plain-text ChatMessage shape.
            List<object> outMessages = BuildOutgoingMessages(snapshot, imageUrls);

            string json;
            if (_gatewayMode == "boxai")
                json = BuildBoxAiRequestJson(outMessages);
            else
                json = System.Text.Json.JsonSerializer.Serialize(new OpenAiRequest
                {
                    Model = _model,
                    Messages = outMessages,
                    Stream = false
                });

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
                messages.Add(new ChatMessage("assistant", reply));
                if (messages.Count > c_maxHistoryMessages)
                    messages.RemoveRange(0, messages.Count - c_maxHistoryMessages);
            }
            return reply;
        }

        /// <summary>
        /// Build the outgoing messages list. Without images, this is just
        /// the snapshot itself (each element is a ChatMessage). With images,
        /// the last user message is swapped for a multi-part variant so the
        /// model can see the attached pictures.
        /// </summary>
        private static List<object> BuildOutgoingMessages(List<ChatMessage> snapshot, string[] imageUrls)
        {
            var outMessages = new List<object>(snapshot.Count);
            foreach (var m in snapshot) outMessages.Add(m);
            if (imageUrls == null || imageUrls.Length == 0) return outMessages;

            // Find the last user message and replace it with a parts-array
            // variant that carries the images.
            for (int i = outMessages.Count - 1; i >= 0; i--)
            {
                if (outMessages[i] is ChatMessage cm && cm.Role == "user")
                {
                    var parts = new List<object>();
                    parts.Add(new OpenAiTextPart { Text = cm.Content });
                    foreach (var url in imageUrls)
                    {
                        if (string.IsNullOrEmpty(url)) continue;
                        parts.Add(new OpenAiImagePart
                        {
                            ImageUrl = new OpenAiImageUrl { Url = url }
                        });
                    }
                    outMessages[i] = new OpenAiMessageWithParts { Role = "user", Content = parts };
                    break;
                }
            }
            return outMessages;
        }

        private static string ParseOpenAiReply(string json)
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg))
                {
                    // Intentionally ignore reasoning fields used by reasoning
                    // models (DeepSeek-R1: reasoning_content; some Ollama
                    // OpenAI-compat builds and OpenAI o-series: reasoning).
                    // The API consumer only wants the user-facing answer.
                    string content = msg.TryGetProperty("content", out var c) ? (c.GetString() ?? "") : "";
                    // Some models still embed <think>...</think> directly in
                    // the content field. Strip them defensively.
                    return ThinkingFilter.StripThink(content);
                }
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
        private string BuildBoxAiRequestJson(List<object> messages)
        {
            // boxai gateway uses standard OpenAI message format but model is optional
            if (!string.IsNullOrEmpty(_model))
                return System.Text.Json.JsonSerializer.Serialize(new OpenAiRequest
                {
                    Model = _model,
                    Messages = messages,
                    Stream = false
                });
            return System.Text.Json.JsonSerializer.Serialize(new OpenAiRequestNoModel
            {
                Messages = messages,
                Stream = false
            });
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

        // -----------------------------------------------------------------
        // Wire-format DTOs. Explicit POCOs let System.Text.Json cache the
        // serialization metadata and skip the anonymous-type reflection cost.
        // -----------------------------------------------------------------

        private sealed class OpenAiRequest
        {
            [JsonPropertyName("model")]    public string Model { get; set; } = "";
            [JsonPropertyName("messages")] public List<object> Messages { get; set; } = new();
            [JsonPropertyName("stream")]   public bool Stream { get; set; }
        }

        private sealed class OpenAiRequestNoModel
        {
            [JsonPropertyName("messages")] public List<object> Messages { get; set; } = new();
            [JsonPropertyName("stream")]   public bool Stream { get; set; }
        }

        private sealed class OpenAiMessageWithParts
        {
            [JsonPropertyName("role")]    public string Role { get; set; } = "";
            [JsonPropertyName("content")] public List<object> Content { get; set; } = new();
        }

        private sealed class OpenAiTextPart
        {
            [JsonPropertyName("type")] public string Type { get; set; } = "text";
            [JsonPropertyName("text")] public string Text { get; set; } = "";
        }

        private sealed class OpenAiImagePart
        {
            [JsonPropertyName("type")]      public string Type { get; set; } = "image_url";
            [JsonPropertyName("image_url")] public OpenAiImageUrl ImageUrl { get; set; } = new();
        }

        private sealed class OpenAiImageUrl
        {
            [JsonPropertyName("url")] public string Url { get; set; } = "";
        }
    }
}
