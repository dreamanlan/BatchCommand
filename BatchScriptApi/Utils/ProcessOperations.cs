using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;

namespace BatchCommand.Utils
{
    public class ProcessOperations
    {
        /// <summary>Process-wide shared instance (one per host).</summary>
        public static ProcessOperations Shared { get; } = new ProcessOperations();
        private readonly Dictionary<string, Process> _processes;
        private readonly object _lockObject = new object();
        // Active callback command tracking (for stuck detection / status query)
        private readonly ConcurrentDictionary<long, (string command, DateTime startTime)> _activeCallbackCommands = new();
        private long _callbackCommandIdCounter = 0;
        // start_process tracking (for status query only, no watchdog alarm)
        private readonly ConcurrentDictionary<string, DateTime> _processStartTimes = new();
        private readonly Timer _watchdogTimer;
        private const int c_watchdogIntervalMs = 30000;
        private const int c_defaultCommandTimeoutLogSeconds = 300;

        public ProcessOperations()
        {
            _processes = new Dictionary<string, Process>();
            _watchdogTimer = new Timer(WatchdogCallback, null, c_watchdogIntervalMs, c_watchdogIntervalMs);
        }

        public ProcessResult ExecuteCommand(string command, string? arguments = null, string? workingDirectory = null, int timeoutMs = 30000)
        {
            try {
                command = Environment.ExpandEnvironmentVariables(command);
                arguments = Environment.ExpandEnvironmentVariables(arguments ?? string.Empty);
                workingDirectory = Environment.ExpandEnvironmentVariables(workingDirectory ?? Directory.GetCurrentDirectory());
                var processInfo = new ProcessStartInfo {
                    FileName = command,
                    Arguments = arguments ?? string.Empty,
                    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                using (var process = new Process { StartInfo = processInfo }) {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data != null)
                            outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) => {
                        if (e.Data != null)
                            errorBuilder.AppendLine(e.Data);
                    };

                    var startTime = DateTime.Now;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool exited = process.WaitForExit(timeoutMs);
                    var endTime = DateTime.Now;

                    if (!exited) {
                        process.Kill();
                        return new ProcessResult {
                            Success = false,
                            ExitCode = -1,
                            Output = outputBuilder.ToString(),
                            Error = "Process timeout",
                            ExecutionTime = endTime - startTime
                        };
                    }

                    process.WaitForExit();
                    endTime = DateTime.Now;
                    return new ProcessResult {
                        Success = process.ExitCode == 0,
                        ExitCode = process.ExitCode,
                        Output = outputBuilder.ToString(),
                        Error = errorBuilder.ToString(),
                        ExecutionTime = endTime - startTime
                    };
                }
            }
            catch (Exception ex) {
                return new ProcessResult {
                    Success = false,
                    ExitCode = -1,
                    Output = string.Empty,
                    Error = $"Exception: {ex.Message}",
                    ExecutionTime = TimeSpan.Zero
                };
            }
        }

