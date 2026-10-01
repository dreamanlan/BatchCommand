# crashsight-skill

CrashSight MCP integration skill. Provides tools to connect and interact with CrashSight MCP server for crash analysis and monitoring.

## Tools

### connect
Connect to CrashSight MCP server with authentication token.
- Usage: call_skill('crashsight-skill:connect')
- Reads token from environment variable %crashsight_token%

### list
List all available CrashSight MCP tools.
- Usage: call_skill('crashsight-skill:list')
- Requires connection first

### help
Show parameter schema for a specific tool.
- Usage: call_skill('crashsight-skill:help', 'tool_name')
- Requires connection first

### tool
Call any CrashSight MCP tool with arguments.
- Usage: call_skill('crashsight-skill:tool', 'tool_name', '{"arg":"value"}')
- Result is returned asynchronously via callback
- Requires connection first

### Shortcuts

apps - Get user's application/project list.
- Usage: call_skill('crashsight-skill:apps')
- Requires connection first

crash_by_share - Get crash detail by shared report link.
- Usage: call_skill('crashsight-skill:crash_by_share', share_id)
- Extract shareId from URL: https://crashsight.qq.com/shared-report/{shareId}
- Requires connection first

crash_by_issue - Get latest crash detail by appId and issueId.
- Usage: call_skill('crashsight-skill:crash_by_issue', app_id, issue_id)
- Extract from URL: https://crashsight.qq.com/crash-reporting/crashes/{appId}/{issueId}
- Requires connection first

### disconnect
Disconnect from CrashSight MCP server.
- Usage: call_skill('crashsight-skill:disconnect')

## Notes
- Always connect before using other tools
- Tool results are returned asynchronously via crashsight_callback
- Timeout is set to 60 seconds
