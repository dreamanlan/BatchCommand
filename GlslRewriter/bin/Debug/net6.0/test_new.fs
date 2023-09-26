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

void main()
{
	// 1065353216 = 1.00f;
	// fs_cbuf8[29] = vec4(287.3111, 1672.144, 1396.669, 0.00);
	// fs_cbuf8[30] = vec4(1.00, 25000.00, 25000.00, 24999.00);
	// fs_cbuf9[139] = vec4(1.00, 0.00, 0.00, 0.00);
	// fs_cbuf9[140] = vec4(3.00, 5.00, 0.00, 0.00);
	// fs_cbuf9[189] = vec4(0.035, 3.50, 1.00, 10.00);
	// fs_cbuf9[190] = vec4(0.75, 2.50, 20.00, 4.00);
	// fs_cbuf15[1] = vec4(0.00, 1.00, 1.895482, 1.00);
	// fs_cbuf15[25] = vec4(0.682, 0.99055, 0.63965, 0.12);
	// fs_cbuf15[26] = vec4(1.12035, 1.3145, 0.66605, 0.4519901);
	// fs_cbuf15[28] = vec4(0.5226594, -0.5741013, -0.6302658, 0.00);
	// fs_cbuf15[42] = vec4(4.40, 3.459608, 2.637844, 0.65);
	// fs_cbuf15[43] = vec4(1.00, 0.885, 0.65, 1.00);
	// fs_cbuf15[44] = vec4(0.90, 0.775, 0.575, 1.00);
	// fs_cbuf15[57] = vec4(-4731.44, 907.7282, 3000.00, 1.00);

	bool b_0_0;
	float f_0_6;
	float f_1_9;
	float f_2_16;
	float f_2_7;
	float f_3_11;
	float f_3_20;
	float f_4_3;
	float f_4_8;
	float f_5_2;
	float f_6_1;
	float f_8_2;
	float f_9_2;
	float f_9_3;
	vec2 f2_0_2;
	vec2 f2_0_3;
	vec3 f3_0_0;
	vec4 f4_0_1;
	vec4 f4_0_3;
	vec4 f4_0_4;
	precise float pf_0_11;
	precise float pf_0_12;
	precise float pf_0_13;
	precise float pf_0_16;
	precise float pf_0_18;
	precise float pf_0_20;
	precise float pf_0_23;
	precise float pf_0_24;
	precise float pf_0_6;
	precise float pf_0_8;
	precise float pf_1_11;
	precise float pf_1_19;
	precise float pf_1_23;
	precise float pf_1_26;
	precise float pf_1_27;
	precise float pf_1_5;
	precise float pf_1_8;
	precise float pf_2_11;
	precise float pf_2_12;
	precise float pf_2_3;
	precise float pf_2_6;
	precise float pf_2_7;
	precise float pf_2_8;
	precise float pf_3_0;
	precise float pf_3_11;
	precise float pf_3_3;
	precise float pf_3_5;
	precise float pf_3_7;
	precise float pf_4_0;
	precise float pf_4_2;
	precise float pf_4_3;
	precise float pf_4_8;
	precise float pf_5_1;
	precise float pf_5_2;
	precise float pf_5_7;
	precise float pf_6_0;
	precise float pf_6_1;
	precise float pf_6_2;
	uint u_0_0;
	// 1  <=>  uint({vec4(textureQueryLod({tex5 : tex5}, vec2({in_attr1.z : 1.46396}, {in_attr1.w : 3.26268})), 0.0, 0.0).y : 1.00})
	u_0_0 = uint(vec4(textureQueryLod(tex5, vec2(in_attr1.z, in_attr1.w)), 0.0, 0.0).y);
	// vec4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex5 : tex5}, vec2({in_attr1.z : 1.46396}, {in_attr1.w : 3.26268}), min((float(({u_0_0 : 1} << 8u)) / 256.0), 2.))
	f4_0_1 = textureLod(tex5, vec2(in_attr1.z, in_attr1.w), min((float((u_0_0 << 8u)) / 256.0), 2.));
	// vec2(0.50,0.5175)  <=>  vec2((((({f4_0_1.x : 0.50} * {utof(fs_cbuf9[189].x) : 0.035}) * 2.) + (0. - {utof(fs_cbuf9[189].x) : 0.035})) + {in_attr1.x : 0.50}), (((({f4_0_1.w : 0.75} * {utof(fs_cbuf9[189].x) : 0.035}) * 2.) + (0. - {utof(fs_cbuf9[189].x) : 0.035})) + {in_attr1.y : 0.50}))
	f2_0_2 = vec2(((((f4_0_1.x * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.x), ((((f4_0_1.w * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.y));
	// 0.0007925  <=>  (1.0 / ({in_attr2.w : 1261.846} * {gl_FragCoord.w : 1.00}))
	f_0_6 = (1.0 / (in_attr2.w * gl_FragCoord.w));
	// 256.00  <=>  float((uint({vec4(textureQueryLod({tex3 : tex3}, {f2_0_2 : vec2(0.50,0.5175)}), 0.0, 0.0).y : 1.00}) << 8u))
	f_2_7 = float((uint(vec4(textureQueryLod(tex3, f2_0_2), 0.0, 0.0).y) << 8u));
	// vec2(0.5659583,0.1917964)  <=>  vec2((({in_attr2.x : 714.152} * {gl_FragCoord.w : 1.00}) * {f_0_6 : 0.0007925}), (({in_attr2.y : 242.0175} * {gl_FragCoord.w : 1.00}) * {f_0_6 : 0.0007925}))
	f2_0_3 = vec2(((in_attr2.x * gl_FragCoord.w) * f_0_6), ((in_attr2.y * gl_FragCoord.w) * f_0_6));
	// vec4(0.50,0.50,0.50,0.75)  <=>  texture({depthTex : depthTex}, {f2_0_3 : vec2(0.5659583,0.1917964)})
	f4_0_3 = texture(depthTex, f2_0_3);
	// 2.00  <=>  float((uint(clamp(roundEven({in_attr6.x : 2.00}), float(0.), float(65535.))) & 0xffff))
	f_1_9 = float((uint(clamp(roundEven(in_attr6.x), float(0.), float(65535.))) & 0xffff));
	// vec3(0.50,0.5175,2.00)  <=>  vec3((((({f4_0_1.x : 0.50} * {utof(fs_cbuf9[189].x) : 0.035}) * 2.) + (0. - {utof(fs_cbuf9[189].x) : 0.035})) + {in_attr1.x : 0.50}), (((({f4_0_1.w : 0.75} * {utof(fs_cbuf9[189].x) : 0.035}) * 2.) + (0. - {utof(fs_cbuf9[189].x) : 0.035})) + {in_attr1.y : 0.50}), {f_1_9 : 2.00})
	f3_0_0 = vec3(((((f4_0_1.x * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.x), ((((f4_0_1.w * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.y), f_1_9);
	// vec4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex4 : tex4}, {f3_0_0 : vec3(0.50,0.5175,2.00)}, min(({f_2_7 : 256.00} / 256.0), 2.))
	f4_0_4 = textureLod(tex4, f3_0_0, min((f_2_7 / 256.0), 2.));
	// -0.0504746  <=>  (1.0 / ((({in_attr2.z : 1260.896} * (1.0 / {in_attr2.w : 1261.846})) * {utof(fs_cbuf8[30].w) : 24999.00}) + (0. - {utof(fs_cbuf8[30].y) : 25000.00})))
	f_4_3 = (1.0 / (((in_attr2.z * (1.0 / in_attr2.w)) * utof(fs_cbuf8[30].w)) + (0. - utof(fs_cbuf8[30].y))));
	// 1225.846  <=>  ((0. - {in_attr4.z : 170.8237}) + {utof(fs_cbuf8[29].z) : 1396.669})
	pf_0_6 = ((0. - in_attr4.z) + utof(fs_cbuf8[29].z));
	// 2247.727  <=>  ((({f_4_3 : -0.0504746} * {utof(fs_cbuf8[30].z) : 25000.00}) + (({f4_0_3.x : 0.50} * {utof(fs_cbuf8[30].w) : 24999.00}) + {utof(fs_cbuf8[30].x) : 1.00})) * (1.0 / {utof(fs_cbuf9[140].y) : 5.00}))
	pf_1_5 = (((f_4_3 * utof(fs_cbuf8[30].z)) + ((f4_0_3.x * utof(fs_cbuf8[30].w)) + utof(fs_cbuf8[30].x))) * (1.0 / utof(fs_cbuf9[140].y)));
	// 0.75  <=>  clamp(((({f4_0_4.w : 0.75} * {in_attr8.z : 1.00}) * {in_attr0.w : 1.00}) + (0. - 0.)), 0.0, 1.0)
	f_5_2 = clamp((((f4_0_4.w * in_attr8.z) * in_attr0.w) + (0. - 0.)), 0.0, 1.0);
	// 0.75  <=>  ((clamp({pf_1_5 : 2247.727}, 0.0, 1.0) * {f_5_2 : 0.75}) * {in_attr3.x : 1.00})
	pf_1_8 = ((clamp(pf_1_5, 0.0, 1.0) * f_5_2) * in_attr3.x);
	// False  <=>  ((({pf_1_8 : 0.75} <= {utof(fs_cbuf9[139].z) : 0.00}) && (! isnan({pf_1_8 : 0.75}))) && (! isnan({utof(fs_cbuf9[139].z) : 0.00})))
	b_0_0 = (((pf_1_8 <= utof(fs_cbuf9[139].z)) && (! isnan(pf_1_8))) && (! isnan(utof(fs_cbuf9[139].z))));
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		discard;
	}
	// -277.3888  <=>  ((0. - {in_attr4.x : 564.6999}) + {utof(fs_cbuf8[29].x) : 287.3111})
	pf_3_0 = ((0. - in_attr4.x) + utof(fs_cbuf8[29].x));
	// 403.9866  <=>  ((0. - {in_attr4.y : 1268.157}) + {utof(fs_cbuf8[29].y) : 1672.144})
	pf_4_0 = ((0. - in_attr4.y) + utof(fs_cbuf8[29].y));
	// 0.75  <=>  clamp(({pf_1_8 : 0.75} + (0. - 0.)), 0.0, 1.0)
	f_2_16 = clamp((pf_1_8 + (0. - 0.)), 0.0, 1.0);
	// 240149.70  <=>  (({pf_4_0 : 403.9866} * {pf_4_0 : 403.9866}) + ({pf_3_0 : -277.3888} * {pf_3_0 : -277.3888}))
	pf_1_11 = ((pf_4_0 * pf_4_0) + (pf_3_0 * pf_3_0));
	// 0.0007575  <=>  inversesqrt((({pf_0_6 : 1225.846} * {pf_0_6 : 1225.846}) + {pf_1_11 : 240149.70}))
	f_3_11 = inversesqrt(((pf_0_6 * pf_0_6) + pf_1_11));
	// -0.870735  <=>  ((({pf_0_6 : 1225.846} * {f_3_11 : 0.0007575}) * {utof(fs_cbuf15[28].z) : -0.6302658}) + ((({pf_4_0 : 403.9866} * {f_3_11 : 0.0007575}) * {utof(fs_cbuf15[28].y) : -0.5741013}) + (({pf_3_0 : -277.3888} * {f_3_11 : 0.0007575}) * {utof(fs_cbuf15[28].x) : 0.5226594})))
	pf_0_8 = (((pf_0_6 * f_3_11) * utof(fs_cbuf15[28].z)) + (((pf_4_0 * f_3_11) * utof(fs_cbuf15[28].y)) + ((pf_3_0 * f_3_11) * utof(fs_cbuf15[28].x))));
	// 0.00  <=>  clamp((({pf_0_8 : -0.870735} * 2.) + (0. - 1.)), 0.0, 1.0)
	f_4_8 = clamp(((pf_0_8 * 2.) + (0. - 1.)), 0.0, 1.0);
	// 0.00  <=>  (({f_4_8 : 0.00} * {in_attr5.y : 175.4452}) * 0.005)
	pf_0_11 = ((f_4_8 * in_attr5.y) * 0.005);
	// -∞  <=>  (0. - (1.0 / {pf_0_11 : 0.00}))
	f_6_1 = (0. - (1.0 / pf_0_11));
	// NaN  <=>  (((1.0 / {pf_0_11 : 0.00}) * ({pf_0_11 : 0.00} + (({f4_0_4.w : 0.75} * {in_attr8.z : 1.00}) * {in_attr0.w : 1.00}))) + {f_6_1 : -∞})
	pf_0_12 = (((1.0 / pf_0_11) * (pf_0_11 + ((f4_0_4.w * in_attr8.z) * in_attr0.w))) + f_6_1);
	// 1.00  <=>  ((0. - {f_4_8 : 0.00}) + 1.)
	pf_0_13 = ((0. - f_4_8) + 1.);
	// -5.00  <=>  ((clamp({pf_0_12 : NaN}, 0.0, 1.0) + (({pf_0_13 : 1.00} * (0. - clamp({pf_0_12 : NaN}, 0.0, 1.0))) + {pf_0_13 : 1.00})) * -5.)
	pf_2_3 = ((clamp(pf_0_12, 0.0, 1.0) + ((pf_0_13 * (0. - clamp(pf_0_12, 0.0, 1.0))) + pf_0_13)) * -5.);
	// -2.50  <=>  (((((0. - {utof(fs_cbuf9[189].y) : 3.50}) + {utof(fs_cbuf9[190].y) : 2.50}) * {in_attr8.x : 1.00}) + {utof(fs_cbuf9[189].y) : 3.50}) * log2(abs({f4_0_4.x : 0.50})))
	pf_1_19 = (((((0. - utof(fs_cbuf9[189].y)) + utof(fs_cbuf9[190].y)) * in_attr8.x) + utof(fs_cbuf9[189].y)) * log2(abs(f4_0_4.x)));
	// 0.00  <=>  ({f_4_8 : 0.00} * (clamp({pf_0_12 : NaN}, 0.0, 1.0) + (({pf_0_13 : 1.00} * (0. - clamp({pf_0_12 : NaN}, 0.0, 1.0))) + {pf_0_13 : 1.00})))
	pf_0_16 = (f_4_8 * (clamp(pf_0_12, 0.0, 1.0) + ((pf_0_13 * (0. - clamp(pf_0_12, 0.0, 1.0))) + pf_0_13)));
	// 0.00  <=>  max(0., (exp2({pf_2_3 : -5.00}) + -0.03125))
	f_3_20 = max(0., (exp2(pf_2_3) + -0.03125));
	// 0.1100001  <=>  (((((clamp({f4_0_4.x : 0.50}, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - {utof(fs_cbuf15[44].y) : 0.775})) + {utof(fs_cbuf15[43].y) : 0.885})
	pf_4_2 = (((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - utof(fs_cbuf15[44].y))) + utof(fs_cbuf15[43].y));
	// 0.7749999  <=>  ((((clamp({f4_0_4.x : 0.50}, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * {utof(fs_cbuf15[44].y) : 0.775})
	pf_0_18 = ((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * utof(fs_cbuf15[44].y));
	// 0.7071068  <=>  ((({in_attr8.x : 1.00} * ((0. - {utof(fs_cbuf9[190].z) : 20.00}) + {utof(fs_cbuf9[190].w) : 4.00})) + {utof(fs_cbuf9[190].z) : 20.00}) * exp2({pf_1_19 : -2.50}))
	pf_3_3 = (((in_attr8.x * ((0. - utof(fs_cbuf9[190].z)) + utof(fs_cbuf9[190].w))) + utof(fs_cbuf9[190].z)) * exp2(pf_1_19));
	// 0.8999999  <=>  ((((clamp({f4_0_4.x : 0.50}, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * {utof(fs_cbuf15[44].x) : 0.90})
	pf_5_1 = ((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * utof(fs_cbuf15[44].x));
	// 0.5749999  <=>  ((((clamp({f4_0_4.x : 0.50}, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * {utof(fs_cbuf15[44].z) : 0.575})
	pf_6_0 = ((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * utof(fs_cbuf15[44].z));
	// 0.075  <=>  (((((clamp({f4_0_4.x : 0.50}, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - {utof(fs_cbuf15[44].z) : 0.575})) + {utof(fs_cbuf15[43].z) : 0.65})
	pf_4_3 = (((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - utof(fs_cbuf15[44].z))) + utof(fs_cbuf15[43].z));
	// 0.1000001  <=>  (((((clamp({f4_0_4.x : 0.50}, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - {utof(fs_cbuf15[44].x) : 0.90})) + {utof(fs_cbuf15[43].x) : 1.00})
	pf_2_6 = (((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - utof(fs_cbuf15[44].x))) + utof(fs_cbuf15[43].x));
	// 0.9499999  <=>  (({f4_0_4.x : 0.50} * {pf_2_6 : 0.1000001}) + {pf_5_1 : 0.8999999})
	pf_2_7 = ((f4_0_4.x * pf_2_6) + pf_5_1);
	// 2.022325  <=>  (({pf_3_3 : 0.7071068} * {utof(fs_cbuf15[42].x) : 4.40}) * {utof(fs_cbuf15[42].w) : 0.65})
	pf_5_2 = ((pf_3_3 * utof(fs_cbuf15[42].x)) * utof(fs_cbuf15[42].w));
	// 1.590103  <=>  (({pf_3_3 : 0.7071068} * {utof(fs_cbuf15[42].y) : 3.459608}) * {utof(fs_cbuf15[42].w) : 0.65})
	pf_6_1 = ((pf_3_3 * utof(fs_cbuf15[42].y)) * utof(fs_cbuf15[42].w));
	// 1.212404  <=>  (({pf_3_3 : 0.7071068} * {utof(fs_cbuf15[42].z) : 2.637844}) * {utof(fs_cbuf15[42].w) : 0.65})
	pf_3_5 = ((pf_3_3 * utof(fs_cbuf15[42].z)) * utof(fs_cbuf15[42].w));
	// 0.00  <=>  ((min((((0. - {f_4_8 : 0.00}) + 1.05) * (1.0 / (({pf_0_16 : 0.00} * {f_3_20 : 0.00}) + 1.))), 1.) * (0. - {utof(fs_cbuf15[57].w) : 1.00})) + {utof(fs_cbuf15[57].w) : 1.00})
	pf_6_2 = ((min((((0. - f_4_8) + 1.05) * (1.0 / ((pf_0_16 * f_3_20) + 1.))), 1.) * (0. - utof(fs_cbuf15[57].w))) + utof(fs_cbuf15[57].w));
	// 2.972325  <=>  ((({pf_5_2 : 2.022325} * (0. - {utof(fs_cbuf15[1].x) : 0.00})) + {pf_5_2 : 2.022325}) + {pf_2_7 : 0.9499999})
	pf_1_23 = (((pf_5_2 * (0. - utof(fs_cbuf15[1].x))) + pf_5_2) + pf_2_7);
	// 2.420103  <=>  ((({pf_6_1 : 1.590103} * (0. - {utof(fs_cbuf15[1].x) : 0.00})) + {pf_6_1 : 1.590103}) + (({f4_0_4.x : 0.50} * {pf_4_2 : 0.1100001}) + {pf_0_18 : 0.7749999}))
	pf_0_20 = (((pf_6_1 * (0. - utof(fs_cbuf15[1].x))) + pf_6_1) + ((f4_0_4.x * pf_4_2) + pf_0_18));
	// 1.824904  <=>  ((({pf_3_5 : 1.212404} * (0. - {utof(fs_cbuf15[1].x) : 0.00})) + {pf_3_5 : 1.212404}) + (({f4_0_4.x : 0.50} * {pf_4_3 : 0.075}) + {pf_6_0 : 0.5749999}))
	pf_2_8 = (((pf_3_5 * (0. - utof(fs_cbuf15[1].x))) + pf_3_5) + ((f4_0_4.x * pf_4_3) + pf_6_0));
	// 1.00  <=>  (({pf_6_2 : 0.00} * -0.9) + 1.)
	pf_3_7 = ((pf_6_2 * -0.9) + 1.);
	// -2.972325  <=>  (0. - ({pf_3_7 : 1.00} * {pf_1_23 : 2.972325}))
	f_8_2 = (0. - (pf_3_7 * pf_1_23));
	// -2.420103  <=>  (0. - ({pf_3_7 : 1.00} * {pf_0_20 : 2.420103}))
	f_9_2 = (0. - (pf_3_7 * pf_0_20));
	// -1.824904  <=>  (0. - ({pf_3_7 : 1.00} * {pf_2_8 : 1.824904}))
	f_9_3 = (0. - (pf_3_7 * pf_2_8));
	// 1.964553  <=>  ((((({f_8_2 : -2.972325} + {utof(fs_cbuf15[26].x) : 1.12035}) * {in_attr7.y : 0.00}) + ({pf_3_7 : 1.00} * {pf_1_23 : 2.972325})) * {in_attr9.w : 0.52491}) + {in_attr9.x : 0.40435})
	pf_1_26 = (((((f_8_2 + utof(fs_cbuf15[26].x)) * in_attr7.y) + (pf_3_7 * pf_1_23)) * in_attr9.w) + in_attr9.x);
	// 1.662286  <=>  ((((({f_9_2 : -2.420103} + {utof(fs_cbuf15[26].y) : 1.3145}) * {in_attr7.y : 0.00}) + ({pf_3_7 : 1.00} * {pf_0_20 : 2.420103})) * {in_attr9.w : 0.52491}) + {in_attr9.y : 0.39195})
	pf_0_23 = (((((f_9_2 + utof(fs_cbuf15[26].y)) * in_attr7.y) + (pf_3_7 * pf_0_20)) * in_attr9.w) + in_attr9.y);
	// 1.46188  <=>  ((((({f_9_3 : -1.824904} + {utof(fs_cbuf15[26].z) : 0.66605}) * {in_attr7.y : 0.00}) + ({pf_3_7 : 1.00} * {pf_2_8 : 1.824904})) * {in_attr9.w : 0.52491}) + {in_attr9.z : 0.50397})
	pf_2_11 = (((((f_9_3 + utof(fs_cbuf15[26].z)) * in_attr7.y) + (pf_3_7 * pf_2_8)) * in_attr9.w) + in_attr9.z);
	// 1.46188  <=>  ((((0. - {pf_2_11 : 1.46188}) + {in_attr11.z : 0.8335}) * {in_attr11.w : 0.00}) + {pf_2_11 : 1.46188})
	pf_2_12 = ((((0. - pf_2_11) + in_attr11.z) * in_attr11.w) + pf_2_11);
	// 1.662286  <=>  ((((0. - {pf_0_23 : 1.662286}) + {in_attr11.y : 0.44214}) * {in_attr11.w : 0.00}) + {pf_0_23 : 1.662286})
	pf_0_24 = ((((0. - pf_0_23) + in_attr11.y) * in_attr11.w) + pf_0_23);
	// 1.964553  <=>  ((((0. - {pf_1_26 : 1.964553}) + {in_attr11.x : 0.20251}) * {in_attr11.w : 0.00}) + {pf_1_26 : 1.964553})
	pf_1_27 = ((((0. - pf_1_26) + in_attr11.x) * in_attr11.w) + pf_1_26);
	// 2.962793  <=>  ((0. - ((((0. - {pf_1_27 : 1.964553}) + {utof(fs_cbuf15[25].x) : 0.682}) * {in_attr7.x : 0.12}) + {pf_1_27 : 1.964553})) + {in_attr10.x : 4.77344})
	pf_3_11 = ((0. - ((((0. - pf_1_27) + utof(fs_cbuf15[25].x)) * in_attr7.x) + pf_1_27)) + in_attr10.x);
	// 1.521842  <=>  ((0. - ((((0. - {pf_0_24 : 1.662286}) + {utof(fs_cbuf15[25].y) : 0.99055}) * {in_attr7.x : 0.12}) + {pf_0_24 : 1.662286})) + {in_attr10.y : 3.10352})
	pf_4_8 = ((0. - ((((0. - pf_0_24) + utof(fs_cbuf15[25].y)) * in_attr7.x) + pf_0_24)) + in_attr10.y);
	// 0.3906972  <=>  ((0. - ((((0. - {pf_2_12 : 1.46188}) + {utof(fs_cbuf15[25].z) : 0.63965}) * {in_attr7.x : 0.12}) + {pf_2_12 : 1.46188})) + {in_attr10.z : 1.75391})
	pf_5_7 = ((0. - ((((0. - pf_2_12) + utof(fs_cbuf15[25].z)) * in_attr7.x) + pf_2_12)) + in_attr10.z);
	// 1.810647  <=>  (({pf_3_11 : 2.962793} * {in_attr10.w : 0.00}) + ((((0. - {pf_1_27 : 1.964553}) + {utof(fs_cbuf15[25].x) : 0.682}) * {in_attr7.x : 0.12}) + {pf_1_27 : 1.964553}))
	frag_color0.x = ((pf_3_11 * in_attr10.w) + ((((0. - pf_1_27) + utof(fs_cbuf15[25].x)) * in_attr7.x) + pf_1_27));
	// 1.581678  <=>  (({pf_4_8 : 1.521842} * {in_attr10.w : 0.00}) + ((((0. - {pf_0_24 : 1.662286}) + {utof(fs_cbuf15[25].y) : 0.99055}) * {in_attr7.x : 0.12}) + {pf_0_24 : 1.662286}))
	frag_color0.y = ((pf_4_8 * in_attr10.w) + ((((0. - pf_0_24) + utof(fs_cbuf15[25].y)) * in_attr7.x) + pf_0_24));
	// 1.363213  <=>  (({pf_5_7 : 0.3906972} * {in_attr10.w : 0.00}) + ((((0. - {pf_2_12 : 1.46188}) + {utof(fs_cbuf15[25].z) : 0.63965}) * {in_attr7.x : 0.12}) + {pf_2_12 : 1.46188}))
	frag_color0.z = ((pf_5_7 * in_attr10.w) + ((((0. - pf_2_12) + utof(fs_cbuf15[25].z)) * in_attr7.x) + pf_2_12));
	// 0.75  <=>  {f_2_16 : 0.75}
	frag_color0.w = f_2_16;
	return;
}
