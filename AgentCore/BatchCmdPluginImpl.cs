using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ScriptableFramework;
using BatchCommand;
using BatchCommand.Utils;
using BatchCmdDsl;
using AgentCore.Core;
using System.Runtime.InteropServices;

namespace AgentCore
{
    /// <summary>
    /// Implementation of IBatchCmdPlugin interface for agent functionality
    /// </summary>
    public class BatchCmdPluginImpl : IBatchCmdPlugin
    {
        private string _basePath = string.Empty;
        private string _appDir = string.Empty;
        private bool _isMac;
        private bool _isInitialized;

        public void Initialize(string basePath, string appDir, bool isMac)
        {
            if (_isInitialized)
                return;

            _basePath = basePath ?? System.IO.Directory.GetCurrentDirectory();
            _appDir = appDir ?? System.IO.Directory.GetCurrentDirectory();
            _isMac = isMac;

            // Initialize the AgentCore singleton if not already initialized
            if (!Core.AgentCore.IsInitialized) {
                Core.AgentCore.Initialize(_basePath, _appDir, isMac);
            }

            _isInitialized = true;
            Core.AgentCore.Instance.Logger.Info($"AgentPlugin initialized at {_basePath}");

            // scan skills directory
            var skillsDir = System.IO.Path.Combine(_basePath, "skills");
            Core.AgentCore.Instance.SkillMgr.LoadSkills(skillsDir);
            Core.AgentCore.Instance.BuildSkillDocs();
            Core.AgentCore.Instance.Logger.Info($"Skills loaded from {skillsDir}");
        }

        int IBatchCmdPlugin.Init(string cmdLine, string basePath)
        {
            bool isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            string appDir = basePath ?? System.IO.Directory.GetCurrentDirectory();
            // The host exe lives in <root>/managed while AgentCore expects
            // <root> as basePath (onnx/, skills/, managed/ subdirectories).
            string rootPath = appDir;
            if (System.IO.Path.GetFileName(rootPath).Equals("managed", StringComparison.OrdinalIgnoreCase)) {
                rootPath = System.IO.Path.GetDirectoryName(rootPath) ?? rootPath;
            }
            // macOS bundle layout: the native host exe lives in
            // <app>.app/Contents/MacOS while the resources (managed/, onnx/,
            // skills/) live in <app>.app/Contents - go up one level so
            // resource-relative paths resolve like on Windows.
            if (isMac && System.IO.Path.GetFileName(rootPath).Equals("MacOS", StringComparison.OrdinalIgnoreCase)) {
                rootPath = System.IO.Path.GetDirectoryName(rootPath) ?? rootPath;
            }
            // Fill the executor's process info (also parses --agentscript=/
            // --agentport=) before any dsl execution on this main thread.
            MetaDslExecutor.SetProcessInfo(cmdLine, rootPath, rootPath, isMac);
            Initialize(rootPath, rootPath, isMac);
            // Load script_agent.dsl on this (host main) thread and run on_init.
            // The websocket server is started by the script itself
            // (ws_start_server(agentport)) so the port stays under dsl control.
            return MetaDslExecutor.InitAgent();
        }

        int IBatchCmdPlugin.Tick()
        {
            if (!_isInitialized) {
                return 0;
            }
            // Hot-reload check, drain the agent main-thread event queue
            // (agent_call/agent_notify, ws connect/disconnect, async service
            // callbacks), then run on_tick.
            return MetaDslExecutor.TickAgent();
        }

        int IBatchCmdPlugin.Shutdown()
        {
            if (_isInitialized) {
                MetaDslExecutor.FinalizeAgent();
                ScriptApi.WebSocketServerManager.StopAll();
                Core.AgentCore.Instance.Logger.Info("AgentPlugin shutting down");
                Core.AgentCore.Instance.Shutdown();
                _isInitialized = false;
            }
            return 0;
        }
    }
}
