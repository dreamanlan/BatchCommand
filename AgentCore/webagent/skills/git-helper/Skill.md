# git-helper

Git repository helper tools. Provides quick access to common git operations like status, log, diff and branch listing.

## Tools

### status
Show working tree status (short format with branch info).

    call_skill('git-helper:status', 'd:/AiClaw');

### log
Show recent commit log (oneline graph format). Default 10 entries.

    call_skill('git-helper:log', 'd:/AiClaw');
    call_skill('git-helper:log', 'd:/AiClaw', '20');

### diff
Show unstaged changes. Optionally specify a file path.

    call_skill('git-helper:diff', 'd:/AiClaw');
    call_skill('git-helper:diff', 'd:/AiClaw', 'js/main.js');

### diff_staged
Show staged (cached) changes. Optionally specify a file path.

    call_skill('git-helper:diff_staged', 'd:/AiClaw');

### branch
List all branches (local and remote) and show current branch.

    call_skill('git-helper:branch', 'd:/AiClaw');

## Notes
- All tools require a repo_path parameter pointing to a git repository
- This skill only performs read-only git operations (no add/commit/push)
- For safety, destructive git commands are intentionally excluded
