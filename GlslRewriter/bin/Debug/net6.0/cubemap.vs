#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable
out gl_PerVertex { vec4 gl_Position; };
layout(location = 0) in vec4 in_attr0;
layout(location = 1) in vec4 in_attr1;
layout(location = 0) out vec4 out_attr0;
#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
void main() {
  float f_0 = float(0);
  float f_1 = float(0);
  float f_2 = float(0);
  float f_3 = float(0);
  precise float pf_0 = float(0);
  precise float pf_1 = float(0);
  gl_Position = vec4(0, 0, 0, 1);
  out_attr0 = vec4(0, 0, 0, 1);
  f_0 = in_attr0.x;
  f_1 = in_attr0.y;
  f_2 = in_attr1.x;
  f_3 = in_attr1.y;
  pf_0 = f_0 * 2.f;
  out_attr0.x = f_2;
  pf_1 = f_1 * 2.f;
  out_attr0.y = f_3;
  gl_Position.x = pf_0;
  gl_Position.y = pf_1;
  return;
}