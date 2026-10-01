skill("unreal-mcp-skill")
{
    tool(connect) {
        document("call_skill('unreal-mcp-skill:connect'); Connect to Unreal MCP (stdio, node bin.js)");
        metadsl()
        {:
            $sid = "unreal";
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "60000");
            mcp_connect($sid, "stdio", "node D:\\GitHub\\unreal-mcp\\dist\\bin.js");
        :};
    };
    tool(list) {
        document("call_skill('unreal-mcp-skill:list'); List available Unreal MCP tools");
        metadsl()
        {:
            mcp_list_tools("unreal");
        :};
    };
    tool(tool) {
        document("call_skill('unreal-mcp-skill:tool', tool_name, tool_args_json); Call an Unreal MCP tool. Result via callback tag 'unreal_callback'");
        metadsl($tool_name, $tool_args)
        {:
            mcp_call_tool_callback("unreal", $tool_name, $tool_args, "unreal_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('unreal-mcp-skill:disconnect'); Disconnect from Unreal MCP");
        metadsl()
        {:
            mcp_disconnect("unreal");
        :};
    };
};
