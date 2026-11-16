void frag_from_glsl(v2f i, inout float4 col)
{
	// 1065353216 = 1.00f;
	// fs_cbuf8[30] = float4(0.10, 25000.00, 2500.00, 24999.90);
	// fs_cbuf9[19] = float4(0.10, 0.10, 0, 0);
	// fs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// fs_cbuf9[140] = float4(1.00, 300.00, 0, 0);
	// fs_cbuf13[0] = float4(0, 0.50, 1.00, 0.50);
	// fs_cbuf15[1] = float4(0, 0, 2.594247, 0.20);
	// fs_cbuf15[25] = float4(0.0282744, 0.0931012, 0.1164359, 0.7006614);
	// fs_cbuf15[26] = float4(0.0174636, 0.1221582, 0.2193998, 0.20);

	bool b_0_0;
	float f_0_9;
	float f_2_2;
	float f_2_9;
	float f_4_4;
	float f_4_5;
	float f_6_3;
	float4 f4_0_0;
	float4 f4_0_1;
	float4 f4_0_2;
	float4 f4_0_3;
	float4 f4_0_4;
	float pf_0_10;
	float pf_1_11;
	float pf_1_6;
	float pf_1_8;
	float pf_1_9;
	float pf_2_3;
	float pf_2_5;
	float pf_2_6;
	float pf_2_8;
	float pf_3_10;
	float pf_3_11;
	float pf_3_13;
	float pf_3_4;
	float pf_3_8;
	float pf_4_6;
	float pf_5_0;
	float pf_5_5;
	float pf_6_2;
	uint u_1_1;
	uint u_1_2;
	uint u_1_phi_4;
	uint u_2_1;
	uint u_2_2;
	uint u_2_phi_5;
	uint u_3_1;
	uint u_3_2;
	uint u_3_phi_3;
	uint u_4_0;
	uint u_4_1;
	uint u_4_phi_2;
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex2 : tex2}, float2({i.fs_attr2.x : 0.97269}, {i.fs_attr2.y : 2.26966}), s_linear_clamp_sampler)
	f4_0_0 = textureSample(tex2, float2(i.fs_attr2.x, i.fs_attr2.y), s_linear_clamp_sampler);
	// 0.0000005  <=>  (1.0f / ({i.fs_attr4.w : 1390.032} * {i.vertex.w : 1390.032}))
	f_2_2 = (1.0f / (i.fs_attr4.w * i.vertex.w));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex4 : tex4}, float2({i.fs_attr3.x : 0.83993}, {i.fs_attr3.y : 0.08826}), s_linear_clamp_sampler)
	f4_0_1 = textureSample(tex4, float2(i.fs_attr3.x, i.fs_attr3.y), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex3 : tex3}, float2(((({i.fs_attr5.x : 1.00} * {(fs_cbuf9_19.x) : 0.10}) * (({f4_0_0.x : 0.50} * 2.f) + -1.f)) + {i.fs_attr2.z : 0.25095}), ((({i.fs_attr5.x : 1.00} * {(fs_cbuf9_19.y) : 0.10}) * (({f4_0_0.y : 0.50} * 2.f) + -1.f)) + {i.fs_attr2.w : 0.98535})), s_linear_clamp_sampler)
	f4_0_2 = textureSample(tex3, float2((((i.fs_attr5.x * (fs_cbuf9_19.x)) * ((f4_0_0.x * 2.f) + -1.f)) + i.fs_attr2.z), (((i.fs_attr5.x * (fs_cbuf9_19.y)) * ((f4_0_0.y * 2.f) + -1.f)) + i.fs_attr2.w)), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({_CameraDepthTexture : _CameraDepthTexture}, float2((({i.fs_attr4.x : 63.37988} * {i.vertex.w : 1390.032}) * {f_2_2 : 0.0000005}), (({i.fs_attr4.y : 448.3828} * {i.vertex.w : 1390.032}) * {f_2_2 : 0.0000005})), s_linear_clamp_sampler)
	f4_0_3 = textureSample(_CameraDepthTexture, float2(((i.fs_attr4.x * i.vertex.w) * f_2_2), ((i.fs_attr4.y * i.vertex.w) * f_2_2)), s_linear_clamp_sampler);
	// 0.27867  <=>  ((clamp(((((1.0f / ((({i.fs_attr4.z : 1389.938} * (1.0f / {i.fs_attr4.w : 1390.032})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_3.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 300.00})), 0.0, 1.0) * clamp((clamp((((({f4_0_2.w : 0.75} * {f4_0_1.w : 0.75}) * 1.f) + (0.f - {i.fs_attr0.w : 0.10})) * 4.f), 0.0, 1.0) * {i.fs_attr1.w : 0.27867}), 0.0, 1.0)) * {i.fs_attr6.x : 1.00})
	pf_0_10 = ((clamp(((((1.0f / (((i.fs_attr4.z * (1.0f / i.fs_attr4.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_3.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0) * clamp((clamp(((((f4_0_2.w * f4_0_1.w) * 1.f) + (0.f - i.fs_attr0.w)) * 4.f), 0.0, 1.0) * i.fs_attr1.w), 0.0, 1.0)) * i.fs_attr6.x);
	// 0  <=>  {(fs_cbuf9_139.z) : 0}
	f_2_9 = (fs_cbuf9_139.z);
	// False  <=>  ((({pf_0_10 : 0.27867} <= {f_2_9 : 0}) && (! isnan({pf_0_10 : 0.27867}))) && (! isnan({f_2_9 : 0})))
	b_0_0 = (((pf_0_10 <= f_2_9) && (! isnan(pf_0_10))) && (! isnan(f_2_9)));
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		discard;
	}
	// 1049538006  <=>  {ftou((clamp(((((1.0f / ((({i.fs_attr4.z : 1389.938} * (1.0f / {i.fs_attr4.w : 1390.032})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_3.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 300.00})), 0.0, 1.0) * clamp((clamp((((({f4_0_2.w : 0.75} * {f4_0_1.w : 0.75}) * 1.f) + (0.f - {i.fs_attr0.w : 0.10})) * 4.f), 0.0, 1.0) * {i.fs_attr1.w : 0.27867}), 0.0, 1.0))) : 1049538006}
	u_4_0 = ftou((clamp(((((1.0f / (((i.fs_attr4.z * (1.0f / i.fs_attr4.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_3.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0) * clamp((clamp(((((f4_0_2.w * f4_0_1.w) * 1.f) + (0.f - i.fs_attr0.w)) * 4.f), 0.0, 1.0) * i.fs_attr1.w), 0.0, 1.0)));
	u_4_phi_2 = u_4_0;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_4_1 = 0u;
		u_4_phi_2 = u_4_1;
	}
	// 995783694  <=>  {ftou((1.0f / {(fs_cbuf9_140.y) : 300.00})) : 995783694}
	u_3_1 = ftou((1.0f / (fs_cbuf9_140.y)));
	u_3_phi_3 = u_3_1;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_3_2 = 0u;
		u_3_phi_3 = u_3_2;
	}
	// 1065353216  <=>  {ftou(clamp(((((1.0f / ((({i.fs_attr4.z : 1389.938} * (1.0f / {i.fs_attr4.w : 1390.032})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_3.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 300.00})), 0.0, 1.0)) : 1065353216}
	u_1_1 = ftou(clamp(((((1.0f / (((i.fs_attr4.z * (1.0f / i.fs_attr4.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_3.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0));
	u_1_phi_4 = u_1_1;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_1_2 = 0u;
		u_1_phi_4 = u_1_2;
	}
	// 1027261148  <=>  {ftou((({i.fs_attr4.x : 63.37988} * {i.vertex.w : 1390.032}) * {f_2_2 : 0.0000005})) : 1027261148}
	u_2_1 = ftou(((i.fs_attr4.x * i.vertex.w) * f_2_2));
	u_2_phi_5 = u_2_1;
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_2_2 = 0u;
		u_2_phi_5 = u_2_2;
	}
	// False  <=>  if(({b_0_0 : False} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0.27867  <=>  {utof(u_4_phi_2) : 0.27867}
		col.x = utof(u_4_phi_2);
		// 0.0033333  <=>  {utof(u_3_phi_3) : 0.0033333}
		col.y = utof(u_3_phi_3);
		// 1.00  <=>  {utof(u_1_phi_4) : 1.00}
		col.z = utof(u_1_phi_4);
		// 0.045596  <=>  {utof(u_2_phi_5) : 0.045596}
		col.w = utof(u_2_phi_5);
		return;
	}
	// 0.1408742  <=>  ({i.fs_attr8.y : 0.1409} * inversesqrt((({i.fs_attr8.z : -0.7353} * {i.fs_attr8.z : -0.7353}) + (({i.fs_attr8.y : 0.1409} * {i.fs_attr8.y : 0.1409}) + ({i.fs_attr8.x : -0.66321} * {i.fs_attr8.x : -0.66321})))))
	pf_3_4 = (i.fs_attr8.y * inversesqrt(((i.fs_attr8.z * i.fs_attr8.z) + ((i.fs_attr8.y * i.fs_attr8.y) + (i.fs_attr8.x * i.fs_attr8.x)))));
	// -0.7351654  <=>  ({i.fs_attr8.z : -0.7353} * inversesqrt((({i.fs_attr8.z : -0.7353} * {i.fs_attr8.z : -0.7353}) + (({i.fs_attr8.y : 0.1409} * {i.fs_attr8.y : 0.1409}) + ({i.fs_attr8.x : -0.66321} * {i.fs_attr8.x : -0.66321})))))
	pf_5_0 = (i.fs_attr8.z * inversesqrt(((i.fs_attr8.z * i.fs_attr8.z) + ((i.fs_attr8.y * i.fs_attr8.y) + (i.fs_attr8.x * i.fs_attr8.x)))));
	// 1.360238  <=>  (1.0f / max(abs({pf_5_0 : -0.7351654}), max(abs({pf_3_4 : 0.1408742}), abs(({i.fs_attr8.x : -0.66321} * inversesqrt((({i.fs_attr8.z : -0.7353} * {i.fs_attr8.z : -0.7353}) + (({i.fs_attr8.y : 0.1409} * {i.fs_attr8.y : 0.1409}) + ({i.fs_attr8.x : -0.66321} * {i.fs_attr8.x : -0.66321})))))))))
	f_0_9 = (1.0f / max(abs(pf_5_0), max(abs(pf_3_4), abs((i.fs_attr8.x * inversesqrt(((i.fs_attr8.z * i.fs_attr8.z) + ((i.fs_attr8.y * i.fs_attr8.y) + (i.fs_attr8.x * i.fs_attr8.x)))))))));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex6 : tex6}, float3((({i.fs_attr8.x : -0.66321} * inversesqrt((({i.fs_attr8.z : -0.7353} * {i.fs_attr8.z : -0.7353}) + (({i.fs_attr8.y : 0.1409} * {i.fs_attr8.y : 0.1409}) + ({i.fs_attr8.x : -0.66321} * {i.fs_attr8.x : -0.66321}))))) * {f_0_9 : 1.360238}), ({pf_3_4 : 0.1408742} * {f_0_9 : 1.360238}), ({pf_5_0 : -0.7351654} * (0.f - {f_0_9 : 1.360238}))), {(fs_cbuf15_1.x) : 0}, s_linear_clamp_sampler)
	f4_0_4 = textureLod(tex6, float3(((i.fs_attr8.x * inversesqrt(((i.fs_attr8.z * i.fs_attr8.z) + ((i.fs_attr8.y * i.fs_attr8.y) + (i.fs_attr8.x * i.fs_attr8.x))))) * f_0_9), (pf_3_4 * f_0_9), (pf_5_0 * (0.f - f_0_9))), (fs_cbuf15_1.x), s_linear_clamp_sampler);
	// 0.61871  <=>  {i.fs_attr1.x : 0.61871}
	f_4_4 = i.fs_attr1.x;
	// 0.70002  <=>  {i.fs_attr1.y : 0.70002}
	f_6_3 = i.fs_attr1.y;
	// 0.72857  <=>  {i.fs_attr1.z : 0.72857}
	f_4_5 = i.fs_attr1.z;
	// 0.83189  <=>  (((({f4_0_2.x : 0.50} * {f4_0_2.x : 0.50}) * ((0.f - {f_4_4 : 0.61871}) + {i.fs_attr0.x : 1.47143})) + {f_4_4 : 0.61871}) * {i.fs_attr7.x : 1.00})
	pf_1_6 = ((((f4_0_2.x * f4_0_2.x) * ((0.f - f_4_4) + i.fs_attr0.x)) + f_4_4) * i.fs_attr7.x);
	// 0.8911025  <=>  (((({f4_0_2.y : 0.50} * {f4_0_2.y : 0.50}) * ((0.f - {f_6_3 : 0.70002}) + {i.fs_attr0.y : 1.46435})) + {f_6_3 : 0.70002}) * {i.fs_attr7.y : 1.00})
	pf_2_3 = ((((f4_0_2.y * f4_0_2.y) * ((0.f - f_6_3) + i.fs_attr0.y)) + f_6_3) * i.fs_attr7.y);
	// 0.88217  <=>  (((({f4_0_2.z : 0.50} * {f4_0_2.z : 0.50}) * ((0.f - {f_4_5 : 0.72857}) + {i.fs_attr0.z : 1.34297})) + {f_4_5 : 0.72857}) * {i.fs_attr7.z : 1.00})
	pf_3_8 = ((((f4_0_2.z * f4_0_2.z) * ((0.f - f_4_5) + i.fs_attr0.z)) + f_4_5) * i.fs_attr7.z);
	// 0.12766  <=>  ((((({f4_0_4.x : 0.50} * (0.f - {i.fs_attr10.w : 0})) + {f4_0_4.x : 0.50}) * {(fs_cbuf13_0.y) : 0.50}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr11.x : 0.07766})
	pf_4_6 = (((((f4_0_4.x * (0.f - i.fs_attr10.w)) + f4_0_4.x) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr11.x);
	// 0.12766  <=>  ((((({f4_0_4.y : 0.50} * (0.f - {i.fs_attr10.w : 0})) + {f4_0_4.y : 0.50}) * {(fs_cbuf13_0.y) : 0.50}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr11.y : 0.07766})
	pf_5_5 = (((((f4_0_4.y * (0.f - i.fs_attr10.w)) + f4_0_4.y) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr11.y);
	// 0.12766  <=>  ((((({f4_0_4.z : 0.50} * (0.f - {i.fs_attr10.w : 0})) + {f4_0_4.z : 0.50}) * {(fs_cbuf13_0.y) : 0.50}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr11.z : 0.07766})
	pf_6_2 = (((((f4_0_4.z * (0.f - i.fs_attr10.w)) + f4_0_4.z) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr11.z);
	// 0.1154382  <=>  (((({pf_2_3 : 0.8911025} * (0.f - {pf_5_5 : 0.12766})) + {(fs_cbuf15_26.y) : 0.1221582}) * {i.fs_attr10.y : 0.20}) + ({pf_2_3 : 0.8911025} * {pf_5_5 : 0.12766}))
	pf_2_5 = ((((pf_2_3 * (0.f - pf_5_5)) + (fs_cbuf15_26.y)) * i.fs_attr10.y) + (pf_2_3 * pf_5_5));
	// 0.088452  <=>  (((({pf_1_6 : 0.83189} * (0.f - {pf_4_6 : 0.12766})) + {(fs_cbuf15_26.x) : 0.0174636}) * {i.fs_attr10.y : 0.20}) + ({pf_1_6 : 0.83189} * {pf_4_6 : 0.12766}))
	pf_1_8 = ((((pf_1_6 * (0.f - pf_4_6)) + (fs_cbuf15_26.x)) * i.fs_attr10.y) + (pf_1_6 * pf_4_6));
	// 0.1339742  <=>  (((({pf_3_8 : 0.88217} * (0.f - {pf_6_2 : 0.12766})) + {(fs_cbuf15_26.z) : 0.2193998}) * {i.fs_attr10.y : 0.20}) + ({pf_3_8 : 0.88217} * {pf_6_2 : 0.12766}))
	pf_3_10 = ((((pf_3_8 * (0.f - pf_6_2)) + (fs_cbuf15_26.z)) * i.fs_attr10.y) + (pf_3_8 * pf_6_2));
	// 0.0420123  <=>  ((((0.f - {pf_1_8 : 0.088452}) + {i.fs_attr12.x : 0.00111}) * {i.fs_attr12.w : 0.5317}) + {pf_1_8 : 0.088452})
	pf_1_9 = ((((0.f - pf_1_8) + i.fs_attr12.x) * i.fs_attr12.w) + pf_1_8);
	// 0.0587227  <=>  ((((0.f - {pf_2_5 : 0.1154382}) + {i.fs_attr12.y : 0.00877}) * {i.fs_attr12.w : 0.5317}) + {pf_2_5 : 0.1154382})
	pf_2_6 = ((((0.f - pf_2_5) + i.fs_attr12.y) * i.fs_attr12.w) + pf_2_5);
	// 0.0675307  <=>  ((((0.f - {pf_3_10 : 0.1339742}) + {i.fs_attr12.z : 0.00901}) * {i.fs_attr12.w : 0.5317}) + {pf_3_10 : 0.1339742})
	pf_3_11 = ((((0.f - pf_3_10) + i.fs_attr12.z) * i.fs_attr12.w) + pf_3_10);
	// 0.0323867  <=>  (((((0.f - {pf_1_9 : 0.0420123}) + {(fs_cbuf15_25.x) : 0.0282744}) * {i.fs_attr10.x : 0.70066}) + {pf_1_9 : 0.0420123}) * {i.fs_attr9.x : 1.00})
	pf_1_11 = (((((0.f - pf_1_9) + (fs_cbuf15_25.x)) * i.fs_attr10.x) + pf_1_9) * i.fs_attr9.x);
	// 0.0828103  <=>  (((((0.f - {pf_2_6 : 0.0587227}) + {(fs_cbuf15_25.y) : 0.0931012}) * {i.fs_attr10.x : 0.70066}) + {pf_2_6 : 0.0587227}) * {i.fs_attr9.x : 1.00})
	pf_2_8 = (((((0.f - pf_2_6) + (fs_cbuf15_25.y)) * i.fs_attr10.x) + pf_2_6) * i.fs_attr9.x);
	// 0.1017966  <=>  (((((0.f - {pf_3_11 : 0.0675307}) + {(fs_cbuf15_25.z) : 0.1164359}) * {i.fs_attr10.x : 0.70066}) + {pf_3_11 : 0.0675307}) * {i.fs_attr9.x : 1.00})
	pf_3_13 = (((((0.f - pf_3_11) + (fs_cbuf15_25.z)) * i.fs_attr10.x) + pf_3_11) * i.fs_attr9.x);
	// 0.0323867  <=>  {pf_1_11 : 0.0323867}
	col.x = pf_1_11;
	// 0.0828103  <=>  {pf_2_8 : 0.0828103}
	col.y = pf_2_8;
	// 0.1017966  <=>  {pf_3_13 : 0.1017966}
	col.z = pf_3_13;
	// 0.27867  <=>  clamp({pf_0_10 : 0.27867}, 0.0, 1.0)
	col.w = clamp(pf_0_10, 0.0, 1.0);
	return;
}
