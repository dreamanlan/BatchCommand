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
	// vs_cbuf9[1] = float4(73899130000000000000000.00, 6.5919E-10, 239.3061, 1182091000000000000000000000.00);
	// vs_cbuf9[2] = float4(4.739716E+30, 3.854E-41, 0, 0);
	// vs_cbuf9[3] = float4(0, 0, 0, 0);
	// vs_cbuf9[4] = float4(0, 0, 0, 0);
	// vs_cbuf9[5] = float4(0, 0, 0, 0);
	// vs_cbuf9[6] = float4(0, 0, 0, 0);
	// vs_cbuf9[7] = float4(1.3E-44, 0, 4.2187E-40, 1.5E-44);
	// vs_cbuf9[8] = float4(0, 4E-45, 0, 7E-45);
	// vs_cbuf9[9] = float4(1E-45, 6E-45, 0, 0);
	// vs_cbuf9[10] = float4(0, 0, 0, 0);
	// vs_cbuf9[11] = float4(0, 0, 0, 0);
	// vs_cbuf9[12] = float4(0, 0, 0, 0);
	// vs_cbuf9[13] = float4(0, 0, 0, 0);
	// vs_cbuf9[14] = float4(0, -1, 0, 0);
	// vs_cbuf9[15] = float4(0.98, 0, 0, 0);
	// vs_cbuf9[16] = float4(0, -0.05, 0, 0.125);
	// vs_cbuf9[17] = float4(0, 1.00, 1.00, 20.00);
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
	// vs_cbuf9[76] = float4(2.50, 1.25, 0.05, 0.05);
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
	// vs_cbuf9[104] = float4(1.10, 0, 0, 0);
	// vs_cbuf9[105] = float4(0.8174603, 0.8135269, 0.7460947, 0);
	// vs_cbuf9[106] = float4(0, 0, 0, 0);
	// vs_cbuf9[107] = float4(0, 0, 0, 0);
	// vs_cbuf9[108] = float4(0, 0, 0, 0);
	// vs_cbuf9[109] = float4(0, 0, 0, 0);
	// vs_cbuf9[110] = float4(0, 0, 0, 0);
	// vs_cbuf9[111] = float4(0, 0, 0, 0);
	// vs_cbuf9[112] = float4(0, 0, 0, 0);
	// vs_cbuf9[113] = float4(0.10, 0.20, 0.20, 0);
	// vs_cbuf9[114] = float4(0.10, 0.10, 0.10, 0.40);
	// vs_cbuf9[115] = float4(0.20, 0.20, 0.20, 1.00);
	// vs_cbuf9[116] = float4(0.20, 0.20, 0.20, 4.00);
	// vs_cbuf9[117] = float4(0.20, 0.20, 0.20, 5.00);
	// vs_cbuf9[118] = float4(0.20, 0.20, 0.20, 6.00);
	// vs_cbuf9[119] = float4(0.20, 0.20, 0.20, 7.00);
	// vs_cbuf9[120] = float4(0.20, 0.20, 0.20, 8.00);
	// vs_cbuf9[121] = float4(0.3957546, 0.461164, 0.484127, 0);
	// vs_cbuf9[122] = float4(0, 0, 0, 0);
	// vs_cbuf9[123] = float4(0, 0, 0, 0);
	// vs_cbuf9[124] = float4(0, 0, 0, 0);
	// vs_cbuf9[125] = float4(0, 0, 0, 0);
	// vs_cbuf9[126] = float4(0, 0, 0, 0);
	// vs_cbuf9[127] = float4(0, 0, 0, 0);
	// vs_cbuf9[128] = float4(0, 0, 0, 0);
	// vs_cbuf9[129] = float4(0, 0, 0, 0);
	// vs_cbuf9[130] = float4(0.075, 0.075, 0.075, 0.31);
	// vs_cbuf9[131] = float4(0.30, 0.30, 0.30, 0.65);
	// vs_cbuf9[132] = float4(0.075, 0.075, 0.075, 0.83);
	// vs_cbuf9[133] = float4(0, 0, 0, 1.00);
	// vs_cbuf9[134] = float4(0, 0, 0, 6.00);
	// vs_cbuf9[135] = float4(0, 0, 0, 7.00);
	// vs_cbuf9[136] = float4(0, 0, 0, 8.00);
	// vs_cbuf9[137] = float4(0, 0.50, 0, 1.00);
	// vs_cbuf9[138] = float4(90.00, 110.00, 225.00, 275.00);
	// vs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[140] = float4(1.00, 25.00, 0, 0);
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
	// vs_cbuf9[197] = float4(250.00, 250.00, 0, 0);
	// vs_cbuf10[0] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[1] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[2] = float4(879.50, 60.00, 1.00, 1.00);
	// vs_cbuf10[3] = float4(1.00, 0.9999999, 1.00, 0.9999999);
	// vs_cbuf13[0] = float4(0, 0.50, 1.00, 1.00);
	// vs_cbuf13[1] = float4(1.00, 1.00, 1.00, 0);
	// vs_cbuf13[2] = float4(0, 0.20, 1.00, 130.00);
	// vs_cbuf13[3] = float4(1.00, 0, 0, 1.00);
	// vs_cbuf13[5] = float4(1.00, 0, 0.50, 0);
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
	bool b_1_42;
	bool b_1_44;
	bool b_1_46;
	bool b_1_67;
	bool b_2_18;
	bool b_2_28;
	bool b_2_29;
	bool b_2_30;
	bool b_2_31;
	bool b_2_32;
	bool b_2_33;
	bool b_2_34;
	bool b_3_5;
	bool b_3_8;
	bool b_4_6;
	bool b_4_8;
	bool b_5_6;
	bool b_5_7;
	bool b_6_13;
	float f_0_38;
	float f_0_46;
	float f_0_7;
	float f_0_8;
	float f_0_84;
	float f_0_87;
	float f_0_88;
	float f_0_90;
	float f_0_91;
	float f_1_102;
	float f_1_162;
	float f_1_170;
	float f_1_35;
	float f_1_41;
	float f_1_42;
	float f_1_47;
	float f_1_50;
	float f_1_53;
	float f_1_54;
	float f_1_58;
	float f_1_65;
	float f_1_73;
	float f_1_74;
	float f_1_78;
	float f_1_87;
	float f_1_92;
	float f_2_12;
	float f_2_27;
	float f_2_77;
	float f_3_13;
	float f_3_39;
	float f_3_75;
	float f_3_95;
	float f_4_31;
	float f_4_42;
	float f_5_12;
	float f_5_15;
	float f_5_18;
	float f_5_21;
	float f_5_23;
	float f_5_36;
	float f_5_41;
	float f_5_46;
	float f_5_52;
	float f_6_10;
	float f_6_29;
	float f_6_33;
	float f_6_38;
	float f_6_43;
	float f_7_0;
	float f_8_22;
	float f_8_30;
	float f_9_20;
	float4 f4_0_0;
	float4 f4_0_1;
	float pf_0_1;
	float pf_0_15;
	float pf_0_17;
	float pf_0_26;
	float pf_1_16;
	float pf_1_19;
	float pf_1_21;
	float pf_1_23;
	float pf_10_8;
	float pf_11_6;
	float pf_11_9;
	float pf_12_12;
	float pf_12_23;
	float pf_12_24;
	float pf_12_29;
	float pf_12_30;
	float pf_12_6;
	float pf_12_8;
	float pf_12_9;
	float pf_13_7;
	float pf_13_9;
	float pf_15_13;
	float pf_16_15;
	float pf_16_26;
	float pf_19_18;
	float pf_19_22;
	float pf_19_9;
	float pf_2_19;
	float pf_2_21;
	float pf_2_9;
	float pf_20_0;
	float pf_20_13;
	float pf_20_9;
	float pf_21_10;
	float pf_23_12;
	float pf_24_12;
	float pf_24_15;
	float pf_24_17;
	float pf_25_5;
	float pf_25_6;
	float pf_26_4;
	float pf_27_5;
	float pf_28_1;
	float pf_3_4;
	float pf_4_1;
	float pf_5_10;
	float pf_5_12;
	float pf_5_3;
	float pf_5_5;
	float pf_6_3;
	float pf_6_5;
	float pf_6_9;
	float pf_7_4;
	float pf_8_9;
	uint u_0_1;
	uint u_0_2;
	uint u_0_3;
	uint u_0_4;
	uint u_0_5;
	uint u_0_phi_16;
	uint u_0_phi_36;
	uint u_1_1;
	uint u_1_10;
	uint u_1_13;
	uint u_1_15;
	uint u_1_16;
	uint u_1_18;
	uint u_1_2;
	uint u_1_21;
	uint u_1_22;
	uint u_1_24;
	uint u_1_25;
	uint u_1_30;
	uint u_1_31;
	uint u_1_33;
	uint u_1_34;
	uint u_1_4;
	uint u_1_5;
	uint u_1_6;
	uint u_1_7;
	uint u_1_phi_11;
	uint u_1_phi_46;
	uint u_1_phi_57;
	uint u_1_phi_67;
	uint u_1_phi_73;
	uint u_1_phi_77;
	uint u_10_14;
	uint u_10_15;
	uint u_10_4;
	uint u_10_phi_79;
	uint u_11_2;
	uint u_11_3;
	uint u_11_5;
	uint u_11_6;
	uint u_11_phi_80;
	uint u_12_1;
	uint u_12_2;
	uint u_12_5;
	uint u_12_6;
	uint u_12_7;
	uint u_12_8;
	uint u_12_9;
	uint u_12_phi_51;
	uint u_13_3;
	uint u_13_7;
	uint u_13_8;
	uint u_13_phi_78;
	uint u_14_1;
	uint u_14_3;
	uint u_14_4;
	uint u_14_phi_21;
	uint u_15_1;
	uint u_15_2;
	uint u_15_3;
	uint u_16_3;
	uint u_17_1;
	uint u_17_2;
	uint u_17_7;
	uint u_18_0;
	uint u_18_1;
	uint u_18_11;
	uint u_18_4;
	uint u_19_15;
	uint u_19_16;
	uint u_19_17;
	uint u_19_22;
	uint u_19_23;
	uint u_19_3;
	uint u_19_4;
	uint u_19_9;
	uint u_19_phi_53;
	uint u_2_1;
	uint u_2_10;
	uint u_2_16;
	uint u_2_18;
	uint u_2_19;
	uint u_2_2;
	uint u_2_21;
	uint u_2_22;
	uint u_2_25;
	uint u_2_26;
	uint u_2_28;
	uint u_2_29;
	uint u_2_32;
	uint u_2_6;
	uint u_2_8;
	uint u_2_9;
	uint u_2_phi_4;
	uint u_2_phi_55;
	uint u_2_phi_69;
	uint u_2_phi_74;
	uint u_2_phi_76;
	uint u_20_14;
	uint u_20_15;
	uint u_20_16;
	uint u_20_3;
	uint u_20_4;
	uint u_20_phi_26;
	uint u_21_0;
	uint u_21_16;
	uint u_22_0;
	uint u_22_1;
	uint u_22_10;
	uint u_22_11;
	uint u_22_12;
	uint u_22_2;
	uint u_22_3;
	uint u_22_6;
	uint u_22_9;
	uint u_22_phi_17;
	uint u_22_phi_31;
	uint u_22_phi_48;
	uint u_22_phi_50;
	uint u_23_1;
	uint u_23_2;
	uint u_23_3;
	uint u_23_phi_22;
	uint u_25_11;
	uint u_25_12;
	uint u_25_2;
	uint u_25_6;
	uint u_25_7;
	uint u_25_phi_49;
	uint u_25_phi_54;
	uint u_26_1;
	uint u_26_6;
	uint u_27_2;
	uint u_27_4;
	uint u_27_5;
	uint u_27_phi_25;
	uint u_28_2;
	uint u_28_3;
	uint u_28_phi_23;
	uint u_29_1;
	uint u_29_2;
	uint u_29_phi_24;
	uint u_3_2;
	uint u_3_23;
	uint u_3_3;
	uint u_3_33;
	uint u_3_34;
	uint u_3_36;
	uint u_3_37;
	uint u_3_38;
	uint u_3_39;
	uint u_3_4;
	uint u_3_40;
	uint u_3_41;
	uint u_3_42;
	uint u_3_43;
	uint u_3_45;
	uint u_3_46;
	uint u_3_49;
	uint u_3_5;
	uint u_3_50;
	uint u_3_phi_15;
	uint u_3_phi_20;
	uint u_3_phi_58;
	uint u_3_phi_66;
	uint u_3_phi_68;
	uint u_3_phi_70;
	uint u_3_phi_75;
	uint u_3_phi_9;
	uint u_30_5;
	uint u_30_6;
	uint u_30_phi_27;
	uint u_31_1;
	uint u_31_2;
	uint u_31_3;
	uint u_31_5;
	uint u_31_6;
	uint u_31_7;
	uint u_31_8;
	uint u_31_phi_28;
	uint u_31_phi_37;
	uint u_32_10;
	uint u_32_2;
	uint u_32_8;
	uint u_32_9;
	uint u_33_3;
	uint u_33_8;
	uint u_34_10;
	uint u_34_11;
	uint u_34_12;
	uint u_34_9;
	uint u_34_phi_35;
	uint u_34_phi_42;
	uint u_35_10;
	uint u_36_10;
	uint u_36_11;
	uint u_36_2;
	uint u_36_3;
	uint u_36_9;
	uint u_36_phi_29;
	uint u_36_phi_38;
	uint u_37_0;
	uint u_37_1;
	uint u_37_3;
	uint u_37_phi_30;
	uint u_38_0;
	uint u_38_1;
	uint u_38_2;
	uint u_38_3;
	uint u_38_phi_30;
	uint u_38_phi_39;
	uint u_39_0;
	uint u_39_1;
	uint u_39_5;
	uint u_39_6;
	uint u_39_phi_30;
	uint u_39_phi_40;
	uint u_4_0;
	uint u_4_1;
	uint u_4_15;
	uint u_4_21;
	uint u_4_22;
	uint u_4_24;
	uint u_4_25;
	uint u_4_27;
	uint u_4_30;
	uint u_4_32;
	uint u_4_34;
	uint u_4_35;
	uint u_4_37;
	uint u_4_38;
	uint u_4_43;
	uint u_4_44;
	uint u_4_phi_2;
	uint u_4_phi_56;
	uint u_4_phi_60;
	uint u_4_phi_71;
	uint u_40_0;
	uint u_40_1;
	uint u_40_10;
	uint u_40_11;
	uint u_40_3;
	uint u_40_phi_30;
	uint u_40_phi_41;
	uint u_41_0;
	uint u_41_1;
	uint u_41_11;
	uint u_41_12;
	uint u_41_8;
	uint u_41_phi_32;
	uint u_41_phi_41;
	uint u_42_0;
	uint u_42_1;
	uint u_42_3;
	uint u_42_4;
	uint u_42_phi_33;
	uint u_42_phi_41;
	uint u_43_0;
	uint u_43_1;
	uint u_43_2;
	uint u_43_3;
	uint u_43_phi_34;
	uint u_43_phi_41;
	uint u_44_0;
	uint u_44_1;
	uint u_44_phi_41;
	uint u_45_0;
	uint u_45_1;
	uint u_45_phi_44;
	uint u_46_0;
	uint u_46_1;
	uint u_46_phi_45;
	uint u_5_5;
	uint u_5_6;
	uint u_5_7;
	uint u_5_8;
	uint u_5_phi_43;
	uint u_6_2;
	uint u_6_4;
	uint u_6_5;
	uint u_6_phi_72;
	uint u_7_11;
	uint u_7_12;
	uint u_7_23;
	uint u_7_24;
	uint u_7_26;
	uint u_7_29;
	uint u_7_31;
	uint u_7_33;
	uint u_7_34;
	uint u_7_36;
	uint u_7_37;
	uint u_7_39;
	uint u_7_40;
	uint u_7_6;
	uint u_7_8;
	uint u_7_9;
	uint u_7_phi_47;
	uint u_7_phi_52;
	uint u_7_phi_59;
	uint u_7_phi_62;
	uint u_7_phi_64;
	uint u_8_18;
	uint u_8_21;
	uint u_8_22;
	uint u_8_23;
	uint u_8_24;
	uint u_8_9;
	uint u_8_phi_63;
	uint u_8_phi_65;
	uint u_9_13;
	uint u_9_14;
	uint u_9_6;
	uint u_9_7;
	uint u_9_8;
	uint u_9_9;
	uint u_9_phi_61;
	// -122.32173  <=>  float(-122.32173)
	o.vertex.x = float(-122.32173);
	// 109.928  <=>  float(109.92797)
	o.vertex.y = float(109.92797);
	// 165.2515  <=>  float(165.25153)
	o.vertex.z = float(165.25153);
	// 165.4502  <=>  float(165.4502)
	o.vertex.w = float(165.4502);
	// 0.89921  <=>  float(0.89921)
	o.fs_attr0.x = float(0.89921);
	// 0.89488  <=>  float(0.89488)
	o.fs_attr0.y = float(0.89488);
	// 0.8207  <=>  float(0.8207)
	o.fs_attr0.z = float(0.8207);
	// 0.10  <=>  float(0.10)
	o.fs_attr0.w = float(0.10);
	// 0.43533  <=>  float(0.43533)
	o.fs_attr1.x = float(0.43533);
	// 0.50728  <=>  float(0.50728)
	o.fs_attr1.y = float(0.50728);
	// 0.53254  <=>  float(0.53254)
	o.fs_attr1.z = float(0.53254);
	// 0.14317  <=>  float(0.14317)
	o.fs_attr1.w = float(0.14317);
	// 0.81006  <=>  float(0.81006)
	o.fs_attr2.x = float(0.81006);
	// 3.93004  <=>  float(3.93004)
	o.fs_attr2.y = float(3.93004);
	// -0.26864  <=>  float(-0.26864)
	o.fs_attr2.z = float(-0.26864);
	// 0.98535  <=>  float(0.98535)
	o.fs_attr2.w = float(0.98535);
	// 0.89506  <=>  float(0.89506)
	o.fs_attr3.x = float(0.89506);
	// 0.85918  <=>  float(0.85918)
	o.fs_attr3.y = float(0.85918);
	// 0  <=>  float(0.00)
	o.fs_attr3.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr3.w = float(1.00);
	// 21.56423  <=>  float(21.56423)
	o.fs_attr4.x = float(21.56423);
	// 27.76111  <=>  float(27.76111)
	o.fs_attr4.y = float(27.76111);
	// 165.3509  <=>  float(165.35086)
	o.fs_attr4.z = float(165.35086);
	// 165.4502  <=>  float(165.4502)
	o.fs_attr4.w = float(165.4502);
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
	// 0.63252  <=>  float(0.63252)
	o.fs_attr10.x = float(0.63252);
	// 0  <=>  float(0.00)
	o.fs_attr10.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr10.z = float(0.00);
	// 0.4995  <=>  float(0.4995)
	o.fs_attr10.w = float(0.4995);
	// 0.06766  <=>  float(0.06766)
	o.fs_attr11.x = float(0.06766);
	// 0.06766  <=>  float(0.06766)
	o.fs_attr11.y = float(0.06766);
	// 0.06766  <=>  float(0.06766)
	o.fs_attr11.z = float(0.06766);
	// 1.00  <=>  float(1.00)
	o.fs_attr11.w = float(1.00);
	// 0.00111  <=>  float(0.00111)
	o.fs_attr12.x = float(0.00111);
	// 0.00877  <=>  float(0.00877)
	o.fs_attr12.y = float(0.00877);
	// 0.00901  <=>  float(0.00901)
	o.fs_attr12.z = float(0.00901);
	// 0.11393  <=>  float(0.11393)
	o.fs_attr12.w = float(0.11393);
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_4_0 = 0u;
	u_4_phi_2 = u_4_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_4_1 = ftou(vs_cbuf8_30.y);
		u_4_phi_2 = u_4_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_4_phi_2 : 0}
	u_2_1 = u_4_phi_2;
	u_2_phi_4 = u_2_1;
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_4_phi_2) : 0} * 5.f)) : 0}
		u_2_2 = ftou((utof(u_4_phi_2) * 5.f));
		u_2_phi_4 = u_2_2;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr6.x = 0.f;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {utof(u_2_phi_4) : 0}
		o.vertex.z = utof(u_2_phi_4);
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		return;
	}
	// 335.00  <=>  ((0.f - {i.vao_attr4.w : 544.50}) + {(vs_cbuf10_2.x) : 879.50})
	pf_0_1 = ((0.f - i.vao_attr4.w) + (vs_cbuf10_2.x));
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_3_2 = 0u;
	u_3_phi_9 = u_3_2;
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_3_3 = ftou(vs_cbuf8_30.y);
		u_3_phi_9 = u_3_3;
	}
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_3_phi_9 : 0}
	u_1_1 = u_3_phi_9;
	u_1_phi_11 = u_1_1;
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_3_phi_9) : 0} * 5.f)) : 0}
		u_1_2 = ftou((utof(u_3_phi_9) * 5.f));
		u_1_phi_11 = u_1_2;
	}
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr6.x = 0.f;
	}
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  {utof(u_1_phi_11) : 0}
		o.vertex.z = utof(u_1_phi_11);
	}
	// False  <=>  if(((((({pf_0_1 : 335.00} < 0.f) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(0.f))) || ((({pf_0_1 : 335.00} >= float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 335.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((pf_0_1 < 0.f) && (! myIsNaN(pf_0_1))) && (! myIsNaN(0.f))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		return;
	}
	// 0.31439  <=>  {i.vao_attr6.x : 0.31439}
	f_0_7 = i.vao_attr6.x;
	// 0.8992063  <=>  (({utof(vs_cbuf9[105].x) : 0.8174603} * {(vs_cbuf10_0.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.10})
	o.fs_attr0.x = ((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x));
	// 0.10  <=>  ({utof(vs_cbuf9[113].x) : 0.10} * {(vs_cbuf10_0.w) : 1.00})
	o.fs_attr0.w = (utof(vs_cbuf9[113].x) * (vs_cbuf10_0.w));
	// 0.8948796  <=>  (({utof(vs_cbuf9[105].y) : 0.8135269} * {(vs_cbuf10_0.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.10})
	o.fs_attr0.y = ((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x));
	// 0.8207042  <=>  (({utof(vs_cbuf9[105].z) : 0.7460947} * {(vs_cbuf10_0.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.10})
	o.fs_attr0.z = ((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x));
	// 0  <=>  0u
	u_3_4 = 0u;
	u_3_phi_15 = u_3_4;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[11].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].w) : 0}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[11].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].w)))) ? true : false))
	{
		// ∞  <=>  (((({f_0_7 : 0.31439} * {utof(vs_cbuf9[13].x) : 0}) * {utof(vs_cbuf9[11].w) : 0}) + {pf_0_1 : 335.00}) * (1.0f / {utof(vs_cbuf9[11].w) : 0}))
		pf_3_4 = ((((f_0_7 * utof(vs_cbuf9[13].x)) * utof(vs_cbuf9[11].w)) + pf_0_1) * (1.0f / utof(vs_cbuf9[11].w)));
		// 4290772992  <=>  {ftou(({pf_3_4 : ∞} + (0.f - floor({pf_3_4 : ∞})))) : 4290772992}
		u_3_5 = ftou((pf_3_4 + (0.f - floor(pf_3_4))));
		u_3_phi_15 = u_3_5;
	}
	// 0  <=>  {u_3_phi_15 : 0}
	u_0_1 = u_3_phi_15;
	u_0_phi_16 = u_0_1;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[11].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].w) : 0})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[11].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].w))))) ? true : false))
	{
		// 1061586110  <=>  {ftou(({pf_0_1 : 335.00} * (1.0f / float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))))) : 1061586110}
		u_0_2 = ftou((pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f)))))))));
		u_0_phi_16 = u_0_2;
	}
	// 0.13457  <=>  {i.vao_attr6.y : 0.13457}
	f_7_0 = i.vao_attr6.y;
	// 0.96409  <=>  {i.vao_attr6.z : 0.96409}
	f_2_12 = i.vao_attr6.z;
	// 0  <=>  floor(({f_0_7 : 0.31439} * 2.f))
	f_5_12 = floor((f_0_7 * 2.f));
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 268435456u)
	u_4_15 = (vs_cbuf9_7_x & 268435456u);
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 1073741824u)
	u_7_6 = (vs_cbuf9_7_x & 1073741824u);
	// 0  <=>  ({vs_cbuf9_7_x : 9} & 536870912u)
	u_3_23 = (vs_cbuf9_7_x & 536870912u);
	// 0.775463  <=>  ({pf_0_1 : 335.00} * (1.0f / float(int((myIsNaN({i.vao_attr3.w : 432.00}) ? 0u : int(clamp(trunc({i.vao_attr3.w : 432.00}), float(-2147483600.f), float(2147483600.f))))))))
	pf_20_0 = (pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr3.w) ? 0u : int(clamp(trunc(i.vao_attr3.w), float(-2147483600.f), float(2147483600.f))))))));
	// 1.00  <=>  floor(({f_2_12 : 0.96409} * 2.f))
	f_1_35 = floor((f_2_12 * 2.f));
	// 0  <=>  floor(({f_7_0 : 0.13457} * 2.f))
	f_5_15 = floor((f_7_0 * 2.f));
	// -3668.575  <=>  ((({i.vao_attr3.z : -177.62726} * {i.vao_attr10.z : -0.74242}) + (({i.vao_attr3.y : -22.76068} * {i.vao_attr10.y : 0}) + ({i.vao_attr3.x : -100.61051} * {i.vao_attr10.x : 0.66993}))) + {i.vao_attr10.w : -3733.0469})
	pf_5_3 = (((i.vao_attr3.z * i.vao_attr10.z) + ((i.vao_attr3.y * i.vao_attr10.y) + (i.vao_attr3.x * i.vao_attr10.x))) + i.vao_attr10.w);
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_8_9 = (myIsNaN(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))));
	// 0.31  <=>  {utof(vs_cbuf9[130].w) : 0.31}
	f_6_10 = utof(vs_cbuf9[130].w);
	// 1065353216  <=>  (((({utof(u_0_phi_16) : 0.775463} >= {f_6_10 : 0.31}) && (! myIsNaN({utof(u_0_phi_16) : 0.775463}))) && (! myIsNaN({f_6_10 : 0.31}))) ? 1065353216u : 0u)
	u_9_6 = ((((utof(u_0_phi_16) >= f_6_10) && (! myIsNaN(utof(u_0_phi_16)))) && (! myIsNaN(f_6_10))) ? 1065353216u : 0u);
	// 342.9746  <=>  ((({i.vao_attr3.z : -177.62726} * {i.vao_attr9.z : 0}) + (({i.vao_attr3.y : -22.76068} * {i.vao_attr9.y : 1.00}) + ({i.vao_attr3.x : -100.61051} * {i.vao_attr9.x : 0}))) + {i.vao_attr9.w : 365.7353})
	pf_6_3 = (((i.vao_attr3.z * i.vao_attr9.z) + ((i.vao_attr3.y * i.vao_attr9.y) + (i.vao_attr3.x * i.vao_attr9.x))) + i.vao_attr9.w);
	// -1725.571  <=>  ((({i.vao_attr3.z : -177.62726} * {i.vao_attr8.z : -0.66993}) + (({i.vao_attr3.y : -22.76068} * {i.vao_attr8.y : 0}) + ({i.vao_attr3.x : -100.61051} * {i.vao_attr8.x : -0.74242}))) + {i.vao_attr8.w : -1919.2642})
	pf_7_4 = (((i.vao_attr3.z * i.vao_attr8.z) + ((i.vao_attr3.y * i.vao_attr8.y) + (i.vao_attr3.x * i.vao_attr8.x))) + i.vao_attr8.w);
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[83].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[83].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_11_2 = (myIsNaN(utof(vs_cbuf9[83].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[83].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[88].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[88].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_10_4 = (myIsNaN(utof(vs_cbuf9[88].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[88].z)), float(-2147483600.f), float(2147483600.f))));
	// 0  <=>  {utof(vs_cbuf9[129].w) : 0}
	f_5_18 = utof(vs_cbuf9[129].w);
	// 1065353216  <=>  (((({utof(u_0_phi_16) : 0.775463} >= {f_5_18 : 0}) && (! myIsNaN({utof(u_0_phi_16) : 0.775463}))) && (! myIsNaN({f_5_18 : 0}))) ? 1065353216u : 0u)
	u_12_1 = ((((utof(u_0_phi_16) >= f_5_18) && (! myIsNaN(utof(u_0_phi_16)))) && (! myIsNaN(f_5_18))) ? 1065353216u : 0u);
	// 0  <=>  {utof(vs_cbuf9[149].w) : 0}
	f_5_21 = utof(vs_cbuf9[149].w);
	// 1065353216  <=>  (((({pf_20_0 : 0.775463} >= {f_5_21 : 0}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_5_21 : 0}))) ? 1065353216u : 0u)
	u_18_0 = ((((pf_20_0 >= f_5_21) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_5_21))) ? 1065353216u : 0u);
	// 0.03  <=>  {utof(vs_cbuf9[150].w) : 0.03}
	f_5_23 = utof(vs_cbuf9[150].w);
	// 1065353216  <=>  (((({pf_20_0 : 0.775463} >= {f_5_23 : 0.03}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_5_23 : 0.03}))) ? 1065353216u : 0u)
	u_12_2 = ((((pf_20_0 >= f_5_23) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_5_23))) ? 1065353216u : 0u);
	// 0.65  <=>  {utof(vs_cbuf9[131].w) : 0.65}
	f_1_41 = utof(vs_cbuf9[131].w);
	// 1065353216  <=>  (((({utof(u_0_phi_16) : 0.775463} >= {f_1_41 : 0.65}) && (! myIsNaN({utof(u_0_phi_16) : 0.775463}))) && (! myIsNaN({f_1_41 : 0.65}))) ? 1065353216u : 0u)
	u_14_1 = ((((utof(u_0_phi_16) >= f_1_41) && (! myIsNaN(utof(u_0_phi_16)))) && (! myIsNaN(f_1_41))) ? 1065353216u : 0u);
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_8_9 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_16_3 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_8_9), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0.94  <=>  {utof(vs_cbuf9[151].w) : 0.94}
	f_1_42 = utof(vs_cbuf9[151].w);
	// 0  <=>  (((({pf_20_0 : 0.775463} >= {f_1_42 : 0.94}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_1_42 : 0.94}))) ? 1065353216u : 0u)
	u_18_1 = ((((pf_20_0 >= f_1_42) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_1_42))) ? 1065353216u : 0u);
	// 1.00  <=>  {utof(vs_cbuf9[152].w) : 1.00}
	f_8_22 = utof(vs_cbuf9[152].w);
	// 0  <=>  (((({pf_20_0 : 0.775463} >= {f_8_22 : 1.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_8_22 : 1.00}))) ? 1065353216u : 0u)
	u_9_7 = ((((pf_20_0 >= f_8_22) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_8_22))) ? 1065353216u : 0u);
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_11_2 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_13_3 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_11_2), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0.83  <=>  {utof(vs_cbuf9[132].w) : 0.83}
	f_6_29 = utof(vs_cbuf9[132].w);
	// 0  <=>  (((({utof(u_0_phi_16) : 0.775463} >= {f_6_29 : 0.83}) && (! myIsNaN({utof(u_0_phi_16) : 0.775463}))) && (! myIsNaN({f_6_29 : 0.83}))) ? 1065353216u : 0u)
	u_15_1 = ((((utof(u_0_phi_16) >= f_6_29) && (! myIsNaN(utof(u_0_phi_16)))) && (! myIsNaN(f_6_29))) ? 1065353216u : 0u);
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_8_9 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_21_0 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_8_9))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 0  <=>  0u
	u_22_0 = 0u;
	u_22_phi_17 = u_22_0;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 1  <=>  1u
		u_22_1 = 1u;
		u_22_phi_17 = u_22_1;
	}
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({f_0_7 : 0.31439} > 0.5f) && (! myIsNaN({f_0_7 : 0.31439}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((f_0_7 > 0.5f) && (! myIsNaN(f_0_7))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_2_12 : 0.96409} > 0.5f) && (! myIsNaN({f_2_12 : 0.96409}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_2_12 > 0.5f) && (! myIsNaN(f_2_12))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_3_33 = ftou(v.uv.y);
	u_3_phi_20 = u_3_33;
	// False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 0}) == 2u) ? 4294967295u : 0u)) | (~ (((({f_7_0 : 0.13457} > 0.5f) && (! myIsNaN({f_7_0 : 0.13457}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((f_7_0 > 0.5f) && (! myIsNaN(f_7_0))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_3_34 = ftou(((0.f - v.uv.y) + 1.f));
		u_3_phi_20 = u_3_34;
	}
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_14_3 = ftou(v.uv.y);
	u_14_phi_21 = u_14_3;
	// False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 0}) == 8u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr6.w : 0.82058} > 0.5f) && (! myIsNaN({i.vao_attr6.w : 0.82058}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr6.w > 0.5f) && (! myIsNaN(i.vao_attr6.w))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_14_4 = ftou(((0.f - v.uv.y) + 1.f));
		u_14_phi_21 = u_14_4;
	}
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_10_4 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_18_4 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_10_4), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1065107430  <=>  {ftou(v.uv.y) : 1065107430}
	u_23_1 = ftou(v.uv.y);
	u_23_phi_22 = u_23_1;
	// False  <=>  if(((! (((~ (((32u & {vs_cbuf9_7_y : 0}) == 32u) ? 4294967295u : 0u)) | (~ (((({f_2_12 : 0.96409} > 0.5f) && (! myIsNaN({f_2_12 : 0.96409}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((32u & vs_cbuf9_7_y) == 32u) ? 4294967295u : 0u)) | (~ ((((f_2_12 > 0.5f) && (! myIsNaN(f_2_12))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1013974656  <=>  {ftou(((0.f - {v.uv.y : 0.98535}) + 1.f)) : 1013974656}
		u_23_2 = ftou(((0.f - v.uv.y) + 1.f));
		u_23_phi_22 = u_23_2;
	}
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_11_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_17_1 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_11_2))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 0  <=>  clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_10_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f))
	f_4_31 = clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_10_4))) + 4294967294u)))), float(0.f), float(4294967300.f));
	// 0  <=>  uint({f_4_31 : 0})
	u_26_1 = uint(f_4_31);
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_0 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_21_0 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_27_2 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_0), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_21_0), int(0u), int(16u))), int(16u), int(16u));
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_28_2 = ftou(v.uv.x);
	u_28_phi_23 = u_28_2;
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 0}) == 1u) ? 4294967295u : 0u)) | (~ (((({f_0_7 : 0.31439} > 0.5f) && (! myIsNaN({f_0_7 : 0.31439}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((f_0_7 > 0.5f) && (! myIsNaN(f_0_7))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_28_3 = ftou(((0.f - v.uv.x) + 1.f));
		u_28_phi_23 = u_28_3;
	}
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_29_1 = ftou(v.uv.x);
	u_29_phi_24 = u_29_1;
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 0}) == 4u) ? 4294967295u : 0u)) | (~ (((({f_2_12 : 0.96409} > 0.5f) && (! myIsNaN({f_2_12 : 0.96409}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((f_2_12 > 0.5f) && (! myIsNaN(f_2_12))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_29_2 = ftou(((0.f - v.uv.x) + 1.f));
		u_29_phi_24 = u_29_2;
	}
	// 0  <=>  (({u_27_2 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_21_0 : 0}), int(0u), int(16u))))))
	u_25_2 = ((u_27_2 << 16u) + uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_21_0), int(0u), int(16u))))));
	// 1056759926  <=>  {ftou(v.uv.x) : 1056759926}
	u_27_4 = ftou(v.uv.x);
	u_27_phi_25 = u_27_4;
	// False  <=>  if(((! (((~ (((16u & {vs_cbuf9_7_y : 0}) == 16u) ? 4294967295u : 0u)) | (~ (((({f_7_0 : 0.13457} > 0.5f) && (! myIsNaN({f_7_0 : 0.13457}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((16u & vs_cbuf9_7_y) == 16u) ? 4294967295u : 0u)) | (~ ((((f_7_0 > 0.5f) && (! myIsNaN(f_7_0))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1057066949  <=>  {ftou(((0.f - {v.uv.x : 0.4939}) + 1.f)) : 1057066949}
		u_27_5 = ftou(((0.f - v.uv.x) + 1.f));
		u_27_phi_25 = u_27_5;
	}
	// 1.00  <=>  {utof(vs_cbuf9[133].w) : 1.00}
	f_1_47 = utof(vs_cbuf9[133].w);
	// 0  <=>  (((({utof(u_0_phi_16) : 0.775463} >= {f_1_47 : 1.00}) && (! myIsNaN({utof(u_0_phi_16) : 0.775463}))) && (! myIsNaN({f_1_47 : 1.00}))) ? 1065353216u : 0u)
	u_0_3 = ((((utof(u_0_phi_16) >= f_1_47) && (! myIsNaN(utof(u_0_phi_16)))) && (! myIsNaN(f_1_47))) ? 1065353216u : 0u);
	// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
	u_20_3 = ftou(f_0_7);
	u_20_phi_26 = u_20_3;
	// True  <=>  if((({u_22_phi_17 : 1} == 1u) ? true : false))
	if(((u_22_phi_17 == 1u) ? true : false))
	{
		// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
		u_20_4 = ftou(f_7_0);
		u_20_phi_26 = u_20_4;
	}
	// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
	u_30_5 = ftou(f_7_0);
	u_30_phi_27 = u_30_5;
	// True  <=>  if((({u_22_phi_17 : 1} == 1u) ? true : false))
	if(((u_22_phi_17 == 1u) ? true : false))
	{
		// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
		u_30_6 = ftou(f_2_12);
		u_30_phi_27 = u_30_6;
	}
	// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
	u_31_1 = ftou(f_0_7);
	u_31_phi_28 = u_31_1;
	// True  <=>  if((({u_22_phi_17 : 1} == 1u) ? true : false))
	if(((u_22_phi_17 == 1u) ? true : false))
	{
		// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
		u_31_2 = ftou(f_7_0);
		u_31_phi_28 = u_31_2;
	}
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_17_1 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_17_1 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_33_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_17_1), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_17_1), int(0u), int(16u))), int(16u), int(16u));
	// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
	u_36_2 = ftou(f_7_0);
	u_36_phi_29 = u_36_2;
	// True  <=>  if((({u_22_phi_17 : 1} == 1u) ? true : false))
	if(((u_22_phi_17 == 1u) ? true : false))
	{
		// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
		u_36_3 = ftou(f_2_12);
		u_36_phi_29 = u_36_3;
	}
	// 1064750746  <=>  {u_30_phi_27 : 1064750746}
	u_37_0 = u_30_phi_27;
	// 1040829624  <=>  {u_20_phi_26 : 1040829624}
	u_38_0 = u_20_phi_26;
	// 1040829624  <=>  {u_31_phi_28 : 1040829624}
	u_39_0 = u_31_phi_28;
	// 1064750746  <=>  {u_36_phi_29 : 1064750746}
	u_40_0 = u_36_phi_29;
	u_37_phi_30 = u_37_0;
	u_38_phi_30 = u_38_0;
	u_39_phi_30 = u_39_0;
	u_40_phi_30 = u_40_0;
	// False  <=>  if(((! ({u_22_phi_17 : 1} == 1u)) ? true : false))
	if(((! (u_22_phi_17 == 1u)) ? true : false))
	{
		// 1040829624  <=>  {u_20_phi_26 : 1040829624}
		u_22_2 = u_20_phi_26;
		u_22_phi_31 = u_22_2;
		// False  <=>  if((({u_22_phi_17 : 1} == 2u) ? true : false))
		if(((u_22_phi_17 == 2u) ? true : false))
		{
			// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
			u_22_3 = ftou(f_2_12);
			u_22_phi_31 = u_22_3;
		}
		// 1064750746  <=>  {u_30_phi_27 : 1064750746}
		u_41_0 = u_30_phi_27;
		u_41_phi_32 = u_41_0;
		// False  <=>  if((({u_22_phi_17 : 1} == 2u) ? true : false))
		if(((u_22_phi_17 == 2u) ? true : false))
		{
			// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
			u_41_1 = ftou(f_0_7);
			u_41_phi_32 = u_41_1;
		}
		// 1040829624  <=>  {u_31_phi_28 : 1040829624}
		u_42_0 = u_31_phi_28;
		u_42_phi_33 = u_42_0;
		// False  <=>  if((({u_22_phi_17 : 1} == 2u) ? true : false))
		if(((u_22_phi_17 == 2u) ? true : false))
		{
			// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
			u_42_1 = ftou(f_0_7);
			u_42_phi_33 = u_42_1;
		}
		// 1064750746  <=>  {u_36_phi_29 : 1064750746}
		u_43_0 = u_36_phi_29;
		u_43_phi_34 = u_43_0;
		// False  <=>  if((({u_22_phi_17 : 1} == 2u) ? true : false))
		if(((u_22_phi_17 == 2u) ? true : false))
		{
			// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
			u_43_1 = ftou(f_7_0);
			u_43_phi_34 = u_43_1;
		}
		// 1064750746  <=>  {u_41_phi_32 : 1064750746}
		u_37_1 = u_41_phi_32;
		// 1040829624  <=>  {u_22_phi_31 : 1040829624}
		u_38_1 = u_22_phi_31;
		// 1040829624  <=>  {u_42_phi_33 : 1040829624}
		u_39_1 = u_42_phi_33;
		// 1064750746  <=>  {u_43_phi_34 : 1064750746}
		u_40_1 = u_43_phi_34;
		u_37_phi_30 = u_37_1;
		u_38_phi_30 = u_38_1;
		u_39_phi_30 = u_39_1;
		u_40_phi_30 = u_40_1;
	}
	// 0  <=>  (({u_33_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_17_1 : 0}), int(0u), int(16u))))))
	u_22_6 = ((u_33_3 << 16u) + uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_17_1), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_26_1 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_26_1 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_31_3 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_26_1), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_26_1), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_26_1 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_26_1 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_26_1 : 0}), int(0u), int(16u))))))
	u_31_5 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_26_1), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_26_1), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_26_1), int(0u), int(16u))))));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_16_3 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_1_50 = utof((ftou((1.0f / float(u_16_3))) + 4294967294u));
	// 0  <=>  ((((((({f_0_7 : 0.31439} + {f_7_0 : 0.13457}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0}) * 2.f) + {utof(vs_cbuf9[195].x) : 0}) * (((0.f - {utof((((({f_1_35 : 1.00} < 0.f) && (! myIsNaN({f_1_35 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_1_35 : 1.00} > 0.f) && (! myIsNaN({f_1_35 : 1.00}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 1.00}) * float(int(abs(int((uint((int(0) - int(((int({u_4_15 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_4_15 : 0}) >= int(0u)) ? 0u : 1u)))))))))))
	pf_12_6 = (((((((f_0_7 + f_7_0) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)) * 2.f) + utof(vs_cbuf9[195].x)) * (((0.f - utof(((((f_1_35 < 0.f) && (! myIsNaN(f_1_35))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_1_35 > 0.f) && (! myIsNaN(f_1_35))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_4_15) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_4_15) >= int(0u)) ? 0u : 1u)))))))))));
	// 0  <=>  uint(clamp(trunc(({f_1_50 : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
	u_15_2 = uint(clamp(trunc((f_1_50 * float(0u))), float(0.f), float(4294967300.f)));
	// 5.00  <=>  {utof(vs_cbuf9[153].w) : 5.00}
	f_1_53 = utof(vs_cbuf9[153].w);
	// 0  <=>  (((({pf_20_0 : 0.775463} >= {f_1_53 : 5.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_1_53 : 5.00}))) ? 1065353216u : 0u)
	u_31_6 = ((((pf_20_0 >= f_1_53) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_1_53))) ? 1065353216u : 0u);
	// 0  <=>  ((((((({f_0_7 : 0.31439} + {f_2_12 : 0.96409}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].z) : 0}) * 2.f) + {utof(vs_cbuf9[195].z) : 0}) * (((0.f - {utof((((({f_5_15 : 0} < 0.f) && (! myIsNaN({f_5_15 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_5_15 : 0} > 0.f) && (! myIsNaN({f_5_15 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) * float(int(abs(int((uint((int(0) - int(((int({u_7_6 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_7_6 : 0}) >= int(0u)) ? 0u : 1u)))))))))))
	pf_13_7 = (((((((f_0_7 + f_2_12) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].z)) * 2.f) + utof(vs_cbuf9[195].z)) * (((0.f - utof(((((f_5_15 < 0.f) && (! myIsNaN(f_5_15))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_5_15 > 0.f) && (! myIsNaN(f_5_15))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_7_6) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_7_6) >= int(0u)) ? 0u : 1u)))))))))));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_13_3 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_1_54 = utof((ftou((1.0f / float(u_13_3))) + 4294967294u));
	// 0  <=>  uint(clamp(trunc((float(0u) * {f_1_54 : 0.9999999})), float(0.f), float(4294967300.f)))
	u_32_2 = uint(clamp(trunc((float(0u) * f_1_54)), float(0.f), float(4294967300.f)));
	// 0  <=>  ((((((({f_7_0 : 0.13457} + {f_2_12 : 0.96409}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].y) : 0}) * 2.f) + {utof(vs_cbuf9[195].y) : 0}) * (((0.f - {utof((((({f_5_12 : 0} < 0.f) && (! myIsNaN({f_5_12 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) + {utof((((({f_5_12 : 0} > 0.f) && (! myIsNaN({f_5_12 : 0}))) && (! myIsNaN(0.f))) ? 1065353216u : 0u)) : 0}) * float(int(abs(int((uint((int(0) - int(((int({u_3_23 : 0}) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int({u_3_23 : 0}) >= int(0u)) ? 0u : 1u)))))))))))
	pf_11_6 = (((((((f_7_0 + f_2_12) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].y)) * 2.f) + utof(vs_cbuf9[195].y)) * (((0.f - utof(((((f_5_12 < 0.f) && (! myIsNaN(f_5_12))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) + utof(((((f_5_12 > 0.f) && (! myIsNaN(f_5_12))) && (! myIsNaN(0.f))) ? 1065353216u : 0u))) * float(int(abs(int((uint((int(0) - int(((int(u_3_23) > int(0u)) ? 4294967295u : 0u)))) + uint((int(0) - int(((int(u_3_23) >= int(0u)) ? 0u : 1u)))))))))));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_18_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_1_58 = utof((ftou((1.0f / float(u_18_4))) + 4294967294u));
	// 0  <=>  uint(clamp(trunc((float(0u) * {f_1_58 : 0.9999999})), float(0.f), float(4294967300.f)))
	u_9_8 = uint(clamp(trunc((float(0u) * f_1_58)), float(0.f), float(4294967300.f)));
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_15_2 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_15_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_40_3 = uint((uint(bitfieldExtract(uint(u_15_2), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_15_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 0  <=>  0u
	u_34_9 = 0u;
	u_34_phi_35 = u_34_9;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 2  <=>  2u
		u_34_10 = 2u;
		u_34_phi_35 = u_34_10;
	}
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_32_2 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_32_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_41_8 = uint((uint(bitfieldExtract(uint(u_32_2), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_32_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 6.00  <=>  {utof(vs_cbuf9[154].w) : 6.00}
	f_1_65 = utof(vs_cbuf9[154].w);
	// 0  <=>  (((({pf_20_0 : 0.775463} >= {f_1_65 : 6.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_1_65 : 6.00}))) ? 1065353216u : 0u)
	u_36_9 = ((((pf_20_0 >= f_1_65) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_1_65))) ? 1065353216u : 0u);
	// 0  <=>  uint((uint(bitfieldExtract(uint({u_9_8 : 0}), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_8 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))))
	u_37_3 = uint((uint(bitfieldExtract(uint(u_9_8), int(16u), int(16u))) * uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_8), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))));
	// 7.00  <=>  {utof(vs_cbuf9[155].w) : 7.00}
	f_3_13 = utof(vs_cbuf9[155].w);
	// 0.1431713  <=>  (({utof(u_0_3) : 0} * {utof(vs_cbuf9[133].x) : 0}) + (((((((0.f - {utof(vs_cbuf9[132].x) : 0.075}) + {utof(vs_cbuf9[133].x) : 0}) * (1.0f / ((0.f - {utof(vs_cbuf9[132].w) : 0.83}) + {utof(vs_cbuf9[133].w) : 1.00}))) * ({utof(u_0_phi_16) : 0.775463} + (0.f - {utof(vs_cbuf9[132].w) : 0.83}))) + {utof(vs_cbuf9[132].x) : 0.075}) * (({utof(u_15_1) : 0} * (0.f - {utof(u_0_3) : 0})) + {utof(u_15_1) : 0})) + (((((({utof(vs_cbuf9[132].x) : 0.075} + (0.f - {utof(vs_cbuf9[131].x) : 0.30})) * (1.0f / ((0.f - {utof(vs_cbuf9[131].w) : 0.65}) + {utof(vs_cbuf9[132].w) : 0.83}))) * ({utof(u_0_phi_16) : 0.775463} + (0.f - {utof(vs_cbuf9[131].w) : 0.65}))) + {utof(vs_cbuf9[131].x) : 0.30}) * (({utof(u_14_1) : 1.00} * (0.f - {utof(u_15_1) : 0})) + {utof(u_14_1) : 1.00})) + (((((((0.f - {utof(vs_cbuf9[130].x) : 0.075}) + {utof(vs_cbuf9[131].x) : 0.30}) * (1.0f / ((0.f - {utof(vs_cbuf9[130].w) : 0.31}) + {utof(vs_cbuf9[131].w) : 0.65}))) * ({utof(u_0_phi_16) : 0.775463} + (0.f - {utof(vs_cbuf9[130].w) : 0.31}))) + {utof(vs_cbuf9[130].x) : 0.075}) * (({utof(u_9_6) : 1.00} * (0.f - {utof(u_14_1) : 1.00})) + {utof(u_9_6) : 1.00})) + ((((({utof(u_0_phi_16) : 0.775463} + (0.f - {utof(vs_cbuf9[129].w) : 0})) * (({utof(vs_cbuf9[130].x) : 0.075} + (0.f - {utof(vs_cbuf9[129].x) : 0})) * (1.0f / ((0.f - {utof(vs_cbuf9[129].w) : 0}) + {utof(vs_cbuf9[130].w) : 0.31})))) + {utof(vs_cbuf9[129].x) : 0}) * (({utof(u_9_6) : 1.00} * (0.f - {utof(u_12_1) : 1.00})) + {utof(u_12_1) : 1.00})) + (({utof(u_12_1) : 1.00} * (0.f - {utof(vs_cbuf9[129].x) : 0})) + {utof(vs_cbuf9[129].x) : 0}))))))
	pf_8_9 = ((utof(u_0_3) * utof(vs_cbuf9[133].x)) + (((((((0.f - utof(vs_cbuf9[132].x)) + utof(vs_cbuf9[133].x)) * (1.0f / ((0.f - utof(vs_cbuf9[132].w)) + utof(vs_cbuf9[133].w)))) * (utof(u_0_phi_16) + (0.f - utof(vs_cbuf9[132].w)))) + utof(vs_cbuf9[132].x)) * ((utof(u_15_1) * (0.f - utof(u_0_3))) + utof(u_15_1))) + ((((((utof(vs_cbuf9[132].x) + (0.f - utof(vs_cbuf9[131].x))) * (1.0f / ((0.f - utof(vs_cbuf9[131].w)) + utof(vs_cbuf9[132].w)))) * (utof(u_0_phi_16) + (0.f - utof(vs_cbuf9[131].w)))) + utof(vs_cbuf9[131].x)) * ((utof(u_14_1) * (0.f - utof(u_15_1))) + utof(u_14_1))) + (((((((0.f - utof(vs_cbuf9[130].x)) + utof(vs_cbuf9[131].x)) * (1.0f / ((0.f - utof(vs_cbuf9[130].w)) + utof(vs_cbuf9[131].w)))) * (utof(u_0_phi_16) + (0.f - utof(vs_cbuf9[130].w)))) + utof(vs_cbuf9[130].x)) * ((utof(u_9_6) * (0.f - utof(u_14_1))) + utof(u_9_6))) + (((((utof(u_0_phi_16) + (0.f - utof(vs_cbuf9[129].w))) * ((utof(vs_cbuf9[130].x) + (0.f - utof(vs_cbuf9[129].x))) * (1.0f / ((0.f - utof(vs_cbuf9[129].w)) + utof(vs_cbuf9[130].w))))) + utof(vs_cbuf9[129].x)) * ((utof(u_9_6) * (0.f - utof(u_12_1))) + utof(u_12_1))) + ((utof(u_12_1) * (0.f - utof(vs_cbuf9[129].x))) + utof(vs_cbuf9[129].x)))))));
	// 0  <=>  {utof(u_36_9) : 0}
	f_5_36 = utof(u_36_9);
	// 0  <=>  (0.f - {utof((((({pf_20_0 : 0.775463} >= {f_3_13 : 7.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_3_13 : 7.00}))) ? 1065353216u : 0u)) : 0})
	f_4_42 = (0.f - utof(((((pf_20_0 >= f_3_13) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_3_13))) ? 1065353216u : 0u)));
	// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
	u_0_4 = ftou(f_0_7);
	u_0_phi_36 = u_0_4;
	// False  <=>  if((({u_34_phi_35 : 2} == 1u) ? true : false))
	if(((u_34_phi_35 == 1u) ? true : false))
	{
		// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
		u_0_5 = ftou(f_7_0);
		u_0_phi_36 = u_0_5;
	}
	// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
	u_31_7 = ftou(f_7_0);
	u_31_phi_37 = u_31_7;
	// False  <=>  if((({u_34_phi_35 : 2} == 1u) ? true : false))
	if(((u_34_phi_35 == 1u) ? true : false))
	{
		// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
		u_31_8 = ftou(f_2_12);
		u_31_phi_37 = u_31_8;
	}
	// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
	u_36_10 = ftou(f_2_12);
	u_36_phi_38 = u_36_10;
	// False  <=>  if((({u_34_phi_35 : 2} == 1u) ? true : false))
	if(((u_34_phi_35 == 1u) ? true : false))
	{
		// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
		u_36_11 = ftou(f_0_7);
		u_36_phi_38 = u_36_11;
	}
	// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
	u_38_2 = ftou(f_0_7);
	u_38_phi_39 = u_38_2;
	// False  <=>  if((({u_34_phi_35 : 2} == 1u) ? true : false))
	if(((u_34_phi_35 == 1u) ? true : false))
	{
		// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
		u_38_3 = ftou(f_7_0);
		u_38_phi_39 = u_38_3;
	}
	// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
	u_39_5 = ftou(f_7_0);
	u_39_phi_40 = u_39_5;
	// False  <=>  if((({u_34_phi_35 : 2} == 1u) ? true : false))
	if(((u_34_phi_35 == 1u) ? true : false))
	{
		// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
		u_39_6 = ftou(f_2_12);
		u_39_phi_40 = u_39_6;
	}
	// 1064750746  <=>  {u_36_phi_38 : 1064750746}
	u_40_10 = u_36_phi_38;
	// 1040829624  <=>  {u_39_phi_40 : 1040829624}
	u_41_11 = u_39_phi_40;
	// 1050736570  <=>  {u_0_phi_36 : 1050736570}
	u_42_3 = u_0_phi_36;
	// 1050736570  <=>  {u_38_phi_39 : 1050736570}
	u_43_2 = u_38_phi_39;
	// 1040829624  <=>  {u_31_phi_37 : 1040829624}
	u_44_0 = u_31_phi_37;
	u_40_phi_41 = u_40_10;
	u_41_phi_41 = u_41_11;
	u_42_phi_41 = u_42_3;
	u_43_phi_41 = u_43_2;
	u_44_phi_41 = u_44_0;
	// True  <=>  if(((! ({u_34_phi_35 : 2} == 1u)) ? true : false))
	if(((! (u_34_phi_35 == 1u)) ? true : false))
	{
		// 1050736570  <=>  {u_0_phi_36 : 1050736570}
		u_34_11 = u_0_phi_36;
		u_34_phi_42 = u_34_11;
		// True  <=>  if((({u_34_phi_35 : 2} == 2u) ? true : false))
		if(((u_34_phi_35 == 2u) ? true : false))
		{
			// 1064750746  <=>  {ftou(f_2_12) : 1064750746}
			u_34_12 = ftou(f_2_12);
			u_34_phi_42 = u_34_12;
		}
		// 1040829624  <=>  {u_31_phi_37 : 1040829624}
		u_5_5 = u_31_phi_37;
		u_5_phi_43 = u_5_5;
		// True  <=>  if((({u_34_phi_35 : 2} == 2u) ? true : false))
		if(((u_34_phi_35 == 2u) ? true : false))
		{
			// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
			u_5_6 = ftou(f_0_7);
			u_5_phi_43 = u_5_6;
		}
		// 1064750746  <=>  {u_36_phi_38 : 1064750746}
		u_45_0 = u_36_phi_38;
		u_45_phi_44 = u_45_0;
		// True  <=>  if((({u_34_phi_35 : 2} == 2u) ? true : false))
		if(((u_34_phi_35 == 2u) ? true : false))
		{
			// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
			u_45_1 = ftou(f_7_0);
			u_45_phi_44 = u_45_1;
		}
		// 1050736570  <=>  {u_38_phi_39 : 1050736570}
		u_46_0 = u_38_phi_39;
		u_46_phi_45 = u_46_0;
		// True  <=>  if((({u_34_phi_35 : 2} == 2u) ? true : false))
		if(((u_34_phi_35 == 2u) ? true : false))
		{
			// 1050736570  <=>  {ftou(f_0_7) : 1050736570}
			u_46_1 = ftou(f_0_7);
			u_46_phi_45 = u_46_1;
		}
		// 1040829624  <=>  {u_39_phi_40 : 1040829624}
		u_1_4 = u_39_phi_40;
		u_1_phi_46 = u_1_4;
		// True  <=>  if((({u_34_phi_35 : 2} == 2u) ? true : false))
		if(((u_34_phi_35 == 2u) ? true : false))
		{
			// 1040829624  <=>  {ftou(f_7_0) : 1040829624}
			u_1_5 = ftou(f_7_0);
			u_1_phi_46 = u_1_5;
		}
		// 1040829624  <=>  {u_45_phi_44 : 1040829624}
		u_40_11 = u_45_phi_44;
		// 1040829624  <=>  {u_1_phi_46 : 1040829624}
		u_41_12 = u_1_phi_46;
		// 1064750746  <=>  {u_34_phi_42 : 1064750746}
		u_42_4 = u_34_phi_42;
		// 1050736570  <=>  {u_46_phi_45 : 1050736570}
		u_43_3 = u_46_phi_45;
		// 1050736570  <=>  {u_5_phi_43 : 1050736570}
		u_44_1 = u_5_phi_43;
		u_40_phi_41 = u_40_11;
		u_41_phi_41 = u_41_12;
		u_42_phi_41 = u_42_4;
		u_43_phi_41 = u_43_3;
		u_44_phi_41 = u_44_1;
	}
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_8_9 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_1_42 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_8_9), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_8_9 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_27_2 : 0}), int(16u), int(16u))))) << 16u) + {u_25_2 : 0}))))))), float(0.f), float(4294967300.f)))
	u_1_6 = uint(clamp(trunc((utof((ftou((1.0f / float(u_8_9))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_8_9), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_27_2), int(16u), int(16u))))) << 16u) + u_25_2))))))), float(0.f), float(4294967300.f)));
	// 1.00  <=>  (((((((0.f - {utof(vs_cbuf9[154].x) : 1.00}) + {utof(vs_cbuf9[155].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[154].w) : 6.00}) + {utof(vs_cbuf9[155].w) : 7.00}))) * ({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[154].w) : 6.00}))) + {utof(vs_cbuf9[154].x) : 1.00}) * (({f_5_36 : 0} * {f_4_42 : 0}) + {utof(u_36_9) : 0})) + (((((((0.f - {utof(vs_cbuf9[153].x) : 1.00}) + {utof(vs_cbuf9[154].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[153].w) : 5.00}) + {utof(vs_cbuf9[154].w) : 6.00}))) * ({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[153].w) : 5.00}))) + {utof(vs_cbuf9[153].x) : 1.00}) * (({utof(u_31_6) : 0} * (0.f - {utof(u_36_9) : 0})) + {utof(u_31_6) : 0})) + (((((((0.f - {utof(vs_cbuf9[152].x) : 1.00}) + {utof(vs_cbuf9[153].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[152].w) : 1.00}) + {utof(vs_cbuf9[153].w) : 5.00}))) * ({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[152].w) : 1.00}))) + {utof(vs_cbuf9[152].x) : 1.00}) * (({utof(u_9_7) : 0} * (0.f - {utof(u_31_6) : 0})) + {utof(u_9_7) : 0})) + (((((((0.f - {utof(vs_cbuf9[151].x) : 1.00}) + {utof(vs_cbuf9[152].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[151].w) : 0.94}) + {utof(vs_cbuf9[152].w) : 1.00}))) * ({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[151].w) : 0.94}))) + {utof(vs_cbuf9[151].x) : 1.00}) * (({utof(u_18_1) : 0} * (0.f - {utof(u_9_7) : 0})) + {utof(u_18_1) : 0})) + (((((((0.f - {utof(vs_cbuf9[150].x) : 1.00}) + {utof(vs_cbuf9[151].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[150].w) : 0.03}) + {utof(vs_cbuf9[151].w) : 0.94}))) * ({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[150].w) : 0.03}))) + {utof(vs_cbuf9[150].x) : 1.00}) * (({utof(u_12_2) : 1.00} * (0.f - {utof(u_18_1) : 0})) + {utof(u_12_2) : 1.00})) + ((((({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[149].w) : 0})) * (((0.f - {utof(vs_cbuf9[149].x) : 1.00}) + {utof(vs_cbuf9[150].x) : 1.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[149].w) : 0}) + {utof(vs_cbuf9[150].w) : 0.03})))) + {utof(vs_cbuf9[149].x) : 1.00}) * (({utof(u_18_0) : 1.00} * (0.f - {utof(u_12_2) : 1.00})) + {utof(u_18_0) : 1.00})) + (({utof(u_18_0) : 1.00} * (0.f - {utof(vs_cbuf9[149].x) : 1.00})) + {utof(vs_cbuf9[149].x) : 1.00})))))))
	pf_10_8 = (((((((0.f - utof(vs_cbuf9[154].x)) + utof(vs_cbuf9[155].x)) * (1.0f / ((0.f - utof(vs_cbuf9[154].w)) + utof(vs_cbuf9[155].w)))) * (pf_20_0 + (0.f - utof(vs_cbuf9[154].w)))) + utof(vs_cbuf9[154].x)) * ((f_5_36 * f_4_42) + utof(u_36_9))) + (((((((0.f - utof(vs_cbuf9[153].x)) + utof(vs_cbuf9[154].x)) * (1.0f / ((0.f - utof(vs_cbuf9[153].w)) + utof(vs_cbuf9[154].w)))) * (pf_20_0 + (0.f - utof(vs_cbuf9[153].w)))) + utof(vs_cbuf9[153].x)) * ((utof(u_31_6) * (0.f - utof(u_36_9))) + utof(u_31_6))) + (((((((0.f - utof(vs_cbuf9[152].x)) + utof(vs_cbuf9[153].x)) * (1.0f / ((0.f - utof(vs_cbuf9[152].w)) + utof(vs_cbuf9[153].w)))) * (pf_20_0 + (0.f - utof(vs_cbuf9[152].w)))) + utof(vs_cbuf9[152].x)) * ((utof(u_9_7) * (0.f - utof(u_31_6))) + utof(u_9_7))) + (((((((0.f - utof(vs_cbuf9[151].x)) + utof(vs_cbuf9[152].x)) * (1.0f / ((0.f - utof(vs_cbuf9[151].w)) + utof(vs_cbuf9[152].w)))) * (pf_20_0 + (0.f - utof(vs_cbuf9[151].w)))) + utof(vs_cbuf9[151].x)) * ((utof(u_18_1) * (0.f - utof(u_9_7))) + utof(u_18_1))) + (((((((0.f - utof(vs_cbuf9[150].x)) + utof(vs_cbuf9[151].x)) * (1.0f / ((0.f - utof(vs_cbuf9[150].w)) + utof(vs_cbuf9[151].w)))) * (pf_20_0 + (0.f - utof(vs_cbuf9[150].w)))) + utof(vs_cbuf9[150].x)) * ((utof(u_12_2) * (0.f - utof(u_18_1))) + utof(u_12_2))) + (((((pf_20_0 + (0.f - utof(vs_cbuf9[149].w))) * (((0.f - utof(vs_cbuf9[149].x)) + utof(vs_cbuf9[150].x)) * (1.0f / ((0.f - utof(vs_cbuf9[149].w)) + utof(vs_cbuf9[150].w))))) + utof(vs_cbuf9[149].x)) * ((utof(u_18_0) * (0.f - utof(u_12_2))) + utof(u_18_0))) + ((utof(u_18_0) * (0.f - utof(vs_cbuf9[149].x))) + utof(vs_cbuf9[149].x))))))));
	// 0  <=>  uint((int(0) - int((({u_41_8 : 0} << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_32_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_32_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_3 : 1}), int(0u), int(16u))))))))))
	u_5_7 = uint((int(0) - int(((u_41_8 << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_32_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_3), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_32_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_3), int(0u), int(16u))))))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(16u), int(16u))) * {u_31_3 : 0})) << 16u) + {u_31_5 : 0}))))
	u_4_21 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_10_4), int(16u), int(16u))) * u_31_3)) << 16u) + u_31_5))));
	// 0  <=>  uint((int(0) - int((({u_40_3 : 0} << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_15_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_15_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_3 : 1}), int(0u), int(16u))))))))))
	u_5_8 = uint((int(0) - int(((u_40_3 << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_15_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_3), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_15_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_3), int(0u), int(16u))))))))));
	// 0  <=>  uint((int(0) - int((({u_37_3 : 0} << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_8 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_8 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))))))))))
	u_4_22 = uint((int(0) - int(((u_37_3 << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_8), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_8), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))))))))));
	// 8.00  <=>  {utof(vs_cbuf9[156].w) : 8.00}
	f_6_33 = utof(vs_cbuf9[156].w);
	// 0  <=>  ({u_21_0 : 0} + {u_1_6 : 0})
	u_1_7 = (u_21_0 + u_1_6);
	// 0  <=>  {utof((((({pf_20_0 : 0.775463} >= {f_3_13 : 7.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_3_13 : 7.00}))) ? 1065353216u : 0u)) : 0}
	f_1_73 = utof(((((pf_20_0 >= f_3_13) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_3_13))) ? 1065353216u : 0u));
	// 0  <=>  {utof((((({pf_20_0 : 0.775463} >= {f_6_33 : 8.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_6_33 : 8.00}))) ? 1065353216u : 0u)) : 0}
	f_8_30 = utof(((((pf_20_0 >= f_6_33) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_6_33))) ? 1065353216u : 0u));
	// 0  <=>  {utof((((({pf_20_0 : 0.775463} >= {f_3_13 : 7.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_3_13 : 7.00}))) ? 1065353216u : 0u)) : 0}
	f_9_20 = utof(((((pf_20_0 >= f_3_13) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_3_13))) ? 1065353216u : 0u)); // maybe duplicate expression on the right side of the assignment, vars:(f_1_73, f_1_73)
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_13_3 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_1_74 = utof((ftou((1.0f / float(u_13_3))) + 4294967294u)); // maybe duplicate expression on the right side of the assignment, vars:(f_1_54, f_1_54)
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_8_9 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_13 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_8_9), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_3_5 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_8_9), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_18_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_1_78 = utof((ftou((1.0f / float(u_18_4))) + 4294967294u)); // maybe duplicate expression on the right side of the assignment, vars:(f_1_58, f_1_58)
	// 1065353214  <=>  ({ftou((1.0f / float({u_18_4 : 1}))) : 1065353216} + 4294967294u)
	u_7_8 = (ftou((1.0f / float(u_18_4))) + 4294967294u);
	u_7_phi_47 = u_7_8;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(log2(abs({utof(vs_cbuf9[195].w) : 1.00}))) : 0}
		u_7_9 = ftou(log2(abs(utof(vs_cbuf9[195].w))));
		u_7_phi_47 = u_7_9;
	}
	// 0  <=>  clamp(trunc(({utof(({ftou((1.0f / float({u_10_4 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float({u_4_21 : 0}))), float(0.f), float(4294967300.f))
	f_1_87 = clamp(trunc((utof((ftou((1.0f / float(u_10_4))) + 4294967294u)) * float(u_4_21))), float(0.f), float(4294967300.f));
	// 0.9999999  <=>  {utof(({ftou((1.0f / float({u_16_3 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999}
	f_1_92 = utof((ftou((1.0f / float(u_16_3))) + 4294967294u)); // maybe duplicate expression on the right side of the assignment, vars:(f_1_50, f_1_50)
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_11_2 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_33_3 : 0}), int(16u), int(16u))))) << 16u) + {u_22_6 : 0}))))))), float(0.f), float(4294967300.f)))
	u_12_5 = uint(clamp(trunc((utof((ftou((1.0f / float(u_11_2))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_11_2), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_33_3), int(16u), int(16u))))) << 16u) + u_22_6))))))), float(0.f), float(4294967300.f)));
	// 0  <=>  {u_7_phi_47 : 0}
	u_22_9 = u_7_phi_47;
	u_22_phi_48 = u_22_9;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {ftou(({pf_0_1 : 335.00} * {utof(u_7_phi_47) : 0})) : 0}
		u_22_10 = ftou((pf_0_1 * utof(u_7_phi_47)));
		u_22_phi_48 = u_22_10;
	}
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_44 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_8_9), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_11_2 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_5_6 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_11_2), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  {u_22_phi_48 : 0}
	u_25_6 = u_22_phi_48;
	u_25_phi_49 = u_25_6;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 0  <=>  {u_22_phi_48 : 0}
		u_25_7 = u_22_phi_48;
		u_25_phi_49 = u_25_7;
	}
	// 0  <=>  ({u_17_1 : 0} + {u_12_5 : 0})
	u_17_2 = (u_17_1 + u_12_5);
	// 0  <=>  {u_12_5 : 0}
	u_22_11 = u_12_5;
	u_22_phi_50 = u_22_11;
	// True  <=>  if(((! (((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00})))) ? true : false))
	if(((! (((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w))))) ? true : false))
	{
		// 1.00  <=>  exp2({utof(u_25_phi_49) : 0})
		f_1_102 = exp2(utof(u_25_phi_49));
		// 1065353216  <=>  {ftou(f_1_102) : 1065353216}
		u_22_12 = ftou(f_1_102);
		u_22_phi_50 = u_22_12;
	}
	// 1067869798  <=>  {ftou(({utof(vs_cbuf9[76].w) : 0.05} + {utof(vs_cbuf9[76].y) : 1.25})) : 1067869798}
	u_12_6 = ftou((utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)));
	u_12_phi_51 = u_12_6;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 0  <=>  {ftou(((0.f - {utof(vs_cbuf9[195].w) : 1.00}) + 1.f)) : 0}
		u_12_7 = ftou(((0.f - utof(vs_cbuf9[195].w)) + 1.f));
		u_12_phi_51 = u_12_7;
	}
	// 1067869798  <=>  {u_12_phi_51 : 1067869798}
	u_7_11 = u_12_phi_51;
	u_7_phi_52 = u_7_11;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1061481551  <=>  {ftou((1.0f / {utof(u_12_phi_51) : 1.30})) : 1061481551}
		u_7_12 = ftou((1.0f / utof(u_12_phi_51)));
		u_7_phi_52 = u_7_12;
	}
	// 0  <=>  ({u_26_1 : 0} + uint({f_1_87 : 0}))
	u_12_8 = (u_26_1 + uint(f_1_87));
	// 1065353216  <=>  {u_22_phi_50 : 1065353216}
	u_19_3 = u_22_phi_50;
	u_19_phi_53 = u_19_3;
	// False  <=>  if(((((0.f == {utof(vs_cbuf9[195].w) : 1.00}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) ? true : false))
	if(((((0.f == utof(vs_cbuf9[195].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[195].w)))) ? true : false))
	{
		// 1065353216  <=>  1065353216u
		u_19_4 = 1065353216u;
		u_19_phi_53 = u_19_4;
	}
	// 1135050752  <=>  {ftou(pf_0_1) : 1135050752}
	u_25_11 = ftou(pf_0_1);
	u_25_phi_54 = u_25_11;
	// False  <=>  if(((! ((({utof(vs_cbuf9[195].w) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[195].w) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[195].w) == 1.f) && (! myIsNaN(utof(vs_cbuf9[195].w)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_7_phi_52) : 1.30} * (0.f - {utof(u_19_phi_53) : 1.00})) + {utof(u_7_phi_52) : 1.30})) : 0}
		u_25_12 = ftou(((utof(u_7_phi_52) * (0.f - utof(u_19_phi_53))) + utof(u_7_phi_52)));
		u_25_phi_54 = u_25_12;
	}
	// 0  <=>  ({u_15_2 : 0} + uint(clamp(trunc(({f_1_92 : 0.9999999} * float({u_5_8 : 0}))), float(0.f), float(4294967300.f))))
	u_15_3 = (u_15_2 + uint(clamp(trunc((f_1_92 * float(u_5_8))), float(0.f), float(4294967300.f))));
	// 0  <=>  ({u_32_2 : 0} + uint(clamp(trunc(({f_1_74 : 0.9999999} * float({u_5_7 : 0}))), float(0.f), float(4294967300.f))))
	u_6_2 = (u_32_2 + uint(clamp(trunc((f_1_74 * float(u_5_7))), float(0.f), float(4294967300.f))));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_11_2 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_2_18 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_11_2), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_4_6 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_11_2), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_1_7 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_1_7 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_33_8 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_1_7), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_1_7), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_1_7 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_1_7 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_1_7 : 0}), int(0u), int(16u))))))
	u_2_6 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_1_7), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_1_7), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_1_7), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_17_2 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_17_2 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_32_8 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_17_2), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_17_2), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_17_2 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_17_2 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_17_2 : 0}), int(0u), int(16u))))))
	u_19_9 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_17_2), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_17_2), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_17_2), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_12_8 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_12_8 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_32_9 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_12_8), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_12_8), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_12_8 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_12_8 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_12_8 : 0}), int(0u), int(16u))))))
	u_26_6 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_12_8), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_12_8), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_12_8), int(0u), int(16u))))));
	// -7.1287136  <=>  (0.f - (({pf_0_1 : 335.00} * {utof(vs_cbuf9[87].x) : 0}) + (({utof(u_40_phi_41) : 0.13457} * {utof(vs_cbuf9[87].z) : 6.283185}) + ({utof(vs_cbuf9[87].z) : 6.283185} + {utof(vs_cbuf9[87].y) : 0}))))
	f_0_8 = (0.f - ((pf_0_1 * utof(vs_cbuf9[87].x)) + ((utof(u_40_phi_41) * utof(vs_cbuf9[87].z)) + (utof(vs_cbuf9[87].z) + utof(vs_cbuf9[87].y)))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_15_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_32_10 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_15_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_15_3 : 0}), int(16u), int(16u))) * {u_32_10 : 1})) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_15_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_15_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_3 : 1}), int(0u), int(16u)))))))
	u_19_15 = ((uint((uint(bitfieldExtract(uint(u_15_3), int(16u), int(16u))) * u_32_10)) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_15_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_3), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_15_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_3), int(0u), int(16u)))))));
	// 0  <=>  ({u_9_8 : 0} + uint(clamp(trunc(({f_1_78 : 0.9999999} * float({u_4_22 : 0}))), float(0.f), float(4294967300.f))))
	u_9_9 = (u_9_8 + uint(clamp(trunc((f_1_78 * float(u_4_22))), float(0.f), float(4294967300.f))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(16u), int(16u))) * {u_33_8 : 0})) << 16u) + {u_2_6 : 0}))))
	u_2_8 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_8_9), int(16u), int(16u))) * u_33_8)) << 16u) + u_2_6))));
	// 0  <=>  uint((int(0) - int({u_19_15 : 0})))
	u_19_16 = uint((int(0) - int(u_19_15)));
	// 0  <=>  (((({pf_12_6 : 0} * -2.f) + (((((({f_0_7 : 0.31439} + {f_7_0 : 0.13457}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].x) : 0}) * 2.f) + {utof(vs_cbuf9[195].x) : 0})) * {utof(u_25_phi_54) : 335.00}) + (({f_0_7 : 0.31439} + -0.5f) * {utof(vs_cbuf9[194].x) : 0}))
	pf_12_8 = ((((pf_12_6 * -2.f) + ((((((f_0_7 + f_7_0) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].x)) * 2.f) + utof(vs_cbuf9[195].x))) * utof(u_25_phi_54)) + ((f_0_7 + -0.5f) * utof(vs_cbuf9[194].x)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_3_8 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_11_2), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[88].w) : 1.00}) * {utof(vs_cbuf9[88].y) : 1.00})
	pf_19_9 = ((1.0f / utof(vs_cbuf9[88].w)) * utof(vs_cbuf9[88].y));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_5_7 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_10_4), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_3 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_35_10 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_3), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_6_2 : 0}), int(16u), int(16u))) * {u_35_10 : 1})) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_6_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_3 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_3 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_6_2 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_3 : 1}), int(0u), int(16u)))))))
	u_20_14 = ((uint((uint(bitfieldExtract(uint(u_6_2), int(16u), int(16u))) * u_35_10)) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_6_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_3), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_3), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_6_2), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_3), int(0u), int(16u)))))));
	// 0  <=>  {utof((((({pf_20_0 : 0.775463} >= {f_6_33 : 8.00}) && (! myIsNaN({pf_20_0 : 0.775463}))) && (! myIsNaN({f_6_33 : 8.00}))) ? 1065353216u : 0u)) : 0}
	f_5_41 = utof(((((pf_20_0 >= f_6_33) && (! myIsNaN(pf_20_0))) && (! myIsNaN(f_6_33))) ? 1065353216u : 0u)); // maybe duplicate expression on the right side of the assignment, vars:(f_8_30, f_8_30)
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[88].z) : 1.00}) * {utof(vs_cbuf9[88].x) : 1.00})
	pf_23_12 = ((1.0f / utof(vs_cbuf9[88].z)) * utof(vs_cbuf9[88].x));
	// 1.00  <=>  (({f_5_41 : 0} * {utof(vs_cbuf9[156].x) : 1.00}) + (((((({utof(vs_cbuf9[156].x) : 1.00} + (0.f - {utof(vs_cbuf9[155].x) : 1.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[155].w) : 7.00}) + {utof(vs_cbuf9[156].w) : 8.00}))) * ({pf_20_0 : 0.775463} + (0.f - {utof(vs_cbuf9[155].w) : 7.00}))) + {utof(vs_cbuf9[155].x) : 1.00}) * (({f_9_20 : 0} * (0.f - {f_8_30 : 0})) + {f_1_73 : 0})) + {pf_10_8 : 1.00}))
	o.fs_attr5.x = ((f_5_41 * utof(vs_cbuf9[156].x)) + ((((((utof(vs_cbuf9[156].x) + (0.f - utof(vs_cbuf9[155].x))) * (1.0f / ((0.f - utof(vs_cbuf9[155].w)) + utof(vs_cbuf9[156].w)))) * (pf_20_0 + (0.f - utof(vs_cbuf9[155].w)))) + utof(vs_cbuf9[155].x)) * ((f_9_20 * (0.f - f_8_30)) + f_1_73)) + pf_10_8));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(16u), int(16u))) * {u_32_8 : 0})) << 16u) + {u_19_9 : 0}))))
	u_4_24 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_11_2), int(16u), int(16u))) * u_32_8)) << 16u) + u_19_9))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(16u), int(16u))) * {u_32_9 : 0})) << 16u) + {u_26_6 : 0}))))
	u_7_23 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_10_4), int(16u), int(16u))) * u_32_9)) << 16u) + u_26_6))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_9 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_23_3 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_9_9 : 0}), int(16u), int(16u))) * {u_23_3 : 1})) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_9 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_9 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_4 : 1}), int(0u), int(16u)))))))
	u_21_16 = ((uint((uint(bitfieldExtract(uint(u_9_9), int(16u), int(16u))) * u_23_3)) << 16u) + ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_4), int(0u), int(16u)))))));
	// 0  <=>  ((uint({u_2_8 : 0}) >= uint({u_8_9 : 1})) ? 4294967295u : 0u)
	u_2_9 = ((uint(u_2_8) >= uint(u_8_9)) ? 4294967295u : 0u);
	// 0  <=>  uint((int(0) - int({u_20_14 : 0})))
	u_19_17 = uint((int(0) - int(u_20_14)));
	// 0  <=>  ((uint({u_4_24 : 0}) >= uint({u_11_2 : 1})) ? 4294967295u : 0u)
	u_4_25 = ((uint(u_4_24) >= uint(u_11_2)) ? 4294967295u : 0u);
	// 0  <=>  uint((int(0) - int({u_21_16 : 0})))
	u_2_10 = uint((int(0) - int(u_21_16)));
	// 0  <=>  (((({pf_13_7 : 0} * -2.f) + (((((({f_0_7 : 0.31439} + {f_2_12 : 0.96409}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].z) : 0}) * 2.f) + {utof(vs_cbuf9[195].z) : 0})) * {utof(u_25_phi_54) : 335.00}) + (({f_2_12 : 0.96409} + -0.5f) * {utof(vs_cbuf9[194].z) : 0}))
	pf_13_9 = ((((pf_13_7 * -2.f) + ((((((f_0_7 + f_2_12) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].z)) * 2.f) + utof(vs_cbuf9[195].z))) * utof(u_25_phi_54)) + ((f_2_12 + -0.5f) * utof(vs_cbuf9[194].z)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_1_7 : 0}))) + {u_2_9 : 0})), int(16u), int(16u)))
	u_20_15 = uint(bitfieldExtract(uint((uint((int(0) - int(u_1_7))) + u_2_9)), int(16u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_1_7 : 0}))) + {u_2_9 : 0})), int(0u), int(16u)))
	u_1_10 = uint(bitfieldExtract(uint((uint((int(0) - int(u_1_7))) + u_2_9)), int(0u), int(16u)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_4_8 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_10_4), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_10_4 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_6_13 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_10_4), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  ((uint({u_7_23 : 0}) >= uint({u_10_4 : 1})) ? 4294967295u : 0u)
	u_7_24 = ((uint(u_7_23) >= uint(u_10_4)) ? 4294967295u : 0u);
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * {u_20_15 : 0})), {u_1_10 : 0}, int(16u), int(16u))), int(16u), int(16u)))
	u_20_16 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * u_20_15)), u_1_10, int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * {u_20_15 : 0})), {u_1_10 : 0}, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int({u_1_7 : 0}))) + {u_2_9 : 0})), int(0u), int(16u))))))
	u_1_13 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * u_20_15)), u_1_10, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_8_9), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int(u_1_7))) + u_2_9)), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int(({u_8_9 : 1} >> 31u))))
	u_12_9 = uint((int(0) - int((u_8_9 >> 31u))));
	// 0  <=>  (({b_3_5 : False} || {b_1_44 : True}) ? ((uint((uint(bitfieldExtract(uint({u_8_9 : 1}), int(16u), int(16u))) * {u_20_16 : 0})) << 16u) + {u_1_13 : 0}) : 4294967295u)
	u_1_15 = ((b_3_5 || b_1_44) ? ((uint((uint(bitfieldExtract(uint(u_8_9), int(16u), int(16u))) * u_20_16)) << 16u) + u_1_13) : 4294967295u);
	// 0  <=>  uint((int(0) - int(({u_10_4 : 1} >> 31u))))
	u_1_16 = uint((int(0) - int((u_10_4 >> 31u))));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_12_8 : 0}))) + {u_7_24 : 0})), int(16u), int(16u)))
	u_19_22 = uint(bitfieldExtract(uint((uint((int(0) - int(u_12_8))) + u_7_24)), int(16u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_12_8 : 0}))) + {u_7_24 : 0})), int(0u), int(16u)))
	u_7_26 = uint(bitfieldExtract(uint((uint((int(0) - int(u_12_8))) + u_7_24)), int(0u), int(16u)));
	// 0  <=>  uint((int(0) - int(({u_11_2 : 1} >> 31u))))
	u_17_7 = uint((int(0) - int((u_11_2 >> 31u))));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_17_2 : 0}))) + {u_4_25 : 0})), int(16u), int(16u)))
	u_19_23 = uint(bitfieldExtract(uint((uint((int(0) - int(u_17_2))) + u_4_25)), int(16u), int(16u)));
	// 0  <=>  uint(bitfieldExtract(uint((uint((int(0) - int({u_17_2 : 0}))) + {u_4_25 : 0})), int(0u), int(16u)))
	u_4_27 = uint(bitfieldExtract(uint((uint((int(0) - int(u_17_2))) + u_4_25)), int(0u), int(16u)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_10_4 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_46 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_10_4), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * {u_19_22 : 0})), {u_7_26 : 0}, int(16u), int(16u))), int(16u), int(16u)))
	u_18_11 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * u_19_22)), u_7_26, int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * {u_19_22 : 0})), {u_7_26 : 0}, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int({u_12_8 : 0}))) + {u_7_24 : 0})), int(0u), int(16u))))))
	u_7_29 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * u_19_22)), u_7_26, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_10_4), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int(u_12_8))) + u_7_24)), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * {u_19_23 : 0})), {u_4_27 : 0}, int(16u), int(16u))), int(16u), int(16u)))
	u_11_3 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * u_19_23)), u_4_27, int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * {u_19_23 : 0})), {u_4_27 : 0}, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int({u_17_2 : 0}))) + {u_4_25 : 0})), int(0u), int(16u))))))
	u_4_30 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * u_19_23)), u_4_27, int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_11_2), int(0u), int(16u))) * uint(bitfieldExtract(uint((uint((int(0) - int(u_17_2))) + u_4_25)), int(0u), int(16u))))));
	// 0  <=>  (uint((int(0) - int({u_12_9 : 0}))) + (({u_15_3 : 0} + uint((int(0) - int(((uint({u_19_16 : 0}) >= uint({u_16_3 : 1})) ? 4294967295u : 0u))))) ^ {u_12_9 : 0}))
	u_8_18 = (uint((int(0) - int(u_12_9))) + ((u_15_3 + uint((int(0) - int(((uint(u_19_16) >= uint(u_16_3)) ? 4294967295u : 0u))))) ^ u_12_9));
	// 0  <=>  (uint((int(0) - int({u_1_16 : 0}))) + (({u_9_9 : 0} + uint((int(0) - int(((uint({u_2_10 : 0}) >= uint({u_18_4 : 1})) ? 4294967295u : 0u))))) ^ {u_1_16 : 0}))
	u_1_18 = (uint((int(0) - int(u_1_16))) + ((u_9_9 + uint((int(0) - int(((uint(u_2_10) >= uint(u_18_4)) ? 4294967295u : 0u))))) ^ u_1_16));
	// 0  <=>  (uint((int(0) - int({u_17_7 : 0}))) + (({u_6_2 : 0} + uint((int(0) - int(((uint({u_19_17 : 0}) >= uint({u_13_3 : 1})) ? 4294967295u : 0u))))) ^ {u_17_7 : 0}))
	u_2_16 = (uint((int(0) - int(u_17_7))) + ((u_6_2 + uint((int(0) - int(((uint(u_19_17) >= uint(u_13_3)) ? 4294967295u : 0u))))) ^ u_17_7));
	// 0  <=>  (({b_4_6 : False} || {b_3_8 : True}) ? ((uint((uint(bitfieldExtract(uint({u_11_2 : 1}), int(16u), int(16u))) * {u_11_3 : 0})) << 16u) + {u_4_30 : 0}) : 4294967295u)
	u_4_32 = ((b_4_6 || b_3_8) ? ((uint((uint(bitfieldExtract(uint(u_11_2), int(16u), int(16u))) * u_11_3)) << 16u) + u_4_30) : 4294967295u);
	// 0  <=>  (({b_5_7 : False} || {b_4_8 : True}) ? ((uint((uint(bitfieldExtract(uint({u_10_4 : 1}), int(16u), int(16u))) * {u_18_11 : 0})) << 16u) + {u_7_29 : 0}) : 4294967295u)
	u_7_31 = ((b_5_7 || b_4_8) ? ((uint((uint(bitfieldExtract(uint(u_10_4), int(16u), int(16u))) * u_18_11)) << 16u) + u_7_29) : 4294967295u);
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].z) : 1.00}) * {utof(vs_cbuf9[78].x) : 1.00})
	pf_25_5 = ((1.0f / utof(vs_cbuf9[78].z)) * utof(vs_cbuf9[78].x));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].z) : 1.00}) * {utof(vs_cbuf9[83].x) : 1.00})
	pf_26_4 = ((1.0f / utof(vs_cbuf9[83].z)) * utof(vs_cbuf9[83].x));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].w) : 1.00}) * {utof(vs_cbuf9[78].y) : 1.00})
	pf_28_1 = ((1.0f / utof(vs_cbuf9[78].w)) * utof(vs_cbuf9[78].y));
	// 0.1431713  <=>  ({pf_8_9 : 0.1431713} * {(vs_cbuf10_1.w) : 1.00})
	o.fs_attr1.w = (pf_8_9 * (vs_cbuf10_1.w));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].w) : 1.00}) * {utof(vs_cbuf9[83].y) : 1.00})
	pf_25_6 = ((1.0f / utof(vs_cbuf9[83].w)) * utof(vs_cbuf9[83].y));
	// 0.5072804  <=>  (({utof(vs_cbuf9[121].y) : 0.461164} * {(vs_cbuf10_1.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.10})
	o.fs_attr1.y = ((utof(vs_cbuf9[121].y) * (vs_cbuf10_1.y)) * utof(vs_cbuf9[104].x));
	// 0.5325397  <=>  (({utof(vs_cbuf9[121].z) : 0.484127} * {(vs_cbuf10_1.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.10})
	o.fs_attr1.z = ((utof(vs_cbuf9[121].z) * (vs_cbuf10_1.z)) * utof(vs_cbuf9[104].x));
	// 0.4353301  <=>  (({utof(vs_cbuf9[121].x) : 0.3957546} * {(vs_cbuf10_1.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 1.10})
	o.fs_attr1.x = ((utof(vs_cbuf9[121].x) * (vs_cbuf10_1.x)) * utof(vs_cbuf9[104].x));
	// 55.6875  <=>  (({i.vao_attr5.y : 55.6875} * {utof(vs_cbuf9[141].y) : 1.00}) * {(vs_cbuf10_3.z) : 1.00})
	pf_4_1 = ((i.vao_attr5.y * utof(vs_cbuf9[141].y)) * (vs_cbuf10_3.z));
	// 1.00  <=>  1.f
	o.fs_attr7.x = 1.f;
	// 1.00  <=>  1.f
	o.fs_attr7.y = 1.f;
	// 1.00  <=>  1.f
	o.fs_attr7.z = 1.f;
	// 1052239018  <=>  {ftou((((((({pf_23_12 : 1.00} * {utof(u_27_phi_25) : 0.4939}) + -0.5f) * sin({f_0_8 : -7.1287136})) + (cos({f_0_8 : -7.1287136}) * (({pf_19_9 : 1.00} * {utof(u_23_phi_22) : 0.98535}) + -0.5f))) * (({pf_0_1 : 335.00} * {utof(vs_cbuf9[85].w) : 0}) + (({utof(u_41_phi_41) : 0.13457} * {utof(vs_cbuf9[86].w) : 0}) + ({utof(vs_cbuf9[86].y) : 1.10} + {utof(vs_cbuf9[86].w) : 0})))) + (0.f - (({pf_19_9 : 1.00} * (0.f - float(int((({b_6_13 : False} || {b_1_46 : True}) ? {u_1_18 : 0} : 4294967295u))))) + (({pf_0_1 : 335.00} * {utof(vs_cbuf9[84].y) : 0}) + ((({utof(u_44_phi_41) : 0.31439} * {utof(vs_cbuf9[85].y) : 0}) * -2.f) + ({utof(vs_cbuf9[85].y) : 0} + {utof(vs_cbuf9[84].w) : 0}))))))) : 1052239018}
	u_2_18 = ftou(((((((pf_23_12 * utof(u_27_phi_25)) + -0.5f) * sin(f_0_8)) + (cos(f_0_8) * ((pf_19_9 * utof(u_23_phi_22)) + -0.5f))) * ((pf_0_1 * utof(vs_cbuf9[85].w)) + ((utof(u_41_phi_41) * utof(vs_cbuf9[86].w)) + (utof(vs_cbuf9[86].y) + utof(vs_cbuf9[86].w))))) + (0.f - ((pf_19_9 * (0.f - float(int(((b_6_13 || b_1_46) ? u_1_18 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[84].y)) + (((utof(u_44_phi_41) * utof(vs_cbuf9[85].y)) * -2.f) + (utof(vs_cbuf9[85].y) + utof(vs_cbuf9[84].w))))))));
	u_2_phi_55 = u_2_18;
	// False  <=>  if((((((0.f == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && (((0.f == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935}))))) ? true : false))
	if((((((0.f == abs((vs_cbuf8_28.z))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((0.f == abs((vs_cbuf8_28.x))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.x)))))) ? true : false))
	{
		// -0.6398518  <=>  {(vs_cbuf8_28.z) : -0.6398518}
		f_0_38 = (vs_cbuf8_28.z);
		// 1078530011  <=>  ((((0.f > {f_0_38 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_0_38 : -0.6398518}))) ? 1078530011u : 0u)
		u_4_34 = ((((0.f > f_0_38) && (! myIsNaN(0.f))) && (! myIsNaN(f_0_38))) ? 1078530011u : 0u);
		u_4_phi_56 = u_4_34;
		// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
		if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
		{
			// 3226013659  <=>  {ftou((0.f - {utof(((((0.f > {f_0_38 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_0_38 : -0.6398518}))) ? 1078530011u : 0u)) : 3.141593})) : 3226013659}
			u_4_35 = ftou((0.f - utof(((((0.f > f_0_38) && (! myIsNaN(0.f))) && (! myIsNaN(f_0_38))) ? 1078530011u : 0u))));
			u_4_phi_56 = u_4_35;
		}
		// 3226013659  <=>  {u_4_phi_56 : 3226013659}
		u_2_19 = u_4_phi_56;
		u_2_phi_55 = u_2_19;
	}
	// 1052239018  <=>  {u_2_phi_55 : 1052239018}
	u_1_21 = u_2_phi_55;
	u_1_phi_57 = u_1_21;
	// True  <=>  if(((! ((((0.f == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && (((0.f == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN(0.f))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935})))))) ? true : false))
	if(((! ((((0.f == abs((vs_cbuf8_28.z))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((0.f == abs((vs_cbuf8_28.x))) && (! myIsNaN(0.f))) && (! myIsNaN(abs((vs_cbuf8_28.x))))))) ? true : false))
	{
		// 2139095040  <=>  2139095040u
		u_3_36 = 2139095040u;
		u_3_phi_58 = u_3_36;
		// False  <=>  if(((((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && ((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935}))))) ? true : false))
		if((((((utof(0x7f800000) == abs((vs_cbuf8_28.z))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((utof(0x7f800000) == abs((vs_cbuf8_28.x))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.x)))))) ? true : false))
		{
			// -0.6398518  <=>  {(vs_cbuf8_28.z) : -0.6398518}
			f_0_46 = (vs_cbuf8_28.z);
			// 1075235812  <=>  ((((0.f > {f_0_46 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_0_46 : -0.6398518}))) ? 1075235812u : 1061752795u)
			u_7_33 = ((((0.f > f_0_46) && (! myIsNaN(0.f))) && (! myIsNaN(f_0_46))) ? 1075235812u : 1061752795u);
			u_7_phi_59 = u_7_33;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
			{
				// 3222719460  <=>  {ftou((0.f - {utof(((((0.f > {f_0_46 : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({f_0_46 : -0.6398518}))) ? 1075235812u : 1061752795u)) : 2.356194})) : 3222719460}
				u_7_34 = ftou((0.f - utof(((((0.f > f_0_46) && (! myIsNaN(0.f))) && (! myIsNaN(f_0_46))) ? 1075235812u : 1061752795u))));
				u_7_phi_59 = u_7_34;
			}
			// 3222719460  <=>  {u_7_phi_59 : 3222719460}
			u_3_37 = u_7_phi_59;
			u_3_phi_58 = u_3_37;
		}
		// 2139095040  <=>  {u_3_phi_58 : 2139095040}
		u_4_37 = u_3_phi_58;
		u_4_phi_60 = u_4_37;
		// True  <=>  if(((! (((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) && ((({utof(0x7f800000) : ∞} == abs({(vs_cbuf8_28.x) : -0.57711935})) && (! myIsNaN({utof(0x7f800000) : ∞}))) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935})))))) ? true : false))
		if(((! ((((utof(0x7f800000) == abs((vs_cbuf8_28.z))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) && (((utof(0x7f800000) == abs((vs_cbuf8_28.x))) && (! myIsNaN(utof(0x7f800000)))) && (! myIsNaN(abs((vs_cbuf8_28.x))))))) ? true : false))
		{
			// 1059310932  <=>  {ftou(max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))) : 1059310932}
			u_9_13 = ftou(max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))));
			u_9_phi_61 = u_9_13;
			// False  <=>  if(((((max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) >= 16.f) && (! myIsNaN(max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))))) && (! myIsNaN(16.f))) ? true : false))
			if(((((max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) >= 16.f) && (! myIsNaN(max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z)))))) && (! myIsNaN(16.f))) ? true : false))
			{
				// 1025756500  <=>  {ftou((max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) * 0.0625f)) : 1025756500}
				u_9_14 = ftou((max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) * 0.0625f));
				u_9_phi_61 = u_9_14;
			}
			// 1058258456  <=>  {ftou(min(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))) : 1058258456}
			u_7_36 = ftou(min(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))));
			u_7_phi_62 = u_7_36;
			// False  <=>  if(((((max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) >= 16.f) && (! myIsNaN(max(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518}))))) && (! myIsNaN(16.f))) ? true : false))
			if(((((max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) >= 16.f) && (! myIsNaN(max(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z)))))) && (! myIsNaN(16.f))) ? true : false))
			{
				// 1024704024  <=>  {ftou((min(abs({(vs_cbuf8_28.x) : -0.57711935}), abs({(vs_cbuf8_28.z) : -0.6398518})) * 0.0625f)) : 1024704024}
				u_7_37 = ftou((min(abs((vs_cbuf8_28.x)), abs((vs_cbuf8_28.z))) * 0.0625f));
				u_7_phi_62 = u_7_37;
			}
			// 0.9019579  <=>  ((1.0f / {utof(u_9_phi_61) : 0.6398518}) * {utof(u_7_phi_62) : 0.5771194})
			pf_15_13 = ((1.0f / utof(u_9_phi_61)) * utof(u_7_phi_62));
			// 0.813528  <=>  ({pf_15_13 : 0.9019579} * {pf_15_13 : 0.9019579})
			pf_16_15 = (pf_15_13 * pf_15_13);
			// 1060888727  <=>  {ftou((((1.0f / (({pf_16_15 : 0.813528} * (({pf_16_15 : 0.813528} * ({pf_16_15 : 0.813528} + 11.335388f)) + 28.842468f)) + 19.69667f)) * ({pf_15_13 : 0.9019579} * ({pf_16_15 : 0.813528} * (({pf_16_15 : 0.813528} * (({pf_16_15 : 0.813528} * -0.82336295f) + -5.674867f)) + -6.565555f)))) + {pf_15_13 : 0.9019579})) : 1060888727}
			u_8_21 = ftou((((1.0f / ((pf_16_15 * ((pf_16_15 * (pf_16_15 + 11.335388f)) + 28.842468f)) + 19.69667f)) * (pf_15_13 * (pf_16_15 * ((pf_16_15 * ((pf_16_15 * -0.82336295f) + -5.674867f)) + -6.565555f)))) + pf_15_13));
			u_8_phi_63 = u_8_21;
			// False  <=>  if(((((abs({(vs_cbuf8_28.x) : -0.57711935}) > abs({(vs_cbuf8_28.z) : -0.6398518})) && (! myIsNaN(abs({(vs_cbuf8_28.x) : -0.57711935})))) && (! myIsNaN(abs({(vs_cbuf8_28.z) : -0.6398518})))) ? true : false))
			if(((((abs((vs_cbuf8_28.x)) > abs((vs_cbuf8_28.z))) && (! myIsNaN(abs((vs_cbuf8_28.x))))) && (! myIsNaN(abs((vs_cbuf8_28.z))))) ? true : false))
			{
				// 1062616863  <=>  {ftou(((0.f - (((1.0f / (({pf_16_15 : 0.813528} * (({pf_16_15 : 0.813528} * ({pf_16_15 : 0.813528} + 11.335388f)) + 28.842468f)) + 19.69667f)) * ({pf_15_13 : 0.9019579} * ({pf_16_15 : 0.813528} * (({pf_16_15 : 0.813528} * (({pf_16_15 : 0.813528} * -0.82336295f) + -5.674867f)) + -6.565555f)))) + {pf_15_13 : 0.9019579})) + 1.5707964f)) : 1062616863}
				u_8_22 = ftou(((0.f - (((1.0f / ((pf_16_15 * ((pf_16_15 * (pf_16_15 + 11.335388f)) + 28.842468f)) + 19.69667f)) * (pf_15_13 * (pf_16_15 * ((pf_16_15 * ((pf_16_15 * -0.82336295f) + -5.674867f)) + -6.565555f)))) + pf_15_13)) + 1.5707964f));
				u_8_phi_63 = u_8_22;
			}
			// 1060888727  <=>  {u_8_phi_63 : 1060888727}
			u_7_39 = u_8_phi_63;
			u_7_phi_64 = u_7_39;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.z) : -0.6398518}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.z) : -0.6398518}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.z)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.z)))) ? true : false))
			{
				// 1075451829  <=>  {ftou(((0.f - {utof(u_8_phi_63) : 0.7338957}) + 3.1415927f)) : 1075451829}
				u_7_40 = ftou(((0.f - utof(u_8_phi_63)) + 3.1415927f));
				u_7_phi_64 = u_7_40;
			}
			// 1075451829  <=>  {u_7_phi_64 : 1075451829}
			u_8_23 = u_7_phi_64;
			u_8_phi_65 = u_8_23;
			// True  <=>  if(((((0.f > {(vs_cbuf8_28.x) : -0.57711935}) && (! myIsNaN(0.f))) && (! myIsNaN({(vs_cbuf8_28.x) : -0.57711935}))) ? true : false))
			if(((((0.f > (vs_cbuf8_28.x)) && (! myIsNaN(0.f))) && (! myIsNaN((vs_cbuf8_28.x)))) ? true : false))
			{
				// 3222935477  <=>  {ftou((0.f - {utof(u_7_phi_64) : 2.407697})) : 3222935477}
				u_8_24 = ftou((0.f - utof(u_7_phi_64)));
				u_8_phi_65 = u_8_24;
			}
			// 3222935477  <=>  {u_8_phi_65 : 3222935477}
			u_4_38 = u_8_phi_65;
			u_4_phi_60 = u_4_38;
		}
		// 3222935477  <=>  {u_4_phi_60 : 3222935477}
		u_1_22 = u_4_phi_60;
		u_1_phi_57 = u_1_22;
	}
	// -2.407697  <=>  ((((({pf_11_6 : 0} * -2.f) + (((((({f_7_0 : 0.13457} + {f_2_12 : 0.96409}) * 0.5f) + -0.5f) * {utof(vs_cbuf9[196].y) : 0}) * 2.f) + {utof(vs_cbuf9[195].y) : 0})) * {utof(u_25_phi_54) : 335.00}) + (({f_7_0 : 0.13457} + -0.5f) * {utof(vs_cbuf9[194].y) : 0})) + {utof(u_1_phi_57) : -2.407697})
	pf_11_9 = (((((pf_11_6 * -2.f) + ((((((f_7_0 + f_2_12) * 0.5f) + -0.5f) * utof(vs_cbuf9[196].y)) * 2.f) + utof(vs_cbuf9[195].y))) * utof(u_25_phi_54)) + ((f_7_0 + -0.5f) * utof(vs_cbuf9[194].y))) + utof(u_1_phi_57));
	// 1.00  <=>  cos({pf_12_8 : 0})
	f_3_39 = cos(pf_12_8);
	// -22.762726  <=>  ({pf_6_3 : 342.9746} + (0.f - {(camera_wpos.y) : 365.7373}))
	pf_12_9 = (pf_6_3 + (0.f - (camera_wpos.y)));
	// 1.00  <=>  cos({pf_13_9 : 0})
	f_5_46 = cos(pf_13_9);
	// 193.6912  <=>  ({pf_7_4 : -1725.571} + (0.f - {(camera_wpos.x) : -1919.2622}))
	pf_21_10 = (pf_7_4 + (0.f - (camera_wpos.x)));
	// 64.47192  <=>  ({pf_5_3 : -3668.575} + (0.f - {(camera_wpos.z) : -3733.0469}))
	pf_24_12 = (pf_5_3 + (0.f - (camera_wpos.z)));
	// 205.4046  <=>  sqrt((({pf_24_12 : 64.47192} * {pf_24_12 : 64.47192}) + (({pf_12_9 : -22.762726} * {pf_12_9 : -22.762726}) + ({pf_21_10 : 193.6912} * {pf_21_10 : 193.6912}))))
	f_2_27 = sqrt(((pf_24_12 * pf_24_12) + ((pf_12_9 * pf_12_9) + (pf_21_10 * pf_21_10))));
	// -0.37217003  <=>  ((((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * ((sin({pf_13_9 : 0}) * ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))) + (0.f - (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0}))))) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * ((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00})) + (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697})))) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * ({f_3_39 : 1.00} * {f_5_46 : 1.00})))) + (((0.f - abs({(vs_cbuf8_28.y) : 0.5074672})) * {(vs_cbuf13_6.w) : 0}) + {(vs_cbuf13_6.w) : 0})) * (1.0f / {pf_4_1 : 55.6875}))
	pf_12_12 = (((((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * ((sin(pf_13_9) * (f_3_39 * sin(pf_11_9))) + (0.f - (cos(pf_11_9) * sin(pf_12_8))))) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * ((sin(pf_13_9) * (cos(pf_11_9) * f_3_39)) + (sin(pf_12_8) * sin(pf_11_9)))) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (f_3_39 * f_5_46)))) + (((0.f - abs((vs_cbuf8_28.y))) * (vs_cbuf13_6.w)) + (vs_cbuf13_6.w))) * (1.0f / pf_4_1));
	// 250.00  <=>  {utof(vs_cbuf9[197].x) : 250.00}
	f_6_38 = utof(vs_cbuf9[197].x);
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_28 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false);
	// 3207296483  <=>  {ftou(({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))) : 3207296483}
	u_3_38 = ftou((f_3_39 * sin(pf_11_9)));
	u_3_phi_66 = u_3_38;
	// False  <=>  if({b_2_28 : False})
	if(b_2_28)
	{
		// 1132068864  <=>  {vs_cbuf9[197].y : 1132068864}
		u_3_39 = vs_cbuf9[197].y;
		u_3_phi_66 = u_3_39;
	}
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_29 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false); // maybe duplicate expression on the right side of the assignment, vars:(b_2_28, b_2_28)
	// 3207296483  <=>  {u_3_phi_66 : 3207296483}
	u_1_24 = u_3_phi_66;
	u_1_phi_67 = u_1_24;
	// False  <=>  if({b_2_29 : False})
	if(b_2_29)
	{
		// 3216972912  <=>  {ftou((1.0f / {utof(u_3_phi_66) : -0.66976756})) : 3216972912}
		u_1_25 = ftou((1.0f / utof(u_3_phi_66)));
		u_1_phi_67 = u_1_25;
	}
	// -0.029853104  <=>  (((({pf_12_12 : -0.37217003} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_12_12 : -0.37217003})) * {(vs_cbuf13_2.y) : 0.20}) * {(vs_cbuf16_1.x) : 0.7282469})
	pf_20_9 = ((((pf_12_12 * (vs_cbuf16_1.w)) * abs(pf_12_12)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.x));
	// -0  <=>  (((({pf_12_12 : -0.37217003} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_12_12 : -0.37217003})) * {(vs_cbuf13_2.y) : 0.20}) * {(vs_cbuf16_1.y) : 0})
	pf_24_15 = ((((pf_12_12 * (vs_cbuf16_1.w)) * abs(pf_12_12)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.y));
	// -0.02809319  <=>  (((({pf_12_12 : -0.37217003} * {(vs_cbuf16_1.w) : 1.479783}) * abs({pf_12_12 : -0.37217003})) * {(vs_cbuf13_2.y) : 0.20}) * {(vs_cbuf16_1.z) : 0.685315})
	pf_19_18 = ((((pf_12_12 * (vs_cbuf16_1.w)) * abs(pf_12_12)) * (vs_cbuf13_2.y)) * (vs_cbuf16_1.z));
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_30 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false); // maybe duplicate expression on the right side of the assignment, vars:(b_2_28, b_2_28)|(b_2_29, b_2_29)
	// 1129146258  <=>  {ftou(f_2_27) : 1129146258}
	u_3_40 = ftou(f_2_27);
	u_3_phi_68 = u_3_40;
	// False  <=>  if({b_2_30 : False})
	if(b_2_30)
	{
		// 1132068864  <=>  {ftou(max({f_2_27 : 205.4046}, {utof(vs_cbuf9[197].y) : 250.00})) : 1132068864}
		u_3_41 = ftou(max(f_2_27, utof(vs_cbuf9[197].y)));
		u_3_phi_68 = u_3_41;
	}
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_31 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false); // maybe duplicate expression on the right side of the assignment, vars:(b_2_28, b_2_28)|(b_2_29, b_2_29)|(b_2_30, b_2_30)
	// 0  <=>  {u_6_2 : 0}
	u_2_21 = u_6_2;
	u_2_phi_69 = u_2_21;
	// False  <=>  if({b_2_31 : False})
	if(b_2_31)
	{
		// 3272184517  <=>  {ftou(({utof(u_3_phi_68) : 205.4046} * {utof(u_1_phi_67) : -0.66976756})) : 3272184517}
		u_2_22 = ftou((utof(u_3_phi_68) * utof(u_1_phi_67)));
		u_2_phi_69 = u_2_22;
	}
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_32 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false); // maybe duplicate expression on the right side of the assignment, vars:(b_2_28, b_2_28)|(b_2_29, b_2_29)|(b_2_30, b_2_30)|(b_2_31, b_2_31)
	// 3302515151  <=>  {ftou(((((0.f - {pf_7_4 : -1725.571}) + (((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_20_9 : -0.029853104}) + ({pf_7_4 : -1725.571} + ((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * (sin({pf_11_9 : -2.407697}) * {f_5_46 : 1.00})) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * (cos({pf_11_9 : -2.407697}) * {f_5_46 : 1.00})) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * (0.f - sin({pf_13_9 : 0})))))))) * (min({f_2_27 : 205.4046}, {utof(vs_cbuf9[197].x) : 250.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 250.00}))) + {pf_7_4 : -1725.571})) : 3302515151}
	u_3_42 = ftou(((((0.f - pf_7_4) + (((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_20_9) + (pf_7_4 + (((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * (sin(pf_11_9) * f_5_46)) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * (cos(pf_11_9) * f_5_46)) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (0.f - sin(pf_13_9)))))))) * (min(f_2_27, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x)))) + pf_7_4));
	u_3_phi_70 = u_3_42;
	// False  <=>  if({b_2_32 : False})
	if(b_2_32)
	{
		// 3302470214  <=>  {ftou(((((0.f - {pf_7_4 : -1725.571}) + (((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_20_9 : -0.029853104}) + ({pf_7_4 : -1725.571} + ((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * (sin({pf_11_9 : -2.407697}) * {f_5_46 : 1.00})) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * (cos({pf_11_9 : -2.407697}) * {f_5_46 : 1.00})) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * (0.f - sin({pf_13_9 : 0})))))))) * {utof(u_2_phi_69) : 0}) + {pf_7_4 : -1725.571})) : 3302470214}
		u_3_43 = ftou(((((0.f - pf_7_4) + (((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_20_9) + (pf_7_4 + (((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * (sin(pf_11_9) * f_5_46)) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * (cos(pf_11_9) * f_5_46)) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (0.f - sin(pf_13_9)))))))) * utof(u_2_phi_69)) + pf_7_4));
		u_3_phi_70 = u_3_43;
	}
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_33 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false); // maybe duplicate expression on the right side of the assignment, vars:(b_2_28, b_2_28)|(b_2_29, b_2_29)|(b_2_30, b_2_30)|(b_2_31, b_2_31)|(b_2_32, b_2_32)
	// 1134754082  <=>  {ftou(((((0.f - {pf_6_3 : 342.9746}) + (((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_24_15 : -0}) + ({pf_6_3 : 342.9746} + ((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * ((sin({pf_13_9 : 0}) * ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))) + (0.f - (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0}))))) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * ((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00})) + (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697})))) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * ({f_3_39 : 1.00} * {f_5_46 : 1.00}))))))) * (min({f_2_27 : 205.4046}, {utof(vs_cbuf9[197].x) : 250.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 250.00}))) + {pf_6_3 : 342.9746})) : 1134754082}
	u_4_43 = ftou(((((0.f - pf_6_3) + (((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_24_15) + (pf_6_3 + (((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * ((sin(pf_13_9) * (f_3_39 * sin(pf_11_9))) + (0.f - (cos(pf_11_9) * sin(pf_12_8))))) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * ((sin(pf_13_9) * (cos(pf_11_9) * f_3_39)) + (sin(pf_12_8) * sin(pf_11_9)))) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (f_3_39 * f_5_46))))))) * (min(f_2_27, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x)))) + pf_6_3));
	u_4_phi_71 = u_4_43;
	// False  <=>  if({b_2_33 : False})
	if(b_2_33)
	{
		// 1135312063  <=>  {ftou(((((0.f - {pf_6_3 : 342.9746}) + (((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_24_15 : -0}) + ({pf_6_3 : 342.9746} + ((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * ((sin({pf_13_9 : 0}) * ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))) + (0.f - (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0}))))) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * ((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00})) + (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697})))) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * ({f_3_39 : 1.00} * {f_5_46 : 1.00}))))))) * {utof(u_2_phi_69) : 0}) + {pf_6_3 : 342.9746})) : 1135312063}
		u_4_44 = ftou(((((0.f - pf_6_3) + (((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_24_15) + (pf_6_3 + (((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * ((sin(pf_13_9) * (f_3_39 * sin(pf_11_9))) + (0.f - (cos(pf_11_9) * sin(pf_12_8))))) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * ((sin(pf_13_9) * (cos(pf_11_9) * f_3_39)) + (sin(pf_12_8) * sin(pf_11_9)))) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (f_3_39 * f_5_46))))))) * utof(u_2_phi_69)) + pf_6_3));
		u_4_phi_71 = u_4_44;
	}
	// False  <=>  (((({f_2_27 : 205.4046} > {f_6_38 : 250.00}) && (! myIsNaN({f_2_27 : 205.4046}))) && (! myIsNaN({f_6_38 : 250.00}))) ? true : false)
	b_2_34 = ((((f_2_27 > f_6_38) && (! myIsNaN(f_2_27))) && (! myIsNaN(f_6_38))) ? true : false); // maybe duplicate expression on the right side of the assignment, vars:(b_2_28, b_2_28)|(b_2_29, b_2_29)|(b_2_30, b_2_30)|(b_2_31, b_2_31)|(b_2_32, b_2_32)|(b_2_33, b_2_33)
	// 3311778956  <=>  {ftou(((((0.f - {pf_5_3 : -3668.575}) + (((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_19_18 : -0.02809319}) + ({pf_5_3 : -3668.575} + ((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * ((sin({pf_13_9 : 0}) * (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697}))) + (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00}))) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * ((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0}))) + (0.f - ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))))) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * (sin({pf_12_8 : 0}) * {f_5_46 : 1.00}))))))) * (min({f_2_27 : 205.4046}, {utof(vs_cbuf9[197].x) : 250.00}) * (1.0f / {utof(vs_cbuf9[197].x) : 250.00}))) + {pf_5_3 : -3668.575})) : 3311778956}
	u_6_4 = ftou(((((0.f - pf_5_3) + (((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_19_18) + (pf_5_3 + (((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * ((sin(pf_13_9) * (sin(pf_12_8) * sin(pf_11_9))) + (cos(pf_11_9) * f_3_39))) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * ((sin(pf_13_9) * (cos(pf_11_9) * sin(pf_12_8))) + (0.f - (f_3_39 * sin(pf_11_9))))) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (sin(pf_12_8) * f_5_46))))))) * (min(f_2_27, utof(vs_cbuf9[197].x)) * (1.0f / utof(vs_cbuf9[197].x)))) + pf_5_3));
	u_6_phi_72 = u_6_4;
	// False  <=>  if({b_2_34 : False})
	if(b_2_34)
	{
		// 3311749427  <=>  {ftou(((((0.f - {pf_5_3 : -3668.575}) + (((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * {pf_19_18 : -0.02809319}) + ({pf_5_3 : -3668.575} + ((((({i.vao_attr5.z : 74.25} * {utof(vs_cbuf9[141].z) : 1.00}) * {(vs_cbuf10_3.w) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.10712})) * ((sin({pf_13_9 : 0}) * (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697}))) + (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00}))) + ((((((clamp(min(0.f, {f_0_7 : 0.31439}), 0.0, 1.0) + {i.vao_attr5.x : 74.25}) * {utof(vs_cbuf9[141].x) : 1.00}) * {(vs_cbuf10_3.y) : 0.9999999}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : -0.01573})) * ((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0}))) + (0.f - ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))))) + (({pf_4_1 : 55.6875} * ((0.5f * {utof(vs_cbuf9[16].y) : -0.05}) + {v.vertex.y : -0.34717})) * (sin({pf_12_8 : 0}) * {f_5_46 : 1.00}))))))) * {utof(u_2_phi_69) : 0}) + {pf_5_3 : -3668.575})) : 3311749427}
		u_6_5 = ftou(((((0.f - pf_5_3) + (((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * pf_19_18) + (pf_5_3 + (((((i.vao_attr5.z * utof(vs_cbuf9[141].z)) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z)) * ((sin(pf_13_9) * (sin(pf_12_8) * sin(pf_11_9))) + (cos(pf_11_9) * f_3_39))) + ((((((clamp(min(0.f, f_0_7), 0.0, 1.0) + i.vao_attr5.x) * utof(vs_cbuf9[141].x)) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x)) * ((sin(pf_13_9) * (cos(pf_11_9) * sin(pf_12_8))) + (0.f - (f_3_39 * sin(pf_11_9))))) + ((pf_4_1 * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y)) * (sin(pf_12_8) * f_5_46))))))) * utof(u_2_phi_69)) + pf_5_3));
		u_6_phi_72 = u_6_5;
	}
	// -101.4032  <=>  ((({utof(u_6_phi_72) : -3675.7842} * {(view_proj[0].z) : 0.6697676}) + (({utof(u_4_phi_71) : 325.9464} * {(view_proj[0].y) : 1.493044E-08}) + ({utof(u_3_phi_70) : -1731.0565} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
	pf_2_9 = (((utof(u_6_phi_72) * (view_proj[0].z)) + ((utof(u_4_phi_71) * (view_proj[0].y)) + (utof(u_3_phi_70) * (view_proj[0].x)))) + (view_proj[0].w));
	// 51.2594  <=>  ((({utof(u_6_phi_72) : -3675.7842} * {(view_proj[1].z) : 0.3768303}) + (({utof(u_4_phi_71) : 325.9464} * {(view_proj[1].y) : 0.8616711}) + ({utof(u_3_phi_70) : -1731.0565} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
	pf_1_16 = (((utof(u_6_phi_72) * (view_proj[1].z)) + ((utof(u_4_phi_71) * (view_proj[1].y)) + (utof(u_3_phi_70) * (view_proj[1].x)))) + (view_proj[1].w));
	// -165.44946  <=>  ((({utof(u_6_phi_72) : -3675.7842} * {(view_proj[2].z) : -0.6398518}) + (({utof(u_4_phi_71) : 325.9464} * {(view_proj[2].y) : 0.5074672}) + ({utof(u_3_phi_70) : -1731.0565} * {(view_proj[2].x) : -0.57711935}))) + {(view_proj[2].w) : -3681.8398})
	pf_12_23 = (((utof(u_6_phi_72) * (view_proj[2].z)) + ((utof(u_4_phi_71) * (view_proj[2].y)) + (utof(u_3_phi_70) * (view_proj[2].x)))) + (view_proj[2].w));
	// 1.00  <=>  ((({utof(u_6_phi_72) : -3675.7842} * {(view_proj[3].z) : 0}) + (({utof(u_4_phi_71) : 325.9464} * {(view_proj[3].y) : 0}) + ({utof(u_3_phi_70) : -1731.0565} * {(view_proj[3].x) : 0}))) + {(view_proj[3].w) : 1.00})
	pf_16_26 = (((utof(u_6_phi_72) * (view_proj[3].z)) + ((utof(u_4_phi_71) * (view_proj[3].y)) + (utof(u_3_phi_70) * (view_proj[3].x)))) + (view_proj[3].w));
	// -188.20569  <=>  ((0.f - {utof(u_3_phi_70) : -1731.0565}) + {(camera_wpos.x) : -1919.2622})
	pf_12_24 = ((0.f - utof(u_3_phi_70)) + (camera_wpos.x));
	// 165.2508  <=>  (({pf_16_26 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_12_23 : -165.44946} * {(view_proj[6].z) : -1.000008}) + (({pf_1_16 : 51.2594} * {(view_proj[6].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[6].x) : 0}))))
	pf_19_22 = ((pf_16_26 * (view_proj[6].w)) + ((pf_12_23 * (view_proj[6].z)) + ((pf_1_16 * (view_proj[6].y)) + (pf_2_9 * (view_proj[6].x)))));
	// 39.79095  <=>  ((0.f - {utof(u_4_phi_71) : 325.9464}) + {(camera_wpos.y) : 365.7373})
	pf_20_13 = ((0.f - utof(u_4_phi_71)) + (camera_wpos.y));
	// 165.4495  <=>  (({pf_16_26 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[7].z) : -1}) + (({pf_1_16 : 51.2594} * {(view_proj[7].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[7].x) : 0}))))
	pf_1_19 = ((pf_16_26 * (view_proj[7].w)) + ((pf_12_23 * (view_proj[7].z)) + ((pf_1_16 * (view_proj[7].y)) + (pf_2_9 * (view_proj[7].x)))));
	// 3302515151  <=>  {u_3_phi_70 : 3302515151}
	u_1_30 = u_3_phi_70;
	u_1_phi_73 = u_1_30;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1136687300  <=>  {ftou(({utof(u_3_phi_70) : -1731.0565} + (0.f - {(vs_cbuf15_52.x) : -2116}))) : 1136687300}
		u_1_31 = ftou((utof(u_3_phi_70) + (0.f - (vs_cbuf15_52.x))));
		u_1_phi_73 = u_1_31;
	}
	// 0  <=>  ((0.f * (({pf_16_26 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[4].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[4].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_16_26 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[5].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[5].y) : 2.144507}) + ({pf_2_9 : -101.4032} * {(view_proj[5].x) : 0}))))))
	pf_24_17 = ((0.f * ((pf_16_26 * (view_proj[4].w)) + ((pf_12_23 * (view_proj[4].z)) + ((pf_1_16 * (view_proj[4].y)) + (pf_2_9 * (view_proj[4].x)))))) + (0.f * ((pf_16_26 * (view_proj[5].w)) + ((pf_12_23 * (view_proj[5].z)) + ((pf_1_16 * (view_proj[5].y)) + (pf_2_9 * (view_proj[5].x)))))));
	// -57.262695  <=>  ((0.f - {utof(u_6_phi_72) : -3675.7842}) + {(camera_wpos.z) : -3733.0469})
	pf_27_5 = ((0.f - utof(u_6_phi_72)) + (camera_wpos.z));
	// 3311778956  <=>  {u_6_phi_72 : 3311778956}
	u_2_25 = u_6_phi_72;
	u_2_phi_74 = u_2_25;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1132469152  <=>  {ftou(({utof(u_6_phi_72) : -3675.7842} + (0.f - {(vs_cbuf15_52.y) : -3932}))) : 1132469152}
		u_2_26 = ftou((utof(u_6_phi_72) + (0.f - (vs_cbuf15_52.y))));
		u_2_phi_74 = u_2_26;
	}
	// 1060320051  <=>  {ftou((({pf_0_1 : 335.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_39_phi_30) : 0.13457} * {utof(vs_cbuf9[81].z) : 0}) + ({utof(vs_cbuf9[81].x) : 0.70} + {utof(vs_cbuf9[81].z) : 0})))) : 1060320051}
	u_3_45 = ftou(((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_39_phi_30) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].x) + utof(vs_cbuf9[81].z)))));
	u_3_phi_75 = u_3_45;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 3239249927  <=>  {ftou(({utof(u_2_phi_74) : -3675.7842} * {(vs_cbuf15_52.z) : 0.0025})) : 3239249927}
		u_3_46 = ftou((utof(u_2_phi_74) * (vs_cbuf15_52.z)));
		u_3_phi_75 = u_3_46;
	}
	// 0.0049824  <=>  inversesqrt((({pf_27_5 : -57.262695} * {pf_27_5 : -57.262695}) + (({pf_20_13 : 39.79095} * {pf_20_13 : 39.79095}) + ({pf_12_24 : -188.20569} * {pf_12_24 : -188.20569}))))
	f_3_75 = inversesqrt(((pf_27_5 * pf_27_5) + ((pf_20_13 * pf_20_13) + (pf_12_24 * pf_12_24))));
	// -0.06617705  <=>  (1.0f / ((((({pf_1_19 : 165.4495} * 0.5f) + (({pf_19_22 : 165.2508} * 0.5f) + {pf_24_17 : 0})) * (1.0f / ({pf_1_19 : 165.4495} + ((0.f * {pf_19_22 : 165.2508}) + {pf_24_17 : 0})))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00})))
	f_5_52 = (1.0f / (((((pf_1_19 * 0.5f) + ((pf_19_22 * 0.5f) + pf_24_17)) * (1.0f / (pf_1_19 + ((0.f * pf_19_22) + pf_24_17)))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y))));
	// 0.0534952  <=>  ((((({pf_27_5 : -57.262695} * {f_3_75 : 0.0049824}) * {(lightDir.z) : -0.08728968}) + ((({pf_20_13 : 39.79095} * {f_3_75 : 0.0049824}) * {(lightDir.y) : -0.4663191}) + (({pf_12_24 : -188.20569} * {f_3_75 : 0.0049824}) * {(lightDir.x) : 0.8802994}))) * 0.5f) + 0.5f)
	pf_12_29 = (((((pf_27_5 * f_3_75) * (lightDir.z)) + (((pf_20_13 * f_3_75) * (lightDir.y)) + ((pf_12_24 * f_3_75) * (lightDir.x)))) * 0.5f) + 0.5f);
	// 1.559591  <=>  (({pf_12_29 : 0.0534952} * (({pf_12_29 : 0.0534952} * (({pf_12_29 : 0.0534952} * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
	pf_12_30 = ((pf_12_29 * ((pf_12_29 * ((pf_12_29 * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f);
	// 0.9928269  <=>  exp2((log2(((0.f - clamp(((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.y) : 1.00}))
	f_6_43 = exp2((log2(((0.f - clamp((((f_5_52 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.y)));
	// 1056964608  <=>  1056964608u
	u_2_28 = 1056964608u;
	u_2_phi_76 = u_2_28;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 3230301193  <=>  {ftou(({utof(u_1_phi_73) : -1731.0565} * {(vs_cbuf15_52.z) : 0.0025})) : 3230301193}
		u_2_29 = ftou((utof(u_1_phi_73) * (vs_cbuf15_52.z)));
		u_2_phi_76 = u_2_29;
	}
	// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, float2(((({pf_12_30 : 1.559591} * (0.f - sqrt(((0.f - {pf_12_29 : 0.0534952}) + 1.f)))) * 0.63661975f) + 1.f), (({f_6_43 : 0.9928269} * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
	f4_0_0 = textureLod(tex0, float2((((pf_12_30 * (0.f - sqrt(((0.f - pf_12_29) + 1.f)))) * 0.63661975f) + 1.f), ((f_6_43 * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler);
	// 1056964608  <=>  {u_2_phi_76 : 1056964608}
	u_1_33 = u_2_phi_76;
	u_1_phi_77 = u_1_33;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex1 : tex1}, float2({utof(u_2_phi_76) : 0.50}, {utof(u_3_phi_75) : 0.70}), 0.0, s_linear_clamp_sampler)
		f4_0_1 = textureLod(tex1, float2(utof(u_2_phi_76), utof(u_3_phi_75)), 0.0, s_linear_clamp_sampler);
		// 1065353216  <=>  {ftou(f4_0_1.w) : 1065353216}
		u_1_34 = ftou(f4_0_1.w);
		u_1_phi_77 = u_1_34;
	}
	// -122.32116  <=>  (({pf_16_26 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[4].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[4].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[4].x) : 1.206285}))))
	o.vertex.x = ((pf_16_26 * (view_proj[4].w)) + ((pf_12_23 * (view_proj[4].z)) + ((pf_1_16 * (view_proj[4].y)) + (pf_2_9 * (view_proj[4].x)))));
	// 109.9261  <=>  (({pf_16_26 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[5].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[5].y) : 2.144507}) + ({pf_2_9 : -101.4032} * {(view_proj[5].x) : 0}))))
	o.vertex.y = ((pf_16_26 * (view_proj[5].w)) + ((pf_12_23 * (view_proj[5].z)) + ((pf_1_16 * (view_proj[5].y)) + (pf_2_9 * (view_proj[5].x)))));
	// 112  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 112u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_3_49 = ((ftou(vs_cbuf0_21.x) + 112u) - ftou(vs_cbuf0_21.x));
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x : }
	u_3_50 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x;
	// 128  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 128u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_2_32 = ((ftou(vs_cbuf0_21.x) + 128u) - ftou(vs_cbuf0_21.x));
	// 165.2508  <=>  {pf_19_22 : 165.2508}
	o.vertex.z = pf_19_22;
	// 165.4495  <=>  {pf_1_19 : 165.4495}
	o.vertex.w = pf_1_19;
	// 165.3501  <=>  (({pf_1_19 : 165.4495} * 0.5f) + (({pf_19_22 : 165.2508} * 0.5f) + {pf_24_17 : 0}))
	o.fs_attr4.z = ((pf_1_19 * 0.5f) + ((pf_19_22 * 0.5f) + pf_24_17));
	// 164.5869  <=>  ((({pf_5_3 : -3668.575} * {(vs_cbuf8_11.z) : 0.6398518}) + (({pf_6_3 : 342.9746} * {(vs_cbuf8_11.y) : -0.50746715}) + ({pf_7_4 : -1725.571} * {(vs_cbuf8_11.x) : 0.5771194}))) + {(vs_cbuf8_11.w) : 3681.84})
	pf_6_5 = (((pf_5_3 * (vs_cbuf8_11.z)) + ((pf_6_3 * (vs_cbuf8_11.y)) + (pf_7_4 * (vs_cbuf8_11.x)))) + (vs_cbuf8_11.w));
	// 165.4495  <=>  ({pf_1_19 : 165.4495} + ((0.f * {pf_19_22 : 165.2508}) + {pf_24_17 : 0}))
	o.fs_attr4.w = (pf_1_19 + ((0.f * pf_19_22) + pf_24_17));
	// 164.3882  <=>  ((({pf_5_3 : -3668.575} * {(vs_cbuf8_10.z) : 0.6398569}) + (({pf_6_3 : 342.9746} * {(vs_cbuf8_10.y) : -0.5074712}) + ({pf_7_4 : -1725.571} * {(vs_cbuf8_10.x) : 0.5771239}))) + {(vs_cbuf8_10.w) : 3681.669})
	pf_5_5 = (((pf_5_3 * (vs_cbuf8_10.z)) + ((pf_6_3 * (vs_cbuf8_10.y)) + (pf_7_4 * (vs_cbuf8_10.x)))) + (vs_cbuf8_10.w));
	// 1.00  <=>  1.f
	o.fs_attr9.x = 1.f;
	// 1041254423  <=>  {ftou(v.offset.y) : 1041254423}
	u_13_7 = ftou(v.offset.y);
	u_13_phi_78 = u_13_7;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1041254423  <=>  {ftou((({v.vertex.y : -0.34717} * {(vs_cbuf13_0.x) : 0}) + {v.offset.y : 0.1409})) : 1041254423}
		u_13_8 = ftou(((v.vertex.y * (vs_cbuf13_0.x)) + v.offset.y));
		u_13_phi_78 = u_13_8;
	}
	// 0  <=>  {ftou(v.offset.x) : 0}
	u_10_14 = ftou(v.offset.x);
	u_10_phi_79 = u_10_14;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.x : -0.01573} * {(vs_cbuf13_0.x) : 0}) + {v.offset.x : 0})) : 0}
		u_10_15 = ftou(((v.vertex.x * (vs_cbuf13_0.x)) + v.offset.x));
		u_10_phi_79 = u_10_15;
	}
	// 1065189135  <=>  {ftou(v.offset.z) : 1065189135}
	u_11_5 = ftou(v.offset.z);
	u_11_phi_80 = u_11_5;
	// True  <=>  if(((! (((({v.vertex.z : 0.10712} == 0.f) && (! myIsNaN({v.vertex.z : 0.10712}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : -0.01573} == 0.f) && (! myIsNaN({v.vertex.x : -0.01573}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : -0.34717} == 0.f) && (! myIsNaN({v.vertex.y : -0.34717}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1065189135  <=>  {ftou((({v.vertex.z : 0.10712} * {(vs_cbuf13_0.x) : 0}) + {v.offset.z : 0.99022})) : 1065189135}
		u_11_6 = ftou(((v.vertex.z * (vs_cbuf13_0.x)) + v.offset.z));
		u_11_phi_80 = u_11_6;
	}
	// -0.26863003  <=>  ((({(vs_cbuf13_2.w) : 130.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[79].y) : 0}) + ((((({pf_0_1 : 335.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_39_phi_30) : 0.13457} * {utof(vs_cbuf9[81].z) : 0}) + ({utof(vs_cbuf9[81].x) : 0.70} + {utof(vs_cbuf9[81].z) : 0}))) * (({pf_26_4 : 1.00} * {utof(u_29_phi_24) : 0.4939}) + -0.5f)) + (({pf_26_4 : 1.00} * float(int({u_4_32 : 0}))) + (({pf_0_1 : 335.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.0001})) + (0.f - ((({utof(u_38_phi_30) : 0.13457} * {utof(vs_cbuf9[80].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 1.00} + {utof(vs_cbuf9[79].z) : 0})))))) + 0.5f))
	o.fs_attr2.z = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[79].y)) + (((((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_39_phi_30) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].x) + utof(vs_cbuf9[81].z)))) * ((pf_26_4 * utof(u_29_phi_24)) + -0.5f)) + ((pf_26_4 * float(int(u_4_32))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_38_phi_30) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))) + 0.5f));
	// 0.810065  <=>  ((({(vs_cbuf13_2.w) : 130.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_25_5 : 1.00} * {utof(u_28_phi_23) : 0.4939}) + -0.5f) * (({pf_0_1 : 335.00} * {utof(vs_cbuf9[75].z) : 0}) + (({f_0_7 : 0.31439} * {utof(vs_cbuf9[76].z) : 0.05}) + ({utof(vs_cbuf9[76].x) : 2.50} + {utof(vs_cbuf9[76].z) : 0.05})))) + (({pf_25_5 : 1.00} * float(int({u_1_15 : 0}))) + (({pf_0_1 : 335.00} * (0.f - {utof(vs_cbuf9[74].x) : 0})) + (0.f - ((({f_0_7 : 0.31439} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0})))))) + 0.5f))
	o.fs_attr2.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[74].y)) + (((((pf_25_5 * utof(u_28_phi_23)) + -0.5f) * ((pf_0_1 * utof(vs_cbuf9[75].z)) + ((f_0_7 * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z))))) + ((pf_25_5 * float(int(u_1_15))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((f_0_7 * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f));
	// 0.8950667  <=>  ((({(vs_cbuf13_2.w) : 130.00} * {(vs_cbuf16_0.z) : -53.610455}) * {utof(vs_cbuf9[84].y) : 0}) + ((((((({pf_23_12 : 1.00} * {utof(u_27_phi_25) : 0.4939}) + -0.5f) * cos({f_0_8 : -7.1287136})) + (0.f - ((({pf_19_9 : 1.00} * {utof(u_23_phi_22) : 0.98535}) + -0.5f) * sin({f_0_8 : -7.1287136})))) * (({pf_0_1 : 335.00} * {utof(vs_cbuf9[85].z) : 0}) + (({utof(u_43_phi_41) : 0.31439} * {utof(vs_cbuf9[86].z) : 0}) + ({utof(vs_cbuf9[86].x) : 1.10} + {utof(vs_cbuf9[86].z) : 0})))) + (({pf_23_12 : 1.00} * float(int({u_7_31 : 0}))) + (({pf_0_1 : 335.00} * (0.f - {utof(vs_cbuf9[84].x) : 0})) + (0.f - ((({utof(u_42_phi_41) : 0.96409} * {utof(vs_cbuf9[85].x) : 0}) * -2.f) + ({utof(vs_cbuf9[85].x) : 0} + {utof(vs_cbuf9[84].z) : 0})))))) + 0.5f))
	o.fs_attr3.x = ((((vs_cbuf13_2.w) * (vs_cbuf16_0.z)) * utof(vs_cbuf9[84].y)) + (((((((pf_23_12 * utof(u_27_phi_25)) + -0.5f) * cos(f_0_8)) + (0.f - (((pf_19_9 * utof(u_23_phi_22)) + -0.5f) * sin(f_0_8)))) * ((pf_0_1 * utof(vs_cbuf9[85].z)) + ((utof(u_43_phi_41) * utof(vs_cbuf9[86].z)) + (utof(vs_cbuf9[86].x) + utof(vs_cbuf9[86].z))))) + ((pf_23_12 * float(int(u_7_31))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[84].x))) + (0.f - (((utof(u_42_phi_41) * utof(vs_cbuf9[85].x)) * -2.f) + (utof(vs_cbuf9[85].x) + utof(vs_cbuf9[84].z))))))) + 0.5f));
	// 0.98535  <=>  (((({(vs_cbuf13_2.w) : 130.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[79].y) : 0}) + ((((({pf_0_1 : 335.00} * {utof(vs_cbuf9[80].w) : 0}) + (({utof(u_40_phi_30) : 0.96409} * {utof(vs_cbuf9[81].w) : 0}) + ({utof(vs_cbuf9[81].w) : 0} + {utof(vs_cbuf9[81].y) : 1.00}))) * (({pf_25_6 : 1.00} * {utof(u_14_phi_21) : 0.98535}) + -0.5f)) + (0.f - (({pf_25_6 : 1.00} * (0.f - float(int((({b_5_6 : False} || {b_2_18 : True}) ? {u_2_16 : 0} : 4294967295u))))) + (({pf_0_1 : 335.00} * {utof(vs_cbuf9[79].y) : 0}) + ((({utof(u_37_phi_30) : 0.96409} * {utof(vs_cbuf9[80].y) : 0}) * -2.f) + ({utof(vs_cbuf9[80].y) : 0} + {utof(vs_cbuf9[79].w) : 0})))))) + 0.5f))
	o.fs_attr2.w = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[79].y)) + (((((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_40_phi_30) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y)))) * ((pf_25_6 * utof(u_14_phi_21)) + -0.5f)) + (0.f - ((pf_25_6 * (0.f - float(int(((b_5_6 || b_2_18) ? u_2_16 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_37_phi_30) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f));
	// 3.93004  <=>  (((({(vs_cbuf13_2.w) : 130.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[74].y) : -0.0001}) + ((((({pf_0_1 : 335.00} * {utof(vs_cbuf9[75].w) : 0}) + (({f_7_0 : 0.13457} * {utof(vs_cbuf9[76].w) : 0.05}) + ({utof(vs_cbuf9[76].w) : 0.05} + {utof(vs_cbuf9[76].y) : 1.25}))) * (({pf_28_1 : 1.00} * {utof(u_3_phi_20) : 0.98535}) + -0.5f)) + (0.f - (({pf_28_1 : 1.00} * (0.f - float(int((({b_1_42 : False} || {b_0_13 : True}) ? {u_8_18 : 0} : 4294967295u))))) + (({pf_0_1 : 335.00} * {utof(vs_cbuf9[74].y) : -0.0001}) + ((({f_7_0 : 0.13457} * {utof(vs_cbuf9[75].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].y) : 1.00} + {utof(vs_cbuf9[74].w) : 0})))))) + 0.5f))
	o.fs_attr2.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[74].y)) + (((((pf_0_1 * utof(vs_cbuf9[75].w)) + ((f_7_0 * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)))) * ((pf_28_1 * utof(u_3_phi_20)) + -0.5f)) + (0.f - ((pf_28_1 * (0.f - float(int(((b_1_42 || b_0_13) ? u_8_18 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((f_7_0 * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f));
	// 0.8591664  <=>  (((({(vs_cbuf13_2.w) : 130.00} * {(vs_cbuf16_0.w) : -268.7061}) * {(vs_cbuf13_3.w) : 1.00}) * {utof(vs_cbuf9[84].y) : 0}) + ((((((({pf_23_12 : 1.00} * {utof(u_27_phi_25) : 0.4939}) + -0.5f) * sin({f_0_8 : -7.1287136})) + (cos({f_0_8 : -7.1287136}) * (({pf_19_9 : 1.00} * {utof(u_23_phi_22) : 0.98535}) + -0.5f))) * (({pf_0_1 : 335.00} * {utof(vs_cbuf9[85].w) : 0}) + (({utof(u_41_phi_41) : 0.13457} * {utof(vs_cbuf9[86].w) : 0}) + ({utof(vs_cbuf9[86].y) : 1.10} + {utof(vs_cbuf9[86].w) : 0})))) + (0.f - (({pf_19_9 : 1.00} * (0.f - float(int((({b_6_13 : False} || {b_1_46 : True}) ? {u_1_18 : 0} : 4294967295u))))) + (({pf_0_1 : 335.00} * {utof(vs_cbuf9[84].y) : 0}) + ((({utof(u_44_phi_41) : 0.31439} * {utof(vs_cbuf9[85].y) : 0}) * -2.f) + ({utof(vs_cbuf9[85].y) : 0} + {utof(vs_cbuf9[84].w) : 0})))))) + 0.5f))
	o.fs_attr3.y = (((((vs_cbuf13_2.w) * (vs_cbuf16_0.w)) * (vs_cbuf13_3.w)) * utof(vs_cbuf9[84].y)) + (((((((pf_23_12 * utof(u_27_phi_25)) + -0.5f) * sin(f_0_8)) + (cos(f_0_8) * ((pf_19_9 * utof(u_23_phi_22)) + -0.5f))) * ((pf_0_1 * utof(vs_cbuf9[85].w)) + ((utof(u_41_phi_41) * utof(vs_cbuf9[86].w)) + (utof(vs_cbuf9[86].y) + utof(vs_cbuf9[86].w))))) + (0.f - ((pf_19_9 * (0.f - float(int(((b_6_13 || b_1_46) ? u_1_18 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[84].y)) + (((utof(u_44_phi_41) * utof(vs_cbuf9[85].y)) * -2.f) + (utof(vs_cbuf9[85].y) + utof(vs_cbuf9[84].w))))))) + 0.5f));
	// 74.55017  <=>  (((1.0f / ((((({pf_5_5 : 164.3882} * 0.5f) + ({pf_6_5 : 164.5869} * 0.5f)) * (1.0f / ((0.f * {pf_5_5 : 164.3882}) + {pf_6_5 : 164.5869}))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 90.00}))
	pf_6_9 = (((1.0f / (((((pf_5_5 * 0.5f) + (pf_6_5 * 0.5f)) * (1.0f / ((0.f * pf_5_5) + pf_6_5))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x)));
	// -1.2089965  <=>  ((((1.0f / ((((({pf_5_5 : 164.3882} * 0.5f) + ({pf_6_5 : 164.5869} * 0.5f)) * (1.0f / ((0.f * {pf_5_5 : 164.3882}) + {pf_6_5 : 164.5869}))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].z) : 225.00})) * (1.0f / ({utof(vs_cbuf9[138].w) : 275.00} + (0.f - {utof(vs_cbuf9[138].z) : 225.00}))))
	pf_5_10 = ((((1.0f / (((((pf_5_5 * 0.5f) + (pf_6_5 * 0.5f)) * (1.0f / ((0.f * pf_5_5) + pf_6_5))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].z))) * (1.0f / (utof(vs_cbuf9[138].w) + (0.f - utof(vs_cbuf9[138].z)))));
	// 1.00  <=>  clamp(({pf_6_9 : 74.55017} * (1.0f / ({utof(vs_cbuf9[138].y) : 110.00} + (0.f - {utof(vs_cbuf9[138].x) : 90.00})))), 0.0, 1.0)
	f_1_162 = clamp((pf_6_9 * (1.0f / (utof(vs_cbuf9[138].y) + (0.f - utof(vs_cbuf9[138].x))))), 0.0, 1.0);
	// 1.00  <=>  ((0.f - clamp({pf_5_10 : -1.2089965}, 0.0, 1.0)) + 1.f)
	pf_5_12 = ((0.f - clamp(pf_5_10, 0.0, 1.0)) + 1.f);
	// -0.66321725  <=>  (((sin({pf_11_9 : -2.407697}) * {f_5_46 : 1.00}) * {utof(u_11_phi_80) : 0.99022}) + (((cos({pf_11_9 : -2.407697}) * {f_5_46 : 1.00}) * {utof(u_10_phi_79) : 0}) + (sin({pf_13_9 : 0}) * (0.f - {utof(u_13_phi_78) : 0.1409}))))
	o.fs_attr8.x = (((sin(pf_11_9) * f_5_46) * utof(u_11_phi_80)) + (((cos(pf_11_9) * f_5_46) * utof(u_10_phi_79)) + (sin(pf_13_9) * (0.f - utof(u_13_phi_78)))));
	// 0.1409  <=>  ((((sin({pf_13_9 : 0}) * ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697}))) + (0.f - (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0})))) * {utof(u_11_phi_80) : 0.99022}) + ((((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00})) + (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697}))) * {utof(u_10_phi_79) : 0}) + (({f_3_39 : 1.00} * {f_5_46 : 1.00}) * {utof(u_13_phi_78) : 0.1409})))
	o.fs_attr8.y = ((((sin(pf_13_9) * (f_3_39 * sin(pf_11_9))) + (0.f - (cos(pf_11_9) * sin(pf_12_8)))) * utof(u_11_phi_80)) + ((((sin(pf_13_9) * (cos(pf_11_9) * f_3_39)) + (sin(pf_12_8) * sin(pf_11_9))) * utof(u_10_phi_79)) + ((f_3_39 * f_5_46) * utof(u_13_phi_78))));
	// 1.00  <=>  (({f_1_162 : 1.00} * {pf_5_12 : 1.00}) * {(vs_cbuf10_3.x) : 1.00})
	o.fs_attr6.x = ((f_1_162 * pf_5_12) * (vs_cbuf10_3.x));
	// 27.76166  <=>  (({pf_1_19 : 165.4495} * 0.5f) + ((0.f * {pf_19_22 : 165.2508}) + ((0.f * (({pf_16_26 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[4].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[4].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_16_26 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[5].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[5].y) : 2.144507}) + ({pf_2_9 : -101.4032} * {(view_proj[5].x) : 0})))) * -0.5f))))
	o.fs_attr4.y = ((pf_1_19 * 0.5f) + ((0.f * pf_19_22) + ((0.f * ((pf_16_26 * (view_proj[4].w)) + ((pf_12_23 * (view_proj[4].z)) + ((pf_1_16 * (view_proj[4].y)) + (pf_2_9 * (view_proj[4].x)))))) + (((pf_16_26 * (view_proj[5].w)) + ((pf_12_23 * (view_proj[5].z)) + ((pf_1_16 * (view_proj[5].y)) + (pf_2_9 * (view_proj[5].x))))) * -0.5f))));
	// -0.73530847  <=>  ((((sin({pf_13_9 : 0}) * (sin({pf_12_8 : 0}) * sin({pf_11_9 : -2.407697}))) + (cos({pf_11_9 : -2.407697}) * {f_3_39 : 1.00})) * {utof(u_11_phi_80) : 0.99022}) + ((((sin({pf_13_9 : 0}) * (cos({pf_11_9 : -2.407697}) * sin({pf_12_8 : 0}))) + (0.f - ({f_3_39 : 1.00} * sin({pf_11_9 : -2.407697})))) * {utof(u_10_phi_79) : 0}) + ((sin({pf_12_8 : 0}) * {f_5_46 : 1.00}) * {utof(u_13_phi_78) : 0.1409})))
	o.fs_attr8.z = ((((sin(pf_13_9) * (sin(pf_12_8) * sin(pf_11_9))) + (cos(pf_11_9) * f_3_39)) * utof(u_11_phi_80)) + ((((sin(pf_13_9) * (cos(pf_11_9) * sin(pf_12_8))) + (0.f - (f_3_39 * sin(pf_11_9)))) * utof(u_10_phi_79)) + ((sin(pf_12_8) * f_5_46) * utof(u_13_phi_78))));
	// 0.7325432  <=>  clamp(((((({pf_1_19 : 165.4495} * 0.5f) + ((0.f * {pf_19_22 : 165.2508}) + ((0.f * (({pf_16_26 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[4].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[4].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_16_26 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[5].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[5].y) : 2.144507}) + ({pf_2_9 : -101.4032} * {(view_proj[5].x) : 0})))) * -0.5f)))) * (1.0f / ({pf_1_19 : 165.4495} + ((0.f * {pf_19_22 : 165.2508}) + {pf_24_17 : 0})))) * -0.7f) + 0.85f), 0.0, 1.0)
	f_3_95 = clamp((((((pf_1_19 * 0.5f) + ((0.f * pf_19_22) + ((0.f * ((pf_16_26 * (view_proj[4].w)) + ((pf_12_23 * (view_proj[4].z)) + ((pf_1_16 * (view_proj[4].y)) + (pf_2_9 * (view_proj[4].x)))))) + (((pf_16_26 * (view_proj[5].w)) + ((pf_12_23 * (view_proj[5].z)) + ((pf_1_16 * (view_proj[5].y)) + (pf_2_9 * (view_proj[5].x))))) * -0.5f)))) * (1.0f / (pf_1_19 + ((0.f * pf_19_22) + pf_24_17)))) * -0.7f) + 0.85f), 0.0, 1.0);
	// 0  <=>  (clamp((((min({(camera_wpos.y) : 365.7373}, {(vs_cbuf15_27.z) : 250.00}) + (0.f - {utof(u_4_phi_71) : 325.9464})) * {(vs_cbuf15_27.y) : 0.0071429}) + {(vs_cbuf15_27.x) : -0.14285715}), 0.0, 1.0) * {(vs_cbuf15_26.w) : 0.20})
	o.fs_attr10.y = (clamp((((min((camera_wpos.y), (vs_cbuf15_27.z)) + (0.f - utof(u_4_phi_71))) * (vs_cbuf15_27.y)) + (vs_cbuf15_27.x)), 0.0, 1.0) * (vs_cbuf15_26.w));
	// 21.56415  <=>  (({pf_1_19 : 165.4495} * 0.5f) + ((0.f * {pf_19_22 : 165.2508}) + (((({pf_16_26 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[4].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[4].y) : 0}) + ({pf_2_9 : -101.4032} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * (({pf_16_26 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_12_23 : -165.44946} * {(view_proj[5].z) : 0}) + (({pf_1_16 : 51.2594} * {(view_proj[5].y) : 2.144507}) + ({pf_2_9 : -101.4032} * {(view_proj[5].x) : 0}))))))))
	o.fs_attr4.x = ((pf_1_19 * 0.5f) + ((0.f * pf_19_22) + ((((pf_16_26 * (view_proj[4].w)) + ((pf_12_23 * (view_proj[4].z)) + ((pf_1_16 * (view_proj[4].y)) + (pf_2_9 * (view_proj[4].x))))) * 0.5f) + (0.f * ((pf_16_26 * (view_proj[5].w)) + ((pf_12_23 * (view_proj[5].z)) + ((pf_1_16 * (view_proj[5].y)) + (pf_2_9 * (view_proj[5].x)))))))));
	// ((0.f - {utof(u_3_50) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
	pf_0_15 = ((0.f - utof(u_3_50)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x));
	// ((0.f - {utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
	pf_1_21 = ((0.f - utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z));
	// ((0.f - {utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
	pf_2_19 = ((0.f - utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y));
	// ({utof(u_3_50) : } + (({f_3_95 : 0.7325432} * (0.f - {pf_0_15 : })) + {pf_0_15 : }))
	pf_0_17 = (utof(u_3_50) + ((f_3_95 * (0.f - pf_0_15)) + pf_0_15));
	// ({utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z) : } + (({f_3_95 : 0.7325432} * (0.f - {pf_1_21 : })) + {pf_1_21 : }))
	pf_1_23 = (utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z) + ((f_3_95 * (0.f - pf_1_21)) + pf_1_21));
	// ({utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y) : } + (({f_3_95 : 0.7325432} * (0.f - {pf_2_19 : })) + {pf_2_19 : }))
	pf_2_21 = (utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y) + ((f_3_95 * (0.f - pf_2_19)) + pf_2_19));
	// (({pf_0_17 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_0_17 : })
	o.fs_attr11.x = ((pf_0_17 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_0_17);
	// 0.8659056  <=>  exp2((log2(((0.f - clamp(((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_23.x) : 20.00}))
	f_1_170 = exp2((log2(((0.f - clamp((((f_5_52 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_23.x)));
	// (({pf_1_23 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_1_23 : })
	o.fs_attr11.z = ((pf_1_23 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_1_23);
	// 0.097269  <=>  exp2((log2(((0.f - clamp(((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_24.x) : 0.002381}) + (0.f - {(vs_cbuf15_24.y) : -0.04761905})), 0.0, 1.0)) + 1.f)) * {(vs_cbuf15_24.w) : 4.00}))
	f_2_77 = exp2((log2(((0.f - clamp((((f_5_52 * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_24.x)) + (0.f - (vs_cbuf15_24.y))), 0.0, 1.0)) + 1.f)) * (vs_cbuf15_24.w)));
	// 0.4995  <=>  ({i.vao_attr11.x : 0.999} * {(vs_cbuf13_5.z) : 0.50})
	o.fs_attr10.w = (i.vao_attr11.x * (vs_cbuf13_5.z));
	// (({pf_2_21 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_21 : })
	o.fs_attr11.y = ((pf_2_21 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_21);
	// 0.1139803  <=>  clamp((({f_1_170 : 0.8659056} * (0.f - {(vs_cbuf15_23.z) : 0.85})) + {(vs_cbuf15_23.z) : 0.85}), 0.0, 1.0)
	f_0_84 = clamp(((f_1_170 * (0.f - (vs_cbuf15_23.z))) + (vs_cbuf15_23.z)), 0.0, 1.0);
	// 0.6325088  <=>  (({f_2_77 : 0.097269} * (0.f - {(vs_cbuf15_25.w) : 0.7006614})) + {(vs_cbuf15_25.w) : 0.7006614})
	o.fs_attr10.x = ((f_2_77 * (0.f - (vs_cbuf15_25.w))) + (vs_cbuf15_25.w));
	// 0.1139803  <=>  clamp((((({i.vao_attr11.x : 0.999} * {(vs_cbuf13_5.z) : 0.50}) * clamp(((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * -0.02f) + 1.f), 0.0, 1.0)) * (0.f - {f_0_84 : 0.1139803})) + {f_0_84 : 0.1139803}), 0.0, 1.0)
	o.fs_attr12.w = clamp(((((i.vao_attr11.x * (vs_cbuf13_5.z)) * clamp((((f_5_52 * (0.f - (vs_cbuf8_30.z))) * -0.02f) + 1.f), 0.0, 1.0)) * (0.f - f_0_84)) + f_0_84), 0.0, 1.0);
	// 0.50  <=>  ({f4_0_0.x : 0.50} * clamp(max((((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0))
	o.fs_attr12.x = (f4_0_0.x * clamp(max(((((f_5_52 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0));
	// 0.50  <=>  ({f4_0_0.y : 0.50} * clamp(max((((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0))
	o.fs_attr12.y = (f4_0_0.y * clamp(max(((((f_5_52 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0));
	// 0.50  <=>  ({f4_0_0.z : 0.50} * clamp(max((((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0))
	o.fs_attr12.z = (f4_0_0.z * clamp(max(((((f_5_52 * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0));
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
		f_0_87 = (1.0f / (vs_cbuf15_51.x));
		// 1.00  <=>  ((({utof(u_1_phi_77) : 0.50} * {(vs_cbuf15_49.x) : 0}) + (0.f - {(vs_cbuf15_49.x) : 0})) + 1.f)
		pf_0_26 = (((utof(u_1_phi_77) * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f);
		// 0.1215186  <=>  clamp(((({f_5_52 : -0.06617705} * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {f_0_87 : 0.0010526}) + (0.f - ({f_0_87 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
		f_0_88 = clamp((((f_5_52 * (0.f - (vs_cbuf8_30.z))) * f_0_87) + (0.f - (f_0_87 * (vs_cbuf15_51.y)))), 0.0, 1.0);
		// -∞  <=>  log2(abs((({pf_0_26 : 1.00} * (0.f - {f_0_88 : 0.1215186})) + {f_0_88 : 0.1215186})))
		f_0_90 = log2(abs(((pf_0_26 * (0.f - f_0_88)) + f_0_88)));
		// 0  <=>  exp2(({f_0_90 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
		f_0_91 = exp2((f_0_90 * (vs_cbuf15_51.z)));
		// 1.00  <=>  (({pf_0_26 : 1.00} * (0.f - (({f_0_91 : 0} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0}))) + {pf_0_26 : 1.00})
		o.fs_attr9.x = ((pf_0_26 * (0.f - ((f_0_91 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_26);
	}
	// True  <=>  ((! (((({pf_5_12 : 1.00} <= 0.f) && (! myIsNaN({pf_5_12 : 1.00}))) && (! myIsNaN(0.f))) || ((({f_1_162 : 1.00} <= 0.f) && (! myIsNaN({f_1_162 : 1.00}))) && (! myIsNaN(0.f))))) ? true : false)
	b_1_67 = ((! ((((pf_5_12 <= 0.f) && (! myIsNaN(pf_5_12))) && (! myIsNaN(0.f))) || (((f_1_162 <= 0.f) && (! myIsNaN(f_1_162))) && (! myIsNaN(0.f))))) ? true : false);
	// True  <=>  if({b_1_67 : True})
	if(b_1_67)
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
