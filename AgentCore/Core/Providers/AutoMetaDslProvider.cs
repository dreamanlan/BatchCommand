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
    /// AutoMetaDSL provider (AGUI protocol: http://knot.woa.com/apigw/api/v1/agents/agui/{agent_id})
    /// </summary>
    internal class AutoMetaDslProvider : ILlmProvider
    {
        private readonly string _baseUrl;
        private readonly string _apiKeyTemplate;
        private readonly Func<string, string> _apiKeyResolver;
        private readonly string _model;
        private readonly int _maxRetries;
        // auth_mode: "personal" uses x-knot-api-token; "agent" uses x-knot-token + X-Username
        private string _authMode = "personal";
        private string _username = "";
        private bool _stream = true;
        private int _contextRounds = 12;
        // system prompts per tag
        private readonly ConcurrentDictionary<string, string> _systemPrompts = new();
        // send count per tag for periodic system prompt injection
        private readonly ConcurrentDictionary<string, int> _sendCounts = new();
        // conversation_id per tag, updated from response to inherit history
        private readonly ConcurrentDictionary<string, string> _conversationIds = new();
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        // chat_extra entries per tag: tag -> (key -> values[])
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string[]>> _chatExtras = new();
        private static readonly HttpClient s_http = CreateHttpClient();

        public AutoMetaDslProvider(string baseUrl, string apiKeyTemplate, string model, Func<string, string> apiKeyResolver, int maxRetries = 3)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKeyTemplate = apiKeyTemplate;
            _apiKeyResolver = apiKeyResolver;
            _model = model.ToLowerInvariant();
            _maxRetries = maxRetries;
        }

        public void SetOption(string key, string value)
        {
            if (key == "auth_mode") _authMode = value;
            else if (key == "username") _username = value;
            else if (key == "stream") _stream = value == "true" || value == "1";
            else if (key == "context_rounds" && int.TryParse(value, out var cr) && cr > 0) _contextRounds = cr;
        }

        public void SetSystemPrompt(string tag, string prompt)
        {
            _systemPrompts[tag] = prompt;
            _sendCounts[tag] = 0;
        }

        public bool IsBusy(string tag) => _busySessions.TryGetValue(tag, out var b) && b;

        public void ClearHistory(string tag) => _conversationIds.TryRemove(tag, out _);

        public void AddChatExtra(string tag, string key, string[] values)
        {
            var extras = _chatExtras.GetOrAdd(tag, _ => new ConcurrentDictionary<string, string[]>());
            extras[key] = values;
        }

        public void ClearChatExtras(string tag)
        {
            _chatExtras.TryRemove(tag, out _);
        }

        public async Task<string> ChatAsync(string tag, string topic, string message)
        {
            return await ChatInternalAsync(tag, topic, message, Array.Empty<string>());
        }

        public async Task<string> ChatWithImagesAsync(string tag, string topic, string message, string[] imageUrls)
        {
            return await ChatInternalAsync(tag, topic, message, imageUrls ?? Array.Empty<string>());
        }

        private async Task<string> ChatInternalAsync(string tag, string topic, string message, string[] imageUrls)
        {
            string convId = _conversationIds.GetOrAdd(tag, _ => "");
            bool useStream = _stream;

            // inject system prompt every _contextRounds sends (including the first)
            int count = _sendCounts.GetOrAdd(tag, _ => 0);
            if (count % _contextRounds == 0 &&
                _systemPrompts.TryGetValue(tag, out var sysPrompt) && !string.IsNullOrEmpty(sysPrompt))
            {
                message = sysPrompt + "\n" + message;
            }
            _sendCounts[tag] = count + 1;

            var input = new
            {
                message,
                conversation_id = convId,
                model = _model,
                stream = useStream,
                enable_web_search = false,
                chat_extra = BuildChatExtra(tag, imageUrls),
                temperature = 0.5
            };
            var body = new { input };
            string json = System.Text.Json.JsonSerializer.Serialize(body);

            string reply = "";
            string newConvId = convId;

            reply = await HttpRetryHelper.RetryAsync(async () =>
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, _baseUrl);
                AddAuthHeaders(req);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // always use ResponseHeadersRead to support SSE line-by-line reading
                using var resp = await s_http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                await HttpResponseHelper.EnsureSuccessOrThrowDetailedAsync(resp);

                var sb = new StringBuilder();
                using var stream = await resp.Content.ReadAsStreamAsync();
                using var reader = new System.IO.StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    // Wrap ReadLineAsync with timeout to prevent indefinite blocking
                    var readTask = reader.ReadLineAsync();
                    using var delayCts = new CancellationTokenSource();
                    var delayTask = Task.Delay(TimeSpan.FromSeconds(120), delayCts.Token);
                    var completed = await Task.WhenAny(readTask, delayTask);
                    if (completed == readTask)
                        delayCts.Cancel(); // cancel unused delay to avoid Task leak
                    else
                        throw new TimeoutException("AutoMetaDslProvider: SSE read timed out (120s per line)");
                    string? line = await readTask;
                    if (string.IsNullOrEmpty(line)) continue;
                    // strip "data:" prefix
                    string data = line.StartsWith("data:") ? line.Substring(5).Trim() : line.Trim();
                    if (data == "[DONE]") break;
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(data);
                        var root = doc.RootElement;
                        // update conversation_id from any event that carries it
                        if (root.TryGetProperty("rawEvent", out var rawEvent))
                        {
                            if (rawEvent.TryGetProperty("conversation_id", out var cid))
                            {
                                string cidStr = cid.GetString() ?? "";
                                if (!string.IsNullOrEmpty(cidStr))
                                    newConvId = cidStr;
                            }
                            if (root.TryGetProperty("type", out var typeEl) &&
                                typeEl.GetString() == "TEXT_MESSAGE_CONTENT" &&
                                rawEvent.TryGetProperty("content", out var content))
                            {
                                sb.Append(content.GetString());
                            }
                        }
                    }
                    catch { /* skip malformed lines */ }
                }
                return sb.ToString();
            }, _maxRetries, "AutoMetaDslProvider");

            // persist updated conversation_id for history continuation
            _conversationIds[tag] = newConvId;
            return reply;
        }

        private void AddAuthHeaders(HttpRequestMessage req)
        {
            // Resolve apiKey at request time; plaintext exists only during this call
            string resolvedKey = _apiKeyResolver(_apiKeyTemplate);
            if (_authMode == "agent")
            {
                if (!string.IsNullOrEmpty(resolvedKey))
                    req.Headers.Add("x-knot-token", resolvedKey);
                if (!string.IsNullOrEmpty(_username))
                    req.Headers.Add("X-Username", _username);
            }
            else
            {
                // default: personal token
                if (!string.IsNullOrEmpty(resolvedKey))
                    req.Headers.Add("x-knot-api-token", resolvedKey);
            }
        }

        private static HttpClient CreateHttpClient() => new HttpClient(
            RedirectHandler.Create(new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            }))
        { Timeout = TimeSpan.FromMinutes(10) };

        /// <summary>Build the chat_extra object from cached extras and image URLs.</summary>
        private object BuildChatExtra(string tag, string[] imageUrls)
        {
            string agentClientUuid = "";
            var extraHeaders = new Dictionary<string, string>();
            if (_chatExtras.TryGetValue(tag, out var extras))
            {
                // agent_client_uuid: single value
                if (extras.TryGetValue("agent_client_uuid", out var uuidVals) && uuidVals.Length > 0)
                    agentClientUuid = uuidVals[0];
                // extra_headers: key,val,key,val,...
                if (extras.TryGetValue("extra_headers", out var hdrs) && hdrs.Length >= 2)
                {
                    for (int i = 0; i + 1 < hdrs.Length; i += 2)
                        extraHeaders[hdrs[i]] = hdrs[i + 1];
                }
            }
            return new
            {
                agent_client_uuid = agentClientUuid,
                attached_images = imageUrls,
                extra_headers = extraHeaders
            };
        }
    }
}
