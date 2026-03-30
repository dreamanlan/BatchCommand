using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Handles loading of native libraries (DLLs) for TreeSitter
    /// </summary>
    public static class NativeLibraryLoader
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        // Fallback managed directory path for HostCLR environments where
        // Assembly.GetExecutingAssembly().Location may return empty string.
        private static string? _managedDir;

        // Log caching for early initialization (before AgentCore.Instance is available)
        private static readonly System.Collections.Generic.List<(string level, string message)> _logCache = new();
        private static bool _logsFlushed = false;

        // Windows API for DLL search path management
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr AddDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDefaultDllDirectories(uint directoryFlags);

        private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

        /// <summary>
        /// Get the full native library filename for the current platform.
        /// On Windows: {name}.dll, on macOS: lib{name}.dylib, on Linux: lib{name}.so
        /// </summary>
        private static string GetNativeLibraryFileName(string baseName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $"{baseName}.dll";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return $"lib{baseName}.dylib";
            return $"lib{baseName}.so"; // Linux and others
        }

        /// <summary>
        /// Internal logging method that caches logs until AgentCore is ready
        /// </summary>
        private static void Log(string level, string message)
        {
            lock (_lock)
            {
                if (_logsFlushed && Core.AgentCore.IsInitialized && null != Core.AgentCore.Instance.GetNativeApi())
                {
                    // AgentCore is ready, log directly
                    switch (level.ToLower())
                    {
                        case "info":
                            Core.AgentCore.Instance.Logger.Info(message);
                            break;
                        case "warning":
                            Core.AgentCore.Instance.Logger.Warning(message);
                            break;
                        case "error":
                            Core.AgentCore.Instance.Logger.Error(message);
                            break;
                        case "debug":
                            Core.AgentCore.Instance.Logger.Debug(message);
                            break;
                    }
                }
                else
                {
                    // Cache the log for later
                    _logCache.Add((level, message));
                }
            }
        }

        /// <summary>
        /// Flush cached logs to AgentCore logger
        /// Should be called after AgentCore.Instance is initialized
        /// </summary>
        public static void FlushLogsToLogger()
        {
            lock (_lock)
            {
                if (_logsFlushed || !Core.AgentCore.IsInitialized || null == Core.AgentCore.Instance.GetNativeApi())
                    return;

                foreach (var (level, message) in _logCache)
                {
                    switch (level.ToLower())
                    {
                        case "info":
                            Core.AgentCore.Instance.Logger.Info(message);
                            break;
                        case "warning":
                            Core.AgentCore.Instance.Logger.Warning(message);
                            break;
                        case "error":
                            Core.AgentCore.Instance.Logger.Error(message);
                            break;
                        case "debug":
                            Core.AgentCore.Instance.Logger.Debug(message);
                            break;
                    }
                }

                _logCache.Clear();
                _logsFlushed = true;
            }
        }

        /// <summary>
        /// Get the AgentCore directory path.
        /// First tries Assembly.GetExecutingAssembly().Location, then falls back
        /// to the stored _managedDir path (for HostCLR environments).
        /// </summary>
        private static string? GetAgentCoreDir()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if (!string.IsNullOrEmpty(location))
            {
                var dir = Path.GetDirectoryName(location);
                if (!string.IsNullOrEmpty(dir))
                    return dir;
            }

            // Fallback to stored managed directory path
            if (!string.IsNullOrEmpty(_managedDir))
            {
                Log("info", $"[NativeLibraryLoader] Using fallback managed directory: {_managedDir}");
                return _managedDir;
            }

            Log("warning", "[NativeLibraryLoader] Assembly.Location is empty and no fallback managed directory set");
            return null;
        }

        /// <summary>
        /// Initialize native library loading.
        /// </summary>
        /// <param name="basePath">Optional base path (parent of 'managed' directory).
        /// Used as fallback when Assembly.Location is empty (e.g. in HostCLR environments
        /// where assemblies may be loaded from byte arrays).</param>
        public static void Initialize(string? basePath = null)
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                // Store managed directory path as fallback for Assembly.Location
                if (!string.IsNullOrEmpty(basePath))
                {
                    _managedDir = Path.Combine(basePath, "managed");
                    Log("info", $"[NativeLibraryLoader] Managed directory fallback set to: {_managedDir}");
                }

                try
                {
                    // Preload Everything64 native DLL for file search
                    PreloadEverythingDll();

                    // Preload SQLite native DLL before any SqliteConnection is created
                    PreloadSqliteDll();

                    // Preload OnnxRuntime native DLLs before any NativeMethods is accessed
                    PreloadOnnxRuntimeDlls();

                    // Preload all TreeSitter native DLLs from the native directory
                    PreloadTreeSitterDlls();

                    // Add native DLL directory to Windows DLL search path
                    AddNativeDllSearchPath();

                    // Set up AssemblyLoad event handler to automatically configure DllImportResolver
                    // when TreeSitter assemblies are loaded
                    AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
                    Log("info", "[NativeLibraryLoader] Registered AssemblyLoad event handler for TreeSitter assemblies");

                    // Try to set up resolvers for already-loaded assemblies
                    SetupDllImportResolver("TreeSitter.DotNet");

                    _initialized = true;
                    Log("info", "[NativeLibraryLoader] TreeSitter native library resolver initialized");
                }
                catch (Exception ex)
                {
                    Log("error", $"[NativeLibraryLoader] Failed to initialize: {ex.Message}");
                    throw;
                }
            }
        }

        private static void PreloadEverythingDll()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            try
            {
var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                    return;

                string rid = GetRuntimeIdentifier();
                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");
                var dllPath = Path.Combine(nativeDir, "Everything.dll");

                if (!File.Exists(dllPath))
                {
                    Log("warning", $"[NativeLibraryLoader] Everything.dll not found at: {dllPath}");
                    return;
                }

                if (NativeLibrary.TryLoad(dllPath, out _))
                    Log("info", $"[NativeLibraryLoader] Successfully preloaded Everything.dll from {dllPath}");
                else
                    Log("warning", $"[NativeLibraryLoader] Failed to preload Everything.dll from {dllPath}");
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Exception preloading Everything.dll: {ex.Message}");
            }
        }

        // Cached path to the native e_sqlite3 library for DllImportResolver
        private static string? _sqliteNativeLibPath;

        private static void PreloadSqliteDll()
        {
            try
            {
                var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                {
                    Log("warning", "[NativeLibraryLoader] Cannot determine AgentCore directory, cannot preload SQLite");
                    return;
                }

                Log("info", $"[NativeLibraryLoader] AgentCore directory: {agentCoreDir}");

                string rid = GetRuntimeIdentifier();
                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");
                var libName = GetNativeLibraryFileName("e_sqlite3");
                var libPath = Path.Combine(nativeDir, libName);

                if (!File.Exists(libPath))
                {
                    Log("warning", $"[NativeLibraryLoader] {libName} not found at: {libPath}");
                    return;
                }

                // Step 1: Preload the native e_sqlite3 library into the process
                if (NativeLibrary.TryLoad(libPath, out _))
                {
                    Log("info", $"[NativeLibraryLoader] Successfully preloaded {libName} from {libPath}");
                    _sqliteNativeLibPath = libPath;
                }
                else
                {
                    Log("warning", $"[NativeLibraryLoader] Failed to preload {libName} from {libPath}");
                    return;
                }

                // Step 2: Register DllImportResolver for the SQLitePCLRaw provider assembly.
                // In HostCLR environments, the default DllImport probing paths may not include
                // the runtimes/<rid>/native directory, causing P/Invoke calls to e_sqlite3 to fail.
                try
                {
                    RegisterSqliteDllImportResolver();
                }
                catch (Exception resolverEx)
                {
                    Log("warning", $"[NativeLibraryLoader] Failed to register SQLite DllImportResolver: {resolverEx.Message}");
                }

                // Step 3: Explicitly initialize SQLitePCLRaw provider.
                // This must happen BEFORE SqliteConnection's static constructor runs,
                // otherwise the cctor will attempt Batteries_V2.Init() via reflection
                // which may fail in custom host environments.
                try
                {
                    SQLitePCL.Batteries_V2.Init();
                    Log("info", "[NativeLibraryLoader] SQLitePCL.Batteries_V2.Init() succeeded");
                }
                catch (Exception initEx)
                {
                    Log("warning", $"[NativeLibraryLoader] SQLitePCL.Batteries_V2.Init() failed: {initEx.Message}");
                    // Fallback: try to manually set the provider
                    try
                    {
                        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
                        Log("info", "[NativeLibraryLoader] Manually set SQLitePCL provider (SQLite3Provider_e_sqlite3) succeeded");
                    }
                    catch (Exception providerEx)
                    {
                        Log("error", $"[NativeLibraryLoader] Failed to manually set SQLitePCL provider: {providerEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Exception preloading SQLite native library: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a DllImportResolver for assemblies that P/Invoke into e_sqlite3.
        /// This ensures that when SQLitePCLRaw.provider.e_sqlite3 calls [DllImport("e_sqlite3")],
        /// the runtime can find the native library at the correct path.
        /// </summary>
        private static void RegisterSqliteDllImportResolver()
        {
            if (string.IsNullOrEmpty(_sqliteNativeLibPath))
                return;

            // Register resolver for already-loaded assemblies
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = asm.GetName().Name;
                if (name != null && name.Contains("SQLitePCLRaw.provider"))
                {
                    try
                    {
                        NativeLibrary.SetDllImportResolver(asm, SqliteDllImportResolver);
                        Log("info", $"[NativeLibraryLoader] Registered SQLite DllImportResolver for {name}");
                    }
                    catch (InvalidOperationException)
                    {
                        // Resolver already set for this assembly, ignore
                        Log("info", $"[NativeLibraryLoader] DllImportResolver already set for {name}");
                    }
                }
            }

            // Also register for future assembly loads
            AppDomain.CurrentDomain.AssemblyLoad += OnSqliteAssemblyLoad;
        }

        private static void OnSqliteAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            var name = args.LoadedAssembly.GetName().Name;
            if (name != null && name.Contains("SQLitePCLRaw.provider"))
            {
                try
                {
                    NativeLibrary.SetDllImportResolver(args.LoadedAssembly, SqliteDllImportResolver);
                    Log("info", $"[NativeLibraryLoader] Registered SQLite DllImportResolver for late-loaded {name}");
                }
                catch (InvalidOperationException)
                {
                    // Already set
                }
            }
        }

        private static IntPtr SqliteDllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // Only handle e_sqlite3 requests
            if (libraryName == "e_sqlite3" && !string.IsNullOrEmpty(_sqliteNativeLibPath))
            {
                if (NativeLibrary.TryLoad(_sqliteNativeLibPath, out IntPtr handle))
                    return handle;
            }
            // Fall back to default resolution for other libraries
            return IntPtr.Zero;
        }

        private static void PreloadOnnxRuntimeDlls()
        {
            try
            {
var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                    return;

                string rid = GetRuntimeIdentifier();
                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                // Load providers_shared first as it is a dependency of onnxruntime
                // On macOS, providers_shared may not exist (only onnxruntime itself)
                string[] baseNames = { "onnxruntime_providers_shared", "onnxruntime" };
                foreach (var baseName in baseNames)
                {
                    var libName = GetNativeLibraryFileName(baseName);
                    var libPath = Path.Combine(nativeDir, libName);
                    if (!File.Exists(libPath))
                    {
                        Log("info", $"[NativeLibraryLoader] {libName} not found at: {libPath}, skipping");
                        continue;
                    }

                    if (NativeLibrary.TryLoad(libPath, out _))
                        Log("info", $"[NativeLibraryLoader] Successfully preloaded {libName} from {libPath}");
                    else
                        Log("warning", $"[NativeLibraryLoader] Failed to preload {libName} from {libPath}");
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Exception preloading OnnxRuntime native libraries: {ex.Message}");
            }
        }

        private static void PreloadTreeSitterDlls()
        {
            try
            {
var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                {
                    Log("error", "[NativeLibraryLoader] Failed to get AgentCore directory for preload");
                    return;
                }

                string rid = GetRuntimeIdentifier();
                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                if (!Directory.Exists(nativeDir))
                {
                    Log("warning", $"[NativeLibraryLoader] Native directory not found for preload: {nativeDir}");
                    return;
                }

                // Determine the glob pattern and core library name based on platform
                string globPattern;
                string coreLibName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    globPattern = "tree-sitter*.dll";
                    coreLibName = "tree-sitter.dll";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    globPattern = "libtree-sitter*.dylib";
                    coreLibName = "libtree-sitter.dylib";
                }
                else
                {
                    globPattern = "libtree-sitter*.so";
                    coreLibName = "libtree-sitter.so";
                }

                // Preload tree-sitter core library first, then all language-specific libraries
                string[] libsToPreload = Directory.GetFiles(nativeDir, globPattern)
                    .Select(Path.GetFileName)
                    .OrderBy(n => n == coreLibName ? 0 : 1) // core library first
                    .ThenBy(n => n)
                    .ToArray()!;

                foreach (var libName in libsToPreload)
                {
                    var libPath = Path.Combine(nativeDir, libName);
                    if (!File.Exists(libPath))
                    {
                        Log("warning", $"[NativeLibraryLoader] Native library not found for preload: {libPath}");
                        continue;
                    }

                    try
                    {
                        if (NativeLibrary.TryLoad(libPath, out IntPtr handle))
                        {
                            Log("info", $"[NativeLibraryLoader] Successfully preloaded: {libName} from {libPath}");

                            // Cache tree-sitter core library handle
                            if (libName == coreLibName)
                            {
                                _treeSitterHandle = handle;
                            }
                        }
                        else
                        {
                            Log("warning", $"[NativeLibraryLoader] Failed to preload: {libName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("error", $"[NativeLibraryLoader] Exception preloading {libName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Error in PreloadTreeSitterDlls: {ex.Message}");
            }
        }

        private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            try
            {
                var assemblyName = args.LoadedAssembly.GetName().Name;
                if (assemblyName != null && (assemblyName.StartsWith("TreeSitter", StringComparison.OrdinalIgnoreCase)))
                {
                    Log("info", $"[NativeLibraryLoader] TreeSitter assembly loaded: {assemblyName}, setting up DllImportResolver");
                    SetupDllImportResolver(assemblyName);
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Error in OnAssemblyLoad: {ex.Message}");
            }
        }

        private static void AddNativeDllSearchPath()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log("info", "[NativeLibraryLoader] Not on Windows, skipping DLL search path setup (preload handles this)");
                return;
            }

            try
            {
var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                {
                    Log("error", "[NativeLibraryLoader] Failed to get AgentCore directory");
                    return;
                }

                string rid = GetRuntimeIdentifier();
                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                if (!Directory.Exists(nativeDir))
                {
                    Log("warning", $"[NativeLibraryLoader] Native directory not found: {nativeDir}");
                    return;
                }

                // Method 1: Use AddDllDirectory (preferred for Windows 8+)
                try
                {
                    IntPtr cookie = AddDllDirectory(nativeDir);
                    if (cookie != IntPtr.Zero)
                    {
                        Log("info", $"[NativeLibraryLoader] Added DLL directory via AddDllDirectory: {nativeDir}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log("warning", $"[NativeLibraryLoader] AddDllDirectory failed: {ex.Message}");
                }

                // Method 2: Fallback to SetDllDirectory (older Windows versions)
                try
                {
                    if (SetDllDirectory(nativeDir))
                    {
                        Log("info", $"[NativeLibraryLoader] Set DLL directory via SetDllDirectory: {nativeDir}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log("warning", $"[NativeLibraryLoader] SetDllDirectory failed: {ex.Message}");
                }

                // Method 3: Manually preload tree-sitter.dll
                try
                {
                    var treeSitterPath = Path.Combine(nativeDir, "tree-sitter.dll");
                    if (File.Exists(treeSitterPath))
                    {
                        if (NativeLibrary.TryLoad(treeSitterPath, out IntPtr handle))
                        {
                            Log("info", $"[NativeLibraryLoader] Preloaded tree-sitter.dll from: {treeSitterPath}");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("warning", $"[NativeLibraryLoader] Preload tree-sitter.dll failed: {ex.Message}");
                }

                Log("warning", "[NativeLibraryLoader] Warning: All DLL search path methods failed");
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Error in AddNativeDllSearchPath: {ex.Message}");
            }
        }

        private static readonly System.Collections.Generic.HashSet<string> _configuredAssemblies = new();

        private static void SetupDllImportResolver(string assemblyName)
        {
            lock (_lock)
            {
                try
                {
                    // Check if already configured
                    if (_configuredAssemblies.Contains(assemblyName))
                    {
                        Log("info", $"[NativeLibraryLoader] Assembly '{assemblyName}' already configured, skipping");
                        return;
                    }

                    // Find the assembly
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == assemblyName);

                    if (assembly == null)
                    {
                        Log("info", $"[NativeLibraryLoader] Assembly '{assemblyName}' not yet loaded, will configure when loaded");
                        return;
                    }

                    // Set up custom DLL import resolver
                    NativeLibrary.SetDllImportResolver(assembly, DllImportResolver);
                    _configuredAssemblies.Add(assemblyName);
                    Log("info", $"[NativeLibraryLoader] DLL import resolver set for '{assemblyName}'");
                }
                catch (Exception ex)
                {
                    Log("error", $"[NativeLibraryLoader] Failed to setup resolver for '{assemblyName}': {ex.Message}");
                }
            }
        }

        private static IntPtr _treeSitterHandle = IntPtr.Zero;

        private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // Log ALL DLL resolution attempts for debugging
            Log("debug", $"[NativeLibraryLoader] DllImportResolver called: libraryName='{libraryName}', assembly='{assembly.GetName().Name}'");

            try
            {
                Log("info", $"[NativeLibraryLoader] Resolving '{libraryName}' for assembly '{assembly.GetName().Name}'");

                // Get the directory where AgentCore.dll is located
var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                {
                    Log("error", "[NativeLibraryLoader] Failed to get AgentCore directory");
                    return IntPtr.Zero;
                }

                // Determine the RID (Runtime Identifier)
                string rid = GetRuntimeIdentifier();
                Log("info", $"[NativeLibraryLoader] Using RID: {rid}");

                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                // CRITICAL: If loading a tree-sitter language-specific library,
                // ensure the core tree-sitter library is loaded first as a dependency
                if (libraryName.StartsWith("tree-sitter-", StringComparison.OrdinalIgnoreCase) && _treeSitterHandle == IntPtr.Zero)
                {
                    var coreLibName = GetNativeLibraryFileName("tree-sitter");
                    var treeSitterPath = Path.Combine(nativeDir, coreLibName);
                    if (File.Exists(treeSitterPath))
                    {
                        Log("info", $"[NativeLibraryLoader] Preloading {coreLibName} dependency from: {treeSitterPath}");
                        if (NativeLibrary.TryLoad(treeSitterPath, out _treeSitterHandle))
                        {
                            Log("info", $"[NativeLibraryLoader] Successfully preloaded {coreLibName}");
                        }
                        else
                        {
                            Log("warning", $"[NativeLibraryLoader] WARNING: Failed to preload {coreLibName}");
                        }
                    }
                }

                // Construct the path to the native library
                // Path format: runtimes/{rid}/native/{platform-specific-name}
                var nativeDllPath = Path.Combine(nativeDir, GetNativeLibraryFileName(libraryName));

                if (!File.Exists(nativeDllPath))
                {
                    Log("warning", $"[NativeLibraryLoader] Native DLL not found at: {nativeDllPath}");
                    return IntPtr.Zero;
                }

                Log("info", $"[NativeLibraryLoader] Loading native DLL from: {nativeDllPath}");

                // Load the native library
                if (NativeLibrary.TryLoad(nativeDllPath, out IntPtr handle))
                {
                    Log("info", $"[NativeLibraryLoader] Successfully loaded '{libraryName}' from {nativeDllPath}");

                    // Cache tree-sitter core library handle
                    if (libraryName == "tree-sitter")
                    {
                        _treeSitterHandle = handle;
                    }

                    return handle;
                }
                else
                {
                    Log("error", $"[NativeLibraryLoader] Failed to load '{libraryName}' from {nativeDllPath}");
                    return IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Exception in DllImportResolver: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        private static string GetRuntimeIdentifier()
        {
            // Determine the runtime identifier based on OS and architecture
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arch = RuntimeInformation.ProcessArchitecture;
                return arch switch
                {
                    Architecture.X64 => "win-x64",
                    Architecture.X86 => "win-x86",
                    Architecture.Arm64 => "win-arm64",
                    _ => "win-x64" // Default to x64
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var arch = RuntimeInformation.ProcessArchitecture;
                return arch switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm64 => "linux-arm64",
                    _ => "linux-x64"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var arch = RuntimeInformation.ProcessArchitecture;
                return arch switch
                {
                    Architecture.X64 => "osx-x64",
                    Architecture.Arm64 => "osx-arm64",
                    _ => "osx-x64"
                };
            }

            return "win-x64"; // Default fallback
        }

        /// <summary>
        /// Check if native libraries are available
        /// </summary>
        public static bool CheckNativeLibraries()
        {
            try
            {
var agentCoreDir = GetAgentCoreDir();
                if (string.IsNullOrEmpty(agentCoreDir))
                    return false;

                string rid = GetRuntimeIdentifier();
                var runtimesDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                if (!Directory.Exists(runtimesDir))
                {
                    Log("warning", $"[NativeLibraryLoader] Runtimes directory not found: {runtimesDir}");
                    return false;
                }

                // Check for tree-sitter core library (required)
                var coreLib = Path.Combine(runtimesDir, GetNativeLibraryFileName("tree-sitter"));
                if (!File.Exists(coreLib))
                {
                    Log("warning", $"[NativeLibraryLoader] Required native library not found: {coreLib}");
                    return false;
                }

                Log("info", $"[NativeLibraryLoader] All required native libraries found in: {runtimesDir}");
                return true;
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Error checking native libraries: {ex.Message}");
                return false;
            }
        }
    }
}
