using System;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.ScriptApi;

namespace CefDotnetApp.AgentCore
{
    /// <summary>
    /// Registers all Script APIs to the DSL script engine
    /// </summary>
    public static class ScriptApiRegistrar
    {
        /// <summary>
        /// Register all agent-related Script APIs
        /// </summary>
        public static void RegisterAllApis()
        {
            // File Operations
            BatchCommand.BatchScript.Register("read_file", "read_file(path)", new ExpressionFactoryHelper<ReadFileExp>());
            BatchCommand.BatchScript.Register("write_file", "write_file(path, content)", new ExpressionFactoryHelper<WriteFileExp>());
            BatchCommand.BatchScript.Register("append_file", "append_file(path, content)", new ExpressionFactoryHelper<AppendFileExp>());

            BatchCommand.BatchScript.Register("delete_file", "delete_file(path)", new ExpressionFactoryHelper<DeleteFileExp>());
            BatchCommand.BatchScript.Register("copy_file", "copy_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<CopyFileExp>());
            BatchCommand.BatchScript.Register("move_file", "move_file(sourcePath, destPath, overwrite)", new ExpressionFactoryHelper<MoveFileExp>());
            BatchCommand.BatchScript.Register("file_exists", "file_exists(path)", new ExpressionFactoryHelper<FileExistsExp>());
            BatchCommand.BatchScript.Register("dir_exists", "dir_exists(path)", new ExpressionFactoryHelper<DirExistsExp>());
            BatchCommand.BatchScript.Register("create_dir", "create_dir(path)", new ExpressionFactoryHelper<CreateDirExp>());
            BatchCommand.BatchScript.Register("delete_dir", "delete_dir(path, recursive)", new ExpressionFactoryHelper<DeleteDirExp>());
            BatchCommand.BatchScript.Register("list_dir", "list_dir(path, pattern, recursive)", new ExpressionFactoryHelper<ListDirExp>());


            // Code Editing Operations
            BatchCommand.BatchScript.Register("replace_file", "replace_file(path, oldString, newString, allOccurrences)", new ExpressionFactoryHelper<ReplaceFileExp>());
            BatchCommand.BatchScript.Register("replace_range", "replace_range(path, startLine, endLine, newContent)", new ExpressionFactoryHelper<ReplaceRangeExp>());
            BatchCommand.BatchScript.Register("insert_after", "insert_after(path, searchPattern, content, allOccurrences)", new ExpressionFactoryHelper<InsertAfterExp>());
            BatchCommand.BatchScript.Register("insert_before", "insert_before(path, searchPattern, content, allOccurrences)", new ExpressionFactoryHelper<InsertBeforeExp>());
            BatchCommand.BatchScript.Register("delete_lines", "delete_lines(path, startLine, endLine)", new ExpressionFactoryHelper<DeleteLinesExp>());
            BatchCommand.BatchScript.Register("search_file", "search_file(path, pattern, useRegex)", new ExpressionFactoryHelper<SearchFileExp>());
            BatchCommand.BatchScript.Register("read_lines", "read_lines(path, startLine, endLine)", new ExpressionFactoryHelper<ReadLinesExp>());
            BatchCommand.BatchScript.Register("get_line_count", "get_line_count(path)", new ExpressionFactoryHelper<GetLineCountExp>());


            // Advanced Code Analysis Operations
            BatchCommand.BatchScript.Register("find_symbol", "find_symbol(path, symbolName, symbolType)", new ExpressionFactoryHelper<FindSymbolExp>());
            BatchCommand.BatchScript.Register("find_usages", "find_usages(path, symbolName)", new ExpressionFactoryHelper<FindUsagesExp>());
            BatchCommand.BatchScript.Register("get_functions", "get_functions(path)", new ExpressionFactoryHelper<GetFunctionsExp>());
            BatchCommand.BatchScript.Register("get_classes", "get_classes(path)", new ExpressionFactoryHelper<GetClassesExp>());
            BatchCommand.BatchScript.Register("find_function", "find_function(path, functionName)", new ExpressionFactoryHelper<FindFunctionExp>());
            BatchCommand.BatchScript.Register("find_class", "find_class(path, className)", new ExpressionFactoryHelper<FindClassExp>());
            BatchCommand.BatchScript.Register("get_imports", "get_imports(path)", new ExpressionFactoryHelper<GetImportsExp>());
            BatchCommand.BatchScript.Register("add_import", "add_import(path, importStatement)", new ExpressionFactoryHelper<AddImportExp>());


            // Diff Operations
            BatchCommand.BatchScript.Register("apply_diff", "apply_diff(targetPath, diffPath)", new ExpressionFactoryHelper<ApplyDiffExp>());
            BatchCommand.BatchScript.Register("apply_diff_full", "apply_diff_full(targetPath, diffPath)", new ExpressionFactoryHelper<ApplyDiffFullExp>());
            BatchCommand.BatchScript.Register("apply_diff_libgit2", "apply_diff_libgit2(targetPath, diffContent, createBackup)", new ExpressionFactoryHelper<ApplyDiffLibGit2Exp>());


            // Clipboard Operations
            BatchCommand.BatchScript.Register("get_clipboard", "get_clipboard()", new ExpressionFactoryHelper<GetClipboardExp>());
            BatchCommand.BatchScript.Register("set_clipboard", "set_clipboard(text)", new ExpressionFactoryHelper<SetClipboardExp>());


            // Logging Operations
            BatchCommand.BatchScript.Register("log_info", "log_info(fmt, ...)", new ExpressionFactoryHelper<LogInfoExp>());
            BatchCommand.BatchScript.Register("log_error", "log_error(fmt, ...)", new ExpressionFactoryHelper<LogErrorExp>());
            BatchCommand.BatchScript.Register("log_warning", "log_warning(fmt, ...)", new ExpressionFactoryHelper<LogWarningExp>());


            // HTTP Operations
            BatchCommand.BatchScript.Register("http_get", "http_get(url)", new ExpressionFactoryHelper<HttpGetExp>());
            BatchCommand.BatchScript.Register("http_post", "http_post(url, content, contentType)", new ExpressionFactoryHelper<HttpPostExp>());
            BatchCommand.BatchScript.Register("download_file", "download_file(url, savePath)", new ExpressionFactoryHelper<DownloadFileExp>());


            // Task Management Operations
            BatchCommand.BatchScript.Register("create_task", "create_task(title, description)", new ExpressionFactoryHelper<CreateTaskExp>());
            BatchCommand.BatchScript.Register("update_task_status", "update_task_status(taskId, status, result)", new ExpressionFactoryHelper<UpdateTaskStatusExp>());
            BatchCommand.BatchScript.Register("get_task_status", "get_task_status(taskId)", new ExpressionFactoryHelper<GetTaskStatusExp>());


            // LLM Interaction Operations
            BatchCommand.BatchScript.Register("create_conversation", "create_conversation(title)", new ExpressionFactoryHelper<CreateConversationExp>());
            BatchCommand.BatchScript.Register("add_message", "add_message(conversationId, role, content)", new ExpressionFactoryHelper<AddMessageExp>());
            BatchCommand.BatchScript.Register("add_user_message", "add_user_message(content)", new ExpressionFactoryHelper<AddUserMessageExp>());
            BatchCommand.BatchScript.Register("add_assistant_message", "add_assistant_message(content)", new ExpressionFactoryHelper<AddAssistantMessageExp>());
            BatchCommand.BatchScript.Register("export_conversation", "export_conversation(conversationId)", new ExpressionFactoryHelper<ExportConversationExp>());


            // JSON Operations
            BatchCommand.BatchScript.Register("to_json", "to_json(obj, prettyPrint)", new ExpressionFactoryHelper<ToJsonExp>());
            BatchCommand.BatchScript.Register("from_json", "from_json(json)", new ExpressionFactoryHelper<FromJsonExp>());
            BatchCommand.BatchScript.Register("new_object", "new_object(key1, value1, key2, value2, ...)", new ExpressionFactoryHelper<NewObjectExp>());


            // Process Operations
            BatchCommand.BatchScript.Register("execute_command", "execute_command(command, arguments, workingDir, timeout)", new ExpressionFactoryHelper<ExecuteCommandExp>());
            BatchCommand.BatchScript.Register("start_process", "start_process(processId, command, arguments, workingDir)", new ExpressionFactoryHelper<StartProcessExp>());
            BatchCommand.BatchScript.Register("stop_process", "stop_process(processId, timeout)", new ExpressionFactoryHelper<StopProcessExp>());
            BatchCommand.BatchScript.Register("is_process_running", "is_process_running(processId)", new ExpressionFactoryHelper<IsProcessRunningExp>());
            BatchCommand.BatchScript.Register("write_process_input", "write_process_input(processId, input)", new ExpressionFactoryHelper<WriteProcessInputExp>());
            BatchCommand.BatchScript.Register("read_process_output", "read_process_output(processId)", new ExpressionFactoryHelper<ReadProcessOutputExp>());
            BatchCommand.BatchScript.Register("read_process_error", "read_process_error(processId)", new ExpressionFactoryHelper<ReadProcessErrorExp>());


            // Context Management Operations
            BatchCommand.BatchScript.Register("create_workspace", "create_workspace(name, rootPath)", new ExpressionFactoryHelper<CreateWorkspaceExp>());
            BatchCommand.BatchScript.Register("set_context_var", "set_context_var(key, value, scope)", new ExpressionFactoryHelper<SetContextVarExp>());
            BatchCommand.BatchScript.Register("get_context_var", "get_context_var(key, scope)", new ExpressionFactoryHelper<GetContextVarExp>());
            BatchCommand.BatchScript.Register("add_open_file", "add_open_file(filePath)", new ExpressionFactoryHelper<AddOpenFileExp>());
            BatchCommand.BatchScript.Register("get_open_files", "get_open_files()", new ExpressionFactoryHelper<GetOpenFilesExp>());
            BatchCommand.BatchScript.Register("add_recent_file", "add_recent_file(filePath)", new ExpressionFactoryHelper<AddRecentFileExp>());
            BatchCommand.BatchScript.Register("get_recent_files", "get_recent_files(maxCount)", new ExpressionFactoryHelper<GetRecentFilesExp>());


            // Template Engine Operations
            BatchCommand.BatchScript.Register("register_template", "register_template(name, template)", new ExpressionFactoryHelper<RegisterTemplateExp>());
            BatchCommand.BatchScript.Register("load_template", "load_template(name, filePath)", new ExpressionFactoryHelper<LoadTemplateExp>());
            BatchCommand.BatchScript.Register("render_template", "render_template(templateName, variables)", new ExpressionFactoryHelper<RenderTemplateExp>());
            BatchCommand.BatchScript.Register("render_template_string", "render_template_string(template, variables)", new ExpressionFactoryHelper<RenderTemplateStringExp>());
            BatchCommand.BatchScript.Register("save_rendered_template", "save_rendered_template(templateName, variables, outputPath)", new ExpressionFactoryHelper<SaveRenderedTemplateExp>());
            BatchCommand.BatchScript.Register("create_class_template", "create_class_template(className, namespaceName, properties)", new ExpressionFactoryHelper<CreateClassTemplateExp>());
            BatchCommand.BatchScript.Register("create_function_template", "create_function_template(functionName, returnType, parameters, body)", new ExpressionFactoryHelper<CreateFunctionTemplateExp>());


            // Browser Interaction Operations
            BatchCommand.BatchScript.Register("build_query_selector", "build_query_selector(selector)", new ExpressionFactoryHelper<BuildQuerySelectorExp>());
            BatchCommand.BatchScript.Register("build_click_element", "build_click_element(selector)", new ExpressionFactoryHelper<BuildClickElementExp>());
            BatchCommand.BatchScript.Register("build_set_value", "build_set_value(selector, value)", new ExpressionFactoryHelper<BuildSetValueExp>());
            BatchCommand.BatchScript.Register("build_get_value", "build_get_value(selector)", new ExpressionFactoryHelper<BuildGetValueExp>());
            BatchCommand.BatchScript.Register("build_get_text", "build_get_text(selector)", new ExpressionFactoryHelper<BuildGetTextExp>());
            BatchCommand.BatchScript.Register("build_set_innerhtml", "build_set_innerhtml(selector, html)", new ExpressionFactoryHelper<BuildSetInnerHTMLExp>());
            BatchCommand.BatchScript.Register("build_wait_for_element", "build_wait_for_element(selector, timeout)", new ExpressionFactoryHelper<BuildWaitForElementExp>());
            BatchCommand.BatchScript.Register("build_scroll_to_element", "build_scroll_to_element(selector)", new ExpressionFactoryHelper<BuildScrollToElementExp>());
            BatchCommand.BatchScript.Register("build_is_visible", "build_is_visible(selector)", new ExpressionFactoryHelper<BuildIsVisibleExp>());
            BatchCommand.BatchScript.Register("build_add_class", "build_add_class(selector, className)", new ExpressionFactoryHelper<BuildAddClassExp>());
            BatchCommand.BatchScript.Register("build_remove_class", "build_remove_class(selector, className)", new ExpressionFactoryHelper<BuildRemoveClassExp>());
            BatchCommand.BatchScript.Register("build_set_style", "build_set_style(selector, property, value)", new ExpressionFactoryHelper<BuildSetStyleExp>());
            BatchCommand.BatchScript.Register("build_inject_css", "build_inject_css(css)", new ExpressionFactoryHelper<BuildInjectCSSExp>());
            BatchCommand.BatchScript.Register("build_navigate_to", "build_navigate_to(url)", new ExpressionFactoryHelper<BuildNavigateToExp>());

            // Agent Message Operations
            BatchCommand.BatchScript.Register("handle_agent_message", "handle_agent_message(jsonData)", new ExpressionFactoryHelper<HandleAgentMessageExp>());
            BatchCommand.BatchScript.Register("send_command_to_inject", "send_command_to_inject(command, paramsJson)", new ExpressionFactoryHelper<SendCommandToInjectExp>());
            BatchCommand.BatchScript.Register("build_agent_response", "build_agent_response(messageId, success, data, error)", new ExpressionFactoryHelper<BuildAgentResponseExp>());
            BatchCommand.BatchScript.Register("parse_agent_message", "parse_agent_message(jsonData)", new ExpressionFactoryHelper<ParseAgentMessageExp>());
            BatchCommand.BatchScript.Register("get_message_param", "get_message_param(paramsObj, key)", new ExpressionFactoryHelper<GetMessageParamExp>());

            // MetaDSL Operations (for self-bootstrapping)
            MetaDSLApi.RegisterApis();
        }
    }
}
