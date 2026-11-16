void frag_from_glsl(v2f i, inout float4 col)
{
	// 1065353216 = 1.00f;
	// fs_cbuf8[30] = float4(0.10, 25000.00, 2500.00, 24999.90);
	// fs_cbuf9[139] = float4(1.00, 0.00, 0.00, 0.00);
	// fs_cbuf9[140] = float4(0.00, 3.00, 0.00, 0.00);
	// fs_cbuf13[0] = float4(0.00, 0.50, 1.00, 0.20);
	// fs_cbuf15[1] = float4(0.00, 0.00, 2.594247, 0.20);

	bool b_0_0;
	float f_0_16;
	float f_0_2;
	float f_0_7;
	float4 f4_0_0;
	float4 f4_0_1;
	float4 f4_0_2;
	float4 f4_0_3;
	precise float pf_0_7;
	precise float pf_1_5;
	precise float pf_3_0;
	uint u_1_1;
	uint u_1_2;
	uint u_1_phi_5;
	uint u_2_1;
	uint u_2_2;
	uint u_2_phi_4;
	uint u_3_1;
	uint u_3_2;
	uint u_3_phi_3;
	uint u_4_0;
	uint u_4_1;
	uint u_4_phi_2;
	// 0.0013332  <=>  (1.0f / ({i.fs_attr3.w : 27.38721} * {i.vertex.w : 27.38721}))
	f_0_2 = (1.0f / (i.fs_attr3.w * i.vertex.w));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex2 : tex2}, float2({i.fs_attr2.z : 0.74605}, {i.fs_attr2.w : 0.92636}), s_linear_clamp_sampler)
	f4_0_0 = textureSample(tex2, float2(i.fs_attr2.z, i.fs_attr2.w), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex1 : tex1}, float2({i.fs_attr2.x : -0.13084}, {i.fs_attr2.y : 1.70043}), s_linear_clamp_sampler)
	f4_0_1 = textureSample(tex1, float2(i.fs_attr2.x, i.fs_attr2.y), s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,0.75)  <=>  textureSample({_CameraDepthTexture : _CameraDepthTexture}, float2((({i.fs_attr3.x : 17.3302} * {i.vertex.w : 27.38721}) * {f_0_2 : 0.0013332}), (({i.fs_attr3.y : 10.47894} * {i.vertex.w : 27.38721}) * {f_0_2 : 0.0013332})), s_linear_clamp_sampler)
	f4_0_2 = textureSample(_CameraDepthTexture, float2(((i.fs_attr3.x * i.vertex.w) * f_0_2), ((i.fs_attr3.y * i.vertex.w) * f_0_2)), s_linear_clamp_sampler);
	// 0.00  <=>  ((clamp(((((1.0f / ((({i.fs_attr3.z : 27.28732} * (1.0f / {i.fs_attr3.w : 27.38721})) * {(fs_cbuf8_30.w) : 24999.90}) + (0.f - {(fs_cbuf8_30.y) : 25000.00}))) * {(fs_cbuf8_30.z) : 2500.00}) + (({f4_0_2.x : 0.50} * {(fs_cbuf8_30.w) : 24999.90}) + {(fs_cbuf8_30.x) : 0.10})) * (1.0f / {(fs_cbuf9_140.y) : 3.00})), 0.0, 1.0) * clamp(((({f4_0_0.w : 0.75} * {f4_0_1.w : 0.75}) * {i.fs_attr0.w : 0.00147}) * {i.fs_attr1.w : 2.10}), 0.0, 1.0)) * {i.fs_attr4.x : 0.00})
	pf_0_7 = ((clamp(((((1.0f / (((i.fs_attr3.z * (1.0f / i.fs_attr3.w)) * (fs_cbuf8_30.w)) + (0.f - (fs_cbuf8_30.y)))) * (fs_cbuf8_30.z)) + ((f4_0_2.x * (fs_cbuf8_30.w)) + (fs_cbuf8_30.x))) * (1.0f / (fs_cbuf9_140.y))), 0.0, 1.0) * clamp((((f4_0_0.w * f4_0_1.w) * i.fs_attr0.w) * i.fs_attr1.w), 0.0, 1.0)) * i.fs_attr4.x);
	// 0.00  <=>  {(fs_cbuf9_139.z) : 0.00}
	f_0_7 = (fs_cbuf9_139.z);
	// True  <=>  ((({pf_0_7 : 0.00} <= {f_0_7 : 0.00}) && (! isnan({pf_0_7 : 0.00}))) && (! isnan({f_0_7 : 0.00})))
	b_0_0 = (((pf_0_7 <= f_0_7) && (! isnan(pf_0_7))) && (! isnan(f_0_7)));
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		discard;
	}
	// 987994412  <=>  {ftou(clamp(((({f4_0_0.w : 0.75} * {f4_0_1.w : 0.75}) * {i.fs_attr0.w : 0.00147}) * {i.fs_attr1.w : 2.10}), 0.0, 1.0)) : 987994412}
	u_4_0 = ftou(clamp((((f4_0_0.w * f4_0_1.w) * i.fs_attr0.w) * i.fs_attr1.w), 0.0, 1.0));
	u_4_phi_2 = u_4_0;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_4_1 = 0u;
		u_4_phi_2 = u_4_1;
	}
	// 1051372203  <=>  {ftou((1.0f / {(fs_cbuf9_140.y) : 3.00})) : 1051372203}
	u_3_1 = ftou((1.0f / (fs_cbuf9_140.y)));
	u_3_phi_3 = u_3_1;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_3_2 = 0u;
		u_3_phi_3 = u_3_2;
	}
	// 1074161254  <=>  {ftou(i.fs_attr1.w) : 1074161254}
	u_2_1 = ftou(i.fs_attr1.w);
	u_2_phi_4 = u_2_1;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_2_2 = 0u;
		u_2_phi_4 = u_2_2;
	}
	// 985705732  <=>  {ftou(i.fs_attr0.w) : 985705732}
	u_1_1 = ftou(i.fs_attr0.w);
	u_1_phi_5 = u_1_1;
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0  <=>  0u
		u_1_2 = 0u;
		u_1_phi_5 = u_1_2;
	}
	// True  <=>  if(({b_0_0 : True} ? true : false))
	if((b_0_0 ? true : false))
	{
		// 0.00  <=>  {utof(u_4_phi_2) : 0.00}
		col.x = utof(u_4_phi_2);
		// 0.00  <=>  {utof(u_3_phi_3) : 0.00}
		col.y = utof(u_3_phi_3);
		// 0.00  <=>  {utof(u_2_phi_4) : 0.00}
		col.z = utof(u_2_phi_4);
		// 0.00  <=>  {utof(u_1_phi_5) : 0.00}
		col.w = utof(u_1_phi_5);
		return;
	}
	// -0.1410421  <=>  ({i.fs_attr6.y : -0.14099} * inversesqrt((({i.fs_attr6.z : -0.90012} * {i.fs_attr6.z : -0.90012}) + (({i.fs_attr6.y : -0.14099} * {i.fs_attr6.y : -0.14099}) + ({i.fs_attr6.x : -0.4113} * {i.fs_attr6.x : -0.4113})))))
	pf_1_5 = (i.fs_attr6.y * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))));
	// -0.9004524  <=>  ({i.fs_attr6.z : -0.90012} * inversesqrt((({i.fs_attr6.z : -0.90012} * {i.fs_attr6.z : -0.90012}) + (({i.fs_attr6.y : -0.14099} * {i.fs_attr6.y : -0.14099}) + ({i.fs_attr6.x : -0.4113} * {i.fs_attr6.x : -0.4113})))))
	pf_3_0 = (i.fs_attr6.z * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))));
	// 1.110553  <=>  (1.0f / max(abs({pf_3_0 : -0.9004524}), max(abs({pf_1_5 : -0.1410421}), abs(({i.fs_attr6.x : -0.4113} * inversesqrt((({i.fs_attr6.z : -0.90012} * {i.fs_attr6.z : -0.90012}) + (({i.fs_attr6.y : -0.14099} * {i.fs_attr6.y : -0.14099}) + ({i.fs_attr6.x : -0.4113} * {i.fs_attr6.x : -0.4113})))))))))
	f_0_16 = (1.0f / max(abs(pf_3_0), max(abs(pf_1_5), abs((i.fs_attr6.x * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x)))))))));
	// float4(0.50,0.50,0.50,0.75)  <=>  textureLod({tex4 : tex4}, float3((({i.fs_attr6.x : -0.4113} * inversesqrt((({i.fs_attr6.z : -0.90012} * {i.fs_attr6.z : -0.90012}) + (({i.fs_attr6.y : -0.14099} * {i.fs_attr6.y : -0.14099}) + ({i.fs_attr6.x : -0.4113} * {i.fs_attr6.x : -0.4113}))))) * {f_0_16 : 1.110553}), ({pf_1_5 : -0.1410421} * {f_0_16 : 1.110553}), ({pf_3_0 : -0.9004524} * (0.f - {f_0_16 : 1.110553}))), {(fs_cbuf15_1.x) : 0.00}, s_linear_clamp_sampler)
	f4_0_3 = textureLod(tex4, float3(((i.fs_attr6.x * inversesqrt(((i.fs_attr6.z * i.fs_attr6.z) + ((i.fs_attr6.y * i.fs_attr6.y) + (i.fs_attr6.x * i.fs_attr6.x))))) * f_0_16), (pf_1_5 * f_0_16), (pf_3_0 * (0.f - f_0_16))), (fs_cbuf15_1.x), s_linear_clamp_sampler);
	// 0.6174472  <=>  ((({i.fs_attr5.x : 1.00} * {i.fs_attr0.x : 1.09333}) * ((((({f4_0_3.x : 0.50} * (0.f - {i.fs_attr8.w : 0.00})) + {f4_0_3.x : 0.50}) * {(fs_cbuf13_0.y) : 0.50}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.x : 0.51474})) * {i.fs_attr7.x : 1.00})
	col.x = (((i.fs_attr5.x * i.fs_attr0.x) * (((((f4_0_3.x * (0.f - i.fs_attr8.w)) + f4_0_3.x) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.x)) * i.fs_attr7.x);
	// 0.6859603  <=>  ((({i.fs_attr5.y : 1.00} * {i.fs_attr0.y : 1.07386}) * ((((({f4_0_3.y : 0.50} * (0.f - {i.fs_attr8.w : 0.00})) + {f4_0_3.y : 0.50}) * {(fs_cbuf13_0.y) : 0.50}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.y : 0.58878})) * {i.fs_attr7.x : 1.00})
	col.y = (((i.fs_attr5.y * i.fs_attr0.y) * (((((f4_0_3.y * (0.f - i.fs_attr8.w)) + f4_0_3.y) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.y)) * i.fs_attr7.x);
	// 0.6318299  <=>  ((({i.fs_attr5.z : 1.00} * {i.fs_attr0.z : 0.96317}) * ((((({f4_0_3.z : 0.50} * (0.f - {i.fs_attr8.w : 0.00})) + {f4_0_3.z : 0.50}) * {(fs_cbuf13_0.y) : 0.50}) * {(fs_cbuf15_1.w) : 0.20}) + {i.fs_attr9.z : 0.60599})) * {i.fs_attr7.x : 1.00})
	col.z = (((i.fs_attr5.z * i.fs_attr0.z) * (((((f4_0_3.z * (0.f - i.fs_attr8.w)) + f4_0_3.z) * (fs_cbuf13_0.y)) * (fs_cbuf15_1.w)) + i.fs_attr9.z)) * i.fs_attr7.x);
	// 0.00  <=>  clamp({pf_0_7 : 0.00}, 0.0, 1.0)
	col.w = clamp(pf_0_7, 0.0, 1.0);
	return;
}
