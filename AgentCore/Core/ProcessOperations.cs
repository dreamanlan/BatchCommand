using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class ProcessOperations : IProcessOperations
    {
        private readonly Dictionary<string, Process> _processes;
        private readonly object _lockObject = new object();

        public ProcessOperations()
        {
            _processes = new Dictionary<string, Process>();
        }

        public ProcessResult ExecuteCommand(string command, string arguments = null, string workingDirectory = null, int timeoutMs = 30000)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
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

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            errorBuilder.AppendLine(e.Data);
                    };

                    var startTime = DateTime.Now;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool exited = process.WaitForExit(timeoutMs);
                    var endTime = DateTime.Now;

                    if (!exited)
                    {
                        process.Kill();
                        return new ProcessResult
                        {
                            Success = false,
                            ExitCode = -1,
                            Output = outputBuilder.ToString(),
                            Error = "Process timeout",
                            ExecutionTime = endTime - startTime
                        };
                    }

                    return new ProcessResult
                    {
                        Success = process.ExitCode == 0,
                        ExitCode = process.ExitCode,
                        Output = outputBuilder.ToString(),
                        Error = errorBuilder.ToString(),
                        ExecutionTime = endTime - startTime
                    };
                }
            }
            catch (Exception ex)
            {
                return new ProcessResult
                {
                    Success = false,
                    ExitCode = -1,
                    Output = string.Empty,
                    Error = $"Exception: {ex.Message}",
                    ExecutionTime = TimeSpan.Zero
                };
            }
        }

        public async Task<ProcessResult> ExecuteCommandAsync(string command, string arguments = null, string workingDirectory = null, int timeoutMs = 30000, CancellationToken cancellationToken = default)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
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

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            outputBuilder.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                            errorBuilder.AppendLine(e.Data);
                    };

                    var startTime = DateTime.Now;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    var tcs = new TaskCompletionSource<bool>();
                    process.EnableRaisingEvents = true;
                    process.Exited += (sender, args) => tcs.TrySetResult(true);

                    using (cancellationToken.Register(() =>
                    {
                        tcs.TrySetCanceled();
                        try { process.Kill(); } catch { }
                    }))
                    {
                        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs, cancellationToken));
                        var endTime = DateTime.Now;

                        if (completedTask != tcs.Task)
                        {
                            try { process.Kill(); } catch { }
                            return new ProcessResult
                            {
                                Success = false,
                                ExitCode = -1,
                                Output = outputBuilder.ToString(),
                                Error = "Process timeout",
                                ExecutionTime = endTime - startTime
                            };
                        }

                        return new ProcessResult
                        {
                            Success = process.ExitCode == 0,
                            ExitCode = process.ExitCode,
                            Output = outputBuilder.ToString(),
                            Error = errorBuilder.ToString(),
                            ExecutionTime = endTime - startTime
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ProcessResult
                {
                    Success = false,
                    ExitCode = -1,
                    Output = string.Empty,
                    Error = $"Exception: {ex.Message}",
                    ExecutionTime = TimeSpan.Zero
                };
            }
        }

        public string StartProcess(string processId, string command, string arguments = null, string workingDirectory = null)
        {
            lock (_lockObject)
            {
                if (_processes.ContainsKey(processId))
                {
                    throw new InvalidOperationException($"Process with ID '{processId}' already exists");
                }

                var processInfo = new ProcessStartInfo
                {
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
                return processId;
            }
        }

        public bool StopProcess(string processId, int timeoutMs = 5000)
        {
            lock (_lockObject)
            {
                if (!_processes.ContainsKey(processId))
                    return false;

                var process = _processes[processId];
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        bool exited = process.WaitForExit(timeoutMs);
                        if (!exited)
                            return false;
                    }

                    _processes.Remove(processId);
                    process.Dispose();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool IsProcessRunning(string processId)
        {
            lock (_lockObject)
            {
                if (!_processes.ContainsKey(processId))
                    return false;

                var process = _processes[processId];
                return !process.HasExited;
            }
        }

        public string ReadProcessOutput(string processId)
        {
            lock (_lockObject)
            {
                if (!_processes.ContainsKey(processId))
                    return null;

                var process = _processes[processId];
                try
                {
                    return process.StandardOutput.ReadToEnd();
                }
                catch
                {
                    return null;
                }
            }
        }

        public string ReadProcessError(string processId)
        {
            lock (_lockObject)
            {
                if (!_processes.ContainsKey(processId))
                    return null;

                var process = _processes[processId];
                try
                {
                    return process.StandardError.ReadToEnd();
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool WriteProcessInput(string processId, string input)
        {
            lock (_lockObject)
            {
                if (!_processes.ContainsKey(processId))
                    return false;

                var process = _processes[processId];
                try
                {
                    process.StandardInput.WriteLine(input);
                    process.StandardInput.Flush();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void StopAllProcesses()
        {
            lock (_lockObject)
            {
                foreach (var kvp in _processes)
                {
                    try
                    {
                        if (!kvp.Value.HasExited)
                        {
                            kvp.Value.Kill();
                            kvp.Value.WaitForExit(5000);
                        }
                        kvp.Value.Dispose();
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }
                _processes.Clear();
            }
        }

        public List<string> GetRunningProcessIds()
        {
            lock (_lockObject)
            {
                var runningIds = new List<string>();
                foreach (var kvp in _processes)
                {
                    if (!kvp.Value.HasExited)
                    {
                        runningIds.Add(kvp.Key);
                    }
                }
                return runningIds;
            }
        }
    }

    public class ProcessResult
    {
        public bool Success { get; set; }
        public int ExitCode { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }
}
