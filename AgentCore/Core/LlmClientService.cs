using System;
using AbstractAgent;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using ScriptableFramework;

namespace AgentCore.Core
{
    /// <summary>
    /// Unified interface for all LLM providers.
    /// Each provider manages its own session history internally.
    /// </summary>
    internal interface ILlmProvider
    {
        /// <summary>Send a message and return the full reply string.</summary>
        Task<string> ChatAsync(string tag, string topic, string message, CancellationToken cancellationToken);
        /// <summary>Clear conversation history for the given session tag.</summary>
        void ClearHistory(string tag);
        /// <summary>Returns true if the session is currently waiting for a reply.</summary>
        bool IsBusy(string tag);
        /// <summary>Set a provider-specific option. Default implementation is no-op.</summary>
        void SetOption(string key, string value);
        /// <summary>Set a system prompt for the given session tag.</summary>
        void SetSystemPrompt(string tag, string prompt);
        /// <summary>Send a message with attached image URLs and return the full reply string.
        /// Default implementation ignores images and falls back to ChatAsync.</summary>
        Task<string> ChatWithImagesAsync(string tag, string topic, string message, string[] imageUrls, CancellationToken cancellationToken)
            => ChatAsync(tag, topic, message, cancellationToken);
        /// <summary>Add a chat_extra entry for the given session tag.
        /// key identifies the extra field (e.g. "agent_client_uuid", "extra_headers").
        /// values are the associated values. Default implementation is no-op.</summary>
        void AddChatExtra(string tag, string key, string[] values) { }
        /// <summary>Clear all chat_extra entries for the given session tag.
        /// Default implementation is no-op.</summary>
        void ClearChatExtras(string tag) { }
    }

    // -------------------------------------------------------------------------
    // LlmClientService: manages multiple providers and sessions
    // -------------------------------------------------------------------------
    /// <summary>
    /// Manages multiple LLM providers and their sessions.
    /// Each provider is identified by a providerId string.
    /// Chat results are forwarded via NativeApi.EnqueueLlmCallback as (providerId, tag, topic, reply).
    /// </summary>
    public class LlmClientService
    {
        private static readonly Lazy<LlmClientService> s_instance =
            new Lazy<LlmClientService>(() => new LlmClientService());
        public static LlmClientService Instance => s_instance.Value;

        private readonly ConcurrentDictionary<string, ILlmProvider> _providers = new();
        // Per-session serial queues (key = "providerId:tag"). Each session runs
        // its items one at a time; up to max_queue_len items may wait in line, and
        // further submissions are rejected with "busy".
        private const int c_defaultQueueSize = 10;
        private readonly ConcurrentDictionary<string, LlmSession> _sessions = new();
        private readonly ConcurrentDictionary<string, int> _providerQueueSizes = new();
        // meta: type, url, model per provider
        private readonly ConcurrentDictionary<string, (string type, string url, string model)> _providerMeta = new();
        // options per provider (sensitive keys like username are excluded)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _providerOptions = new();
        private static readonly System.Collections.Generic.HashSet<string> s_sensitiveKeys =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "username", "token", "api_key" };

        // Busy-session tracking: records when each session became busy (for stuck detection)
        private readonly ConcurrentDictionary<string, DateTime> _busySince = new();
        // Active CancellationTokenSource per session (for external cancel)
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeCts = new();
        // Watchdog timer for auto-cancelling stuck sessions
        private readonly Timer _watchdogTimer;
        private const int c_watchdogIntervalMs = 10000; // check every 10s
        private const int c_defaultMaxBusySeconds = 600; // 10 min default limit

        private LlmClientService()
        {
            _watchdogTimer = new Timer(WatchdogCallback, null, c_watchdogIntervalMs, c_watchdogIntervalMs);
        }

        /// <summary>
        /// Registers a provider. type: "ollama", "openai", "claude", "auto_metadsl"
        /// Unified params: url, apiKey, model (same semantics for all types)
        /// url/apiKey may contain %var% templates; actual values are resolved at HTTP request time
        /// via the environResolver callback.
        /// Returns "ok" or an error string.
        /// </summary>
        public string SetProvider(string providerId, string type, string url, string apiKey, string model)
        {
            // Create a resolver that decrypts + expands env vars at call time
            Func<string, string> keyEnvResolver = template =>
                AgentCore.Instance.ResolveEnvironmentValue("llm", providerId, template);

            ILlmProvider provider;
            switch (type)
            {
                case "ollama":
                    provider = new OllamaProvider(url, model);
                    break;
                case "openai":
                    provider = new OpenAiProvider(url, apiKey, model, keyEnvResolver);
                    break;
                case "claude":
                    provider = new ClaudeProvider(url, apiKey, model, keyEnvResolver);
                    break;
                case "auto_metadsl":
                    provider = new AutoMetaDslProvider(url, apiKey, model, keyEnvResolver);
                    break;
                default:
                    return $"error: unknown provider type '{type}', use 'ollama', 'openai', 'claude', or 'auto_metadsl'";
            }
            _providers[providerId] = provider;
            _providerMeta[providerId] = (type, url, model);
            _providerOptions[providerId] = new ConcurrentDictionary<string, string>();
            return "ok";
        }

