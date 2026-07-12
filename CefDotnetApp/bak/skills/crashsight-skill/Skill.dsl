skill("crashsight-skill")
{
    tool(connect) {
        document("call_skill('crashsight-skill:connect'); Connect to CrashSight MCP server");
        metadsl()
        {:
            $sid = "crashsight";
            $url = "https://crashsight.qq.com/mcp";
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "60000");
            mcp_set_option($sid, "header", "Accept:application/json");
            mcp_set_option($sid, "header", "X-CSMCP-API-KEY:%crashsight_token%");
            mcp_connect($sid, "streamable-http", $url);
        :};
    };
    tool(list) {
        document("call_skill('crashsight-skill:list'); List available CrashSight MCP tools");
        metadsl()
        {:
            $sid = "crashsight";
            mcp_list_tools($sid);
        :};
    };
    tool(help) {
        document("call_skill('crashsight-skill:help', tool_name); Show tool parameter schema");
        metadsl($tool_name)
        {:
            $sid = "crashsight";
            mcp_call_tool_callback($sid, "lookup_tool_param_schema", '{"tool_name":"'+$tool_name+'"}', "crashsight_callback");
        :};
    };
    tool(apps) {
        document("call_skill('crashsight-skill:apps'); Get user's application/project list");
        metadsl()
        {:
            $sid = "crashsight";
            mcp_call_tool_callback($sid, "get_app_info_list", '{}', "crashsight_callback");
        :};
    };
    tool(crash_by_share) {
        document("call_skill('crashsight-skill:crash_by_share', share_id); Get crash detail by shared report link. Extract shareId from URL: https://crashsight.qq.com/shared-report/{shareId}");
        metadsl($share_id)
        {:
            $sid = "crashsight";
            mcp_call_tool_callback($sid, "fetch_crash_detail_by_share_id", '{"shareId":"'+$share_id+'"}', "crashsight_callback");
        :};
    };
    tool(crash_by_issue) {
        document("call_skill('crashsight-skill:crash_by_issue', app_id, issue_id); Get latest crash detail by appId and issueId. Extract from URL: https://crashsight.qq.com/crash-reporting/crashes/{appId}/{issueId}");
        metadsl($app_id, $issue_id)
        {:
            $sid = "crashsight";
            mcp_call_tool_callback($sid, "fetch_last_crash_detail_in_issue", '{"appId":"'+$app_id+'","issueId":"'+$issue_id+'"}', "crashsight_callback");
        :};
    };
    tool(tool) {
        document("call_skill('crashsight-skill:tool', tool_name, tool_args_json); Call a CrashSight MCP tool. tool_name: MCP tool name; tool_args_json: JSON string of tool arguments; result is returned asynchronously via callback");
        metadsl($tool_name, $tool_args)
        {:
            $sid = "crashsight";
            mcp_call_tool_callback($sid, $tool_name, $tool_args, "crashsight_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('crashsight-skill:disconnect'); Disconnect from CrashSight MCP server");
        metadsl()
        {:
            $sid = "crashsight";
            mcp_disconnect($sid);
        :};
    };
};
