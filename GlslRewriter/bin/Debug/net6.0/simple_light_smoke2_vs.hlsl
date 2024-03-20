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
	// vs_cbuf9[1] = float4(73899130000000000000000.00, 6.410001E-10, 63868990000000000000000000000.00, 7.713018E+31);
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
	// vs_cbuf9[81] = float4(0.70, 1.00, 0, 0);
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
	// vs_cbuf9[104] = float4(1.80, 0, 0, 0);
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
	// vs_cbuf9[121] = float4(0.3437264, 0.3889022, 0.4047619, 0);
	// vs_cbuf9[122] = float4(0, 0, 0, 0);
	// vs_cbuf9[123] = float4(0, 0, 0, 0);
	// vs_cbuf9[124] = float4(0, 0, 0, 0);
	// vs_cbuf9[125] = float4(0, 0, 0, 0);
	// vs_cbuf9[126] = float4(0, 0, 0, 0);
	// vs_cbuf9[127] = float4(0, 0, 0, 0);
	// vs_cbuf9[128] = float4(0, 0, 0, 0);
	// vs_cbuf9[129] = float4(0, 0, 0, 0);
	// vs_cbuf9[130] = float4(0.40, 0.40, 0.40, 0.40);
	// vs_cbuf9[131] = float4(0, 0, 0, 1.00);
	// vs_cbuf9[132] = float4(0, 0, 0, 4.00);
	// vs_cbuf9[133] = float4(0, 0, 0, 5.00);
	// vs_cbuf9[134] = float4(0, 0, 0, 6.00);
	// vs_cbuf9[135] = float4(0, 0, 0, 7.00);
	// vs_cbuf9[136] = float4(0, 0, 0, 8.00);
	// vs_cbuf9[137] = float4(0, 0.50, 0, 1.00);
	// vs_cbuf9[138] = float4(900.00, 1100.00, 1800.00, 2200.00);
	// vs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[140] = float4(1.00, 300.00, 0, 0);
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
	// vs_cbuf9[197] = float4(2000.00, 2000.00, 0, 0);
	// vs_cbuf10[0] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[1] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[2] = float4(879.50, 86.00, 1.00, 1.00);
	// vs_cbuf10[3] = float4(1.00, 0.9999999, 1.00, 0.9999999);
	// vs_cbuf10[4] = float4(-0.74257076, 0, -0.66976756, -1919.2622);
	// vs_cbuf10[5] = float4(0, 1.00, 0, 365.7375);
	// vs_cbuf10[6] = float4(0.6697676, 0, -0.74257076, -3733.0469);
	// vs_cbuf13[0] = float4(0, 0.50, 1.00, 0.50);
	// vs_cbuf13[1] = float4(0.025, 1.00, 1.00, 0);
	// vs_cbuf13[2] = float4(100.00, 0.01, 1.00, 45.00);
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
	bool b_1_78;
	bool b_2_23;
	bool b_2_25;
	bool b_3_4;
	bool b_3_6;
	bool b_4_9;
	bool b_5_5;
	bool b_5_6;
	float f_0_11;
	float f_0_23;
	float f_0_26;
	float f_0_27;
	float f_0_29;
	float f_0_30;
	float f_0_6;
	float f_1_17;
	float f_1_20;
	float f_1_84;
	float f_10_8;
	float f_2_102;
	float f_2_103;
	float f_2_134;
	float f_2_142;
	float f_2_176;
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
	float f_3_97;
	float f_4_16;
	float f_4_22;
	float f_4_27;
	float f_4_35;
	float f_4_36;
	float f_4_40;
	float f_4_56;
	float f_4_61;
	float f_4_86;
	float f_4_89;
	float f_5_14;
	float f_5_2;
	float f_5_26;
	float f_6_20;
	float f_6_6;
	float f_7_13;
	float f_7_16;
	float f_7_26;
	float f_7_42;
	float f_8_10;
	float f_8_11;
	float f_8_13;
	float f_9_49;
	float f_9_64;
	float f_9_65;
	float4 f4_0_0;
	float4 f4_0_1;
	float pf_0_1;
	float pf_0_15;
	float pf_0_20;
	float pf_1_1;
	float pf_1_13;
	float pf_1_16;
	float pf_1_25;
	float pf_1_27;
	float pf_1_29;
	float pf_1_30;
	float pf_1_32;
	float pf_1_5;
	float pf_1_9;
	float pf_11_11;
	float pf_11_15;
	float pf_11_21;
	float pf_11_23;
	float pf_11_27;
	float pf_11_28;
	float pf_11_8;
	float pf_12_5;
	float pf_12_7;
	float pf_13_9;
	float pf_14_5;
	float pf_16_1;
	float pf_16_15;
	float pf_16_20;
	float pf_16_21;
	float pf_16_22;
	float pf_16_30;
	float pf_16_7;
	float pf_17_15;
	float pf_17_16;
	float pf_17_6;
	float pf_17_9;
	float pf_19_8;
	float pf_2_16;
	float pf_2_27;
	float pf_2_29;
	float pf_2_5;
	float pf_20_14;
	float pf_20_17;
	float pf_20_19;
	float pf_21_19;
	float pf_22_10;
	float pf_22_8;
	float pf_24_4;
	float pf_27_2;
	float pf_27_6;
	float pf_28_1;
	float pf_28_2;
	float pf_28_3;
	float pf_28_4;
	float pf_3_18;
	float pf_3_20;
	float pf_3_4;
	float pf_4_11;
	float pf_4_8;
	float pf_5_15;
	float pf_5_25;
	float pf_5_26;
	float pf_5_31;
	float pf_5_32;
	float pf_5_38;
	float pf_5_5;
	float pf_6_11;
	float pf_6_5;
	float pf_7_1;
	float pf_7_15;
	float pf_7_3;
	float pf_7_4;
	float pf_8_3;
	uint u_0_1;
	uint u_0_11;
	uint u_0_12;
	uint u_0_3;
	uint u_0_8;
	uint u_0_9;
	uint u_0_phi_86;
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
	uint u_1_40;
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
	uint u_10_phi_15;
	uint u_10_phi_17;
	uint u_10_phi_70;
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
	uint u_2_16;
	uint u_2_17;
	uint u_2_19;
	uint u_2_2;
	uint u_2_20;
	uint u_2_7;
	uint u_2_phi_64;
	uint u_2_phi_81;
	uint u_2_phi_85;
	uint u_2_phi_90;
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
	uint u_6_34;
	uint u_6_35;
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
	uint u_7_3;
	uint u_7_4;
	uint u_7_5;
	uint u_7_6;
	uint u_7_7;
	uint u_7_9;
	uint u_7_phi_76;
	uint u_7_phi_83;
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
	uint u_8_30;
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
	// -1263.2722  <=>  float(-1263.27222)
	o.vertex.x = float(-1263.27222);
	// 493.2664  <=>  float(493.26645)
	o.vertex.y = float(493.26645);
	// 1389.843  <=>  float(1389.84314)
	o.vertex.z = float(1389.84314);
	// 1390.032  <=>  float(1390.03198)
	o.vertex.w = float(1390.03198);
	// 1.47143  <=>  float(1.47143)
	o.fs_attr0.x = float(1.47143);
	// 1.46435  <=>  float(1.46435)
	o.fs_attr0.y = float(1.46435);
	// 1.34297  <=>  float(1.34297)
	o.fs_attr0.z = float(1.34297);
	// 0.10  <=>  float(0.10)
	o.fs_attr0.w = float(0.10);
	// 0.61871  <=>  float(0.61871)
	o.fs_attr1.x = float(0.61871);
	// 0.70002  <=>  float(0.70002)
	o.fs_attr1.y = float(0.70002);
	// 0.72857  <=>  float(0.72857)
	o.fs_attr1.z = float(0.72857);
	// 0.27867  <=>  float(0.27867)
	o.fs_attr1.w = float(0.27867);
	// 0.97269  <=>  float(0.97269)
	o.fs_attr2.x = float(0.97269);
	// 2.26966  <=>  float(2.26966)
	o.fs_attr2.y = float(2.26966);
	// 0.25095  <=>  float(0.25095)
	o.fs_attr2.z = float(0.25095);
	// 0.98535  <=>  float(0.98535)
	o.fs_attr2.w = float(0.98535);
	// 0.83993  <=>  float(0.83993)
	o.fs_attr3.x = float(0.83993);
	// 0.08826  <=>  float(0.08826)
	o.fs_attr3.y = float(0.08826);
	// 0  <=>  float(0.00)
	o.fs_attr3.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr3.w = float(1.00);
	// 63.37988  <=>  float(63.37988)
	o.fs_attr4.x = float(63.37988);
	// 448.3828  <=>  float(448.38275)
	o.fs_attr4.y = float(448.38275);
	// 1389.938  <=>  float(1389.9375)
	o.fs_attr4.z = float(1389.9375);
	// 1390.032  <=>  float(1390.03198)
	o.fs_attr4.w = float(1390.03198);
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
	// 0.07766  <=>  float(0.07766)
	o.fs_attr11.x = float(0.07766);
	// 0.07766  <=>  float(0.07766)
	o.fs_attr11.y = float(0.07766);
	// 0.07766  <=>  float(0.07766)
	o.fs_attr11.z = float(0.07766);
	// 1.00  <=>  float(1.00)
	o.fs_attr11.w = float(1.00);
	// 0.00111  <=>  float(0.00111)
	o.fs_attr12.x = float(0.00111);
	// 0.00877  <=>  float(0.00877)
	o.fs_attr12.y = float(0.00877);
	// 0.00901  <=>  float(0.00901)
	o.fs_attr12.z = float(0.00901);
	// 0.5317  <=>  float(0.5317)
	o.fs_attr12.w = float(0.5317);
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_8_0 = 0u;
	u_8_phi_2 = u_8_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_8_1 = ftou(vs_cbuf8_30.y);
		u_8_phi_2 = u_8_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_8_phi_2 : 0}
	u_6_1 = u_8_phi_2;
	u_6_phi_4 = u_6_1;
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_8_phi_2) : 0} * 5.f)) : 0}
		u_6_2 = ftou((utof(u_8_phi_2) * 5.f));
		u_6_phi_4 = u_6_2;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr6.x = 0.f;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {utof(u_6_phi_4) : 0}
		o.vertex.z = utof(u_6_phi_4);
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		return;
	}
	// 291.00  <=>  ((0.f - {i.vao_attr4.w : 588.50}) + {(vs_cbuf10_2.x) : 879.50})
	pf_0_1 = ((0.f - i.vao_attr4.w) + (vs_cbuf10_2.x));
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 1142104064  <=>  {ftou(i.vao_attr4.w) : 1142104064}
	u_8_2 = ftou(i.vao_attr4.w);
	u_8_phi_9 = u_8_2;
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_8_3 = ftou(vs_cbuf8_30.y);
		u_8_phi_9 = u_8_3;
	}
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 1142104064  <=>  {u_8_phi_9 : 1142104064}
	u_6_4 = u_8_phi_9;
	u_6_phi_11 = u_6_4;
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1161291776  <=>  {ftou(({utof(u_8_phi_9) : 588.50} * 5.f)) : 1161291776}
		u_6_5 = ftou((utof(u_8_phi_9) * 5.f));
		u_6_phi_11 = u_6_5;
	}
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr6.x = 0.f;
	}
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 588.50  <=>  {utof(u_6_phi_11) : 588.50}
		o.vertex.z = utof(u_6_phi_11);
	}
	// False  <=>  if(((((({i.vao_attr4.w : 588.50} > {(vs_cbuf10_2.x) : 879.50}) && (! myIsNaN({i.vao_attr4.w : 588.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 879.50}))) || ((({pf_0_1 : 291.00} >= float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr4.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr4.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		return;
	}
	// 0.62995  <=>  {i.vao_attr6.x : 0.62995}
	f_0_6 = i.vao_attr6.x;
	// 292.00  <=>  ({pf_0_1 : 291.00} + {(vs_cbuf10_2.w) : 1.00})
	pf_1_1 = (pf_0_1 + (vs_cbuf10_2.w));
	// 1142104064  <=>  {u_6_phi_11 : 1142104064}
	u_10_0 = u_6_phi_11;
	u_10_phi_15 = u_10_0;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1202096128  <=>  {ftou(({pf_1_1 : 292.00} * {pf_1_1 : 292.00})) : 1202096128}
		u_10_1 = ftou((pf_1_1 * pf_1_1));
		u_10_phi_15 = u_10_1;
	}
	// 1202096128  <=>  {u_10_phi_15 : 1202096128}
	u_11_1 = u_10_phi_15;
	u_11_phi_16 = u_11_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_10_phi_15) : 85264.00} * {utof(vs_cbuf9[14].w) : 0})) : 0}
		u_11_2 = ftou((utof(u_10_phi_15) * utof(vs_cbuf9[14].w)));
		u_11_phi_16 = u_11_2;
	}
	// 0  <=>  {ftou(clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0)) : 0}
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
	// 2147483648  <=>  {u_6_phi_18 : 2147483648}
	u_5_1 = u_6_phi_18;
	// 0  <=>  {u_4_phi_19 : 0}
	u_11_4 = u_4_phi_19;
	// 0  <=>  {u_10_phi_17 : 0}
	u_12_3 = u_10_phi_17;
	u_5_phi_20 = u_5_1;
	u_11_phi_20 = u_11_4;
	u_12_phi_20 = u_12_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 292.00}))
		f_8_10 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// ∞  <=>  ((1.0f / log2({utof(vs_cbuf9[15].x) : 1.00})) * 1.442695f)
		pf_5_5 = ((1.0f / log2(utof(vs_cbuf9[15].x))) * 1.442695f);
		// 4290772992  <=>  {ftou((((((0.f - (({pf_5_5 : ∞} * {f_8_10 : 1.00}) + (0.f - {pf_5_5 : ∞}))) + {pf_1_1 : 292.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].y) : -1})) : 4290772992}
		u_5_2 = ftou((((((0.f - ((pf_5_5 * f_8_10) + (0.f - pf_5_5))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].y)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_5_5 : ∞} * {f_8_10 : 1.00}) + (0.f - {pf_5_5 : ∞}))) + {pf_1_1 : 292.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].z) : 0})) : 4290772992}
		u_11_5 = ftou((((((0.f - ((pf_5_5 * f_8_10) + (0.f - pf_5_5))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].z)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_5_5 : ∞} * {f_8_10 : 1.00}) + (0.f - {pf_5_5 : ∞}))) + {pf_1_1 : 292.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].x) : 0})) : 4290772992}
		u_12_4 = ftou((((((0.f - ((pf_5_5 * f_8_10) + (0.f - pf_5_5))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].x)));
		u_5_phi_20 = u_5_2;
		u_11_phi_20 = u_11_5;
		u_12_phi_20 = u_12_4;
	}
	// 1133641728  <=>  {ftou(pf_1_1) : 1133641728}
	u_4_3 = ftou(pf_1_1);
	u_4_phi_21 = u_4_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// ∞  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))
		f_2_24 = (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f));
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 292.00}))
		f_8_11 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// 4290772992  <=>  {ftou((({f_8_11 : 1.00} * (0.f - {f_2_24 : ∞})) + {f_2_24 : ∞})) : 4290772992}
		u_4_4 = ftou(((f_8_11 * (0.f - f_2_24)) + f_2_24));
		u_4_phi_21 = u_4_4;
	}
	// 0.10  <=>  ({utof(vs_cbuf9[113].x) : 0.10} * {(vs_cbuf10_0.w) : 1.00})
	o.fs_attr0.w = (utof(vs_cbuf9[113].x) * (vs_cbuf10_0.w));
	// 1.471429  <=>  (({utof(vs_cbuf9[105].x) : 0.8174603} * {(vs_cbuf10_0.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.80})
	o.fs_attr0.x = ((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x));
	// -1037.87  <=>  (((({utof(u_4_phi_21) : 292.00} * {i.vao_attr4.x : 0}) + {utof(u_12_phi_20) : 0}) * {i.vao_attr5.w : 1.00}) + {i.vao_attr3.x : -1037.87})
	pf_1_5 = ((((utof(u_4_phi_21) * i.vao_attr4.x) + utof(u_12_phi_20)) * i.vao_attr5.w) + i.vao_attr3.x);
	// 1.464348  <=>  (({utof(vs_cbuf9[105].y) : 0.8135269} * {(vs_cbuf10_0.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.80})
	o.fs_attr0.y = ((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x));
	// -361.23502  <=>  (((({utof(u_4_phi_21) : 292.00} * {i.vao_attr4.y : 0}) + {utof(u_5_phi_20) : -0}) * {i.vao_attr5.w : 1.00}) + {i.vao_attr3.y : -361.23502})
	pf_3_4 = ((((utof(u_4_phi_21) * i.vao_attr4.y) + utof(u_5_phi_20)) * i.vao_attr5.w) + i.vao_attr3.y);
	// 1.34297  <=>  (({utof(vs_cbuf9[105].z) : 0.7460947} * {(vs_cbuf10_0.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.80})
	o.fs_attr0.z = ((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x));
	// -1372.1118  <=>  (((({utof(u_4_phi_21) : 292.00} * {i.vao_attr4.z : 0}) + {utof(u_11_phi_20) : 0}) * {i.vao_attr5.w : 1.00}) + {i.vao_attr3.z : -1372.1118})
	pf_2_5 = ((((utof(u_4_phi_21) * i.vao_attr4.z) + utof(u_11_phi_20)) * i.vao_attr5.w) + i.vao_attr3.z);
	// 0  <=>  0u
	u_4_5 = 0u;
	u_4_phi_22 = u_4_5;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[11].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].w) : 0}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[11].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].w)))) ? true : false))
	{
		// ∞  <=>  (((({f_0_6 : 0.62995} * {utof(vs_cbuf9[13].x) : 0}) * {utof(vs_cbuf9[11].w) : 0}) + {pf_0_1 : 291.00}) * (1.0f / {utof(vs_cbuf9[11].w) : 0}))
		pf_6_5 = ((((f_0_6 * utof(vs_cbuf9[13].x)) * utof(vs_cbuf9[11].w)) + pf_0_1) * (1.0f / utof(vs_cbuf9[11].w)));
		// 4290772992  <=>  {ftou(({pf_6_5 : ∞} + (0.f - floor({pf_6_5 : ∞})))) : 4290772992}
		u_4_6 = ftou((pf_6_5 + (0.f - floor(pf_6_5))));
		u_4_phi_22 = u_4_6;
	}
	// 0  <=>  {u_4_phi_22 : 0}
	u_3_1 = u_4_phi_22;
	u_3_phi_23 = u_3_1;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[11].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].w) : 0})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[11].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].w))))) ? true : false))
	{
		// 1058340340  <=>  {ftou(({pf_0_1 : 291.00} * (1.0f / float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))))) : 1058340340}
		u_3_2 = ftou((pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))));
		u_3_phi_23 = u_3_2;
	}
	// 0.39216  <=>  {i.vao_attr6.y : 0.39216}
	f_8_13 = i.vao_attr6.y;
	// 0.73934  <=>  {i.vao_attr6.z : 0.73934}
	f_5_2 = i.vao_attr6.z;
	// 0  <=>  ({vs_cbuf9_7_x : 1} & 268435456u)
	u_11_13 = (vs_cbuf9_7_x & 268435456u);
	// 0  <=>  ({vs_cbuf9_7_x : 1} & 1073741824u)
	u_12_13 = (vs_cbuf9_7_x & 1073741824u);
	// 1.00  <=>  floor(({f_0_6 : 0.62995} * 2.f))
	f_3_12 = floor((f_0_6 * 2.f));
	// 0  <=>  ({vs_cbuf9_7_x : 1} & 536870912u)
	u_5_14 = (vs_cbuf9_7_x & 536870912u);
	// 0.582  <=>  ({pf_0_1 : 291.00} * (1.0f / float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f))))))))
	pf_16_1 = (pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))));
	// 1.00  <=>  floor(({f_5_2 : 0.73934} * 2.f))
	f_3_15 = floor((f_5_2 * 2.f));
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_5_19 = ftou(v.uv.x);
	u_5_phi_24 = u_5_19;
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({f_0_6 : 0.62995} > 0.5f) && (! myIsNaN({f_0_6 : 0.62995}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((f_0_6 > 0.5f) && (! myIsNaN(f_0_6))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_5_20 = ftou(((0.f - v.uv.x) + 1.f));
		u_5_phi_24 = u_5_20;
	}
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({f_0_6 : 0.62995} > 0.5f) && (! myIsNaN({f_0_6 : 0.62995}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
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
	// 0  <=>  floor(({f_8_13 : 0.39216} * 2.f))
	f_3_19 = floor((f_8_13 * 2.f));
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_2_2 = (myIsNaN(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))));
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_16_2 = ftou(v.uv.x);
	u_16_phi_27 = u_16_2;
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_5_2 : 0.73934} > 0.5f) && (! myIsNaN({f_5_2 : 0.73934}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_5_2 > 0.5f) && (! myIsNaN(f_5_2))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_16_3 = ftou(((0.f - v.uv.x) + 1.f));
		u_16_phi_27 = u_16_3;
	}
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_5_2 : 0.73934} > 0.5f) && (! myIsNaN({f_5_2 : 0.73934}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_5_2 > 0.5f) && (! myIsNaN(f_5_2))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 0  <=>  {ftou((((({f_0_6 : 0.62995} + {f_8_13 : 0.39216}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0})) : 0}
	u_12_23 = ftou(((((f_0_6 + f_8_13) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)));
	u_12_phi_29 = u_12_23;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(({pf_0_1 : 291.00} * {utof(u_14_phi_26) : 0})) : 0}
		u_12_24 = ftou((pf_0_1 * utof(u_14_phi_26)));
		u_12_phi_29 = u_12_24;
	}
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_13_9 = ftou(v.uv.x);
	u_13_phi_30 = u_13_9;
	// False  <=>  if(((! (((~ (((16u & {vs_cbuf9_7_y : 0}) == 16u) ? 4294967295u : 0u)) | (~ (((({f_8_13 : 0.39216} > 0.5f) && (! myIsNaN({f_8_13 : 0.39216}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((16u & vs_cbuf9_7_y) == 16u) ? 4294967295u : 0u)) | (~ ((((f_8_13 > 0.5f) && (! myIsNaN(f_8_13))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_13_10 = ftou(((0.f - v.uv.x) + 1.f));
		u_13_phi_30 = u_13_10;
	}
	// -3409.6157  <=>  ((({pf_2_5 : -1372.1118} * {i.vao_attr10.z : -0.74244}) + (({pf_3_4 : -361.23502} * {i.vao_attr10.y : 0}) + ({pf_1_5 : -1037.87} * {i.vao_attr10.x : 0.66991}))) + {i.vao_attr10.w : -3733.0469})
	pf_8_3 = (((pf_2_5 * i.vao_attr10.z) + ((pf_3_4 * i.vao_attr10.y) + (pf_1_5 * i.vao_attr10.x))) + i.vao_attr10.w);
	// 0  <=>  {utof(vs_cbuf9[129].w) : 0}
	f_3_27 = utof(vs_cbuf9[129].w);
	// 1065353216  <=>  (((({utof(u_3_phi_23) : 0.582} >= {f_3_27 : 0}) && (! myIsNaN({utof(u_3_phi_23) : 0.582}))) && (! myIsNaN({f_3_27 : 0}))) ? 1065353216u : 0u)
	u_9_6 = ((((utof(u_3_phi_23) >= f_3_27) && (! myIsNaN(utof(u_3_phi_23)))) && (! myIsNaN(f_3_27))) ? 1065353216u : 0u);
	// 0.40  <=>  {utof(vs_cbuf9[130].w) : 0.40}
	f_3_30 = utof(vs_cbuf9[130].w);
	// 1065353216  <=>  (((({utof(u_3_phi_23) : 0.582} >= {f_3_30 : 0.40}) && (! myIsNaN({utof(u_3_phi_23) : 0.582}))) && (! myIsNaN({f_3_30 : 0.40}))) ? 1065353216u : 0u)
	u_14_9 = ((((utof(u_3_phi_23) >= f_3_30) && (! myIsNaN(utof(u_3_phi_23)))) && (! myIsNaN(f_3_30))) ? 1065353216u : 0u);
	// 4.500549  <=>  ((({pf_2_5 : -1372.1118} * {i.vao_attr9.z : 0}) + (({pf_3_4 : -361.23502} * {i.vao_attr9.y : 1.00}) + ({pf_1_5 : -1037.87} * {i.vao_attr9.x : 0}))) + {i.vao_attr9.w : 365.7356})
	pf_6_11 = (((pf_2_5 * i.vao_attr9.z) + ((pf_3_4 * i.vao_attr9.y) + (pf_1_5 * i.vao_attr9.x))) + i.vao_attr9.w);
	// 0  <=>  0u
	u_15_6 = 0u;
	u_15_phi_31 = u_15_6;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {u_12_phi_29 : 0}
		u_15_7 = u_12_phi_29;
		u_15_phi_31 = u_15_7;
	}
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[83].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[83].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_1_2 = (myIsNaN(utof(vs_cbuf9[83].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[83].z)), float(-2147483600.f), float(2147483600.f))));
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
	// 0  <=>  {utof(vs_cbuf9[149].w) : 0}
	f_3_36 = utof(vs_cbuf9[149].w);
	// 1065353216  <=>  (((({pf_16_1 : 0.582} >= {f_3_36 : 0}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_3_36 : 0}))) ? 1065353216u : 0u)
	u_0_1 = ((((pf_16_1 >= f_3_36) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_3_36))) ? 1065353216u : 0u);
	// 0.03  <=>  {utof(vs_cbuf9[150].w) : 0.03}
	f_9_49 = utof(vs_cbuf9[150].w);
	// 1065353216  <=>  (((({pf_16_1 : 0.582} >= {f_9_49 : 0.03}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_9_49 : 0.03}))) ? 1065353216u : 0u)
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
	// 1065353216  <=>  {u_19_phi_33 : 1065353216}
	u_17_1 = u_19_phi_33;
	u_17_phi_34 = u_17_1;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1065353216  <=>  {ftou((1.0f / {utof(u_19_phi_33) : 1.00})) : 1065353216}
		u_17_2 = ftou((1.0f / utof(u_19_phi_33)));
		u_17_phi_34 = u_17_2;
	}
	// -229.51624  <=>  ((({pf_2_5 : -1372.1118} * {i.vao_attr8.z : -0.66991}) + (({pf_3_4 : -361.23502} * {i.vao_attr8.y : 0}) + ({pf_1_5 : -1037.87} * {i.vao_attr8.x : -0.74244}))) + {i.vao_attr8.w : -1919.2639})
	pf_1_9 = (((pf_2_5 * i.vao_attr8.z) + ((pf_3_4 * i.vao_attr8.y) + (pf_1_5 * i.vao_attr8.x))) + i.vao_attr8.w);
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[88].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[88].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_0_3 = (myIsNaN(utof(vs_cbuf9[88].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[88].z)), float(-2147483600.f), float(2147483600.f))));
	// 0.94  <=>  {utof(vs_cbuf9[151].w) : 0.94}
	f_6_6 = utof(vs_cbuf9[151].w);
	// 0  <=>  (((({pf_16_1 : 0.582} >= {f_6_6 : 0.94}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_6_6 : 0.94}))) ? 1065353216u : 0u)
	u_20_2 = ((((pf_16_1 >= f_6_6) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_6))) ? 1065353216u : 0u);
	// False  <=>  if(((! (((~ (((16u & {vs_cbuf9_7_y : 0}) == 16u) ? 4294967295u : 0u)) | (~ (((({f_8_13 : 0.39216} > 0.5f) && (! myIsNaN({f_8_13 : 0.39216}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((16u & vs_cbuf9_7_y) == 16u) ? 4294967295u : 0u)) | (~ ((((f_8_13 > 0.5f) && (! myIsNaN(f_8_13))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 1065353216  <=>  {u_12_phi_32 : 1065353216}
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
	// False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 0}) == 2u) ? 4294967295u : 0u)) | (~ (((({f_8_13 : 0.39216} > 0.5f) && (! myIsNaN({f_8_13 : 0.39216}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((f_8_13 > 0.5f) && (! myIsNaN(f_8_13))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_12_28 = ftou(((0.f - v.uv.y) + 1.f));
		u_12_phi_37 = u_12_28;
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_22_1 = ftou(v.uv.y);
	u_22_phi_38 = u_22_1;
	// False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 0}) == 8u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr6.w : 0.80673} > 0.5f) && (! myIsNaN({i.vao_attr6.w : 0.80673}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr6.w > 0.5f) && (! myIsNaN(i.vao_attr6.w))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_22_2 = ftou(((0.f - v.uv.y) + 1.f));
		u_22_phi_38 = u_22_2;
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_23_1 = ftou(v.uv.y);
	u_23_phi_39 = u_23_1;
	// False  <=>  if(((! (((~ (((32u & {vs_cbuf9_7_y : 0}) == 32u) ? 4294967295u : 0u)) | (~ (((({f_5_2 : 0.73934} > 0.5f) && (! myIsNaN({f_5_2 : 0.73934}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
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
	// 1.00  <=>  {utof(vs_cbuf9[131].w) : 1.00}
	f_4_16 = utof(vs_cbuf9[131].w);
	// 0  <=>  (((({utof(u_3_phi_23) : 0.582} >= {f_4_16 : 1.00}) && (! myIsNaN({utof(u_3_phi_23) : 0.582}))) && (! myIsNaN({f_4_16 : 1.00}))) ? 1065353216u : 0u)
	u_3_3 = ((((utof(u_3_phi_23) >= f_4_16) && (! myIsNaN(utof(u_3_phi_23)))) && (! myIsNaN(f_4_16))) ? 1065353216u : 0u);
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_1_2 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_9_10 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_1_2), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_2_2 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_18_4 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_2_2), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_2_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_14_10 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_2_2))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 1.00  <=>  {utof(vs_cbuf9[152].w) : 1.00}
	f_4_22 = utof(vs_cbuf9[152].w);
	// 0  <=>  (((({pf_16_1 : 0.582} >= {f_4_22 : 1.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_4_22 : 1.00}))) ? 1065353216u : 0u)
	u_19_3 = ((((pf_16_1 >= f_4_22) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_4_22))) ? 1065353216u : 0u);
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
	// 1065353216  <=>  {u_15_phi_40 : 1065353216}
	u_26_2 = u_15_phi_40;
	u_26_phi_42 = u_26_2;
	// True  <=>  if((((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1133608960  <=>  {ftou(pf_0_1) : 1133608960}
		u_26_3 = ftou(pf_0_1);
		u_26_phi_42 = u_26_3;
	}
	// 0  <=>  (({u_25_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_2_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_10 : 0}), int(0u), int(16u))))))
	u_15_14 = ((u_25_3 << 16u) + uint((uint(bitfieldExtract(uint(u_2_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_10), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_0_3 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_15_17 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_0_3), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_1_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_25_4 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_1_2))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
	u_27_0 = ftou(f_0_6);
	u_27_phi_43 = u_27_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
		u_27_1 = ftou(f_8_13);
		u_27_phi_43 = u_27_1;
	}
	// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
	u_28_0 = ftou(f_8_13);
	u_28_phi_44 = u_28_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
		u_28_1 = ftou(f_5_2);
		u_28_phi_44 = u_28_1;
	}
	// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
	u_29_0 = ftou(f_0_6);
	u_29_phi_45 = u_29_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
		u_29_1 = ftou(f_8_13);
		u_29_phi_45 = u_29_1;
	}
	// 0  <=>  ((((((({f_0_6 : 0.62995} + {f_8_13 : 0.39216}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0}) * 2.f) + {utof(vs_cbuf9[195].x) : 0}) * (((0.f - {utof((((({f_3_15 : 1.00} < 0.f) && (! myIsNaN({f_3_15 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_3_15 : 1.00} > 0.f) && (! myIsNaN({f_3_15 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 1.00}) * float(int(abs(int((uint((int(0) - int(((int({u_11_13 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_11_13 : 0}) >= int(0u)) ? 0u : 1u)))))))))))
	pf_12_5 = (((((((f_0_6 + f_8_13) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)) * 2.f) + utof(vs_cbuf9[195].x)) * (((0.f - utof(((((f_3_15 < 0.f) && (! myIsNaN(f_3_15))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_15 > 0.f) && (! myIsNaN(f_3_15))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_11_13) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_11_13) >= int(0u)) ? 0u : 1u)))))))))));
	// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
	u_30_0 = ftou(f_8_13);
	u_30_phi_46 = u_30_0;
	// True  <=>  if((({u_21_phi_41 : 1} == 1u) ? true : false))
	if(((u_21_phi_41 == 1u) ? true : false))
	{
		// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
		u_30_1 = ftou(f_5_2);
		u_30_phi_46 = u_30_1;
	}
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_1_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_25_4 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_25_4 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_32_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_1_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_25_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_25_4), int(0u), int(16u))), int(16u), int(16u));
	// 1060980067  <=>  {u_30_phi_46 : 1060980067}
	u_33_2 = u_30_phi_46;
	// 1060980067  <=>  {u_28_phi_44 : 1060980067}
	u_34_0 = u_28_phi_44;
	// 1053346098  <=>  {u_29_phi_45 : 1053346098}
	u_35_0 = u_29_phi_45;
	// 1053346098  <=>  {u_27_phi_43 : 1053346098}
	u_36_0 = u_27_phi_43;
	u_33_phi_47 = u_33_2;
	u_34_phi_47 = u_34_0;
	u_35_phi_47 = u_35_0;
	u_36_phi_47 = u_36_0;
	// False  <=>  if(((! ({u_21_phi_41 : 1} == 1u)) ? true : false))
	if(((! (u_21_phi_41 == 1u)) ? true : false))
	{
		// 1053346098  <=>  {u_27_phi_43 : 1053346098}
		u_21_4 = u_27_phi_43;
		u_21_phi_48 = u_21_4;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
			u_21_5 = ftou(f_5_2);
			u_21_phi_48 = u_21_5;
		}
		// 1060980067  <=>  {u_28_phi_44 : 1060980067}
		u_37_0 = u_28_phi_44;
		u_37_phi_49 = u_37_0;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
			u_37_1 = ftou(f_0_6);
			u_37_phi_49 = u_37_1;
		}
		// 1053346098  <=>  {u_29_phi_45 : 1053346098}
		u_38_0 = u_29_phi_45;
		u_38_phi_50 = u_38_0;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
			u_38_1 = ftou(f_0_6);
			u_38_phi_50 = u_38_1;
		}
		// 1060980067  <=>  {u_30_phi_46 : 1060980067}
		u_39_0 = u_30_phi_46;
		u_39_phi_51 = u_39_0;
		// False  <=>  if((({u_21_phi_41 : 1} == 2u) ? true : false))
		if(((u_21_phi_41 == 2u) ? true : false))
		{
			// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
			u_39_1 = ftou(f_8_13);
			u_39_phi_51 = u_39_1;
		}
		// 1060980067  <=>  {u_39_phi_51 : 1060980067}
		u_33_3 = u_39_phi_51;
		// 1060980067  <=>  {u_37_phi_49 : 1060980067}
		u_34_1 = u_37_phi_49;
		// 1053346098  <=>  {u_38_phi_50 : 1053346098}
		u_35_1 = u_38_phi_50;
		// 1053346098  <=>  {u_21_phi_48 : 1053346098}
		u_36_1 = u_21_phi_48;
		u_33_phi_47 = u_33_3;
		u_34_phi_47 = u_34_1;
		u_35_phi_47 = u_35_1;
		u_36_phi_47 = u_36_1;
	}
	// 0  <=>  ((((((({f_8_13 : 0.39216} + {f_5_2 : 0.73934}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].y) : 0}) * 2.f) + {utof(vs_cbuf9[195].y) : 0}) * (((0.f - {utof((((({f_3_12 : 1.00} < 0.f) && (! myIsNaN({f_3_12 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_3_12 : 1.00} > 0.f) && (! myIsNaN({f_3_12 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 1.00}) * float(int(abs(int((uint((int(0) - int(((int({u_5_14 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_5_14 : 0}) >= int(0u)) ? 0u : 1u)))))))))))
	pf_14_5 = (((((((f_8_13 + f_5_2) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].y)) * 2.f) + utof(vs_cbuf9[195].y)) * (((0.f - utof(((((f_3_12 < 0.f) && (! myIsNaN(f_3_12))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_12 > 0.f) && (! myIsNaN(f_3_12))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_5_14) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_5_14) >= int(0u)) ? 0u : 1u)))))))))));
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
	// 0  <=>  (((({pf_16_1 : 0.582} >= {f_2_70 : 5.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_2_70 : 5.00}))) ? 1065353216u : 0u)
	u_32_6 = ((((pf_16_1 >= f_2_70) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_2_70))) ? 1065353216u : 0u);
	// 0  <=>  ((((((({f_0_6 : 0.62995} + {f_5_2 : 0.73934}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].z) : 0}) * 2.f) + {utof(vs_cbuf9[195].z) : 0}) * (((0.f - {utof((((({f_3_19 : 0} < 0.f) && (! myIsNaN({f_3_19 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_3_19 : 0} > 0.f) && (! myIsNaN({f_3_19 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) * float(int(abs(int((uint((int(0) - int(((int({u_12_13 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_12_13 : 0}) >= int(0u)) ? 0u : 1u)))))))))))
	pf_19_8 = (((((((f_0_6 + f_5_2) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].z)) * 2.f) + utof(vs_cbuf9[195].z)) * (((0.f - utof(((((f_3_19 < 0.f) && (! myIsNaN(f_3_19))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_3_19 > 0.f) && (! myIsNaN(f_3_19))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_12_13) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_12_13) >= int(0u)) ? 0u : 1u)))))))))));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_18_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_72 = utof((ftou((1.0f / float(u_18_4))) + 4294967294u));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_6 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_21_6 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_38_2 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_6), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_21_6), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_6 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_21_6 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_6 : 0}), int(0u), int(16u))))))
	u_29_4 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_6), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_21_6), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_0_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_6), int(0u), int(16u))))));
	// 0  <=>  uint(clamp(trunc(({f_2_72 : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
	u_31_6 = uint(clamp(trunc((f_2_72 * float(0u))), float(0.f), float(4294967300.f)));
	// 0  <=>  (((({pf_12_5 : 0} * -2.f) + (((((({f_0_6 : 0.62995} + {f_8_13 : 0.39216}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0}) * 2.f) + {utof(vs_cbuf9[195].x) : 0})) * {utof(u_26_phi_42) : 291.00}) + (({f_0_6 : 0.62995} + -0.5f) * {utof(vs_cbuf9[194].x) : 0}))
	pf_12_7 = ((((pf_12_5 * -2.f) + ((((((f_0_6 + f_8_13) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)) * 2.f) + utof(vs_cbuf9[195].x))) * utof(u_26_phi_42)) + ((f_0_6 + -0.5f) * utof(vs_cbuf9[194].x)));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_15_17 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_2_79 = utof((ftou((1.0f / float(u_15_17))) + 4294967294u));
	// 0  <=>  uint(clamp(trunc((float(0u) * {f_2_79 : 0.9999999})), float(0.f), float(4294967300.f)))
	u_38_4 = uint(clamp(trunc((float(0u) * f_2_79)), float(0.f), float(4294967300.f)));
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_30_3 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_30_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_9_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_9_10 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_39_3 = uint((uint(bitfieldExtract(uint(u_30_3), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_30_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_9_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_9_10), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 0  <=>  (((({pf_19_8 : 0} * -2.f) + (((((({f_0_6 : 0.62995} + {f_5_2 : 0.73934}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].z) : 0}) * 2.f) + {utof(vs_cbuf9[195].z) : 0})) * {utof(u_26_phi_42) : 291.00}) + (({f_5_2 : 0.73934} + -0.5f) * {utof(vs_cbuf9[194].z) : 0}))
	pf_13_9 = ((((pf_19_8 * -2.f) + ((((((f_0_6 + f_5_2) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].z)) * 2.f) + utof(vs_cbuf9[195].z))) * utof(u_26_phi_42)) + ((f_5_2 + -0.5f) * utof(vs_cbuf9[194].z)));
	// 6.00  <=>  {utof(vs_cbuf9[154].w) : 6.00}
	f_2_85 = utof(vs_cbuf9[154].w);
	// 0  <=>  (((({pf_16_1 : 0.582} >= {f_2_85 : 6.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_2_85 : 6.00}))) ? 1065353216u : 0u)
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
	// 0  <=>  (0.f - {utof((((({pf_16_1 : 0.582} >= {f_6_20 : 7.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_6_20 : 7.00}))) ? 1065353216u : 0u)) : 0})
	f_7_13 = (0.f - utof(((((pf_16_1 >= f_6_20) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_20))) ? 1065353216u : 0u)));
	// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
	u_26_5 = ftou(f_0_6);
	u_26_phi_53 = u_26_5;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
		u_26_6 = ftou(f_8_13);
		u_26_phi_53 = u_26_6;
	}
	// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
	u_29_9 = ftou(f_8_13);
	u_29_phi_54 = u_29_9;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
		u_29_10 = ftou(f_5_2);
		u_29_phi_54 = u_29_10;
	}
	// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
	u_32_7 = ftou(f_5_2);
	u_32_phi_55 = u_32_7;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
		u_32_8 = ftou(f_0_6);
		u_32_phi_55 = u_32_8;
	}
	// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
	u_33_6 = ftou(f_0_6);
	u_33_phi_56 = u_33_6;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
		u_33_7 = ftou(f_8_13);
		u_33_phi_56 = u_33_7;
	}
	// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
	u_34_2 = ftou(f_8_13);
	u_34_phi_57 = u_34_2;
	// False  <=>  if((({u_40_phi_52 : 2} == 1u) ? true : false))
	if(((u_40_phi_52 == 1u) ? true : false))
	{
		// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
		u_34_3 = ftou(f_5_2);
		u_34_phi_57 = u_34_3;
	}
	// 1059144807  <=>  {u_26_phi_53 : 1059144807}
	u_35_2 = u_26_phi_53;
	// 1053346098  <=>  {u_29_phi_54 : 1053346098}
	u_37_16 = u_29_phi_54;
	// 1059144807  <=>  {u_33_phi_56 : 1059144807}
	u_39_12 = u_33_phi_56;
	// 1053346098  <=>  {u_34_phi_57 : 1053346098}
	u_41_1 = u_34_phi_57;
	// 1060980067  <=>  {u_32_phi_55 : 1060980067}
	u_42_0 = u_32_phi_55;
	u_35_phi_58 = u_35_2;
	u_37_phi_58 = u_37_16;
	u_39_phi_58 = u_39_12;
	u_41_phi_58 = u_41_1;
	u_42_phi_58 = u_42_0;
	// True  <=>  if(((! ({u_40_phi_52 : 2} == 1u)) ? true : false))
	if(((! (u_40_phi_52 == 1u)) ? true : false))
	{
		// 1059144807  <=>  {u_26_phi_53 : 1059144807}
		u_40_5 = u_26_phi_53;
		u_40_phi_59 = u_40_5;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1060980067  <=>  {ftou(f_5_2) : 1060980067}
			u_40_6 = ftou(f_5_2);
			u_40_phi_59 = u_40_6;
		}
		// 1053346098  <=>  {u_29_phi_54 : 1053346098}
		u_6_11 = u_29_phi_54;
		u_6_phi_60 = u_6_11;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
			u_6_12 = ftou(f_0_6);
			u_6_phi_60 = u_6_12;
		}
		// 1060980067  <=>  {u_32_phi_55 : 1060980067}
		u_43_0 = u_32_phi_55;
		u_43_phi_61 = u_43_0;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
			u_43_1 = ftou(f_8_13);
			u_43_phi_61 = u_43_1;
		}
		// 1059144807  <=>  {u_33_phi_56 : 1059144807}
		u_44_0 = u_33_phi_56;
		u_44_phi_62 = u_44_0;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1059144807  <=>  {ftou(f_0_6) : 1059144807}
			u_44_1 = ftou(f_0_6);
			u_44_phi_62 = u_44_1;
		}
		// 1053346098  <=>  {u_34_phi_57 : 1053346098}
		u_8_5 = u_34_phi_57;
		u_8_phi_63 = u_8_5;
		// True  <=>  if((({u_40_phi_52 : 2} == 2u) ? true : false))
		if(((u_40_phi_52 == 2u) ? true : false))
		{
			// 1053346098  <=>  {ftou(f_8_13) : 1053346098}
			u_8_6 = ftou(f_8_13);
			u_8_phi_63 = u_8_6;
		}
		// 1060980067  <=>  {u_40_phi_59 : 1060980067}
		u_35_3 = u_40_phi_59;
		// 1059144807  <=>  {u_6_phi_60 : 1059144807}
		u_37_17 = u_6_phi_60;
		// 1059144807  <=>  {u_44_phi_62 : 1059144807}
		u_39_13 = u_44_phi_62;
		// 1053346098  <=>  {u_8_phi_63 : 1053346098}
		u_41_2 = u_8_phi_63;
		// 1053346098  <=>  {u_43_phi_61 : 1053346098}
		u_42_1 = u_43_phi_61;
		u_35_phi_58 = u_35_3;
		u_37_phi_58 = u_37_17;
		u_39_phi_58 = u_39_13;
		u_41_phi_58 = u_41_2;
		u_42_phi_58 = u_42_1;
	}
	// 1.00  <=>  (((((((0.f - {utof(vs_cbuf9[154].x) : 1.00}) + {utof(vs_cbuf9[155].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[154].w) : 6.00}) + {utof(vs_cbuf9[155].w) : 7.00}))) * ({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[154].w) : 6.00}))) + {utof(vs_cbuf9[154].x) : 1.00}) * (({f_9_64 : 0} * {f_7_13 : 0}) + {utof(u_26_4) : 0})) + (((((((0.f - {utof(vs_cbuf9[153].x) : 1.00}) + {utof(vs_cbuf9[154].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[153].w) : 5.00}) + {utof(vs_cbuf9[154].w) : 6.00}))) * ({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[153].w) : 5.00}))) + {utof(vs_cbuf9[153].x) : 1.00}) * (({utof(u_32_6) : 0} * (0.f - {utof(u_26_4) : 0})) + {utof(u_32_6) : 0})) + (((((((0.f - {utof(vs_cbuf9[152].x) : 1.00}) + {utof(vs_cbuf9[153].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[152].w) : 1.00}) + {utof(vs_cbuf9[153].w) : 5.00}))) * ({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[152].w) : 1.00}))) + {utof(vs_cbuf9[152].x) : 1.00}) * (({utof(u_19_3) : 0} * (0.f - {utof(u_32_6) : 0})) + {utof(u_19_3) : 0})) + (((((((0.f - {utof(vs_cbuf9[151].x) : 1.00}) + {utof(vs_cbuf9[152].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[151].w) : 0.94}) + {utof(vs_cbuf9[152].w) : 1.00}))) * ({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[151].w) : 0.94}))) + {utof(vs_cbuf9[151].x) : 1.00}) * (({utof(u_20_2) : 0} * (0.f - {utof(u_19_3) : 0})) + {utof(u_20_2) : 0})) + (((((((0.f - {utof(vs_cbuf9[150].x) : 1.00}) + {utof(vs_cbuf9[151].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[150].w) : 0.03}) + {utof(vs_cbuf9[151].w) : 0.94}))) * ({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[150].w) : 0.03}))) + {utof(vs_cbuf9[150].x) : 1.00}) * (({utof(u_15_9) : 1.00} * (0.f - {utof(u_20_2) : 0})) + {utof(u_15_9) : 1.00})) + ((((({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[149].w) : 0})) * (((0.f - {utof(vs_cbuf9[149].x) : 1.00}) + {utof(vs_cbuf9[150].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[149].w) : 0}) + {utof(vs_cbuf9[150].w) : 0.03})))) + {utof(vs_cbuf9[149].x) : 1.00}) * (({utof(u_0_1) : 1.00} * (0.f - {utof(u_15_9) : 1.00})) + {utof(u_0_1) : 1.00})) + (({utof(u_0_1) : 1.00} * (0.f - {utof(vs_cbuf9[149].x) : 1.00})) + {utof(vs_cbuf9[149].x) : 1.00})))))))
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
	// 0  <=>  {utof((((({pf_16_1 : 0.582} >= {f_6_20 : 7.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_6_20 : 7.00}))) ? 1065353216u : 0u)) : 0}
	f_7_16 = utof(((((pf_16_1 >= f_6_20) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_6_20))) ? 1065353216u : 0u));
	// 0  <=>  {utof((((({pf_16_1 : 0.582} >= {f_3_56 : 8.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_3_56 : 8.00}))) ? 1065353216u : 0u)) : 0}
	f_9_65 = utof(((((pf_16_1 >= f_3_56) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_3_56))) ? 1065353216u : 0u));
	// 0  <=>  {utof((((({pf_16_1 : 0.582} >= {f_6_20 : 7.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_6_20 : 7.00}))) ? 1065353216u : 0u)) : 0}
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
	// 0  <=>  {utof((((({pf_16_1 : 0.582} >= {f_3_56 : 8.00}) && (! myIsNaN({pf_16_1 : 0.582}))) && (! myIsNaN({f_3_56 : 8.00}))) ? 1065353216u : 0u)) : 0}
	f_3_61 = utof(((((pf_16_1 >= f_3_56) && (! myIsNaN(pf_16_1))) && (! myIsNaN(f_3_56))) ? 1065353216u : 0u)); // maybe duplicate expression on the right side of the assignment, vars:(f_9_65, f_9_65)
	// 1.00  <=>  (({f_3_61 : 0} * {utof(vs_cbuf9[156].x) : 1.00}) + ((((({pf_16_1 : 0.582} + (0.f - {utof(vs_cbuf9[155].w) : 7.00})) * (({utof(vs_cbuf9[156].x) : 1.00} + (0.f - {utof(vs_cbuf9[155].x) : 1.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[155].w) : 7.00}) + {utof(vs_cbuf9[156].w) : 8.00})))) + {utof(vs_cbuf9[155].x) : 1.00}) * (({f_10_8 : 0} * (0.f - {f_9_65 : 0})) + {f_7_16 : 0})) + {pf_2_16 : 1.00}))
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
	// -8.747199  <=>  (0.f - (({pf_0_1 : 291.00} * {utof(vs_cbuf9[87].x) : 0}) + (({utof(u_42_phi_58) : 0.39216} * {utof(vs_cbuf9[87].z) : 6.283185}) + ({utof(vs_cbuf9[87].z) : 6.283185} + {utof(vs_cbuf9[87].y) : 0}))))
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
	// 0.6187075  <=>  (({utof(vs_cbuf9[121].x) : 0.3437264} * {(vs_cbuf10_1.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.80})
	o.fs_attr1.x = ((utof(vs_cbuf9[121].x) * (vs_cbuf10_1.x)) * utof(vs_cbuf9[104].x));
	// 0.7285714  <=>  (({utof(vs_cbuf9[121].z) : 0.4047619} * {(vs_cbuf10_1.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.80})
	o.fs_attr1.z = ((utof(vs_cbuf9[121].z) * (vs_cbuf10_1.z)) * utof(vs_cbuf9[104].x));
	// 0.7000239  <=>  (({utof(vs_cbuf9[121].y) : 0.3889022} * {(vs_cbuf10_1.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.80})
	o.fs_attr1.y = ((utof(vs_cbuf9[121].y) * (vs_cbuf10_1.y)) * utof(vs_cbuf9[104].x));
	// 0.2786667  <=>  ((({utof(u_3_3) : 0} * {utof(vs_cbuf9[131].x) : 0}) + (((((((0.f - {utof(vs_cbuf9[130].x) : 0.40}) + {utof(vs_cbuf9[131].x) : 0}) * (1.0f / ((0.f - {utof(vs_cbuf9[130].w) : 0.40}) + {utof(vs_cbuf9[131].w) : 1.00}))) * ({utof(u_3_phi_23) : 0.582} + (0.f - {utof(vs_cbuf9[130].w) : 0.40}))) + {utof(vs_cbuf9[130].x) : 0.40}) * (({utof(u_14_9) : 1.00} * (0.f - {utof(u_3_3) : 0})) + {utof(u_14_9) : 1.00})) + ((((({utof(u_3_phi_23) : 0.582} + (0.f - {utof(vs_cbuf9[129].w) : 0})) * (({utof(vs_cbuf9[130].x) : 0.40} + (0.f - {utof(vs_cbuf9[129].x) : 0})) * (1.0f / ((0.f - {utof(vs_cbuf9[129].w) : 0}) + {utof(vs_cbuf9[130].w) : 0.40})))) + {utof(vs_cbuf9[129].x) : 0}) * (({utof(u_14_9) : 1.00} * (0.f - {utof(u_9_6) : 1.00})) + {utof(u_9_6) : 1.00})) + (({utof(u_9_6) : 1.00} * (0.f - {utof(vs_cbuf9[129].x) : 0})) + {utof(vs_cbuf9[129].x) : 0})))) * {(vs_cbuf10_1.w) : 1.00})
	o.fs_attr1.w = (((utof(u_3_3) * utof(vs_cbuf9[131].x)) + (((((((0.f - utof(vs_cbuf9[130].x)) + utof(vs_cbuf9[131].x)) * (1.0f / ((0.f - utof(vs_cbuf9[130].w)) + utof(vs_cbuf9[131].w)))) * (utof(u_3_phi_23) + (0.f - utof(vs_cbuf9[130].w)))) + utof(vs_cbuf9[130].x)) * ((utof(u_14_9) * (0.f - utof(u_3_3))) + utof(u_14_9))) + (((((utof(u_3_phi_23) + (0.f - utof(vs_cbuf9[129].w))) * ((utof(vs_cbuf9[130].x) + (0.f - utof(vs_cbuf9[129].x))) * (1.0f / ((0.f - utof(vs_cbuf9[129].w)) + utof(vs_cbuf9[130].w))))) + utof(vs_cbuf9[129].x)) * ((utof(u_14_9) * (0.f - utof(u_9_6))) + utof(u_9_6))) + ((utof(u_9_6) * (0.f - utof(vs_cbuf9[129].x))) + utof(vs_cbuf9[129].x))))) * (vs_cbuf10_1.w));
	// 1.00  <=>  1.f
	o.fs_attr7.x = 1.f;
	// 480.6114  <=>  (({i.vao_attr5.y : 480.6114} * {utof(vs_cbuf9[141].y) : 1.00}) * {(vs_cbuf10_3.z) : 1.00})
	pf_7_1 = ((i.vao_attr5.y * utof(vs_cbuf9[141].y)) * (vs_cbuf10_3.z));
	// 1.00  <=>  1.f
	o.fs_attr7.y = 1.f;
	// 1.00  <=>  1.f
	o.fs_attr7.z = 1.f;
	// 3195995895  <=>  {ftou((((({pf_0_1 : 291.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_35_phi_47) : 0.39216} * {utof(vs_cbuf9[81].z) : 0}) + ({utof(vs_cbuf9[81].z) : 0} + {utof(vs_cbuf9[81].x) : 0.70}))) * (({pf_27_2 : 1.00} * {utof(u_16_phi_27) : 0.4939}) + -0.5f)) + (({pf_27_2 : 1.00} * float(int({u_1_7 : 0}))) + (({pf_0_1 : 291.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.0001})) + (0.f - ((({utof(u_36_phi_47) : 0.39216} * {utof(vs_cbuf9[80].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 1.00} + {utof(vs_cbuf9[79].z) : 0}))))))) : 3195995895}
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
		// 3226013659  <=>  {u_6_phi_65 : 3226013659}
		u_2_11 = u_6_phi_65;
		u_2_phi_64 = u_2_11;
	}
	// 3195995895  <=>  {u_2_phi_64 : 3195995895}
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
			// 3222719460  <=>  {u_8_phi_68 : 3222719460}
			u_5_24 = u_8_phi_68;
			u_5_phi_67 = u_5_24;
		}
		// 2139095040  <=>  {u_5_phi_67 : 2139095040}
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
			// 1060888727  <=>  {u_9_phi_72 : 1060888727}
			u_8_26 = u_9_phi_72;
			u_8_phi_73 = u_8_26;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.z) : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.z) : -0.6398518}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.z)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.z)))) ? true : false))
			{
				// 1075451829  <=>  {ftou(((0.f - {utof(u_9_phi_72) : 0.7338957}) + 3.1415927f)) : 1075451829}
				u_8_27 = ftou(((0.f - utof(u_9_phi_72)) + 3.1415927f));
				u_8_phi_73 = u_8_27;
			}
			// 1075451829  <=>  {u_8_phi_73 : 1075451829}
			u_9_21 = u_8_phi_73;
			u_9_phi_74 = u_9_21;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
			{
				// 3222935477  <=>  {ftou((0.f - {utof(u_8_phi_73) : 2.407697})) : 3222935477}
				u_9_22 = ftou((0.f - utof(u_8_phi_73)));
				u_9_phi_74 = u_9_22;
			}
			// 3222935477  <=>  {u_9_phi_74 : 3222935477}
			u_6_23 = u_9_phi_74;
			u_6_phi_69 = u_6_23;
		}
		// 3222935477  <=>  {u_6_phi_69 : 3222935477}
		u_1_19 = u_6_phi_69;
		u_1_phi_66 = u_1_19;
	}
	// -2.407697  <=>  ((((({pf_14_5 : 0} * -2.f) + (((((({f_8_13 : 0.39216} + {f_5_2 : 0.73934}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].y) : 0}) * 2.f) + {utof(vs_cbuf9[195].y) : 0})) * {utof(u_26_phi_42) : 291.00}) + (({f_8_13 : 0.39216} + -0.5f) * {utof(vs_cbuf9[194].y) : 0})) + {utof(u_1_phi_66) : -2.407697})
	pf_11_8 = (((((pf_14_5 * -2.f) + ((((((f_8_13 + f_5_2) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].y)) * 2.f) + utof(vs_cbuf9[195].y))) * utof(u_26_phi_42)) + ((f_8_13 + -0.5f) * utof(vs_cbuf9[194].y))) + utof(u_1_phi_66));
	// 1.00  <=>  cos({pf_13_9 : 0})
	f_4_56 = cos(pf_13_9);
	// 0  <=>  sin({pf_12_7 : 0})
	f_7_26 = sin(pf_12_7);
	// 64.35386  <=>  ((({i.vao_attr5.z : 600.7642} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712}))
	pf_5_15 = (((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z));
	// -9.45002  <=>  ((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573}))
	pf_20_14 = ((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x));
	// -0.66976756  <=>  (sin({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))
	pf_22_10 = (sin(pf_11_8) * cos(pf_12_7));
	// -166.85384  <=>  ({pf_7_1 : 480.6114} * ((0.5f * {utof(vs_cbuf9[16].y) : 0}) + {v.vertex.y : -0.34717}))
	pf_11_11 = (pf_7_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y));
	// 1133608960  <=>  {ftou(pf_0_1) : 1133608960}
	u_1_20 = ftou(pf_0_1);
	u_1_phi_75 = u_1_20;
	// False  <=>  if((((({pf_0_1 : 291.00} == 0.f) && (! myIsNaN({pf_0_1 : 291.00}))) && (! myIsNaN(0.f))) ? true : false))
	if(((((pf_0_1 == 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) ? true : false))
	{
		// 1065353216  <=>  1065353216u
		u_1_21 = 1065353216u;
		u_1_phi_75 = u_1_21;
	}
	// 1689.746  <=>  ({pf_1_9 : -229.51624} + (0.f - {(camera_wpos.x) : -1919.2622}))
	pf_11_15 = (pf_1_9 + (0.f - (camera_wpos.x)));
	// -361.23676  <=>  ({pf_6_11 : 4.500549} + (0.f - {(camera_wpos.y) : 365.7373}))
	pf_28_3 = (pf_6_11 + (0.f - (camera_wpos.y)));
	// 323.4312  <=>  ({pf_8_3 : -3409.6157} + (0.f - {(camera_wpos.z) : -3733.0469}))
	pf_28_4 = (pf_8_3 + (0.f - (camera_wpos.z)));
	// 1757.937  <=>  sqrt((({pf_28_4 : 323.4312} * {pf_28_4 : 323.4312}) + (({pf_28_3 : -361.23676} * {pf_28_3 : -361.23676}) + ({pf_11_15 : 1689.746} * {pf_11_15 : 1689.746}))))
	f_5_14 = sqrt(((pf_28_4 * pf_28_4) + ((pf_28_3 * pf_28_3) + (pf_11_15 * pf_11_15))));
	// -0.34716997  <=>  (((({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0}))))) + (((0.f - abs({(vs_cbuf8_28.y) : 0.5074672})) * {(vs_cbuf13_6.w) : 0}) + {(vs_cbuf13_6.w) : 0})) * (1.0f / {pf_7_1 : 480.6114}))
	pf_7_3 = ((((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7))))) + (((0.f - abs((vs_cbuf8_28.y))) * (vs_cbuf13_6.w)) + (vs_cbuf13_6.w))) * (1.0f / pf_7_1));
	// 2000.00  <=>  {utof(vs_cbuf9[197].x) : 2000.00}
	f_1_17 = utof(vs_cbuf9[197].x);
	// False  <=>  ((({f_5_14 : 1757.937} > {f_1_17 : 2000.00}) && (! myIsNaN({f_5_14 : 1757.937}))) && (! myIsNaN({f_1_17 : 2000.00})))
	b_1_67 = (((f_5_14 > f_1_17) && (! myIsNaN(f_5_14))) && (! myIsNaN(f_1_17)));
	// -2240.8733  <=>  (((({(vs_cbuf10_4.w) : -1919.2622} + {(vs_cbuf10_5.w) : 365.7375}) + {(vs_cbuf10_6.w) : -3733.0469}) * 0.105f) + (((((({f_5_2 : 0.73934} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_8_3 : -3409.6157} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0})))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -166.85384} * ({f_7_26 : 0} * {f_4_56 : 1.00})))))) + (((({f_0_6 : 0.62995} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_1_9 : -229.51624} + (({pf_5_15 : 64.35386} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -9.45002} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -166.85384} * (0.f - sin({pf_13_9 : 0}))))))) + ((({f_8_13 : 0.39216} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_6_11 : 4.500549} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0}))))))))) * {(vs_cbuf13_0.w) : 0.50}) + ({(vs_cbuf13_1.x) : 0.025} * {(vs_cbuf10_2.x) : 879.50})))
	pf_7_4 = (((((vs_cbuf10_4.w) + (vs_cbuf10_5.w)) + (vs_cbuf10_6.w)) * 0.105f) + ((((((f_5_2 + 1.f) * (vs_cbuf13_2.x)) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (f_7_26 * sin(pf_11_8))) + (cos(pf_11_8) * cos(pf_12_7)))) + ((pf_20_14 * ((sin(pf_13_9) * (f_7_26 * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (f_7_26 * f_4_56)))))) + ((((f_0_6 + 1.f) * (vs_cbuf13_2.x)) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))) + (((f_8_13 + 1.f) * (vs_cbuf13_2.x)) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7))))))))) * (vs_cbuf13_0.w)) + ((vs_cbuf13_1.x) * (vs_cbuf10_2.x))));
	// -2240.8733  <=>  (((({(vs_cbuf10_4.w) : -1919.2622} + {(vs_cbuf10_5.w) : 365.7375}) + {(vs_cbuf10_6.w) : -3733.0469}) * 0.105f) + (((((({f_5_2 : 0.73934} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_8_3 : -3409.6157} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0})))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -166.85384} * ({f_7_26 : 0} * {f_4_56 : 1.00})))))) + (((({f_0_6 : 0.62995} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_1_9 : -229.51624} + (({pf_5_15 : 64.35386} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -9.45002} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -166.85384} * (0.f - sin({pf_13_9 : 0}))))))) + ((({f_8_13 : 0.39216} + 1.f) * {(vs_cbuf13_2.x) : 100.00}) + ({pf_6_11 : 4.500549} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0}))))))))) * ({(vs_cbuf13_0.w) : 0.50} * {(vs_cbuf13_1.z) : 1.00})) + ({(vs_cbuf13_1.x) : 0.025} * {(vs_cbuf10_2.x) : 879.50})))
	pf_11_21 = (((((vs_cbuf10_4.w) + (vs_cbuf10_5.w)) + (vs_cbuf10_6.w)) * 0.105f) + ((((((f_5_2 + 1.f) * (vs_cbuf13_2.x)) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (f_7_26 * sin(pf_11_8))) + (cos(pf_11_8) * cos(pf_12_7)))) + ((pf_20_14 * ((sin(pf_13_9) * (f_7_26 * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (f_7_26 * f_4_56)))))) + ((((f_0_6 + 1.f) * (vs_cbuf13_2.x)) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))) + (((f_8_13 + 1.f) * (vs_cbuf13_2.x)) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7))))))))) * ((vs_cbuf13_0.w) * (vs_cbuf13_1.z))) + ((vs_cbuf13_1.x) * (vs_cbuf10_2.x))));
	// 0.7942238  <=>  sin({pf_7_4 : -2240.8733})
	f_1_20 = sin(pf_7_4);
	// 1065353216  <=>  {ftou(clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 291.00}), 0.0, 1.0)) : 1065353216}
	u_6_24 = ftou(clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0));
	// 1065353216  <=>  {ftou(clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 291.00}), 0.0, 1.0)) : 1065353216}
	u_7_3 = ftou(clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(u_6_24, u_6_24)
	// 1065353216  <=>  {ftou(clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 291.00}), 0.0, 1.0)) : 1065353216}
	u_8_28 = ftou(clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(u_6_24, u_6_24)|(u_7_3, u_7_3)
	u_6_phi_76 = u_6_24;
	u_7_phi_76 = u_7_3;
	u_8_phi_76 = u_8_28;
	// False  <=>  if(((((0.f != {(vs_cbuf13_5.y) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf13_5.y) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf13_5.y)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf13_5.y))) ? true : false))
	{
		// -1653.6611  <=>  ((0.f - ({pf_1_9 : -229.51624} + (({pf_5_15 : 64.35386} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -9.45002} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -166.85384} * (0.f - sin({pf_13_9 : 0}))))))) + {(vs_cbuf10_4.w) : -1919.2622})
		pf_16_15 = ((0.f - (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))) + (vs_cbuf10_4.w));
		// 528.0908  <=>  ((0.f - ({pf_6_11 : 4.500549} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0}))))))) + {(vs_cbuf10_5.w) : 365.7375})
		pf_17_15 = ((0.f - (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7))))))) + (vs_cbuf10_5.w));
		// -269.31445  <=>  ((0.f - ({pf_8_3 : -3409.6157} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0})))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -166.85384} * ({f_7_26 : 0} * {f_4_56 : 1.00})))))) + {(vs_cbuf10_6.w) : -3733.0469})
		pf_21_19 = ((0.f - (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (f_7_26 * sin(pf_11_8))) + (cos(pf_11_8) * cos(pf_12_7)))) + ((pf_20_14 * ((sin(pf_13_9) * (f_7_26 * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (f_7_26 * f_4_56)))))) + (vs_cbuf10_6.w));
		// 1.00  <=>  (clamp(((1.0f / {(vs_cbuf13_5.y) : 0}) * sqrt((({pf_21_19 : -269.31445} * {pf_21_19 : -269.31445}) + (({pf_17_15 : 528.0908} * {pf_17_15 : 528.0908}) + ({pf_16_15 : -1653.6611} * {pf_16_15 : -1653.6611}))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 291.00}), 0.0, 1.0))
		pf_16_20 = (clamp(((1.0f / (vs_cbuf13_5.y)) * sqrt(((pf_21_19 * pf_21_19) + ((pf_17_15 * pf_17_15) + (pf_16_15 * pf_16_15))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0));
		// 1.00  <=>  (clamp(((1.0f / {(vs_cbuf13_5.y) : 0}) * sqrt((({pf_21_19 : -269.31445} * {pf_21_19 : -269.31445}) + (({pf_17_15 : 528.0908} * {pf_17_15 : 528.0908}) + ({pf_16_15 : -1653.6611} * {pf_16_15 : -1653.6611}))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 291.00}), 0.0, 1.0))
		pf_16_21 = (clamp(((1.0f / (vs_cbuf13_5.y)) * sqrt(((pf_21_19 * pf_21_19) + ((pf_17_15 * pf_17_15) + (pf_16_15 * pf_16_15))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))) * (vs_cbuf13_1.w))) * utof(u_1_phi_75)), 0.0, 1.0)); // maybe duplicate expression on the right side of the assignment, vars:(pf_16_20, pf_16_20)
		// 1.00  <=>  (clamp(((1.0f / {(vs_cbuf13_5.y) : 0}) * sqrt((({pf_21_19 : -269.31445} * {pf_21_19 : -269.31445}) + (({pf_17_15 : 528.0908} * {pf_17_15 : 528.0908}) + ({pf_16_15 : -1653.6611} * {pf_16_15 : -1653.6611}))))), 0.0, 1.0) * clamp(((1.0f / (float(int((myIsNaN({i.vao_attr3.w : 500.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 500.00}), float(-2147483600.f), float(2147483600.f)))))) * {(vs_cbuf13_1.w) : 0})) * {utof(u_1_phi_75) : 291.00}), 0.0, 1.0))
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
	// 0.7942238  <=>  sin({pf_11_21 : -2240.8733})
	f_4_61 = sin(pf_11_21);
	// -0.6076254  <=>  cos({pf_7_4 : -2240.8733})
	f_7_42 = cos(pf_7_4);
	// 1061900864  <=>  {ftou(f_1_20) : 1061900864}
	u_1_23 = ftou(f_1_20);
	u_1_phi_77 = u_1_23;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1157234688  <=>  {vs_cbuf9[197].y : 1157234688}
		u_1_24 = vs_cbuf9[197].y;
		u_1_phi_77 = u_1_24;
	}
	// -0.0012988561  <=>  (((({pf_7_3 : -0.34716997} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_7_3 : -0.34716997})) * {(vs_cbuf13_2.y) : 0.01}) * {(vs_cbuf16_1.x) : 0.7282469})
	pf_17_16 = ((((pf_7_3 * (vs_cbuf16_1.w)) * abs(pf_7_3)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.x));
	// 1061900864  <=>  {u_1_phi_77 : 1061900864}
	u_5_26 = u_1_phi_77;
	u_5_phi_78 = u_5_26;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1067526629  <=>  {ftou((1.0f / {utof(u_1_phi_77) : 0.7942238})) : 1067526629}
		u_5_27 = ftou((1.0f / utof(u_1_phi_77)));
		u_5_phi_78 = u_5_27;
	}
	// 1155251705  <=>  {ftou(f_5_14) : 1155251705}
	u_1_25 = ftou(f_5_14);
	u_1_phi_79 = u_1_25;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1157234688  <=>  {ftou(max({f_5_14 : 1757.937}, {utof(vs_cbuf9[197].y) : 2000.00})) : 1157234688}
		u_1_26 = ftou(max(f_5_14, utof(vs_cbuf9[197].y)));
		u_1_phi_79 = u_1_26;
	}
	// -0  <=>  (((({pf_7_3 : -0.34716997} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_7_3 : -0.34716997})) * {(vs_cbuf13_2.y) : 0.01}) * {(vs_cbuf16_1.y) : 0})
	pf_20_17 = ((((pf_7_3 * (vs_cbuf16_1.w)) * abs(pf_7_3)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.y));
	// -0.0012222854  <=>  (((({pf_7_3 : -0.34716997} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_7_3 : -0.34716997})) * {(vs_cbuf13_2.y) : 0.01}) * {(vs_cbuf16_1.z) : 0.685315})
	pf_11_23 = ((((pf_7_3 * (vs_cbuf16_1.w)) * abs(pf_7_3)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.z));
	// 3273756447  <=>  {ftou((((({(vs_cbuf13_0.z) : 1.00} * {(vs_cbuf13_1.y) : 1.00}) * {utof(u_7_phi_76) : 1.00}) * {f_4_61 : 0.7942238}) + ({pf_6_11 : 4.500549} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0})))))))) : 3273756447}
	u_6_29 = ftou((((((vs_cbuf13_0.z) * (vs_cbuf13_1.y)) * utof(u_7_phi_76)) * f_4_61) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7))))))));
	u_6_phi_80 = u_6_29;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1152288318  <=>  {ftou(({utof(u_1_phi_79) : 1757.937} * {utof(u_5_phi_78) : 0.7942238})) : 1152288318}
		u_6_30 = ftou((utof(u_1_phi_79) * utof(u_5_phi_78)));
		u_6_phi_80 = u_6_30;
	}
	// 3280116823  <=>  {ftou((((min({f_5_14 : 1757.937}, {utof(vs_cbuf9[197].x) : 2000.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 2000.00})) * ((0.f - {pf_1_9 : -229.51624}) + (((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_17_16 : -0.0012988561}) + (({f_1_20 : 0.7942238} * ({utof(u_8_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00})) + ({pf_1_9 : -229.51624} + (({pf_5_15 : 64.35386} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -9.45002} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -166.85384} * (0.f - sin({pf_13_9 : 0})))))))))) + {pf_1_9 : -229.51624})) : 3280116823}
	u_1_28 = ftou((((min(f_5_14, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x))) * ((0.f - pf_1_9) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_17_16) + ((f_1_20 * (utof(u_8_phi_76) * (vs_cbuf13_0.z))) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9)))))))))) + pf_1_9));
	// 3280116823  <=>  {u_1_28 : }
	u_2_14 = u_1_28;
	u_2_phi_81 = u_2_14;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1169092738  <=>  {ftou(((((0.f - {pf_1_9 : -229.51624}) + (((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_17_16 : -0.0012988561}) + (({f_1_20 : 0.7942238} * ({utof(u_8_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00})) + ({pf_1_9 : -229.51624} + (({pf_5_15 : 64.35386} * ({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697}))) + (({pf_20_14 : -9.45002} * (cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00})) + ({pf_11_11 : -166.85384} * (0.f - sin({pf_13_9 : 0}))))))))) * {utof(u_6_phi_80) : -161.55907}) + {pf_1_9 : -229.51624})) : 1169092738}
		u_5_28 = ftou(((((0.f - pf_1_9) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_17_16) + ((f_1_20 * (utof(u_8_phi_76) * (vs_cbuf13_0.z))) + (pf_1_9 + ((pf_5_15 * (f_4_56 * sin(pf_11_8))) + ((pf_20_14 * (cos(pf_11_8) * f_4_56)) + (pf_11_11 * (0.f - sin(pf_13_9))))))))) * utof(u_6_phi_80)) + pf_1_9));
		// 1169092738  <=>  {u_5_28 : }
		u_2_15 = u_5_28;
		u_2_phi_81 = u_2_15;
	}
	// 3272439274  <=>  {ftou((((min({f_5_14 : 1757.937}, {utof(vs_cbuf9[197].x) : 2000.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 2000.00})) * ((0.f - {pf_6_11 : 4.500549}) + (((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_20_17 : -0}) + (((({(vs_cbuf13_0.z) : 1.00} * {(vs_cbuf13_1.y) : 1.00}) * {utof(u_7_phi_76) : 1.00}) * {f_4_61 : 0.7942238}) + ({pf_6_11 : 4.500549} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0})))))))))) + {pf_6_11 : 4.500549})) : 3272439274}
	u_1_29 = ftou((((min(f_5_14, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x))) * ((0.f - pf_6_11) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_20_17) + (((((vs_cbuf13_0.z) * (vs_cbuf13_1.y)) * utof(u_7_phi_76)) * f_4_61) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7)))))))))) + pf_6_11));
	// 3272439274  <=>  {u_1_29 : }
	u_5_29 = u_1_29;
	u_5_phi_82 = u_5_29;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1188143584  <=>  {ftou(((((0.f - {pf_6_11 : 4.500549}) + (((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_20_17 : -0}) + (((({(vs_cbuf13_0.z) : 1.00} * {(vs_cbuf13_1.y) : 1.00}) * {utof(u_7_phi_76) : 1.00}) * {f_4_61 : 0.7942238}) + ({pf_6_11 : 4.500549} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697})))) + ({pf_11_11 : -166.85384} * ({f_4_56 : 1.00} * cos({pf_12_7 : 0}))))))))) * {utof(u_6_phi_80) : -161.55907}) + {pf_6_11 : 4.500549})) : 1188143584}
		u_7_5 = ftou(((((0.f - pf_6_11) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_20_17) + (((((vs_cbuf13_0.z) * (vs_cbuf13_1.y)) * utof(u_7_phi_76)) * f_4_61) + (pf_6_11 + ((pf_5_15 * ((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8))))) + ((pf_20_14 * ((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8)))) + (pf_11_11 * (f_4_56 * cos(pf_12_7))))))))) * utof(u_6_phi_80)) + pf_6_11));
		// 1188143584  <=>  {u_7_5 : }
		u_5_30 = u_7_5;
		u_5_phi_82 = u_5_30;
	}
	// 3310888396  <=>  {ftou((((min({f_5_14 : 1757.937}, {utof(vs_cbuf9[197].x) : 2000.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 2000.00})) * ((0.f - {pf_8_3 : -3409.6157}) + (((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_11_23 : -0.0012222854}) + ((({utof(u_6_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00}) * {f_7_42 : -0.6076254}) + ({pf_8_3 : -3409.6157} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0})))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -166.85384} * ({f_7_26 : 0} * {f_4_56 : 1.00}))))))))) + {pf_8_3 : -3409.6157})) : 3310888396}
	u_1_30 = ftou((((min(f_5_14, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x))) * ((0.f - pf_8_3) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_11_23) + (((utof(u_6_phi_76) * (vs_cbuf13_0.z)) * f_7_42) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (f_7_26 * sin(pf_11_8))) + (cos(pf_11_8) * cos(pf_12_7)))) + ((pf_20_14 * ((sin(pf_13_9) * (f_7_26 * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (f_7_26 * f_4_56))))))))) + pf_8_3));
	// 3310888396  <=>  {u_1_30 : }
	u_7_6 = u_1_30;
	u_7_phi_83 = u_7_6;
	// False  <=>  if(({b_1_67 : False} ? true : false))
	if((b_1_67 ? true : false))
	{
		// 1168994809  <=>  {ftou(((((0.f - {pf_8_3 : -3409.6157}) + (((((clamp(min(0.f, {f_0_6 : 0.62995}), 0.0, 1.0) + {i.vao_attr5.x : 600.7642}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_11_23 : -0.0012222854}) + ((({utof(u_6_phi_76) : 1.00} * {(vs_cbuf13_0.z) : 1.00}) * {f_7_42 : -0.6076254}) + ({pf_8_3 : -3409.6157} + (({pf_5_15 : 64.35386} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0})))) + (({pf_20_14 : -9.45002} * ((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756}))) + ({pf_11_11 : -166.85384} * ({f_7_26 : 0} * {f_4_56 : 1.00})))))))) * {utof(u_6_phi_80) : -161.55907}) + {pf_8_3 : -3409.6157})) : 1168994809}
		u_6_31 = ftou(((((0.f - pf_8_3) + (((((clamp(min(0.f, f_0_6), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_11_23) + (((utof(u_6_phi_76) * (vs_cbuf13_0.z)) * f_7_42) + (pf_8_3 + ((pf_5_15 * ((sin(pf_13_9) * (f_7_26 * sin(pf_11_8))) + (cos(pf_11_8) * cos(pf_12_7)))) + ((pf_20_14 * ((sin(pf_13_9) * (f_7_26 * cos(pf_11_8))) + (0.f - pf_22_10))) + (pf_11_11 * (f_7_26 * f_4_56)))))))) * utof(u_6_phi_80)) + pf_8_3));
		// 1168994809  <=>  {u_6_31 : }
		u_7_7 = u_6_31;
		u_7_phi_83 = u_7_7;
	}
	// 230.0145  <=>  ((({utof(u_7_phi_83) : -3458.3623} * {(view_proj[1].z) : 0.3768303}) + (({utof(u_5_phi_82) : -141.4606} * {(view_proj[1].y) : 0.8616711}) + ({utof(u_2_phi_81) : -261.2214} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
	pf_4_8 = (((utof(u_7_phi_83) * (view_proj[1].z)) + ((utof(u_5_phi_82) * (view_proj[1].y)) + (utof(u_2_phi_81) * (view_proj[1].x)))) + (view_proj[1].w));
	// -1047.2377  <=>  ((({utof(u_7_phi_83) : -3458.3623} * {(view_proj[0].z) : 0.6697676}) + (({utof(u_5_phi_82) : -141.4606} * {(view_proj[0].y) : 1.493044E-08}) + ({utof(u_2_phi_81) : -261.2214} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
	pf_7_15 = (((utof(u_7_phi_83) * (view_proj[0].z)) + ((utof(u_5_phi_82) * (view_proj[0].y)) + (utof(u_2_phi_81) * (view_proj[0].x)))) + (view_proj[0].w));
	// 1.00  <=>  ((({utof(u_7_phi_83) : -3458.3623} * {(view_proj[3].z) : 0}) + (({utof(u_5_phi_82) : -141.4606} * {(view_proj[3].y) : 0}) + ({utof(u_2_phi_81) : -261.2214} * {(view_proj[3].x) : 0}))) + {(view_proj[3].w) : 1.00})
	pf_5_25 = (((utof(u_7_phi_83) * (view_proj[3].z)) + ((utof(u_5_phi_82) * (view_proj[3].y)) + (utof(u_2_phi_81) * (view_proj[3].x)))) + (view_proj[3].w));
	// -1390.0312  <=>  ((({utof(u_7_phi_83) : -3458.3623} * {(view_proj[2].z) : -0.6398518}) + (({utof(u_5_phi_82) : -141.4606} * {(view_proj[2].y) : 0.5074672}) + ({utof(u_2_phi_81) : -261.2214} * {(view_proj[2].x) : -0.57711935}))) + {(view_proj[2].w) : -3681.8398})
	pf_11_27 = (((utof(u_7_phi_83) * (view_proj[2].z)) + ((utof(u_5_phi_82) * (view_proj[2].y)) + (utof(u_2_phi_81) * (view_proj[2].x)))) + (view_proj[2].w));
	// 1390.031  <=>  (({pf_5_25 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[7].z) : -1}) + (({pf_4_8 : 230.0145} * {(view_proj[7].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[7].x) : 0}))))
	pf_11_28 = ((pf_5_25 * (view_proj[7].w)) + ((pf_11_27 * (view_proj[7].z)) + ((pf_4_8 * (view_proj[7].y)) + (pf_7_15 * (view_proj[7].x)))));
	// 507.1979  <=>  ((0.f - {utof(u_5_phi_82) : -141.4606}) + {(camera_wpos.y) : 365.7373})
	pf_16_30 = ((0.f - utof(u_5_phi_82)) + (camera_wpos.y));
	// 1389.842  <=>  (({pf_5_25 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_11_27 : -1390.0312} * {(view_proj[6].z) : -1.000008}) + (({pf_4_8 : 230.0145} * {(view_proj[6].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[6].x) : 0}))))
	pf_4_11 = ((pf_5_25 * (view_proj[6].w)) + ((pf_11_27 * (view_proj[6].z)) + ((pf_4_8 * (view_proj[6].y)) + (pf_7_15 * (view_proj[6].x)))));
	// -1658.0408  <=>  ((0.f - {utof(u_2_phi_81) : -261.2214}) + {(camera_wpos.x) : -1919.2622})
	pf_5_26 = ((0.f - utof(u_2_phi_81)) + (camera_wpos.x));
	// 3280116823  <=>  {u_2_phi_81 : 3280116823}
	u_1_31 = u_2_phi_81;
	u_1_phi_84 = u_1_31;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1156045034  <=>  {ftou(({utof(u_2_phi_81) : -261.2214} + (0.f - {(vs_cbuf15_52.x) : -2116}))) : 1156045034}
		u_1_32 = ftou((utof(u_2_phi_81) + (0.f - (vs_cbuf15_52.x))));
		u_1_phi_84 = u_1_32;
	}
	// 0  <=>  ((0.f * (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[4].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : -1047.2377} * {(view_proj[5].x) : 0}))))))
	pf_20_19 = ((0.f * ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))))) + (0.f * ((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x)))))));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_0_3 : 1}), int(16u), int(16u))) * {u_5_21 : 0})) << 16u) + {u_1_13 : 0})
	u_2_16 = ((uint((uint(bitfieldExtract(uint(u_0_3), int(16u), int(16u))) * u_5_21)) << 16u) + u_1_13);
	u_2_phi_85 = u_2_16;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 3207016068  <=>  {ftou(({utof(u_1_phi_84) : -261.2214} * {(vs_cbuf15_52.z) : 0.0025})) : 3207016068}
		u_2_17 = ftou((utof(u_1_phi_84) * (vs_cbuf15_52.z)));
		u_2_phi_85 = u_2_17;
	}
	// -274.68457  <=>  ((0.f - {utof(u_7_phi_83) : -3458.3623}) + {(camera_wpos.z) : -3733.0469})
	pf_27_6 = ((0.f - utof(u_7_phi_83)) + (camera_wpos.z));
	// 3310888396  <=>  {u_7_phi_83 : 3310888396}
	u_0_8 = u_7_phi_83;
	u_0_phi_86 = u_0_8;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1139593632  <=>  {ftou(({utof(u_7_phi_83) : -3458.3623} + (0.f - {(vs_cbuf15_52.y) : -3932}))) : 1139593632}
		u_0_9 = ftou((utof(u_7_phi_83) + (0.f - (vs_cbuf15_52.y))));
		u_0_phi_86 = u_0_9;
	}
	// 0.0005696  <=>  inversesqrt((({pf_27_6 : -274.68457} * {pf_27_6 : -274.68457}) + (({pf_16_30 : 507.1979} * {pf_16_30 : 507.1979}) + ({pf_5_26 : -1658.0408} * {pf_5_26 : -1658.0408}))))
	f_4_86 = inversesqrt(((pf_27_6 * pf_27_6) + ((pf_16_30 * pf_16_30) + (pf_5_26 * pf_5_26))));
	// 0  <=>  {u_26_20 : 0}
	u_1_35 = u_26_20;
	u_1_phi_87 = u_1_35;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 3238679969  <=>  {ftou(({utof(u_0_phi_86) : -3458.3623} * {(vs_cbuf15_52.z) : 0.0025})) : 3238679969}
		u_1_36 = ftou((utof(u_0_phi_86) * (vs_cbuf15_52.z)));
		u_1_phi_87 = u_1_36;
	}
	// -0.5556095  <=>  (1.0f / ((((({pf_11_28 : 1390.031} * 0.5f) + (({pf_4_11 : 1389.842} * 0.5f) + {pf_20_19 : 0})) * (1.0f / ({pf_11_28 : 1390.031} + ((0.f * {pf_4_11 : 1389.842}) + {pf_20_19 : 0})))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00})))
	f_4_89 = (1.0f / (((((pf_11_28 * 0.5f) + ((pf_4_11 * 0.5f) + pf_20_19)) * (1.0f / (pf_11_28 + ((0.f * pf_4_11) + pf_20_19)))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y))));
	// 0.0237522  <=>  ((((({pf_27_6 : -274.68457} * {f_4_86 : 0.0005696}) * {(lightDir.z) : -0.08728968}) + ((({pf_16_30 : 507.1979} * {f_4_86 : 0.0005696}) * {(lightDir.y) : -0.4663191}) + (({pf_5_26 : -1658.0408} * {f_4_86 : 0.0005696}) * {(lightDir.x) : 0.8802994}))) * 0.5f) + 0.5f)
	pf_5_31 = (((((pf_27_6 * f_4_86) * (lightDir.z)) + (((pf_16_30 * f_4_86) * (lightDir.y)) + ((pf_5_26 * f_4_86) * (lightDir.x)))) * 0.5f) + 0.5f);
	// 1.565732  <=>  (({pf_5_31 : 0.0237522} * (({pf_5_31 : 0.0237522} * (({pf_5_31 : 0.0237522} * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
	pf_5_32 = ((pf_5_31 * ((pf_5_31 * ((pf_5_31 * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f);
	// 0.9520816  <=>  exp2((log2(((0.f - clamp(((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.y) : 1.00}))
	f_5_26 = exp2((log2(((0.f - clamp((((f_4_89 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.y)));
	// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, float2(((({pf_5_32 : 1.565732} * (0.f - sqrt(((0.f - {pf_5_31 : 0.0237522}) + 1.f)))) * 0.63661975f) + 1.f), (({f_5_26 : 0.9520816} * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
	f4_0_0 = textureLod(tex0, float2((((pf_5_32 * (0.f - sqrt(((0.f - pf_5_31) + 1.f)))) * 0.63661975f) + 1.f), ((f_5_26 * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler);
	// 0  <=>  {u_2_phi_85 : 0}
	u_0_11 = u_2_phi_85;
	u_0_phi_88 = u_0_11;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex1 : tex1}, float2({utof(u_2_phi_85) : 0}, {utof(u_1_phi_87) : 0}), 0.0, s_linear_clamp_sampler)
		f4_0_1 = textureLod(tex1, float2(utof(u_2_phi_85), utof(u_1_phi_87)), 0.0, s_linear_clamp_sampler);
		// 1065353216  <=>  {ftou(f4_0_1.w) : 1065353216}
		u_0_12 = ftou(f4_0_1.w);
		u_0_phi_88 = u_0_12;
	}
	// 1.00  <=>  1.f
	o.fs_attr9.x = 1.f;
	// 112  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 112u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_6_34 = ((ftou(vs_cbuf0_21.x) + 112u) - ftou(vs_cbuf0_21.x));
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x : }
	u_6_35 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y : }
	u_7_9 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y;
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z : }
	u_8_30 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z;
	// 128  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 128u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_1_40 = ((ftou(vs_cbuf0_21.x) + 128u) - ftou(vs_cbuf0_21.x));
	// -1263.2671  <=>  (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[4].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[4].x) : 1.206285}))))
	o.vertex.x = ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))));
	// 493.2678  <=>  (({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : -1047.2377} * {(view_proj[5].x) : 0}))))
	o.vertex.y = ((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x)))));
	// 1389.842  <=>  {pf_4_11 : 1389.842}
	o.vertex.z = pf_4_11;
	// 1390.031  <=>  {pf_11_28 : 1390.031}
	o.vertex.w = pf_11_28;
	// 1389.937  <=>  (({pf_11_28 : 1390.031} * 0.5f) + (({pf_4_11 : 1389.842} * 0.5f) + {pf_20_19 : 0}))
	o.fs_attr4.z = ((pf_11_28 * 0.5f) + ((pf_4_11 * 0.5f) + pf_20_19));
	// 1390.031  <=>  ({pf_11_28 : 1390.031} + ((0.f * {pf_4_11 : 1389.842}) + {pf_20_19 : 0}))
	o.fs_attr4.w = (pf_11_28 + ((0.f * pf_4_11) + pf_20_19));
	// 1365.449  <=>  ((({pf_8_3 : -3409.6157} * {(vs_cbuf8_11.z) : 0.6398518}) + (({pf_6_11 : 4.500549} * {(vs_cbuf8_11.y) : -0.50746715}) + ({pf_1_9 : -229.51624} * {(vs_cbuf8_11.x) : 0.5771194}))) + {(vs_cbuf8_11.w) : 3681.84})
	pf_5_38 = (((pf_8_3 * (vs_cbuf8_11.z)) + ((pf_6_11 * (vs_cbuf8_11.y)) + (pf_1_9 * (vs_cbuf8_11.x)))) + (vs_cbuf8_11.w));
	// 1365.26  <=>  ((({pf_8_3 : -3409.6157} * {(vs_cbuf8_10.z) : 0.6398569}) + (({pf_6_11 : 4.500549} * {(vs_cbuf8_10.y) : -0.5074712}) + ({pf_1_9 : -229.51624} * {(vs_cbuf8_10.x) : 0.5771239}))) + {(vs_cbuf8_10.w) : 3681.669})
	pf_1_13 = (((pf_8_3 * (vs_cbuf8_10.z)) + ((pf_6_11 * (vs_cbuf8_10.y)) + (pf_1_9 * (vs_cbuf8_10.x)))) + (vs_cbuf8_10.w));
	// 0.25095  <=>  ((({(vs_cbuf13_2.w) : 45.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[79].y) : 0}) + ((((({pf_0_1 : 291.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_35_phi_47) : 0.39216} * {utof(vs_cbuf9[81].z) : 0}) + ({utof(vs_cbuf9[81].z) : 0} + {utof(vs_cbuf9[81].x) : 0.70}))) * (({pf_27_2 : 1.00} * {utof(u_16_phi_27) : 0.4939}) + -0.5f)) + (({pf_27_2 : 1.00} * float(int({u_1_7 : 0}))) + (({pf_0_1 : 291.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.0001})) + (0.f - ((({utof(u_36_phi_47) : 0.39216} * {utof(vs_cbuf9[80].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 1.00} + {utof(vs_cbuf9[79].z) : 0})))))) + 0.5f))
	o.fs_attr2.z = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[79].y)) + (((((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_35_phi_47) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].z) + utof(vs_cbuf9[81].x)))) * ((pf_27_2 * utof(u_16_phi_27)) + -0.5f)) + ((pf_27_2 * float(int(u_1_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_36_phi_47) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))) + 0.5f));
	// 0.8399248  <=>  ((({(vs_cbuf13_2.w) : 45.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[84].y) : 0}) + ((((((({pf_22_8 : 1.00} * {utof(u_13_phi_30) : 0.4939}) + -0.5f) * cos({f_4_40 : -8.747199})) + (0.f - ((({pf_17_6 : 1.00} * {utof(u_23_phi_39) : 0.98535}) + -0.5f) * sin({f_4_40 : -8.747199})))) * (({pf_0_1 : 291.00} * {utof(vs_cbuf9[85].z) : 0}) + (({utof(u_39_phi_58) : 0.62995} * {utof(vs_cbuf9[86].z) : 0}) + ({utof(vs_cbuf9[86].x) : 1.10} + {utof(vs_cbuf9[86].z) : 0})))) + (({pf_22_8 : 1.00} * float(int({u_1_14 : 0}))) + (({pf_0_1 : 291.00} * (0.f - {utof(vs_cbuf9[84].x) : 0})) + (0.f - ((({utof(u_35_phi_58) : 0.73934} * {utof(vs_cbuf9[85].x) : 0}) * -2.f) + ({utof(vs_cbuf9[85].x) : 0} + {utof(vs_cbuf9[84].z) : 0})))))) + 0.5f))
	o.fs_attr3.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[84].y)) + (((((((pf_22_8 * utof(u_13_phi_30)) + -0.5f) * cos(f_4_40)) + (0.f - (((pf_17_6 * utof(u_23_phi_39)) + -0.5f) * sin(f_4_40)))) * ((pf_0_1 * utof(vs_cbuf9[85].z)) + ((utof(u_39_phi_58) * utof(vs_cbuf9[86].z)) + (utof(vs_cbuf9[86].x) + utof(vs_cbuf9[86].z))))) + ((pf_22_8 * float(int(u_1_14))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[84].x))) + (0.f - (((utof(u_35_phi_58) * utof(vs_cbuf9[85].x)) * -2.f) + (utof(vs_cbuf9[85].x) + utof(vs_cbuf9[84].z))))))) + 0.5f));
	// 1065189135  <=>  {ftou(v.offset.z) : 1065189135}
	u_15_22 = ftou(v.offset.z);
	u_15_phi_89 = u_15_22;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1065189135  <=>  {ftou((({v.vertex.z : 0.10712} * {(vs_cbuf13_0.x) : 0}) + {v.offset.z : 0.99022})) : 1065189135}
		u_15_23 = ftou(((v.vertex.z * (vs_cbuf13_0.x)) + v.offset.z));
		u_15_phi_89 = u_15_23;
	}
	// 1041254423  <=>  {ftou(v.offset.y) : 1041254423}
	u_2_19 = ftou(v.offset.y);
	u_2_phi_90 = u_2_19;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1041254423  <=>  {ftou((({v.vertex.y : -0.34717} * {(vs_cbuf13_0.x) : 0}) + {v.offset.y : 0.1409})) : 1041254423}
		u_2_20 = ftou(((v.vertex.y * (vs_cbuf13_0.x)) + v.offset.y));
		u_2_phi_90 = u_2_20;
	}
	// 2.269656  <=>  (((({(vs_cbuf13_2.w) : 45.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_0_1 : 291.00} * {utof(vs_cbuf9[75].w) : 0}) + (({f_8_13 : 0.39216} * {utof(vs_cbuf9[76].w) : 0.10}) + ({utof(vs_cbuf9[76].w) : 0.10} + {utof(vs_cbuf9[76].y) : 1.40}))) * (({pf_28_1 : 1.00} * {utof(u_12_phi_37) : 0.98535}) + -0.5f)) + (0.f - (({pf_28_1 : 1.00} * (0.f - float(int((({b_1_53 : False} || {b_0_13 : True}) ? {u_9_14 : 0} : 4294967295u))))) + (({pf_0_1 : 291.00} * {utof(vs_cbuf9[74].y) : -0.0001}) + ((({f_8_13 : 0.39216} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0})))))) + 0.5f))
	o.fs_attr2.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[74].y)) + (((((pf_0_1 * utof(vs_cbuf9[75].w)) + ((f_8_13 * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)))) * ((pf_28_1 * utof(u_12_phi_37)) + -0.5f)) + (0.f - ((pf_28_1 * (0.f - float(int(((b_1_53 || b_0_13) ? u_9_14 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((f_8_13 * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f));
	// -1.8340976  <=>  ((((({pf_1_13 : 1365.26} * 0.5f) + ({pf_5_38 : 1365.449} * 0.5f)) * (1.0f / ((0.f * {pf_1_13 : 1365.26}) + {pf_5_38 : 1365.449}))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))
	pf_1_16 = (((((pf_1_13 * 0.5f) + (pf_5_38 * 0.5f)) * (1.0f / ((0.f * pf_1_13) + pf_5_38))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)));
	// -0.5452272  <=>  (1.0f / {pf_1_16 : -1.8340976})
	f_0_11 = (1.0f / pf_1_16);
	// 0  <=>  {ftou(v.offset.x) : 0}
	u_12_33 = ftou(v.offset.x);
	u_12_phi_91 = u_12_33;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.x : -0.01573} * {(vs_cbuf13_0.x) : 0}) + {v.offset.x : 0})) : 0}
		u_12_34 = ftou(((v.vertex.x * (vs_cbuf13_0.x)) + v.offset.x));
		u_12_phi_91 = u_12_34;
	}
	// 0.9727027  <=>  ((({(vs_cbuf13_2.w) : 45.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_24_4 : 1.00} * {utof(u_5_phi_24) : 0.4939}) + -0.5f) * (({pf_0_1 : 291.00} * {utof(vs_cbuf9[75].z) : 0}) + (({f_0_6 : 0.62995} * {utof(vs_cbuf9[76].z) : 0.10}) + ({utof(vs_cbuf9[76].x) : 4.50} + {utof(vs_cbuf9[76].z) : 0.10})))) + (({pf_24_4 : 1.00} * float(int({u_2_7 : 0}))) + (({pf_0_1 : 291.00} * (0.f - {utof(vs_cbuf9[74].x) : 0})) + (0.f - ((({f_0_6 : 0.62995} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0})))))) + 0.5f))
	o.fs_attr2.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[74].y)) + (((((pf_24_4 * utof(u_5_phi_24)) + -0.5f) * ((pf_0_1 * utof(vs_cbuf9[75].z)) + ((f_0_6 * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z))))) + ((pf_24_4 * float(int(u_2_7))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((f_0_6 * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f));
	// 0.98535  <=>  (((({(vs_cbuf13_2.w) : 45.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[79].y) : 0}) + ((((({pf_0_1 : 291.00} * {utof(vs_cbuf9[80].w) : 0}) + (({utof(u_33_phi_47) : 0.73934} * {utof(vs_cbuf9[81].w) : 0}) + ({utof(vs_cbuf9[81].w) : 0} + {utof(vs_cbuf9[81].y) : 1.00}))) * (({pf_28_2 : 1.00} * {utof(u_22_phi_38) : 0.98535}) + -0.5f)) + (0.f - (({pf_28_2 : 1.00} * (0.f - float(int((({b_5_5 : False} || {b_1_57 : True}) ? {u_8_18 : 0} : 4294967295u))))) + (({pf_0_1 : 291.00} * {utof(vs_cbuf9[79].y) : 0}) + ((({utof(u_34_phi_47) : 0.73934} * {utof(vs_cbuf9[80].y) : 0}) * -2.f) + ({utof(vs_cbuf9[80].y) : 0} + {utof(vs_cbuf9[79].w) : 0})))))) + 0.5f))
	o.fs_attr2.w = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[79].y)) + (((((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_33_phi_47) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y)))) * ((pf_28_2 * utof(u_22_phi_38)) + -0.5f)) + (0.f - ((pf_28_2 * (0.f - float(int(((b_5_5 || b_1_57) ? u_8_18 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_34_phi_47) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f));
	// -0.66321725  <=>  ((({f_4_56 : 1.00} * sin({pf_11_8 : -2.407697})) * {utof(u_15_phi_89) : 0.99022}) + (((cos({pf_11_8 : -2.407697}) * {f_4_56 : 1.00}) * {utof(u_12_phi_91) : 0}) + (sin({pf_13_9 : 0}) * (0.f - {utof(u_2_phi_90) : 0.1409}))))
	o.fs_attr8.x = (((f_4_56 * sin(pf_11_8)) * utof(u_15_phi_89)) + (((cos(pf_11_8) * f_4_56) * utof(u_12_phi_91)) + (sin(pf_13_9) * (0.f - utof(u_2_phi_90)))));
	// -0.73530847  <=>  ((((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) + (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) * {utof(u_15_phi_89) : 0.99022}) + ((((sin({pf_13_9 : 0}) * ({f_7_26 : 0} * cos({pf_11_8 : -2.407697}))) + (0.f - {pf_22_10 : -0.66976756})) * {utof(u_12_phi_91) : 0}) + (({f_7_26 : 0} * {f_4_56 : 1.00}) * {utof(u_2_phi_90) : 0.1409})))
	o.fs_attr8.z = ((((sin(pf_13_9) * (f_7_26 * sin(pf_11_8))) + (cos(pf_11_8) * cos(pf_12_7))) * utof(u_15_phi_89)) + ((((sin(pf_13_9) * (f_7_26 * cos(pf_11_8))) + (0.f - pf_22_10)) * utof(u_12_phi_91)) + ((f_7_26 * f_4_56) * utof(u_2_phi_90))));
	// 0.1409  <=>  ((((sin({pf_13_9 : 0}) * {pf_22_10 : -0.66976756}) + (0.f - ({f_7_26 : 0} * cos({pf_11_8 : -2.407697})))) * {utof(u_15_phi_89) : 0.99022}) + ((((sin({pf_13_9 : 0}) * (cos({pf_11_8 : -2.407697}) * cos({pf_12_7 : 0}))) + ({f_7_26 : 0} * sin({pf_11_8 : -2.407697}))) * {utof(u_12_phi_91) : 0}) + (({f_4_56 : 1.00} * cos({pf_12_7 : 0})) * {utof(u_2_phi_90) : 0.1409})))
	o.fs_attr8.y = ((((sin(pf_13_9) * pf_22_10) + (0.f - (f_7_26 * cos(pf_11_8)))) * utof(u_15_phi_89)) + ((((sin(pf_13_9) * (cos(pf_11_8) * cos(pf_12_7))) + (f_7_26 * sin(pf_11_8))) * utof(u_12_phi_91)) + ((f_4_56 * cos(pf_12_7)) * utof(u_2_phi_90))));
	// 1.00  <=>  clamp(((({f_0_11 : -0.5452272} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 900.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[138].x) : 900.00}) + {utof(vs_cbuf9[138].y) : 1100.00}))), 0.0, 1.0)
	f_3_97 = clamp((((f_0_11 * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x))) * (1.0f / ((0.f - utof(vs_cbuf9[138].x)) + utof(vs_cbuf9[138].y)))), 0.0, 1.0);
	// 0.0882606  <=>  (((({(vs_cbuf13_2.w) : 45.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[84].y) : 0}) + ((((((({pf_22_8 : 1.00} * {utof(u_13_phi_30) : 0.4939}) + -0.5f) * sin({f_4_40 : -8.747199})) + (cos({f_4_40 : -8.747199}) * (({pf_17_6 : 1.00} * {utof(u_23_phi_39) : 0.98535}) + -0.5f))) * (({pf_0_1 : 291.00} * {utof(vs_cbuf9[85].w) : 0}) + (({utof(u_41_phi_58) : 0.39216} * {utof(vs_cbuf9[86].w) : 0}) + ({utof(vs_cbuf9[86].w) : 0} + {utof(vs_cbuf9[86].y) : 1.10})))) + (0.f - (({pf_17_6 : 1.00} * (0.f - float(int((({b_5_6 : False} || {b_0_15 : True}) ? {u_10_29 : 0} : 4294967295u))))) + (({pf_0_1 : 291.00} * {utof(vs_cbuf9[84].y) : 0}) + ((({utof(u_37_phi_58) : 0.62995} * {utof(vs_cbuf9[85].y) : 0}) * -2.f) + ({utof(vs_cbuf9[85].y) : 0} + {utof(vs_cbuf9[84].w) : 0})))))) + 0.5f))
	o.fs_attr3.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[84].y)) + (((((((pf_22_8 * utof(u_13_phi_30)) + -0.5f) * sin(f_4_40)) + (cos(f_4_40) * ((pf_17_6 * utof(u_23_phi_39)) + -0.5f))) * ((pf_0_1 * utof(vs_cbuf9[85].w)) + ((utof(u_41_phi_58) * utof(vs_cbuf9[86].w)) + (utof(vs_cbuf9[86].w) + utof(vs_cbuf9[86].y))))) + (0.f - ((pf_17_6 * (0.f - float(int(((b_5_6 || b_0_15) ? u_10_29 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[84].y)) + (((utof(u_37_phi_58) * utof(vs_cbuf9[85].y)) * -2.f) + (utof(vs_cbuf9[85].y) + utof(vs_cbuf9[84].w))))))) + 0.5f));
	// 448.3817  <=>  (({pf_11_28 : 1390.031} * 0.5f) + ((0.f * {pf_4_11 : 1389.842}) + ((0.f * (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[4].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : -1047.2377} * {(view_proj[5].x) : 0})))) * -0.5f))))
	o.fs_attr4.y = ((pf_11_28 * 0.5f) + ((0.f * pf_4_11) + ((0.f * ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))))) + (((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x))))) * -0.5f))));
	// 1.00  <=>  ((0.f - clamp(((({f_0_11 : -0.5452272} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].z) : 1800.00})) * (1.0f / ({utof(vs_cbuf9[138].w) : 2200.00} + (0.f - {utof(vs_cbuf9[138].z) : 1800.00})))), 0.0, 1.0)) + 1.f)
	pf_1_25 = ((0.f - clamp((((f_0_11 * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].z))) * (1.0f / (utof(vs_cbuf9[138].w) + (0.f - utof(vs_cbuf9[138].z))))), 0.0, 1.0)) + 1.f);
	// 0.20  <=>  (clamp((((min({(camera_wpos.y) : 365.7373}, {(vs_cbuf15_27.z) : 250.00}) + (0.f - {utof(u_5_phi_82) : -141.4606})) * {(vs_cbuf15_27.y) : 0.0071429}) + {(vs_cbuf15_27.x) : -0.14285715}), 0.0, 1.0) * {(vs_cbuf15_26.w) : 0.20})
	o.fs_attr10.y = (clamp((((min((camera_wpos.y), (vs_cbuf15_27.z)) + (0.f - utof(u_5_phi_82))) * (vs_cbuf15_27.y)) + (vs_cbuf15_27.x)), 0.0, 1.0) * (vs_cbuf15_26.w));
	// 63.38208  <=>  (({pf_11_28 : 1390.031} * 0.5f) + ((0.f * {pf_4_11 : 1389.842}) + (((({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[4].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * (({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : -1047.2377} * {(view_proj[5].x) : 0}))))))))
	o.fs_attr4.x = ((pf_11_28 * 0.5f) + ((0.f * pf_4_11) + ((((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x))))) * 0.5f) + (0.f * ((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x)))))))));
	// False  <=>  (((({pf_1_25 : 1.00} <= 0.f) && (! myIsNaN({pf_1_25 : 1.00}))) && (! myIsNaN(0.f))) || ((({f_3_97 : 1.00} <= 0.f) && (! myIsNaN({f_3_97 : 1.00}))) && (! myIsNaN(0.f))))
	b_1_78 = ((((pf_1_25 <= 0.f) && (! myIsNaN(pf_1_25))) && (! myIsNaN(0.f))) || (((f_3_97 <= 0.f) && (! myIsNaN(f_3_97))) && (! myIsNaN(0.f))));
	// 1.00  <=>  (({f_3_97 : 1.00} * {pf_1_25 : 1.00}) * {(vs_cbuf10_3.x) : 1.00})
	o.fs_attr6.x = ((f_3_97 * pf_1_25) * (vs_cbuf10_3.x));
	// 0.6242014  <=>  clamp(((((({pf_11_28 : 1390.031} * 0.5f) + ((0.f * {pf_4_11 : 1389.842}) + ((0.f * (({pf_5_25 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[4].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[4].y) : 0}) + ({pf_7_15 : -1047.2377} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_5_25 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_11_27 : -1390.0312} * {(view_proj[5].z) : 0}) + (({pf_4_8 : 230.0145} * {(view_proj[5].y) : 2.144507}) + ({pf_7_15 : -1047.2377} * {(view_proj[5].x) : 0})))) * -0.5f)))) * (1.0f / ({pf_11_28 : 1390.031} + ((0.f * {pf_4_11 : 1389.842}) + {pf_20_19 : 0})))) * -0.7f) + 0.85f), 0.0, 1.0)
	f_2_176 = clamp((((((pf_11_28 * 0.5f) + ((0.f * pf_4_11) + ((0.f * ((pf_5_25 * (view_proj[4].w)) + ((pf_11_27 * (view_proj[4].z)) + ((pf_4_8 * (view_proj[4].y)) + (pf_7_15 * (view_proj[4].x)))))) + (((pf_5_25 * (view_proj[5].w)) + ((pf_11_27 * (view_proj[5].z)) + ((pf_4_8 * (view_proj[5].y)) + (pf_7_15 * (view_proj[5].x))))) * -0.5f)))) * (1.0f / (pf_11_28 + ((0.f * pf_4_11) + pf_20_19)))) * -0.7f) + 0.85f), 0.0, 1.0);
	// ((0.f - {utof(u_6_35) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
	pf_1_27 = ((0.f - utof(u_6_35)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x));
	// ((0.f - {utof(u_7_9) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
	pf_2_27 = ((0.f - utof(u_7_9)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y));
	// ((0.f - {utof(u_8_30) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
	pf_3_18 = ((0.f - utof(u_8_30)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z));
	// ({utof(u_6_35) : } + (({pf_1_27 : } * (0.f - {f_2_176 : 0.6242014})) + {pf_1_27 : }))
	pf_1_29 = (utof(u_6_35) + ((pf_1_27 * (0.f - f_2_176)) + pf_1_27));
	// ({utof(u_8_30) : } + (({pf_3_18 : } * (0.f - {f_2_176 : 0.6242014})) + {pf_3_18 : }))
	pf_3_20 = (utof(u_8_30) + ((pf_3_18 * (0.f - f_2_176)) + pf_3_18));
	// ({utof(u_7_9) : } + (({pf_2_27 : } * (0.f - {f_2_176 : 0.6242014})) + {pf_2_27 : }))
	pf_2_29 = (utof(u_7_9) + ((pf_2_27 * (0.f - f_2_176)) + pf_2_27));
	// 0.3745275  <=>  exp2((log2(((0.f - clamp(((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.x) : 20.00}))
	f_0_23 = exp2((log2(((0.f - clamp((((f_4_89 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.x)));
	// (({pf_1_29 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_1_29 : })
	pf_1_30 = ((pf_1_29 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_1_29);
	// 0  <=>  exp2((log2(((0.f - clamp(((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_24.x) : 0.002381}) + (0.f - {(vs_cbuf15_24.y) : -0.04761905})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_24.w) : 4.00}))
	f_1_84 = exp2((log2(((0.f - clamp((((f_4_89 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_24.x)) + (0.f - (vs_cbuf15_24.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_24.w)));
	// {pf_1_30 : }
	o.fs_attr11.x = pf_1_30;
	// (({pf_2_29 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_29 : })
	pf_0_15 = ((pf_2_29 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_29);
	// (({pf_3_20 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_3_20 : })
	pf_1_32 = ((pf_3_20 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_3_20);
	// {pf_0_15 : }
	o.fs_attr11.y = pf_0_15;
	// {pf_1_32 : }
	o.fs_attr11.z = pf_1_32;
	// 0.7006614  <=>  (({f_1_84 : 0} * (0.f - {(vs_cbuf15_25.w) : 0.7006614})) + {(vs_cbuf15_25.w) : 0.7006614})
	o.fs_attr10.x = ((f_1_84 * (0.f - (vs_cbuf15_25.w))) + (vs_cbuf15_25.w));
	// 0.5316517  <=>  clamp((({f_0_23 : 0.3745275} * (0.f - {(vs_cbuf15_23.z) : 0.85})) + {(vs_cbuf15_23.z) : 0.85}), 0.0, 1.0)
	o.fs_attr12.w = clamp(((f_0_23 * (0.f - (vs_cbuf15_23.z))) + (vs_cbuf15_23.z)), 0.0, 1.0);
	// 0.50  <=>  (clamp(max((((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.x : 0.50})
	o.fs_attr12.x = (clamp(max(((((f_4_89 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.x);
	// 0.50  <=>  (clamp(max((((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.y : 0.50})
	o.fs_attr12.y = (clamp(max(((((f_4_89 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.y);
	// 0.50  <=>  (clamp(max((((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.z : 0.50})
	o.fs_attr12.z = (clamp(max(((((f_4_89 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.z);
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
		f_0_26 = (1.0f / (vs_cbuf15_51.x));
		// 1.00  <=>  ((({utof(u_0_phi_88) : 0} * {(vs_cbuf15_49.x) : 0}) + (0.f - {(vs_cbuf15_49.x) : 0})) + 1.f)
		pf_0_20 = (((utof(u_0_phi_88) * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f);
		// 1.00  <=>  clamp(((({f_4_89 : -0.5556095} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {f_0_26 : 0.0010526}) + (0.f - ({f_0_26 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
		f_0_27 = clamp((((f_4_89 * (0.f - (vs_cbuf8_30.z))) * f_0_26) + (0.f - (f_0_26 * (vs_cbuf15_51.y)))), 0.0, 1.0);
		// -∞  <=>  log2(abs((({pf_0_20 : 1.00} * (0.f - {f_0_27 : 1.00})) + {f_0_27 : 1.00})))
		f_0_29 = log2(abs(((pf_0_20 * (0.f - f_0_27)) + f_0_27)));
		// 0  <=>  exp2(({f_0_29 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
		f_0_30 = exp2((f_0_29 * (vs_cbuf15_51.z)));
		// 1.00  <=>  (({pf_0_20 : 1.00} * (0.f - (({f_0_30 : 0} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0}))) + {pf_0_20 : 1.00})
		o.fs_attr9.x = ((pf_0_20 * (0.f - ((f_0_30 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_20);
	}
	// 0  <=>  0.f
	o.fs_attr10.w = 0.f;
	// True  <=>  if(((! {b_1_78 : False}) ? true : false))
	if(((! b_1_78) ? true : false))
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
