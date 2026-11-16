#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable
out gl_PerVertex { vec4 gl_Position; };
layout(location = 0) in vec4 in_attr0;
layout(location = 0) out vec4 out_attr0;
layout(location = 1) out vec4 out_attr1;
layout(location = 2) out vec4 out_attr2;
layout(std140, binding = 0) uniform vs_cbuf_3 { uvec4 vs_cbuf3[4096]; };
layout(std140, binding = 1) uniform vs_cbuf_4 { uvec4 vs_cbuf4[4096]; };
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
  float f_0 = float(0);
  float f_1 = float(0);
  float f_2 = float(0);
  float f_3 = float(0);
  float f_4 = float(0);
  precise float pf_0 = float(0);
  precise float pf_1 = float(0);
  gl_Position = vec4(0, 0, 0, 1);
  out_attr0 = vec4(0, 0, 0, 1);
  out_attr1 = vec4(0, 0, 0, 1);
  out_attr2 = vec4(0, 0, 0, 1);
  f_0 = in_attr0.x;
  f_1 = in_attr0.y;
  u_0 = (vs_cbuf4[7].y);
  b_0 = 0u != u_0;
  f_2 = utof(vs_cbuf4[3].w);
  f_2 = min(f_2, 0.9999f);
  f_3 = utof(vs_cbuf3[8].y);
  f_4 = utof(vs_cbuf3[8].x);
  f_4 = 0.f - (f_4);
  pf_0 = f_4 + f_3;
  f_2 = max(f_2, 0.0001f);
  f_3 = utof(vs_cbuf3[8].x);
  pf_0 = fma(pf_0, f_2, f_3);
  f_2 = (1.0f) / pf_0;
  out_attr0.x = pf_0;
  pf_0 = f_0 * 2.f;
  pf_1 = f_1 * 2.f;
  gl_Position.x = pf_0;
  f_0 = utof(vs_cbuf3[8].x);
  pf_0 = f_2 * f_0;
  u_0 = ftou(pf_0);
  gl_Position.y = pf_1;
  f_0 = 0.f - (pf_0);
  pf_0 = fma(pf_0, f_0, 1.f);
  f_0 = min(max(pf_0, 0.0), 1.0);
  b_1 = b_0 ? true : false;
  u_1 = u_0;
  if (b_1) {
    u_1 = 3212836864u;
  }
  f_0 = sqrt(f_0);
  f_1 = 0.f - (f_0);
  b_0 = !b_0;
  b_1 = b_0 ? true : false;
  u_0 = u_1;
  if (b_1) {
    f_2 = utof(vs_cbuf3[2].w);
    f_0 = 0.f - (f_0);
    pf_0 = f_0 + f_2;
    u_2 = ftou(pf_0);
    u_0 = u_2;
  }
  out_attr1.x = f_1;
  f_0 = utof(u_0);
  out_attr2.x = f_0;
  return;
}