vec4 gl_FragCoord;
layout(location = 0) in vec4 in_attr0;
layout(location = 0) out vec4 frag_color0;
layout(location = 1) out vec4 frag_color1;
layout(location = 2) out vec4 frag_color2;
layout(location = 3) out vec4 frag_color3;
layout(location = 4) out vec4 frag_color4;
layout(location = 5) out vec4 frag_color5;
layout(location = 6) out vec4 frag_color6;
layout(location = 7) out vec4 frag_color7;
layout(std140, binding = 0) uniform fs_cbuf_0 { uvec4 fs_cbuf0[4096]; };
layout(std140, binding = 1) uniform fs_cbuf_3 { uvec4 fs_cbuf3[4096]; };
layout(std430, binding = 0) buffer fs_ssbo_0 { uint fs_ssbo0[]; };
layout(binding = 1) uniform sampler2D tex1;
void main() {
gl_FragCoord = vec4(0, 255, 1, 1);
in_attr0.x = float(-6.25000E-06);
in_attr0.y = float(-0.00001);
in_attr0.z = float(0.00);
in_attr0.w = float(1.00);
fs_cbuf0[81] = uvec4(675610624, 6, 29376, 0);
fs_cbuf3[0] = uvec4(1065353216, 1187205120, 1071877691, 1065352545);
fs_cbuf3[1] = uvec4(0, 0, 1187204608, 942131044);
  bool b_0 = bool(0);
  bool b_1 = bool(0);
  uint u_0 = uint(0);
  uint u_1 = uint(0);
  uint u_2 = uint(0);
  uint u_3 = uint(0);
  uint u_ssbo = uint(0);
  float f_0 = float(0);
  float f_1 = float(0);
  float f_2 = float(0);
  float f_3 = float(0);
  float f_4 = float(0);
  float f_5 = float(0);
  float f_6 = float(0);
  float f_7 = float(0);
  vec2 f2_0 = vec2(0);
  vec4 f4_0 = vec4(0);
  precise float pf_0 = float(0);
  precise float pf_1 = float(0);
  f_0 = in_attr0.x;
  f_1 = in_attr0.y;
  f2_0 = vec2(f_0, f_1);
  f4_0 = textureGather(tex1, f2_0, int(0));
  f_2 = f4_0.x;
  f_3 = f4_0.y;
  f_4 = f4_0.z;
  f_5 = f4_0.w;
  f_6 = gl_FragCoord.x;
  f_7 = gl_FragCoord.y;
  pf_0 = f_0 * 32.f;
  pf_1 = f_1 * 18.f;
  f_0 = trunc(pf_0);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_0 = int(f_0);
  b_0 = isnan(pf_0);
  u_0 = b_0 ? ( 0u ) : ( u_0);
  f_0 = trunc(pf_1);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_1 = int(f_0);
  b_0 = isnan(pf_1);
  u_1 = b_0 ? ( 0u ) : ( u_1);
  u_1 = u_1 << 5u;
  u_0 = u_1 + u_0;
  f_0 = trunc(f_6);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_1 = int(f_0);
  b_0 = isnan(f_6);
  u_1 = b_0 ? ( 0u ) : ( u_1);
  f_0 = trunc(f_7);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_2 = int(f_0);
  b_0 = isnan(f_7);
  u_2 = b_0 ? ( 0u ) : ( u_2);
  u_1 = u_1 & 1u;
  u_1 = bitfieldInsert(u_1, u_2, int(1u), int(1u));
  u_0 = u_0 << 2u;
  u_0 = u_0 + u_1;
  u_0 = u_0 << 2u;
  u_0 = u_0 + 19264u;
  u_1 = (fs_cbuf0[81].x);
  u_0 = u_0 + u_1;
  f_0 = min(f_2, f_3);
  f_1 = min(f_4, f_5);
  f_0 = min(f_0, f_1);
  f_1 = utof(fs_cbuf3[0].x);
  f_2 = utof(fs_cbuf3[1].z);
  pf_0 = fma(f_5, f_2, f_1);
  b_0 = f_5 == 1.f && !isnan(f_5) && !isnan(1.f);
  f_1 = (1.0f) / pf_0;
  pf_0 = f_0 * 16777215.f;
  f_0 = trunc(pf_0);
  f_0 = min(max(f_0, float(0.f)), float(4294967300.f));
  u_1 = uint(f_0);
  f_0 = utof(fs_cbuf3[0].y);
  pf_0 = f_5 * f_0;
  pf_0 = pf_0 * f_1;
  u_2 = ftou(pf_0);
  u_0 = u_0 + 0u;
  u_3 = (fs_cbuf0[81].x);
  u_3 = u_3 & 4294967280u;
  u_0 = u_0 - u_3;
  u_ssbo = atomicMin(fs_ssbo0[u_0 >> 2], u_1);
  b_1 = b_0 ? ( true ) : ( false);
  u_0 = u_2;
  if (b_1) {
    u_0 = 1065353216u;
  }
  frag_color0.x = f_5;
  frag_color0.y = f_5;
  frag_color0.z = f_5;
  frag_color0.w = f_5;
  f_0 = utof(u_0);
  gl_FragDepth = f_0;
  return;
}
