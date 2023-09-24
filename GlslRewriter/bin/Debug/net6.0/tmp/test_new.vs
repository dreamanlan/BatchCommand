
out gl_PerVertex {
    vec4 gl_Position;
};
layout(location = 0) in vec4 in_attr0; 
layout(location = 1) in vec4 in_attr1; 
layout(location = 4) in vec4 in_attr4; 
layout(location = 5) in vec4 in_attr5; 
layout(location = 6) in vec4 in_attr6; 
layout(location = 7) in vec4 in_attr7; 
layout(location = 9) in vec4 in_attr9; 
layout(location = 10) in vec4 in_attr10; 
layout(location = 11) in vec4 in_attr11; 

layout(location = 0) out vec4 out_attr0; 
layout(location = 1) out vec4 out_attr1; 
layout(location = 2) out vec4 out_attr2; 
layout(location = 3) out vec4 out_attr3; 
layout(location = 4) out vec4 out_attr4; 
layout(location = 5) out vec4 out_attr5; 
layout(location = 6) out vec4 out_attr6; 
layout(location = 7) out vec4 out_attr7; 
layout(location = 8) out vec4 out_attr8; 
layout(location = 9) out vec4 out_attr9; 
layout(location = 10) out vec4 out_attr10; 
layout(location = 11) out vec4 out_attr11; 

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

layout(binding = 0) uniform sampler2D tex0; 
layout(binding = 1) uniform sampler2D tex1; 
layout(binding = 2) uniform sampler2D tex2; 



