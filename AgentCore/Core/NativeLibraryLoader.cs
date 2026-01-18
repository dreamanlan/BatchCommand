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
        /// Initialize native library loading for TreeSitter
        /// Must be called before any TreeSitter functionality is used
        /// </summary>
        public static void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                    return;

                try
                {
                    // Preload all TreeSitter native DLLs from the native directory
                    PreloadTreeSitterDlls();

                    // Add native DLL directory to Windows DLL search path
                    AddNativeDllSearchPath();

                    // Set up AssemblyLoad event handler to automatically configure DllImportResolver
                    // when TreeSitter assemblies are loaded
                    AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
                    Log("info", "[NativeLibraryLoader] Registered AssemblyLoad event handler for TreeSitter assemblies");

                    // Try to set up resolvers for already-loaded assemblies
                    SetupDllImportResolver("TreeSitterSharp");
                    SetupDllImportResolver("TreeSitterSharp.C");
                    SetupDllImportResolver("TreeSitterSharp.Cpp");

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

        private static void PreloadTreeSitterDlls()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Log("info", "[NativeLibraryLoader] Not on Windows, skipping DLL preload");
                return;
            }

            try
            {
                var agentCoreDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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

                // Preload DLLs in dependency order: tree-sitter.dll first, then language-specific DLLs
                string[] dllsToPreload = { "tree-sitter.dll", "tree-sitter-c.dll", "tree-sitter-cpp.dll" };

                foreach (var dllName in dllsToPreload)
                {
                    var dllPath = Path.Combine(nativeDir, dllName);
                    if (!File.Exists(dllPath))
                    {
                        Log("warning", $"[NativeLibraryLoader] DLL not found for preload: {dllPath}");
                        continue;
                    }

                    try
                    {
                        if (NativeLibrary.TryLoad(dllPath, out IntPtr handle))
                        {
                            Log("info", $"[NativeLibraryLoader] Successfully preloaded: {dllName} from {dllPath}");

                            // Cache tree-sitter.dll handle
                            if (dllName == "tree-sitter.dll")
                            {
                                _treeSitterHandle = handle;
                            }
                        }
                        else
                        {
                            Log("warning", $"[NativeLibraryLoader] Failed to preload: {dllName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("error", $"[NativeLibraryLoader] Exception preloading {dllName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Error in PreloadTreeSitterDlls: {ex.Message}");
            }
        }

        private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            try
            {
                var assemblyName = args.LoadedAssembly.GetName().Name;
                if (assemblyName != null && assemblyName.StartsWith("TreeSitterSharp", StringComparison.OrdinalIgnoreCase))
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
                Log("info", "[NativeLibraryLoader] Not on Windows, skipping DLL search path setup");
                return;
            }

            try
            {
                var agentCoreDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
                // Only handle tree-sitter related DLLs
                if (!libraryName.StartsWith("tree-sitter", StringComparison.OrdinalIgnoreCase))
                {
                    Log("debug", $"[NativeLibraryLoader] Skipping '{libraryName}' (not a tree-sitter DLL)");
                    return IntPtr.Zero; // Let default resolver handle it
                }

                Log("info", $"[NativeLibraryLoader] Resolving '{libraryName}' for assembly '{assembly.GetName().Name}'");

                // Get the directory where AgentCore.dll is located
                var agentCoreDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(agentCoreDir))
                {
                    Log("error", "[NativeLibraryLoader] Failed to get AgentCore directory");
                    return IntPtr.Zero;
                }

                // Determine the RID (Runtime Identifier)
                string rid = GetRuntimeIdentifier();
                Log("info", $"[NativeLibraryLoader] Using RID: {rid}");

                var nativeDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                // CRITICAL: If loading a language-specific DLL (tree-sitter-c, tree-sitter-cpp),
                // ensure tree-sitter.dll is loaded first as a dependency
                if (libraryName != "tree-sitter" && _treeSitterHandle == IntPtr.Zero)
                {
                    var treeSitterPath = Path.Combine(nativeDir, "tree-sitter.dll");
                    if (File.Exists(treeSitterPath))
                    {
                        Log("info", $"[NativeLibraryLoader] Preloading tree-sitter.dll dependency from: {treeSitterPath}");
                        if (NativeLibrary.TryLoad(treeSitterPath, out _treeSitterHandle))
                        {
                            Log("info", $"[NativeLibraryLoader] Successfully preloaded tree-sitter.dll");
                        }
                        else
                        {
                            Log("warning", $"[NativeLibraryLoader] WARNING: Failed to preload tree-sitter.dll");
                        }
                    }
                }

                // Construct the path to the native DLL
                // Path format: runtimes/{rid}/native/{libraryName}.dll
                var nativeDllPath = Path.Combine(nativeDir, $"{libraryName}.dll");

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

                    // Cache tree-sitter.dll handle
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
                var agentCoreDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(agentCoreDir))
                    return false;

                string rid = GetRuntimeIdentifier();
                var runtimesDir = Path.Combine(agentCoreDir, "runtimes", rid, "native");

                if (!Directory.Exists(runtimesDir))
                {
                    Log("warning", $"[NativeLibraryLoader] Runtimes directory not found: {runtimesDir}");
                    return false;
                }

                // Check for required DLLs
                string[] requiredDlls = { "tree-sitter.dll", "tree-sitter-c.dll", "tree-sitter-cpp.dll" };
                foreach (var dll in requiredDlls)
                {
                    var dllPath = Path.Combine(runtimesDir, dll);
                    if (!File.Exists(dllPath))
                    {
                        Log("warning", $"[NativeLibraryLoader] Required DLL not found: {dllPath}");
                        return false;
                    }
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
