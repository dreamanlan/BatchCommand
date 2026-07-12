# skill-guide

Skill system reference guide. Covers three skill types, configuration syntax, directory structure and API usage.

## Overview

Skills are modular extensions that add capabilities to the agent. Each skill lives in its own subdirectory under the skills/ folder. The system supports three types of skills:

1. MetaDSL Script Skill - executable metadsl code with named functions
2. Command Skill - invokes external programs (Python, shell scripts, etc.)
3. Document Skill - reference knowledge, no executable code

## Directory Structure

    skills/
      my-skill/
        Skill.dsl      # skill configuration (optional for document-only skills)
        Skill.md       # skill documentation (required)
        other-files     # additional scripts, data, etc.

## Type 1: MetaDSL Script Skill

Defines one or more named functions implemented in metadsl. Best for tasks that use agent built-in APIs (file operations, MCP calls, string processing, etc.).

File composition: Skill.dsl (required) + Skill.md (required)

### Skill.dsl Syntax

    skill("skill-name")
    {
        tool(function_name) {
            document("call_skill('skill-name:function_name', arg1, arg2); Description of what this function does");
            metadsl($param1, $param2)
            {:
                // metadsl code here
                // use $param1, $param2 as parameters
                // last expression value is the return value
            :};
        };
        tool(another_function) {
            document("call_skill('skill-name:another_function'); Another function description");
            metadsl()
            {:
                // code with no parameters
            :};
        };
    };

### Key Points

- Each tool() block defines one callable function
- The tool name becomes the function name after the colon in call_skill
- document() string should include the call_skill invocation example and parameter description
- metadsl() parameters use $ prefix, they map positionally to call_skill arguments
- metadsl code block uses {: :} delimiters
    - When a skill has only one tool, the tool name can be omitted: tool { ... }
    - When a skill has multiple tools, each tool MUST have a unique name: tool(name) { ... }

### Calling Convention

    Single tool (no name):  call_skill('skill-name', arg1, arg2)
    Named tool:             call_skill('skill-name:function_name', arg1, arg2)

### Real Example: tapd-skill

    skill("tapd-skill")
    {
        tool(connect) {
            document("call_skill('tapd-skill:connect'); Connect to TAPD MCP server");
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
        tool(bugs) {
            document("call_skill('tapd-skill:bugs'); Get user todo bug list");
            metadsl()
            {:
                $sid = "tapd";
                mcp_call_tool_callback($sid, "proxy_execute_tool", '{"tool_name":"user_todo_bugs_get","tool_args":{}}', "tapd_callback");
            :};
        };
    };

## Type 2: Command Skill

Invokes an external program (Python, shell script, executable, etc.). Best for tasks that need external libraries or tools not available in metadsl.

File composition: Skill.dsl (required) + Skill.md (required) + external script(s)

### Skill.dsl Syntax

    skill("skill-name")
    {
        tool {
            document("call_skill('skill-name', arg1, arg2); Description");
            command($param1, $param2)
            {:
                python {% basepath %}/skills/skill-name/script.py {% $param1 %} {% $param2 %}
            :};
        };
    };

### Key Points

- command() block defines the external command to execute
- Use {% basepath %} to reference the agent runtime base directory
- Use {% $param %} to interpolate parameters into the command line
- When a skill has only one tool, the tool name can be omitted: tool { ... }
    - When a skill has multiple tools, each tool MUST have a unique name: tool(name) { ... }
- External scripts should handle their own error reporting via stdout/stderr

### Calling Convention

    call_skill('skill-name', arg1, arg2)

### Real Example: flip-image

    skill("flip-image")
    {
        tool {
            document("call_skill('flip-image', image_path, flip_direction); flip_direction: 0=horizontal, 1=vertical");
            command($image_path, $flip_direction)
            {:
                python {% basepath %}/skills/flip-image/flip-image.py {% $image_path %} {% $flip_direction %}
            :};
        };
    };

## Type 3: Document Skill

Provides reference knowledge that LLM can query via help(). No executable code.

### Option A: With Skill.dsl

File composition: Skill.dsl (required) + Skill.md (required)

    skill("skill-name")
    {
        document("Brief description of this skill");
    };

The document() string appears as the skill summary in help() results.

### Option B: Skill.md Only (Zero Configuration)

File composition: Skill.md only (no Skill.dsl needed)

Simply create a directory under skills/ and put a Skill.md file in it. The system automatically:
- Uses the directory name as the skill name
- Extracts the first non-empty paragraph after the first # heading as the summary

If the Skill.md has YAML frontmatter, the system extracts:
- name field as the skill name
- description field as the summary

YAML frontmatter example (first lines of Skill.md):

    ---
    name: my-skill
    description: Brief description of what this skill provides
    ---
    # my-skill
    Detailed content here...

Plain markdown example (no frontmatter):

    # my-skill
    Brief description extracted as summary from this first paragraph.

    ## Section 1
    More detailed content...

### Calling Convention

    help("skill-name")       // query skill documentation
    help("keyword")          // search across all skills by keyword

## API Reference

### call_skill

Call an executable skill (Type 1 or Type 2).

    call_skill('skill-name:function', arg1, arg2, ...)   // metadsl skill with named function
    call_skill('skill-name', arg1, arg2, ...)             // command skill (anonymous function)

### refresh_skills

Reload all skills from the skills directory. Use after adding or modifying skill files.

    refresh_skills()

### help

Query skill documentation. Accepts regex patterns. Multiple patterns are OR-matched.

    help("skill-name")           // exact skill lookup
    help("keyword")              // search by keyword across all skills
    help("pattern1", "pattern2") // search with multiple patterns

## Tips

- Skill.md is always required. It serves as both the help content and the skill documentation.
- For metadsl skills, include call_skill examples in each tool's document() string so LLM knows how to invoke them.
- For command skills, ensure external dependencies (Python, pip packages, etc.) are pre-installed.
- Use refresh_skills() after creating or modifying any skill files.
- Document skills (Type 3) are the simplest to create. Just write a Skill.md file.
- The skills/ directory path is configured in the agent runtime, typically at the same level as the managed/ directory.
