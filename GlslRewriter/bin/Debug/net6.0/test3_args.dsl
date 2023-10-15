vs
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 512;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;

        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt");
            split_on("log2", 12);
        };

        split_object_assignment{
            set(out_attr0.x, 64, 20480, true, true);
            out_attr0.y,
            out_attr0.z,
            out_attr0.w,
            out_attr1.x,
            out_attr1.y,
            out_attr1.z,
            out_attr1.w,
            out_attr2.x,
            out_attr2.y,
            out_attr2.z,
            out_attr2.w,
            out_attr3.x,
            out_attr3.y,
            out_attr3.z,
            out_attr3.w,
            out_attr4.x,
            out_attr4.y,
            out_attr4.z,
            out_attr4.w,
            out_attr5.x,
            out_attr5.y,
            out_attr5.z,
            out_attr5.w,
            out_attr6.x,
            out_attr6.y,
            out_attr6.z,
            out_attr6.w,
            out_attr7.x,
            out_attr7.y,
            out_attr7.z,
            out_attr7.w,
            out_attr8.x,
            out_attr8.y,
            out_attr8.z,
            out_attr8.w,
            out_attr9.x,
            out_attr9.y,
            out_attr9.z,
            out_attr9.w,
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
        };
    };
    shader_arg
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
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 512;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;

        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
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
    shader_arg
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
    calculator
    {
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        texture(*,*) = vec4(0.5,0.5,0.5,0.75);
        textureQueryLod(*,*) = vec2(4,1);
    };
};
cs
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 512;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;
    };
};