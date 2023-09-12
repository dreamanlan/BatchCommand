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
            out_attr10.x,
            out_attr10.y,
            out_attr10.z,
            out_attr10.w,
            out_attr11.x,
            out_attr11.y,
            out_attr11.z,
            out_attr11.w,
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            b_0_1,
            b_0_3,
            b_0_5,
            b_0_6,
            b_0_7,
            b_0_8,
            b_1_10,
            b_1_11,
            b_1_12,
            b_1_13,
            b_1_14,
            b_1_8,
            b_1_9,
            f_0_0,
            f_0_1,
            f_0_10,
            f_0_11,
            f_0_2,
            f_0_3,
            f_0_4,
            f_0_5,
            f_1_11,
            f_1_13,
            f_1_19,
            f_1_2,
            f_1_21,
            f_1_25,
            f_1_54,
            f_1_55,
            f_1_57,
            f_1_6,
            f_1_60,
            f_1_66,
            f_1_69,
            f_1_70,
            f_1_73,
            f_1_74,
            f_11_3,
            f_11_4,
            f_15_12,
            f_15_20,
            f_15_6,
            f_16_6,
            f_16_8,
            f_17_1,
            f_17_10,
            f_18_1,
            f_2_1,
            f_2_16,
            f_2_18,
            f_2_2,
            f_2_25,
            f_2_3,
            f_2_32,
            f_2_42,
            f_2_44,
            f_2_45,
            f_2_50,
            f_2_58,
            f_2_63,
            f_3_31,
            f_3_32,
            f_3_36,
            f_3_37,
            f_3_42,
            f_3_44,
            f_3_49,
            f_3_56,
            f_3_61,
            f_4_28,
            f_4_43,
            f_4_45,
            f_4_51,
            f_4_54,
            f_4_57,
            f_4_70,
            f_4_74,
            f_4_8,
            f_5_13,
            f_5_18,
            f_5_19,
            f_5_27,
            f_5_28,
            f_6_10,
            f_6_6,
            f_6_7,
            f_6_8,
            f_7_6,
            f_8_1,
            f_9_2,
            f_9_3,
            f2_0_1,
            f2_0_3,
            f4_0_0,
            f4_0_1,
            f4_0_3,
            pf_0_0,
            pf_0_1,
            pf_0_10,
            pf_0_11,
            pf_0_16,
            pf_0_17,
            pf_0_18,
            pf_0_24,
            pf_0_25,
            pf_0_28,
            pf_0_29,
            pf_0_3,
            pf_0_32,
            pf_0_33,
            pf_0_4,
            pf_0_5,
            pf_0,
            pf_1_0,
            pf_1_10,
            pf_1_11,
            pf_1_13,
            pf_1_14,
            pf_1_15,
            pf_1_17,
            pf_1_18,
            pf_1_19,
            pf_1_20,
            pf_1_21,
            pf_1_23,
            pf_1_25,
            pf_1_3,
            pf_1_4,
            pf_1_5,
            pf_1_6,
            pf_1,
            pf_10_1,
            pf_10_10,
            pf_10_11,
            pf_10_17,
            pf_10_18,
            pf_10_4,
            pf_10_7,
            pf_11_10,
            pf_11_15,
            pf_11_16,
            pf_11_3,
            pf_11_4,
            pf_11_5,
            pf_11_8,
            pf_11_9,
            pf_12_2,
            pf_12_3,
            pf_12_6,
            pf_13_1,
            pf_13_12,
            pf_13_14,
            pf_13_3,
            pf_13_5,
            pf_13_6,
            pf_13_7,
            pf_14_10,
            pf_14_2,
            pf_14_6,
            pf_14_7,
            pf_15_12,
            pf_15_14,
            pf_15_15,
            pf_15_18,
            pf_15_19,
            pf_15_2,
            pf_15_22,
            pf_15_8,
            pf_16_14,
            pf_16_15,
            pf_16_2,
            pf_16_5,
            pf_16_8,
            pf_17_10,
            pf_17_11,
            pf_17_2,
            pf_17_3,
            pf_17_5,
            pf_17_7,
            pf_18_0,
            pf_18_6,
            pf_18_7,
            pf_18_9,
            pf_19_12,
            pf_19_3,
            pf_19_7,
            pf_19_8,
            pf_2_0,
            pf_2_1,
            pf_2_10,
            pf_2_11,
            pf_2_2,
            pf_2_3,
            pf_2_5,
            pf_2_7,
            pf_20_0,
            pf_20_10,
            pf_20_12,
            pf_20_13,
            pf_20_14,
            pf_20_15,
            pf_20_16,
            pf_20_18,
            pf_20_19,
            pf_20_4,
            pf_20_7,
            pf_21_4,
            pf_21_9,
            pf_23_0,
            pf_23_1,
            pf_23_6,
            pf_24_0,
            pf_24_1,
            pf_25_1,
            pf_26_0,
            pf_26_1,
            pf_3_11,
            pf_3_7,
            pf_4_0,
            pf_4_1,
            pf_4_12,
            pf_4_13,
            pf_4_14,
            pf_4_2,
            pf_4_20,
            pf_4_3,
            pf_4_5,
            pf_4_6,
            pf_4,
            pf_5_3,
            pf_5_5,
            pf_5_6,
            pf_6_13,
            pf_6_17,
            pf_6_19,
            pf_6_4,
            pf_6_8,
            pf_6_9,
            pf_7_12,
            pf_7_13,
            pf_7_15,
            pf_7_17,
            pf_7_18,
            pf_7_19,
            pf_7_20,
            pf_7_21,
            pf_7_22,
            pf_7_23,
            pf_7_26,
            pf_7_27,
            pf_7_5,
            pf_7_7,
            pf_7_9,
            pf_8_0,
            pf_8_3,
            pf_8_4,
            pf_8_7,
            pf_8_8,
            pf_9_11,
            pf_9_15,
            pf_9_16,
            pf_9_17,
            pf_9_18,
            pf_9_19,
            pf_9_2,
            pf_9_20,
            pf_9_21,
            pf_9_3,
            pf_9_4,
            pf_9_5,
            pf_9_7,
            pf_9_9,
            u_0_1,
            u_0_10,
            u_0_12,
            u_0_2,
            u_0_4,
            u_0_5,
            u_0_6,
            u_0_7,
            u_0_8,
            u_0_9,
            u_0_phi_11,
            u_0_phi_15,
            u_0_phi_4,
            u_1_3,
            u_1_4,
            u_1_phi_16,
            u_2_0,
            u_2_1,
            u_2_2,
            u_2_3,
            u_2_4,
            u_2_5,
            u_2_phi_2,
            u_3_0,
            u_3_1,
            u_3_2,
            u_3_3,
            u_3_4,
            u_3_5,
            u_3_phi_15,
            u_3_phi_9,
            u_3,
            u_4_0,
            u_4_1,
            u_4_2,
            u_4_3,
            u_4,
            u_5_0,
            u_5,
            u2_0_0,
            u2_0_1,
        };
    };
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