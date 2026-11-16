// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    set_env("PLAYWRIGHT_DRIVER_SEARCH_PATH", combine_path(base_path, "managed"));

    if (process_type == 1) {
        @UserName = get_file_name(get_directory_name(get_directory_name(get_env("LOCALAPPDATA"))));
    }
    elif (is_mac) {
        @UserName = get_env("USER");
    }
    else {
        @UserName = get_env("USERNAME");
    };
};
script(on_init)
{
    native_log("[dsl] on_init finish, pid:{0}, type:{1}", pid(), process_type);
    file_echo(true);
    // no-sandbox = false
    // Always return false in the renderer.
    return(false);
};
script(on_finalize)
{
    native_log("[dsl] on_finalize finish, pid:{0}, type:{1}", pid(), process_type);
};
script(on_before_command_line_processing)params($processType, $cmdLine)
{
    // At this point, the sandbox has not yet fully taken effect, so files can still be accessed.
    if ($processType > 1) {
        //debugger_launch();
    };

    native_log("[dsl] on_before_command_line_processing: process_type={0}, pid={1}", $processType, pid());
};
