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
        def_max_length = 1024;
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
            split_on("texture", 3);
            split_on("textureLod", 3);
            split_on("texelFetch", 3);
            split_on("texelQueryLod", 3);
            split_on("texelSize", 3);
            split_on("log2", 12);
        };

        variable_assignment
        {
            u_2_phi_2 = 0u;
            u_0_phi_4 = 0u;
            u_3_phi_9 = 1144610816u;
            u_0_phi_11 = 1144610816u;
            u_0_phi_15 = 1147158528u;
            u_3_phi_15 = 1148846080u;
            u_1_phi_16 = 1063625163u;
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
            out_attr12.x,
            out_attr12.y,
            out_attr12.z,
            out_attr12.w,
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            b_0_2,
            b_0_5,
            b_1_10,
            b_1_11,
            b_1_12,
            b_1_13,
            b_1_14,
            b_1_8,
            b_1_9,
            f_0_12,
            f_0_16,
            f_0_17,
            f_0_19,
            f_0_28,
            f_0_32,
            f_0_8,
            f_1_24,
            f_1_27,
            f_1_33,
            f_11_3,
            f_15_5,
            f_16_12,
            f_16_14,
            f_16_6,
            f_16_8,
            f_17_1,
            f_17_8,
            f_18_5,
            f_19_8,
            f_2_35,
            f_2_48,
            f_2_57,
            f_2_59,
            f_2_68,
            f_2_70,
            f_2_75,
            f_2_80,
            f_20_3,
            f_3_100,
            f_3_33,
            f_3_52,
            f_3_58,
            f_3_60,
            f_3_8,
            f_4_16,
            f_4_20,
            f_4_21,
            f_4_23,
            f_4_24,
            f_4_38,
            f_4_42,
            f_5_4,
            f_5_6,
            f_8_1,
            f_9_1,
            f_9_2,
            f2_0_1,
            f2_0_3,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f4_0_5,
            pf_0_1,
            pf_0_12,
            pf_0_28,
            pf_0_29,
            pf_0_3,
            pf_0_30,
            pf_0_31,
            pf_1_10,
            pf_1_11,
            pf_1_14,
            pf_1_18,
            pf_1_5,
            pf_1_6,
            pf_10_12,
            pf_10_13,
            pf_10_3,
            pf_10_6,
            pf_10_8,
            pf_10_9,
            pf_11_10,
            pf_11_12,
            pf_11_13,
            pf_11_3,
            pf_11_6,
            pf_12_10,
            pf_12_13,
            pf_12_2,
            pf_12_5,
            pf_12_6,
            pf_12_7,
            pf_12_8,
            pf_13_10,
            pf_13_12,
            pf_13_13,
            pf_13_2,
            pf_13_3,
            pf_13_4,
            pf_13_6,
            pf_13_7,
            pf_13_8,
            pf_14_11,
            pf_14_14,
            pf_14_17,
            pf_14_18,
            pf_14_2,
            pf_14_3,
            pf_14_4,
            pf_14_7,
            pf_14_8,
            pf_15_10,
            pf_15_12,
            pf_15_13,
            pf_15_19,
            pf_15_2,
            pf_15_3,
            pf_15_4,
            pf_15_6,
            pf_15_8,
            pf_16_3,
            pf_17_12,
            pf_17_14,
            pf_17_17,
            pf_17_18,
            pf_17_19,
            pf_17_2,
            pf_17_3,
            pf_17_5,
            pf_17_6,
            pf_17_7,
            pf_18_4,
            pf_18_6,
            pf_18_7,
            pf_19_12,
            pf_19_2,
            pf_19_3,
            pf_19_4,
            pf_19_5,
            pf_19_7,
            pf_2_0,
            pf_2_17,
            pf_2_4,
            pf_2_5,
            pf_2_9,
            pf_20_4,
            pf_20_5,
            pf_21_1,
            pf_21_3,
            pf_21_5,
            pf_21_7,
            pf_22_0,
            pf_23_1,
            pf_24_0,
            pf_24_1,
            pf_24_11,
            pf_24_2,
            pf_24_7,
            pf_24_9,
            pf_25_0,
            pf_25_1,
            pf_25_2,
            pf_25_3,
            pf_25_4,
            pf_25_5,
            pf_25_7,
            pf_25_8,
            pf_26_2,
            pf_27_0,
            pf_27_1,
            pf_3_15,
            pf_3_16,
            pf_3_17,
            pf_4_11,
            pf_4_14,
            pf_4_16,
            pf_4_18,
            pf_4_2,
            pf_4_24,
            pf_4_25,
            pf_4_27,
            pf_4_4,
            pf_4_6,
            pf_4_9,
            pf_5_3,
            pf_5_4,
            pf_5_5,
            pf_5_6,
            pf_6_3,
            pf_6_7,
            pf_7_0,
            pf_7_11,
            pf_7_13,
            pf_7_14,
            pf_7_16,
            pf_7_28,
            pf_7_4,
            pf_7_5,
            pf_7_6,
            pf_7_7,
            pf_7_8,
            pf_8_11,
            pf_8_2,
            pf_8_3,
            pf_8_7,
            pf_8_8,
            pf_9_10,
            pf_9_11,
            pf_9_17,
            pf_9_2,
            pf_9_5,
            pf_9_6,
            pf_9_8,
            u_0_1,
            u_0_11,
            u_0_2,
            u_0_4,
            u_0_5,
            u_0_6,
            u_0_7,
            u_0_phi_11,
            u_0_phi_15,
            u_0_phi_4,
            u_1_1,
            u_1_3,
            u_1_4,
            u_1_phi_16,
            u_2_0,
            u_2_1,
            u_2_5,
            u_2_phi_2,
            u_3_2,
            u_3_3,
            u_3_4,
            u_3_5,
            u_3_7,
            u_3_phi_15,
            u_3_phi_9,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/cloud_2/vs_in.csv", "d:/UC/cloud_2/vs_out.csv", 0);
        uniform("d:/UC/cloud_2/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(29,30);
        };
        uniform("d:/UC/cloud_2/vs_cbuf9.csv", "uvec4"){
            add(11,12,16,141);
            add_range(113,116);
        };
        uniform("d:/UC/cloud_2/vs_cbuf10.csv", "uvec4"){
            add(0,2,3);
        };
        uniform("d:/UC/cloud_2/vs_cbuf13.csv", "uvec4"){
            add(6);
        };
        uniform("d:/UC/cloud_2/vs_cbuf15.csv", "uvec4"){
            add(1,54,55,57,60,61);
            add_range(22,28);
        };
        vao_attr("var in_attr4_array", "vec4", "d:/uc/VAO_2/vertex_in_attr4.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr5_array", "vec4", "d:/uc/VAO_2/vertex_in_attr5.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr6_array", "vec4", "d:/uc/VAO_2/vertex_in_attr6.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr7_array", "vec4", "d:/uc/VAO_2/vertex_in_attr7.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr9_array", "vec4", "d:/uc/VAO_2/vertex_in_attr9.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr10_array", "vec4", "d:/uc/VAO_2/vertex_in_attr10.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr11_array", "vec4", "d:/uc/VAO_2/vertex_in_attr11.csv"){
            add_range(0,35);
        };
    };
    shader_arg(1)
    {
        vs_attr("d:/UC/cloud_2/1/c2_1_23_in.csv", "d:/UC/cloud_2/1/c2_1_23_out.csv", 0);
        uniform("d:/UC/cloud_2/1/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(29,30);
        };
        uniform("d:/UC/cloud_2/1/vs_cbuf9.csv", "uvec4"){
            add(11,12,16,141);
            add_range(113,116);
        };
        uniform("d:/UC/cloud_2/1/vs_cbuf10.csv", "uvec4"){
            add(0,2,3);
        };
        uniform("d:/UC/cloud_2/1/vs_cbuf13.csv", "uvec4"){
            add(6);
        };
        uniform("d:/UC/cloud_2/1/vs_cbuf15.csv", "uvec4"){
            add(1,54,55,57,60,61);
            add_range(22,28);
        };
        vao_attr("var in_attr4_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr4.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr5_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr5.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr6_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr6.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr7_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr7.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr9_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr9.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr10_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr10.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr11_array", "vec4", "d:/uc/VAO_2/1/vertex_in_attr11.csv"){
            add_range(0,35);
        };
    };
    shader_arg(2)
    {
        vs_attr("d:/UC/cloud_2/2/c2_2_20_in.csv", "d:/UC/cloud_2/2/c2_2_20_out.csv", 0);
        uniform("d:/UC/cloud_2/2/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(29,30);
        };
        uniform("d:/UC/cloud_2/2/vs_cbuf9.csv", "uvec4"){
            add(11,12,16,141);
            add_range(113,116);
        };
        uniform("d:/UC/cloud_2/2/vs_cbuf10.csv", "uvec4"){
            add(0,2,3);
        };
        uniform("d:/UC/cloud_2/2/vs_cbuf13.csv", "uvec4"){
            add(6);
        };
        uniform("d:/UC/cloud_2/2/vs_cbuf15.csv", "uvec4"){
            add(1,54,55,57,60,61);
            add_range(22,28);
        };
        vao_attr("var in_attr4_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr4.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr5_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr5.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr6_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr6.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr7_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr7.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr9_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr9.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr10_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr10.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr11_array", "vec4", "d:/uc/VAO_2/2/vertex_in_attr11.csv"){
            add_range(0,35);
        };
    };
    shader_arg(3)
    {
        vs_attr("d:/UC/cloud_2/3/c2_3_20_in.csv", "d:/UC/cloud_2/3/c2_3_20_out.csv", 0);
        uniform("d:/UC/cloud_2/3/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(29,30);
        };
        uniform("d:/UC/cloud_2/3/vs_cbuf9.csv", "uvec4"){
            add(11,12,16,141);
            add_range(113,116);
        };
        uniform("d:/UC/cloud_2/3/vs_cbuf10.csv", "uvec4"){
            add(0,2,3);
        };
        uniform("d:/UC/cloud_2/3/vs_cbuf13.csv", "uvec4"){
            add(6);
        };
        uniform("d:/UC/cloud_2/3/vs_cbuf15.csv", "uvec4"){
            add(1,54,55,57,60,61);
            add_range(22,28);
        };
        vao_attr("var in_attr4_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr4.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr5_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr5.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr6_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr6.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr7_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr7.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr9_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr9.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr10_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr10.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr11_array", "vec4", "d:/uc/VAO_2/3/vertex_in_attr11.csv"){
            add_range(0,35);
        };
    };
    shader_arg(4)
    {
        vs_attr("d:/UC/cloud_2/4/c2_4_17_in.csv", "d:/UC/cloud_2/4/c2_4_17_out.csv", 0);
        uniform("d:/UC/cloud_2/4/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(29,30);
        };
        uniform("d:/UC/cloud_2/4/vs_cbuf9.csv", "uvec4"){
            add(11,12,16,141);
            add_range(113,116);
        };
        uniform("d:/UC/cloud_2/4/vs_cbuf10.csv", "uvec4"){
            add(0,2,3);
        };
        uniform("d:/UC/cloud_2/4/vs_cbuf13.csv", "uvec4"){
            add(6);
        };
        uniform("d:/UC/cloud_2/4/vs_cbuf15.csv", "uvec4"){
            add(1,54,55,57,60,61);
            add_range(22,28);
        };
        vao_attr("var in_attr4_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr4.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr5_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr5.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr6_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr6.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr7_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr7.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr9_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr9.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr10_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr10.csv"){
            add_range(0,35);
        };
        vao_attr("var in_attr11_array", "vec4", "d:/uc/VAO_2/4/vertex_in_attr11.csv"){
            add_range(0,35);
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
        def_max_length = 1024;
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
            split_on("texture", 3);
            split_on("textureLod", 3);
            split_on("texelFetch", 3);
            split_on("texelQueryLod", 3);
            split_on("texelSize", 3);
            split_on("log2", 12);
        };

        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            b_0_0,
            b_1_0,
            f_0_10,
            f_0_18,
            f_1_18,
            f_2_7,
            f_3_2,
            f_5_3,
            f_6_6,
            f_6_7,
            f_7_5,
            f_8_2,
            f_8_3,
            f_8_4,
            f2_0_2,
            f2_0_3,
            f3_0_0,
            f4_0_1,
            f4_0_3,
            f4_0_4,
            pf_0_11,
            pf_0_13,
            pf_0_14,
            pf_0_16,
            pf_0_20,
            pf_0_9,
            pf_1_11,
            pf_1_16,
            pf_1_19,
            pf_1_21,
            pf_1_23,
            pf_1_27,
            pf_1_5,
            pf_1_6,
            pf_1_8,
            pf_2_10,
            pf_2_14,
            pf_2_2,
            pf_2_3,
            pf_2_6,
            pf_2_8,
            pf_3_0,
            pf_3_10,
            pf_3_2,
            pf_3_5,
            pf_4_0,
            pf_4_11,
            pf_4_3,
            pf_4_4,
            pf_4_6,
            pf_5_2,
            pf_5_3,
            pf_5_5,
            pf_5_9,
            pf_6_0,
            pf_6_1,
            u_0_0,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/cloud_2/vs_out.csv", 0){
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
            map_in_attr("out_attr12","in_attr12");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/cloud_2/fs_cbuf8.csv", "uvec4"){
            add(29,30);
        };
        uniform("d:/UC/cloud_2/fs_cbuf9.csv", "uvec4"){
            add(139,140,189,190);
        };
        uniform("d:/UC/cloud_2/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,28,42,43,44,57);
        };
    };
    shader_arg(1)
    {
        ps_attr("d:/UC/cloud_2/1/c2_1_23_out.csv", 0){
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
            map_in_attr("out_attr12","in_attr12");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/cloud_2/1/fs_cbuf8.csv", "uvec4"){
            add(29,30);
        };
        uniform("d:/UC/cloud_2/1/fs_cbuf9.csv", "uvec4"){
            add(139,140,189,190);
        };
        uniform("d:/UC/cloud_2/1/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,28,42,43,44,57);
        };
    };
    shader_arg(2)
    {
        ps_attr("d:/UC/cloud_2/2/c2_2_20_out.csv", 0){
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
            map_in_attr("out_attr12","in_attr12");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/cloud_2/2/fs_cbuf8.csv", "uvec4"){
            add(29,30);
        };
        uniform("d:/UC/cloud_2/2/fs_cbuf9.csv", "uvec4"){
            add(139,140,189,190);
        };
        uniform("d:/UC/cloud_2/2/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,28,42,43,44,57);
        };
    };
    shader_arg(3)
    {
        ps_attr("d:/UC/cloud_2/3/c2_3_20_out.csv", 0){
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
            map_in_attr("out_attr12","in_attr12");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/cloud_2/3/fs_cbuf8.csv", "uvec4"){
            add(29,30);
        };
        uniform("d:/UC/cloud_2/3/fs_cbuf9.csv", "uvec4"){
            add(139,140,189,190);
        };
        uniform("d:/UC/cloud_2/3/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,28,42,43,44,57);
        };
    };
    shader_arg(4)
    {
        ps_attr("d:/UC/cloud_2/4/c2_4_17_out.csv", 0){
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
            map_in_attr("out_attr12","in_attr12");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/cloud_2/4/fs_cbuf8.csv", "uvec4"){
            add(29,30);
        };
        uniform("d:/UC/cloud_2/4/fs_cbuf9.csv", "uvec4"){
            add(139,140,189,190);
        };
        uniform("d:/UC/cloud_2/4/fs_cbuf15.csv", "uvec4"){
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