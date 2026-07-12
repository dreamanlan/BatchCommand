# tapd-skill

TAPD MCP tool wrapper. Provides convenient functions to interact with TAPD via MCP protocol.

## Environment Variables

- tapd_token: TAPD access token (required, set in agent config)

## Usage Flow

1. Connect: call_skill('tapd-skill:connect')
2. Query/Execute: use tool functions below
3. Disconnect: call_skill('tapd-skill:disconnect')

## Functions

### Connection

connect - Connect to TAPD MCP server. Token is read from %tapd_token% internally.
  call_skill('tapd-skill:connect')

disconnect - Disconnect from TAPD MCP server.
  call_skill('tapd-skill:disconnect')

### Discovery

list - List all available TAPD MCP tools.
  call_skill('tapd-skill:list')

help - Show parameter schema for a specific tool.
  call_skill('tapd-skill:help', tool_name)
  Example: call_skill('tapd-skill:help', 'user_todo_bugs_get')

### Generic

tool - Generic tool call. Pass tool name and JSON args string.
  call_skill('tapd-skill:tool', tool_name, tool_args_json)
  Example: call_skill('tapd-skill:tool', 'proxy_execute_tool', '{"tool_name":"user_todo_bugs_get","tool_args":{}}')

### Shortcuts

bugs - Get user's todo bug list. No parameters needed.
  call_skill('tapd-skill:bugs')

stories - Get user's todo story/requirement list. No parameters needed.
  call_skill('tapd-skill:stories')

tasks - Get user's todo task list. No parameters needed.
  call_skill('tapd-skill:tasks')

workspaces - Get user's participated workspace/project list. No parameters needed.
  call_skill('tapd-skill:workspaces')
### Context & Field Queries

set_context - Set workspace_id and object_type context for field queries.
  call_skill('tapd-skill:set_context', workspace_id, object_type)
  Example: call_skill('tapd-skill:set_context', '20424652', 'bug')

fields_summary - Get field summary for current context. Set context first.
  call_skill('tapd-skill:fields_summary')

field_detail - Get field detail for current context. Pass comma-separated field names.
  call_skill('tapd-skill:field_detail', field_names_csv)
  Example: call_skill('tapd-skill:field_detail', 'status,priority_label')

## Notes

- MCP calls are async. Results are returned via callback tag "tapd_callback".
- TAPD MCP uses proxy pattern: most tools need to be called via proxy_execute_tool.
- Use help function to check tool parameter schema before calling.