        public async Task<ProcessResult> ExecuteCommandAsync(string command, string? arguments = null, string? workingDirectory = null, int timeoutMs = 30000, CancellationToken cancellationToken = default)
        {
            try {
                command = Environment.ExpandEnvironmentVariables(command);
                arguments = Environment.ExpandEnvironmentVariables(arguments ?? string.Empty);
                workingDirectory = Environment.ExpandEnvironmentVariables(workingDirectory ?? Directory.GetCurrentDirectory());
                var processInfo = new ProcessStartInfo {
                    FileName = command,
                    Arguments = arguments ?? string.Empty,
                    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                using (var process = new Process { StartInfo = processInfo }) {
                    var exitedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    var outputCompletedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    var errorCompletedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data != null)
                            outputBuilder.AppendLine(e.Data);
                        else
                            outputCompletedTcs.TrySetResult(true);
                    };

                    process.ErrorDataReceived += (sender, e) => {
                        if (e.Data != null)
                            errorBuilder.AppendLine(e.Data);
                        else
                            errorCompletedTcs.TrySetResult(true);
                    };

                    process.Exited += (sender, args) => exitedTcs.TrySetResult(true);

                    var startTime = DateTime.Now;
                    process.Start();
                    process.EnableRaisingEvents = true;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    using (cancellationToken.Register(() => {
                        exitedTcs.TrySetCanceled();
                        try { process.Kill(); } catch { }
                    })) {
                        var completedTask = await Task.WhenAny(exitedTcs.Task, Task.Delay(timeoutMs, cancellationToken));
                        var endTime = DateTime.Now;

                        if (completedTask != exitedTcs.Task || exitedTcs.Task.IsCanceled) {
                            try { process.Kill(); } catch { }
                            return new ProcessResult {
                                Success = false,
                                ExitCode = -1,
                                Output = outputBuilder.ToString(),
                                Error = "Process timeout",
                                ExecutionTime = endTime - startTime
                            };
                        }

                        await Task.WhenAll(outputCompletedTcs.Task, errorCompletedTcs.Task);
                        endTime = DateTime.Now;
                        return new ProcessResult {
                            Success = process.ExitCode == 0,
                            ExitCode = process.ExitCode,
                            Output = outputBuilder.ToString(),
                            Error = errorBuilder.ToString(),
                            ExecutionTime = endTime - startTime
                        };
                    }
                }
            }
            catch (Exception ex) {
                return new ProcessResult {
                    Success = false,
                    ExitCode = -1,
                    Output = string.Empty,
                    Error = $"Exception: {ex.Message}",
                    ExecutionTime = TimeSpan.Zero
                };
            }
        }

        /// <summary>
        /// Executes a command asynchronously and sends the result to the originating
        /// connection via the callback queue. callbackMsg is used as the callback
        /// message name, resultJson as the argument. Returns immediately.
        /// </summary>
        public void ExecuteCommandWithCallback(string command, string? arguments, string? workingDirectory, int timeoutMs, string callbackMsg, string? cleanupFile = null)
        {
            object? ctx = HostBridge.CaptureCallbackContext?.Invoke();
            if (ctx == null) return;

            long cmdId = Interlocked.Increment(ref _callbackCommandIdCounter);
            _activeCallbackCommands[cmdId] = (command, DateTime.UtcNow);

            Task.Run(async () =>
            {
                try
                {
                    var result = await ExecuteCommandAsync(command, arguments, workingDirectory, timeoutMs);
                    string resultJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = result.Success,
                        exitCode = result.ExitCode,
                        output = result.Output,
                        error = result.Error,
                        executionTime = result.ExecutionTime.TotalMilliseconds
                    });
                    HostBridge.DeliverCallback?.Invoke(callbackMsg, new BoxedValue[] { command, arguments ?? string.Empty, workingDirectory ?? string.Empty, resultJson }, ctx);
                }
                catch (Exception ex)
                {
                    string errorJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        success = false,
                        exitCode = -1,
                        output = string.Empty,
                        error = $"Exception: {ex.Message}",
                        executionTime = 0.0
                    });
                    HostBridge.DeliverCallback?.Invoke(callbackMsg, new BoxedValue[] { command, arguments ?? string.Empty, workingDirectory ?? string.Empty, errorJson }, ctx);
                }
                finally
                {
                    _activeCallbackCommands.TryRemove(cmdId, out _);
                    if (cleanupFile != null)
                    {
                        try { File.Delete(cleanupFile); } catch { }
                    }
                }
            });
        }

        public string StartProcess(string processId, string command, string? arguments = null, string? workingDirectory = null)
        {
            lock (_lockObject) {
                command = Environment.ExpandEnvironmentVariables(command);
                arguments = Environment.ExpandEnvironmentVariables(arguments ?? string.Empty);
                workingDirectory = Environment.ExpandEnvironmentVariables(workingDirectory ?? Directory.GetCurrentDirectory());
                if (_processes.ContainsKey(processId)) {
                    throw new InvalidOperationException($"Process with ID '{processId}' already exists");
                }

                var processInfo = new ProcessStartInfo {
                    FileName = command,
                    Arguments = arguments ?? string.Empty,
                    WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                var process = new Process { StartInfo = processInfo };
                process.Start();

                _processes[processId] = process;
                _processStartTimes[processId] = DateTime.UtcNow;
                return processId;
            }
        }

        public bool StopProcess(string processId, int timeoutMs = 5000)
        {
            lock (_lockObject) {
                if (!_processes.ContainsKey(processId))
                    return false;

                var process = _processes[processId];
                try {
                    if (!process.HasExited) {
                        process.Kill();
                        bool exited = process.WaitForExit(timeoutMs);
                        if (!exited)
                            return false;
                    }

                    _processes.Remove(processId);
                    _processStartTimes.TryRemove(processId, out _);
                    process.Dispose();
                    return true;
                }
                catch {
                    return false;
                }
            }
        }

        public bool IsProcessRunning(string processId)
        {
            lock (_lockObject) {
                if (!_processes.ContainsKey(processId))
                    return false;

                var process = _processes[processId];
                return !process.HasExited;
            }
        }

        public string? ReadProcessOutput(string processId)
        {
            lock (_lockObject) {
                if (!_processes.ContainsKey(processId))
                    return null;

                var process = _processes[processId];
                try {
                    return process.StandardOutput.ReadToEnd();
                }
                catch {
                    return null;
                }
            }
        }

        public string? ReadProcessError(string processId)
        {
            lock (_lockObject) {
                if (!_processes.ContainsKey(processId))
                    return null;

                var process = _processes[processId];
                try {
                    return process.StandardError.ReadToEnd();
                }
                catch {
                    return null;
                }
            }
        }

        public bool WriteProcessInput(string processId, string input)
        {
            lock (_lockObject) {
                if (!_processes.ContainsKey(processId))
                    return false;

                var process = _processes[processId];
                try {
                    process.StandardInput.WriteLine(input);
                    process.StandardInput.Flush();
                    return true;
                }
                catch {
                    return false;
                }
            }
        }

        /// <summary>
        /// Generate a unique random file path in the temp directory with the specified extension.
        /// Uses Path.GetRandomFileName() with File.Exists check to avoid collisions.
        /// </summary>
        public static string GetUniqueRandomFilePath(string ext)
        {
            string tempDir = Path.GetTempPath();
            string filePath;
            int maxAttempts = 10;
            do {
                filePath = Path.Combine(tempDir, Path.GetRandomFileName() + ext);
            } while (File.Exists(filePath) && --maxAttempts > 0);
            if (File.Exists(filePath)) {
                throw new InvalidOperationException("Failed to generate a unique temp file path after multiple attempts.");
            }
            return filePath;
        }

        public void StopAllProcesses()
        {
            lock (_lockObject) {
                foreach (var kvp in _processes) {
                    try {
                        if (!kvp.Value.HasExited) {
                            kvp.Value.Kill();
                            kvp.Value.WaitForExit(5000);
                        }
                        kvp.Value.Dispose();
                    }
                    catch {
                        // Ignore errors during cleanup
                    }
                }
                _processes.Clear();
                _processStartTimes.Clear();
            }
        }

        /// <summary>
        /// Memory footprint in bytes of the current process (both arguments
        /// omitted), of a pid, or of the largest process carrying a name (with
        /// or without the .exe suffix). Returns 0 when nothing matches.
        /// </summary>
        /// <remarks>
        /// This is the number a memory guard has to watch. The JS heap alone is
        /// misleading: a long lived page reported ~1.3GB of heap while its
        /// renderer process held ~6GB, the difference being Blink objects and
        /// allocator pages that are never handed back. The renderer process runs
        /// managed code too, so the no-argument form reports the very process
        /// that is growing.
        /// </remarks>
        public long GetProcessMemoryBytes(int pid = 0, string? name = null)
        {
            try {
                if (pid > 0) {
                    try {
                        using var p = Process.GetProcessById(pid);
                        return MemoryBytes(p);
                    }
                    catch (ArgumentException) {
                        return 0; // process already gone
                    }
                }
                if (!string.IsNullOrEmpty(name)) {
                    var processName = name;
                    if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
                        processName = processName.Substring(0, processName.Length - 4);
                    }
                    long largest = 0;
                    foreach (var p in Process.GetProcessesByName(processName)) {
                        try {
                            long v = MemoryBytes(p);
                            if (v > largest) largest = v;
                        }
                        catch {
                            // access denied, or exited between enumeration and read
                        }
                        finally {
                            p.Dispose();
                        }
                    }
                    return largest;
                }
                using var self = Process.GetCurrentProcess();
                return MemoryBytes(self);
            }
            catch (Exception ex) {
                HostBridge.Log?.Invoke($"[ProcessOperations] GetProcessMemoryBytes failed: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Private bytes on Windows (the real footprint), working set elsewhere
        /// where private bytes is not available. Counters are cached on the
        /// Process instance, so a refresh is needed for a current reading.
        /// </summary>
        private static long MemoryBytes(Process p)
        {
            p.Refresh();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                try {
                    return p.PrivateMemorySize64;
                }
                catch {
                    return p.WorkingSet64;
                }
            }
            return p.WorkingSet64;
        }

        public List<string> GetRunningProcessIds()
        {
            lock (_lockObject) {
                var runningIds = new List<string>();
                foreach (var kvp in _processes) {
                    if (!kvp.Value.HasExited) {
                        runningIds.Add(kvp.Key);
                    }
                }
                return runningIds;
            }
        }

        /// <summary>
        /// Returns a status string of all active callback commands and started processes.
        /// </summary>
        public string GetActiveCommandStatus()
        {
            var now = DateTime.UtcNow;
            var sb = new StringBuilder();
            int stuck = 0;
            foreach (var kv in _activeCallbackCommands)
            {
                int duration = (int)(now - kv.Value.startTime).TotalSeconds;
                sb.AppendLine($"cmd {kv.Key}: {duration}s - {kv.Value.command}");
                if (duration > c_defaultCommandTimeoutLogSeconds) stuck++;
            }
            foreach (var kv in _processStartTimes)
            {
                int duration = (int)(now - kv.Value).TotalSeconds;
                int? pid = null;
                lock (_lockObject)
                {
                    if (_processes.TryGetValue(kv.Key, out var p) && !p.HasExited)
                        pid = p.Id;
                }
                string pidStr = pid.HasValue ? $" pid={pid.Value}" : "";
                sb.AppendLine($"proc {kv.Key}: {duration}s{pidStr}");
            }
            sb.AppendLine($"callback cmds: {_activeCallbackCommands.Count}, stuck(>{c_defaultCommandTimeoutLogSeconds}s): {stuck}, started procs: {_processStartTimes.Count}");
            return sb.ToString().TrimEnd();
        }

        /// <summary>Watchdog callback: logs warnings for long-running callback commands.</summary>
        private void WatchdogCallback(object? state)
        {
            try
            {
                var now = DateTime.UtcNow;
                foreach (var kv in _activeCallbackCommands)
                {
                    int duration = (int)(now - kv.Value.startTime).TotalSeconds;
                    if (duration > c_defaultCommandTimeoutLogSeconds)
                    {
                        HostBridge.Log?.Invoke($"[ProcessOperations] Callback command {kv.Key} running for {duration}s: {kv.Value.command}");
                    }
                }
            }
            catch (Exception ex)
            {
                HostBridge.Log?.Invoke($"[ProcessOperations] Watchdog error: {ex.Message}");
            }
        }
    }

    public class ProcessResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public TimeSpan ExecutionTime { get; set; }
    }
}
