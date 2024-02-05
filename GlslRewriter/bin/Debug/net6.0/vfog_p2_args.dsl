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
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            f_0_69,
            f_0_70,
            f_1_31,
            f_12_21,
            f_13_6,
            f_13_7,
            f_2_15,
            f4_0_0,
            pf_0_11,
            pf_0_18,
            pf_0_2,
            pf_0_21,
            pf_0_7,
            pf_1_13,
            pf_1_15,
            pf_1_5,
            pf_2_17,
            pf_2_5,
            pf_3_4,
            pf_3_6,
            pf_3_9,
            pf_4_1,
            pf_4_7,
            pf_5_1,
            pf_5_5,
            pf_5_7,
            pf_6_10,
            pf_6_2,
            u_0_1,
            u_0_2,
            u_0_3,
            u_0_4,
            u_0_phi_18,
            u_0_phi_21,
            u_1_1,
            u_1_2,
            u_1_phi_17,
            u_2_1,
            u_2_10,
            u_2_2,
            u_2_4,
            u_2_5,
            u_2_7,
            u_2_8,
            u_2_9,
            u_2_phi_11,
            u_2_phi_16,
            u_2_phi_20,
            u_2_phi_4,
            u_3_2,
            u_3_3,
            u_3_phi_9,
            u_4_0,
            u_4_1,
            u_4_5,
            u_4_6,
            u_4_phi_19,
            u_4_phi_2,
            u_5_2,
            u_5_3,
            u_5_8,
            u_5_9,
            u_5_phi_15,
            u_5_phi_20,
            u_6_1,
            u_6_2,
            u_6_phi_20,
            u_7_0,
            u_8_0,
            u_9_0,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/vfog_p2_1/vs_in.csv", "d:/UC/vfog_p2_1/vs_out.csv", 0);
        uniform("d:/UC/vfog_p2_1/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(30);
        };
        uniform("d:/UC/vfog_p2_1/vs_cbuf9.csv", "uvec4"){
            add(14,15,16,104,105,113,141);
        };
        uniform("d:/UC/vfog_p2_1/vs_cbuf10.csv", "uvec4"){
            add(0,2,3,4,5,6,8,9,10);
        };
        uniform("d:/UC/vfog_p2_1/vs_cbuf15.csv", "uvec4"){
            add(49,51,52);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_vfog_p2_1/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,479);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_vfog_p2_1/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,479);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_vfog_p2_1/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,479);
        };
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_vfog_p2_1/vertex_in_attr7.csv", "[NonSerialized]"){
            add_range(0,479);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/vfog_p2_3", "d:/uc/VAO_vfog_p2_3");
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
        textureSize(tex1,*) = vec2(200,112);
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
            f_1_9,
            f4_0_0,
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
        ps_attr("d:/UC/vfog_p2_1/vs_out.csv", 0){
            map_in_attr("out_attr0","in_attr0");
            map_in_attr("out_attr1","in_attr1");
            map_in_attr("out_attr2","in_attr2");
            map_in_attr("out_attr3","in_attr3");
            map_in_attr("out_attr4","in_attr4");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/vfog_p2_1/fs_cbuf9.csv", "uvec4"){
            add(139);
        };
        uniform("d:/UC/vfog_p2_1/fs_cbuf15.csv", "uvec4"){
            add(33);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/vfog_p2_3", "d:/uc/VAO_vfog_p2_3");
    };
    calculator
    {
        textureSize(tex0,*) = vec2(4,4);
        textureSize(tex1,*) = vec2(200,112);
        textureSize(tex2,*) = vec2(64,64);
        textureSize(tex3,*) = vec2(64,64);
        textureSize(tex4,*) = vec2(64,64);
        textureSize(tex5,*) = vec2(512,512);
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
