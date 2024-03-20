void vert_from_glsl(appdata v, vaodata i, inout v2f o)
{
    // 675610624u, 6u, 58880u, 0u
    uint vs_cbuf0_21_x = 675610624u;
    uint vs_cbuf0_21_y = 6u;
    uint vs_cbuf0_21_z = 58880u;
    uint vs_cbuf0_21_w = 0u;
    // 9u, 0u, 301056u, 47u
    uint vs_cbuf9_7_x = 9u;
    uint vs_cbuf9_7_y = 0u;
    uint vs_cbuf9_7_z = 301056u;
    uint vs_cbuf9_7_w = 47u;

	// 1065353216 = 1.00f;
	// vs_cbuf0[21] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf8[0] = float4(-0.7425708, 0.00, 0.6697676, 1075.086);
	// vs_cbuf8[1] = float4(0.339885, 0.8616711, 0.3768303, 1743.908);
	// vs_cbuf8[2] = float4(-0.5771194, 0.5074672, -0.6398518, -3681.84);
	// vs_cbuf8[3] = float4(0.00, 0.00, 0.00, 1.00);
	// vs_cbuf8[4] = float4(1.206285, 0.00, 0.00, 0.00);
	// vs_cbuf8[5] = float4(0.00, 2.144507, 0.00, 0.00);
	// vs_cbuf8[6] = float4(0.00, 0.00, -1.000008, -0.2000008);
	// vs_cbuf8[7] = float4(0.00, 0.00, -1.00, 0.00);
	// vs_cbuf8[10] = float4(0.5771239, -0.5074712, 0.6398569, 3681.669);
	// vs_cbuf8[11] = float4(0.5771194, -0.5074672, 0.6398518, 3681.84);
	// vs_cbuf8[24] = float4(-0.7425708, 0.339885, -0.5771194, 0.00);
	// vs_cbuf8[25] = float4(0.00, 0.8616711, 0.5074672, 0.00);
	// vs_cbuf8[26] = float4(0.6697676, 0.3768303, -0.6398518, 0.00);
	// vs_cbuf8[27] = float4(0.00, 0.00, 0.00, 1.00);
	// vs_cbuf8[28] = float4(-0.5771194, 0.5074672, -0.6398518, 0.00);
	// vs_cbuf8[29] = float4(-1919.262, 365.7373, -3733.047, 0.00);
	// vs_cbuf8[30] = float4(0.10, 25000.00, 2500.00, 24999.90);
	// vs_cbuf9[0] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[1] = float4(73899130000000000000000.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[2] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[3] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[4] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[5] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[6] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[7] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[8] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[9] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[10] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[11] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[12] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[13] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[14] = float4(0.00, -1.00, 0.00, 0.0006);
	// vs_cbuf9[15] = float4(0.97, 0.00, 0.00, 0.00);
	// vs_cbuf9[16] = float4(0.00, 0.50, 0.00, 0.25);
	// vs_cbuf9[17] = float4(0.30, 1.00, 80.00, 20.00);
	// vs_cbuf9[18] = float4(1.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[19] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[20] = float4(2.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[21] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[22] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[23] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[24] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[25] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[26] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[27] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[28] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[29] = float4(2.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[30] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[31] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[32] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[33] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[34] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[35] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[36] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[37] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[38] = float4(2.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[39] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[40] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[41] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[42] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[43] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[44] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[45] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[46] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[47] = float4(4.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[48] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[49] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[50] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[51] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[52] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[53] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[54] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[55] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[56] = float4(4.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[57] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[58] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[59] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[60] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[61] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[62] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[63] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[64] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[65] = float4(4.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[66] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[67] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[68] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[69] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[70] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[71] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[72] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[73] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[74] = float4(0.00, -0.0001, 0.00, 0.00);
	// vs_cbuf9[75] = float4(1.00, 1.00, 0.001, 0.001);
	// vs_cbuf9[76] = float4(0.60, 0.25, 0.10, 0.05);
	// vs_cbuf9[77] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[78] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[79] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[80] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[81] = float4(1.10, 1.10, 0.00, 0.00);
	// vs_cbuf9[82] = float4(0.00, 0.00, 6.283185, 0.00);
	// vs_cbuf9[83] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[84] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[85] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[86] = float4(1.00, 1.00, 0.00, 0.00);
	// vs_cbuf9[87] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[88] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[89] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[90] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[91] = float4(1.00, 1.00, 0.00, 0.00);
	// vs_cbuf9[92] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[93] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[94] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[95] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[96] = float4(1.00, 1.00, 0.00, 0.00);
	// vs_cbuf9[97] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[98] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[99] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[100] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[101] = float4(1.00, 1.00, 0.00, 0.00);
	// vs_cbuf9[102] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[103] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[104] = float4(1.40, 0.00, 0.00, 0.00);
	// vs_cbuf9[105] = float4(0.6507937, 0.6392028, 0.5733182, 0.00);
	// vs_cbuf9[106] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[107] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[108] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[109] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[110] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[111] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[112] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[113] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[114] = float4(0.25, 0.25, 0.25, 0.11);
	// vs_cbuf9[115] = float4(0.80, 0.80, 0.80, 0.23);
	// vs_cbuf9[116] = float4(1.00, 1.00, 1.00, 0.42);
	// vs_cbuf9[117] = float4(0.80, 0.80, 0.80, 0.65);
	// vs_cbuf9[118] = float4(0.25, 0.25, 0.25, 0.83);
	// vs_cbuf9[119] = float4(0.00, 0.00, 0.00, 1.00);
	// vs_cbuf9[120] = float4(0.00, 0.00, 0.00, 8.00);
	// vs_cbuf9[121] = float4(1.00, 1.00, 1.00, 0.00);
	// vs_cbuf9[122] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[123] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[124] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[125] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[126] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[127] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[128] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[129] = float4(2.10, 1.00, 1.00, 0.00);
	// vs_cbuf9[130] = float4(2.10, 1.00, 1.00, 1.00);
	// vs_cbuf9[131] = float4(2.10, 1.00, 1.00, 2.00);
	// vs_cbuf9[132] = float4(2.10, 1.00, 1.00, 3.00);
	// vs_cbuf9[133] = float4(2.10, 1.00, 1.00, 4.00);
	// vs_cbuf9[134] = float4(2.10, 1.00, 1.00, 5.00);
	// vs_cbuf9[135] = float4(2.10, 1.00, 1.00, 6.00);
	// vs_cbuf9[136] = float4(2.10, 1.00, 1.00, 7.00);
	// vs_cbuf9[137] = float4(0.00, 0.50, 0.70, 1.00);
	// vs_cbuf9[138] = float4(3.00, 5.00, 10.00, 20.00);
	// vs_cbuf9[139] = float4(1.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[140] = float4(0.00, 3.00, 0.00, 0.00);
	// vs_cbuf9[141] = float4(0.76, 0.76, 0.76, 0.00);
	// vs_cbuf9[142] = float4(0.88, 0.88, 0.88, 0.20);
	// vs_cbuf9[143] = float4(1.20, 1.20, 1.20, 0.48);
	// vs_cbuf9[144] = float4(1.32, 1.32, 1.32, 0.67);
	// vs_cbuf9[145] = float4(1.41, 1.41, 1.41, 1.00);
	// vs_cbuf9[146] = float4(1.41, 1.41, 1.41, 6.00);
	// vs_cbuf9[147] = float4(1.41, 1.41, 1.41, 7.00);
	// vs_cbuf9[148] = float4(1.41, 1.41, 1.41, 8.00);
	// vs_cbuf9[149] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[150] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[151] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[152] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[153] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[154] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[155] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[156] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[157] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[158] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[159] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[160] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[194] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf9[195] = float4(0.00, 0.00, 0.00, 1.00);
	// vs_cbuf9[196] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf10[0] = float4(1.20, 1.20, 1.20, 0.20);
	// vs_cbuf10[1] = float4(0.468254, 0.468254, 0.468254, 1.00);
	// vs_cbuf10[2] = float4(791.50, 103.00, 1.00, 1.00);
	// vs_cbuf10[3] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf13[0] = float4(0.00, 0.50, 1.00, 0.20);
	// vs_cbuf13[1] = float4(0.01, 0.50, 1.00, 0.00);
	// vs_cbuf13[2] = float4(100.00, 0.00, 1.00, 70.00);
	// vs_cbuf13[3] = float4(1.00, 0.00, 0.00, 1.00);
	// vs_cbuf13[5] = float4(1.00, 0.00, 0.80, 0.00);
	// vs_cbuf13[6] = float4(1.00, 1.00, 10.00, 0.00);
	// vs_cbuf15[22] = float4(0.0000333, -0.0016639, 0.00, 0.00);
	// vs_cbuf15[23] = float4(20.00, 1.00, 0.85, -0.0107255);
	// vs_cbuf15[24] = float4(0.002381, -0.0476191, 3.363175, 4.00);
	// vs_cbuf15[25] = float4(0.0282744, 0.0931012, 0.1164359, 0.7006614);
	// vs_cbuf15[26] = float4(0.0174636, 0.1221582, 0.2193998, 0.20);
	// vs_cbuf15[27] = float4(-0.1428571, 0.0071429, 250.00, 0.00);
	// vs_cbuf15[28] = float4(0.8802994, -0.4663191, -0.0872897, 0.00);
	// vs_cbuf15[49] = float4(0.00, 0.00, 0.00, 0.00);
	// vs_cbuf15[51] = float4(950.00, 50.00, 1.50, 1.00);
	// vs_cbuf15[52] = float4(-2116.00, -3932.00, 0.0025, 0.00);
	// vs_cbuf15[58] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf16[0] = float4(0.00, 0.00, -33.41627, -256.3035);
	// vs_cbuf16[1] = float4(0.7282469, 0.00, 0.685315, 1.479783);

	bool b_0_21;
	bool b_0_23;
	bool b_1_40;
	bool b_1_42;
	bool b_1_45;
	bool b_1_59;
	bool b_2_14;
	bool b_2_16;
	bool b_2_20;
	bool b_3_4;
	bool b_4_8;
	float f_0_25;
	float f_0_50;
	float f_0_51;
	float f_1_17;
	float f_1_20;
	float f_1_25;
	float f_1_46;
	float f_1_65;
	float f_10_25;
	float f_10_26;
	float f_2_108;
	float f_2_176;
	float f_2_187;
	float f_2_44;
	float f_2_64;
	float f_2_65;
	float f_2_70;
	float f_3_122;
	float f_3_123;
	float f_3_126;
	float f_3_33;
	float f_3_50;
	float f_3_51;
	float f_3_53;
	float f_3_54;
	float f_3_66;
	float f_4_26;
	float f_5_22;
	float f_5_29;
	float f_5_30;
	float f_6_12;
	float f_6_15;
	float f_7_25;
	float f_9_15;
	float f_9_24;
	float4 f4_0_0;
	precise float pf_0_1;
	precise float pf_0_10;
	precise float pf_0_11;
	precise float pf_0_15;
	precise float pf_0_16;
	precise float pf_0_22;
	precise float pf_0_5;
	precise float pf_0_6;
	precise float pf_0_7;
	precise float pf_0_9;
	precise float pf_1_12;
	precise float pf_1_2;
	precise float pf_1_3;
	precise float pf_1_5;
	precise float pf_1_6;
	precise float pf_1_7;
	precise float pf_1_9;
	precise float pf_10_10;
	precise float pf_10_12;
	precise float pf_10_17;
	precise float pf_11_20;
	precise float pf_11_22;
	precise float pf_12_11;
	precise float pf_12_12;
	precise float pf_12_17;
	precise float pf_12_18;
	precise float pf_12_23;
	precise float pf_13_12;
	precise float pf_14_10;
	precise float pf_14_11;
	precise float pf_14_14;
	precise float pf_14_24;
	precise float pf_14_7;
	precise float pf_17_10;
	precise float pf_17_13;
	precise float pf_18_6;
	precise float pf_18_7;
	precise float pf_18_8;
	precise float pf_19_1;
	precise float pf_19_2;
	precise float pf_19_3;
	precise float pf_19_8;
	precise float pf_2_11;
	precise float pf_2_12;
	precise float pf_2_14;
	precise float pf_2_15;
	precise float pf_2_8;
	precise float pf_2_9;
	precise float pf_20_17;
	precise float pf_20_18;
	precise float pf_20_19;
	precise float pf_20_23;
	precise float pf_20_24;
	precise float pf_20_9;
	precise float pf_21_2;
	precise float pf_22_5;
	precise float pf_22_8;
	precise float pf_23_0;
	precise float pf_23_13;
	precise float pf_23_7;
	precise float pf_24_5;
	precise float pf_25_5;
	precise float pf_27_0;
	precise float pf_3_0;
	precise float pf_3_14;
	precise float pf_3_16;
	precise float pf_3_17;
	precise float pf_3_18;
	precise float pf_3_19;
	precise float pf_3_26;
	precise float pf_3_27;
	precise float pf_3_3;
	precise float pf_4_9;
	precise float pf_5_10;
	precise float pf_5_21;
	precise float pf_6_11;
	precise float pf_6_13;
	precise float pf_7_15;
	precise float pf_7_16;
	precise float pf_7_18;
	precise float pf_7_8;
	precise float pf_8_14;
	precise float pf_8_7;
	precise float pf_9_14;
	uint u_0_1;
	uint u_0_11;
	uint u_0_14;
	uint u_0_15;
	uint u_0_17;
	uint u_0_18;
	uint u_0_2;
	uint u_0_20;
	uint u_0_21;
	uint u_0_3;
	uint u_0_4;
	uint u_0_5;
	uint u_0_9;
	uint u_0_phi_20;
	uint u_0_phi_56;
	uint u_0_phi_58;
	uint u_0_phi_60;
	uint u_1_1;
	uint u_1_2;
	uint u_1_4;
	uint u_1_5;
	uint u_1_7;
	uint u_1_8;
	uint u_1_phi_14;
	uint u_1_phi_37;
	uint u_1_phi_6;
	uint u_10_10;
	uint u_10_12;
	uint u_10_6;
	uint u_10_7;
	uint u_10_9;
	uint u_10_phi_48;
	uint u_11_12;
	uint u_11_13;
	uint u_11_5;
	uint u_11_phi_57;
	uint u_12_6;
	uint u_13_2;
	uint u_13_3;
	uint u_13_5;
	uint u_13_6;
	uint u_13_7;
	uint u_13_phi_27;
	uint u_14_1;
	uint u_14_2;
	uint u_14_3;
	uint u_14_4;
	uint u_14_phi_26;
	uint u_14_phi_54;
	uint u_15_1;
	uint u_15_4;
	uint u_15_6;
	uint u_15_7;
	uint u_16_1;
	uint u_16_10;
	uint u_16_11;
	uint u_17_0;
	uint u_17_1;
	uint u_17_11;
	uint u_17_12;
	uint u_17_4;
	uint u_17_6;
	uint u_17_7;
	uint u_17_phi_55;
	uint u_18_0;
	uint u_18_3;
	uint u_18_6;
	uint u_18_7;
	uint u_19_0;
	uint u_19_1;
	uint u_19_11;
	uint u_19_12;
	uint u_19_13;
	uint u_2_13;
	uint u_20_0;
	uint u_20_1;
	uint u_20_2;
	uint u_20_phi_25;
	uint u_21_0;
	uint u_21_1;
	uint u_21_2;
	uint u_21_3;
	uint u_21_4;
	uint u_21_9;
	uint u_21_phi_21;
	uint u_21_phi_33;
	uint u_22_0;
	uint u_22_6;
	uint u_23_0;
	uint u_23_9;
	uint u_24_10;
	uint u_24_11;
	uint u_24_3;
	uint u_24_9;
	uint u_24_phi_23;
	uint u_25_11;
	uint u_25_12;
	uint u_25_4;
	uint u_25_9;
	uint u_25_phi_24;
	uint u_26_15;
	uint u_26_16;
	uint u_26_17;
	uint u_26_18;
	uint u_26_5;
	uint u_26_7;
	uint u_26_phi_28;
	uint u_27_4;
	uint u_27_6;
	uint u_27_7;
	uint u_27_8;
	uint u_27_phi_29;
	uint u_28_4;
	uint u_28_5;
	uint u_28_phi_30;
	uint u_29_4;
	uint u_29_5;
	uint u_29_phi_31;
	uint u_3_0;
	uint u_3_1;
	uint u_3_11;
	uint u_3_12;
	uint u_3_2;
	uint u_3_3;
	uint u_3_5;
	uint u_3_phi_1;
	uint u_3_phi_11;
	uint u_30_4;
	uint u_30_5;
	uint u_30_phi_32;
	uint u_31_5;
	uint u_31_6;
	uint u_31_7;
	uint u_31_phi_32;
	uint u_32_1;
	uint u_32_2;
	uint u_32_phi_32;
	uint u_33_0;
	uint u_33_1;
	uint u_33_phi_32;
	uint u_34_0;
	uint u_34_1;
	uint u_34_phi_32;
	uint u_35_0;
	uint u_35_1;
	uint u_35_phi_35;
	uint u_36_0;
	uint u_36_1;
	uint u_36_phi_36;
	uint u_4_0;
	uint u_4_1;
	uint u_4_4;
	uint u_4_5;
	uint u_4_phi_3;
	uint u_4_phi_63;
	uint u_5_16;
	uint u_5_17;
	uint u_5_19;
	uint u_5_2;
	uint u_5_20;
	uint u_5_3;
	uint u_5_4;
	uint u_5_5;
	uint u_5_6;
	uint u_5_phi_43;
	uint u_5_phi_59;
	uint u_5_phi_64;
	uint u_5_phi_9;
	uint u_6_13;
	uint u_6_14;
	uint u_6_15;
	uint u_6_16;
	uint u_6_17;
	uint u_6_2;
	uint u_6_21;
	uint u_6_22;
	uint u_6_24;
	uint u_6_28;
	uint u_6_30;
	uint u_6_31;
	uint u_6_33;
	uint u_6_35;
	uint u_6_36;
	uint u_6_42;
	uint u_6_43;
	uint u_6_44;
	uint u_6_45;
	uint u_6_7;
	uint u_6_phi_18;
	uint u_6_phi_19;
	uint u_6_phi_41;
	uint u_6_phi_61;
	uint u_6_phi_62;
	uint u_7_10;
	uint u_7_11;
	uint u_7_12;
	uint u_7_13;
	uint u_7_14;
	uint u_7_16;
	uint u_7_17;
	uint u_7_18;
	uint u_7_2;
	uint u_7_9;
	uint u_7_phi_17;
	uint u_7_phi_19;
	uint u_7_phi_34;
	uint u_8_17;
	uint u_8_22;
	uint u_8_23;
	uint u_8_25;
	uint u_8_26;
	uint u_8_30;
	uint u_8_5;
	uint u_8_phi_42;
	uint u_8_phi_50;
	uint u_9_0;
	uint u_9_11;
	uint u_9_15;
	uint u_9_16;
	uint u_9_17;
	uint u_9_19;
	uint u_9_5;
	uint u_9_phi_51;
	// 0.00  <=>  float(0.00)
	o.vertex.x = float(0.00);
	// 0.00  <=>  float(0.00)
	o.vertex.y = float(0.00);
	// 125000.00  <=>  float(125000.00)
	o.vertex.z = float(125000.00);
	// 27.38721  <=>  float(27.38721)
	o.vertex.w = float(27.38721);
	// 1.09333  <=>  float(1.09333)
	o.fs_attr0.x = float(1.09333);
	// 1.07386  <=>  float(1.07386)
	o.fs_attr0.y = float(1.07386);
	// 0.96317  <=>  float(0.96317)
	o.fs_attr0.z = float(0.96317);
	// 0.00147  <=>  float(0.00147)
	o.fs_attr0.w = float(0.00147);
	// 0.00  <=>  float(0.00)
	o.fs_attr1.x = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr1.y = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr1.z = float(0.00);
	// 2.10  <=>  float(2.10)
	o.fs_attr1.w = float(2.10);
	// -0.13084  <=>  float(-0.13084)
	o.fs_attr2.x = float(-0.13084);
	// 1.70043  <=>  float(1.70043)
	o.fs_attr2.y = float(1.70043);
	// 0.74605  <=>  float(0.74605)
	o.fs_attr2.z = float(0.74605);
	// 0.92636  <=>  float(0.92636)
	o.fs_attr2.w = float(0.92636);
	// 17.3302  <=>  float(17.3302)
	o.fs_attr3.x = float(17.3302);
	// 10.47894  <=>  float(10.47894)
	o.fs_attr3.y = float(10.47894);
	// 27.28732  <=>  float(27.28732)
	o.fs_attr3.z = float(27.28732);
	// 27.38721  <=>  float(27.38721)
	o.fs_attr3.w = float(27.38721);
	// 0.00  <=>  float(0.00)
	o.fs_attr4.x = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr4.y = float(0.00);
	// 0.00  <=>  float(0.00)
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
	// -0.4113  <=>  float(-0.4113)
	o.fs_attr6.x = float(-0.4113);
	// -0.14099  <=>  float(-0.14099)
	o.fs_attr6.y = float(-0.14099);
	// -0.90012  <=>  float(-0.90012)
	o.fs_attr6.z = float(-0.90012);
	// 1.00  <=>  float(1.00)
	o.fs_attr6.w = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.x = float(1.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr7.y = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr7.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.w = float(1.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr8.x = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr8.y = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr8.z = float(0.00);
	// 0.00  <=>  float(0.00)
	o.fs_attr8.w = float(0.00);
	// 0.51474  <=>  float(0.51474)
	o.fs_attr9.x = float(0.51474);
	// 0.58878  <=>  float(0.58878)
	o.fs_attr9.y = float(0.58878);
	// 0.60599  <=>  float(0.60599)
	o.fs_attr9.z = float(0.60599);
	// 1.00  <=>  float(1.00)
	o.fs_attr9.w = float(1.00);
	// 1  <=>  1u
	u_3_0 = 1u;
	u_3_phi_1 = u_3_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0u
		u_3_1 = 0u;
		u_3_phi_1 = u_3_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_4_0 = 0u;
	u_4_phi_3 = u_4_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_4_1 = ftou(vs_cbuf8_30.y);
		u_4_phi_3 = u_4_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.y = 0.f;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0.00  <=>  0.f
		o.fs_attr4.x = 0.f;
	}
	// 0  <=>  {u_4_phi_3 : 0}
	u_1_1 = u_4_phi_3;
	u_1_phi_6 = u_1_1;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_4_phi_3) : 0.00} * 5.f)) : 0}
		u_1_2 = ftou((utof(u_4_phi_3) * 5.f));
		u_1_phi_6 = u_1_2;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0.00  <=>  {utof(u_1_phi_6) : 0.00}
		o.vertex.z = utof(u_1_phi_6);
	}
	// False  <=>  if(((! ({u_3_phi_1 : 1} != 0u)) ? true : false))
	if(((! (u_3_phi_1 != 0u)) ? true : false))
	{
		return;
	}
	// 199.00  <=>  ((0.f - {i.vao_attr5.w : 592.50}) + {(vs_cbuf10_2.x) : 791.50})
	pf_0_1 = ((0.f - i.vao_attr5.w) + (vs_cbuf10_2.x));
	// 1  <=>  {u_3_phi_1 : 1}
	u_5_2 = u_3_phi_1;
	u_5_phi_9 = u_5_2;
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0u
		u_5_3 = 0u;
		u_5_phi_9 = u_5_3;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 1142169600  <=>  {ftou(i.vao_attr5.w) : 1142169600}
	u_3_2 = ftou(i.vao_attr5.w);
	u_3_phi_11 = u_3_2;
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_3_3 = ftou(vs_cbuf8_30.y);
		u_3_phi_11 = u_3_3;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.y = 0.f;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0.00  <=>  0.f
		o.fs_attr4.x = 0.f;
	}
	// 1142169600  <=>  {u_3_phi_11 : 1142169600}
	u_1_4 = u_3_phi_11;
	u_1_phi_14 = u_1_4;
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1161373696  <=>  {ftou(({utof(u_3_phi_11) : 592.50} * 5.f)) : 1161373696}
		u_1_5 = ftou((utof(u_3_phi_11) * 5.f));
		u_1_phi_14 = u_1_5;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 592.50} > {(vs_cbuf10_2.x) : 791.50}) && (! myIsNaN({i.vao_attr5.w : 592.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 791.50}))) || ((({pf_0_1 : 199.00} >= float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 199.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 592.50  <=>  {utof(u_1_phi_14) : 592.50}
		o.vertex.z = utof(u_1_phi_14);
	}
	// False  <=>  if(((! ({u_5_phi_9 : 1} != 0u)) ? true : false))
	if(((! (u_5_phi_9 != 0u)) ? true : false))
	{
		return;
	}
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 2u)
	u_6_2 = (vs_cbuf9_7_x & 2u);
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 4u)
	u_6_7 = (vs_cbuf9_7_x & 4u);
	// 2.63305  <=>  ((({pf_0_1 : 199.00} + {utof(vs_cbuf9[18].z) : 0.00}) * (1.0f / {utof(vs_cbuf9[17].z) : 80.00})) + ({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[18].x) : 1.00}))
	pf_1_2 = (((pf_0_1 + utof(vs_cbuf9[18].z)) * (1.0f / utof(vs_cbuf9[17].z))) + (i.vao_attr7.x * utof(vs_cbuf9[18].x)));
	// 1  <=>  ({vs_cbuf9_7_x : 9} & 1u)
	u_7_2 = (vs_cbuf9_7_x & 1u);
	// 0.63305  <=>  ({pf_1_2 : 2.63305} + (0.f - floor({pf_1_2 : 2.63305})))
	pf_1_3 = (pf_1_2 + (0.f - floor(pf_1_2)));
	// 0.00  <=>  (({utof((((({pf_1_3 : 0.63305} >= 0.5f) && (! myIsNaN({pf_1_3 : 0.63305}))) && (! myIsNaN(0.5f))) ? 1065353216u : 0u)) : 1.00} * (0.f - {utof(vs_cbuf9[17].x) : 0.30})) + {utof(vs_cbuf9[17].x) : 0.30})
	pf_3_0 = ((utof(((((pf_1_3 >= 0.5f) && (! myIsNaN(pf_1_3))) && (! myIsNaN(0.5f))) ? 1065353216u : 0u)) * (0.f - utof(vs_cbuf9[17].x))) + utof(vs_cbuf9[17].x));
	// 0.9505678  <=>  (((((cos(({pf_1_2 : 2.63305} * 6.2831855f)) * 0.5f) + 0.5f) * (0.f - {utof(vs_cbuf9[17].x) : 0.30})) + 1.f) * float(int(abs(int((uint((int(0) - int(((int({u_7_2 : 1}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_7_2 : 1}) >= int(0u)) ? 0u : 1u))))))))))
	pf_1_5 = (((((cos((pf_1_2 * 6.2831855f)) * 0.5f) + 0.5f) * (0.f - utof(vs_cbuf9[17].x))) + 1.f) * float(int(abs(int((uint((int(0) - int(((int(u_7_2) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_7_2) >= int(0u)) ? 0u : 1u))))))))));
	// 0.9505678  <=>  ((float(int(abs(int((uint((int(0) - int(((int({u_6_2 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_6_2 : 0}) >= int(0u)) ? 0u : 1u))))))))) * abs((({pf_1_3 : 0.63305} * (0.f - {utof(vs_cbuf9[17].x) : 0.30})) + 1.f))) + {pf_1_5 : 0.9505678})
	pf_1_6 = ((float(int(abs(int((uint((int(0) - int(((int(u_6_2) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_6_2) >= int(0u)) ? 0u : 1u))))))))) * abs(((pf_1_3 * (0.f - utof(vs_cbuf9[17].x))) + 1.f))) + pf_1_5);
	// 0.9505678  <=>  ((float(int(abs(int((uint((int(0) - int(((int({u_6_7 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_6_7 : 0}) >= int(0u)) ? 0u : 1u))))))))) * abs(((0.f - {pf_3_0 : 0.00}) + 1.f))) + {pf_1_6 : 0.9505678})
	pf_1_7 = ((float(int(abs(int((uint((int(0) - int(((int(u_6_7) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_6_7) >= int(0u)) ? 0u : 1u))))))))) * abs(((0.f - pf_3_0) + 1.f))) + pf_1_6);
	// 0  <=>  {ftou(clamp(min(0.f, {i.vao_attr7.x : 0.14555}), 0.0, 1.0)) : 0}
	u_7_9 = ftou(clamp(min(0.f, i.vao_attr7.x), 0.0, 1.0));
	u_7_phi_17 = u_7_9;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[12].x) : 0.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[12].x) : 0.00}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[12].x)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[12].x)))) ? true : false))
	{
		// ∞  <=>  (((({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[13].y) : 0.00}) * {utof(vs_cbuf9[12].x) : 0.00}) + {pf_0_1 : 199.00}) * (1.0f / {utof(vs_cbuf9[12].x) : 0.00}))
		pf_3_3 = ((((i.vao_attr7.x * utof(vs_cbuf9[13].y)) * utof(vs_cbuf9[12].x)) + pf_0_1) * (1.0f / utof(vs_cbuf9[12].x)));
		// 4290772992  <=>  {ftou(({pf_3_3 : ∞} + (0.f - floor({pf_3_3 : ∞})))) : 4290772992}
		u_7_10 = ftou((pf_3_3 + (0.f - floor(pf_3_3))));
		u_7_phi_17 = u_7_10;
	}
	// 0  <=>  {u_7_phi_17 : 0}
	u_6_13 = u_7_phi_17;
	u_6_phi_18 = u_6_13;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[12].x) : 0.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[12].x) : 0.00})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[12].x)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[12].x))))) ? true : false))
	{
		// 1065269330  <=>  {ftou(({pf_0_1 : 199.00} * (1.0f / float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))))) : 1065269330}
		u_6_14 = ftou((pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))));
		u_6_phi_18 = u_6_14;
	}
	// 0.00  <=>  {utof(vs_cbuf9[141].w) : 0.00}
	f_1_17 = utof(vs_cbuf9[141].w);
	// 1065353216  <=>  (((({utof(u_6_phi_18) : 0.995} >= {f_1_17 : 0.00}) && (! myIsNaN({utof(u_6_phi_18) : 0.995}))) && (! myIsNaN({f_1_17 : 0.00}))) ? 1065353216u : 0u)
	u_7_11 = ((((utof(u_6_phi_18) >= f_1_17) && (! myIsNaN(utof(u_6_phi_18)))) && (! myIsNaN(f_1_17))) ? 1065353216u : 0u);
	// 0.20  <=>  {utof(vs_cbuf9[142].w) : 0.20}
	f_1_20 = utof(vs_cbuf9[142].w);
	// 1065353216  <=>  (((({utof(u_6_phi_18) : 0.995} >= {f_1_20 : 0.20}) && (! myIsNaN({utof(u_6_phi_18) : 0.995}))) && (! myIsNaN({f_1_20 : 0.20}))) ? 1065353216u : 0u)
	u_8_5 = ((((utof(u_6_phi_18) >= f_1_20) && (! myIsNaN(utof(u_6_phi_18)))) && (! myIsNaN(f_1_20))) ? 1065353216u : 0u);
	// 0.48  <=>  {utof(vs_cbuf9[143].w) : 0.48}
	f_1_25 = utof(vs_cbuf9[143].w);
	// 1065353216  <=>  (((({utof(u_6_phi_18) : 0.995} >= {f_1_25 : 0.48}) && (! myIsNaN({utof(u_6_phi_18) : 0.995}))) && (! myIsNaN({f_1_25 : 0.48}))) ? 1065353216u : 0u)
	u_9_0 = ((((utof(u_6_phi_18) >= f_1_25) && (! myIsNaN(utof(u_6_phi_18)))) && (! myIsNaN(f_1_25))) ? 1065353216u : 0u);
	// 0.67  <=>  {utof(vs_cbuf9[144].w) : 0.67}
	f_2_44 = utof(vs_cbuf9[144].w);
	// 1065353216  <=>  (((({utof(u_6_phi_18) : 0.995} >= {f_2_44 : 0.67}) && (! myIsNaN({utof(u_6_phi_18) : 0.995}))) && (! myIsNaN({f_2_44 : 0.67}))) ? 1065353216u : 0u)
	u_7_12 = ((((utof(u_6_phi_18) >= f_2_44) && (! myIsNaN(utof(u_6_phi_18)))) && (! myIsNaN(f_2_44))) ? 1065353216u : 0u);
	// 1.00  <=>  {utof(vs_cbuf9[145].w) : 1.00}
	f_3_33 = utof(vs_cbuf9[145].w);
	// 0  <=>  (((({utof(u_6_phi_18) : 0.995} >= {f_3_33 : 1.00}) && (! myIsNaN({utof(u_6_phi_18) : 0.995}))) && (! myIsNaN({f_3_33 : 1.00}))) ? 1065353216u : 0u)
	u_6_15 = ((((utof(u_6_phi_18) >= f_3_33) && (! myIsNaN(utof(u_6_phi_18)))) && (! myIsNaN(f_3_33))) ? 1065353216u : 0u);
	// 1.408636  <=>  (({utof(u_6_15) : 0.00} * {utof(vs_cbuf9[145].x) : 1.41}) + (((((({utof(vs_cbuf9[145].x) : 1.41} + (0.f - {utof(vs_cbuf9[144].x) : 1.32})) * (1.0f / ({utof(vs_cbuf9[145].w) : 1.00} + (0.f - {utof(vs_cbuf9[144].w) : 0.67})))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[144].w) : 0.67}))) + {utof(vs_cbuf9[144].x) : 1.32}) * (({utof(u_7_12) : 1.00} * (0.f - {utof(u_6_15) : 0.00})) + {utof(u_7_12) : 1.00})) + (((((((0.f - {utof(vs_cbuf9[143].x) : 1.20}) + {utof(vs_cbuf9[144].x) : 1.32}) * (1.0f / ((0.f - {utof(vs_cbuf9[143].w) : 0.48}) + {utof(vs_cbuf9[144].w) : 0.67}))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[143].w) : 0.48}))) + {utof(vs_cbuf9[143].x) : 1.20}) * (({utof(u_9_0) : 1.00} * (0.f - {utof(u_7_12) : 1.00})) + {utof(u_9_0) : 1.00})) + (((((({utof(vs_cbuf9[143].x) : 1.20} + (0.f - {utof(vs_cbuf9[142].x) : 0.88})) * (1.0f / ({utof(vs_cbuf9[143].w) : 0.48} + (0.f - {utof(vs_cbuf9[142].w) : 0.20})))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[142].w) : 0.20}))) + {utof(vs_cbuf9[142].x) : 0.88}) * (({utof(u_8_5) : 1.00} * (0.f - {utof(u_9_0) : 1.00})) + {utof(u_8_5) : 1.00})) + ((((({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[141].w) : 0.00})) * (({utof(vs_cbuf9[142].x) : 0.88} + (0.f - {utof(vs_cbuf9[141].x) : 0.76})) * (1.0f / ((0.f - {utof(vs_cbuf9[141].w) : 0.00}) + {utof(vs_cbuf9[142].w) : 0.20})))) + {utof(vs_cbuf9[141].x) : 0.76}) * (({utof(u_7_11) : 1.00} * (0.f - {utof(u_8_5) : 1.00})) + {utof(u_7_11) : 1.00})) + (({utof(u_7_11) : 1.00} * (0.f - {utof(vs_cbuf9[141].x) : 0.76})) + {utof(vs_cbuf9[141].x) : 0.76}))))))
	pf_3_14 = ((utof(u_6_15) * utof(vs_cbuf9[145].x)) + ((((((utof(vs_cbuf9[145].x) + (0.f - utof(vs_cbuf9[144].x))) * (1.0f / (utof(vs_cbuf9[145].w) + (0.f - utof(vs_cbuf9[144].w))))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[144].w)))) + utof(vs_cbuf9[144].x)) * ((utof(u_7_12) * (0.f - utof(u_6_15))) + utof(u_7_12))) + (((((((0.f - utof(vs_cbuf9[143].x)) + utof(vs_cbuf9[144].x)) * (1.0f / ((0.f - utof(vs_cbuf9[143].w)) + utof(vs_cbuf9[144].w)))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[143].w)))) + utof(vs_cbuf9[143].x)) * ((utof(u_9_0) * (0.f - utof(u_7_12))) + utof(u_9_0))) + ((((((utof(vs_cbuf9[143].x) + (0.f - utof(vs_cbuf9[142].x))) * (1.0f / (utof(vs_cbuf9[143].w) + (0.f - utof(vs_cbuf9[142].w))))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[142].w)))) + utof(vs_cbuf9[142].x)) * ((utof(u_8_5) * (0.f - utof(u_9_0))) + utof(u_8_5))) + (((((utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[141].w))) * ((utof(vs_cbuf9[142].x) + (0.f - utof(vs_cbuf9[141].x))) * (1.0f / ((0.f - utof(vs_cbuf9[141].w)) + utof(vs_cbuf9[142].w))))) + utof(vs_cbuf9[141].x)) * ((utof(u_7_11) * (0.f - utof(u_8_5))) + utof(u_7_11))) + ((utof(u_7_11) * (0.f - utof(vs_cbuf9[141].x))) + utof(vs_cbuf9[141].x)))))));
	// 1.013725  <=>  ((({utof(u_6_15) : 0.00} * {utof(vs_cbuf9[145].z) : 1.41}) + (((((({utof(vs_cbuf9[145].z) : 1.41} + (0.f - {utof(vs_cbuf9[144].z) : 1.32})) * (1.0f / ({utof(vs_cbuf9[145].w) : 1.00} + (0.f - {utof(vs_cbuf9[144].w) : 0.67})))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[144].w) : 0.67}))) + {utof(vs_cbuf9[144].z) : 1.32}) * (({utof(u_7_12) : 1.00} * (0.f - {utof(u_6_15) : 0.00})) + {utof(u_7_12) : 1.00})) + (((((((0.f - {utof(vs_cbuf9[143].z) : 1.20}) + {utof(vs_cbuf9[144].z) : 1.32}) * (1.0f / ((0.f - {utof(vs_cbuf9[143].w) : 0.48}) + {utof(vs_cbuf9[144].w) : 0.67}))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[143].w) : 0.48}))) + {utof(vs_cbuf9[143].z) : 1.20}) * (({utof(u_9_0) : 1.00} * (0.f - {utof(u_7_12) : 1.00})) + {utof(u_9_0) : 1.00})) + (((((({utof(vs_cbuf9[143].z) : 1.20} + (0.f - {utof(vs_cbuf9[142].z) : 0.88})) * (1.0f / ({utof(vs_cbuf9[143].w) : 0.48} + (0.f - {utof(vs_cbuf9[142].w) : 0.20})))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[142].w) : 0.20}))) + {utof(vs_cbuf9[142].z) : 0.88}) * (({utof(u_8_5) : 1.00} * (0.f - {utof(u_9_0) : 1.00})) + {utof(u_8_5) : 1.00})) + ((((({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[141].w) : 0.00})) * (({utof(vs_cbuf9[142].z) : 0.88} + (0.f - {utof(vs_cbuf9[141].z) : 0.76})) * (1.0f / ((0.f - {utof(vs_cbuf9[141].w) : 0.00}) + {utof(vs_cbuf9[142].w) : 0.20})))) + {utof(vs_cbuf9[141].z) : 0.76}) * (({utof(u_7_11) : 1.00} * (0.f - {utof(u_8_5) : 1.00})) + {utof(u_7_11) : 1.00})) + (({utof(u_7_11) : 1.00} * (0.f - {utof(vs_cbuf9[141].z) : 0.76})) + {utof(vs_cbuf9[141].z) : 0.76})))))) * {i.vao_attr6.z : 0.71965})
	pf_4_9 = (((utof(u_6_15) * utof(vs_cbuf9[145].z)) + ((((((utof(vs_cbuf9[145].z) + (0.f - utof(vs_cbuf9[144].z))) * (1.0f / (utof(vs_cbuf9[145].w) + (0.f - utof(vs_cbuf9[144].w))))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[144].w)))) + utof(vs_cbuf9[144].z)) * ((utof(u_7_12) * (0.f - utof(u_6_15))) + utof(u_7_12))) + (((((((0.f - utof(vs_cbuf9[143].z)) + utof(vs_cbuf9[144].z)) * (1.0f / ((0.f - utof(vs_cbuf9[143].w)) + utof(vs_cbuf9[144].w)))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[143].w)))) + utof(vs_cbuf9[143].z)) * ((utof(u_9_0) * (0.f - utof(u_7_12))) + utof(u_9_0))) + ((((((utof(vs_cbuf9[143].z) + (0.f - utof(vs_cbuf9[142].z))) * (1.0f / (utof(vs_cbuf9[143].w) + (0.f - utof(vs_cbuf9[142].w))))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[142].w)))) + utof(vs_cbuf9[142].z)) * ((utof(u_8_5) * (0.f - utof(u_9_0))) + utof(u_8_5))) + (((((utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[141].w))) * ((utof(vs_cbuf9[142].z) + (0.f - utof(vs_cbuf9[141].z))) * (1.0f / ((0.f - utof(vs_cbuf9[141].w)) + utof(vs_cbuf9[142].w))))) + utof(vs_cbuf9[141].z)) * ((utof(u_7_11) * (0.f - utof(u_8_5))) + utof(u_7_11))) + ((utof(u_7_11) * (0.f - utof(vs_cbuf9[141].z))) + utof(vs_cbuf9[141].z))))))) * i.vao_attr6.z);
	// 0  <=>  0u
	u_6_16 = 0u;
	// 1128792064  <=>  {ftou(float(int((myIsNaN({i.vao_attr4.w : 200.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 200.00}), float(-2147483600.f), float(2147483600.f))))))) : 1128792064}
	u_7_13 = ftou(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))));
	u_6_phi_19 = u_6_16;
	u_7_phi_19 = u_7_13;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[11].y) : 0.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].y) : 0.00}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[11].y)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].y)))) ? true : false))
	{
		// ∞  <=>  (((({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[12].z) : 0.00}) * {utof(vs_cbuf9[11].y) : 0.00}) + {pf_0_1 : 199.00}) * (1.0f / {utof(vs_cbuf9[11].y) : 0.00}))
		pf_5_10 = ((((i.vao_attr7.x * utof(vs_cbuf9[12].z)) * utof(vs_cbuf9[11].y)) + pf_0_1) * (1.0f / utof(vs_cbuf9[11].y)));
		// 4290772992  <=>  {ftou(({pf_5_10 : ∞} + (0.f - floor({pf_5_10 : ∞})))) : 4290772992}
		u_6_17 = ftou((pf_5_10 + (0.f - floor(pf_5_10))));
		// 2139095040  <=>  {ftou(pf_5_10) : 2139095040}
		u_7_14 = ftou(pf_5_10);
		u_6_phi_19 = u_6_17;
		u_7_phi_19 = u_7_14;
	}
	// 0  <=>  {u_6_phi_19 : 0}
	u_0_1 = u_6_phi_19;
	u_0_phi_20 = u_0_1;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[11].y) : 0.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].y) : 0.00})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[11].y)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].y))))) ? true : false))
	{
		// 1065269330  <=>  {ftou(({pf_0_1 : 199.00} * (1.0f / {utof(u_7_phi_19) : 200.00}))) : 1065269330}
		u_0_2 = ftou((pf_0_1 * (1.0f / utof(u_7_phi_19))));
		u_0_phi_20 = u_0_2;
	}
	// 0.08906  <=>  {i.vao_attr7.y : 0.08906}
	f_4_26 = i.vao_attr7.y;
	// 0.84634  <=>  {i.vao_attr7.z : 0.84634}
	f_1_46 = i.vao_attr7.z;
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[83].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[83].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_9_5 = (myIsNaN(utof(vs_cbuf9[83].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[83].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_10_6 = (myIsNaN(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_10_6 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_8_17 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_10_6), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_9_5 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_11_5 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_9_5), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1.00  <=>  (1.0f / float({u_8_17 : 1}))
	f_6_12 = (1.0f / float(u_8_17));
	// 1.00  <=>  (1.0f / float({u_11_5 : 1}))
	f_6_15 = (1.0f / float(u_11_5));
	// 0.11  <=>  {utof(vs_cbuf9[114].w) : 0.11}
	f_3_50 = utof(vs_cbuf9[114].w);
	// 1065353216  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_3_50 : 0.11}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_3_50 : 0.11}))) ? 1065353216u : 0u)
	u_17_0 = ((((utof(u_0_phi_20) >= f_3_50) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_3_50))) ? 1065353216u : 0u);
	// 354.1841  <=>  ((({i.vao_attr4.z : 20.30853} * {i.vao_attr10.z : 0.00}) + (({i.vao_attr4.y : -10.90624} * {i.vao_attr10.y : 1.00}) + ({i.vao_attr4.x : 6.81513} * {i.vao_attr10.x : 0.00}))) + {i.vao_attr10.w : 365.0903})
	pf_7_8 = (((i.vao_attr4.z * i.vao_attr10.z) + ((i.vao_attr4.y * i.vao_attr10.y) + (i.vao_attr4.x * i.vao_attr10.x))) + i.vao_attr10.w);
	// 0.23  <=>  {utof(vs_cbuf9[115].w) : 0.23}
	f_3_51 = utof(vs_cbuf9[115].w);
	// 1065353216  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_3_51 : 0.23}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_3_51 : 0.23}))) ? 1065353216u : 0u)
	u_18_0 = ((((utof(u_0_phi_20) >= f_3_51) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_3_51))) ? 1065353216u : 0u);
	// 0.00  <=>  {utof(vs_cbuf9[113].w) : 0.00}
	f_3_53 = utof(vs_cbuf9[113].w);
	// 1065353216  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_3_53 : 0.00}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_3_53 : 0.00}))) ? 1065353216u : 0u)
	u_19_0 = ((((utof(u_0_phi_20) >= f_3_53) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_3_53))) ? 1065353216u : 0u);
	// -1906.829  <=>  ((({i.vao_attr4.z : 20.30853} * {i.vao_attr9.z : 0.0632}) + (({i.vao_attr4.y : -10.90624} * {i.vao_attr9.y : 0.00}) + ({i.vao_attr4.x : 6.81513} * {i.vao_attr9.x : 0.998}))) + {i.vao_attr9.w : -1914.914})
	pf_8_7 = (((i.vao_attr4.z * i.vao_attr9.z) + ((i.vao_attr4.y * i.vao_attr9.y) + (i.vao_attr4.x * i.vao_attr9.x))) + i.vao_attr9.w);
	// 0.42  <=>  {utof(vs_cbuf9[116].w) : 0.42}
	f_3_54 = utof(vs_cbuf9[116].w);
	// 1065353216  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_3_54 : 0.42}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_3_54 : 0.42}))) ? 1065353216u : 0u)
	u_20_0 = ((((utof(u_0_phi_20) >= f_3_54) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_3_54))) ? 1065353216u : 0u);
	// 0  <=>  0u
	u_21_0 = 0u;
	u_21_phi_21 = u_21_0;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 1  <=>  1u
		u_21_1 = 1u;
		u_21_phi_21 = u_21_1;
	}
	// 1065353214  <=>  ({ftou(f_6_12) : 1065353216} + 4294967294u)
	u_15_1 = (ftou(f_6_12) + 4294967294u);
	// 1065353214  <=>  ({ftou(f_6_15) : 1065353216} + 4294967294u)
	u_16_1 = (ftou(f_6_15) + 4294967294u);
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_10_6 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_19_1 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_10_6))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 0.00  <=>  clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_9_5 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f))
	f_3_66 = clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_9_5))) + 4294967294u)))), float(0.f), float(4294967300.f));
	// 0  <=>  uint({f_3_66 : 0.00})
	u_22_0 = uint(f_3_66);
	// 0  <=>  uint(clamp(trunc(({utof(u_15_1) : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
	u_23_0 = uint(clamp(trunc((utof(u_15_1) * float(0u))), float(0.f), float(4294967300.f)));
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(u_16_1) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_17_1 = uint(clamp(trunc((float(0u) * utof(u_16_1))), float(0.f), float(4294967300.f)));
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.x : 0.14555} > 0.5f) && (! myIsNaN({i.vao_attr7.x : 0.14555}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.x > 0.5f) && (! myIsNaN(i.vao_attr7.x))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_19_1 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_19_1 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_24_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_19_1), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_19_1), int(0u), int(16u))), int(16u), int(16u));
	// 0  <=>  (({u_24_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_19_1 : 0}), int(0u), int(16u))))))
	u_18_3 = ((u_24_3 << 16u) + uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_19_1), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_22_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_22_0 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_31_5 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_22_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_22_0), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_22_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_22_0 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_22_0 : 0}), int(0u), int(16u))))))
	u_25_4 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_22_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_22_0), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_22_0), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_23_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_17 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_26_5 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_23_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_23_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_17 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_23_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 1}), int(0u), int(16u))))))
	u_26_7 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_23_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_23_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_17_1 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_11_5 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_11_5 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_27_4 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_17_1), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_11_5), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_11_5), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_17_1 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_11_5 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_11_5 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_17_1 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_11_5 : 1}), int(0u), int(16u))))))
	u_27_6 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_17_1), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_11_5), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_11_5), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_17_1), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_11_5), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(16u), int(16u))) * {u_31_5 : 0})) << 16u) + {u_25_4 : 0}))))
	u_24_9 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_9_5), int(16u), int(16u))) * u_31_5)) << 16u) + u_25_4))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_23_0 : 0}), int(16u), int(16u))) * {u_26_5 : 1})) << 16u) + {u_26_7 : 0}))))
	u_25_9 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_23_0), int(16u), int(16u))) * u_26_5)) << 16u) + u_26_7))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_17_1 : 0}), int(16u), int(16u))) * {u_27_4 : 1})) << 16u) + {u_27_6 : 0}))))
	u_18_6 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_17_1), int(16u), int(16u))) * u_27_4)) << 16u) + u_27_6))));
	// 0.65  <=>  {utof(vs_cbuf9[117].w) : 0.65}
	f_9_15 = utof(vs_cbuf9[117].w);
	// 1065353216  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_9_15 : 0.65}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_9_15 : 0.65}))) ? 1065353216u : 0u)
	u_18_7 = ((((utof(u_0_phi_20) >= f_9_15) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_9_15))) ? 1065353216u : 0u);
	// 1051240557  <=>  {ftou(v.uv.x) : 1051240557}
	u_24_10 = ftou(v.uv.x);
	u_24_phi_23 = u_24_10;
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.x : 0.14555} > 0.5f) && (! myIsNaN({i.vao_attr7.x : 0.14555}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.x > 0.5f) && (! myIsNaN(i.vao_attr7.x))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1059826634  <=>  {ftou(((0.f - {v.uv.x : 0.32941}) + 1.f)) : 1059826634}
		u_24_11 = ftou(((0.f - v.uv.x) + 1.f));
		u_24_phi_23 = u_24_11;
	}
	// 1063905846  <=>  {ftou(v.uv.y) : 1063905846}
	u_25_11 = ftou(v.uv.y);
	u_25_phi_24 = u_25_11;
	// False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 0}) == 2u) ? 4294967295u : 0u)) | (~ (((({f_4_26 : 0.08906} > 0.5f) && (! myIsNaN({f_4_26 : 0.08906}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((f_4_26 > 0.5f) && (! myIsNaN(f_4_26))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1034989136  <=>  {ftou(((0.f - {v.uv.y : 0.91373}) + 1.f)) : 1034989136}
		u_25_12 = ftou(((0.f - v.uv.y) + 1.f));
		u_25_phi_24 = u_25_12;
	}
	// 1051240557  <=>  {ftou(v.uv.x) : 1051240557}
	u_20_1 = ftou(v.uv.x);
	u_20_phi_25 = u_20_1;
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_1_46 : 0.84634} > 0.5f) && (! myIsNaN({f_1_46 : 0.84634}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_1_46 > 0.5f) && (! myIsNaN(f_1_46))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1059826634  <=>  {ftou(((0.f - {v.uv.x : 0.32941}) + 1.f)) : 1059826634}
		u_20_2 = ftou(((0.f - v.uv.x) + 1.f));
		u_20_phi_25 = u_20_2;
	}
	// 1063905846  <=>  {ftou(v.uv.y) : 1063905846}
	u_14_1 = ftou(v.uv.y);
	u_14_phi_26 = u_14_1;
	// False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 0}) == 8u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.w : 0.62996} > 0.5f) && (! myIsNaN({i.vao_attr7.w : 0.62996}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.w > 0.5f) && (! myIsNaN(i.vao_attr7.w))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1034989136  <=>  {ftou(((0.f - {v.uv.y : 0.91373}) + 1.f)) : 1034989136}
		u_14_2 = ftou(((0.f - v.uv.y) + 1.f));
		u_14_phi_26 = u_14_2;
	}
	// 1041566479  <=>  {ftou(i.vao_attr7.x) : 1041566479}
	u_13_2 = ftou(i.vao_attr7.x);
	u_13_phi_27 = u_13_2;
	// True  <=>  if((({u_21_phi_21 : 1} == 1u) ? true : false))
	if(((u_21_phi_21 == 1u) ? true : false))
	{
		// 1035363607  <=>  {ftou(f_4_26) : 1035363607}
		u_13_3 = ftou(f_4_26);
		u_13_phi_27 = u_13_3;
	}
	// 1035363607  <=>  {ftou(f_4_26) : 1035363607}
	u_26_15 = ftou(f_4_26);
	u_26_phi_28 = u_26_15;
	// True  <=>  if((({u_21_phi_21 : 1} == 1u) ? true : false))
	if(((u_21_phi_21 == 1u) ? true : false))
	{
		// 1062775229  <=>  {ftou(f_1_46) : 1062775229}
		u_26_16 = ftou(f_1_46);
		u_26_phi_28 = u_26_16;
	}
	// 1062775229  <=>  {ftou(f_1_46) : 1062775229}
	u_27_7 = ftou(f_1_46);
	u_27_phi_29 = u_27_7;
	// True  <=>  if((({u_21_phi_21 : 1} == 1u) ? true : false))
	if(((u_21_phi_21 == 1u) ? true : false))
	{
		// 1041566479  <=>  {ftou(i.vao_attr7.x) : 1041566479}
		u_27_8 = ftou(i.vao_attr7.x);
		u_27_phi_29 = u_27_8;
	}
	// 1041566479  <=>  {ftou(i.vao_attr7.x) : 1041566479}
	u_28_4 = ftou(i.vao_attr7.x);
	u_28_phi_30 = u_28_4;
	// True  <=>  if((({u_21_phi_21 : 1} == 1u) ? true : false))
	if(((u_21_phi_21 == 1u) ? true : false))
	{
		// 1035363607  <=>  {ftou(f_4_26) : 1035363607}
		u_28_5 = ftou(f_4_26);
		u_28_phi_30 = u_28_5;
	}
	// 1035363607  <=>  {ftou(f_4_26) : 1035363607}
	u_29_4 = ftou(f_4_26);
	u_29_phi_31 = u_29_4;
	// True  <=>  if((({u_21_phi_21 : 1} == 1u) ? true : false))
	if(((u_21_phi_21 == 1u) ? true : false))
	{
		// 1062775229  <=>  {ftou(f_1_46) : 1062775229}
		u_29_5 = ftou(f_1_46);
		u_29_phi_31 = u_29_5;
	}
	// 1062775229  <=>  {u_29_phi_31 : 1062775229}
	u_30_4 = u_29_phi_31;
	// 1035363607  <=>  {u_13_phi_27 : 1035363607}
	u_31_6 = u_13_phi_27;
	// 1062775229  <=>  {u_26_phi_28 : 1062775229}
	u_32_1 = u_26_phi_28;
	// 1035363607  <=>  {u_28_phi_30 : 1035363607}
	u_33_0 = u_28_phi_30;
	// 1041566479  <=>  {u_27_phi_29 : 1041566479}
	u_34_0 = u_27_phi_29;
	u_30_phi_32 = u_30_4;
	u_31_phi_32 = u_31_6;
	u_32_phi_32 = u_32_1;
	u_33_phi_32 = u_33_0;
	u_34_phi_32 = u_34_0;
	// False  <=>  if(((! ({u_21_phi_21 : 1} == 1u)) ? true : false))
	if(((! (u_21_phi_21 == 1u)) ? true : false))
	{
		// 1035363607  <=>  {u_13_phi_27 : 1035363607}
		u_21_2 = u_13_phi_27;
		u_21_phi_33 = u_21_2;
		// False  <=>  if((({u_21_phi_21 : 1} == 2u) ? true : false))
		if(((u_21_phi_21 == 2u) ? true : false))
		{
			// 1062775229  <=>  {ftou(f_1_46) : 1062775229}
			u_21_3 = ftou(f_1_46);
			u_21_phi_33 = u_21_3;
		}
		// 1062775229  <=>  {u_26_phi_28 : 1062775229}
		u_7_16 = u_26_phi_28;
		u_7_phi_34 = u_7_16;
		// False  <=>  if((({u_21_phi_21 : 1} == 2u) ? true : false))
		if(((u_21_phi_21 == 2u) ? true : false))
		{
			// 1041566479  <=>  {ftou(i.vao_attr7.x) : 1041566479}
			u_7_17 = ftou(i.vao_attr7.x);
			u_7_phi_34 = u_7_17;
		}
		// 1041566479  <=>  {u_27_phi_29 : 1041566479}
		u_35_0 = u_27_phi_29;
		u_35_phi_35 = u_35_0;
		// False  <=>  if((({u_21_phi_21 : 1} == 2u) ? true : false))
		if(((u_21_phi_21 == 2u) ? true : false))
		{
			// 1035363607  <=>  {ftou(f_4_26) : 1035363607}
			u_35_1 = ftou(f_4_26);
			u_35_phi_35 = u_35_1;
		}
		// 1035363607  <=>  {u_28_phi_30 : 1035363607}
		u_36_0 = u_28_phi_30;
		u_36_phi_36 = u_36_0;
		// False  <=>  if((({u_21_phi_21 : 1} == 2u) ? true : false))
		if(((u_21_phi_21 == 2u) ? true : false))
		{
			// 1041566479  <=>  {ftou(i.vao_attr7.x) : 1041566479}
			u_36_1 = ftou(i.vao_attr7.x);
			u_36_phi_36 = u_36_1;
		}
		// 1062775229  <=>  {u_29_phi_31 : 1062775229}
		u_1_7 = u_29_phi_31;
		u_1_phi_37 = u_1_7;
		// False  <=>  if((({u_21_phi_21 : 1} == 2u) ? true : false))
		if(((u_21_phi_21 == 2u) ? true : false))
		{
			// 1035363607  <=>  {ftou(f_4_26) : 1035363607}
			u_1_8 = ftou(f_4_26);
			u_1_phi_37 = u_1_8;
		}
		// 1062775229  <=>  {u_1_phi_37 : 1062775229}
		u_30_5 = u_1_phi_37;
		// 1035363607  <=>  {u_21_phi_33 : 1035363607}
		u_31_7 = u_21_phi_33;
		// 1062775229  <=>  {u_7_phi_34 : 1062775229}
		u_32_2 = u_7_phi_34;
		// 1035363607  <=>  {u_36_phi_36 : 1035363607}
		u_33_1 = u_36_phi_36;
		// 1041566479  <=>  {u_35_phi_35 : 1041566479}
		u_34_1 = u_35_phi_35;
		u_30_phi_32 = u_30_5;
		u_31_phi_32 = u_31_7;
		u_32_phi_32 = u_32_2;
		u_33_phi_32 = u_33_1;
		u_34_phi_32 = u_34_1;
	}
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_1_40 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_10_6), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_10_6 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_24_3 : 0}), int(16u), int(16u))))) << 16u) + {u_18_3 : 0}))))))), float(0.f), float(4294967300.f)))
	u_6_21 = uint(clamp(trunc((utof((ftou((1.0f / float(u_10_6))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_10_6), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_24_3), int(16u), int(16u))))) << 16u) + u_18_3))))))), float(0.f), float(4294967300.f)));
	// 0.00  <=>  clamp(trunc(({utof(({ftou((1.0f / float({u_9_5 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float({u_24_9 : 0}))), float(0.f), float(4294967300.f))
	f_2_64 = clamp(trunc((utof((ftou((1.0f / float(u_9_5))) + 4294967294u)) * float(u_24_9))), float(0.f), float(4294967300.f));
	// 0.83  <=>  {utof(vs_cbuf9[118].w) : 0.83}
	f_2_65 = utof(vs_cbuf9[118].w);
	// 1065353216  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_2_65 : 0.83}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_2_65 : 0.83}))) ? 1065353216u : 0u)
	u_7_18 = ((((utof(u_0_phi_20) >= f_2_65) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_2_65))) ? 1065353216u : 0u);
	// 1.00  <=>  {utof(vs_cbuf9[119].w) : 1.00}
	f_2_70 = utof(vs_cbuf9[119].w);
	// 0  <=>  (((({utof(u_0_phi_20) : 0.995} >= {f_2_70 : 1.00}) && (! myIsNaN({utof(u_0_phi_20) : 0.995}))) && (! myIsNaN({f_2_70 : 1.00}))) ? 1065353216u : 0u)
	u_0_3 = ((((utof(u_0_phi_20) >= f_2_70) && (! myIsNaN(utof(u_0_phi_20)))) && (! myIsNaN(f_2_70))) ? 1065353216u : 0u);
	// 0  <=>  ({u_19_1 : 0} + {u_6_21 : 0})
	u_6_22 = (u_19_1 + u_6_21);
	// -11.55322  <=>  ({pf_7_8 : 354.1841} + (0.f - {(camera_wpos.y) : 365.7373}))
	pf_19_1 = (pf_7_8 + (0.f - (camera_wpos.y)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_21 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_10_6), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_2_14 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_9_5), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0.00  <=>  (((((({utof(vs_cbuf9[118].x) : 0.25} + (0.f - {utof(vs_cbuf9[117].x) : 0.80})) * (1.0f / ((0.f - {utof(vs_cbuf9[117].w) : 0.65}) + {utof(vs_cbuf9[118].w) : 0.83}))) * ({utof(u_0_phi_20) : 0.995} + (0.f - {utof(vs_cbuf9[117].w) : 0.65}))) + {utof(vs_cbuf9[117].x) : 0.80}) * (({utof(u_18_7) : 1.00} * (0.f - {utof(u_7_18) : 1.00})) + {utof(u_18_7) : 1.00})) + (((((((0.f - {utof(vs_cbuf9[116].x) : 1.00}) + {utof(vs_cbuf9[117].x) : 0.80}) * (1.0f / ({utof(vs_cbuf9[117].w) : 0.65} + (0.f - {utof(vs_cbuf9[116].w) : 0.42})))) * ({utof(u_0_phi_20) : 0.995} + (0.f - {utof(vs_cbuf9[116].w) : 0.42}))) + {utof(vs_cbuf9[116].x) : 1.00}) * (({utof(u_20_0) : 1.00} * (0.f - {utof(u_18_7) : 1.00})) + {utof(u_20_0) : 1.00})) + (((((({utof(vs_cbuf9[116].x) : 1.00} + (0.f - {utof(vs_cbuf9[115].x) : 0.80})) * (1.0f / ((0.f - {utof(vs_cbuf9[115].w) : 0.23}) + {utof(vs_cbuf9[116].w) : 0.42}))) * ({utof(u_0_phi_20) : 0.995} + (0.f - {utof(vs_cbuf9[115].w) : 0.23}))) + {utof(vs_cbuf9[115].x) : 0.80}) * (({utof(u_18_0) : 1.00} * (0.f - {utof(u_20_0) : 1.00})) + {utof(u_18_0) : 1.00})) + (((((((0.f - {utof(vs_cbuf9[114].x) : 0.25}) + {utof(vs_cbuf9[115].x) : 0.80}) * (1.0f / ({utof(vs_cbuf9[115].w) : 0.23} + (0.f - {utof(vs_cbuf9[114].w) : 0.11})))) * ({utof(u_0_phi_20) : 0.995} + (0.f - {utof(vs_cbuf9[114].w) : 0.11}))) + {utof(vs_cbuf9[114].x) : 0.25}) * (({utof(u_17_0) : 1.00} * (0.f - {utof(u_18_0) : 1.00})) + {utof(u_17_0) : 1.00})) + (((((({utof(vs_cbuf9[114].x) : 0.25} + (0.f - {utof(vs_cbuf9[113].x) : 0.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[113].w) : 0.00}) + {utof(vs_cbuf9[114].w) : 0.11}))) * ({utof(u_0_phi_20) : 0.995} + (0.f - {utof(vs_cbuf9[113].w) : 0.00}))) + {utof(vs_cbuf9[113].x) : 0.00}) * (({utof(u_17_0) : 1.00} * (0.f - {utof(u_19_0) : 1.00})) + {utof(u_19_0) : 1.00})) + (({utof(u_19_0) : 1.00} * (0.f - {utof(vs_cbuf9[113].x) : 0.00})) + {utof(vs_cbuf9[113].x) : 0.00}))))))
	pf_10_10 = ((((((utof(vs_cbuf9[118].x) + (0.f - utof(vs_cbuf9[117].x))) * (1.0f / ((0.f - utof(vs_cbuf9[117].w)) + utof(vs_cbuf9[118].w)))) * (utof(u_0_phi_20) + (0.f - utof(vs_cbuf9[117].w)))) + utof(vs_cbuf9[117].x)) * ((utof(u_18_7) * (0.f - utof(u_7_18))) + utof(u_18_7))) + (((((((0.f - utof(vs_cbuf9[116].x)) + utof(vs_cbuf9[117].x)) * (1.0f / (utof(vs_cbuf9[117].w) + (0.f - utof(vs_cbuf9[116].w))))) * (utof(u_0_phi_20) + (0.f - utof(vs_cbuf9[116].w)))) + utof(vs_cbuf9[116].x)) * ((utof(u_20_0) * (0.f - utof(u_18_7))) + utof(u_20_0))) + ((((((utof(vs_cbuf9[116].x) + (0.f - utof(vs_cbuf9[115].x))) * (1.0f / ((0.f - utof(vs_cbuf9[115].w)) + utof(vs_cbuf9[116].w)))) * (utof(u_0_phi_20) + (0.f - utof(vs_cbuf9[115].w)))) + utof(vs_cbuf9[115].x)) * ((utof(u_18_0) * (0.f - utof(u_20_0))) + utof(u_18_0))) + (((((((0.f - utof(vs_cbuf9[114].x)) + utof(vs_cbuf9[115].x)) * (1.0f / (utof(vs_cbuf9[115].w) + (0.f - utof(vs_cbuf9[114].w))))) * (utof(u_0_phi_20) + (0.f - utof(vs_cbuf9[114].w)))) + utof(vs_cbuf9[114].x)) * ((utof(u_17_0) * (0.f - utof(u_18_0))) + utof(u_17_0))) + ((((((utof(vs_cbuf9[114].x) + (0.f - utof(vs_cbuf9[113].x))) * (1.0f / ((0.f - utof(vs_cbuf9[113].w)) + utof(vs_cbuf9[114].w)))) * (utof(u_0_phi_20) + (0.f - utof(vs_cbuf9[113].w)))) + utof(vs_cbuf9[113].x)) * ((utof(u_17_0) * (0.f - utof(u_19_0))) + utof(u_19_0))) + ((utof(u_19_0) * (0.f - utof(vs_cbuf9[113].x))) + utof(vs_cbuf9[113].x)))))));
	// 0  <=>  ({u_22_0 : 0} + uint({f_2_64 : 0.00}))
	u_2_13 = (u_22_0 + uint(f_2_64));
	// 0  <=>  ({u_17_1 : 0} + uint(clamp(trunc(({utof(u_16_1) : 0.9999999} * float({u_18_6 : 0}))), float(0.f), float(4294967300.f))))
	u_13_5 = (u_17_1 + uint(clamp(trunc((utof(u_16_1) * float(u_18_6))), float(0.f), float(4294967300.f))));
	// 0  <=>  ({u_23_0 : 0} + uint(clamp(trunc(({utof(u_15_1) : 0.9999999} * float({u_25_9 : 0}))), float(0.f), float(4294967300.f))))
	u_12_6 = (u_23_0 + uint(clamp(trunc((utof(u_15_1) * float(u_25_9))), float(0.f), float(4294967300.f))));
	// 12.43286  <=>  ({pf_8_7 : -1906.829} + (0.f - {(camera_wpos.x) : -1919.262}))
	pf_12_11 = (pf_8_7 + (0.f - (camera_wpos.x)));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_22 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_6_22 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_21_4 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_22), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_6_22), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_22 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_6_22 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_22 : 0}), int(0u), int(16u))))))
	u_15_4 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_22), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_6_22), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_22), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_2_13 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_2_13 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_26_17 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_2_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_2_13), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_2_13 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_2_13 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_2_13 : 0}), int(0u), int(16u))))))
	u_17_4 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_2_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_2_13), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_2_13), int(0u), int(16u))))));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_42 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_9_5), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_10_6 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_3_4 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_10_6), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_12_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_17 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_26_18 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_12_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_12_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_17 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
	u_21_9 = (bitfieldInsert(uint((uint(bitfieldExtract(uint(u_12_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u))), int(16u), int(16u)) << 16u);
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_12_6 : 0}), int(16u), int(16u))) * {u_26_18 : 1})) << 16u) + ({u_21_9 : 0} + uint((uint(bitfieldExtract(uint({u_12_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_17 : 1}), int(0u), int(16u)))))))
	u_19_11 = ((uint((uint(bitfieldExtract(uint(u_12_6), int(16u), int(16u))) * u_26_18)) << 16u) + (u_21_9 + uint((uint(bitfieldExtract(uint(u_12_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_17), int(0u), int(16u)))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_13_5 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_11_5 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_11_5 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_23_9 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_13_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_11_5), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_11_5), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_13_5 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_11_5 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_11_5 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
	u_22_6 = (bitfieldInsert(uint((uint(bitfieldExtract(uint(u_13_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_11_5), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_11_5), int(0u), int(16u))), int(16u), int(16u)) << 16u);
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_13_5 : 0}), int(16u), int(16u))) * {u_23_9 : 1})) << 16u) + ({u_22_6 : 0} + uint((uint(bitfieldExtract(uint({u_13_5 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_11_5 : 1}), int(0u), int(16u)))))))
	u_16_10 = ((uint((uint(bitfieldExtract(uint(u_13_5), int(16u), int(16u))) * u_23_9)) << 16u) + (u_22_6 + uint((uint(bitfieldExtract(uint(u_13_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_11_5), int(0u), int(16u)))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(16u), int(16u))) * {u_21_4 : 0})) << 16u) + {u_15_4 : 0}))))
	u_15_6 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_10_6), int(16u), int(16u))) * u_21_4)) << 16u) + u_15_4))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(16u), int(16u))) * {u_26_17 : 0})) << 16u) + {u_17_4 : 0}))))
	u_17_6 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_9_5), int(16u), int(16u))) * u_26_17)) << 16u) + u_17_4))));
	// 0  <=>  uint((int(0) - int({u_19_11 : 0})))
	u_19_12 = uint((int(0) - int(u_19_11)));
	// 24.2605  <=>  (((({i.vao_attr4.z : 20.30853} * {i.vao_attr11.z : 0.998}) + (({i.vao_attr4.y : -10.90624} * {i.vao_attr11.y : 0.00}) + ({i.vao_attr4.x : 6.81513} * {i.vao_attr11.x : -0.0632}))) + {i.vao_attr11.w : -3728.624}) + (0.f - {(camera_wpos.z) : -3733.047}))
	pf_18_6 = ((((i.vao_attr4.z * i.vao_attr11.z) + ((i.vao_attr4.y * i.vao_attr11.y) + (i.vao_attr4.x * i.vao_attr11.x))) + i.vao_attr11.w) + (0.f - (camera_wpos.z)));
	// 0  <=>  ((uint({u_15_6 : 0}) >= uint({u_10_6 : 1})) ? 4294967295u : 0u)
	u_15_7 = ((uint(u_15_6) >= uint(u_10_6)) ? 4294967295u : 0u);
	// 0.0073529  <=>  (({utof(u_0_3) : 0.00} * {utof(vs_cbuf9[119].x) : 0.00}) + (((((((0.f - {utof(vs_cbuf9[118].x) : 0.25}) + {utof(vs_cbuf9[119].x) : 0.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[118].w) : 0.83}) + {utof(vs_cbuf9[119].w) : 1.00}))) * ({utof(u_0_phi_20) : 0.995} + (0.f - {utof(vs_cbuf9[118].w) : 0.83}))) + {utof(vs_cbuf9[118].x) : 0.25}) * (({utof(u_7_18) : 1.00} * (0.f - {utof(u_0_3) : 0.00})) + {utof(u_7_18) : 1.00})) + {pf_10_10 : 0.00}))
	pf_10_12 = ((utof(u_0_3) * utof(vs_cbuf9[119].x)) + (((((((0.f - utof(vs_cbuf9[118].x)) + utof(vs_cbuf9[119].x)) * (1.0f / ((0.f - utof(vs_cbuf9[118].w)) + utof(vs_cbuf9[119].w)))) * (utof(u_0_phi_20) + (0.f - utof(vs_cbuf9[118].w)))) + utof(vs_cbuf9[118].x)) * ((utof(u_7_18) * (0.f - utof(u_0_3))) + utof(u_7_18))) + pf_10_10));
	// 0  <=>  ((uint({u_17_6 : 0}) >= uint({u_9_5 : 1})) ? 4294967295u : 0u)
	u_0_4 = ((uint(u_17_6) >= uint(u_9_5)) ? 4294967295u : 0u);
	// 0  <=>  uint((int(0) - int({u_16_10 : 0})))
	u_16_11 = uint((int(0) - int(u_16_10)));
	// 0  <=>  (uint((int(0) - int({u_6_22 : 0}))) + {u_15_7 : 0})
	u_6_24 = (uint((int(0) - int(u_6_22))) + u_15_7);
	// 0  <=>  (uint((int(0) - int({u_2_13 : 0}))) + {u_0_4 : 0})
	u_0_5 = (uint((int(0) - int(u_2_13))) + u_0_4);
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_10_6 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_2_16 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_10_6), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_9_5 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_4_8 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_9_5), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(bitfieldExtract(uint({u_6_24 : 0}), int(16u), int(16u)))
	u_17_7 = uint(bitfieldExtract(uint(u_6_24), int(16u), int(16u)));
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.x = 0.f;
	}
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  uint(bitfieldExtract(uint({u_0_5 : 0}), int(16u), int(16u)))
	u_19_13 = uint(bitfieldExtract(uint(u_0_5), int(16u), int(16u)));
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 0.00  <=>  0.f
		o.fs_attr4.x = 0.f;
	}
	// -7.197703  <=>  (0.f - (({pf_0_1 : 199.00} * {utof(vs_cbuf9[82].x) : 0.00}) + (({utof(u_34_phi_32) : 0.14555} * {utof(vs_cbuf9[82].z) : 6.283185}) + ({utof(vs_cbuf9[82].z) : 6.283185} + {utof(vs_cbuf9[82].y) : 0.00}))))
	f_5_22 = (0.f - ((pf_0_1 * utof(vs_cbuf9[82].x)) + ((utof(u_34_phi_32) * utof(vs_cbuf9[82].z)) + (utof(vs_cbuf9[82].z) + utof(vs_cbuf9[82].y)))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * {u_17_7 : 0})), uint(bitfieldExtract(uint({u_6_24 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_13_6 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * u_17_7)), uint(bitfieldExtract(uint(u_6_24), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * {u_17_7 : 0})), uint(bitfieldExtract(uint({u_6_24 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_24 : 0}), int(0u), int(16u))))))
	u_6_28 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * u_17_7)), uint(bitfieldExtract(uint(u_6_24), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_10_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_24), int(0u), int(16u))))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].w) : 1.00}) * {utof(vs_cbuf9[83].y) : 1.00})
	pf_13_12 = ((1.0f / utof(vs_cbuf9[83].w)) * utof(vs_cbuf9[83].y));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * {u_19_13 : 0})), uint(bitfieldExtract(uint({u_0_5 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_13_7 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * u_19_13)), uint(bitfieldExtract(uint(u_0_5), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * {u_19_13 : 0})), uint(bitfieldExtract(uint({u_0_5 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_0_5 : 0}), int(0u), int(16u))))))
	u_0_9 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * u_19_13)), uint(bitfieldExtract(uint(u_0_5), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_5), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_0_5), int(0u), int(16u))))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].z) : 1.00}) * {utof(vs_cbuf9[83].x) : 1.00})
	pf_14_7 = ((1.0f / utof(vs_cbuf9[83].z)) * utof(vs_cbuf9[83].x));
	// 0  <=>  (({b_1_40 : False} || {b_0_21 : True}) ? ((uint((uint(bitfieldExtract(uint({u_10_6 : 1}), int(16u), int(16u))) * {u_13_6 : 0})) << 16u) + {u_6_28 : 0}) : 4294967295u)
	u_6_30 = ((b_1_40 || b_0_21) ? ((uint((uint(bitfieldExtract(uint(u_10_6), int(16u), int(16u))) * u_13_6)) << 16u) + u_6_28) : 4294967295u);
	// 0  <=>  uint((int(0) - int(({u_9_5 : 1} >> 31u))))
	u_10_7 = uint((int(0) - int((u_9_5 >> 31u))));
	// 0  <=>  uint((int(0) - int(({u_10_6 : 1} >> 31u))))
	u_6_31 = uint((int(0) - int((u_10_6 >> 31u))));
	// 1.00  <=>  {v.coeff.x : 1.00}
	o.fs_attr5.x = v.coeff.x;
	// 0  <=>  (({b_2_14 : False} || {b_1_42 : True}) ? ((uint((uint(bitfieldExtract(uint({u_9_5 : 1}), int(16u), int(16u))) * {u_13_7 : 0})) << 16u) + {u_0_9 : 0}) : 4294967295u)
	u_0_11 = ((b_2_14 || b_1_42) ? ((uint((uint(bitfieldExtract(uint(u_9_5), int(16u), int(16u))) * u_13_7)) << 16u) + u_0_9) : 4294967295u);
	// 1.00  <=>  {v.coeff.z : 1.00}
	o.fs_attr5.z = v.coeff.z;
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_9_5 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_23 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_9_5), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// 1.00  <=>  {v.coeff.y : 1.00}
	o.fs_attr5.y = v.coeff.y;
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].z) : 1.00}) * {utof(vs_cbuf9[78].x) : 1.00})
	pf_23_0 = ((1.0f / utof(vs_cbuf9[78].z)) * utof(vs_cbuf9[78].x));
	// 0  <=>  (uint((int(0) - int({u_10_7 : 0}))) + (({u_13_5 : 0} + uint((int(0) - int(((uint({u_16_11 : 0}) >= uint({u_11_5 : 1})) ? 4294967295u : 0u))))) ^ {u_10_7 : 0}))
	u_9_11 = (uint((int(0) - int(u_10_7))) + ((u_13_5 + uint((int(0) - int(((uint(u_16_11) >= uint(u_11_5)) ? 4294967295u : 0u))))) ^ u_10_7));
	// 0  <=>  (uint((int(0) - int({u_6_31 : 0}))) + (({u_12_6 : 0} + uint((int(0) - int(((uint({u_19_12 : 0}) >= uint({u_8_17 : 1})) ? 4294967295u : 0u))))) ^ {u_6_31 : 0}))
	u_6_33 = (uint((int(0) - int(u_6_31))) + ((u_12_6 + uint((int(0) - int(((uint(u_19_12) >= uint(u_8_17)) ? 4294967295u : 0u))))) ^ u_6_31));
	// 2.10  <=>  ({utof(vs_cbuf9[129].x) : 2.10} * {(vs_cbuf10_1.w) : 1.00})
	o.fs_attr1.w = (utof(vs_cbuf9[129].x) * (vs_cbuf10_1.w));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].w) : 1.00}) * {utof(vs_cbuf9[78].y) : 1.00})
	pf_27_0 = ((1.0f / utof(vs_cbuf9[78].w)) * utof(vs_cbuf9[78].y));
	// 0  <=>  (({u_12_6 : 0} + uint((int(0) - int(((uint({u_19_12 : 0}) >= uint({u_8_17 : 1})) ? 4294967295u : 0u))))) ^ {u_6_31 : 0})
	u_6_35 = ((u_12_6 + uint((int(0) - int(((uint(u_19_12) >= uint(u_8_17)) ? 4294967295u : 0u))))) ^ u_6_31);
	u_6_phi_41 = u_6_35;
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_6_36 = ftou(vs_cbuf8_30.y);
		u_6_phi_41 = u_6_36;
	}
	// 1.093333  <=>  (({utof(vs_cbuf9[105].x) : 0.6507937} * {(vs_cbuf10_0.x) : 1.20}) * {utof(vs_cbuf9[104].x) : 1.40})
	o.fs_attr0.x = ((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x));
	// 1  <=>  {u_5_phi_9 : 1}
	u_8_22 = u_5_phi_9;
	u_8_phi_42 = u_8_22;
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 0  <=>  0u
		u_8_23 = 0u;
		u_8_phi_42 = u_8_23;
	}
	// 0.0337748  <=>  inversesqrt((({pf_18_6 : 24.2605} * {pf_18_6 : 24.2605}) + (({pf_19_1 : -11.55322} * {pf_19_1 : -11.55322}) + ({pf_12_11 : 12.43286} * {pf_12_11 : 12.43286}))))
	f_2_108 = inversesqrt(((pf_18_6 * pf_18_6) + ((pf_19_1 * pf_19_1) + (pf_12_11 * pf_12_11))));
	// 0  <=>  {u_6_phi_41 : 0}
	u_5_4 = u_6_phi_41;
	u_5_phi_43 = u_5_4;
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_6_phi_41) : 0.00} * 5.f)) : 0}
		u_5_5 = ftou((utof(u_6_phi_41) * 5.f));
		u_5_phi_43 = u_5_5;
	}
	// False  <=>  if((((((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194}))) >= 0.f) && (! myIsNaN((({pf_18_6 : 24.2605} * {(vs_cbuf8_28.z) : -0.6398518}) + (({pf_19_1 : -11.55322} * {(vs_cbuf8_28.y) : 0.5074672}) + ({pf_12_11 : 12.43286} * {(vs_cbuf8_28.x) : -0.5771194})))))) && (! myIsNaN(0.f))) ? true : false))
	if(((((((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x)))) >= 0.f) && (! myIsNaN(((pf_18_6 * (vs_cbuf8_28.z)) + ((pf_19_1 * (vs_cbuf8_28.y)) + (pf_12_11 * (vs_cbuf8_28.x))))))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 0.00  <=>  {utof(u_5_phi_43) : 0.00}
		o.vertex.z = utof(u_5_phi_43);
	}
	// 0.9631746  <=>  (({utof(vs_cbuf9[105].z) : 0.5733182} * {(vs_cbuf10_0.z) : 1.20}) * {utof(vs_cbuf9[104].x) : 1.40})
	o.fs_attr0.z = ((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x));
	// 1.073861  <=>  (({utof(vs_cbuf9[105].y) : 0.6392028} * {(vs_cbuf10_0.y) : 1.20}) * {utof(vs_cbuf9[104].x) : 1.40})
	o.fs_attr0.y = ((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x));
	// 1.165791  <=>  ((((clamp(min(0.f, {i.vao_attr7.x : 0.14555}), 0.0, 1.0) + {i.vao_attr6.x : 3.31041}) * {pf_3_14 : 1.408636}) * {(vs_cbuf10_3.y) : 1.00}) * {utof(vs_cbuf9[16].w) : 0.25})
	pf_14_10 = ((((clamp(min(0.f, i.vao_attr7.x), 0.0, 1.0) + i.vao_attr6.x) * pf_3_14) * (vs_cbuf10_3.y)) * utof(vs_cbuf9[16].w));
	// 0.0014706  <=>  ({pf_10_12 : 0.0073529} * {(vs_cbuf10_0.w) : 0.20})
	o.fs_attr0.w = (pf_10_12 * (vs_cbuf10_0.w));
	// 0.7460654  <=>  (((((cos({f_5_22 : -7.197703}) * (({pf_14_7 : 1.00} * {utof(u_20_phi_25) : 0.32941}) + -0.5f)) + (0.f - ((({pf_13_12 : 1.00} * {utof(u_14_phi_26) : 0.91373}) + -0.5f) * sin({f_5_22 : -7.197703})))) * (({pf_0_1 : 199.00} * {utof(vs_cbuf9[80].z) : 0.00}) + (({utof(u_33_phi_32) : 0.08906} * {utof(vs_cbuf9[81].z) : 0.00}) + ({utof(vs_cbuf9[81].z) : 0.00} + {utof(vs_cbuf9[81].x) : 1.10})))) + (({pf_14_7 : 1.00} * float(int({u_0_11 : 0}))) + (({pf_0_1 : 199.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.00})) + (0.f - ((({utof(u_31_phi_32) : 0.08906} * {utof(vs_cbuf9[80].x) : 0.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 0.00} + {utof(vs_cbuf9[79].z) : 0.00})))))) + 0.5f)
	o.fs_attr2.z = (((((cos(f_5_22) * ((pf_14_7 * utof(u_20_phi_25)) + -0.5f)) + (0.f - (((pf_13_12 * utof(u_14_phi_26)) + -0.5f) * sin(f_5_22)))) * ((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_33_phi_32) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].z) + utof(vs_cbuf9[81].x))))) + ((pf_14_7 * float(int(u_0_11))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_31_phi_32) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))) + 0.5f);
	// -0.3647434  <=>  ((((({pf_0_1 : 199.00} * {utof(vs_cbuf9[75].z) : 0.001}) + (({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[76].z) : 0.10}) + ({utof(vs_cbuf9[76].x) : 0.60} + {utof(vs_cbuf9[76].z) : 0.10}))) * (({pf_23_0 : 1.00} * {utof(u_24_phi_23) : 0.32941}) + -0.5f)) + (({pf_23_0 : 1.00} * float(int({u_6_30 : 0}))) + (({pf_0_1 : 199.00} * (0.f - {utof(vs_cbuf9[74].x) : 0.00})) + (0.f - ((({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0.00})))))) + 0.5f)
	o.fs_attr2.x = (((((pf_0_1 * utof(vs_cbuf9[75].z)) + ((i.vao_attr7.x * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z)))) * ((pf_23_0 * utof(u_24_phi_23)) + -0.5f)) + ((pf_23_0 * float(int(u_6_30))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((i.vao_attr7.x * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f);
	// 0.9263599  <=>  ((((((({pf_14_7 : 1.00} * {utof(u_20_phi_25) : 0.32941}) + -0.5f) * sin({f_5_22 : -7.197703})) + ((({pf_13_12 : 1.00} * {utof(u_14_phi_26) : 0.91373}) + -0.5f) * cos({f_5_22 : -7.197703}))) * (({pf_0_1 : 199.00} * {utof(vs_cbuf9[80].w) : 0.00}) + (({utof(u_30_phi_32) : 0.84634} * {utof(vs_cbuf9[81].w) : 0.00}) + ({utof(vs_cbuf9[81].w) : 0.00} + {utof(vs_cbuf9[81].y) : 1.10})))) + (0.f - (({pf_13_12 : 1.00} * (0.f - float(int((({b_4_8 : False} || {b_0_23 : True}) ? {u_9_11 : 0} : 4294967295u))))) + (({pf_0_1 : 199.00} * {utof(vs_cbuf9[79].y) : 0.00}) + ((({utof(u_32_phi_32) : 0.84634} * {utof(vs_cbuf9[80].y) : 0.00}) * -2.f) + ({utof(vs_cbuf9[80].y) : 0.00} + {utof(vs_cbuf9[79].w) : 0.00})))))) + 0.5f)
	o.fs_attr2.w = (((((((pf_14_7 * utof(u_20_phi_25)) + -0.5f) * sin(f_5_22)) + (((pf_13_12 * utof(u_14_phi_26)) + -0.5f) * cos(f_5_22))) * ((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_30_phi_32) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y))))) + (0.f - ((pf_13_12 * (0.f - float(int(((b_4_8 || b_0_23) ? u_9_11 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_32_phi_32) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f);
	// -0.0936863  <=>  ((((({pf_0_1 : 199.00} * {utof(vs_cbuf9[75].w) : 0.001}) + (({f_4_26 : 0.08906} * {utof(vs_cbuf9[76].w) : 0.05}) + ({utof(vs_cbuf9[76].w) : 0.05} + {utof(vs_cbuf9[76].y) : 0.25}))) * (({pf_27_0 : 1.00} * {utof(u_25_phi_24) : 0.91373}) + -0.5f)) + (0.f - (({pf_27_0 : 1.00} * (0.f - float(int((({b_3_4 : False} || {b_2_16 : True}) ? {u_6_33 : 0} : 4294967295u))))) + (({pf_0_1 : 199.00} * {utof(vs_cbuf9[74].y) : -0.0001}) + ((({f_4_26 : 0.08906} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0.00})))))) + 0.5f)
	o.fs_attr2.y = (((((pf_0_1 * utof(vs_cbuf9[75].w)) + ((f_4_26 * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)))) * ((pf_27_0 * utof(u_25_phi_24)) + -0.5f)) + (0.f - ((pf_27_0 * (0.f - float(int(((b_3_4 || b_2_16) ? u_6_33 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((f_4_26 * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f);
	// False  <=>  if(((! ({u_8_phi_42 : 1} != 0u)) ? true : false))
	if(((! (u_8_phi_42 != 0u)) ? true : false))
	{
		return;
	}
	// -1907.319  <=>  ({pf_8_7 : -1906.829} + ({pf_14_10 : 1.165791} * ({pf_12_11 : 12.43286} * (0.f - {f_2_108 : 0.0337748}))))
	pf_17_10 = (pf_8_7 + (pf_14_10 * (pf_12_11 * (0.f - f_2_108))));
	// 354.639  <=>  ({pf_7_8 : 354.1841} + ({pf_14_10 : 1.165791} * ({pf_19_1 : -11.55322} * (0.f - {f_2_108 : 0.0337748}))))
	pf_14_11 = (pf_7_8 + (pf_14_10 * (pf_19_1 * (0.f - f_2_108))));
	// -3709.742  <=>  (((({i.vao_attr4.z : 20.30853} * {i.vao_attr11.z : 0.998}) + (({i.vao_attr4.y : -10.90624} * {i.vao_attr11.y : 0.00}) + ({i.vao_attr4.x : 6.81513} * {i.vao_attr11.x : -0.0632}))) + {i.vao_attr11.w : -3728.624}) + ({pf_14_10 : 1.165791} * ({pf_18_6 : 24.2605} * (0.f - {f_2_108 : 0.0337748}))))
	pf_21_2 = ((((i.vao_attr4.z * i.vao_attr11.z) + ((i.vao_attr4.y * i.vao_attr11.y) + (i.vao_attr4.x * i.vao_attr11.x))) + i.vao_attr11.w) + (pf_14_10 * (pf_18_6 * (0.f - f_2_108))));
	// -27.43664  <=>  ((({pf_21_2 : -3709.742} + (0.f - {(camera_wpos.z) : -3733.047})) * {(vs_cbuf8_28.z) : -0.6398518}) + ((({pf_14_11 : 354.639} + (0.f - {(camera_wpos.y) : 365.7373})) * {(vs_cbuf8_28.y) : 0.5074672}) + (({pf_17_10 : -1907.319} + (0.f - {(camera_wpos.x) : -1919.262})) * {(vs_cbuf8_28.x) : -0.5771194})))
	pf_20_9 = (((pf_21_2 + (0.f - (camera_wpos.z))) * (vs_cbuf8_28.z)) + (((pf_14_11 + (0.f - (camera_wpos.y))) * (vs_cbuf8_28.y)) + ((pf_17_10 + (0.f - (camera_wpos.x))) * (vs_cbuf8_28.x))));
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 268435456u)
	u_5_6 = (vs_cbuf9_7_x & 268435456u);
	// False  <=>  ((({pf_20_9 : -27.43664} >= 0.f) && (! myIsNaN({pf_20_9 : -27.43664}))) && (! myIsNaN(0.f)))
	b_1_45 = (((pf_20_9 >= 0.f) && (! myIsNaN(pf_20_9))) && (! myIsNaN(0.f)));
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 536870912u)
	u_9_15 = (vs_cbuf9_7_x & 536870912u);
	// False  <=>  ({b_1_45 : False} ? true : false)
	b_2_20 = (b_1_45 ? true : false);
	// False  <=>  if({b_2_20 : False})
	if(b_2_20)
	{
		// 0.00  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 1073741824u)
	u_3_5 = (vs_cbuf9_7_x & 1073741824u);
	// False  <=>  if(({b_1_45 : False} ? true : false))
	if((b_1_45 ? true : false))
	{
		// 0.00  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 1  <=>  {u_8_phi_42 : 1}
	u_10_9 = u_8_phi_42;
	u_10_phi_48 = u_10_9;
	// False  <=>  if(({b_1_45 : False} ? true : false))
	if((b_1_45 ? true : false))
	{
		// 0  <=>  0u
		u_10_10 = 0u;
		u_10_phi_48 = u_10_10;
	}
	// False  <=>  if(({b_1_45 : False} ? true : false))
	if((b_1_45 ? true : false))
	{
		// 0.00  <=>  0.f
		o.fs_attr4.x = 0.f;
	}
	// 0  <=>  ((int({u_5_6 : 0}) > int(0u)) ? 4294967295u : 0u)
	u_8_25 = ((int(u_5_6) > int(0u)) ? 4294967295u : 0u);
	u_8_phi_50 = u_8_25;
	// False  <=>  if(({b_1_45 : False} ? true : false))
	if((b_1_45 ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_8_26 = ftou(vs_cbuf8_30.y);
		u_8_phi_50 = u_8_26;
	}
	// 0.00  <=>  floor(({i.vao_attr7.x : 0.14555} * 2.f))
	f_3_122 = floor((i.vao_attr7.x * 2.f));
	// 1.00  <=>  floor(({f_1_46 : 0.84634} * 2.f))
	f_5_29 = floor((f_1_46 * 2.f));
	// 0  <=>  {u_8_phi_50 : 0}
	u_9_16 = u_8_phi_50;
	u_9_phi_51 = u_9_16;
	// False  <=>  if(({b_1_45 : False} ? true : false))
	if((b_1_45 ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_8_phi_50) : 0.00} * 5.f)) : 0}
		u_9_17 = ftou((utof(u_8_phi_50) * 5.f));
		u_9_phi_51 = u_9_17;
	}
	// False  <=>  if(({b_1_45 : False} ? true : false))
	if((b_1_45 ? true : false))
	{
		// 0.00  <=>  {utof(u_9_phi_51) : 0.00}
		o.vertex.z = utof(u_9_phi_51);
	}
	// False  <=>  if(((! ({u_10_phi_48 : 1} != 0u)) ? true : false))
	if(((! (u_10_phi_48 != 0u)) ? true : false))
	{
		return;
	}
	// 176  <=>  (({vs_cbuf0_21_x : 675610624} + 176u) - {vs_cbuf0_21_x : 675610624})
	u_3_11 = ((vs_cbuf0_21_x + 176u) - vs_cbuf0_21_x);
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x : }
	u_3_12 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y : }
	u_9_19 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z : }
	u_10_12 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z;
	// 192  <=>  (({vs_cbuf0_21_x : 675610624} + 192u) - {vs_cbuf0_21_x : 675610624})
	u_8_30 = ((vs_cbuf0_21_x + 192u) - vs_cbuf0_21_x);
	// 0  <=>  {ftou(float(int({u_0_11 : 0}))) : 0}
	u_14_3 = ftou(float(int(u_0_11)));
	u_14_phi_54 = u_14_3;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(log2(abs({utof(vs_cbuf9[195].w) : 1.00}))) : 0}
		u_14_4 = ftou(log2(abs(utof(vs_cbuf9[195].w))));
		u_14_phi_54 = u_14_4;
	}
	// 0.00  <=>  floor(({f_4_26 : 0.08906} * 2.f))
	f_3_123 = floor((f_4_26 * 2.f));
	// 1.00  <=>  1.f
	o.fs_attr7.x = 1.f;
	// -0.17453  <=>  {i.vao_attr8.x : -0.17453}
	f_9_24 = i.vao_attr8.x;
	// 0.00  <=>  {i.vao_attr8.y : 0.00}
	f_5_30 = i.vao_attr8.y;
	// 0  <=>  {u_14_phi_54 : 0}
	u_17_11 = u_14_phi_54;
	u_17_phi_55 = u_17_11;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(({pf_0_1 : 199.00} * {utof(u_14_phi_54) : 0.00})) : 0}
		u_17_12 = ftou((pf_0_1 * utof(u_14_phi_54)));
		u_17_phi_55 = u_17_12;
	}
	// 0.00  <=>  {i.vao_attr8.z : 0.00}
	f_10_25 = i.vao_attr8.z;
	// 1062775229  <=>  {u_30_phi_32 : 1062775229}
	u_0_14 = u_30_phi_32;
	u_0_phi_56 = u_0_14;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {u_17_phi_55 : 0}
		u_0_15 = u_17_phi_55;
		u_0_phi_56 = u_0_15;
	}
	// 6  <=>  {vs_cbuf0_21_y : 6}
	u_11_12 = vs_cbuf0_21_y;
	u_11_phi_57 = u_11_12;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 1.00  <=>  exp2({utof(u_0_phi_56) : 0.00})
		f_3_126 = exp2(utof(u_0_phi_56));
		// 1065353216  <=>  {ftou(f_3_126) : 1065353216}
		u_11_13 = ftou(f_3_126);
		u_11_phi_57 = u_11_13;
	}
	// 1065353216  <=>  {vs_cbuf9[195].w : 1065353216}
	u_0_17 = vs_cbuf9[195].w;
	u_0_phi_58 = u_0_17;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 0  <=>  {ftou(((0.f - {utof(vs_cbuf9[195].w) : 1.00}) + 1.f)) : 0}
		u_0_18 = ftou(((0.f - utof(vs_cbuf9[195].w)) + 1.f));
		u_0_phi_58 = u_0_18;
	}
	// 0  <=>  {u_17_phi_55 : 0}
	u_5_16 = u_17_phi_55;
	u_5_phi_59 = u_5_16;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1065353216  <=>  {ftou((1.0f / {utof(u_0_phi_58) : 1.00})) : 1065353216}
		u_5_17 = ftou((1.0f / utof(u_0_phi_58)));
		u_5_phi_59 = u_5_17;
	}
	// 1065353216  <=>  {u_11_phi_57 : 1065353216}
	u_0_20 = u_11_phi_57;
	u_0_phi_60 = u_0_20;
	// False  <=>  if(((((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) ? true : false))
	if(((((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w)))) ? true : false))
	{
		// 1065353216  <=>  1065353216u
		u_0_21 = 1065353216u;
		u_0_phi_60 = u_0_21;
	}
	// 0.00  <=>  (float(int(abs(int((uint((int(0) - int(((int({u_9_15 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_9_15 : 0}) >= int(0u)) ? 0u : 1u))))))))) * ((0.f - {utof((((({f_3_122 : 0.00} < 0.f) && (! myIsNaN({f_3_122 : 0.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0.00}) + {utof((((({f_3_122 : 0.00} > 0.f) && (! myIsNaN({f_3_122 : 0.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0.00}))
	pf_24_5 = (float(int(abs(int((uint((int(0) - int(((int(u_9_15) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_9_15) >= int(0u)) ? 0u : 1u))))))))) * ((0.f - utof(((((f_3_122 < 0.f) && (! myIsNaN(f_3_122))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_122 > 0.f) && (! myIsNaN(f_3_122))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))));
	// 0.00  <=>  (((((({f_4_26 : 0.08906} + {f_1_46 : 0.84634}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].y) : 0.00}) * 2.f) + {utof(vs_cbuf9[195].y) : 0.00})
	pf_0_5 = ((((((f_4_26 + f_1_46) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].y)) * 2.f) + utof(vs_cbuf9[195].y));
	// 0.00  <=>  (((0.f - {utof((((({f_5_29 : 1.00} < 0.f) && (! myIsNaN({f_5_29 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0.00}) + {utof((((({f_5_29 : 1.00} > 0.f) && (! myIsNaN({f_5_29 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 1.00}) * float(int(abs(int((uint((int(0) - int(((int({u_5_6 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_5_6 : 0}) >= int(0u)) ? 0u : 1u))))))))))
	pf_22_5 = (((0.f - utof(((((f_5_29 < 0.f) && (! myIsNaN(f_5_29))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_5_29 > 0.f) && (! myIsNaN(f_5_29))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_5_6) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_5_6) >= int(0u)) ? 0u : 1u))))))))));
	// 0.00  <=>  (((0.f - {utof((((({f_3_123 : 0.00} < 0.f) && (! myIsNaN({f_3_123 : 0.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0.00}) + {utof((((({f_3_123 : 0.00} > 0.f) && (! myIsNaN({f_3_123 : 0.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0.00}) * float(int(abs(int((uint((int(0) - int(((int({u_3_5 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_3_5 : 0}) >= int(0u)) ? 0u : 1u))))))))))
	pf_25_5 = (((0.f - utof(((((f_3_123 < 0.f) && (! myIsNaN(f_3_123))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_123 > 0.f) && (! myIsNaN(f_3_123))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_3_5) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_3_5) >= int(0u)) ? 0u : 1u))))))))));
	// 0.00  <=>  (((((({i.vao_attr7.x : 0.14555} + {f_4_26 : 0.08906}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0.00}) * 2.f) + {utof(vs_cbuf9[195].x) : 0.00})
	pf_20_17 = ((((((i.vao_attr7.x + f_4_26) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)) * 2.f) + utof(vs_cbuf9[195].x));
	// 0.00  <=>  (((((({i.vao_attr7.x : 0.14555} + {f_1_46 : 0.84634}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].z) : 0.00}) * 2.f) + {utof(vs_cbuf9[195].z) : 0.00})
	pf_23_7 = ((((((i.vao_attr7.x + f_1_46) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].z)) * 2.f) + utof(vs_cbuf9[195].z));
	// 1128726528  <=>  {ftou(pf_0_1) : 1128726528}
	u_6_42 = ftou(pf_0_1);
	u_6_phi_61 = u_6_42;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_5_phi_59) : 0.00} * (0.f - {utof(u_0_phi_60) : 1.00})) + {utof(u_5_phi_59) : 0.00})) : 0}
		u_6_43 = ftou(((utof(u_5_phi_59) * (0.f - utof(u_0_phi_60))) + utof(u_5_phi_59)));
		u_6_phi_61 = u_6_43;
	}
	// 11.94333  <=>  ({pf_12_11 : 12.43286} + ({pf_14_10 : 1.165791} * ({pf_12_11 : 12.43286} * (0.f - {f_2_108 : 0.0337748}))))
	pf_12_12 = (pf_12_11 + (pf_14_10 * (pf_12_11 * (0.f - f_2_108))));
	// -11.09832  <=>  ({pf_19_1 : -11.55322} + ({pf_14_10 : 1.165791} * ({pf_19_1 : -11.55322} * (0.f - {f_2_108 : 0.0337748}))))
	pf_19_2 = (pf_19_1 + (pf_14_10 * (pf_19_1 * (0.f - f_2_108))));
	// 0.00  <=>  ((({pf_0_5 : 0.00} * {pf_24_5 : 0.00}) * -2.f) + {pf_0_5 : 0.00})
	pf_0_6 = (((pf_0_5 * pf_24_5) * -2.f) + pf_0_5);
	// 0.00  <=>  ((({pf_20_17 : 0.00} * {pf_22_5 : 0.00}) * -2.f) + {pf_20_17 : 0.00})
	pf_20_18 = (((pf_20_17 * pf_22_5) * -2.f) + pf_20_17);
	// 23.30526  <=>  ({pf_18_6 : 24.2605} + ({pf_14_10 : 1.165791} * ({pf_18_6 : 24.2605} * (0.f - {f_2_108 : 0.0337748}))))
	pf_18_7 = (pf_18_6 + (pf_14_10 * (pf_18_6 * (0.f - f_2_108))));
	// 0.00  <=>  (({pf_0_6 : 0.00} * {utof(u_6_phi_61) : 199.00}) + ((({f_4_26 : 0.08906} + -0.5f) * {utof(vs_cbuf9[194].y) : 0.00}) + ((({pf_24_5 : 0.00} * {f_5_30 : 0.00}) * -2.f) + {f_5_30 : 0.00})))
	pf_0_7 = ((pf_0_6 * utof(u_6_phi_61)) + (((f_4_26 + -0.5f) * utof(vs_cbuf9[194].y)) + (((pf_24_5 * f_5_30) * -2.f) + f_5_30)));
	// -0.17453  <=>  (({pf_20_18 : 0.00} * {utof(u_6_phi_61) : 199.00}) + ((({i.vao_attr7.x : 0.14555} + -0.5f) * {utof(vs_cbuf9[194].x) : 0.00}) + ((({pf_22_5 : 0.00} * {f_9_24 : -0.17453}) * -2.f) + {f_9_24 : -0.17453})))
	pf_19_3 = ((pf_20_18 * utof(u_6_phi_61)) + (((i.vao_attr7.x + -0.5f) * utof(vs_cbuf9[194].x)) + (((pf_22_5 * f_9_24) * -2.f) + f_9_24)));
	// 0.00  <=>  ((((({pf_23_7 : 0.00} * {pf_25_5 : 0.00}) * -2.f) + {pf_23_7 : 0.00}) * {utof(u_6_phi_61) : 199.00}) + ((({f_1_46 : 0.84634} + -0.5f) * {utof(vs_cbuf9[194].z) : 0.00}) + ((({pf_25_5 : 0.00} * {f_10_25 : 0.00}) * -2.f) + {f_10_25 : 0.00})))
	pf_20_19 = (((((pf_23_7 * pf_25_5) * -2.f) + pf_23_7) * utof(u_6_phi_61)) + (((f_1_46 + -0.5f) * utof(vs_cbuf9[194].z)) + (((pf_25_5 * f_10_25) * -2.f) + f_10_25)));
	// 1.00  <=>  cos({pf_20_19 : 0.00})
	f_7_25 = cos(pf_20_19);
	// 0.00  <=>  sin({pf_20_19 : 0.00})
	f_10_26 = sin(pf_20_19);
	// 2.230209  <=>  (((({utof(u_6_15) : 0.00} * {utof(vs_cbuf9[145].y) : 1.41}) + (((((({utof(vs_cbuf9[145].y) : 1.41} + (0.f - {utof(vs_cbuf9[144].y) : 1.32})) * (1.0f / ({utof(vs_cbuf9[145].w) : 1.00} + (0.f - {utof(vs_cbuf9[144].w) : 0.67})))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[144].w) : 0.67}))) + {utof(vs_cbuf9[144].y) : 1.32}) * (({utof(u_7_12) : 1.00} * (0.f - {utof(u_6_15) : 0.00})) + {utof(u_7_12) : 1.00})) + (((((((0.f - {utof(vs_cbuf9[143].y) : 1.20}) + {utof(vs_cbuf9[144].y) : 1.32}) * (1.0f / ((0.f - {utof(vs_cbuf9[143].w) : 0.48}) + {utof(vs_cbuf9[144].w) : 0.67}))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[143].w) : 0.48}))) + {utof(vs_cbuf9[143].y) : 1.20}) * (({utof(u_9_0) : 1.00} * (0.f - {utof(u_7_12) : 1.00})) + {utof(u_9_0) : 1.00})) + (((((({utof(vs_cbuf9[143].y) : 1.20} + (0.f - {utof(vs_cbuf9[142].y) : 0.88})) * (1.0f / ({utof(vs_cbuf9[143].w) : 0.48} + (0.f - {utof(vs_cbuf9[142].w) : 0.20})))) * ({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[142].w) : 0.20}))) + {utof(vs_cbuf9[142].y) : 0.88}) * (({utof(u_8_5) : 1.00} * (0.f - {utof(u_9_0) : 1.00})) + {utof(u_8_5) : 1.00})) + ((((({utof(u_6_phi_18) : 0.995} + (0.f - {utof(vs_cbuf9[141].w) : 0.00})) * (({utof(vs_cbuf9[142].y) : 0.88} + (0.f - {utof(vs_cbuf9[141].y) : 0.76})) * (1.0f / ((0.f - {utof(vs_cbuf9[141].w) : 0.00}) + {utof(vs_cbuf9[142].w) : 0.20})))) + {utof(vs_cbuf9[141].y) : 0.76}) * (({utof(u_7_11) : 1.00} * (0.f - {utof(u_8_5) : 1.00})) + {utof(u_7_11) : 1.00})) + (({utof(u_7_11) : 1.00} * (0.f - {utof(vs_cbuf9[141].y) : 0.76})) + {utof(vs_cbuf9[141].y) : 0.76})))))) * {i.vao_attr6.y : 1.58324}) * {(vs_cbuf10_3.z) : 1.00})
	pf_3_16 = ((((utof(u_6_15) * utof(vs_cbuf9[145].y)) + ((((((utof(vs_cbuf9[145].y) + (0.f - utof(vs_cbuf9[144].y))) * (1.0f / (utof(vs_cbuf9[145].w) + (0.f - utof(vs_cbuf9[144].w))))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[144].w)))) + utof(vs_cbuf9[144].y)) * ((utof(u_7_12) * (0.f - utof(u_6_15))) + utof(u_7_12))) + (((((((0.f - utof(vs_cbuf9[143].y)) + utof(vs_cbuf9[144].y)) * (1.0f / ((0.f - utof(vs_cbuf9[143].w)) + utof(vs_cbuf9[144].w)))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[143].w)))) + utof(vs_cbuf9[143].y)) * ((utof(u_9_0) * (0.f - utof(u_7_12))) + utof(u_9_0))) + ((((((utof(vs_cbuf9[143].y) + (0.f - utof(vs_cbuf9[142].y))) * (1.0f / (utof(vs_cbuf9[143].w) + (0.f - utof(vs_cbuf9[142].w))))) * (utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[142].w)))) + utof(vs_cbuf9[142].y)) * ((utof(u_8_5) * (0.f - utof(u_9_0))) + utof(u_8_5))) + (((((utof(u_6_phi_18) + (0.f - utof(vs_cbuf9[141].w))) * ((utof(vs_cbuf9[142].y) + (0.f - utof(vs_cbuf9[141].y))) * (1.0f / ((0.f - utof(vs_cbuf9[141].w)) + utof(vs_cbuf9[142].w))))) + utof(vs_cbuf9[141].y)) * ((utof(u_7_11) * (0.f - utof(u_8_5))) + utof(u_7_11))) + ((utof(u_7_11) * (0.f - utof(vs_cbuf9[141].y))) + utof(vs_cbuf9[141].y))))))) * i.vao_attr6.y) * (vs_cbuf10_3.z));
	// -0.1736453  <=>  (sin({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))
	pf_18_8 = (sin(pf_19_3) * cos(pf_0_7));
	// 0.00  <=>  (cos({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))
	pf_22_8 = (cos(pf_19_3) * sin(pf_0_7));
	// 2.142396  <=>  ({pf_3_16 : 2.230209} * ({f_2_108 : 0.0337748} * sqrt((({pf_18_7 : 23.30526} * {pf_18_7 : 23.30526}) + (({pf_19_2 : -11.09832} * {pf_19_2 : -11.09832}) + ({pf_12_12 : 11.94333} * {pf_12_12 : 11.94333}))))))
	pf_3_17 = (pf_3_16 * (f_2_108 * sqrt(((pf_18_7 * pf_18_7) + ((pf_19_2 * pf_19_2) + (pf_12_12 * pf_12_12))))));
	// 4.479554  <=>  ((((clamp(min(0.f, {i.vao_attr7.x : 0.14555}), 0.0, 1.0) + {i.vao_attr6.x : 3.31041}) * {pf_3_14 : 1.408636}) * {(vs_cbuf10_3.y) : 1.00}) * ({f_2_108 : 0.0337748} * sqrt((({pf_18_7 : 23.30526} * {pf_18_7 : 23.30526}) + (({pf_19_2 : -11.09832} * {pf_19_2 : -11.09832}) + ({pf_12_12 : 11.94333} * {pf_12_12 : 11.94333}))))))
	pf_2_8 = ((((clamp(min(0.f, i.vao_attr7.x), 0.0, 1.0) + i.vao_attr6.x) * pf_3_14) * (vs_cbuf10_3.y)) * (f_2_108 * sqrt(((pf_18_7 * pf_18_7) + ((pf_19_2 * pf_19_2) + (pf_12_12 * pf_12_12))))));
	// -0.2845316  <=>  ({pf_3_17 : 2.142396} * ((0.5f * {utof(vs_cbuf9[16].y) : 0.50}) + {v.vertex.y : -0.38281}))
	pf_3_18 = (pf_3_17 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y));
	// 0.9738102  <=>  (({pf_4_9 : 1.013725} * {(vs_cbuf10_3.w) : 1.00}) * ({f_2_108 : 0.0337748} * sqrt((({pf_18_7 : 23.30526} * {pf_18_7 : 23.30526}) + (({pf_19_2 : -11.09832} * {pf_19_2 : -11.09832}) + ({pf_12_12 : 11.94333} * {pf_12_12 : 11.94333}))))))
	pf_0_9 = ((pf_4_9 * (vs_cbuf10_3.w)) * (f_2_108 * sqrt(((pf_18_7 * pf_18_7) + ((pf_19_2 * pf_19_2) + (pf_12_12 * pf_12_12))))));
	// -0.7103229  <=>  ({pf_2_8 : 4.479554} * ((0.5f * {utof(vs_cbuf9[16].x) : 0.00}) + {v.vertex.x : -0.15857}))
	pf_12_17 = (pf_2_8 * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x));
	// 0.00  <=>  ({pf_0_9 : 0.9738102} * ((0.5f * {utof(vs_cbuf9[16].z) : 0.00}) + {v.vertex.z : 0.00}))
	pf_0_10 = (pf_0_9 * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z));
	// 0.0494076  <=>  ((sin({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {pf_3_18 : -0.2845316})
	pf_3_19 = ((sin(pf_19_3) * f_7_25) * pf_3_18);
	// -0.7103229  <=>  (((sin({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {pf_0_10 : 0.00}) + (((cos({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {pf_12_17 : -0.7103229}) + ({f_10_26 : 0.00} * (0.f - {pf_3_18 : -0.2845316}))))
	pf_12_18 = (((sin(pf_0_7) * f_7_25) * pf_0_10) + (((cos(pf_0_7) * f_7_25) * pf_12_17) + (f_10_26 * (0.f - pf_3_18))));
	// -0.2802091  <=>  (((({f_10_26 : 0.00} * {pf_22_8 : 0.00}) + (0.f - {pf_18_8 : -0.1736453})) * {pf_0_10 : 0.00}) + (((({f_10_26 : 0.00} * (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) + (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) * {pf_12_17 : -0.7103229}) + ((cos({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {pf_3_18 : -0.2845316})))
	pf_20_23 = ((((f_10_26 * pf_22_8) + (0.f - pf_18_8)) * pf_0_10) + ((((f_10_26 * (cos(pf_19_3) * cos(pf_0_7))) + (sin(pf_19_3) * sin(pf_0_7))) * pf_12_17) + ((cos(pf_19_3) * f_7_25) * pf_3_18)));
	// 0.0494076  <=>  (((({f_10_26 : 0.00} * (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) + (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) * {pf_0_10 : 0.00}) + (((({f_10_26 : 0.00} * {pf_18_8 : -0.1736453}) + (0.f - {pf_22_8 : 0.00})) * {pf_12_17 : -0.7103229}) + {pf_3_19 : 0.0494076}))
	pf_0_11 = ((((f_10_26 * (sin(pf_19_3) * sin(pf_0_7))) + (cos(pf_19_3) * cos(pf_0_7))) * pf_0_10) + ((((f_10_26 * pf_18_8) + (0.f - pf_22_8)) * pf_12_17) + pf_3_19));
	// -0.1009969  <=>  ((((({pf_0_11 : 0.0494076} * {(vs_cbuf8_25.z) : 0.5074672}) + (({pf_20_23 : -0.2802091} * {(vs_cbuf8_25.y) : 0.8616711}) + ({pf_12_18 : -0.7103229} * {(vs_cbuf8_25.x) : 0.00}))) + {(vs_cbuf8_25.w) : 0.00}) + (((0.f - abs({(vs_cbuf8_28.y) : 0.5074672})) * {(vs_cbuf13_6.w) : 0.00}) + {(vs_cbuf13_6.w) : 0.00})) * (1.0f / {pf_3_17 : 2.142396}))
	pf_17_13 = (((((pf_0_11 * (vs_cbuf8_25.z)) + ((pf_20_23 * (vs_cbuf8_25.y)) + (pf_12_18 * (vs_cbuf8_25.x)))) + (vs_cbuf8_25.w)) + (((0.f - abs((vs_cbuf8_28.y))) * (vs_cbuf13_6.w)) + (vs_cbuf13_6.w))) * (1.0f / pf_3_17));
	// 27.43677  <=>  ((({pf_21_2 : -3709.742} * {(vs_cbuf8_11.z) : 0.6398518}) + (({pf_14_11 : 354.639} * {(vs_cbuf8_11.y) : -0.5074672}) + ({pf_17_10 : -1907.319} * {(vs_cbuf8_11.x) : 0.5771194}))) + {(vs_cbuf8_11.w) : 3681.84})
	pf_20_24 = (((pf_21_2 * (vs_cbuf8_11.z)) + ((pf_14_11 * (vs_cbuf8_11.y)) + (pf_17_10 * (vs_cbuf8_11.x)))) + (vs_cbuf8_11.w));
	// 27.23682  <=>  ((({pf_21_2 : -3709.742} * {(vs_cbuf8_10.z) : 0.6398569}) + (({pf_14_11 : 354.639} * {(vs_cbuf8_10.y) : -0.5074712}) + ({pf_17_10 : -1907.319} * {(vs_cbuf8_10.x) : 0.5771239}))) + {(vs_cbuf8_10.w) : 3681.669})
	pf_14_14 = (((pf_21_2 * (vs_cbuf8_10.z)) + ((pf_14_11 * (vs_cbuf8_10.y)) + (pf_17_10 * (vs_cbuf8_10.x)))) + (vs_cbuf8_10.w));
	// -0.00  <=>  ((({pf_17_13 : -0.1009969} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_17_13 : -0.1009969})) * {(vs_cbuf13_2.y) : 0.00})
	pf_23_13 = (((pf_17_13 * (vs_cbuf16_1.w)) * abs(pf_17_13)) * (vs_cbuf13_2.y));
	// 3208141928  <=>  {ftou(v.offset.y) : 3208141928}
	u_6_44 = ftou(v.offset.y);
	u_6_phi_62 = u_6_44;
	// True  <=>  if(((! (((({v.vertex.z : 0.00} == 0.f) && (! myIsNaN({v.vertex.z : 0.00}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.15857} == 0.f) && (! myIsNaN({v.vertex.x : -0.15857}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.38281} == 0.f) && (! myIsNaN({v.vertex.y : -0.38281}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 3208141928  <=>  {ftou((({v.vertex.y : -0.38281} * {(vs_cbuf13_0.x) : 0.00}) + {v.offset.y : -0.72016})) : 3208141928}
		u_6_45 = ftou(((v.vertex.y * (vs_cbuf13_0.x)) + v.offset.y));
		u_6_phi_62 = u_6_45;
	}
	// -0.1308295  <=>  ((({(vs_cbuf13_2.w) : 70.00} * {(vs_cbuf16_0.z) : -33.41627}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_0_1 : 199.00} * {utof(vs_cbuf9[75].z) : 0.001}) + (({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[76].z) : 0.10}) + ({utof(vs_cbuf9[76].x) : 0.60} + {utof(vs_cbuf9[76].z) : 0.10}))) * (({pf_23_0 : 1.00} * {utof(u_24_phi_23) : 0.32941}) + -0.5f)) + (({pf_23_0 : 1.00} * float(int({u_6_30 : 0}))) + (({pf_0_1 : 199.00} * (0.f - {utof(vs_cbuf9[74].x) : 0.00})) + (0.f - ((({i.vao_attr7.x : 0.14555} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0.00})))))) + 0.5f))
	o.fs_attr2.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[74].y)) + (((((pf_0_1 * utof(vs_cbuf9[75].z)) + ((i.vao_attr7.x * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z)))) * ((pf_23_0 * utof(u_24_phi_23)) + -0.5f)) + ((pf_23_0 * float(int(u_6_30))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((i.vao_attr7.x * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f));
	// 1059082228  <=>  {ftou(v.offset.z) : 1059082228}
	u_4_4 = ftou(v.offset.z);
	u_4_phi_63 = u_4_4;
	// True  <=>  if(((! (((({v.vertex.z : 0.00} == 0.f) && (! myIsNaN({v.vertex.z : 0.00}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.15857} == 0.f) && (! myIsNaN({v.vertex.x : -0.15857}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.38281} == 0.f) && (! myIsNaN({v.vertex.y : -0.38281}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1059082228  <=>  {ftou((({v.vertex.z : 0.00} * {(vs_cbuf13_0.x) : 0.00}) + {v.offset.z : 0.62622})) : 1059082228}
		u_4_5 = ftou(((v.vertex.z * (vs_cbuf13_0.x)) + v.offset.z));
		u_4_phi_63 = u_4_5;
	}
	// -1906.915  <=>  (({pf_14_10 : 1.165791} * ({pf_12_11 : 12.43286} * (0.f - {f_2_108 : 0.0337748}))) + (({pf_2_8 : 4.479554} * ({pf_23_13 : -0.00} * {(vs_cbuf16_1.x) : 0.7282469})) + ({pf_8_7 : -1906.829} + ((({pf_0_11 : 0.0494076} * {(vs_cbuf8_24.z) : -0.5771194}) + (({pf_20_23 : -0.2802091} * {(vs_cbuf8_24.y) : 0.339885}) + ({pf_12_18 : -0.7103229} * {(vs_cbuf8_24.x) : -0.7425708}))) + {(vs_cbuf8_24.w) : 0.00}))))
	pf_5_21 = ((pf_14_10 * (pf_12_11 * (0.f - f_2_108))) + ((pf_2_8 * (pf_23_13 * (vs_cbuf16_1.x))) + (pf_8_7 + (((pf_0_11 * (vs_cbuf8_24.z)) + ((pf_20_23 * (vs_cbuf8_24.y)) + (pf_12_18 * (vs_cbuf8_24.x)))) + (vs_cbuf8_24.w)))));
	// -3709.40  <=>  (({pf_2_8 : 4.479554} * ({pf_23_13 : -0.00} * {(vs_cbuf16_1.z) : 0.685315})) + (((({i.vao_attr4.z : 20.30853} * {i.vao_attr11.z : 0.998}) + (({i.vao_attr4.y : -10.90624} * {i.vao_attr11.y : 0.00}) + ({i.vao_attr4.x : 6.81513} * {i.vao_attr11.x : -0.0632}))) + {i.vao_attr11.w : -3728.624}) + ((({pf_0_11 : 0.0494076} * {(vs_cbuf8_26.z) : -0.6398518}) + (({pf_20_23 : -0.2802091} * {(vs_cbuf8_26.y) : 0.3768303}) + ({pf_12_18 : -0.7103229} * {(vs_cbuf8_26.x) : 0.6697676}))) + {(vs_cbuf8_26.w) : 0.00})))
	pf_0_15 = ((pf_2_8 * (pf_23_13 * (vs_cbuf16_1.z))) + ((((i.vao_attr4.z * i.vao_attr11.z) + ((i.vao_attr4.y * i.vao_attr11.y) + (i.vao_attr4.x * i.vao_attr11.x))) + i.vao_attr11.w) + (((pf_0_11 * (vs_cbuf8_26.z)) + ((pf_20_23 * (vs_cbuf8_26.y)) + (pf_12_18 * (vs_cbuf8_26.x)))) + (vs_cbuf8_26.w))));
	// 354.4226  <=>  (({pf_14_10 : 1.165791} * ({pf_19_1 : -11.55322} * (0.f - {f_2_108 : 0.0337748}))) + (({pf_2_8 : 4.479554} * ({pf_23_13 : -0.00} * {(vs_cbuf16_1.y) : 0.00})) + ({pf_7_8 : 354.1841} + ((({pf_0_11 : 0.0494076} * {(vs_cbuf8_25.z) : 0.5074672}) + (({pf_20_23 : -0.2802091} * {(vs_cbuf8_25.y) : 0.8616711}) + ({pf_12_18 : -0.7103229} * {(vs_cbuf8_25.x) : 0.00}))) + {(vs_cbuf8_25.w) : 0.00}))))
	pf_2_9 = ((pf_14_10 * (pf_19_1 * (0.f - f_2_108))) + ((pf_2_8 * (pf_23_13 * (vs_cbuf16_1.y))) + (pf_7_8 + (((pf_0_11 * (vs_cbuf8_25.z)) + ((pf_20_23 * (vs_cbuf8_25.y)) + (pf_12_18 * (vs_cbuf8_25.x)))) + (vs_cbuf8_25.w)))));
	// 3197652141  <=>  {ftou(v.offset.x) : 3197652141}
	u_5_19 = ftou(v.offset.x);
	u_5_phi_64 = u_5_19;
	// True  <=>  if(((! (((({v.vertex.z : 0.00} == 0.f) && (! myIsNaN({v.vertex.z : 0.00}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.15857} == 0.f) && (! myIsNaN({v.vertex.x : -0.15857}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.38281} == 0.f) && (! myIsNaN({v.vertex.y : -0.38281}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 3197652141  <=>  {ftou((({v.vertex.x : -0.15857} * {(vs_cbuf13_0.x) : 0.00}) + {v.offset.x : -0.29746})) : 3197652141}
		u_5_20 = ftou(((v.vertex.x * (vs_cbuf13_0.x)) + v.offset.x));
		u_5_phi_64 = u_5_20;
	}
	// -3710.355  <=>  (({pf_14_10 : 1.165791} * ({pf_18_6 : 24.2605} * (0.f - {f_2_108 : 0.0337748}))) + {pf_0_15 : -3709.40})
	pf_0_16 = ((pf_14_10 * (pf_18_6 * (0.f - f_2_108))) + pf_0_15);
	// 1280.375  <=>  (({pf_2_9 : 354.4226} * {(view_proj[2].y) : 0.5074672}) + ({pf_5_21 : -1906.915} * {(view_proj[2].x) : -0.5771194}))
	pf_11_20 = ((pf_2_9 * (view_proj[2].y)) + (pf_5_21 * (view_proj[2].x)));
	// 0.9963562  <=>  ((({pf_14_14 : 27.23682} * 0.5f) + ({pf_20_24 : 27.43677} * 0.5f)) * (1.0f / ((0.f * {pf_14_14 : 27.23682}) + {pf_20_24 : 27.43677})))
	pf_6_11 = (((pf_14_14 * 0.5f) + (pf_20_24 * 0.5f)) * (1.0f / ((0.f * pf_14_14) + pf_20_24)));
	// 6.029907  <=>  ((({pf_0_16 : -3710.355} * {(view_proj[0].z) : 0.6697676}) + (({pf_2_9 : 354.4226} * {(view_proj[0].y) : 0.00}) + ({pf_5_21 : -1906.915} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
	pf_8_14 = (((pf_0_16 * (view_proj[0].z)) + ((pf_2_9 * (view_proj[0].y)) + (pf_5_21 * (view_proj[0].x)))) + (view_proj[0].w));
	// -0.0109656  <=>  (1.0f / (({pf_6_11 : 0.9963562} * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00})))
	f_0_25 = (1.0f / ((pf_6_11 * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y))));
	// 2.997681  <=>  ((({pf_0_16 : -3710.355} * {(view_proj[1].z) : 0.3768303}) + (({pf_2_9 : 354.4226} * {(view_proj[1].y) : 0.8616711}) + ({pf_5_21 : -1906.915} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
	pf_6_13 = (((pf_0_16 * (view_proj[1].z)) + ((pf_2_9 * (view_proj[1].y)) + (pf_5_21 * (view_proj[1].x)))) + (view_proj[1].w));
	// 0.00  <=>  (({pf_0_16 : -3710.355} * {(view_proj[3].z) : 0.00}) + (({pf_2_9 : 354.4226} * {(view_proj[3].y) : 0.00}) + ({pf_5_21 : -1906.915} * {(view_proj[3].x) : 0.00})))
	pf_2_11 = ((pf_0_16 * (view_proj[3].z)) + ((pf_2_9 * (view_proj[3].y)) + (pf_5_21 * (view_proj[3].x))));
	// -27.38721  <=>  ((({pf_0_16 : -3710.355} * {(view_proj[2].z) : -0.6398518}) + {pf_11_20 : 1280.375}) + {(view_proj[2].w) : -3681.84})
	pf_11_22 = (((pf_0_16 * (view_proj[2].z)) + pf_11_20) + (view_proj[2].w));
	// 1.00  <=>  ({pf_2_11 : 0.00} + {(view_proj[3].w) : 1.00})
	pf_2_12 = (pf_2_11 + (view_proj[3].w));
	// 6.428547  <=>  (({pf_2_12 : 1.00} * {(view_proj[5].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[5].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[5].y) : 2.144507}) + ({pf_8_14 : 6.029907} * {(view_proj[5].x) : 0.00}))))
	o.vertex.y = ((pf_2_12 * (view_proj[5].w)) + ((pf_11_22 * (view_proj[5].z)) + ((pf_6_13 * (view_proj[5].y)) + (pf_8_14 * (view_proj[5].x)))));
	// 7.273787  <=>  (({pf_2_12 : 1.00} * {(view_proj[4].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[4].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[4].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[4].x) : 1.206285}))))
	o.vertex.x = ((pf_2_12 * (view_proj[4].w)) + ((pf_11_22 * (view_proj[4].z)) + ((pf_6_13 * (view_proj[4].y)) + (pf_8_14 * (view_proj[4].x)))));
	// 27.18743  <=>  (({pf_2_12 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_22 : -27.38721} * {(view_proj[6].z) : -1.000008}) + (({pf_6_13 : 2.997681} * {(view_proj[6].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[6].x) : 0.00}))))
	o.vertex.z = ((pf_2_12 * (view_proj[6].w)) + ((pf_11_22 * (view_proj[6].z)) + ((pf_6_13 * (view_proj[6].y)) + (pf_8_14 * (view_proj[6].x)))));
	// 1.00  <=>  clamp(((({f_0_25 : -0.0109656} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 3.00})) * (1.0f / ({utof(vs_cbuf9[138].y) : 5.00} + (0.f - {utof(vs_cbuf9[138].x) : 3.00})))), 0.0, 1.0)
	f_2_176 = clamp((((f_0_25 * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x))) * (1.0f / (utof(vs_cbuf9[138].y) + (0.f - utof(vs_cbuf9[138].x))))), 0.0, 1.0);
	// 0.00  <=>  ((0.f - clamp(((({f_0_25 : -0.0109656} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].z) : 10.00})) * (1.0f / ({utof(vs_cbuf9[138].w) : 20.00} + (0.f - {utof(vs_cbuf9[138].z) : 10.00})))), 0.0, 1.0)) + 1.f)
	pf_19_8 = ((0.f - clamp((((f_0_25 * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].z))) * (1.0f / (utof(vs_cbuf9[138].w) + (0.f - utof(vs_cbuf9[138].z))))), 0.0, 1.0)) + 1.f);
	// 27.38721  <=>  (({pf_2_12 : 1.00} * {(view_proj[7].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[7].z) : -1.00}) + (({pf_6_13 : 2.997681} * {(view_proj[7].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[7].x) : 0.00}))))
	o.vertex.w = ((pf_2_12 * (view_proj[7].w)) + ((pf_11_22 * (view_proj[7].z)) + ((pf_6_13 * (view_proj[7].y)) + (pf_8_14 * (view_proj[7].x)))));
	// 1.700438  <=>  (((({(vs_cbuf13_2.w) : 70.00} * {(vs_cbuf16_0.w) : -256.3035}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_0_1 : 199.00} * {utof(vs_cbuf9[75].w) : 0.001}) + (({f_4_26 : 0.08906} * {utof(vs_cbuf9[76].w) : 0.05}) + ({utof(vs_cbuf9[76].w) : 0.05} + {utof(vs_cbuf9[76].y) : 0.25}))) * (({pf_27_0 : 1.00} * {utof(u_25_phi_24) : 0.91373}) + -0.5f)) + (0.f - (({pf_27_0 : 1.00} * (0.f - float(int((({b_3_4 : False} || {b_2_16 : True}) ? {u_6_33 : 0} : 4294967295u))))) + (({pf_0_1 : 199.00} * {utof(vs_cbuf9[74].y) : -0.0001}) + ((({f_4_26 : 0.08906} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0.00})))))) + 0.5f))
	o.fs_attr2.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[74].y)) + (((((pf_0_1 * utof(vs_cbuf9[75].w)) + ((f_4_26 * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)))) * ((pf_27_0 * utof(u_25_phi_24)) + -0.5f)) + (0.f - ((pf_27_0 * (0.f - float(int(((b_3_4 || b_2_16) ? u_6_33 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((f_4_26 * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f));
	// 27.38721  <=>  ((({pf_2_12 : 1.00} * {(view_proj[7].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[7].z) : -1.00}) + (({pf_6_13 : 2.997681} * {(view_proj[7].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[7].x) : 0.00})))) + ((0.f * (({pf_2_12 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_22 : -27.38721} * {(view_proj[6].z) : -1.000008}) + (({pf_6_13 : 2.997681} * {(view_proj[6].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[6].x) : 0.00}))))) + ((0.f * (({pf_2_12 : 1.00} * {(view_proj[4].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[4].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[4].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_2_12 : 1.00} * {(view_proj[5].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[5].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[5].y) : 2.144507}) + ({pf_8_14 : 6.029907} * {(view_proj[5].x) : 0.00}))))))))
	pf_7_15 = (((pf_2_12 * (view_proj[7].w)) + ((pf_11_22 * (view_proj[7].z)) + ((pf_6_13 * (view_proj[7].y)) + (pf_8_14 * (view_proj[7].x))))) + ((0.f * ((pf_2_12 * (view_proj[6].w)) + ((pf_11_22 * (view_proj[6].z)) + ((pf_6_13 * (view_proj[6].y)) + (pf_8_14 * (view_proj[6].x)))))) + ((0.f * ((pf_2_12 * (view_proj[4].w)) + ((pf_11_22 * (view_proj[4].z)) + ((pf_6_13 * (view_proj[4].y)) + (pf_8_14 * (view_proj[4].x)))))) + (0.f * ((pf_2_12 * (view_proj[5].w)) + ((pf_11_22 * (view_proj[5].z)) + ((pf_6_13 * (view_proj[5].y)) + (pf_8_14 * (view_proj[5].x)))))))));
	// 27.38721  <=>  {pf_7_15 : 27.38721}
	o.fs_attr3.w = pf_7_15;
	// ((0.f - {utof(u_3_12) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
	pf_7_16 = ((0.f - utof(u_3_12)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x));
	// ((0.f - {utof(u_9_19) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
	pf_12_23 = ((0.f - utof(u_9_19)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y));
	// ((0.f - {utof(u_10_12) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
	pf_14_24 = ((0.f - utof(u_10_12)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z));
	// 0.00  <=>  ({pf_1_7 : 0.9505678} * (({f_2_176 : 1.00} * {pf_19_8 : 0.00}) * {(vs_cbuf10_3.x) : 1.00}))
	o.fs_attr4.x = (pf_1_7 * ((f_2_176 * pf_19_8) * (vs_cbuf10_3.x)));
	// 10.47933  <=>  (((({pf_2_12 : 1.00} * {(view_proj[7].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[7].z) : -1.00}) + (({pf_6_13 : 2.997681} * {(view_proj[7].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[7].x) : 0.00})))) * 0.5f) + ((0.f * (({pf_2_12 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_22 : -27.38721} * {(view_proj[6].z) : -1.000008}) + (({pf_6_13 : 2.997681} * {(view_proj[6].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[6].x) : 0.00}))))) + ((0.f * (({pf_2_12 : 1.00} * {(view_proj[4].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[4].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[4].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_2_12 : 1.00} * {(view_proj[5].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[5].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[5].y) : 2.144507}) + ({pf_8_14 : 6.029907} * {(view_proj[5].x) : 0.00})))) * -0.5f))))
	pf_1_9 = ((((pf_2_12 * (view_proj[7].w)) + ((pf_11_22 * (view_proj[7].z)) + ((pf_6_13 * (view_proj[7].y)) + (pf_8_14 * (view_proj[7].x))))) * 0.5f) + ((0.f * ((pf_2_12 * (view_proj[6].w)) + ((pf_11_22 * (view_proj[6].z)) + ((pf_6_13 * (view_proj[6].y)) + (pf_8_14 * (view_proj[6].x)))))) + ((0.f * ((pf_2_12 * (view_proj[4].w)) + ((pf_11_22 * (view_proj[4].z)) + ((pf_6_13 * (view_proj[4].y)) + (pf_8_14 * (view_proj[4].x)))))) + (((pf_2_12 * (view_proj[5].w)) + ((pf_11_22 * (view_proj[5].z)) + ((pf_6_13 * (view_proj[5].y)) + (pf_8_14 * (view_proj[5].x))))) * -0.5f))));
	// 10.47933  <=>  {pf_1_9 : 10.47933}
	o.fs_attr3.y = pf_1_9;
	// 0.7460654  <=>  ((({(vs_cbuf13_2.w) : 70.00} * {(vs_cbuf16_0.z) : -33.41627}) * {utof(vs_cbuf9[79].y) : 0.00}) + (((((cos({f_5_22 : -7.197703}) * (({pf_14_7 : 1.00} * {utof(u_20_phi_25) : 0.32941}) + -0.5f)) + (0.f - ((({pf_13_12 : 1.00} * {utof(u_14_phi_26) : 0.91373}) + -0.5f) * sin({f_5_22 : -7.197703})))) * (({pf_0_1 : 199.00} * {utof(vs_cbuf9[80].z) : 0.00}) + (({utof(u_33_phi_32) : 0.08906} * {utof(vs_cbuf9[81].z) : 0.00}) + ({utof(vs_cbuf9[81].z) : 0.00} + {utof(vs_cbuf9[81].x) : 1.10})))) + (({pf_14_7 : 1.00} * float(int({u_0_11 : 0}))) + (({pf_0_1 : 199.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.00})) + (0.f - ((({utof(u_31_phi_32) : 0.08906} * {utof(vs_cbuf9[80].x) : 0.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 0.00} + {utof(vs_cbuf9[79].z) : 0.00})))))) + 0.5f))
	o.fs_attr2.z = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[79].y)) + (((((cos(f_5_22) * ((pf_14_7 * utof(u_20_phi_25)) + -0.5f)) + (0.f - (((pf_13_12 * utof(u_14_phi_26)) + -0.5f) * sin(f_5_22)))) * ((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_33_phi_32) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].z) + utof(vs_cbuf9[81].x))))) + ((pf_14_7 * float(int(u_0_11))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_31_phi_32) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))) + 0.5f));
	// 0.9263599  <=>  (((({(vs_cbuf13_2.w) : 70.00} * {(vs_cbuf16_0.w) : -256.3035}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[79].y) : 0.00}) + ((((((({pf_14_7 : 1.00} * {utof(u_20_phi_25) : 0.32941}) + -0.5f) * sin({f_5_22 : -7.197703})) + ((({pf_13_12 : 1.00} * {utof(u_14_phi_26) : 0.91373}) + -0.5f) * cos({f_5_22 : -7.197703}))) * (({pf_0_1 : 199.00} * {utof(vs_cbuf9[80].w) : 0.00}) + (({utof(u_30_phi_32) : 0.84634} * {utof(vs_cbuf9[81].w) : 0.00}) + ({utof(vs_cbuf9[81].w) : 0.00} + {utof(vs_cbuf9[81].y) : 1.10})))) + (0.f - (({pf_13_12 : 1.00} * (0.f - float(int((({b_4_8 : False} || {b_0_23 : True}) ? {u_9_11 : 0} : 4294967295u))))) + (({pf_0_1 : 199.00} * {utof(vs_cbuf9[79].y) : 0.00}) + ((({utof(u_32_phi_32) : 0.84634} * {utof(vs_cbuf9[80].y) : 0.00}) * -2.f) + ({utof(vs_cbuf9[80].y) : 0.00} + {utof(vs_cbuf9[79].w) : 0.00})))))) + 0.5f))
	o.fs_attr2.w = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[79].y)) + (((((((pf_14_7 * utof(u_20_phi_25)) + -0.5f) * sin(f_5_22)) + (((pf_13_12 * utof(u_14_phi_26)) + -0.5f) * cos(f_5_22))) * ((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_30_phi_32) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y))))) + (0.f - ((pf_13_12 * (0.f - float(int(((b_4_8 || b_0_23) ? u_9_11 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_32_phi_32) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f));
	// 27.28732  <=>  (((({pf_2_12 : 1.00} * {(view_proj[7].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[7].z) : -1.00}) + (({pf_6_13 : 2.997681} * {(view_proj[7].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[7].x) : 0.00})))) * 0.5f) + (((({pf_2_12 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_22 : -27.38721} * {(view_proj[6].z) : -1.000008}) + (({pf_6_13 : 2.997681} * {(view_proj[6].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[6].x) : 0.00})))) * 0.5f) + ((0.f * (({pf_2_12 : 1.00} * {(view_proj[4].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[4].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[4].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_2_12 : 1.00} * {(view_proj[5].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[5].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[5].y) : 2.144507}) + ({pf_8_14 : 6.029907} * {(view_proj[5].x) : 0.00}))))))))
	pf_1_12 = ((((pf_2_12 * (view_proj[7].w)) + ((pf_11_22 * (view_proj[7].z)) + ((pf_6_13 * (view_proj[7].y)) + (pf_8_14 * (view_proj[7].x))))) * 0.5f) + ((((pf_2_12 * (view_proj[6].w)) + ((pf_11_22 * (view_proj[6].z)) + ((pf_6_13 * (view_proj[6].y)) + (pf_8_14 * (view_proj[6].x))))) * 0.5f) + ((0.f * ((pf_2_12 * (view_proj[4].w)) + ((pf_11_22 * (view_proj[4].z)) + ((pf_6_13 * (view_proj[4].y)) + (pf_8_14 * (view_proj[4].x)))))) + (0.f * ((pf_2_12 * (view_proj[5].w)) + ((pf_11_22 * (view_proj[5].z)) + ((pf_6_13 * (view_proj[5].y)) + (pf_8_14 * (view_proj[5].x)))))))));
	// 27.28732  <=>  {pf_1_12 : 27.28732}
	o.fs_attr3.z = pf_1_12;
	// True  <=>  (((({pf_19_8 : 0.00} <= 0.f) && (! myIsNaN({pf_19_8 : 0.00}))) && (! myIsNaN(0.f))) || ((({f_2_176 : 1.00} <= 0.f) && (! myIsNaN({f_2_176 : 1.00}))) && (! myIsNaN(0.f))))
	b_1_59 = ((((pf_19_8 <= 0.f) && (! myIsNaN(pf_19_8))) && (! myIsNaN(0.f))) || (((f_2_176 <= 0.f) && (! myIsNaN(f_2_176))) && (! myIsNaN(0.f))));
	// 0.00  <=>  ({i.vao_attr12.x : 0.00} * {(vs_cbuf13_5.z) : 0.80})
	o.fs_attr8.w = (i.vao_attr12.x * (vs_cbuf13_5.z));
	// ({utof(u_9_19) : } + (({pf_12_23 : } * (0.f - clamp(((({pf_1_9 : 10.47933} * (1.0f / {pf_7_15 : 27.38721})) * -0.7f) + 0.85f), 0.0, 1.0))) + {pf_12_23 : }))
	pf_9_14 = (utof(u_9_19) + ((pf_12_23 * (0.f - clamp((((pf_1_9 * (1.0f / pf_7_15)) * -0.7f) + 0.85f), 0.0, 1.0))) + pf_12_23));
	// ({utof(u_3_12) : } + (({pf_7_16 : } * (0.f - clamp(((({pf_1_9 : 10.47933} * (1.0f / {pf_7_15 : 27.38721})) * -0.7f) + 0.85f), 0.0, 1.0))) + {pf_7_16 : }))
	pf_7_18 = (utof(u_3_12) + ((pf_7_16 * (0.f - clamp((((pf_1_9 * (1.0f / pf_7_15)) * -0.7f) + 0.85f), 0.0, 1.0))) + pf_7_16));
	// ({utof(u_10_12) : } + (({pf_14_24 : } * (0.f - clamp(((({pf_1_9 : 10.47933} * (1.0f / {pf_7_15 : 27.38721})) * -0.7f) + 0.85f), 0.0, 1.0))) + {pf_14_24 : }))
	pf_10_17 = (utof(u_10_12) + ((pf_14_24 * (0.f - clamp((((pf_1_9 * (1.0f / pf_7_15)) * -0.7f) + 0.85f), 0.0, 1.0))) + pf_14_24));
	// -0.4112923  <=>  (((((({f_10_26 : 0.00} * (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) + (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) * {(vs_cbuf8_24.z) : -0.5771194}) + (((sin({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {(vs_cbuf8_24.x) : -0.7425708}) + ((({f_10_26 : 0.00} * {pf_22_8 : 0.00}) + (0.f - {pf_18_8 : -0.1736453})) * {(vs_cbuf8_24.y) : 0.339885}))) * {utof(u_4_phi_63) : 0.62622}) + (((((({f_10_26 : 0.00} * {pf_18_8 : -0.1736453}) + (0.f - {pf_22_8 : 0.00})) * {(vs_cbuf8_24.z) : -0.5771194}) + (((cos({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {(vs_cbuf8_24.x) : -0.7425708}) + ((({f_10_26 : 0.00} * (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) + (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) * {(vs_cbuf8_24.y) : 0.339885}))) * {utof(u_5_phi_64) : -0.29746}) + ((((sin({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {(vs_cbuf8_24.z) : -0.5771194}) + (({f_10_26 : 0.00} * (0.f - {(vs_cbuf8_24.x) : -0.7425708})) + ((cos({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {(vs_cbuf8_24.y) : 0.339885}))) * {utof(u_6_phi_62) : -0.72016})))
	o.fs_attr6.x = ((((((f_10_26 * (sin(pf_19_3) * sin(pf_0_7))) + (cos(pf_19_3) * cos(pf_0_7))) * (vs_cbuf8_24.z)) + (((sin(pf_0_7) * f_7_25) * (vs_cbuf8_24.x)) + (((f_10_26 * pf_22_8) + (0.f - pf_18_8)) * (vs_cbuf8_24.y)))) * utof(u_4_phi_63)) + ((((((f_10_26 * pf_18_8) + (0.f - pf_22_8)) * (vs_cbuf8_24.z)) + (((cos(pf_0_7) * f_7_25) * (vs_cbuf8_24.x)) + (((f_10_26 * (cos(pf_19_3) * cos(pf_0_7))) + (sin(pf_19_3) * sin(pf_0_7))) * (vs_cbuf8_24.y)))) * utof(u_5_phi_64)) + ((((sin(pf_19_3) * f_7_25) * (vs_cbuf8_24.z)) + ((f_10_26 * (0.f - (vs_cbuf8_24.x))) + ((cos(pf_19_3) * f_7_25) * (vs_cbuf8_24.y)))) * utof(u_6_phi_62))));
	// (({pf_7_18 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_7_18 : })
	pf_3_26 = ((pf_7_18 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_7_18);
	// -0.1409974  <=>  (((((({f_10_26 : 0.00} * (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) + (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) * {(vs_cbuf8_25.z) : 0.5074672}) + (((sin({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {(vs_cbuf8_25.x) : 0.00}) + ((({f_10_26 : 0.00} * {pf_22_8 : 0.00}) + (0.f - {pf_18_8 : -0.1736453})) * {(vs_cbuf8_25.y) : 0.8616711}))) * {utof(u_4_phi_63) : 0.62622}) + (((((({f_10_26 : 0.00} * {pf_18_8 : -0.1736453}) + (0.f - {pf_22_8 : 0.00})) * {(vs_cbuf8_25.z) : 0.5074672}) + (((cos({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {(vs_cbuf8_25.x) : 0.00}) + ((({f_10_26 : 0.00} * (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) + (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) * {(vs_cbuf8_25.y) : 0.8616711}))) * {utof(u_5_phi_64) : -0.29746}) + ((((sin({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {(vs_cbuf8_25.z) : 0.5074672}) + (({f_10_26 : 0.00} * (0.f - {(vs_cbuf8_25.x) : 0.00})) + ((cos({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {(vs_cbuf8_25.y) : 0.8616711}))) * {utof(u_6_phi_62) : -0.72016})))
	o.fs_attr6.y = ((((((f_10_26 * (sin(pf_19_3) * sin(pf_0_7))) + (cos(pf_19_3) * cos(pf_0_7))) * (vs_cbuf8_25.z)) + (((sin(pf_0_7) * f_7_25) * (vs_cbuf8_25.x)) + (((f_10_26 * pf_22_8) + (0.f - pf_18_8)) * (vs_cbuf8_25.y)))) * utof(u_4_phi_63)) + ((((((f_10_26 * pf_18_8) + (0.f - pf_22_8)) * (vs_cbuf8_25.z)) + (((cos(pf_0_7) * f_7_25) * (vs_cbuf8_25.x)) + (((f_10_26 * (cos(pf_19_3) * cos(pf_0_7))) + (sin(pf_19_3) * sin(pf_0_7))) * (vs_cbuf8_25.y)))) * utof(u_5_phi_64)) + ((((sin(pf_19_3) * f_7_25) * (vs_cbuf8_25.z)) + ((f_10_26 * (0.f - (vs_cbuf8_25.x))) + ((cos(pf_19_3) * f_7_25) * (vs_cbuf8_25.y)))) * utof(u_6_phi_62))));
	// {pf_3_26 : }
	o.fs_attr9.x = pf_3_26;
	// 17.3305  <=>  (((({pf_2_12 : 1.00} * {(view_proj[7].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[7].z) : -1.00}) + (({pf_6_13 : 2.997681} * {(view_proj[7].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[7].x) : 0.00})))) * 0.5f) + ((0.f * (({pf_2_12 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_22 : -27.38721} * {(view_proj[6].z) : -1.000008}) + (({pf_6_13 : 2.997681} * {(view_proj[6].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[6].x) : 0.00}))))) + (((({pf_2_12 : 1.00} * {(view_proj[4].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[4].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[4].y) : 0.00}) + ({pf_8_14 : 6.029907} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * (({pf_2_12 : 1.00} * {(view_proj[5].w) : 0.00}) + (({pf_11_22 : -27.38721} * {(view_proj[5].z) : 0.00}) + (({pf_6_13 : 2.997681} * {(view_proj[5].y) : 2.144507}) + ({pf_8_14 : 6.029907} * {(view_proj[5].x) : 0.00}))))))))
	pf_2_14 = ((((pf_2_12 * (view_proj[7].w)) + ((pf_11_22 * (view_proj[7].z)) + ((pf_6_13 * (view_proj[7].y)) + (pf_8_14 * (view_proj[7].x))))) * 0.5f) + ((0.f * ((pf_2_12 * (view_proj[6].w)) + ((pf_11_22 * (view_proj[6].z)) + ((pf_6_13 * (view_proj[6].y)) + (pf_8_14 * (view_proj[6].x)))))) + ((((pf_2_12 * (view_proj[4].w)) + ((pf_11_22 * (view_proj[4].z)) + ((pf_6_13 * (view_proj[4].y)) + (pf_8_14 * (view_proj[4].x))))) * 0.5f) + (0.f * ((pf_2_12 * (view_proj[5].w)) + ((pf_11_22 * (view_proj[5].z)) + ((pf_6_13 * (view_proj[5].y)) + (pf_8_14 * (view_proj[5].x)))))))));
	// -0.9001238  <=>  (((((({f_10_26 : 0.00} * (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) + (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) * {(vs_cbuf8_26.z) : -0.6398518}) + (((sin({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {(vs_cbuf8_26.x) : 0.6697676}) + ((({f_10_26 : 0.00} * {pf_22_8 : 0.00}) + (0.f - {pf_18_8 : -0.1736453})) * {(vs_cbuf8_26.y) : 0.3768303}))) * {utof(u_4_phi_63) : 0.62622}) + (((((({f_10_26 : 0.00} * {pf_18_8 : -0.1736453}) + (0.f - {pf_22_8 : 0.00})) * {(vs_cbuf8_26.z) : -0.6398518}) + (((cos({pf_0_7 : 0.00}) * {f_7_25 : 1.00}) * {(vs_cbuf8_26.x) : 0.6697676}) + ((({f_10_26 : 0.00} * (cos({pf_19_3 : -0.17453}) * cos({pf_0_7 : 0.00}))) + (sin({pf_19_3 : -0.17453}) * sin({pf_0_7 : 0.00}))) * {(vs_cbuf8_26.y) : 0.3768303}))) * {utof(u_5_phi_64) : -0.29746}) + ((((sin({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {(vs_cbuf8_26.z) : -0.6398518}) + (({f_10_26 : 0.00} * (0.f - {(vs_cbuf8_26.x) : 0.6697676})) + ((cos({pf_19_3 : -0.17453}) * {f_7_25 : 1.00}) * {(vs_cbuf8_26.y) : 0.3768303}))) * {utof(u_6_phi_62) : -0.72016})))
	o.fs_attr6.z = ((((((f_10_26 * (sin(pf_19_3) * sin(pf_0_7))) + (cos(pf_19_3) * cos(pf_0_7))) * (vs_cbuf8_26.z)) + (((sin(pf_0_7) * f_7_25) * (vs_cbuf8_26.x)) + (((f_10_26 * pf_22_8) + (0.f - pf_18_8)) * (vs_cbuf8_26.y)))) * utof(u_4_phi_63)) + ((((((f_10_26 * pf_18_8) + (0.f - pf_22_8)) * (vs_cbuf8_26.z)) + (((cos(pf_0_7) * f_7_25) * (vs_cbuf8_26.x)) + (((f_10_26 * (cos(pf_19_3) * cos(pf_0_7))) + (sin(pf_19_3) * sin(pf_0_7))) * (vs_cbuf8_26.y)))) * utof(u_5_phi_64)) + ((((sin(pf_19_3) * f_7_25) * (vs_cbuf8_26.z)) + ((f_10_26 * (0.f - (vs_cbuf8_26.x))) + ((cos(pf_19_3) * f_7_25) * (vs_cbuf8_26.y)))) * utof(u_6_phi_62))));
	// (({pf_9_14 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_9_14 : })
	pf_3_27 = ((pf_9_14 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_9_14);
	// 17.3305  <=>  {pf_2_14 : 17.3305}
	o.fs_attr3.x = pf_2_14;
	// (({pf_10_17 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_10_17 : })
	pf_2_15 = ((pf_10_17 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_10_17);
	// {pf_3_27 : }
	o.fs_attr9.y = pf_3_27;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0.00}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0.00})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, float2((({pf_5_21 : -1906.915} + (0.f - {(vs_cbuf15_52.x) : -2116.00})) * {(vs_cbuf15_52.z) : 0.0025}), (({pf_0_16 : -3710.355} + (0.f - {(vs_cbuf15_52.y) : -3932.00})) * {(vs_cbuf15_52.z) : 0.0025})), 0.0, s_linear_clamp_sampler)
		f4_0_0 = textureLod(tex0, float2(((pf_5_21 + (0.f - (vs_cbuf15_52.x))) * (vs_cbuf15_52.z)), ((pf_0_16 + (0.f - (vs_cbuf15_52.y))) * (vs_cbuf15_52.z))), 0.0, s_linear_clamp_sampler);
		// 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
		f_2_187 = (1.0f / (vs_cbuf15_51.x));
		// 0.00  <=>  clamp(((((1.0f / ((({pf_1_12 : 27.28732} * (1.0f / {pf_7_15 : 27.38721})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {f_2_187 : 0.0010526}) + (0.f - ({f_2_187 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
		f_1_65 = clamp(((((1.0f / (((pf_1_12 * (1.0f / pf_7_15)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) * f_2_187) + (0.f - (f_2_187 * (vs_cbuf15_51.y)))), 0.0, 1.0);
		// 1.00  <=>  ((({f4_0_0.w : 1.00} * {(vs_cbuf15_49.x) : 0.00}) + (0.f - {(vs_cbuf15_49.x) : 0.00})) + 1.f)
		pf_0_22 = (((f4_0_0.w * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f);
		// -∞  <=>  log2(abs((({pf_0_22 : 1.00} * (0.f - {f_1_65 : 0.00})) + {f_1_65 : 0.00})))
		f_0_50 = log2(abs(((pf_0_22 * (0.f - f_1_65)) + f_1_65)));
		// 0.00  <=>  exp2(({f_0_50 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
		f_0_51 = exp2((f_0_50 * (vs_cbuf15_51.z)));
		// 1.00  <=>  (({pf_0_22 : 1.00} * (0.f - (({f_0_51 : 0.00} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0.00}))) + {pf_0_22 : 1.00})
		o.fs_attr7.x = ((pf_0_22 * (0.f - ((f_0_51 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_22);
	}
	// {pf_2_15 : }
	o.fs_attr9.z = pf_2_15;
	// False  <=>  if(((! {b_1_59 : True}) ? true : false))
	if(((! b_1_59) ? true : false))
	{
		return;
	}
	// 0.00  <=>  0.f
	o.vertex.x = 0.f;
	// 0.00  <=>  0.f
	o.vertex.y = 0.f;
	// 125000.00  <=>  ({(vs_cbuf8_30.y) : 25000.00} * 5.f)
	o.vertex.z = ((vs_cbuf8_30.y) * 5.f);
	// 0.00  <=>  0.f
	o.fs_attr4.x = 0.f;
	return;
}
