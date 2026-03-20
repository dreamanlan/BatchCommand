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
    /// Ollama provider - streaming NDJSON chat API.
    /// Implements ILlmProvider so it can be managed by LlmClientService.
    /// </summary>
    internal class OllamaProvider : ILlmProvider
    {
        private const int c_maxHistoryMessages = 64;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly ConcurrentDictionary<string, List<object>> _sessions = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        private readonly ConcurrentDictionary<string, int> _sendCounts = new();
        private int _contextRounds = 12;
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

            var requestBody = new { model = _model, messages = snapshot, stream = true };
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
            string fullReply = sb.ToString();
            lock (messages)
            {
                messages.Add(new { role = "assistant", content = fullReply });
                if (messages.Count > c_maxHistoryMessages)
                    messages.RemoveRange(0, messages.Count - c_maxHistoryMessages);
            }
            return fullReply;
        }

        /// <summary>
        /// Parses a single NDJSON line from Ollama streaming response.
        /// Returns the content chunk; sets done=true when stream is finished.
        /// </summary>
        private static string ParseChunk(string line, out bool done)
        {
            done = false;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(line);
                var root = doc.RootElement;
                if (root.TryGetProperty("done", out var doneEl) && doneEl.GetBoolean())
                {
                    done = true;
                    return "";
                }
                if (root.TryGetProperty("message", out var msgEl) &&
                    msgEl.TryGetProperty("content", out var contentEl))
                {
                    return contentEl.GetString() ?? "";
                }
            }
            catch { /* ignore malformed lines */ }
            return "";
        }
    }
}
