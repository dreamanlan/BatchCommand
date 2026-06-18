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

        // Cached handle to the loaded onnxruntime native library, used by DllImportResolver
        // to satisfy [DllImport("onnxruntime")] calls in Microsoft.ML.OnnxRuntime assembly.
        private static IntPtr _onnxRuntimeHandle = IntPtr.Zero;

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

                    // On macOS, the libonnxruntime.dylib shipped by Microsoft.ML.OnnxRuntime
                    // has its install_name set to "@rpath/libonnxruntime.X.Y.Z.dylib" (versioned),
                    // but only the unversioned file exists on disk. As a result, dyld registers the
                    // loaded module under the versioned name, and CoreCLR's [DllImport("onnxruntime")]
                    // (which probes for the unversioned "libonnxruntime.dylib") cannot reuse the
                    // preloaded handle and re-resolves from disk -> dyld then chases the @rpath
                    // versioned name and fails to find it -> NativeMethods cctor throws.
                    //
                    // Fix: ensure a sibling file exists with the install_name's basename so dyld
                    // can satisfy the rpath lookup. We use a hard link (or copy as fallback) to
                    // avoid having to re-codesign the dylib (which would be required if we used
                    // install_name_tool to rewrite the id).
                    if (baseName == "onnxruntime" && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        TryMaterializeOnnxRuntimeInstallNameAlias(libPath);
                    }

                    if (NativeLibrary.TryLoad(libPath, out IntPtr handle))
                    {
                        Log("info", $"[NativeLibraryLoader] Successfully preloaded {libName} from {libPath}");
                        if (baseName == "onnxruntime")
                            _onnxRuntimeHandle = handle;
                    }
                    else
                    {
                        Log("warning", $"[NativeLibraryLoader] Failed to preload {libName} from {libPath}");
                    }
                }

                // Plan B: register a DllImportResolver for Microsoft.ML.OnnxRuntime so that any
                // [DllImport("onnxruntime")] in NativeMethods is satisfied by the preloaded handle.
                // This works regardless of install_name / RID-based probing quirks, especially in
                // HostCLR environments where the assembly may be loaded from byte arrays
                // (Assembly.Location is empty, so the runtime can't infer runtimes/<rid>/native).
                try
                {
                    RegisterOnnxRuntimeDllImportResolver();
                }
                catch (Exception resolverEx)
                {
                    Log("warning", $"[NativeLibraryLoader] Failed to register OnnxRuntime DllImportResolver: {resolverEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Log("error", $"[NativeLibraryLoader] Exception preloading OnnxRuntime native libraries: {ex.Message}");
            }
        }

        /// <summary>
        /// On macOS, read the dylib's install_name (LC_ID_DYLIB) and, if it points to a
        /// different basename in @rpath (e.g. "libonnxruntime.1.20.1.dylib"), create a
        /// hard link (or copy) next to <paramref name="libPath"/> so dyld can resolve it.
        /// Safe to call multiple times; no-op if alias already exists or install_name matches.
        /// </summary>
        private static void TryMaterializeOnnxRuntimeInstallNameAlias(string libPath)
        {
            try
            {
                string? installName = ReadDylibInstallName(libPath);
                if (string.IsNullOrEmpty(installName))
                {
                    Log("info", $"[NativeLibraryLoader] Could not read install_name from {libPath}, skipping alias materialization");
                    return;
                }

                string installBaseName = Path.GetFileName(installName);
                string actualBaseName = Path.GetFileName(libPath);
                if (string.Equals(installBaseName, actualBaseName, StringComparison.Ordinal))
                {
                    // install_name already matches the on-disk filename, nothing to do
                    return;
                }

                string? nativeDir = Path.GetDirectoryName(libPath);
                if (string.IsNullOrEmpty(nativeDir))
                    return;

                string aliasPath = Path.Combine(nativeDir, installBaseName);
                if (File.Exists(aliasPath))
                {
                    Log("info", $"[NativeLibraryLoader] OnnxRuntime install_name alias already exists: {aliasPath}");
                    return;
                }

                Log("info", $"[NativeLibraryLoader] Creating OnnxRuntime install_name alias '{installBaseName}' -> '{actualBaseName}' in {nativeDir}");

                // Try hard link first (cheap, no extra disk space, no re-codesign needed)
                if (TryCreateHardLink(libPath, aliasPath))
                {
                    Log("info", $"[NativeLibraryLoader] Successfully created hard link: {aliasPath}");
                    return;
                }

                // Fallback: copy the file
                try
                {
                    File.Copy(libPath, aliasPath, overwrite: false);
                    Log("info", $"[NativeLibraryLoader] Successfully copied dylib to alias: {aliasPath}");
                }
                catch (Exception copyEx)
                {
                    Log("warning", $"[NativeLibraryLoader] Failed to copy dylib alias '{aliasPath}': {copyEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Log("warning", $"[NativeLibraryLoader] TryMaterializeOnnxRuntimeInstallNameAlias failed: {ex.Message}");
            }
        }

        // P/Invoke into libc's link(2) for hard link creation on POSIX systems.
        [DllImport("libc", EntryPoint = "link", SetLastError = true)]
        private static extern int PosixLink(string oldpath, string newpath);

        private static bool TryCreateHardLink(string source, string target)
        {
            try
            {
                int rc = PosixLink(source, target);
                return rc == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read the LC_ID_DYLIB install_name from a Mach-O dylib file.
        /// Supports both 32/64-bit and fat (universal) binaries; returns the install_name
        /// from the first matching arch slice. Returns null on any parse error.
        /// </summary>
        private static string? ReadDylibInstallName(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var br = new BinaryReader(fs);

                uint magic = br.ReadUInt32();

                // Fat / universal binary: walk arch slices and parse the first one we can read.
                // FAT_MAGIC = 0xCAFEBABE (big-endian on disk), FAT_MAGIC_64 = 0xCAFEBABF.
                if (magic == 0xBEBAFECA || magic == 0xBFBAFECA)
                {
                    bool is64 = magic == 0xBFBAFECA;
                    uint nfat = ReadUInt32BigEndian(br);
                    for (int i = 0; i < nfat; i++)
                    {
                        // fat_arch / fat_arch_64
                        br.ReadUInt32(); // cputype
                        br.ReadUInt32(); // cpusubtype
                        ulong offset = is64 ? ReadUInt64BigEndian(br) : ReadUInt32BigEndian(br);
                        if (is64) ReadUInt64BigEndian(br); else ReadUInt32BigEndian(br); // size
                        br.ReadUInt32(); // align
                        if (is64) br.ReadUInt32(); // reserved

                        long savedPos = fs.Position;
                        fs.Seek((long)offset, SeekOrigin.Begin);
                        string? name = ReadInstallNameFromMachOSlice(br);
                        if (!string.IsNullOrEmpty(name))
                            return name;
                        fs.Seek(savedPos, SeekOrigin.Begin);
                    }
                    return null;
                }

                // Thin Mach-O: rewind and parse from start
                fs.Seek(0, SeekOrigin.Begin);
                return ReadInstallNameFromMachOSlice(br);
            }
            catch
            {
                return null;
            }
        }

        private static uint ReadUInt32BigEndian(BinaryReader br)
        {
            byte[] b = br.ReadBytes(4);
            return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        }

        private static ulong ReadUInt64BigEndian(BinaryReader br)
        {
            byte[] b = br.ReadBytes(8);
            return ((ulong)b[0] << 56) | ((ulong)b[1] << 48) | ((ulong)b[2] << 40) | ((ulong)b[3] << 32)
                 | ((ulong)b[4] << 24) | ((ulong)b[5] << 16) | ((ulong)b[6] << 8) | b[7];
        }

        private static string? ReadInstallNameFromMachOSlice(BinaryReader br)
        {
            // Mach-O magic numbers (host endian, 32 / 64 bit)
            const uint MH_MAGIC = 0xFEEDFACE;
            const uint MH_CIGAM = 0xCEFAEDFE;
            const uint MH_MAGIC_64 = 0xFEEDFACF;
            const uint MH_CIGAM_64 = 0xCFFAEDFE;
            const uint LC_ID_DYLIB = 0x0D;

            uint magic = br.ReadUInt32();
            bool is64;
            bool swap;
            if (magic == MH_MAGIC) { is64 = false; swap = false; }
            else if (magic == MH_MAGIC_64) { is64 = true; swap = false; }
            else if (magic == MH_CIGAM) { is64 = false; swap = true; }
            else if (magic == MH_CIGAM_64) { is64 = true; swap = true; }
            else return null;

            // mach_header(_64) remaining fields:
            //   cputype, cpusubtype, filetype, ncmds, sizeofcmds, flags [, reserved (64-bit)]
            br.ReadUInt32(); // cputype
            br.ReadUInt32(); // cpusubtype
            br.ReadUInt32(); // filetype
            uint ncmds = swap ? ReadUInt32BigEndian(br) : br.ReadUInt32();
            br.ReadUInt32(); // sizeofcmds
            br.ReadUInt32(); // flags
            if (is64) br.ReadUInt32(); // reserved

            for (int i = 0; i < ncmds; i++)
            {
                long cmdStart = br.BaseStream.Position;
                uint cmd = swap ? ReadUInt32BigEndian(br) : br.ReadUInt32();
                uint cmdSize = swap ? ReadUInt32BigEndian(br) : br.ReadUInt32();
                if (cmdSize < 8) return null;

                if (cmd == LC_ID_DYLIB)
                {
                    // dylib_command:
                    //   uint32 cmd; uint32 cmdsize;
                    //   struct dylib { union lc_str name; uint32 timestamp; uint32 current_version; uint32 compatibility_version; }
                    uint nameOffset = swap ? ReadUInt32BigEndian(br) : br.ReadUInt32();
                    br.ReadUInt32(); // timestamp
                    br.ReadUInt32(); // current_version
                    br.ReadUInt32(); // compatibility_version

                    long nameAbs = cmdStart + nameOffset;
                    long nameMaxLen = cmdStart + cmdSize - nameAbs;
                    if (nameMaxLen <= 0) return null;

                    br.BaseStream.Seek(nameAbs, SeekOrigin.Begin);
                    byte[] buf = br.ReadBytes((int)nameMaxLen);
                    int zero = Array.IndexOf<byte>(buf, 0);
                    int len = zero >= 0 ? zero : buf.Length;
                    return System.Text.Encoding.UTF8.GetString(buf, 0, len);
                }

                // Skip to next command
                br.BaseStream.Seek(cmdStart + cmdSize, SeekOrigin.Begin);
            }

            return null;
        }

        /// <summary>
        /// Register a DllImportResolver for Microsoft.ML.OnnxRuntime so that
        /// [DllImport("onnxruntime")] inside NativeMethods is resolved to the
        /// preloaded native handle (cached in <see cref="_onnxRuntimeHandle"/>).
        /// </summary>
        private static void RegisterOnnxRuntimeDllImportResolver()
        {
            if (_onnxRuntimeHandle == IntPtr.Zero)
                return;

            // Register resolver for already-loaded Microsoft.ML.OnnxRuntime assembly
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = asm.GetName().Name;
                if (name != null && name.Equals("Microsoft.ML.OnnxRuntime", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        NativeLibrary.SetDllImportResolver(asm, OnnxRuntimeDllImportResolver);
                        Log("info", $"[NativeLibraryLoader] Registered OnnxRuntime DllImportResolver for {name}");
                    }
                    catch (InvalidOperationException)
                    {
                        Log("info", $"[NativeLibraryLoader] DllImportResolver already set for {name}");
                    }
                }
            }

            // Also register for future assembly loads
            AppDomain.CurrentDomain.AssemblyLoad += OnOnnxRuntimeAssemblyLoad;
        }

        private static void OnOnnxRuntimeAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
            var name = args.LoadedAssembly.GetName().Name;
            if (name != null && name.Equals("Microsoft.ML.OnnxRuntime", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    NativeLibrary.SetDllImportResolver(args.LoadedAssembly, OnnxRuntimeDllImportResolver);
                    Log("info", $"[NativeLibraryLoader] Registered OnnxRuntime DllImportResolver for late-loaded {name}");
                }
                catch (InvalidOperationException)
                {
                    // Already set
                }
            }
        }

        private static IntPtr OnnxRuntimeDllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // Microsoft.ML.OnnxRuntime uses [DllImport("onnxruntime")] for all P/Invoke.
            // Return the cached handle directly; this avoids dyld's @rpath resolution
            // chasing a versioned filename that doesn't exist on disk.
            if (_onnxRuntimeHandle != IntPtr.Zero &&
                string.Equals(libraryName, "onnxruntime", StringComparison.Ordinal))
            {
                return _onnxRuntimeHandle;
            }
            return IntPtr.Zero;
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
