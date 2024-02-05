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
        remove_duplicate_expression;

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
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            b_0_13,
            b_0_19,
            b_0_20,
            b_0_phi_31,
            b_1_27,
            b_2_16,
            b_3_3,
            b_3_4,
            b_3_phi_27,
            b_4_1,
            b_4_2,
            b_4_phi_27,
            f_0_146,
            f_0_147,
            f_0_44,
            f_0_56,
            f_0_64,
            f_0_66,
            f_0_67,
            f_0_8,
            f_1_24,
            f_1_41,
            f_1_63,
            f_1_65,
            f_10_24,
            f_10_5,
            f_10_6,
            f_11_0,
            f_2_14,
            f_2_21,
            f_2_25,
            f_2_39,
            f_3_14,
            f_4_8,
            f_5_2,
            f_6_7,
            f_7_19,
            f_7_20,
            f_7_21,
            f_7_4,
            f_7_7,
            f_9_1,
            f4_0_0,
            pf_0_1,
            pf_0_19,
            pf_0_23,
            pf_0_24,
            pf_0_31,
            pf_0_34,
            pf_1_11,
            pf_1_13,
            pf_1_16,
            pf_1_17,
            pf_1_4,
            pf_10_8,
            pf_10_9,
            pf_11_10,
            pf_11_11,
            pf_11_13,
            pf_12_12,
            pf_12_16,
            pf_12_2,
            pf_12_4,
            pf_12_5,
            pf_13_5,
            pf_13_9,
            pf_14_11,
            pf_14_12,
            pf_14_14,
            pf_14_4,
            pf_14_9,
            pf_15_12,
            pf_15_3,
            pf_16_1,
            pf_16_3,
            pf_16_9,
            pf_17_5,
            pf_2_12,
            pf_2_13,
            pf_2_2,
            pf_2_8,
            pf_2_9,
            pf_3_10,
            pf_3_2,
            pf_4_11,
            pf_4_16,
            pf_4_2,
            pf_5_11,
            pf_5_8,
            pf_6_4,
            pf_7_3,
            pf_8_3,
            pf_9_8,
            pf_9_9,
            u_0_1,
            u_0_2,
            u_0_3,
            u_0_4,
            u_0_5,
            u_0_8,
            u_0_9,
            u_0_phi_20,
            u_0_phi_25,
            u_0_phi_29,
            u_1_16,
            u_1_17,
            u_1_19,
            u_1_20,
            u_1_3,
            u_1_5,
            u_1_7,
            u_1_phi_42,
            u_1_phi_49,
            u_10_20,
            u_10_21,
            u_10_4,
            u_10_5,
            u_10_7,
            u_10_8,
            u_10_9,
            u_10_phi_18,
            u_10_phi_21,
            u_10_phi_27,
            u_11_1,
            u_11_2,
            u_11_5,
            u_11_8,
            u_11_9,
            u_11_phi_22,
            u_11_phi_27,
            u_12_0,
            u_12_1,
            u_12_14,
            u_12_16,
            u_12_17,
            u_12_18,
            u_12_19,
            u_12_2,
            u_12_3,
            u_12_6,
            u_12_7,
            u_12_phi_17,
            u_12_phi_19,
            u_12_phi_23,
            u_12_phi_28,
            u_12_phi_31,
            u_13_10,
            u_13_7,
            u_13_8,
            u_13_9,
            u_13_phi_28,
            u_13_phi_31,
            u_14_2,
            u_14_3,
            u_14_4,
            u_14_5,
            u_14_6,
            u_14_phi_28,
            u_14_phi_31,
            u_15_0,
            u_15_1,
            u_15_2,
            u_15_3,
            u_15_phi_28,
            u_15_phi_31,
            u_16_1,
            u_16_2,
            u_16_4,
            u_16_5,
            u_16_phi_31,
            u_16_phi_45,
            u_17_10,
            u_17_11,
            u_17_3,
            u_17_4,
            u_17_5,
            u_17_6,
            u_17_7,
            u_17_8,
            u_17_phi_32,
            u_17_phi_38,
            u_17_phi_40,
            u_17_phi_46,
            u_18_1,
            u_18_2,
            u_18_4,
            u_18_5,
            u_18_7,
            u_18_8,
            u_18_phi_33,
            u_18_phi_39,
            u_18_phi_48,
            u_19_10,
            u_19_2,
            u_19_3,
            u_19_6,
            u_19_7,
            u_19_9,
            u_19_phi_34,
            u_19_phi_41,
            u_19_phi_47,
            u_2_1,
            u_2_2,
            u_2_5,
            u_2_phi_16,
            u_20_0,
            u_20_1,
            u_20_4,
            u_20_5,
            u_20_phi_35,
            u_20_phi_43,
            u_21_0,
            u_21_1,
            u_21_3,
            u_21_4,
            u_21_phi_36,
            u_21_phi_44,
            u_3_1,
            u_3_2,
            u_3_21,
            u_3_23,
            u_3_24,
            u_3_25,
            u_3_28,
            u_3_3,
            u_3_32,
            u_3_33,
            u_3_4,
            u_3_5,
            u_3_6,
            u_3_phi_11,
            u_3_phi_15,
            u_3_phi_24,
            u_3_phi_29,
            u_3_phi_4,
            u_4_15,
            u_4_17,
            u_4_18,
            u_4_19,
            u_4_20,
            u_4_phi_29,
            u_5_0,
            u_5_1,
            u_5_4,
            u_5_5,
            u_5_7,
            u_5_8,
            u_5_phi_2,
            u_5_phi_26,
            u_5_phi_29,
            u_6_10,
            u_6_16,
            u_6_2,
            u_6_21,
            u_6_27,
            u_6_3,
            u_6_32,
            u_6_33,
            u_6_4,
            u_6_5,
            u_6_phi_15,
            u_6_phi_37,
            u_6_phi_9,
            u_7_3,
            u_7_7,
            u_8_12,
            u_8_16,
            u_8_19,
            u_8_21,
            u_8_22,
            u_8_8,
            u_8_phi_27,
            u_9_0,
            u_9_15,
            u_9_21,
            u_9_22,
            u_9_6,
            u_9_phi_27,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/vfog_1/vs_in.csv", "d:/UC/vfog_1/vs_out.csv", 0);
        uniform("d:/UC/vfog_1/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(24,25,26,29,30);
        };
        uniform("d:/UC/vfog_1/vs_cbuf9.csv", "uvec4"){
            add(7,11,12,16,74,75,76,78,104,105,113,114,115,116,121,141,194,195,196,197);
        };
        uniform("d:/UC/vfog_1/vs_cbuf10.csv", "uvec4"){
            add_range(0,3);
        };
        uniform("d:/UC/vfog_1/vs_cbuf12.csv", "uvec4"){
            add_range(0,11);
        };
        uniform("d:/UC/vfog_1/vs_cbuf15.csv", "uvec4"){
            add(49,51,52);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr7.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr8.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,143);
        };
        vao_attr("private Vector4[] in_attr11_array", "vec4", "d:/uc/VAO_vfog1/vertex_in_attr11.csv", "[NonSerialized]"){
            add_range(0,143);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/vfog_11", "d:/uc/VAO_vfog11");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/vfog_3", "d:/uc/VAO_vfog3");
    };
    type_replacement
    {
        vs_cbuf0 = float;
        vs_cbuf8 = float;
        vs_cbuf10 = float;
        vs_cbuf13 = float;
        vs_cbuf15 = float;
        vs_cbuf16 = float;
    };
    function_replacement
    {
        textureLod = textureLodTest(@arg(0), @arg(1), s_sampler);
        vs_ssbo0[*] = vs_ssbo0[@arg(1)];
        vs_cbuf9[*] = vs_cbuf9[@arg(1)];
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
        regex(@"\buni_attr0\b", "v.vertex");
        regex(@"\buni_attr1\b", "v.uv");
        regex(@"\buni_attr2\b", "v.offset");
    };
    calculator
    {
        textureSize(tex0,*) = vec2(4,4);
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
        remove_duplicate_expression;

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
            f_4_1,
            f_5_2,
            f_7_0,
            f4_0_1,
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
        ps_attr("d:/UC/vfog_1/vs_out.csv", 0){
            map_in_attr("out_attr0","in_attr0");
            map_in_attr("out_attr1","in_attr1");
            map_in_attr("out_attr2","in_attr2");
            map_in_attr("out_attr3","in_attr3");
            map_in_attr("out_attr4","in_attr4");
            map_in_attr("out_attr5","in_attr5");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/vfog_1/fs_cbuf9.csv", "uvec4"){
            add(139);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/vfog_11", "d:/uc/VAO_vfog_11");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/vfog_3", "d:/uc/VAO_vfog_3");
    };
    calculator
    {
        textureSize(tex1,*) = vec2(512,512);
        textureSize(depthTex,*) = vec2(800,450);
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        texture(tex5,*) = vec4(0.00020,0.0002,0.0002,0.0002);
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
        remove_duplicate_expression;
    };
};
