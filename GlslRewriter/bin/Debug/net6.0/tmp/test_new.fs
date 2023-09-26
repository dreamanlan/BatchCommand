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


void main()
{
gl_FragCoord = vec4(320,240,0.5,1.0);
in_attr0.x = float(0.00);
in_attr0.y = float(0.00);
in_attr0.z = float(0.00);
in_attr0.w = float(1.00);
in_attr1.x = float(0.74048);
in_attr1.y = float(1.04115);
in_attr1.z = float(1.72853);
in_attr1.w = float(3.85744);
in_attr2.x = float(759.15698);
in_attr2.y = float(353.81418);
in_attr2.z = float(1280.54175);
in_attr2.w = float(1281.49048);
in_attr3.x = float(1.00);
in_attr3.y = float(0.00);
in_attr3.z = float(0.00);
in_attr3.w = float(1.00);
in_attr4.x = float(618.52814);
in_attr4.y = float(1178.30042);
in_attr4.z = float(213.96782);
in_attr4.w = float(1.00);
in_attr5.x = float(0.00);
in_attr5.y = float(175.44524);
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
in_attr8.x = float(1.00);
in_attr8.y = float(0.00);
in_attr8.z = float(1.00);
in_attr8.w = float(1.00);
in_attr9.x = float(0.40435);
in_attr9.y = float(0.39195);
in_attr9.z = float(0.50397);
in_attr9.w = float(0.52491);
in_attr10.x = float(5.28516);
in_attr10.y = float(3.31641);
in_attr10.z = float(1.82227);
in_attr10.w = float(0.00);
in_attr11.x = float(0.20813);
in_attr11.y = float(0.45215);
in_attr11.z = float(0.84668);
in_attr11.w = float(0.00);
fs_cbuf8[29] = uvec4(1133488083, 1154548889, 1152292204, 0);
fs_cbuf8[30] = uvec4(1065353216, 1187205120, 1187205120, 1187204608);
fs_cbuf9[139] = uvec4(1065353216, 0, 0, 0);
fs_cbuf9[140] = uvec4(1077936128, 1084227584, 0, 0);
fs_cbuf9[189] = uvec4(1024416809, 1080033280, 1065353216, 1092616192);
fs_cbuf9[190] = uvec4(1061158912, 1075838976, 1101004800, 1082130432);
fs_cbuf15[1] = uvec4(0, 1065353216, 1072865060, 1065353216);
fs_cbuf15[25] = uvec4(1060018062, 1065194672, 1059307546, 1039516303);
fs_cbuf15[26] = uvec4(1066362785, 1067991434, 1059750465, 1055353662);
fs_cbuf15[28] = uvec4(1057344769, 3205691469, 3206633754, 0);
fs_cbuf15[42] = uvec4(1082969293, 1079863863, 1076417135, 1059481190);
fs_cbuf15[43] = uvec4(1065353216, 1063423836, 1059481190, 1065353216);
fs_cbuf15[44] = uvec4(1063675494, 1061578342, 1058222899, 1065353216);
fs_cbuf15[57] = uvec4(3314801541, 1147334299, 1161527296, 1065353216);
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

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
	
	u_0_0 = uint(vec4(textureQueryLod(tex5, vec2(in_attr1.z, in_attr1.w)), 0.0, 0.0).y);
	
	f4_0_1 = textureLod(tex5, vec2(in_attr1.z, in_attr1.w), min((float((u_0_0 << 8u)) / 256.0), 2.));
	
	f2_0_2 = vec2(((((f4_0_1.x * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.x), ((((f4_0_1.w * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.y));
	
	f_0_6 = (1.0 / (in_attr2.w * gl_FragCoord.w));
	
	f_2_7 = float((uint(vec4(textureQueryLod(tex3, f2_0_2), 0.0, 0.0).y) << 8u));
	
	f2_0_3 = vec2(((in_attr2.x * gl_FragCoord.w) * f_0_6), ((in_attr2.y * gl_FragCoord.w) * f_0_6));
	
	f4_0_3 = texture(depthTex, f2_0_3);
	
	f_1_9 = float((uint(clamp(roundEven(in_attr6.x), float(0.), float(65535.))) & 0xffff));
	
	f3_0_0 = vec3(((((f4_0_1.x * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.x), ((((f4_0_1.w * utof(fs_cbuf9[189].x)) * 2.) + (0. - utof(fs_cbuf9[189].x))) + in_attr1.y), f_1_9);
	
	f4_0_4 = textureLod(tex4, f3_0_0, min((f_2_7 / 256.0), 2.));
	
	f_4_3 = (1.0 / (((in_attr2.z * (1.0 / in_attr2.w)) * utof(fs_cbuf8[30].w)) + (0. - utof(fs_cbuf8[30].y))));
	
	pf_0_6 = ((0. - in_attr4.z) + utof(fs_cbuf8[29].z));
	
	pf_1_5 = (((f_4_3 * utof(fs_cbuf8[30].z)) + ((f4_0_3.x * utof(fs_cbuf8[30].w)) + utof(fs_cbuf8[30].x))) * (1.0 / utof(fs_cbuf9[140].y)));
	
	f_5_2 = clamp((((f4_0_4.w * in_attr8.z) * in_attr0.w) + (0. - 0.)), 0.0, 1.0);
	
	pf_1_8 = ((clamp(pf_1_5, 0.0, 1.0) * f_5_2) * in_attr3.x);
	
	b_0_0 = (((pf_1_8 <= utof(fs_cbuf9[139].z)) && (! isnan(pf_1_8))) && (! isnan(utof(fs_cbuf9[139].z))));
	
	if((b_0_0 ? ( true ) : ( false)))
	{
		discard;
	}
	
	pf_3_0 = ((0. - in_attr4.x) + utof(fs_cbuf8[29].x));
	
	pf_4_0 = ((0. - in_attr4.y) + utof(fs_cbuf8[29].y));
	
	f_2_16 = clamp((pf_1_8 + (0. - 0.)), 0.0, 1.0);
	
	pf_1_11 = ((pf_4_0 * pf_4_0) + (pf_3_0 * pf_3_0));
	
	f_3_11 = inversesqrt(((pf_0_6 * pf_0_6) + pf_1_11));
	
	pf_0_8 = (((pf_0_6 * f_3_11) * utof(fs_cbuf15[28].z)) + (((pf_4_0 * f_3_11) * utof(fs_cbuf15[28].y)) + ((pf_3_0 * f_3_11) * utof(fs_cbuf15[28].x))));
	
	f_4_8 = clamp(((pf_0_8 * 2.) + (0. - 1.)), 0.0, 1.0);
	
	pf_0_11 = ((f_4_8 * in_attr5.y) * 0.005);
	
	f_6_1 = (0. - (1.0 / pf_0_11));
	
	pf_0_12 = (((1.0 / pf_0_11) * (pf_0_11 + ((f4_0_4.w * in_attr8.z) * in_attr0.w))) + f_6_1);
	
	pf_0_13 = ((0. - f_4_8) + 1.);
	
	pf_2_3 = ((clamp(pf_0_12, 0.0, 1.0) + ((pf_0_13 * (0. - clamp(pf_0_12, 0.0, 1.0))) + pf_0_13)) * -5.);
	
	pf_1_19 = (((((0. - utof(fs_cbuf9[189].y)) + utof(fs_cbuf9[190].y)) * in_attr8.x) + utof(fs_cbuf9[189].y)) * log2(abs(f4_0_4.x)));
	
	pf_0_16 = (f_4_8 * (clamp(pf_0_12, 0.0, 1.0) + ((pf_0_13 * (0. - clamp(pf_0_12, 0.0, 1.0))) + pf_0_13)));
	
	f_3_20 = max(0., (exp2(pf_2_3) + -0.03125));
	
	pf_4_2 = (((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - utof(fs_cbuf15[44].y))) + utof(fs_cbuf15[43].y));
	
	pf_0_18 = ((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * utof(fs_cbuf15[44].y));
	
	pf_3_3 = (((in_attr8.x * ((0. - utof(fs_cbuf9[190].z)) + utof(fs_cbuf9[190].w))) + utof(fs_cbuf9[190].z)) * exp2(pf_1_19));
	
	pf_5_1 = ((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * utof(fs_cbuf15[44].x));
	
	pf_6_0 = ((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * utof(fs_cbuf15[44].z));
	
	pf_4_3 = (((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - utof(fs_cbuf15[44].z))) + utof(fs_cbuf15[43].z));
	
	pf_2_6 = (((((clamp(f4_0_4.x, 0.1, 0.3) + -0.1) * 3.3499994) + 0.33) * (0. - utof(fs_cbuf15[44].x))) + utof(fs_cbuf15[43].x));
	
	pf_2_7 = ((f4_0_4.x * pf_2_6) + pf_5_1);
	
	pf_5_2 = ((pf_3_3 * utof(fs_cbuf15[42].x)) * utof(fs_cbuf15[42].w));
	
	pf_6_1 = ((pf_3_3 * utof(fs_cbuf15[42].y)) * utof(fs_cbuf15[42].w));
	
	pf_3_5 = ((pf_3_3 * utof(fs_cbuf15[42].z)) * utof(fs_cbuf15[42].w));
	
	pf_6_2 = ((min((((0. - f_4_8) + 1.05) * (1.0 / ((pf_0_16 * f_3_20) + 1.))), 1.) * (0. - utof(fs_cbuf15[57].w))) + utof(fs_cbuf15[57].w));
	
	pf_1_23 = (((pf_5_2 * (0. - utof(fs_cbuf15[1].x))) + pf_5_2) + pf_2_7);
	
	pf_0_20 = (((pf_6_1 * (0. - utof(fs_cbuf15[1].x))) + pf_6_1) + ((f4_0_4.x * pf_4_2) + pf_0_18));
	
	pf_2_8 = (((pf_3_5 * (0. - utof(fs_cbuf15[1].x))) + pf_3_5) + ((f4_0_4.x * pf_4_3) + pf_6_0));
	
	pf_3_7 = ((pf_6_2 * -0.9) + 1.);
	
	f_8_2 = (0. - (pf_3_7 * pf_1_23));
	
	f_9_2 = (0. - (pf_3_7 * pf_0_20));
	
	f_9_3 = (0. - (pf_3_7 * pf_2_8));
	
	pf_1_26 = (((((f_8_2 + utof(fs_cbuf15[26].x)) * in_attr7.y) + (pf_3_7 * pf_1_23)) * in_attr9.w) + in_attr9.x);
	
	pf_0_23 = (((((f_9_2 + utof(fs_cbuf15[26].y)) * in_attr7.y) + (pf_3_7 * pf_0_20)) * in_attr9.w) + in_attr9.y);
	
	pf_2_11 = (((((f_9_3 + utof(fs_cbuf15[26].z)) * in_attr7.y) + (pf_3_7 * pf_2_8)) * in_attr9.w) + in_attr9.z);
	
	pf_2_12 = ((((0. - pf_2_11) + in_attr11.z) * in_attr11.w) + pf_2_11);
	
	pf_0_24 = ((((0. - pf_0_23) + in_attr11.y) * in_attr11.w) + pf_0_23);
	
	pf_1_27 = ((((0. - pf_1_26) + in_attr11.x) * in_attr11.w) + pf_1_26);
	
	pf_3_11 = ((0. - ((((0. - pf_1_27) + utof(fs_cbuf15[25].x)) * in_attr7.x) + pf_1_27)) + in_attr10.x);
	
	pf_4_8 = ((0. - ((((0. - pf_0_24) + utof(fs_cbuf15[25].y)) * in_attr7.x) + pf_0_24)) + in_attr10.y);
	
	pf_5_7 = ((0. - ((((0. - pf_2_12) + utof(fs_cbuf15[25].z)) * in_attr7.x) + pf_2_12)) + in_attr10.z);
	
	frag_color0.x = ((pf_3_11 * in_attr10.w) + ((((0. - pf_1_27) + utof(fs_cbuf15[25].x)) * in_attr7.x) + pf_1_27));
	
	frag_color0.y = ((pf_4_8 * in_attr10.w) + ((((0. - pf_0_24) + utof(fs_cbuf15[25].y)) * in_attr7.x) + pf_0_24));
	
	frag_color0.z = ((pf_5_7 * in_attr10.w) + ((((0. - pf_2_12) + utof(fs_cbuf15[25].z)) * in_attr7.x) + pf_2_12));
	
	frag_color0.w = f_2_16;
	return;
}
