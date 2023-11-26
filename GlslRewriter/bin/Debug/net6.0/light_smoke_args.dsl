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

        add_utof(1065353216u);

        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt", 9);
            split_on("texture", 3);
            split_on("textureLod", 3);
            split_on("texelFetch", 3);
            split_on("texelQueryLod", 3);
            split_on("texelSize", 3);
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
            b_2_19,
            b_5_2,
            b_6_5,
            f_0_10,
            f_0_104,
            f_0_113,
            f_0_125,
            f_0_128,
            f_0_130,
            f_0_15,
            f_0_16,
            f_1_11,
            f_1_2,
            f_1_25,
            f_1_26,
            f_1_40,
            f_1_44,
            f_1_60,
            f_1_89,
            f_1_90,
            f_2_103,
            f_2_109,
            f_2_17,
            f_2_43,
            f_3_36,
            f_4_23,
            f_4_34,
            f_5_43,
            f_5_55,
            f_5_64,
            f_7_30,
            f_8_11,
            f_8_12,
            f_8_36,
            f_8_5,
            f2_0_0,
            f4_0_1,
            pf_0_1,
            pf_0_17,
            pf_0_19,
            pf_0_23,
            pf_0_25,
            pf_1_10,
            pf_1_11,
            pf_1_7,
            pf_1_8,
            pf_1_9,
            pf_10_10,
            pf_10_9,
            pf_11_16,
            pf_12_5,
            pf_12_8,
            pf_13_6,
            pf_14_3,
            pf_14_5,
            pf_15_1,
            pf_16_15,
            pf_16_28,
            pf_16_30,
            pf_16_4,
            pf_18_0,
            pf_18_16,
            pf_18_19,
            pf_2_10,
            pf_2_12,
            pf_2_17,
            pf_2_2,
            pf_20_11,
            pf_20_13,
            pf_20_6,
            pf_21_15,
            pf_21_16,
            pf_21_21,
            pf_21_25,
            pf_21_4,
            pf_21_6,
            pf_25_10,
            pf_25_2,
            pf_28_3,
            pf_3_19,
            pf_3_23,
            pf_3_6,
            pf_4_12,
            pf_4_14,
            pf_4_8,
            pf_5_0,
            pf_5_10,
            pf_5_21,
            pf_6_20,
            pf_7_6,
            pf_7_7,
            pf_8_0,
            u_0_1,
            u_0_14,
            u_0_16,
            u_0_2,
            u_0_20,
            u_0_21,
            u_0_22,
            u_0_23,
            u_0_25,
            u_0_26,
            u_0_4,
            u_0_5,
            u_0_7,
            u_0_8,
            u_0_phi_11,
            u_0_phi_4,
            u_0_phi_56,
            u_0_phi_58,
            u_0_phi_71,
            u_1_13,
            u_1_18,
            u_1_20,
            u_1_21,
            u_1_23,
            u_1_24,
            u_1_26,
            u_1_27,
            u_1_29,
            u_1_3,
            u_1_30,
            u_1_4,
            u_1_6,
            u_1_8,
            u_1_phi_37,
            u_1_phi_60,
            u_1_phi_70,
            u_1_phi_72,
            u_1_phi_75,
            u_10_1,
            u_11_1,
            u_11_15,
            u_11_2,
            u_11_8,
            u_11_phi_30,
            u_12_18,
            u_12_21,
            u_12_24,
            u_12_26,
            u_12_31,
            u_12_34,
            u_12_8,
            u_13_1,
            u_13_15,
            u_13_7,
            u_13_8,
            u_13_9,
            u_13_phi_48,
            u_14_0,
            u_14_10,
            u_14_12,
            u_14_16,
            u_14_24,
            u_14_25,
            u_14_3,
            u_14_32,
            u_14_34,
            u_14_36,
            u_14_38,
            u_14_39,
            u_14_42,
            u_14_45,
            u_14_46,
            u_14_5,
            u_14_phi_26,
            u_14_phi_74,
            u_15_11,
            u_15_17,
            u_15_18,
            u_15_24,
            u_15_30,
            u_15_4,
            u_15_43,
            u_15_49,
            u_15_8,
            u_15_phi_17,
            u_16_17,
            u_16_18,
            u_16_3,
            u_16_39,
            u_16_42,
            u_16_43,
            u_16_51,
            u_16_57,
            u_16_60,
            u_16_63,
            u_16_66,
            u_16_67,
            u_16_7,
            u_16_75,
            u_16_8,
            u_16_phi_18,
            u_16_phi_25,
            u_17_10,
            u_17_13,
            u_17_15,
            u_17_30,
            u_17_31,
            u_17_32,
            u_17_phi_28,
            u_18_8,
            u_18_9,
            u_18_phi_20,
            u_19_17,
            u_19_23,
            u_19_24,
            u_19_phi_21,
            u_2_0,
            u_2_1,
            u_2_14,
            u_2_16,
            u_2_2,
            u_2_21,
            u_2_24,
            u_2_27,
            u_2_3,
            u_2_30,
            u_2_31,
            u_2_32,
            u_2_33,
            u_2_34,
            u_2_41,
            u_2_47,
            u_2_49,
            u_2_50,
            u_2_51,
            u_2_52,
            u_2_53,
            u_2_54,
            u_2_55,
            u_2_56,
            u_2_58,
            u_2_59,
            u_2_phi_2,
            u_2_phi_47,
            u_2_phi_59,
            u_2_phi_61,
            u_2_phi_69,
            u_2_phi_73,
            u_2_phi_9,
            u_20_12,
            u_20_14,
            u_20_15,
            u_20_16,
            u_20_2,
            u_20_22,
            u_20_phi_23,
            u_21_0,
            u_21_4,
            u_21_5,
            u_21_phi_22,
            u_22_27,
            u_22_28,
            u_22_37,
            u_22_38,
            u_22_8,
            u_22_phi_49,
            u_23_16,
            u_23_19,
            u_23_25,
            u_23_37,
            u_23_41,
            u_23_46,
            u_24_2,
            u_24_20,
            u_24_22,
            u_24_23,
            u_24_24,
            u_24_25,
            u_24_27,
            u_24_4,
            u_24_5,
            u_24_7,
            u_24_phi_29,
            u_24_phi_46,
            u_24_phi_53,
            u_25_15,
            u_26_14,
            u_26_18,
            u_26_2,
            u_26_3,
            u_26_phi_32,
            u_27_1,
            u_27_11,
            u_27_12,
            u_27_2,
            u_27_22,
            u_27_8,
            u_27_phi_33,
            u_28_1,
            u_28_11,
            u_28_12,
            u_28_15,
            u_28_16,
            u_28_21,
            u_28_3,
            u_28_4,
            u_28_phi_36,
            u_28_phi_50,
            u_29_0,
            u_29_10,
            u_29_11,
            u_29_12,
            u_29_17,
            u_29_19,
            u_29_2,
            u_29_20,
            u_29_28,
            u_29_6,
            u_29_phi_38,
            u_29_phi_51,
            u_3_18,
            u_3_19,
            u_3_21,
            u_3_23,
            u_3_25,
            u_3_30,
            u_3_31,
            u_3_35,
            u_3_36,
            u_3_phi_16,
            u_3_phi_63,
            u_3_phi_76,
            u_30_11,
            u_30_3,
            u_30_4,
            u_30_5,
            u_30_6,
            u_30_phi_35,
            u_30_phi_42,
            u_31_13,
            u_31_14,
            u_31_21,
            u_31_24,
            u_31_25,
            u_31_5,
            u_31_6,
            u_31_phi_39,
            u_31_phi_52,
            u_32_10,
            u_32_2,
            u_32_9,
            u_32_phi_52,
            u_33_2,
            u_33_3,
            u_33_8,
            u_33_9,
            u_33_phi_34,
            u_33_phi_52,
            u_34_10,
            u_34_11,
            u_34_12,
            u_34_phi_52,
            u_35_10,
            u_35_3,
            u_35_7,
            u_35_8,
            u_35_9,
            u_35_phi_40,
            u_35_phi_52,
            u_36_2,
            u_37_4,
            u_37_5,
            u_37_phi_41,
            u_38_1,
            u_38_2,
            u_38_3,
            u_38_4,
            u_38_phi_41,
            u_38_phi_55,
            u_39_0,
            u_39_1,
            u_39_2,
            u_39_3,
            u_39_phi_41,
            u_39_phi_57,
            u_4_12,
            u_4_13,
            u_4_15,
            u_4_17,
            u_4_19,
            u_4_20,
            u_4_24,
            u_4_5,
            u_4_6,
            u_4_phi_15,
            u_4_phi_31,
            u_40_0,
            u_40_1,
            u_40_phi_41,
            u_41_0,
            u_41_1,
            u_41_phi_43,
            u_42_0,
            u_42_1,
            u_42_phi_44,
            u_43_0,
            u_43_1,
            u_43_phi_45,
            u_5_14,
            u_5_15,
            u_5_20,
            u_5_21,
            u_5_22,
            u_5_25,
            u_5_27,
            u_5_33,
            u_6_13,
            u_6_15,
            u_6_2,
            u_6_21,
            u_6_7,
            u_7_14,
            u_7_15,
            u_7_17,
            u_7_18,
            u_7_2,
            u_7_20,
            u_7_21,
            u_7_4,
            u_7_5,
            u_7_8,
            u_7_phi_62,
            u_7_phi_65,
            u_7_phi_67,
            u_8_1,
            u_8_18,
            u_8_19,
            u_8_2,
            u_8_20,
            u_8_21,
            u_8_6,
            u_8_8,
            u_8_phi_54,
            u_8_phi_66,
            u_8_phi_68,
            u_9_16,
            u_9_17,
            u_9_18,
            u_9_19,
            u_9_phi_27,
            u_9_phi_64,
            u2_0_1,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/light_smoke/vs_in.csv", "d:/UC/light_smoke/vs_out.csv", 0);
        uniform("d:/UC/light_smoke/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/light_smoke/vs_cbuf8.csv", "uvec4"){
            add(10,11,28,29,30);
            add_range(0,7);
        };
        uniform("d:/UC/light_smoke/vs_cbuf9.csv", "uvec4"){
            //add(7,11,13,16,17,18,20,29,74,75,76,78,79,80,81,83,84,85,86,87,88,104,105,113,121,129,130,131,137,138,141,157);
            add(194,195,196);
            add_range(0, 160);
        };
        uniform("d:/UC/light_smoke/vs_cbuf10.csv", "uvec4"){
            add_range(0,6);
        };
        uniform("d:/UC/light_smoke/vs_cbuf13.csv", "uvec4"){
            add(0,1,2,3,5,6);
        };
        uniform("d:/UC/light_smoke/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        uniform("d:/UC/light_smoke/vs_cbuf16.csv", "uvec4"){
            add(0,1);
        };
        vao_attr("private Vector4[] in_attr3_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr3.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr8_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr8.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr11_array", "vec4", "d:/uc/VAO_light_smoke/vertex_in_attr11.csv", "[NonSerialized]"){
            add_range(0,34);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/light_smoke/1", "d:/uc/VAO_light_smoke/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/light_smoke/2", "d:/uc/VAO_light_smoke/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/light_smoke/3", "d:/uc/VAO_light_smoke/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/light_smoke/4", "d:/uc/VAO_light_smoke/4");
    };
    calculator
    {
        textureSize(tex0,*) = vec2(8,13);
        textureSize(tex1,*) = vec2(256,256);
        textureSize(tex2,*) = vec2(256,256);
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
            b_1_0,
            f_0_12,
            f4_0_1,
            f4_0_4,
            f4_0_5,
            f4_0_6,
            f4_0_7,
            f4_0_8,
            pf_0_16,
            pf_0_19,
            pf_0_23,
            pf_0_24,
            pf_0_26,
            pf_1_13,
            pf_1_14,
            pf_1_16,
            pf_2_10,
            pf_2_12,
            pf_2_9,
            pf_3_5,
            pf_4_3,
            pf_5_3,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/light_smoke/vs_out.csv", 0){
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
        uniform("d:/UC/light_smoke/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/light_smoke/fs_cbuf9.csv", "uvec4"){
            add(139,140);
        };
        uniform("d:/UC/light_smoke/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/light_smoke/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,39);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/light_smoke/1", "d:/uc/VAO_light_smoke/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/light_smoke/2", "d:/uc/VAO_light_smoke/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/light_smoke/3", "d:/uc/VAO_light_smoke/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/light_smoke/4", "d:/uc/VAO_light_smoke/4");
    };
    calculator
    {
        textureSize(tex3,*) = vec2(64,64);
        textureSize(tex4,*) = vec2(64,64);
        textureSize(tex5,*) = vec2(512,512);
        textureSize(depthTex,*) = vec2(800,450);
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