void main()
{
in_attr0.x = float(0.00);
in_attr0.y = float(0.00);
in_attr0.z = float(0.00);
in_attr0.w = float(1.00);
in_attr1.x = float(0.50);
in_attr1.y = float(0.50);
in_attr1.z = float(12.00);
in_attr1.w = float(1.00);
in_attr4.x = float(277.38879);
in_attr4.y = float(-253.98657);
in_attr4.z = float(-1225.84583);
in_attr4.w = float(1000.00);
in_attr5.x = float(0.00);
in_attr5.y = float(0.00);
in_attr5.z = float(0.00);
in_attr5.w = float(1060.50);
in_attr6.x = float(256.74911);
in_attr6.y = float(175.44524);
in_attr6.z = float(175.44524);
in_attr6.w = float(1.00);
in_attr7.x = float(0.33394);
in_attr7.y = float(0.62269);
in_attr7.z = float(0.01691);
in_attr7.w = float(0.7787);
in_attr9.x = float(1.00);
in_attr9.y = float(0.00);
in_attr9.z = float(0.00);
in_attr9.w = float(287.3111);
in_attr10.x = float(0.00);
in_attr10.y = float(1.00);
in_attr10.z = float(0.00);
in_attr10.w = float(1522.14368);
in_attr11.x = float(0.00);
in_attr11.y = float(0.00);
in_attr11.z = float(1.00);
in_attr11.w = float(1396.66956);
gl_Position.x = float(166.45822);
gl_Position.y = float(777.81085);
gl_Position.z = float(1259.94666);
gl_Position.w = float(1261.84583);
out_attr0.x = float(0.00);
out_attr0.y = float(0.00);
out_attr0.z = float(0.00);
out_attr0.w = float(1.00);
out_attr1.x = float(0.50);
out_attr1.y = float(0.50);
out_attr1.z = float(1.46396);
out_attr1.w = float(3.26268);
out_attr2.x = float(714.15204);
out_attr2.y = float(242.01749);
out_attr2.z = float(1260.89624);
out_attr2.w = float(1261.84583);
out_attr3.x = float(1.00);
out_attr3.y = float(0.00);
out_attr3.z = float(0.00);
out_attr3.w = float(1.00);
out_attr4.x = float(564.69989);
out_attr4.y = float(1268.1571);
out_attr4.z = float(170.82373);
out_attr4.w = float(1.00);
out_attr5.x = float(0.00);
out_attr5.y = float(175.44524);
out_attr5.z = float(0.00);
out_attr5.w = float(1.00);
out_attr6.x = float(2.00);
out_attr6.y = float(0.00);
out_attr6.z = float(0.00);
out_attr6.w = float(1.00);
out_attr7.x = float(0.12);
out_attr7.y = float(0.00);
out_attr7.z = float(0.00);
out_attr7.w = float(1.00);
out_attr8.x = float(1.00);
out_attr8.y = float(0.00);
out_attr8.z = float(1.00);
out_attr8.w = float(1.00);
out_attr9.x = float(0.40435);
out_attr9.y = float(0.39195);
out_attr9.z = float(0.50397);
out_attr9.w = float(0.52491);
out_attr10.x = float(4.77344);
out_attr10.y = float(3.10352);
out_attr10.z = float(1.75391);
out_attr10.w = float(0.00);
out_attr11.x = float(0.20251);
out_attr11.y = float(0.44214);
out_attr11.z = float(0.8335);
out_attr11.w = float(0.00);
vs_cbuf8[0] = uvec4(1065247121, 860928284, 1038480600, 3286050967);
vs_cbuf8[1] = uvec4(1031828066, 1062491979, 3205371645, 3290264139);
vs_cbuf8[2] = uvec4(3183394095, 1057947259, 1062403977, 3305154163);
vs_cbuf8[3] = uvec4(0, 0, 0, 1065353216);
vs_cbuf8[4] = uvec4(1067083659, 0, 0, 0);
vs_cbuf8[5] = uvec4(0, 1074347930, 0, 0);
vs_cbuf8[6] = uvec4(0, 0, 3212837535, 3221225807);
vs_cbuf8[7] = uvec4(0, 0, 3212836864, 0);
vs_cbuf8[29] = uvec4(1133488083, 1154548889, 1152292204, 0);
vs_cbuf8[30] = uvec4(1065353216, 1187205120, 1187205120, 1187204608);
vs_cbuf9[11] = uvec4(0, 0, 0, 0);
vs_cbuf9[12] = uvec4(0, 0, 0, 0);
vs_cbuf9[16] = uvec4(0, 0, 0, 1065353216);
vs_cbuf9[113] = uvec4(0, 0, 0, 0);
vs_cbuf9[114] = uvec4(1065353216, 1065353216, 1065353216, 1036831949);
vs_cbuf9[115] = uvec4(1065353216, 1065353216, 1065353216, 1063675494);
vs_cbuf9[116] = uvec4(0, 0, 0, 1065353216);
vs_cbuf9[141] = uvec4(1065353216, 1065353216, 1065353216, 0);
vs_cbuf10[0] = uvec4(1065353216, 1065353216, 1065353216, 1065353216);
vs_cbuf10[2] = uvec4(1154273280, 1126105088, 1065353216, 1065353216);
vs_cbuf10[3] = uvec4(1065353166, 1065353216, 1065353216, 1065353216);
vs_cbuf13[6] = uvec4(1065353216, 1065353216, 1092616192, 0);
vs_cbuf15[1] = uvec4(0, 1065353216, 1072865060, 1065353216);
vs_cbuf15[22] = uvec4(942622237, 994030768, 0, 0);
vs_cbuf15[23] = uvec4(1110048768, 1075838976, 1062836634, 3166425518);
vs_cbuf15[24] = uvec4(995783694, 0, 1058239677, 1082130432);
vs_cbuf15[25] = uvec4(1060018062, 1065194672, 1059307546, 1039516303);
vs_cbuf15[26] = uvec4(1066362785, 1067991434, 1059750465, 1055353662);
vs_cbuf15[27] = uvec4(3183095435, 999617033, 1132068864, 0);
vs_cbuf15[28] = uvec4(1057344769, 3205691469, 3206633754, 0);
vs_cbuf15[54] = uvec4(1062228420, 1071140278, 1117126656, 1157234688);
vs_cbuf15[55] = uvec4(1062855138, 1062417203, 1065863112, 1060320051);
vs_cbuf15[57] = uvec4(3314801541, 1147334299, 1161527296, 1065353216);
vs_cbuf15[60] = uvec4(1061158912, 1056964608, 1082130432, 1154548889);
vs_cbuf15[61] = uvec4(1065353216, 0, 0, 0);
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	bool b_0_2;
	bool b_0_5;
	bool b_1_10;
	bool b_1_11;
	bool b_1_12;
	bool b_1_13;
	bool b_1_14;
	bool b_1_8;
	bool b_1_9;
	float f_0_12;
	float f_1_10;
	float f_1_18;
	float f_1_51;
	float f_1_56;
	float f_1_65;
	float f_1_70;
	float f_11_3;
	float f_11_4;
	float f_15_12;
	float f_15_15;
	float f_15_17;
	float f_16_12;
	float f_16_6;
	float f_16_8;
	float f_17_0;
	float f_17_10;
	float f_2_42;
	float f_2_50;
	float f_2_63;
	float f_2_64;
	float f_3_32;
	float f_3_42;
	float f_3_44;
	float f_3_49;
	float f_3_57;
	float f_3_61;
	float f_4_28;
	float f_4_43;
	float f_4_45;
	float f_4_46;
	float f_4_52;
	float f_4_57;
	float f_4_58;
	float f_4_70;
	float f_4_79;
	float f_4_8;
	float f_4_81;
	float f_5_13;
	float f_5_18;
	float f_5_21;
	float f_5_25;
	float f_5_28;
	float f_5_29;
	float f_6_12;
	float f_6_7;
	float f_8_1;
	float f_9_2;
	vec2 f2_0_1;
	vec2 f2_0_2;
	vec4 f4_0_0;
	vec4 f4_0_1;
	vec4 f4_0_2;
	vec4 f4_0_3;
	vec4 f4_0_4;
	vec4 f4_0_5;
	precise float pf_0_1;
	precise float pf_0_11;
	precise float pf_0_12;
	precise float pf_0_25;
	precise float pf_0_29;
	precise float pf_0_3;
	precise float pf_0_31;
	precise float pf_0_33;
	precise float pf_0_34;
	precise float pf_0_4;
	precise float pf_1_11;
	precise float pf_1_15;
	precise float pf_1_16;
	precise float pf_1_24;
	precise float pf_1_25;
	precise float pf_1_26;
	precise float pf_1_4;
	precise float pf_1_5;
	precise float pf_1_6;
	precise float pf_1_8;
	precise float pf_1_9;
	precise float pf_10_1;
	precise float pf_10_10;
	precise float pf_10_14;
	precise float pf_10_17;
	precise float pf_10_18;
	precise float pf_10_4;
	precise float pf_10_6;
	precise float pf_10_7;
	precise float pf_10_9;
	precise float pf_11_10;
	precise float pf_11_13;
	precise float pf_11_15;
	precise float pf_11_3;
	precise float pf_11_4;
	precise float pf_11_5;
	precise float pf_11_8;
	precise float pf_11_9;
	precise float pf_12_2;
	precise float pf_12_6;
	precise float pf_12_8;
	precise float pf_13_1;
	precise float pf_13_10;
	precise float pf_13_11;
	precise float pf_13_15;
	precise float pf_13_5;
	precise float pf_13_6;
	precise float pf_13_7;
	precise float pf_14_13;
	precise float pf_14_2;
	precise float pf_14_4;
	precise float pf_14_6;
	precise float pf_14_7;
	precise float pf_14_9;
	precise float pf_15_11;
	precise float pf_15_14;
	precise float pf_15_2;
	precise float pf_15_20;
	precise float pf_15_6;
	precise float pf_15_8;
	precise float pf_16_10;
	precise float pf_16_2;
	precise float pf_16_3;
	precise float pf_16_4;
	precise float pf_17_3;
	precise float pf_17_5;
	precise float pf_17_6;
	precise float pf_17_9;
	precise float pf_18_1;
	precise float pf_18_10;
	precise float pf_18_2;
	precise float pf_18_6;
	precise float pf_18_9;
	precise float pf_19_3;
	precise float pf_19_6;
	precise float pf_19_7;
	precise float pf_2_0;
	precise float pf_2_3;
	precise float pf_2_4;
	precise float pf_2_5;
	precise float pf_20_0;
	precise float pf_20_11;
	precise float pf_20_17;
	precise float pf_20_19;
	precise float pf_20_5;
	precise float pf_20_6;
	precise float pf_20_9;
	precise float pf_21_7;
	precise float pf_22_0;
	precise float pf_23_0;
	precise float pf_23_6;
	precise float pf_24_1;
	precise float pf_25_0;
	precise float pf_25_1;
	precise float pf_25_3;
	precise float pf_26_1;
	precise float pf_3_11;
	precise float pf_3_12;
	precise float pf_3_2;
	precise float pf_3_4;
	precise float pf_4_10;
	precise float pf_4_11;
	precise float pf_4_13;
	precise float pf_4_2;
	precise float pf_4_21;
	precise float pf_4_4;
	precise float pf_4_6;
	precise float pf_5_3;
	precise float pf_5_5;
	precise float pf_5_6;
	precise float pf_6_11;
	precise float pf_6_19;
	precise float pf_6_4;
	precise float pf_6_6;
	precise float pf_6_7;
	precise float pf_6_8;
	precise float pf_7_0;
	precise float pf_7_11;
	precise float pf_7_12;
	precise float pf_7_13;
	precise float pf_7_15;
	precise float pf_7_17;
	precise float pf_7_19;
	precise float pf_7_27;
	precise float pf_7_5;
	precise float pf_7_6;
	precise float pf_7_7;
	precise float pf_8_0;
	precise float pf_8_10;
	precise float pf_8_2;
	precise float pf_8_4;
	precise float pf_8_5;
	precise float pf_8_6;
	precise float pf_8_8;
	precise float pf_9_11;
	precise float pf_9_13;
	precise float pf_9_14;
	precise float pf_9_15;
	precise float pf_9_17;
	precise float pf_9_2;
	precise float pf_9_21;
	precise float pf_9_3;
	precise float pf_9_4;
	precise float pf_9_5;
	precise float pf_9_9;
	uint u_0_1;
	uint u_0_11;
	uint u_0_12;
	uint u_0_13;
	uint u_0_2;
	uint u_0_4;
	uint u_0_5;
	uint u_0_6;
	uint u_0_7;
	uint u_0_phi_11;
	uint u_0_phi_15;
	uint u_0_phi_4;
	uint u_1_1;
	uint u_1_3;
	uint u_1_4;
	uint u_1_phi_16;
	uint u_2_0;
	uint u_2_1;
	uint u_2_5;
	uint u_2_6;
	uint u_2_phi_2;
	uint u_3_2;
	uint u_3_3;
	uint u_3_4;
	uint u_3_5;
	uint u_3_7;
	uint u_3_phi_15;
	uint u_3_phi_9;
	uint u_4_2;
	uvec2 u2_0_0;
	
	gl_Position.x = float(166.45822);
	
	gl_Position.y = float(777.81085);
	
	gl_Position.z = float(1259.94666);
	
	gl_Position.w = float(1261.84583);
	
	out_attr0.x = float(0.00);
	
	out_attr0.y = float(0.00);
	
	out_attr0.z = float(0.00);
	
	out_attr0.w = float(1.00);
	
	out_attr1.x = float(0.50);
	
	out_attr1.y = float(0.50);
	
	out_attr1.z = float(1.46396);
	
	out_attr1.w = float(3.26268);
	
	out_attr2.x = float(714.15204);
	
	out_attr2.y = float(242.01749);
	
	out_attr2.z = float(1260.89624);
	
	out_attr2.w = float(1261.84583);
	
	out_attr3.x = float(1.00);
	
	out_attr3.y = float(0.00);
	
	out_attr3.z = float(0.00);
	
	out_attr3.w = float(1.00);
	
	out_attr4.x = float(564.69989);
	
	out_attr4.y = float(1268.1571);
	
	out_attr4.z = float(170.82373);
	
	out_attr4.w = float(1.00);
	
	out_attr5.x = float(0.00);
	
	out_attr5.y = float(175.44524);
	
	out_attr5.z = float(0.00);
	
	out_attr5.w = float(1.00);
	
	out_attr6.x = float(2.00);
	
	out_attr6.y = float(0.00);
	
	out_attr6.z = float(0.00);
	
	out_attr6.w = float(1.00);
	
	out_attr7.x = float(0.12);
	
	out_attr7.y = float(0.00);
	
	out_attr7.z = float(0.00);
	
	out_attr7.w = float(1.00);
	
	out_attr8.x = float(1.00);
	
	out_attr8.y = float(0.00);
	
	out_attr8.z = float(1.00);
	
	out_attr8.w = float(1.00);
	
	out_attr9.x = float(0.40435);
	
	out_attr9.y = float(0.39195);
	
	out_attr9.z = float(0.50397);
	
	out_attr9.w = float(0.52491);
	
	out_attr10.x = float(4.77344);
	
	out_attr10.y = float(3.10352);
	
	out_attr10.z = float(1.75391);
	
	out_attr10.w = float(0.00);
	
	out_attr11.x = float(0.20251);
	
	out_attr11.y = float(0.44214);
	
	out_attr11.z = float(0.8335);
	
	out_attr11.w = float(0.00);
	
	u_1_1 = (isnan(in_attr4.w) ? ( 0u ) : ( int(clamp(trunc(in_attr4.w), float(-2147483600.), float(2147483600.)))));
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		
		gl_Position.x = 0.;
	}
	
	u_2_0 = 0u;
	u_2_phi_2 = u_2_0;
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		
		u_2_1 = vs_cbuf8[30].y;
		u_2_phi_2 = u_2_1;
	}
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		
		gl_Position.y = 0.;
	}
	
	u_0_1 = u_2_phi_2;
	u_0_phi_4 = u_0_1;
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		
		u_0_2 = ftou((utof(u_2_phi_2) * 5.));
		u_0_phi_4 = u_0_2;
	}
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		
		out_attr3.x = 0.;
	}
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		
		gl_Position.z = utof(u_0_phi_4);
	}
	
	if(((int(u_1_1) <= int(0u)) ? ( true ) : ( false)))
	{
		return;
	}
	
	pf_0_1 = ((0. - in_attr5.w) + utof(vs_cbuf10[2].x));
	
	b_0_2 = (((pf_0_1 >= float(int(u_1_1))) && (! isnan(pf_0_1))) && (! isnan(float(int(u_1_1)))));
	
	b_1_8 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	if(b_1_8)
	{
		
		gl_Position.x = 0.;
	}
	
	b_1_9 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	u_3_2 = ftou(in_attr5.w);
	u_3_phi_9 = u_3_2;
	
	if(b_1_9)
	{
		
		u_3_3 = vs_cbuf8[30].y;
		u_3_phi_9 = u_3_3;
	}
	
	b_1_10 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	if(b_1_10)
	{
		
		gl_Position.y = 0.;
	}
	
	b_1_11 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	u_0_4 = u_3_phi_9;
	u_0_phi_11 = u_0_4;
	
	if(b_1_11)
	{
		
		u_0_5 = ftou((utof(u_3_phi_9) * 5.));
		u_0_phi_11 = u_0_5;
	}
	
	b_1_12 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	if(b_1_12)
	{
		
		out_attr3.x = 0.;
	}
	
	b_1_13 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	if(b_1_13)
	{
		
		gl_Position.z = utof(u_0_phi_11);
	}
	
	b_1_14 = (((((in_attr5.w > utof(vs_cbuf10[2].x)) && (! isnan(in_attr5.w))) && (! isnan(utof(vs_cbuf10[2].x)))) || b_0_2) ? ( true ) : ( false));
	
	if(b_1_14)
	{
		return;
	}
	
	pf_2_0 = (clamp((min(0., in_attr7.x) + (0. - 0.)), 0.0, 1.0) + in_attr6.x);
	
	u_0_6 = ftou(pf_0_1);
	
	u_3_4 = ftou(float(int(u_1_1)));
	u_0_phi_15 = u_0_6;
	u_3_phi_15 = u_3_4;
	
	if(((((0. < utof(vs_cbuf9[11].y)) && (! isnan(0.))) && (! isnan(utof(vs_cbuf9[11].y)))) ? ( true ) : ( false)))
	{
		
		pf_0_3 = (((in_attr7.x * utof(vs_cbuf9[12].z)) * utof(vs_cbuf9[11].y) + pf_0_1) * (1.0 / utof(vs_cbuf9[11].y)));
		
		u_4_2 = ftou(pf_0_3);
		
		f_1_10 = floor(pf_0_3);
		
		pf_0_4 = (pf_0_3 + (0. - f_1_10));
		
		u_0_7 = ftou(pf_0_4);
		
		u_3_5 = u_4_2;
		u_0_phi_15 = u_0_7;
		u_3_phi_15 = u_3_5;
	}
	
	b_0_5 = ((! (((0. < utof(vs_cbuf9[11].y)) && (! isnan(0.))) && (! isnan(utof(vs_cbuf9[11].y))))) ? ( true ) : ( false));
	
	u_1_3 = u_0_phi_15;
	u_1_phi_16 = u_1_3;
	
	if(b_0_5)
	{
		
		u_1_4 = ftou((utof(u_0_phi_15) * (1.0 / utof(u_3_phi_15))));
		u_1_phi_16 = u_1_4;
	}
	
	pf_4_2 = ((utof(vs_cbuf9[114].x) + (0. - utof(vs_cbuf9[113].x))) * (1.0 / ((0. - utof(vs_cbuf9[113].w)) + utof(vs_cbuf9[114].w))));
	
	f_8_1 = clamp((floor((in_attr7.x * 8.)) * 0.1429), 0.0, 1.0);
	
	u_2_5 = (isnan(in_attr1.z) ? ( 0u ) : ( int(clamp(trunc(in_attr1.z), float(-2147483600.), float(2147483600.)))));
	
	pf_7_0 = (float(int(uvec4(uvec2(textureSize(tex0, int(0u))), 0u, 0u).x)) + -1.);
	
	pf_8_0 = (pf_7_0 * f_8_1);
	
	u_3_7 = (isnan(pf_8_0) ? ( 0u ) : ( int(clamp(trunc(pf_8_0), float(-2147483600.), float(2147483600.)))));
	
	u_0_11 = min(int((isnan(pf_7_0) ? ( 0u ) : ( int(clamp(trunc(pf_7_0), float(-2147483600.), float(2147483600.)))))), int((u_3_7 + 1u)));
	
	u2_0_0 = uvec2(u_3_7, u_2_5);
	
	f4_0_0 = texelFetch(tex0, ivec2(u2_0_0), int(0u));
	
	f_1_18 = f4_0_0.x;
	
	f4_0_1 = texelFetch(tex0, ivec2(uvec2(u_0_11, u_2_5)), int(0u));
	
	f_9_2 = f4_0_1.y;
	
	u_0_12 = ((((utof(u_1_phi_16) >= utof(vs_cbuf9[113].w)) && (! isnan(utof(u_1_phi_16)))) && (! isnan(utof(vs_cbuf9[113].w)))) ? ( 1065353216u ) : ( 0u));
	
	out_attr6.x = floor((in_attr7.x * 8.));
	
	u_2_6 = ((((utof(u_1_phi_16) >= utof(vs_cbuf9[114].w)) && (! isnan(utof(u_1_phi_16)))) && (! isnan(utof(vs_cbuf9[114].w)))) ? ( 1065353216u ) : ( 0u));
	
	f_4_8 = utof(u_0_12);
	
	pf_10_1 = ((utof(vs_cbuf9[116].x) + (0. - utof(vs_cbuf9[115].x))) * (1.0 / ((0. - utof(vs_cbuf9[115].w)) + utof(vs_cbuf9[116].w))));
	
	pf_9_2 = (((0. - utof(vs_cbuf9[114].x)) + utof(vs_cbuf9[115].x)) * (1.0 / (utof(vs_cbuf9[115].w) + (0. - utof(vs_cbuf9[114].w)))));
	
	pf_4_4 = ((pf_4_2 * (utof(u_1_phi_16) + (0. - utof(vs_cbuf9[113].w))) + utof(vs_cbuf9[113].x)) * (utof(u_2_6) * (0. - f_4_8) + utof(u_0_12)) + (utof(u_0_12) * (0. - utof(vs_cbuf9[113].x)) + utof(vs_cbuf9[113].x)));
	
	pf_7_5 = (utof(vs_cbuf8[2].z) * utof(vs_cbuf8[3].w) + (0. - (utof(vs_cbuf8[2].w) * utof(vs_cbuf8[3].z))));
	
	pf_13_1 = (utof(vs_cbuf8[2].x) * utof(vs_cbuf8[3].z) + (0. - (utof(vs_cbuf8[2].z) * utof(vs_cbuf8[3].x))));
	
	pf_6_4 = ((in_attr4.z * in_attr9.z + (in_attr4.y * in_attr9.y + (in_attr4.x * in_attr9.x))) + in_attr9.w);
	
	pf_20_0 = (utof(vs_cbuf8[2].y) * utof(vs_cbuf8[3].z) + (0. - (utof(vs_cbuf8[2].z) * utof(vs_cbuf8[3].y))));
	
	u_0_13 = ((((utof(u_1_phi_16) >= utof(vs_cbuf9[115].w)) && (! isnan(utof(u_1_phi_16)))) && (! isnan(utof(vs_cbuf9[115].w)))) ? ( 1065353216u ) : ( 0u));
	
	pf_11_3 = ((utof(vs_cbuf8[2].z) * utof(vs_cbuf8[3].x) + (0. - (utof(vs_cbuf8[2].x) * utof(vs_cbuf8[3].z)))) * utof(vs_cbuf8[1].y));
	
	pf_5_3 = ((in_attr4.z * in_attr10.z + (in_attr4.y * in_attr10.y + (in_attr4.x * in_attr10.x))) + in_attr10.w);
	
	pf_22_0 = ((0. - pf_6_4) + utof(vs_cbuf8[29].x));
	
	pf_9_3 = (pf_9_2 * (utof(u_1_phi_16) + (0. - utof(vs_cbuf9[114].w))) + utof(vs_cbuf9[114].x));
	
	f_3_32 = utof(u_0_13);
	
	f_4_28 = utof(u_2_6);
	
	pf_18_1 = (utof(vs_cbuf8[2].w) * utof(vs_cbuf8[3].x) + (0. - (utof(vs_cbuf8[2].x) * utof(vs_cbuf8[3].w))));
	
	pf_23_0 = ((0. - pf_5_3) + utof(vs_cbuf8[29].y));
	
	pf_0_11 = ((in_attr4.z * in_attr11.z + (in_attr4.y * in_attr11.y + (in_attr4.x * in_attr11.x))) + in_attr11.w);
	
	pf_9_4 = ((utof(vs_cbuf8[2].y) * utof(vs_cbuf8[3].w) + (0. - (utof(vs_cbuf8[2].w) * utof(vs_cbuf8[3].y)))) * utof(vs_cbuf8[1].x));
	
	pf_11_4 = ((utof(vs_cbuf8[3].z) * utof(vs_cbuf8[2].y) + (0. - (utof(vs_cbuf8[2].z) * utof(vs_cbuf8[3].y)))) * utof(vs_cbuf8[1].x) + pf_11_3);
	
	pf_24_1 = ((0. - pf_0_11) + utof(vs_cbuf8[29].z));
	
	pf_25_0 = (pf_13_1 * utof(vs_cbuf8[1].w));
	
	pf_9_5 = ((utof(vs_cbuf8[2].x) * utof(vs_cbuf8[3].y) + (0. - (utof(vs_cbuf8[3].x) * utof(vs_cbuf8[2].y)))) * utof(vs_cbuf8[1].w) + pf_9_4);
	
	pf_12_2 = ((utof(vs_cbuf8[2].w) * utof(vs_cbuf8[3].y) + (0. - (utof(vs_cbuf8[2].y) * utof(vs_cbuf8[3].w)))) * utof(vs_cbuf8[0].z) + (pf_7_5 * utof(vs_cbuf8[0].y)));
	
	pf_15_2 = ((utof(vs_cbuf8[2].w) * utof(vs_cbuf8[3].y) + (0. - (utof(vs_cbuf8[3].w) * utof(vs_cbuf8[2].y)))) * utof(vs_cbuf8[1].z));
	
	pf_17_3 = (pf_24_1 * pf_24_1 + (pf_23_0 * pf_23_0 + (pf_22_0 * pf_22_0)));
	
	pf_16_2 = ((utof(vs_cbuf8[2].y) * utof(vs_cbuf8[3].w) + (0. - (utof(vs_cbuf8[2].w) * utof(vs_cbuf8[3].y)))) * utof(vs_cbuf8[0].x));
	
	pf_11_5 = ((utof(vs_cbuf8[2].x) * utof(vs_cbuf8[3].y) + (0. - (utof(vs_cbuf8[2].y) * utof(vs_cbuf8[3].x)))) * utof(vs_cbuf8[1].z) + pf_11_4);
	
	pf_7_6 = (pf_7_5 * utof(vs_cbuf8[1].x) + (pf_18_1 * utof(vs_cbuf8[1].z) + pf_25_0));
	
	pf_16_3 = (pf_18_1 * utof(vs_cbuf8[0].y) + pf_16_2);
	
	pf_17_5 = (pf_23_0 * inversesqrt(pf_17_3));
	
	out_attr5.y = ((in_attr6.y * utof(vs_cbuf9[141].y)) * utof(vs_cbuf10[3].z));
	
	pf_18_2 = (pf_24_1 * inversesqrt(pf_17_3));
	
	pf_19_3 = (pf_22_0 * inversesqrt(pf_17_3));
	
	pf_14_2 = ((utof(vs_cbuf8[2].x) * utof(vs_cbuf8[3].y) + (0. - (utof(vs_cbuf8[2].y) * utof(vs_cbuf8[3].x)))) * utof(vs_cbuf8[0].w) + pf_16_3);
	
	pf_16_4 = (0. * pf_17_5 + (0. - pf_18_2));
	
	pf_7_7 = (pf_7_6 * (0. - utof(vs_cbuf8[0].y)) + ((pf_20_0 * utof(vs_cbuf8[1].w) + (pf_7_5 * utof(vs_cbuf8[1].y) + pf_15_2)) * utof(vs_cbuf8[0].x)));
	
	pf_15_6 = ((0. - 0.) + pf_19_3);
	
	f_2_42 = inversesqrt((pf_15_6 * pf_15_6 + (pf_16_4 * pf_16_4 + 0.)));
	
	f_4_43 = (1.0 / (pf_11_5 * (0. - utof(vs_cbuf8[0].w)) + ((pf_18_1 * utof(vs_cbuf8[1].y) + pf_9_5) * utof(vs_cbuf8[0].z) + pf_7_7)));
	
	pf_15_8 = (pf_18_2 * (0. * f_2_42) + (0. - (pf_17_5 * (pf_15_6 * f_2_42))));
	
	pf_7_11 = (pf_19_3 * (pf_15_6 * f_2_42) + (0. - (pf_18_2 * (pf_16_4 * f_2_42))));
	
	pf_9_9 = (pf_17_5 * (pf_16_4 * f_2_42) + (0. - (pf_19_3 * (0. * f_2_42))));
	
	pf_11_8 = (pf_7_11 * pf_7_11 + (pf_15_8 * pf_15_8));
	
	pf_11_9 = (pf_9_9 * pf_9_9 + pf_11_8);
	
	f_2_50 = inversesqrt(pf_11_9);
	
	pf_11_10 = ((pf_13_1 * utof(vs_cbuf8[0].w) + (pf_18_1 * utof(vs_cbuf8[0].z) + (pf_7_5 * utof(vs_cbuf8[0].x)))) * f_4_43);
	
	f_4_45 = utof(u_0_13);
	
	f_5_13 = utof(((((utof(u_1_phi_16) >= utof(vs_cbuf9[116].w)) && (! isnan(utof(u_1_phi_16)))) && (! isnan(utof(vs_cbuf9[116].w)))) ? ( 1065353216u ) : ( 0u)));
	
	f_11_3 = utof(u_0_13);
	
	f_4_46 = (0. - (pf_17_5 * ((pf_20_0 * utof(vs_cbuf8[0].w) + pf_12_2) * (0. - f_4_43))));
	
	pf_4_6 = ((pf_10_1 * (utof(u_1_phi_16) + (0. - utof(vs_cbuf9[115].w))) + utof(vs_cbuf9[115].x)) * (f_11_3 * (0. - f_5_13) + f_4_45) + (pf_9_3 * (f_4_28 * (0. - f_3_32) + utof(u_2_6)) + pf_4_4));
	
	pf_7_12 = (pf_7_11 * f_2_50);
	
	pf_14_4 = (pf_18_2 * ((pf_20_0 * utof(vs_cbuf8[0].w) + pf_12_2) * (0. - f_4_43)) + (0. - (pf_19_3 * (pf_14_2 * (0. - f_4_43)))));
	
	pf_10_4 = (pf_17_5 * (pf_14_2 * (0. - f_4_43)) + (0. - (pf_18_2 * pf_11_10)));
	
	pf_20_5 = ((0. - clamp((pf_7_12 + (0. - 0.)), 0.0, 1.0)) + 1.);
	
	f_11_4 = utof(((((utof(u_1_phi_16) >= utof(vs_cbuf9[116].w)) && (! isnan(utof(u_1_phi_16)))) && (! isnan(utof(vs_cbuf9[116].w)))) ? ( 1065353216u ) : ( 0u)));
	
	pf_20_6 = (pf_14_4 * pf_14_4 + (pf_10_4 * pf_10_4));
	
	f_4_52 = inversesqrt(((pf_19_3 * pf_11_10 + f_4_46) * (pf_19_3 * pf_11_10 + f_4_46) + pf_20_6));
	
	f_5_18 = exp2((log2(pf_20_5) * utof(vs_cbuf13[6].y)));
	
	pf_25_1 = ((0. - f_5_18) + 1.);
	
	pf_20_9 = (pf_17_5 * (pf_9_9 * f_2_50) + (0. - (pf_18_2 * pf_7_12)));
	
	pf_10_6 = ((pf_10_4 * f_4_52) * f_5_18);
	
	pf_14_6 = ((pf_14_4 * f_4_52) * f_5_18);
	
	pf_21_7 = (pf_18_2 * (pf_15_8 * f_2_50) + (0. - (pf_19_3 * (pf_9_9 * f_2_50))));
	
	pf_12_6 = ((pf_15_8 * f_2_50) * pf_25_1 + (((pf_20_0 * utof(vs_cbuf8[0].w) + pf_12_2) * (0. - f_4_43)) * f_5_18));
	
	pf_16_10 = (((pf_19_3 * pf_11_10 + f_4_46) * f_4_52) * f_5_18);
	
	pf_15_11 = (pf_19_3 * pf_7_12 + (0. - (pf_17_5 * (pf_15_8 * f_2_50))));
	
	pf_26_1 = (pf_21_7 * pf_21_7 + (pf_20_9 * pf_20_9));
	
	pf_9_11 = ((pf_9_9 * f_2_50) * pf_25_1 + ((pf_14_2 * (0. - f_4_43)) * f_5_18));
	
	pf_13_5 = (pf_15_11 * pf_15_11 + pf_26_1);
	
	pf_13_6 = (pf_15_11 * inversesqrt(pf_13_5));
	
	pf_7_13 = (pf_7_12 * pf_25_1 + (pf_11_10 * f_5_18));
	
	pf_13_7 = (pf_13_6 * pf_25_1 + pf_16_10);
	
	pf_14_7 = ((pf_21_7 * inversesqrt(pf_13_5)) * pf_25_1 + pf_14_6);
	
	pf_11_13 = (pf_19_3 * pf_25_1 + (pf_19_3 * f_5_18));
	
	out_attr0.w = ((f_11_4 * utof(vs_cbuf9[116].x) + pf_4_6) * utof(vs_cbuf10[0].w));
	
	pf_10_7 = ((pf_20_9 * inversesqrt(pf_13_5)) * pf_25_1 + pf_10_6);
	
	pf_15_14 = (pf_18_2 * pf_25_1 + (pf_18_2 * f_5_18));
	
	pf_4_10 = (pf_9_11 * pf_9_11 + (pf_7_13 * pf_7_13 + (pf_12_6 * pf_12_6)));
	
	pf_4_11 = (pf_17_5 * pf_25_1 + (pf_17_5 * f_5_18));
	
	pf_17_6 = (pf_14_7 * pf_14_7 + (pf_10_7 * pf_10_7));
	
	f_5_21 = (0. - f4_0_0.y);
	
	pf_20_11 = ((0. - f4_0_0.z) + f4_0_1.z);
	
	pf_17_9 = (((0. - f_1_18) + f4_0_1.x) * (pf_8_0 + (0. - floor(pf_8_0))) + f_1_18);
	
	pf_19_6 = ((f_5_21 + f_9_2) * (pf_8_0 + (0. - floor(pf_8_0))) + f4_0_0.y);
	
	pf_18_6 = (pf_15_14 * pf_15_14 + (pf_4_11 * pf_4_11 + (pf_11_13 * pf_11_13)));
	
	pf_8_2 = (pf_20_11 * (pf_8_0 + (0. - floor(pf_8_0))) + f4_0_0.z);
	
	pf_2_3 = (((pf_2_0 * utof(vs_cbuf9[141].x)) * utof(vs_cbuf10[3].y)) * (0.5 * utof(vs_cbuf9[16].x) + pf_17_9));
	
	pf_1_4 = (((in_attr6.y * utof(vs_cbuf9[141].y)) * utof(vs_cbuf10[3].z)) * (0.5 * utof(vs_cbuf9[16].y) + pf_19_6));
	
	pf_3_2 = (pf_8_2 * ((in_attr6.z * utof(vs_cbuf9[141].z)) * utof(vs_cbuf10[3].w)));
	
	pf_10_9 = (pf_2_3 * (inversesqrt((pf_13_7 * pf_13_7 + pf_17_6)) * pf_10_7));
	
	pf_14_9 = (pf_2_3 * (inversesqrt((pf_13_7 * pf_13_7 + pf_17_6)) * pf_14_7));
	
	pf_2_4 = (pf_2_3 * (inversesqrt((pf_13_7 * pf_13_7 + pf_17_6)) * pf_13_7));
	
	pf_8_4 = (pf_1_4 * (inversesqrt(pf_4_10) * pf_12_6) + pf_10_9);
	
	pf_7_15 = (pf_1_4 * (inversesqrt(pf_4_10) * pf_7_13) + pf_14_9);
	
	pf_1_5 = (pf_1_4 * (inversesqrt(pf_4_10) * pf_9_11) + pf_2_4);
	
	pf_2_5 = (pf_3_2 * (inversesqrt(pf_18_6) * pf_11_13) + pf_8_4);
	
	pf_4_13 = (pf_3_2 * (inversesqrt(pf_18_6) * pf_4_11) + pf_7_15);
	
	pf_1_6 = (pf_3_2 * (inversesqrt(pf_18_6) * pf_15_14) + pf_1_5);
	
	pf_0_12 = (pf_0_11 + (pf_1_6 + (0. * pf_25_1 + (0. * f_5_18))));
	
	pf_1_8 = ((pf_6_4 + (pf_2_5 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[0].x));
	
	pf_8_5 = ((pf_6_4 + (pf_2_5 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[1].x));
	
	pf_6_6 = ((pf_22_0 * (1.0 / (sqrt(pf_17_3) + float(1e-05)))) * utof(vs_cbuf15[28].x));
	
	pf_9_13 = ((pf_6_4 + (pf_2_5 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[2].x));
	
	f_4_57 = (0. - (pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18)))));
	
	pf_10_10 = (f_4_57 + utof(vs_cbuf15[60].w));
	
	f_4_58 = (0. - (pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18)))));
	
	pf_11_15 = (f_4_58 + utof(vs_cbuf8[29].y));
	
	pf_1_9 = ((pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[0].y) + pf_1_8);
	
	pf_8_6 = ((pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[1].y) + pf_8_5);
	
	pf_12_8 = ((pf_23_0 * (1.0 / (sqrt(pf_17_3) + float(1e-05)))) * 0.5);
	
	pf_13_10 = ((pf_6_4 + (pf_2_5 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[3].x));
	
	pf_9_14 = ((pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[2].y) + pf_9_13);
	
	pf_6_7 = (pf_12_8 * utof(vs_cbuf15[28].y) + pf_6_6);
	
	pf_13_11 = ((pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18)))) * utof(vs_cbuf8[3].y) + pf_13_10);
	
	pf_1_11 = ((pf_0_12 * utof(vs_cbuf8[0].z) + pf_1_9) + utof(vs_cbuf8[0].w));
	
	pf_9_15 = (pf_0_12 * utof(vs_cbuf8[2].z) + pf_9_14);
	
	pf_8_8 = ((pf_0_12 * utof(vs_cbuf8[1].z) + pf_8_6) + utof(vs_cbuf8[1].w));
	
	pf_6_8 = ((pf_24_1 * (1.0 / (sqrt(pf_17_3) + float(1e-05)))) * utof(vs_cbuf15[28].z) + pf_6_7);
	
	pf_7_17 = (pf_0_12 * utof(vs_cbuf8[3].z) + pf_13_11);
	
	pf_13_15 = ((pf_7_17 + utof(vs_cbuf8[3].w)) * utof(vs_cbuf8[5].w) + ((pf_9_15 + utof(vs_cbuf8[2].w)) * utof(vs_cbuf8[5].z) + (pf_8_8 * utof(vs_cbuf8[5].y) + (pf_1_11 * utof(vs_cbuf8[5].x)))));
	
	pf_14_13 = ((pf_7_17 + utof(vs_cbuf8[3].w)) * utof(vs_cbuf8[4].w) + ((pf_9_15 + utof(vs_cbuf8[2].w)) * utof(vs_cbuf8[4].z) + (pf_8_8 * utof(vs_cbuf8[4].y) + (pf_1_11 * utof(vs_cbuf8[4].x)))));
	
	f_5_25 = (0. - (pf_6_4 + (pf_2_5 + (0. * pf_25_1 + (0. * f_5_18)))));
	
	pf_9_17 = (f_5_25 + utof(vs_cbuf8[29].x));
	
	pf_8_10 = ((pf_7_17 + utof(vs_cbuf8[3].w)) * utof(vs_cbuf8[6].w) + ((pf_9_15 + utof(vs_cbuf8[2].w)) * utof(vs_cbuf8[6].z) + (pf_8_8 * utof(vs_cbuf8[6].y) + (pf_1_11 * utof(vs_cbuf8[6].x)))));
	
	pf_1_15 = ((pf_7_17 + utof(vs_cbuf8[3].w)) * utof(vs_cbuf8[7].w) + ((pf_9_15 + utof(vs_cbuf8[2].w)) * utof(vs_cbuf8[7].z) + (pf_8_8 * utof(vs_cbuf8[7].y) + (pf_1_11 * utof(vs_cbuf8[7].x)))));
	
	pf_7_19 = (pf_9_17 * pf_9_17);
	
	f_1_51 = (0. - sqrt(((0. - (pf_6_8 * 0.5 + 0.5)) + 1.)));
	
	pf_6_11 = (((pf_6_8 * 0.5 + 0.5) * ((pf_6_8 * 0.5 + 0.5) * ((pf_6_8 * 0.5 + 0.5) * -0.0187293 + 0.074260995) + -0.2121144) + 1.5707288) * f_1_51);
	
	pf_19_7 = ((0. - pf_0_12) + utof(vs_cbuf8[29].z));
	
	f_5_28 = inversesqrt((pf_19_7 * pf_19_7 + (pf_10_10 * pf_10_10 + pf_7_19)));
	
	f_6_7 = (1.0 / (pf_1_15 + (0. * pf_8_10 + (0. * pf_14_13 + (0. * pf_13_15)))));
	
	f_4_70 = inversesqrt((pf_19_7 * pf_19_7 + (pf_11_15 * pf_11_15 + pf_7_19)));
	
	pf_15_20 = ((pf_1_15 * 0.5 + (pf_8_10 * 0.5 + (0. * pf_14_13 + (0. * pf_13_15)))) * f_6_7);
	
	pf_18_9 = ((0. - (pf_19_6 + (0. - in_attr0.y))) + in_attr1.y);
	
	out_attr1.y = pf_18_9;
	
	f_1_56 = (0. - ((pf_17_9 + (0. - in_attr0.x)) + in_attr1.x));
	
	pf_18_10 = ((1.0 / (pf_15_20 * utof(vs_cbuf8[30].w) + (0. - utof(vs_cbuf8[30].y)))) * (0. - utof(vs_cbuf8[30].z)));
	
	pf_9_21 = ((pf_19_7 * f_4_70) * utof(vs_cbuf15[28].z) + ((pf_11_15 * f_4_70) * utof(vs_cbuf15[28].y) + ((pf_9_17 * f_4_70) * utof(vs_cbuf15[28].x))));
	
	pf_20_17 = (((pf_19_7 * f_5_28) * utof(vs_cbuf15[28].z) + ((pf_10_10 * f_5_28) * utof(vs_cbuf15[28].y) + ((pf_9_17 * f_5_28) * utof(vs_cbuf15[28].x)))) * 0.5 + 0.5);
	
	f_1_65 = (0. - clamp((pf_18_10 * utof(vs_cbuf15[22].x) + (0. - utof(vs_cbuf15[22].y))), 0.0, 1.0));
	
	pf_25_3 = ((pf_23_0 * (1.0 / (sqrt(pf_17_3) + float(1e-05)))) * 3.3333333);
	
	pf_20_19 = ((pf_20_17 * (pf_20_17 * (pf_20_17 * -0.0187293 + 0.074260995) + -0.2121144) + 1.5707288) * (0. - sqrt(((0. - pf_20_17) + 1.))));
	
	f_0_12 = exp2((log2((f_1_65 + 1.)) * utof(vs_cbuf15[23].y)));
	
	f_2_63 = (0. - sqrt(((0. - (pf_9_21 * 0.5 + 0.5)) + 1.)));
	
	pf_23_6 = (((pf_9_21 * 0.5 + 0.5) * ((pf_9_21 * 0.5 + 0.5) * ((pf_9_21 * 0.5 + 0.5) * -0.0187293 + 0.074260995) + -0.2121144) + 1.5707288) * f_2_63);
	
	f4_0_2 = textureLod(tex2, vec2(0.5, (clamp(max(pf_25_3, 0.0), 0.5, 1.0) * -0.1 + 0.68)), 0.0);
	
	f_2_64 = f4_0_2.x;
	
	f_5_29 = f4_0_2.y;
	
	f_6_12 = f4_0_2.z;
	
	f2_0_1 = vec2((pf_6_11 * 0.63661975 + 1.), (pf_12_8 * 0.5 + 0.5));
	
	f4_0_3 = textureLod(tex2, f2_0_1, 0.0);
	
	f2_0_2 = vec2((pf_20_19 * 0.63661975 + 1.), ((pf_10_10 * f_5_28) * -0.5 + 0.5));
	
	f4_0_4 = textureLod(tex2, f2_0_2, 0.0);
	
	f4_0_5 = textureLod(tex1, vec2((pf_23_6 * 0.63661975 + 1.), (f_0_12 * 0.5 + 0.5)), 0.0);
	
	out_attr1.w = (((pf_18_9 + in_attr7.w) + utof(vs_cbuf15[54].y)) * (in_attr7.y * 0.4 + 0.85));
	
	pf_10_14 = (((pf_19_7 * f_5_28) * utof(vs_cbuf15[28].z) + ((pf_10_10 * f_5_28) * utof(vs_cbuf15[28].y) + ((pf_9_17 * f_5_28) * utof(vs_cbuf15[28].x)))) + (0. - utof(vs_cbuf15[60].y)));
	
	out_attr4.z = pf_0_12;
	
	out_attr1.x = (f_1_56 + 1.);
	
	out_attr4.x = (pf_6_4 + (pf_2_5 + (0. * pf_25_1 + (0. * f_5_18))));
	
	f_17_0 = sqrt((pf_22_0 * pf_22_0 + (pf_24_1 * pf_24_1)));
	
	gl_Position.x = pf_14_13;
	
	out_attr4.y = (pf_5_3 + (pf_4_13 + (0. * pf_25_1 + (0. * f_5_18))));
	
	f_15_12 = (0. - clamp((sqrt(pf_17_3) * utof(vs_cbuf15[22].x) + (0. - utof(vs_cbuf15[22].y))), 0.0, 1.0));
	
	out_attr1.z = ((((f_1_56 + 1.) + in_attr7.z) + utof(vs_cbuf15[54].x)) * (in_attr7.x * 0.3 + 1.));
	
	gl_Position.y = pf_13_15;
	
	gl_Position.z = pf_8_10;
	
	out_attr8.z = clamp((f_17_0 * 0.006666667 + (0. - 1.)), 0.0, 1.0);
	
	gl_Position.w = pf_1_15;
	
	out_attr2.z = (pf_1_15 * 0.5 + (pf_8_10 * 0.5 + (0. * pf_14_13 + (0. * pf_13_15))));
	
	out_attr2.w = (pf_1_15 + (0. * pf_8_10 + (0. * pf_14_13 + (0. * pf_13_15))));
	
	out_attr8.x = clamp(max(pf_25_3, 0.0), 0.5, 1.0);
	
	f_15_15 = clamp(((pf_5_3 + (0. - utof(vs_cbuf15[60].w))) * 0.1), 0.0, 1.0);
	
	f_16_6 = (0. - clamp((sqrt(pf_17_3) * utof(vs_cbuf15[24].x) + (0. - utof(vs_cbuf15[24].y))), 0.0, 1.0));
	
	pf_10_17 = ((clamp((pf_10_14 * utof(vs_cbuf15[60].z)), 0.0, 1.0) * clamp((sqrt(pf_17_3) * 0.001 + (0. - 0.5)), 0.0, 1.0)) * f_15_15);
	
	pf_10_18 = (pf_10_17 * utof(vs_cbuf15[60].x));
	
	f_15_17 = exp2((log2((f_1_65 + 1.)) * utof(vs_cbuf15[23].x)));
	
	f_16_8 = exp2((log2((f_15_12 + 1.)) * utof(vs_cbuf15[23].x)));
	
	f_3_42 = clamp(((sqrt(pf_17_3) + (0. - utof(vs_cbuf15[54].w))) * (1.0 / utof(vs_cbuf15[57].z))), 0.0, 1.0);
	
	out_attr2.x = (pf_1_15 * 0.5 + (0. * pf_8_10 + (pf_14_13 * 0.5 + (0. * pf_13_15))));
	
	pf_3_4 = ((pf_23_0 * (1.0 / (sqrt(pf_17_3) + float(1e-05)))) * 2.);
	
	out_attr10.w = (pf_10_18 * (0. - utof(vs_cbuf15[1].x)) + pf_10_18);
	
	f_3_44 = exp2((log2((f_16_6 + 1.)) * utof(vs_cbuf15[24].w)));
	
	f_16_12 = clamp(((pf_9_21 + (0. - utof(vs_cbuf15[60].y))) * utof(vs_cbuf15[60].z)), 0.0, 1.0);
	
	f_17_10 = min(((sqrt(pf_17_3) + (0. - utof(vs_cbuf15[54].z))) * (1.0 / utof(vs_cbuf15[54].w))), utof(vs_cbuf15[55].w));
	
	pf_1_16 = (f_16_12 * clamp((f_15_17 * (0. - utof(vs_cbuf15[23].z)) + utof(vs_cbuf15[23].z)), 0.0, 1.0));
	
	out_attr2.y = (pf_1_15 * 0.5 + (0. * pf_8_10 + (0. * pf_14_13 + (pf_13_15 * -0.5))));
	
	f_1_70 = max(0., f_17_10);
	
	out_attr7.x = (f_3_44 * (0. - utof(vs_cbuf15[25].w)) + utof(vs_cbuf15[25].w));
	
	pf_0_25 = (clamp((f_16_8 * (0. - utof(vs_cbuf15[23].z)) + utof(vs_cbuf15[23].z)), 0.0, 1.0) * f_1_70);
	
	f_3_49 = clamp((((0. - pf_5_3) + min(utof(vs_cbuf8[29].y), utof(vs_cbuf15[27].z))) * utof(vs_cbuf15[27].y) + utof(vs_cbuf15[27].x)), 0.0, 1.0);
	
	out_attr11.w = ((pf_1_16 * (0. - utof(vs_cbuf15[1].x)) + pf_1_16) * utof(vs_cbuf15[61].x));
	
	out_attr7.y = (f_3_49 * utof(vs_cbuf15[26].w));
	
	pf_0_29 = ((pf_18_10 + utof(vs_cbuf15[28].y)) * (1.0 / clamp((utof(vs_cbuf15[28].y) * 1.5 + 1.5), 0.0, 1.0)));
	
	out_attr10.x = f4_0_4.x;
	
	out_attr10.y = f4_0_4.y;
	
	out_attr10.z = f4_0_4.z;
	
	pf_3_11 = (((f4_0_3.y + (0. - utof(vs_cbuf15[55].y))) * (clamp(pf_3_4, 0.0, 1.0) * f_3_42) + utof(vs_cbuf15[55].y)) * clamp(clamp(pf_25_3, 0.0, 1.0),  0.5, pf_0_25));
	
	pf_4_21 = (((f4_0_3.x + (0. - utof(vs_cbuf15[55].x))) * (clamp(pf_3_4, 0.0, 1.0) * f_3_42) + utof(vs_cbuf15[55].x)) * clamp(clamp(pf_25_3, 0.0, 1.0),  0.5, pf_0_25));
	
	f_4_79 = abs(max((f_6_12 * 0.06 + (f_2_64 * 0.22 + (f_5_29 * 0.72))), 1.));
	
	f_3_57 = (1.0 / max((f_6_12 * 0.06 + (f_2_64 * 0.22 + (f_5_29 * 0.72))), 1.));
	
	out_attr3.x = utof(vs_cbuf10[3].x);
	
	f_4_81 = (0. - ((clamp(max(pf_25_3, 0.0), 0.5, 1.0) * (0. - f_1_70) + f_1_70) * f_3_57));
	
	f_3_61 = exp2((log2(f_4_79) * 0.7));
	
	pf_1_24 = (((f4_0_3.z + (0. - utof(vs_cbuf15[55].z))) * (clamp(pf_3_4, 0.0, 1.0) * f_3_42) + utof(vs_cbuf15[55].z)) * clamp(clamp(pf_25_3, 0.0, 1.0),  0.5, pf_0_25));
	
	pf_5_5 = (((0. - clamp(clamp(pf_25_3, 0.0, 1.0),  0.5, pf_0_25)) + f_4_81) + 1.);
	
	out_attr9.w = pf_5_5;
	
	pf_5_6 = (f_5_29 * (1.0 / f_3_61));
	
	pf_6_19 = (f_2_64 * (1.0 / f_3_61));
	
	pf_7_27 = (f_6_12 * (1.0 / f_3_61));
	
	pf_3_12 = (pf_5_6 * ((clamp(max(pf_25_3, 0.0), 0.5, 1.0) * (0. - f_1_70) + f_1_70) * f_3_57) + pf_3_11);
	
	out_attr9.y = pf_3_12;
	
	pf_0_31 = (pf_6_19 * ((clamp(max(pf_25_3, 0.0), 0.5, 1.0) * (0. - f_1_70) + f_1_70) * f_3_57) + pf_4_21);
	
	pf_1_25 = (pf_7_27 * ((clamp(max(pf_25_3, 0.0), 0.5, 1.0) * (0. - f_1_70) + f_1_70) * f_3_57) + pf_1_24);
	
	out_attr9.x = pf_0_31;
	
	out_attr9.z = pf_1_25;
	
	pf_0_33 = (f4_0_5.x * clamp((max((pf_0_29 * 0.06666667), 0.2) + (0. - 0.)), 0.0, 1.0));
	
	pf_1_26 = (f4_0_5.y * clamp((max((pf_0_29 * 0.06666667), 0.2) + (0. - 0.)), 0.0, 1.0));
	
	out_attr11.x = pf_0_33;
	
	pf_0_34 = (f4_0_5.z * clamp((max((pf_0_29 * 0.06666667), 0.2) + (0. - 0.)), 0.0, 1.0));
	
	out_attr11.y = pf_1_26;
	
	out_attr11.z = pf_0_34;
	return;
}
