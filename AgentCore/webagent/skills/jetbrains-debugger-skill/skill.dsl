skill("jetbrains-debugger-skill")
{
    tool(connect) {
        document("call_skill('jetbrains-debugger-skill:connect'); Connect to JetBrains Debugger MCP (streamable-http)");
        metadsl()
        {:
            $sid = "jetbrains-debugger";
            $url = "http://127.0.0.1:29191/debugger-mcp/streamable-http";
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "60000");
            mcp_connect($sid, "streamable-http", $url);
        :};
    };
    tool(list) {
        document("call_skill('jetbrains-debugger-skill:list'); List available JetBrains Debugger MCP tools");
        metadsl()
        {:
            mcp_list_tools("jetbrains-debugger");
        :};
    };
    tool(tool) {
        document("call_skill('jetbrains-debugger-skill:tool', tool_name, tool_args_json); Call a JetBrains Debugger MCP tool. Result via callback tag 'jetbrains_debugger_callback'");
        metadsl($tool_name, $tool_args)
        {:
            mcp_call_tool_callback("jetbrains-debugger", $tool_name, $tool_args, "jetbrains_debugger_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('jetbrains-debugger-skill:disconnect'); Disconnect from JetBrains Debugger MCP");
        metadsl()
        {:
            mcp_disconnect("jetbrains-debugger");
        :};
    };
};
