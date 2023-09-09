vs
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_use_multiline_comments;
        //def_variable_expanded_only_once;
        def_max_level_for_expression = -1;
        def_max_length_for_expression = 512;
        //def_skip_all_comments;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;

        set_object_comment(out_attr0_0.x, 64, 20480, true, true);
        set_object_comment(out_attr0_0.y, 64, 20480);
        set_object_comment(out_attr0_0.z, 64, 20480);
        set_object_comment(out_attr0_0.w, 64, 20480);
        set_object_comment(out_attr1_0.x, 64, 20480);
        set_object_comment(out_attr1_0.y, 64, 20480);
        set_object_comment(out_attr1_0.z, 64, 20480);
        set_object_comment(out_attr1_0.w, 64, 20480);
        set_object_comment(out_attr2_0.x, 64, 20480);
        set_object_comment(out_attr2_0.y, 64, 20480);
        set_object_comment(out_attr2_0.z, 64, 20480);
        set_object_comment(out_attr2_0.w, 64, 20480);
        set_object_comment(out_attr3_0.x, 64, 20480);
        set_object_comment(out_attr3_0.y, 64, 20480);
        set_object_comment(out_attr3_0.z, 64, 20480);
        set_object_comment(out_attr3_0.w, 64, 20480);
        set_object_comment(out_attr4_0.x, 64, 20480);
        set_object_comment(out_attr4_0.y, 64, 20480);
        set_object_comment(out_attr4_0.z, 64, 20480);
        set_object_comment(out_attr4_0.w, 64, 20480);
        set_object_comment(out_attr5_0.x, 64, 20480);
        set_object_comment(out_attr5_0.y, 64, 20480);
        set_object_comment(out_attr5_0.z, 64, 20480);
        set_object_comment(out_attr5_0.w, 64, 20480);
        set_object_comment(out_attr6_0.x, 64, 20480);
        set_object_comment(out_attr6_0.y, 64, 20480);
        set_object_comment(out_attr6_0.z, 64, 20480);
        set_object_comment(out_attr6_0.w, 64, 20480);
        set_object_comment(out_attr7_0.x, 64, 20480);
        set_object_comment(out_attr7_0.y, 64, 20480);
        set_object_comment(out_attr7_0.z, 64, 20480);
        set_object_comment(out_attr7_0.w, 64, 20480);
        set_object_comment(out_attr8_0.x, 64, 20480);
        set_object_comment(out_attr8_0.y, 64, 20480);
        set_object_comment(out_attr8_0.z, 64, 20480);
        set_object_comment(out_attr8_0.w, 64, 20480);
        set_object_comment(out_attr9_0.x, 64, 20480);
        set_object_comment(out_attr9_0.y, 64, 20480);
        set_object_comment(out_attr9_0.z, 64, 20480);
        set_object_comment(out_attr9_0.w, 64, 20480);
        set_object_comment(out_attr10_0.x, 64, 20480);
        set_object_comment(out_attr10_0.y, 64, 20480);
        set_object_comment(out_attr10_0.z, 64, 20480);
        set_object_comment(out_attr10_0.w, 64, 20480);
        set_object_comment(out_attr11_0.x, 64, 20480);
        set_object_comment(out_attr11_0.y, 64, 20480);
        set_object_comment(out_attr11_0.z, 64, 20480);
        set_object_comment(out_attr11_0.w, 64, 20480);

        set_variable_comment(f4_0_0, 64, 20480);
        set_variable_comment(f4_0_1, 64, 20480);

        dont_expand_variable{
            f_2_31,
            f4_0_0,
            f4_0_1,
            pf_4_1,
            pf_4_2,
            pf_4_3,
            pf_4_4,
            pf_4_5,
            pf_4_6,
            pf_7_3,
            pf_8_1,
            pf_8_3,
            pf_9_2,
            pf_9_3,
            pf_10_1,
            pf_10_2,
            pf_14_3,
            pf_19_5,
            pf_21_0,
            pf_21_1,
            pf_22_0,
            pf_23_0,
            u_0_9,
            u_0_10,
            u_1_3,
            u_1_4,
            u_1_5,
            u_2_t,
        };

        invalid_variable{
            in_attr0,
            in_attr1,
            in_attr4,
            in_attr5,
            in_attr6,
            in_attr7,
            in_attr9,
            in_attr10,
            in_attr11
        };
    };
    //vs_attr("d:/UC/vs_in.csv", "d:/UC/vs_out.csv", 0);
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
    calculator
    {
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,1.0);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,1.0);
    };
};
ps
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_use_multiline_comments;
        //def_variable_expanded_only_once;
        def_max_level_for_expression = 32;
        def_max_length_for_expression = 512;
        //def_skip_all_comments;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;

        set_object_comment(frag_color0.x, 64, 2048, true, true);
        set_object_comment(frag_color0.y, 64, 2048);
        set_object_comment(frag_color0.z, 64, 2048);
        set_object_comment(frag_color0.w, 64, 2048);

        set_variable_comment(f_2_16, 64, 2048);
        set_variable_comment(f4_0_0, 64, 2048);
        set_variable_comment(f4_0_1, 64, 2048);
        set_variable_comment(f4_0_2, 64, 2048);
        set_variable_comment(f4_0_3, 64, 2048);
        set_variable_comment(f4_0_4, 64, 2048);
        set_variable_comment(f_5_3, 64, 2048);
        set_variable_comment(pf_3_7, 64, 2048);
        set_variable_comment(pf_1_23, 64, 2048);
        set_variable_comment(pf_0_20, 64, 2048);
        set_variable_comment(pf_2_8, 64, 2048);

        dont_expand_variable{
            f_0_12,
            f_2_16,
            f_2_11,
            f_2_16,
            f_3_11,
            f_3_22,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f_4_8,
            f_5_2,
            f_5_3,
            f_5_4,
            pf_0_15,
            pf_0_19,
            pf_0_20,
            pf_0_23,
            pf_0_24,
            pf_0_25,
            pf_1_23,
            pf_1_26,
            pf_1_27,
            pf_1_28,
            pf_2_11,
            pf_2_12,
            pf_2_13,
            pf_2_5,
            pf_2_7,
            pf_2_8,
            pf_3_2,
            pf_3_3,
            pf_3_5,
            pf_3_7,
            pf_4_4,
            pf_5_2,
            pf_6_1,
        };
    };
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
    calculator
    {
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,1.0);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,1.0);
        texture(*,*) = vec4(0.5,0.5,0.5,1.0);
        textureQueryLod(*,*) = vec2(4,1);
    };
};
cs
{
    setting
    {
        debug_mode;
        //print_graph;
        //use_multiline_comments;
        //variable_expanded_only_once;
        def_max_level_for_value = 64;
        def_max_level_for_expression = -1;
        //def_skip_all_comments;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        max_length_for_value = 16 * 1024;
        max_length_for_expression = 64 * 1024;
    };
};