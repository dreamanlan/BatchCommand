void frag_from_glsl(v2f i, inout float4 col)
{
	// 1065353216 = 1.00f;
	// fs_cbuf8[30] = float4(0.10, 25000.00, 2500.00, 24999.90);
	// fs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// fs_cbuf9[140] = float4(0, 100.00, 0, 0);
	// fs_cbuf13[0] = float4(0, 1.00, 1.00, 1.00);
	// fs_cbuf15[1] = float4(0, 0, 2.594247, 0.20);
	// fs_cbuf15[25] = float4(0.0282744, 0.0931012, 0.1164359, 0.7006614);
	// fs_cbuf15[26] = float4(0.0174636, 0.1221582, 0.2193998, 0.20);

	bool b_0_0;
	float f_0_6;
	float f_2_2;
	float f_2_3;
	float f_3_2;
	float f_4_4;
	float f_4_8;
	float4 f4_0_2;
	float4 f4_0_3;
	float4 f4_0_4;
	float4 f4_0_5;
	float4 f4_0_6;
	precise float pf_0_5;
	precise float pf_1_10;
	precise float pf_1_12;
	precise float pf_1_7;
	precise float pf_1_9;
	precise float pf_2_3;
	precise float pf_2_5;
	precise float pf_2_6;
	precise float pf_2_8;
	precise float pf_3_10;
	precise float pf_3_12;
	precise float pf_3_3;
	precise float pf_3_7;
	precise float pf_3_9;
	precise float pf_4_7;
	precise float pf_5_0;
	precise float pf_5_4;
	precise float pf_6_2;
	uint u_0_3;
	uint u_0_4;
	uint u_0_phi_5;
	uint u_2_1;
	uint u_2_2;
	uint u_2_phi_3;
	uint u_3_1;
	uint u_3_2;
	uint u_3_phi_4;
	uint u_4_0;
	uint u_4_1;
	uint u_4_phi_2;
	// 0.0000034  <=>  (1.0f / ({i.fs_attr4.w : 540.2476} * {i.vertex.w : 540.2476}))
	f_4_4 = (1.0f / (i.fs_attr4.w * i.vertex.w));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex3 : tex3}, float2({i.fs_attr2.z : -0.05795}, {i.fs_attr2.w : -0.30511}), min((float((uint({float4(textureQueryLod({tex3 : tex3}, float2({i.fs_attr2.z : -0.05795}, {i.fs_attr2.w : -0.30511}), s_linear_clamp_sampler), 0.0, 0.0).y : 1.00}) << 8u)) * 0.00390625f), 2.f), s_linear_clamp_sampler)
	f4_0_2 = textureLod(tex3, float2(i.fs_attr2.z, i.fs_attr2.w), min((float((uint(float4(textureQueryLod(tex3, float2(i.fs_attr2.z, i.fs_attr2.w), s_linear_clamp_sampler), 0.0, 0.0).y) << 8u)) * 0.00390625f), 2.f), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex2 : tex2}, float2({i.fs_attr2.x : 0.9785}, {i.fs_attr2.y : 0.50196}), min((float((uint({float4(textureQueryLod({tex2 : tex2}, float2({i.fs_attr2.x : 0.9785}, {i.fs_attr2.y : 0.50196}), s_linear_clamp_sampler), 0.0, 0.0).y : 1.00}) << 8u)) * 0.00390625f), 2.f), s_linear_clamp_sampler)
	f4_0_3 = textureLod(tex2, float2(i.fs_attr2.x, i.fs_attr2.y), min((float((uint(float4(textureQueryLod(tex2, float2(i.fs_attr2.x, i.fs_attr2.y), s_linear_clamp_sampler), 0.0, 0.0).y) << 8u)) * 0.00390625f), 2.f), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex4 : tex4}, float2({i.fs_attr3.x : 0.50196}, {i.fs_attr3.y : -2.49902}), s_linear_clamp_sampler)
	f4_0_4 = textureSample(tex4, float2(i.fs_attr3.x, i.fs_attr3.y), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({_CameraDepthTexture : _CameraDepthTexture}, float2((({i.fs_attr4.x : -187.33215} * {i.vertex.w : 540.2476}) * {f_4_4 : 0.0000034}), (({i.fs_attr4.y : 255.7101} * {i.vertex.w : 540.2476}) * {f_4_4 : 0.0000034})), s_linear_clamp_sampler)
	f4_0_5 = textureSample(_CameraDepthTexture, float2(((i.fs_attr4.x * i.vertex.w) * f_4_4), ((i.fs_attr4.y * i.vertex.w) * f_4_4)), s_linear_clamp_sampler);
	// 0  <=>  ((clamp(((((1.0f / ((({i.fs_attr4.z : 540.1497} * (1.0f / {i.fs_attr4.w : 540.2476})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_5.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 100.00})), 0.0, 1.0) * clamp((((0.f - {f4_0_4.w : 0.75}) + ({f4_0_3.w : 0.75} + (0.f - {f4_0_2.w : 0.75}))) * {i.fs_attr0.w : 1.00}), 0.0, 1.0)) * {i.fs_attr5.x : 0.3291})
	pf_0_5 = ((clamp(((((1.0f / (((i.fs_attr4.z * (1.0f / i.fs_attr4.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_5.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0) * clamp((((0.f - f4_0_4.w) + (f4_0_3.w + (0.f - f4_0_2.w))) * i.fs_attr0.w), 0.0, 1.0)) * i.fs_attr5.x);
	// 0  <=>  {(fs_cbuf9_139.z) : 0}
	f_4_8 = (fs_cbuf9_139.z);
	// True  <=>  ((({pf_0_5 : 0} <= {f_4_8 : 0}) && (! isnan({pf_0_5 : 0}))) && (! isnan({f_4_8 : 0})))
	b_0_0 = (((pf_0_5 <= f_4_8) && (! isnan(pf_0_5))) && (! isnan(f_4_8)));
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		discard;
	}
	// 1051230156  <=>  {ftou(i.fs_attr5.x) : 1051230156}
	u_4_0 = ftou(i.fs_attr5.x);
	u_4_phi_2 = u_4_0;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_4_1 = 0u;
		u_4_phi_2 = u_4_1;
	}
	// 0  <=>  {ftou((clamp(((((1.0f / ((({i.fs_attr4.z : 540.1497} * (1.0f / {i.fs_attr4.w : 540.2476})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_5.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 100.00})), 0.0, 1.0) * clamp((((0.f - {f4_0_4.w : 0.75}) + ({f4_0_3.w : 0.75} + (0.f - {f4_0_2.w : 0.75}))) * {i.fs_attr0.w : 1.00}), 0.0, 1.0))) : 0}
	u_2_1 = ftou((clamp(((((1.0f / (((i.fs_attr4.z * (1.0f / i.fs_attr4.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_5.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0) * clamp((((0.f - f4_0_4.w) + (f4_0_3.w + (0.f - f4_0_2.w))) * i.fs_attr0.w), 0.0, 1.0)));
	u_2_phi_3 = u_2_1;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_2_2 = 0u;
		u_2_phi_3 = u_2_2;
	}
	// 1061158912  <=>  {ftou(f4_0_4.w) : 1061158912}
	u_3_1 = ftou(f4_0_4.w);
	u_3_phi_4 = u_3_1;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_3_2 = 0u;
		u_3_phi_4 = u_3_2;
	}
	// 1008981770  <=>  {ftou((1.0f / {(fs_cbuf9_140.y) : 100.00})) : 1008981770}
	u_0_3 = ftou((1.0f / (fs_cbuf9_140.y)));
	u_0_phi_5 = u_0_3;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_0_4 = 0u;
		u_0_phi_5 = u_0_4;
	}
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  {utof(u_4_phi_2) : 0}
		col.x = utof(u_4_phi_2);
		// 0  <=>  {utof(u_2_phi_3) : 0}
		col.y = utof(u_2_phi_3);
		// 0  <=>  {utof(u_3_phi_4) : 0}
		col.z = utof(u_3_phi_4);
		// 0  <=>  {utof(u_0_phi_5) : 0}
		col.w = utof(u_0_phi_5);
		return;
	}
	// 0  <=>  ({i.fs_attr6.y : 0} * inversesqrt((({i.fs_attr6.z : -0.74257} * {i.fs_attr6.z : -0.74257}) + (({i.fs_attr6.y : 0} * {i.fs_attr6.y : 0}) + ({i.fs_attr6.x : -0.66977} * {i.fs_attr6.x : -0.66977})))))
	pf_3_3 = (i.fs_attr6.y * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))));
	// -0.74256927  <=>  ({i.fs_attr6.z : -0.74257} * inversesqrt((({i.fs_attr6.z : -0.74257} * {i.fs_attr6.z : -0.74257}) + (({i.fs_attr6.y : 0} * {i.fs_attr6.y : 0}) + ({i.fs_attr6.x : -0.66977} * {i.fs_attr6.x : -0.66977})))))
	pf_5_0 = (i.fs_attr6.z * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))));
	// 1.346676  <=>  (1.0f / max(abs({pf_5_0 : -0.74256927}), max(abs({pf_3_3 : 0}), abs(({i.fs_attr6.x : -0.66977} * inversesqrt((({i.fs_attr6.z : -0.74257} * {i.fs_attr6.z : -0.74257}) + (({i.fs_attr6.y : 0} * {i.fs_attr6.y : 0}) + ({i.fs_attr6.x : -0.66977} * {i.fs_attr6.x : -0.66977})))))))))
	f_0_6 = (1.0f / max(abs(pf_5_0), max(abs(pf_3_3), abs((i.fs_attr6.x * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))))))));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex6 : tex6}, float3((({i.fs_attr6.x : -0.66977} * inversesqrt((({i.fs_attr6.z : -0.74257} * {i.fs_attr6.z : -0.74257}) + (({i.fs_attr6.y : 0} * {i.fs_attr6.y : 0}) + ({i.fs_attr6.x : -0.66977} * {i.fs_attr6.x : -0.66977}))))) * {f_0_6 : 1.346676}), ({pf_3_3 : 0} * {f_0_6 : 1.346676}), ({pf_5_0 : -0.74256927} * (0.f - {f_0_6 : 1.346676}))), {(fs_cbuf15_1.x) : 0}, s_linear_clamp_sampler)
	f4_0_6 = textureLod(tex6, float3(((i.fs_attr6.x * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x))))) * f_0_6), (pf_3_3 * f_0_6), (pf_5_0 * (0.f - f_0_6))), (fs_cbuf15_1.x), s_linear_clamp_sampler);
	// 0.1241  <=>  {i.fs_attr1.x : 0.1241}
	f_2_2 = i.fs_attr1.x;
	// 0.16799  <=>  (((({f4_0_3.x : 0.50} * {f4_0_3.x : 0.50}) + ({f4_0_2.x : 0.50} * {f4_0_2.x : 0.50})) * ((0.f - {f_2_2 : 0.1241}) + {i.fs_attr0.x : 0.21188})) + {f_2_2 : 0.1241})
	pf_1_7 = ((((f4_0_3.x * f4_0_3.x) + (f4_0_2.x * f4_0_2.x)) * ((0.f - f_2_2) + i.fs_attr0.x)) + f_2_2);
	// 0.13335  <=>  {i.fs_attr1.y : 0.13335}
	f_2_3 = i.fs_attr1.y;
	// 0.14087  <=>  {i.fs_attr1.z : 0.14087}
	f_3_2 = i.fs_attr1.z;
	// 0.18076  <=>  (((({f4_0_3.y : 0.50} * {f4_0_3.y : 0.50}) + ({f4_0_2.y : 0.50} * {f4_0_2.y : 0.50})) * ((0.f - {f_2_3 : 0.13335}) + {i.fs_attr0.y : 0.22817})) + {f_2_3 : 0.13335})
	pf_2_3 = ((((f4_0_3.y * f4_0_3.y) + (f4_0_2.y * f4_0_2.y)) * ((0.f - f_2_3) + i.fs_attr0.y)) + f_2_3);
	// 0.18433  <=>  (((({f4_0_3.z : 0.50} * {f4_0_3.z : 0.50}) + ({f4_0_2.z : 0.50} * {f4_0_2.z : 0.50})) * ((0.f - {f_3_2 : 0.14087}) + {i.fs_attr0.z : 0.22779})) + {f_3_2 : 0.14087})
	pf_3_7 = ((((f4_0_3.z * f4_0_3.z) + (f4_0_2.z * f4_0_2.z)) * ((0.f - f_3_2) + i.fs_attr0.z)) + f_3_2);
	// 0.18741  <=>  ((((({f4_0_6.x : 0.50} * (0.f - {i.fs_attr8.w : 0})) + {f4_0_6.x : 0.50}) * {(fs_cbuf13_0.y) : 1.00}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.x : 0.08741})
	pf_4_7 = (((((f4_0_6.x * (0.f - i.fs_attr8.w)) + f4_0_6.x) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.x);
	// 0.18741  <=>  ((((({f4_0_6.y : 0.50} * (0.f - {i.fs_attr8.w : 0})) + {f4_0_6.y : 0.50}) * {(fs_cbuf13_0.y) : 1.00}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.y : 0.08741})
	pf_5_4 = (((((f4_0_6.y * (0.f - i.fs_attr8.w)) + f4_0_6.y) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.y);
	// 0.18741  <=>  ((((({f4_0_6.z : 0.50} * (0.f - {i.fs_attr8.w : 0})) + {f4_0_6.z : 0.50}) * {(fs_cbuf13_0.y) : 1.00}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.z : 0.08741})
	pf_6_2 = (((((f4_0_6.z * (0.f - i.fs_attr8.w)) + f4_0_6.z) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.z);
	// 0.0289427  <=>  (((({pf_1_7 : 0.16799} * (0.f - {pf_4_7 : 0.18741})) + {(fs_cbuf15_26.x) : 0.0174636}) * {i.fs_attr8.y : 0.1812}) + ({pf_1_7 : 0.16799} * {pf_4_7 : 0.18741}))
	pf_1_9 = ((((pf_1_7 * (0.f - pf_4_7)) + (fs_cbuf15_26.x)) * i.fs_attr8.y) + (pf_1_7 * pf_4_7));
	// 0.0498729  <=>  (((({pf_2_3 : 0.18076} * (0.f - {pf_5_4 : 0.18741})) + {(fs_cbuf15_26.y) : 0.1221582}) * {i.fs_attr8.y : 0.1812}) + ({pf_2_3 : 0.18076} * {pf_5_4 : 0.18741}))
	pf_2_5 = ((((pf_2_3 * (0.f - pf_5_4)) + (fs_cbuf15_26.y)) * i.fs_attr8.y) + (pf_2_3 * pf_5_4));
	// 0.0680409  <=>  (((({pf_3_7 : 0.18433} * (0.f - {pf_6_2 : 0.18741})) + {(fs_cbuf15_26.z) : 0.2193998}) * {i.fs_attr8.y : 0.1812}) + ({pf_3_7 : 0.18433} * {pf_6_2 : 0.18741}))
	pf_3_9 = ((((pf_3_7 * (0.f - pf_6_2)) + (fs_cbuf15_26.z)) * i.fs_attr8.y) + (pf_3_7 * pf_6_2));
	// 0.0516055  <=>  ((((0.f - {pf_3_9 : 0.0680409}) + {i.fs_attr10.z : 0.00901}) * {i.fs_attr10.w : 0.27842}) + {pf_3_9 : 0.0680409})
	pf_3_10 = ((((0.f - pf_3_9) + i.fs_attr10.z) * i.fs_attr10.w) + pf_3_9);
	// 0.038429  <=>  ((((0.f - {pf_2_5 : 0.0498729}) + {i.fs_attr10.y : 0.00877}) * {i.fs_attr10.w : 0.27842}) + {pf_2_5 : 0.0498729})
	pf_2_6 = ((((0.f - pf_2_5) + i.fs_attr10.y) * i.fs_attr10.w) + pf_2_5);
	// 0.0211935  <=>  ((((0.f - {pf_1_9 : 0.0289427}) + {i.fs_attr10.x : 0.00111}) * {i.fs_attr10.w : 0.27842}) + {pf_1_9 : 0.0289427})
	pf_1_10 = ((((0.f - pf_1_9) + i.fs_attr10.x) * i.fs_attr10.w) + pf_1_9);
	// 0.0261548  <=>  (((((0.f - {pf_1_10 : 0.0211935}) + {(fs_cbuf15_25.x) : 0.0282744}) * {i.fs_attr8.x : 0.70066}) + {pf_1_10 : 0.0211935}) * {i.fs_attr7.x : 1.00})
	pf_1_12 = (((((0.f - pf_1_10) + (fs_cbuf15_25.x)) * i.fs_attr8.x) + pf_1_10) * i.fs_attr7.x);
	// 0.0767356  <=>  (((((0.f - {pf_2_6 : 0.038429}) + {(fs_cbuf15_25.y) : 0.0931012}) * {i.fs_attr8.x : 0.70066}) + {pf_2_6 : 0.038429}) * {i.fs_attr7.x : 1.00})
	pf_2_8 = (((((0.f - pf_2_6) + (fs_cbuf15_25.y)) * i.fs_attr8.x) + pf_2_6) * i.fs_attr7.x);
	// 0.0970296  <=>  (((((0.f - {pf_3_10 : 0.0516055}) + {(fs_cbuf15_25.z) : 0.1164359}) * {i.fs_attr8.x : 0.70066}) + {pf_3_10 : 0.0516055}) * {i.fs_attr7.x : 1.00})
	pf_3_12 = (((((0.f - pf_3_10) + (fs_cbuf15_25.z)) * i.fs_attr8.x) + pf_3_10) * i.fs_attr7.x);
	// 0.0261548  <=>  {pf_1_12 : 0.0261548}
	col.x = pf_1_12;
	// 0.0767356  <=>  {pf_2_8 : 0.0767356}
	col.y = pf_2_8;
	// 0.0970296  <=>  {pf_3_12 : 0.0970296}
	col.z = pf_3_12;
	// 0  <=>  clamp({pf_0_5 : 0}, 0.0, 1.0)
	col.w = clamp(pf_0_5, 0.0, 1.0);
	return;
}
