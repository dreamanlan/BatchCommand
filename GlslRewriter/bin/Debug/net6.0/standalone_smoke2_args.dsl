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
            b_0_25,
            b_0_27,
            b_0_29,
            b_1_58,
            b_1_60,
            b_2_24,
            b_2_26,
            b_3_0,
            b_3_1,
            b_3_10,
            b_3_12,
            b_3_phi_33,
            b_4_4,
            b_4_6,
            b_5_10,
            f_0_34,
            f_0_40,
            f_0_45,
            f_0_50,
            f_0_51,
            f_0_53,
            f_0_54,
            f_1_71,
            f_10_25,
            f_10_43,
            f_11_15,
            f_11_16,
            f_11_19,
            f_11_20,
            f_12_18,
            f_12_22,
            f_12_28,
            f_13_5,
            f_13_8,
            f_2_19,
            f_3_38,
            f_3_55,
            f_4_14,
            f_4_16,
            f_4_18,
            f_4_25,
            f_4_27,
            f_4_6,
            f_4_78,
            f_4_8,
            f_4_84,
            f_4_85,
            f_5_0,
            f_6_38,
            f_7_0,
            f_9_1,
            f_9_24,
            f4_0_0,
            f4_0_1,
            pf_0_1,
            pf_0_11,
            pf_0_21,
            pf_0_25,
            pf_0_7,
            pf_1_1,
            pf_1_16,
            pf_1_20,
            pf_1_24,
            pf_1_25,
            pf_1_35,
            pf_1_6,
            pf_10_10,
            pf_10_14,
            pf_11_17,
            pf_12_0,
            pf_12_8,
            pf_12_9,
            pf_13_9,
            pf_14_6,
            pf_15_10,
            pf_15_20,
            pf_15_21,
            pf_15_31,
            pf_15_4,
            pf_16_7,
            pf_17_10,
            pf_18_3,
            pf_18_5,
            pf_18_7,
            pf_18_8,
            pf_2_12,
            pf_2_28,
            pf_2_48,
            pf_2_55,
            pf_2_63,
            pf_2_65,
            pf_2_69,
            pf_2_70,
            pf_2_75,
            pf_2_79,
            pf_2_81,
            pf_20_11,
            pf_20_8,
            pf_21_8,
            pf_22_1,
            pf_24_2,
            pf_24_9,
            pf_25_4,
            pf_26_0,
            pf_3_12,
            pf_3_14,
            pf_3_3,
            pf_3_7,
            pf_4_11,
            pf_4_3,
            pf_4_7,
            pf_4_8,
            pf_5_12,
            pf_5_16,
            pf_5_20,
            pf_5_26,
            pf_5_3,
            pf_5_31,
            pf_5_35,
            pf_5_8,
            pf_6_13,
            pf_6_21,
            pf_6_30,
            pf_6_33,
            pf_6_39,
            pf_6_4,
            pf_6_9,
            pf_7_20,
            pf_7_28,
            pf_7_30,
            pf_7_34,
            pf_7_38,
            pf_8_11,
            pf_8_12,
            pf_8_7,
            pf_9_14,
            u_0_1,
            u_0_11,
            u_0_12,
            u_0_13,
            u_0_15,
            u_0_19,
            u_0_2,
            u_0_21,
            u_0_22,
            u_0_24,
            u_0_27,
            u_0_3,
            u_0_31,
            u_0_32,
            u_0_34,
            u_0_4,
            u_0_7,
            u_0_8,
            u_0_phi_22,
            u_0_phi_29,
            u_0_phi_69,
            u_0_phi_71,
            u_1_1,
            u_1_10,
            u_1_2,
            u_1_6,
            u_1_7,
            u_1_8,
            u_1_9,
            u_1_phi_21,
            u_1_phi_39,
            u_1_phi_40,
            u_10_12,
            u_10_13,
            u_10_15,
            u_10_16,
            u_10_19,
            u_10_20,
            u_10_6,
            u_10_7,
            u_10_8,
            u_10_9,
            u_10_phi_51,
            u_10_phi_70,
            u_10_phi_74,
            u_10_phi_79,
            u_11_0,
            u_11_1,
            u_11_10,
            u_11_14,
            u_11_15,
            u_11_3,
            u_11_4,
            u_11_5,
            u_11_6,
            u_11_7,
            u_11_8,
            u_11_phi_19,
            u_11_phi_2,
            u_11_phi_26,
            u_11_phi_31,
            u_11_phi_70,
            u_12_10,
            u_12_2,
            u_12_3,
            u_12_5,
            u_12_6,
            u_12_7,
            u_12_8,
            u_12_9,
            u_12_phi_28,
            u_12_phi_31,
            u_12_phi_35,
            u_12_phi_9,
            u_13_14,
            u_13_16,
            u_13_17,
            u_13_19,
            u_13_20,
            u_13_3,
            u_13_4,
            u_13_7,
            u_13_phi_32,
            u_13_phi_72,
            u_13_phi_77,
            u_14_0,
            u_14_1,
            u_14_10,
            u_14_13,
            u_14_17,
            u_14_19,
            u_14_7,
            u_14_8,
            u_14_9,
            u_14_phi_15,
            u_14_phi_31,
            u_14_phi_35,
            u_15_10,
            u_15_6,
            u_15_7,
            u_15_phi_32,
            u_16_14,
            u_16_15,
            u_16_17,
            u_16_3,
            u_16_4,
            u_16_phi_33,
            u_17_1,
            u_17_16,
            u_17_2,
            u_17_25,
            u_17_26,
            u_17_30,
            u_17_31,
            u_17_39,
            u_17_40,
            u_17_phi_33,
            u_18_1,
            u_18_11,
            u_18_12,
            u_18_2,
            u_18_20,
            u_18_21,
            u_18_22,
            u_18_23,
            u_18_phi_33,
            u_19_1,
            u_19_10,
            u_19_11,
            u_19_12,
            u_19_13,
            u_19_14,
            u_19_16,
            u_19_2,
            u_19_3,
            u_19_4,
            u_19_7,
            u_19_8,
            u_19_9,
            u_19_phi_33,
            u_19_phi_37,
            u_19_phi_47,
            u_19_phi_56,
            u_19_phi_60,
            u_19_phi_66,
            u_2_1,
            u_2_2,
            u_2_4,
            u_2_5,
            u_2_7,
            u_2_8,
            u_2_phi_26,
            u_2_phi_75,
            u_2_phi_78,
            u_20_0,
            u_20_1,
            u_20_2,
            u_20_3,
            u_20_6,
            u_20_7,
            u_20_8,
            u_20_phi_33,
            u_20_phi_37,
            u_20_phi_44,
            u_21_1,
            u_21_12,
            u_21_15,
            u_21_17,
            u_21_18,
            u_21_19,
            u_21_2,
            u_21_20,
            u_21_7,
            u_21_phi_35,
            u_22_0,
            u_22_1,
            u_22_14,
            u_22_15,
            u_22_2,
            u_22_25,
            u_22_3,
            u_22_7,
            u_22_phi_34,
            u_22_phi_36,
            u_23_0,
            u_23_1,
            u_23_14,
            u_23_18,
            u_23_19,
            u_23_2,
            u_23_3,
            u_23_5,
            u_23_phi_34,
            u_23_phi_36,
            u_24_0,
            u_24_1,
            u_24_2,
            u_24_3,
            u_24_7,
            u_24_8,
            u_24_phi_34,
            u_24_phi_36,
            u_24_phi_45,
            u_25_0,
            u_25_1,
            u_25_2,
            u_25_3,
            u_25_6,
            u_25_7,
            u_25_phi_34,
            u_25_phi_36,
            u_25_phi_46,
            u_26_1,
            u_26_2,
            u_26_5,
            u_26_phi_36,
            u_27_14,
            u_27_15,
            u_27_17,
            u_27_18,
            u_27_2,
            u_27_3,
            u_27_6,
            u_27_8,
            u_27_phi_37,
            u_28_5,
            u_29_4,
            u_29_6,
            u_29_7,
            u_29_phi_48,
            u_3_1,
            u_3_10,
            u_3_11,
            u_3_12,
            u_3_17,
            u_3_18,
            u_3_19,
            u_3_2,
            u_3_21,
            u_3_25,
            u_3_27,
            u_3_3,
            u_3_4,
            u_3_6,
            u_3_7,
            u_3_8,
            u_3_phi_25,
            u_3_phi_27,
            u_3_phi_38,
            u_3_phi_41,
            u_30_7,
            u_30_8,
            u_30_phi_49,
            u_31_3,
            u_32_0,
            u_33_0,
            u_33_1,
            u_33_phi_50,
            u_34_1,
            u_34_2,
            u_34_phi_52,
            u_35_0,
            u_35_1,
            u_35_14,
            u_35_4,
            u_35_phi_53,
            u_36_10,
            u_36_12,
            u_36_4,
            u_36_7,
            u_37_3,
            u_38_11,
            u_38_12,
            u_38_13,
            u_38_6,
            u_38_phi_61,
            u_39_10,
            u_39_3,
            u_39_9,
            u_39_phi_62,
            u_4_1,
            u_4_2,
            u_4_4,
            u_4_5,
            u_4_6,
            u_4_7,
            u_4_phi_23,
            u_4_phi_28,
            u_4_phi_30,
            u_40_10,
            u_40_11,
            u_40_2,
            u_40_3,
            u_40_9,
            u_40_phi_54,
            u_40_phi_63,
            u_41_0,
            u_41_1,
            u_41_phi_55,
            u_42_0,
            u_42_1,
            u_42_phi_55,
            u_43_0,
            u_43_1,
            u_43_phi_55,
            u_44_0,
            u_44_1,
            u_44_2,
            u_44_3,
            u_44_4,
            u_44_phi_55,
            u_44_phi_64,
            u_45_0,
            u_45_1,
            u_45_2,
            u_45_3,
            u_45_phi_57,
            u_45_phi_65,
            u_46_0,
            u_46_1,
            u_46_2,
            u_46_3,
            u_46_phi_58,
            u_46_phi_65,
            u_47_0,
            u_47_1,
            u_47_2,
            u_47_3,
            u_47_phi_59,
            u_47_phi_65,
            u_48_0,
            u_48_1,
            u_48_phi_65,
            u_49_0,
            u_49_1,
            u_49_phi_68,
            u_5_1,
            u_5_2,
            u_5_3,
            u_5_4,
            u_5_5,
            u_5_6,
            u_5_phi_24,
            u_5_phi_28,
            u_5_phi_30,
            u_6_1,
            u_6_2,
            u_6_3,
            u_6_4,
            u_6_6,
            u_6_phi_18,
            u_6_phi_30,
            u_7_1,
            u_7_10,
            u_7_13,
            u_7_14,
            u_7_17,
            u_7_19,
            u_7_2,
            u_7_23,
            u_7_24,
            u_7_26,
            u_7_7,
            u_7_8,
            u_7_9,
            u_7_phi_17,
            u_7_phi_26,
            u_7_phi_31,
            u_7_phi_40,
            u_7_phi_71,
            u_8_1,
            u_8_10,
            u_8_12,
            u_8_14,
            u_8_17,
            u_8_18,
            u_8_2,
            u_8_3,
            u_8_4,
            u_8_5,
            u_8_6,
            u_8_9,
            u_8_phi_16,
            u_8_phi_20,
            u_8_phi_31,
            u_8_phi_67,
            u_8_phi_71,
            u_9_1,
            u_9_11,
            u_9_19,
            u_9_2,
            u_9_21,
            u_9_22,
            u_9_23,
            u_9_26,
            u_9_27,
            u_9_29,
            u_9_30,
            u_9_4,
            u_9_5,
            u_9_7,
            u_9_8,
            u_9_phi_11,
            u_9_phi_32,
            u_9_phi_4,
            u_9_phi_70,
            u_9_phi_73,
            u_9_phi_76,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/standalone_smoke2/vs_in.csv", "d:/UC/standalone_smoke2/vs_out.csv", 0);
        uniform("d:/UC/standalone_smoke2/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/standalone_smoke2/vs_cbuf8.csv", "uvec4"){
            add(10,11,28,29,30);
            add_range(0,7);
        };
        uniform("d:/UC/standalone_smoke2/vs_cbuf9.csv", "uvec4"){
            add_range(0, 160);
            add(197);
        };
        uniform("d:/UC/standalone_smoke2/vs_cbuf10.csv", "uvec4"){
            add_range(0,3);
        };
        uniform("d:/UC/standalone_smoke2/vs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/standalone_smoke2/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr7.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr11_array", "vec4", "d:/uc/VAO_standalone_smoke2/vertex_in_attr11.csv", "[NonSerialized]"){
            add_range(0,102);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/standalone_smoke2/1", "d:/uc/VAO_standalone_smoke2/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/standalone_smoke2/2", "d:/uc/VAO_standalone_smoke2/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/standalone_smoke2/3", "d:/uc/VAO_standalone_smoke2/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/standalone_smoke2/4", "d:/uc/VAO_standalone_smoke2/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/standalone_smoke2/5", "d:/uc/VAO_standalone_smoke2/5");
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
        vs_cbuf9[197] = vs_cbuf9_197;
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
            b_0_0,
            f_0_6,
            f_2_2,
            f_2_3,
            f_3_2,
            f_4_4,
            f_4_8,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f4_0_5,
            f4_0_6,
            pf_0_5,
            pf_1_10,
            pf_1_12,
            pf_1_7,
            pf_1_9,
            pf_2_3,
            pf_2_5,
            pf_2_6,
            pf_2_8,
            pf_3_10,
            pf_3_12,
            pf_3_3,
            pf_3_7,
            pf_3_9,
            pf_4_7,
            pf_5_0,
            pf_5_4,
            pf_6_2,
            u_0_3,
            u_0_4,
            u_0_phi_5,
            u_2_1,
            u_2_2,
            u_2_phi_3,
            u_3_1,
            u_3_2,
            u_3_phi_4,
            u_4_0,
            u_4_1,
            u_4_phi_2,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/standalone_smoke2/vs_out.csv", 0){
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
        uniform("d:/UC/standalone_smoke2/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/standalone_smoke2/fs_cbuf9.csv", "uvec4"){
            add(139,140);
        };
        uniform("d:/UC/standalone_smoke2/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/standalone_smoke2/fs_cbuf15.csv", "uvec4"){
            add(1,25,26);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/standalone_smoke2/1", "d:/uc/VAO_standalone_smoke2/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/standalone_smoke2/2", "d:/uc/VAO_standalone_smoke2/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/standalone_smoke2/3", "d:/uc/VAO_standalone_smoke2/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/standalone_smoke2/4", "d:/uc/VAO_standalone_smoke2/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/standalone_smoke2/5", "d:/uc/VAO_standalone_smoke2/5");
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
    // 1u, 1048576u, 301056u, 0u
    uint vs_cbuf9_7_x = 1u;
    uint vs_cbuf9_7_y = 1048576u;
    uint vs_cbuf9_7_z = 301056u;
    uint vs_cbuf9_7_w = 0u;
:};
