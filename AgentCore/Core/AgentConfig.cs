using System;
using System.IO;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Configuration for agent core
    /// </summary>
    public class AgentConfig
    {
        public string BasePath { get; set; }
        public string PluginPath { get; set; }
        public bool EnableHotReload { get; set; }
        public int HotReloadCheckIntervalMs { get; set; }
        public bool UseExternalPlugin { get; set; }

        public static AgentConfig Default()
        {
            var basePath = Directory.GetCurrentDirectory();
            return new AgentConfig
            {
                BasePath = basePath,
                PluginPath = Path.Combine(basePath, "plugins", "CefDotnetApp.AgentCore.dll"),
                EnableHotReload = true,
                HotReloadCheckIntervalMs = 5000,
                UseExternalPlugin = false
            };
        }
    }
}
