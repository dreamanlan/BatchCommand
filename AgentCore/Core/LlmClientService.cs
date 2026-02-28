using System;
using AgentPlugin.Abstractions;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Unified interface for all LLM providers.
    /// Each provider manages its own session history internally.
    /// </summary>
    internal interface ILlmProvider
    {
        /// <summary>Send a message and return the full reply string.</summary>
        Task<string> ChatAsync(string tag, string topic, string message);
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
        Task<string> ChatWithImagesAsync(string tag, string topic, string message, string[] imageUrls)
            => ChatAsync(tag, topic, message);
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
        private readonly ConcurrentDictionary<string, bool> _busySessions = new();
        // meta: type, url, model per provider
        private readonly ConcurrentDictionary<string, (string type, string url, string model)> _providerMeta = new();
        // options per provider (sensitive keys like username are excluded)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _providerOptions = new();
        private static readonly System.Collections.Generic.HashSet<string> s_sensitiveKeys =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "username", "token", "api_key" };

        private LlmClientService() { }

        /// <summary>
        /// Registers a provider. type: "ollama", "openai", "claude", "auto_metadsl"
        /// Unified params: url, apiKey, model (same semantics for all types)
        /// apiKey may contain %var% templates; actual values are resolved at HTTP request time
        /// via the apiKeyResolver callback.
        /// Returns "ok" or an error string.
        /// </summary>
        public string SetProvider(string providerId, string type, string url, string apiKey, string model)
        {
            // Create a resolver that decrypts + expands env vars at call time
            Func<string, string> apiKeyResolver = template =>
                AgentCore.Instance.ResolveEnvironmentValue("llm", providerId, template);

            ILlmProvider provider;
            switch (type)
            {
                case "ollama":
                    provider = new OllamaProvider(url, model);
                    break;
                case "openai":
                    provider = new OpenAiProvider(url, apiKey, model, apiKeyResolver);
                    break;
                case "claude":
                    provider = new ClaudeProvider(url, apiKey, model, apiKeyResolver);
                    break;
                case "auto_metadsl":
                    provider = new AutoMetaDslProvider(url, apiKey, model, apiKeyResolver);
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
        /// Returns "ok" immediately; result arrives via llm_callback(providerId, tag, topic, reply).
        /// </summary>
        public string Chat(string providerId, string tag, string topic, string message)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";

            string sessionKey = $"{providerId}:{tag}";
            if (_busySessions.TryGetValue(sessionKey, out var busy) && busy)
                return "busy";

            var nativeApi = AgentCore.Instance.GetNativeApi();
            if (nativeApi == null)
                return "error: nativeApi not available";

            _busySessions[sessionKey] = true;

            Task.Run(async () =>
            {
                try
                {
                    string reply = await provider.ChatAsync(tag, topic, message);
                    nativeApi.EnqueueLlmCallback(providerId, tag, topic, reply);
                }
                catch (Exception ex)
                {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[LlmClientService] Chat error for '{providerId}/{tag}': {ex.Message}");
                    nativeApi.EnqueueLlmCallback(providerId, tag, topic, $"[error] {ex.Message}");
                }
                finally
                {
                    _busySessions[sessionKey] = false;
                }
            });

            return "ok";
        }

        /// <summary>
        /// Sends a chat message with attached image URLs asynchronously.
        /// Returns "ok" immediately; result arrives via llm_callback.
        /// Only auto_metadsl actually sends images; other providers ignore them.
        /// </summary>
        public string ChatWithImages(string providerId, string tag, string topic, string message, string[] imageUrls)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";

            string sessionKey = $"{providerId}:{tag}";
            if (_busySessions.TryGetValue(sessionKey, out var busy) && busy)
                return "busy";

            var nativeApi = AgentCore.Instance.GetNativeApi();
            if (nativeApi == null)
                return "error: nativeApi not available";

            _busySessions[sessionKey] = true;

            Task.Run(async () =>
            {
                try
                {
                    string reply = await provider.ChatWithImagesAsync(tag, topic, message, imageUrls);
                    nativeApi.EnqueueLlmCallback(providerId, tag, topic, reply);
                }
                catch (Exception ex)
                {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"[LlmClientService] ChatWithImages error for '{providerId}/{tag}': {ex.Message}");
                    nativeApi.EnqueueLlmCallback(providerId, tag, topic, $"[error] {ex.Message}");
                }
                finally
                {
                    _busySessions[sessionKey] = false;
                }
            });

            return "ok";
        }

        /// <summary>Clears conversation history for the given provider+tag session.</summary>
        public string ClearHistory(string providerId, string tag)
        {
            if (!_providers.TryGetValue(providerId, out var provider))
                return $"error: provider '{providerId}' not configured";
            provider.ClearHistory(tag);
            _busySessions.TryRemove($"{providerId}:{tag}", out _);
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

        /// <summary>Returns true if the session is currently waiting for a reply.</summary>
        public bool IsBusy(string providerId, string tag)
        {
            string sessionKey = $"{providerId}:{tag}";
            return _busySessions.TryGetValue(sessionKey, out var busy) && busy;
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
    }
}
