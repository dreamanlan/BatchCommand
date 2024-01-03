#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable
layout(location = 0) in vec4 in_attr0;
layout(location = 0) out vec4 frag_color0;
layout(location = 1) out vec4 frag_color1;
layout(location = 2) out vec4 frag_color2;
layout(location = 3) out vec4 frag_color3;
layout(location = 4) out vec4 frag_color4;
layout(location = 5) out vec4 frag_color5;
layout(location = 6) out vec4 frag_color6;
layout(location = 7) out vec4 frag_color7;
layout(std140, binding = 0) uniform fs_cbuf_3 { uvec4 fs_cbuf3[4096]; };
layout(binding = 1) uniform sampler2D tex1;
#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
void main() {
  bool b_0 = bool(0);
  bool b_1 = bool(0);
  uint u_0 = uint(0);
  uint u_1 = uint(0);
  float f_0 = float(0);
  float f_1 = float(0);
  float f_2 = float(0);
  float f_3 = float(0);
  vec2 f2_0 = vec2(0);
  vec4 f4_0 = vec4(0);
  precise float pf_0 = float(0);
  precise float pf_1 = float(0);
  f_0 = gl_FragCoord.w;
  f_1 = in_attr0.x;
  f_2 = gl_FragCoord.w;
  f_1 = f_1 * f_2;
  f_2 = in_attr0.y;
  f_3 = gl_FragCoord.w;
  f_2 = f_2 * f_3;
  f_0 = (1.0f) / f_0;
  pf_0 = f_0 * f_1;
  pf_1 = f_0 * f_2;
  f2_0 = vec2(pf_0, pf_1);
  f4_0 = texture(tex1, f2_0);
  f_0 = f4_0.x;
  f_1 = utof(fs_cbuf3[0].y);
  f_1 = 0.f - (f_1);
  pf_0 = fma(f_0, f_1, 1.f);
  b_0 = f_0 == 1.f && !isnan(f_0) && !isnan(1.f);
  f_0 = (1.0f) / pf_0;
  f_1 = utof(fs_cbuf3[0].x);
  f_2 = utof(fs_cbuf3[0].x);
  f_1 = 0.f - (f_1);
  pf_0 = fma(f_0, f_2, f_1);
  u_0 = ftou(pf_0);
  b_1 = b_0 ? true : false;
  u_1 = u_0;
  if (b_1) {
    u_1 = 1065353216u;
  }
  f_0 = utof(u_1);
  frag_color0.x = f_0;
  f_0 = utof(u_1);
  frag_color0.y = f_0;
  f_0 = utof(u_1);
  frag_color0.z = f_0;
  f_0 = utof(u_1);
  frag_color0.w = f_0;
  return;
}