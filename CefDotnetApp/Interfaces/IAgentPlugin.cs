using ScriptableFramework;

namespace CefDotnetApp.Interfaces
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
        void SetNativeApi(DotNetLib.NativeApi nativeApi);

        /// <summary>
        /// Register script APIs to DSL engine
        /// </summary>
        void RegisterScriptApis();

        /// <summary>
        /// Convert script result to string
        /// </summary>
        string ResultToString(BoxedValue result);

        /// <summary>
        /// Shutdown the agent
        /// </summary>
        void Shutdown();
    }
}
