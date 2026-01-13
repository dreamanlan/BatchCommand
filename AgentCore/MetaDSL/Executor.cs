using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.MetaDSL
{
    /// <summary>
    /// MetaDSL executor for executing MetaDSL scripts
    /// This enables the agent to modify itself
    /// </summary>
    public class Executor
    {
        private AgentCore.Core.AgentCore _agentCore;

        public Executor()
        {
        }

        public string Execute(string script, AgentCore.Core.AgentCore agentCore)
        {
            _agentCore = agentCore;

            try
            {
                // Parse and execute MetaDSL statements
                var parser = new Parser();
                var statements = parser.Parse(script);

                var results = new List<string>();

                foreach (var statement in statements)
                {
                    var result = ExecuteStatement(statement);
                    if (!string.IsNullOrEmpty(result))
                    {
                        results.Add(result);
                    }
                }

                return results.Count > 0 ? string.Join("\n", results) : "Execution completed";
            }
            catch (Exception ex)
            {
                return $"Execution error: {ex.Message}";
            }
        }

        private string ExecuteStatement(Statement statement)
        {
            switch (statement.Type)
            {
                case StatementType.ReadFile:
                    return ExecuteReadFile(statement);
                case StatementType.WriteFile:
                    return ExecuteWriteFile(statement);
                case StatementType.ModifyFile:
                    return ExecuteModifyFile(statement);
                case StatementType.DeleteFile:
                    return ExecuteDeleteFile(statement);
                case StatementType.ExecuteCommand:
                    return ExecuteCommand(statement);
                case StatementType.HotReload:
                    return ExecuteHotReload(statement);
                case StatementType.SendMessage:
                    return ExecuteSendMessage(statement);
                case StatementType.Comment:
                    return null;
                default:
                    throw new NotSupportedException($"Unknown statement type: {statement.Type}");
            }
        }

        private string ExecuteReadFile(Statement statement)
        {
            string path = GetParameter(statement, "path");
            if (string.IsNullOrEmpty(path))
                return "Error: path parameter required";

            try
            {
                string fullPath = ResolvePath(path);
                string content = File.ReadAllText(fullPath);
                return $"File read: {path} ({content.Length} bytes)";
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        private string ExecuteWriteFile(Statement statement)
        {
            string path = GetParameter(statement, "path");
            string content = GetParameter(statement, "content");

            if (string.IsNullOrEmpty(path))
                return "Error: path parameter required";

            try
            {
                string fullPath = ResolvePath(path);

                // Create directory if needed
                string directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(fullPath, content);
                return $"File written: {path}";
            }
            catch (Exception ex)
            {
                return $"Error writing file: {ex.Message}";
            }
        }

        private string ExecuteModifyFile(Statement statement)
        {
            string path = GetParameter(statement, "path");
            string oldText = GetParameter(statement, "old");
            string newText = GetParameter(statement, "new");

            if (string.IsNullOrEmpty(path))
                return "Error: path parameter required";
            if (string.IsNullOrEmpty(oldText))
                return "Error: old parameter required";
            if (string.IsNullOrEmpty(newText))
                return "Error: new parameter required";

            try
            {
                string fullPath = ResolvePath(path);
                string content = File.ReadAllText(fullPath);

                if (!content.Contains(oldText))
                {
                    return $"Error: old text not found in file";
                }

                string newContent = content.Replace(oldText, newText);
                File.WriteAllText(fullPath, newContent);

                return $"File modified: {path} ({content.Length - newContent.Length} bytes changed)";
            }
            catch (Exception ex)
            {
                return $"Error modifying file: {ex.Message}";
            }
        }

        private string ExecuteDeleteFile(Statement statement)
        {
            string path = GetParameter(statement, "path");

            if (string.IsNullOrEmpty(path))
                return "Error: path parameter required";

            try
            {
                string fullPath = ResolvePath(path);

                if (!File.Exists(fullPath))
                {
                    return $"Error: file not found: {path}";
                }

                File.Delete(fullPath);
                return $"File deleted: {path}";
            }
            catch (Exception ex)
            {
                return $"Error deleting file: {ex.Message}";
            }
        }

        private string ExecuteCommand(Statement statement)
        {
            string command = GetParameter(statement, "command");

            if (string.IsNullOrEmpty(command))
                return "Error: command parameter required";

            try
            {
                // Execute command via ProcessOperations
                var result = _agentCore.ProcessOps.ExecuteCommand(command, null, null, 30000);
                return $"Command executed: {command}\n{result.Output}";
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        private string ExecuteHotReload(Statement statement)
        {
            string component = GetParameter(statement, "component");

            if (string.IsNullOrEmpty(component))
                component = "all";

            try
            {
                // Trigger hot reload via AgentPlugin
                // This will be called from the script API layer
                return $"Hot reload triggered for: {component}";
            }
            catch (Exception ex)
            {
                return $"Error hot reloading: {ex.Message}";
            }
        }

        private string ExecuteSendMessage(Statement statement)
        {
            string message = GetParameter(statement, "message");
            string parameters = GetParameter(statement, "params");

            if (string.IsNullOrEmpty(message))
                return "Error: message parameter required";

            try
            {
                // BrowserInteraction is a marker interface, SendMessage is not defined
                // This functionality should be implemented via native API
                return $"Message send requested: {message} (requires native API)";
            }
            catch (Exception ex)
            {
                return $"Error sending message: {ex.Message}";
            }
        }

        private string ResolvePath(string path)
        {
            // If path is relative, resolve against base path from AgentCore
            if (!Path.IsPathRooted(path))
            {
                // Get base path from AgentCore's context
                var currentWorkspace = _agentCore.ContextManager.GetCurrentWorkspace();
                if (currentWorkspace != null)
                {
                    return Path.Combine(currentWorkspace.RootPath, path);
                }

                // Fallback to current directory
                return Path.Combine(Directory.GetCurrentDirectory(), path);
            }
            return path;
        }

        private string GetParameter(Statement statement, string name)
        {
            if (statement.Parameters.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }
    }

    /// <summary>
    /// Simple MetaDSL parser
    /// </summary>
    public class Parser
    {
        public List<Statement> Parse(string script)
        {
            var statements = new List<Statement>();
            var lines = script.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // Skip comments
                if (trimmed.StartsWith("#") || trimmed.StartsWith("//"))
                {
                    statements.Add(new Statement { Type = StatementType.Comment });
                    continue;
                }

                // Parse statements
                var statement = ParseStatement(trimmed);
                if (statement != null)
                {
                    statements.Add(statement);
                }
            }

            return statements;
        }

        private Statement ParseStatement(string line)
        {
            // Simple parsing: function(params)
            var match = System.Text.RegularExpressions.Regex.Match(line, @"(\w+)\((.*)\)");

            if (!match.Success)
                return null;

            string functionName = match.Groups[1].Value;
            string paramsText = match.Groups[2].Value;

            var statement = new Statement();
            statement.Parameters = new Dictionary<string, string>();

            switch (functionName.ToLower())
            {
                case "read_file":
                    statement.Type = StatementType.ReadFile;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                case "write_file":
                    statement.Type = StatementType.WriteFile;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                case "modify_file":
                    statement.Type = StatementType.ModifyFile;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                case "delete_file":
                    statement.Type = StatementType.DeleteFile;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                case "execute":
                    statement.Type = StatementType.ExecuteCommand;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                case "hot_reload":
                    statement.Type = StatementType.HotReload;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                case "send_message":
                    statement.Type = StatementType.SendMessage;
                    ParseParams(paramsText, statement.Parameters);
                    break;
                default:
                    return null;
            }

            return statement;
        }

        private void ParseParams(string text, Dictionary<string, string> parameters)
        {
            // Simple parameter parsing: name=value
            var parts = text.Split(',');
            foreach (var part in parts)
            {
                var keyValue = part.Split(new[] { '=' }, 2);
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim().Trim('\'', '"', '{', '}');
                    parameters[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// MetaDSL statement
    /// </summary>
    public class Statement
    {
        public StatementType Type { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }

    /// <summary>
    /// Statement type enum
    /// </summary>
    public enum StatementType
    {
        ReadFile,
        WriteFile,
        ModifyFile,
        DeleteFile,
        ExecuteCommand,
        HotReload,
        SendMessage,
        Comment
    }
}
