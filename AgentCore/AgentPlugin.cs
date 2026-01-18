using System;
using CefDotnetApp.Interfaces;
using CefDotnetApp.AgentCore.Core;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Utils;
using System.Text;

namespace CefDotnetApp.AgentCore
{
    /// <summary>
    /// Implementation of IAgentPlugin interface for agent functionality
    /// </summary>
    public class AgentPlugin : IAgentPlugin
    {
        private string _basePath;
        private string _appDir;
        private bool _isMac;
        private bool _isInitialized;

        public void Initialize(string basePath, string appDir, bool isMac)
        {
            if (_isInitialized)
                return;

            _basePath = basePath ?? System.IO.Directory.GetCurrentDirectory();
            _appDir = appDir ?? System.IO.Directory.GetCurrentDirectory();
            _isMac = isMac;

            // Initialize the AgentCore singleton if not already initialized
            if (!Core.AgentCore.IsInitialized) {
                Core.AgentCore.Initialize(_basePath, _appDir, isMac);
            }

            _isInitialized = true;
            Core.AgentCore.Instance.Logger.Info($"AgentPlugin initialized at {_basePath}");
        }

        /// <summary>
        /// Set the native API for browser interaction
        /// </summary>
        public void SetNativeApi(DotNetLib.NativeApi nativeApi)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("AgentPlugin not initialized. Call Initialize first.");

            try {
                Core.AgentCore.Instance.SetNativeApi(nativeApi);
                Core.AgentCore.Instance.Logger.Info("NativeApi set successfully in AgentPlugin");
            }
            catch (Exception ex) {
                Core.AgentCore.Instance.Logger.Error($"Error setting NativeApi: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Register all Script APIs to the DSL script engine
        /// This should be called by the host application after loading the plugin
        /// </summary>
        public void RegisterScriptApis()
        {
            try {
                // Register all script APIs including MetaDSL
                CefDotnetApp.AgentCore.ScriptApiRegistrar.RegisterAllApis();
                Core.AgentCore.Instance.Logger.Info("Script APIs registered successfully");
            }
            catch (Exception ex) {
                Core.AgentCore.Instance.Logger.Error($"Error registering script APIs: {ex.Message}");
                throw;
            }
        }

        public string ResultToString(BoxedValue result)
        {
            var sb = new StringBuilder();
            DslHelper.ConvertToString(result, sb, 0, true);
            return sb.ToString();
        }

        public void Shutdown()
        {
            if (_isInitialized) {
                Core.AgentCore.Instance.Logger.Info("AgentPlugin shutting down");
                Core.AgentCore.Instance.Shutdown();
                _isInitialized = false;
            }
        }
    }
}
