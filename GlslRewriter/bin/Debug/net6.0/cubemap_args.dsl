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
        vs_attr("d:/UC/cubemap/vs_in.csv", "d:/UC/cubemap/vs_out.csv", 0);
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/cubemap1", "d:/uc/VAO_cubemap");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/cubemap2", "d:/uc/VAO_cubemap");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/cubemap3", "d:/uc/VAO_cubemap");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/cubemap4", "d:/uc/VAO_cubemap");
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
            f4_0_0,
            f4_0_1,
            f4_0_2,
            pf_1_1,
            pf_1_2,
            pf_1_5,
            pf_1_6,
            pf_10_4,
            pf_12_0,
            pf_13_0,
            pf_14_0,
            pf_15_0,
            pf_16_0,
            pf_4_1,
            pf_4_2,
            pf_4_5,
            pf_4_6,
            pf_4_8,
            pf_6_9,
            pf_8_4,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/cubemap/vs_out.csv", 0){
            map_in_attr("out_attr0","in_attr0");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/cubemap/fs_cbuf4.csv", "uvec4"){
            add(1,2);
        };
        uniform("d:/UC/cubemap/fs_cbuf5.csv", "uvec4"){
            add(8,9,16,17,18,19);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/cubemap1", "d:/uc/VAO_cubemap");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/cubemap2", "d:/uc/VAO_cubemap");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/cubemap3", "d:/uc/VAO_cubemap");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/cubemap4", "d:/uc/VAO_cubemap");
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