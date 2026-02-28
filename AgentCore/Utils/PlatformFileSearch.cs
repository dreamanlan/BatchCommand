using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CefDotnetApp.AgentCore.Utils
{
    /// <summary>
    /// Cross-platform file search utility.
    /// macOS: uses mdfind (Spotlight index).
    /// Linux: uses locate (mlocate/plocate), falls back to find.
    /// Windows: not handled here, use EverythingSDK directly.
    /// </summary>
    public static class PlatformFileSearch
    {
        /// <summary>
        /// Search files on macOS or Linux.
        /// Returns a list of full paths found.
        /// </summary>
        public static List<string> Search(string query, uint maxCount)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return SearchMacOS(query, maxCount);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return SearchLinux(query, maxCount);
            return new List<string>();
        }

        /// <summary>
        /// macOS: mdfind -name "query"
        /// mdfind uses Spotlight index, very fast.
        /// </summary>
        private static List<string> SearchMacOS(string query, uint maxCount)
        {
            // mdfind does not have a built-in limit flag, we read lines up to maxCount
            return RunCommandAndCollect("mdfind", $"-name \"{EscapeShellArg(query)}\"", maxCount);
        }

        /// <summary>
        /// Linux: try locate first, fallback to find.
        /// locate uses a pre-built database (updatedb), very fast.
        /// find is slower but always available.
        /// </summary>
        private static List<string> SearchLinux(string query, uint maxCount)
        {
            // Try locate first (with limit flag)
            if (CommandExists("locate")) {
                return RunCommandAndCollect("locate", $"-i -l {maxCount} \"{EscapeShellArg(query)}\"", maxCount);
            }
            // Fallback to find from home directory (limited scope for performance)
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(homeDir))
                homeDir = "/";
            return RunCommandAndCollect("find", $"\"{homeDir}\" -iname \"*{EscapeShellArg(query)}*\" -maxdepth 5 2>/dev/null", maxCount);
        }

        /// <summary>
        /// Run a command and collect output lines up to maxCount.
        /// </summary>
        private static List<string> RunCommandAndCollect(string command, string arguments, uint maxCount)
        {
            var results = new List<string>();
            try {
                var psi = new ProcessStartInfo {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi)) {
                    if (proc == null)
                        return results;
                    // Read lines from stdout
                    string? line;
                    while ((line = proc.StandardOutput.ReadLine()) != null) {
                        if (!string.IsNullOrWhiteSpace(line)) {
                            results.Add(line.Trim());
                            if (results.Count >= maxCount)
                                break;
                        }
                    }
                    // Don't wait forever
                    if (!proc.HasExited)
                        proc.Kill();
                    proc.WaitForExit(c_TimeoutMs);
                }
            }
            catch {
                // Silently fail - command not available or other error
            }
            return results;
        }

        /// <summary>
        /// Check if a command exists on the system (Linux/macOS).
        /// </summary>
        private static bool CommandExists(string command)
        {
            try {
                var psi = new ProcessStartInfo {
                    FileName = "which",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi)) {
                    if (proc == null)
                        return false;
                    proc.WaitForExit(3000);
                    return proc.ExitCode == 0;
                }
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Escape special characters for shell arguments.
        /// </summary>
        private static string EscapeShellArg(string arg)
        {
            if (arg == null)
                return string.Empty;
            // Remove double quotes and backslashes to prevent injection
            return arg.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$").Replace("`", "\\`");
        }

        /// <summary>
        /// Get file info (size, modified time) for a given path.
        /// Used to provide consistent output across platforms.
        /// </summary>
        public static (long size, DateTime modified) GetFileInfo(string path)
        {
            try {
                var fi = new FileInfo(path);
                if (fi.Exists)
                    return (fi.Length, fi.LastWriteTime);
                // For directories
                var di = new DirectoryInfo(path);
                if (di.Exists)
                    return (0, di.LastWriteTime);
            }
            catch {
                // Ignore
            }
            return (-1, DateTime.MinValue);
        }

        private const int c_TimeoutMs = 10000;
    }
}
