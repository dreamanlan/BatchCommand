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
            b_0_8,
            b_1_22,
            b_1_24,
            b_2_11,
            f_0_24,
            f_0_28,
            f_0_31,
            f_0_32,
            f_0_34,
            f_0_35,
            f_1_35,
            f_10_9,
            f_11_9,
            f_13_6,
            f_13_7,
            f_14_4,
            f_16_6,
            f_2_73,
            f_3_15,
            f_5_3,
            f_7_20,
            f_8_6,
            f4_0_0,
            f4_0_1,
            pf_0_1,
            pf_0_9,
            pf_1_1,
            pf_1_10,
            pf_1_6,
            pf_10_5,
            pf_11_3,
            pf_12_3,
            pf_12_8,
            pf_12_9,
            pf_13_2,
            pf_16_0,
            pf_17_0,
            pf_17_1,
            pf_2_11,
            pf_2_12,
            pf_2_13,
            pf_2_14,
            pf_2_4,
            pf_2_6,
            pf_3_16,
            pf_3_18,
            pf_3_19,
            pf_3_4,
            pf_4_13,
            pf_4_17,
            pf_4_21,
            pf_4_3,
            pf_6_1,
            pf_6_5,
            pf_6_7,
            pf_6_8,
            pf_7_1,
            pf_7_2,
            pf_8_12,
            pf_8_6,
            pf_8_8,
            pf_9_6,
            u_0_1,
            u_0_2,
            u_0_3,
            u_0_4,
            u_0_7,
            u_0_8,
            u_0_phi_19,
            u_0_phi_21,
            u_1_1,
            u_1_2,
            u_1_5,
            u_1_7,
            u_1_phi_18,
            u_11_2,
            u_11_3,
            u_11_6,
            u_11_8,
            u_11_9,
            u_11_phi_25,
            u_12_1,
            u_12_11,
            u_12_12,
            u_12_17,
            u_12_3,
            u_12_4,
            u_12_phi_26,
            u_13_10,
            u_13_6,
            u_13_7,
            u_13_9,
            u_13_phi_30,
            u_14_6,
            u_14_7,
            u_14_phi_29,
            u_15_3,
            u_15_5,
            u_15_8,
            u_15_9,
            u_15_phi_27,
            u_16_0,
            u_16_8,
            u_17_0,
            u_19_4,
            u_2_1,
            u_2_2,
            u_2_4,
            u_2_5,
            u_2_phi_11,
            u_2_phi_4,
            u_20_1,
            u_20_2,
            u_20_3,
            u_20_4,
            u_20_phi_23,
            u_20_phi_24,
            u_3_3,
            u_3_4,
            u_3_6,
            u_3_7,
            u_3_phi_16,
            u_3_phi_20,
            u_4_0,
            u_4_1,
            u_4_2,
            u_4_3,
            u_4_5,
            u_4_6,
            u_4_8,
            u_4_phi_2,
            u_4_phi_20,
            u_4_phi_9,
            u_5_4,
            u_5_5,
            u_5_6,
            u_5_7,
            u_5_8,
            u_5_phi_15,
            u_5_phi_17,
            u_6_4,
            u_6_5,
            u_6_7,
            u_6_phi_20,
            u_7_3,
            u_7_8,
            u_8_10,
            u_8_15,
            u_8_17,
            u_8_21,
            u_9_14,
            u_9_20,
            u_9_21,
            u_9_7,
            u_9_9,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/standalone_smoke1/vs_in.csv", "d:/UC/standalone_smoke1/vs_out.csv", 0);
        uniform("d:/UC/standalone_smoke1/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/standalone_smoke1/vs_cbuf8.csv", "uvec4"){
            add(29,30);
            add_range(0,7);
        };
        uniform("d:/UC/standalone_smoke1/vs_cbuf9.csv", "uvec4"){
            add_range(0, 160);
        };
        uniform("d:/UC/standalone_smoke1/vs_cbuf10.csv", "uvec4"){
            add_range(0,10);
        };
        uniform("d:/UC/standalone_smoke1/vs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/standalone_smoke1/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_standalone_smoke1/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_standalone_smoke1/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_standalone_smoke1/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,102);
        };
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_standalone_smoke1/vertex_in_attr7.csv", "[NonSerialized]"){
            add_range(0,102);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/standalone_smoke1/1", "d:/uc/VAO_standalone_smoke1/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/standalone_smoke1/2", "d:/uc/VAO_standalone_smoke1/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/standalone_smoke1/3", "d:/uc/VAO_standalone_smoke1/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/standalone_smoke1/4", "d:/uc/VAO_standalone_smoke1/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/standalone_smoke1/5", "d:/uc/VAO_standalone_smoke1/5");
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
            f_3_3,
            f_3_4,
            f_6_5,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            pf_0_7,
            pf_1_4,
            pf_1_6,
            pf_1_7,
            pf_1_9,
            pf_2_2,
            pf_2_4,
            pf_2_5,
            pf_2_7,
            pf_3_10,
            pf_3_12,
            pf_3_3,
            pf_3_7,
            pf_3_9,
            pf_4_6,
            pf_5_0,
            pf_5_5,
            pf_6_2,
            u_1_1,
            u_1_2,
            u_1_phi_4,
            u_2_1,
            u_2_2,
            u_2_phi_3,
            u_3_1,
            u_3_2,
            u_3_phi_5,
            u_4_0,
            u_4_1,
            u_4_phi_2,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/standalone_smoke1/vs_out.csv", 0){
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
        uniform("d:/UC/standalone_smoke1/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/standalone_smoke1/fs_cbuf9.csv", "uvec4"){
            add(139,140);
        };
        uniform("d:/UC/standalone_smoke1/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/standalone_smoke1/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,39);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/standalone_smoke1/1", "d:/uc/VAO_standalone_smoke1/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/standalone_smoke1/2", "d:/uc/VAO_standalone_smoke1/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/standalone_smoke1/3", "d:/uc/VAO_standalone_smoke1/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/standalone_smoke1/4", "d:/uc/VAO_standalone_smoke1/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/standalone_smoke1/5", "d:/uc/VAO_standalone_smoke1/5");
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
        string("tex4", "_CameraDepthTexture");
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
    // 1u, 0u, 299520u, 0u
    uint vs_cbuf9_7_x = 1u;
    uint vs_cbuf9_7_y = 0u;
    uint vs_cbuf9_7_z = 299520u;
    uint vs_cbuf9_7_w = 0u;
:};
