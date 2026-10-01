skill("ida-skill")
{
    tool(connect) {
        document("call_skill('ida-skill:connect'); Connect to IDA Pro MCP server (streamable-http)");
        metadsl()
        {:
            $sid = "ida-pro-mcp";
            $url = "http://localhost:13337/mcp";
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "60000");
            mcp_connect($sid, "streamable-http", $url);
        :};
    };
    tool(list) {
        document("call_skill('ida-skill:list'); List available IDA MCP tools");
        metadsl()
        {:
            mcp_list_tools("ida-pro-mcp");
        :};
    };
    tool(tool) {
        document("call_skill('ida-skill:tool', tool_name, tool_args_json); Call an IDA MCP tool. Result via callback tag 'ida_pro_mcp_callback'");
        metadsl($tool_name, $tool_args)
        {:
            mcp_call_tool_callback("ida-pro-mcp", $tool_name, $tool_args, "ida_pro_mcp_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('ida-skill:disconnect'); Disconnect from IDA MCP server");
        metadsl()
        {:
            mcp_disconnect("ida-pro-mcp");
        :};
    };
};
