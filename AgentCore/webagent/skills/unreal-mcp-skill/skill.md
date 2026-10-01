# unreal-mcp-skill

Unreal Engine MCP integration (stdio). Wraps MCP session so metadsl can drive it via mcp_call_tool_callback. Talks to Unreal Editor via Python Remote Execution multicast.

## Usage Flow

1. Connect: call_skill('unreal-mcp-skill:connect')
2. List / Call: call_skill('unreal-mcp-skill:list') or call_skill('unreal-mcp-skill:tool', tool_name, tool_args_json)
3. Disconnect: call_skill('unreal-mcp-skill:disconnect')

## Functions

connect     - Establish MCP session (serverId="unreal", transport="stdio", target="node D:\\GitHub\\unreal-mcp\\dist\\bin.js")
list        - mcp_list_tools on the session
tool        - Generic mcp_call_tool_callback wrapper; result via 'unreal_callback'
disconnect  - Close the session

## Example

call_skill('unreal-mcp-skill:connect');
call_skill('unreal-mcp-skill:list');
call_skill('unreal-mcp-skill:tool', 'editortaillog', '{"path":"...","offset":0}');
call_skill('unreal-mcp-skill:disconnect');

## Notes

- Tool calls are async; results arrive via callback tag 'unreal_callback'.
- Requires Node.js and the built unreal-mcp bundle at D:\\GitHub\\unreal-mcp\\dist\\bin.js.
- Requires Unreal Editor running with Python Remote Execution enabled.
- Extra tool sugars are intentionally omitted; add on demand later.
