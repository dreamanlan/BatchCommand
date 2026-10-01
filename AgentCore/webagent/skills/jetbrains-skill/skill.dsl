skill("jetbrains-skill")
{
    tool(connect) {
        document("call_skill('jetbrains-skill:connect', project_path); Connect to JetBrains MCP via SSE. project_path optional (pass \"\" if unused); if non-empty, sets IJ_MCP_SERVER_PROJECT_PATH header");
        metadsl($project_path)
        {:
            $sid = "jetbrains";
            $url = "http://localhost:64342/sse";
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "60000");
            if($project_path != ""){
                mcp_set_option($sid, "header", "IJ_MCP_SERVER_PROJECT_PATH: "+$project_path);
            };
            mcp_connect($sid, "sse", $url);
        :};
    };
    tool(list) {
        document("call_skill('jetbrains-skill:list'); List available JetBrains MCP tools");
        metadsl()
        {:
            mcp_list_tools("jetbrains");
        :};
    };
    tool(tool) {
        document("call_skill('jetbrains-skill:tool', tool_name, tool_args_json); Call a JetBrains MCP tool. Result via callback tag 'jetbrains_callback'");
        metadsl($tool_name, $tool_args)
        {:
            mcp_call_tool_callback("jetbrains", $tool_name, $tool_args, "jetbrains_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('jetbrains-skill:disconnect'); Disconnect from JetBrains MCP");
        metadsl()
        {:
            mcp_disconnect("jetbrains");
        :};
    };
};
