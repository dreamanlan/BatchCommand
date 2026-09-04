using System.Collections.Generic;
using AbstractAgent;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace AgentCore.ScriptApi
{
    /// <summary>
    /// mcp_connect(serverId, type, target)
    /// Connects to an MCP server. type="stdio" or "sse". target=command line or URL.
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class McpConnectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_connect requires (serverId, type, target)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string serverId = operands[0].AsString;
            string type = operands[1].AsString;
            string target = operands[2].AsString;
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.Connect(serverId, type, target));
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_disconnect requires (serverId)");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_is_connected requires (serverId)");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_list_tools requires (serverId)");
                return BoxedValue.FromString("error: missing parameters");
            }
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.ListTools(operands[0].AsString));
        }
    }

    /// <summary>
    /// mcp_call_tool_callback(serverId, toolName, argsJson, tag)
    /// Calls an MCP tool asynchronously.
    /// argsJson: JSON object of tool arguments, e.g. {"path":"/tmp/foo.txt"}
    /// Result arrives via mcp_callback CEF message: (serverId, tag, resultText)
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class McpCallToolCallbackExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_call_tool_callback requires (serverId, toolName, argsJson, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string serverId = operands[0].AsString;
            string toolName = operands[1].AsString;
            string argsJson = operands[2].AsString;
            string tag = operands[3].AsString;
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.CallToolCallback(serverId, toolName, argsJson, tag));
        }
    }

    /// <summary>
    /// mcp_set_option(serverId, key, value)
    /// Sets a connection option. Can be called before or after mcp_connect.
    /// key="timeout" (ms) or key="header" (Name:Value) or key="max_busy_seconds" (watchdog threshold).
    /// Can be called multiple times for headers.
    /// Returns true.
    /// </summary>
    sealed class McpSetOptionExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_set_option requires (serverId, key, value)");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_clear_options requires (serverId)");
                return BoxedValue.FromBool(false);
            }
            AgentCore.Core.McpClientService.Instance.ClearOptions(operands[0].AsString);
            return BoxedValue.FromBool(true);
        }
    }

    /// <summary>
    /// mcp_call_tool(serverId, toolName, argsJson)
    /// Synchronously calls an MCP tool and blocks until the result is received.
    /// Returns the result string directly.
    /// </summary>
    sealed class McpCallToolExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_call_tool requires (serverId, toolName, argsJson)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string serverId = operands[0].AsString;
            string toolName = operands[1].AsString;
            string argsJson = operands[2].AsString;
            try {
                string result = AgentCore.Core.McpClientService.Instance.CallTool(serverId, toolName, argsJson).GetAwaiter().GetResult();
                return BoxedValue.FromString(result);
            }
            catch (System.Exception ex) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine($"mcp_call_tool error: {ex.Message}");
                return BoxedValue.FromString($"[error] {ex.Message}");
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
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_get_busy_duration requires (serverId, tag)");
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
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class McpCancelExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_cancel requires (serverId, tag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string serverId = operands[0].AsString;
            string tag = operands[1].AsString;
            return BoxedValue.FromString(AgentCore.Core.McpClientService.Instance.Cancel(serverId, tag));
        }
    }

    /// <summary>
    /// Registers all MCP DSL APIs
    /// </summary>
    public static class McpApi
    {
        public static void RegisterApis()
        {
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_connect",
                "mcp_connect(serverId, type, target) - connect to MCP server, type='stdio'/'sse'/'streamable-http', target=command or URL",
                new ExpressionFactoryHelper<McpConnectExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_disconnect",
                "mcp_disconnect(serverId) - disconnect from MCP server",
                new ExpressionFactoryHelper<McpDisconnectExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_is_connected",
                "mcp_is_connected(serverId) - check if MCP server is connected",
                new ExpressionFactoryHelper<McpIsConnectedExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_list_tools",
                "mcp_list_tools(serverId) - list available tools on MCP server (LLM-friendly format)",
                new ExpressionFactoryHelper<McpListToolsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_call_tool_callback",
                "mcp_call_tool_callback(serverId, toolName, argsJson, tag) - call MCP tool async, result via mcp_callback CEF message",
                new ExpressionFactoryHelper<McpCallToolCallbackExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_set_option",
                "mcp_set_option(serverId, key, value) - set connection option, key='timeout'(ms)/'header'(Name:Value)/'max_busy_seconds'(watchdog threshold)",
                new ExpressionFactoryHelper<McpSetOptionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_clear_options",
                "mcp_clear_options(serverId) - clear all pending connection options for a server",
                new ExpressionFactoryHelper<McpClearOptionsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_call_tool",
                "mcp_call_tool(serverId, toolName, argsJson) - synchronous tool call, blocks until result is received and returns it directly",
                new ExpressionFactoryHelper<McpCallToolExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_get_busy_duration",
                "mcp_get_busy_duration(serverId, tag) - returns seconds the call has been busy (0 if not busy)",
                new ExpressionFactoryHelper<McpGetBusyDurationExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_cancel",
                "mcp_cancel(serverId, tag) - cancel an active MCP tool call for the session",
                new ExpressionFactoryHelper<McpCancelExp>());

        }
    }
}
