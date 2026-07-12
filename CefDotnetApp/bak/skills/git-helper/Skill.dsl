skill("git-helper")
{
    tool(status) {
        document("call_skill('git-helper:status', repo_path); Show git working tree status for the given repository path");
        metadsl($repo_path)
        {:
            execute_command("git", "status --short --branch", $repo_path);
        :};
    };
    tool(log) {
        document("call_skill('git-helper:log', repo_path[, count]); Show recent commit log. count defaults to 10");
        metadsl($repo_path, $count)
        {:
            if($count == ""){
                $count = "10";
            };
            $args = "log --oneline --graph --decorate -" + $count;
            execute_command("git", $args, $repo_path);
        :};
    };
    tool(diff) {
        document("call_skill('git-helper:diff', repo_path[, file_path]); Show unstaged changes. Optionally specify a file path");
        metadsl($repo_path, $file_path)
        {:
            if($file_path == ""){
                execute_command("git", "diff", $repo_path);
            }else{
                $args = "diff -- " + $file_path;
                execute_command("git", $args, $repo_path);
            };
        :};
    };
    tool(diff_staged) {
        document("call_skill('git-helper:diff_staged', repo_path[, file_path]); Show staged changes. Optionally specify a file path");
        metadsl($repo_path, $file_path)
        {:
            if($file_path == ""){
                execute_command("git", "diff --cached", $repo_path);
            }else{
                $args = "diff --cached -- " + $file_path;
                execute_command("git", $args, $repo_path);
            };
        :};
    };
    tool(branch) {
        document("call_skill('git-helper:branch', repo_path); List all branches and show current branch");
        metadsl($repo_path)
        {:
            execute_command("git", "branch -a", $repo_path);
        :};
    };
};
