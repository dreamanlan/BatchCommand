#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shader_ballot : enable
#extension GL_ARB_shader_group_vote : enable
#extension GL_ARB_gpu_shader_int64 : enable
layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;
layout(std140, binding = 0) uniform cs_cbuf_0 { uvec4 cs_cbuf0[4096]; };
layout(std140, binding = 1) uniform cs_cbuf_3 { uvec4 cs_cbuf3[4096]; };
layout(std430, binding = 0) buffer cs_ssbo_0 { uint cs_ssbo0[]; };
layout(binding = 0) uniform sampler2D tex0;
#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
uint CasMinS32(uint op_a, uint op_b) { return uint(min(int(op_a), int(op_b))); }
void main() {
  bool b_0 = bool(0);
  bool b_1 = bool(0);
  uint tu_0 = uint(0);
  uint u_0 = uint(0);
  uint u_1 = uint(0);
  uint u_2 = uint(0);
  uint u_3 = uint(0);
  uint u_4 = uint(0);
  uint u_5 = uint(0);
  uint u_6 = uint(0);
  uint u_7 = uint(0);
  uint u_8 = uint(0);
  float f_0 = float(0);
  float f_1 = float(0);
  float f_2 = float(0);
  uvec2 u2_0 = uvec2(0);
  uvec2 u2_1 = uvec2(0);
  uvec3 u3_0 = uvec3(0);
  uvec4 u4_0 = uvec4(0);
  vec4 f4_0 = vec4(0);
  precise float pf_0 = float(0);
  precise float pf_1 = float(0);
  bool shfl_in_bounds;
  u4_0 = uvec4(uvec2(textureSize(tex0, int(0u))), 0u, 0u);
  u_0 = u4_0.x;
  u_1 = u4_0.y;
  u3_0 = gl_WorkGroupID;
  u_2 = u3_0.x;
  u3_0 = gl_LocalInvocationID;
  u_3 = u3_0.x;
  u3_0 = gl_WorkGroupID;
  u_4 = u3_0.y;
  u3_0 = gl_LocalInvocationID;
  u_5 = u3_0.y;
  u_2 = u_2 << 3u;
  u_2 = u_2 + u_3;
  f_0 = float(int(u_1));
  f_1 = float(int(u_0));
  u_1 = u_1 + 4294967295u;
  u_0 = u_0 + 4294967295u;
  f_2 = utof(cs_cbuf3[0].y);
  f_2 = 0.f - (f_2);
  pf_0 = fma(f_0, f_2, f_0);
  f_0 = utof(cs_cbuf3[0].x);
  pf_1 = f_1 * f_0;
  f_0 = trunc(pf_0);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_3 = int(f_0);
  b_0 = isnan(pf_0);
  u_3 = b_0 ? 0u : u_3;
  f_0 = trunc(pf_1);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_6 = int(f_0);
  b_0 = isnan(pf_1);
  u_6 = b_0 ? 0u : u_6;
  u_1 = uint(int(0) - int(u_1));
  u_1 = u_1 + 4u;
  u_1 = u_1 + u_3;
  u_7 = u_3 + 4294967293u;
  u_0 = uint(int(0) - int(u_0));
  u_0 = u_0 + 4u;
  u_0 = u_0 + u_6;
  u_8 = u_6 + 4294967293u;
  u_1 = max(int(0u), int(u_1));
  u_0 = max(int(0u), int(u_0));
  u_8 = min(int(0u), int(u_8));
  u_7 = min(int(0u), int(u_7));
  u_4 = u_4 << 3u;
  u_4 = u_4 + u_5;
  u_0 = uint(int(0) - int(u_0));
  u_5 = uint(int(0) - int(u_8));
  u_0 = u_0 + u_6;
  u_0 = u_0 + u_5;
  u_1 = uint(int(0) - int(u_1));
  u_5 = uint(int(0) - int(u_7));
  u_1 = u_1 + u_3;
  u_1 = u_1 + u_5;
  u_0 = u_0 + u_2;
  u_1 = u_1 + u_4;
  u2_0 = uvec2(u_0, u_1);
  u2_1 = uvec2(4294967293u, 4294967293u);
  f4_0 = texelFetchOffset(tex0, ivec2(u2_0), int(0u), ivec2(u2_1));
  f_0 = f4_0.w;
  u_0 = gl_SubGroupInvocationARB & 31u;
  b_0 = u_0 == 0u;
  pf_0 = f_0 * 1000.f;
  u_0 = (cs_cbuf0[49].x);
  f_0 = trunc(pf_0);
  f_0 = min(max(f_0, float(-2147483600.f)), float(2147483600.f));
  u_1 = int(f_0);
  b_1 = isnan(pf_0);
  u_1 = b_1 ? 0u : u_1;
  shfl_in_bounds = int((gl_SubGroupInvocationARB ^ 1u)) <=
                   int(((gl_SubGroupInvocationARB & 0u)) | (31u & (~0u)));
  u_2 = shfl_in_bounds ? readInvocationARB(u_1, (gl_SubGroupInvocationARB ^ 1u))
                       : u_1;
  u_1 = min(int(u_1), int(u_2));
  shfl_in_bounds = int((gl_SubGroupInvocationARB ^ 2u)) <=
                   int(((gl_SubGroupInvocationARB & 0u)) | (31u & (~0u)));
  u_2 = shfl_in_bounds ? readInvocationARB(u_1, (gl_SubGroupInvocationARB ^ 2u))
                       : u_1;
  u_1 = min(int(u_1), int(u_2));
  shfl_in_bounds = int((gl_SubGroupInvocationARB ^ 4u)) <=
                   int(((gl_SubGroupInvocationARB & 0u)) | (31u & (~0u)));
  u_2 = shfl_in_bounds ? readInvocationARB(u_1, (gl_SubGroupInvocationARB ^ 4u))
                       : u_1;
  u_1 = min(int(u_1), int(u_2));
  shfl_in_bounds = int((gl_SubGroupInvocationARB ^ 8u)) <=
                   int(((gl_SubGroupInvocationARB & 0u)) | (31u & (~0u)));
  u_2 = shfl_in_bounds ? readInvocationARB(u_1, (gl_SubGroupInvocationARB ^ 8u))
                       : u_1;
  u_1 = min(int(u_1), int(u_2));
  shfl_in_bounds = int((gl_SubGroupInvocationARB ^ 16u)) <=
                   int(((gl_SubGroupInvocationARB & 0u)) | (31u & (~0u)));
  u_2 = shfl_in_bounds
            ? readInvocationARB(u_1, (gl_SubGroupInvocationARB ^ 16u))
            : u_1;
  u_1 = min(int(u_1), int(u_2));
  b_1 = b_0 ? true : false;
  if (b_1) {
    u_2 = (cs_cbuf0[49].x);
    u_0 = u_0 - u_2;
    for (;;) {
      uint old = cs_ssbo0[u_0 >> 2];
      tu_0 = atomicCompSwap(cs_ssbo0[u_0 >> 2], old,
                            CasMinS32(cs_ssbo0[u_0 >> 2], uint(u_1)));
      if (tu_0 == old) {
        break;
      }
      u_0 = 123;
    }
  }
  return;
}