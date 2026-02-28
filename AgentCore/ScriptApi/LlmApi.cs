using System.Collections.Generic;
using AgentPlugin.Abstractions;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// llm_set_provider(provider_id, type, url, api_key, model)
    /// Configures an LLM provider. Unified params for all types:
    /// type="openai": url=baseUrl, api_key=Bearer token, model=model name
    /// type="claude": url=API endpoint (empty=default Anthropic), api_key=x-api-key, model=model name
    /// type="auto_metadsl": url=baseUrl, api_key=token (optional), model=ignored
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class LlmSetProviderExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 5) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_set_provider requires (provider_id, type, url, api_key, model)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string type = operands[1].AsString;
            string url = operands[2].AsString;
            string apiKey = operands[3].AsString;
            string model = operands[4].AsString;
            // apiKey may contain %var% placeholders (e.g. %person_token%).
            // It is stored as a template; actual value is resolved at HTTP request time.
            return BoxedValue.FromString(Core.LlmClientService.Instance.SetProvider(providerId, type, url, apiKey, model));
        }
    }

    /// <summary>
    /// llm_set_provider_option(provider_id, key, value)
    /// Sets a provider-specific option. Supported keys: max_tokens (claude only).
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class LlmSetProviderOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_set_provider_option requires (provider_id, key, value)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string key = operands[1].AsString;
            string value = operands[2].AsString;
            return BoxedValue.FromString(Core.LlmClientService.Instance.SetProviderOption(providerId, key, value));
        }
    }

    /// <summary>
    /// llm_chat(provider_id, tag, topic, message)
    /// Sends a message to the specified provider+session.
    /// Returns "ok", "busy", or an error string.
    /// Full reply arrives via llm_callback(provider_id, tag, topic, reply) CEF message.
    /// </summary>
    sealed class LlmChatExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_chat requires (provider_id, tag, topic, message)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            string topic = operands[2].AsString;
            string message = operands[3].AsString;
            return BoxedValue.FromString(Core.LlmClientService.Instance.Chat(providerId, tag, topic, message));
        }
    }

    /// <summary>
    /// llm_chat_with_images(provider_id, tag, topic, message [, image_url1, image_url2, ...])
    /// Sends a message with attached image URLs to the specified provider+session.
    /// Images must be COS URLs (starting with http:// or https://).
    /// Returns "ok", "busy", or an error string.
    /// Full reply arrives via llm_callback(provider_id, tag, topic, reply) CEF message.
    /// </summary>
    sealed class LlmChatWithImagesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_chat_with_images requires (provider_id, tag, topic, message [, image_url1, image_url2, ...])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            string topic = operands[2].AsString;
            string message = operands[3].AsString;
            // collect variadic image URLs from operands[4..]
            var imageUrls = new List<string>();
            for (int i = 4; i < operands.Count; i++) {
                string url = operands[i].AsString;
                if (string.IsNullOrWhiteSpace(url)) continue;
                if (!url.StartsWith("http://") && !url.StartsWith("https://")) {
                    return BoxedValue.FromString($"error: image must be a URL (http/https), got: {url}");
                }
                imageUrls.Add(url);
            }
            return BoxedValue.FromString(Core.LlmClientService.Instance.ChatWithImages(providerId, tag, topic, message, imageUrls.ToArray()));
        }
    }

    /// <summary>
    /// llm_clear_history(provider_id, tag)
    /// Clears conversation history for the given provider+session.
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class LlmClearHistoryExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_clear_history requires (provider_id, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            return BoxedValue.FromString(Core.LlmClientService.Instance.ClearHistory(providerId, tag));
        }
    }

    /// <summary>
    /// llm_add_chat_extra(provider_id, tag, key, values...)
    /// Adds a chat_extra entry for the given provider+session.
    /// key: "agent_client_uuid" (single value), "extra_headers" (key,val,key,val,...), or other custom keys.
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class LlmAddChatExtraExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_add_chat_extra requires (provider_id, tag, key [, value1, value2, ...])");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            string key = operands[2].AsString;
            // collect variadic values from operands[3..]
            var values = new List<string>();
            for (int i = 3; i < operands.Count; i++) {
                values.Add(operands[i].AsString);
            }
            return BoxedValue.FromString(Core.LlmClientService.Instance.AddChatExtra(providerId, tag, key, values.ToArray()));
        }
    }

    /// <summary>
    /// llm_clear_chat_extras(provider_id, tag)
    /// Clears all chat_extra entries for the given provider+session.
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class LlmClearChatExtrasExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_clear_chat_extras requires (provider_id, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            return BoxedValue.FromString(Core.LlmClientService.Instance.ClearChatExtras(providerId, tag));
        }
    }

    /// <summary>
    /// llm_is_busy(provider_id, tag)
    /// Returns true if the session is currently waiting for a reply.
    /// </summary>
    sealed class LlmIsBusyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_is_busy requires (provider_id, tag)");
                return BoxedValue.FromBool(false);
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            return BoxedValue.FromBool(Core.LlmClientService.Instance.IsBusy(providerId, tag));
        }
    }

    /// <summary>
    /// llm_get_providers_config()
    /// Returns a human-readable summary of all configured LLM providers.
    /// Excludes sensitive fields (token, api_key, username).
    /// Format: one key:value per line, providers separated by blank lines.
    /// </summary>
    sealed class LlmGetProvidersConfigExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            return BoxedValue.FromString(Core.LlmClientService.Instance.GetProvidersConfig());
        }
    }

    /// <summary>
    /// llm_set_system_prompt(provider_id, tag, prompt)
    /// Sets a system prompt for the given provider+session.
    /// For openai/claude/ollama: prepended to every request as a system message.
    /// For auto_metadsl: prepended to the message every context_rounds sends (default 12).
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class LlmSetSystemPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("llm_set_system_prompt requires (provider_id, tag, prompt)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string providerId = operands[0].AsString;
            string tag = operands[1].AsString;
            string prompt = operands[2].AsString;
            return BoxedValue.FromString(Core.LlmClientService.Instance.SetSystemPrompt(providerId, tag, prompt));
        }
    }

    /// <summary>
    /// Registers all LLM DSL APIs.
    /// </summary>
    public static class LlmApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("llm_set_provider",
                "llm_set_provider(provider_id, type, url, api_key, model) - configure LLM provider (type: openai/claude/auto_metadsl)",
                new ExpressionFactoryHelper<LlmSetProviderExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_set_provider_option",
                "llm_set_provider_option(provider_id, key, value) - set provider option (e.g. max_tokens for claude)",
                new ExpressionFactoryHelper<LlmSetProviderOptionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_chat",
                "llm_chat(provider_id, tag, topic, message) - send message to LLM, full reply via llm_callback CEF message",
                new ExpressionFactoryHelper<LlmChatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_chat_with_images",
                "llm_chat_with_images(provider_id, tag, topic, message [, image_url1, ...]) - send message with images (COS URLs) to LLM",
                new ExpressionFactoryHelper<LlmChatWithImagesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_clear_history",
                "llm_clear_history(provider_id, tag) - clear conversation history for session",
                new ExpressionFactoryHelper<LlmClearHistoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_add_chat_extra",
                "llm_add_chat_extra(provider_id, tag, key [, val1, val2, ...]) - add chat_extra entry (e.g. agent_client_uuid, extra_headers)",
                new ExpressionFactoryHelper<LlmAddChatExtraExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_clear_chat_extras",
                "llm_clear_chat_extras(provider_id, tag) - clear all chat_extra entries for session",
                new ExpressionFactoryHelper<LlmClearChatExtrasExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_is_busy",
                "llm_is_busy(provider_id, tag) - check if session is currently waiting for reply",
                new ExpressionFactoryHelper<LlmIsBusyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_get_providers_config",
                "llm_get_providers_config() - return all configured providers config (excludes sensitive fields)",
                new ExpressionFactoryHelper<LlmGetProvidersConfigExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("llm_set_system_prompt",
                "llm_set_system_prompt(provider_id, tag, prompt) - set system prompt for session (auto_metadsl: injected every context_rounds sends)",
                new ExpressionFactoryHelper<LlmSetSystemPromptExp>());
        }
    }
}
