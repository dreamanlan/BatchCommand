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
    /// Ollama provider - streaming NDJSON chat API.
    /// Implements ILlmProvider so it can be managed by LlmClientService.
    /// </summary>
    internal class OllamaProvider : ILlmProvider
    {
        private const int c_maxHistoryMessages = 64;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private readonly ConcurrentDictionary<string, int> _sendCounts = new();
        private int _contextRounds = 16;
        private int _maxContextChars = 128 * 1024; // 128KB default
        private int _timeoutSeconds = 600;
        private static readonly HttpClient s_http = new HttpClient(
            RedirectHandler.Create(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            }))
        { Timeout = TimeSpan.FromMinutes(10) };

        public OllamaProvider(string baseUrl, string model)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _model = model;
        }

        private bool _keepSession = false;

        public void SetOption(string key, string value)
        {
            if (key == "keep_session") _keepSession = value == "true" || value == "1";
            else if (key == "context_rounds" && int.TryParse(value, out var cr) && cr > 0) _contextRounds = cr;
            else if (key == "max_context_chars" && int.TryParse(value, out var mcc) && mcc > 0) _maxContextChars = mcc;
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
            // the LAST user message is replaced by a variant that carries
            // an "images" sibling field (Ollama's native format).
            List<object> outMessages = BuildOutgoingMessages(snapshot, imageUrls);

            var requestBody = new OllamaRequest
            {
                Model = _model,
                Messages = outMessages,
                Stream = true
            };
            string json = System.Text.Json.JsonSerializer.Serialize(requestBody);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            string url = $"{_baseUrl}/api/chat";
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            using var response = await s_http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(response);

            var sb = new StringBuilder();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                // Wrap ReadLineAsync with timeout to prevent indefinite blocking
                var readTask = reader.ReadLineAsync();
                using var delayCts = new CancellationTokenSource();
                var delayTask = Task.Delay(TimeSpan.FromSeconds(120), delayCts.Token);
                var completed = await Task.WhenAny(readTask, delayTask);
                if (completed == readTask)
                    delayCts.Cancel();
                else
                    throw new TimeoutException("OllamaProvider: stream read timed out (120s per line)");
                string? line = await readTask;
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string chunk = ParseChunk(line, out bool done);
                if (chunk.Length > 0)
                    sb.Append(chunk);
                if (done)
                    break;
            }

            // Record assistant reply in session history
            string fullReply = ThinkingFilter.StripThink(sb.ToString());
            lock (messages)
            {
                messages.Add(new ChatMessage("assistant", fullReply));
                if (messages.Count > c_maxHistoryMessages)
                    messages.RemoveRange(0, messages.Count - c_maxHistoryMessages);
            }
            return fullReply;
        }

        /// <summary>
        /// Build the outgoing messages list. Without images this is just the
        /// snapshot itself. With images, the last user message is replaced by
        /// a variant carrying an "images" array of base64-encoded pictures
        /// (Ollama's native format).
        ///
        /// Ollama only accepts raw base64 data (no data-URI prefix, no http
        /// URLs). Data-URI images are decoded to their base64 payload; plain
        /// http(s) URLs are skipped because Ollama cannot fetch them.
        /// </summary>
        private static List<object> BuildOutgoingMessages(List<ChatMessage> snapshot, string[] imageUrls)
        {
            var outMessages = new List<object>(snapshot.Count);
            foreach (var m in snapshot) outMessages.Add(m);
            if (imageUrls == null || imageUrls.Length == 0) return outMessages;

            var base64Images = new List<string>();
            foreach (var url in imageUrls)
            {
                if (string.IsNullOrEmpty(url)) continue;
                if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                {
                    int comma = url.IndexOf(',');
                    if (comma > 0)
                        base64Images.Add(url.Substring(comma + 1));
                }
                // Non-data URLs are skipped: Ollama does not fetch remote images.
            }
            if (base64Images.Count == 0) return outMessages;

            for (int i = outMessages.Count - 1; i >= 0; i--)
            {
                if (outMessages[i] is ChatMessage cm && cm.Role == "user")
                {
                    outMessages[i] = new OllamaMessageWithImages
                    {
                        Role = "user",
                        Content = cm.Content,
                        Images = base64Images
                    };
                    break;
                }
            }
            return outMessages;
        }

        /// <summary>
        /// Parses a single NDJSON line from Ollama streaming response.
        /// Returns the content chunk; sets done=true when stream is finished.
        /// Intentionally ignores message.thinking so reasoning output never
        /// leaks into the final answer returned by the API.
        /// </summary>
        private static string ParseChunk(string line, out bool done)
        {
            done = false;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(line);
                var root = doc.RootElement;
                // Note: 'done' may co-occur with a final 'message.content'
                // payload on the same line. Read content first, then signal
                // completion - otherwise we drop the last text fragment.
                string text = "";
                if (root.TryGetProperty("message", out var msgEl) &&
                    msgEl.TryGetProperty("content", out var contentEl))
                {
                    text = contentEl.GetString() ?? "";
                }
                if (root.TryGetProperty("done", out var doneEl) && doneEl.GetBoolean())
                {
                    done = true;
                }
                return text;
            }
            catch { /* ignore malformed lines */ }
            return "";
        }

        // -----------------------------------------------------------------
        // Wire-format DTOs. Explicit POCOs let System.Text.Json cache the
        // serialization metadata and skip anonymous-type reflection.
        // -----------------------------------------------------------------

        private sealed class OllamaRequest
        {
            [JsonPropertyName("model")]    public string Model { get; set; } = "";
            [JsonPropertyName("messages")] public List<object> Messages { get; set; } = new();
            [JsonPropertyName("stream")]   public bool Stream { get; set; }
        }

        private sealed class OllamaMessageWithImages
        {
            [JsonPropertyName("role")]    public string Role { get; set; } = "";
            [JsonPropertyName("content")] public string Content { get; set; } = "";
            [JsonPropertyName("images")]  public List<string> Images { get; set; } = new();
        }
    }
}
