using ScriptableFramework;
using System.Text.RegularExpressions;

namespace AgentPlugin.Abstractions
{
    /// <summary>
    /// Interface for agent core functionality
    /// </summary>
    public interface IAgentPlugin
    {
        /// <summary>
        /// Initialize the agent core
        /// </summary>
        void Initialize(string basePath, string appDir, bool isMac);

        /// <summary>
        /// Set the native API for browser interaction
        /// </summary>
        void SetNativeApi(INativeApi nativeApi);

        /// <summary>
        /// Register script APIs to DSL engine
        /// </summary>
        void RegisterScriptApis();

        /// <summary>
        /// Convert script result to string
        /// </summary>
        string ResultToString(BoxedValue result);

        /// <summary>
        /// Skill Help search
        /// </summary>
        string SkillHelp(IList<Regex> keyRegexes);

        /// <summary>
        /// Semantic search over a set of (key, text) candidates.
        /// Each query in queries returns top-N results independently; final result is the union sorted by score descending.
        /// Returns null if embedding service is not available.
        /// </summary>
        IList<(string key, string text, float score)>? SemanticSearch(IList<string> queries, IEnumerable<(string key, string text)> candidates, int topN);

        /// <summary>
        /// Get the MaxResultSize setting (0=unlimited)
        /// </summary>
        int GetMaxResultSize();

        /// <summary>
        /// Shutdown the agent
        /// </summary>
        void Shutdown();
    }
}