vs
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_multiline;
        def_expanded_only_once;
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

        auto_split(5){
            split_on("exp2", 3);
            split_on("inversesqrt", 3);
            split_on("texture", 1);
            split_on("textureLod", 1);
            split_on("texelFetch", 1);
            split_on("texelQueryLod", 1);
            split_on("texelSize", 1);
            split_on("textureGather", 1);
            split_on("log2", 3);
        };

        variable_assignment
        {
            u_2_phi_2 = 0u;
            u_0_phi_4 = 0u;
            u_3_phi_9 = 1149538304u;
            u_0_phi_11 = 1149538304u;
            u_0_phi_15 = 1141932032u;
            u_3_phi_15 = 1148846080u;
            u_1_phi_16 = 1058273231u;
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
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            b_1_1,
            f_0_2,
            pf_0_0,
            pf_0_1,
            pf_0_4,
            u_0_1,
            u_0_2,
            u_0_3,
            u_0_phi_2,
            u_1_0,
            u_1_1,
            u_1_phi_1,
            u_2_0,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/lighttex/vs_in.csv", "d:/UC/lighttex/vs_out.csv", 0);
        uniform("d:/UC/lighttex/vs_cbuf3.csv", "uvec4"){
            add(2,8);
        };
        uniform("d:/UC/lighttex/vs_cbuf4.csv", "uvec4"){
            add(3,7);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/lighttex1", "d:/uc/VAO_lighttex1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/lighttex2", "d:/uc/VAO_lighttex2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/lighttex3", "d:/uc/VAO_lighttex3");
    };
    type_replacement
    {
        vs_cbuf3 = float;
        vs_cbuf4 = float;
    };
    function_replacement
    {
        (*)[*] = @join(@arg(0), _, @arg(1));
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
        regex(@"\buni_attr0\b", "v.vertex");
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
        def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 2048;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;
        remove_duplicate_expression;

        auto_split(5){
            split_on("exp2", 3);
            split_on("inversesqrt", 3);
            split_on("texture", 1);
            split_on("textureLod", 1);
            split_on("texelFetch", 1);
            split_on("texelQueryLod", 1);
            split_on("texelSize", 1);
            split_on("textureGather", 1);
            split_on("log2", 3);
        };

        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            b_0_0,
            b_0_1,
            b_1_0,
            b_1_1,
            b_3_11,
            b_3_3,
            b_3_4,
            b_3_7,
            b_4_2,
            b_4_3,
            f_0_16,
            f_0_17,
            f_0_2,
            f_1_14,
            f_1_15,
            f_1_18,
            f_1_27,
            f_1_28,
            f_1_35,
            f_1_38,
            f_1_46,
            f_1_49,
            f_1_5,
            f_10_0,
            f_11_0,
            f_12_0,
            f_13_0,
            f_2_12,
            f_2_14,
            f_2_15,
            f_2_5,
            f_3_1,
            f_3_10,
            f_3_6,
            f_4_14,
            f_4_16,
            f_4_17,
            f_4_18,
            f_5_5,
            f_6_2,
            f_8_6,
            f_9_0,
            f_9_1,
            f3_0_0,
            f3_0_1,
            f3_0_2,
            f3_0_3,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            pf_0_1,
            pf_0_14,
            pf_0_15,
            pf_0_17,
            pf_0_4,
            pf_0_5,
            pf_0_7,
            pf_0_9,
            pf_1_0,
            pf_1_1,
            pf_1_10,
            pf_1_11,
            pf_1_14,
            pf_1_3,
            pf_1_8,
            pf_10_3,
            pf_11_1,
            pf_12_0,
            pf_13_0,
            pf_13_1,
            pf_13_2,
            pf_14_0,
            pf_14_1,
            pf_2_11,
            pf_2_12,
            pf_2_14,
            pf_2_16,
            pf_2_17,
            pf_2_2,
            pf_2_20,
            pf_2_22,
            pf_2_3,
            pf_2_4,
            pf_2_6,
            pf_3_0,
            pf_3_5,
            pf_3_8,
            pf_3_9,
            pf_4_13,
            pf_4_3,
            pf_4_5,
            pf_4_6,
            pf_5_0,
            pf_5_1,
            pf_5_16,
            pf_5_17,
            pf_5_18,
            pf_5_2,
            pf_5_21,
            pf_5_24,
            pf_5_30,
            pf_5_31,
            pf_5_4,
            pf_5_6,
            pf_7_0,
            pf_7_1,
            pf_7_2,
            pf_8_1,
            pf_8_12,
            pf_8_14,
            pf_8_2,
            pf_8_3,
            pf_8_4,
            pf_8_5,
            pf_8_6,
            pf_9_1,
            pf_9_2,
            pf_9_3,
            u_0_1,
            u_0_2,
            u_0_4,
            u_0_5,
            u_0_6,
            u_0_phi_13,
            u_0_phi_9,
            u_1_1,
            u_1_2,
            u_1_3,
            u_1_4,
            u_1_5,
            u_1_phi_21,
            u_1_phi_25,
            u_2_1,
            u_2_2,
            u_2_4,
            u_2_5,
            u_2_6,
            u_2_7,
            u_2_8,
            u_2_9,
            u_2_phi_16,
            u_2_phi_19,
            u_2_phi_23,
            u_2_phi_24,
            u_3_1,
            u_3_2,
            u_3_4,
            u_3_5,
            u_3_6,
            u_3_7,
            u_3_8,
            u_3_9,
            u_3_phi_17,
            u_3_phi_20,
            u_3_phi_22,
            u_4_2,
            u_4_3,
            u_4_4,
            u_4_5,
            u_4_6,
            u_4_7,
            u_4_phi_12,
            u_4_phi_15,
            u_4_phi_5,
            u_5_1,
            u_5_10,
            u_5_13,
            u_5_14,
            u_5_2,
            u_5_3,
            u_5_4,
            u_5_5,
            u_5_6,
            u_5_7,
            u_5_8,
            u_5_9,
            u_5_phi_10,
            u_5_phi_18,
            u_5_phi_4,
            u_5_phi_6,
            u_5_phi_8,
            u_6_0,
            u_6_1,
            u_6_3,
            u_6_4,
            u_6_phi_1,
            u_6_phi_11,
            u_7_1,
            u_7_2,
            u_7_3,
            u_7_4,
            u_7_5,
            u_7_phi_2,
            u_7_phi_7,
            u_8_1,
            u_8_2,
            u_8_phi_3,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/lighttex/vs_out.csv", 0){
            map_in_attr("out_attr0","in_attr0");
            map_in_attr("out_attr1","in_attr1");
            map_in_attr("out_attr2","in_attr2");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/lighttex/fs_cbuf3.csv", "uvec4"){
            add(2,4,5,6,7,8);
        };
        uniform("d:/UC/lighttex/fs_cbuf4.csv", "uvec4"){
            add(0,1,2,3,7);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/lighttex1", "d:/uc/VAO_lighttex1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/lighttex2", "d:/uc/VAO_lighttex2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/lighttex3", "d:/uc/VAO_lighttex3");
    };
    type_replacement
    {
        fs_cbuf3 = float;
        fs_cbuf4 = float;
    };
    function_replacement
    {
        texture(*,*) = textureSample(@arg(0), @arg(1), s_linear_clamp_sampler);
        (*)[*] = @join(@arg(0), _, @arg(1));
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
        textureGather(*,*,*) = vec4(0.5,0.5,0.5,0.75);
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