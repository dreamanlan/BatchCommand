using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace BatchCmdDsl
{
    public sealed class PluginService
    {
        private static PluginService? _instance;
        private static readonly object _lock = new object();

        public IBatchCmdPlugin? BatchCmdPlugin { get; private set; }

        // The ALC that hosts this assembly: the hostfxr load context when
        // running under the standalone native host (BatchCmdDslHost.exe),
        // or the default context under the CEF host. The plugin and its
        // dependencies MUST load into the SAME context as this assembly:
        // when they are split across contexts, shared types (BoxedValue,
        // Dsl types) get duplicated and cross-assembly member binding fails
        // with MissingMethodException (e.g. BatchScript.Call(String)).
        private static readonly AssemblyLoadContext s_HostAlc =
            AssemblyLoadContext.GetLoadContext(typeof(PluginService).Assembly)
            ?? AssemblyLoadContext.Default;

        private static readonly bool s_HostAlcIsDefault =
            ReferenceEquals(s_HostAlc, AssemblyLoadContext.Default);

        private static Assembly LoadAssemblyFromPath(string path)
        {
            // The default ALC does not support LoadFromAssemblyPath; the
            // classic LoadFrom keeps the old (working) CEF behavior.
            if (s_HostAlcIsDefault) {
                return Assembly.LoadFrom(path);
            }
            return s_HostAlc.LoadFromAssemblyPath(path);
        }

        private static Assembly LoadAssemblyFromBytes(byte[] dll, byte[]? pdb)
        {
            if (s_HostAlcIsDefault) {
                return pdb != null ? Assembly.Load(dll, pdb) : Assembly.Load(dll);
            }
            using var ms = new MemoryStream(dll);
            if (pdb != null) {
                using var ps = new MemoryStream(pdb);
                return s_HostAlc.LoadFromStream(ms, ps);
            }
            return s_HostAlc.LoadFromStream(ms);
        }

        // Pre-cached assembly bytes for sandbox environments where file I/O
        // may be restricted after initial loading phase.
        // Key: assembly name (without extension), Value: (dll bytes, pdb bytes or null)
        private Dictionary<string, (byte[] dll, byte[]? pdb)>? _assemblyCache;

        private PluginService() { }

        public static PluginService Instance
        {
            get {
                if (_instance == null) {
                    lock (_lock) {
                        _instance ??= new PluginService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Log a message via NativeApi with thread info.
        /// Uses NativeLog on main thread, EnqueueNativeLog otherwise.
        /// </summary>
        public void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>
        /// Assembly resolve handler for loading dependencies from managed and plugin directories.
        /// Must be registered before LoadPlugin is called.
        /// </summary>
        private Assembly? OnAssemblyResolve(object? sender, ResolveEventArgs args, string basePath, string pluginDir)
        {
            try {
                string? assemblyName = new AssemblyName(args.Name).Name;
                // Satellite resource probes ("Xxx.resources", culture-specific
                // message fallbacks) are always benign: when the satellite is
                // absent the runtime falls back to the default culture. Probe
                // the directories silently instead of logging a failure for
                // every one of them.
                bool isSatellite = assemblyName != null && assemblyName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase);
                // Microsoft.Data.Sqlite's static ctor probes for WinRT via
                // Type.GetType("..., Windows, ContentType=WindowsRuntime") to
                // register UWP app data directories. The probe failing IS the
                // expected desktop path (Sqlite proceeds normally), so a
                // "Windows" request from it is not a missing-assembly failure.
                // Do NOT supply a real Windows.dll projection here: it would
                // make the probe succeed and push Sqlite onto the UWP path.
                bool isWinRTProbe = assemblyName == "Windows"
                    && args.RequestingAssembly?.GetName().Name == "Microsoft.Data.Sqlite";
                if (!isSatellite && !isWinRTProbe) {
                    Log($"[csharp] AssemblyResolve: Requesting {assemblyName} (requested by {args.RequestingAssembly?.FullName ?? "unknown"})");
                }

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
                        return LoadAssemblyFromBytes(cached.dll, cached.pdb);
                    }
                    catch (Exception cacheEx) {
                        Log($"[csharp] AssemblyResolve: Cache loading failed for {assemblyName}: {cacheEx.Message}");
                    }
                }

                // Fallback: try to load from the managed directory and the plugin directory
                string managedPath = Path.Combine(basePath, "managed");
                var probeDirs = new List<string>();
                if (Directory.Exists(managedPath)) {
                    probeDirs.Add(managedPath);
                }
                if (!string.IsNullOrEmpty(pluginDir) && Directory.Exists(pluginDir) &&
                    !probeDirs.Contains(pluginDir, StringComparer.OrdinalIgnoreCase)) {
                    probeDirs.Add(pluginDir);
                }

                foreach (string probeDir in probeDirs) {
                    string assemblyPath = Path.Combine(probeDir, assemblyName + ".dll");
                    if (!File.Exists(assemblyPath)) {
                        continue;
                    }
                    Log($"[csharp] AssemblyResolve: Loading {assemblyName} from {assemblyPath}");
                    try {
                        return LoadAssemblyFromPath(assemblyPath);
                    }
                    catch (Exception loadFromEx) {
                        // Path-based loading may fail on macOS in sandbox environments.
                        // Fall back to loading from raw bytes.
                        Log($"[csharp] AssemblyResolve: LoadFrom failed for {assemblyName}: {loadFromEx.Message}");
                        Log($"[csharp] AssemblyResolve: Falling back to byte[] loading for {assemblyName}");
                        try {
                            byte[] assemblyBytes = ReadAllBytesShared(assemblyPath);
                            string pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
                            if (File.Exists(pdbPath)) {
                                byte[] pdbBytes = ReadAllBytesShared(pdbPath);
                                return LoadAssemblyFromBytes(assemblyBytes, pdbBytes);
                            }
                            return LoadAssemblyFromBytes(assemblyBytes, null);
                        }
                        catch (Exception bytesEx) {
                            Log($"[csharp] AssemblyResolve: Byte[] loading also failed for {assemblyName}: {bytesEx.Message}");
                        }
                    }
                }

                if (!isSatellite && !isWinRTProbe) {
                    Log($"[csharp] AssemblyResolve: Could not find {assemblyName} in {managedPath} or plugin directory");
                    // For non-satellite misses log the activation stack: it names
                    // the exact type/method that pulled the missing assembly in.
                    try {
                        Log("[csharp] AssemblyResolve: resolve stack: " + Environment.StackTrace);
                    }
                    catch { /* stack is best-effort */ }
                }
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
                        byte[] dllBytes = ReadAllBytesShared(dllPath);
                        byte[]? pdbBytes = null;

                        string pdbPath = Path.ChangeExtension(dllPath, ".pdb");
                        if (File.Exists(pdbPath)) {
                            try {
                                pdbBytes = ReadAllBytesShared(pdbPath);
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
        /// Load a plugin dll implementing IBatchCmdPlugin via reflection and initialize it.
        /// The first concrete class implementing IBatchCmdPlugin in the assembly is used.
        /// </summary>
        public IBatchCmdPlugin? LoadPlugin(string pluginPath, string cmdLine, string basePath)
        {
            try {
                // Pre-cache all managed DLLs into memory BEFORE sandbox restrictions
                // take effect. On macOS, Chromium's sandbox may restrict file I/O
                // for renderer/helper processes, so we read everything into memory
                // while we still have file system access.
                PreCacheAssemblies(basePath);

                string resolvedPath = pluginPath;
                if (!Path.IsPathRooted(resolvedPath)) {
                    resolvedPath = Path.Combine(basePath, resolvedPath);
                }
                string pluginDir = Path.GetDirectoryName(Path.GetFullPath(resolvedPath)) ?? basePath;

                // Register assembly resolve event handler before loading the plugin
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => OnAssemblyResolve(sender, args, basePath, pluginDir);

                Log($"[csharp] Loading plugin from: {resolvedPath}");

                Assembly? pluginAssembly = null;

                // Try loading from file first (normal path)
                try {
                    if (File.Exists(resolvedPath)) {
                        pluginAssembly = LoadAssemblyFromPath(resolvedPath);
                    }
                }
                catch (Exception fileLoadEx) {
                    Log($"[csharp] File-based loading failed for plugin: {fileLoadEx.Message}");
                }

                // Fallback: load from raw bytes (file may be locked or partially written)
                if (pluginAssembly == null && File.Exists(resolvedPath)) {
                    Log("[csharp] Loading plugin from raw bytes");
                    try {
                        byte[] assemblyBytes = ReadAllBytesShared(resolvedPath);
                        string pdbPath = Path.ChangeExtension(resolvedPath, ".pdb");
                        if (File.Exists(pdbPath)) {
                            byte[] pdbBytes = ReadAllBytesShared(pdbPath);
                            pluginAssembly = LoadAssemblyFromBytes(assemblyBytes, pdbBytes);
                        }
                        else {
                            pluginAssembly = LoadAssemblyFromBytes(assemblyBytes, null);
                        }
                    }
                    catch (Exception bytesEx) {
                        Log($"[csharp] Byte[] loading also failed for plugin: {bytesEx.Message}");
                    }
                }

                if (pluginAssembly == null) {
                    Log($"[csharp] Plugin could not be loaded from: {resolvedPath}");
                    return null;
                }

                Log($"[csharp] Loaded plugin assembly: {pluginAssembly.FullName}");

                // Find the plugin type implementing IBatchCmdPlugin
                Type? pluginType = FindPluginType(pluginAssembly);
                if (pluginType == null) {
                    Log("[csharp] No type implementing IBatchCmdPlugin found in plugin assembly");
                    Log("[csharp] Available types in plugin assembly:");
                    foreach (var type in SafeGetTypes(pluginAssembly)) {
                        Log($"[csharp]   - {type.FullName}");
                    }
                    return null;
                }

                Log($"[csharp] Found plugin type: {pluginType.FullName}");

                // Create instance and use interface casting
                object? pluginInstance = Activator.CreateInstance(pluginType);
                if (pluginInstance == null) {
                    Log("[csharp] Failed to create plugin instance (Activator.CreateInstance returned null)");
                    return null;
                }

                Log($"[csharp] Created instance of type: {pluginInstance.GetType().FullName}");

                // Try to cast to IBatchCmdPlugin
                IBatchCmdPlugin? plugin = pluginInstance as IBatchCmdPlugin;
                if (plugin == null) {
                    Log("[csharp] Failed to cast plugin instance to IBatchCmdPlugin");

                    Type interfaceType = typeof(IBatchCmdPlugin);
                    Log($"[csharp] IBatchCmdPlugin interface: {interfaceType.AssemblyQualifiedName}");
                    Log($"[csharp] Instance implements IBatchCmdPlugin: {interfaceType.IsAssignableFrom(pluginType)}");

                    Log("[csharp] Interfaces implemented by plugin type:");
                    foreach (var iface in pluginType.GetInterfaces()) {
                        Log($"[csharp]   - {iface.AssemblyQualifiedName}");
                    }

                    return null;
                }

                Log("[csharp] Successfully cast to IBatchCmdPlugin");

                // Initialize the plugin
                plugin.Init(cmdLine, basePath);
                BatchCmdPlugin = plugin;
                Log("[csharp] Plugin loaded and initialized successfully");

                return plugin;
            }
            catch (Exception ex) {
                Log($"[csharp] Error loading plugin: {ex.Message}");
                Log($"[csharp] Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null) {
                    Log($"[csharp] Inner exception: {ex.InnerException.Message}");
                    Log($"[csharp] Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return null;
            }
        }

        private Type? FindPluginType(Assembly assembly)
        {
            Type? found = null;
            foreach (var type in SafeGetTypes(assembly)) {
                if (type.IsClass && !type.IsAbstract && typeof(IBatchCmdPlugin).IsAssignableFrom(type)) {
                    if (found == null) {
                        found = type;
                    }
                    else {
                        Log($"[csharp] Multiple IBatchCmdPlugin types found, using the first: {found.FullName} (ignored: {type.FullName})");
                    }
                }
            }
            return found;
        }

        private Type[] SafeGetTypes(Assembly assembly)
        {
            try {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex) {
                Log($"[csharp] ReflectionTypeLoadException: {ex.LoaderExceptions?.Length ?? 0} loader exceptions");
                if (null != ex.LoaderExceptions) {
                    foreach (var le in ex.LoaderExceptions) {
                        if (null != le) {
                            Log($"[csharp]   loader exception: {le.Message}");
                        }
                    }
                }
                return ex.Types.Where(t => t != null).Select(t => t!).ToArray();
            }
        }

        /// <summary>
        /// Stop hot reload and shutdown plugin.
        /// </summary>
        public int ShutdownPlugin()
        {
            int exitCode = 0;
            if (BatchCmdPlugin != null) {
                exitCode = BatchCmdPlugin.Shutdown();
                BatchCmdPlugin = null;
            }
            return exitCode;
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

        private static byte[] ReadAllBytesShared(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
