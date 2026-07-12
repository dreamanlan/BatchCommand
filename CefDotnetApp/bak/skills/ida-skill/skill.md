# ida-skill

IDA Pro MCP integration. Wraps MCP streamable-http connection so metadsl can drive it via mcp_call_tool_callback.

## Usage Flow

1. Connect: call_skill('ida-skill:connect')
2. List / Call: call_skill('ida-skill:list') or call_skill('ida-skill:tool', tool_name, tool_args_json)
3. Disconnect: call_skill('ida-skill:disconnect')

## Functions

connect     - Establish MCP session (serverId="ida-pro-mcp", transport="streamable-http", target="http://localhost:13337/mcp")
list        - mcp_list_tools on the session
tool        - Generic mcp_call_tool_callback wrapper; result via 'ida_pro_mcp_callback'
disconnect  - Close the session

## Example

call_skill('ida-skill:connect');
call_skill('ida-skill:list');
call_skill('ida-skill:tool', 'some_tool_name', '{"k":"v"}');
call_skill('ida-skill:disconnect');

## Notes

- Tool calls are async; results arrive via callback tag 'ida_pro_mcp_callback'.
- Extra tool sugars are intentionally omitted; add on demand later.
