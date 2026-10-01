skill("echo_skill")
{
    tool(echo) {
        document("call_skill('echo_skill:echo', input); 回显输入内容");
        metadsl($input)
        {:
            // 原始块不处理转义，直接写{0}作为format占位符
            return(format('技能回显：{0}', $input));
        :};
    };
};
