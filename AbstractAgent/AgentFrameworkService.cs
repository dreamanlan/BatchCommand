using System.Reflection;
using System.Text.RegularExpressions;

namespace AgentPlugin.Abstractions
{
    /// <summary>
    /// Singleton service that provides framework-level abstractions to AgentCore.
    /// CefDotnetApp (host) sets the concrete implementations at startup;
    /// AgentCore consumes them through the interfaces exposed here.
    /// </summary>
    public sealed class AgentFrameworkService
    {
        private static AgentFrameworkService? _instance;
        private static readonly object _lock = new object();

        public INativeApi? NativeApi { get; private set; }
        public IDslEngine? DslEngine { get; private set; }
        public IErrorReporter? ErrorReporter { get; private set; }
        public IAgentPlugin? AgentPlugin { get; private set; }
        public int MainThreadId { get; private set; }

        private HotReloadManager? _hotReloadManager;

        private AgentFrameworkService() { }

        public static AgentFrameworkService Instance
        {
            get {
                if (_instance == null) {
                    lock (_lock) {
                        _instance ??= new AgentFrameworkService();
                    }
                }
                return _instance;
            }
        }

        public void SetNativeApi(INativeApi nativeApi)
        {
            NativeApi = nativeApi;
        }

        public void SetDslEngine(IDslEngine dslEngine)
        {
            DslEngine = dslEngine;
        }

        public void SetErrorReporter(IErrorReporter errorReporter)
        {
            ErrorReporter = errorReporter;
        }

        public void SetMainThreadId(int threadId)
        {
            MainThreadId = threadId;
        }

        /// <summary>
        /// Log a message via NativeApi with thread info.
        /// Uses NativeLog on main thread, EnqueueNativeLog otherwise.
        /// </summary>
        public void Log(string msg)
        {
            if (null != NativeApi) {
                NativeApi.NativeLog(msg);
            }
        }

