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
            f_0_12,
            f_0_26,
            f_0_39,
            f_0_40,
            f_0_41,
            f_0_48,
            f_0_56,
            f_0_78,
            f_0_88,
            f_0_89,
            f_0_91,
            f_0_92,
            f_1_151,
            f_1_152,
            f_1_2,
            f_1_37,
            f_1_40,
            f_1_47,
            f_1_49,
            f_1_51,
            f_1_58,
            f_1_64,
            f_1_68,
            f_1_71,
            f_1_76,
            f_1_85,
            f_1_95,
            f_2_104,
            f_3_23,
            f_3_35,
            f_3_56,
            f_3_63,
            f_3_72,
            f_3_89,
            f_4_3,
            f_5_13,
            f_5_25,
            f_5_44,
            f_6_21,
            f_6_42,
            f_6_49,
            f_7_25,
            f_8_11,
            f_8_26,
            f2_0_0,
            f4_0_1,
            pf_0_1,
            pf_0_17,
            pf_0_19,
            pf_0_24,
            pf_0_25,
            pf_0_27,
            pf_1_17,
            pf_1_19,
            pf_1_23,
            pf_1_6,
            pf_10_12,
            pf_11_10,
            pf_11_11,
            pf_12_3,
            pf_12_7,
            pf_13_8,
            pf_13_9,
            pf_15_12,
            pf_15_25,
            pf_15_28,
            pf_16_15,
            pf_16_18,
            pf_17_17,
            pf_18_15,
            pf_18_16,
            pf_19_2,
            pf_19_4,
            pf_2_14,
            pf_2_19,
            pf_2_21,
            pf_2_7,
            pf_21_5,
            pf_22_14,
            pf_22_24,
            pf_22_7,
            pf_23_1,
            pf_24_6,
            pf_24_8,
            pf_25_13,
            pf_26_10,
            pf_26_15,
            pf_27_10,
            pf_27_2,
            pf_27_5,
            pf_29_1,
            pf_3_10,
            pf_3_3,
            pf_3_7,
            pf_30_2,
            pf_4_2,
            pf_5_5,
            pf_6_5,
            pf_9_9,
            u_0_12,
            u_0_13,
            u_0_15,
            u_0_16,
            u_0_2,
            u_0_7,
            u_0_9,
            u_0_phi_56,
            u_0_phi_71,
            u_1_1,
            u_1_15,
            u_1_16,
            u_1_18,
            u_1_19,
            u_1_2,
            u_1_20,
            u_1_22,
            u_1_4,
            u_1_9,
            u_1_phi_22,
            u_1_phi_58,
            u_1_phi_68,
            u_10_10,
            u_10_11,
            u_10_12,
            u_10_14,
            u_10_15,
            u_10_16,
            u_10_17,
            u_10_18,
            u_10_25,
            u_10_26,
            u_10_4,
            u_10_9,
            u_10_phi_19,
            u_10_phi_21,
            u_10_phi_73,
            u_11_14,
            u_11_16,
            u_11_20,
            u_11_21,
            u_11_6,
            u_11_phi_74,
            u_12_13,
            u_12_14,
            u_12_16,
            u_12_19,
            u_12_24,
            u_12_5,
            u_12_phi_24,
            u_13_10,
            u_13_11,
            u_13_12,
            u_13_13,
            u_13_15,
            u_13_18,
            u_13_21,
            u_13_phi_25,
            u_14_3,
            u_14_4,
            u_14_5,
            u_14_6,
            u_14_phi_17,
            u_14_phi_72,
            u_15_3,
            u_16_1,
            u_16_2,
            u_16_phi_26,
            u_17_12,
            u_17_5,
            u_17_8,
            u_18_2,
            u_19_10,
            u_19_3,
            u_19_4,
            u_2_1,
            u_2_10,
            u_2_11,
            u_2_15,
            u_2_17,
            u_2_2,
            u_2_21,
            u_2_22,
            u_2_23,
            u_2_24,
            u_2_26,
            u_2_27,
            u_2_3,
            u_2_30,
            u_2_4,
            u_2_7,
            u_2_8,
            u_2_9,
            u_2_phi_20,
            u_2_phi_23,
            u_2_phi_28,
            u_2_phi_40,
            u_2_phi_57,
            u_2_phi_59,
            u_2_phi_70,
            u_20_1,
            u_20_12,
            u_20_13,
            u_20_19,
            u_20_2,
            u_20_25,
            u_20_26,
            u_20_8,
            u_21_1,
            u_21_12,
            u_21_18,
            u_21_2,
            u_22_1,
            u_22_2,
            u_22_phi_30,
            u_23_1,
            u_23_5,
            u_24_0,
            u_24_1,
            u_24_11,
            u_24_15,
            u_24_16,
            u_24_5,
            u_24_phi_27,
            u_25_0,
            u_25_1,
            u_25_2,
            u_25_phi_31,
            u_27_3,
            u_27_5,
            u_27_6,
            u_27_phi_33,
            u_28_2,
            u_28_3,
            u_28_phi_32,
            u_29_10,
            u_29_11,
            u_29_12,
            u_29_16,
            u_29_7,
            u_29_8,
            u_29_phi_35,
            u_29_phi_45,
            u_3_1,
            u_3_12,
            u_3_13,
            u_3_14,
            u_3_15,
            u_3_17,
            u_3_18,
            u_3_2,
            u_3_4,
            u_3_6,
            u_3_phi_16,
            u_3_phi_61,
            u_3_phi_67,
            u_3_phi_69,
            u_30_1,
            u_30_2,
            u_30_4,
            u_30_phi_36,
            u_31_0,
            u_31_1,
            u_31_12,
            u_31_8,
            u_31_phi_37,
            u_32_2,
            u_32_3,
            u_32_4,
            u_32_5,
            u_32_9,
            u_32_phi_44,
            u_32_phi_51,
            u_33_3,
            u_33_4,
            u_34_2,
            u_34_3,
            u_34_phi_38,
            u_35_0,
            u_35_1,
            u_35_phi_39,
            u_36_0,
            u_36_1,
            u_36_phi_39,
            u_37_0,
            u_37_1,
            u_37_2,
            u_37_phi_39,
            u_38_0,
            u_38_1,
            u_38_phi_39,
            u_39_0,
            u_39_1,
            u_39_13,
            u_39_14,
            u_39_5,
            u_39_phi_41,
            u_39_phi_46,
            u_4_1,
            u_4_10,
            u_4_2,
            u_4_4,
            u_4_5,
            u_4_7,
            u_4_8,
            u_4_phi_11,
            u_4_phi_4,
            u_4_phi_54,
            u_40_0,
            u_40_1,
            u_40_11,
            u_40_12,
            u_40_9,
            u_40_phi_42,
            u_40_phi_47,
            u_41_0,
            u_41_1,
            u_41_10,
            u_41_5,
            u_41_9,
            u_41_phi_43,
            u_41_phi_48,
            u_42_3,
            u_42_4,
            u_42_phi_49,
            u_43_0,
            u_43_1,
            u_43_phi_50,
            u_44_0,
            u_44_1,
            u_44_phi_50,
            u_45_0,
            u_45_1,
            u_45_phi_50,
            u_46_0,
            u_46_1,
            u_46_phi_50,
            u_47_0,
            u_47_1,
            u_47_phi_50,
            u_48_0,
            u_48_1,
            u_48_phi_53,
            u_49_0,
            u_49_1,
            u_49_phi_55,
            u_5_10,
            u_5_7,
            u_5_8,
            u_6_0,
            u_6_1,
            u_6_10,
            u_6_14,
            u_6_16,
            u_6_2,
            u_6_22,
            u_6_23,
            u_6_25,
            u_6_26,
            u_6_28,
            u_6_29,
            u_6_3,
            u_6_4,
            u_6_5,
            u_6_9,
            u_6_phi_15,
            u_6_phi_2,
            u_6_phi_60,
            u_6_phi_63,
            u_6_phi_65,
            u_6_phi_9,
            u_7_10,
            u_7_13,
            u_7_15,
            u_7_19,
            u_7_25,
            u_7_26,
            u_7_27,
            u_7_28,
            u_7_7,
            u_7_8,
            u_7_9,
            u_7_phi_52,
            u_7_phi_64,
            u_7_phi_66,
            u_8_10,
            u_8_15,
            u_8_19,
            u_8_20,
            u_8_21,
            u_8_phi_62,
            u_9_13,
            u_9_14,
            u_9_21,
            u_9_phi_34,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/light_smoke_0/vs_in.csv", "d:/UC/light_smoke_0/vs_out.csv", 0);
        uniform("d:/UC/light_smoke_0/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/light_smoke_0/vs_cbuf8.csv", "uvec4"){
            add(10,11,28,29,30);
            add_range(0,7);
        };
        uniform("d:/UC/light_smoke_0/vs_cbuf9.csv", "uvec4"){
            //add(7,11,13,16,17,18,20,29,74,75,76,78,79,80,81,83,84,85,86,87,88,104,105,113,121,129,130,131,137,138,141,157);
            add(194,195,196);
            add_range(0, 160);
        };
        uniform("d:/UC/light_smoke_0/vs_cbuf10.csv", "uvec4"){
            add_range(0,6);
        };
        uniform("d:/UC/light_smoke_0/vs_cbuf13.csv", "uvec4"){
            add(0,1,2,3,5,6);
        };
        uniform("d:/UC/light_smoke_0/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        uniform("d:/UC/light_smoke_0/vs_cbuf16.csv", "uvec4"){
            add(0,1);
        };
        vao_attr("private Vector4[] in_attr3_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr3.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr8_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr8.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,34);
        };
        vao_attr("private Vector4[] in_attr11_array", "vec4", "d:/uc/VAO_light_smoke_0/vertex_in_attr11.csv", "[NonSerialized]"){
            add_range(0,34);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/light_smoke_0/1", "d:/uc/VAO_light_smoke_0/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/light_smoke_0/2", "d:/uc/VAO_light_smoke_0/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/light_smoke_0/3", "d:/uc/VAO_light_smoke_0/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/light_smoke_0/4", "d:/uc/VAO_light_smoke_0/4");
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
        vs_ssbo0[44] = @join(vs_ssbo_color1.x * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[45] = @join(vs_ssbo_color1.y * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[46] = @join(vs_ssbo_color1.z * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[47] = @join(vs_ssbo_color1.w * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[48] = @join(vs_ssbo_color2.x * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[49] = @join(vs_ssbo_color2.y * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[50] = @join(vs_ssbo_color2.z * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
        vs_ssbo0[51] = @join(vs_ssbo_color2.w * vs_ssbo_scale, @skip_and_lvlup(0), @skip_and_lvlup(1));
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
        string("in_attr", "uni_attr");
        string("out_attr", "o.fs_attr");
        string("gl_Position", "o.vertex");
        string("return;", "return o;");
        string("tex5", "_CameraDepthTexture");
        string("isnan", "myIsNaN");
        regex(@"\buni_attr0\b", "v.vertex");
        regex(@"\buni_attr1\b", "v.uv");
        regex(@"\buni_attr2\b", "v.offset");
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
        ps_attr("d:/UC/light_smoke_0/vs_out.csv", 0){
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
        uniform("d:/UC/light_smoke_0/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/light_smoke_0/fs_cbuf9.csv", "uvec4"){
            add(19,139,140);
        };
        uniform("d:/UC/light_smoke_0/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/light_smoke_0/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,39);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/light_smoke_0/1", "d:/uc/VAO_light_smoke_0/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/light_smoke_0/2", "d:/uc/VAO_light_smoke_0/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/light_smoke_0/3", "d:/uc/VAO_light_smoke_0/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/light_smoke_0/4", "d:/uc/VAO_light_smoke_0/4");
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
        string("return;", "return col;");
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
