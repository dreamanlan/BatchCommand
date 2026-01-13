// ========================================
// Agent DSL Script Example
// This script demonstrates the usage of all agent APIs
// ========================================

// ========== Initialization ==========
script(on_init)
{
    nativelog("[dsl] Agent initialization started");

    // Create workspace
    $workspaceId = createworkspace("CodingProject", basepath);
    nativelog("[dsl] Created workspace: {0}", $workspaceId);

    // Set context variables
    setcontextvar("project_type", "csharp", "workspace");
    setcontextvar("session_start", timestamp(), "session");

    nativelog("[dsl] Agent initialization completed");
    return(0);
};

// ========== Browser Initialization ==========
script(on_browser_init)
{
    nativelog("[dsl] Browser initialized");

    // Inject custom CSS
    $css = ".agent-highlight { background-color: yellow; }";
    $script = buildinjectcss($css);
    nativeapi.ExecuteJavascript($script);

    return(0);
};

// ========== Message Handlers ==========

// Handle CEF messages from browser process
script(on_receive_cef_message)params($msg, $arg1, $arg2, $arg3)
{
    nativelog("[dsl] Received CEF message: {0}", $msg);

    if ($msg == "analyze_project") {
        handle_analyze_project($arg1);
    }
    elif ($msg == "generate_code") {
        handle_generate_code($arg1, $arg2);
    }
    elif ($msg == "run_build") {
        handle_run_build($arg1);
    }
    elif ($msg == "search_symbol") {
        handle_search_symbol($arg1, $arg2);
    };

    return(0);
};

// Handle JavaScript messages from renderer process
script(on_receive_js_message)params($msg, $arg1, $arg2)
{
    nativelog("[dsl] Received JS message: {0}", $msg);

    if ($msg == "click_button") {
        // Build and execute click script
        $script = buildclickelement($arg1);
        nativeapi.ExecuteJavascript($script);
    }
    elif ($msg == "get_input_value") {
        // Build script to get input value
        $script = buildgetvalue($arg1);
        nativeapi.ExecuteJavascript($script);
    }
    elif ($msg == "set_input_value") {
        // Build script to set input value
        $script = buildsetvalue($arg1, $arg2);
        nativeapi.ExecuteJavascript($script);
    }
    elif ($msg == "wait_for_element") {
        // Build script to wait for element
        $script = buildwaitforelement($arg1, 5000);
        nativeapi.ExecuteJavascript($script);
    };

    return(0);
};

// ========== Handler Functions ==========

// Analyze project code
script(handle_analyze_project)params($projectPath)
{
    nativelog("[dsl] Analyzing project: {0}", $projectPath);

    // Analyze all C# files
    $stats = analyzedirectory($projectPath, "*.cs", true);

    $totalLines = 0;
    $totalCodeLines = 0;
    $totalFiles = 0;

    // Calculate totals
    foreach ($file in $stats) {
        $totalFiles = $totalFiles + 1;
        $totalLines = $totalLines + $file.totalLines;
        $totalCodeLines = $totalCodeLines + $file.codeLines;
    };

    // Log results
    loginfo("Project Analysis Results:");
    loginfo("  Total Files: {0}", $totalFiles);
    loginfo("  Total Lines: {0}", $totalLines);
    loginfo("  Code Lines: {0}", $totalCodeLines);

    // Save results to context
    setcontextvar("last_analysis_files", $totalFiles, "session");
    setcontextvar("last_analysis_lines", $totalLines, "session");

    // Create analysis report
    $report = format("Project Analysis\n================\nFiles: {0}\nLines: {1}\nCode: {2}\n",
                     $totalFiles, $totalLines, $totalCodeLines);
    writefile("analysis_report.txt", $report);

    return(0);
};

// Generate code from template
script(handle_generate_code)params($codeType, $params)
{
    nativelog("[dsl] Generating code: {0}", $codeType);

    if ($codeType == "class") {
        // Parse parameters
        $className = $params.className;
        $namespace = $params.namespace;
        $properties = $params.properties;

        // Generate class
        $code = createclasstemplate($className, $namespace, $properties);

        // Save to file
        $filename = format("{0}.cs", $className);
        writefile($filename, $code);

        loginfo("Generated class: {0}", $filename);
        addrecentfile($filename);
    }
    elif ($codeType == "function") {
        // Parse parameters
        $funcName = $params.functionName;
        $returnType = $params.returnType;
        $parameters = $params.parameters;

        // Generate function
        $code = createfunctiontemplate($funcName, $returnType, $parameters, null);

        loginfo("Generated function: {0}", $funcName);

        // Copy to clipboard
        setclipboard($code);
        loginfo("Function code copied to clipboard");
    }
    elif ($codeType == "custom") {
        // Use custom template
        $templateName = $params.templateName;
        $variables = $params.variables;

        // Render template
        $code = rendertemplate($templateName, $variables);

        if ($code != null) {
            $outputPath = $params.outputPath;
            writefile($outputPath, $code);
            loginfo("Generated code from template: {0}", $outputPath);
        } else {
            logerror("Template not found: {0}", $templateName);
        };
    };

    return(0);
};

