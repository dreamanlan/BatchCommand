#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable
layout(location = 0) in vec4 in_attr0;
layout(location = 1) in vec4 in_attr1;
layout(location = 2) in vec4 in_attr2;
layout(location = 3) in vec4 in_attr3;
layout(location = 4) in vec4 in_attr4;
layout(location = 5) in vec4 in_attr5;
layout(location = 6) in vec4 in_attr6;
layout(location = 7) in vec4 in_attr7;
layout(location = 8) in vec4 in_attr8;
layout(location = 9) in vec4 in_attr9;
layout(location = 10) in vec4 in_attr10;
layout(location = 0) out vec4 frag_color0;
layout(location = 1) out vec4 frag_color1;
layout(location = 2) out vec4 frag_color2;
layout(location = 3) out vec4 frag_color3;
layout(location = 4) out vec4 frag_color4;
layout(location = 5) out vec4 frag_color5;
layout(location = 6) out vec4 frag_color6;
layout(location = 7) out vec4 frag_color7;
layout(std140, binding = 6) uniform fs_cbuf_8 { uvec4 fs_cbuf8[4096]; };
layout(std140, binding = 7) uniform fs_cbuf_9 { uvec4 fs_cbuf9[4096]; };
layout(std140, binding = 8) uniform fs_cbuf_13 { uvec4 fs_cbuf13[4096]; };
layout(std140, binding = 9) uniform fs_cbuf_15 { uvec4 fs_cbuf15[4096]; };
layout(binding = 2) uniform sampler2D tex2;
layout(binding = 3) uniform sampler2D tex3;
layout(binding = 4) uniform sampler2D tex4;
layout(binding = 5) uniform samplerCube tex5;
#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
void main() {
  bool b_0 = bool(0);
  bool b_1 = bool(0);
  uint u_0 = uint(0);
  uint u_1 = uint(0);
  uint u_2 = uint(0);
  uint u_3 = uint(0);
  uint u_4 = uint(0);
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
  float f_10 = float(0);
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
  f_0 = in_attr3.w;
  f_1 = gl_FragCoord.w;
  f_0 = f_0 * f_1;
  f_0 = (1.0f) / f_0;
  f_1 = in_attr2.w;
  f_2 = in_attr2.z;
  f_3 = in_attr2.x;
  f_4 = in_attr2.y;
  f_5 = in_attr3.x;
  f_6 = gl_FragCoord.w;
  f_5 = f_5 * f_6;
  f_5 = f_5 * f_0;
  f_6 = in_attr3.y;
  f_7 = gl_FragCoord.w;
  f_6 = f_6 * f_7;
  f_0 = f_6 * f_0;
  f2_0 = vec2(f_2, f_1);
  f4_0 = texture(tex3, f2_0);
  f_1 = f4_0.w;
  f2_0 = vec2(f_3, f_4);
  f4_0 = texture(tex2, f2_0);
  f_2 = f4_0.x;
  f_3 = f4_0.y;
  f_4 = f4_0.z;
  f_6 = f4_0.w;
  f2_0 = vec2(f_5, f_0);
  f4_0 = texture(tex4, f2_0);
  f_0 = f4_0.x;
  u_0 = ftou(f_0);
  f_5 = in_attr3.w;
  f_7 = in_attr3.z;
  f_8 = in_attr5.w;
  f_9 = in_attr0.w;
  f_5 = (1.0f) / f_5;
  f_10 = utof(fs_cbuf9[140].y);
  f_10 = (1.0f) / f_10;
  u_1 = ftou(f_10);
  pf_0 = f_7 * f_5;
  f_5 = utof(fs_cbuf8[30].y);
  f_7 = utof(fs_cbuf8[30].w);
  f_5 = 0.f - (f_5);
  pf_0 = fma(pf_0, f_7, f_5);
  f_5 = in_attr4.x;
  f_7 = (1.0f) / pf_0;
  pf_0 = f_6 + f_1;
  f_1 = utof(fs_cbuf8[30].x);
  f_6 = utof(fs_cbuf8[30].w);
  pf_1 = fma(f_0, f_6, f_1);
  pf_0 = pf_0 * f_8;
  u_2 = ftou(pf_0);
  f_0 = utof(fs_cbuf8[30].z);
  pf_1 = fma(f_7, f_0, pf_1);
  pf_0 = pf_0 * f_9;
  f_0 = min(max(pf_0, 0.0), 1.0);
  u_3 = ftou(f_0);
  pf_0 = pf_1 * f_10;
  f_1 = min(max(pf_0, 0.0), 1.0);
  pf_0 = f_1 * f_0;
  pf_0 = pf_0 * f_5;
  f_0 = utof(fs_cbuf9[139].z);
  b_0 = pf_0 <= f_0 && !isnan(pf_0) && !isnan(f_0);
  b_1 = b_0 ? true : false;
  if (b_1) {
    discard;
  }
  b_1 = b_0 ? true : false;
  u_4 = u_2;
  if (b_1) {
    u_4 = 0u;
  }
  b_1 = b_0 ? true : false;
  u_2 = u_1;
  if (b_1) {
    u_2 = 0u;
  }
  b_1 = b_0 ? true : false;
  u_1 = u_3;
  if (b_1) {
    u_1 = 0u;
  }
  b_1 = b_0 ? true : false;
  u_3 = u_0;
  if (b_1) {
    u_3 = 0u;
  }
  b_1 = b_0 ? true : false;
  if (b_1) {
    f_0 = utof(u_4);
    frag_color0.x = f_0;
    f_0 = utof(u_2);
    frag_color0.y = f_0;
    f_0 = utof(u_1);
    frag_color0.z = f_0;
    f_0 = utof(u_3);
    frag_color0.w = f_0;
    return;
  }
  f_0 = in_attr6.x;
  pf_1 = f_2 * f_2;
  f_1 = in_attr6.y;
  pf_2 = f_3 * f_3;
  f_2 = in_attr6.z;
  pf_3 = f_0 * f_0;
  pf_3 = fma(f_1, f_1, pf_3);
  pf_3 = fma(f_2, f_2, pf_3);
  f_3 = inversesqrt(pf_3);
  pf_3 = f_1 * f_3;
  pf_4 = f_0 * f_3;
  pf_5 = f_2 * f_3;
  f_0 = abs(pf_3);
  f_1 = abs(pf_4);
  f_0 = max(f_0, f_1);
  f_1 = abs(pf_5);
  f_0 = max(f_1, f_0);
  f_0 = (1.0f) / f_0;
  pf_4 = pf_4 * f_0;
  pf_3 = pf_3 * f_0;
  f_0 = 0.f - (f_0);
  pf_5 = pf_5 * f_0;
  f_0 = utof(fs_cbuf15[1].x);
  f3_0 = vec3(pf_4, pf_3, pf_5);
  f4_0 = textureLod(tex5, f3_0, f_0);
  f_0 = f4_0.x;
  f_1 = f4_0.y;
  f_2 = f4_0.z;
  f_3 = in_attr1.x;
  f_5 = in_attr0.x;
  f_6 = in_attr1.y;
  f_7 = in_attr0.y;
  f_8 = in_attr5.x;
  pf_3 = f_4 * f_4;
  f_4 = in_attr5.z;
  f_9 = 0.f - (f_3);
  pf_4 = f_9 + f_5;
  f_5 = 0.f - (f_6);
  pf_5 = f_5 + f_7;
  pf_1 = fma(pf_1, pf_4, f_3);
  f_3 = in_attr1.z;
  pf_2 = fma(pf_2, pf_5, f_6);
  f_5 = in_attr0.z;
  pf_1 = pf_1 * f_8;
  f_6 = in_attr8.w;
  f_7 = in_attr5.y;
  f_8 = in_attr9.z;
  f_9 = 0.f - (f_3);
  pf_4 = f_9 + f_5;
  pf_2 = pf_2 * f_7;
  pf_3 = fma(pf_3, pf_4, f_3);
  f_3 = in_attr9.x;
  pf_3 = pf_3 * f_4;
  f_4 = in_attr9.y;
  f_5 = 0.f - (f_6);
  pf_4 = fma(f_0, f_5, f_0);
  f_0 = 0.f - (f_6);
  pf_5 = fma(f_1, f_0, f_1);
  f_0 = 0.f - (f_6);
  pf_6 = fma(f_2, f_0, f_2);
  f_0 = utof(fs_cbuf13[0].y);
  pf_4 = pf_4 * f_0;
  f_0 = in_attr8.y;
  f_1 = utof(fs_cbuf13[0].y);
  pf_5 = pf_5 * f_1;
  f_1 = utof(fs_cbuf13[0].y);
  pf_6 = pf_6 * f_1;
  f_1 = utof(fs_cbuf15[1].w);
  pf_4 = fma(pf_4, f_1, f_3);
  f_1 = utof(fs_cbuf15[1].w);
  pf_5 = fma(pf_5, f_1, f_4);
  f_1 = utof(fs_cbuf15[1].w);
  pf_6 = fma(pf_6, f_1, f_8);
  pf_7 = pf_1 * pf_4;
  f_1 = utof(fs_cbuf15[26].x);
  f_2 = 0.f - (pf_4);
  pf_1 = fma(pf_1, f_2, f_1);
  f_1 = in_attr10.x;
  pf_4 = pf_2 * pf_5;
  f_2 = in_attr10.y;
  f_3 = utof(fs_cbuf15[26].y);
  f_4 = 0.f - (pf_5);
  pf_2 = fma(pf_2, f_4, f_3);
  f_3 = in_attr10.z;
  pf_5 = pf_3 * pf_6;
  f_4 = in_attr10.w;
  f_5 = utof(fs_cbuf15[26].z);
  f_6 = 0.f - (pf_6);
  pf_3 = fma(pf_3, f_6, f_5);
  f_5 = in_attr8.x;
  pf_2 = fma(pf_2, f_0, pf_4);
  f_6 = in_attr7.x;
  pf_1 = fma(pf_1, f_0, pf_7);
  pf_3 = fma(pf_3, f_0, pf_5);
  f_0 = 0.f - (pf_1);
  pf_4 = f_0 + f_1;
  f_0 = 0.f - (pf_2);
  pf_5 = f_0 + f_2;
  f_0 = 0.f - (pf_3);
  pf_6 = f_0 + f_3;
  pf_1 = fma(pf_4, f_4, pf_1);
  pf_2 = fma(pf_5, f_4, pf_2);
  pf_3 = fma(pf_6, f_4, pf_3);
  f_0 = utof(fs_cbuf15[25].x);
  f_1 = 0.f - (pf_1);
  pf_4 = f_1 + f_0;
  f_0 = utof(fs_cbuf15[25].y);
  f_1 = 0.f - (pf_2);
  pf_5 = f_1 + f_0;
  f_0 = utof(fs_cbuf15[25].z);
  f_1 = 0.f - (pf_3);
  pf_6 = f_1 + f_0;
  pf_1 = fma(pf_4, f_5, pf_1);
  pf_2 = fma(pf_5, f_5, pf_2);
  pf_3 = fma(pf_6, f_5, pf_3);
  pf_1 = pf_1 * f_6;
  pf_2 = pf_2 * f_6;
  pf_3 = pf_3 * f_6;
  f_0 = min(max(pf_0, 0.0), 1.0);
  frag_color0.x = pf_1;
  frag_color0.y = pf_2;
  frag_color0.z = pf_3;
  frag_color0.w = f_0;
  return;
}