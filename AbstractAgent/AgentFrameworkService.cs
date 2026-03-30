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

        // Pre-cached assembly bytes for sandbox environments where file I/O
        // may be restricted after initial loading phase.
        // Key: assembly name (without extension), Value: (dll bytes, pdb bytes or null)
        private Dictionary<string, (byte[] dll, byte[]? pdb)>? _assemblyCache;

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

                // Try to load from pre-cached bytes first (for sandbox environments)
                if (_assemblyCache != null && assemblyName != null &&
                    _assemblyCache.TryGetValue(assemblyName, out var cached)) {
                    Log($"[csharp] AssemblyResolve: Loading {assemblyName} from pre-cached bytes");
                    try {
                        if (cached.pdb != null) {
                            return Assembly.Load(cached.dll, cached.pdb);
                        }
                        return Assembly.Load(cached.dll);
                    }
                    catch (Exception cacheEx) {
                        Log($"[csharp] AssemblyResolve: Cache loading failed for {assemblyName}: {cacheEx.Message}");
                    }
                }

                // Fallback: try to load from the managed directory directly
                string managedPath = Path.Combine(basePath, "managed");
                string assemblyPath = Path.Combine(managedPath, assemblyName + ".dll");

                if (File.Exists(assemblyPath)) {
                    Log($"[csharp] AssemblyResolve: Loading {assemblyName} from {assemblyPath}");
                    try {
                        return Assembly.LoadFrom(assemblyPath);
                    }
                    catch (Exception loadFromEx) {
                        // Assembly.LoadFrom may fail on macOS in sandbox environments.
                        // Fall back to loading from raw bytes.
                        Log($"[csharp] AssemblyResolve: LoadFrom failed for {assemblyName}: {loadFromEx.Message}");
                        Log($"[csharp] AssemblyResolve: Falling back to byte[] loading for {assemblyName}");
                        try {
                            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
                            string pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
                            if (File.Exists(pdbPath)) {
                                byte[] pdbBytes = File.ReadAllBytes(pdbPath);
                                return Assembly.Load(assemblyBytes, pdbBytes);
                            }
                            return Assembly.Load(assemblyBytes);
                        }
                        catch (Exception bytesEx) {
                            Log($"[csharp] AssemblyResolve: Byte[] loading also failed for {assemblyName}: {bytesEx.Message}");
                            return null;
                        }
                    }
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
        /// Pre-cache all DLL files from the managed directory into memory.
        /// This must be called before sandbox restrictions take effect, so that
        /// AssemblyResolve can load assemblies from memory even when file I/O
        /// is blocked by the Chromium sandbox on macOS.
        /// </summary>
        private void PreCacheAssemblies(string basePath)
        {
            try {
                string managedPath = Path.Combine(basePath, "managed");
                if (!Directory.Exists(managedPath)) {
                    Log($"[csharp] PreCacheAssemblies: managed directory not found: {managedPath}");
                    return;
                }

                _assemblyCache = new Dictionary<string, (byte[] dll, byte[]? pdb)>(StringComparer.OrdinalIgnoreCase);
                string[] dllFiles = Directory.GetFiles(managedPath, "*.dll");
                int cachedCount = 0;

                foreach (string dllPath in dllFiles) {
                    try {
                        string name = Path.GetFileNameWithoutExtension(dllPath);
                        byte[] dllBytes = File.ReadAllBytes(dllPath);
                        byte[]? pdbBytes = null;

                        string pdbPath = Path.ChangeExtension(dllPath, ".pdb");
                        if (File.Exists(pdbPath)) {
                            try {
                                pdbBytes = File.ReadAllBytes(pdbPath);
                            }
                            catch {
                                // PDB loading is optional, ignore errors
                            }
                        }

                        _assemblyCache[name] = (dllBytes, pdbBytes);
                        cachedCount++;
                    }
                    catch (Exception ex) {
                        Log($"[csharp] PreCacheAssemblies: Failed to cache {Path.GetFileName(dllPath)}: {ex.Message}");
                    }
                }

                Log($"[csharp] PreCacheAssemblies: Cached {cachedCount}/{dllFiles.Length} assemblies from {managedPath}");
            }
            catch (Exception ex) {
                Log($"[csharp] PreCacheAssemblies: Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load AgentCore.dll plugin via reflection and initialize it.
        /// </summary>
        public bool LoadAgentPlugin(string basePath, string appDir, bool isMac)
        {
            try {
                // Pre-cache all managed DLLs into memory BEFORE sandbox restrictions
                // take effect. On macOS, Chromium's sandbox may restrict file I/O
                // for renderer/helper processes, so we read everything into memory
                // while we still have file system access.
                PreCacheAssemblies(basePath);

                // Register assembly resolve event handler before loading AgentCore
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => OnAssemblyResolve(sender, args, basePath);

                string pluginPath = Path.Combine(basePath, "managed", "AgentCore.dll");

                Log($"[csharp] Loading AgentCore.dll from: {pluginPath}");

                Assembly? pluginAssembly = null;

                // Try loading from file first (normal path)
                try {
                    if (File.Exists(pluginPath)) {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(pluginPath);
                        Log($"[csharp] Assembly name: {assemblyName.FullName}");
                        pluginAssembly = Assembly.Load(assemblyName);
                        if (pluginAssembly == null) {
                            Log("[csharp] Failed to load AgentCore.dll using Assembly.Load");
                            Log("[csharp] Trying fallback to Assembly.LoadFrom...");
                            pluginAssembly = Assembly.LoadFrom(pluginPath);
                        }
                    }
                }
                catch (Exception fileLoadEx) {
                    Log($"[csharp] File-based loading failed for AgentCore.dll: {fileLoadEx.Message}");
                }

                // Fallback: load from pre-cached bytes (for sandbox environments)
                if (pluginAssembly == null && _assemblyCache != null &&
                    _assemblyCache.TryGetValue("AgentCore", out var cachedCore)) {
                    Log("[csharp] Loading AgentCore.dll from pre-cached bytes");
                    if (cachedCore.pdb != null) {
                        pluginAssembly = Assembly.Load(cachedCore.dll, cachedCore.pdb);
                    } else {
                        pluginAssembly = Assembly.Load(cachedCore.dll);
                    }
                }

                if (pluginAssembly == null) {
                    Log("[csharp] AgentCore.dll could not be loaded from any source");
                    return false;
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
