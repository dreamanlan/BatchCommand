vec4 gl_FragCoord;
layout(location=0)in vec4 in_attr0;layout(location=1)in vec4 in_attr1;layout(location=2)in vec4 in_attr2;layout(location=3)in vec4 in_attr3;layout(location=4)in vec4 in_attr4;layout(location=5)in vec4 in_attr5;layout(location=0)out vec4 frag_color0;layout(location=1)out vec4 frag_color1;layout(location=2)out vec4 frag_color2;layout(location=3)out vec4 frag_color3;layout(location=4)out vec4 frag_color4;layout(location=5)out vec4 frag_color5;layout(location=6)out vec4 frag_color6;layout(location=7)out vec4 frag_color7;layout(std140,binding=5) uniform fs_cbuf_9{uvec4 fs_cbuf9[140];};layout(binding=1) uniform sampler2D tex1;
void main(){
gl_FragCoord = vec4(284.15543, 862.6293, 4.406625, 8.01319);
in_attr0.x = float(2.50);
in_attr0.y = float(2.50);
in_attr0.z = float(2.50);
in_attr0.w = float(3.00);
in_attr1.x = float(0.00);
in_attr1.y = float(0.00);
in_attr1.z = float(0.00);
in_attr1.w = float(1.00);
in_attr2.x = float(0.67167);
in_attr2.y = float(0.44615);
in_attr2.z = float(0.00);
in_attr2.w = float(1.00);
in_attr3.x = float(0.70);
in_attr3.y = float(0.00);
in_attr3.z = float(0.00);
in_attr3.w = float(1.00);
in_attr4.x = float(1.00);
in_attr4.y = float(1.00);
in_attr4.z = float(1.00);
in_attr4.w = float(1.00);
in_attr5.x = float(1.00);
in_attr5.y = float(0.00);
in_attr5.z = float(0.00);
in_attr5.w = float(1.00);
fs_cbuf9[139] = uvec4(1065353216, 0, 0, 0);
bool b_0=bool(0);bool b_1=bool(0);uint u_0=uint(0);uint u_1=uint(0);uint u_2=uint(0);uint u_3=uint(0);uint u_4=uint(0);float f_0=float(0);float f_1=float(0);float f_2=float(0);float f_3=float(0);float f_4=float(0);float f_5=float(0);float f_6=float(0);float f_7=float(0);float f_8=float(0);float f_9=float(0);float f_10=float(0);vec2 f2_0=vec2(0);vec4 f4_0=vec4(0);precise float pf_0=float(0);precise float pf_1=float(0);precise float pf_2=float(0);precise float pf_3=float(0);precise float pf_4=float(0);precise float pf_5=float(0);f_0=in_attr2.x;
f_1=in_attr2.y;
f2_0=vec2(f_0,f_1);
f4_0=vec4(textureQueryLod(tex1,f2_0),0.0,0.0);
f_2=f4_0.y;
u_0=uint(f_2);
u_0=u_0<<8u;
f_2=float(u_0);
pf_0=f_2*0.00390625f;
f_2=min(pf_0,2.f);
f2_0=vec2(f_0,f_1);
f4_0=textureLod(tex1,f2_0,f_2);
f_0=f4_0.x;
f_1=f4_0.y;
f_2=f4_0.z;
f_3=f4_0.w;
f_4=in_attr4.w;
f_5=in_attr0.w;
f_6=in_attr3.x;
f_7=in_attr1.y;
f_8=in_attr0.y;
f_9=in_attr0.z;
f_10=0.f-(f_7);
pf_0=f_10+f_8;
pf_1=f_3*f_4;
pf_1=pf_1*f_5;
f_3=min(max(pf_1,0.0),1.0);
f_4=in_attr1.x;
pf_1=f_3*f_6;
f_3=in_attr0.x;
f_5=utof(fs_cbuf9[139].z);
b_0=pf_1<=f_5&&!isnan(pf_1)&&!isnan(f_5);
f_5=in_attr1.z;
u_0=ftou(f_5);
b_1=b_0? (true) : (false);
if(b_1){
discard;
}
pf_2=f_0*f_0;
u_1=ftou(pf_2);
pf_3=f_1*f_1;
u_2=ftou(pf_3);
pf_4=f_2*f_2;
u_3=ftou(pf_4);
f_0=0.f-(f_4);
pf_5=f_0+f_3;
f_0=in_attr4.z;
pf_0=fma(pf_3,pf_0,f_7);
f_1=in_attr4.x;
b_1=b_0? (true) : (false);
u_4=u_2;
if(b_1){
u_4=0u;
}
pf_2=fma(pf_2,pf_5,f_4);
f_2=in_attr4.y;
f_3=0.f-(f_5);
pf_3=f_3+f_9;
b_1=b_0? (true) : (false);
u_2=u_1;
if(b_1){
u_2=0u;
}
pf_3=fma(pf_4,pf_3,f_5);
b_1=b_0? (true) : (false);
u_1=u_3;
if(b_1){
u_1=0u;
}
b_1=b_0? (true) : (false);
u_3=u_0;
if(b_1){
u_3=0u;
}
b_1=b_0? (true) : (false);
if(b_1){
f_3=utof(u_1);
frag_color0.x=f_3;
f_3=utof(u_3);
frag_color0.y=f_3;
f_3=utof(u_2);
frag_color0.z=f_3;
f_3=utof(u_4);
frag_color0.w=f_3;
return;
}
pf_2=pf_2*f_1;
f_1=in_attr5.x;
pf_0=pf_0*f_2;
pf_3=pf_3*f_0;
pf_2=pf_2*f_1;
pf_0=pf_0*f_1;
pf_3=pf_3*f_1;
f_0=min(max(pf_1,0.0),1.0);
frag_color0.x=pf_2;
frag_color0.y=pf_0;
frag_color0.z=pf_3;
frag_color0.w=f_0;
return;
}
