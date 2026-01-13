using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Plugin loader for dynamic agent core loading
    /// </summary>
    public class AgentPluginLoader
    {
        private IAgentPlugin _currentPlugin;
        private string _pluginPath;
        private DateTime _lastLoadTime;

        /// <summary>
        /// Load agent plugin from DLL file
        /// </summary>
        public IAgentPlugin LoadPlugin(string pluginPath, string basePath)
        {
            try
            {
                if (!File.Exists(pluginPath))
                    throw new FileNotFoundException($"Plugin file not found: {pluginPath}");

                // Create a separate load context for the plugin
                var pluginAssembly = Assembly.LoadFrom(pluginPath);

                // Find the plugin type
                var pluginType = pluginAssembly.GetTypes()
                    .FirstOrDefault(t => typeof(IAgentPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                if (pluginType == null)
                    throw new InvalidOperationException($"No plugin type found implementing IAgentPlugin in {pluginPath}");

                // Create plugin instance
                var plugin = (IAgentPlugin)Activator.CreateInstance(pluginType);

                // Initialize plugin
                plugin.Initialize(basePath);

                _currentPlugin = plugin;
                _pluginPath = pluginPath;
                _lastLoadTime = File.GetLastWriteTime(pluginPath);

                AgentCore.Instance.Logger.Info($"Plugin loaded successfully: {plugin.GetMetadata().Id} v{plugin.GetMetadata().Version}");

                return plugin;
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Error($"Failed to load plugin: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reload plugin if file has been modified
        /// </summary>
        public IAgentPlugin ReloadIfNeeded(string basePath)
        {
            if (_currentPlugin == null || string.IsNullOrEmpty(_pluginPath))
                return _currentPlugin;

            try
            {
                DateTime currentModTime = File.GetLastWriteTime(_pluginPath);

                if (currentModTime > _lastLoadTime)
                {
                    AgentCore.Instance.Logger.Info("Plugin file modified, reloading...");

                    // Unload current plugin
                    Unload();

                    // Reload plugin
                    return LoadPlugin(_pluginPath, basePath);
                }
            }
            catch (Exception ex)
            {
                AgentCore.Instance.Logger.Error($"Failed to reload plugin: {ex.Message}");
            }

            return _currentPlugin;
        }

        /// <summary>
        /// Unload current plugin
        /// </summary>
        public void Unload()
        {
            if (_currentPlugin != null)
            {
                try
                {
                    _currentPlugin.Unload();
                    AgentCore.Instance.Logger.Info("Plugin unloaded successfully");
                }
                catch (Exception ex)
                {
                    AgentCore.Instance.Logger.Error($"Error unloading plugin: {ex.Message}");
                }

                _currentPlugin = null;
            }
        }

        /// <summary>
        /// Get current plugin
        /// </summary>
        public IAgentPlugin GetCurrentPlugin()
        {
            return _currentPlugin;
        }

        /// <summary>
        /// Check if plugin is loaded
        /// </summary>
        public bool IsPluginLoaded()
        {
            return _currentPlugin != null;
        }
    }
}
