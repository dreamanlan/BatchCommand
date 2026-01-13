using System;
using CefDotnetApp.Interfaces;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.MetaDSL;

namespace CefDotnetApp.AgentCore
{
    /// <summary>
    /// Implementation of IAgentPlugin interface for agent functionality
    /// </summary>
    public class AgentPlugin : IAgentPlugin
    {
        private string _basePath;
        private bool _isInitialized;

        // IAgentCore operation properties - delegate to AgentCore.Instance
        public IFileOperations FileOps => Core.AgentCore.Instance.FileOps;
        public IDiffOperations DiffOps => Core.AgentCore.Instance.DiffOps;
        public IClipboardOperations ClipboardOps => Core.AgentCore.Instance.ClipboardOps;
        public ILoggingAndDebugging Logger => Core.AgentCore.Instance.Logger;
        public IHttpClientOperations HttpClient => Core.AgentCore.Instance.HttpClient;
        public ITaskManagement TaskManager => Core.AgentCore.Instance.TaskManager;
        public ILLMInteraction LLMManager => Core.AgentCore.Instance.LLMManager;
        public IProcessOperations ProcessOps => Core.AgentCore.Instance.ProcessOps;
        public IContextManagement ContextManager => Core.AgentCore.Instance.ContextManager;
        public ICodeAnalysis CodeAnalyzer => Core.AgentCore.Instance.CodeAnalyzer;
        public ITemplateEngine TemplateEngine => Core.AgentCore.Instance.TemplateEngine;
        public IBrowserInteraction BrowserOps => Core.AgentCore.Instance.BrowserOps;
        public IAgentMessageHandler MessageHandler => Core.AgentCore.Instance.MessageHandler;

        public void Initialize(string basePath)
        {
            if (_isInitialized)
                return;

            _basePath = basePath ?? System.IO.Directory.GetCurrentDirectory();

            // Initialize the AgentCore singleton if not already initialized
            if (!Core.AgentCore.IsInitialized)
            {
                Core.AgentCore.Initialize(_basePath);
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

            try
            {
                Core.AgentCore.Instance.SetNativeApi(nativeApi);
                Core.AgentCore.Instance.Logger.Info("NativeApi set successfully in AgentPlugin");
            }
            catch (Exception ex)
            {
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
            try
            {
                // Register all script APIs including MetaDSL
                CefDotnetApp.AgentCore.ScriptApiRegistrar.RegisterAllApis();
                Core.AgentCore.Instance.Logger.Info("Script APIs registered successfully");
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"Error registering script APIs: {ex.Message}");
                throw;
            }
        }

        public void ProcessJsCommand(string command, string parameters)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("AgentPlugin not initialized. Call Initialize first.");

            try
            {
                Core.AgentCore.Instance.Logger.Info($"Processing command: {command}");

                // Delegate to AgentCore's message handler
                Core.AgentCore.Instance.OnReceiveJsMessage(command, parameters);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"Error processing command {command}: {ex.Message}");
                throw;
            }
        }

        public AgentInfo GetAgentInfo()
        {
            return new AgentInfo
            {
                Version = "1.0.0",
                Name = "WebAgent",
                Description = "A web-based programming agent with code analysis and editing capabilities",
                Capabilities = new[]
                {
                    "file_operations",
                    "code_analysis",
                    "advanced_editing",
                    "diff_operations",
                    "search_replace",
                    "symbol_navigation",
                    "plugin_system",
                    "self_bootstrap"
                }
            };
        }

        public AgentPluginMetadata GetMetadata()
        {
            return new AgentPluginMetadata
            {
                Id = "CefDotnetApp.AgentCore",
                Version = "1.0.0",
                Author = "WebAgent Team",
                Dependencies = new[] { "CefDotnetApp", "LibGit2Sharp" }
            };
        }

        public void Shutdown()
        {
            if (_isInitialized)
            {
                Core.AgentCore.Instance.Logger.Info("AgentPlugin shutting down");
                Core.AgentCore.Instance.Shutdown();
                _isInitialized = false;
            }
        }

        public void Unload()
        {
            Shutdown();
        }

        public string ExecuteMetaDSL(string script)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("AgentPlugin not initialized. Call Initialize first.");

            try
            {
                Core.AgentCore.Instance.Logger.Info("Executing MetaDSL script...");

                // Delegate to MetaDSL executor
                var executor = new MetaDSL.Executor();
                return executor.Execute(script, Core.AgentCore.Instance);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"Error executing MetaDSL: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public void HotReload(string component)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("AgentPlugin not initialized. Call Initialize first.");

            try
            {
                // Delegate to AgentCore's TriggerHotReload method
                Core.AgentCore.Instance.TriggerHotReload(component);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"Error hot reloading {component}: {ex.Message}");
                throw;
            }
        }
    }
}
