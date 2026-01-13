namespace CefDotnetApp.Interfaces
{
    /// <summary>
    /// Interface for agent core functionality
    /// </summary>
    public interface IAgentCore
    {
        /// <summary>
        /// Initialize the agent core
        /// </summary>
        void Initialize(string basePath);

        /// <summary>
        /// Set the native API for browser interaction
        /// </summary>
        void SetNativeApi(DotNetLib.NativeApi nativeApi);

        /// <summary>
        /// Process a command from JavaScript
        /// </summary>
        void ProcessJsCommand(string command, string parameters);

        /// <summary>
        /// Get agent information
        /// </summary>
        AgentInfo GetAgentInfo();

        /// <summary>
        /// Shutdown the agent
        /// </summary>
        void Shutdown();

        // Operation classes properties
        IFileOperations FileOps { get; }
        IDiffOperations DiffOps { get; }
        IClipboardOperations ClipboardOps { get; }
        ILoggingAndDebugging Logger { get; }
        IHttpClientOperations HttpClient { get; }
        ITaskManagement TaskManager { get; }
        ILLMInteraction LLMManager { get; }
        IProcessOperations ProcessOps { get; }
        IContextManagement ContextManager { get; }
        ICodeAnalysis CodeAnalyzer { get; }
        ITemplateEngine TemplateEngine { get; }
        IBrowserInteraction BrowserOps { get; }
        IAgentMessageHandler MessageHandler { get; }
    }

    /// <summary>
    /// Agent information
    /// </summary>
    public class AgentInfo
    {
        public string Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Capabilities { get; set; }
    }

    /// <summary>
    /// Interface for dynamic agent plugin
    /// </summary>
    public interface IAgentPlugin : IAgentCore
    {
        /// <summary>
        /// Register script APIs to DSL engine
        /// </summary>
        void RegisterScriptApis();

        /// <summary>
        /// Get plugin metadata
        /// </summary>
        AgentPluginMetadata GetMetadata();

        /// <summary>
        /// Called when plugin is unloaded
        /// </summary>
        void Unload();

        /// <summary>
        /// Execute MetaDSL script
        /// </summary>
        string ExecuteMetaDSL(string script);

        /// <summary>
        /// Trigger hot reload for specific component
        /// </summary>
        void HotReload(string component);
    }

    /// <summary>
    /// Agent plugin metadata
    /// </summary>
    public class AgentPluginMetadata
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string[] Dependencies { get; set; }
    }
}
