script(on_init)
{
    nativelog("[dsl] on_init");
};

script(on_tick)
{
    $ct = nativeapi.CountProcess("webagent.exe");
    if ($ct <= 0)
    {
        nativelog("[dsl] on_tick restart webagent.exe");
        process("webagent.exe", " --url=https://evaluation.woa.com/chat --projectidentity=aiclaw --metadsl=script.dsl,script_renderer.dsl --remote-debugging-port=9228 --remote-allow-origins=*");
    };
};