void vert_from_glsl(appdata v, vaodata i, inout v2f o)
{
    // 1u, 0u, 301056u, 0u
    uint vs_cbuf9_7_x = 1u;
    uint vs_cbuf9_7_y = 0u;
    uint vs_cbuf9_7_z = 301056u;
    uint vs_cbuf9_7_w = 0u;

	// 1065353216 = 1.00f;
	// vs_cbuf0[21] = float4(1.0935697E-14, 8E-45, 8.2508E-41, 0);
	// vs_cbuf8[0] = float4(-0.7425708, 1.493044E-08, 0.6697676, 1075.086);
	// vs_cbuf8[1] = float4(0.339885, 0.8616711, 0.3768303, 1743.908);
	// vs_cbuf8[2] = float4(-0.57711935, 0.5074672, -0.6398518, -3681.8398);
	// vs_cbuf8[3] = float4(0, 0, 0, 1.00);
	// vs_cbuf8[4] = float4(1.206285, 0, 0, 0);
	// vs_cbuf8[5] = float4(0, 2.144507, 0, 0);
	// vs_cbuf8[6] = float4(0, 0, -1.000008, -0.2000008);
	// vs_cbuf8[7] = float4(0, 0, -1, 0);
	// vs_cbuf8[29] = float4(-1919.2622, 365.7373, -3733.0469, 0);
	// vs_cbuf8[30] = float4(0.10, 25000.00, 2500.00, 24999.90);
	// vs_cbuf9[0] = float4(0, 0, 0, 0);
	// vs_cbuf9[1] = float4(17441310000000000000000000000.00, 1.42E-43, 0, 0);
	// vs_cbuf9[2] = float4(0, 0, 0, 0);
	// vs_cbuf9[3] = float4(0, 0, 0, 0);
	// vs_cbuf9[4] = float4(0, 0, 0, 0);
	// vs_cbuf9[5] = float4(0, 0, 0, 0);
	// vs_cbuf9[6] = float4(0, 0, 0, 0);
	// vs_cbuf9[7] = float4(1E-45, 0, 4.19717E-40, 0);
	// vs_cbuf9[8] = float4(0, 1E-45, 0, 1E-45);
	// vs_cbuf9[9] = float4(1E-45, 0, 0, 0);
	// vs_cbuf9[10] = float4(0, 0, 0, 0);
	// vs_cbuf9[11] = float4(0, 0, 0, 0);
	// vs_cbuf9[12] = float4(0, 0, 0, 0);
	// vs_cbuf9[13] = float4(0, 0, 0, 0);
	// vs_cbuf9[14] = float4(0, -1, 0, 0);
	// vs_cbuf9[15] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[16] = float4(0, 0, 0, 0);
	// vs_cbuf9[17] = float4(1.00, 1.00, 20.00, 20.00);
	// vs_cbuf9[18] = float4(0, 0, 0, 0);
	// vs_cbuf9[19] = float4(0, 0, 0, 0);
	// vs_cbuf9[20] = float4(2.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[21] = float4(0, 0, 0, 0);
	// vs_cbuf9[22] = float4(0, 0, 0, 0);
	// vs_cbuf9[23] = float4(0, 0, 0, 0);
	// vs_cbuf9[24] = float4(0, 0, 0, 0);
	// vs_cbuf9[25] = float4(0, 0, 0, 0);
	// vs_cbuf9[26] = float4(0, 0, 0, 0);
	// vs_cbuf9[27] = float4(0, 0, 0, 0);
	// vs_cbuf9[28] = float4(0, 0, 0, 0);
	// vs_cbuf9[29] = float4(2.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[30] = float4(0, 0, 0, 0);
	// vs_cbuf9[31] = float4(0, 0, 0, 0);
	// vs_cbuf9[32] = float4(0, 0, 0, 0);
	// vs_cbuf9[33] = float4(0, 0, 0, 0);
	// vs_cbuf9[34] = float4(0, 0, 0, 0);
	// vs_cbuf9[35] = float4(0, 0, 0, 0);
	// vs_cbuf9[36] = float4(0, 0, 0, 0);
	// vs_cbuf9[37] = float4(0, 0, 0, 0);
	// vs_cbuf9[38] = float4(2.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[39] = float4(0, 0, 0, 0);
	// vs_cbuf9[40] = float4(0, 0, 0, 0);
	// vs_cbuf9[41] = float4(0, 0, 0, 0);
	// vs_cbuf9[42] = float4(0, 0, 0, 0);
	// vs_cbuf9[43] = float4(0, 0, 0, 0);
	// vs_cbuf9[44] = float4(0, 0, 0, 0);
	// vs_cbuf9[45] = float4(0, 0, 0, 0);
	// vs_cbuf9[46] = float4(0, 0, 0, 0);
	// vs_cbuf9[47] = float4(4.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[48] = float4(0, 1E-45, 3E-45, 4E-45);
	// vs_cbuf9[49] = float4(6E-45, 7E-45, 8E-45, 1E-44);
	// vs_cbuf9[50] = float4(1.1E-44, 1.3E-44, 1.4E-44, 1.5E-44);
	// vs_cbuf9[51] = float4(1.7E-44, 1.8E-44, 2E-44, 2.1E-44);
	// vs_cbuf9[52] = float4(2.2E-44, 2.4E-44, 2.5E-44, 2.7E-44);
	// vs_cbuf9[53] = float4(2.8E-44, 3E-44, 3.1E-44, 3.2E-44);
	// vs_cbuf9[54] = float4(3.4E-44, 3.5E-44, 3.6E-44, 3.8E-44);
	// vs_cbuf9[55] = float4(3.9E-44, 4E-44, 4.2E-44, 4.3E-44);
	// vs_cbuf9[56] = float4(4.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[57] = float4(0, 1E-45, 3E-45, 4E-45);
	// vs_cbuf9[58] = float4(6E-45, 7E-45, 8E-45, 1E-44);
	// vs_cbuf9[59] = float4(1.1E-44, 1.3E-44, 1.4E-44, 1.5E-44);
	// vs_cbuf9[60] = float4(1.7E-44, 1.8E-44, 2E-44, 2.1E-44);
	// vs_cbuf9[61] = float4(2.2E-44, 2.4E-44, 2.5E-44, 2.7E-44);
	// vs_cbuf9[62] = float4(2.8E-44, 3E-44, 3.1E-44, 3.2E-44);
	// vs_cbuf9[63] = float4(3.4E-44, 3.5E-44, 3.6E-44, 3.8E-44);
	// vs_cbuf9[64] = float4(3.9E-44, 4E-44, 4.2E-44, 4.3E-44);
	// vs_cbuf9[65] = float4(4.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[66] = float4(0, 1E-45, 3E-45, 4E-45);
	// vs_cbuf9[67] = float4(6E-45, 7E-45, 8E-45, 1E-44);
	// vs_cbuf9[68] = float4(1.1E-44, 1.3E-44, 1.4E-44, 1.5E-44);
	// vs_cbuf9[69] = float4(1.7E-44, 1.8E-44, 2E-44, 2.1E-44);
	// vs_cbuf9[70] = float4(2.2E-44, 2.4E-44, 2.5E-44, 2.7E-44);
	// vs_cbuf9[71] = float4(2.8E-44, 3E-44, 3.1E-44, 3.2E-44);
	// vs_cbuf9[72] = float4(3.4E-44, 3.5E-44, 3.6E-44, 3.8E-44);
	// vs_cbuf9[73] = float4(3.9E-44, 4E-44, 4.2E-44, 4.3E-44);
	// vs_cbuf9[74] = float4(0, 0, 0, 0);
	// vs_cbuf9[75] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[76] = float4(2.00, 2.00, 0, 0);
	// vs_cbuf9[77] = float4(0.0008727, 0, 0, 0);
	// vs_cbuf9[78] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[79] = float4(0, 0, 0, 0);
	// vs_cbuf9[80] = float4(0, 0, 0, 0);
	// vs_cbuf9[81] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[82] = float4(0, 0, 0, 0);
	// vs_cbuf9[83] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[84] = float4(0, 0, 0, 0);
	// vs_cbuf9[85] = float4(0, 0, 0, 0);
	// vs_cbuf9[86] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[87] = float4(0, 0, 0, 0);
	// vs_cbuf9[88] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[89] = float4(0, 0, 0, 0);
	// vs_cbuf9[90] = float4(0, 0, 0, 0);
	// vs_cbuf9[91] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[92] = float4(0, 0, 0, 0);
	// vs_cbuf9[93] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[94] = float4(0, 0, 0, 0);
	// vs_cbuf9[95] = float4(0, 0, 0, 0);
	// vs_cbuf9[96] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[97] = float4(0, 0, 0, 0);
	// vs_cbuf9[98] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[99] = float4(0, 0, 0, 0);
	// vs_cbuf9[100] = float4(0, 0, 0, 0);
	// vs_cbuf9[101] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[102] = float4(0, 0, 0, 0);
	// vs_cbuf9[103] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[104] = float4(0.30, 0, 0, 0);
	// vs_cbuf9[105] = float4(0.8475056, 0.9126984, 0.9111589, 0);
	// vs_cbuf9[106] = float4(0, 0, 0, 0);
	// vs_cbuf9[107] = float4(0, 0, 0, 0);
	// vs_cbuf9[108] = float4(0, 0, 0, 0);
	// vs_cbuf9[109] = float4(0, 0, 0, 0);
	// vs_cbuf9[110] = float4(0, 0, 0, 0);
	// vs_cbuf9[111] = float4(0, 0, 0, 0);
	// vs_cbuf9[112] = float4(0, 0, 0, 0);
	// vs_cbuf9[113] = float4(1.50, 1.00, 1.00, 0);
	// vs_cbuf9[114] = float4(1.50, 1.00, 1.00, 1.00);
	// vs_cbuf9[115] = float4(1.50, 1.00, 1.00, 2.00);
	// vs_cbuf9[116] = float4(1.50, 1.00, 1.00, 3.00);
	// vs_cbuf9[117] = float4(1.50, 1.00, 1.00, 4.00);
	// vs_cbuf9[118] = float4(1.50, 1.00, 1.00, 5.00);
	// vs_cbuf9[119] = float4(1.50, 1.00, 1.00, 6.00);
	// vs_cbuf9[120] = float4(1.50, 1.00, 1.00, 7.00);
	// vs_cbuf9[121] = float4(0.4303351, 0.4726913, 0.484127, 0);
	// vs_cbuf9[122] = float4(0, 0, 0, 0);
	// vs_cbuf9[123] = float4(0, 0, 0, 0);
	// vs_cbuf9[124] = float4(0, 0, 0, 0);
	// vs_cbuf9[125] = float4(0, 0, 0, 0);
	// vs_cbuf9[126] = float4(0, 0, 0, 0);
	// vs_cbuf9[127] = float4(0, 0, 0, 0);
	// vs_cbuf9[128] = float4(0, 0, 0, 0);
	// vs_cbuf9[129] = float4(1.00, 1.00, 1.00, 0);
	// vs_cbuf9[130] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[131] = float4(1.00, 1.00, 1.00, 2.00);
	// vs_cbuf9[132] = float4(1.00, 1.00, 1.00, 3.00);
	// vs_cbuf9[133] = float4(1.00, 1.00, 1.00, 4.00);
	// vs_cbuf9[134] = float4(1.00, 1.00, 1.00, 5.00);
	// vs_cbuf9[135] = float4(1.00, 1.00, 1.00, 6.00);
	// vs_cbuf9[136] = float4(1.00, 1.00, 1.00, 7.00);
	// vs_cbuf9[137] = float4(0, 0.50, 1.00, 0);
	// vs_cbuf9[138] = float4(10.00, 30.00, 80.00, 100.00);
	// vs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[140] = float4(0, 100.00, 0, 0);
	// vs_cbuf9[141] = float4(1.00, 1.00, 1.00, 0);
	// vs_cbuf10[0] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[1] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[2] = float4(999.50, 1.00, 1.00, 1.00);
	// vs_cbuf10[3] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[4] = float4(1.00, 0, 0, -1134.2701);
	// vs_cbuf10[5] = float4(0, 1.00, 0, 35.58958);
	// vs_cbuf10[6] = float4(0, 0, 1.00, -3936.7583);
	// vs_cbuf10[7] = float4(0, 0, 0, 1.00);
	// vs_cbuf10[8] = float4(1.00, 0, 0, -1134.2701);
	// vs_cbuf10[9] = float4(0, 1.00, 0, 35.58958);
	// vs_cbuf10[10] = float4(0, 0, 1.00, -3936.7583);
	// vs_cbuf13[0] = float4(0, 1.00, 1.00, 1.00);
	// vs_cbuf15[22] = float4(0.0000333, -0.0016638935, 0, 0);
	// vs_cbuf15[23] = float4(20.00, 1.00, 0.85, -0.010725529);
	// vs_cbuf15[24] = float4(0.002381, -0.04761905, 3.363175, 4.00);
	// vs_cbuf15[25] = float4(0.0282744, 0.0931012, 0.1164359, 0.7006614);
	// vs_cbuf15[26] = float4(0.0174636, 0.1221582, 0.2193998, 0.20);
	// vs_cbuf15[27] = float4(-0.14285715, 0.0071429, 250.00, 0);
	// vs_cbuf15[28] = float4(0.8802994, -0.4663191, -0.08728968, 0);
	// vs_cbuf15[49] = float4(0, 0, 0, 0);
	// vs_cbuf15[51] = float4(950.00, 50.00, 1.50, 1.00);
	// vs_cbuf15[52] = float4(-2116, -3932, 0.0025, 0);
	// vs_cbuf15[58] = float4(1.00, 1.00, 1.00, 1.00);

	bool b_0_8;
	bool b_1_22;
	bool b_1_24;
	bool b_2_11;
	float f_0_24;
	float f_0_28;
	float f_0_31;
	float f_0_32;
	float f_0_34;
	float f_0_35;
	float f_1_35;
	float f_10_9;
	float f_11_9;
	float f_13_6;
	float f_13_7;
	float f_14_4;
	float f_16_6;
	float f_2_73;
	float f_3_15;
	float f_5_3;
	float f_7_20;
	float f_8_6;
	float4 f4_0_0;
	float4 f4_0_1;
	precise float pf_0_1;
	precise float pf_0_9;
	precise float pf_1_1;
	precise float pf_1_10;
	precise float pf_1_6;
	precise float pf_10_5;
	precise float pf_11_3;
	precise float pf_12_3;
	precise float pf_12_8;
	precise float pf_12_9;
	precise float pf_13_2;
	precise float pf_16_0;
	precise float pf_17_0;
	precise float pf_17_1;
	precise float pf_2_11;
	precise float pf_2_12;
	precise float pf_2_13;
	precise float pf_2_14;
	precise float pf_2_4;
	precise float pf_2_6;
	precise float pf_3_16;
	precise float pf_3_18;
	precise float pf_3_19;
	precise float pf_3_4;
	precise float pf_4_13;
	precise float pf_4_17;
	precise float pf_4_21;
	precise float pf_4_3;
	precise float pf_6_1;
	precise float pf_6_5;
	precise float pf_6_7;
	precise float pf_6_8;
	precise float pf_7_1;
	precise float pf_7_2;
	precise float pf_8_12;
	precise float pf_8_6;
	precise float pf_8_8;
	precise float pf_9_6;
	uint u_0_1;
	uint u_0_2;
	uint u_0_3;
	uint u_0_4;
	uint u_0_7;
	uint u_0_8;
	uint u_0_phi_19;
	uint u_0_phi_21;
	uint u_1_1;
	uint u_1_2;
	uint u_1_5;
	uint u_1_7;
	uint u_1_phi_18;
	uint u_11_2;
	uint u_11_3;
	uint u_11_6;
	uint u_11_8;
	uint u_11_9;
	uint u_11_phi_25;
	uint u_12_1;
	uint u_12_11;
	uint u_12_12;
	uint u_12_17;
	uint u_12_3;
	uint u_12_4;
	uint u_12_phi_26;
	uint u_13_10;
	uint u_13_6;
	uint u_13_7;
	uint u_13_9;
	uint u_13_phi_30;
	uint u_14_6;
	uint u_14_7;
	uint u_14_phi_29;
	uint u_15_3;
	uint u_15_5;
	uint u_15_8;
	uint u_15_9;
	uint u_15_phi_27;
	uint u_16_0;
	uint u_16_8;
	uint u_17_0;
	uint u_19_4;
	uint u_2_1;
	uint u_2_2;
	uint u_2_4;
	uint u_2_5;
	uint u_2_phi_11;
	uint u_2_phi_4;
	uint u_20_1;
	uint u_20_2;
	uint u_20_3;
	uint u_20_4;
	uint u_20_phi_23;
	uint u_20_phi_24;
	uint u_3_3;
	uint u_3_4;
	uint u_3_6;
	uint u_3_7;
	uint u_3_phi_16;
	uint u_3_phi_20;
	uint u_4_0;
	uint u_4_1;
	uint u_4_2;
	uint u_4_3;
	uint u_4_5;
	uint u_4_6;
	uint u_4_8;
	uint u_4_phi_2;
	uint u_4_phi_20;
	uint u_4_phi_9;
	uint u_5_4;
	uint u_5_5;
	uint u_5_6;
	uint u_5_7;
	uint u_5_8;
	uint u_5_phi_15;
	uint u_5_phi_17;
	uint u_6_4;
	uint u_6_5;
	uint u_6_7;
	uint u_6_phi_20;
	uint u_7_3;
	uint u_7_8;
	uint u_8_10;
	uint u_8_15;
	uint u_8_17;
	uint u_8_21;
	uint u_9_14;
	uint u_9_20;
	uint u_9_21;
	uint u_9_7;
	uint u_9_9;
	// -867.7429  <=>  float(-867.74292)
	o.vertex.x = float(-867.74292);
	// -201.18993  <=>  float(-201.18993)
	o.vertex.y = float(-201.18993);
	// 489.6669  <=>  float(489.66693)
	o.vertex.z = float(489.66693);
	// 489.863  <=>  float(489.86304)
	o.vertex.w = float(489.86304);
	// 0.25425  <=>  float(0.25425)
	o.fs_attr0.x = float(0.25425);
	// 0.27381  <=>  float(0.27381)
	o.fs_attr0.y = float(0.27381);
	// 0.27335  <=>  float(0.27335)
	o.fs_attr0.z = float(0.27335);
	// 1.50  <=>  float(1.50)
	o.fs_attr0.w = float(1.50);
	// 0.1291  <=>  float(0.1291)
	o.fs_attr1.x = float(0.1291);
	// 0.14181  <=>  float(0.14181)
	o.fs_attr1.y = float(0.14181);
	// 0.14524  <=>  float(0.14524)
	o.fs_attr1.z = float(0.14524);
	// 1.00  <=>  float(1.00)
	o.fs_attr1.w = float(1.00);
	// 0.60233  <=>  float(0.60233)
	o.fs_attr2.x = float(0.60233);
	// 1.42053  <=>  float(1.42053)
	o.fs_attr2.y = float(1.42053);
	// 0.50196  <=>  float(0.50196)
	o.fs_attr2.z = float(0.50196);
	// 0.50196  <=>  float(0.50196)
	o.fs_attr2.w = float(0.50196);
	// -188.93994  <=>  float(-188.93994)
	o.fs_attr3.x = float(-188.93994);
	// 345.5265  <=>  float(345.52649)
	o.fs_attr3.y = float(345.52649);
	// 489.765  <=>  float(489.76498)
	o.fs_attr3.z = float(489.76498);
	// 489.863  <=>  float(489.86304)
	o.fs_attr3.w = float(489.86304);
	// 1.00  <=>  float(1.00)
	o.fs_attr4.x = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr4.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr4.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr4.w = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.x = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.y = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.z = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.w = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr6.x = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr6.y = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr6.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr6.w = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.x = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr7.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr7.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.w = float(1.00);
	// 0.70066  <=>  float(0.70066)
	o.fs_attr8.x = float(0.70066);
	// 0.20  <=>  float(0.20)
	o.fs_attr8.y = float(0.20);
	// 0  <=>  float(0.00)
	o.fs_attr8.z = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr8.w = float(0.00);
	// 0.10241  <=>  float(0.10241)
	o.fs_attr9.x = float(0.10241);
	// 0.10241  <=>  float(0.10241)
	o.fs_attr9.y = float(0.10241);
	// 0.10241  <=>  float(0.10241)
	o.fs_attr9.z = float(0.10241);
	// 1.00  <=>  float(1.00)
	o.fs_attr9.w = float(1.00);
	// 0.00111  <=>  float(0.00111)
	o.fs_attr10.x = float(0.00111);
	// 0.00877  <=>  float(0.00877)
	o.fs_attr10.y = float(0.00877);
	// 0.00901  <=>  float(0.00901)
	o.fs_attr10.z = float(0.00901);
	// 0.2585  <=>  float(0.2585)
	o.fs_attr10.w = float(0.2585);
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_4_0 = 0u;
	u_4_phi_2 = u_4_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_4_1 = ftou(vs_cbuf8_30.y);
		u_4_phi_2 = u_4_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_4_phi_2 : 0}
	u_2_1 = u_4_phi_2;
	u_2_phi_4 = u_2_1;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_4_phi_2) : 0} * 5.f)) : 0}
		u_2_2 = ftou((utof(u_4_phi_2) * 5.f));
		u_2_phi_4 = u_2_2;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr4.x = 0.f;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {utof(u_2_phi_4) : 0}
		o.vertex.z = utof(u_2_phi_4);
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		return;
	}
	// 999.50  <=>  ((0.f - {i.vao_attr5.w : 0}) + {(vs_cbuf10_2.x) : 999.50})
	pf_0_1 = ((0.f - i.vao_attr5.w) + (vs_cbuf10_2.x));
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  {ftou(i.vao_attr5.w) : 0}
	u_4_2 = ftou(i.vao_attr5.w);
	u_4_phi_9 = u_4_2;
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_4_3 = ftou(vs_cbuf8_30.y);
		u_4_phi_9 = u_4_3;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_4_phi_9 : 0}
	u_2_4 = u_4_phi_9;
	u_2_phi_11 = u_2_4;
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_4_phi_9) : 0} * 5.f)) : 0}
		u_2_5 = ftou((utof(u_4_phi_9) * 5.f));
		u_2_phi_11 = u_2_5;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr4.x = 0.f;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  {utof(u_2_phi_11) : 0}
		o.vertex.z = utof(u_2_phi_11);
	}
	// False  <=>  if(((((({i.vao_attr5.w : 0} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 0}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 999.50} >= float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 999.50}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		return;
	}
	// 1000.50  <=>  ({pf_0_1 : 999.50} + {(vs_cbuf10_2.w) : 1.00})
	pf_1_1 = (pf_0_1 + (vs_cbuf10_2.w));
	// 1300234212  <=>  {ftou(float(int((myIsNaN({i.vao_attr4.w : 268435000.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 268435000.00}), float(-2147483600.f), float(2147483600.f))))))) : 1300234212}
	u_5_4 = ftou(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))));
	u_5_phi_15 = u_5_4;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1232364164  <=>  {ftou(({pf_1_1 : 1000.50} * {pf_1_1 : 1000.50})) : 1232364164}
		u_5_5 = ftou((pf_1_1 * pf_1_1));
		u_5_phi_15 = u_5_5;
	}
	// 1232364164  <=>  {u_5_phi_15 : 1232364164}
	u_3_3 = u_5_phi_15;
	u_3_phi_16 = u_3_3;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_5_phi_15) : 1001000.00} * {utof(vs_cbuf9[14].w) : 0})) : 0}
		u_3_4 = ftou((utof(u_5_phi_15) * utof(vs_cbuf9[14].w)));
		u_3_phi_16 = u_3_4;
	}
	// 0  <=>  0u
	u_5_6 = 0u;
	u_5_phi_17 = u_5_6;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 2147483648  <=>  {ftou((({utof(u_3_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].y) : -1})) : 2147483648}
		u_5_7 = ftou(((utof(u_3_phi_16) * 0.5f) * utof(vs_cbuf9[14].y)));
		u_5_phi_17 = u_5_7;
	}
	// 0  <=>  0u
	u_1_1 = 0u;
	u_1_phi_18 = u_1_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_3_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].z) : 0})) : 0}
		u_1_2 = ftou(((utof(u_3_phi_16) * 0.5f) * utof(vs_cbuf9[14].z)));
		u_1_phi_18 = u_1_2;
	}
	// 1144258560  <=>  {ftou(i.vao_attr6.x) : 1144258560}
	u_0_1 = ftou(i.vao_attr6.x);
	u_0_phi_19 = u_0_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_3_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].x) : 0})) : 0}
		u_0_2 = ftou(((utof(u_3_phi_16) * 0.5f) * utof(vs_cbuf9[14].x)));
		u_0_phi_19 = u_0_2;
	}
	// 0  <=>  {u_1_phi_18 : 0}
	u_3_6 = u_1_phi_18;
	// 2147483648  <=>  {u_5_phi_17 : 2147483648}
	u_4_5 = u_5_phi_17;
	// 0  <=>  {u_0_phi_19 : 0}
	u_6_4 = u_0_phi_19;
	u_3_phi_20 = u_3_6;
	u_4_phi_20 = u_4_5;
	u_6_phi_20 = u_6_4;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 1000.50}))
		f_13_6 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// ∞  <=>  ((1.0f / log2({utof(vs_cbuf9[15].x) : 1.00})) * 1.442695f)
		pf_4_3 = ((1.0f / log2(utof(vs_cbuf9[15].x))) * 1.442695f);
		// 4290772992  <=>  {ftou((((((0.f - (({pf_4_3 : ∞} * {f_13_6 : 1.00}) + (0.f - {pf_4_3 : ∞}))) + {pf_1_1 : 1000.50}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].z) : 0})) : 4290772992}
		u_3_7 = ftou((((((0.f - ((pf_4_3 * f_13_6) + (0.f - pf_4_3))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].z)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_4_3 : ∞} * {f_13_6 : 1.00}) + (0.f - {pf_4_3 : ∞}))) + {pf_1_1 : 1000.50}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].y) : -1})) : 4290772992}
		u_4_6 = ftou((((((0.f - ((pf_4_3 * f_13_6) + (0.f - pf_4_3))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].y)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_4_3 : ∞} * {f_13_6 : 1.00}) + (0.f - {pf_4_3 : ∞}))) + {pf_1_1 : 1000.50}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].x) : 0})) : 4290772992}
		u_6_5 = ftou((((((0.f - ((pf_4_3 * f_13_6) + (0.f - pf_4_3))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].x)));
		u_3_phi_20 = u_3_7;
		u_4_phi_20 = u_4_6;
		u_6_phi_20 = u_6_5;
	}
	// 1148854272  <=>  {ftou(pf_1_1) : 1148854272}
	u_0_3 = ftou(pf_1_1);
	u_0_phi_21 = u_0_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// ∞  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))
		f_13_7 = (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f));
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 1000.50}))
		f_7_20 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// 4290772992  <=>  {ftou((({f_7_20 : 1.00} * (0.f - {f_13_7 : ∞})) + {f_13_7 : ∞})) : 4290772992}
		u_0_4 = ftou(((f_7_20 * (0.f - f_13_7)) + f_13_7));
		u_0_phi_21 = u_0_4;
	}
	// 0  <=>  (((({utof(u_0_phi_21) : 1000.50} * {i.vao_attr5.x : 0}) + {utof(u_6_phi_20) : 0}) * {i.vao_attr6.w : 1.00}) + {i.vao_attr4.x : 0})
	pf_3_4 = ((((utof(u_0_phi_21) * i.vao_attr5.x) + utof(u_6_phi_20)) * i.vao_attr6.w) + i.vao_attr4.x);
	// 0.72  <=>  ((({i.vao_attr6.z : 720.00} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.001}))
	pf_2_4 = (((i.vao_attr6.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z));
	// 0  <=>  (((({utof(u_0_phi_21) : 1000.50} * {i.vao_attr5.y : 0}) + {utof(u_4_phi_20) : -0}) * {i.vao_attr6.w : 1.00}) + {i.vao_attr4.y : 0})
	pf_6_1 = ((((utof(u_0_phi_21) * i.vao_attr5.y) + utof(u_4_phi_20)) * i.vao_attr6.w) + i.vao_attr4.y);
	// 0  <=>  ((((clamp(min(0.f, {i.vao_attr7.x : 0.5484}), 0.0, 1.0) + {i.vao_attr6.x : 720.00}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : 0}))
	pf_1_6 = ((((clamp(min(0.f, i.vao_attr7.x), 0.0, 1.0) + i.vao_attr6.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x));
	// 0  <=>  (((({utof(u_0_phi_21) : 1000.50} * {i.vao_attr5.z : 0}) + {utof(u_3_phi_20) : 0}) * {i.vao_attr6.w : 1.00}) + {i.vao_attr4.z : 0})
	pf_7_1 = ((((utof(u_0_phi_21) * i.vao_attr5.z) + utof(u_3_phi_20)) * i.vao_attr6.w) + i.vao_attr4.z);
	// 0  <=>  ((({i.vao_attr6.y : 720.00} * {utof(vs_cbuf9[141].y) : 1.00}) * {(vs_cbuf10_3.z) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].y) : 0}) + {v.vertex.y : 0}))
	pf_4_13 = (((i.vao_attr6.y * utof(vs_cbuf9[141].y)) * (vs_cbuf10_3.z)) * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y));
	// -1134.2701  <=>  (((({pf_7_1 : 0} * {(vs_cbuf10_4.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_4.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_4.x) : 1.00}))) + {(vs_cbuf10_4.w) : -1134.2701}) + (({pf_4_13 : 0} * (0.f - {(vs_cbuf10_8.z) : 0})) + (({pf_2_4 : 0.72} * {(vs_cbuf10_8.y) : 0}) + ({pf_1_6 : 0} * {(vs_cbuf10_8.x) : 1.00}))))
	pf_7_2 = ((((pf_7_1 * (vs_cbuf10_4.z)) + ((pf_6_1 * (vs_cbuf10_4.y)) + (pf_3_4 * (vs_cbuf10_4.x)))) + (vs_cbuf10_4.w)) + ((pf_4_13 * (0.f - (vs_cbuf10_8.z))) + ((pf_2_4 * (vs_cbuf10_8.y)) + (pf_1_6 * (vs_cbuf10_8.x)))));
	// 36.30958  <=>  (((({pf_7_1 : 0} * {(vs_cbuf10_5.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_5.y) : 1.00}) + ({pf_3_4 : 0} * {(vs_cbuf10_5.x) : 0}))) + {(vs_cbuf10_5.w) : 35.58958}) + (({pf_4_13 : 0} * (0.f - {(vs_cbuf10_9.z) : 0})) + (({pf_2_4 : 0.72} * {(vs_cbuf10_9.y) : 1.00}) + ({pf_1_6 : 0} * {(vs_cbuf10_9.x) : 0}))))
	pf_2_6 = ((((pf_7_1 * (vs_cbuf10_5.z)) + ((pf_6_1 * (vs_cbuf10_5.y)) + (pf_3_4 * (vs_cbuf10_5.x)))) + (vs_cbuf10_5.w)) + ((pf_4_13 * (0.f - (vs_cbuf10_9.z))) + ((pf_2_4 * (vs_cbuf10_9.y)) + (pf_1_6 * (vs_cbuf10_9.x)))));
	// -3936.7583  <=>  (((({pf_7_1 : 0} * {(vs_cbuf10_6.z) : 1.00}) + (({pf_6_1 : 0} * {(vs_cbuf10_6.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_6.x) : 0}))) + {(vs_cbuf10_6.w) : -3936.7583}) + (({pf_4_13 : 0} * (0.f - {(vs_cbuf10_10.z) : 1.00})) + (({pf_2_4 : 0.72} * {(vs_cbuf10_10.y) : 0}) + ({pf_1_6 : 0} * {(vs_cbuf10_10.x) : 0}))))
	pf_1_10 = ((((pf_7_1 * (vs_cbuf10_6.z)) + ((pf_6_1 * (vs_cbuf10_6.y)) + (pf_3_4 * (vs_cbuf10_6.x)))) + (vs_cbuf10_6.w)) + ((pf_4_13 * (0.f - (vs_cbuf10_10.z))) + ((pf_2_4 * (vs_cbuf10_10.y)) + (pf_1_6 * (vs_cbuf10_10.x)))));
	// -719.3513  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[0].z) : 0.6697676}) + (({pf_2_6 : 36.30958} * {(view_proj[0].y) : 1.493044E-08}) + ({pf_7_2 : -1134.2701} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
	pf_4_17 = (((pf_1_10 * (view_proj[0].z)) + ((pf_2_6 * (view_proj[0].y)) + (pf_7_2 * (view_proj[0].x)))) + (view_proj[0].w));
	// -93.81641  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[1].z) : 0.3768303}) + (({pf_2_6 : 36.30958} * {(view_proj[1].y) : 0.8616711}) + ({pf_7_2 : -1134.2701} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
	pf_8_6 = (((pf_1_10 * (view_proj[1].z)) + ((pf_2_6 * (view_proj[1].y)) + (pf_7_2 * (view_proj[1].x)))) + (view_proj[1].w));
	// -489.8628  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[2].z) : -0.6398518}) + (({pf_2_6 : 36.30958} * {(view_proj[2].y) : 0.5074672}) + ({pf_7_2 : -1134.2701} * {(view_proj[2].x) : -0.57711935}))) + {(view_proj[2].w) : -3681.8398})
	pf_9_6 = (((pf_1_10 * (view_proj[2].z)) + ((pf_2_6 * (view_proj[2].y)) + (pf_7_2 * (view_proj[2].x)))) + (view_proj[2].w));
	// 1.00  <=>  ((({pf_1_10 : -3936.7583} * {(view_proj[3].z) : 0}) + (({pf_2_6 : 36.30958} * {(view_proj[3].y) : 0}) + ({pf_7_2 : -1134.2701} * {(view_proj[3].x) : 0}))) + {(view_proj[3].w) : 1.00})
	pf_10_5 = (((pf_1_10 * (view_proj[3].z)) + ((pf_2_6 * (view_proj[3].y)) + (pf_7_2 * (view_proj[3].x)))) + (view_proj[3].w));
	// -201.18993  <=>  (({pf_10_5 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[5].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[5].y) : 2.144507}) + ({pf_4_17 : -719.3513} * {(view_proj[5].x) : 0}))))
	pf_8_8 = ((pf_10_5 * (view_proj[5].w)) + ((pf_9_6 * (view_proj[5].z)) + ((pf_8_6 * (view_proj[5].y)) + (pf_4_17 * (view_proj[5].x)))));
	// 489.6667  <=>  (({pf_10_5 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_9_6 : -489.8628} * {(view_proj[6].z) : -1.000008}) + (({pf_8_6 : -93.81641} * {(view_proj[6].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[6].x) : 0}))))
	pf_11_3 = ((pf_10_5 * (view_proj[6].w)) + ((pf_9_6 * (view_proj[6].z)) + ((pf_8_6 * (view_proj[6].y)) + (pf_4_17 * (view_proj[6].x)))));
	// 489.8628  <=>  (({pf_10_5 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[7].z) : -1}) + (({pf_8_6 : -93.81641} * {(view_proj[7].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[7].x) : 0}))))
	pf_4_21 = ((pf_10_5 * (view_proj[7].w)) + ((pf_9_6 * (view_proj[7].z)) + ((pf_8_6 * (view_proj[7].y)) + (pf_4_17 * (view_proj[7].x)))));
	// -784.99207  <=>  ((0.f - {pf_7_2 : -1134.2701}) + {(camera_wpos.x) : -1919.2622})
	pf_12_3 = ((0.f - pf_7_2) + (camera_wpos.x));
	// -0  <=>  ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + (0.f * {pf_8_8 : -201.18993}))
	pf_13_2 = ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (0.f * pf_8_8));
	// 203.7114  <=>  ((0.f - {pf_1_10 : -3936.7583}) + {(camera_wpos.z) : -3733.0469})
	pf_16_0 = ((0.f - pf_1_10) + (camera_wpos.z));
	// 329.4277  <=>  ((0.f - {pf_2_6 : 36.30958}) + {(camera_wpos.y) : 365.7373})
	pf_17_0 = ((0.f - pf_2_6) + (camera_wpos.y));
	// 0.0011424  <=>  inversesqrt((({pf_16_0 : 203.7114} * {pf_16_0 : 203.7114}) + (({pf_17_0 : 329.4277} * {pf_17_0 : 329.4277}) + ({pf_12_3 : -784.99207} * {pf_12_3 : -784.99207}))))
	f_3_15 = inversesqrt(((pf_16_0 * pf_16_0) + ((pf_17_0 * pf_17_0) + (pf_12_3 * pf_12_3))));
	// 489.7266  <=>  ((1.0f / ((((({pf_4_21 : 489.8628} * 0.5f) + (({pf_11_3 : 489.6667} * 0.5f) + {pf_13_2 : -0})) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00}))
	pf_17_1 = ((1.0f / (((((pf_4_21 * 0.5f) + ((pf_11_3 * 0.5f) + pf_13_2)) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z)));
	// 0.0073795  <=>  ((((({pf_16_0 : 203.7114} * {f_3_15 : 0.0011424}) * {(lightDir.z) : -0.08728968}) + ((({pf_17_0 : 329.4277} * {f_3_15 : 0.0011424}) * {(lightDir.y) : -0.4663191}) + (({pf_12_3 : -784.99207} * {f_3_15 : 0.0011424}) * {(lightDir.x) : 0.8802994}))) * 0.5f) + 0.5f)
	pf_12_8 = (((((pf_16_0 * f_3_15) * (lightDir.z)) + (((pf_17_0 * f_3_15) * (lightDir.y)) + ((pf_12_3 * f_3_15) * (lightDir.x)))) * 0.5f) + 0.5f);
	// 1.569167  <=>  (({pf_12_8 : 0.0073795} * (({pf_12_8 : 0.0073795} * (({pf_12_8 : 0.0073795} * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
	pf_12_9 = ((pf_12_8 * ((pf_12_8 * ((pf_12_8 * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f);
	// 0.9820282  <=>  exp2((log2(((0.f - clamp((({pf_17_1 : 489.7266} * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.y) : 1.00}))
	f_5_3 = exp2((log2(((0.f - clamp(((pf_17_1 * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.y)));
	// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, float2(((({pf_12_9 : 1.569167} * (0.f - sqrt(((0.f - {pf_12_8 : 0.0073795}) + 1.f)))) * 0.63661975f) + 1.f), (({f_5_3 : 0.9820282} * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
	f4_0_0 = textureLod(tex0, float2((((pf_12_9 * (0.f - sqrt(((0.f - pf_12_8) + 1.f)))) * 0.63661975f) + 1.f), ((f_5_3 * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler);
	// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex1 : tex1}, float2((({pf_7_2 : -1134.2701} + (0.f - {(vs_cbuf15_52.x) : -2116})) * {(vs_cbuf15_52.z) : 0.0025}), (({pf_1_10 : -3936.7583} + (0.f - {(vs_cbuf15_52.y) : -3932})) * {(vs_cbuf15_52.z) : 0.0025})), 0.0, s_linear_clamp_sampler)
	f4_0_1 = textureLod(tex1, float2(((pf_7_2 + (0.f - (vs_cbuf15_52.x))) * (vs_cbuf15_52.z)), ((pf_1_10 + (0.f - (vs_cbuf15_52.y))) * (vs_cbuf15_52.z))), 0.0, s_linear_clamp_sampler);
	// 128  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 128u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_1_5 = ((ftou(vs_cbuf0_21.x) + 128u) - ftou(vs_cbuf0_21.x));
	// 112  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 112u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_0_7 = ((ftou(vs_cbuf0_21.x) + 112u) - ftou(vs_cbuf0_21.x));
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x : }
	u_0_8 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y : }
	u_4_8 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z : }
	u_5_8 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z;
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_6_7 = (myIsNaN(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_6_7 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_7_3 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))))), int(0u), int(32u)));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_1_22 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_6_7), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 1.00  <=>  1.f
	o.fs_attr7.x = 1.f;
	// 1.00  <=>  (1.0f / float({u_7_3 : 1}))
	f_14_4 = (1.0f / float(u_7_3));
	// 1065353214  <=>  ({ftou(f_14_4) : 1065353216} + 4294967294u)
	u_12_1 = (ftou(f_14_4) + 4294967294u);
	// 1.00  <=>  {v.coeff.x : 1.00}
	o.fs_attr5.x = v.coeff.x;
	// -201.18993  <=>  {pf_8_8 : -201.18993}
	o.vertex.y = pf_8_8;
	// 489.7647  <=>  (({pf_4_21 : 489.8628} * 0.5f) + (({pf_11_3 : 489.6667} * 0.5f) + {pf_13_2 : -0}))
	o.fs_attr3.z = ((pf_4_21 * 0.5f) + ((pf_11_3 * 0.5f) + pf_13_2));
	// -867.7427  <=>  (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))
	o.vertex.x = ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))));
	// 0  <=>  clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_6_7 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f))
	f_8_6 = clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_6_7))) + 4294967294u)))), float(0.f), float(4294967300.f));
	// 0  <=>  uint({f_8_6 : 0})
	u_16_0 = uint(f_8_6);
	// 0  <=>  uint(clamp(trunc(({utof(u_12_1) : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
	u_17_0 = uint(clamp(trunc((utof(u_12_1) * float(0u))), float(0.f), float(4294967300.f)));
	// 489.6667  <=>  {pf_11_3 : 489.6667}
	o.vertex.z = pf_11_3;
	// 489.8628  <=>  {pf_4_21 : 489.8628}
	o.vertex.w = pf_4_21;
	// 489.8628  <=>  ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0}))
	o.fs_attr3.w = (pf_4_21 + ((0.f * pf_11_3) + pf_13_2));
	// 0  <=>  0.f
	o.fs_attr8.w = 0.f;
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_0 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_9_7 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_0 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_0 : 0}), int(0u), int(16u))))))
	u_9_9 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_0), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_15_3 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))))))
	u_15_5 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_17_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(16u), int(16u))) * {u_9_7 : 0})) << 16u) + {u_9_9 : 0}))))
	u_8_10 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u))) * u_9_7)) << 16u) + u_9_9))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_17_0 : 0}), int(16u), int(16u))) * {u_15_3 : 1})) << 16u) + {u_15_5 : 0}))))
	u_9_14 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_17_0), int(16u), int(16u))) * u_15_3)) << 16u) + u_15_5))));
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.z : 0.58835} > 0.5f) && (! myIsNaN({i.vao_attr7.z : 0.58835}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.z > 0.5f) && (! myIsNaN(i.vao_attr7.z))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 1056997491  <=>  {ftou(v.uv.y) : 1056997491}
	u_20_1 = ftou(v.uv.y);
	u_20_phi_23 = u_20_1;
	// False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 0}) == 8u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.w : 0.42892} > 0.5f) && (! myIsNaN({i.vao_attr7.w : 0.42892}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.w > 0.5f) && (! myIsNaN(i.vao_attr7.w))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.y : 0.50196}) + 1.f)) : 1056898842}
		u_20_2 = ftou(((0.f - v.uv.y) + 1.f));
		u_20_phi_23 = u_20_2;
	}
	// 1056997491  <=>  {ftou(v.uv.x) : 1056997491}
	u_20_3 = ftou(v.uv.x);
	u_20_phi_24 = u_20_3;
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.z : 0.58835} > 0.5f) && (! myIsNaN({i.vao_attr7.z : 0.58835}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.z > 0.5f) && (! myIsNaN(i.vao_attr7.z))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.x : 0.50196}) + 1.f)) : 1056898842}
		u_20_4 = ftou(((0.f - v.uv.x) + 1.f));
		u_20_phi_24 = u_20_4;
	}
	// 0  <=>  clamp(trunc(({utof(({ftou((1.0f / float({u_6_7 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float({u_8_10 : 0}))), float(0.f), float(4294967300.f))
	f_16_6 = clamp(trunc((utof((ftou((1.0f / float(u_6_7))) + 4294967294u)) * float(u_8_10))), float(0.f), float(4294967300.f));
	// 0.50196  <=>  (({utof(u_20_phi_23) : 0.50196} * {utof(vs_cbuf9[83].y) : 1.00}) * (1.0f / {utof(vs_cbuf9[83].w) : 1.00}))
	o.fs_attr2.w = ((utof(u_20_phi_23) * utof(vs_cbuf9[83].y)) * (1.0f / utof(vs_cbuf9[83].w)));
	// 0  <=>  {ftou(v.offset.x) : 0}
	u_11_2 = ftou(v.offset.x);
	u_11_phi_25 = u_11_2;
	// True  <=>  if(((! (((({v.vertex.z : 0.001} == 0.f) && (! myIsNaN({v.vertex.z : 0.001}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : 0} == 0.f) && (! myIsNaN({v.vertex.x : 0}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : 0} == 0.f) && (! myIsNaN({v.vertex.y : 0}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.x : 0} * {(vs_cbuf13_0.x) : 0}) + {v.offset.x : 0})) : 0}
		u_11_3 = ftou(((v.vertex.x * (vs_cbuf13_0.x)) + v.offset.x));
		u_11_phi_25 = u_11_3;
	}
	// 1065353216  <=>  {ftou(v.offset.z) : 1065353216}
	u_12_3 = ftou(v.offset.z);
	u_12_phi_26 = u_12_3;
	// True  <=>  if(((! (((({v.vertex.z : 0.001} == 0.f) && (! myIsNaN({v.vertex.z : 0.001}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : 0} == 0.f) && (! myIsNaN({v.vertex.x : 0}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : 0} == 0.f) && (! myIsNaN({v.vertex.y : 0}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1065353216  <=>  {ftou((({v.vertex.z : 0.001} * {(vs_cbuf13_0.x) : 0}) + {v.offset.z : 1.00})) : 1065353216}
		u_12_4 = ftou(((v.vertex.z * (vs_cbuf13_0.x)) + v.offset.z));
		u_12_phi_26 = u_12_4;
	}
	// 0.50196  <=>  (({utof(u_20_phi_24) : 0.50196} * {utof(vs_cbuf9[83].x) : 1.00}) * (1.0f / {utof(vs_cbuf9[83].z) : 1.00}))
	o.fs_attr2.z = ((utof(u_20_phi_24) * utof(vs_cbuf9[83].x)) * (1.0f / utof(vs_cbuf9[83].z)));
	// 0  <=>  ({u_16_0 : 0} + uint({f_16_6 : 0}))
	u_8_15 = (u_16_0 + uint(f_16_6));
	// 0  <=>  ({u_17_0 : 0} + uint(clamp(trunc(({utof(u_12_1) : 0.9999999} * float({u_9_14 : 0}))), float(0.f), float(4294967300.f))))
	u_9_20 = (u_17_0 + uint(clamp(trunc((utof(u_12_1) * float(u_9_14))), float(0.f), float(4294967300.f))));
	// 0  <=>  {ftou(v.offset.y) : 0}
	u_15_8 = ftou(v.offset.y);
	u_15_phi_27 = u_15_8;
	// True  <=>  if(((! (((({v.vertex.z : 0.001} == 0.f) && (! myIsNaN({v.vertex.z : 0.001}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : 0} == 0.f) && (! myIsNaN({v.vertex.x : 0}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : 0} == 0.f) && (! myIsNaN({v.vertex.y : 0}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.y : 0} * {(vs_cbuf13_0.x) : 0}) + {v.offset.y : 0})) : 0}
		u_15_9 = ftou(((v.vertex.y * (vs_cbuf13_0.x)) + v.offset.y));
		u_15_phi_27 = u_15_9;
	}
	// 1.50  <=>  ({utof(vs_cbuf9[113].x) : 1.50} * {(vs_cbuf10_0.w) : 1.00})
	o.fs_attr0.w = (utof(vs_cbuf9[113].x) * (vs_cbuf10_0.w));
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.x : 0.5484} > 0.5f) && (! myIsNaN({i.vao_attr7.x : 0.5484}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.x > 0.5f) && (! myIsNaN(i.vao_attr7.x))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 1056997491  <=>  {ftou(v.uv.y) : 1056997491}
	u_14_6 = ftou(v.uv.y);
	u_14_phi_29 = u_14_6;
	// False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 0}) == 2u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.y : 0.96051} > 0.5f) && (! myIsNaN({i.vao_attr7.y : 0.96051}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.y > 0.5f) && (! myIsNaN(i.vao_attr7.y))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.y : 0.50196}) + 1.f)) : 1056898842}
		u_14_7 = ftou(((0.f - v.uv.y) + 1.f));
		u_14_phi_29 = u_14_7;
	}
	// 0.2738095  <=>  (({utof(vs_cbuf9[105].y) : 0.9126984} * {(vs_cbuf10_0.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
	o.fs_attr0.y = ((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x));
	// 0.2542517  <=>  (({utof(vs_cbuf9[105].x) : 0.8475056} * {(vs_cbuf10_0.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
	o.fs_attr0.x = ((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_15 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_19_4 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_15 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_15 : 0}), int(0u), int(16u))))))
	u_11_6 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_15), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_13_6 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
	u_13_7 = (bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u))), int(16u), int(16u)) << 16u);
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(16u), int(16u))) * {u_13_6 : 1})) << 16u) + ({u_13_7 : 0} + uint((uint(bitfieldExtract(uint({u_9_20 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_7_3 : 1}), int(0u), int(16u)))))))
	u_12_11 = ((uint((uint(bitfieldExtract(uint(u_9_20), int(16u), int(16u))) * u_13_6)) << 16u) + (u_13_7 + uint((uint(bitfieldExtract(uint(u_9_20), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_7_3), int(0u), int(16u)))))));
	// 1056997491  <=>  {ftou(v.uv.x) : 1056997491}
	u_13_9 = ftou(v.uv.x);
	u_13_phi_30 = u_13_9;
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.x : 0.5484} > 0.5f) && (! myIsNaN({i.vao_attr7.x : 0.5484}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.x > 0.5f) && (! myIsNaN(i.vao_attr7.x))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.x : 0.50196}) + 1.f)) : 1056898842}
		u_13_10 = ftou(((0.f - v.uv.x) + 1.f));
		u_13_phi_30 = u_13_10;
	}
	// 0.2733477  <=>  (({utof(vs_cbuf9[105].z) : 0.9111589} * {(vs_cbuf10_0.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
	o.fs_attr0.z = ((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x));
	// 0.1291005  <=>  (({utof(vs_cbuf9[121].x) : 0.4303351} * {(vs_cbuf10_1.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
	o.fs_attr1.x = ((utof(vs_cbuf9[121].x) * (vs_cbuf10_1.x)) * utof(vs_cbuf9[104].x));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(16u), int(16u))) * {u_19_4 : 0})) << 16u) + {u_11_6 : 0}))))
	u_11_8 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u))) * u_19_4)) << 16u) + u_11_6))));
	// 0  <=>  uint((int(0) - int({u_12_11 : 0})))
	u_12_12 = uint((int(0) - int(u_12_11)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_8 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_6_7), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_2_11 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 1.00  <=>  {v.coeff.y : 1.00}
	o.fs_attr5.y = v.coeff.y;
	// 0  <=>  ((uint({u_11_8 : 0}) >= uint({u_6_7 : 1})) ? 4294967295u : 0u)
	u_11_9 = ((uint(u_11_8) >= uint(u_6_7)) ? 4294967295u : 0u);
	// 1.00  <=>  {v.coeff.z : 1.00}
	o.fs_attr5.z = v.coeff.z;
	// 1.00  <=>  {v.coeff.w : 1.00}
	o.fs_attr5.w = v.coeff.w;
	// 0  <=>  (uint((int(0) - int({u_8_15 : 0}))) + {u_11_9 : 0})
	u_8_17 = (uint((int(0) - int(u_8_15))) + u_11_9);
	// 0  <=>  ((0.f * ((({pf_7_1 : 0} * {(vs_cbuf10_6.z) : 1.00}) + (({pf_6_1 : 0} * {(vs_cbuf10_6.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_6.x) : 0}))) + {(vs_cbuf10_6.w) : -3936.7583})) + (({utof(u_15_phi_27) : 0} * (0.f - {(vs_cbuf10_10.z) : 1.00})) + (({utof(u_12_phi_26) : 1.00} * {(vs_cbuf10_10.y) : 0}) + ({utof(u_11_phi_25) : 0} * {(vs_cbuf10_10.x) : 0}))))
	o.fs_attr6.z = ((0.f * (((pf_7_1 * (vs_cbuf10_6.z)) + ((pf_6_1 * (vs_cbuf10_6.y)) + (pf_3_4 * (vs_cbuf10_6.x)))) + (vs_cbuf10_6.w))) + ((utof(u_15_phi_27) * (0.f - (vs_cbuf10_10.z))) + ((utof(u_12_phi_26) * (vs_cbuf10_10.y)) + (utof(u_11_phi_25) * (vs_cbuf10_10.x)))));
	// 0  <=>  uint((int(0) - int(({u_6_7 : 1} >> 31u))))
	u_9_21 = uint((int(0) - int((u_6_7 >> 31u))));
	// 0  <=>  uint(bitfieldExtract(uint({u_8_17 : 0}), int(16u), int(16u)))
	u_16_8 = uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_6_7 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_24 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_6_7), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * {u_16_8 : 0})), uint(bitfieldExtract(uint({u_8_17 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_12_17 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * u_16_8)), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * {u_16_8 : 0})), uint(bitfieldExtract(uint({u_8_17 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 0}), int(0u), int(16u))))))
	u_8_21 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * u_16_8)), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_7), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))))));
	// 0  <=>  (uint((int(0) - int({u_9_21 : 0}))) + (({u_9_20 : 0} + uint((int(0) - int(((uint({u_12_12 : 0}) >= uint({u_7_3 : 1})) ? 4294967295u : 0u))))) ^ {u_9_21 : 0}))
	u_7_8 = (uint((int(0) - int(u_9_21))) + ((u_9_20 + uint((int(0) - int(((uint(u_12_12) >= uint(u_7_3)) ? 4294967295u : 0u))))) ^ u_9_21));
	// 1.00  <=>  ((0.f * ((({pf_7_1 : 0} * {(vs_cbuf10_5.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_5.y) : 1.00}) + ({pf_3_4 : 0} * {(vs_cbuf10_5.x) : 0}))) + {(vs_cbuf10_5.w) : 35.58958})) + (({utof(u_15_phi_27) : 0} * (0.f - {(vs_cbuf10_9.z) : 0})) + (({utof(u_12_phi_26) : 1.00} * {(vs_cbuf10_9.y) : 1.00}) + ({utof(u_11_phi_25) : 0} * {(vs_cbuf10_9.x) : 0}))))
	o.fs_attr6.y = ((0.f * (((pf_7_1 * (vs_cbuf10_5.z)) + ((pf_6_1 * (vs_cbuf10_5.y)) + (pf_3_4 * (vs_cbuf10_5.x)))) + (vs_cbuf10_5.w))) + ((utof(u_15_phi_27) * (0.f - (vs_cbuf10_9.z))) + ((utof(u_12_phi_26) * (vs_cbuf10_9.y)) + (utof(u_11_phi_25) * (vs_cbuf10_9.x)))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].w) : 1.00}) * {utof(vs_cbuf9[78].y) : 1.00})
	pf_6_5 = ((1.0f / utof(vs_cbuf9[78].w)) * utof(vs_cbuf9[78].y));
	// 0  <=>  ((0.f * ((({pf_7_1 : 0} * {(vs_cbuf10_4.z) : 0}) + (({pf_6_1 : 0} * {(vs_cbuf10_4.y) : 0}) + ({pf_3_4 : 0} * {(vs_cbuf10_4.x) : 1.00}))) + {(vs_cbuf10_4.w) : -1134.2701})) + (({utof(u_15_phi_27) : 0} * (0.f - {(vs_cbuf10_8.z) : 0})) + (({utof(u_12_phi_26) : 1.00} * {(vs_cbuf10_8.y) : 0}) + ({utof(u_11_phi_25) : 0} * {(vs_cbuf10_8.x) : 1.00}))))
	o.fs_attr6.x = ((0.f * (((pf_7_1 * (vs_cbuf10_4.z)) + ((pf_6_1 * (vs_cbuf10_4.y)) + (pf_3_4 * (vs_cbuf10_4.x)))) + (vs_cbuf10_4.w))) + ((utof(u_15_phi_27) * (0.f - (vs_cbuf10_8.z))) + ((utof(u_12_phi_26) * (vs_cbuf10_8.y)) + (utof(u_11_phi_25) * (vs_cbuf10_8.x)))));
	// 345.5264  <=>  (({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f))))
	o.fs_attr3.y = ((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f))));
	// 0.1418074  <=>  (({utof(vs_cbuf9[121].y) : 0.4726913} * {(vs_cbuf10_1.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
	o.fs_attr1.y = ((utof(vs_cbuf9[121].y) * (vs_cbuf10_1.y)) * utof(vs_cbuf9[104].x));
	// ((0.f - {utof(u_0_8) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
	pf_3_16 = ((0.f - utof(u_0_8)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x));
	// ((0.f - {utof(u_4_8) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
	pf_6_7 = ((0.f - utof(u_4_8)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y));
	// -0.87222826  <=>  (0.f - (({pf_0_1 : 999.50} * {utof(vs_cbuf9[77].x) : 0.0008727}) + (({i.vao_attr7.z : 0.58835} * {utof(vs_cbuf9[77].z) : 0}) + ({utof(vs_cbuf9[77].z) : 0} + {utof(vs_cbuf9[77].y) : 0}))))
	f_1_35 = (0.f - ((pf_0_1 * utof(vs_cbuf9[77].x)) + ((i.vao_attr7.z * utof(vs_cbuf9[77].z)) + (utof(vs_cbuf9[77].z) + utof(vs_cbuf9[77].y)))));
	// -0.3562527  <=>  (0.f - clamp(((((({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f)))) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * -0.7f) + 0.85f), 0.0, 1.0))
	f_10_9 = (0.f - clamp((((((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f)))) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * -0.7f) + 0.85f), 0.0, 1.0));
	// -0.3562527  <=>  (0.f - clamp(((((({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f)))) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * -0.7f) + 0.85f), 0.0, 1.0))
	f_11_9 = (0.f - clamp((((((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f)))) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * -0.7f) + 0.85f), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(f_10_9, f_10_9)
	// 0  <=>  (({b_1_22 : False} || {b_0_8 : True}) ? ((uint((uint(bitfieldExtract(uint({u_6_7 : 1}), int(16u), int(16u))) * {u_12_17 : 0})) << 16u) + {u_8_21 : 0}) : 4294967295u)
	u_1_7 = ((b_1_22 || b_0_8) ? ((uint((uint(bitfieldExtract(uint(u_6_7), int(16u), int(16u))) * u_12_17)) << 16u) + u_8_21) : 4294967295u);
	// ((0.f - {utof(u_5_8) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
	pf_6_8 = ((0.f - utof(u_5_8)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].z) : 1.00}) * {utof(vs_cbuf9[78].x) : 1.00})
	pf_8_12 = ((1.0f / utof(vs_cbuf9[78].z)) * utof(vs_cbuf9[78].x));
	// ({utof(u_0_8) : } + (({pf_3_16 : } * {f_10_9 : -0.3562527}) + {pf_3_16 : }))
	pf_2_11 = (utof(u_0_8) + ((pf_3_16 * f_10_9) + pf_3_16));
	// -0.3562527  <=>  (0.f - clamp(((((({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + ((0.f * (({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285}))))) + ({pf_8_8 : -201.18993} * -0.5f)))) * (1.0f / ({pf_4_21 : 489.8628} + ((0.f * {pf_11_3 : 489.6667}) + {pf_13_2 : -0})))) * -0.7f) + 0.85f), 0.0, 1.0))
	f_0_24 = (0.f - clamp((((((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((0.f * ((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x)))))) + (pf_8_8 * -0.5f)))) * (1.0f / (pf_4_21 + ((0.f * pf_11_3) + pf_13_2)))) * -0.7f) + 0.85f), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(f_10_9, f_10_9)|(f_11_9, f_11_9)
	// (({pf_2_11 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_11 : })
	pf_2_12 = ((pf_2_11 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_11);
	// ({utof(u_4_8) : } + (({pf_6_7 : } * {f_11_9 : -0.3562527}) + {pf_6_7 : }))
	pf_3_18 = (utof(u_4_8) + ((pf_6_7 * f_11_9) + pf_6_7));
	// {pf_2_12 : }
	o.fs_attr9.x = pf_2_12;
	// ({utof(u_5_8) : } + (({pf_6_8 : } * {f_0_24 : -0.3562527}) + {pf_6_8 : }))
	pf_2_13 = (utof(u_5_8) + ((pf_6_8 * f_0_24) + pf_6_8));
	// 0.6957914  <=>  exp2((log2(((0.f - clamp((({pf_17_1 : 489.7266} * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.x) : 20.00}))
	f_0_28 = exp2((log2(((0.f - clamp(((pf_17_1 * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.x)));
	// 0  <=>  exp2((log2(((0.f - clamp((({pf_17_1 : 489.7266} * {(vs_cbuf15_24.x) : 0.002381}) + (0.f - {(vs_cbuf15_24.y) : -0.04761905})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_24.w) : 4.00}))
	f_2_73 = exp2((log2(((0.f - clamp(((pf_17_1 * (vs_cbuf15_24.x)) + (0.f - (vs_cbuf15_24.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_24.w)));
	// (({pf_3_18 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_3_18 : })
	pf_3_19 = ((pf_3_18 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_3_18);
	// {pf_3_19 : }
	o.fs_attr9.y = pf_3_19;
	// (({pf_2_13 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_13 : })
	pf_2_14 = ((pf_2_13 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_13);
	// {pf_2_14 : }
	o.fs_attr9.z = pf_2_14;
	// 0.1452381  <=>  (({utof(vs_cbuf9[121].z) : 0.484127} * {(vs_cbuf10_1.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.30})
	o.fs_attr1.z = ((utof(vs_cbuf9[121].z) * (vs_cbuf10_1.z)) * utof(vs_cbuf9[104].x));
	// 0.20  <=>  (clamp(((((0.f - {pf_2_6 : 36.30958}) + min({(camera_wpos.y) : 365.7373}, {(vs_cbuf15_27.z) : 250.00})) * {(vs_cbuf15_27.y) : 0.0071429}) + {(vs_cbuf15_27.x) : -0.14285715}), 0.0, 1.0) * {(vs_cbuf15_26.w) : 0.20})
	o.fs_attr8.y = (clamp(((((0.f - pf_2_6) + min((camera_wpos.y), (vs_cbuf15_27.z))) * (vs_cbuf15_27.y)) + (vs_cbuf15_27.x)), 0.0, 1.0) * (vs_cbuf15_26.w));
	// 0.6023228  <=>  ((((({pf_0_1 : 999.50} * {utof(vs_cbuf9[75].z) : 0}) + (({i.vao_attr7.x : 0.5484} * {utof(vs_cbuf9[76].z) : 0}) + ({utof(vs_cbuf9[76].x) : 2.00} + {utof(vs_cbuf9[76].z) : 0}))) * ((cos({f_1_35 : -0.87222826}) * (({pf_8_12 : 1.00} * {utof(u_13_phi_30) : 0.50196}) + -0.5f)) + (0.f - ((({pf_6_5 : 1.00} * {utof(u_14_phi_29) : 0.50196}) + -0.5f) * sin({f_1_35 : -0.87222826}))))) + (({pf_8_12 : 1.00} * float(int({u_1_7 : 0}))) + (({pf_0_1 : 999.50} * (0.f - {utof(vs_cbuf9[74].x) : 0})) + (0.f - ((({i.vao_attr7.x : 0.5484} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0})))))) + 0.5f)
	o.fs_attr2.x = (((((pf_0_1 * utof(vs_cbuf9[75].z)) + ((i.vao_attr7.x * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z)))) * ((cos(f_1_35) * ((pf_8_12 * utof(u_13_phi_30)) + -0.5f)) + (0.f - (((pf_6_5 * utof(u_14_phi_29)) + -0.5f) * sin(f_1_35))))) + ((pf_8_12 * float(int(u_1_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((i.vao_attr7.x * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f);
	// -188.93994  <=>  (({pf_4_21 : 489.8628} * 0.5f) + ((0.f * {pf_11_3 : 489.6667}) + (((({pf_10_5 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_9_6 : -489.8628} * {(view_proj[4].z) : 0}) + (({pf_8_6 : -93.81641} * {(view_proj[4].y) : 0}) + ({pf_4_17 : -719.3513} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * {pf_8_8 : -201.18993}))))
	o.fs_attr3.x = ((pf_4_21 * 0.5f) + ((0.f * pf_11_3) + ((((pf_10_5 * (view_proj[4].w)) + ((pf_9_6 * (view_proj[4].z)) + ((pf_8_6 * (view_proj[4].y)) + (pf_4_17 * (view_proj[4].x))))) * 0.5f) + (0.f * pf_8_8))));
	// 1.00  <=>  {(vs_cbuf10_3.x) : 1.00}
	o.fs_attr4.x = (vs_cbuf10_3.x);
	// 1.420539  <=>  ((((((({pf_8_12 : 1.00} * {utof(u_13_phi_30) : 0.50196}) + -0.5f) * sin({f_1_35 : -0.87222826})) + (cos({f_1_35 : -0.87222826}) * (({pf_6_5 : 1.00} * {utof(u_14_phi_29) : 0.50196}) + -0.5f))) * (({pf_0_1 : 999.50} * {utof(vs_cbuf9[75].w) : 0}) + (({i.vao_attr7.y : 0.96051} * {utof(vs_cbuf9[76].w) : 0}) + ({utof(vs_cbuf9[76].y) : 2.00} + {utof(vs_cbuf9[76].w) : 0})))) + (0.f - (({pf_6_5 : 1.00} * (0.f - float(int((({b_2_11 : False} || {b_1_24 : True}) ? {u_7_8 : 0} : 4294967295u))))) + (({pf_0_1 : 999.50} * {utof(vs_cbuf9[74].y) : 0}) + ((({i.vao_attr7.y : 0.96051} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0})))))) + 0.5f)
	o.fs_attr2.y = (((((((pf_8_12 * utof(u_13_phi_30)) + -0.5f) * sin(f_1_35)) + (cos(f_1_35) * ((pf_6_5 * utof(u_14_phi_29)) + -0.5f))) * ((pf_0_1 * utof(vs_cbuf9[75].w)) + ((i.vao_attr7.y * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].y) + utof(vs_cbuf9[76].w))))) + (0.f - ((pf_6_5 * (0.f - float(int(((b_2_11 || b_1_24) ? u_7_8 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((i.vao_attr7.y * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f);
	// 0.2585773  <=>  clamp((({f_0_28 : 0.6957914} * (0.f - {(vs_cbuf15_23.z) : 0.85})) + {(vs_cbuf15_23.z) : 0.85}), 0.0, 1.0)
	o.fs_attr10.w = clamp(((f_0_28 * (0.f - (vs_cbuf15_23.z))) + (vs_cbuf15_23.z)), 0.0, 1.0);
	// 0.7006614  <=>  (({f_2_73 : 0} * (0.f - {(vs_cbuf15_25.w) : 0.7006614})) + {(vs_cbuf15_25.w) : 0.7006614})
	o.fs_attr8.x = ((f_2_73 * (0.f - (vs_cbuf15_25.w))) + (vs_cbuf15_25.w));
	// 0.50  <=>  (clamp(max(((({pf_17_1 : 489.7266} + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.x : 0.50})
	o.fs_attr10.x = (clamp(max((((pf_17_1 + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.x);
	// 0.50  <=>  (clamp(max(((({pf_17_1 : 489.7266} + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.y : 0.50})
	o.fs_attr10.y = (clamp(max((((pf_17_1 + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.y);
	// 0.50  <=>  (clamp(max(((({pf_17_1 : 489.7266} + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.z : 0.50})
	o.fs_attr10.z = (clamp(max((((pf_17_1 + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.z);
	// True  <=>  if(((! (((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0}))) ? true : false))
	if(((! (((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x)))) ? true : false))
	{
		return;
	}
	// 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
	f_0_31 = (1.0f / (vs_cbuf15_51.x));
	// 1.00  <=>  ((({f4_0_1.w : 1.00} * {(vs_cbuf15_49.x) : 0}) + (0.f - {(vs_cbuf15_49.x) : 0})) + 1.f)
	pf_0_9 = (((f4_0_1.w * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f);
	// 0.4628701  <=>  clamp((({pf_17_1 : 489.7266} * {f_0_31 : 0.0010526}) + (0.f - ({f_0_31 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
	f_0_32 = clamp(((pf_17_1 * f_0_31) + (0.f - (f_0_31 * (vs_cbuf15_51.y)))), 0.0, 1.0);
	// -∞  <=>  log2(abs((({pf_0_9 : 1.00} * (0.f - {f_0_32 : 0.4628701})) + {f_0_32 : 0.4628701})))
	f_0_34 = log2(abs(((pf_0_9 * (0.f - f_0_32)) + f_0_32)));
	// 0  <=>  exp2(({f_0_34 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
	f_0_35 = exp2((f_0_34 * (vs_cbuf15_51.z)));
	// 1.00  <=>  (({pf_0_9 : 1.00} * (0.f - (({f_0_35 : 0} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0}))) + {pf_0_9 : 1.00})
	o.fs_attr7.x = ((pf_0_9 * (0.f - ((f_0_35 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_9);
	return;
}
