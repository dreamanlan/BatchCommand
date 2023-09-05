#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec4 in_attr0; //(只用w)
layout(location = 1) in vec4 in_attr1; //(xyzw全用)
layout(location = 2) in vec4 in_attr2; //(xyzw全用)
layout(location = 3) in vec4 in_attr3; //(只用x)
layout(location = 4) in vec4 in_attr4; //(用xyz)
layout(location = 5) flat in vec4 in_attr5; //(只用y)
layout(location = 6) flat in vec4 in_attr6; //(只用x)
layout(location = 7) in vec4 in_attr7; //(用xy)
layout(location = 8) in vec4 in_attr8; //(用xz)
layout(location = 9) in vec4 in_attr9; //(xyzw全用)
layout(location = 10) in vec4 in_attr10; //(xyzw全用)
layout(location = 11) in vec4 in_attr11; //(xyzw全用)

layout(location = 0) out vec4 frag_color0;

layout(std140, binding = 5) uniform fs_cbuf_8 {
    uvec4 fs_cbuf8[4096];
};
//fs_cbuf8[29] 1133488083, 1154548889, 1152292204, 0        464 uint4
//fs_cbuf8[30] 1065353216, 1187205120, 1187205120, 1187204608 480 uint4

layout(std140, binding = 6) uniform fs_cbuf_9 {
    uvec4 fs_cbuf9[4096];
};
//fs_cbuf9[139] 1065353216, 0, 0, 0 2224 uint4
//fs_cbuf9[140] 1077936128, 1084227584, 0, 0 2240 uint4
//fs_cbuf9[189] 1024416809, 1080033280, 1065353216, 1092616192 3024 uint4
//fs_cbuf9[190] 1061158912, 1075838976, 1101004800, 1082130432 3040 uint4

layout(std140, binding = 7) uniform fs_cbuf_15 {
    uvec4 fs_cbuf15[4096];
};
//fs_cbuf15[1] 0, 1065353216, 1072865060, 1065353216 16 uint4
//fs_cbuf15[25] 1060018062, 1065194672, 1059307546, 1039516303 400 uint4
//fs_cbuf15[26] 1066362785, 1067991434, 1059750465, 1055353662 416 uint4
//fs_cbuf15[28] 1057344769, 3205691469, 3206633754, 0 448 uint4
//fs_cbuf15[42] 1082969293, 1079863863, 1076417135, 1059481190 672 uint4
//fs_cbuf15[43] 1065353216, 1063423836, 1059481190, 1065353216 688 uint4
//fs_cbuf15[44] 1063675494, 1061578342, 1058222899, 1065353216 704 uint4
//fs_cbuf15[57] 3314801541, 1147334299, 1161527296, 1065353216 912 uint4

layout(binding = 3) uniform sampler2D tex3; //64*64
layout(binding = 4) uniform sampler2DArray tex4; //64*64
layout(binding = 5) uniform sampler2D tex5; //512*512
layout(binding = 6) uniform sampler2D depthTex;

#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat

