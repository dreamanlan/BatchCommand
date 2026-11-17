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
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            b_0_21,
            b_0_23,
            b_1_40,
            b_1_42,
            b_1_45,
            b_1_59,
            b_2_14,
            b_2_16,
            b_2_20,
            b_3_4,
            b_4_8,
            f_0_25,
            f_0_50,
            f_0_51,
            f_1_17,
            f_1_20,
            f_1_25,
            f_1_46,
            f_1_65,
            f_10_25,
            f_10_26,
            f_2_108,
            f_2_176,
            f_2_187,
            f_2_44,
            f_2_64,
            f_2_65,
            f_2_70,
            f_3_122,
            f_3_123,
            f_3_126,
            f_3_33,
            f_3_50,
            f_3_51,
            f_3_53,
            f_3_54,
            f_3_66,
            f_4_26,
            f_5_22,
            f_5_29,
            f_5_30,
            f_6_12,
            f_6_15,
            f_7_25,
            f_9_15,
            f_9_24,
            f4_0_0,
            pf_0_1,
            pf_0_10,
            pf_0_11,
            pf_0_15,
            pf_0_16,
            pf_0_22,
            pf_0_5,
            pf_0_6,
            pf_0_7,
            pf_0_9,
            pf_1_12,
            pf_1_2,
            pf_1_3,
            pf_1_5,
            pf_1_6,
            pf_1_7,
            pf_1_9,
            pf_10_10,
            pf_10_12,
            pf_10_17,
            pf_11_20,
            pf_11_22,
            pf_12_11,
            pf_12_12,
            pf_12_17,
            pf_12_18,
            pf_12_23,
            pf_13_12,
            pf_14_10,
            pf_14_11,
            pf_14_14,
            pf_14_24,
            pf_14_7,
            pf_17_10,
            pf_17_13,
            pf_18_6,
            pf_18_7,
            pf_18_8,
            pf_19_1,
            pf_19_2,
            pf_19_3,
            pf_19_8,
            pf_2_11,
            pf_2_12,
            pf_2_14,
            pf_2_15,
            pf_2_8,
            pf_2_9,
            pf_20_17,
            pf_20_18,
            pf_20_19,
            pf_20_23,
            pf_20_24,
            pf_20_9,
            pf_21_2,
            pf_22_5,
            pf_22_8,
            pf_23_0,
            pf_23_13,
            pf_23_7,
            pf_24_5,
            pf_25_5,
            pf_27_0,
            pf_3_0,
            pf_3_14,
            pf_3_16,
            pf_3_17,
            pf_3_18,
            pf_3_19,
            pf_3_26,
            pf_3_27,
            pf_3_3,
            pf_4_9,
            pf_5_10,
            pf_5_21,
            pf_6_11,
            pf_6_13,
            pf_7_15,
            pf_7_16,
            pf_7_18,
            pf_7_8,
            pf_8_14,
            pf_8_7,
            pf_9_14,
            u_0_1,
            u_0_11,
            u_0_14,
            u_0_15,
            u_0_17,
            u_0_18,
            u_0_2,
            u_0_20,
            u_0_21,
            u_0_3,
            u_0_4,
            u_0_5,
            u_0_9,
            u_0_phi_20,
            u_0_phi_56,
            u_0_phi_58,
            u_0_phi_60,
            u_1_1,
            u_1_2,
            u_1_4,
            u_1_5,
            u_1_7,
            u_1_8,
            u_1_phi_14,
            u_1_phi_37,
            u_1_phi_6,
            u_10_10,
            u_10_12,
            u_10_6,
            u_10_7,
            u_10_9,
            u_10_phi_48,
            u_11_12,
            u_11_13,
            u_11_5,
            u_11_phi_57,
            u_12_6,
            u_13_2,
            u_13_3,
            u_13_5,
            u_13_6,
            u_13_7,
            u_13_phi_27,
            u_14_1,
            u_14_2,
            u_14_3,
            u_14_4,
            u_14_phi_26,
            u_14_phi_54,
            u_15_1,
            u_15_4,
            u_15_6,
            u_15_7,
            u_16_1,
            u_16_10,
            u_16_11,
            u_17_0,
            u_17_1,
            u_17_11,
            u_17_12,
            u_17_4,
            u_17_6,
            u_17_7,
            u_17_phi_55,
            u_18_0,
            u_18_3,
            u_18_6,
            u_18_7,
            u_19_0,
            u_19_1,
            u_19_11,
            u_19_12,
            u_19_13,
            u_2_13,
            u_20_0,
            u_20_1,
            u_20_2,
            u_20_phi_25,
            u_21_0,
            u_21_1,
            u_21_2,
            u_21_3,
            u_21_4,
            u_21_9,
            u_21_phi_21,
            u_21_phi_33,
            u_22_0,
            u_22_6,
            u_23_0,
            u_23_9,
            u_24_10,
            u_24_11,
            u_24_3,
            u_24_9,
            u_24_phi_23,
            u_25_11,
            u_25_12,
            u_25_4,
            u_25_9,
            u_25_phi_24,
            u_26_15,
            u_26_16,
            u_26_17,
            u_26_18,
            u_26_5,
            u_26_7,
            u_26_phi_28,
            u_27_4,
            u_27_6,
            u_27_7,
            u_27_8,
            u_27_phi_29,
            u_28_4,
            u_28_5,
            u_28_phi_30,
            u_29_4,
            u_29_5,
            u_29_phi_31,
            u_3_0,
            u_3_1,
            u_3_11,
            u_3_12,
            u_3_2,
            u_3_3,
            u_3_5,
            u_3_phi_1,
            u_3_phi_11,
            u_30_4,
            u_30_5,
            u_30_phi_32,
            u_31_5,
            u_31_6,
            u_31_7,
            u_31_phi_32,
            u_32_1,
            u_32_2,
            u_32_phi_32,
            u_33_0,
            u_33_1,
            u_33_phi_32,
            u_34_0,
            u_34_1,
            u_34_phi_32,
            u_35_0,
            u_35_1,
            u_35_phi_35,
            u_36_0,
            u_36_1,
            u_36_phi_36,
            u_4_0,
            u_4_1,
            u_4_4,
            u_4_5,
            u_4_phi_3,
            u_4_phi_63,
            u_5_16,
            u_5_17,
            u_5_19,
            u_5_2,
            u_5_20,
            u_5_3,
            u_5_4,
            u_5_5,
            u_5_6,
            u_5_phi_43,
            u_5_phi_59,
            u_5_phi_64,
            u_5_phi_9,
            u_6_13,
            u_6_14,
            u_6_15,
            u_6_16,
            u_6_17,
            u_6_2,
            u_6_21,
            u_6_22,
            u_6_24,
            u_6_28,
            u_6_30,
            u_6_31,
            u_6_33,
            u_6_35,
            u_6_36,
            u_6_42,
            u_6_43,
            u_6_44,
            u_6_45,
            u_6_7,
            u_6_phi_18,
            u_6_phi_19,
            u_6_phi_41,
            u_6_phi_61,
            u_6_phi_62,
            u_7_10,
            u_7_11,
            u_7_12,
            u_7_13,
            u_7_14,
            u_7_16,
            u_7_17,
            u_7_18,
            u_7_2,
            u_7_9,
            u_7_phi_17,
            u_7_phi_19,
            u_7_phi_34,
            u_8_17,
            u_8_22,
            u_8_23,
            u_8_25,
            u_8_26,
            u_8_30,
            u_8_5,
            u_8_phi_42,
            u_8_phi_50,
            u_9_0,
            u_9_11,
            u_9_15,
            u_9_16,
            u_9_17,
            u_9_19,
            u_9_5,
            u_9_phi_51,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/standalone_smoke3/vs_in.csv", "d:/UC/standalone_smoke3/vs_out.csv", 0);
        uniform("d:/UC/standalone_smoke3/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/standalone_smoke3/vs_cbuf8.csv", "uvec4"){
            add(10,11);
            add_range(0,7);
            add_range(24,30);
        };
        uniform("d:/UC/standalone_smoke3/vs_cbuf9.csv", "uvec4"){
            add_range(0, 160);
            add(194,195,196);
        };
        uniform("d:/UC/standalone_smoke3/vs_cbuf10.csv", "uvec4"){
            add_range(0,3);
        };
        uniform("d:/UC/standalone_smoke3/vs_cbuf13.csv", "uvec4"){
            add(0,1,2,3,5,6);
        };
        uniform("d:/UC/standalone_smoke3/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        uniform("d:/UC/standalone_smoke3/vs_cbuf16.csv", "uvec4"){
            add(0,1);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr7.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr8_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr8.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr11_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr11.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr12_array", "vec4", "d:/uc/VAO_standalone_smoke3/vertex_in_attr12.csv", "[NonSerialized]"){
            add_range(0,102);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/standalone_smoke3/1", "d:/uc/VAO_standalone_smoke3/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/standalone_smoke3/2", "d:/uc/VAO_standalone_smoke3/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/standalone_smoke3/3", "d:/uc/VAO_standalone_smoke3/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/standalone_smoke3/4", "d:/uc/VAO_standalone_smoke3/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/standalone_smoke3/5", "d:/uc/VAO_standalone_smoke3/5");
    };
    type_replacement
    {
        //vs_cbuf0 = float;
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
        vs_cbuf0[21].x = vs_cbuf0_21_x;
        vs_cbuf0[21].y = vs_cbuf0_21_y;
        vs_cbuf0[21].z = vs_cbuf0_21_z;
        vs_cbuf0[21].w = vs_cbuf0_21_w;
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
        string("tex3", "_CameraDepthTexture");
        string("isnan", "myIsNaN");
        regex(@"\bin_attr0\b", "v.vertex");
        regex(@"\bin_attr1\b", "v.uv");
        regex(@"\bin_attr2\b", "v.offset");
        regex(@"\bin_attr3\b", "v.coeff");
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
            f_0_16,
            f_0_2,
            f_0_7,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            pf_0_7,
            pf_1_5,
            pf_3_0,
            u_1_1,
            u_1_2,
            u_1_phi_5,
            u_2_1,
            u_2_2,
            u_2_phi_4,
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
        ps_attr("d:/UC/standalone_smoke3/vs_out.csv", 0){
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
        uniform("d:/UC/standalone_smoke3/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/standalone_smoke3/fs_cbuf9.csv", "uvec4"){
            add(139,140);
        };
        uniform("d:/UC/standalone_smoke3/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/standalone_smoke3/fs_cbuf15.csv", "uvec4"){
            add(1);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/standalone_smoke3/1", "d:/uc/VAO_standalone_smoke3/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/standalone_smoke3/2", "d:/uc/VAO_standalone_smoke3/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/standalone_smoke3/3", "d:/uc/VAO_standalone_smoke3/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/standalone_smoke3/4", "d:/uc/VAO_standalone_smoke3/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/standalone_smoke3/5", "d:/uc/VAO_standalone_smoke3/5");
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
        string("tex3", "_CameraDepthTexture");
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
    // 675610624u, 6u, 58880u, 0u
    uint vs_cbuf0_21_x = 675610624u;
    uint vs_cbuf0_21_y = 6u;
    uint vs_cbuf0_21_z = 58880u;
    uint vs_cbuf0_21_w = 0u;
    // 9u, 0u, 301056u, 47u
    uint vs_cbuf9_7_x = 9u;
    uint vs_cbuf9_7_y = 0u;
    uint vs_cbuf9_7_z = 301056u;
    uint vs_cbuf9_7_w = 47u;
:};
