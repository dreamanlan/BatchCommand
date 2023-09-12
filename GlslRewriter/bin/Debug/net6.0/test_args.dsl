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

        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt");
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
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
        };
    };
    vs_attr("d:/UC/cloud_1/vs_in.csv", "d:/UC/cloud_1/vs_out.csv", 0);
    uniform("d:/UC/cloud_1/vs_cbuf8.csv", "uvec4"){
        add_range(0,7);
        add(29,30);
    };
    uniform("d:/UC/cloud_1/vs_cbuf9.csv", "uvec4"){
        add(11,12,16,141);
        add_range(113,116);
    };
    uniform("d:/UC/cloud_1/vs_cbuf10.csv", "uvec4"){
        add(0,2,3);
    };
    uniform("d:/UC/cloud_1/vs_cbuf13.csv", "uvec4"){
        add(6);
    };
    uniform("d:/UC/cloud_1/vs_cbuf15.csv", "uvec4"){
        add(1,54,55,57,60,61);
        add_range(22,28);
    };
    calculator
    {
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,1.0);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,1.0);
    };
    code_block
    {:
#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable

out gl_PerVertex {
    vec4 gl_Position;
};
layout(location = 0) in vec4 in_vertex; //顶点坐标（位于xy平面，z始终为0，w为1）
layout(location = 1) in vec4 in_texcoord; //UV（xy是UV，z是从6点开始逆时针方向的顶点序号，中心点为12，w为1）
layout(location = 4) in vec4 in_attr4; //固定值 277.38879	-253.98657	-1225.84583	1000 （xyzw全用）
layout(location = 5) in vec4 in_attr5; //固定值 0	0	0	1060.5 （只用w）
layout(location = 6) in vec4 in_attr6; //固定值 256.74911	175.44524	175.44524	1 (用xyz)
layout(location = 7) in vec4 in_attr7; //固定值 0.33394	0.62269	0.01691	0.7787 （xyzw全用）
layout(location = 9) in vec4 in_attr9; //固定值 1	0	0	287.3111 （xyzw全用）
layout(location = 10) in vec4 in_attr10; //固定值 0	1	0	1522.14368 （xyzw全用）
layout(location = 11) in vec4 in_attr11; //固定值 0	0	1	1396.66956 （xyzw全用）

layout(location = 0) out vec4 out_attr0; //只写w
layout(location = 1) out vec4 out_attr1; //xyzw全写
layout(location = 2) out vec4 out_attr2; //xyzw全写
layout(location = 3) out vec4 out_attr3; //只写x
layout(location = 4) out vec4 out_attr4; //写xyz
layout(location = 5) out vec4 out_attr5; //只写y
layout(location = 6) out vec4 out_attr6; //只写x
layout(location = 7) out vec4 out_attr7; //写xy
layout(location = 8) out vec4 out_attr8; //写xz
layout(location = 9) out vec4 out_attr9; //xyzw全写
layout(location = 10) out vec4 out_attr10; //xyzw全写
layout(location = 11) out vec4 out_attr11; //xyzw全写

layout(std140, binding = 0) uniform vs_cbuf_8 {
    uvec4 vs_cbuf8[4096];
};
layout(std140, binding = 1) uniform vs_cbuf_9 {
    uvec4 vs_cbuf9[4096];
};
layout(std140, binding = 2) uniform vs_cbuf_10 {
    uvec4 vs_cbuf10[4096];
};
layout(std140, binding = 3) uniform vs_cbuf_13 {
    uvec4 vs_cbuf13[4096];
};
layout(std140, binding = 4) uniform vs_cbuf_15 {
    uvec4 vs_cbuf15[4096];
};

layout(binding = 0) uniform sampler2D tex0; //256*13
layout(binding = 1) uniform sampler2D tex1; //256*256
layout(binding = 2) uniform sampler2D tex2; //256*256

#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat

    :};
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

        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            f_0_12,
            f_2_16,
            f_2_11,
            f_2_16,
            f_3_11,
            f_3_22,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f_4_8,
            f_5_2,
            f_5_3,
            f_5_4,
            pf_0_15,
            pf_0_19,
            pf_0_20,
            pf_0_23,
            pf_0_24,
            pf_0_25,
            pf_1_23,
            pf_1_26,
            pf_1_27,
            pf_1_28,
            pf_2_11,
            pf_2_12,
            pf_2_13,
            pf_2_5,
            pf_2_7,
            pf_2_8,
            pf_3_2,
            pf_3_3,
            pf_3_5,
            pf_3_7,
            pf_4_4,
            pf_5_2,
            pf_6_1,
        };
    };
    ps_attr("d:/UC/cloud_1/vs_out.csv", 0){
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
        remove_in_attr("gl_Position");
    };
    uniform("d:/UC/cloud_1/fs_cbuf8.csv", "uvec4"){
        add(29,30);
    };
    uniform("d:/UC/cloud_1/fs_cbuf9.csv", "uvec4"){
        add(139,140,189,190);
    };
    uniform("d:/UC/cloud_1/fs_cbuf15.csv", "uvec4"){
        add(1,25,26,28,42,43,44,57);
    };
    calculator
    {
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        texture(*,*) = vec4(0.5,0.5,0.5,0.75);
        textureQueryLod(*,*) = vec2(4,1);
    };
    code_block("global")
    {:
#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec4 in_attr0;
layout(location = 1) in vec4 in_attr1;
layout(location = 2) in vec4 in_attr2;
layout(location = 3) in vec4 in_attr3;
layout(location = 4) in vec4 in_attr4;
layout(location = 5) flat in vec4 in_attr5;
layout(location = 6) flat in vec4 in_attr6;
layout(location = 7) in vec4 in_attr7;
layout(location = 8) in vec4 in_attr8;
layout(location = 9) in vec4 in_attr9;
layout(location = 10) in vec4 in_attr10;
layout(location = 11) in vec4 in_attr11;

layout(location = 0) out vec4 frag_color0;

layout(std140, binding = 5) uniform fs_cbuf_8 {
    uvec4 fs_cbuf8[4096];
};
layout(std140, binding = 6) uniform fs_cbuf_9 {
    uvec4 fs_cbuf9[4096];
};
layout(std140, binding = 7) uniform fs_cbuf_15 {
    uvec4 fs_cbuf15[4096];
};

layout(binding = 3) uniform sampler2D tex3; //64*64
layout(binding = 4) uniform sampler2DArray tex4; //64*64
layout(binding = 5) uniform sampler2D tex5; //512*512
layout(binding = 6) uniform sampler2D depthTex;

#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
    :};
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