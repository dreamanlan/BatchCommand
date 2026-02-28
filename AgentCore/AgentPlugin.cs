using System;
using System.Collections.Generic;
using AgentPlugin.Abstractions;
using CefDotnetApp.AgentCore.Core;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace CefDotnetApp.AgentCore
{
    /// <summary>
    /// Implementation of IAgentPlugin interface for agent functionality
    /// </summary>
    public class AgentPlugin : IAgentPlugin
    {
        private string _basePath = string.Empty;
        private string _appDir = string.Empty;
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

            // scan skills directory
            var skillsDir = System.IO.Path.Combine(_basePath, "skills");
            Core.AgentCore.Instance.SkillMgr.LoadSkills(skillsDir);
            Core.AgentCore.Instance.BuildSkillDocs();
            Core.AgentCore.Instance.Logger.Info($"Skills loaded from {skillsDir}");
        }

        /// <summary>
        /// Set the native API for browser interaction
        /// </summary>
        public void SetNativeApi(INativeApi nativeApi)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("AgentPlugin not initialized. Call Initialize first.");

            try {
                Core.AgentCore.Instance.SetNativeApi(nativeApi);
                Core.AgentCore.Instance.Logger.Info("NativeApi set successfully in AgentPlugin");
            }
            catch (Exception ex) {
                Core.AgentCore.Instance.Logger.Error($"Error setting NativeApi: {ex.Message}\nStack: {ex.StackTrace}");
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
                Core.AgentCore.Instance.Logger.Error($"Error registering script APIs: {ex.Message}\nStack: {ex.StackTrace}");
                throw;
            }
        }

        public string ResultToString(BoxedValue result)
        {
            var sb = new StringBuilder();
            DslHelper.ConvertToString(result, sb, 0, true);
            return sb.ToString();
        }

        public string SkillHelp(IList<Regex> keyRegexes)
        {
            return Core.AgentCore.Instance.GetSkillHelp(keyRegexes);
        }

        public IList<(string key, string text, float score)>? SemanticSearch(
            IList<string> queries,
            IEnumerable<(string key, string text)> candidates,
            int topN)
        {
            return Core.AgentCore.Instance.SemanticSearch(queries, candidates, topN);
        }

        public int GetMaxResultSize()
        {
            return Core.AgentCore.Instance.MaxResultSize;
        }

        public string RefreshSkills()
        {
            var skillsDir = System.IO.Path.Combine(_basePath, "skills");
            return Core.AgentCore.Instance.RefreshSkills(skillsDir);
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
