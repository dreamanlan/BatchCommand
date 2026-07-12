# renderdoc-skill

RenderDoc MCP integration (stdio). Wraps MCP session so metadsl can drive it via mcp_call_tool_callback. Automatically injects PYTHONWARNINGS to suppress SWIG deprecation noise on connect.

## Usage Flow

1. Connect: call_skill('renderdoc-skill:connect')
2. List / Call: call_skill('renderdoc-skill:list') or call_skill('renderdoc-skill:tool', tool_name, tool_args_json)
3. Disconnect: call_skill('renderdoc-skill:disconnect')

## Functions

connect     - set_environment('PYTHONWARNINGS','ignore::DeprecationWarning'); mcp_connect(serverId="renderdoc", transport="stdio", target="renderdoc-mcp.exe")
list        - mcp_list_tools on the session
tool        - Generic mcp_call_tool_callback wrapper; result via 'renderdoc_callback'
disconnect  - Close the session

## Example

call_skill('renderdoc-skill:connect');
call_skill('renderdoc-skill:list');
call_skill('renderdoc-skill:tool', 'open_capture', '{"path":"E:/tmp/xxx.rdc"}');
call_skill('renderdoc-skill:disconnect');

## Notes

- Tool calls are async; results arrive via callback tag 'renderdoc_callback'.
- Requires renderdoc-mcp.exe on PATH (source: D:/GitHub/RenderDocMCP).
- Extra tool sugars are intentionally omitted; add on demand later.
