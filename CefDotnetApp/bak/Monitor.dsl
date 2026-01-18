script(on_init)
{
    nativelog("[dsl] on_init");
};

script(on_tick)
{
    $ct = nativeapi.CountProcess("cefclient.exe");
    if ($ct <= 0)
    {
        nativelog("[dsl] on_tick restart cefclient.exe");
        process("cefclient.exe", "--no-sandbox --no-proxy-server --url=https://evaluation.woa.com/chat");
    };
};