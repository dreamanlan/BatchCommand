skill("renderdoc-skill")
{
    tool(connect) {
        document("call_skill('renderdoc-skill:connect'); Connect to RenderDoc MCP (stdio). Sets PYTHONWARNINGS to suppress deprecation noise");
        metadsl()
        {:
            $sid = "renderdoc";
            set_environment("PYTHONWARNINGS", "ignore::DeprecationWarning");
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "60000");
            mcp_connect($sid, "stdio", "renderdoc-mcp.exe");
        :};
    };
    tool(list) {
        document("call_skill('renderdoc-skill:list'); List available RenderDoc MCP tools");
        metadsl()
        {:
            mcp_list_tools("renderdoc");
        :};
    };
    tool(tool) {
        document("call_skill('renderdoc-skill:tool', tool_name, tool_args_json); Call a RenderDoc MCP tool. Result via callback tag 'renderdoc_callback'");
        metadsl($tool_name, $tool_args)
        {:
            mcp_call_tool_callback("renderdoc", $tool_name, $tool_args, "renderdoc_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('renderdoc-skill:disconnect'); Disconnect from RenderDoc MCP");
        metadsl()
        {:
            mcp_disconnect("renderdoc");
        :};
    };
};
