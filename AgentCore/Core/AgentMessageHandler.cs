using System;
using System.Collections.Generic;
using System.Text.Json;
using DotnetStoryScript;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    // Message structure from inject.js
    public class AgentMessage
    {
        public long Id { get; set; }
        public string Command { get; set; }
        public Dictionary<string, object> Params { get; set; }
        public long Timestamp { get; set; }
    }

    // Response structure to inject.js
    public class AgentResponse
    {
        public long Id { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
    }

    public class AgentMessageHandler : IAgentMessageHandler
    {
        private static List<string> s_EmptyArgs = new List<string>();

        private Action<string> _sendToInject;
        private readonly Action<string> _log;

        public AgentMessageHandler(Action<string> sendToInject, Action<string> log)
        {
            _sendToInject = sendToInject;
            _log = log;
        }

        // Set the callback to send messages to inject.js
        public void SetSendToInjectCallback(Action<string> sendToInject)
        {
            _sendToInject = sendToInject;
        }

        // Parse and handle message from inject.js
        public void HandleMessage(string jsonData)
        {
            try
            {
                _log($"[AgentMessageHandler] Received message: {jsonData}");

                // Parse JSON with camelCase naming policy
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var message = JsonSerializer.Deserialize<AgentMessage>(jsonData, options);
                if (message == null)
                {
                    SendError(null, "Failed to parse message");
                    return;
                }

                // Dispatch command
                var response = DispatchCommand(message);

                // Send response
                SendResponse(response);
            }
            catch (Exception ex)
            {
                _log($"[AgentMessageHandler] Error handling message: {ex.Message}");
                SendError(null, ex.Message);
            }
        }

        // Dispatch command to appropriate handler
        private AgentResponse DispatchCommand(AgentMessage message)
        {
            try
            {
                switch (message.Command)

                {
                    case "ping":
                        return HandlePing(message);

                    case "send_and_wait":
                        return HandleSendAndWait(message);

                    case "get_response":
                        return HandleGetResponse(message);

                    case "extract_code":
                        return HandleExtractCode(message);

                    case "file_operation":
                        return HandleFileOperation(message);

                    case "code_analysis":
                        return HandleCodeAnalysis(message);

                    case "execute_command":
                        return HandleExecuteCommand(message);

                    case "get_page_info":
                        return HandleGetPageInfo(message);

                    case "execute_metadsl":
                        return HandleExecuteMetaDsl(message);

                    default:
                        return new AgentResponse
                        {
                            Id = message.Id,
                            Success = false,
                            Error = $"Unknown command: {message.Command}"
                        };
                }
            }
            catch (Exception ex)
            {
                _log($"[AgentMessageHandler] Error dispatching command: {ex.Message}");
                return new AgentResponse
                {
                    Id = message.Id,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        // Command handlers
        private AgentResponse HandlePing(AgentMessage message)
        {
            return new AgentResponse
            {
                Id = message.Id,
                Success = true,
                Data = "pong"
            };
        }

        private AgentResponse HandleSendAndWait(AgentMessage message)
        {
            // This command is handled by inject.js
            // C# just acknowledges receipt
            return new AgentResponse

            {
                Id = message.Id,
                Success = true,
                Data = "Command acknowledged. LLM interaction handled by inject.js"
            };
        }

        private AgentResponse HandleGetResponse(AgentMessage message)
        {
            // Request inject.js to get the last LLM response
            return new AgentResponse

            {
                Id = message.Id,
                Success = true,
                Data = "Request sent to inject.js"
            };
        }

        private AgentResponse HandleExtractCode(AgentMessage message)
        {
            // Request inject.js to extract code from LLM response
            return new AgentResponse

            {
                Id = message.Id,
                Success = true,
                Data = "Request sent to inject.js"
            };
        }

        private AgentResponse HandleFileOperation(AgentMessage message)
        {
            try
            {
                if (!message.Params.ContainsKey("operation"))
                {
                    return new AgentResponse
                    {
                        Id = message.Id,
                        Success = false,
                        Error = "Missing 'operation' parameter"
                    };
                }

                string operation = message.Params["operation"].ToString();
                var core = AgentCore.Instance;


                switch (operation)
                {
                    case "read_file":
                        {
                            string path = message.Params["path"].ToString();
                            string content = core.FileOps.ReadFile(path);
                            return new AgentResponse
                            {
                                Id = message.Id,
                                Success = true,
                                Data = content
                            };
                        }

                    case "write_file":
                        {
                            string path = message.Params["path"].ToString();
                            string content = message.Params["content"].ToString();
                            core.FileOps.WriteFile(path, content);
                            return new AgentResponse
                            {
                                Id = message.Id,
                                Success = true,
                                Data = "File written successfully"
                            };
                        }

                    case "list_directory":
                        {
                            string path = message.Params["path"].ToString();
                            string pattern = message.Params.ContainsKey("pattern") ? message.Params["pattern"].ToString() : "*";
                            bool recursive = message.Params.ContainsKey("recursive") && Convert.ToBoolean(message.Params["recursive"]);
                            var files = core.FileOps.ListDirectory(path, pattern, recursive);
                            return new AgentResponse
                            {
                                Id = message.Id,
                                Success = true,
                                Data = files
                            };
                        }

                    default:
                        return new AgentResponse
                        {
                            Id = message.Id,
                            Success = false,
                            Error = $"Unknown file operation: {operation}"
                        };
                }
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Id = message.Id,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private AgentResponse HandleCodeAnalysis(AgentMessage message)
        {
            try
            {
                if (!message.Params.ContainsKey("operation"))
                {
                    return new AgentResponse
                    {
                        Id = message.Id,
                        Success = false,
                        Error = "Missing 'operation' parameter"
                    };
                }

                string operation = message.Params["operation"].ToString();
                var core = AgentCore.Instance;


                switch (operation)
                {
                    case "analyze_file":
                        {
                            string path = message.Params["path"].ToString();
                            var result = core.CodeAnalyzer.AnalyzeFile(path);
                            return new AgentResponse
                            {
                                Id = message.Id,
                                Success = true,
                                Data = result
                            };
                        }

                    case "find_symbol":
                        {
                            string symbol = message.Params["symbol"].ToString();
                            string directory = message.Params.ContainsKey("directory") ? message.Params["directory"].ToString() : ".";
                            string filePattern = message.Params.ContainsKey("filePattern") ? message.Params["filePattern"].ToString() : "*.*";
                            bool recursive = message.Params.ContainsKey("recursive") && Convert.ToBoolean(message.Params["recursive"]);
                            var results = core.CodeAnalyzer.FindSymbol(symbol, directory, filePattern, recursive);
                            return new AgentResponse
                            {
                                Id = message.Id,
                                Success = true,
                                Data = results
                            };
                        }

                    case "search_code":
                        {
                            string pattern = message.Params["pattern"].ToString();
                            string directory = message.Params.ContainsKey("directory") ? message.Params["directory"].ToString() : ".";
                            string filePattern = message.Params.ContainsKey("filePattern") ? message.Params["filePattern"].ToString() : "*.*";
                            bool useRegex = message.Params.ContainsKey("useRegex") && Convert.ToBoolean(message.Params["useRegex"]);
                            bool recursive = message.Params.ContainsKey("recursive") && Convert.ToBoolean(message.Params["recursive"]);
                            var results = core.CodeAnalyzer.SearchCode(pattern, directory, filePattern, useRegex, recursive);
                            return new AgentResponse
                            {
                                Id = message.Id,
                                Success = true,
                                Data = results
                            };
                        }

                    default:
                        return new AgentResponse
                        {
                            Id = message.Id,
                            Success = false,
                            Error = $"Unknown code analysis operation: {operation}"
                        };
                }
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Id = message.Id,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private AgentResponse HandleExecuteCommand(AgentMessage message)
        {
            try
            {
                if (!message.Params.ContainsKey("command"))
                {
                    return new AgentResponse
                    {
                        Id = message.Id,
                        Success = false,
                        Error = "Missing 'command' parameter"
                    };
                }

                string command = message.Params["command"].ToString();
                string arguments = message.Params.ContainsKey("arguments") ? message.Params["arguments"].ToString() : "";
                string workingDir = message.Params.ContainsKey("workingDir") ? message.Params["workingDir"].ToString() : "";
                int timeout = message.Params.ContainsKey("timeout") ? Convert.ToInt32(message.Params["timeout"]) : 30000;

                _log($"[AgentMessageHandler] Execute command: {command} {arguments}");

                var core = AgentCore.Instance;
                var result = core.ProcessOps.ExecuteCommand(command, arguments, workingDir, timeout);

                return new AgentResponse
                {
                    Id = message.Id,
                    Success = result.ExitCode == 0,
                    Data = new
                    {
                        exitCode = result.ExitCode,
                        output = result.Output,
                        error = result.Error
                    }
                };
            }
            catch (Exception ex)
            {
                return new AgentResponse
                {
                    Id = message.Id,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private AgentResponse HandleGetPageInfo(AgentMessage message)
        {
            // This is handled by inject.js
            return new AgentResponse

            {
                Id = message.Id,
                Success = true,
                Data = "Request sent to inject.js"
            };
        }

        private AgentResponse HandleExecuteMetaDsl(AgentMessage message)
        {
            try
            {                if (!message.Params.ContainsKey("script"))
                {
                    return new AgentResponse
                    {
                        Id = message.Id,
                        Success = false,
                        Error = "Missing 'script' parameter"
                    };
                }

                string script = message.Params["script"].ToString();
                string source = message.Params.ContainsKey("source") ? message.Params["source"].ToString() : "unknown";

                _log($"[AgentMessageHandler] Executing MetaDSL from {source}");

                // Store the script for execution by DSL

                var core = AgentCore.Instance;

                // Execute MetaDSL script through a helper API
                // The script will be executed in the DSL context
                var result = ExecuteMetaDslScript(script);

                return new AgentResponse

                {
                    Id = message.Id,
                    Success = true,
                    Data = new
                    {
                        executed = true,
                        result = result,
                        source = source
                    }
                };
            }
            catch (Exception ex)
            {
                _log($"[AgentMessageHandler] Error executing MetaDSL: {ex.Message}");
                return new AgentResponse
                {
                    Id = message.Id,
                    Success = false,
                    Error = $"MetaDSL execution error: {ex.Message}"
                };
            }
        }

        // Execute MetaDSL script
        private string ExecuteMetaDslScript(string script)
        {
            try
            {
                // Execute the script directly using the DSL interpreter

                // BatchCommand.BatchScript is the C# DSL interpreter
                var id = BatchCommand.BatchScript.EvalAsFunc(script, s_EmptyArgs);
                var result = BatchCommand.BatchScript.Call(id);

                string resultStr = result.IsNullObject ? "null" : result.ToString();
                return resultStr;

            }
            catch (Exception ex)
            {
                _log($"[AgentMessageHandler] Error executing MetaDSL script: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        // Send response to inject.js
        private void SendResponse(AgentResponse response)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string json = JsonSerializer.Serialize(response, options);
                _sendToInject(json);

            }
            catch (Exception ex)
            {
                _log($"[AgentMessageHandler] Error sending response: {ex.Message}");
            }
        }

        // Send error response
        private void SendError(long? messageId, string error)
        {
            var response = new AgentResponse
            {
                Id = messageId ?? 0,
                Success = false,
                Error = error
            };
            SendResponse(response);
        }

        // Send command to inject.js
        public void SendCommandToInject(string command, Dictionary<string, object> parameters)
        {
            try
            {
                var cmd = new
                {
                    command = command,
                    @params = parameters
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                string json = JsonSerializer.Serialize(cmd, options);

                string script = $"window.AgentAPI.bridge.triggerEvent('execute_command',{json})";

                // This needs to be executed via NativeApi.ExecuteJavascript
                // Will be called from DSL
                _log($"[AgentMessageHandler] Sending command to inject.js: {command}");
            }
            catch (Exception ex)
            {
                _log($"[AgentMessageHandler] Error sending command: {ex.Message}");
            }
        }
    }
}
