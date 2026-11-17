common
{
    setting
    {
        for_hlsl_shader;
        debug_mode;
        //print_graph;
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 4096;
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
    };
};
vs
{
    setting
    {
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
            b_0_13,
            b_0_15,
            b_1_53,
            b_1_55,
            b_1_57,
            b_1_67,
            b_2_23,
            b_2_25,
            b_3_4,
            b_3_6,
            b_4_9,
            b_5_5,
            b_5_6,
            f_0_13,
            f_0_15,
            f_0_18,
            f_0_19,
            f_0_21,
            f_0_22,
            f_0_6,
            f_1_17,
            f_1_20,
            f_1_83,
            f_10_8,
            f_2_102,
            f_2_103,
            f_2_134,
            f_2_142,
            f_2_24,
            f_2_67,
            f_2_70,
            f_2_72,
            f_2_79,
            f_2_85,
            f_3_12,
            f_3_15,
            f_3_19,
            f_3_27,
            f_3_30,
            f_3_34,
            f_3_36,
            f_3_56,
            f_3_61,
            f_4_16,
            f_4_22,
            f_4_27,
            f_4_35,
            f_4_36,
            f_4_40,
            f_4_56,
            f_4_61,
            f_4_85,
            f_4_88,
            f_5_14,
            f_5_2,
            f_5_27,
            f_5_3,
            f_6_20,
            f_6_6,
            f_7_13,
            f_7_16,
            f_7_42,
            f_8_10,
            f_8_11,
            f_8_13,
            f_9_49,
            f_9_64,
            f_9_65,
            f4_0_0,
            f4_0_1,
            pf_0_1,
            pf_0_11,
            pf_0_13,
            pf_0_14,
            pf_0_16,
            pf_0_21,
            pf_1_1,
            pf_1_13,
            pf_1_16,
            pf_1_24,
            pf_1_26,
            pf_1_27,
            pf_1_5,
            pf_1_9,
            pf_11_11,
            pf_11_15,
            pf_11_21,
            pf_11_23,
            pf_11_27,
            pf_11_28,
            pf_11_8,
            pf_12_7,
            pf_13_9,
            pf_14_4,
            pf_16_1,
            pf_16_15,
            pf_16_20,
            pf_16_21,
            pf_16_22,
            pf_16_30,
            pf_16_7,
            pf_17_15,
            pf_17_16,
            pf_17_6,
            pf_17_9,
            pf_19_6,
            pf_2_16,
            pf_2_5,
            pf_20_14,
            pf_20_17,
            pf_20_18,
            pf_20_7,
            pf_21_19,
            pf_21_5,
            pf_22_1,
            pf_22_10,
            pf_22_8,
            pf_23_4,
            pf_24_4,
            pf_27_2,
            pf_27_6,
            pf_28_1,
            pf_28_2,
            pf_28_3,
            pf_28_4,
            pf_3_10,
            pf_3_12,
            pf_3_4,
            pf_4_11,
            pf_4_8,
            pf_5_15,
            pf_5_25,
            pf_5_26,
            pf_5_31,
            pf_5_32,
            pf_5_38,
            pf_5_5,
            pf_6_11,
            pf_6_5,
            pf_7_1,
            pf_7_15,
            pf_7_3,
            pf_7_4,
            pf_8_3,
            u_0_1,
            u_0_3,
            u_0_8,
            u_0_9,
            u_0_phi_88,
            u_1_13,
            u_1_14,
            u_1_18,
            u_1_19,
            u_1_2,
            u_1_20,
            u_1_21,
            u_1_23,
            u_1_24,
            u_1_25,
            u_1_26,
            u_1_28,
            u_1_29,
            u_1_30,
            u_1_31,
            u_1_32,
            u_1_35,
            u_1_36,
            u_1_39,
            u_1_7,
            u_1_phi_66,
            u_1_phi_75,
            u_1_phi_77,
            u_1_phi_79,
            u_1_phi_84,
            u_1_phi_87,
            u_10_0,
            u_10_1,
            u_10_16,
            u_10_2,
            u_10_21,
            u_10_22,
            u_10_29,
            u_10_3,
            u_10_30,
            u_10_31,
            u_10_37,
            u_10_38,
            u_10_phi_15,
            u_10_phi_17,
            u_10_phi_70,
            u_10_phi_90,
            u_11_1,
            u_11_13,
            u_11_18,
            u_11_2,
            u_11_4,
            u_11_5,
            u_11_phi_16,
            u_11_phi_20,
            u_12_13,
            u_12_23,
            u_12_24,
            u_12_25,
            u_12_26,
            u_12_27,
            u_12_28,
            u_12_3,
            u_12_33,
            u_12_34,
            u_12_4,
            u_12_phi_20,
            u_12_phi_29,
            u_12_phi_32,
            u_12_phi_37,
            u_12_phi_91,
            u_13_10,
            u_13_11,
            u_13_9,
            u_13_phi_30,
            u_14_10,
            u_14_11,
            u_14_13,
            u_14_5,
            u_14_6,
            u_14_9,
            u_14_phi_26,
            u_15_10,
            u_15_11,
            u_15_14,
            u_15_17,
            u_15_22,
            u_15_23,
            u_15_6,
            u_15_7,
            u_15_9,
            u_15_phi_31,
            u_15_phi_40,
            u_15_phi_89,
            u_16_2,
            u_16_3,
            u_16_phi_27,
            u_17_1,
            u_17_2,
            u_17_phi_34,
            u_18_10,
            u_18_11,
            u_18_15,
            u_18_4,
            u_18_9,
            u_19_0,
            u_19_1,
            u_19_3,
            u_19_6,
            u_19_phi_33,
            u_2_10,
            u_2_11,
            u_2_14,
            u_2_15,
            u_2_2,
            u_2_20,
            u_2_21,
            u_2_7,
            u_2_phi_64,
            u_2_phi_81,
            u_20_11,
            u_20_2,
            u_20_6,
            u_20_7,
            u_21_0,
            u_21_1,
            u_21_12,
            u_21_14,
            u_21_15,
            u_21_18,
            u_21_2,
            u_21_3,
            u_21_4,
            u_21_5,
            u_21_6,
            u_21_phi_36,
            u_21_phi_41,
            u_21_phi_48,
            u_22_1,
            u_22_2,
            u_22_phi_38,
            u_23_1,
            u_23_2,
            u_23_3,
            u_23_4,
            u_23_phi_39,
            u_24_19,
            u_24_21,
            u_25_12,
            u_25_3,
            u_25_4,
            u_25_7,
            u_26_2,
            u_26_20,
            u_26_3,
            u_26_4,
            u_26_5,
            u_26_6,
            u_26_7,
            u_26_phi_42,
            u_26_phi_53,
            u_27_0,
            u_27_1,
            u_27_15,
            u_27_8,
            u_27_phi_43,
            u_28_0,
            u_28_1,
            u_28_22,
            u_28_4,
            u_28_phi_44,
            u_29_0,
            u_29_1,
            u_29_10,
            u_29_18,
            u_29_19,
            u_29_4,
            u_29_9,
            u_29_phi_45,
            u_29_phi_54,
            u_3_1,
            u_3_11,
            u_3_14,
            u_3_17,
            u_3_2,
            u_3_3,
            u_3_9,
            u_3_phi_23,
            u_30_0,
            u_30_1,
            u_30_3,
            u_30_4,
            u_30_6,
            u_30_phi_46,
            u_31_6,
            u_32_3,
            u_32_6,
            u_32_7,
            u_32_8,
            u_32_phi_55,
            u_33_2,
            u_33_3,
            u_33_6,
            u_33_7,
            u_33_phi_47,
            u_33_phi_56,
            u_34_0,
            u_34_1,
            u_34_2,
            u_34_3,
            u_34_phi_47,
            u_34_phi_57,
            u_35_0,
            u_35_1,
            u_35_2,
            u_35_3,
            u_35_phi_47,
            u_35_phi_58,
            u_36_0,
            u_36_1,
            u_36_3,
            u_36_phi_47,
            u_37_0,
            u_37_1,
            u_37_16,
            u_37_17,
            u_37_phi_49,
            u_37_phi_58,
            u_38_0,
            u_38_1,
            u_38_2,
            u_38_4,
            u_38_phi_50,
            u_39_0,
            u_39_1,
            u_39_12,
            u_39_13,
            u_39_3,
            u_39_phi_51,
            u_39_phi_58,
            u_4_1,
            u_4_2,
            u_4_3,
            u_4_4,
            u_4_5,
            u_4_6,
            u_4_phi_19,
            u_4_phi_21,
            u_4_phi_22,
            u_40_3,
            u_40_4,
            u_40_5,
            u_40_6,
            u_40_phi_52,
            u_40_phi_59,
            u_41_1,
            u_41_2,
            u_41_phi_58,
            u_42_0,
            u_42_1,
            u_42_phi_58,
            u_43_0,
            u_43_1,
            u_43_phi_61,
            u_44_0,
            u_44_1,
            u_44_phi_62,
            u_5_1,
            u_5_14,
            u_5_19,
            u_5_2,
            u_5_20,
            u_5_21,
            u_5_23,
            u_5_24,
            u_5_26,
            u_5_27,
            u_5_28,
            u_5_29,
            u_5_30,
            u_5_phi_20,
            u_5_phi_24,
            u_5_phi_67,
            u_5_phi_78,
            u_5_phi_82,
            u_6_1,
            u_6_11,
            u_6_12,
            u_6_13,
            u_6_15,
            u_6_18,
            u_6_19,
            u_6_2,
            u_6_20,
            u_6_22,
            u_6_23,
            u_6_24,
            u_6_25,
            u_6_29,
            u_6_30,
            u_6_31,
            u_6_33,
            u_6_34,
            u_6_36,
            u_6_4,
            u_6_5,
            u_6_7,
            u_6_8,
            u_6_phi_11,
            u_6_phi_18,
            u_6_phi_4,
            u_6_phi_60,
            u_6_phi_65,
            u_6_phi_69,
            u_6_phi_76,
            u_6_phi_80,
            u_6_phi_85,
            u_7_10,
            u_7_3,
            u_7_4,
            u_7_5,
            u_7_6,
            u_7_7,
            u_7_8,
            u_7_9,
            u_7_phi_76,
            u_7_phi_83,
            u_7_phi_86,
            u_8_0,
            u_8_1,
            u_8_11,
            u_8_14,
            u_8_18,
            u_8_2,
            u_8_20,
            u_8_21,
            u_8_23,
            u_8_24,
            u_8_26,
            u_8_27,
            u_8_28,
            u_8_29,
            u_8_3,
            u_8_5,
            u_8_6,
            u_8_7,
            u_8_8,
            u_8_phi_2,
            u_8_phi_63,
            u_8_phi_68,
            u_8_phi_71,
            u_8_phi_73,
            u_8_phi_76,
            u_8_phi_9,
            u_9_10,
            u_9_12,
            u_9_14,
            u_9_16,
            u_9_19,
            u_9_20,
            u_9_21,
            u_9_22,
            u_9_6,
            u_9_phi_72,
            u_9_phi_74,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/simple_light_smoke1/vs_in.csv", "d:/UC/simple_light_smoke1/vs_out.csv", 0);
        uniform("d:/UC/simple_light_smoke1/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/simple_light_smoke1/vs_cbuf8.csv", "uvec4"){
            add(10,11,28,29,30);
            add_range(0,7);
        };
        uniform("d:/UC/simple_light_smoke1/vs_cbuf9.csv", "uvec4"){
            add(194,195,196,197);
            add_range(0, 160);
        };
        uniform("d:/UC/simple_light_smoke1/vs_cbuf10.csv", "uvec4"){
            add_range(0,6);
        };
        uniform("d:/UC/simple_light_smoke1/vs_cbuf13.csv", "uvec4"){
            add(0,1,2,3,5,6);
        };
        uniform("d:/UC/simple_light_smoke1/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        uniform("d:/UC/simple_light_smoke1/vs_cbuf16.csv", "uvec4"){
            add(0,1);
        };
        vao_attr("private Vector4[] in_attr3_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr3.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr8_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr8.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_simple_light_smoke1/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,101);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/simple_light_smoke1/1", "d:/uc/VAO_simple_light_smoke1/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/simple_light_smoke1/2", "d:/uc/VAO_simple_light_smoke1/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/simple_light_smoke1/3", "d:/uc/VAO_simple_light_smoke1/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/simple_light_smoke1/4", "d:/uc/VAO_simple_light_smoke1/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/simple_light_smoke1/5", "d:/uc/VAO_simple_light_smoke1/5");
    };
    type_replacement
    {
        vs_cbuf0 = float;
        vs_cbuf8 = float;
        //vs_cbuf9 = float;
        vs_cbuf10 = float;
        vs_cbuf13 = float;
        vs_cbuf15 = float;
        vs_cbuf16 = float;
    };
    function_replacement
    {
        textureQueryLod = textureQueryLod(@arg(0), @arg(1), s_linear_clamp_sampler);
        textureLod = textureLod(@arg(0), @arg(1), @arg(2), s_linear_clamp_sampler);
        vs_cbuf9[7].x = vs_cbuf9_7_x;
        vs_cbuf9[7].y = vs_cbuf9_7_y;
        vs_cbuf9[7].z = vs_cbuf9_7_z;
        vs_cbuf9[7].w = vs_cbuf9_7_w;
        vs_cbuf9[*] = vs_cbuf9[@arg(1)];
        replacement(vs_cbuf8[$iter], view_proj[@arg(1)])for(0,7);
        vs_cbuf8[29] = camera_wpos;
        vs_cbuf15[28] = lightDir;
        vs_ssbo0[28] = @join(vs_ssbo_color1.x * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[29] = @join(vs_ssbo_color1.y * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[30] = @join(vs_ssbo_color1.z * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[31] = @join(vs_ssbo_color1.w * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[32] = @join(vs_ssbo_color2.x * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[33] = @join(vs_ssbo_color2.y * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[34] = @join(vs_ssbo_color2.z * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[35] = @join(vs_ssbo_color2.w * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[*] = vs_ssbo0[@arg(1)];
        (*)[*] = @join(@arg_and_lvlup(0), _, @arg_and_lvlup(1));
    };
    string_replacement
    {
        string("uvec", "uint");
        string("ivec", "int");
        string("vec2", "float2");
        string("vec3", "float3");
        string("vec4", "float4");
        string("tex5", "_CameraDepthTexture");
        string("isnan", "myIsNaN");
        regex(@"\bin_attr0\b", "v.vertex");
        regex(@"\bin_attr1\b", "v.uv");
        regex(@"\bin_attr2\b", "v.offset");
        string("in_attr", "i.vao_attr");
        string("out_attr", "o.fs_attr");
        string("gl_Position", "o.vertex");
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
        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            b_1_0,
            b_1_1,
            b_1_2,
            b_1_3,
            b_1_4,
            b_1_5,
            f_0_9,
            f_2_2,
            f_2_9,
            f_4_3,
            f_4_4,
            f_4_5,
            f_6_3,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            pf_0_10,
            pf_1_11,
            pf_1_6,
            pf_1_8,
            pf_1_9,
            pf_2_3,
            pf_2_5,
            pf_2_6,
            pf_2_8,
            pf_3_10,
            pf_3_11,
            pf_3_13,
            pf_3_4,
            pf_3_8,
            pf_4_6,
            pf_5_0,
            pf_5_5,
            pf_6_2,
            u_1_1,
            u_1_2,
            u_1_phi_4,
            u_2_1,
            u_2_2,
            u_2_phi_5,
            u_3_1,
            u_3_2,
            u_3_phi_3,
            u_4_0,
            u_4_1,
            u_4_phi_2,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/simple_light_smoke1/vs_out.csv", 0){
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
        uniform("d:/UC/simple_light_smoke1/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/simple_light_smoke1/fs_cbuf9.csv", "uvec4"){
            add(19,139,140);
        };
        uniform("d:/UC/simple_light_smoke1/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/simple_light_smoke1/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,39);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/simple_light_smoke1/1", "d:/uc/VAO_simple_light_smoke1/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/simple_light_smoke1/2", "d:/uc/VAO_simple_light_smoke1/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/simple_light_smoke1/3", "d:/uc/VAO_simple_light_smoke1/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/simple_light_smoke1/4", "d:/uc/VAO_simple_light_smoke1/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/simple_light_smoke1/5", "d:/uc/VAO_simple_light_smoke1/5");
    };
    type_replacement
    {
        fs_cbuf8 = float;
        fs_cbuf9 = float;
        fs_cbuf13 = float;
        fs_cbuf15 = float;
    };
    function_replacement
    {
        textureQueryLod = textureQueryLod(@arg(0), @arg(1), s_linear_clamp_sampler);
        textureLod = textureLod(@arg(0), @arg(1), @arg(2), s_linear_clamp_sampler);
        texture(*,*) = textureSample(@arg(0), @arg(1), s_linear_clamp_sampler);
        (*)[*] = @join(@arg_and_lvlup(0), _, @arg_and_lvlup(1));
    };
    string_replacement
    {
        string("uvec", "uint");
        string("ivec", "int");
        string("vec2", "float2");
        string("vec3", "float3");
        string("vec4", "float4");
        string("in_attr", "i.fs_attr");
        string("gl_FragCoord", "i.vertex");
        string("frag_color0", "col");
        string("tex5", "_CameraDepthTexture");
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
vs_code_block(hlsl_prologue)
{:
    // 1u, 0u, 301056u, 0u
    uint vs_cbuf9_7_x = 1u;
    uint vs_cbuf9_7_y = 0u;
    uint vs_cbuf9_7_z = 301056u;
    uint vs_cbuf9_7_w = 0u;
:};
