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
    /// Claude provider (Anthropic API)
    /// </summary>
    internal class ClaudeProvider : ILlmProvider
    {
        private const int c_maxHistoryMessages = 64;
        private readonly string _url;
        private readonly string _apiKeyTemplate;
        private readonly Func<string, string> _apiKeyResolver;
        private readonly string _model;
        private int _maxTokens = 4096;
        private readonly int _maxRetries;
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private readonly ConcurrentDictionary<string, int> _sendCounts = new();
        private const string c_defaultUrl = "https://api.anthropic.com/v1/messages";
        private const string c_apiVersion = "2023-06-01";
        private int _timeoutSeconds = 300;
        private static readonly HttpClient s_http = CreateHttpClient();

        public ClaudeProvider(string url, string apiKeyTemplate, string model, Func<string, string> apiKeyResolver, int maxRetries = 3)
        {
            _url = string.IsNullOrEmpty(url) ? c_defaultUrl : url.TrimEnd('/');
            _apiKeyTemplate = apiKeyTemplate;
            _apiKeyResolver = apiKeyResolver;
            _model = model;
            _maxRetries = maxRetries;
        }

        private bool _keepSession = false;

        private int _contextRounds = 16;
        private int _maxContextChars = 128 * 1024; // 128KB default

        public void SetOption(string key, string value)
        {
            if (key == "max_tokens" && int.TryParse(value, out var mt))
                _maxTokens = mt;
            else if (key == "keep_session")
                _keepSession = value == "true" || value == "1";
            else if (key == "context_rounds" && int.TryParse(value, out var cr) && cr > 0)
                _contextRounds = cr;
            else if (key == "max_context_chars" && int.TryParse(value, out var mcc) && mcc > 0)
                _maxContextChars = mcc;
            else if (key == "timeout" && int.TryParse(value, out var ts) && ts > 0)
                _timeoutSeconds = ts;
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
            string? sysPrompt;
            lock (messages)
            {
                if (_keepSession)
                {
                    // server session mode: only send current message
                    snapshot = new List<ChatMessage> { new ChatMessage("user", message) };
                    // inject system prompt every _contextRounds sends
                    int count = _sendCounts.GetOrAdd(tag, _ => 0);
                    sysPrompt = (count % _contextRounds == 0 &&
                        _systemPrompts.TryGetValue(tag, out var sp1) && !string.IsNullOrEmpty(sp1)) ? sp1 : null;
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
                        for (int i = snapshot.Count - 2; i >= 0; i--)
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
                    sysPrompt = _systemPrompts.TryGetValue(tag, out var sp2) && !string.IsNullOrEmpty(sp2) ? sp2 : null;
                }
            }

            // Build the outbound messages list. When image URLs are attached,
            // the LAST user message is replaced by a content-blocks variant
            // (text block + image blocks). All other messages keep the plain
            // ChatMessage shape.
            List<object> outMessages = BuildOutgoingMessages(snapshot, imageUrls);

            string json;
            if (sysPrompt != null)
            {
                json = System.Text.Json.JsonSerializer.Serialize(new ClaudeRequestWithSystem
                {
                    Model = _model,
                    MaxTokens = _maxTokens,
                    System = sysPrompt,
                    Messages = outMessages
                });
            }
            else
            {
                json = System.Text.Json.JsonSerializer.Serialize(new ClaudeRequest
                {
                    Model = _model,
                    MaxTokens = _maxTokens,
                    Messages = outMessages
                });
            }

            string reply = await HttpRetryHelper.RetryAsync(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
                // Resolve apiKey at request time; plaintext exists only during this call
                string resolvedKey = _apiKeyResolver(_apiKeyTemplate);
                using var req = new HttpRequestMessage(HttpMethod.Post, _url);
                req.Headers.Add("x-api-key", resolvedKey);
                req.Headers.Add("anthropic-version", c_apiVersion);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await s_http.SendAsync(req, cts.Token);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(resp);
                string respJson = await resp.Content.ReadAsStringAsync();
                return ParseClaudeReply(respJson);
            }, _maxRetries, "ClaudeProvider");

            lock (messages)
            {
                messages.Add(new ChatMessage("assistant", reply));
                if (messages.Count > c_maxHistoryMessages)
                    messages.RemoveRange(0, messages.Count - c_maxHistoryMessages);
            }
            return reply;
        }

        /// <summary>
        /// Build the outgoing messages list. Without images this is just the
        /// snapshot itself. With images, the last user message is replaced
        /// by a content-blocks variant (text + image blocks) so Claude can
        /// see the pictures.
        /// Supported URL forms:
        ///   * "data:image/xxx;base64,YYYY" - decoded and sent via source.type=base64
        ///   * "http(s)://..."              - sent via source.type=url
        /// </summary>
        private static List<object> BuildOutgoingMessages(List<ChatMessage> snapshot, string[] imageUrls)
        {
            var outMessages = new List<object>(snapshot.Count);
            foreach (var m in snapshot) outMessages.Add(m);
            if (imageUrls == null || imageUrls.Length == 0) return outMessages;

            for (int i = outMessages.Count - 1; i >= 0; i--)
            {
                if (outMessages[i] is ChatMessage cm && cm.Role == "user")
                {
                    var blocks = new List<object>();
                    blocks.Add(new ClaudeTextBlock { Text = cm.Content });
                    foreach (var url in imageUrls)
                    {
                        if (string.IsNullOrEmpty(url)) continue;
                        var block = BuildImageBlock(url);
                        if (block != null) blocks.Add(block);
                    }
                    outMessages[i] = new ClaudeMessageWithBlocks { Role = "user", Content = blocks };
                    break;
                }
            }
            return outMessages;
        }

        /// <summary>
        /// Build a single Claude image block from a URL string. Returns null
        /// when the input cannot be classified (empty, malformed data URL).
        /// </summary>
        private static ClaudeImageBlock? BuildImageBlock(string url)
        {
            // data:image/xxx;base64,YYYY
            if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                int semi = url.IndexOf(';');
                int comma = url.IndexOf(',');
                if (semi <= 5 || comma <= semi) return null;
                string mediaType = url.Substring(5, semi - 5); // e.g. "image/png"
                string data = url.Substring(comma + 1);
                return new ClaudeImageBlock
                {
                    Source = new ClaudeImageSource
                    {
                        Type = "base64",
                        MediaType = mediaType,
                        Data = data
                    }
                };
            }
            // Regular URL (http/https). Claude accepts source.type="url".
            return new ClaudeImageBlock
            {
                Source = new ClaudeImageSource
                {
                    Type = "url",
                    Url = url
                }
            };
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
                    // Only collect plain text blocks. Anthropic extended-thinking
                    // emits type=="thinking" blocks which we deliberately skip
                    // so reasoning output never reaches the API consumer.
                    if (block.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                        block.TryGetProperty("text", out var text))
                        sb.Append(text.GetString());
                }
                // Defensive: strip any in-band <think>/<thinking> tags
                // that might have been produced by an upstream proxy.
                return ThinkingFilter.StripThink(sb.ToString());
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

        // -----------------------------------------------------------------
        // Wire-format DTOs. Explicit POCOs let System.Text.Json cache the
        // serialization metadata and skip anonymous-type reflection.
        // -----------------------------------------------------------------

        private sealed class ClaudeRequest
        {
            [JsonPropertyName("model")]      public string Model { get; set; } = "";
            [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
            [JsonPropertyName("messages")]   public List<object> Messages { get; set; } = new();
        }

        private sealed class ClaudeRequestWithSystem
        {
            [JsonPropertyName("model")]      public string Model { get; set; } = "";
            [JsonPropertyName("max_tokens")] public int MaxTokens { get; set; }
            [JsonPropertyName("system")]     public string System { get; set; } = "";
            [JsonPropertyName("messages")]   public List<object> Messages { get; set; } = new();
        }

        private sealed class ClaudeMessageWithBlocks
        {
            [JsonPropertyName("role")]    public string Role { get; set; } = "";
            [JsonPropertyName("content")] public List<object> Content { get; set; } = new();
        }

        private sealed class ClaudeTextBlock
        {
            [JsonPropertyName("type")] public string Type { get; set; } = "text";
            [JsonPropertyName("text")] public string Text { get; set; } = "";
        }

        private sealed class ClaudeImageBlock
        {
            [JsonPropertyName("type")]   public string Type { get; set; } = "image";
            [JsonPropertyName("source")] public ClaudeImageSource Source { get; set; } = new();
        }

        private sealed class ClaudeImageSource
        {
            [JsonPropertyName("type")]       public string Type { get; set; } = "";
            [JsonPropertyName("media_type"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? MediaType { get; set; }
            [JsonPropertyName("data"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Data { get; set; }
            [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Url { get; set; }
        }
    }
}