        /// <summary>
        /// Sets a provider option. Currently supported: max_tokens (claude only).
        /// Returns "ok" or an error string.
        /// </summary>
        public string SetProviderOption(string providerId, string key, string value)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";
            provider.SetOption(key, value);
            // max_queue_len controls the per-session waiting-queue depth (default
            // c_defaultQueueSize). Applied to future sessions and any existing
            // sessions of this provider.
            if (key == "max_queue_len" && int.TryParse(value, out var qs) && qs >= 0)
            {
                _providerQueueSizes[providerId] = qs;
                string prefix = $"{providerId}:";
                foreach (var kv in _sessions)
                {
                    if (kv.Key.StartsWith(prefix))
                        kv.Value.MaxQueue = qs;
                }
            }
            // record non-sensitive options
            if (!s_sensitiveKeys.Contains(key))
            {
                var opts = _providerOptions.GetOrAdd(providerId, _ => new ConcurrentDictionary<string, string>());
                opts[key] = value;
            }
            return "ok";
        }

        /// <summary>
        /// Returns a human-readable summary of all configured providers (excluding sensitive fields).
        /// Format: one key:value per line, providers separated by blank lines.
        /// </summary>
        public string GetProvidersConfig()
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var kv in _providerMeta)
            {
                if (!first) sb.AppendLine();
                first = false;
                string id = kv.Key;
                var (type, url, model) = kv.Value;
                sb.AppendLine($"provider: {id}");
                sb.AppendLine($"type: {type}");
                sb.AppendLine($"url: {url}");
                sb.AppendLine($"model: {model}");
                if (_providerOptions.TryGetValue(id, out var opts))
                {
                    foreach (var opt in opts)
                        sb.AppendLine($"{opt.Key}: {opt.Value}");
                }
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Sends a chat message asynchronously.
        /// The request is appended to the session's serial queue; up to
        /// max_queue_len (default 10) requests may wait in line. When the queue is
        /// full "busy" is returned; otherwise "ok" is returned immediately and
        /// the reply arrives via llm_callback(providerId, tag, topic, reply).
        /// </summary>
        public string ChatCallback(string providerId, string tag, string topic, string message)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";

            var nativeApi = AgentCore.Instance.GetNativeApi();
            if (nativeApi == null)
                return "error: nativeApi not available";

            var session = GetSession(providerId, tag);
            var item = new WorkItem
            {
                ProviderId = providerId,
                Tag = tag,
                Topic = topic,
                Run = ct => provider.ChatAsync(tag, topic, message, ct),
                Deliver = result => nativeApi.EnqueueCefMessage("llm_callback", new BoxedValue[] { providerId, tag, topic, result })
            };
            return Submit(session, item) ? "ok" : "busy";
        }
        /// <summary>
        /// Sends a chat message and returns the reply directly via Task.
        /// Used by async script expressions (llm_chat_async). The request is
        /// queued per session; when the queue is full the returned task
        /// completes with "error: busy".
        /// </summary>
        public Task<string> ChatAsync(string providerId, string tag, string topic, string message)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return Task.FromResult($"error: provider '{providerId}' not configured");

            var session = GetSession(providerId, tag);
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var item = new WorkItem
            {
                ProviderId = providerId,
                Tag = tag,
                Topic = topic,
                Run = ct => provider.ChatAsync(tag, topic, message, ct),
                Deliver = result => tcs.TrySetResult(result)
            };
            if (!Submit(session, item))
                return Task.FromResult("error: busy");
            return tcs.Task;
        }


        /// <summary>
        /// Sends a chat message with attached image URLs asynchronously.
        /// Queued per session like ChatCallback. Returns "ok"/"busy"; result
        /// arrives via llm_callback.
        /// Only auto_metadsl actually sends images; other providers ignore them.
        /// </summary>
        public string ChatWithImagesCallback(string providerId, string tag, string topic, string message, string[] imageUrls)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";

            var nativeApi = AgentCore.Instance.GetNativeApi();
            if (nativeApi == null)
                return "error: nativeApi not available";

            var session = GetSession(providerId, tag);
            var item = new WorkItem
            {
                ProviderId = providerId,
                Tag = tag,
                Topic = topic,
                Run = ct => provider.ChatWithImagesAsync(tag, topic, message, imageUrls, ct),
                Deliver = result => nativeApi.EnqueueCefMessage("llm_callback", new BoxedValue[] { providerId, tag, topic, result })
            };
            return Submit(session, item) ? "ok" : "busy";
        }

        /// <summary>
        /// Sends a chat message with images and returns the reply directly via Task.
        /// Used by async script expressions (llm_chat_with_images_async). Queued
        /// per session; when the queue is full the task completes with "error: busy".
        /// </summary>
        public Task<string> ChatWithImagesAsync(string providerId, string tag, string topic, string message, string[] imageUrls)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return Task.FromResult($"error: provider '{providerId}' not configured");

            var session = GetSession(providerId, tag);
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var item = new WorkItem
            {
                ProviderId = providerId,
                Tag = tag,
                Topic = topic,
                Run = ct => provider.ChatWithImagesAsync(tag, topic, message, imageUrls, ct),
                Deliver = result => tcs.TrySetResult(result)
            };
            if (!Submit(session, item))
                return Task.FromResult("error: busy");
            return tcs.Task;
        }

        /// <summary>
        /// Returns the per-session queue (creating it lazily). The queue depth
        /// limit is taken from the provider's max_queue_len option, defaulting to
        /// c_defaultQueueSize.
        /// </summary>
        private LlmSession GetSession(string providerId, string tag)
        {
            string sessionKey = $"{providerId}:{tag}";
            return _sessions.GetOrAdd(sessionKey, k =>
                new LlmSession(k, _providerQueueSizes.TryGetValue(providerId, out var qs) ? qs : c_defaultQueueSize));
        }

        /// <summary>
        /// Enqueues a work item and starts the session worker if it is idle.
        /// Returns false when the session queue is full (caller should report busy).
        /// </summary>
        private bool Submit(LlmSession session, WorkItem item)
        {
            if (!session.TryEnqueue(item, out bool startWorker))
                return false;
            if (startWorker)
                StartWorker(session);
            return true;
        }

        /// <summary>
        /// Drains a session's queue on a background task, running items one at a
        /// time. Busy/cancellation tracking is maintained per running item so the
        /// watchdog and llm_cancel keep working as before.
        /// </summary>
        private void StartWorker(LlmSession session)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var item = session.Dequeue();
                    if (item == null)
                        break;

                    string sessionKey = session.Key;
                    var cts = new CancellationTokenSource();
                    _busySince[sessionKey] = DateTime.UtcNow;
                    _activeCts[sessionKey] = cts;

                    string result;
                    try
                    {
                        result = await item.Run(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        string errMsg = (ex is OperationCanceledException)
                            ? $"LLM request cancelled (busy for {GetBusyDuration(item.ProviderId, item.Tag)}s)"
                            : ex.Message;
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[LlmClientService] Chat error for '{item.ProviderId}/{item.Tag}': {errMsg}");
                        result = $"[error] {errMsg}";
                    }
                    finally
                    {
                        _busySince.TryRemove(sessionKey, out _);
                        _activeCts.TryRemove(sessionKey, out var oldCts);
                        oldCts?.Dispose();
                    }

                    try { item.Deliver(result); }
                    catch (Exception ex)
                    {
                        AgentFrameworkService.Instance.Log($"[LlmClientService] Deliver error for '{item.ProviderId}/{item.Tag}': {ex.Message}");
                    }
                }
            });
        }

        /// <summary>Clears conversation history for the given provider+tag session.</summary>
        public string ClearHistory(string providerId, string tag)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";
            provider.ClearHistory(tag);
            string sessionKey = $"{providerId}:{tag}";
            _busySince.TryRemove(sessionKey, out _);
            _activeCts.TryRemove(sessionKey, out var oldCts);
            oldCts?.Dispose();
            return "ok";
        }

        /// <summary>Sets a system prompt for the given provider+tag session.</summary>
        public string SetSystemPrompt(string providerId, string tag, string prompt)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";
            provider.SetSystemPrompt(tag, prompt);
            return "ok";
        }

        /// <summary>Returns true if the session is currently running or has queued requests.</summary>
        public bool IsBusy(string providerId, string tag)
        {
            string sessionKey = $"{providerId}:{tag}";
            return _sessions.TryGetValue(sessionKey, out var session) && session.IsActive;
        }

        /// <summary>Add a chat_extra entry for the given provider+tag session.</summary>
        public string AddChatExtra(string providerId, string tag, string key, string[] values)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";
            provider.AddChatExtra(tag, key, values);
            return "ok";
        }

        /// <summary>Clear all chat_extra entries for the given provider+tag session.</summary>
        public string ClearChatExtras(string providerId, string tag)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";
            provider.ClearChatExtras(tag);
            return "ok";
        }

        /// <summary>Returns how many seconds the session has been busy (0 if not busy).</summary>
        public int GetBusyDuration(string providerId, string tag)
        {
            string sessionKey = $"{providerId}:{tag}";
            if (_busySince.TryGetValue(sessionKey, out var since))
                return (int)(DateTime.UtcNow - since).TotalSeconds;
            return 0;
        }

        /// <summary>Cancels an active LLM request for the given session.</summary>
        public string Cancel(string providerId, string tag)
        {
            string sessionKey = $"{providerId}:{tag}";
            if (_activeCts.TryGetValue(sessionKey, out var cts))
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { }
                AgentFrameworkService.Instance.Log($"[LlmClientService] Cancel requested for session '{sessionKey}'");
                return "ok";
            }
            return "error: session not active";
        }

        /// <summary>Reads max_busy_seconds option for a provider (default 600s).</summary>
        private int GetMaxBusySeconds(string providerId)
        {
            if (_providerOptions.TryGetValue(providerId, out var opts) &&
                opts.TryGetValue("max_busy_seconds", out var val) &&
                int.TryParse(val, out var seconds) && seconds > 0)
                return seconds;
            return c_defaultMaxBusySeconds;
        }

        /// <summary>Watchdog callback: auto-cancel sessions busy longer than max_busy_seconds.</summary>
        private void WatchdogCallback(object? state)
        {
            try
            {
                var now = DateTime.UtcNow;
                foreach (var kv in _busySince)
                {
                    string sessionKey = kv.Key;
                    int duration = (int)(now - kv.Value).TotalSeconds;
                    int colonIdx = sessionKey.IndexOf(':');
                    if (colonIdx <= 0) continue;
                    string providerId = sessionKey.Substring(0, colonIdx);
                    int maxBusy = GetMaxBusySeconds(providerId);
                    if (duration > maxBusy)
                    {
                        AgentFrameworkService.Instance.Log($"[LlmClientService] Watchdog: session '{sessionKey}' busy for {duration}s (limit {maxBusy}s), auto-cancelling");
                        if (_activeCts.TryGetValue(sessionKey, out var cts))
                        {
                            try { cts.Cancel(); }
                            catch (ObjectDisposedException) { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AgentFrameworkService.Instance.Log($"[LlmClientService] Watchdog error: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------
        // Per-session serial queue support
        // -----------------------------------------------------------------

        /// <summary>A single queued chat request.</summary>
        private sealed class WorkItem
        {
            public string ProviderId = "";
            public string Tag = "";
            public string Topic = "";
            public Func<CancellationToken, Task<string>> Run = _ => Task.FromResult("");
            public Action<string> Deliver = _ => { };
        }

        /// <summary>
        /// A per-session FIFO queue with a single active worker. At most one item
        /// runs at a time; up to MaxQueue additional items may wait in line.
        /// </summary>
        private sealed class LlmSession
        {
            public readonly string Key;
            public int MaxQueue;
            private readonly Queue<WorkItem> _pending = new();
            private bool _running;

            public LlmSession(string key, int maxQueue)
            {
                Key = key;
                MaxQueue = maxQueue;
            }

            public bool IsActive
            {
                get { lock (_pending) { return _running || _pending.Count > 0; } }
            }

            /// <summary>
            /// Enqueues an item. When the session is idle the item is accepted and
            /// startWorker is set true. When busy the item is queued unless the
            /// queue already holds MaxQueue waiting items, in which case false is
            /// returned (caller should report busy).
            /// </summary>
            public bool TryEnqueue(WorkItem item, out bool startWorker)
            {
                startWorker = false;
                lock (_pending)
                {
                    if (_running)
                    {
                        if (_pending.Count >= MaxQueue)
                            return false;
                        _pending.Enqueue(item);
                        return true;
                    }
                    _pending.Enqueue(item);
                    _running = true;
                    startWorker = true;
                    return true;
                }
            }

            /// <summary>
            /// Returns the next item, or null when the queue is empty (which also
            /// marks the session idle so a future submit restarts a worker).
            /// </summary>
            public WorkItem? Dequeue()
            {
                lock (_pending)
                {
                    if (_pending.Count == 0)
                    {
                        _running = false;
                        return null;
                    }
                    return _pending.Dequeue();
                }
            }
        }
    }
}
