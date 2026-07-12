# jetbrains-skill

JetBrains IDE MCP integration (SSE). Wraps MCP session so metadsl can drive it via mcp_call_tool_callback.

## Usage Flow

1. Connect: call_skill('jetbrains-skill:connect', project_path)  (pass "" if project path not needed)
2. List / Call: call_skill('jetbrains-skill:list') or call_skill('jetbrains-skill:tool', tool_name, tool_args_json)
3. Disconnect: call_skill('jetbrains-skill:disconnect')

## Functions

connect     - Establish MCP session (serverId="jetbrains", transport="sse", target="http://localhost:64342/sse"); if project_path non-empty, adds header IJ_MCP_SERVER_PROJECT_PATH
list        - mcp_list_tools on the session
tool        - Generic mcp_call_tool_callback wrapper; result via 'jetbrains_callback'
disconnect  - Close the session

## Example

call_skill('jetbrains-skill:connect', '');
call_skill('jetbrains-skill:connect', 'D:/MyProject');
call_skill('jetbrains-skill:list');
call_skill('jetbrains-skill:tool', 'some_tool_name', '{"k":"v"}');
call_skill('jetbrains-skill:disconnect');

## Notes

- Tool calls are async; results arrive via callback tag 'jetbrains_callback'.
- Requires JetBrains IDE with MCP server plugin running on port 64342.
- Extra tool sugars are intentionally omitted; add on demand later.
