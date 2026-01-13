using System;
using System.IO;
using System.Collections.Generic;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Monitor file changes and trigger hot reload
    /// </summary>
    public class HotReloadManager
    {
        private string _basePath;
        private Dictionary<string, FileSystemWatcher> _watchers;
        private Dictionary<string, DateTime> _lastModified;
        private bool _enabled;
        private Action<string, string>? _onFileChanged;
        private LoggingAndDebugging _logger;

        public HotReloadManager(string basePath, LoggingAndDebugging logger = null)
        {
            _basePath = basePath;
            _watchers = new Dictionary<string, FileSystemWatcher>();
            _lastModified = new Dictionary<string, DateTime>();
            _enabled = false;
            _logger = logger ?? new LoggingAndDebugging();
        }

        public void SetCallback(Action<string, string> onFileChanged)
        {
            _onFileChanged = onFileChanged;
        }

        public void StartWatching()
        {
            if (_enabled)
                return;

            _enabled = true;

            // Watch AgentCore.dll
            WatchFile("AgentCore.dll", "managed", "AgentCore DLL");

            // Watch Script.dsl
            WatchFile("Script.dsl", "managed", "DSL Script");

            // Watch inject.js
            WatchFile("inject.js", "", "Inject Script");
        }

        public void StopWatching()
        {
            if (!_enabled)
                return;

            _enabled = false;

            foreach (var watcher in _watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            _watchers.Clear();
        }

        private void WatchFile(string fileName, string relativePath, string fileType)
        {
            try
            {
                string fullPath = string.IsNullOrEmpty(relativePath)
                    ? Path.Combine(_basePath, fileName)
                    : Path.Combine(_basePath, relativePath, fileName);

                string directory = Path.GetDirectoryName(fullPath) ?? _basePath;

                if (!Directory.Exists(directory))
                {
                    _logger.Warning($"[HotReload] Directory not found: {directory}");
                    return;
                }

                var watcher = new FileSystemWatcher(directory)
                {
                    Filter = fileName,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                watcher.Changed += (sender, e) => OnFileChanged(e.FullPath, fileType);
                watcher.EnableRaisingEvents = true;

                _watchers[fullPath] = watcher;
                _lastModified[fullPath] = File.GetLastWriteTime(fullPath);

                _logger.Info($"[HotReload] Watching: {fileType} at {fullPath}");
            }
            catch (Exception ex)
            {
                _logger.Error($"[HotReload] Failed to watch {fileType}: {ex.Message}");
            }
        }

        private void OnFileChanged(string filePath, string fileType)
        {
            try
            {
                // Debounce: wait a moment for file write to complete
                System.Threading.Thread.Sleep(200);

                var currentModified = File.GetLastWriteTime(filePath);

                // Check if file actually changed (avoid duplicate events)
                if (_lastModified.TryGetValue(filePath, out var lastModified) &&
                    currentModified <= lastModified.AddMilliseconds(100))
                {
                    return;
                }

                _lastModified[filePath] = currentModified;

                _logger.Info($"[HotReload] {fileType} changed: {filePath}");

                _onFileChanged?.Invoke(filePath, fileType);
            }
            catch (Exception ex)
            {
                _logger.Error($"[HotReload] Error handling file change: {ex.Message}");
            }
        }

        public bool IsEnabled => _enabled;
    }
}
