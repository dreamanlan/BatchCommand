out gl_PerVertex{vec4 gl_Position;};layout(location=0)in vec4 in_attr0;layout(location=1)in vec4 in_attr1;layout(location=3)in vec4 in_attr3;layout(location=4)in vec4 in_attr4;layout(location=5)in vec4 in_attr5;layout(location=6)in vec4 in_attr6;layout(location=7)in vec4 in_attr7;layout(location=8)in vec4 in_attr8;layout(location=9)in vec4 in_attr9;layout(location=10)in vec4 in_attr10;layout(location=11)in vec4 in_attr11;layout(location=0)out vec4 out_attr0;layout(location=1)out vec4 out_attr1;layout(location=2)out vec4 out_attr2;layout(location=3)out vec4 out_attr3;layout(location=4)out vec4 out_attr4;layout(location=5)out vec4 out_attr5;layout(std140,binding=0) uniform vs_cbuf_8{uvec4 vs_cbuf8[31];};layout(std140,binding=1) uniform vs_cbuf_9{uvec4 vs_cbuf9[198];};layout(std140,binding=2) uniform vs_cbuf_10{uvec4 vs_cbuf10[4];};layout(std140,binding=3) uniform vs_cbuf_12{uvec4 vs_cbuf12[12];};layout(std140,binding=4) uniform vs_cbuf_15{uvec4 vs_cbuf15[53];};layout(binding=0) uniform sampler2D tex0;
void main(){
in_attr0.x = float(0.00);
in_attr0.y = float(0.00);
in_attr0.z = float(0.001);
in_attr0.w = float(1.00);
in_attr1.x = float(0.50196);
in_attr1.y = float(0.50196);
in_attr3.x = float(1.00);
in_attr3.y = float(1.00);
in_attr3.z = float(1.00);
in_attr3.w = float(1.00);
in_attr4.x = float(12.2097);
in_attr4.y = float(1.6336);
in_attr4.z = float(27.63979);
in_attr4.w = float(200.00);
in_attr5.x = float(0.00);
in_attr5.y = float(0.00);
in_attr5.z = float(0.00);
in_attr5.w = float(263.00);
in_attr6.x = float(5.00);
in_attr6.y = float(5.00);
in_attr6.z = float(5.00);
in_attr6.w = float(1.00);
in_attr7.x = float(0.4664);
in_attr7.y = float(0.4643);
in_attr7.z = float(0.44973);
in_attr7.w = float(0.86573);
in_attr8.x = float(0.00);
in_attr8.y = float(0.00);
in_attr8.z = float(0.00);
in_attr8.w = float(3.00);
in_attr9.x = float(1.00);
in_attr9.y = float(0.00);
in_attr9.z = float(0.00);
in_attr9.w = float(886.4256);
in_attr10.x = float(0.00);
in_attr10.y = float(1.00);
in_attr10.z = float(0.00);
in_attr10.w = float(125.03452);
in_attr11.x = float(0.00);
in_attr11.y = float(0.00);
in_attr11.z = float(1.00);
in_attr11.w = float(2363.07495);
gl_Position.x = float(-1.76702);
gl_Position.y = float(3.52854);
gl_Position.z = float(2.67051);
gl_Position.w = float(2.87048);
out_attr0.x = float(2.50);
out_attr0.y = float(2.50);
out_attr0.z = float(2.50);
out_attr0.w = float(1.50);
out_attr1.x = float(0.00);
out_attr1.y = float(0.00);
out_attr1.z = float(0.00);
out_attr1.w = float(1.00);
out_attr2.x = float(0.43575);
out_attr2.y = float(0.25454);
out_attr2.z = float(0.00);
out_attr2.w = float(1.00);
out_attr3.x = float(0.70);
out_attr3.y = float(0.00);
out_attr3.z = float(0.00);
out_attr3.w = float(1.00);
out_attr4.x = float(1.00);
out_attr4.y = float(1.00);
out_attr4.z = float(1.00);
out_attr4.w = float(1.00);
out_attr5.x = float(1.00);
out_attr5.y = float(0.00);
out_attr5.z = float(0.00);
out_attr5.w = float(1.00);
vs_cbuf8[0] = vec4(0.3411014, -1.1641534E-10, 0.9400266, -2523.7144);
vs_cbuf8[1] = vec4(0.0038459, 0.9999917, -0.0013955286, -125.14482);
vs_cbuf8[2] = vec4(-0.9400186, 0.0040912, 0.3410985, 26.70357);
vs_cbuf8[3] = vec4(0, 0, 0, 1.00);
vs_cbuf8[4] = vec4(1.206285, 0, 0, 0);
vs_cbuf8[5] = vec4(0, 2.144507, 0, 0);
vs_cbuf8[6] = vec4(0, 0, -1.000008, -0.2000008);
vs_cbuf8[7] = vec4(0, 0, -1, 0);
vs_cbuf8[24] = vec4(0.3411014, 0.0038459, -0.9400186, 0);
vs_cbuf8[25] = vec4(-1.1641534E-10, 0.9999917, 0.0040912, 0);
vs_cbuf8[26] = vec4(0.9400266, -0.0013955286, 0.3410985, 0);
vs_cbuf8[29] = vec4(886.4257, 125.0345, 2363.075, 0);
vs_cbuf8[30] = vec4(0.10, 25000.00, 2500.00, 24999.90);
vs_cbuf9[7] = uvec4(1073741825, 0, 301056, 25);
vs_cbuf9[11] = uvec4(0, 0, 0, 0);
vs_cbuf9[12] = uvec4(0, 0, 0, 0);
vs_cbuf9[16] = uvec4(0, 0, 0, 0);
vs_cbuf9[74] = uvec4(0, 981668463, 0, 0);
vs_cbuf9[75] = uvec4(1065353216, 1065353216, 0, 0);
vs_cbuf9[76] = uvec4(1069547520, 1069547520, 0, 0);
vs_cbuf9[78] = uvec4(1065353216, 1065353216, 1065353216, 1065353216);
vs_cbuf9[104] = uvec4(1075838976, 0, 0, 0);
vs_cbuf9[105] = uvec4(1065353216, 1065353216, 1065353216, 0);
vs_cbuf9[113] = uvec4(0, 0, 0, 0);
vs_cbuf9[114] = uvec4(1077936128, 1077936128, 1077936128, 1046562734);
vs_cbuf9[115] = uvec4(1077936128, 1077936128, 1077936128, 1061494456);
vs_cbuf9[116] = uvec4(0, 0, 0, 1065353216);
vs_cbuf9[121] = uvec4(0, 0, 0, 0);
vs_cbuf9[141] = uvec4(1065353216, 1065353216, 1065353216, 0);
vs_cbuf9[194] = uvec4(0, 0, 1086918619, 0);
vs_cbuf9[195] = uvec4(0, 0, 968295123, 1065353216);
vs_cbuf9[196] = uvec4(0, 0, 988070792, 0);
vs_cbuf9[197] = uvec4(1077936128, 1112014848, 0, 0);
vs_cbuf10[0] = vec4(1.00, 1.00, 1.00, 1.00);
vs_cbuf10[1] = vec4(1.00, 1.00, 1.00, 1.00);
vs_cbuf10[2] = vec4(440.00, 23.00, 1.00, 1.00);
vs_cbuf10[3] = vec4(0.70, 1.00, 1.00, 1.00);
vs_cbuf12[0] = uvec4(0, 0, 0, 1065353216);
vs_cbuf12[1] = uvec4(1050253722, 1050253722, 1050253722, 0);
vs_cbuf12[2] = uvec4(1065353216, 0, 0, 0);
vs_cbuf12[3] = uvec4(0, 1065353216, 0, 0);
vs_cbuf12[4] = uvec4(0, 0, 1065353216, 0);
vs_cbuf12[5] = uvec4(1147077781, 1123878171, 1158910062, 1065353216);
vs_cbuf12[6] = uvec4(1065353216, 2147483648, 0, 2147483648);
vs_cbuf12[7] = uvec4(2147483648, 1065353216, 2147483648, 0);
vs_cbuf12[8] = uvec4(0, 2147483648, 1065353216, 2147483648);
vs_cbuf12[9] = uvec4(3294561429, 3271361819, 3306393710, 1065353216);
vs_cbuf12[10] = uvec4(0, 0, 0, 0);
vs_cbuf12[11] = uvec4(1084227584, 1082130432, 1084227584, 0);
vs_cbuf15[49] = vec4(0, 0, 0, 0);
vs_cbuf15[51] = vec4(950.00, 50.00, 1.50, 1.00);
vs_cbuf15[52] = vec4(688.00, 2160.00, 0.0025, 0);
bool b_0=bool(0);bool b_1=bool(0);bool b_2=bool(0);bool b_3=bool(0);bool b_4=bool(0);bool b_5=bool(0);bool b_6=bool(0);bool b_7=bool(0);uint u_0=uint(0);uint u_1=uint(0);uint u_2=uint(0);uint u_3=uint(0);uint u_4=uint(0);uint u_5=uint(0);uint u_6=uint(0);uint u_7=uint(0);uint u_8=uint(0);uint u_9=uint(0);uint u_10=uint(0);uint u_11=uint(0);uint u_12=uint(0);uint u_13=uint(0);uint u_14=uint(0);uint u_15=uint(0);uint u_16=uint(0);uint u_17=uint(0);uint u_18=uint(0);uint u_19=uint(0);uint u_20=uint(0);uint u_21=uint(0);uint u_22=uint(0);float f_0=float(0);float f_1=float(0);float f_2=float(0);float f_3=float(0);float f_4=float(0);float f_5=float(0);float f_6=float(0);float f_7=float(0);float f_8=float(0);float f_9=float(0);float f_10=float(0);float f_11=float(0);float f_12=float(0);float f_13=float(0);float f_14=float(0);vec2 f2_0=vec2(0);vec4 f4_0=vec4(0);precise float pf_0=float(0);precise float pf_1=float(0);precise float pf_2=float(0);precise float pf_3=float(0);precise float pf_4=float(0);precise float pf_5=float(0);precise float pf_6=float(0);precise float pf_7=float(0);precise float pf_8=float(0);precise float pf_9=float(0);precise float pf_10=float(0);precise float pf_11=float(0);precise float pf_12=float(0);precise float pf_13=float(0);precise float pf_14=float(0);precise float pf_15=float(0);precise float pf_16=float(0);precise float pf_17=float(0);precise float pf_18=float(0);precise float pf_19=float(0);precise float pf_20=float(0);precise float pf_21=float(0);precise float pf_22=float(0);precise float pf_23=float(0);u_0=0u;
u_1=0u;
u_2=0u;
u_3=0u;
gl_Position=vec4(0,0,0,1);
out_attr0=vec4(0,0,0,1);
out_attr1=vec4(0,0,0,1);
out_attr2=vec4(0,0,0,1);
out_attr3=vec4(0,0,0,1);
out_attr4=vec4(0,0,0,1);
out_attr5=vec4(0,0,0,1);
f_0=in_attr4.w;
f_1=trunc(f_0);
f_1=min(max(f_1,float(-2147483600.f)),float(2147483600.f));
u_4=int(f_1);
b_0=isnan(f_0);
u_4=b_0? (0u) : (u_4);
b_0=int(u_4)<=int(0u);
b_1=b_0? (true) : (false);
if(b_1){
gl_Position.x=0.f;
}
b_1=b_0? (true) : (false);
u_5=u_3;
if(b_1){
u_6=(vs_cbuf8[30].y);
u_5=u_6;
}
b_1=b_0? (true) : (false);
if(b_1){
gl_Position.y=0.f;
}
b_1=b_0? (true) : (false);
u_3=u_5;
if(b_1){
f_0=utof(u_5);
pf_0=f_0*5.f;
u_6=ftou(pf_0);
u_3=u_6;
}
b_1=b_0? (true) : (false);
if(b_1){
out_attr3.x=0.f;
}
b_1=b_0? (true) : (false);
if(b_1){
f_0=utof(u_3);
gl_Position.z=f_0;
}
b_1=b_0? (true) : (false);
if(b_1){
return;
}
f_0=in_attr5.w;
f_1=float(int(u_4));
u_4=ftou(f_1);
f_2=utof(vs_cbuf10[2].x);
f_0=0.f-(f_0);
pf_0=f_0+f_2;
u_5=ftou(pf_0);
b_0=pf_0>=f_1&&!isnan(pf_0)&&!isnan(f_1);
b_1=pf_0<0.f&&!isnan(pf_0)&&!isnan(0.f);
b_0=b_1||b_0;
b_1=b_0? (true) : (false);
if(b_1){
gl_Position.x=0.f;
}
b_1=b_0? (true) : (false);
u_6=u_3;
if(b_1){
u_7=(vs_cbuf8[30].y);
u_6=u_7;
}
b_1=b_0? (true) : (false);
if(b_1){
gl_Position.y=0.f;
}
b_1=b_0? (true) : (false);
u_3=u_6;
if(b_1){
f_0=utof(u_6);
pf_1=f_0*5.f;
u_7=ftou(pf_1);
u_3=u_7;
}
b_1=b_0? (true) : (false);
if(b_1){
out_attr3.x=0.f;
}
b_1=b_0? (true) : (false);
if(b_1){
f_0=utof(u_3);
gl_Position.z=f_0;
}
b_1=b_0? (true) : (false);
if(b_1){
return;
}
f_0=utof(vs_cbuf9[11].y);
b_0=0.f<f_0&&!isnan(0.f)&&!isnan(f_0);
f_0=in_attr7.x;
f_1=in_attr6.x;
f_2=in_attr6.y;
f_3=in_attr6.z;
f_4=min(0.f,f_0);
f_4=min(max(f_4,0.0),1.0);
pf_1=f_4+f_1;
f_1=utof(vs_cbuf9[141].y);
pf_2=f_2*f_1;
f_1=utof(vs_cbuf9[141].z);
pf_3=f_3*f_1;
f_1=utof(vs_cbuf9[141].x);
pf_1=pf_1*f_1;
b_1=!b_0;
b_2=b_0? (true) : (false);
u_3=u_2;
u_6=u_4;
if(b_2){
f_1=utof(vs_cbuf9[12].z);
pf_4=f_0*f_1;
f_1=utof(vs_cbuf9[11].y);
f_1=(1.0f)/f_1;
f_2=utof(vs_cbuf9[11].y);
pf_4=fma(pf_4,f_2,pf_0);
pf_4=pf_4*f_1;
u_7=ftou(pf_4);
f_1=floor(pf_4);
f_1=0.f-(f_1);
pf_4=pf_4+f_1;
u_8=ftou(pf_4);
u_3=u_8;
u_6=u_7;
}
b_0=b_1? (true) : (false);
u_2=u_3;
if(b_0){
f_1=utof(u_6);
f_1=(1.0f)/f_1;
pf_4=pf_0*f_1;
u_4=ftou(pf_4);
u_2=u_4;
}
f_1=in_attr4.x;
f_2=utof(vs_cbuf10[3].y);
pf_1=pf_1*f_2;
f_2=in_attr11.x;
f_3=utof(vs_cbuf9[75].x);
pf_4=f_0*f_3;
f_3=in_attr9.x;
f_4=utof(vs_cbuf9[74].w);
f_5=utof(vs_cbuf9[75].y);
pf_5=f_5+f_4;
f_4=in_attr10.x;
f_5=utof(vs_cbuf9[74].z);
f_6=utof(vs_cbuf9[75].x);
pf_6=f_6+f_5;
f_5=in_attr7.y;
f_6=utof(vs_cbuf10[3].z);
pf_2=pf_2*f_6;
f_6=in_attr4.y;
b_0=f_0>0.5f&&!isnan(f_0)&&!isnan(0.5f);
u_3=b_0? (4294967295u) : (0u);
f_7=in_attr11.y;
pf_4=fma(pf_4,-2.f,pf_6);
f_8=in_attr9.y;
pf_6=f_1*f_2;
f_2=in_attr0.x;
pf_7=f_1*f_3;
f_3=in_attr0.y;
pf_8=f_1*f_4;
f_1=in_attr0.z;
f_4=in_attr10.y;
f_9=utof(vs_cbuf9[75].y);
pf_9=f_5*f_9;
f_9=in_attr8.z;
pf_6=fma(f_6,f_7,pf_6);
f_7=utof(vs_cbuf10[3].w);
pf_3=pf_3*f_7;
pf_7=fma(f_6,f_8,pf_7);
pf_10=f_0*2.f;
pf_5=fma(pf_9,-2.f,pf_5);
f_7=floor(pf_10);
f_8=utof(vs_cbuf9[16].x);
pf_9=fma(0.5f,f_8,f_2);
f_2=utof(vs_cbuf9[16].y);
pf_10=fma(0.5f,f_2,f_3);
f_2=utof(vs_cbuf9[114].w);
f_3=utof(vs_cbuf9[115].w);
f_2=0.f-(f_2);
pf_11=f_3+f_2;
pf_1=pf_1*pf_9;
f_2=in_attr4.z;
u_4=(vs_cbuf9[7].y);
u_4=1u&u_4;
f_3=(1.0f)/pf_11;
f_8=utof(vs_cbuf9[16].z);
pf_9=fma(0.5f,f_8,f_1);
f_1=utof(vs_cbuf9[116].w);
f_8=utof(vs_cbuf9[115].w);
f_8=0.f-(f_8);
pf_11=f_8+f_1;
pf_2=pf_2*pf_10;
f_1=in_attr9.z;
b_0=f_5>0.5f&&!isnan(f_5)&&!isnan(0.5f);
u_6=b_0? (4294967295u) : (0u);
pf_8=fma(f_6,f_4,pf_8);
u_7=(vs_cbuf9[7].x);
pf_3=pf_3*pf_9;
f_4=in_attr11.z;
b_0=u_4==1u;
u_4=b_0? (4294967295u) : (0u);
f_6=utof(vs_cbuf9[115].x);
f_8=utof(vs_cbuf9[116].x);
f_6=0.f-(f_6);
pf_9=f_8+f_6;
u_8=u_7&536870912u;
u_9=u_7&268435456u;
u_10=(vs_cbuf9[7].y);
u_10=2u&u_10;
u_7=u_7&1073741824u;
u_4=~u_4;
u_3=~u_3;
u_3=u_4|u_3;
b_0=u_3!=0u;
f_6=(1.0f)/pf_11;
f_8=utof(vs_cbuf9[114].w);
f_10=utof(vs_cbuf9[113].w);
f_10=0.f-(f_10);
pf_10=f_10+f_8;
b_1=int(u_8)>=int(0u);
u_3=b_1? (0u) : (1u);
f_8=(1.0f)/pf_10;
b_1=u_10==2u;
u_4=b_1? (4294967295u) : (0u);
f_10=in_attr10.z;
b_1=int(u_8)>int(0u);
u_8=b_1? (4294967295u) : (0u);
f_11=in_attr7.z;
b_1=int(u_9)>=int(0u);
u_10=b_1? (0u) : (1u);
u_4=~u_4;
u_6=~u_6;
u_4=u_4|u_6;
b_1=u_4!=0u;
b_2=int(u_9)>int(0u);
u_4=b_2? (4294967295u) : (0u);
pf_7=fma(f_2,f_1,pf_7);
b_2=int(u_7)>=int(0u);
u_6=b_2? (0u) : (1u);
f_1=utof(vs_cbuf9[114].w);
f_12=utof(u_2);
f_1=0.f-(f_1);
pf_10=f_12+f_1;
u_8=uint(int(0)-int(u_8));
u_3=uint(int(0)-int(u_3));
u_3=u_8+u_3;
pf_9=pf_9*f_6;
u_3=abs(int(u_3));
f_1=float(int(u_3));
u_3=uint(int(0)-int(u_4));
u_4=uint(int(0)-int(u_10));
u_3=u_3+u_4;
f_6=utof(vs_cbuf9[115].x);
f_12=utof(vs_cbuf9[114].x);
f_12=0.f-(f_12);
pf_11=f_12+f_6;
u_3=abs(int(u_3));
f_6=float(int(u_3));
b_2=int(u_7)>int(0u);
u_3=b_2? (4294967295u) : (0u);
f_12=utof(vs_cbuf9[115].w);
f_13=utof(u_2);
f_12=0.f-(f_12);
pf_12=f_13+f_12;
b_2=f_7>0.f&&!isnan(f_7)&&!isnan(0.f);
u_4=b_2? (1065353216u) : (0u);
f_12=utof(vs_cbuf9[113].x);
f_13=utof(vs_cbuf9[114].x);
f_12=0.f-(f_12);
pf_13=f_13+f_12;
b_2=f_7<0.f&&!isnan(f_7)&&!isnan(0.f);
u_7=b_2? (1065353216u) : (0u);
pf_11=pf_11*f_3;
f_3=in_attr11.w;
pf_6=fma(f_2,f_4,pf_6);
f_4=in_attr1.x;
u_8=ftou(f_4);
pf_8=fma(f_2,f_10,pf_8);
f_2=utof(vs_cbuf9[114].x);
pf_10=fma(pf_11,pf_10,f_2);
f_2=utof(vs_cbuf9[115].x);
pf_9=fma(pf_9,pf_12,f_2);
pf_11=f_11*2.f;
f_2=utof(u_7);
f_7=utof(u_4);
f_2=0.f-(f_2);
pf_12=f_7+f_2;
f_2=floor(pf_11);
pf_11=pf_13*f_8;
f_7=utof(vs_cbuf9[78].z);
f_8=trunc(f_7);
f_8=min(max(f_8,float(-2147483600.f)),float(2147483600.f));
u_4=int(f_8);
b_2=isnan(f_7);
u_4=b_2? (0u) : (u_4);
pf_13=f_5*2.f;
f_7=utof(vs_cbuf9[113].w);
f_8=utof(u_2);
f_7=0.f-(f_7);
pf_14=f_8+f_7;
f_7=floor(pf_13);
pf_13=f_5+f_11;
f_8=in_attr10.w;
pf_15=f_0+f_11;
f_10=float(u_4);
f_12=utof(vs_cbuf9[195].w);
b_2=0.f==f_12&&!isnan(0.f)&&!isnan(f_12);
u_7=uint(bitfieldExtract(uint(u_4),int(0u),int(32u)));
u_7=uint(bitfieldExtract(uint(u_7),int(0u),int(32u)));
b_3=u_7==0;
b_4=int(u_7)<0;
f_12=utof(vs_cbuf9[113].x);
pf_11=fma(pf_11,pf_14,f_12);
pf_14=f_0+f_5;
pf_13=fma(pf_13,0.5f,-0.5f);
b_5=f_2>0.f&&!isnan(f_2)&&!isnan(0.f);
u_9=b_5? (1065353216u) : (0u);
pf_15=fma(pf_15,0.5f,-0.5f);
b_5=f_2<0.f&&!isnan(f_2)&&!isnan(0.f);
u_10=b_5? (1065353216u) : (0u);
u_11=(vs_cbuf9[195].w);
pf_14=fma(pf_14,0.5f,-0.5f);
b_5=!b_2;
b_6=b_5? (true) : (false);
u_12=u_1;
if(b_6){
f_2=utof(vs_cbuf9[195].w);
f_2=abs(f_2);
f_2=log2(f_2);
u_13=ftou(f_2);
u_12=u_13;
}
f_2=utof(vs_cbuf9[196].y);
pf_13=pf_13*f_2;
b_5=f_7<0.f&&!isnan(f_7)&&!isnan(0.f);
u_1=b_5? (1065353216u) : (0u);
f_2=utof(vs_cbuf9[196].z);
pf_15=pf_15*f_2;
b_5=f_7>0.f&&!isnan(f_7)&&!isnan(0.f);
u_13=b_5? (1065353216u) : (0u);
pf_6=pf_6+f_3;
u_14=uint(bitfieldExtract(int(u_4),int(0u),int(32u)));
u_14=abs(int(u_14));
u_14=uint(bitfieldExtract(uint(u_14),int(0u),int(32u)));
u_3=uint(int(0)-int(u_3));
u_6=uint(int(0)-int(u_6));
u_3=u_3+u_6;
f_2=in_attr9.w;
f_3=utof(vs_cbuf9[195].y);
pf_13=fma(pf_13,2.f,f_3);
u_3=abs(int(u_3));
f_3=float(int(u_3));
f_7=utof(vs_cbuf9[196].x);
pf_14=pf_14*f_7;
f_7=utof(vs_cbuf9[195].z);
pf_15=fma(pf_15,2.f,f_7);
f_7=float(u_14);
f_12=utof(u_10);
f_13=utof(u_9);
f_12=0.f-(f_12);
pf_16=f_13+f_12;
f_12=utof(u_13);
f_13=utof(u_1);
f_13=0.f-(f_13);
pf_17=f_13+f_12;
f_10=(1.0f)/f_10;
u_1=ftou(f_10);
f_10=utof(vs_cbuf9[113].w);
f_12=utof(u_2);
b_5=f_12>=f_10&&!isnan(f_12)&&!isnan(f_10);
u_3=b_5? (1065353216u) : (0u);
pf_8=pf_8+f_8;
f_8=float(0u);
f_10=utof(vs_cbuf9[114].w);
f_12=utof(u_2);
b_5=f_12>=f_10&&!isnan(f_12)&&!isnan(f_10);
u_6=b_5? (1065353216u) : (0u);
f_7=(1.0f)/f_7;
u_9=ftou(f_7);
f_7=utof(vs_cbuf9[195].w);
b_5=f_7==1.f&&!isnan(f_7)&&!isnan(1.f);
f_7=utof(vs_cbuf9[113].x);
f_10=utof(vs_cbuf9[113].x);
f_12=utof(u_3);
f_10=0.f-(f_10);
pf_18=fma(f_12,f_10,f_7);
b_6=!b_2;
b_7=b_6? (true) : (false);
u_10=u_12;
if(b_7){
f_7=utof(u_12);
pf_19=pf_0*f_7;
u_13=ftou(pf_19);
u_10=u_13;
}
f_7=utof(u_3);
f_10=utof(u_6);
f_12=utof(u_3);
f_10=0.f-(f_10);
pf_19=fma(f_12,f_10,f_7);
u_3=ftou(pf_19);
u_1=u_1+4294967294u;
pf_7=pf_7+f_2;
b_6=!b_2;
b_7=b_6? (true) : (false);
u_12=u_0;
if(b_7){
u_12=u_10;
}
f_2=in_attr8.y;
f_7=utof(vs_cbuf9[76].y);
f_10=utof(vs_cbuf9[76].w);
pf_20=f_10+f_7;
f_7=utof(vs_cbuf9[76].x);
f_10=utof(vs_cbuf9[76].z);
pf_21=f_10+f_7;
pf_11=fma(pf_11,pf_19,pf_18);
b_6=!b_2;
b_7=b_6? (true) : (false);
u_0=u_3;
if(b_7){
f_7=utof(u_12);
f_7=exp2(f_7);
u_10=ftou(f_7);
u_0=u_10;
}
f_7=utof(vs_cbuf9[115].w);
f_10=utof(u_2);
b_6=f_10>=f_7&&!isnan(f_10)&&!isnan(f_7);
u_3=b_6? (1065353216u) : (0u);
f_7=in_attr8.x;
b_6=!b_5;
b_7=b_6? (true) : (false);
u_10=u_11;
if(b_7){
f_10=utof(vs_cbuf9[195].w);
f_10=0.f-(f_10);
pf_18=f_10+1.f;
u_12=ftou(pf_18);
u_10=u_12;
}
f_10=utof(u_1);
pf_18=f_8*f_10;
b_6=!b_5;
b_7=b_6? (true) : (false);
u_11=u_10;
if(b_7){
f_10=utof(u_10);
f_10=(1.0f)/f_10;
u_12=ftou(f_10);
u_11=u_12;
}
f_10=utof(vs_cbuf9[76].w);
pf_19=fma(f_5,f_10,pf_20);
f_10=trunc(pf_18);
f_10=min(max(f_10,float(0.f)),float(4294967300.f));
u_10=uint(f_10);
f_10=utof(vs_cbuf9[76].z);
pf_18=fma(f_0,f_10,pf_21);
b_6=b_2? (true) : (false);
u_12=u_0;
if(b_6){
u_12=1065353216u;
}
f_10=utof(u_6);
f_12=utof(u_3);
f_13=utof(u_6);
f_12=0.f-(f_12);
pf_20=fma(f_13,f_12,f_10);
f_10=utof(vs_cbuf9[116].w);
f_12=utof(u_2);
b_2=f_12>=f_10&&!isnan(f_12)&&!isnan(f_10);
u_0=b_2? (1065353216u) : (0u);
f_10=utof(vs_cbuf9[74].y);
pf_5=fma(pf_0,f_10,pf_5);
f_10=utof(vs_cbuf9[75].w);
pf_19=fma(pf_0,f_10,pf_19);
f_10=utof(vs_cbuf9[74].x);
f_10=0.f-(f_10);
f_12=0.f-(pf_4);
pf_4=fma(pf_0,f_10,f_12);
f_10=utof(vs_cbuf9[75].z);
pf_0=fma(pf_0,f_10,pf_18);
f_10=utof(vs_cbuf9[195].x);
pf_14=fma(pf_14,2.f,f_10);
f_10=in_attr1.y;
u_2=ftou(f_10);
f_12=utof(u_3);
f_13=utof(u_0);
f_14=utof(u_3);
f_13=0.f-(f_13);
pf_18=fma(f_14,f_13,f_12);
b_2=!b_5;
b_5=b_2? (true) : (false);
u_3=u_5;
if(b_5){
f_12=utof(u_11);
f_13=utof(u_12);
f_14=utof(u_11);
f_13=0.f-(f_13);
pf_21=fma(f_14,f_13,f_12);
u_6=ftou(pf_21);
u_3=u_6;
}
pf_16=pf_16*f_6;
pf_10=fma(pf_10,pf_20,pf_11);
pf_11=pf_17*f_3;
u_5=u_9+4294967294u;
u_6=uint(bitfieldExtract(uint(u_4),int(0u),int(16u)));
u_9=uint(bitfieldExtract(uint(u_10),int(0u),int(16u)));
u_6=uint(u_6*u_9);
u_9=uint(bitfieldExtract(uint(u_4),int(0u),int(16u)));
u_11=uint(bitfieldExtract(uint(u_10),int(16u),int(16u)));
u_9=uint(u_9*u_11);
u_11=uint(bitfieldExtract(uint(u_10),int(0u),int(16u)));
u_9=bitfieldInsert(u_9,u_11,int(16u),int(16u));
pf_12=pf_12*f_1;
pf_17=pf_16*f_7;
pf_9=fma(pf_9,pf_18,pf_10);
pf_10=pf_14*pf_16;
pf_16=pf_11*f_9;
f_1=utof(u_5);
pf_18=f_1*f_8;
f_1=utof(vs_cbuf12[0].y);
pf_20=pf_8+f_1;
f_1=trunc(pf_18);
f_1=min(max(f_1,float(0.f)),float(4294967300.f));
u_11=uint(f_1);
u_12=uint(bitfieldExtract(uint(u_4),int(16u),int(16u)));
u_13=uint(bitfieldExtract(uint(u_9),int(16u),int(16u)));
u_12=uint(u_12*u_13);
u_12=u_12<<16u;
u_9=u_9<<16u;
u_6=u_9+u_6;
u_6=u_12+u_6;
pf_11=pf_15*pf_11;
pf_17=fma(pf_17,-2.f,f_7);
pf_10=fma(pf_10,-2.f,pf_14);
pf_14=pf_12*f_2;
pf_16=fma(pf_16,-2.f,f_9);
pf_18=f_0+-0.5f;
pf_21=f_11+-0.5f;
f_0=utof(vs_cbuf12[0].x);
pf_22=pf_7+f_0;
f_0=utof(vs_cbuf12[7].y);
pf_23=pf_20*f_0;
pf_11=fma(pf_11,-2.f,pf_15);
pf_14=fma(pf_14,-2.f,f_2);
f_0=utof(vs_cbuf12[7].x);
pf_15=pf_20*f_0;
f_0=utof(vs_cbuf9[194].x);
pf_17=fma(pf_18,f_0,pf_17);
f_0=utof(vs_cbuf12[7].z);
pf_18=pf_20*f_0;
pf_20=f_5+-0.5f;
f_0=utof(vs_cbuf9[194].z);
pf_16=fma(pf_21,f_0,pf_16);
f_0=utof(vs_cbuf12[6].y);
pf_21=fma(pf_22,f_0,pf_23);
f_0=utof(vs_cbuf12[0].z);
pf_23=pf_6+f_0;
u_9=uint(bitfieldExtract(uint(u_14),int(0u),int(16u)));
u_12=uint(bitfieldExtract(uint(u_11),int(0u),int(16u)));
u_9=uint(u_9*u_12);
u_12=uint(bitfieldExtract(uint(u_14),int(0u),int(16u)));
u_13=uint(bitfieldExtract(uint(u_11),int(16u),int(16u)));
u_12=uint(u_12*u_13);
u_13=uint(bitfieldExtract(uint(u_11),int(0u),int(16u)));
u_12=bitfieldInsert(u_12,u_13,int(16u),int(16u));
f_0=utof(vs_cbuf12[6].x);
pf_15=fma(pf_22,f_0,pf_15);
pf_12=pf_13*pf_12;
f_0=utof(vs_cbuf12[6].z);
pf_18=fma(pf_22,f_0,pf_18);
f_0=utof(vs_cbuf9[194].y);
pf_14=fma(pf_20,f_0,pf_14);
u_6=uint(int(0)-int(u_6));
f_0=utof(vs_cbuf12[8].y);
pf_20=fma(pf_23,f_0,pf_21);
f_0=float(u_6);
u_6=uint(bitfieldExtract(uint(u_14),int(16u),int(16u)));
u_13=uint(bitfieldExtract(uint(u_12),int(16u),int(16u)));
u_6=uint(u_6*u_13);
u_6=u_6<<16u;
u_12=u_12<<16u;
u_9=u_12+u_9;
u_6=u_6+u_9;
f_1=utof(vs_cbuf12[11].x);
f_1=(1.0f)/f_1;
f_2=utof(vs_cbuf12[8].x);
pf_15=fma(pf_23,f_2,pf_15);
f_2=utof(vs_cbuf12[11].y);
f_2=(1.0f)/f_2;
pf_12=fma(pf_12,-2.f,pf_13);
f_3=utof(vs_cbuf12[8].z);
pf_13=fma(pf_23,f_3,pf_18);
f_3=utof(vs_cbuf12[9].y);
pf_18=pf_20+f_3;
u_6=uint(int(0)-int(u_6));
f_3=utof(vs_cbuf12[11].z);
f_3=(1.0f)/f_3;
f_5=utof(vs_cbuf12[9].x);
pf_15=pf_15+f_5;
f_5=float(u_6);
f_6=utof(u_3);
pf_10=fma(pf_10,f_6,pf_17);
f_6=utof(u_3);
pf_12=fma(pf_12,f_6,pf_14);
f_6=utof(u_3);
pf_11=fma(pf_11,f_6,pf_16);
f_6=utof(vs_cbuf12[9].z);
pf_13=pf_13+f_6;
f_6=utof(u_1);
pf_14=f_6*f_0;
pf_15=pf_15*f_1;
pf_16=pf_18*f_2;
f_0=trunc(pf_14);
f_0=min(max(f_0,float(0.f)),float(4294967300.f));
u_1=uint(f_0);
f_0=utof(vs_cbuf9[116].x);
f_1=utof(u_0);
pf_9=fma(f_1,f_0,pf_9);
f_0=cos(pf_10);
pf_13=pf_13*f_3;
f_1=cos(pf_12);
f_2=sin(pf_12);
f_3=utof(u_5);
pf_12=f_3*f_5;
f_3=sin(pf_10);
pf_10=fma(pf_15,0.5f,0.5f);
f_5=cos(pf_11);
pf_14=fma(pf_16,0.5f,0.5f);
f_6=sin(pf_11);
b_0=!b_0;
b_2=b_0? (true) : (false);
u_0=u_8;
if(b_2){
f_4=0.f-(f_4);
pf_11=f_4+1.f;
u_3=ftou(pf_11);
u_0=u_3;
}
f_4=trunc(pf_12);
f_4=min(max(f_4,float(0.f)),float(4294967300.f));
u_3=uint(f_4);
pf_11=fma(pf_13,0.5f,0.5f);
u_1=u_10+u_1;
b_0=!b_1;
b_1=b_0? (true) : (false);
u_5=u_2;
if(b_1){
f_4=0.f-(f_10);
pf_12=f_4+1.f;
u_6=ftou(pf_12);
u_5=u_6;
}
b_0=!b_3;
b_0=b_4||b_0;
u_2=uint(bitfieldExtract(int(u_4),int(0u),int(32u)));
u_2=uint(bitfieldExtract(uint(u_2),int(0u),int(32u)));
b_1=u_2==0;
b_2=int(u_2)<0;
f_4=utof(vs_cbuf10[0].w);
pf_9=pf_9*f_4;
pf_12=f_1*f_0;
out_attr0.w=pf_9;
pf_9=f_3*f_2;
pf_13=f_2*f_0;
pf_15=f_0*f_5;
pf_16=f_3*f_1;
pf_17=f_1*f_5;
pf_18=f_3*f_5;
pf_20=fma(f_6,pf_12,pf_9);
pf_9=fma(f_6,pf_9,pf_12);
u_3=u_11+u_3;
f_0=0.f-(pf_13);
pf_12=fma(f_6,pf_16,f_0);
f_0=0.f-(pf_16);
pf_13=fma(f_6,pf_13,f_0);
f_0=floor(pf_14);
u_6=uint(bitfieldExtract(uint(u_4),int(0u),int(16u)));
u_8=uint(bitfieldExtract(uint(u_1),int(0u),int(16u)));
u_6=uint(u_6*u_8);
u_8=uint(bitfieldExtract(uint(u_4),int(0u),int(16u)));
u_9=uint(bitfieldExtract(uint(u_1),int(16u),int(16u)));
u_8=uint(u_8*u_9);
u_9=uint(bitfieldExtract(uint(u_1),int(0u),int(16u)));
u_8=bitfieldInsert(u_8,u_9,int(16u),int(16u));
f_1=0.f-(pf_2);
pf_16=f_6*f_1;
pf_21=f_2*f_5;
f_1=floor(pf_10);
pf_15=pf_2*pf_15;
pf_2=pf_2*pf_18;
u_9=uint(bitfieldExtract(uint(u_14),int(0u),int(16u)));
u_10=uint(bitfieldExtract(uint(u_3),int(0u),int(16u)));
u_9=uint(u_9*u_10);
u_10=uint(bitfieldExtract(uint(u_4),int(16u),int(16u)));
u_11=uint(bitfieldExtract(uint(u_8),int(16u),int(16u)));
u_10=uint(u_10*u_11);
u_10=u_10<<16u;
u_8=u_8<<16u;
u_6=u_8+u_6;
u_6=u_10+u_6;
f_2=floor(pf_11);
pf_16=fma(pf_1,pf_17,pf_16);
u_8=uint(bitfieldExtract(uint(u_14),int(0u),int(16u)));
u_10=uint(bitfieldExtract(uint(u_3),int(16u),int(16u)));
u_8=uint(u_8*u_10);
u_10=uint(bitfieldExtract(uint(u_3),int(0u),int(16u)));
u_8=bitfieldInsert(u_8,u_10,int(16u),int(16u));
pf_15=fma(pf_20,pf_1,pf_15);
pf_1=fma(pf_1,pf_12,pf_2);
u_6=uint(int(0)-int(u_6));
f_3=utof(vs_cbuf9[78].z);
f_3=(1.0f)/f_3;
pf_2=fma(pf_3,pf_21,pf_16);
u_10=uint(bitfieldExtract(uint(u_14),int(16u),int(16u)));
u_11=uint(bitfieldExtract(uint(u_8),int(16u),int(16u)));
u_10=uint(u_10*u_11);
u_10=u_10<<16u;
u_8=u_8<<16u;
u_8=u_8+u_9;
u_8=u_10+u_8;
pf_12=fma(pf_3,pf_13,pf_15);
pf_1=fma(pf_9,pf_3,pf_1);
f_1=0.f-(f_1);
pf_3=pf_10+f_1;
b_3=uint(u_6)>=uint(u_4);
u_6=b_3? (4294967295u) : (0u);
u_8=uint(int(0)-int(u_8));
f_0=0.f-(f_0);
pf_9=pf_14+f_0;
u_1=uint(int(0)-int(u_1));
u_1=u_1+u_6;
f_0=utof(vs_cbuf8[26].x);
pf_10=pf_2*f_0;
f_0=utof(vs_cbuf8[24].x);
pf_13=pf_2*f_0;
f_0=utof(vs_cbuf8[25].x);
pf_2=pf_2*f_0;
b_3=uint(u_8)>=uint(u_14);
u_6=b_3? (4294967295u) : (0u);
f_0=in_attr3.x;
f_1=0.f-(f_2);
pf_11=pf_11+f_1;
u_8=uint(bitfieldExtract(uint(u_4),int(0u),int(16u)));
u_9=uint(bitfieldExtract(uint(u_1),int(16u),int(16u)));
u_8=uint(u_8*u_9);
u_9=uint(bitfieldExtract(uint(u_1),int(0u),int(16u)));
u_8=bitfieldInsert(u_8,u_9,int(16u),int(16u));
f_1=utof(vs_cbuf8[26].y);
pf_10=fma(pf_12,f_1,pf_10);
f_1=utof(vs_cbuf8[24].y);
pf_13=fma(pf_12,f_1,pf_13);
f_1=utof(vs_cbuf8[25].y);
pf_2=fma(pf_12,f_1,pf_2);
u_9=uint(bitfieldExtract(uint(u_4),int(0u),int(16u)));
u_1=uint(bitfieldExtract(uint(u_1),int(0u),int(16u)));
u_1=uint(u_9*u_1);
u_6=uint(int(0)-int(u_6));
u_3=u_3+u_6;
pf_12=pf_9+-0.5f;
f_1=utof(vs_cbuf8[26].z);
pf_10=fma(pf_1,f_1,pf_10);
f_1=utof(vs_cbuf8[24].z);
pf_13=fma(pf_1,f_1,pf_13);
u_6=ftou(pf_13);
f_1=utof(vs_cbuf8[25].z);
pf_1=fma(pf_1,f_1,pf_2);
pf_2=pf_3+-0.5f;
u_9=uint(bitfieldExtract(uint(u_4),int(16u),int(16u)));
u_10=uint(bitfieldExtract(uint(u_8),int(16u),int(16u)));
u_9=uint(u_9*u_10);
u_9=u_9<<16u;
u_8=u_8<<16u;
u_1=u_8+u_1;
u_1=u_9+u_1;
f_1=in_attr3.y;
u_4=u_4>>31u;
pf_12=pf_12*2.f;
pf_14=pf_11+-0.5f;
out_attr4.x=f_0;
pf_2=pf_2*2.f;
u_4=uint(int(0)-int(u_4));
f_0=in_attr3.z;
f_2=utof(vs_cbuf12[11].y);
pf_12=pf_12*f_2;
out_attr4.y=f_1;
f_1=utof(vs_cbuf12[11].x);
pf_2=pf_2*f_1;
u_1=b_0? (u_1) : (4294967295u);
u_3=u_3^u_4;
f_1=float(int(u_1));
pf_14=pf_14*2.f;
b_0=!b_1;
b_0=b_2||b_0;
f_2=utof(vs_cbuf12[3].y);
pf_15=pf_12*f_2;
out_attr4.z=f_0;
f_0=utof(vs_cbuf12[3].z);
pf_16=pf_12*f_0;
f_0=utof(vs_cbuf12[3].x);
pf_12=pf_12*f_0;
f_0=in_attr3.w;
f_2=utof(vs_cbuf12[11].z);
pf_14=pf_14*f_2;
f_2=utof(vs_cbuf12[2].y);
pf_15=fma(pf_2,f_2,pf_15);
f_2=utof(vs_cbuf12[2].z);
pf_16=fma(pf_2,f_2,pf_16);
f_2=utof(vs_cbuf9[78].x);
pf_17=f_3*f_2;
u_1=uint(int(0)-int(u_4));
u_1=u_1+u_3;
f_2=utof(vs_cbuf12[2].x);
pf_2=fma(pf_2,f_2,pf_12);
f_2=utof(vs_cbuf9[78].w);
f_2=(1.0f)/f_2;
f_3=utof(vs_cbuf12[4].z);
pf_12=fma(pf_14,f_3,pf_16);
pf_4=fma(pf_17,f_1,pf_4);
f_1=utof(u_0);
pf_16=fma(pf_17,f_1,-0.5f);
u_0=b_0? (u_1) : (4294967295u);
f_1=utof(vs_cbuf12[4].x);
pf_2=fma(pf_14,f_1,pf_2);
f_1=float(int(u_0));
f_3=utof(vs_cbuf12[4].y);
pf_14=fma(pf_14,f_3,pf_15);
out_attr4.w=f_0;
f_0=utof(vs_cbuf10[0].y);
f_3=utof(vs_cbuf9[105].y);
pf_15=f_3*f_0;
f_0=utof(vs_cbuf9[78].y);
pf_17=f_2*f_0;
f_0=utof(vs_cbuf10[0].x);
f_2=utof(vs_cbuf9[105].x);
pf_18=f_2*f_0;
f_0=utof(vs_cbuf9[104].x);
pf_15=pf_15*f_0;
out_attr0.y=pf_15;
f_0=utof(u_5);
pf_15=fma(pf_17,f_0,-0.5f);
f_0=0.f-(f_1);
pf_5=fma(pf_17,f_0,pf_5);
f_0=utof(vs_cbuf12[10].x);
b_0=f_0==1.f&&!isnan(f_0)&&!isnan(1.f);
f_0=utof(vs_cbuf8[26].w);
pf_10=pf_10+f_0;
f_0=utof(vs_cbuf8[25].w);
pf_1=pf_1+f_0;
f_0=utof(vs_cbuf12[5].x);
pf_2=pf_2+f_0;
f_0=utof(vs_cbuf8[24].w);
pf_13=pf_13+f_0;
f_0=utof(vs_cbuf12[5].y);
pf_14=pf_14+f_0;
f_0=utof(vs_cbuf12[5].z);
pf_12=pf_12+f_0;
f_0=utof(vs_cbuf9[104].x);
pf_17=pf_18*f_0;
f_0=utof(vs_cbuf10[0].z);
f_1=utof(vs_cbuf9[105].z);
pf_18=f_1*f_0;
out_attr0.x=pf_17;
f_0=utof(vs_cbuf10[1].x);
f_1=utof(vs_cbuf9[121].x);
pf_17=f_1*f_0;
f_0=utof(vs_cbuf10[1].y);
f_1=utof(vs_cbuf9[121].y);
pf_20=f_1*f_0;
f_0=utof(vs_cbuf10[1].z);
f_1=utof(vs_cbuf9[121].z);
pf_21=f_1*f_0;
pf_0=fma(pf_0,pf_16,pf_4);
f_0=0.f-(pf_5);
pf_4=fma(pf_19,pf_15,f_0);
pf_5=pf_6+pf_10;
u_0=ftou(pf_5);
pf_10=pf_7+pf_13;
pf_1=pf_8+pf_1;
u_1=ftou(pf_1);
f_0=0.f-(pf_7);
pf_7=f_0+pf_2;
f_0=0.f-(pf_8);
pf_8=f_0+pf_14;
f_0=0.f-(pf_6);
pf_6=f_0+pf_12;
f_0=utof(vs_cbuf9[104].x);
pf_13=pf_18*f_0;
f_0=utof(vs_cbuf9[104].x);
pf_15=pf_17*f_0;
out_attr0.z=pf_13;
f_0=utof(vs_cbuf9[104].x);
pf_13=pf_20*f_0;
out_attr1.x=pf_15;
f_0=utof(vs_cbuf9[104].x);
pf_15=pf_21*f_0;
out_attr1.y=pf_13;
pf_0=pf_0+0.5f;
out_attr1.z=pf_15;
pf_4=pf_4+0.5f;
out_attr2.x=pf_0;
pf_0=pf_10+pf_7;
u_3=ftou(pf_0);
out_attr2.y=pf_4;
pf_0=pf_1+pf_8;
u_4=ftou(pf_0);
pf_0=pf_5+pf_6;
u_5=ftou(pf_0);
b_1=!b_0;
b_2=b_0? (true) : (false);
u_8=u_4;
u_9=u_5;
u_10=u_3;
u_11=u_0;
b_3=b_0? (true) : (false);
b_4=false? (true) : (false);
if(b_2){
f_0=utof(vs_cbuf12[10].y);
b_2=pf_14>f_0&&!isnan(pf_14)&&!isnan(f_0);
b_5=!b_2;
b_6=b_2? (true) : (false);
u_12=u_4;
u_13=u_5;
u_14=u_3;
u_15=u_0;
if(b_6){
f_0=utof(vs_cbuf8[30].y);
pf_0=f_0*5.f;
u_16=ftou(pf_0);
u_12=0u;
u_13=u_16;
u_14=0u;
u_15=0u;
}
u_8=u_12;
u_9=u_13;
u_10=u_14;
u_11=u_15;
b_3=b_2? (true) : (false);
b_4=b_5? (true) : (false);
}
b_2=b_1||b_4;
b_5=b_2? (true) : (false);
u_0=u_11;
u_3=u_9;
u_4=u_8;
u_5=u_10;
if(b_5){
b_2=b_0? (true) : (false);
if(b_2){
}
b_0=!b_4;
b_0=b_1||b_0;
b_1=b_0? (true) : (false);
u_12=u_8;
u_13=u_9;
u_14=u_10;
u_15=u_11;
u_16=u_6;
b_0=b_3? (true) : (false);
if(b_1){
f_0=utof(vs_cbuf12[10].x);
b_1=f_0==2.f&&!isnan(f_0)&&!isnan(2.f);
u_17=b_1? (4294967295u) : (0u);
f_0=utof(vs_cbuf12[10].y);
b_1=pf_14<f_0&&!isnan(pf_14)&&!isnan(f_0);
u_18=b_1? (4294967295u) : (0u);
u_17=~u_17;
u_19=~u_18;
u_17=u_17|u_19;
b_1=u_17!=0u;
b_2=!b_1;
b_5=b_2? (true) : (false);
u_17=u_18;
if(b_5){
u_19=(vs_cbuf8[30].y);
u_17=u_19;
}
b_2=!b_1;
b_5=b_2? (true) : (false);
u_18=u_10;
if(b_5){
u_18=0u;
}
b_2=!b_1;
b_5=b_2? (true) : (false);
u_19=u_8;
if(b_5){
u_19=0u;
}
b_2=!b_1;
b_5=b_2? (true) : (false);
u_20=u_11;
if(b_5){
u_20=0u;
}
b_2=!b_1;
b_5=b_2? (true) : (false);
u_21=u_9;
if(b_5){
f_0=utof(u_17);
pf_0=f_0*5.f;
u_22=ftou(pf_0);
u_21=u_22;
}
u_12=u_19;
u_13=u_21;
u_14=u_18;
u_15=u_20;
u_16=u_17;
b_0=b_1? (true) : (false);
}
b_0=b_4||b_0;
b_1=b_0? (true) : (false);
u_6=u_15;
if(b_1){
f_0=utof(vs_cbuf12[1].x);
b_0=0.f<f_0&&!isnan(0.f)&&!isnan(f_0);
f_0=utof(vs_cbuf12[1].y);
b_1=0.f<f_0&&!isnan(0.f)&&!isnan(f_0);
f_0=utof(vs_cbuf12[1].z);
b_2=0.f<f_0&&!isnan(0.f)&&!isnan(f_0);
pf_0=fma(pf_3,2.f,-1.f);
pf_1=fma(pf_9,2.f,-1.f);
pf_3=fma(pf_11,2.f,-1.f);
b_3=b_0? (true) : (false);
u_17=u_16;
if(b_3){
u_18=(vs_cbuf12[1].x);
u_17=u_18;
}
f_0=abs(pf_0);
f_0=0.f-(f_0);
pf_0=f_0+1.f;
u_16=ftou(pf_0);
b_3=b_0? (true) : (false);
u_18=u_17;
if(b_3){
f_0=utof(u_17);
f_0=(1.0f)/f_0;
u_19=ftou(f_0);
u_18=u_19;
}
b_3=b_1? (true) : (false);
u_17=u_15;
if(b_3){
u_19=(vs_cbuf12[1].y);
u_17=u_19;
}
b_3=b_2? (true) : (false);
u_19=u_1;
if(b_3){
u_20=(vs_cbuf12[1].z);
u_19=u_20;
}
b_3=b_1? (true) : (false);
u_1=u_17;
if(b_3){
f_0=utof(u_17);
f_0=(1.0f)/f_0;
u_20=ftou(f_0);
u_1=u_20;
}
f_0=abs(pf_1);
f_0=0.f-(f_0);
pf_0=f_0+1.f;
u_17=ftou(pf_0);
b_3=b_2? (true) : (false);
u_20=u_19;
if(b_3){
f_0=utof(u_19);
f_0=(1.0f)/f_0;
u_21=ftou(f_0);
u_20=u_21;
}
f_0=abs(pf_3);
f_0=0.f-(f_0);
pf_0=f_0+1.f;
u_19=ftou(pf_0);
b_3=!b_0;
b_4=b_3? (true) : (false);
u_21=u_16;
if(b_4){
u_21=1065353216u;
}
b_3=!b_1;
b_4=b_3? (true) : (false);
u_16=u_17;
if(b_4){
u_16=1065353216u;
}
b_3=!b_2;
b_4=b_3? (true) : (false);
u_17=u_19;
if(b_4){
u_17=1065353216u;
}
b_3=b_0? (true) : (false);
u_19=u_21;
if(b_3){
f_0=utof(u_18);
f_1=utof(u_21);
pf_0=f_1*f_0;
f_0=min(max(pf_0,0.0),1.0);
u_18=ftou(f_0);
u_19=u_18;
}
b_0=b_1? (true) : (false);
u_18=u_16;
if(b_0){
f_0=utof(u_1);
f_1=utof(u_16);
pf_0=f_1*f_0;
f_0=min(max(pf_0,0.0),1.0);
u_1=ftou(f_0);
u_18=u_1;
}
b_0=b_2? (true) : (false);
u_1=u_17;
if(b_0){
f_0=utof(u_20);
f_1=utof(u_17);
pf_0=f_1*f_0;
f_0=min(max(pf_0,0.0),1.0);
u_16=ftou(f_0);
u_1=u_16;
}
f_0=utof(u_19);
f_1=utof(u_18);
pf_0=f_1*f_0;
f_0=utof(u_1);
pf_0=pf_0*f_0;
u_1=ftou(pf_0);
u_6=u_1;
}
u_0=u_6;
u_3=u_13;
u_4=u_12;
u_5=u_14;
}
f_0=utof(vs_cbuf8[29].x);
f_0=0.f-(f_0);
pf_0=pf_2+f_0;
f_0=utof(vs_cbuf8[29].y);
f_0=0.f-(f_0);
pf_1=pf_14+f_0;
f_0=utof(u_5);
f_1=0.f-(pf_2);
pf_3=f_1+f_0;
f_0=utof(vs_cbuf15[49].x);
b_0=0.f!=f_0||isnan(0.f)||isnan(f_0);
pf_0=pf_0*pf_0;
f_0=utof(vs_cbuf8[29].z);
f_0=0.f-(f_0);
pf_4=pf_12+f_0;
pf_0=fma(pf_1,pf_1,pf_0);
f_0=utof(vs_cbuf9[197].x);
f_0=(1.0f)/f_0;
pf_0=fma(pf_4,pf_4,pf_0);
f_1=sqrt(pf_0);
out_attr5.x=1.f;
f_2=utof(vs_cbuf9[197].x);
f_1=min(f_1,f_2);
pf_0=f_1*f_0;
f_0=utof(u_4);
f_1=0.f-(pf_14);
pf_1=f_1+f_0;
pf_2=fma(pf_0,pf_3,pf_2);
f_0=utof(u_3);
f_1=0.f-(pf_12);
pf_3=f_1+f_0;
pf_1=fma(pf_0,pf_1,pf_14);
f_0=utof(vs_cbuf10[3].x);
f_1=utof(u_0);
pf_4=f_1*f_0;
f_0=utof(vs_cbuf8[0].x);
pf_5=pf_2*f_0;
out_attr3.x=pf_4;
pf_0=fma(pf_0,pf_3,pf_12);
f_0=utof(vs_cbuf8[1].x);
pf_3=pf_2*f_0;
f_0=utof(vs_cbuf8[2].x);
pf_4=pf_2*f_0;
f_0=utof(vs_cbuf8[0].y);
pf_5=fma(pf_1,f_0,pf_5);
f_0=utof(vs_cbuf8[1].y);
pf_3=fma(pf_1,f_0,pf_3);
f_0=utof(vs_cbuf8[3].x);
pf_6=pf_2*f_0;
f_0=utof(vs_cbuf8[2].y);
pf_4=fma(pf_1,f_0,pf_4);
f_0=utof(vs_cbuf8[0].z);
pf_5=fma(pf_0,f_0,pf_5);
f_0=utof(vs_cbuf8[1].z);
pf_3=fma(pf_0,f_0,pf_3);
f_0=utof(vs_cbuf8[3].y);
pf_1=fma(pf_1,f_0,pf_6);
f_0=utof(vs_cbuf8[2].z);
pf_4=fma(pf_0,f_0,pf_4);
f_0=utof(vs_cbuf8[0].w);
pf_5=pf_5+f_0;
f_0=utof(vs_cbuf8[1].w);
pf_3=pf_3+f_0;
f_0=utof(vs_cbuf8[3].z);
pf_1=fma(pf_0,f_0,pf_1);
f_0=utof(vs_cbuf8[2].w);
pf_4=pf_4+f_0;
f_0=utof(vs_cbuf8[5].x);
pf_6=pf_5*f_0;
f_0=utof(vs_cbuf8[4].x);
pf_7=pf_5*f_0;
f_0=utof(vs_cbuf8[3].w);
pf_1=pf_1+f_0;
f_0=utof(vs_cbuf8[5].y);
pf_6=fma(pf_3,f_0,pf_6);
f_0=utof(vs_cbuf8[7].x);
pf_8=pf_5*f_0;
f_0=utof(vs_cbuf8[6].x);
pf_5=pf_5*f_0;
f_0=utof(vs_cbuf8[4].y);
pf_7=fma(pf_3,f_0,pf_7);
f_0=utof(vs_cbuf8[5].z);
pf_6=fma(pf_4,f_0,pf_6);
f_0=utof(vs_cbuf8[7].y);
pf_8=fma(pf_3,f_0,pf_8);
f_0=utof(vs_cbuf8[6].y);
pf_3=fma(pf_3,f_0,pf_5);
f_0=utof(vs_cbuf8[4].z);
pf_5=fma(pf_4,f_0,pf_7);
f_0=utof(vs_cbuf8[5].w);
pf_6=fma(pf_1,f_0,pf_6);
f_0=utof(vs_cbuf8[7].z);
pf_7=fma(pf_4,f_0,pf_8);
gl_Position.y=pf_6;
f_0=utof(vs_cbuf8[6].z);
pf_3=fma(pf_4,f_0,pf_3);
f_0=utof(vs_cbuf8[4].w);
pf_4=fma(pf_1,f_0,pf_5);
f_0=utof(vs_cbuf8[7].w);
pf_5=fma(pf_1,f_0,pf_7);
gl_Position.x=pf_4;
f_0=utof(vs_cbuf8[6].w);
pf_1=fma(pf_1,f_0,pf_3);
gl_Position.w=pf_5;
gl_Position.z=pf_1;
b_0=!b_0;
b_1=b_0? (true) : (false);
if(b_1){
return;
}
f_0=utof(vs_cbuf15[52].x);
f_0=0.f-(f_0);
pf_2=pf_2+f_0;
f_0=utof(vs_cbuf15[52].y);
f_0=0.f-(f_0);
pf_0=pf_0+f_0;
f_0=utof(vs_cbuf15[52].z);
pf_2=pf_2*f_0;
f_0=utof(vs_cbuf15[52].z);
pf_0=pf_0*f_0;
f2_0=vec2(pf_2,pf_0);
f4_0=textureLod(tex0,f2_0,0.0);
f_0=f4_0.w;
pf_0=fma(0.f,pf_1,pf_5);
pf_2=pf_5*0.5f;
f_1=(1.0f)/pf_0;
f_2=utof(vs_cbuf15[51].x);
f_2=(1.0f)/f_2;
pf_0=fma(pf_1,0.5f,pf_2);
pf_0=pf_0*f_1;
f_1=utof(vs_cbuf8[30].y);
f_3=utof(vs_cbuf8[30].w);
f_1=0.f-(f_1);
pf_0=fma(pf_0,f_3,f_1);
f_1=utof(vs_cbuf15[51].y);
pf_1=f_2*f_1;
f_1=(1.0f)/pf_0;
f_3=utof(vs_cbuf8[30].z);
f_3=0.f-(f_3);
pf_0=f_1*f_3;
f_1=0.f-(pf_1);
pf_0=fma(pf_0,f_2,f_1);
f_1=min(max(pf_0,0.0),1.0);
f_2=utof(vs_cbuf15[49].x);
f_3=utof(vs_cbuf15[49].x);
f_2=0.f-(f_2);
pf_0=fma(f_0,f_3,f_2);
pf_0=pf_0+1.f;
f_0=0.f-(f_1);
pf_1=fma(pf_0,f_0,f_1);
f_0=abs(pf_1);
f_0=log2(f_0);
f_1=utof(vs_cbuf15[51].z);
pf_1=f_0*f_1;
f_0=exp2(pf_1);
f_1=utof(vs_cbuf15[51].w);
pf_1=f_0*f_1;
f_0=utof(vs_cbuf15[49].x);
pf_1=pf_1*f_0;
f_0=0.f-(pf_1);
pf_0=fma(pf_0,f_0,pf_0);
out_attr5.x=pf_0;
return;
}