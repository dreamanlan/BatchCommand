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
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/simple_light_smoke2/vs_in.csv", "d:/UC/simple_light_smoke2/vs_out.csv", 0);
        uniform("d:/UC/simple_light_smoke2/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/simple_light_smoke2/vs_cbuf8.csv", "uvec4"){
            add(10,11,28,29,30);
            add_range(0,7);
        };
        uniform("d:/UC/simple_light_smoke2/vs_cbuf9.csv", "uvec4"){
            add(194,195,196,197);
            add_range(0, 160);
        };
        uniform("d:/UC/simple_light_smoke2/vs_cbuf10.csv", "uvec4"){
            add_range(0,6);
        };
        uniform("d:/UC/simple_light_smoke2/vs_cbuf13.csv", "uvec4"){
            add(0,1,2,3,5,6);
        };
        uniform("d:/UC/simple_light_smoke2/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        uniform("d:/UC/simple_light_smoke2/vs_cbuf16.csv", "uvec4"){
            add(0,1);
        };
        vao_attr("private Vector4[] in_attr3_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr3.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr8_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr8.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,101);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_simple_light_smoke2/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,101);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/simple_light_smoke2/1", "d:/uc/VAO_simple_light_smoke2/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/simple_light_smoke2/2", "d:/uc/VAO_simple_light_smoke2/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/simple_light_smoke2/3", "d:/uc/VAO_simple_light_smoke2/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/simple_light_smoke2/4", "d:/uc/VAO_simple_light_smoke2/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/simple_light_smoke2/5", "d:/uc/VAO_simple_light_smoke2/5");
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
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/simple_light_smoke2/vs_out.csv", 0){
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
        uniform("d:/UC/simple_light_smoke2/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/simple_light_smoke2/fs_cbuf9.csv", "uvec4"){
            add(19,139,140);
        };
        uniform("d:/UC/simple_light_smoke2/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/simple_light_smoke2/fs_cbuf15.csv", "uvec4"){
            add(1,25,26,39);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/simple_light_smoke2/1", "d:/uc/VAO_simple_light_smoke2/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/simple_light_smoke2/2", "d:/uc/VAO_simple_light_smoke2/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/simple_light_smoke2/3", "d:/uc/VAO_simple_light_smoke2/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/simple_light_smoke2/4", "d:/uc/VAO_simple_light_smoke2/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/simple_light_smoke2/5", "d:/uc/VAO_simple_light_smoke2/5");
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
