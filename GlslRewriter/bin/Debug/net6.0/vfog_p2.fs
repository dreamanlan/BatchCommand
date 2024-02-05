#version 460
#pragma optionNV(fastmath off)
#extension GL_ARB_separate_shader_objects : enable
layout(location=0)in vec4 in_attr0;layout(location=1)in vec4 in_attr1;layout(location=2)in vec4 in_attr2;layout(location=3)in vec4 in_attr3;layout(location=4)in vec4 in_attr4;layout(location=0)out vec4 frag_color0;layout(location=1)out vec4 frag_color1;layout(location=2)out vec4 frag_color2;layout(location=3)out vec4 frag_color3;layout(location=4)out vec4 frag_color4;layout(location=5)out vec4 frag_color5;layout(location=6)out vec4 frag_color6;layout(location=7)out vec4 frag_color7;layout(std140,binding=4) uniform fs_cbuf_9{uvec4 fs_cbuf9[140];};layout(std140,binding=5) uniform fs_cbuf_15{uvec4 fs_cbuf15[34];};layout(binding=1) uniform sampler2D tex1;
#define ftoi floatBitsToInt
#define ftou floatBitsToUint
#define itof intBitsToFloat
#define utof uintBitsToFloat
void main(){
bool b_0=bool(0);bool b_1=bool(0);uint u_0=uint(0);uint u_1=uint(0);uint u_2=uint(0);uint u_3=uint(0);uint u_4=uint(0);float f_0=float(0);float f_1=float(0);float f_2=float(0);float f_3=float(0);float f_4=float(0);float f_5=float(0);float f_6=float(0);float f_7=float(0);vec2 f2_0=vec2(0);vec4 f4_0=vec4(0);precise float pf_0=float(0);precise float pf_1=float(0);precise float pf_2=float(0);precise float pf_3=float(0);precise float pf_4=float(0);precise float pf_5=float(0);precise float pf_6=float(0);u_0=0u;
u_1=0u;
f_0=in_attr3.w;
f_1=in_attr0.w;
f_2=in_attr2.x;
u_2=ftou(f_2);
pf_0=f_0*f_1;
f_1=min(max(pf_0,0.0),1.0);
u_3=ftou(f_1);
pf_0=f_1*f_2;
f_1=utof(fs_cbuf9[139].z);
b_0=pf_0<=f_1&&!isnan(pf_0)&&!isnan(f_1);
b_1=b_0?true:false;
if(b_1){
discard;
}
b_1=b_0?true:false;
u_4=u_3;
if(b_1){
u_4=0u;
}
b_1=b_0?true:false;
u_3=u_2;
if(b_1){
u_3=0u;
}
b_1=b_0?true:false;
u_2=u_1;
if(b_1){
u_2=0u;
}
b_1=b_0?true:false;
u_1=u_0;
if(b_1){
u_1=0u;
}
b_1=b_0?true:false;
if(b_1){
f_1=utof(u_4);
frag_color0.x=f_1;
f_1=utof(u_3);
frag_color0.y=f_1;
f_1=utof(u_2);
frag_color0.z=f_1;
f_1=utof(u_1);
frag_color0.w=f_1;
return;
}
f_1=in_attr1.w;
f_2=gl_FragCoord.w;
f_1=f_1*f_2;
f_1=(1.0f)/f_1;
f_2=in_attr1.x;
f_3=gl_FragCoord.w;
f_2=f_2*f_3;
f_2=f_2*f_1;
f_3=in_attr1.y;
f_4=gl_FragCoord.w;
f_3=f_3*f_4;
f_1=f_3*f_1;
f2_0=vec2(f_2,f_1);
f4_0=texture(tex1,f2_0);
f_1=f4_0.x;
f_2=in_attr0.x;
f_3=in_attr0.y;
f_4=in_attr0.z;
f_5=in_attr4.x;
f_6=utof(fs_cbuf15[33].y);
f_7=utof(fs_cbuf15[33].w);
pf_1=f_7*f_6;
f_6=utof(fs_cbuf15[33].z);
f_7=utof(fs_cbuf15[33].w);
pf_2=f_7*f_6;
pf_3=f_0*f_2;
pf_4=f_0*f_3;
pf_5=f_0*f_4;
f_0=utof(fs_cbuf15[33].x);
f_2=utof(fs_cbuf15[33].w);
pf_6=f_2*f_0;
pf_1=pf_4*pf_1;
pf_2=pf_5*pf_2;
pf_3=pf_3*pf_6;
pf_1=pf_1*f_5;
pf_3=pf_3*f_5;
pf_2=pf_2*f_5;
pf_4=f_1*0.5f;
f_0=min(max(pf_4,0.0),1.0);
pf_0=pf_0*f_0;
f_0=min(max(pf_0,0.0),1.0);
frag_color0.x=pf_3;
frag_color0.y=pf_1;
frag_color0.z=pf_2;
frag_color0.w=f_0;
return;
}