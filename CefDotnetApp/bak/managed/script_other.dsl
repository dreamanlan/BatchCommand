// Note:The design philosophy behind these DSL scripts is to be stateless;
// all state resides within C# or JS. The DSL's global variables are utilized
// for configuring constants, and each hot-reload operation executes independently.
script(init_global_consts)
{
    setenv("PLAYWRIGHT_DRIVER_SEARCH_PATH", combinepath(basepath, "managed"));

    if (processtype == 1) {
        @UserName = getfilename(getdirectoryname(getdirectoryname(getenv("LOCALAPPDATA"))));
    }
    elif (ismac) {
        @UserName = getenv("USER");
    }
    else {
        @UserName = getenv("USERNAME");
    };
};
script(on_init)
{
    nativelog("[dsl] on_init finish, pid:{0}, type:{1}", pid(), processtype);
    fileecho(true);
    // no-sandbox = false
    // Always return false in the renderer.
    return(false);
};
script(on_finalize)
{
    nativelog("[dsl] on_finalize finish, pid:{0}, type:{1}", pid(), processtype);
};
script(on_before_command_line_processing)params($processType, $cmdLine)
{
    // At this point, the sandbox has not yet fully taken effect, so files can still be accessed.
    if ($processType > 1) {
        //debuggerlaunch();
    };

    nativelog("[dsl] on_before_command_line_processing: process_type={0}, pid={1}", $processType, pid());
};