// Run build command
script(handle_run_build)params($projectPath)
{
    nativelog("[dsl] Running build: {0}", $projectPath);

    // Create build task
    $taskId = createtask("Build Project", format("Building {0}", $projectPath));
    updatetaskstatus($taskId, "InProgress", null);

    // Execute build command
    $result = executecommand("dotnet", "build", $projectPath, 60000);

    if ($result.success) {
        loginfo("Build succeeded");
        loginfo("Output: {0}", $result.output);
        updatetaskstatus($taskId, "Completed", "Build succeeded");
    } else {
        logerror("Build failed");
        logerror("Error: {0}", $result.error);
        updatetaskstatus($taskId, "Failed", $result.error);
    };

    loginfo("Build time: {0}ms", $result.executionTime);

    return(0);
};

// Search for symbol in code
script(handle_search_symbol)params($symbol, $directory)
{
    nativelog("[dsl] Searching for symbol: {0} in {1}", $symbol, $directory);

    // Find files containing symbol
    $files = findsymbol($symbol, $directory, "*.cs", true);

    loginfo("Found symbol in {0} files", listsize($files));

    // Search for exact matches
    $matches = searchcode($symbol, $directory, "*.cs", false, true);

    loginfo("Found {0} exact matches", listsize($matches));

    // Log first few matches
    $count = 0;
    foreach ($match in $matches) {
        if ($count < 5) {
            loginfo("  {0}:{1} - {2}", $match.filePath, $match.lineNumber, $match.lineContent);
            $count = $count + 1;
        };
    };

    return(0);
};

// ========== Utility Functions ==========

// Extract all functions from a file
script(extract_file_functions)params($filePath)
{
    $language = "csharp";

    // Detect language from extension
    if (endswith($filePath, ".js")) {
        $language = "javascript";
    }
    elif (endswith($filePath, ".py")) {
        $language = "python";
    };

    // Extract functions
    $functions = extractfunctions($filePath, $language);

    loginfo("Extracted {0} functions from {1}", listsize($functions), $filePath);

    foreach ($func in $functions) {
        loginfo("  Function: {0} at line {1}", $func.name, $func.lineNumber);
    };

    return($functions);
};

// Extract all classes from a file
script(extract_file_classes)params($filePath)
{
    $language = "csharp";

    // Detect language from extension
    if (endswith($filePath, ".js")) {
        $language = "javascript";
    }
    elif (endswith($filePath, ".py")) {
        $language = "python";
    };

    // Extract classes
    $classes = extractclasses($filePath, $language);

    loginfo("Extracted {0} classes from {1}", listsize($classes), $filePath);

    foreach ($class in $classes) {
        loginfo("  Class: {0} at line {1}", $class.name, $class.lineNumber);
    };

    return($classes);
};

// Create and manage a background process
script(run_background_process)params($processId, $command, $args)
{
    nativelog("[dsl] Starting background process: {0}", $processId);

    // Start process
    startprocess($processId, $command, $args, basepath);

    // Wait a bit
    sleep(1000);

    // Check if running
    if (isprocessrunning($processId)) {
        loginfo("Process {0} is running", $processId);

        // Read output
        $output = readprocessoutput($processId);
        if ($output != null) {
            loginfo("Process output: {0}", $output);
        };

        // Read error
        $error = readprocesserror($processId);
        if ($error != null) {
            logerror("Process error: {0}", $error);
        };
    } else {
        logerror("Process {0} failed to start", $processId);
    };

    return(0);
};

// ========== Template Examples ==========

// Register custom templates
script(register_custom_templates)
{
    // Register a simple class template
    $classTemplate = "using System;\n\nnamespace {{namespace}}\n{\n    public class {{className}}\n    {\n        {{#each properties}}\n        public {{item}} { get; set; }\n        {{/each}}\n    }\n}";
    registertemplate("simple_class", $classTemplate);

    // Register a controller template
    $controllerTemplate = "using Microsoft.AspNetCore.Mvc;\n\nnamespace {{namespace}}.Controllers\n{\n    [ApiController]\n    [Route(\"api/[controller]\")]\n    public class {{controllerName}} : ControllerBase\n    {\n        // TODO: Add controller actions\n    }\n}";
    registertemplate("api_controller", $controllerTemplate);

    loginfo("Registered custom templates");

    return(0);
};

// ========== Finalization ==========
script(on_finalize)
{
    nativelog("[dsl] Agent finalization");

    // Log session statistics
    $sessionStart = getcontextvar("session_start", "session");
    $sessionEnd = timestamp();

    loginfo("Session duration: {0}s", $sessionEnd - $sessionStart);

    // Get recent files
    $recentFiles = getrecentfiles(5);
    loginfo("Recent files: {0}", listsize($recentFiles));

    return(0);
};
