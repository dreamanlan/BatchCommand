            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 fs_attr0 : NORMAL;
                float4 fs_attr1 : TANGENT;
                float4 fs_attr2 : COLOR;
            };

            float4 vs_cbuf3_2;
            float4 vs_cbuf3_8;
            float4 vs_cbuf4_3;
            float4 vs_cbuf4_7;
            
            float4 fs_cbuf3_2;
            float4 fs_cbuf3_4;
            float4 fs_cbuf3_5;
            float4 fs_cbuf3_6;
            float4 fs_cbuf3_7;
            float4 fs_cbuf3_8;
            float4 fs_cbuf4_0;
            float4 fs_cbuf4_1;
            float4 fs_cbuf4_2;
            float4 fs_cbuf4_3;
            float4 fs_cbuf4_7;

            Texture3D tex0;

            SamplerState s_linear_clamp_sampler;

            //#define ftoi floatBitsToInt
            //#define ftou floatBitsToUint
            //#define itof intBitsToFloat
            //#define utof uintBitsToFloat
            #define ftou asuint
            #define utof asfloat

            uint2 Idx2Tex(uint idx, uint texWidth)
            {
                return uint2(idx % texWidth, idx / texWidth);
            }

            float LoadFromRwTexFloat(RWTexture2D<float> tex, uint idx, uint texWidth)
            {
                uint2 texPos = Idx2Tex(idx, texWidth);
                float pixel = tex[texPos];
                return pixel;
            }
            float LoadFromTexFloat(Texture2D<float> tex, uint idx, uint texWidth)
            {
                uint2 texPos = Idx2Tex(idx, texWidth);
                float pixel = tex[texPos];
                return pixel;
            }

            uint ssbo_get_uint(RWTexture2D<float> tex, int index)
            {
                uint w, h;
                tex.GetDimensions(w, h);
                float v = LoadFromRwTexFloat(tex, index, w);
                return asuint(v);
            }

            uint ssbo_get_uint(Texture2D<float> tex, int index)
            {
                uint w, h;
                tex.GetDimensions(w, h);
                float v = LoadFromTexFloat(tex, index, w);
                return asuint(v);
            }

            int2 textureSize(Texture2D tex, int lod)
            {
                uint w, h, lvls;
                tex.GetDimensions(uint(lod), w, h, lvls);
                return int2(w, h);
            }

            float4 texelFetch(Texture2D tex, int2 pos, int lod)
            {
                return tex.Load(int3(pos, lod));
            }

            float2 textureQueryLod(Texture2D tex, float2 uv, SamplerState s)
            {
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE)
                float lod = tex.CalculateLevelOfDetail(s, uv);
                uint w, h, lvls;
                tex.GetDimensions(uint(lod), w, h, lvls);
                return float2(lvls, lod);
            #else
                return float2(1.0f, 1.0f);
            #endif
            }

            float2 textureQueryLod(Texture2DArray tex, float2 uv, SamplerState s)
            {
            #if defined(SHADER_API_D3D11) || defined(SHADER_API_GLCORE)
                float lod = tex.CalculateLevelOfDetail(s, uv);
                uint w, h, elems, lvls;
                tex.GetDimensions(uint(lod), w, h, elems, lvls);
                return float2(lvls, lod);
            #else
                return float2(1.0f, 1.0f);
            #endif
            }

            float4 textureLod(Texture2D tex, float2 uv, float lod, SamplerState s)
            {
                return tex.SampleLevel(s, uv, lod);
            }

            float4 textureLod(Texture2DArray tex, float3 uvw, float lod, SamplerState s)
            {
                return tex.SampleLevel(s, uvw, lod);
            }

            float4 textureLod(TextureCube tex, float3 uvw, float lod, SamplerState s)
            {
                return tex.SampleLevel(s, uvw, lod);
            }

            float4 textureSample(Texture2D tex, float2 uv, SamplerState s)
            {
                return tex.Sample(s, uv);
            }

            float4 textureSample(Texture2DArray tex, float3 uv, SamplerState s)
            {
                return tex.Sample(s, uv);
            }

            float4 textureSample(TextureCube tex, float3 uv, SamplerState s)
            {
                return tex.Sample(s, uv);
            }

            float4 textureSample(Texture3D tex, float3 uv, SamplerState s)
            {
                return tex.Sample(s, uv);
            }

            float roundEven(float v)
            {
                return round(v);
            }

            float inversesqrt(float v)
            {
                return rsqrt(v);
            }

            uint bitfieldExtract(uint uval, uint offset, uint bits)
            {
                if (bits == 0)
                    return 0;
                if(offset == 0  && bits == 32)
                    return uval;
                uint mask = (1u << bits) - 1u;
                uint v = (uval >> offset) & mask;
                return v;
            }
            uint bitfieldExtract(uint uval, int offset, int bits)
            {
                return bitfieldExtract(uval, uint(offset), uint(bits));
            }
            int bitfieldExtract(int ival, int offset, int bits)
            {
                if (bits == 0)
                    return 0;
                if(offset == 0  && bits == 32)
                    return ival;
                int shifted = ival >> offset;
                int signBit = shifted & (int) (1u << (bits - 1));
                int mask = (int) ((1u << bits) - 1u);

                int v = -signBit | (shifted & mask);
                return v;
            }
            uint bitfieldInsert(uint uval, uint insert, uint offset, uint bits)
            {
                if (bits == 0)
                    return uval;
                if(offset == 0  && bits == 32)
                    return insert;
                uint maskBits = (1u << bits) - 1u;
                uint mask = maskBits << offset;
                uint src = insert << offset;
                uint v = (src & mask) | (uval & ~mask);
                return v;
            }

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                // 1065353216 = 1.00f;
                // vs_cbuf3[2] = float4(256.00, 256.00, 0.0039063, 0.0039063);
                // vs_cbuf3[8] = float4(6360.00, 6420.00, 0.00, 0.00);
                // vs_cbuf4[3] = float4(18.00, 16.58824, 15.30, 0.0021038);
                // vs_cbuf4[7] = float4(0.00, 0.00, 0.00, 0.00);

                bool b_1_1;
                float f_0_2;
                float pf_0_0;
                float pf_0_1;
                float pf_0_4;
                uint u_0_1;
                uint u_0_2;
                uint u_0_3;
                uint u_0_phi_2;
                uint u_1_0;
                uint u_1_1;
                uint u_1_phi_1;
                uint u_2_0;
                // -1.00  <=>  float(-1.00)
                o.vertex.x = float(-1.00);
                // 1.00  <=>  float(1.00)
                o.vertex.y = float(1.00);
                // 0.00  <=>  float(0.00)
                o.vertex.z = float(0.00);
                // 1.00  <=>  float(1.00)
                o.vertex.w = float(1.00);
                // 6360.126  <=>  float(6360.12646)
                o.fs_attr0.x = float(6360.12646);
                // 0.00  <=>  float(0.00)
                o.fs_attr0.y = float(0.00);
                // 0.00  <=>  float(0.00)
                o.fs_attr0.z = float(0.00);
                // 1.00  <=>  float(1.00)
                o.fs_attr0.w = float(1.00);
                // -0.00631  <=>  float(-0.00631)
                o.fs_attr1.x = float(-0.00631);
                // 0.00  <=>  float(0.00)
                o.fs_attr1.y = float(0.00);
                // 0.00  <=>  float(0.00)
                o.fs_attr1.z = float(0.00);
                // 1.00  <=>  float(1.00)
                o.fs_attr1.w = float(1.00);
                // -0.0024  <=>  float(-0.0024)
                o.fs_attr2.x = float(-0.0024);
                // 0.00  <=>  float(0.00)
                o.fs_attr2.y = float(0.00);
                // 0.00  <=>  float(0.00)
                o.fs_attr2.z = float(0.00);
                // 1.00  <=>  float(1.00)
                o.fs_attr2.w = float(1.00);
                // 60.00  <=>  ((0.f - {(vs_cbuf3_8.x) : 6360.00}) + {(vs_cbuf3_8.y) : 6420.00})
                pf_0_0 = ((0.f - (vs_cbuf3_8.x)) + (vs_cbuf3_8.y));
                // 6360.126  <=>  (({pf_0_0 : 60.00} * clamp({(vs_cbuf4_3.w) : 0.0021038}, 0.0001f, 0.9999f)) + {(vs_cbuf3_8.x) : 6360.00})
                pf_0_1 = ((pf_0_0 * clamp((vs_cbuf4_3.w), 0.0001f, 0.9999f)) + (vs_cbuf3_8.x));
                // 6360.126  <=>  {pf_0_1 : 6360.126}
                o.fs_attr0.x = pf_0_1;
                // -1.00  <=>  ({v.vertex.x : -0.50} * 2.f)
                o.vertex.x = (v.vertex.x * 2.f);
                // 1065352882  <=>  {ftou(((1.0f / {pf_0_1 : 6360.126}) * {(vs_cbuf3_8.x) : 6360.00})) : 1065352882}
                u_0_1 = ftou(((1.0f / pf_0_1) * (vs_cbuf3_8.x)));
                // 1.00  <=>  ({v.vertex.y : 0.50} * 2.f)
                o.vertex.y = (v.vertex.y * 2.f);
                // -0.9999801  <=>  (0.f - ((1.0f / {pf_0_1 : 6360.126}) * {(vs_cbuf3_8.x) : 6360.00}))
                f_0_2 = (0.f - ((1.0f / pf_0_1) * (vs_cbuf3_8.x)));
                // 0.0000398  <=>  ((((1.0f / {pf_0_1 : 6360.126}) * {(vs_cbuf3_8.x) : 6360.00}) * {f_0_2 : -0.9999801}) + 1.f)
                pf_0_4 = ((((1.0f / pf_0_1) * (vs_cbuf3_8.x)) * f_0_2) + 1.f);
                // 1065352882  <=>  {u_0_1 : 1065352882}
                u_1_0 = u_0_1;
                u_1_phi_1 = u_1_0;
                // False  <=>  if(((0u != ftou({vs_cbuf4_7.y : 0.00})) ? true : false))
                if(((0u != ftou(vs_cbuf4_7.y)) ? true : false))
                {
                    // 3212836864  <=>  3212836864u
                    u_1_1 = 3212836864u;
                    u_1_phi_1 = u_1_1;
                }
                // True  <=>  ((! (0u != ftou({vs_cbuf4_7.y : 0.00}))) ? true : false)
                b_1_1 = ((! (0u != ftou(vs_cbuf4_7.y))) ? true : false);
                // 1065352882  <=>  {u_1_phi_1 : 1065352882}
                u_0_2 = u_1_phi_1;
                u_0_phi_2 = u_0_2;
                // True  <=>  if({b_1_1 : True})
                if(b_1_1)
                {
                    // 3139274396  <=>  {ftou(((0.f - sqrt(clamp({pf_0_4 : 0.0000398}, 0.0, 1.0))) + {(vs_cbuf3_2.w) : 0.0039063})) : 3139274396}
                    u_2_0 = ftou(((0.f - sqrt(clamp(pf_0_4, 0.0, 1.0))) + (vs_cbuf3_2.w)));
                    // 3139274396  <=>  {u_2_0 : 3139274396}
                    u_0_3 = u_2_0;
                    u_0_phi_2 = u_0_3;
                }
                // -0.00631  <=>  (0.f - sqrt(clamp({pf_0_4 : 0.0000398}, 0.0, 1.0)))
                o.fs_attr1.x = (0.f - sqrt(clamp(pf_0_4, 0.0, 1.0)));
                // -0.0024037  <=>  {utof(u_0_phi_2) : -0.0024037}
                o.fs_attr2.x = utof(u_0_phi_2);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = float4(0,0,0,0);

                // 1065353216 = 1.00f;
                // fs_cbuf3[2] = float4(256.00, 256.00, 0.0039063, 0.0039063);
                // fs_cbuf3[4] = float4(32.00, 0.03125, 0.0322581, 0.0666667);
                // fs_cbuf3[5] = float4(32.00, 0.03125, 0.0322581, 0.0666667);
                // fs_cbuf3[6] = float4(8.00, 0.125, 0.1428571, 0.3333333);
                // fs_cbuf3[7] = float4(16.00, 0.0625, 0.0666667, 0.1428571);
                // fs_cbuf3[8] = float4(6360.00, 6420.00, 0.00, 0.00);
                // fs_cbuf4[0] = float4(0.00411, 0.011275, 0.028364, 0.0018);
                // fs_cbuf4[1] = float4(0.75, 1.00, 12.00, 0.00);
                // fs_cbuf4[2] = float4(-0.1714878, 0.6634241, 0.7283272, 0.00);
                // fs_cbuf4[3] = float4(18.00, 16.58824, 15.30, 0.0021038);
                // fs_cbuf4[7] = float4(0.00, 0.00, 0.00, 0.00);

                bool b_0_0;
                bool b_1_0;
                bool b_1_1;
                bool b_3_3;
                bool b_3_4;
                bool b_3_7;
                bool b_4_2;
                bool b_4_3;
                float f_0_16;
                float f_0_17;
                float f_1_14;
                float f_1_15;
                float f_1_18;
                float f_1_27;
                float f_1_28;
                float f_1_35;
                float f_1_38;
                float f_1_46;
                float f_1_49;
                float f_1_5;
                float f_10_0;
                float f_11_0;
                float f_12_0;
                float f_13_0;
                float f_2_12;
                float f_2_14;
                float f_2_15;
                float f_3_1;
                float f_3_10;
                float f_3_6;
                float f_4_14;
                float f_4_16;
                float f_4_17;
                float f_4_18;
                float f_6_2;
                float f_8_6;
                float f_9_0;
                float f_9_1;
                float3 f3_0_0;
                float3 f3_0_1;
                float3 f3_0_2;
                float3 f3_0_3;
                float4 f4_0_0;
                float4 f4_0_1;
                float4 f4_0_2;
                float4 f4_0_3;
                float pf_0_1;
                float pf_0_14;
                float pf_0_15;
                float pf_0_17;
                float pf_0_4;
                float pf_0_5;
                float pf_0_7;
                float pf_0_9;
                float pf_1_0;
                float pf_1_1;
                float pf_1_10;
                float pf_1_11;
                float pf_1_14;
                float pf_1_3;
                float pf_1_8;
                float pf_10_3;
                float pf_11_1;
                float pf_12_0;
                float pf_13_0;
                float pf_13_1;
                float pf_13_2;
                float pf_14_0;
                float pf_14_1;
                float pf_2_11;
                float pf_2_12;
                float pf_2_14;
                float pf_2_16;
                float pf_2_17;
                float pf_2_2;
                float pf_2_20;
                float pf_2_22;
                float pf_2_3;
                float pf_2_4;
                float pf_2_6;
                float pf_3_0;
                float pf_3_5;
                float pf_3_8;
                float pf_3_9;
                float pf_4_13;
                float pf_4_3;
                float pf_4_5;
                float pf_4_6;
                float pf_5_0;
                float pf_5_1;
                float pf_5_17;
                float pf_5_18;
                float pf_5_2;
                float pf_5_21;
                float pf_5_24;
                float pf_5_30;
                float pf_5_31;
                float pf_5_4;
                float pf_5_6;
                float pf_7_0;
                float pf_7_1;
                float pf_7_2;
                float pf_8_1;
                float pf_8_12;
                float pf_8_14;
                float pf_8_2;
                float pf_8_3;
                float pf_8_4;
                float pf_8_5;
                float pf_8_6;
                float pf_9_1;
                float pf_9_2;
                float pf_9_3;
                uint u_0_1;
                uint u_0_2;
                uint u_0_4;
                uint u_0_5;
                uint u_0_6;
                uint u_0_phi_13;
                uint u_0_phi_9;
                uint u_1_1;
                uint u_1_2;
                uint u_1_3;
                uint u_1_4;
                uint u_1_5;
                uint u_1_phi_21;
                uint u_1_phi_25;
                uint u_2_2;
                uint u_2_4;
                uint u_2_5;
                uint u_2_6;
                uint u_2_7;
                uint u_2_8;
                uint u_2_9;
                uint u_2_phi_16;
                uint u_2_phi_19;
                uint u_2_phi_23;
                uint u_2_phi_24;
                uint u_3_1;
                uint u_3_2;
                uint u_3_4;
                uint u_3_5;
                uint u_3_6;
                uint u_3_7;
                uint u_3_8;
                uint u_3_9;
                uint u_3_phi_17;
                uint u_3_phi_20;
                uint u_3_phi_22;
                uint u_4_2;
                uint u_4_3;
                uint u_4_4;
                uint u_4_5;
                uint u_4_7;
                uint u_4_phi_12;
                uint u_4_phi_15;
                uint u_4_phi_5;
                uint u_5_1;
                uint u_5_10;
                uint u_5_13;
                uint u_5_14;
                uint u_5_2;
                uint u_5_3;
                uint u_5_4;
                uint u_5_5;
                uint u_5_6;
                uint u_5_7;
                uint u_5_8;
                uint u_5_9;
                uint u_5_phi_10;
                uint u_5_phi_18;
                uint u_5_phi_4;
                uint u_5_phi_6;
                uint u_5_phi_8;
                uint u_6_0;
                uint u_6_1;
                uint u_6_3;
                uint u_6_4;
                uint u_6_phi_1;
                uint u_6_phi_11;
                uint u_7_1;
                uint u_7_2;
                uint u_7_3;
                uint u_7_4;
                uint u_7_5;
                uint u_7_phi_2;
                uint u_7_phi_7;
                uint u_8_1;
                uint u_8_2;
                uint u_8_phi_3;
                // 3.54907  <=>  (max({(fs_cbuf4_2.y) : 0.6634241}, -0.1975f) * 5.3496246f)
                pf_1_0 = (max((fs_cbuf4_2.y), -0.1975f) * 5.3496246f);
                // -40449600.00  <=>  (0.f - ({(fs_cbuf3_8.x) : 6360.00} * {(fs_cbuf3_8.x) : 6360.00}))
                f_3_1 = (0.f - ((fs_cbuf3_8.x) * (fs_cbuf3_8.x)));
                // 0.46875  <=>  ((0.f - {(fs_cbuf3_4.y) : 0.03125}) + 0.5f)
                pf_3_0 = ((0.f - (fs_cbuf3_4.y)) + 0.5f);
                // 875.6711  <=>  sqrt((({(fs_cbuf3_8.y) : 6420.00} * {(fs_cbuf3_8.y) : 6420.00}) + {f_3_1 : -40449600.00}))
                f_1_5 = sqrt((((fs_cbuf3_8.y) * (fs_cbuf3_8.y)) + f_3_1));
                // True  <=>  (((abs({pf_1_0 : 3.54907}) > 1.f) && (! isnan(abs({pf_1_0 : 3.54907})))) && (! isnan(1.f)))
                b_0_0 = (((abs(pf_1_0) > 1.f) && (! isnan(abs(pf_1_0)))) && (! isnan(1.f)));
                // False  <=>  (((max(abs({pf_1_0 : 3.54907}), 1.f) >= 16.f) && (! isnan(max(abs({pf_1_0 : 3.54907}), 1.f)))) && (! isnan(16.f)))
                b_1_0 = (((max(abs(pf_1_0), 1.f) >= 16.f) && (! isnan(max(abs(pf_1_0), 1.f)))) && (! isnan(16.f)));
                // 0.875024  <=>  ((({i.vertex.y : 240.00} * {(fs_cbuf3_2.w) : 0.0039063}) * 2.f) + -1.f)
                pf_1_1 = (((i.vertex.y * (fs_cbuf3_2.w)) * 2.f) + -1.f);
                // 1080239094  <=>  {ftou(max(abs({pf_1_0 : 3.54907}), 1.f)) : 1080239094}
                u_6_0 = ftou(max(abs(pf_1_0), 1.f));
                u_6_phi_1 = u_6_0;
                // False  <=>  if(({b_1_0 : False} ? true : false))
                if((b_1_0 ? true : false))
                {
                    // 1046684662  <=>  {ftou((max(abs({pf_1_0 : 3.54907}), 1.f) * 0.0625f)) : 1046684662}
                    u_6_1 = ftou((max(abs(pf_1_0), 1.f) * 0.0625f));
                    u_6_phi_1 = u_6_1;
                }
                // 1065353216  <=>  {ftou(min(abs({pf_1_0 : 3.54907}), 1.f)) : 1065353216}
                u_7_1 = ftou(min(abs(pf_1_0), 1.f));
                u_7_phi_2 = u_7_1;
                // False  <=>  if(({b_1_0 : False} ? true : false))
                if((b_1_0 ? true : false))
                {
                    // 1031798784  <=>  {ftou((min(abs({pf_1_0 : 3.54907}), 1.f) * 0.0625f)) : 1031798784}
                    u_7_2 = ftou((min(abs(pf_1_0), 1.f) * 0.0625f));
                    u_7_phi_2 = u_7_2;
                }
                // 1.625024  <=>  ((({i.vertex.y : 240.00} * {(fs_cbuf3_2.w) : 0.0039063}) * 2.f) + -0.25f)
                pf_2_2 = (((i.vertex.y * (fs_cbuf3_2.w)) * 2.f) + -0.25f);
                // 0.281764  <=>  ((1.0f / {utof(u_6_phi_1) : 3.54907}) * {utof(u_7_phi_2) : 1.00})
                pf_2_3 = ((1.0f / utof(u_6_phi_1)) * utof(u_7_phi_2));
                // 1034065847  <=>  {ftou(({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764})) : 1034065847}
                u_7_3 = ftou((pf_2_3 * pf_2_3));
                // 11.41478  <=>  (({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764}) + 11.335388f)
                pf_5_0 = ((pf_2_3 * pf_2_3) + 11.335388f);
                // -5.740235  <=>  ((({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764}) * -0.82336295f) + -5.674867f)
                pf_7_0 = (((pf_2_3 * pf_2_3) * -0.82336295f) + -5.674867f);
                // 29.7487  <=>  ((({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764}) * {pf_5_0 : 11.41478}) + 28.842468f)
                pf_5_1 = (((pf_2_3 * pf_2_3) * pf_5_0) + 28.842468f);
                // -7.021278  <=>  ((({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764}) * {pf_7_0 : -5.740235}) + -6.565555f)
                pf_7_1 = (((pf_2_3 * pf_2_3) * pf_7_0) + -6.565555f);
                // 22.05845  <=>  ((({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764}) * {pf_5_1 : 29.7487}) + 19.69667f)
                pf_5_2 = (((pf_2_3 * pf_2_3) * pf_5_1) + 19.69667f);
                // True  <=>  (! ((({pf_1_1 : 0.875024} < {i.fs_attr2.x : -0.0024}) && (! isnan({pf_1_1 : 0.875024}))) && (! isnan({i.fs_attr2.x : -0.0024}))))
                b_3_3 = (! (((pf_1_1 < i.fs_attr2.x) && (! isnan(pf_1_1))) && (! isnan(i.fs_attr2.x))));
                // 3139258706  <=>  {ftou(i.fs_attr2.x) : 3139258706}
                u_8_1 = ftou(i.fs_attr2.x);
                u_8_phi_3 = u_8_1;
                // True  <=>  if(({b_3_3 : True} ? true : false))
                if((b_3_3 ? true : false))
                {
                    // 1063256466  <=>  {ftou(pf_1_1) : 1063256466}
                    u_8_2 = ftou(pf_1_1);
                    u_8_phi_3 = u_8_2;
                }
                // -0.5574258  <=>  (({pf_2_3 : 0.281764} * {pf_2_3 : 0.281764}) * {pf_7_1 : -7.021278})
                pf_4_3 = ((pf_2_3 * pf_2_3) * pf_7_1);
                // 5565.263  <=>  ({i.fs_attr0.x : 6360.126} * {utof(u_8_phi_3) : 0.875024})
                pf_7_2 = (i.fs_attr0.x * utof(u_8_phi_3));
                // -9479054.00  <=>  (({pf_7_2 : 5565.263} * {pf_7_2 : 5565.263}) + (0.f - ({i.fs_attr0.x : 6360.126} * {i.fs_attr0.x : 6360.126})))
                pf_8_1 = ((pf_7_2 * pf_7_2) + (0.f - (i.fs_attr0.x * i.fs_attr0.x)));
                // 0.2746437  <=>  (((1.0f / {pf_5_2 : 22.05845}) * ({pf_2_3 : 0.281764} * {pf_4_3 : -0.5574258})) + {pf_2_3 : 0.281764})
                pf_2_4 = (((1.0f / pf_5_2) * (pf_2_3 * pf_4_3)) + pf_2_3);
                // True  <=>  ((({pf_8_1 : -9479054.00} > {f_3_1 : -40449600.00}) && (! isnan({pf_8_1 : -9479054.00}))) && (! isnan({f_3_1 : -40449600.00})))
                b_3_4 = (((pf_8_1 > f_3_1) && (! isnan(pf_8_1))) && (! isnan(f_3_1)));
                // 30970550.00  <=>  (({(fs_cbuf3_8.x) : 6360.00} * {(fs_cbuf3_8.x) : 6360.00}) + {pf_8_1 : -9479054.00})
                pf_4_5 = (((fs_cbuf3_8.x) * (fs_cbuf3_8.x)) + pf_8_1);
                // 1.963521  <=>  (({i.vertex.x : 320.00} * {(fs_cbuf3_2.z) : 0.0039063}) * 1.5707964f)
                pf_8_2 = ((i.vertex.x * (fs_cbuf3_2.z)) * 1.5707964f);
                // 1049402905  <=>  {ftou(pf_2_4) : 1049402905}
                u_5_1 = ftou(pf_2_4);
                u_5_phi_4 = u_5_1;
                // True  <=>  if(({b_0_0 : True} ? true : false))
                if((b_0_0 ? true : false))
                {
                    // 1067837525  <=>  {ftou(((0.f - {pf_2_4 : 0.2746437}) + 1.5707964f)) : 1067837525}
                    u_5_2 = ftou(((0.f - pf_2_4) + 1.5707964f));
                    u_5_phi_4 = u_5_2;
                }
                // 2.250032  <=>  ((({i.vertex.x : 320.00} * {(fs_cbuf3_2.z) : 0.0039063}) * 2.f) + -0.25f)
                pf_2_6 = (((i.vertex.x * (fs_cbuf3_2.z)) * 2.f) + -0.25f);
                // False  <=>  (((({pf_1_0 : 3.54907} < 0.f) && (! isnan({pf_1_0 : 3.54907}))) && (! isnan(0.f))) ? true : false)
                b_4_2 = ((((pf_1_0 < 0.f) && (! isnan(pf_1_0))) && (! isnan(0.f))) ? true : false);
                // 1067837525  <=>  {u_5_phi_4 : 1067837525}
                u_4_2 = u_5_phi_4;
                u_4_phi_5 = u_4_2;
                // False  <=>  if({b_4_2 : False})
                if(b_4_2)
                {
                    // 3215321173  <=>  {ftou((0.f - {utof(u_5_phi_4) : 1.296153})) : 3215321173}
                    u_4_3 = ftou((0.f - utof(u_5_phi_4)));
                    u_4_phi_5 = u_4_3;
                }
                // False  <=>  ((({pf_7_2 : 5565.263} < 0.f) && (! isnan({pf_7_2 : 5565.263}))) && (! isnan(0.f)))
                b_4_3 = (((pf_7_2 < 0.f) && (! isnan(pf_7_2))) && (! isnan(0.f)));
                // 1034065847  <=>  {u_7_3 : 1034065847}
                u_5_3 = u_7_3;
                u_5_phi_6 = u_5_3;
                // False  <=>  if((({b_4_3 : False} && {b_3_4 : True}) ? true : false))
                if(((b_4_3 && b_3_4) ? true : false))
                {
                    // 0  <=>  0u
                    u_5_4 = 0u;
                    u_5_phi_6 = u_5_4;
                }
                // 1034065847  <=>  {u_5_phi_6 : 1034065847}
                u_7_4 = u_5_phi_6;
                u_7_phi_7 = u_7_4;
                // True  <=>  if(((! ({b_4_3 : False} && {b_3_4 : True})) ? true : false))
                if(((! (b_4_3 && b_3_4)) ? true : false))
                {
                    // 1228616959  <=>  {ftou(({f_1_5 : 875.6711} * {f_1_5 : 875.6711})) : 1228616959}
                    u_7_5 = ftou((f_1_5 * f_1_5));
                    u_7_phi_7 = u_7_5;
                }
                // 1080239094  <=>  {ftou(pf_1_0) : 1080239094}
                u_5_5 = ftou(pf_1_0);
                u_5_phi_8 = u_5_5;
                // False  <=>  if((({b_4_3 : False} && {b_3_4 : True}) ? true : false))
                if(((b_4_3 && b_3_4) ? true : false))
                {
                    // 0  <=>  0u
                    u_5_6 = 0u;
                    u_5_phi_8 = u_5_6;
                }
                // 1080239094  <=>  {u_5_phi_8 : 1080239094}
                u_0_1 = u_5_phi_8;
                u_0_phi_9 = u_0_1;
                // True  <=>  if(((! ({b_4_3 : False} && {b_3_4 : True})) ? true : false))
                if(((! (b_4_3 && b_3_4)) ? true : false))
                {
                    // 1146809076  <=>  {ftou(f_1_5) : 1146809076}
                    u_0_2 = ftou(f_1_5);
                    u_0_phi_9 = u_0_2;
                }
                // 1049641822  <=>  {ftou(pf_2_3) : 1049641822}
                u_5_7 = ftou(pf_2_3);
                u_5_phi_10 = u_5_7;
                // False  <=>  if((({b_4_3 : False} && {b_3_4 : True}) ? true : false))
                if(((b_4_3 && b_3_4) ? true : false))
                {
                    // 1065353216  <=>  1065353216u
                    u_5_8 = 1065353216u;
                    u_5_phi_10 = u_5_8;
                }
                // 1049641822  <=>  {u_5_phi_10 : 1049641822}
                u_6_3 = u_5_phi_10;
                u_6_phi_11 = u_6_3;
                // True  <=>  if(((! ({b_4_3 : False} && {b_3_4 : True})) ? true : false))
                if(((! (b_4_3 && b_3_4)) ? true : false))
                {
                    // 3212836864  <=>  3212836864u
                    u_6_4 = 3212836864u;
                    u_6_phi_11 = u_6_4;
                }
                // 0.8477398  <=>  ((sin({pf_8_2 : 1.963521}) * 2.f) + -1.f)
                pf_4_6 = ((sin(pf_8_2) * 2.f) + -1.f);
                // 0.001092  <=>  (1.0f / (sqrt((({i.fs_attr0.x : 6360.126} * {i.fs_attr0.x : 6360.126}) + {f_3_1 : -40449600.00})) + {utof(u_0_phi_9) : 875.6711}))
                f_6_2 = (1.0f / (sqrt(((i.fs_attr0.x * i.fs_attr0.x) + f_3_1)) + utof(u_0_phi_9)));
                // 60.00  <=>  ((0.f - {(fs_cbuf3_8.x) : 6360.00}) + {(fs_cbuf3_8.y) : 6420.00})
                pf_5_4 = ((0.f - (fs_cbuf3_8.x)) + (fs_cbuf3_8.y));
                // 0.9238699  <=>  (({pf_4_6 : 0.8477398} * 0.5f) + 0.5f)
                pf_8_3 = ((pf_4_6 * 0.5f) + 0.5f);
                // 0.9591603  <=>  ((({utof(u_4_phi_5) : 1.296153} * 0.9090909f) + 0.74f) * 0.5f)
                pf_2_11 = (((utof(u_4_phi_5) * 0.9090909f) + 0.74f) * 0.5f);
                // 6.467089  <=>  (({pf_8_3 : 0.9238699} * {(fs_cbuf3_6.x) : 8.00}) + (0.f - {pf_8_3 : 0.9238699}))
                pf_8_4 = ((pf_8_3 * (fs_cbuf3_6.x)) + (0.f - pf_8_3));
                // 1064075965  <=>  {ftou(pf_8_3) : 1064075965}
                u_4_4 = ftou(pf_8_3);
                u_4_phi_12 = u_4_4;
                // False  <=>  if((({b_4_3 : False} && {b_3_4 : True}) ? true : false))
                if(((b_4_3 && b_3_4) ? true : false))
                {
                    // 1056440320  <=>  {ftou((({(fs_cbuf3_4.y) : 0.03125} * -0.5f) + 0.5f)) : 1056440320}
                    u_5_9 = ftou((((fs_cbuf3_4.y) * -0.5f) + 0.5f));
                    // 1056440320  <=>  {u_5_9 : 1056440320}
                    u_4_5 = u_5_9;
                    u_4_phi_12 = u_4_5;
                }
                // 0.0746115  <=>  ((({pf_7_2 : 5565.263} * {utof(u_6_phi_11) : -1.00}) + sqrt(({pf_4_5 : 30970550.00} + {utof(u_7_phi_7) : 766799.90}))) * {f_6_2 : 0.001092})
                pf_5_6 = (((pf_7_2 * utof(u_6_phi_11)) + sqrt((pf_4_5 + utof(u_7_phi_7)))) * f_6_2);
                // 0.1264648  <=>  ({i.fs_attr0.x : 6360.126} + (0.f - {(fs_cbuf3_8.x) : 6360.00}))
                pf_9_1 = (i.fs_attr0.x + (0.f - (fs_cbuf3_8.x)));
                // 0.9291866  <=>  (({pf_2_11 : 0.9591603} * (0.f - {(fs_cbuf3_5.y) : 0.03125})) + {pf_2_11 : 0.9591603})
                pf_2_12 = ((pf_2_11 * (0.f - (fs_cbuf3_5.y))) + pf_2_11);
                // 1064075965  <=>  {u_4_phi_12 : 1064075965}
                u_0_4 = u_4_phi_12;
                u_0_phi_13 = u_0_4;
                // True  <=>  if(((! ({b_4_3 : False} && {b_3_4 : True})) ? true : false))
                if(((! (b_4_3 && b_3_4)) ? true : false))
                {
                    // 1057226752  <=>  {ftou((({(fs_cbuf3_4.y) : 0.03125} * 0.5f) + 0.5f)) : 1057226752}
                    u_5_10 = ftou((((fs_cbuf3_4.y) * 0.5f) + 0.5f));
                    // 1057226752  <=>  {u_5_10 : 1057226752}
                    u_0_5 = u_5_10;
                    u_0_phi_13 = u_0_5;
                }
                // 0.0021077  <=>  ({pf_9_1 : 0.1264648} * (1.0f / {pf_5_4 : 60.00}))
                pf_9_2 = (pf_9_1 * (1.0f / pf_5_4));
                // 0.001976  <=>  (({pf_9_2 : 0.0021077} * (0.f - {(fs_cbuf3_7.y) : 0.0625})) + {pf_9_2 : 0.0021077})
                pf_9_3 = ((pf_9_2 * (0.f - (fs_cbuf3_7.y))) + pf_9_2);
                // 6.944812  <=>  (((0.5f * {(fs_cbuf3_5.y) : 0.03125}) + {pf_2_12 : 0.9291866}) + floor({pf_8_4 : 6.467089}))
                pf_2_14 = (((0.5f * (fs_cbuf3_5.y)) + pf_2_12) + floor(pf_8_4));
                // float3(0.9931015,0.5505992,0.033226)  <=>  float3((({pf_2_14 : 6.944812} * {(fs_cbuf3_6.y) : 0.125}) + {(fs_cbuf3_6.y) : 0.125}), (({pf_5_6 : 0.0746115} * {pf_3_0 : 0.46875}) + {utof(u_0_phi_13) : 0.515625}), ((0.5f * {(fs_cbuf3_7.y) : 0.0625}) + {pf_9_3 : 0.001976}))
                f3_0_0 = float3(((pf_2_14 * (fs_cbuf3_6.y)) + (fs_cbuf3_6.y)), ((pf_5_6 * pf_3_0) + utof(u_0_phi_13)), ((0.5f * (fs_cbuf3_7.y)) + pf_9_3));
                // float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex0 : tex0}, {f3_0_0 : float3(0.9931015,0.5505992,0.033226)}, s_linear_clamp_sampler)
                f4_0_0 = textureSample(tex0, f3_0_0, s_linear_clamp_sampler);
                // float3(0.8681015,0.5505992,0.033226)  <=>  float3(({pf_2_14 : 6.944812} * {(fs_cbuf3_6.y) : 0.125}), (({pf_5_6 : 0.0746115} * {pf_3_0 : 0.46875}) + {utof(u_0_phi_13) : 0.515625}), ((0.5f * {(fs_cbuf3_7.y) : 0.0625}) + {pf_9_3 : 0.001976}))
                f3_0_1 = float3((pf_2_14 * (fs_cbuf3_6.y)), ((pf_5_6 * pf_3_0) + utof(u_0_phi_13)), ((0.5f * (fs_cbuf3_7.y)) + pf_9_3));
                // float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex0 : tex0}, {f3_0_1 : float3(0.8681015,0.5505992,0.033226)}, s_linear_clamp_sampler)
                f4_0_1 = textureSample(tex0, f3_0_1, s_linear_clamp_sampler);
                // 0.50  <=>  {f4_0_1.x : 0.50}
                f_9_0 = f4_0_1.x;
                // 0.50  <=>  {f4_0_1.y : 0.50}
                f_10_0 = f4_0_1.y;
                // 0.50  <=>  {f4_0_1.z : 0.50}
                f_11_0 = f4_0_1.z;
                // 0.75  <=>  {f4_0_1.w : 0.75}
                f_12_0 = f4_0_1.w;
                // -0.00631  <=>  {i.fs_attr1.x : -0.00631}
                f_13_0 = i.fs_attr1.x;
                // 0.75  <=>  (max(clamp({pf_2_2 : 1.625024}, 0.0, 1.0), clamp({pf_2_6 : 2.250032}, 0.0, 1.0)) * {(fs_cbuf4_1.x) : 0.75})
                pf_2_16 = (max(clamp(pf_2_2, 0.0, 1.0), clamp(pf_2_6, 0.0, 1.0)) * (fs_cbuf4_1.x));
                // 0.00  <=>  {(fs_cbuf4_7.x) : 0.00}
                f_1_14 = (fs_cbuf4_7.x);
                // False  <=>  (((0.f < {f_1_14 : 0.00}) && (! isnan(0.f))) && (! isnan({f_1_14 : 0.00})))
                b_3_7 = (((0.f < f_1_14) && (! isnan(0.f))) && (! isnan(f_1_14)));
                // 2.5625  <=>  (({pf_2_16 : 0.75} * {pf_2_16 : 0.75}) + 2.f)
                pf_8_5 = ((pf_2_16 * pf_2_16) + 2.f);
                // 1065353216  <=>  (((({pf_1_1 : 0.875024} >= {f_13_0 : -0.00631}) || isnan({pf_1_1 : 0.875024})) || isnan({f_13_0 : -0.00631})) ? 1065353216u : 0u)
                u_0_6 = ((((pf_1_1 >= f_13_0) || isnan(pf_1_1)) || isnan(f_13_0)) ? 1065353216u : 0u);
                // 1.50  <=>  ({pf_2_16 : 0.75} * 2.f)
                f_1_15 = (pf_2_16 * 2.f);
                // -0.7091098  <=>  (({pf_2_16 : 0.75} * {pf_2_16 : 0.75}) + (0.f - ({f_1_15 : 1.50} * {pf_4_6 : 0.8477398})))
                pf_1_3 = ((pf_2_16 * pf_2_16) + (0.f - (f_1_15 * pf_4_6)));
                // 0.5625  <=>  ({pf_2_16 : 0.75} * {pf_2_16 : 0.75})
                pf_2_17 = (pf_2_16 * pf_2_16);
                // -1.781453  <=>  log2(abs(({pf_1_3 : -0.7091098} + 1.f)))
                f_1_18 = log2(abs((pf_1_3 + 1.f)));
                // 1.718663  <=>  (({pf_4_6 : 0.8477398} * {pf_4_6 : 0.8477398}) + 1.f)
                pf_8_6 = ((pf_4_6 * pf_4_6) + 1.f);
                // 1.8703  <=>  ((({pf_8_6 : 1.718663} * (0.f - {pf_2_17 : 0.5625})) + {pf_8_6 : 1.718663}) * (1.0f / ({pf_8_5 : 2.5625} * exp2(({f_1_18 : -1.781453} * 1.5f)))))
                pf_1_8 = (((pf_8_6 * (0.f - pf_2_17)) + pf_8_6) * (1.0f / (pf_8_5 * exp2((f_1_18 * 1.5f)))));
                // 0.1025751  <=>  (((({pf_4_6 : 0.8477398} * {pf_4_6 : 0.8477398}) * 0.059683103f) + 0.059683103f) * {(fs_cbuf4_1.y) : 1.00})
                pf_2_20 = ((((pf_4_6 * pf_4_6) * 0.059683103f) + 0.059683103f) * (fs_cbuf4_1.y));
                // 2.679007  <=>  (({pf_1_8 : 1.8703} * {(fs_cbuf4_1.z) : 12.00}) * 0.119366206f)
                pf_1_10 = ((pf_1_8 * (fs_cbuf4_1.z)) * 0.119366206f);
                // 0.50  <=>  max(0.f, ((({f4_0_0.x : 0.50} + (0.f - {f_9_0 : 0.50})) * ({pf_8_4 : 6.467089} + (0.f - floor({pf_8_4 : 6.467089})))) + {f_9_0 : 0.50}))
                f_1_27 = max(0.f, (((f4_0_0.x + (0.f - f_9_0)) * (pf_8_4 + (0.f - floor(pf_8_4)))) + f_9_0));
                // 0.50  <=>  max(0.f, ((({f4_0_0.y : 0.50} + (0.f - {f_10_0 : 0.50})) * ({pf_8_4 : 6.467089} + (0.f - floor({pf_8_4 : 6.467089})))) + {f_10_0 : 0.50}))
                f_4_14 = max(0.f, (((f4_0_0.y + (0.f - f_10_0)) * (pf_8_4 + (0.f - floor(pf_8_4)))) + f_10_0));
                // 0.75  <=>  max(0.f, ((({f4_0_0.w : 0.75} + (0.f - {f_12_0 : 0.75})) * ({pf_8_4 : 6.467089} + (0.f - floor({pf_8_4 : 6.467089})))) + {f_12_0 : 0.75}))
                f_8_6 = max(0.f, (((f4_0_0.w + (0.f - f_12_0)) * (pf_8_4 + (0.f - floor(pf_8_4)))) + f_12_0));
                // 0.50  <=>  max(0.f, ((({f4_0_0.z : 0.50} + (0.f - {f_11_0 : 0.50})) * ({pf_8_4 : 6.467089} + (0.f - floor({pf_8_4 : 6.467089})))) + {f_11_0 : 0.50}))
                f_9_1 = max(0.f, (((f4_0_0.z + (0.f - f_11_0)) * (pf_8_4 + (0.f - floor(pf_8_4)))) + f_11_0));
                // 0.144902  <=>  ((1.0f / {(fs_cbuf4_0.z) : 0.028364}) * {(fs_cbuf4_0.x) : 0.00411})
                pf_10_3 = ((1.0f / (fs_cbuf4_0.z)) * (fs_cbuf4_0.x));
                // 1.00  <=>  ((1.0f / {(fs_cbuf4_0.x) : 0.00411}) * {(fs_cbuf4_0.x) : 0.00411})
                pf_11_1 = ((1.0f / (fs_cbuf4_0.x)) * (fs_cbuf4_0.x));
                // 0.3645233  <=>  ((1.0f / {(fs_cbuf4_0.y) : 0.011275}) * {(fs_cbuf4_0.x) : 0.00411})
                pf_12_0 = ((1.0f / (fs_cbuf4_0.y)) * (fs_cbuf4_0.x));
                // 2.009255  <=>  ({pf_1_10 : 2.679007} * ({pf_11_1 : 1.00} * (({f_1_27 : 0.50} * {f_8_6 : 0.75}) * (1.0f / max({f_1_27 : 0.50}, 0.0001f)))))
                pf_4_13 = (pf_1_10 * (pf_11_1 * ((f_1_27 * f_8_6) * (1.0f / max(f_1_27, 0.0001f)))));
                // 0.2911451  <=>  ({pf_1_10 : 2.679007} * ({pf_10_3 : 0.144902} * (({f_9_1 : 0.50} * {f_8_6 : 0.75}) * (1.0f / max({f_1_27 : 0.50}, 0.0001f)))))
                pf_8_12 = (pf_1_10 * (pf_10_3 * ((f_9_1 * f_8_6) * (1.0f / max(f_1_27, 0.0001f)))));
                // 0.7324204  <=>  ({pf_1_10 : 2.679007} * ({pf_12_0 : 0.3645233} * (({f_4_14 : 0.50} * {f_8_6 : 0.75}) * (1.0f / max({f_1_27 : 0.50}, 0.0001f)))))
                pf_1_11 = (pf_1_10 * (pf_12_0 * ((f_4_14 * f_8_6) * (1.0f / max(f_1_27, 0.0001f)))));
                // 2.060543  <=>  max(0.f, (({pf_2_20 : 0.1025751} * {f_1_27 : 0.50}) + {pf_4_13 : 2.009255}))
                f_1_28 = max(0.f, ((pf_2_20 * f_1_27) + pf_4_13));
                // True  <=>  if(((! {b_3_7 : False}) ? true : false))
                if(((! b_3_7) ? true : false))
                {
                    // 37.08978  <=>  ({f_1_28 : 2.060543} * {(fs_cbuf4_3.x) : 18.00})
                    col.x = (f_1_28 * (fs_cbuf4_3.x));
                    // 13.00034  <=>  (max(0.f, (({pf_2_20 : 0.1025751} * {f_4_14 : 0.50}) + {pf_1_11 : 0.7324204})) * {(fs_cbuf4_3.y) : 16.58824})
                    col.y = (max(0.f, ((pf_2_20 * f_4_14) + pf_1_11)) * (fs_cbuf4_3.y));
                    // 5.23922  <=>  (max(0.f, (({pf_2_20 : 0.1025751} * {f_9_1 : 0.50}) + {pf_8_12 : 0.2911451})) * {(fs_cbuf4_3.z) : 15.30})
                    col.z = (max(0.f, ((pf_2_20 * f_9_1) + pf_8_12)) * (fs_cbuf4_3.z));
                    // 1.00  <=>  {utof(u_0_6) : 1.00}
                    col.w = utof(u_0_6);
                    return col;
                }
                u_4_phi_15 = u_6_0;
                // False  <=>  if(({b_1_0 : False} ? true : false))
                if((b_1_0 ? true : false))
                {
                    // 1046684662  <=>  {ftou((max(abs({pf_1_0 : 3.54907}), 1.f) * 0.0625f)) : 1046684662}
                    u_4_7 = ftou((max(abs(pf_1_0), 1.f) * 0.0625f));
                    u_4_phi_15 = u_4_7;
                }
                u_2_phi_16 = u_7_1;
                // False  <=>  if(({b_1_0 : False} ? true : false))
                if((b_1_0 ? true : false))
                {
                    // 1031798784  <=>  {ftou((min(abs({pf_1_0 : 3.54907}), 1.f) * 0.0625f)) : 1031798784}
                    u_2_2 = ftou((min(abs(pf_1_0), 1.f) * 0.0625f));
                    u_2_phi_16 = u_2_2;
                }
                // True  <=>  ((({pf_8_1 : -9479054.00} > {f_3_1 : -40449600.00}) && (! isnan({pf_8_1 : -9479054.00}))) && (! isnan({f_3_1 : -40449600.00})))
                b_1_1 = (((pf_8_1 > f_3_1) && (! isnan(pf_8_1))) && (! isnan(f_3_1)));
                // 30970550.00  <=>  (({(fs_cbuf3_8.x) : 6360.00} * {(fs_cbuf3_8.x) : 6360.00}) + {pf_8_1 : -9479054.00})
                pf_0_1 = (((fs_cbuf3_8.x) * (fs_cbuf3_8.x)) + pf_8_1);
                // 0.281764  <=>  ((1.0f / {utof(u_4_phi_15) : 3.54907}) * {utof(u_2_phi_16) : 1.00})
                pf_5_17 = ((1.0f / utof(u_4_phi_15)) * utof(u_2_phi_16));
                // 3.00  <=>  floor(((0.5f * {(fs_cbuf3_6.x) : 8.00}) + -0.5f))
                f_1_35 = floor(((0.5f * (fs_cbuf3_6.x)) + -0.5f));
                // 11.41478  <=>  (({pf_5_17 : 0.281764} * {pf_5_17 : 0.281764}) + 11.335388f)
                pf_13_0 = ((pf_5_17 * pf_5_17) + 11.335388f);
                // -5.740235  <=>  ((({pf_5_17 : 0.281764} * {pf_5_17 : 0.281764}) * -0.82336295f) + -5.674867f)
                pf_14_0 = (((pf_5_17 * pf_5_17) * -0.82336295f) + -5.674867f);
                // 29.7487  <=>  ((({pf_5_17 : 0.281764} * {pf_5_17 : 0.281764}) * {pf_13_0 : 11.41478}) + 28.842468f)
                pf_13_1 = (((pf_5_17 * pf_5_17) * pf_13_0) + 28.842468f);
                // -7.021278  <=>  ((({pf_5_17 : 0.281764} * {pf_5_17 : 0.281764}) * {pf_14_0 : -5.740235}) + -6.565555f)
                pf_14_1 = (((pf_5_17 * pf_5_17) * pf_14_0) + -6.565555f);
                // 1146809076  <=>  {ftou(f_1_5) : 1146809076}
                u_3_1 = ftou(f_1_5);
                u_3_phi_17 = u_3_1;
                // False  <=>  if((({b_4_3 : False} && {b_1_1 : True}) ? true : false))
                if(((b_4_3 && b_1_1) ? true : false))
                {
                    // 0  <=>  0u
                    u_3_2 = 0u;
                    u_3_phi_17 = u_3_2;
                }
                // 22.05845  <=>  ((({pf_5_17 : 0.281764} * {pf_5_17 : 0.281764}) * {pf_13_1 : 29.7487}) + 19.69667f)
                pf_13_2 = (((pf_5_17 * pf_5_17) * pf_13_1) + 19.69667f);
                // -0.5574258  <=>  (({pf_5_17 : 0.281764} * {pf_5_17 : 0.281764}) * {pf_14_1 : -7.021278})
                pf_8_14 = ((pf_5_17 * pf_5_17) * pf_14_1);
                // 3189822718  <=>  {ftou(({pf_5_17 : 0.281764} * {pf_8_14 : -0.5574258})) : 3189822718}
                u_1_1 = ftou((pf_5_17 * pf_8_14));
                // 0.2746437  <=>  (((1.0f / {pf_13_2 : 22.05845}) * ({pf_5_17 : 0.281764} * {pf_8_14 : -0.5574258})) + {pf_5_17 : 0.281764})
                pf_5_18 = (((1.0f / pf_13_2) * (pf_5_17 * pf_8_14)) + pf_5_17);
                // 1049641822  <=>  {ftou(pf_5_17) : 1049641822}
                u_5_13 = ftou(pf_5_17);
                u_5_phi_18 = u_5_13;
                // False  <=>  if((({b_4_3 : False} && {b_1_1 : True}) ? true : false))
                if(((b_4_3 && b_1_1) ? true : false))
                {
                    // 0  <=>  0u
                    u_5_14 = 0u;
                    u_5_phi_18 = u_5_14;
                }
                // 1049641822  <=>  {u_5_phi_18 : 1049641822}
                u_2_4 = u_5_phi_18;
                u_2_phi_19 = u_2_4;
                // True  <=>  if(((! ({b_4_3 : False} && {b_1_1 : True})) ? true : false))
                if(((! (b_4_3 && b_1_1)) ? true : false))
                {
                    // 1228616959  <=>  {ftou(({utof(u_3_phi_17) : 875.6711} * {utof(u_3_phi_17) : 875.6711})) : 1228616959}
                    u_2_5 = ftou((utof(u_3_phi_17) * utof(u_3_phi_17)));
                    u_2_phi_19 = u_2_5;
                }
                // 3189822718  <=>  {u_1_1 : 3189822718}
                u_3_4 = u_1_1;
                u_3_phi_20 = u_3_4;
                // False  <=>  if((({b_4_3 : False} && {b_1_1 : True}) ? true : false))
                if(((b_4_3 && b_1_1) ? true : false))
                {
                    // 1065353216  <=>  1065353216u
                    u_3_5 = 1065353216u;
                    u_3_phi_20 = u_3_5;
                }
                // 3189822718  <=>  {u_3_phi_20 : 3189822718}
                u_1_2 = u_3_phi_20;
                u_1_phi_21 = u_1_2;
                // True  <=>  if(((! ({b_4_3 : False} && {b_1_1 : True})) ? true : false))
                if(((! (b_4_3 && b_1_1)) ? true : false))
                {
                    // 3212836864  <=>  3212836864u
                    u_1_3 = 3212836864u;
                    u_1_phi_21 = u_1_3;
                }
                // 1049402905  <=>  {ftou(pf_5_18) : 1049402905}
                u_3_6 = ftou(pf_5_18);
                u_3_phi_22 = u_3_6;
                // True  <=>  if(({b_0_0 : True} ? true : false))
                if((b_0_0 ? true : false))
                {
                    // 1067837525  <=>  {ftou(((0.f - {pf_5_18 : 0.2746437}) + 1.5707964f)) : 1067837525}
                    u_3_7 = ftou(((0.f - pf_5_18) + 1.5707964f));
                    u_3_phi_22 = u_3_7;
                }
                // 1067837525  <=>  {u_3_phi_22 : 1067837525}
                u_2_6 = u_3_phi_22;
                u_2_phi_23 = u_2_6;
                // False  <=>  if({b_4_2 : False})
                if(b_4_2)
                {
                    // 3215321173  <=>  {ftou((0.f - {utof(u_3_phi_22) : 1.296153})) : 3215321173}
                    u_2_7 = ftou((0.f - utof(u_3_phi_22)));
                    u_2_phi_23 = u_2_7;
                }
                // 0.001092  <=>  (1.0f / (sqrt((({i.fs_attr0.x : 6360.126} * {i.fs_attr0.x : 6360.126}) + {f_3_1 : -40449600.00})) + {utof(u_3_phi_17) : 875.6711}))
                f_2_12 = (1.0f / (sqrt(((i.fs_attr0.x * i.fs_attr0.x) + f_3_1)) + utof(u_3_phi_17)));
                // 0.9591603  <=>  ((({utof(u_2_phi_23) : 1.296153} * 0.9090909f) + 0.74f) * 0.5f)
                pf_0_4 = (((utof(u_2_phi_23) * 0.9090909f) + 0.74f) * 0.5f);
                // 3212836864  <=>  {u_1_phi_21 : 3212836864}
                u_2_8 = u_1_phi_21;
                u_2_phi_24 = u_2_8;
                // False  <=>  if((({b_4_3 : False} && {b_1_1 : True}) ? true : false))
                if(((b_4_3 && b_1_1) ? true : false))
                {
                    // 1056440320  <=>  {ftou((({(fs_cbuf3_4.y) : 0.03125} * -0.5f) + 0.5f)) : 1056440320}
                    u_3_8 = ftou((((fs_cbuf3_4.y) * -0.5f) + 0.5f));
                    // 1056440320  <=>  {u_3_8 : 1056440320}
                    u_2_9 = u_3_8;
                    u_2_phi_24 = u_2_9;
                }
                // 3212836864  <=>  {u_2_phi_24 : 3212836864}
                u_1_4 = u_2_phi_24;
                u_1_phi_25 = u_1_4;
                // True  <=>  if(((! ({b_4_3 : False} && {b_1_1 : True})) ? true : false))
                if(((! (b_4_3 && b_1_1)) ? true : false))
                {
                    // 1057226752  <=>  {ftou((({(fs_cbuf3_4.y) : 0.03125} * 0.5f) + 0.5f)) : 1057226752}
                    u_3_9 = ftou((((fs_cbuf3_4.y) * 0.5f) + 0.5f));
                    // 1057226752  <=>  {u_3_9 : 1057226752}
                    u_1_5 = u_3_9;
                    u_1_phi_25 = u_1_5;
                }
                // 0.9291866  <=>  (({pf_0_4 : 0.9591603} * (0.f - {(fs_cbuf3_5.y) : 0.03125})) + {pf_0_4 : 0.9591603})
                pf_0_5 = ((pf_0_4 * (0.f - (fs_cbuf3_5.y))) + pf_0_4);
                // 0.0746115  <=>  ((({pf_7_2 : 5565.263} * {utof(u_1_phi_21) : -1.00}) + sqrt(({pf_0_1 : 30970550.00} + {utof(u_2_phi_19) : 766799.90}))) * {f_2_12 : 0.001092})
                pf_5_21 = (((pf_7_2 * utof(u_1_phi_21)) + sqrt((pf_0_1 + utof(u_2_phi_19)))) * f_2_12);
                // 3.944812  <=>  (((0.5f * {(fs_cbuf3_5.y) : 0.03125}) + {pf_0_5 : 0.9291866}) + {f_1_35 : 3.00})
                pf_0_7 = (((0.5f * (fs_cbuf3_5.y)) + pf_0_5) + f_1_35);
                // float3(0.4931014,0.5505992,0.033226)  <=>  float3(({pf_0_7 : 3.944812} * {(fs_cbuf3_6.y) : 0.125}), (({pf_3_0 : 0.46875} * {pf_5_21 : 0.0746115}) + {utof(u_1_phi_25) : 0.515625}), ((0.5f * {(fs_cbuf3_7.y) : 0.0625}) + {pf_9_3 : 0.001976}))
                f3_0_2 = float3((pf_0_7 * (fs_cbuf3_6.y)), ((pf_3_0 * pf_5_21) + utof(u_1_phi_25)), ((0.5f * (fs_cbuf3_7.y)) + pf_9_3));
                // float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex0 : tex0}, {f3_0_2 : float3(0.4931014,0.5505992,0.033226)}, s_linear_clamp_sampler)
                f4_0_2 = textureSample(tex0, f3_0_2, s_linear_clamp_sampler);
                // 0.50  <=>  {f4_0_2.x : 0.50}
                f_0_16 = f4_0_2.x;
                // 0.50  <=>  {f4_0_2.y : 0.50}
                f_2_14 = f4_0_2.y;
                // 0.50  <=>  {f4_0_2.z : 0.50}
                f_3_6 = f4_0_2.z;
                // 0.75  <=>  {f4_0_2.w : 0.75}
                f_4_16 = f4_0_2.w;
                // float3(0.6181015,0.5505992,0.033226)  <=>  float3((({pf_0_7 : 3.944812} * {(fs_cbuf3_6.y) : 0.125}) + {(fs_cbuf3_6.y) : 0.125}), (({pf_3_0 : 0.46875} * {pf_5_21 : 0.0746115}) + {utof(u_1_phi_25) : 0.515625}), ((0.5f * {(fs_cbuf3_7.y) : 0.0625}) + {pf_9_3 : 0.001976}))
                f3_0_3 = float3(((pf_0_7 * (fs_cbuf3_6.y)) + (fs_cbuf3_6.y)), ((pf_3_0 * pf_5_21) + utof(u_1_phi_25)), ((0.5f * (fs_cbuf3_7.y)) + pf_9_3));
                // float4(0.50,0.50,0.50,0.75)  <=>  textureSample({tex0 : tex0}, {f3_0_3 : float3(0.6181015,0.5505992,0.033226)}, s_linear_clamp_sampler)
                f4_0_3 = textureSample(tex0, f3_0_3, s_linear_clamp_sampler);
                // 0.50  <=>  (((0.5f * {(fs_cbuf3_6.x) : 8.00}) + -0.5f) + (0.f - {f_1_35 : 3.00}))
                pf_0_9 = (((0.5f * (fs_cbuf3_6.x)) + -0.5f) + (0.f - f_1_35));
                // 0.6438562  <=>  log2((({(fs_cbuf4_1.x) : 0.75} * {(fs_cbuf4_1.x) : 0.75}) + 1.f))
                f_1_38 = log2((((fs_cbuf4_1.x) * (fs_cbuf4_1.x)) + 1.f));
                // 5.004883  <=>  ((({(fs_cbuf4_1.x) : 0.75} * {(fs_cbuf4_1.x) : 0.75}) + 2.f) * exp2(({f_1_38 : 0.6438562} * 1.5f)))
                pf_3_5 = ((((fs_cbuf4_1.x) * (fs_cbuf4_1.x)) + 2.f) * exp2((f_1_38 * 1.5f)));
                // 0.0522227  <=>  ((({(fs_cbuf4_1.x) : 0.75} * {(fs_cbuf4_1.x) : 0.75}) * -0.119366206f) + 0.119366206f)
                pf_5_24 = ((((fs_cbuf4_1.x) * (fs_cbuf4_1.x)) * -0.119366206f) + 0.119366206f);
                // 0.50  <=>  max(0.f, (({pf_0_9 : 0.50} * ((0.f - {f_0_16 : 0.50}) + {f4_0_3.x : 0.50})) + {f_0_16 : 0.50}))
                f_0_17 = max(0.f, ((pf_0_9 * ((0.f - f_0_16) + f4_0_3.x)) + f_0_16));
                // 0.75  <=>  max(0.f, (({pf_0_9 : 0.50} * ((0.f - {f_4_16 : 0.75}) + {f4_0_3.w : 0.75})) + {f_4_16 : 0.75}))
                f_1_46 = max(0.f, ((pf_0_9 * ((0.f - f_4_16) + f4_0_3.w)) + f_4_16));
                // 0.50  <=>  max(0.f, (({pf_0_9 : 0.50} * ((0.f - {f_2_14 : 0.50}) + {f4_0_3.y : 0.50})) + {f_2_14 : 0.50}))
                f_2_15 = max(0.f, ((pf_0_9 * ((0.f - f_2_14) + f4_0_3.y)) + f_2_14));
                // 0.50  <=>  max(0.f, (({pf_0_9 : 0.50} * ((0.f - {f_3_6 : 0.50}) + {f4_0_3.z : 0.50})) + {f_3_6 : 0.50}))
                f_4_17 = max(0.f, ((pf_0_9 * ((0.f - f_3_6) + f4_0_3.z)) + f_3_6));
                // 0.0939092  <=>  ((({pf_5_24 : 0.0522227} * (1.0f / {pf_3_5 : 5.004883})) * {(fs_cbuf4_1.z) : 12.00}) * ({pf_11_1 : 1.00} * (({f_0_17 : 0.50} * {f_1_46 : 0.75}) * (1.0f / max({f_0_17 : 0.50}, 0.0001f)))))
                pf_0_14 = (((pf_5_24 * (1.0f / pf_3_5)) * (fs_cbuf4_1.z)) * (pf_11_1 * ((f_0_17 * f_1_46) * (1.0f / max(f_0_17, 0.0001f)))));
                // 0.0342321  <=>  ((({pf_5_24 : 0.0522227} * (1.0f / {pf_3_5 : 5.004883})) * {(fs_cbuf4_1.z) : 12.00}) * ({pf_12_0 : 0.3645233} * (({f_2_15 : 0.50} * {f_1_46 : 0.75}) * (1.0f / max({f_0_17 : 0.50}, 0.0001f)))))
                pf_5_30 = (((pf_5_24 * (1.0f / pf_3_5)) * (fs_cbuf4_1.z)) * (pf_12_0 * ((f_2_15 * f_1_46) * (1.0f / max(f_0_17, 0.0001f)))));
                // 0.0136076  <=>  ((({pf_5_24 : 0.0522227} * (1.0f / {pf_3_5 : 5.004883})) * {(fs_cbuf4_1.z) : 12.00}) * ({pf_10_3 : 0.144902} * (({f_4_17 : 0.50} * {f_1_46 : 0.75}) * (1.0f / max({f_0_17 : 0.50}, 0.0001f)))))
                pf_3_8 = (((pf_5_24 * (1.0f / pf_3_5)) * (fs_cbuf4_1.z)) * (pf_10_3 * ((f_4_17 * f_1_46) * (1.0f / max(f_0_17, 0.0001f)))));
                // 0.1237507  <=>  (({f_0_17 : 0.50} * ({(fs_cbuf4_1.y) : 1.00} * 0.059683103f)) + {pf_0_14 : 0.0939092})
                pf_0_15 = ((f_0_17 * ((fs_cbuf4_1.y) * 0.059683103f)) + pf_0_14);
                // 0.0640736  <=>  (({f_2_15 : 0.50} * ({(fs_cbuf4_1.y) : 1.00} * 0.059683103f)) + {pf_5_30 : 0.0342321})
                pf_5_31 = ((f_2_15 * ((fs_cbuf4_1.y) * 0.059683103f)) + pf_5_30);
                // 0.0434492  <=>  (({f_4_17 : 0.50} * ({(fs_cbuf4_1.y) : 1.00} * 0.059683103f)) + {pf_3_8 : 0.0136076})
                pf_3_9 = ((f_4_17 * ((fs_cbuf4_1.y) * 0.059683103f)) + pf_3_8);
                // -37.08978  <=>  (0.f - ({f_1_28 : 2.060543} * {(fs_cbuf4_3.x) : 18.00}))
                f_4_18 = (0.f - (f_1_28 * (fs_cbuf4_3.x)));
                // -13.00034  <=>  (0.f - (max(0.f, (({pf_2_20 : 0.1025751} * {f_4_14 : 0.50}) + {pf_1_11 : 0.7324204})) * {(fs_cbuf4_3.y) : 16.58824}))
                f_3_10 = (0.f - (max(0.f, ((pf_2_20 * f_4_14) + pf_1_11)) * (fs_cbuf4_3.y)));
                // -5.23922  <=>  (0.f - (max(0.f, (({pf_2_20 : 0.1025751} * {f_9_1 : 0.50}) + {pf_8_12 : 0.2911451})) * {(fs_cbuf4_3.z) : 15.30}))
                f_1_49 = (0.f - (max(0.f, ((pf_2_20 * f_9_1) + pf_8_12)) * (fs_cbuf4_3.z)));
                // 37.08978  <=>  ((((max(0.f, {pf_0_15 : 0.1237507}) * {(fs_cbuf4_3.x) : 18.00}) + {f_4_18 : -37.08978}) * {(fs_cbuf4_7.x) : 0.00}) + ({f_1_28 : 2.060543} * {(fs_cbuf4_3.x) : 18.00}))
                pf_0_17 = ((((max(0.f, pf_0_15) * (fs_cbuf4_3.x)) + f_4_18) * (fs_cbuf4_7.x)) + (f_1_28 * (fs_cbuf4_3.x)));
                // 13.00034  <=>  ((((max(0.f, {pf_5_31 : 0.0640736}) * {(fs_cbuf4_3.y) : 16.58824}) + {f_3_10 : -13.00034}) * {(fs_cbuf4_7.x) : 0.00}) + (max(0.f, (({pf_2_20 : 0.1025751} * {f_4_14 : 0.50}) + {pf_1_11 : 0.7324204})) * {(fs_cbuf4_3.y) : 16.58824}))
                pf_1_14 = ((((max(0.f, pf_5_31) * (fs_cbuf4_3.y)) + f_3_10) * (fs_cbuf4_7.x)) + (max(0.f, ((pf_2_20 * f_4_14) + pf_1_11)) * (fs_cbuf4_3.y)));
                // 5.23922  <=>  ((((max(0.f, {pf_3_9 : 0.0434492}) * {(fs_cbuf4_3.z) : 15.30}) + {f_1_49 : -5.23922}) * {(fs_cbuf4_7.x) : 0.00}) + (max(0.f, (({pf_2_20 : 0.1025751} * {f_9_1 : 0.50}) + {pf_8_12 : 0.2911451})) * {(fs_cbuf4_3.z) : 15.30}))
                pf_2_22 = ((((max(0.f, pf_3_9) * (fs_cbuf4_3.z)) + f_1_49) * (fs_cbuf4_7.x)) + (max(0.f, ((pf_2_20 * f_9_1) + pf_8_12)) * (fs_cbuf4_3.z)));
                // 37.08978  <=>  {pf_0_17 : 37.08978}
                col.x = pf_0_17;
                // 13.00034  <=>  {pf_1_14 : 13.00034}
                col.y = pf_1_14;
                // 5.23922  <=>  {pf_2_22 : 5.23922}
                col.z = pf_2_22;
                // 1.00  <=>  {utof(u_0_6) : 1.00}
                col.w = utof(u_0_6);

                return col;
            }