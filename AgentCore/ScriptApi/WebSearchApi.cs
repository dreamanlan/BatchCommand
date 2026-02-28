using System;
using AgentPlugin.Abstractions;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// brave_set_api_key(api_key)
    /// Sets the Brave Search API key and activates Brave as the current search engine.
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class BraveSetApiKeyExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("brave_set_api_key requires (api_key)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string apiKey = operands[0].AsString;
            // apiKey may contain %var% placeholders (e.g. %brave_api_key%).
            // It is stored as a template; actual value is resolved at HTTP request time.
            string result = Core.AgentCore.Instance.BraveSearch.SetApiKey(apiKey);
            if (result == "ok") {
                Core.AgentCore.Instance.WebSearchRouter.ActivateBrave();
            }
            return BoxedValue.FromString(result);
        }
    }

    /// <summary>
    /// searxng_set_url(url)
    /// Sets the SearXNG instance URL and activates SearXNG as the current search engine.
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class SearXNGSetUrlExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("searxng_set_url requires (url)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string url = operands[0].AsString;
            string result = Core.AgentCore.Instance.SearXNGSearch.SetUrl(url);
            if (result == "ok") {
                Core.AgentCore.Instance.WebSearchRouter.ActivateSearXNG();
            }
            return BoxedValue.FromString(result);
        }
    }

    /// <summary>
    /// web_search(query[, count])
    /// Searches the web via the currently active search engine (Brave or SearXNG).
    /// Returns formatted text results (title + URL + description).
    /// Default count is 5, max is 20.
    /// </summary>
    sealed class WebSearchExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("web_search requires (query[, count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string query = operands[0].AsString;
                int count = operands.Count > 1 ? operands[1].GetInt() : 5;
                return BoxedValue.FromString(Core.AgentCore.Instance.WebSearchRouter.Search(query, count));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"web_search error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    /// <summary>
    /// web_search_raw(query[, count])
    /// Searches the web via the currently active search engine (Brave or SearXNG).
    /// Returns raw JSON response.
    /// </summary>
    sealed class WebSearchRawExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("web_search_raw requires (query[, count])");
                return BoxedValue.FromString("error: missing parameters");
            }
            try {
                string query = operands[0].AsString;
                int count = operands.Count > 1 ? operands[1].GetInt() : 5;
                return BoxedValue.FromString(Core.AgentCore.Instance.WebSearchRouter.SearchRaw(query, count));
            }
            catch (Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"web_search_raw error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Registers all Web Search DSL APIs.
    /// </summary>
    public static class WebSearchApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("brave_set_api_key",
                "brave_set_api_key(api_key) - set Brave Search API key and activate Brave engine",
                new ExpressionFactoryHelper<BraveSetApiKeyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("searxng_set_url",
                "searxng_set_url(url) - set SearXNG instance URL and activate SearXNG engine",
                new ExpressionFactoryHelper<SearXNGSetUrlExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("web_search",
                "web_search(query[, count]) - search the web via active engine (Brave/SearXNG), returns formatted text (default 5 results, max 20)",
                new ExpressionFactoryHelper<WebSearchExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("web_search_raw",
                "web_search_raw(query[, count]) - search the web via active engine (Brave/SearXNG), returns raw JSON",
                new ExpressionFactoryHelper<WebSearchRawExp>());
        }
    }
}
