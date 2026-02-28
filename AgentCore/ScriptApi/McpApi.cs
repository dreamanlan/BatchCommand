using System.Collections.Generic;
using AgentPlugin.Abstractions;
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
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.McpClientService.Instance.Connect(serverId, type, target));
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
            CefDotnetApp.AgentCore.Core.McpClientService.Instance.Disconnect(operands[0].AsString);
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
            return BoxedValue.FromBool(CefDotnetApp.AgentCore.Core.McpClientService.Instance.IsConnected(operands[0].AsString));
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
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.McpClientService.Instance.ListTools(operands[0].AsString));
        }
    }

    /// <summary>
    /// mcp_call_tool(serverId, toolName, argsJson, callbackTag)
    /// Calls an MCP tool asynchronously.
    /// argsJson: JSON object of tool arguments, e.g. {"path":"/tmp/foo.txt"}
    /// Result arrives via mcp_callback CEF message: (serverId, callbackTag, resultText)
    /// Returns "ok" or an error string.
    /// </summary>
    sealed class McpCallToolExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 4) {
                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("mcp_call_tool requires (serverId, toolName, argsJson, callbackTag)");
                return BoxedValue.FromString("error: missing parameters");
            }
            string serverId = operands[0].AsString;
            string toolName = operands[1].AsString;
            string argsJson = operands[2].AsString;
            string callbackTag = operands[3].AsString;
            return BoxedValue.FromString(CefDotnetApp.AgentCore.Core.McpClientService.Instance.CallTool(serverId, toolName, argsJson, callbackTag));
        }
    }

    /// <summary>
    /// mcp_set_option(serverId, key, value)
    /// Sets a connection option before calling mcp_connect.
    /// key="timeout" (ms) or key="header" (Name:Value). Can be called multiple times for headers.
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
            string value = operands[2].AsString;
            CefDotnetApp.AgentCore.Core.McpClientService.Instance.SetOption(serverId, key, value);
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
            CefDotnetApp.AgentCore.Core.McpClientService.Instance.ClearOptions(operands[0].AsString);
            return BoxedValue.FromBool(true);
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
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_call_tool",
                "mcp_call_tool(serverId, toolName, argsJson, callbackTag) - call MCP tool async, result via mcp_callback CEF message",
                new ExpressionFactoryHelper<McpCallToolExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_set_option",
                "mcp_set_option(serverId, key, value) - set connection option before mcp_connect, key='timeout'(ms) or 'header'(Name:Value)",
                new ExpressionFactoryHelper<McpSetOptionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mcp_clear_options",
                "mcp_clear_options(serverId) - clear all pending connection options for a server",
                new ExpressionFactoryHelper<McpClearOptionsExp>());
        }
    }
}