        /// <summary>
        /// Assembly resolve handler for loading dependencies from managed directory.
        /// Must be registered before LoadAgentPlugin is called.
        /// </summary>
        private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args, string basePath)
        {
            try {
                string? assemblyName = new AssemblyName(args.Name).Name;
                Log($"[csharp] AssemblyResolve: Requesting {assemblyName}");

                // If the assembly is already loaded, return it
                Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in loadedAssemblies) {
                    if (assembly.GetName().Name == assemblyName) {
                        Log($"[csharp] AssemblyResolve: {assemblyName} already loaded");
                        return assembly;
                    }
                }

                // Try to load from the managed directory
                string managedPath = Path.Combine(basePath, "managed");
                string assemblyPath = Path.Combine(managedPath, assemblyName + ".dll");

                if (File.Exists(assemblyPath)) {
                    Log($"[csharp] AssemblyResolve: Loading {assemblyName} from {assemblyPath}");
                    return Assembly.LoadFrom(assemblyPath);
                }

                Log($"[csharp] AssemblyResolve: Could not find {assemblyName} in {managedPath}");
                return null;
            }
            catch (Exception ex) {
                Log($"[csharp] AssemblyResolve exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load AgentCore.dll plugin via reflection and initialize it.
        /// </summary>
        public bool LoadAgentPlugin(string basePath, string appDir, bool isMac)
        {
            try {
                // Register assembly resolve event handler before loading AgentCore
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => OnAssemblyResolve(sender, args, basePath);

                string pluginPath = Path.Combine(basePath, "managed", "AgentCore.dll");
                if (!File.Exists(pluginPath)) {
                    Log($"[csharp] AgentCore.dll not found at: {pluginPath}");
                    return false;
                }

                Log($"[csharp] Loading AgentCore.dll from: {pluginPath}");

                // Use Assembly.Load instead of LoadFrom to share assembly load context
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(pluginPath);
                Log($"[csharp] Assembly name: {assemblyName.FullName}");

                // Load the assembly into the default load context
                Assembly? pluginAssembly = Assembly.Load(assemblyName);
                if (pluginAssembly == null) {
                    Log("[csharp] Failed to load AgentCore.dll using Assembly.Load");
                    Log("[csharp] Trying fallback to Assembly.LoadFrom...");
                    pluginAssembly = Assembly.LoadFrom(pluginPath);
                }

                Log($"[csharp] Loaded assembly: {pluginAssembly.FullName}");

                // Find the AgentPlugin type
                Type? pluginType = pluginAssembly.GetType("CefDotnetApp.AgentCore.AgentPlugin");
                if (pluginType == null) {
                    Log("[csharp] AgentPlugin type not found in AgentCore.dll");
                    Log("[csharp] Available types in AgentCore.dll:");
                    foreach (var type in pluginAssembly.GetTypes()) {
                        Log($"[csharp]   - {type.FullName}");
                    }
                    return false;
                }

                Log($"[csharp] Found AgentPlugin type: {pluginType.FullName}");

                // Create instance and use interface casting
                object? pluginInstance = Activator.CreateInstance(pluginType);
                if (pluginInstance == null) {
                    Log("[csharp] Failed to create AgentPlugin instance (Activator.CreateInstance returned null)");
                    return false;
                }

                Log($"[csharp] Created instance of type: {pluginInstance.GetType().FullName}");

                // Try to cast to IAgentPlugin
                AgentPlugin = pluginInstance as IAgentPlugin;
                if (AgentPlugin == null) {
                    Log("[csharp] Failed to cast AgentPlugin instance to IAgentPlugin");

                    Type interfaceType = typeof(IAgentPlugin);
                    Log($"[csharp] IAgentPlugin interface: {interfaceType.AssemblyQualifiedName}");
                    Log($"[csharp] Instance implements IAgentPlugin: {interfaceType.IsAssignableFrom(pluginType)}");

                    Log("[csharp] Interfaces implemented by AgentPlugin:");
                    foreach (var iface in pluginType.GetInterfaces()) {
                        Log($"[csharp]   - {iface.AssemblyQualifiedName}");
                    }

                    return false;
                }

                Log("[csharp] Successfully cast to IAgentPlugin");

                // Initialize the plugin
                AgentPlugin.Initialize(basePath, appDir, isMac);
                Log("[csharp] AgentPlugin loaded and initialized successfully");

                // Set native API for the plugin
                if (NativeApi != null) {
                    AgentPlugin.SetNativeApi(NativeApi);
                    Log("[csharp] NativeApi set successfully for AgentPlugin");
                }

                // Register Script APIs through the plugin
                AgentPlugin.RegisterScriptApis();
                Log("[csharp] Script APIs registered successfully");

                // Start hot reload file watcher
                StartHotReload(basePath);

                return true;
            }
            catch (Exception ex) {
                Log($"[csharp] Error loading AgentPlugin: {ex.Message}");
                Log($"[csharp] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null) {
                    Log($"[csharp] Inner exception: {ex.InnerException.Message}");
                    Log($"[csharp] Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return false;
            }
        }

        /// <summary>
        /// Handle file change events from HotReloadManager.
        /// </summary>
        public void OnFileChanged(string filePath, string fileType)
        {
            Log($"[csharp] File changed detected: {fileType} - {filePath}");

            try {
                switch (fileType) {
                    case "DSL Script":
                        Log("[csharp] Script.dsl will be reloaded on next message");
                        break;

                    case "AgentCore DLL":
                        Log("[csharp] AgentCore.dll changed - hot reload scheduled");
                        break;

                    case "Inject Script":
                        if (NativeApi != null) {
                            Log("[csharp] Inject script changed, triggering hot reload directly");
                            // All C# to JS calls go through window object methods
                            NativeApi.SendJavascriptCode("window.onAgentCommand(JSON.stringify({command:'hot_reload',params:{component:'inject'}}))");
                        }
                        break;
                }
            }
            catch (Exception ex) {
                Log($"[csharp] Error handling file change: {ex.Message}");
            }
        }

        /// <summary>
        /// Start the hot reload file watcher.
        /// </summary>
        private void StartHotReload(string basePath)
        {
            if (_hotReloadManager == null) {
                _hotReloadManager = new HotReloadManager(basePath);
                _hotReloadManager.SetCallback(OnFileChanged);
                _hotReloadManager.StartWatching();
                Log("[csharp] Hot reload manager started in renderer process");
            }
        }

        /// <summary>
        /// Stop hot reload and shutdown plugin.
        /// </summary>
        public void ShutdownPlugin()
        {
            _hotReloadManager?.StopWatching();
            _hotReloadManager = null;

            if (AgentPlugin != null) {
                AgentPlugin.Shutdown();
                AgentPlugin = null;
            }
        }

        /// <summary>
        /// Strip all non-alphanumeric characters from a string to produce clean tokens.
        /// </summary>
        public static string CleanStringData(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return pattern;
            string s = Regex.Replace(pattern, "[^\\p{L}\\p{N}]", " ");
            return Regex.Replace(s, " {2,}", " ").Trim();
        }
    }
}
