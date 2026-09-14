using System;
using System.Collections.Generic;
using BatchCommand.Utils;

namespace AgentCore.Core
{
    /// <summary>
    /// Pushes agent messages to inject.js over the websocket connections.
    /// Replaces the CEF-era AgentBridge (window.onAgentCommand/onAgentResponse
    /// native js calls). Messages bound to a request are sent through the
    /// current DslContext (the originating connection); context-less pushes
    /// (e.g. from on_tick) are broadcast to all clients of all running servers.
    /// </summary>
    internal static class AgentPush
    {
        // {"type":"agent_command","command":...,"params":{...}} -> js side dispatches
        // it through the same logic as the old window.onAgentCommand path.
        internal static void SendCommandToInject(string command, Dictionary<string, object> parameters)
        {
            try {
                var payload = new Dictionary<string, object?> {
                    ["type"] = "agent_command",
                    ["command"] = command,
                    ["params"] = parameters,
                };
                Send(JsonHelper.ToJson(payload));
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] SendCommandToInject error: " + ex.Message);
            }
        }

        // dsl path (send_command_to_inject api): paramsJson was built by the
        // dsl to_json() call and is spliced in VERBATIM. Deserializing it into
        // Dictionary<string, object> first boxes every value as a System.Text.Json
        // JsonElement, and the LitJson serializer (JsonHelper.ToJson) reflects
        // that wrapper into {"ValueKind": 3} instead of the actual value - every
        // pushed agent reply string (PM flows, send_message) turned into
        // {"ValueKind": 3} (JsonValueKind 3 = String).
        internal static void SendCommandToInject(string command, string paramsJson)
        {
            try {
                string paramsSafe = "{}";
                if (!string.IsNullOrEmpty(paramsJson)) {
                    try {
                        using var doc = System.Text.Json.JsonDocument.Parse(paramsJson);
                        if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
                            paramsSafe = paramsJson;
                        }
                        else {
                            AgentCore.Instance.Logger.Error("[csharp] SendCommandToInject params is not a json object: " + paramsJson);
                        }
                    }
                    catch (Exception) {
                        AgentCore.Instance.Logger.Error("[csharp] SendCommandToInject invalid params json: " + paramsJson);
                    }
                }
                var sb = new System.Text.StringBuilder();
                sb.Append("{\"type\":\"agent_command\",\"command\":");
                sb.Append(System.Text.Json.JsonSerializer.Serialize(command ?? string.Empty));
                sb.Append(",\"params\":");
                sb.Append(paramsSafe);
                sb.Append("}");
                Send(sb.ToString());
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] SendCommandToInject error: " + ex.Message);
            }
        }

        // responseJson is the build_agent_response output
        // {"id":...,"success":...,"data":...,"error":...}; rewrapped as an
        // agent_result envelope so the js callback table can match it by id.
        internal static void SendResponseToInject(string responseJson)
        {
            try {
                long id = 0;
                bool success = false;
                string? data = null;
                string error = string.Empty;
                if (!string.IsNullOrEmpty(responseJson)) {
                    using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Object) {
                        if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == System.Text.Json.JsonValueKind.Number && idEl.TryGetInt64(out long longId)) {
                            id = longId;
                        }
                        if (root.TryGetProperty("success", out var successEl) && (successEl.ValueKind == System.Text.Json.JsonValueKind.True || successEl.ValueKind == System.Text.Json.JsonValueKind.False)) {
                            success = successEl.GetBoolean();
                        }
                        if (root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind != System.Text.Json.JsonValueKind.Null) {
                            data = dataEl.ValueKind == System.Text.Json.JsonValueKind.String ? dataEl.GetString() : dataEl.GetRawText();
                        }
                        if (root.TryGetProperty("error", out var errorEl) && errorEl.ValueKind == System.Text.Json.JsonValueKind.String) {
                            error = errorEl.GetString() ?? string.Empty;
                        }
                    }
                }
                var payload = new Dictionary<string, object?> {
                    ["type"] = "agent_result",
                    ["id"] = id,
                    ["success"] = success,
                    ["data"] = data,
                    ["error"] = error,
                };
                Send(JsonHelper.ToJson(payload));
            }
            catch (Exception ex) {
                AgentCore.Instance.Logger.Error("[csharp] SendResponseToInject error: " + ex.Message);
            }
        }

        private static void Send(string json)
        {
            var ctx = MetaDslExecutor.CurrentContext;
            if (null != ctx) {
                ctx.Send(json);
            }
            else {
                global::AgentCore.ScriptApi.WebSocketServerManager.BroadcastAll(json);
            }
        }
    }
}
