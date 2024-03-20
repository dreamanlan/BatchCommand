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
	float f_0_16;
	float f_0_2;
	float f_0_7;
	float f_3_3;
	float f_3_4;
	float f_6_5;
	float4 f4_0_0;
	float4 f4_0_1;
	float4 f4_0_2;
	float4 f4_0_3;
	precise float pf_0_7;
	precise float pf_1_4;
	precise float pf_1_6;
	precise float pf_1_7;
	precise float pf_1_9;
	precise float pf_2_2;
	precise float pf_2_4;
	precise float pf_2_5;
	precise float pf_2_7;
	precise float pf_3_10;
	precise float pf_3_12;
	precise float pf_3_3;
	precise float pf_3_7;
	precise float pf_3_9;
	precise float pf_4_6;
	precise float pf_5_0;
	precise float pf_5_5;
	precise float pf_6_2;
	uint u_1_1;
	uint u_1_2;
	uint u_1_phi_4;
	uint u_2_1;
	uint u_2_2;
	uint u_2_phi_3;
	uint u_3_1;
	uint u_3_2;
	uint u_3_phi_5;
	uint u_4_0;
	uint u_4_1;
	uint u_4_phi_2;
	// 0.0000042  <=>  (1.0f / ({i.fs_attr3.w : 489.863} * {i.vertex.w : 489.863}))
	f_0_2 = (1.0f / (i.fs_attr3.w * i.vertex.w));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex3 : tex3}, float2({i.fs_attr2.z : 0.50196}, {i.fs_attr2.w : 0.50196}), s_linear_clamp_sampler)
	f4_0_0 = textureSample(tex3, float2(i.fs_attr2.z, i.fs_attr2.w), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex2 : tex2}, float2({i.fs_attr2.x : 0.60233}, {i.fs_attr2.y : 1.42053}), s_linear_clamp_sampler)
	f4_0_1 = textureSample(tex2, float2(i.fs_attr2.x, i.fs_attr2.y), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({_CameraDepthTexture : _CameraDepthTexture}, float2((({i.fs_attr3.x : -188.93994} * {i.vertex.w : 489.863}) * {f_0_2 : 0.0000042}), (({i.fs_attr3.y : 345.5265} * {i.vertex.w : 489.863}) * {f_0_2 : 0.0000042})), s_linear_clamp_sampler)
	f4_0_2 = textureSample(_CameraDepthTexture, float2(((i.fs_attr3.x * i.vertex.w) * f_0_2), ((i.fs_attr3.y * i.vertex.w) * f_0_2)), s_linear_clamp_sampler);
	// 1.00  <=>  ((clamp(((((1.0f / ((({i.fs_attr3.z : 489.765} * (1.0f / {i.fs_attr3.w : 489.863})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_2.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 100.00})), 0.0, 1.0) * clamp(((({f4_0_1.w : 0.75} + {f4_0_0.w : 0.75}) * {i.fs_attr5.w : 1.00}) * {i.fs_attr0.w : 1.50}), 0.0, 1.0)) * {i.fs_attr4.x : 1.00})
	pf_0_7 = ((clamp(((((1.0f / (((i.fs_attr3.z * (1.0f / i.fs_attr3.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_2.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0) * clamp((((f4_0_1.w + f4_0_0.w) * i.fs_attr5.w) * i.fs_attr0.w), 0.0, 1.0)) * i.fs_attr4.x);
	// 0  <=>  {(fs_cbuf9_139.z) : 0}
	f_0_7 = (fs_cbuf9_139.z);
	// False  <=>  ((({pf_0_7 : 1.00} <= {f_0_7 : 0}) && (! isnan({pf_0_7 : 1.00}))) && (! isnan({f_0_7 : 0})))
	b_0_0 = (((pf_0_7 <= f_0_7) && (! isnan(pf_0_7))) && (! isnan(f_0_7)));
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		discard;
	}
	// 1069547520  <=>  {ftou((({f4_0_1.w : 0.75} + {f4_0_0.w : 0.75}) * {i.fs_attr5.w : 1.00})) : 1069547520}
	u_4_0 = ftou(((f4_0_1.w + f4_0_0.w) * i.fs_attr5.w));
	u_4_phi_2 = u_4_0;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_4_1 = 0u;
		u_4_phi_2 = u_4_1;
	}
	// 1008981770  <=>  {ftou((1.0f / {(fs_cbuf9_140.y) : 100.00})) : 1008981770}
	u_2_1 = ftou((1.0f / (fs_cbuf9_140.y)));
	u_2_phi_3 = u_2_1;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_2_2 = 0u;
		u_2_phi_3 = u_2_2;
	}
	// 1065353216  <=>  {ftou(clamp(((({f4_0_1.w : 0.75} + {f4_0_0.w : 0.75}) * {i.fs_attr5.w : 1.00}) * {i.fs_attr0.w : 1.50}), 0.0, 1.0)) : 1065353216}
	u_1_1 = ftou(clamp((((f4_0_1.w + f4_0_0.w) * i.fs_attr5.w) * i.fs_attr0.w), 0.0, 1.0));
	u_1_phi_4 = u_1_1;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_1_2 = 0u;
		u_1_phi_4 = u_1_2;
	}
	// 1056964608  <=>  {ftou(f4_0_2.x) : 1056964608}
	u_3_1 = ftou(f4_0_2.x);
	u_3_phi_5 = u_3_1;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_3_2 = 0u;
		u_3_phi_5 = u_3_2;
	}
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 1.50  <=>  {utof(u_4_phi_2) : 1.50}
		col.x = utof(u_4_phi_2);
		// 0.01  <=>  {utof(u_2_phi_3) : 0.01}
		col.y = utof(u_2_phi_3);
		// 1.00  <=>  {utof(u_1_phi_4) : 1.00}
		col.z = utof(u_1_phi_4);
		// 0.50  <=>  {utof(u_3_phi_5) : 0.50}
		col.w = utof(u_3_phi_5);
		return;
	}
	// 1.00  <=>  ({i.fs_attr6.y : 1.00} * inversesqrt((({i.fs_attr6.z : 0} * {i.fs_attr6.z : 0}) + (({i.fs_attr6.y : 1.00} * {i.fs_attr6.y : 1.00}) + ({i.fs_attr6.x : 0} * {i.fs_attr6.x : 0})))))
	pf_3_3 = (i.fs_attr6.y * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))));
	// 0  <=>  ({i.fs_attr6.z : 0} * inversesqrt((({i.fs_attr6.z : 0} * {i.fs_attr6.z : 0}) + (({i.fs_attr6.y : 1.00} * {i.fs_attr6.y : 1.00}) + ({i.fs_attr6.x : 0} * {i.fs_attr6.x : 0})))))
	pf_5_0 = (i.fs_attr6.z * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))));
	// 1.00  <=>  (1.0f / max(abs({pf_5_0 : 0}), max(abs({pf_3_3 : 1.00}), abs(({i.fs_attr6.x : 0} * inversesqrt((({i.fs_attr6.z : 0} * {i.fs_attr6.z : 0}) + (({i.fs_attr6.y : 1.00} * {i.fs_attr6.y : 1.00}) + ({i.fs_attr6.x : 0} * {i.fs_attr6.x : 0})))))))))
	f_0_16 = (1.0f / max(abs(pf_5_0), max(abs(pf_3_3), abs((i.fs_attr6.x * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))))))));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex5 : tex5}, float3((({i.fs_attr6.x : 0} * inversesqrt((({i.fs_attr6.z : 0} * {i.fs_attr6.z : 0}) + (({i.fs_attr6.y : 1.00} * {i.fs_attr6.y : 1.00}) + ({i.fs_attr6.x : 0} * {i.fs_attr6.x : 0}))))) * {f_0_16 : 1.00}), ({pf_3_3 : 1.00} * {f_0_16 : 1.00}), ({pf_5_0 : 0} * (0.f - {f_0_16 : 1.00}))), {(fs_cbuf15_1.x) : 0}, s_linear_clamp_sampler)
	f4_0_3 = textureLod(tex5, float3(((i.fs_attr6.x * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x))))) * f_0_16), (pf_3_3 * f_0_16), (pf_5_0 * (0.f - f_0_16))), (fs_cbuf15_1.x), s_linear_clamp_sampler);
	// 0.1291  <=>  {i.fs_attr1.x : 0.1291}
	f_3_3 = i.fs_attr1.x;
	// 0.14181  <=>  {i.fs_attr1.y : 0.14181}
	f_6_5 = i.fs_attr1.y;
	// 0.14524  <=>  {i.fs_attr1.z : 0.14524}
	f_3_4 = i.fs_attr1.z;
	// 0.1603875  <=>  (((({f4_0_1.x : 0.50} * {f4_0_1.x : 0.50}) * ((0.f - {f_3_3 : 0.1291}) + {i.fs_attr0.x : 0.25425})) + {f_3_3 : 0.1291}) * {i.fs_attr5.x : 1.00})
	pf_1_4 = ((((f4_0_1.x * f4_0_1.x) * ((0.f - f_3_3) + i.fs_attr0.x)) + f_3_3) * i.fs_attr5.x);
	// 0.17481  <=>  (((({f4_0_1.y : 0.50} * {f4_0_1.y : 0.50}) * ((0.f - {f_6_5 : 0.14181}) + {i.fs_attr0.y : 0.27381})) + {f_6_5 : 0.14181}) * {i.fs_attr5.y : 1.00})
	pf_2_2 = ((((f4_0_1.y * f4_0_1.y) * ((0.f - f_6_5) + i.fs_attr0.y)) + f_6_5) * i.fs_attr5.y);
	// 0.1772675  <=>  (((({f4_0_1.z : 0.50} * {f4_0_1.z : 0.50}) * ((0.f - {f_3_4 : 0.14524}) + {i.fs_attr0.z : 0.27335})) + {f_3_4 : 0.14524}) * {i.fs_attr5.z : 1.00})
	pf_3_7 = ((((f4_0_1.z * f4_0_1.z) * ((0.f - f_3_4) + i.fs_attr0.z)) + f_3_4) * i.fs_attr5.z);
	// 0.20241  <=>  ((((({f4_0_3.x : 0.50} * (0.f - {i.fs_attr8.w : 0})) + {f4_0_3.x : 0.50}) * {(fs_cbuf13_0.y) : 1.00}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.x : 0.10241})
	pf_4_6 = (((((f4_0_3.x * (0.f - i.fs_attr8.w)) + f4_0_3.x) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.x);
	// 0.20241  <=>  ((((({f4_0_3.y : 0.50} * (0.f - {i.fs_attr8.w : 0})) + {f4_0_3.y : 0.50}) * {(fs_cbuf13_0.y) : 1.00}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.y : 0.10241})
	pf_5_5 = (((((f4_0_3.y * (0.f - i.fs_attr8.w)) + f4_0_3.y) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.y);
	// 0.20241  <=>  ((((({f4_0_3.z : 0.50} * (0.f - {i.fs_attr8.w : 0})) + {f4_0_3.z : 0.50}) * {(fs_cbuf13_0.y) : 1.00}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.z : 0.10241})
	pf_6_2 = (((((f4_0_3.z * (0.f - i.fs_attr8.w)) + f4_0_3.z) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.z);
	// 0.0527383  <=>  (((({pf_2_2 : 0.17481} * (0.f - {pf_5_5 : 0.20241})) + {(fs_cbuf15_26.y) : 0.1221582}) * {i.fs_attr8.y : 0.20}) + ({pf_2_2 : 0.17481} * {pf_5_5 : 0.20241}))
	pf_2_4 = ((((pf_2_2 * (0.f - pf_5_5)) + (fs_cbuf15_26.y)) * i.fs_attr8.y) + (pf_2_2 * pf_5_5));
	// 0.029464  <=>  (((({pf_1_4 : 0.1603875} * (0.f - {pf_4_6 : 0.20241})) + {(fs_cbuf15_26.x) : 0.0174636}) * {i.fs_attr8.y : 0.20}) + ({pf_1_4 : 0.1603875} * {pf_4_6 : 0.20241}))
	pf_1_6 = ((((pf_1_4 * (0.f - pf_4_6)) + (fs_cbuf15_26.x)) * i.fs_attr8.y) + (pf_1_4 * pf_4_6));
	// 0.0725845  <=>  (((({pf_3_7 : 0.1772675} * (0.f - {pf_6_2 : 0.20241})) + {(fs_cbuf15_26.z) : 0.2193998}) * {i.fs_attr8.y : 0.20}) + ({pf_3_7 : 0.1772675} * {pf_6_2 : 0.20241}))
	pf_3_9 = ((((pf_3_7 * (0.f - pf_6_2)) + (fs_cbuf15_26.z)) * i.fs_attr8.y) + (pf_3_7 * pf_6_2));
	// 0.0221345  <=>  ((((0.f - {pf_1_6 : 0.029464}) + {i.fs_attr10.x : 0.00111}) * {i.fs_attr10.w : 0.2585}) + {pf_1_6 : 0.029464})
	pf_1_7 = ((((0.f - pf_1_6) + i.fs_attr10.x) * i.fs_attr10.w) + pf_1_6);
	// 0.0413725  <=>  ((((0.f - {pf_2_4 : 0.0527383}) + {i.fs_attr10.y : 0.00877}) * {i.fs_attr10.w : 0.2585}) + {pf_2_4 : 0.0527383})
	pf_2_5 = ((((0.f - pf_2_4) + i.fs_attr10.y) * i.fs_attr10.w) + pf_2_4);
	// 0.0561505  <=>  ((((0.f - {pf_3_9 : 0.0725845}) + {i.fs_attr10.z : 0.00901}) * {i.fs_attr10.w : 0.2585}) + {pf_3_9 : 0.0725845})
	pf_3_10 = ((((0.f - pf_3_9) + i.fs_attr10.z) * i.fs_attr10.w) + pf_3_9);
	// 0.0264365  <=>  (((((0.f - {pf_1_7 : 0.0221345}) + {(fs_cbuf15_25.x) : 0.0282744}) * {i.fs_attr8.x : 0.70066}) + {pf_1_7 : 0.0221345}) * {i.fs_attr7.x : 1.00})
	pf_1_9 = (((((0.f - pf_1_7) + (fs_cbuf15_25.x)) * i.fs_attr8.x) + pf_1_7) * i.fs_attr7.x);
	// 0.0776167  <=>  (((((0.f - {pf_2_5 : 0.0413725}) + {(fs_cbuf15_25.y) : 0.0931012}) * {i.fs_attr8.x : 0.70066}) + {pf_2_5 : 0.0413725}) * {i.fs_attr7.x : 1.00})
	pf_2_7 = (((((0.f - pf_2_5) + (fs_cbuf15_25.y)) * i.fs_attr8.x) + pf_2_5) * i.fs_attr7.x);
	// 0.0983901  <=>  (((((0.f - {pf_3_10 : 0.0561505}) + {(fs_cbuf15_25.z) : 0.1164359}) * {i.fs_attr8.x : 0.70066}) + {pf_3_10 : 0.0561505}) * {i.fs_attr7.x : 1.00})
	pf_3_12 = (((((0.f - pf_3_10) + (fs_cbuf15_25.z)) * i.fs_attr8.x) + pf_3_10) * i.fs_attr7.x);
	// 0.0264365  <=>  {pf_1_9 : 0.0264365}
	col.x = pf_1_9;
	// 0.0776167  <=>  {pf_2_7 : 0.0776167}
	col.y = pf_2_7;
	// 0.0983901  <=>  {pf_3_12 : 0.0983901}
	col.z = pf_3_12;
	// 1.00  <=>  clamp({pf_0_7 : 1.00}, 0.0, 1.0)
	col.w = clamp(pf_0_7, 0.0, 1.0);
	return;
}
