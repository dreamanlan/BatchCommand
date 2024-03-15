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
	// vs_cbuf8[10] = float4(0.5771239, -0.5074712, 0.6398569, 3681.669);
	// vs_cbuf8[11] = float4(0.5771194, -0.50746715, 0.6398518, 3681.84);
	// vs_cbuf8[28] = float4(-0.57711935, 0.5074672, -0.6398518, 0);
	// vs_cbuf8[29] = float4(-1919.2622, 365.7373, -3733.0469, 0);
	// vs_cbuf8[30] = float4(0.10, 25000.00, 2500.00, 24999.90);
	// vs_cbuf9[0] = float4(0, 0, 0, 0);
	// vs_cbuf9[1] = float4(73899130000000000000000.00, 6.410285E-10, 63868990000000000000000000000.00, 7.713018E+31);
	// vs_cbuf9[2] = float4(62948070000000000000000000000.00, 9.866377E-39, 0, 0);
	// vs_cbuf9[3] = float4(0, 0, 0, 0);
	// vs_cbuf9[4] = float4(0, 0, 0, 0);
	// vs_cbuf9[5] = float4(0, 0, 0, 0);
	// vs_cbuf9[6] = float4(0, 0, 0, 0);
	// vs_cbuf9[7] = float4(1E-45, 0, 4.2187E-40, 0);
	// vs_cbuf9[8] = float4(0, 4E-45, 0, 4E-45);
	// vs_cbuf9[9] = float4(1E-45, 6E-45, 0, 0);
	// vs_cbuf9[10] = float4(0, 0, 0, 0);
	// vs_cbuf9[11] = float4(0, 0, 0, 0);
	// vs_cbuf9[12] = float4(0, 0, 0, 0);
	// vs_cbuf9[13] = float4(0, 0, 0, 0);
	// vs_cbuf9[14] = float4(0, -1, 0, 0);
	// vs_cbuf9[15] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[16] = float4(0, 0, 0, 0.125);
	// vs_cbuf9[17] = float4(1.00, 1.00, 20.00, 20.00);
	// vs_cbuf9[18] = float4(0, 0, 0, 0);
	// vs_cbuf9[19] = float4(0.10, 0.10, 0, 0);
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
	// vs_cbuf9[74] = float4(0, -0.0001, 0, 0);
	// vs_cbuf9[75] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[76] = float4(4.50, 1.40, 0.10, 0.10);
	// vs_cbuf9[77] = float4(0, 0, 0, 0);
	// vs_cbuf9[78] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[79] = float4(0.0001, 0, 0, 0);
	// vs_cbuf9[80] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[81] = float4(0.80, 1.00, 0, 0);
	// vs_cbuf9[82] = float4(0, 0, 0, 0);
	// vs_cbuf9[83] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[84] = float4(0, 0, 0, 0);
	// vs_cbuf9[85] = float4(0, 0, 0, 0);
	// vs_cbuf9[86] = float4(1.10, 1.10, 0, 0);
	// vs_cbuf9[87] = float4(0, 0, 6.283185, 0);
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
	// vs_cbuf9[104] = float4(2.20, 0, 0, 0);
	// vs_cbuf9[105] = float4(0.8174603, 0.8135269, 0.7460947, 0);
	// vs_cbuf9[106] = float4(0, 0, 0, 0);
	// vs_cbuf9[107] = float4(0, 0, 0, 0);
	// vs_cbuf9[108] = float4(0, 0, 0, 0);
	// vs_cbuf9[109] = float4(0, 0, 0, 0);
	// vs_cbuf9[110] = float4(0, 0, 0, 0);
	// vs_cbuf9[111] = float4(0, 0, 0, 0);
	// vs_cbuf9[112] = float4(0, 0, 0, 0);
	// vs_cbuf9[113] = float4(0.10, 0.20, 0.20, 0);
	// vs_cbuf9[114] = float4(0.05, 0.05, 0.05, 0.40);
	// vs_cbuf9[115] = float4(0.40, 0.40, 0.40, 1.00);
	// vs_cbuf9[116] = float4(0.40, 0.40, 0.40, 4.00);
	// vs_cbuf9[117] = float4(0.40, 0.40, 0.40, 5.00);
	// vs_cbuf9[118] = float4(0.40, 0.40, 0.40, 6.00);
	// vs_cbuf9[119] = float4(0.40, 0.40, 0.40, 7.00);
	// vs_cbuf9[120] = float4(0.40, 0.40, 0.40, 8.00);
	// vs_cbuf9[121] = float4(0.271164, 0.3113047, 0.3253968, 0);
	// vs_cbuf9[122] = float4(0, 0, 0, 0);
	// vs_cbuf9[123] = float4(0, 0, 0, 0);
	// vs_cbuf9[124] = float4(0, 0, 0, 0);
	// vs_cbuf9[125] = float4(0, 0, 0, 0);
	// vs_cbuf9[126] = float4(0, 0, 0, 0);
	// vs_cbuf9[127] = float4(0, 0, 0, 0);
	// vs_cbuf9[128] = float4(0, 0, 0, 0);
	// vs_cbuf9[129] = float4(0, 0, 0, 0);
	// vs_cbuf9[130] = float4(0.50, 0.50, 0.50, 0.40);
	// vs_cbuf9[131] = float4(0, 0, 0, 1.00);
	// vs_cbuf9[132] = float4(0, 0, 0, 4.00);
	// vs_cbuf9[133] = float4(0, 0, 0, 5.00);
	// vs_cbuf9[134] = float4(0, 0, 0, 6.00);
	// vs_cbuf9[135] = float4(0, 0, 0, 7.00);
	// vs_cbuf9[136] = float4(0, 0, 0, 8.00);
	// vs_cbuf9[137] = float4(0, 0.50, 0, 1.00);
	// vs_cbuf9[138] = float4(1800.00, 2200.00, 80.00, 100.00);
	// vs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[140] = float4(1.00, 400.00, 0, 0);
	// vs_cbuf9[141] = float4(1.00, 1.00, 1.00, 0);
	// vs_cbuf9[142] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[143] = float4(1.00, 1.00, 1.00, 2.00);
	// vs_cbuf9[144] = float4(1.00, 1.00, 1.00, 3.00);
	// vs_cbuf9[145] = float4(1.00, 1.00, 1.00, 4.00);
	// vs_cbuf9[146] = float4(1.00, 1.00, 1.00, 5.00);
	// vs_cbuf9[147] = float4(1.00, 1.00, 1.00, 6.00);
	// vs_cbuf9[148] = float4(1.00, 1.00, 1.00, 7.00);
	// vs_cbuf9[149] = float4(1.00, 1.00, 1.00, 0);
	// vs_cbuf9[150] = float4(1.00, 1.00, 1.00, 0.03);
	// vs_cbuf9[151] = float4(1.00, 1.00, 1.00, 0.94);
	// vs_cbuf9[152] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[153] = float4(1.00, 1.00, 1.00, 5.00);
	// vs_cbuf9[154] = float4(1.00, 1.00, 1.00, 6.00);
	// vs_cbuf9[155] = float4(1.00, 1.00, 1.00, 7.00);
	// vs_cbuf9[156] = float4(1.00, 1.00, 1.00, 8.00);
	// vs_cbuf9[157] = float4(0, 0, 0, 0);
	// vs_cbuf9[158] = float4(0, 0, 0, 0);
	// vs_cbuf9[159] = float4(0, 0, 0, 0);
	// vs_cbuf9[160] = float4(0, 0, 0, 0);
	// vs_cbuf9[194] = float4(0, 0, 0, 0);
	// vs_cbuf9[195] = float4(0, 0, 0, 1.00);
	// vs_cbuf9[196] = float4(0, 0, 0, 0);
	// vs_cbuf9[197] = float4(4000.00, 4000.00, 0, 0);
	// vs_cbuf10[0] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[1] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[2] = float4(879.50, 102.00, 1.00, 1.00);
	// vs_cbuf10[3] = float4(1.00, 0.9999999, 1.00, 0.9999999);
	// vs_cbuf10[4] = float4(-0.74257076, 0, -0.66976756, -1919.2622);
	// vs_cbuf10[5] = float4(0, 1.00, 0, 365.7375);
	// vs_cbuf10[6] = float4(0.6697676, 0, -0.74257076, -3733.0469);
	// vs_cbuf13[0] = float4(0, 0.50, 1.00, 0.50);
	// vs_cbuf13[1] = float4(0.025, 1.00, 1.00, 0);
	// vs_cbuf13[2] = float4(100.00, 0.01, 1.00, 40.00);
	// vs_cbuf13[3] = float4(1.00, 0, 0, 1.00);
	// vs_cbuf13[5] = float4(1.00, 0, 1.00, 0);
	// vs_cbuf13[6] = float4(1.00, 1.00, 10.00, 0);
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
	// vs_cbuf16[0] = float4(0, 0, -53.610455, -268.7061);
	// vs_cbuf16[1] = float4(0.7282469, 0, 0.685315, 1.479783);

	bool b_0_13;
	bool b_0_15;
	bool b_1_53;
	bool b_1_55;
	bool b_1_57;
	bool b_1_67;
	bool b_2_23;
	bool b_2_25;
	bool b_3_4;
	bool b_3_6;
	bool b_4_9;
	bool b_5_5;
	bool b_5_6;
	float f_0_13;
	float f_0_15;
	float f_0_18;
	float f_0_19;
	float f_0_21;
	float f_0_22;
	float f_0_6;
	float f_1_17;
	float f_1_20;
	float f_1_83;
	float f_10_8;
	float f_2_102;
	float f_2_103;
	float f_2_134;
	float f_2_142;
	float f_2_24;
	float f_2_67;
	float f_2_70;
	float f_2_72;
	float f_2_79;
	float f_2_85;
	float f_3_12;
	float f_3_15;
	float f_3_19;
	float f_3_27;
	float f_3_30;
	float f_3_34;
	float f_3_36;
	float f_3_56;
	float f_3_61;
	float f_4_16;
	float f_4_22;
	float f_4_27;
	float f_4_35;
	float f_4_36;
	float f_4_40;
	float f_4_56;
	float f_4_61;
	float f_4_85;
	float f_4_88;
	float f_5_14;
	float f_5_2;
	float f_5_27;
	float f_5_3;
	float f_6_20;
	float f_6_6;
	float f_7_13;
	float f_7_16;
	float f_7_42;
	float f_8_10;
	float f_8_11;
	float f_8_13;
	float f_9_49;
	float f_9_64;
	float f_9_65;
	float4 f4_0_0;
	float4 f4_0_1;
	precise float pf_0_1;
	precise float pf_0_11;
	precise float pf_0_13;
	precise float pf_0_14;
	precise float pf_0_16;
	precise float pf_0_21;
	precise float pf_1_1;
	precise float pf_1_13;
	precise float pf_1_16;
	precise float pf_1_24;
	precise float pf_1_26;
	precise float pf_1_27;
	precise float pf_1_5;
	precise float pf_1_9;
	precise float pf_11_11;
	precise float pf_11_15;
	precise float pf_11_21;
	precise float pf_11_23;
	precise float pf_11_27;
	precise float pf_11_28;
	precise float pf_11_8;
	precise float pf_12_7;
	precise float pf_13_9;
	precise float pf_14_4;
	precise float pf_16_1;
	precise float pf_16_15;
	precise float pf_16_20;
	precise float pf_16_21;
	precise float pf_16_22;
	precise float pf_16_30;
	precise float pf_16_7;
	precise float pf_17_15;
	precise float pf_17_16;
	precise float pf_17_6;
	precise float pf_17_9;
	precise float pf_19_6;
	precise float pf_2_16;
	precise float pf_2_5;
	precise float pf_20_14;
	precise float pf_20_17;
	precise float pf_20_18;
	precise float pf_20_7;
	precise float pf_21_19;
	precise float pf_21_5;
	precise float pf_22_1;
	precise float pf_22_10;
	precise float pf_22_8;
	precise float pf_23_4;
	precise float pf_24_4;
	precise float pf_27_2;
	precise float pf_27_6;
	precise float pf_28_1;
	precise float pf_28_2;
	precise float pf_28_3;
	precise float pf_28_4;
	precise float pf_3_10;
	precise float pf_3_12;
	precise float pf_3_4;
	precise float pf_4_11;
	precise float pf_4_8;
	precise float pf_5_15;
	precise float pf_5_25;
	precise float pf_5_26;
	precise float pf_5_31;
	precise float pf_5_32;
	precise float pf_5_38;
	precise float pf_5_5;
	precise float pf_6_11;
	precise float pf_6_5;
	precise float pf_7_1;
	precise float pf_7_15;
	precise float pf_7_3;
	precise float pf_7_4;
	precise float pf_8_3;
	uint u_0_1;
	uint u_0_3;
	uint u_0_8;
	uint u_0_9;
	uint u_0_phi_88;
	uint u_1_13;
	uint u_1_14;
	uint u_1_18;
	uint u_1_19;
	uint u_1_2;
	uint u_1_20;
	uint u_1_21;
	uint u_1_23;
	uint u_1_24;
	uint u_1_25;
	uint u_1_26;
	uint u_1_28;
	uint u_1_29;
	uint u_1_30;
	uint u_1_31;
	uint u_1_32;
	uint u_1_35;
	uint u_1_36;
	uint u_1_39;
	uint u_1_7;
	uint u_1_phi_66;
	uint u_1_phi_75;
	uint u_1_phi_77;
	uint u_1_phi_79;
	uint u_1_phi_84;
	uint u_1_phi_87;
	uint u_10_0;
	uint u_10_1;
	uint u_10_16;
	uint u_10_2;
	uint u_10_21;
	uint u_10_22;
	uint u_10_29;
	uint u_10_3;
	uint u_10_30;
	uint u_10_31;
	uint u_10_37;
	uint u_10_38;
	uint u_10_phi_15;
	uint u_10_phi_17;
	uint u_10_phi_70;
	uint u_10_phi_90;
	uint u_11_1;
	uint u_11_13;
	uint u_11_18;
	uint u_11_2;
	uint u_11_4;
	uint u_11_5;
	uint u_11_phi_16;
	uint u_11_phi_20;
	uint u_12_13;
	uint u_12_23;
	uint u_12_24;
	uint u_12_25;
	uint u_12_26;
	uint u_12_27;
	uint u_12_28;
	uint u_12_3;
	uint u_12_33;
	uint u_12_34;
	uint u_12_4;
	uint u_12_phi_20;
	uint u_12_phi_29;
	uint u_12_phi_32;
	uint u_12_phi_37;
	uint u_12_phi_91;
	uint u_13_10;
	uint u_13_11;
	uint u_13_9;
	uint u_13_phi_30;
	uint u_14_10;
	uint u_14_11;
	uint u_14_13;
	uint u_14_5;
	uint u_14_6;
	uint u_14_9;
	uint u_14_phi_26;
	uint u_15_10;
	uint u_15_11;
	uint u_15_14;
	uint u_15_17;
	uint u_15_22;
	uint u_15_23;
	uint u_15_6;
	uint u_15_7;
	uint u_15_9;
	uint u_15_phi_31;
	uint u_15_phi_40;
	uint u_15_phi_89;
	uint u_16_2;
	uint u_16_3;
	uint u_16_phi_27;
	uint u_17_1;
	uint u_17_2;
	uint u_17_phi_34;
	uint u_18_10;
	uint u_18_11;
	uint u_18_15;
	uint u_18_4;
	uint u_18_9;
	uint u_19_0;
	uint u_19_1;
	uint u_19_3;
	uint u_19_6;
	uint u_19_phi_33;
	uint u_2_10;
	uint u_2_11;
	uint u_2_14;
	uint u_2_15;
	uint u_2_2;
	uint u_2_20;
	uint u_2_21;
	uint u_2_7;
	uint u_2_phi_64;
	uint u_2_phi_81;
	uint u_20_11;
	uint u_20_2;
	uint u_20_6;
	uint u_20_7;
	uint u_21_0;
	uint u_21_1;
	uint u_21_12;
	uint u_21_14;
	uint u_21_15;
	uint u_21_18;
	uint u_21_2;
	uint u_21_3;
	uint u_21_4;
	uint u_21_5;
	uint u_21_6;
	uint u_21_phi_36;
	uint u_21_phi_41;
	uint u_21_phi_48;
	uint u_22_1;
	uint u_22_2;
	uint u_22_phi_38;
	uint u_23_1;
	uint u_23_2;
	uint u_23_3;
	uint u_23_4;
	uint u_23_phi_39;
	uint u_24_19;
	uint u_24_21;
	uint u_25_12;
	uint u_25_3;
	uint u_25_4;
	uint u_25_7;
	uint u_26_2;
	uint u_26_20;
	uint u_26_3;
	uint u_26_4;
	uint u_26_5;
	uint u_26_6;
	uint u_26_7;
	uint u_26_phi_42;
	uint u_26_phi_53;
	uint u_27_0;
	uint u_27_1;
	uint u_27_15;
	uint u_27_8;
	uint u_27_phi_43;
	uint u_28_0;
	uint u_28_1;
	uint u_28_22;
	uint u_28_4;
	uint u_28_phi_44;
	uint u_29_0;
	uint u_29_1;
	uint u_29_10;
	uint u_29_18;
	uint u_29_19;
	uint u_29_4;
	uint u_29_9;
	uint u_29_phi_45;
	uint u_29_phi_54;
	uint u_3_1;
	uint u_3_11;
	uint u_3_14;
	uint u_3_17;
	uint u_3_2;
	uint u_3_3;
	uint u_3_9;
	uint u_3_phi_23;
	uint u_30_0;
	uint u_30_1;
	uint u_30_3;
	uint u_30_4;
	uint u_30_6;
	uint u_30_phi_46;
	uint u_31_6;
	uint u_32_3;
	uint u_32_6;
	uint u_32_7;
	uint u_32_8;
	uint u_32_phi_55;
	uint u_33_2;
	uint u_33_3;
	uint u_33_6;
	uint u_33_7;
	uint u_33_phi_47;
	uint u_33_phi_56;
	uint u_34_0;
	uint u_34_1;
	uint u_34_2;
	uint u_34_3;
	uint u_34_phi_47;
	uint u_34_phi_57;
	uint u_35_0;
	uint u_35_1;
	uint u_35_2;
	uint u_35_3;
	uint u_35_phi_47;
	uint u_35_phi_58;
	uint u_36_0;
	uint u_36_1;
	uint u_36_3;
	uint u_36_phi_47;
	uint u_37_0;
	uint u_37_1;
	uint u_37_16;
	uint u_37_17;
	uint u_37_phi_49;
	uint u_37_phi_58;
	uint u_38_0;
	uint u_38_1;
	uint u_38_2;
	uint u_38_4;
	uint u_38_phi_50;
	uint u_39_0;
	uint u_39_1;
	uint u_39_12;
	uint u_39_13;
	uint u_39_3;
	uint u_39_phi_51;
	uint u_39_phi_58;
	uint u_4_1;
	uint u_4_2;
	uint u_4_3;
	uint u_4_4;
	uint u_4_5;
	uint u_4_6;
	uint u_4_phi_19;
	uint u_4_phi_21;
	uint u_4_phi_22;
	uint u_40_3;
	uint u_40_4;
	uint u_40_5;
	uint u_40_6;
	uint u_40_phi_52;
	uint u_40_phi_59;
	uint u_41_1;
	uint u_41_2;
	uint u_41_phi_58;
	uint u_42_0;
	uint u_42_1;
	uint u_42_phi_58;
	uint u_43_0;
	uint u_43_1;
	uint u_43_phi_61;
	uint u_44_0;
	uint u_44_1;
	uint u_44_phi_62;
	uint u_5_1;
	uint u_5_14;
	uint u_5_19;
	uint u_5_2;
	uint u_5_20;
	uint u_5_21;
	uint u_5_23;
	uint u_5_24;
	uint u_5_26;
	uint u_5_27;
	uint u_5_28;
	uint u_5_29;
	uint u_5_30;
	uint u_5_phi_20;
	uint u_5_phi_24;
	uint u_5_phi_67;
	uint u_5_phi_78;
	uint u_5_phi_82;
	uint u_6_1;
	uint u_6_11;
	uint u_6_12;
	uint u_6_13;
	uint u_6_15;
	uint u_6_18;
	uint u_6_19;
	uint u_6_2;
	uint u_6_20;
	uint u_6_22;
	uint u_6_23;
	uint u_6_24;
	uint u_6_25;
	uint u_6_29;
	uint u_6_30;
	uint u_6_31;
	uint u_6_33;
	uint u_6_34;
	uint u_6_36;
	uint u_6_4;
	uint u_6_5;
	uint u_6_7;
	uint u_6_8;
	uint u_6_phi_11;
	uint u_6_phi_18;
	uint u_6_phi_4;
	uint u_6_phi_60;
	uint u_6_phi_65;
	uint u_6_phi_69;
	uint u_6_phi_76;
	uint u_6_phi_80;
	uint u_6_phi_85;
	uint u_7_10;
	uint u_7_3;
	uint u_7_4;
	uint u_7_5;
	uint u_7_6;
	uint u_7_7;
	uint u_7_8;
	uint u_7_9;
	uint u_7_phi_76;
	uint u_7_phi_83;
	uint u_7_phi_86;
	uint u_8_0;
	uint u_8_1;
	uint u_8_11;
	uint u_8_14;
	uint u_8_18;
	uint u_8_2;
	uint u_8_20;
	uint u_8_21;
	uint u_8_23;
	uint u_8_24;
	uint u_8_26;
	uint u_8_27;
	uint u_8_28;
	uint u_8_29;
	uint u_8_3;
	uint u_8_5;
	uint u_8_6;
	uint u_8_7;
	uint u_8_8;
	uint u_8_phi_2;
	uint u_8_phi_63;
	uint u_8_phi_68;
	uint u_8_phi_71;
	uint u_8_phi_73;
	uint u_8_phi_76;
	uint u_8_phi_9;
	uint u_9_10;
	uint u_9_12;
	uint u_9_14;
	uint u_9_16;
	uint u_9_19;
	uint u_9_20;
	uint u_9_21;
	uint u_9_22;
	uint u_9_6;
	uint u_9_phi_72;
	uint u_9_phi_74;
	// 7908.656  <=>  float(7908.65625)
	o.vertex.x = float(7908.65625);
	// 4820.093  <=>  float(4820.09326)
	o.vertex.y = float(4820.09326);
	// 5649.854  <=>  float(5649.85352)
	o.vertex.z = float(5649.85352);
	// 5650.009  <=>  float(5650.00879)
	o.vertex.w = float(5650.00879);
	// 1.79841  <=>  float(1.79841)
	o.fs_attr0.x = float(1.79841);
	// 1.78976  <=>  float(1.78976)
	o.fs_attr0.y = float(1.78976);
	// 1.64141  <=>  float(1.64141)
	o.fs_attr0.z = float(1.64141);
	// 0.10  <=>  float(0.10)
	o.fs_attr0.w = float(0.10);
	// 0.59656  <=>  float(0.59656)
	o.fs_attr1.x = float(0.59656);
	// 0.68487  <=>  float(0.68487)
	o.fs_attr1.y = float(0.68487);
	// 0.71587  <=>  float(0.71587)
	o.fs_attr1.z = float(0.71587);
	// 0.38125  <=>  float(0.38125)
	o.fs_attr1.w = float(0.38125);
	// 1.5015  <=>  float(1.5015)
	o.fs_attr2.x = float(1.5015);
	// 1.66788  <=>  float(1.66788)
	o.fs_attr2.y = float(1.66788);
	// -0.18467  <=>  float(-0.18467)
	o.fs_attr2.z = float(-0.18467);
	// 0.98535  <=>  float(0.98535)
	o.fs_attr2.w = float(0.98535);
	// 0.96338  <=>  float(0.96338)
	o.fs_attr3.x = float(0.96338);
	// 0.76525  <=>  float(0.76525)
	o.fs_attr3.y = float(0.76525);
	// 0  <=>  float(0.00)
	o.fs_attr3.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr3.w = float(1.00);
	// 6779.333  <=>  float(6779.33252)
	o.fs_attr4.x = float(6779.33252);
	// 414.9578  <=>  float(414.95776)
	o.fs_attr4.y = float(414.95776);
	// 5649.931  <=>  float(5649.93115)
	o.fs_attr4.z = float(5649.93115);
	// 5650.009  <=>  float(5650.00879)
	o.fs_attr4.w = float(5650.00879);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.x = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr5.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr5.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.w = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr6.x = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr6.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr6.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr6.w = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.x = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.y = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.z = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr7.w = float(1.00);
	// -0.66321  <=>  float(-0.66321)
	o.fs_attr8.x = float(-0.66321);
	// 0.1409  <=>  float(0.1409)
	o.fs_attr8.y = float(0.1409);
	// -0.7353  <=>  float(-0.7353)
	o.fs_attr8.z = float(-0.7353);
	// 1.00  <=>  float(1.00)
	o.fs_attr8.w = float(1.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr9.x = float(1.00);
	// 0  <=>  float(0.00)
	o.fs_attr9.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr9.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr9.w = float(1.00);
	// 0.70066  <=>  float(0.70066)
	o.fs_attr10.x = float(0.70066);
	// 0.20  <=>  float(0.20)
	o.fs_attr10.y = float(0.20);
	// 0  <=>  float(0.00)
	o.fs_attr10.z = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr10.w = float(0.00);
	// 0.06156  <=>  float(0.06156)
	o.fs_attr11.x = float(0.06156);
	// 0.06156  <=>  float(0.06156)
	o.fs_attr11.y = float(0.06156);
	// 0.06156  <=>  float(0.06156)
	o.fs_attr11.z = float(0.06156);
	// 1.00  <=>  float(1.00)
	o.fs_attr11.w = float(1.00);
	// 0.00129  <=>  float(0.00129)
	o.fs_attr12.x = float(0.00129);
	// 0.00999  <=>  float(0.00999)
	o.fs_attr12.y = float(0.00999);
	// 0.00983  <=>  float(0.00983)
	o.fs_attr12.z = float(0.00983);
	// 0.83716  <=>  float(0.83716)
	o.fs_attr12.w = float(0.83716);
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_8_0 = 0u;
	u_8_phi_2 = u_8_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_8_1 = ftou(vs_cbuf8_30.y);
		u_8_phi_2 = u_8_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_8_phi_2 : }
	u_6_1 = u_8_phi_2;
	u_6_phi_4 = u_6_1;
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_8_phi_2) : 0} * 5.f)) : 0}
		u_6_2 = ftou((utof(u_8_phi_2) * 5.f));
		u_6_phi_4 = u_6_2;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr6.x = 0.f;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {utof(u_6_phi_4) : 0}
		o.vertex.z = utof(u_6_phi_4);
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		return;
	}
	// 183.00  <=>  ((0.f - {i.vao_attr4.w : 696.50}) + {(vs_cbuf10_2.x) : 879.50})
	pf_0_1 = ((0.f - i.vao_attr4.w) + (vs_cbuf10_2.x));
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 1143873536  <=>  {ftou(i.vao_attr4.w) : 1143873536}
	u_8_2 = ftou(i.vao_attr4.w);
	u_8_phi_9 = u_8_2;
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_8_3 = ftou(vs_cbuf8_30.y);
		u_8_phi_9 = u_8_3;
	}
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 1143873536  <=>  {u_8_phi_9 : }
	u_6_4 = u_8_phi_9;
	u_6_phi_11 = u_6_4;
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1163503616  <=>  {ftou(({utof(u_8_phi_9) : 696.50} * 5.f)) : 1163503616}
		u_6_5 = ftou((utof(u_8_phi_9) * 5.f));
		u_6_phi_11 = u_6_5;
	}
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr6.x = 0.f;
	}
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 696.50  <=>  {utof(u_6_phi_11) : 696.50}
		o.vertex.z = utof(u_6_phi_11);
	}
	// False  <=>  if(((((({i.vao_attr4.w : 696.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 696.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 183.00} >= float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		return;
	}
	// 0.90784  <=>  {i.vao_attr6.x : 0.90784}
	f_0_6 = i.vao_attr6.x;
	// 184.00  <=>  ({pf_0_1 : 183.00} + {(vs_cbuf10_2.w) : 1.00})
	pf_1_1 = (pf_0_1 + (vs_cbuf10_2.w));
	// 1143873536  <=>  {u_6_phi_11 : 1143873536}
	u_10_0 = u_6_phi_11;
	u_10_phi_15 = u_10_0;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1191460864  <=>  {ftou(({pf_1_1 : 184.00} * {pf_1_1 : 184.00})) : 1191460864}
		u_10_1 = ftou((pf_1_1 * pf_1_1));
		u_10_phi_15 = u_10_1;
	}
	// 1191460864  <=>  {u_10_phi_15 : }
	u_11_1 = u_10_phi_15;
	u_11_phi_16 = u_11_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_10_phi_15) : 33856.00} * {utof(vs_cbuf9[14].w) : 0})) : 0}
		u_11_2 = ftou((utof(u_10_phi_15) * utof(vs_cbuf9[14].w)));
		u_11_phi_16 = u_11_2;
	}
	// 0  <=>  {ftou(clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0)) : 0}
	u_10_2 = ftou(clamp(min(0.f, f_0_6), 0.0, 1.0));
	u_10_phi_17 = u_10_2;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_11_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].x) : 0})) : 0}
		u_10_3 = ftou(((utof(u_11_phi_16) * 0.5f) * utof(vs_cbuf9[14].x)));
		u_10_phi_17 = u_10_3;
	}
	// 0  <=>  0u
	u_6_7 = 0u;
	u_6_phi_18 = u_6_7;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 2147483648  <=>  {ftou((({utof(u_11_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].y) : -1})) : 2147483648}
		u_6_8 = ftou(((utof(u_11_phi_16) * 0.5f) * utof(vs_cbuf9[14].y)));
		u_6_phi_18 = u_6_8;
	}
	// 0  <=>  0u
	u_4_1 = 0u;
	u_4_phi_19 = u_4_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_11_phi_16) : 0} * 0.5f) * {utof(vs_cbuf9[14].z) : 0})) : 0}
		u_4_2 = ftou(((utof(u_11_phi_16) * 0.5f) * utof(vs_cbuf9[14].z)));
		u_4_phi_19 = u_4_2;
	}
	// 2147483648  <=>  {u_6_phi_18 : }
	u_5_1 = u_6_phi_18;
	// 0  <=>  {u_4_phi_19 : }
	u_11_4 = u_4_phi_19;
	// 0  <=>  {u_10_phi_17 : }
	u_12_3 = u_10_phi_17;
	u_5_phi_20 = u_5_1;
	u_11_phi_20 = u_11_4;
	u_12_phi_20 = u_12_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 184.00}))
		f_8_10 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// ∞  <=>  ((1.0f / log2({utof(vs_cbuf9[15].x) : 1.00})) * 1.442695f)
		pf_5_5 = ((1.0f / log2(utof(vs_cbuf9[15].x))) * 1.442695f);
		// 4290772992  <=>  {ftou((((((0.f - (({pf_5_5 : ∞} * {f_8_10 : 1.00}) + (0.f - {pf_5_5 : ∞}))) + {pf_1_1 : 184.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].y) : -1})) : 4290772992}
		u_5_2 = ftou((((((0.f - ((pf_5_5 * f_8_10) + (0.f - pf_5_5))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].y)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_5_5 : ∞} * {f_8_10 : 1.00}) + (0.f - {pf_5_5 : ∞}))) + {pf_1_1 : 184.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].z) : 0})) : 4290772992}
		u_11_5 = ftou((((((0.f - ((pf_5_5 * f_8_10) + (0.f - pf_5_5))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].z)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_5_5 : ∞} * {f_8_10 : 1.00}) + (0.f - {pf_5_5 : ∞}))) + {pf_1_1 : 184.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].x) : 0})) : 4290772992}
		u_12_4 = ftou((((((0.f - ((pf_5_5 * f_8_10) + (0.f - pf_5_5))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].x)));
		u_5_phi_20 = u_5_2;
		u_11_phi_20 = u_11_5;
		u_12_phi_20 = u_12_4;
	}
	// 1127743488  <=>  {ftou(pf_1_1) : 1127743488}
	u_4_3 = ftou(pf_1_1);
	u_4_phi_21 = u_4_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// ∞  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))
		f_2_24 = (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f));
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 184.00}))
		f_8_11 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// 4290772992  <=>  {ftou((({f_8_11 : 1.00} * (0.f - {f_2_24 : ∞})) + {f_2_24 : ∞})) : 4290772992}
		u_4_4 = ftou(((f_8_11 * (0.f - f_2_24)) + f_2_24));
		u_4_phi_21 = u_4_4;
	}
	// 0.10  <=>  ({utof(vs_cbuf9[113].x) : 0.10} * {(vs_cbuf10_0.w) : 1.00})
	o.fs_attr0.w = (utof(vs_cbuf9[113].x) * (vs_cbuf10_0.w));
	// 1.798413  <=>  (({utof(vs_cbuf9[105].x) : 0.8174603} * {(vs_cbuf10_0.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 2.20})
	o.fs_attr0.x = ((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x));
	// -2551.6372  <=>  (((({utof(u_4_phi_21) : 184.00} * {i.vao_attr4.x : 0}) + {utof(u_12_phi_20) : 0}) * {i.vao_attr5.w : 1.00}) + {i.vao_attr3.x : -2551.6372})
	pf_1_5 = ((((utof(u_4_phi_21) * i.vao_attr4.x) + utof(u_12_phi_20)) * i.vao_attr5.w) + i.vao_attr3.x);
	// 1.789759  <=>  (({utof(vs_cbuf9[105].y) : 0.8135269} * {(vs_cbuf10_0.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 2.20})
	o.fs_attr0.y = ((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x));
	// 235.3523  <=>  (((({utof(u_4_phi_21) : 184.00} * {i.vao_attr4.y : 0}) + {utof(u_5_phi_20) : -0}) * {i.vao_attr5.w : 1.00}) + {i.vao_attr3.y : 235.3523})
	pf_3_4 = ((((utof(u_4_phi_21) * i.vao_attr4.y) + utof(u_5_phi_20)) * i.vao_attr5.w) + i.vao_attr3.y);
	// 1.641408  <=>  (({utof(vs_cbuf9[105].z) : 0.7460947} * {(vs_cbuf10_0.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 2.20})
	o.fs_attr0.z = ((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x));
	// 1438.32  <=>  (((({utof(u_4_phi_21) : 184.00} * {i.vao_attr4.z : 0}) + {utof(u_11_phi_20) : 0}) * {i.vao_attr5.w : 1.00}) + {i.vao_attr3.z : 1438.32})
	pf_2_5 = ((((utof(u_4_phi_21) * i.vao_attr4.z) + utof(u_11_phi_20)) * i.vao_attr5.w) + i.vao_attr3.z);
	// 0  <=>  0u
	u_4_5 = 0u;
	u_4_phi_22 = u_4_5;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[11].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].w) : 0}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[11].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].w)))) ? true : false))
	{
		// ∞  <=>  (((({f_0_6 : 0.90784} * {utof(vs_cbuf9[13].x) : 0}) * {utof(vs_cbuf9[11].w) : 0}) + {pf_0_1 : 183.00}) * (1.0f / {utof(vs_cbuf9[11].w) : 0}))
		pf_6_5 = ((((f_0_6 * utof(vs_cbuf9[13].x)) * utof(vs_cbuf9[11].w)) + pf_0_1) * (1.0f / utof(vs_cbuf9[11].w)));
		// 4290772992  <=>  {ftou(({pf_6_5 : ∞} + (0.f - floor({pf_6_5 : ∞})))) : 4290772992}
		u_4_6 = ftou((pf_6_5 + (0.f - floor(pf_6_5))));
		u_4_phi_22 = u_4_6;
	}
	// 0  <=>  {u_4_phi_22 : }
	u_3_1 = u_4_phi_22;
	u_3_phi_23 = u_3_1;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[11].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].w) : 0})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[11].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].w))))) ? true : false))
	{
		// 1050421494  <=>  {ftou(({pf_0_1 : 183.00} * (1.0f / float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))))) : 1050421494}
		u_3_2 = ftou((pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))));
		u_3_phi_23 = u_3_2;
	}
	// 0.16926  <=>  {i.vao_attr6.y : 0.16926}
	f_8_13 = i.vao_attr6.y;
	// 0.76809  <=>  {i.vao_attr6.z : 0.76809}
	f_5_2 = i.vao_attr6.z;
	// 0  <=>  ({vs_cbuf9_7_x : 1} & 268435456u)
	u_11_13 = (vs_cbuf9_7_x & 268435456u);
	// 0  <=>  ({vs_cbuf9_7_x : 1} & 1073741824u)
	u_12_13 = (vs_cbuf9_7_x & 1073741824u);
	// 1.00  <=>  floor(({f_0_6 : 0.90784} * 2.f))
	f_3_12 = floor((f_0_6 * 2.f));
	// 0  <=>  ({vs_cbuf9_7_x : 1} & 536870912u)
	u_5_14 = (vs_cbuf9_7_x & 536870912u);
	// 0.305  <=>  ({pf_0_1 : 183.00} * (1.0f / float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f))))))))
	pf_16_1 = (pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))));
	// 1.00  <=>  floor(({f_5_2 : 0.76809} * 2.f))
	f_3_15 = floor((f_5_2 * 2.f));
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_5_19 = ftou(v.uv.x);
	u_5_phi_24 = u_5_19;
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({f_0_6 : 0.90784} > 0.5f) && (! myIsNaN({f_0_6 : 0.90784}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((f_0_6 > 0.5f) && (! myIsNaN(f_0_6))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_5_20 = ftou(((0.f - v.uv.x) + 1.f));
		u_5_phi_24 = u_5_20;
	}
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({f_0_6 : 0.90784} > 0.5f) && (! myIsNaN({f_0_6 : 0.90784}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((f_0_6 > 0.5f) && (! myIsNaN(f_0_6))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 0  <=>  0u
	u_14_5 = 0u;
	u_14_phi_26 = u_14_5;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(log2(abs({utof(vs_cbuf9[195].w) : 1.00}))) : 0}
		u_14_6 = ftou(log2(abs(utof(vs_cbuf9[195].w))));
		u_14_phi_26 = u_14_6;
	}
	// 0  <=>  floor(({f_8_13 : 0.16926} * 2.f))
	f_3_19 = floor((f_8_13 * 2.f));
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_2_2 = (myIsNaN(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))));
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_16_2 = ftou(v.uv.x);
	u_16_phi_27 = u_16_2;
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_5_2 : 0.76809} > 0.5f) && (! myIsNaN({f_5_2 : 0.76809}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_5_2 > 0.5f) && (! myIsNaN(f_5_2))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_16_3 = ftou(((0.f - v.uv.x) + 1.f));
		u_16_phi_27 = u_16_3;
	}
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_5_2 : 0.76809} > 0.5f) && (! myIsNaN({f_5_2 : 0.76809}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_5_2 > 0.5f) && (! myIsNaN(f_5_2))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 0  <=>  (((((({f_0_6 : 0.90784} + {f_8_13 : 0.16926}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0}) * 2.f) + {utof(vs_cbuf9[195].x) : 0})
	pf_19_6 = ((((((f_0_6 + f_8_13) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)) * 2.f) + utof(vs_cbuf9[195].x));
	// 0  <=>  {ftou((((({f_0_6 : 0.90784} + {f_8_13 : 0.16926}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0})) : 0}
	u_12_23 = ftou(((((f_0_6 + f_8_13) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)));
	u_12_phi_29 = u_12_23;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(({pf_0_1 : 183.00} * {utof(u_14_phi_26) : 0})) : 0}
		u_12_24 = ftou((pf_0_1 * utof(u_14_phi_26)));
		u_12_phi_29 = u_12_24;
	}
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_13_9 = ftou(v.uv.x);
	u_13_phi_30 = u_13_9;
	// False  <=>  if(((! (((~ (((16u & {vs_cbuf9_7_y : 0}) == 16u) ? 4294967295u : 0u)) | (~ (((({f_8_13 : 0.16926} > 0.5f) && (! myIsNaN({f_8_13 : 0.16926}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((16u & vs_cbuf9_7_y) == 16u) ? 4294967295u : 0u)) | (~ ((((f_8_13 > 0.5f) && (! myIsNaN(f_8_13))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_13_10 = ftou(((0.f - v.uv.x) + 1.f));
		u_13_phi_30 = u_13_10;
	}
	// 5438.32  <=>  ((({pf_2_5 : 1438.32} * {i.vao_attr10.z : 1.00}) + (({pf_3_4 : 235.3523} * {i.vao_attr10.y : 0}) + ({pf_1_5 : -2551.6372} * {i.vao_attr10.x : 0}))) + {i.vao_attr10.w : 4000.00})
	pf_8_3 = (((pf_2_5 * i.vao_attr10.z) + ((pf_3_4 * i.vao_attr10.y) + (pf_1_5 * i.vao_attr10.x))) + i.vao_attr10.w);
	// 0  <=>  {utof(vs_cbuf9[129].w) : 0}
	f_3_27 = utof(vs_cbuf9[129].w);
	// 1065353216  <=>  (((({utof(u_3_phi_23) : 0.305} >= {f_3_27 : 0}) && (! myIsNaN({utof(u_3_phi_23) : 0.305}))) && (! myIsNaN({f_3_27 : 0}))) ? 1065353216u : 0u)
	u_9_6 = ((((utof(u_3_phi_23) >= f_3_27) && (! myIsNaN(utof(u_3_phi_23)))) && (! myIsNaN(f_3_27))) ? 1065353216u : 0u);
	// 0.40  <=>  {utof(vs_cbuf9[130].w) : 0.40}
	f_3_30 = utof(vs_cbuf9[130].w);
	// 0  <=>  (((({utof(u_3_phi_23) : 0.305} >= {f_3_30 : 0.40}) && (! myIsNaN({utof(u_3_phi_23) : 0.305}))) && (! myIsNaN({f_3_30 : 0.40}))) ? 1065353216u : 0u)
	u_14_9 = ((((utof(u_3_phi_23) >= f_3_30) && (! myIsNaN(utof(u_3_phi_23)))) && (! myIsNaN(f_3_30))) ? 1065353216u : 0u);
	// 235.3523  <=>  ((({pf_2_5 : 1438.32} * {i.vao_attr9.z : 0}) + (({pf_3_4 : 235.3523} * {i.vao_attr9.y : 1.00}) + ({pf_1_5 : -2551.6372} * {i.vao_attr9.x : 0}))) + {i.vao_attr9.w : 0})
	pf_6_11 = (((pf_2_5 * i.vao_attr9.z) + ((pf_3_4 * i.vao_attr9.y) + (pf_1_5 * i.vao_attr9.x))) + i.vao_attr9.w);
	// 0  <=>  0u
	u_15_6 = 0u;
	u_15_phi_31 = u_15_6;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {u_12_phi_29 : }
		u_15_7 = u_12_phi_29;
		u_15_phi_31 = u_15_7;
	}
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[83].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[83].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_1_2 = (myIsNaN(utof(vs_cbuf9[83].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[83].z)), float(-2147483600.f), float(2147483600.f))));
	// 0  <=>  (((((({f_0_6 : 0.90784} + {f_5_2 : 0.76809}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].z) : 0}) * 2.f) + {utof(vs_cbuf9[195].z) : 0})
	pf_20_7 = ((((((f_0_6 + f_5_2) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].z)) * 2.f) + utof(vs_cbuf9[195].z));
	// 0  <=>  0u
	u_12_25 = 0u;
	u_12_phi_32 = u_12_25;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 1.00  <=>  exp2({utof(u_15_phi_31) : 0})
		f_3_34 = exp2(utof(u_15_phi_31));
		// 1065353216  <=>  {ftou(f_3_34) : 1065353216}
		u_12_26 = ftou(f_3_34);
		u_12_phi_32 = u_12_26;
	}
	// 0  <=>  (((((({f_8_13 : 0.16926} + {f_5_2 : 0.76809}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].y) : 0}) * 2.f) + {utof(vs_cbuf9[195].y) : 0})
	pf_21_5 = ((((((f_8_13 + f_5_2) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].y)) * 2.f) + utof(vs_cbuf9[195].y));
	// 0  <=>  {utof(vs_cbuf9[149].w) : 0}
	f_3_36 = utof(vs_cbuf9[149].w);
	// 1065353216  <=>  (((({pf_16_1 : 0.305} >= {f_3_36 : 0}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_3_36 : 0}))) ? 1065353216u : 0u)
	u_0_1 = ((((pf_16_1 >= f_3_36) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_3_36))) ? 1065353216u : 0u);
	// 0.03  <=>  {utof(vs_cbuf9[150].w) : 0.03}
	f_9_49 = utof(vs_cbuf9[150].w);
	// 1065353216  <=>  (((({pf_16_1 : 0.305} >= {f_9_49 : 0.03}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_9_49 : 0.03}))) ? 1065353216u : 0u)
	u_15_9 = ((((pf_16_1 >= f_9_49) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_9_49))) ? 1065353216u : 0u);
	// 1065353216  <=>  {vs_cbuf9[195].w : 1065353216}
	u_19_0 = vs_cbuf9[195].w;
	u_19_phi_33 = u_19_0;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 0  <=>  {ftou(((0.f - {utof(vs_cbuf9[195].w) : 1.00}) + 1.f)) : 0}
		u_19_1 = ftou(((0.f - utof(vs_cbuf9[195].w)) + 1.f));
		u_19_phi_33 = u_19_1;
	}
	// 1065353216  <=>  {u_19_phi_33 : }
	u_17_1 = u_19_phi_33;
	u_17_phi_34 = u_17_1;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1065353216  <=>  {ftou((1.0f / {utof(u_19_phi_33) : 1.00})) : 1065353216}
		u_17_2 = ftou((1.0f / utof(u_19_phi_33)));
		u_17_phi_34 = u_17_2;
	}
	// -2551.6372  <=>  ((({pf_2_5 : 1438.32} * {i.vao_attr8.z : 0}) + (({pf_3_4 : 235.3523} * {i.vao_attr8.y : 0}) + ({pf_1_5 : -2551.6372} * {i.vao_attr8.x : 1.00}))) + {i.vao_attr8.w : 0})
	pf_1_9 = (((pf_2_5 * i.vao_attr8.z) + ((pf_3_4 * i.vao_attr8.y) + (pf_1_5 * i.vao_attr8.x))) + i.vao_attr8.w);
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[88].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[88].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_0_3 = (myIsNaN(utof(vs_cbuf9[88].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[88].z)), float(-2147483600.f), float(2147483600.f))));
	// 0.94  <=>  {utof(vs_cbuf9[151].w) : 0.94}
	f_6_6 = utof(vs_cbuf9[151].w);
	// 0  <=>  (((({pf_16_1 : 0.305} >= {f_6_6 : 0.94}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_6_6 : 0.94}))) ? 1065353216u : 0u)
	u_20_2 = ((((pf_16_1 >= f_6_6) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_6))) ? 1065353216u : 0u);
	// False  <=>  if(((! (((~ (((16u & {vs_cbuf9_7_y : 0}) == 16u) ? 4294967295u : 0u)) | (~ (((({f_8_13 : 0.16926} > 0.5f) && (! myIsNaN({f_8_13 : 0.16926}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((16u & vs_cbuf9_7_y) == 16u) ? 4294967295u : 0u)) | (~ ((((f_8_13 > 0.5f) && (! myIsNaN(f_8_13))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 1065353216  <=>  {u_12_phi_32 : }
	u_21_0 = u_12_phi_32;
	u_21_phi_36 = u_21_0;
	// False  <=>  if(((((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) ? true : false))
	if(((((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w)))) ? true : false))
	{
		// 1065353216  <=>  1065353216u
		u_21_1 = 1065353216u;
		u_21_phi_36 = u_21_1;
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_12_27 = ftou(v.uv.y);
	u_12_phi_37 = u_12_27;
	// False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 0}) == 2u) ? 4294967295u : 0u)) | (~ (((({f_8_13 : 0.16926} > 0.5f) && (! myIsNaN({f_8_13 : 0.16926}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((f_8_13 > 0.5f) && (! myIsNaN(f_8_13))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_12_28 = ftou(((0.f - v.uv.y) + 1.f));
		u_12_phi_37 = u_12_28;
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_22_1 = ftou(v.uv.y);
	u_22_phi_38 = u_22_1;
	// False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 0}) == 8u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr6.w : 0.07008} > 0.5f) && (! myIsNaN({i.vao_attr6.w : 0.07008}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr6.w > 0.5f) && (! myIsNaN(i.vao_attr6.w))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_22_2 = ftou(((0.f - v.uv.y) + 1.f));
		u_22_phi_38 = u_22_2;
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_23_1 = ftou(v.uv.y);
	u_23_phi_39 = u_23_1;
	// False  <=>  if(((! (((~ (((32u & {vs_cbuf9_7_y : 0}) == 32u) ? 4294967295u : 0u)) | (~ (((({f_5_2 : 0.76809} > 0.5f) && (! myIsNaN({f_5_2 : 0.76809}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((32u & vs_cbuf9_7_y) == 32u) ? 4294967295u : 0u)) | (~ ((((f_5_2 > 0.5f) && (! myIsNaN(f_5_2))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_23_2 = ftou(((0.f - v.uv.y) + 1.f));
		u_23_phi_39 = u_23_2;
	}
	// 1065353216  <=>  {u_9_6 : 1065353216}
	u_15_10 = u_9_6;
	u_15_phi_40 = u_15_10;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_17_phi_34) : 1.00} * (0.f - {utof(u_21_phi_36) : 1.00})) + {utof(u_17_phi_34) : 1.00})) : 0}
		u_15_11 = ftou(((utof(u_17_phi_34) * (0.f - utof(u_21_phi_36))) + utof(u_17_phi_34)));
		u_15_phi_40 = u_15_11;
	}
	// 0  <=>  (((0.f - {utof((((({f_3_19 : 0} < 0.f) && (! myIsNaN({f_3_19 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_3_19 : 0} > 0.f) && (! myIsNaN({f_3_19 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) * float(int(abs(int((uint((int(0) - int(((int({u_12_13 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_12_13 : 0}) >= int(0u)) ? 0u : 1u))))))))))
	pf_23_4 = (((0.f - utof(((((f_3_19 < 0.f) && (! myIsNaN(f_3_19))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_19 > 0.f) && (! myIsNaN(f_3_19))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_12_13) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_12_13) >= int(0u)) ? 0u : 1u))))))))));
	// 1.00  <=>  {utof(vs_cbuf9[131].w) : 1.00}
	f_4_16 = utof(vs_cbuf9[131].w);
	// 0  <=>  (((({utof(u_3_phi_23) : 0.305} >= {f_4_16 : 1.00}) && (! myIsNaN({utof(u_3_phi_23) : 0.305}))) && (! myIsNaN({f_4_16 : 1.00}))) ? 1065353216u : 0u)
	u_3_3 = ((((utof(u_3_phi_23) >= f_4_16) && (! myIsNaN(utof(u_3_phi_23)))) && (! myIsNaN(f_4_16))) ? 1065353216u : 0u);
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_1_2 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_9_10 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_1_2), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_2_2 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_18_4 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_2_2), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_2_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_14_10 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_2_2))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 1.00  <=>  {utof(vs_cbuf9[152].w) : 1.00}
	f_4_22 = utof(vs_cbuf9[152].w);
	// 0  <=>  (((({pf_16_1 : 0.305} >= {f_4_22 : 1.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_4_22 : 1.00}))) ? 1065353216u : 0u)
	u_19_3 = ((((pf_16_1 >= f_4_22) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_4_22))) ? 1065353216u : 0u);
	// 0  <=>  (((0.f - {utof((((({f_3_15 : 1.00} < 0.f) && (! myIsNaN({f_3_15 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_3_15 : 1.00} > 0.f) && (! myIsNaN({f_3_15 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 1.00}) * float(int(abs(int((uint((int(0) - int(((int({u_11_13 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_11_13 : 0}) >= int(0u)) ? 0u : 1u))))))))))
	pf_14_4 = (((0.f - utof(((((f_3_15 < 0.f) && (! myIsNaN(f_3_15))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_15 > 0.f) && (! myIsNaN(f_3_15))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_11_13) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_11_13) >= int(0u)) ? 0u : 1u))))))))));
	// 0  <=>  (((0.f - {utof((((({f_3_12 : 1.00} < 0.f) && (! myIsNaN({f_3_12 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_3_12 : 1.00} > 0.f) && (! myIsNaN({f_3_12 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 1.00}) * float(int(abs(int((uint((int(0) - int(((int({u_5_14 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_5_14 : 0}) >= int(0u)) ? 0u : 1u))))))))))
	pf_22_1 = (((0.f - utof(((((f_3_12 < 0.f) && (! myIsNaN(f_3_12))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_12 > 0.f) && (! myIsNaN(f_3_12))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_5_14) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_5_14) >= int(0u)) ? 0u : 1u))))))))));
	// 0  <=>  0u
	u_21_2 = 0u;
	u_21_phi_41 = u_21_2;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 1  <=>  1u
		u_21_3 = 1u;
		u_21_phi_41 = u_21_3;
	}
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_10 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_14_10 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_25_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_14_10), int(0u), int(16u))), int(16u), int(16u));
	// 1065353216  <=>  {u_15_phi_40 : }
	u_26_2 = u_15_phi_40;
	u_26_phi_42 = u_26_2;
	// True  <=>  if((((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1127677952  <=>  {ftou(pf_0_1) : 1127677952}
		u_26_3 = ftou(pf_0_1);
		u_26_phi_42 = u_26_3;
	}
	// 0  <=>  (({u_25_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_10 : 0}), int(0u), int(16u))))))
	u_15_14 = ((u_25_3 << 16u) + uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_10), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_0_3 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_15_17 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_0_3), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_1_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_25_4 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_1_2))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
	u_27_0 = ftou(f_0_6);
	u_27_phi_43 = u_27_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
		u_27_1 = ftou(f_8_13);
		u_27_phi_43 = u_27_1;
	}
	// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
	u_28_0 = ftou(f_8_13);
	u_28_phi_44 = u_28_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
		u_28_1 = ftou(f_5_2);
		u_28_phi_44 = u_28_1;
	}
	// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
	u_29_0 = ftou(f_0_6);
	u_29_phi_45 = u_29_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
		u_29_1 = ftou(f_8_13);
		u_29_phi_45 = u_29_1;
	}
	// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
	u_30_0 = ftou(f_8_13);
	u_30_phi_46 = u_30_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
		u_30_1 = ftou(f_5_2);
		u_30_phi_46 = u_30_1;
	}
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_25_4 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_25_4 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_32_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_25_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_25_4), int(0u), int(16u))), int(16u), int(16u));
	// 1061462412  <=>  {u_30_phi_46 : }
	u_33_2 = u_30_phi_46;
	// 1061462412  <=>  {u_28_phi_44 : }
	u_34_0 = u_28_phi_44;
	// 1043157630  <=>  {u_29_phi_45 : }
	u_35_0 = u_29_phi_45;
	// 1043157630  <=>  {u_27_phi_43 : }
	u_36_0 = u_27_phi_43;
	u_33_phi_47 = u_33_2;
	u_34_phi_47 = u_34_0;
	u_35_phi_47 = u_35_0;
	u_36_phi_47 = u_36_0;
	// False  <=>  if(((! ({u_21_phi_41 : 1} == 1u)) ? true : false))
	if(((! (u_21_phi_41 == 1u)) ? true : false))
	{
		// 1043157630  <=>  {u_27_phi_43 : }
		u_21_4 = u_27_phi_43;
		u_21_phi_48 = u_21_4;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
			u_21_5 = ftou(f_5_2);
			u_21_phi_48 = u_21_5;
		}
		// 1061462412  <=>  {u_28_phi_44 : }
		u_37_0 = u_28_phi_44;
		u_37_phi_49 = u_37_0;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
			u_37_1 = ftou(f_0_6);
			u_37_phi_49 = u_37_1;
		}
		// 1043157630  <=>  {u_29_phi_45 : }
		u_38_0 = u_29_phi_45;
		u_38_phi_50 = u_38_0;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
			u_38_1 = ftou(f_0_6);
			u_38_phi_50 = u_38_1;
		}
		// 1061462412  <=>  {u_30_phi_46 : }
		u_39_0 = u_30_phi_46;
		u_39_phi_51 = u_39_0;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
			u_39_1 = ftou(f_8_13);
			u_39_phi_51 = u_39_1;
		}
		// 1061462412  <=>  {u_39_phi_51 : }
		u_33_3 = u_39_phi_51;
		// 1061462412  <=>  {u_37_phi_49 : }
		u_34_1 = u_37_phi_49;
		// 1043157630  <=>  {u_38_phi_50 : }
		u_35_1 = u_38_phi_50;
		// 1043157630  <=>  {u_21_phi_48 : }
		u_36_1 = u_21_phi_48;
		u_33_phi_47 = u_33_3;
		u_34_phi_47 = u_34_1;
		u_35_phi_47 = u_35_1;
		u_36_phi_47 = u_36_1;
	}
	// 0  <=>  clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_0_3 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f))
	f_4_27 = clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_0_3))) + 4294967294u)))), float(0.f), float(4294967300.f));
	// 0  <=>  uint({f_4_27 : 0})
	u_21_6 = uint(f_4_27);
	// 0  <=>  (({u_32_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_25_4 : 0}), int(0u), int(16u))))))
	u_28_4 = ((u_32_3 << 16u) + uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_25_4), int(0u), int(16u))))));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_9_10 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_67 = utof((ftou((1.0f / float(u_9_10))) + 4294967294u));
	// 0  <=>  uint(clamp(trunc((float(0u) * {f_2_67 : 0.9999999})), float(0.f), float(4294967300.f)))
	u_30_3 = uint(clamp(trunc((float(0u) * f_2_67)), float(0.f), float(4294967300.f)));
	// 5.00  <=>  {utof(vs_cbuf9[153].w) : 5.00}
	f_2_70 = utof(vs_cbuf9[153].w);
	// 0  <=>  (((({pf_16_1 : 0.305} >= {f_2_70 : 5.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_2_70 : 5.00}))) ? 1065353216u : 0u)
	u_32_6 = ((((pf_16_1 >= f_2_70) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_2_70))) ? 1065353216u : 0u);
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_18_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_72 = utof((ftou((1.0f / float(u_18_4))) + 4294967294u));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_6 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_21_6 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_38_2 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_6), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_21_6), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_6 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_21_6 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_6 : 0}), int(0u), int(16u))))))
	u_29_4 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_6), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_21_6), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_6), int(0u), int(16u))))));
	// 0  <=>  uint(clamp(trunc(({f_2_72 : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
	u_31_6 = uint(clamp(trunc((f_2_72 * float(0u))), float(0.f), float(4294967300.f)));
	// 0  <=>  ((((({pf_19_6 : 0} * {pf_14_4 : 0}) * -2.f) + {pf_19_6 : 0}) * {utof(u_26_phi_42) : 183.00}) + (({f_0_6 : 0.90784} + -0.5f) * {utof(vs_cbuf9[194].x) : 0}))
	pf_12_7 = (((((pf_19_6 * pf_14_4) * -2.f) + pf_19_6) * utof(u_26_phi_42)) + ((f_0_6 + -0.5f) * utof(vs_cbuf9[194].x)));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_15_17 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_79 = utof((ftou((1.0f / float(u_15_17))) + 4294967294u));
	// 0  <=>  uint(clamp(trunc((float(0u) * {f_2_79 : 0.9999999})), float(0.f), float(4294967300.f)))
	u_38_4 = uint(clamp(trunc((float(0u) * f_2_79)), float(0.f), float(4294967300.f)));
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_30_3 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_30_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_39_3 = uint((uint(bitfieldExtract(uint(u_30_3), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_30_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 0  <=>  ((((({pf_20_7 : 0} * {pf_23_4 : 0}) * -2.f) + {pf_20_7 : 0}) * {utof(u_26_phi_42) : 183.00}) + (({f_5_2 : 0.76809} + -0.5f) * {utof(vs_cbuf9[194].z) : 0}))
	pf_13_9 = (((((pf_20_7 * pf_23_4) * -2.f) + pf_20_7) * utof(u_26_phi_42)) + ((f_5_2 + -0.5f) * utof(vs_cbuf9[194].z)));
	// 6.00  <=>  {utof(vs_cbuf9[154].w) : 6.00}
	f_2_85 = utof(vs_cbuf9[154].w);
	// 0  <=>  (((({pf_16_1 : 0.305} >= {f_2_85 : 6.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_2_85 : 6.00}))) ? 1065353216u : 0u)
	u_26_4 = ((((pf_16_1 >= f_2_85) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_2_85))) ? 1065353216u : 0u);
	// 0  <=>  0u
	u_40_3 = 0u;
	u_40_phi_52 = u_40_3;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 2  <=>  2u
		u_40_4 = 2u;
		u_40_phi_52 = u_40_4;
	}
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_31_6 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_31_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_36_3 = uint((uint(bitfieldExtract(uint(u_31_6), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_31_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(16u), int(16u))) * {u_38_2 : 0})) << 16u) + {u_29_4 : 0}))))
	u_3_9 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_0_3), int(16u), int(16u))) * u_38_2)) << 16u) + u_29_4))));
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_38_4 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_38_4 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_17 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_27_8 = uint((uint(bitfieldExtract(uint(u_38_4), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_38_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 7.00  <=>  {utof(vs_cbuf9[155].w) : 7.00}
	f_6_20 = utof(vs_cbuf9[155].w);
	// 0  <=>  {utof(u_26_4) : 0}
	f_9_64 = utof(u_26_4);
	// 0  <=>  (0.f - {utof((((({pf_16_1 : 0.305} >= {f_6_20 : 7.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_6_20 : 7.00}))) ? 1065353216u : 0u)) : 0})
	f_7_13 = (0.f - utof(((((pf_16_1 >= f_6_20) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_20))) ? 1065353216u : 0u)));
	// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
	u_26_5 = ftou(f_0_6);
	u_26_phi_53 = u_26_5;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
		u_26_6 = ftou(f_8_13);
		u_26_phi_53 = u_26_6;
	}
	// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
	u_29_9 = ftou(f_8_13);
	u_29_phi_54 = u_29_9;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
		u_29_10 = ftou(f_5_2);
		u_29_phi_54 = u_29_10;
	}
	// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
	u_32_7 = ftou(f_5_2);
	u_32_phi_55 = u_32_7;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
		u_32_8 = ftou(f_0_6);
		u_32_phi_55 = u_32_8;
	}
	// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
	u_33_6 = ftou(f_0_6);
	u_33_phi_56 = u_33_6;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
		u_33_7 = ftou(f_8_13);
		u_33_phi_56 = u_33_7;
	}
	// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
	u_34_2 = ftou(f_8_13);
	u_34_phi_57 = u_34_2;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
		u_34_3 = ftou(f_5_2);
		u_34_phi_57 = u_34_3;
	}
	// 1063807028  <=>  {u_26_phi_53 : }
	u_35_2 = u_26_phi_53;
	// 1043157630  <=>  {u_29_phi_54 : }
	u_37_16 = u_29_phi_54;
	// 1063807028  <=>  {u_33_phi_56 : }
	u_39_12 = u_33_phi_56;
	// 1043157630  <=>  {u_34_phi_57 : }
	u_41_1 = u_34_phi_57;
	// 1061462412  <=>  {u_32_phi_55 : }
	u_42_0 = u_32_phi_55;
	u_35_phi_58 = u_35_2;
	u_37_phi_58 = u_37_16;
	u_39_phi_58 = u_39_12;
	u_41_phi_58 = u_41_1;
	u_42_phi_58 = u_42_0;
	// True  <=>  if(((! ({u_40_phi_52 : 2} == 1u)) ? true : false))
	if(((! (u_40_phi_52 == 1u)) ? true : false))
	{
		// 1063807028  <=>  {u_26_phi_53 : }
		u_40_5 = u_26_phi_53;
		u_40_phi_59 = u_40_5;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1061462412  <=>  {ftou(f_5_2) : 1061462412}
			u_40_6 = ftou(f_5_2);
			u_40_phi_59 = u_40_6;
		}
		// 1043157630  <=>  {u_29_phi_54 : }
		u_6_11 = u_29_phi_54;
		u_6_phi_60 = u_6_11;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
			u_6_12 = ftou(f_0_6);
			u_6_phi_60 = u_6_12;
		}
		// 1061462412  <=>  {u_32_phi_55 : }
		u_43_0 = u_32_phi_55;
		u_43_phi_61 = u_43_0;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
			u_43_1 = ftou(f_8_13);
			u_43_phi_61 = u_43_1;
		}
		// 1063807028  <=>  {u_33_phi_56 : }
		u_44_0 = u_33_phi_56;
		u_44_phi_62 = u_44_0;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1063807028  <=>  {ftou(f_0_6) : 1063807028}
			u_44_1 = ftou(f_0_6);
			u_44_phi_62 = u_44_1;
		}
		// 1043157630  <=>  {u_34_phi_57 : }
		u_8_5 = u_34_phi_57;
		u_8_phi_63 = u_8_5;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1043157630  <=>  {ftou(f_8_13) : 1043157630}
			u_8_6 = ftou(f_8_13);
			u_8_phi_63 = u_8_6;
		}
		// 1061462412  <=>  {u_40_phi_59 : }
		u_35_3 = u_40_phi_59;
		// 1063807028  <=>  {u_6_phi_60 : }
		u_37_17 = u_6_phi_60;
		// 1063807028  <=>  {u_44_phi_62 : }
		u_39_13 = u_44_phi_62;
		// 1043157630  <=>  {u_8_phi_63 : }
		u_41_2 = u_8_phi_63;
		// 1043157630  <=>  {u_43_phi_61 : }
		u_42_1 = u_43_phi_61;
		u_35_phi_58 = u_35_3;
		u_37_phi_58 = u_37_17;
		u_39_phi_58 = u_39_13;
		u_41_phi_58 = u_41_2;
		u_42_phi_58 = u_42_1;
	}
	// 1.00  <=>  (((((((0.f - {utof(vs_cbuf9[154].x) : 1.00}) + {utof(vs_cbuf9[155].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[154].w) : 6.00}) + {utof(vs_cbuf9[155].w) : 7.00}))) * ({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[154].w) : 6.00}))) + {utof(vs_cbuf9[154].x) : 1.00}) * (({f_9_64 : 0} * {f_7_13 : 0}) + {utof(u_26_4) : 0})) + (((((((0.f - {utof(vs_cbuf9[153].x) : 1.00}) + {utof(vs_cbuf9[154].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[153].w) : 5.00}) + {utof(vs_cbuf9[154].w) : 6.00}))) * ({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[153].w) : 5.00}))) + {utof(vs_cbuf9[153].x) : 1.00}) * (({utof(u_32_6) : 0} * (0.f - {utof(u_26_4) : 0})) + {utof(u_32_6) : 0})) + (((((((0.f - {utof(vs_cbuf9[152].x) : 1.00}) + {utof(vs_cbuf9[153].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[152].w) : 1.00}) + {utof(vs_cbuf9[153].w) : 5.00}))) * ({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[152].w) : 1.00}))) + {utof(vs_cbuf9[152].x) : 1.00}) * (({utof(u_19_3) : 0} * (0.f - {utof(u_32_6) : 0})) + {utof(u_19_3) : 0})) + (((((((0.f - {utof(vs_cbuf9[151].x) : 1.00}) + {utof(vs_cbuf9[152].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[151].w) : 0.94}) + {utof(vs_cbuf9[152].w) : 1.00}))) * ({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[151].w) : 0.94}))) + {utof(vs_cbuf9[151].x) : 1.00}) * (({utof(u_20_2) : 0} * (0.f - {utof(u_19_3) : 0})) + {utof(u_20_2) : 0})) + (((((((0.f - {utof(vs_cbuf9[150].x) : 1.00}) + {utof(vs_cbuf9[151].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[150].w) : 0.03}) + {utof(vs_cbuf9[151].w) : 0.94}))) * ({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[150].w) : 0.03}))) + {utof(vs_cbuf9[150].x) : 1.00}) * (({utof(u_15_9) : 1.00} * (0.f - {utof(u_20_2) : 0})) + {utof(u_15_9) : 1.00})) + ((((({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[149].w) : 0})) * (((0.f - {utof(vs_cbuf9[149].x) : 1.00}) + {utof(vs_cbuf9[150].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[149].w) : 0}) + {utof(vs_cbuf9[150].w) : 0.03})))) + {utof(vs_cbuf9[149].x) : 1.00}) * (({utof(u_0_1) : 1.00} * (0.f - {utof(u_15_9) : 1.00})) + {utof(u_0_1) : 1.00})) + (({utof(u_0_1) : 1.00} * (0.f - {utof(vs_cbuf9[149].x) : 1.00})) + {utof(vs_cbuf9[149].x) : 1.00})))))))
	pf_2_16 = (((((((0.f - utof(vs_cbuf9[154].x)) + utof(vs_cbuf9[155].x)) * (1.0f / ((0.f - utof(vs_cbuf9[154].w)) + utof(vs_cbuf9[155].w)))) * (pf_16_1 + (0.f - utof(vs_cbuf9[154].w)))) + utof(vs_cbuf9[154].x)) * ((f_9_64 * f_7_13) + utof(u_26_4))) + (((((((0.f - utof(vs_cbuf9[153].x)) + utof(vs_cbuf9[154].x)) * (1.0f / ((0.f - utof(vs_cbuf9[153].w)) + utof(vs_cbuf9[154].w)))) * (pf_16_1 + (0.f - utof(vs_cbuf9[153].w)))) + utof(vs_cbuf9[153].x)) * ((utof(u_32_6) * (0.f - utof(u_26_4))) + utof(u_32_6))) + (((((((0.f - utof(vs_cbuf9[152].x)) + utof(vs_cbuf9[153].x)) * (1.0f / ((0.f - utof(vs_cbuf9[152].w)) + utof(vs_cbuf9[153].w)))) * (pf_16_1 + (0.f - utof(vs_cbuf9[152].w)))) + utof(vs_cbuf9[152].x)) * ((utof(u_19_3) * (0.f - utof(u_32_6))) + utof(u_19_3))) + (((((((0.f - utof(vs_cbuf9[151].x)) + utof(vs_cbuf9[152].x)) * (1.0f / ((0.f - utof(vs_cbuf9[151].w)) + utof(vs_cbuf9[152].w)))) * (pf_16_1 + (0.f - utof(vs_cbuf9[151].w)))) + utof(vs_cbuf9[151].x)) * ((utof(u_20_2) * (0.f - utof(u_19_3))) + utof(u_20_2))) + (((((((0.f - utof(vs_cbuf9[150].x)) + utof(vs_cbuf9[151].x)) * (1.0f / ((0.f - utof(vs_cbuf9[150].w)) + utof(vs_cbuf9[151].w)))) * (pf_16_1 + (0.f - utof(vs_cbuf9[150].w)))) + utof(vs_cbuf9[150].x)) * ((utof(u_15_9) * (0.f - utof(u_20_2))) + utof(u_15_9))) + (((((pf_16_1 + (0.f - utof(vs_cbuf9[149].w))) * (((0.f - utof(vs_cbuf9[149].x)) + utof(vs_cbuf9[150].x)) * (1.0f / ((0.f - utof(vs_cbuf9[149].w)) + utof(vs_cbuf9[150].w))))) + utof(vs_cbuf9[149].x)) * ((utof(u_0_1) * (0.f - utof(u_15_9))) + utof(u_0_1))) + ((utof(u_0_1) * (0.f - utof(vs_cbuf9[149].x))) + utof(vs_cbuf9[149].x))))))));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_2_2 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_1_53 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_2_2), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_1_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_32_3 : 0}), int(16u), int(16u))))) << 16u) + {u_28_4 : 0}))))))), float(0.f), float(4294967300.f)))
	u_6_13 = uint(clamp(trunc((utof((ftou((1.0f / float(u_1_2))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_1_2), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_32_3), int(16u), int(16u))))) << 16u) + u_28_4))))))), float(0.f), float(4294967300.f)));
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_2_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_25_3 : 0}), int(16u), int(16u))))) << 16u) + {u_15_14 : 0}))))))), float(0.f), float(4294967300.f)))
	u_8_7 = uint(clamp(trunc((utof((ftou((1.0f / float(u_2_2))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_2_2), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_25_3), int(16u), int(16u))))) << 16u) + u_15_14))))))), float(0.f), float(4294967300.f)));
	// 0  <=>  uint((int(0) - int((({u_36_3 : 0} << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_31_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_31_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))))))))))
	u_10_16 = uint((int(0) - int(((u_36_3 << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_31_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_31_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))))))))));
	// 8.00  <=>  {utof(vs_cbuf9[156].w) : 8.00}
	f_3_56 = utof(vs_cbuf9[156].w);
	// 0  <=>  uint((int(0) - int((({u_39_3 : 0} << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_30_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_30_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u))))))))))
	u_11_18 = uint((int(0) - int(((u_39_3 << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_30_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_30_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u))))))))));
	// 0  <=>  uint((int(0) - int((({u_27_8 : 0} << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_38_4 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_17 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_38_4 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_17 : 1}), int(0u), int(16u))))))))))
	u_26_7 = uint((int(0) - int(((u_27_8 << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_38_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_17), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_38_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_17), int(0u), int(16u))))))))));
	// 0  <=>  {utof((((({pf_16_1 : 0.305} >= {f_6_20 : 7.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_6_20 : 7.00}))) ? 1065353216u : 0u)) : 0}
	f_7_16 = utof(((((pf_16_1 >= f_6_20) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_20))) ? 1065353216u : 0u));
	// 0  <=>  {utof((((({pf_16_1 : 0.305} >= {f_3_56 : 8.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_3_56 : 8.00}))) ? 1065353216u : 0u)) : 0}
	f_9_65 = utof(((((pf_16_1 >= f_3_56) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_3_56))) ? 1065353216u : 0u));
	// 0  <=>  {utof((((({pf_16_1 : 0.305} >= {f_6_20 : 7.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_6_20 : 7.00}))) ? 1065353216u : 0u)) : 0}
	f_10_8 = utof(((((pf_16_1 >= f_6_20) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_20))) ? 1065353216u : 0u)); // maybe duplicate expression on the right side of the assignment, vars:(f_7_16, f_7_16)
	// 0  <=>  ({u_25_4 : 0} + {u_6_13 : 0})
	u_3_11 = (u_25_4 + u_6_13);
	// 0  <=>  clamp(trunc(({utof(({ftou((1.0f / float({u_0_3 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float({u_3_9 : 0}))), float(0.f), float(4294967300.f))
	f_4_35 = clamp(trunc((utof((ftou((1.0f / float(u_0_3))) + 4294967294u)) * float(u_3_9))), float(0.f), float(4294967300.f));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_2_2 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_13 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_2_2), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_2_23 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_2_2), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_18_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_4_36 = utof((ftou((1.0f / float(u_18_4))) + 4294967294u)); // maybe duplicate expression on the right side of the assignment, vars:(f_2_72, f_2_72)
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_9_10 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_102 = utof((ftou((1.0f / float(u_9_10))) + 4294967294u)); // maybe duplicate expression on the right side of the assignment, vars:(f_2_67, f_2_67)
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_15_17 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_103 = utof((ftou((1.0f / float(u_15_17))) + 4294967294u)); // maybe duplicate expression on the right side of the assignment, vars:(f_2_79, f_2_79)
	// 0  <=>  ({u_14_10 : 0} + {u_8_7 : 0})
	u_8_8 = (u_14_10 + u_8_7);
	// 0  <=>  ({u_31_6 : 0} + uint(clamp(trunc(({f_4_36 : 0.9999999} * float({u_10_16 : 0}))), float(0.f), float(4294967300.f))))
	u_14_11 = (u_31_6 + uint(clamp(trunc((f_4_36 * float(u_10_16))), float(0.f), float(4294967300.f))));
	// 0  <=>  {utof((((({pf_16_1 : 0.305} >= {f_3_56 : 8.00}) && (! myIsNaN({pf_16_1 : 0.305}))) && (! myIsNaN({f_3_56 : 8.00}))) ? 1065353216u : 0u)) : 0}
	f_3_61 = utof(((((pf_16_1 >= f_3_56) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_3_56))) ? 1065353216u : 0u)); // maybe duplicate expression on the right side of the assignment, vars:(f_9_65, f_9_65)
	// 1.00  <=>  (({f_3_61 : 0} * {utof(vs_cbuf9[156].x) : 1.00}) + ((((({pf_16_1 : 0.305} + (0.f - {utof(vs_cbuf9[155].w) : 7.00})) * (({utof(vs_cbuf9[156].x) : 1.00} + (0.f - {utof(vs_cbuf9[155].x) : 1.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[155].w) : 7.00}) + {utof(vs_cbuf9[156].w) : 8.00})))) + {utof(vs_cbuf9[155].x) : 1.00}) * (({f_10_8 : 0} * (0.f - {f_9_65 : 0})) + {f_7_16 : 0})) + {pf_2_16 : 1.00}))
	o.fs_attr5.x = ((f_3_61 * utof(vs_cbuf9[156].x)) + (((((pf_16_1 + (0.f - utof(vs_cbuf9[155].w))) * ((utof(vs_cbuf9[156].x) + (0.f - utof(vs_cbuf9[155].x))) * (1.0f / ((0.f - utof(vs_cbuf9[155].w)) + utof(vs_cbuf9[156].w))))) + utof(vs_cbuf9[155].x)) * ((f_10_8 * (0.f - f_9_65)) + f_7_16)) + pf_2_16));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_55 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_2_2), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_3_4 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_1_2), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  ({u_21_6 : 0} + uint({f_4_35 : 0}))
	u_6_15 = (u_21_6 + uint(f_4_35));
	// 0  <=>  ({u_38_4 : 0} + uint(clamp(trunc(({f_2_103 : 0.9999999} * float({u_26_7 : 0}))), float(0.f), float(4294967300.f))))
	u_20_6 = (u_38_4 + uint(clamp(trunc((f_2_103 * float(u_26_7))), float(0.f), float(4294967300.f))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_14_11 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_25_7 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_14_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_14_11 : 0}), int(16u), int(16u))) * {u_25_7 : 1})) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_14_11 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_14_11 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u)))))))
	u_10_21 = ((uint((uint(bitfieldExtract(uint(u_14_11), int(16u), int(16u))) * u_25_7)) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_14_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_14_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u)))))));
	// 0  <=>  uint((int(0) - int({u_10_21 : 0})))
	u_10_22 = uint((int(0) - int(u_10_21)));
	// 0  <=>  ({u_30_3 : 0} + uint(clamp(trunc(({f_2_102 : 0.9999999} * float({u_11_18 : 0}))), float(0.f), float(4294967300.f))))
	u_19_6 = (u_30_3 + uint(clamp(trunc((f_2_102 * float(u_11_18))), float(0.f), float(4294967300.f))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_11 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_3_11 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_30_4 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_11), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_3_11), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_11 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_3_11 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_11 : 0}), int(0u), int(16u))))))
	u_21_12 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_11), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_3_11), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_11), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_8 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_8 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_29_18 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_8), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_8), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_8 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_8_8 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_8_8 : 0}), int(0u), int(16u))))))
	u_25_12 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_8), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_8_8), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_8_8), int(0u), int(16u))))));
	// -7.3466773  <=>  (0.f - (({pf_0_1 : 183.00} * {utof(vs_cbuf9[87].x) : 0}) + (({utof(u_42_phi_58) : 0.16926} * {utof(vs_cbuf9[87].z) : 6.283185}) + ({utof(vs_cbuf9[87].z) : 6.283185} + {utof(vs_cbuf9[87].y) : 0}))))
	f_4_40 = (0.f - ((pf_0_1 * utof(vs_cbuf9[87].x)) + ((utof(u_42_phi_58) * utof(vs_cbuf9[87].z)) + (utof(vs_cbuf9[87].z) + utof(vs_cbuf9[87].y)))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[88].w) : 1.00}) * {utof(vs_cbuf9[88].y) : 1.00})
	pf_17_6 = ((1.0f / utof(vs_cbuf9[88].w)) * utof(vs_cbuf9[88].y));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_2_25 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_1_2), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_4_9 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_0_3), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_6_15 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_29_19 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_6_15), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_6_15 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_6_15 : 0}), int(0u), int(16u))))))
	u_27_15 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_6_15), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_6_15), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(16u), int(16u))) * {u_29_18 : 0})) << 16u) + {u_25_12 : 0}))))
	u_18_9 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_2_2), int(16u), int(16u))) * u_29_18)) << 16u) + u_25_12))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[88].z) : 1.00}) * {utof(vs_cbuf9[88].x) : 1.00})
	pf_22_8 = ((1.0f / utof(vs_cbuf9[88].z)) * utof(vs_cbuf9[88].x));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(16u), int(16u))) * {u_29_19 : 0})) << 16u) + {u_27_15 : 0}))))
	u_23_3 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_0_3), int(16u), int(16u))) * u_29_19)) << 16u) + u_27_15))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_19_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_30_6 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_19_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_19_6 : 0}), int(16u), int(16u))) * {u_30_6 : 1})) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_19_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_19_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u)))))))
	u_26_20 = ((uint((uint(bitfieldExtract(uint(u_19_6), int(16u), int(16u))) * u_30_6)) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_19_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_19_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u)))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(16u), int(16u))) * {u_30_4 : 0})) << 16u) + {u_21_12 : 0}))))
	u_21_14 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_1_2), int(16u), int(16u))) * u_30_4)) << 16u) + u_21_12))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_20_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_17 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_28_22 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_20_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_20_6 : 0}), int(16u), int(16u))) * {u_28_22 : 1})) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_20_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_17 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_17 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_20_6 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_17 : 1}), int(0u), int(16u)))))))
	u_24_19 = ((uint((uint(bitfieldExtract(uint(u_20_6), int(16u), int(16u))) * u_28_22)) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_20_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_17), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_17), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_20_6), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_17), int(0u), int(16u)))))));
	// 0  <=>  ((uint({u_18_9 : 0}) >= uint({u_2_2 : 1})) ? 4294967295u : 0u)
	u_18_10 = ((uint(u_18_9) >= uint(u_2_2)) ? 4294967295u : 0u);
	// 0  <=>  ((uint({u_21_14 : 0}) >= uint({u_1_2 : 1})) ? 4294967295u : 0u)
	u_13_11 = ((uint(u_21_14) >= uint(u_1_2)) ? 4294967295u : 0u);
	// 0  <=>  uint((int(0) - int({u_24_19 : 0})))
	u_21_15 = uint((int(0) - int(u_24_19)));
	// 0  <=>  uint((int(0) - int({u_26_20 : 0})))
	u_18_11 = uint((int(0) - int(u_26_20)));
	// 0  <=>  ((uint({u_23_3 : 0}) >= uint({u_0_3 : 1})) ? 4294967295u : 0u)
	u_23_4 = ((uint(u_23_3) >= uint(u_0_3)) ? 4294967295u : 0u);
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_3_6 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_0_3), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_1_2 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_5_5 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_1_2), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_8_8 : 0}))) + {u_18_10 : 0})), int(16u), int(16u)))
	u_24_21 = uint(bitfieldExtract(uint((uint((int(0) - int(u_8_8))) + u_18_10)), int(16u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_8_8 : 0}))) + {u_18_10 : 0})), int(0u), int(16u)))
	u_8_11 = uint(bitfieldExtract(uint((uint((int(0) - int(u_8_8))) + u_18_10)), int(0u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * {u_24_21 : 0})), {u_8_11 : 0}, int(16u), int(16u))), int(16u), int(16u)))
	u_20_7 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * u_24_21)), u_8_11, int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * {u_24_21 : 0})), {u_8_11 : 0}, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int({u_8_8 : 0}))) + {u_18_10 : 0})), int(0u), int(16u))))))
	u_8_14 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * u_24_21)), u_8_11, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int(u_8_8))) + u_18_10)), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int(({u_2_2 : 1} >> 31u))))
	u_9_12 = uint((int(0) - int((u_2_2 >> 31u))));
	// 0  <=>  uint((int(0) - int(({u_1_2 : 1} >> 31u))))
	u_18_15 = uint((int(0) - int((u_1_2 >> 31u))));
	// 0  <=>  uint((int(0) - int(({u_0_3 : 1} >> 31u))))
	u_14_13 = uint((int(0) - int((u_0_3 >> 31u))));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_3_11 : 0}))) + {u_13_11 : 0})), int(16u), int(16u)))
	u_21_18 = uint(bitfieldExtract(uint((uint((int(0) - int(u_3_11))) + u_13_11)), int(16u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_3_11 : 0}))) + {u_13_11 : 0})), int(0u), int(16u)))
	u_3_14 = uint(bitfieldExtract(uint((uint((int(0) - int(u_3_11))) + u_13_11)), int(0u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * {u_21_18 : 0})), {u_3_14 : 0}, int(16u), int(16u))), int(16u), int(16u)))
	u_20_11 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * u_21_18)), u_3_14, int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * {u_21_18 : 0})), {u_3_14 : 0}, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int({u_3_11 : 0}))) + {u_13_11 : 0})), int(0u), int(16u))))))
	u_3_17 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * u_21_18)), u_3_14, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int(u_3_11))) + u_13_11)), int(0u), int(16u))))));
	// 0  <=>  (({b_2_23 : False} || {b_1_55 : True}) ? ((uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(16u), int(16u))) * {u_20_7 : 0})) << 16u) + {u_8_14 : 0}) : 4294967295u)
	u_2_7 = ((b_2_23 || b_1_55) ? ((uint((uint(bitfieldExtract(uint(u_2_2), int(16u), int(16u))) * u_20_7)) << 16u) + u_8_14) : 4294967295u);
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_1_2 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_57 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_1_2), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_0_3 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_5_6 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_0_3), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  (uint((int(0) - int({u_9_12 : 0}))) + (({u_14_11 : 0} + uint((int(0) - int(((uint({u_10_22 : 0}) >= uint({u_18_4 : 1})) ? 4294967295u : 0u))))) ^ {u_9_12 : 0}))
	u_9_14 = (uint((int(0) - int(u_9_12))) + ((u_14_11 + uint((int(0) - int(((uint(u_10_22) >= uint(u_18_4)) ? 4294967295u : 0u))))) ^ u_9_12));
	// 0  <=>  (uint((int(0) - int({u_18_15 : 0}))) + (({u_19_6 : 0} + uint((int(0) - int(((uint({u_18_11 : 0}) >= uint({u_9_10 : 1})) ? 4294967295u : 0u))))) ^ {u_18_15 : 0}))
	u_8_18 = (uint((int(0) - int(u_18_15))) + ((u_19_6 + uint((int(0) - int(((uint(u_18_11) >= uint(u_9_10)) ? 4294967295u : 0u))))) ^ u_18_15));
	// 0  <=>  (uint((int(0) - int({u_14_13 : 0}))) + (({u_20_6 : 0} + uint((int(0) - int(((uint({u_21_15 : 0}) >= uint({u_15_17 : 1})) ? 4294967295u : 0u))))) ^ {u_14_13 : 0}))
	u_10_29 = (uint((int(0) - int(u_14_13))) + ((u_20_6 + uint((int(0) - int(((uint(u_21_15) >= uint(u_15_17)) ? 4294967295u : 0u))))) ^ u_14_13));
	// 0  <=>  (({b_3_4 : False} || {b_2_25 : True}) ? ((uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(16u), int(16u))) * {u_20_11 : 0})) << 16u) + {u_3_17 : 0}) : 4294967295u)
	u_1_7 = ((b_3_4 || b_2_25) ? ((uint((uint(bitfieldExtract(uint(u_1_2), int(16u), int(16u))) * u_20_11)) << 16u) + u_3_17) : 4294967295u);
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].z) : 1.00}) * {utof(vs_cbuf9[78].x) : 1.00})
	pf_24_4 = ((1.0f / utof(vs_cbuf9[78].z)) * utof(vs_cbuf9[78].x));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_6_15 : 0}))) + {u_23_4 : 0})), int(16u), int(16u)))
	u_9_16 = uint(bitfieldExtract(uint((uint((int(0) - int(u_6_15))) + u_23_4)), int(16u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_6_15 : 0}))) + {u_23_4 : 0})), int(0u), int(16u)))
	u_6_18 = uint(bitfieldExtract(uint((uint((int(0) - int(u_6_15))) + u_23_4)), int(0u), int(16u)));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].z) : 1.00}) * {utof(vs_cbuf9[83].x) : 1.00})
	pf_27_2 = ((1.0f / utof(vs_cbuf9[83].z)) * utof(vs_cbuf9[83].x));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_0_3 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_15 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_0_3), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * {u_9_16 : 0})), {u_6_18 : 0}, int(16u), int(16u))), int(16u), int(16u)))
	u_5_21 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * u_9_16)), u_6_18, int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * {u_9_16 : 0})), {u_6_18 : 0}, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int({u_6_15 : 0}))) + {u_23_4 : 0})), int(0u), int(16u))))))
	u_1_13 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * u_9_16)), u_6_18, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int(u_6_15))) + u_23_4)), int(0u), int(16u))))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].w) : 1.00}) * {utof(vs_cbuf9[78].y) : 1.00})
	pf_28_1 = ((1.0f / utof(vs_cbuf9[78].w)) * utof(vs_cbuf9[78].y));
	// 0  <=>  (({b_4_9 : False} || {b_3_6 : True}) ? ((uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(16u), int(16u))) * {u_5_21 : 0})) << 16u) + {u_1_13 : 0}) : 4294967295u)
	u_1_14 = ((b_4_9 || b_3_6) ? ((uint((uint(bitfieldExtract(uint(u_0_3), int(16u), int(16u))) * u_5_21)) << 16u) + u_1_13) : 4294967295u);
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].w) : 1.00}) * {utof(vs_cbuf9[83].y) : 1.00})
	pf_28_2 = ((1.0f / utof(vs_cbuf9[83].w)) * utof(vs_cbuf9[83].y));
	// 0.5965608  <=>  (({utof(vs_cbuf9[121].x) : 0.271164} * {(vs_cbuf10_1.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 2.20})
	o.fs_attr1.x = ((utof(vs_cbuf9[121].x) * (vs_cbuf10_1.x)) * utof(vs_cbuf9[104].x));
	// 0.715873  <=>  (({utof(vs_cbuf9[121].z) : 0.3253968} * {(vs_cbuf10_1.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 2.20})
	o.fs_attr1.z = ((utof(vs_cbuf9[121].z) * (vs_cbuf10_1.z)) * utof(vs_cbuf9[104].x));
	// 0.6848703  <=>  (({utof(vs_cbuf9[121].y) : 0.3113047} * {(vs_cbuf10_1.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 2.20})
	o.fs_attr1.y = ((utof(vs_cbuf9[121].y) * (vs_cbuf10_1.y)) * utof(vs_cbuf9[104].x));
	// 0.38125  <=>  ((({utof(u_3_3) : 0} * {utof(vs_cbuf9[131].x) : 0}) + (((((((0.f - {utof(vs_cbuf9[130].x) : 0.50}) + {utof(vs_cbuf9[131].x) : 0}) * (1.0f / ((0.f - {utof(vs_cbuf9[130].w) : 0.40}) + {utof(vs_cbuf9[131].w) : 1.00}))) * ({utof(u_3_phi_23) : 0.305} + (0.f - {utof(vs_cbuf9[130].w) : 0.40}))) + {utof(vs_cbuf9[130].x) : 0.50}) * (({utof(u_14_9) : 0} * (0.f - {utof(u_3_3) : 0})) + {utof(u_14_9) : 0})) + ((((({utof(u_3_phi_23) : 0.305} + (0.f - {utof(vs_cbuf9[129].w) : 0})) * (({utof(vs_cbuf9[130].x) : 0.50} + (0.f - {utof(vs_cbuf9[129].x) : 0})) * (1.0f / ((0.f - {utof(vs_cbuf9[129].w) : 0}) + {utof(vs_cbuf9[130].w) : 0.40})))) + {utof(vs_cbuf9[129].x) : 0}) * (({utof(u_14_9) : 0} * (0.f - {utof(u_9_6) : 1.00})) + {utof(u_9_6) : 1.00})) + (({utof(u_9_6) : 1.00} * (0.f - {utof(vs_cbuf9[129].x) : 0})) + {utof(vs_cbuf9[129].x) : 0})))) * {(vs_cbuf10_1.w) : 1.00})
	o.fs_attr1.w = (((utof(u_3_3) * utof(vs_cbuf9[131].x)) + (((((((0.f - utof(vs_cbuf9[130].x)) + utof(vs_cbuf9[131].x)) * (1.0f / ((0.f - utof(vs_cbuf9[130].w)) + utof(vs_cbuf9[131].w)))) * (utof(u_3_phi_23) + (0.f - utof(vs_cbuf9[130].w)))) + utof(vs_cbuf9[130].x)) * ((utof(u_14_9) * (0.f - utof(u_3_3))) + utof(u_14_9))) + (((((utof(u_3_phi_23) + (0.f - utof(vs_cbuf9[129].w))) * ((utof(vs_cbuf9[130].x) + (0.f - utof(vs_cbuf9[129].x))) * (1.0f / ((0.f - utof(vs_cbuf9[129].w)) + utof(vs_cbuf9[130].w))))) + utof(vs_cbuf9[129].x)) * ((utof(u_14_9) * (0.f - utof(u_9_6))) + utof(u_9_6))) + ((utof(u_9_6) * (0.f - utof(vs_cbuf9[129].x))) + utof(vs_cbuf9[129].x))))) * (vs_cbuf10_1.w));
	// 1.00  <=>  1.f
	o.fs_attr7.x = 1.f;
	// 1003.85  <=>  (({i.vao_attr5.y : 1003.85} * {utof(vs_cbuf9[141].y) : 1.00}) * {(vs_cbuf10_3.z) : 1.00})
	pf_7_1 = ((i.vao_attr5.y * utof(vs_cbuf9[141].y)) * (vs_cbuf10_3.z));
	// 1.00  <=>  1.f
	o.fs_attr7.y = 1.f;
	// 1.00  <=>  1.f
	o.fs_attr7.z = 1.f;
	// 3207546337  <=>  {ftou((((({pf_0_1 : 183.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_35_phi_47) : 0.16926} * {utof(vs_cbuf9[81].z) : 0}) + ({utof(vs_cbuf9[81].z) : 0} + {utof(vs_cbuf9[81].x) : 0.80}))) * (({pf_27_2 : 1.00} * {utof(u_16_phi_27) : 0.4939}) + -0.5f)) + (({pf_27_2 : 1.00} * float(int({u_1_7 : 0}))) + (({pf_0_1 : 183.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.0001})) + (0.f - ((({utof(u_36_phi_47) : 0.16926} * {utof(vs_cbuf9[80].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 1.00} + {utof(vs_cbuf9[79].z) : 0}))))))) : 3207546337}
	u_2_10 = ftou(((((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_35_phi_47) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].z) + utof(vs_cbuf9[81].x)))) * ((pf_27_2 * utof(u_16_phi_27)) + -0.5f)) + ((pf_27_2 * float(int(u_1_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_36_phi_47) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))));
	u_2_phi_64 = u_2_10;
	// False  <=>  if((((((0.f == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && (((0.f == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935}))))) ? true : false))
	if((((((0.f == abs((vs_cbuf8_28.z))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((0.f == abs((vs_cbuf8_28.x))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.x)))))) ? true : false))
	{
		// -0.6398518  <=>  {(vs_cbuf8_28.z) : -0.6398518}
		f_2_134 = (vs_cbuf8_28.z);
		// 1078530011  <=>  ((((0.f > {f_2_134 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_2_134 : -0.6398518}))) ? 1078530011u : 0u)
		u_6_19 = ((((0.f > f_2_134) && (! myIsNaN(0.f))) && (! myIsNaN(f_2_134))) ? 1078530011u : 0u);
		u_6_phi_65 = u_6_19;
		// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
		if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
		{
			// 3226013659  <=>  {ftou((0.f - {utof(((((0.f > {f_2_134 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_2_134 : -0.6398518}))) ? 1078530011u : 0u)) : 3.141593})) : 3226013659}
			u_6_20 = ftou((0.f - utof(((((0.f > f_2_134) && (! myIsNaN(0.f))) && (! myIsNaN(f_2_134))) ? 1078530011u : 0u))));
			u_6_phi_65 = u_6_20;
		}
		// 3226013659  <=>  {u_6_phi_65 : }
		u_2_11 = u_6_phi_65;
		u_2_phi_64 = u_2_11;
	}
	// 3207546337  <=>  {u_2_phi_64 : }
	u_1_18 = u_2_phi_64;
	u_1_phi_66 = u_1_18;
	// True  <=>  if(((! ((((0.f == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && (((0.f == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935})))))) ? true : false))
	if(((! ((((0.f == abs((vs_cbuf8_28.z))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((0.f == abs((vs_cbuf8_28.x))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.x))))))) ? true : false))
	{
		// 2139095040  <=>  2139095040u
		u_5_23 = 2139095040u;
		u_5_phi_67 = u_5_23;
		// False  <=>  if(((((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && ((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935}))))) ? true : false))
		if((((((utof(0x7f800000) == abs((vs_cbuf8_28.z))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((utof(0x7f800000) == abs((vs_cbuf8_28.x))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.x)))))) ? true : false))
		{
			// -0.6398518  <=>  {(vs_cbuf8_28.z) : -0.6398518}
			f_2_142 = (vs_cbuf8_28.z);
			// 1075235812  <=>  ((((0.f > {f_2_142 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_2_142 : -0.6398518}))) ? 1075235812u : 1061752795u)
			u_8_20 = ((((0.f > f_2_142) && (! myIsNaN(0.f))) && (! myIsNaN(f_2_142))) ? 1075235812u : 1061752795u);
			u_8_phi_68 = u_8_20;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
			{
				// 3222719460  <=>  {ftou((0.f - {utof(((((0.f > {f_2_142 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_2_142 : -0.6398518}))) ? 1075235812u : 1061752795u)) : 2.356194})) : 3222719460}
				u_8_21 = ftou((0.f - utof(((((0.f > f_2_142) && (! myIsNaN(0.f))) && (! myIsNaN(f_2_142))) ? 1075235812u : 1061752795u))));
				u_8_phi_68 = u_8_21;
			}
			// 3222719460  <=>  {u_8_phi_68 : }
			u_5_24 = u_8_phi_68;
			u_5_phi_67 = u_5_24;
		}
		// 2139095040  <=>  {u_5_phi_67 : }
		u_6_22 = u_5_phi_67;
		u_6_phi_69 = u_6_22;
		// True  <=>  if(((! (((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && ((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935})))))) ? true : false))
		if(((! ((((utof(0x7f800000) == abs((vs_cbuf8_28.z))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((utof(0x7f800000) == abs((vs_cbuf8_28.x))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.x))))))) ? true : false))
		{
			// 1059310932  <=>  {ftou(max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))) : 1059310932}
			u_10_30 = ftou(max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))));
			u_10_phi_70 = u_10_30;
			// False  <=>  if(((((max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) >= 16.f) && (! myIsNaN(max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))))) && (! myIsNaN(16.f))) ? true : false))
			if(((((max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) >= 16.f) && (! myIsNaN(max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z)))))) && (! myIsNaN(16.f))) ? true : false))
			{
				// 1025756500  <=>  {ftou((max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) * 0.0625f)) : 1025756500}
				u_10_31 = ftou((max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) * 0.0625f));
				u_10_phi_70 = u_10_31;
			}
			// 1058258456  <=>  {ftou(min(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))) : 1058258456}
			u_8_23 = ftou(min(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))));
			u_8_phi_71 = u_8_23;
			// False  <=>  if(((((max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) >= 16.f) && (! myIsNaN(max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))))) && (! myIsNaN(16.f))) ? true : false))
			if(((((max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) >= 16.f) && (! myIsNaN(max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z)))))) && (! myIsNaN(16.f))) ? true : false))
			{
				// 1024704024  <=>  {ftou((min(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) * 0.0625f)) : 1024704024}
				u_8_24 = ftou((min(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) * 0.0625f));
				u_8_phi_71 = u_8_24;
			}
			// 0.9019579  <=>  ((1.0f / {utof(u_10_phi_70) : 0.6398518}) * {utof(u_8_phi_71) : 0.5771194})
			pf_16_7 = ((1.0f / utof(u_10_phi_70)) * utof(u_8_phi_71));
			// 0.813528  <=>  ({pf_16_7 : 0.9019579} * {pf_16_7 : 0.9019579})
			pf_17_9 = (pf_16_7 * pf_16_7);
			// 1060888727  <=>  {ftou((((1.0f / (({pf_17_9 : 0.813528} * (({pf_17_9 : 0.813528} * ({pf_17_9 : 0.813528} + 11.335388f)) + 28.842468f)) + 19.69667f)) * ({pf_16_7 : 0.9019579} * ({pf_17_9 : 0.813528} * (({pf_17_9 : 0.813528} * (({pf_17_9 : 0.813528} * -0.82336295f) + -5.674867f)) + -6.565555f)))) + {pf_16_7 : 0.9019579})) : 1060888727}
			u_9_19 = ftou((((1.0f / ((pf_17_9 * ((pf_17_9 * (pf_17_9 + 11.335388f)) + 28.842468f)) + 19.69667f)) * (pf_16_7 * (pf_17_9 * ((pf_17_9 * ((pf_17_9 * -0.82336295f) + -5.674867f)) + -6.565555f)))) + pf_16_7));
			u_9_phi_72 = u_9_19;
			// False  <=>  if(((((abs({(vs_cbuf8_28.x) : -0.57711935}) > abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935})))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) ? true : false))
			if(((((abs((vs_cbuf8_28.x)) > abs((vs_cbuf8_28.z))) && (! myIsNaN(abs((vs_cbuf8_28.x))))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) ? true : false))
			{
				// 1062616863  <=>  {ftou(((0.f - (((1.0f / (({pf_17_9 : 0.813528} * (({pf_17_9 : 0.813528} * ({pf_17_9 : 0.813528} + 11.335388f)) + 28.842468f)) + 19.69667f)) * ({pf_16_7 : 0.9019579} * ({pf_17_9 : 0.813528} * (({pf_17_9 : 0.813528} * (({pf_17_9 : 0.813528} * -0.82336295f) + -5.674867f)) + -6.565555f)))) + {pf_16_7 : 0.9019579})) + 1.5707964f)) : 1062616863}
				u_9_20 = ftou(((0.f - (((1.0f / ((pf_17_9 * ((pf_17_9 * (pf_17_9 + 11.335388f)) + 28.842468f)) + 19.69667f)) * (pf_16_7 * (pf_17_9 * ((pf_17_9 * ((pf_17_9 * -0.82336295f) + -5.674867f)) + -6.565555f)))) + pf_16_7)) + 1.5707964f));
				u_9_phi_72 = u_9_20;
			}
			// 1060888727  <=>  {u_9_phi_72 : }
			u_8_26 = u_9_phi_72;
			u_8_phi_73 = u_8_26;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.z) : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.z) : -0.6398518}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.z)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.z)))) ? true : false))
			{
				// 1075451829  <=>  {ftou(((0.f - {utof(u_9_phi_72) : 0.7338957}) + 3.1415927f)) : 1075451829}
				u_8_27 = ftou(((0.f - utof(u_9_phi_72)) + 3.1415927f));
				u_8_phi_73 = u_8_27;
			}
			// 1075451829  <=>  {u_8_phi_73 : }
			u_9_21 = u_8_phi_73;
			u_9_phi_74 = u_9_21;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
			{
				// 3222935477  <=>  {ftou((0.f - {utof(u_8_phi_73) : 2.407697})) : 3222935477}
				u_9_22 = ftou((0.f - utof(u_8_phi_73)));
				u_9_phi_74 = u_9_22;
			}
			// 3222935477  <=>  {u_9_phi_74 : }
			u_6_23 = u_9_phi_74;
			u_6_phi_69 = u_6_23;
		}
		// 3222935477  <=>  {u_6_phi_69 : }
		u_1_19 = u_6_phi_69;
		u_1_phi_66 = u_1_19;
	}
	// -2.407697  <=>  (((((({pf_21_5 : 0} * {pf_22_1 : 0}) * -2.f) + {pf_21_5 : 0}) * {utof(u_26_phi_42) : 183.00}) + (({f_8_13 : 0.16926} + -0.5f) * {utof(vs_cbuf9[194].y) : 0})) + {utof(u_1_phi_66) : -2.407697})
	pf_11_8 = ((((((pf_21_5 * pf_22_1) * -2.f) + pf_21_5) * utof(u_26_phi_42)) + ((f_8_13 + -0.5f) * utof(vs_cbuf9[194].y))) + utof(u_1_phi_66));
	// 1.00  <=>  cos({pf_13_9 : 0})
	f_4_56 = cos(pf_13_9);
	// 1.00  <=>  cos({pf_12_7 : 0})
	f_5_3 = cos(pf_12_7);
	// 161.2986  <=>  ((({i.vao_attr5.z : 1505.775} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712}))
	pf_5_15 = (((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z));
	// -23.685835  <=>  ((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573}))
	pf_20_14 = ((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x));
	// -0.66976756  <=>  (sin({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})
	pf_22_10 = (sin(pf_11_8) * f_5_3);
	// -348.50656  <=>  ({pf_7_1 : 1003.85} * ((0.5f * {utof(vs_cbuf9[16].y) : 0}) + {v.vertex.y : -0.34717}))
	pf_11_11 = (pf_7_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y));
	// 1127677952  <=>  {ftou(pf_0_1) : 1127677952}
	u_1_20 = ftou(pf_0_1);
	u_1_phi_75 = u_1_20;
	// False  <=>  if((((({pf_0_1 : 183.00} == 0.f) && (! myIsNaN({pf_0_1 : 183.00}))) && (! myIsNaN(0.f))) ? true : false))
	if(((((pf_0_1 == 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 1065353216  <=>  1065353216u
		u_1_21 = 1065353216u;
		u_1_phi_75 = u_1_21;
	}
	// -632.375  <=>  ({pf_1_9 : -2551.6372} + (0.f - {(camera_wpos.x) : -1919.2622}))
	pf_11_15 = (pf_1_9 + (0.f - (camera_wpos.x)));
	// -130.38501  <=>  ({pf_6_11 : 235.3523} + (0.f - {(camera_wpos.y) : 365.7373}))
	pf_28_3 = (pf_6_11 + (0.f - (camera_wpos.y)));
	// 9171.367  <=>  ({pf_8_3 : 5438.32} + (0.f - {(camera_wpos.z) : -3733.0469}))
	pf_28_4 = (pf_8_3 + (0.f - (camera_wpos.z)));
	// 9194.067  <=>  sqrt((({pf_28_4 : 9171.367} * {pf_28_4 : 9171.367}) + (({pf_28_3 : -130.38501} * {pf_28_3 : -130.38501}) + ({pf_11_15 : -632.375} * {pf_11_15 : -632.375}))))
	f_5_14 = sqrt(((pf_28_4 * pf_28_4) + ((pf_28_3 * pf_28_3) + (pf_11_15 * pf_11_15))));
	// -0.34717  <=>  (((({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00})))) + (((0.f - abs({(vs_cbuf8_28.y) : 0.5074672})) * {(vs_cbuf13_6.w) : 0}) + {(vs_cbuf13_6.w) : 0})) * (1.0f / {pf_7_1 : 1003.85}))
	pf_7_3 = ((((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3)))) + (((0.f - abs((vs_cbuf8_28.y))) * (vs_cbuf13_6.w)) + (vs_cbuf13_6.w))) * (1.0f / pf_7_1));
	// 4000.00  <=>  {utof(vs_cbuf9[197].x) : 4000.00}
	f_1_17 = utof(vs_cbuf9[197].x);
	// True  <=>  ((({f_5_14 : 9194.067} > {f_1_17 : 4000.00}) && (! myIsNaN({f_5_14 : 9194.067}))) && (! myIsNaN({f_1_17 : 4000.00})))
	b_1_67 = (((f_5_14 > f_1_17) && (! myIsNaN(f_5_14))) && (! myIsNaN(f_1_17)));
	// 982.8796  <=>  (((({(vs_cbuf10_4.w) : -1919.2622} + {(vs_cbuf10_5.w) : 365.7375}) + {(vs_cbuf10_6.w) : -3733.0469}) * 0.105f) + (((((({f_5_2 : 0.76809} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_8_3 : 5438.32} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00}))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -348.50656} * (sin({pf_12_7 : 0}) * {f_4_56 : 1.00})))))) + (((({f_0_6 : 0.90784} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_1_9 : -2551.6372} + (({pf_5_15 : 161.2986} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -23.685835} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -348.50656} * (0.f - sin({pf_13_9 : 0}))))))) + ((({f_8_13 : 0.16926} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_6_11 : 235.3523} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00})))))))) * {(vs_cbuf13_0.w) : 0.50}) + ({(vs_cbuf13_1.x) : 0.025} * {(vs_cbuf10_2.x) : 879.50})))
	pf_7_4 = (((((vs_cbuf10_4.w) + (vs_cbuf10_5.w)) + (vs_cbuf10_6.w)) * 0.105f) + ((((((f_5_2 + 1.f) * (vs_cbuf13_2.x)) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (sin(pf_12_7) * sin(pf_11_8))) + (cos(pf_11_8) * f_5_3))) + ((pf_20_14 * ((sin(pf_13_9) * (sin(pf_12_7) * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (sin(pf_12_7) * f_4_56)))))) + ((((f_0_6 + 1.f) * (vs_cbuf13_2.x)) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))) + (((f_8_13 + 1.f) * (vs_cbuf13_2.x)) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3)))))))) * (vs_cbuf13_0.w)) + ((vs_cbuf13_1.x) * (vs_cbuf10_2.x))));
	// 982.8796  <=>  (((({(vs_cbuf10_4.w) : -1919.2622} + {(vs_cbuf10_5.w) : 365.7375}) + {(vs_cbuf10_6.w) : -3733.0469}) * 0.105f) + (((((({f_5_2 : 0.76809} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_8_3 : 5438.32} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00}))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -348.50656} * (sin({pf_12_7 : 0}) * {f_4_56 : 1.00})))))) + (((({f_0_6 : 0.90784} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_1_9 : -2551.6372} + (({pf_5_15 : 161.2986} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -23.685835} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -348.50656} * (0.f - sin({pf_13_9 : 0}))))))) + ((({f_8_13 : 0.16926} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_6_11 : 235.3523} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00})))))))) * ({(vs_cbuf13_0.w) : 0.50} * {(vs_cbuf13_1.z) : 1.00})) + ({(vs_cbuf13_1.x) : 0.025} * {(vs_cbuf10_2.x) : 879.50})))
	pf_11_21 = (((((vs_cbuf10_4.w) + (vs_cbuf10_5.w)) + (vs_cbuf10_6.w)) * 0.105f) + ((((((f_5_2 + 1.f) * (vs_cbuf13_2.x)) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (sin(pf_12_7) * sin(pf_11_8))) + (cos(pf_11_8) * f_5_3))) + ((pf_20_14 * ((sin(pf_13_9) * (sin(pf_12_7) * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (sin(pf_12_7) * f_4_56)))))) + ((((f_0_6 + 1.f) * (vs_cbuf13_2.x)) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))) + (((f_8_13 + 1.f) * (vs_cbuf13_2.x)) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3)))))))) * ((vs_cbuf13_0.w) * (vs_cbuf13_1.z))) + ((vs_cbuf13_1.x) * (vs_cbuf10_2.x))));
	// 0.4249095  <=>  sin({pf_7_4 : 982.8796})
	f_1_20 = sin(pf_7_4);
	// 1065353216  <=>  {ftou(clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 183.00}), 0.0, 1.0)) : 1065353216}
	u_6_24 = ftou(clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0));
	// 1065353216  <=>  {ftou(clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 183.00}), 0.0, 1.0)) : 1065353216}
	u_7_3 = ftou(clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(u_6_24, u_6_24)
	// 1065353216  <=>  {ftou(clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 183.00}), 0.0, 1.0)) : 1065353216}
	u_8_28 = ftou(clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(u_6_24, u_6_24)|(u_7_3, u_7_3)
	u_6_phi_76 = u_6_24;
	u_7_phi_76 = u_7_3;
	u_8_phi_76 = u_8_28;
	// False  <=>  if(((((0.f != {(vs_cbuf13_5.y) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf13_5.y) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf13_5.y)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf13_5.y))) ? true : false))
	{
		// 722.8191  <=>  ((0.f - ({pf_1_9 : -2551.6372} + (({pf_5_15 : 161.2986} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -23.685835} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -348.50656} * (0.f - sin({pf_13_9 : 0}))))))) + {(vs_cbuf10_4.w) : -1919.2622})
		pf_16_15 = ((0.f - (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))) + (vs_cbuf10_4.w));
		// 478.8918  <=>  ((0.f - ({pf_6_11 : 235.3523} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00})))))) + {(vs_cbuf10_5.w) : 365.7375})
		pf_17_15 = ((0.f - (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3)))))) + (vs_cbuf10_5.w));
		// -9035.728  <=>  ((0.f - ({pf_8_3 : 5438.32} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00}))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -348.50656} * (sin({pf_12_7 : 0}) * {f_4_56 : 1.00})))))) + {(vs_cbuf10_6.w) : -3733.0469})
		pf_21_19 = ((0.f - (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (sin(pf_12_7) * sin(pf_11_8))) + (cos(pf_11_8) * f_5_3))) + ((pf_20_14 * ((sin(pf_13_9) * (sin(pf_12_7) * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (sin(pf_12_7) * f_4_56)))))) + (vs_cbuf10_6.w));
		// 1.00  <=>  (clamp(((1.0f / {(vs_cbuf13_5.y) : 0}) * sqrt((({pf_21_19 : -9035.728} * {pf_21_19 : -9035.728}) + (({pf_17_15 : 478.8918} * {pf_17_15 : 478.8918}) + ({pf_16_15 : 722.8191} * {pf_16_15 : 722.8191}))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 183.00}), 0.0, 1.0))
		pf_16_20 = (clamp(((1.0f / (vs_cbuf13_5.y)) * sqrt(((pf_21_19 * pf_21_19) + ((pf_17_15 * pf_17_15) + (pf_16_15 * pf_16_15))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0));
		// 1.00  <=>  (clamp(((1.0f / {(vs_cbuf13_5.y) : 0}) * sqrt((({pf_21_19 : -9035.728} * {pf_21_19 : -9035.728}) + (({pf_17_15 : 478.8918} * {pf_17_15 : 478.8918}) + ({pf_16_15 : 722.8191} * {pf_16_15 : 722.8191}))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 183.00}), 0.0, 1.0))
		pf_16_21 = (clamp(((1.0f / (vs_cbuf13_5.y)) * sqrt(((pf_21_19 * pf_21_19) + ((pf_17_15 * pf_17_15) + (pf_16_15 * pf_16_15))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(pf_16_20, pf_16_20)
		// 1.00  <=>  (clamp(((1.0f / {(vs_cbuf13_5.y) : 0}) * sqrt((({pf_21_19 : -9035.728} * {pf_21_19 : -9035.728}) + (({pf_17_15 : 478.8918} * {pf_17_15 : 478.8918}) + ({pf_16_15 : 722.8191} * {pf_16_15 : 722.8191}))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 600.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 600.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 183.00}), 0.0, 1.0))
		pf_16_22 = (clamp(((1.0f / (vs_cbuf13_5.y)) * sqrt(((pf_21_19 * pf_21_19) + ((pf_17_15 * pf_17_15) + (pf_16_15 * pf_16_15))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(pf_16_20, pf_16_20)|(pf_16_21, pf_16_21)
		// 1065353216  <=>  {ftou(pf_16_22) : 1065353216}
		u_6_25 = ftou(pf_16_22);
		// 1065353216  <=>  {ftou(pf_16_21) : 1065353216}
		u_7_4 = ftou(pf_16_21);
		// 1065353216  <=>  {ftou(pf_16_20) : 1065353216}
		u_8_29 = ftou(pf_16_20);
		u_6_phi_76 = u_6_25;
		u_7_phi_76 = u_7_4;
		u_8_phi_76 = u_8_29;
	}
	// 0.4249095  <=>  sin({pf_11_21 : 982.8796})
	f_4_61 = sin(pf_11_21);
	// -0.9052358  <=>  cos({pf_7_4 : 982.8796})
	f_7_42 = cos(pf_7_4);
	// 1054444989  <=>  {ftou(f_1_20) : 1054444989}
	u_1_23 = ftou(f_1_20);
	u_1_phi_77 = u_1_23;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1165623296  <=>  {vs_cbuf9[197].y : 1165623296}
		u_1_24 = vs_cbuf9[197].y;
		u_1_phi_77 = u_1_24;
	}
	// -0.0012988562  <=>  (((({pf_7_3 : -0.34717} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_7_3 : -0.34717})) * {(vs_cbuf13_2.y) : 0.01}) * {(vs_cbuf16_1.x) : 0.7282469})
	pf_17_16 = ((((pf_7_3 * (vs_cbuf16_1.w)) * abs(pf_7_3)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.x));
	// 1165623296  <=>  {u_1_phi_77 : }
	u_5_26 = u_1_phi_77;
	u_5_phi_78 = u_5_26;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 964891247  <=>  {ftou((1.0f / {utof(u_1_phi_77) : 4000.00})) : 964891247}
		u_5_27 = ftou((1.0f / utof(u_1_phi_77)));
		u_5_phi_78 = u_5_27;
	}
	// 1175431237  <=>  {ftou(f_5_14) : 1175431237}
	u_1_25 = ftou(f_5_14);
	u_1_phi_79 = u_1_25;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1175431237  <=>  {ftou(max({f_5_14 : 9194.067}, {utof(vs_cbuf9[197].y) : 4000.00})) : 1175431237}
		u_1_26 = ftou(max(f_5_14, utof(vs_cbuf9[197].y)));
		u_1_phi_79 = u_1_26;
	}
	// -0  <=>  (((({pf_7_3 : -0.34717} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_7_3 : -0.34717})) * {(vs_cbuf13_2.y) : 0.01}) * {(vs_cbuf16_1.y) : 0})
	pf_20_17 = ((((pf_7_3 * (vs_cbuf16_1.w)) * abs(pf_7_3)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.y));
	// -0.0012222854  <=>  (((({pf_7_3 : -0.34717} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_7_3 : -0.34717})) * {(vs_cbuf13_2.y) : 0.01}) * {(vs_cbuf16_1.z) : 0.685315})
	pf_11_23 = ((((pf_7_3 * (vs_cbuf16_1.w)) * abs(pf_7_3)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.z));
	// 3269555566  <=>  {ftou((((({(vs_cbuf13_0.z) : 1.00} * {(vs_cbuf13_1.y) : 1.00}) * {utof(u_7_phi_76) : 1.00}) * {f_4_61 : 0.4249095}) + ({pf_6_11 : 235.3523} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00}))))))) : 3269555566}
	u_6_29 = ftou((((((vs_cbuf13_0.z) * (vs_cbuf13_1.y)) * utof(u_7_phi_76)) * f_4_61) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3)))))));
	u_6_phi_80 = u_6_29;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1074993895  <=>  {ftou(({utof(u_1_phi_79) : 9194.067} * {utof(u_5_phi_78) : 0.00025})) : 1074993895}
		u_6_30 = ftou((utof(u_1_phi_79) * utof(u_5_phi_78)));
		u_6_phi_80 = u_6_30;
	}
	// 3307551180  <=>  {ftou((((min({f_5_14 : 9194.067}, {utof(vs_cbuf9[197].x) : 4000.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 4000.00})) * ((0.f - {pf_1_9 : -2551.6372}) + (((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_17_16 : -0.0012988562}) + (({f_1_20 : 0.4249095} * ({utof(u_8_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00})) + ({pf_1_9 : -2551.6372} + (({pf_5_15 : 161.2986} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -23.685835} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -348.50656} * (0.f - sin({pf_13_9 : 0})))))))))) + {pf_1_9 : -2551.6372})) : 3307551180}
	u_1_28 = ftou((((min(f_5_14, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x))) * ((0.f - pf_1_9) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_17_16) + ((f_1_20 * (utof(u_8_phi_76) * (vs_cbuf13_0.z))) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9)))))))))) + pf_1_9));
	// 3307551180  <=>  {u_1_28 : }
	u_2_14 = u_1_28;
	u_2_phi_81 = u_2_14;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 3308040370  <=>  {ftou(((((0.f - {pf_1_9 : -2551.6372}) + (((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_17_16 : -0.0012988562}) + (({f_1_20 : 0.4249095} * ({utof(u_8_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00})) + ({pf_1_9 : -2551.6372} + (({pf_5_15 : 161.2986} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -23.685835} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -348.50656} * (0.f - sin({pf_13_9 : 0}))))))))) * {utof(u_6_phi_80) : 2.298517}) + {pf_1_9 : -2551.6372})) : 3308040370}
		u_5_28 = ftou(((((0.f - pf_1_9) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_17_16) + ((f_1_20 * (utof(u_8_phi_76) * (vs_cbuf13_0.z))) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))))) * utof(u_6_phi_80)) + pf_1_9));
		// 3308040370  <=>  {u_5_28 : }
		u_2_15 = u_5_28;
		u_2_phi_81 = u_2_15;
	}
	// 3269555568  <=>  {ftou((((min({f_5_14 : 9194.067}, {utof(vs_cbuf9[197].x) : 4000.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 4000.00})) * ((0.f - {pf_6_11 : 235.3523}) + (((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_20_17 : -0}) + (((({(vs_cbuf13_0.z) : 1.00} * {(vs_cbuf13_1.y) : 1.00}) * {utof(u_7_phi_76) : 1.00}) * {f_4_61 : 0.4249095}) + ({pf_6_11 : 235.3523} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00}))))))))) + {pf_6_11 : 235.3523})) : 3269555568}
	u_1_29 = ftou((((min(f_5_14, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x))) * ((0.f - pf_6_11) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_20_17) + (((((vs_cbuf13_0.z) * (vs_cbuf13_1.y)) * utof(u_7_phi_76)) * f_4_61) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3))))))))) + pf_6_11));
	// 3269555568  <=>  {u_1_29 : }
	u_5_29 = u_1_29;
	u_5_phi_82 = u_5_29;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 3289198089  <=>  {ftou(((((0.f - {pf_6_11 : 235.3523}) + (((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_20_17 : -0}) + (((({(vs_cbuf13_0.z) : 1.00} * {(vs_cbuf13_1.y) : 1.00}) * {utof(u_7_phi_76) : 1.00}) * {f_4_61 : 0.4249095}) + ({pf_6_11 : 235.3523} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -348.50656} * ({f_4_56 : 1.00} * {f_5_3 : 1.00})))))))) * {utof(u_6_phi_80) : 2.298517}) + {pf_6_11 : 235.3523})) : 3289198089}
		u_7_5 = ftou(((((0.f - pf_6_11) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_20_17) + (((((vs_cbuf13_0.z) * (vs_cbuf13_1.y)) * utof(u_7_phi_76)) * f_4_61) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * f_5_3)))))))) * utof(u_6_phi_80)) + pf_6_11));
		// 3289198089  <=>  {u_7_5 : }
		u_5_30 = u_7_5;
		u_5_phi_82 = u_5_30;
	}
	// 1168482171  <=>  {ftou((((min({f_5_14 : 9194.067}, {utof(vs_cbuf9[197].x) : 4000.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 4000.00})) * ((0.f - {pf_8_3 : 5438.32}) + (((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_11_23 : -0.0012222854}) + ((({utof(u_6_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00}) * {f_7_42 : -0.9052358}) + ({pf_8_3 : 5438.32} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00}))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -348.50656} * (sin({pf_12_7 : 0}) * {f_4_56 : 1.00}))))))))) + {pf_8_3 : 5438.32})) : 1168482171}
	u_1_30 = ftou((((min(f_5_14, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x))) * ((0.f - pf_8_3) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_11_23) + (((utof(u_6_phi_76) * (vs_cbuf13_0.z)) * f_7_42) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (sin(pf_12_7) * sin(pf_11_8))) + (cos(pf_11_8) * f_5_3))) + ((pf_20_14 * ((sin(pf_13_9) * (sin(pf_12_7) * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (sin(pf_12_7) * f_4_56))))))))) + pf_8_3));
	// 1168482171  <=>  {u_1_30 : }
	u_7_6 = u_1_30;
	u_7_phi_83 = u_7_6;
	// True  <=>  if(({b_1_67 : True} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1168114154  <=>  {ftou(((((0.f - {pf_8_3 : 5438.32}) + (((((clamp(min(0.f, {f_0_6 : 0.90784}), 0.0, 1.0) + {i.vao_attr5.x : 1505.775}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_11_23 : -0.0012222854}) + ((({utof(u_6_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00}) * {f_7_42 : -0.9052358}) + ({pf_8_3 : 5438.32} + (({pf_5_15 : 161.2986} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00}))) + (({pf_20_14 : -23.685835} * ((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -348.50656} * (sin({pf_12_7 : 0}) * {f_4_56 : 1.00})))))))) * {utof(u_6_phi_80) : 2.298517}) + {pf_8_3 : 5438.32})) : 1168114154}
		u_6_31 = ftou(((((0.f - pf_8_3) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_11_23) + (((utof(u_6_phi_76) * (vs_cbuf13_0.z)) * f_7_42) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (sin(pf_12_7) * sin(pf_11_8))) + (cos(pf_11_8) * f_5_3))) + ((pf_20_14 * ((sin(pf_13_9) * (sin(pf_12_7) * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (sin(pf_12_7) * f_4_56)))))))) * utof(u_6_phi_80)) + pf_8_3));
		// 1168114154  <=>  {u_6_31 : }
		u_7_7 = u_6_31;
		u_7_phi_83 = u_7_7;
	}
	// 2247.65  <=>  ((({utof(u_7_phi_83) : 5120.239} * {(view_proj[1].z) : 0.3768303}) + (({utof(u_5_phi_82) : -564.7193} * {(view_proj[1].y) : 0.8616711}) + ({utof(u_2_phi_81) : -2763.0435} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
	pf_4_8 = (((utof(u_7_phi_83) * (view_proj[1].z)) + ((utof(u_5_phi_82) * (view_proj[1].y)) + (utof(u_2_phi_81) * (view_proj[1].x)))) + (view_proj[1].w));
	// 6556.212  <=>  ((({utof(u_7_phi_83) : 5120.239} * {(view_proj[0].z) : 0.6697676}) + (({utof(u_5_phi_82) : -564.7193} * {(view_proj[0].y) : 1.493044E-08}) + ({utof(u_2_phi_81) : -2763.0435} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
	pf_7_15 = (((utof(u_7_phi_83) * (view_proj[0].z)) + ((utof(u_5_phi_82) * (view_proj[0].y)) + (utof(u_2_phi_81) * (view_proj[0].x)))) + (view_proj[0].w));
	// 1.00  <=>  ((({utof(u_7_phi_83) : 5120.239} * {(view_proj[3].z) : 0}) + (({utof(u_5_phi_82) : -564.7193} * {(view_proj[3].y) : 0}) + ({utof(u_2_phi_81) : -2763.0435} * {(view_proj[3].x) : 0}))) + {(view_proj[3].w) : 1.00})
	pf_5_25 = (((utof(u_7_phi_83) * (view_proj[3].z)) + ((utof(u_5_phi_82) * (view_proj[3].y)) + (utof(u_2_phi_81) * (view_proj[3].x)))) + (view_proj[3].w));
	// -5650.005  <=>  ((({utof(u_7_phi_83) : 5120.239} * {(view_proj[2].z) : -0.6398518}) + (({utof(u_5_phi_82) : -564.7193} * {(view_proj[2].y) : 0.5074672}) + ({utof(u_2_phi_81) : -2763.0435} * {(view_proj[2].x) : -0.57711935}))) + {(view_proj[2].w) : -3681.8398})
	pf_11_27 = (((utof(u_7_phi_83) * (view_proj[2].z)) + ((utof(u_5_phi_82) * (view_proj[2].y)) + (utof(u_2_phi_81) * (view_proj[2].x)))) + (view_proj[2].w));
	// 5650.005  <=>  (({pf_5_25 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[7].z) : -1}) + (({pf_4_8 : 2247.65} * {(view_proj[7].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[7].x) : 0}))))
	pf_11_28 = ((pf_5_25 * (view_proj[7].w)) + ((pf_11_27 * (view_proj[7].z)) + ((pf_4_8 * (view_proj[7].y)) + (pf_7_15 * (view_proj[7].x)))));
	// 930.4566  <=>  ((0.f - {utof(u_5_phi_82) : -564.7193}) + {(camera_wpos.y) : 365.7373})
	pf_16_30 = ((0.f - utof(u_5_phi_82)) + (camera_wpos.y));
	// 5649.85  <=>  (({pf_5_25 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_27 : -5650.005} * {(view_proj[6].z) : -1.000008}) + (({pf_4_8 : 2247.65} * {(view_proj[6].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[6].x) : 0}))))
	pf_4_11 = ((pf_5_25 * (view_proj[6].w)) + ((pf_11_27 * (view_proj[6].z)) + ((pf_4_8 * (view_proj[6].y)) + (pf_7_15 * (view_proj[6].x)))));
	// 843.7812  <=>  ((0.f - {utof(u_2_phi_81) : -2763.0435}) + {(camera_wpos.x) : -1919.2622})
	pf_5_26 = ((0.f - utof(u_2_phi_81)) + (camera_wpos.x));
	// 0  <=>  ((0.f * (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[4].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : 6556.212} * {(view_proj[5].x) : 0}))))))
	pf_20_18 = ((0.f * ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))))) + (0.f * ((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x)))))));
	// -8853.286  <=>  ((0.f - {utof(u_7_phi_83) : 5120.239}) + {(camera_wpos.z) : -3733.0469})
	pf_27_6 = ((0.f - utof(u_7_phi_83)) + (camera_wpos.z));
	// 1168114154  <=>  {u_7_phi_83 : 1168114154}
	u_1_31 = u_7_phi_83;
	u_1_phi_84 = u_1_31;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1175286005  <=>  {ftou(({utof(u_7_phi_83) : 5120.239} + (0.f - {(vs_cbuf15_52.y) : -3932}))) : 1175286005}
		u_1_32 = ftou((utof(u_7_phi_83) + (0.f - (vs_cbuf15_52.y))));
		u_1_phi_84 = u_1_32;
	}
	// 0  <=>  {u_26_20 : 0}
	u_6_33 = u_26_20;
	u_6_phi_85 = u_6_33;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1095552832  <=>  {ftou(({utof(u_1_phi_84) : 5120.239} * {(vs_cbuf15_52.z) : 0.0025})) : 1095552832}
		u_6_34 = ftou((utof(u_1_phi_84) * (vs_cbuf15_52.z)));
		u_6_phi_85 = u_6_34;
	}
	// 0.0001118  <=>  inversesqrt((({pf_27_6 : -8853.286} * {pf_27_6 : -8853.286}) + (({pf_16_30 : 930.4566} * {pf_16_30 : 930.4566}) + ({pf_5_26 : 843.7812} * {pf_5_26 : 843.7812}))))
	f_4_85 = inversesqrt(((pf_27_6 * pf_27_6) + ((pf_16_30 * pf_16_30) + (pf_5_26 * pf_5_26))));
	// -2.2607315  <=>  (1.0f / ((((({pf_11_28 : 5650.005} * 0.5f) + (({pf_4_11 : 5649.85} * 0.5f) + {pf_20_18 : 0})) * (1.0f / ({pf_11_28 : 5650.005} + ((0.f * {pf_4_11 : 5649.85}) + {pf_20_18 : 0})))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00})))
	f_4_88 = (1.0f / (((((pf_11_28 * 0.5f) + ((pf_4_11 * 0.5f) + pf_20_18)) * (1.0f / (pf_11_28 + ((0.f * pf_4_11) + pf_20_18)))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y))));
	// 0.5604841  <=>  ((((({pf_27_6 : -8853.286} * {f_4_85 : 0.0001118}) * {(lightDir.z) : -0.08728968}) + ((({pf_16_30 : 930.4566} * {f_4_85 : 0.0001118}) * {(lightDir.y) : -0.4663191}) + (({pf_5_26 : 843.7812} * {f_4_85 : 0.0001118}) * {(lightDir.x) : 0.8802994}))) * 0.5f) + 0.5f)
	pf_5_31 = (((((pf_27_6 * f_4_85) * (lightDir.z)) + (((pf_16_30 * f_4_85) * (lightDir.y)) + ((pf_5_26 * f_4_85) * (lightDir.x)))) * 0.5f) + 0.5f);
	// 1.471873  <=>  (({pf_5_31 : 0.5604841} * (({pf_5_31 : 0.5604841} * (({pf_5_31 : 0.5604841} * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
	pf_5_32 = ((pf_5_31 * ((pf_5_31 * ((pf_5_31 * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f);
	// 1057979363  <=>  {ftou(pf_5_31) : 1057979363}
	u_7_8 = ftou(pf_5_31);
	u_7_phi_86 = u_7_8;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 3290546888  <=>  {ftou(({utof(u_2_phi_81) : -2763.0435} + (0.f - {(vs_cbuf15_52.x) : -2116}))) : 3290546888}
		u_7_9 = ftou((utof(u_2_phi_81) + (0.f - (vs_cbuf15_52.x))));
		u_7_phi_86 = u_7_9;
	}
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(16u), int(16u))) * {u_5_21 : 0})) << 16u) + {u_1_13 : 0})
	u_1_35 = ((uint((uint(bitfieldExtract(uint(u_0_3), int(16u), int(16u))) * u_5_21)) << 16u) + u_1_13);
	u_1_phi_87 = u_1_35;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 985114832  <=>  {ftou(({utof(u_7_phi_86) : 0.5604841} * {(vs_cbuf15_52.z) : 0.0025})) : 985114832}
		u_1_36 = ftou((utof(u_7_phi_86) * (vs_cbuf15_52.z)));
		u_1_phi_87 = u_1_36;
	}
	// 0.8101302  <=>  exp2((log2(((0.f - clamp(((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.y) : 1.00}))
	f_5_27 = exp2((log2(((0.f - clamp((((f_4_88 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.y)));
	// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, float2(((({pf_5_32 : 1.471873} * (0.f - sqrt(((0.f - {pf_5_31 : 0.5604841}) + 1.f)))) * 0.63661975f) + 1.f), (({f_5_27 : 0.8101302} * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
	f4_0_0 = textureLod(tex0, float2((((pf_5_32 * (0.f - sqrt(((0.f - pf_5_31) + 1.f)))) * 0.63661975f) + 1.f), ((f_5_27 * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler);
	// 0  <=>  {u_1_phi_87 : }
	u_0_8 = u_1_phi_87;
	u_0_phi_88 = u_0_8;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex1 : tex1}, float2({utof(u_1_phi_87) : 0}, {utof(u_6_phi_85) : 0}), 0.0, s_linear_clamp_sampler)
		f4_0_1 = textureLod(tex1, float2(utof(u_1_phi_87), utof(u_6_phi_85)), 0.0, s_linear_clamp_sampler);
		// 1065353216  <=>  {ftou(f4_0_1.w) : 1065353216}
		u_0_9 = ftou(f4_0_1.w);
		u_0_phi_88 = u_0_9;
	}
	// 1.00  <=>  1.f
	o.fs_attr9.x = 1.f;
	// 7908.66  <=>  (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[4].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[4].x) : 1.206285}))))
	o.vertex.x = ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))));
	// 112  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 112u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_2_20 = ((ftou(vs_cbuf0_21.x) + 112u) - ftou(vs_cbuf0_21.x));
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x : }
	u_2_21 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y : }
	u_6_36 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z : }
	u_7_10 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z;
	// 128  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 128u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_1_39 = ((ftou(vs_cbuf0_21.x) + 128u) - ftou(vs_cbuf0_21.x));
	// 4820.101  <=>  (({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : 6556.212} * {(view_proj[5].x) : 0}))))
	o.vertex.y = ((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x)))));
	// 5649.85  <=>  {pf_4_11 : 5649.85}
	o.vertex.z = pf_4_11;
	// 5650.005  <=>  {pf_11_28 : 5650.005}
	o.vertex.w = pf_11_28;
	// 5649.927  <=>  (({pf_11_28 : 5650.005} * 0.5f) + (({pf_4_11 : 5649.85} * 0.5f) + {pf_20_18 : 0}))
	o.fs_attr4.z = ((pf_11_28 * 0.5f) + ((pf_4_11 * 0.5f) + pf_20_18));
	// 5650.005  <=>  ({pf_11_28 : 5650.005} + ((0.f * {pf_4_11 : 5649.85}) + {pf_20_18 : 0}))
	o.fs_attr4.w = (pf_11_28 + ((0.f * pf_4_11) + pf_20_18));
	// 5569.526  <=>  ((({pf_8_3 : 5438.32} * {(vs_cbuf8_11.z) : 0.6398518}) + (({pf_6_11 : 235.3523} * {(vs_cbuf8_11.y) : -0.50746715}) + ({pf_1_9 : -2551.6372} * {(vs_cbuf8_11.x) : 0.5771194}))) + {(vs_cbuf8_11.w) : 3681.84})
	pf_5_38 = (((pf_8_3 * (vs_cbuf8_11.z)) + ((pf_6_11 * (vs_cbuf8_11.y)) + (pf_1_9 * (vs_cbuf8_11.x)))) + (vs_cbuf8_11.w));
	// 5569.37  <=>  ((({pf_8_3 : 5438.32} * {(vs_cbuf8_10.z) : 0.6398569}) + (({pf_6_11 : 235.3523} * {(vs_cbuf8_10.y) : -0.5074712}) + ({pf_1_9 : -2551.6372} * {(vs_cbuf8_10.x) : 0.5771239}))) + {(vs_cbuf8_10.w) : 3681.669})
	pf_1_13 = (((pf_8_3 * (vs_cbuf8_10.z)) + ((pf_6_11 * (vs_cbuf8_10.y)) + (pf_1_9 * (vs_cbuf8_10.x)))) + (vs_cbuf8_10.w));
	// -0.18466002  <=>  ((({(vs_cbuf13_2.w) : 40.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[79].y) : 0}) + ((((({pf_0_1 : 183.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_35_phi_47) : 0.16926} * {utof(vs_cbuf9[81].z) : 0}) + ({utof(vs_cbuf9[81].z) : 0} + {utof(vs_cbuf9[81].x) : 0.80}))) * (({pf_27_2 : 1.00} * {utof(u_16_phi_27) : 0.4939}) + -0.5f)) + (({pf_27_2 : 1.00} * float(int({u_1_7 : 0}))) + (({pf_0_1 : 183.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.0001})) + (0.f - ((({utof(u_36_phi_47) : 0.16926} * {utof(vs_cbuf9[80].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 1.00} + {utof(vs_cbuf9[79].z) : 0})))))) + 0.5f))
	o.fs_attr2.z = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[79].y)) + (((((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_35_phi_47) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].z) + utof(vs_cbuf9[81].x)))) * ((pf_27_2 * utof(u_16_phi_27)) + -0.5f)) + ((pf_27_2 * float(int(u_1_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_36_phi_47) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))) + 0.5f));
	// 1.501508  <=>  ((({(vs_cbuf13_2.w) : 40.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_24_4 : 1.00} * {utof(u_5_phi_24) : 0.4939}) + -0.5f) * (({pf_0_1 : 183.00} * {utof(vs_cbuf9[75].z) : 0}) + (({f_0_6 : 0.90784} * {utof(vs_cbuf9[76].z) : 0.10}) + ({utof(vs_cbuf9[76].x) : 4.50} + {utof(vs_cbuf9[76].z) : 0.10})))) + (({pf_24_4 : 1.00} * float(int({u_2_7 : 0}))) + (({pf_0_1 : 183.00} * (0.f - {utof(vs_cbuf9[74].x) : 0})) + (0.f - ((({f_0_6 : 0.90784} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0})))))) + 0.5f))
	o.fs_attr2.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[74].y)) + (((((pf_24_4 * utof(u_5_phi_24)) + -0.5f) * ((pf_0_1 * utof(vs_cbuf9[75].z)) + ((f_0_6 * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z))))) + ((pf_24_4 * float(int(u_2_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((f_0_6 * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f));
	// 1041254423  <=>  {ftou(v.offset.y) : 1041254423}
	u_15_22 = ftou(v.offset.y);
	u_15_phi_89 = u_15_22;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1041254423  <=>  {ftou((({v.vertex.y : -0.34717} * {(vs_cbuf13_0.x) : 0}) + {v.offset.y : 0.1409})) : 1041254423}
		u_15_23 = ftou(((v.vertex.y * (vs_cbuf13_0.x)) + v.offset.y));
		u_15_phi_89 = u_15_23;
	}
	// 0  <=>  {ftou(v.offset.x) : 0}
	u_10_37 = ftou(v.offset.x);
	u_10_phi_90 = u_10_37;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.x : -0.01573} * {(vs_cbuf13_0.x) : 0}) + {v.offset.x : 0})) : 0}
		u_10_38 = ftou(((v.vertex.x * (vs_cbuf13_0.x)) + v.offset.x));
		u_10_phi_90 = u_10_38;
	}
	// 1065189135  <=>  {ftou(v.offset.z) : 1065189135}
	u_12_33 = ftou(v.offset.z);
	u_12_phi_91 = u_12_33;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1065189135  <=>  {ftou((({v.vertex.z : 0.10712} * {(vs_cbuf13_0.x) : 0}) + {v.offset.z : 0.99022})) : 1065189135}
		u_12_34 = ftou(((v.vertex.z * (vs_cbuf13_0.x)) + v.offset.z));
		u_12_phi_91 = u_12_34;
	}
	// -0.44978526  <=>  ((((({pf_1_13 : 5569.37} * 0.5f) + ({pf_5_38 : 5569.526} * 0.5f)) * (1.0f / ((0.f * {pf_1_13 : 5569.37}) + {pf_5_38 : 5569.526}))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))
	pf_1_16 = (((((pf_1_13 * 0.5f) + (pf_5_38 * 0.5f)) * (1.0f / ((0.f * pf_1_13) + pf_5_38))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)));
	// 0.9633862  <=>  ((({(vs_cbuf13_2.w) : 40.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[84].y) : 0}) + ((((((({pf_22_8 : 1.00} * {utof(u_13_phi_30) : 0.4939}) + -0.5f) * cos({f_4_40 : -7.3466773})) + (0.f - ((({pf_17_6 : 1.00} * {utof(u_23_phi_39) : 0.98535}) + -0.5f) * sin({f_4_40 : -7.3466773})))) * (({pf_0_1 : 183.00} * {utof(vs_cbuf9[85].z) : 0}) + (({utof(u_39_phi_58) : 0.90784} * {utof(vs_cbuf9[86].z) : 0}) + ({utof(vs_cbuf9[86].x) : 1.10} + {utof(vs_cbuf9[86].z) : 0})))) + (({pf_22_8 : 1.00} * float(int({u_1_14 : 0}))) + (({pf_0_1 : 183.00} * (0.f - {utof(vs_cbuf9[84].x) : 0})) + (0.f - ((({utof(u_35_phi_58) : 0.76809} * {utof(vs_cbuf9[85].x) : 0}) * -2.f) + ({utof(vs_cbuf9[85].x) : 0} + {utof(vs_cbuf9[84].z) : 0})))))) + 0.5f))
	o.fs_attr3.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[84].y)) + (((((((pf_22_8 * utof(u_13_phi_30)) + -0.5f) * cos(f_4_40)) + (0.f - (((pf_17_6 * utof(u_23_phi_39)) + -0.5f) * sin(f_4_40)))) * ((pf_0_1 * utof(vs_cbuf9[85].z)) + ((utof(u_39_phi_58) * utof(vs_cbuf9[86].z)) + (utof(vs_cbuf9[86].x) + utof(vs_cbuf9[86].z))))) + ((pf_22_8 * float(int(u_1_14))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[84].x))) + (0.f - (((utof(u_35_phi_58) * utof(vs_cbuf9[85].x)) * -2.f) + (utof(vs_cbuf9[85].x) + utof(vs_cbuf9[84].z))))))) + 0.5f));
	// -0.66321725  <=>  ((({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697})) * {utof(u_12_phi_91) : 0.99022}) + (((cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00}) * {utof(u_10_phi_90) : 0}) + (sin({pf_13_9 : 0}) * (0.f - {utof(u_15_phi_89) : 0.1409}))))
	o.fs_attr8.x = (((f_4_56 * sin(pf_11_8)) * utof(u_12_phi_91)) + (((cos(pf_11_8) * f_4_56) * utof(u_10_phi_90)) + (sin(pf_13_9) * (0.f - utof(u_15_phi_89)))));
	// 0.1409  <=>  ((((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697})))) * {utof(u_12_phi_91) : 0.99022}) + ((((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) + (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) * {utof(u_10_phi_90) : 0}) + (({f_4_56 : 1.00} * {f_5_3 : 1.00}) * {utof(u_15_phi_89) : 0.1409})))
	o.fs_attr8.y = ((((sin(pf_13_9) * pf_22_10) + (0.f - (sin(pf_12_7) * cos(pf_11_8)))) * utof(u_12_phi_91)) + ((((sin(pf_13_9) * (cos(pf_11_8) * f_5_3)) + (sin(pf_12_7) * sin(pf_11_8))) * utof(u_10_phi_90)) + ((f_4_56 * f_5_3) * utof(u_15_phi_89))));
	// -0.73530847  <=>  ((((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * {f_5_3 : 1.00})) * {utof(u_12_phi_91) : 0.99022}) + ((((sin({pf_13_9 : 0}) * (sin({pf_12_7 : 0}) * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756})) * {utof(u_10_phi_90) : 0}) + ((sin({pf_12_7 : 0}) * {f_4_56 : 1.00}) * {utof(u_15_phi_89) : 0.1409})))
	o.fs_attr8.z = ((((sin(pf_13_9) * (sin(pf_12_7) * sin(pf_11_8))) + (cos(pf_11_8) * f_5_3)) * utof(u_12_phi_91)) + ((((sin(pf_13_9) * (sin(pf_12_7) * cos(pf_11_8))) + (0.f - pf_22_10)) * utof(u_10_phi_90)) + ((sin(pf_12_7) * f_4_56) * utof(u_15_phi_89))));
	// 414.9519  <=>  (({pf_11_28 : 5650.005} * 0.5f) + ((0.f * {pf_4_11 : 5649.85}) + ((0.f * (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[4].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : 6556.212} * {(view_proj[5].x) : 0})))) * -0.5f))))
	o.fs_attr4.y = ((pf_11_28 * 0.5f) + ((0.f * pf_4_11) + ((0.f * ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))))) + (((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x))))) * -0.5f))));
	// 0.20  <=>  (clamp((((min({(camera_wpos.y) : 365.7373}, {(vs_cbuf15_27.z) : 250.00}) + (0.f - {utof(u_5_phi_82) : -564.7193})) * {(vs_cbuf15_27.y) : 0.0071429}) + {(vs_cbuf15_27.x) : -0.14285715}), 0.0, 1.0) * {(vs_cbuf15_26.w) : 0.20})
	o.fs_attr10.y = (clamp((((min((camera_wpos.y), (vs_cbuf15_27.z)) + (0.f - utof(u_5_phi_82))) * (vs_cbuf15_27.y)) + (vs_cbuf15_27.x)), 0.0, 1.0) * (vs_cbuf15_26.w));
	// 0.98535  <=>  (((({(vs_cbuf13_2.w) : 40.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[79].y) : 0}) + ((((({pf_0_1 : 183.00} * {utof(vs_cbuf9[80].w) : 0}) + (({utof(u_33_phi_47) : 0.76809} * {utof(vs_cbuf9[81].w) : 0}) + ({utof(vs_cbuf9[81].w) : 0} + {utof(vs_cbuf9[81].y) : 1.00}))) * (({pf_28_2 : 1.00} * {utof(u_22_phi_38) : 0.98535}) + -0.5f)) + (0.f - (({pf_28_2 : 1.00} * (0.f - float(int((({b_5_5 : False} || {b_1_57 : True}) ? {u_8_18 : 0} : 4294967295u))))) + (({pf_0_1 : 183.00} * {utof(vs_cbuf9[79].y) : 0}) + ((({utof(u_34_phi_47) : 0.76809} * {utof(vs_cbuf9[80].y) : 0}) * -2.f) + ({utof(vs_cbuf9[80].y) : 0} + {utof(vs_cbuf9[79].w) : 0})))))) + 0.5f))
	o.fs_attr2.w = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[79].y)) + (((((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_33_phi_47) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y)))) * ((pf_28_2 * utof(u_22_phi_38)) + -0.5f)) + (0.f - ((pf_28_2 * (0.f - float(int(((b_5_5 || b_1_57) ? u_8_18 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_34_phi_47) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f));
	// 1.667884  <=>  (((({(vs_cbuf13_2.w) : 40.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_0_1 : 183.00} * {utof(vs_cbuf9[75].w) : 0}) + (({f_8_13 : 0.16926} * {utof(vs_cbuf9[76].w) : 0.10}) + ({utof(vs_cbuf9[76].w) : 0.10} + {utof(vs_cbuf9[76].y) : 1.40}))) * (({pf_28_1 : 1.00} * {utof(u_12_phi_37) : 0.98535}) + -0.5f)) + (0.f - (({pf_28_1 : 1.00} * (0.f - float(int((({b_1_53 : False} || {b_0_13 : True}) ? {u_9_14 : 0} : 4294967295u))))) + (({pf_0_1 : 183.00} * {utof(vs_cbuf9[74].y) : -0.0001}) + ((({f_8_13 : 0.16926} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0})))))) + 0.5f))
	o.fs_attr2.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[74].y)) + (((((pf_0_1 * utof(vs_cbuf9[75].w)) + ((f_8_13 * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)))) * ((pf_28_1 * utof(u_12_phi_37)) + -0.5f)) + (0.f - ((pf_28_1 * (0.f - float(int(((b_1_53 || b_0_13) ? u_9_14 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((f_8_13 * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f));
	// 0.7652385  <=>  (((({(vs_cbuf13_2.w) : 40.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[84].y) : 0}) + ((((((({pf_22_8 : 1.00} * {utof(u_13_phi_30) : 0.4939}) + -0.5f) * sin({f_4_40 : -7.3466773})) + (cos({f_4_40 : -7.3466773}) * (({pf_17_6 : 1.00} * {utof(u_23_phi_39) : 0.98535}) + -0.5f))) * (({pf_0_1 : 183.00} * {utof(vs_cbuf9[85].w) : 0}) + (({utof(u_41_phi_58) : 0.16926} * {utof(vs_cbuf9[86].w) : 0}) + ({utof(vs_cbuf9[86].w) : 0} + {utof(vs_cbuf9[86].y) : 1.10})))) + (0.f - (({pf_17_6 : 1.00} * (0.f - float(int((({b_5_6 : False} || {b_0_15 : True}) ? {u_10_29 : 0} : 4294967295u))))) + (({pf_0_1 : 183.00} * {utof(vs_cbuf9[84].y) : 0}) + ((({utof(u_37_phi_58) : 0.90784} * {utof(vs_cbuf9[85].y) : 0}) * -2.f) + ({utof(vs_cbuf9[85].y) : 0} + {utof(vs_cbuf9[84].w) : 0})))))) + 0.5f))
	o.fs_attr3.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[84].y)) + (((((((pf_22_8 * utof(u_13_phi_30)) + -0.5f) * sin(f_4_40)) + (cos(f_4_40) * ((pf_17_6 * utof(u_23_phi_39)) + -0.5f))) * ((pf_0_1 * utof(vs_cbuf9[85].w)) + ((utof(u_41_phi_58) * utof(vs_cbuf9[86].w)) + (utof(vs_cbuf9[86].w) + utof(vs_cbuf9[86].y))))) + (0.f - ((pf_17_6 * (0.f - float(int(((b_5_6 || b_0_15) ? u_10_29 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[84].y)) + (((utof(u_37_phi_58) * utof(vs_cbuf9[85].y)) * -2.f) + (utof(vs_cbuf9[85].y) + utof(vs_cbuf9[84].w))))))) + 0.5f));
	// 1.00  <=>  (clamp(((((1.0f / {pf_1_16 : -0.44978526}) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 1800.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[138].x) : 1800.00}) + {utof(vs_cbuf9[138].y) : 2200.00}))), 0.0, 1.0) * {(vs_cbuf10_3.x) : 1.00})
	o.fs_attr6.x = (clamp(((((1.0f / pf_1_16) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x))) * (1.0f / ((0.f - utof(vs_cbuf9[138].x)) + utof(vs_cbuf9[138].y)))), 0.0, 1.0) * (vs_cbuf10_3.x));
	// 6779.333  <=>  (({pf_11_28 : 5650.005} * 0.5f) + ((0.f * {pf_4_11 : 5649.85}) + (((({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[4].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * (({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : 6556.212} * {(view_proj[5].x) : 0}))))))))
	o.fs_attr4.x = ((pf_11_28 * 0.5f) + ((0.f * pf_4_11) + ((((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x))))) * 0.5f) + (0.f * ((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x)))))))));
	// 0.7985901  <=>  clamp(((((({pf_11_28 : 5650.005} * 0.5f) + ((0.f * {pf_4_11 : 5649.85}) + ((0.f * (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[4].y) : 0}) + ({pf_7_15 : 6556.212} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -5650.005} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 2247.65} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : 6556.212} * {(view_proj[5].x) : 0})))) * -0.5f)))) * (1.0f / ({pf_11_28 : 5650.005} + ((0.f * {pf_4_11 : 5649.85}) + {pf_20_18 : 0})))) * -0.7f) + 0.85f), 0.0, 1.0)
	f_0_13 = clamp((((((pf_11_28 * 0.5f) + ((0.f * pf_4_11) + ((0.f * ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))))) + (((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x))))) * -0.5f)))) * (1.0f / (pf_11_28 + ((0.f * pf_4_11) + pf_20_18)))) * -0.7f) + 0.85f), 0.0, 1.0);
	// ((0.f - {utof(u_2_21) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
	pf_0_11 = ((0.f - utof(u_2_21)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x));
	// ((0.f - {utof(u_6_36) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
	pf_1_24 = ((0.f - utof(u_6_36)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y));
	// ((0.f - {utof(u_7_10) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
	pf_3_10 = ((0.f - utof(u_7_10)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z));
	// 0.0148285  <=>  exp2((log2(((0.f - clamp(((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.x) : 20.00}))
	f_0_15 = exp2((log2(((0.f - clamp((((f_4_88 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.x)));
	// ({utof(u_2_21) : } + (({pf_0_11 : } * (0.f - {f_0_13 : 0.7985901})) + {pf_0_11 : }))
	pf_0_13 = (utof(u_2_21) + ((pf_0_11 * (0.f - f_0_13)) + pf_0_11));
	// ({utof(u_6_36) : } + (({pf_1_24 : } * (0.f - {f_0_13 : 0.7985901})) + {pf_1_24 : }))
	pf_1_26 = (utof(u_6_36) + ((pf_1_24 * (0.f - f_0_13)) + pf_1_24));
	// ({utof(u_7_10) : } + (({pf_3_10 : } * (0.f - {f_0_13 : 0.7985901})) + {pf_3_10 : }))
	pf_3_12 = (utof(u_7_10) + ((pf_3_10 * (0.f - f_0_13)) + pf_3_10));
	// (({pf_0_13 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_0_13 : })
	pf_0_14 = ((pf_0_13 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_0_13);
	// {pf_0_14 : }
	o.fs_attr11.x = pf_0_14;
	// 0  <=>  exp2((log2(((0.f - clamp(((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_24.x) : 0.002381}) + (0.f - {(vs_cbuf15_24.y) : -0.04761905})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_24.w) : 4.00}))
	f_1_83 = exp2((log2(((0.f - clamp((((f_4_88 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_24.x)) + (0.f - (vs_cbuf15_24.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_24.w)));
	// (({pf_1_26 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_1_26 : })
	pf_0_16 = ((pf_1_26 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_1_26);
	// (({pf_3_12 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_3_12 : })
	pf_1_27 = ((pf_3_12 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_3_12);
	// {pf_0_16 : }
	o.fs_attr11.y = pf_0_16;
	// {pf_1_27 : }
	o.fs_attr11.z = pf_1_27;
	// 0.7006614  <=>  (({f_1_83 : 0} * (0.f - {(vs_cbuf15_25.w) : 0.7006614})) + {(vs_cbuf15_25.w) : 0.7006614})
	o.fs_attr10.x = ((f_1_83 * (0.f - (vs_cbuf15_25.w))) + (vs_cbuf15_25.w));
	// 0.8373958  <=>  clamp((({f_0_15 : 0.0148285} * (0.f - {(vs_cbuf15_23.z) : 0.85})) + {(vs_cbuf15_23.z) : 0.85}), 0.0, 1.0)
	o.fs_attr12.w = clamp(((f_0_15 * (0.f - (vs_cbuf15_23.z))) + (vs_cbuf15_23.z)), 0.0, 1.0);
	// 0.50  <=>  (clamp(max((((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.x : 0.50})
	o.fs_attr12.x = (clamp(max(((((f_4_88 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.x);
	// 0.50  <=>  (clamp(max((((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.y : 0.50})
	o.fs_attr12.y = (clamp(max(((((f_4_88 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.y);
	// 0.50  <=>  (clamp(max((((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.z : 0.50})
	o.fs_attr12.z = (clamp(max(((((f_4_88 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.z);
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
		f_0_18 = (1.0f / (vs_cbuf15_51.x));
		// 1.00  <=>  ((({utof(u_0_phi_88) : 0} * {(vs_cbuf15_49.x) : 0}) + (0.f - {(vs_cbuf15_49.x) : 0})) + 1.f)
		pf_0_21 = (((utof(u_0_phi_88) * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f);
		// 1.00  <=>  clamp(((({f_4_88 : -2.2607315} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {f_0_18 : 0.0010526}) + (0.f - ({f_0_18 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
		f_0_19 = clamp((((f_4_88 * (0.f - (vs_cbuf8_30.z))) * f_0_18) + (0.f - (f_0_18 * (vs_cbuf15_51.y)))), 0.0, 1.0);
		// -∞  <=>  log2(abs((({pf_0_21 : 1.00} * (0.f - {f_0_19 : 1.00})) + {f_0_19 : 1.00})))
		f_0_21 = log2(abs(((pf_0_21 * (0.f - f_0_19)) + f_0_19)));
		// 0  <=>  exp2(({f_0_21 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
		f_0_22 = exp2((f_0_21 * (vs_cbuf15_51.z)));
		// 1.00  <=>  (({pf_0_21 : 1.00} * (0.f - (({f_0_22 : 0} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0}))) + {pf_0_21 : 1.00})
		o.fs_attr9.x = ((pf_0_21 * (0.f - ((f_0_22 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_21);
	}
	// 0  <=>  0.f
	o.fs_attr10.w = 0.f;
	// True  <=>  if(((! (((clamp(((((1.0f / {pf_1_16 : -0.44978526}) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 1800.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[138].x) : 1800.00}) + {utof(vs_cbuf9[138].y) : 2200.00}))), 0.0, 1.0) <= 0.f) && (! myIsNaN(clamp(((((1.0f / {pf_1_16 : -0.44978526}) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 1800.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[138].x) : 1800.00}) + {utof(vs_cbuf9[138].y) : 2200.00}))), 0.0, 1.0)))) && (! myIsNaN(0.f)))) ? true : false))
	if(((! (((clamp(((((1.0f / pf_1_16) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x))) * (1.0f / ((0.f - utof(vs_cbuf9[138].x)) + utof(vs_cbuf9[138].y)))), 0.0, 1.0) <= 0.f) && (! myIsNaN(clamp(((((1.0f / pf_1_16) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x))) * (1.0f / ((0.f - utof(vs_cbuf9[138].x)) + utof(vs_cbuf9[138].y)))), 0.0, 1.0)))) && (! myIsNaN(0.f)))) ? true : false))
	{
		return;
	}
	// 0  <=>  0.f
	o.vertex.x = 0.f;
	// 0  <=>  0.f
	o.vertex.y = 0.f;
	// 125000.00  <=>  ({(vs_cbuf8_30.y) : 25000.00} * 5.f)
	o.vertex.z = ((vs_cbuf8_30.y) * 5.f);
	// 0  <=>  0.f
	o.fs_attr6.x = 0.f;
	return;
}
