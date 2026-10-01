# jetbrains-debugger-skill

JetBrains Debugger MCP integration (streamable-http). Wraps MCP session so metadsl can drive it via mcp_call_tool_callback.

## Usage Flow

1. Connect: call_skill('jetbrains-debugger-skill:connect')
2. List / Call: call_skill('jetbrains-debugger-skill:list') or call_skill('jetbrains-debugger-skill:tool', tool_name, tool_args_json)
3. Disconnect: call_skill('jetbrains-debugger-skill:disconnect')

## Functions

connect     - Establish MCP session (serverId="jetbrains-debugger", transport="streamable-http", target="http://127.0.0.1:29191/debugger-mcp/streamable-http")
list        - mcp_list_tools on the session
tool        - Generic mcp_call_tool_callback wrapper; result via 'jetbrains_debugger_callback'
disconnect  - Close the session

## Example

call_skill('jetbrains-debugger-skill:connect');
call_skill('jetbrains-debugger-skill:list');
call_skill('jetbrains-debugger-skill:tool', 'some_tool_name', '{"k":"v"}');
call_skill('jetbrains-debugger-skill:disconnect');

## Notes

- Tool calls are async; results arrive via callback tag 'jetbrains_debugger_callback'.
- Requires JetBrains debugger MCP endpoint running on 127.0.0.1:29191.
- Extra tool sugars are intentionally omitted; add on demand later.
