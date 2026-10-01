skill("tapd-skill")
{
    tool(connect) {
        document("call_skill('tapd-skill:connect'); Connect to TAPD MCP server with authentication token");
        metadsl()
        {:
            $sid = "tapd";
            $url = "http://mcp-oa.tapd.woa.com/mcp/";
            mcp_clear_options($sid);
            mcp_set_option($sid, "timeout", "20000");
            mcp_set_option($sid, "header", "Accept:application/json");
            mcp_set_option($sid, "header", "X-Tapd-Access-Token:%tapd_token%");
            mcp_connect($sid, "streamable-http", $url);
        :};
    };
    tool(list) {
        document("call_skill('tapd-skill:list'); List available TAPD MCP tools");
        metadsl()
        {:
            $sid = "tapd";
            mcp_list_tools($sid);
        :};
    };
    tool(help) {
        document("call_skill('tapd-skill:help', tool_name); Show tool help");
        metadsl($tool_name)
        {:
            $sid = "tapd";
            mcp_call_tool_callback($sid, "lookup_tool_param_schema", '{"tool_name":"'+$tool_name+'"}', "tapd_callback");
        :};
    };
    tool(bugs) {
        document("call_skill('tapd-skill:bugs'); Requires TAPD MCP connected first");
        metadsl()
        {:
            $sid = "tapd";
            mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"user_todo_bugs_get","tool_args":{}}', "tapd_callback");
        :};
    };
    tool(stories) {
        document("call_skill('tapd-skill:stories'); Get user todo stories list. Requires TAPD MCP connected first");
        metadsl()
        {:
            $sid = "tapd";
            mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"user_todo_stories_get","tool_args":{}}', "tapd_callback");
        :};
    };
    tool(tasks) {
        document("call_skill('tapd-skill:tasks'); Get user todo tasks list. Requires TAPD MCP connected first");
        metadsl()
        {:
            $sid = "tapd";
            mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"user_todo_tasks_get","tool_args":{}}', "tapd_callback");
        :};
    };
    tool(workspaces) {
        document("call_skill('tapd-skill:workspaces'); Get user participant workspace list. Requires TAPD MCP connected first");
        metadsl()
        {:
            $sid = "tapd";
            mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"user_participant_workspace_get","tool_args":{}}', "tapd_callback");
        :};
    };
    tool(set_context) {
        document("call_skill('tapd-skill:set_context', workspace_id, object_type); Set TAPD context for field queries. workspace_id: project ID; object_type: story/bug/task");
        metadsl($workspace_id, $object_type)
        {:
            set_context_var("tapd_workspace_id", $workspace_id);
            set_context_var("tapd_object_type", $object_type);
            "context set: workspace_id="+$workspace_id+", object_type="+$object_type;
        :};
    };
    tool(fields_summary) {
        document("call_skill('tapd-skill:fields_summary'); Get field summary for current context. Set context first with set_context");
        metadsl()
        {:
            $sid = "tapd";
            $wid = get_context_var("tapd_workspace_id");
            $otype = get_context_var("tapd_object_type");
            mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"tapd_fields_summary_get","tool_args":{"workspace_id":"'+$wid+'","object_type":"'+$otype+'"}}', "tapd_callback");
        :};
    };
    tool(field_detail) {
        document("call_skill('tapd-skill:field_detail', field_names_csv); Get field detail for current context. field_names_csv: comma-separated field names like 'status,priority'. Set context first with set_context");
        metadsl($field_names_csv)
        {:
            $sid = "tapd";
            $wid = get_context_var("tapd_workspace_id");
            $otype = get_context_var("tapd_object_type");
            $parts = string_split($field_names_csv, ",");
            $arr = "[";
            $first = 1;
            looplist($parts) {
                $f = $$;
                if($first == 1){$first = 0;}else{$arr = $arr + ",";};
                $arr = $arr + '"' + $f + '"';
            };
            $arr = $arr + "]";
            mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"tapd_field_detail_get","tool_args":{"workspace_id":"'+$wid+'","object_type":"'+$otype+'","field_names":'+$arr+'}}', "tapd_callback");
        :};
    };
    tool(tool) {
        document("call_skill('tapd-skill:tool', tool_name, tool_args_json); Requires TAPD MCP connected first. tool_name: TAPD MCP tool name like proxy_execute_tool; tool_args_json: JSON string of tool arguments; result is returned asynchronously via callback");
        metadsl($tool_name, $tool_args)
        {:
            $sid = "tapd";
            mcp_call_tool_callback($sid, $tool_name, $tool_args, "tapd_callback");
        :};
    };
    tool(disconnect) {
        document("call_skill('tapd-skill:disconnect'); Disconnect from TAPD MCP server");
        metadsl()
        {:
            $sid = "tapd";
            mcp_disconnect($sid);
        :};
    };
};