void main() {
    bool b_0 = bool(0);
    bool b_1 = bool(0);
    uint u_0 = uint(0);
    uint u_1 = uint(0);
    float f_0 = float(0);
    float f_1 = float(0);
    float f_2 = float(0);
    float f_3 = float(0);
    float f_4 = float(0);
    float f_5 = float(0);
    float f_6 = float(0);
    float f_7 = float(0);
    float f_8 = float(0);
    float f_9 = float(0);
    vec2 f2_0 = vec2(0);
    vec3 f3_0 = vec3(0);
    vec4 f4_0 = vec4(0);

    precise float pf_0 = float(0);
    precise float pf_1 = float(0);
    precise float pf_2 = float(0);
    precise float pf_3 = float(0);
    precise float pf_4 = float(0);
    precise float pf_5 = float(0);
    precise float pf_6 = float(0);
    precise float pf_7 = float(0);
    precise float pf_8 = float(0);

    f_0 = in_attr1.z;
    f_1 = in_attr1.w;
    f2_0 = vec2(f_0, f_1);
    f4_0 = vec4(textureQueryLod(tex5, f2_0), 0.0, 0.0);
    f_2 = f4_0.y;

    u_0 = uint(f_2);
    u_0 = u_0 << 8u;
    f_2 = float(u_0);
    pf_0 = f_2 / 256.0;

    f_2 = min(pf_0, 2.);
    f2_0 = vec2(f_0, f_1);
    f4_0 = textureLod(tex5, f2_0, f_2);
    f_0 = f4_0.x;
    f_1 = f4_0.w;

    f_2 = in_attr1.x;
    f_3 = in_attr1.y;
    f_4 = utof(fs_cbuf9[189].x); //1024416809, 1080033280, 1065353216, 1092616192
    pf_0 = f_0 * f_4;
    pf_1 = f_1 * f_4;
    f_0 = 0. - (f_4);
    pf_0 = fma(pf_0, 2., f_0);
    pf_1 = fma(pf_1, 2., f_0);
    pf_0 = pf_0 + f_2;
    pf_1 = pf_1 + f_3;
    f2_0 = vec2(pf_0, pf_1);

    f4_0 = vec4(textureQueryLod(tex3, f2_0), 0.0, 0.0);
    f_0 = f4_0.y;

    u_0 = uint(f_0);
    u_0 = u_0 << 8u;

    f_0 = in_attr2.w;
    f_1 = gl_FragCoord.w;
    f_0 = f_0 * f_1;
    f_1 = in_attr6.x;
    f_0 = (1.0) / f_0;
    f_1 = roundEven(f_1);
    f_1 = min(max(f_1, float(0.)), float(65535.));
    u_1 = uint(f_1);
    f_1 = in_attr2.x;
    f_2 = gl_FragCoord.w;
    f_1 = f_1 * f_2;
    f_1 = f_1 * f_0;
    f_2 = in_attr2.y;
    f_3 = gl_FragCoord.w;
    f_2 = f_2 * f_3;
    f_0 = f_2 * f_0;

    f_2 = float(u_0);
    pf_2 = f_2 / 256.0;

    f_2 = min(pf_2, 2.);
    f2_0 = vec2(f_1, f_0);
    f4_0 = texture(depthTex, f2_0);
    f_0 = f4_0.x;

    f_1 = float(u_1 & 0xffff);
    f3_0 = vec3(pf_0, pf_1, f_1);
    f4_0 = textureLod(tex4, f3_0, f_2);
    f_1 = f4_0.x;
    f_2 = f4_0.w;

    f_3 = in_attr2.w;
    f_4 = in_attr2.z;
    f_5 = in_attr8.z;
    f_6 = in_attr0.w;
    f_3 = (1.0) / f_3;
    f_7 = in_attr4.z;
    f_8 = utof(fs_cbuf9[140].y); //1077936128, 1084227584, 0, 0
    f_8 = (1.0) / f_8;
    pf_0 = f_4 * f_3;
    f_3 = utof(fs_cbuf8[30].y); //1065353216, 1187205120, 1187205120, 1187204608
    f_4 = utof(fs_cbuf8[30].w); //1065353216, 1187205120, 1187205120, 1187204608
    f_3 = 0. - (f_3);
    pf_0 = fma(pf_0, f_4, f_3);
    f_3 = in_attr3.x;
    f_4 = (1.0) / pf_0;
    f_9 = utof(fs_cbuf8[29].z); //1133488083, 1154548889, 1152292204, 0
    f_7 = 0. - (f_7);
    pf_0 = f_7 + f_9;
    f_7 = utof(fs_cbuf8[30].x); //1065353216, 1187205120, 1187205120, 1187204608
    f_9 = utof(fs_cbuf8[30].w); //1065353216, 1187205120, 1187205120, 1187204608
    pf_1 = fma(f_0, f_9, f_7);
    f_0 = abs(f_1);
    f_0 = log2(f_0);
    pf_2 = f_2 * f_5;
    f_2 = utof(fs_cbuf8[30].z); //1065353216, 1187205120, 1187205120, 1187204608
    pf_1 = fma(f_4, f_2, pf_1);
    pf_2 = pf_2 * f_6;
    pf_1 = pf_1 * f_8;
    f_2 = min(max(pf_1, 0.0), 1.0);
    f_4 = in_attr4.y;
    f_5 = 0. - (0.);
    pf_1 = pf_2 + f_5;
    f_5 = min(max(pf_1, 0.0), 1.0);
    pf_1 = f_2 * f_5;
    f_2 = in_attr4.x;
    pf_1 = pf_1 * f_3;
    f_3 = utof(fs_cbuf9[139].z); //1065353216, 0, 0, 0
    b_0 = pf_1 <= f_3 && !isnan(pf_1) && !isnan(f_3);
    b_1 = b_0 ? true : false;
    //if(b_1) {
    //    discard;
    //}

    f_3 = utof(fs_cbuf8[29].x); //1133488083, 1154548889, 1152292204, 0
    f_2 = 0. - (f_2);
    pf_3 = f_2 + f_3;
    f_2 = utof(fs_cbuf8[29].y); //1133488083, 1154548889, 1152292204, 0
    f_3 = 0. - (f_4);
    pf_4 = f_3 + f_2;
    f_2 = 0. - (0.);
    pf_1 = pf_1 + f_2;
    f_2 = min(max(pf_1, 0.0), 1.0);
    pf_1 = pf_3 * pf_3;
    f_3 = utof(fs_cbuf9[190].w); //1061158912, 1075838976, 1101004800, 1082130432
    f_4 = utof(fs_cbuf9[190].z); //1061158912, 1075838976, 1101004800, 1082130432
    f_4 = 0. - (f_4);
    pf_5 = f_4 + f_3;
    pf_1 = fma(pf_4, pf_4, pf_1);
    pf_1 = fma(pf_0, pf_0, pf_1);
    f_3 = inversesqrt(pf_1);
    pf_1 = pf_3 * f_3;
    pf_3 = pf_4 * f_3;
    pf_0 = pf_0 * f_3;
    f_3 = utof(fs_cbuf15[28].x); //1057344769, 3205691469, 3206633754, 0
    pf_1 = pf_1 * f_3;
    f_3 = utof(fs_cbuf15[28].y); //1057344769, 3205691469, 3206633754, 0
    pf_1 = fma(pf_3, f_3, pf_1);
    f_3 = utof(fs_cbuf15[28].z); //1057344769, 3205691469, 3206633754, 0
    pf_0 = fma(pf_0, f_3, pf_1);
    f_3 = in_attr5.y;
    f_4 = 0. - (1.);
    pf_0 = fma(pf_0, 2., f_4);
    f_4 = min(max(pf_0, 0.0), 1.0);
    pf_0 = f_4 * f_3;
    pf_0 = pf_0 * 0.005;
    f_3 = in_attr8.x;
    pf_1 = pf_0 + pf_2;
    f_5 = (1.0) / pf_0;
    f_6 = 0. - (f_5);
    pf_0 = fma(f_5, pf_1, f_6);
    f_5 = min(max(pf_0, 0.0), 1.0);
    f_6 = 0. - (f_4);
    pf_0 = f_6 + 1.;
    f_6 = 0. - (f_5);
    pf_0 = fma(pf_0, f_6, pf_0);
    pf_0 = f_5 + pf_0;
    f_5 = utof(fs_cbuf9[190].y); //1061158912, 1075838976, 1101004800, 1082130432
    f_6 = utof(fs_cbuf9[189].y); //1024416809, 1080033280, 1065353216, 1092616192
    f_6 = 0. - (f_6);
    pf_1 = f_6 + f_5;
    pf_2 = pf_0 * -5.;
    f_5 = utof(fs_cbuf9[189].y); //1024416809, 1080033280, 1065353216, 1092616192
    pf_1 = fma(pf_1, f_3, f_5);
    f_5 = utof(fs_cbuf9[190].z); //1061158912, 1075838976, 1101004800, 1082130432
    pf_3 = fma(f_3, pf_5, f_5);
    pf_1 = pf_1 * f_0;
    f_0 = exp2(pf_2);
    f_3 = min(f_1, 0.3);
    f_3 = max(f_3, 0.1);
    pf_2 = f_0 + -0.03125;
    pf_4 = f_3 + -0.1;
    pf_0 = f_4 * pf_0;
    f_0 = exp2(pf_1);
    f_3 = 0. - (f_4);
    pf_1 = f_3 + 1.05;
    f_3 = max(0., pf_2);
    pf_2 = fma(pf_4, 3.3499994, 0.33);
    pf_0 = fma(pf_0, f_3, 1.);
    f_3 = utof(fs_cbuf15[43].y); //1065353216, 1063423836, 1059481190, 1065353216
    f_4 = utof(fs_cbuf15[44].y); //1063675494, 1061578342, 1058222899, 1065353216
    f_4 = 0. - (f_4);
    pf_4 = fma(pf_2, f_4, f_3);
    f_3 = (1.0) / pf_0;
    f_4 = utof(fs_cbuf15[44].y); //1063675494, 1061578342, 1058222899, 1065353216
    pf_0 = pf_2 * f_4;
    pf_3 = pf_3 * f_0;
    f_0 = utof(fs_cbuf15[44].x); //1063675494, 1061578342, 1058222899, 1065353216
    pf_5 = pf_2 * f_0;
    f_0 = utof(fs_cbuf15[44].z); //1063675494, 1061578342, 1058222899, 1065353216
    pf_6 = pf_2 * f_0;
    pf_0 = fma(f_1, pf_4, pf_0);
    f_0 = utof(fs_cbuf15[43].z); //1065353216, 1063423836, 1059481190, 1065353216
    f_4 = utof(fs_cbuf15[44].z); //1063675494, 1061578342, 1058222899, 1065353216
    f_4 = 0. - (f_4);
    pf_4 = fma(pf_2, f_4, f_0);
    f_0 = utof(fs_cbuf15[43].x); //1065353216, 1063423836, 1059481190, 1065353216
    f_4 = utof(fs_cbuf15[44].x); //1063675494, 1061578342, 1058222899, 1065353216
    f_4 = 0. - (f_4);
    pf_2 = fma(pf_2, f_4, f_0);
    f_0 = utof(fs_cbuf15[42].x); //1082969293, 1079863863, 1076417135, 1059481190
    pf_7 = pf_3 * f_0;
    f_0 = utof(fs_cbuf15[42].y); //1082969293, 1079863863, 1076417135, 1059481190
    pf_8 = pf_3 * f_0;
    f_0 = utof(fs_cbuf15[42].z); //1082969293, 1079863863, 1076417135, 1059481190
    pf_3 = pf_3 * f_0;
    pf_1 = pf_1 * f_3;
    pf_4 = fma(f_1, pf_4, pf_6);
    pf_2 = fma(f_1, pf_2, pf_5);
    f_0 = in_attr7.y;
    f_1 = utof(fs_cbuf15[42].w); //1082969293, 1079863863, 1076417135, 1059481190
    pf_5 = pf_7 * f_1;
    f_1 = in_attr9.y;
    f_3 = utof(fs_cbuf15[42].w); //1082969293, 1079863863, 1076417135, 1059481190
    pf_6 = pf_8 * f_3;
    f_3 = utof(fs_cbuf15[42].w); //1082969293, 1079863863, 1076417135, 1059481190
    pf_3 = pf_3 * f_3;
    f_3 = min(pf_1, 1.);
    f_4 = utof(fs_cbuf15[1].x); //0, 1065353216, 1072865060, 1065353216
    f_4 = 0. - (f_4);
    pf_1 = fma(pf_5, f_4, pf_5);
    f_4 = utof(fs_cbuf15[1].x); //0, 1065353216, 1072865060, 1065353216
    f_4 = 0. - (f_4);
    pf_5 = fma(pf_6, f_4, pf_6);
    f_4 = utof(fs_cbuf15[1].x); //0, 1065353216, 1072865060, 1065353216
    f_4 = 0. - (f_4);
    pf_3 = fma(pf_3, f_4, pf_3);
    f_4 = utof(fs_cbuf15[57].w); //3314801541, 1147334299, 1161527296, 1065353216
    f_5 = utof(fs_cbuf15[57].w); //3314801541, 1147334299, 1161527296, 1065353216
    f_5 = 0. - (f_5);
    pf_6 = fma(f_3, f_5, f_4);
    f_3 = in_attr11.w;
    pf_1 = pf_1 + pf_2;
    f_4 = in_attr9.x;
    pf_0 = pf_5 + pf_0;
    f_5 = in_attr9.z;
    pf_2 = pf_3 + pf_4;
    pf_3 = fma(pf_6, -0.9, 1.);
    pf_1 = pf_3 * pf_1;
    pf_0 = pf_3 * pf_0;
    pf_2 = pf_3 * pf_2;
    f_6 = in_attr9.w;
    f_7 = utof(fs_cbuf15[26].x); //1066362785, 1067991434, 1059750465, 1055353662
    f_8 = 0. - (pf_1);
    pf_3 = f_8 + f_7;
    f_7 = in_attr11.z;
    f_8 = utof(fs_cbuf15[26].y); //1066362785, 1067991434, 1059750465, 1055353662
    f_9 = 0. - (pf_0);
    pf_4 = f_9 + f_8;
    f_8 = utof(fs_cbuf15[26].z); //1066362785, 1067991434, 1059750465, 1055353662
    f_9 = 0. - (pf_2);
    pf_5 = f_9 + f_8;
    pf_1 = fma(pf_3, f_0, pf_1);
    f_8 = in_attr11.y;
    pf_0 = fma(pf_4, f_0, pf_0);
    f_9 = in_attr10.x;
    pf_2 = fma(pf_5, f_0, pf_2);
    f_0 = in_attr11.x;
    pf_1 = fma(pf_1, f_6, f_4);
    f_4 = in_attr7.x;
    pf_0 = fma(pf_0, f_6, f_1);
    pf_2 = fma(pf_2, f_6, f_5);
    f_1 = in_attr10.y;
    f_5 = 0. - (pf_2);
    pf_3 = f_5 + f_7;
    f_5 = in_attr10.z;
    f_6 = 0. - (pf_0);
    pf_4 = f_6 + f_8;
    f_6 = in_attr10.w;
    f_7 = 0. - (pf_1);
    pf_5 = f_7 + f_0;
    pf_2 = fma(pf_3, f_3, pf_2);
    pf_0 = fma(pf_4, f_3, pf_0);
    pf_1 = fma(pf_5, f_3, pf_1);
    f_0 = utof(fs_cbuf15[25].z); //1060018062, 1065194672, 1059307546, 1039516303
    f_3 = 0. - (pf_2);
    pf_3 = f_3 + f_0;
    f_0 = utof(fs_cbuf15[25].y); //1060018062, 1065194672, 1059307546, 1039516303
    f_3 = 0. - (pf_0);
    pf_4 = f_3 + f_0;
    f_0 = utof(fs_cbuf15[25].x); //1060018062, 1065194672, 1059307546, 1039516303
    f_3 = 0. - (pf_1);
    pf_5 = f_3 + f_0;
    pf_1 = fma(pf_5, f_4, pf_1);
    pf_0 = fma(pf_4, f_4, pf_0);
    pf_2 = fma(pf_3, f_4, pf_2);
    f_0 = 0. - (pf_1);
    pf_3 = f_0 + f_9;
    f_0 = 0. - (pf_0);
    pf_4 = f_0 + f_1;
    f_0 = 0. - (pf_2);
    pf_5 = f_0 + f_5;
    pf_1 = fma(pf_3, f_6, pf_1);
    pf_0 = fma(pf_4, f_6, pf_0);
    pf_2 = fma(pf_5, f_6, pf_2);

    frag_color0.x = pf_1;
    frag_color0.y = pf_0;
    frag_color0.z = pf_2;
    frag_color0.w = 0.0;//f_2;
    return;
}