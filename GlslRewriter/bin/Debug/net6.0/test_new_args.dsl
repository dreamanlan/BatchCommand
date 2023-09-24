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

        add_utof(1065353216u);

        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt", 9);
            split_on("texture", 3);
            split_on("textureLod", 3);
            split_on("texelFetch", 3);
            split_on("log2", 12);
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
            b_0_2,
            b_0_5,
            b_1_10,
            b_1_11,
            b_1_12,
            b_1_13,
            b_1_14,
            b_1_8,
            b_1_9,
            f_0_12,
            f_1_10,
            f_1_18,
            f_1_51,
            f_1_56,
            f_1_65,
            f_1_70,
            f_11_3,
            f_11_4,
            f_15_12,
            f_15_15,
            f_15_17,
            f_16_12,
            f_16_6,
            f_16_8,
            f_17_0,
            f_17_10,
            f_2_42,
            f_2_50,
            f_2_63,
            f_2_64,
            f_3_32,
            f_3_42,
            f_3_44,
            f_3_49,
            f_3_57,
            f_3_61,
            f_4_28,
            f_4_43,
            f_4_45,
            f_4_46,
            f_4_52,
            f_4_57,
            f_4_58,
            f_4_70,
            f_4_79,
            f_4_8,
            f_4_81,
            f_5_13,
            f_5_18,
            f_5_21,
            f_5_25,
            f_5_28,
            f_5_29,
            f_6_12,
            f_6_7,
            f_8_1,
            f_9_2,
            f2_0_1,
            f2_0_2,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f4_0_5,
            pf_0_1,
            pf_0_11,
            pf_0_12,
            pf_0_25,
            pf_0_29,
            pf_0_3,
            pf_0_31,
            pf_0_33,
            pf_0_34,
            pf_0_4,
            pf_1_11,
            pf_1_15,
            pf_1_16,
            pf_1_24,
            pf_1_25,
            pf_1_26,
            pf_1_4,
            pf_1_5,
            pf_1_6,
            pf_1_8,
            pf_1_9,
            pf_10_1,
            pf_10_10,
            pf_10_14,
            pf_10_17,
            pf_10_18,
            pf_10_4,
            pf_10_6,
            pf_10_7,
            pf_10_9,
            pf_11_10,
            pf_11_13,
            pf_11_15,
            pf_11_3,
            pf_11_4,
            pf_11_5,
            pf_11_8,
            pf_11_9,
            pf_12_2,
            pf_12_6,
            pf_12_8,
            pf_13_1,
            pf_13_10,
            pf_13_11,
            pf_13_15,
            pf_13_5,
            pf_13_6,
            pf_13_7,
            pf_14_13,
            pf_14_2,
            pf_14_4,
            pf_14_6,
            pf_14_7,
            pf_14_9,
            pf_15_11,
            pf_15_14,
            pf_15_2,
            pf_15_20,
            pf_15_6,
            pf_15_8,
            pf_16_10,
            pf_16_2,
            pf_16_3,
            pf_16_4,
            pf_17_3,
            pf_17_5,
            pf_17_6,
            pf_17_9,
            pf_18_1,
            pf_18_10,
            pf_18_2,
            pf_18_6,
            pf_18_9,
            pf_19_3,
            pf_19_6,
            pf_19_7,
            pf_2_0,
            pf_2_3,
            pf_2_4,
            pf_2_5,
            pf_20_0,
            pf_20_11,
            pf_20_17,
            pf_20_19,
            pf_20_5,
            pf_20_6,
            pf_20_9,
            pf_21_7,
            pf_22_0,
            pf_23_0,
            pf_23_6,
            pf_24_1,
            pf_25_0,
            pf_25_1,
            pf_25_3,
            pf_26_1,
            pf_3_11,
            pf_3_12,
            pf_3_2,
            pf_3_4,
            pf_4_10,
            pf_4_11,
            pf_4_13,
            pf_4_2,
            pf_4_21,
            pf_4_4,
            pf_4_6,
            pf_5_3,
            pf_5_5,
            pf_5_6,
            pf_6_11,
            pf_6_19,
            pf_6_4,
            pf_6_6,
            pf_6_7,
            pf_6_8,
            pf_7_0,
            pf_7_11,
            pf_7_12,
            pf_7_13,
            pf_7_15,
            pf_7_17,
            pf_7_19,
            pf_7_27,
            pf_7_5,
            pf_7_6,
            pf_7_7,
            pf_8_0,
            pf_8_10,
            pf_8_2,
            pf_8_4,
            pf_8_5,
            pf_8_6,
            pf_8_8,
            pf_9_11,
            pf_9_13,
            pf_9_14,
            pf_9_15,
            pf_9_17,
            pf_9_2,
            pf_9_21,
            pf_9_3,
            pf_9_4,
            pf_9_5,
            pf_9_9,
            u_0_1,
            u_0_11,
            u_0_12,
            u_0_13,
            u_0_2,
            u_0_4,
            u_0_5,
            u_0_6,
            u_0_7,
            u_0_phi_11,
            u_0_phi_15,
            u_0_phi_4,
            u_1_1,
            u_1_3,
            u_1_4,
            u_1_phi_16,
            u_2_0,
            u_2_1,
            u_2_5,
            u_2_6,
            u_2_phi_2,
            u_3_2,
            u_3_3,
            u_3_4,
            u_3_5,
            u_3_7,
            u_3_phi_15,
            u_3_phi_9,
            u_4_2,
            u2_0_0,
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
        textureSize(tex0,*) = vec2(8,13);
        textureSize(tex1,*) = vec2(256,256);
        textureSize(tex2,*) = vec2(256,256);
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = rand_color();//vec4(0.5,0.5,0.5,1.0);
        textureLod(*,*,*) = rand_color();//vec4(0.5,0.5,0.5,1.0);
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
            split_on("log2", 12);
        };

        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            b_0_0,
            f_0_6,
            f_1_9,
            f_2_16,
            f_2_7,
            f_3_11,
            f_3_20,
            f_4_3,
            f_4_8,
            f_5_2,
            f_6_1,
            f_8_2,
            f_9_2,
            f_9_3,
            f2_0_2,
            f2_0_3,
            f3_0_0,
            f4_0_1,
            f4_0_3,
            f4_0_4,
            pf_0_11,
            pf_0_12,
            pf_0_13,
            pf_0_16,
            pf_0_18,
            pf_0_20,
            pf_0_23,
            pf_0_24,
            pf_0_6,
            pf_0_8,
            pf_1_11,
            pf_1_19,
            pf_1_23,
            pf_1_26,
            pf_1_27,
            pf_1_5,
            pf_1_8,
            pf_2_11,
            pf_2_12,
            pf_2_3,
            pf_2_6,
            pf_2_7,
            pf_2_8,
            pf_3_0,
            pf_3_11,
            pf_3_3,
            pf_3_5,
            pf_3_7,
            pf_4_0,
            pf_4_2,
            pf_4_3,
            pf_4_8,
            pf_5_1,
            pf_5_2,
            pf_5_7,
            pf_6_0,
            pf_6_1,
            pf_6_2,
            u_0_0,
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
vs_code_block
{:
#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable

out gl_PerVertex {
    vec4 gl_Position;
};
layout(location = 0) in vec4 in_attr0; //顶点坐标（位于xy平面，z始终为0，w为1）
layout(location = 1) in vec4 in_attr1; //UV（xy是UV，z是从6点开始逆时针方向的顶点序号，中心点为12，w为1）
layout(location = 4) in vec4 in_attr4; //固定值 277.38879	-253.98657	-1225.84583	1000 （xyzw全用）
layout(location = 5) in vec4 in_attr5; //固定值 0	0	0	1060.5 （只用w）
layout(location = 6) in vec4 in_attr6; //固定值 256.74911	175.44524	175.44524	1 (用xyz)
layout(location = 7) in vec4 in_attr7; //固定值 0.33394	0.62269	0.01691	0.7787 （xyzw全用）
layout(location = 9) in vec4 in_attr9; //固定值 1	0	0	287.3111 （xyzw全用）
layout(location = 10) in vec4 in_attr10; //固定值 0	1	0	1522.14368 （xyzw全用）
layout(location = 11) in vec4 in_attr11; //固定值 0	0	1	1396.66956 （xyzw全用）

layout(location = 0) out vec4 out_attr0; //只写w，用作系数，参与输出颜色与alpha的计算
layout(location = 1) out vec4 out_attr1; //xyzw全写，zw是大的云纹理采样坐标，xy是小的纹理数组的采样坐标
layout(location = 2) out vec4 out_attr2; //xyzw全写，xy/w是深度纹理的采样坐标，z/w参与输出alpha的计算
layout(location = 3) out vec4 out_attr3; //只写x，用作最终alpha值的乘数
layout(location = 4) out vec4 out_attr4; //写xyz，是一个坐标，用于与一个固定点计算单位向量
layout(location = 5) out vec4 out_attr5; //只写y，参与计算输出颜色一部分的系数的指数
layout(location = 6) out vec4 out_attr6; //只写x，用作小的纹理数组的下标
layout(location = 7) out vec4 out_attr7; //写xy，x与y分别用作系数
layout(location = 8) out vec4 out_attr8; //写xz，x参与颜色分量的计算，z参与颜色分量与alpha的计算
layout(location = 9) out vec4 out_attr9; //xyzw全写，xyz是颜色，w用作混合系数
layout(location = 10) out vec4 out_attr10; //xyzw全写，xyz是颜色，w用作混合系数
layout(location = 11) out vec4 out_attr11; //xyzw全写，xyz是颜色，w用作混合系数

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

layout(binding = 0) uniform sampler2D tex0; //8*13
layout(binding = 1) uniform sampler2D tex1; //256*256
layout(binding = 2) uniform sampler2D tex2; //256*256

#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat

:};
ps_code_block("global")
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
layout(binding = 6) uniform sampler2D depthTex; //800*450

#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
:};