vs
{
    vs_attr("d:/UC/vs_in.csv", "d:/UC/vs_out.csv", 0);
    uniform("d:/UC/vs_cbuf8.csv", "uvec4"){
        add_range(0,7);
        add(29,30);
    };
    uniform("d:/UC/vs_cbuf9.csv", "uvec4"){
        add(11,12,16,141);
        add_range(113,116);
    };
    uniform("d:/UC/vs_cbuf10.csv", "uvec4"){
        add(0,2,3);
    };
    uniform("d:/UC/vs_cbuf13.csv", "uvec4"){
        add(6);
    };
    uniform("d:/UC/vs_cbuf15.csv", "uvec4"){
        add(1,54,55,57,60,61);
        add_range(22,28);
    };
};
ps
{
    ps_attr("d:/UC/vs_out.csv", 0){
        map_in_attr("out_attr0","in_attr0");
        map_in_attr("out_attr1","in_attr1");
        map_in_attr("out_attr2","in_attr2");
        map_in_attr("out_attr3","in_attr3");
        map_in_attr("out_attr4","in_attr4");
        map_in_attr("out_attr5","in_attr5");
        map_in_attr("out_attr6","in_attr6");
        map_in_attr("out_attr7","in_attr7");
        map_in_attr("out_attr8","in_attr8");
        map_in_attr("out_attr9","in_attr9");
        map_in_attr("out_attr10","in_attr10");
        map_in_attr("out_attr11","in_attr11");
        remove_in_attr("gl_Position");
    };
    uniform("d:/UC/fs_cbuf8.csv", "uvec4"){
        add(29,30);
    };
    uniform("d:/UC/fs_cbuf9.csv", "uvec4"){
        add(139,140,189,190);
    };
    uniform("d:/UC/fs_cbuf15.csv", "uvec4"){
        add(1,25,26,28,42,43,44,57);
    };
};
cs
{
};