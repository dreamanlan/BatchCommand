vec4 gl_FragCoord;

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









layout(binding = 3) uniform sampler2D tex3; 
layout(binding = 4) uniform sampler2DArray tex4; 
layout(binding = 5) uniform sampler2D tex5; 
layout(binding = 6) uniform sampler2D depthTex;


void main() {
gl_FragCoord = vec4(320,240,0.5,1.0);
in_attr0.x = float(0.00);
in_attr0.y = float(0.00);
in_attr0.z = float(0.00);
in_attr0.w = float(0.77);
in_attr1.x = float(0.50);
in_attr1.y = float(0.50);
in_attr1.z = float(1.3886);
in_attr1.w = float(2.62434);
in_attr2.x = float(174.79773);
in_attr2.y = float(211.81522);
in_attr2.z = float(1031.18091);
in_attr2.w = float(1032.13977);
in_attr3.x = float(1.00);
in_attr3.y = float(0.00);
in_attr3.z = float(0.00);
in_attr3.w = float(1.00);
in_attr4.x = float(-818.58844);
in_attr4.y = float(1790.07166);
in_attr4.z = float(-2576.06763);
in_attr4.w = float(1.00);
in_attr5.x = float(0.00);
in_attr5.y = float(204.01236);
in_attr5.z = float(0.00);
in_attr5.w = float(1.00);
in_attr6.x = float(2.00);
in_attr6.y = float(0.00);
in_attr6.z = float(0.00);
in_attr6.w = float(1.00);
in_attr7.x = float(0.12);
in_attr7.y = float(0.00);
in_attr7.z = float(0.00);
in_attr7.w = float(1.00);
in_attr8.x = float(0.50);
in_attr8.y = float(0.00);
in_attr8.z = float(1.00);
in_attr8.w = float(1.00);
in_attr9.x = float(0.31615);
in_attr9.y = float(0.5853);
in_attr9.z = float(1.04314);
in_attr9.w = float(0.24803);
in_attr10.x = float(2.17773);
in_attr10.y = float(2.19922);
in_attr10.z = float(1.92871);
in_attr10.w = float(0.00);
in_attr11.x = float(0.14111);
in_attr11.y = float(0.396);
in_attr11.z = float(1.0166);
in_attr11.w = float(0.00);
fs_cbuf8[29] = uvec4(1136206159, 1156671898, 3308014955, 0);
fs_cbuf8[30] = uvec4(1065353216, 1187205120, 1187205120, 1187204608);
fs_cbuf9[139] = uvec4(1065353216, 0, 0, 0);
fs_cbuf9[140] = uvec4(1077936128, 1084227584, 0, 0);
fs_cbuf9[189] = uvec4(1024416809, 1080033280, 1065353216, 1092616192);
fs_cbuf9[190] = uvec4(1061158912, 1075838976, 1101004800, 1082130432);
fs_cbuf15[1] = uvec4(0, 1065353216, 1068391770, 1065353216);
fs_cbuf15[25] = uvec4(1059372138, 1066192077, 1063450680, 1039516303);
fs_cbuf15[26] = uvec4(1069772335, 1066136712, 1051149625, 1053609165);
fs_cbuf15[28] = uvec4(3196278355, 3206979389, 3208047673, 0);
fs_cbuf15[42] = uvec4(1084122727, 1083308542, 1080904223, 1056964608);
fs_cbuf15[43] = uvec4(1065353216, 1064011040, 1060320052, 1065353216);
fs_cbuf15[44] = uvec4(1061997774, 1062836634, 1059481190, 1065353216);
fs_cbuf15[57] = uvec4(1152292082, 3295878240, 1161527296, 1065353216);
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
    f_4 = utof(fs_cbuf9[189].x); 
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
    f_8 = utof(fs_cbuf9[140].y); 
    f_8 = (1.0) / f_8;
    pf_0 = f_4 * f_3;
    f_3 = utof(fs_cbuf8[30].y); 
    f_4 = utof(fs_cbuf8[30].w); 
    f_3 = 0. - (f_3);
    pf_0 = fma(pf_0, f_4, f_3);
    f_3 = in_attr3.x;
    f_4 = (1.0) / pf_0;
    f_9 = utof(fs_cbuf8[29].z); 
    f_7 = 0. - (f_7);
    pf_0 = f_7 + f_9;
    f_7 = utof(fs_cbuf8[30].x); 
    f_9 = utof(fs_cbuf8[30].w); 
    pf_1 = fma(f_0, f_9, f_7);
    f_0 = abs(f_1);
    f_0 = log2(f_0);
    pf_2 = f_2 * f_5;
    f_2 = utof(fs_cbuf8[30].z); 
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
    f_3 = utof(fs_cbuf9[139].z); 
    b_0 = pf_1 <= f_3 && !isnan(pf_1) && !isnan(f_3);
    b_1 = b_0 ? ( true ) : ( false);
    if(b_1) {
        discard;
    }

    f_3 = utof(fs_cbuf8[29].x); 
    f_2 = 0. - (f_2);
    pf_3 = f_2 + f_3;
    f_2 = utof(fs_cbuf8[29].y); 
    f_3 = 0. - (f_4);
    pf_4 = f_3 + f_2;
    f_2 = 0. - (0.);
    pf_1 = pf_1 + f_2;
    f_2 = min(max(pf_1, 0.0), 1.0);
    pf_1 = pf_3 * pf_3;
    f_3 = utof(fs_cbuf9[190].w); 
    f_4 = utof(fs_cbuf9[190].z); 
    f_4 = 0. - (f_4);
    pf_5 = f_4 + f_3;
    pf_1 = fma(pf_4, pf_4, pf_1);
    pf_1 = fma(pf_0, pf_0, pf_1);
    f_3 = inversesqrt(pf_1);
    pf_1 = pf_3 * f_3;
    pf_3 = pf_4 * f_3;
    pf_0 = pf_0 * f_3;
    f_3 = utof(fs_cbuf15[28].x); 
    pf_1 = pf_1 * f_3;
    f_3 = utof(fs_cbuf15[28].y); 
    pf_1 = fma(pf_3, f_3, pf_1);
    f_3 = utof(fs_cbuf15[28].z); 
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
    f_5 = utof(fs_cbuf9[190].y); 
    f_6 = utof(fs_cbuf9[189].y); 
    f_6 = 0. - (f_6);
    pf_1 = f_6 + f_5;
    pf_2 = pf_0 * -5.;
    f_5 = utof(fs_cbuf9[189].y); 
    pf_1 = fma(pf_1, f_3, f_5);
    f_5 = utof(fs_cbuf9[190].z); 
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
    f_3 = utof(fs_cbuf15[43].y); 
    f_4 = utof(fs_cbuf15[44].y); 
    f_4 = 0. - (f_4);
    pf_4 = fma(pf_2, f_4, f_3);
    f_3 = (1.0) / pf_0;
    f_4 = utof(fs_cbuf15[44].y); 
    pf_0 = pf_2 * f_4;
    pf_3 = pf_3 * f_0;
    f_0 = utof(fs_cbuf15[44].x); 
    pf_5 = pf_2 * f_0;
    f_0 = utof(fs_cbuf15[44].z); 
    pf_6 = pf_2 * f_0;
    pf_0 = fma(f_1, pf_4, pf_0);
    f_0 = utof(fs_cbuf15[43].z); 
    f_4 = utof(fs_cbuf15[44].z); 
    f_4 = 0. - (f_4);
    pf_4 = fma(pf_2, f_4, f_0);
    f_0 = utof(fs_cbuf15[43].x); 
    f_4 = utof(fs_cbuf15[44].x); 
    f_4 = 0. - (f_4);
    pf_2 = fma(pf_2, f_4, f_0);
    f_0 = utof(fs_cbuf15[42].x); 
    pf_7 = pf_3 * f_0;
    f_0 = utof(fs_cbuf15[42].y); 
    pf_8 = pf_3 * f_0;
    f_0 = utof(fs_cbuf15[42].z); 
    pf_3 = pf_3 * f_0;
    pf_1 = pf_1 * f_3;
    pf_4 = fma(f_1, pf_4, pf_6);
    pf_2 = fma(f_1, pf_2, pf_5);
    f_0 = in_attr7.y;
    f_1 = utof(fs_cbuf15[42].w); 
    pf_5 = pf_7 * f_1;
    f_1 = in_attr9.y;
    f_3 = utof(fs_cbuf15[42].w); 
    pf_6 = pf_8 * f_3;
    f_3 = utof(fs_cbuf15[42].w); 
    pf_3 = pf_3 * f_3;
    f_3 = min(pf_1, 1.);
    f_4 = utof(fs_cbuf15[1].x); 
    f_4 = 0. - (f_4);
    pf_1 = fma(pf_5, f_4, pf_5);
    f_4 = utof(fs_cbuf15[1].x); 
    f_4 = 0. - (f_4);
    pf_5 = fma(pf_6, f_4, pf_6);
    f_4 = utof(fs_cbuf15[1].x); 
    f_4 = 0. - (f_4);
    pf_3 = fma(pf_3, f_4, pf_3);
    f_4 = utof(fs_cbuf15[57].w); 
    f_5 = utof(fs_cbuf15[57].w); 
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
    f_7 = utof(fs_cbuf15[26].x); 
    f_8 = 0. - (pf_1);
    pf_3 = f_8 + f_7;
    f_7 = in_attr11.z;
    f_8 = utof(fs_cbuf15[26].y); 
    f_9 = 0. - (pf_0);
    pf_4 = f_9 + f_8;
    f_8 = utof(fs_cbuf15[26].z); 
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
    f_0 = utof(fs_cbuf15[25].z); 
    f_3 = 0. - (pf_2);
    pf_3 = f_3 + f_0;
    f_0 = utof(fs_cbuf15[25].y); 
    f_3 = 0. - (pf_0);
    pf_4 = f_3 + f_0;
    f_0 = utof(fs_cbuf15[25].x); 
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
    frag_color0.w = f_2;
    return;
}
