using System.Collections.Generic;

using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// mcp_connect(serverId, type, target)
    /// Connects to an MCP server. type="stdio" or "sse". target=command line or URL.
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class McpConnectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_connect requires (serverId, type, target)");
                return BoxedValue.From(false);
            }
            string serverId = operands[0].AsString;
            string type = operands[1].AsString;
            string target = operands[2].AsString;
            return BoxedValue.From(AgentCore.Core.McpClientService.Instance.Connect(serverId, type, target));
        }
    }

    /// <summary>
    /// mcp_disconnect(serverId)
    /// Disconnects from an MCP server.
    /// </summary>
    sealed class McpDisconnectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_disconnect requires (serverId)");
                return BoxedValue.FromBool(false);
            }
            AgentCore.Core.McpClientService.Instance.Disconnect(operands[0].AsString);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// mcp_is_connected(serverId)
    /// Returns true if the server is connected.
    /// </summary>
    sealed class McpIsConnectedExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_is_connected requires (serverId)");
                return BoxedValue.FromBool(false);
            }
            return BoxedValue.FromBool(AgentCore.Core.McpClientService.Instance.IsConnected(operands[0].AsString));
        }
    }

    /// <summary>
    /// mcp_list_tools(serverId)
    /// Returns a human-readable string listing all tools available on the server.
    /// Suitable for direct LLM consumption.
    /// </summary>
    sealed class McpListToolsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_list_tools requires (serverId)");
                return BoxedValue.FromString(string.Empty);
            }
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.ListTools(operands[0].AsString));
        }
    }

    /// <summary>
    /// mcp_list_resources(serverId)
    /// Lists resources exposed by the server (cursor-paginated).
    /// Format: "uri: name - description (mimeType)" per line.
    /// </summary>
    sealed class McpListResourcesExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_list_resources requires (serverId)");
                return BoxedValue.FromString(string.Empty);
            }
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.ListResources(operands[0].AsString));
        }
    }

    /// <summary>
    /// mcp_read_resource(serverId, uri, mrtrJson?)
    /// Synchronously reads an MCP resource and blocks until the result is received.
    /// Returns the resource text (or "[binary N base64 chars]"); errors go to the api error info.
    /// mrtrJson (optional): {"inputResponses": {...}, "requestState": "..."} to answer an
    /// "[input required]" result (2026-07-28 MRTR retry).
    /// </summary>
    sealed class McpReadResourceExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_read_resource requires (serverId, uri)");
                return BoxedValue.FromString(string.Empty);
            }
            string serverId = operands[0].AsString;
            string uri = operands[1].AsString;
            string? mrtrJson = operands.Count >= 3 ? operands[2].AsString : null;
            try {
                string result = AgentCore.Core.McpClientService.Instance.ReadResource(serverId, uri, mrtrJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"mcp_read_resource error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// mcp_read_resource_callback(serverId, uri, tag, mrtrJson?)
    /// Reads an MCP resource asynchronously.
    /// mrtrJson (optional): {"inputResponses": {...}, "requestState": "..."} to answer an
    /// "[input required]" result (2026-07-28 MRTR retry).
    /// Result arrives via mcp_callback CEF message: (serverId, tag, resultText)
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class McpReadResourceCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_read_resource_callback requires (serverId, uri, tag)");
                return BoxedValue.From(false);
            }
            string serverId = operands[0].AsString;
            string uri = operands[1].AsString;
            string tag = operands[2].AsString;
            string? mrtrJson = operands.Count >= 4 ? operands[3].AsString : null;
            return BoxedValue.From(AgentCore.Core.McpClientService.Instance.ReadResourceCallback(serverId, uri, tag, mrtrJson));
        }
    }

    /// <summary>
    /// mcp_list_prompts(serverId)
    /// Lists prompt templates defined on the server (cursor-paginated).
    /// Format: "name: description" plus an optional "  args: arg1(required) arg2" line.
    /// </summary>
    sealed class McpListPromptsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_list_prompts requires (serverId)");
                return BoxedValue.FromString(string.Empty);
            }
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.ListPrompts(operands[0].AsString));
        }
    }

    /// <summary>
    /// mcp_read_prompt(serverId, promptName, argsJson?, mrtrJson?)
    /// Synchronously renders a prompt template and blocks until the result is received.
    /// Returns "[role] text" lines; resource_link contents are inlined when possible.
    /// mrtrJson (optional): {"inputResponses": {...}, "requestState": "..."} to answer an
    /// "[input required]" result (2026-07-28 MRTR retry).
    /// </summary>
    sealed class McpReadPromptExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_read_prompt requires (serverId, promptName[, argsJson])");
                return BoxedValue.FromString(string.Empty);
            }
            string serverId = operands[0].AsString;
            string promptName = operands[1].AsString;
            string argsJson = operands.Count >= 3 ? operands[2].AsString : "{}";
            string? mrtrJson = operands.Count >= 4 ? operands[3].AsString : null;
            try {
                string result = AgentCore.Core.McpClientService.Instance.ReadPrompt(serverId, promptName, argsJson, mrtrJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"mcp_read_prompt error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// mcp_read_prompt_callback(serverId, promptName, argsJson, tag, mrtrJson?)
    /// Renders a prompt template asynchronously.
    /// mrtrJson (optional): {"inputResponses": {...}, "requestState": "..."} to answer an
    /// "[input required]" result (2026-07-28 MRTR retry).
    /// Result arrives via mcp_callback CEF message: (serverId, tag, resultText)
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class McpReadPromptCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_read_prompt_callback requires (serverId, promptName, argsJson, tag)");
                return BoxedValue.From(false);
            }
            string serverId = operands[0].AsString;
            string promptName = operands[1].AsString;
            string argsJson = operands[2].AsString;
            string tag = operands[3].AsString;
            string? mrtrJson = operands.Count >= 5 ? operands[4].AsString : null;
            return BoxedValue.From(AgentCore.Core.McpClientService.Instance.ReadPromptCallback(serverId, promptName, argsJson, tag, mrtrJson));
        }
    }

    /// <summary>
    /// mcp_call_tool_callback(serverId, toolName, argsJson, tag, mrtrJson?)
    /// Calls an MCP tool asynchronously.
    /// argsJson: JSON object of tool arguments, e.g. {"path":"/tmp/foo.txt"}
    /// mrtrJson (optional): {"inputResponses": {...}, "requestState": "..."} to answer an
    /// "[input required]" result (2026-07-28 MRTR retry).
    /// Result arrives via mcp_callback CEF message: (serverId, tag, resultText)
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class McpCallToolCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_call_tool_callback requires (serverId, toolName, argsJson, tag)");
                return BoxedValue.From(false);
            }
            string serverId = operands[0].AsString;
            string toolName = operands[1].AsString;
            string argsJson = operands[2].AsString;
            string tag = operands[3].AsString;
            string? mrtrJson = operands.Count >= 5 ? operands[4].AsString : null;
            return BoxedValue.From(AgentCore.Core.McpClientService.Instance.CallToolCallback(serverId, toolName, argsJson, tag, mrtrJson));
        }
    }

    /// <summary>
    /// mcp_set_option(serverId, key, value)
    /// Sets a connection option. Can be called before or after mcp_connect.
    /// key="timeout" (ms) or key="header" (Name:Value) or key="max_busy_seconds" (watchdog threshold)
    /// or key="max_queue_len" (per-session async call waiting-queue depth, default 10)
    /// or key="protocol_version" (MCP protocol sent in initialize, default 2025-11-25).
    /// Can be called multiple times for headers.
    /// Returns true.
    /// </summary>
    sealed class McpSetOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_set_option requires (serverId, key, value)");
                return BoxedValue.FromBool(false);
            }
            string serverId = operands[0].AsString;
            string key = operands[1].AsString;
            string value = operands[2].ToString();
            AgentCore.Core.McpClientService.Instance.SetOption(serverId, key, value);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// mcp_clear_options(serverId)
    /// Clears all pending connection options for a server.
    /// Returns true.
    /// </summary>
    sealed class McpClearOptionsExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_clear_options requires (serverId)");
                return BoxedValue.FromBool(false);
            }
            AgentCore.Core.McpClientService.Instance.ClearOptions(operands[0].AsString);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// mcp_call_tool(serverId, toolName, argsJson, mrtrJson?)
    /// Synchronously calls an MCP tool and blocks until the result is received.
    /// Returns the result string directly.
    /// mrtrJson (optional): {"inputResponses": {...}, "requestState": "..."} to answer an
    /// "[input required]" result (2026-07-28 MRTR retry).
    /// </summary>
    sealed class McpCallToolExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_call_tool requires (serverId, toolName, argsJson)");
                return BoxedValue.FromString(string.Empty);
            }
            string serverId = operands[0].AsString;
            string toolName = operands[1].AsString;
            string argsJson = operands[2].AsString;
            string? mrtrJson = operands.Count >= 4 ? operands[3].AsString : null;
            try {
                string result = AgentCore.Core.McpClientService.Instance.CallTool(serverId, toolName, argsJson, mrtrJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine($"mcp_call_tool error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    /// <summary>
    /// mcp_get_busy_duration(serverId, tag)
    /// Returns how many seconds the call has been busy (0 if not busy).
    /// </summary>
    sealed class McpGetBusyDurationExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_get_busy_duration requires (serverId, tag)");
                return BoxedValue.From(0);
            }
            string serverId = operands[0].AsString;
            string tag = operands[1].AsString;
            return BoxedValue.From(AgentCore.Core.McpClientService.Instance.GetBusyDuration(serverId, tag));
        }
    }

    /// <summary>
    /// mcp_cancel(serverId, tag)
    /// Cancels an active MCP tool call for the given session.
    /// Returns true/false; errors go to the api error info.
    /// </summary>
    sealed class McpCancelExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentCore.Core.MetaDslExecutor.AppendApiErrorInfoLine("mcp_cancel requires (serverId, tag)");
                return BoxedValue.From(false);
            }
            string serverId = operands[0].AsString;
            string tag = operands[1].AsString;
            return BoxedValue.From(AgentCore.Core.McpClientService.Instance.Cancel(serverId, tag));
        }
    }

    /// <summary>
    /// Registers all MCP DSL APIs
    /// </summary>
    public static class McpApi
    {
        public static void RegisterApis()
        {
            BatchCommand.BatchScript.Register("mcp_connect",
                "mcp_connect(serverId, type, target) - connect to MCP server, type='stdio'/'sse'/'streamable-http', target=command or URL",
                new ExpressionFactoryHelper<McpConnectExp>());
            BatchCommand.BatchScript.Register("mcp_disconnect",
                "mcp_disconnect(serverId) - disconnect from MCP server",
                new ExpressionFactoryHelper<McpDisconnectExp>());
            BatchCommand.BatchScript.Register("mcp_is_connected",
                "mcp_is_connected(serverId) - check if MCP server is connected",
                new ExpressionFactoryHelper<McpIsConnectedExp>());
            BatchCommand.BatchScript.Register("mcp_list_tools",
                "mcp_list_tools(serverId) - list available tools on MCP server (LLM-friendly format)",
                new ExpressionFactoryHelper<McpListToolsExp>());
            BatchCommand.BatchScript.Register("mcp_list_resources",
                "mcp_list_resources(serverId) - list resources exposed by MCP server (uri: name - description (mimeType))",
                new ExpressionFactoryHelper<McpListResourcesExp>());
            BatchCommand.BatchScript.Register("mcp_read_resource",
                "mcp_read_resource(serverId, uri) - read an MCP resource by uri, blocks until the result is received and returns it directly",
                new ExpressionFactoryHelper<McpReadResourceExp>());
            BatchCommand.BatchScript.Register("mcp_read_resource_callback",
                "mcp_read_resource_callback(serverId, uri, tag) - read an MCP resource async, result via mcp_callback CEF message",
                new ExpressionFactoryHelper<McpReadResourceCallbackExp>());
            BatchCommand.BatchScript.Register("mcp_list_prompts",
                "mcp_list_prompts(serverId) - list prompt templates on MCP server (name: description / args)",
                new ExpressionFactoryHelper<McpListPromptsExp>());
            BatchCommand.BatchScript.Register("mcp_read_prompt",
                "mcp_read_prompt(serverId, promptName, argsJson?) - render a prompt template, blocks and returns '[role] text' lines",
                new ExpressionFactoryHelper<McpReadPromptExp>());
            BatchCommand.BatchScript.Register("mcp_read_prompt_callback",
                "mcp_read_prompt_callback(serverId, promptName, argsJson, tag) - render a prompt template async, result via mcp_callback CEF message",
                new ExpressionFactoryHelper<McpReadPromptCallbackExp>());
            BatchCommand.BatchScript.Register("mcp_call_tool_callback",
                "mcp_call_tool_callback(serverId, toolName, argsJson, tag) - call MCP tool async, result via mcp_callback CEF message",
                new ExpressionFactoryHelper<McpCallToolCallbackExp>());
            BatchCommand.BatchScript.Register("mcp_set_option",
                "mcp_set_option(serverId, key, value) - set connection option, key='timeout'(ms)/'header'(Name:Value)/'max_busy_seconds'(watchdog threshold)/'max_queue_len'(async call queue depth)/'protocol_version'('2026-07-28' forces the stateless era, older values force the handshake era; default auto-detects the server era)",
                new ExpressionFactoryHelper<McpSetOptionExp>());
            BatchCommand.BatchScript.Register("mcp_clear_options",
                "mcp_clear_options(serverId) - clear all pending connection options for a server",
                new ExpressionFactoryHelper<McpClearOptionsExp>());
            BatchCommand.BatchScript.Register("mcp_call_tool",
                "mcp_call_tool(serverId, toolName, argsJson) - synchronous tool call, blocks until result is received and returns it directly",
                new ExpressionFactoryHelper<McpCallToolExp>());
            BatchCommand.BatchScript.Register("mcp_get_busy_duration",
                "mcp_get_busy_duration(serverId, tag) - returns seconds the call has been busy (0 if not busy)",
                new ExpressionFactoryHelper<McpGetBusyDurationExp>());
            BatchCommand.BatchScript.Register("mcp_cancel",
                "mcp_cancel(serverId, tag) - cancel an active MCP tool call for the session",
                new ExpressionFactoryHelper<McpCancelExp>());

        }
    }
}
