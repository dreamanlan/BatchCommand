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
	// vs_cbuf9[1] = float4(1143217000000000000000000000.00, 4.465202E+30, 1.4E-43, 0);
	// vs_cbuf9[2] = float4(0, 0, 0, 0);
	// vs_cbuf9[3] = float4(0, 0, 0, 0);
	// vs_cbuf9[4] = float4(0, 0, 0, 0);
	// vs_cbuf9[5] = float4(0, 0, 0, 0);
	// vs_cbuf9[6] = float4(0, 0, 0, 0);
	// vs_cbuf9[7] = float4(1E-45, 1.469368E-39, 4.2187E-40, 0);
	// vs_cbuf9[8] = float4(0, 6E-45, 0, 6E-45);
	// vs_cbuf9[9] = float4(3E-45, 0, 0, 0);
	// vs_cbuf9[10] = float4(0, 0, 0, 0);
	// vs_cbuf9[11] = float4(0, 0, 0, 0);
	// vs_cbuf9[12] = float4(0, 0, 0, 0);
	// vs_cbuf9[13] = float4(0, 0, 0, 0);
	// vs_cbuf9[14] = float4(0, -1, 0, 0);
	// vs_cbuf9[15] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[16] = float4(0, 0.20, 0, 15.00);
	// vs_cbuf9[17] = float4(1.00, 1.00, 20.00, 20.00);
	// vs_cbuf9[18] = float4(0, 0, 0, 0);
	// vs_cbuf9[19] = float4(0.001, 0.05, 0, 0);
	// vs_cbuf9[20] = float4(2.00, 1.00, 2.00, 1.00);
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
	// vs_cbuf9[74] = float4(0.001, 0, 0, 0);
	// vs_cbuf9[75] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[76] = float4(1.40, 1.00, 0, 0);
	// vs_cbuf9[77] = float4(0, 0, 0, 0);
	// vs_cbuf9[78] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[79] = float4(0.001, -0.0007, 0, 0);
	// vs_cbuf9[80] = float4(1.00, 1.00, 0, 0);
	// vs_cbuf9[81] = float4(3.20, 2.40, 0.10, 0.10);
	// vs_cbuf9[82] = float4(0, 0, 0, 0);
	// vs_cbuf9[83] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf9[84] = float4(0, 0, 0, 3.00);
	// vs_cbuf9[85] = float4(0, 0, 0, 0);
	// vs_cbuf9[86] = float4(1.00, 0.50, 0, 0);
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
	// vs_cbuf9[104] = float4(0.25, 0, 0, 0);
	// vs_cbuf9[105] = float4(0.8475056, 0.9126984, 0.9111589, 0);
	// vs_cbuf9[106] = float4(0, 0, 0, 0);
	// vs_cbuf9[107] = float4(0, 0, 0, 0);
	// vs_cbuf9[108] = float4(0, 0, 0, 0);
	// vs_cbuf9[109] = float4(0, 0, 0, 0);
	// vs_cbuf9[110] = float4(0, 0, 0, 0);
	// vs_cbuf9[111] = float4(0, 0, 0, 0);
	// vs_cbuf9[112] = float4(0, 0, 0, 0);
	// vs_cbuf9[113] = float4(0, 0, 0, 0);
	// vs_cbuf9[114] = float4(1.00, 1.00, 1.00, 0.21);
	// vs_cbuf9[115] = float4(1.00, 1.00, 1.00, 0.84);
	// vs_cbuf9[116] = float4(0, 0, 0, 1.00);
	// vs_cbuf9[117] = float4(0, 0, 0, 5.00);
	// vs_cbuf9[118] = float4(0, 0, 0, 6.00);
	// vs_cbuf9[119] = float4(0, 0, 0, 7.00);
	// vs_cbuf9[120] = float4(0, 0, 0, 8.00);
	// vs_cbuf9[121] = float4(0.4964097, 0.5333842, 0.5634921, 0);
	// vs_cbuf9[122] = float4(0, 0, 0, 0);
	// vs_cbuf9[123] = float4(0, 0, 0, 0);
	// vs_cbuf9[124] = float4(0, 0, 0, 0);
	// vs_cbuf9[125] = float4(0, 0, 0, 0);
	// vs_cbuf9[126] = float4(0, 0, 0, 0);
	// vs_cbuf9[127] = float4(0, 0, 0, 0);
	// vs_cbuf9[128] = float4(0, 0, 0, 0);
	// vs_cbuf9[129] = float4(2.00, 0, 0, 0);
	// vs_cbuf9[130] = float4(0.20, 0.20, 0.20, 0.33);
	// vs_cbuf9[131] = float4(0.20, 0.20, 0.20, 0.68);
	// vs_cbuf9[132] = float4(0, 0, 0, 1.00);
	// vs_cbuf9[133] = float4(0, 0, 0, 5.00);
	// vs_cbuf9[134] = float4(0, 0, 0, 6.00);
	// vs_cbuf9[135] = float4(0, 0, 0, 7.00);
	// vs_cbuf9[136] = float4(0, 0, 0, 8.00);
	// vs_cbuf9[137] = float4(0, 0.50, 0.20, 1.00);
	// vs_cbuf9[138] = float4(100.00, 200.00, 800.00, 1000.00);
	// vs_cbuf9[139] = float4(1.00, 0, 0, 0);
	// vs_cbuf9[140] = float4(0, 100.00, 0, 0);
	// vs_cbuf9[141] = float4(1.13, 1.13, 1.13, 0);
	// vs_cbuf9[142] = float4(1.26, 1.26, 1.26, 1.00);
	// vs_cbuf9[143] = float4(1.26, 1.26, 1.26, 3.00);
	// vs_cbuf9[144] = float4(1.26, 1.26, 1.26, 4.00);
	// vs_cbuf9[145] = float4(1.26, 1.26, 1.26, 5.00);
	// vs_cbuf9[146] = float4(1.26, 1.26, 1.26, 6.00);
	// vs_cbuf9[147] = float4(1.26, 1.26, 1.26, 7.00);
	// vs_cbuf9[148] = float4(1.26, 1.26, 1.26, 8.00);
	// vs_cbuf9[149] = float4(0, 0, 0, 0);
	// vs_cbuf9[150] = float4(0, 0, 0, 0);
	// vs_cbuf9[151] = float4(0, 0, 0, 0);
	// vs_cbuf9[152] = float4(0, 0, 0, 0);
	// vs_cbuf9[153] = float4(0, 0, 0, 0);
	// vs_cbuf9[154] = float4(0, 0, 0, 0);
	// vs_cbuf9[155] = float4(0, 0, 0, 0);
	// vs_cbuf9[156] = float4(0, 0, 0, 0);
	// vs_cbuf9[157] = float4(0, 0, 0, 0);
	// vs_cbuf9[158] = float4(0, 0, 0, 0);
	// vs_cbuf9[159] = float4(0, 0, 0, 0);
	// vs_cbuf9[160] = float4(0, 0, 0, 0);
	// vs_cbuf9[197] = float4(400.00, 50.00, 0, 0);
	// vs_cbuf10[0] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[1] = float4(1.00, 1.00, 1.00, 1.00);
	// vs_cbuf10[2] = float4(999.50, 6.00, 1.00, 1.00);
	// vs_cbuf10[3] = float4(1.00, 1.00, 1.00, 1.00);
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

	bool b_0_25;
	bool b_0_27;
	bool b_0_29;
	bool b_1_58;
	bool b_1_60;
	bool b_2_24;
	bool b_2_26;
	bool b_3_0;
	bool b_3_1;
	bool b_3_10;
	bool b_3_12;
	bool b_3_phi_33;
	bool b_4_4;
	bool b_4_6;
	bool b_5_10;
	float f_0_34;
	float f_0_40;
	float f_0_45;
	float f_0_50;
	float f_0_51;
	float f_0_53;
	float f_0_54;
	float f_1_71;
	float f_10_25;
	float f_10_43;
	float f_11_15;
	float f_11_16;
	float f_11_19;
	float f_11_20;
	float f_12_18;
	float f_12_22;
	float f_12_28;
	float f_13_5;
	float f_13_8;
	float f_2_19;
	float f_3_38;
	float f_3_55;
	float f_4_14;
	float f_4_16;
	float f_4_18;
	float f_4_25;
	float f_4_27;
	float f_4_6;
	float f_4_78;
	float f_4_8;
	float f_4_84;
	float f_4_85;
	float f_5_0;
	float f_6_38;
	float f_7_0;
	float f_9_1;
	float f_9_24;
	float4 f4_0_0;
	float4 f4_0_1;
	precise float pf_0_1;
	precise float pf_0_11;
	precise float pf_0_21;
	precise float pf_0_25;
	precise float pf_0_7;
	precise float pf_1_1;
	precise float pf_1_16;
	precise float pf_1_20;
	precise float pf_1_24;
	precise float pf_1_25;
	precise float pf_1_35;
	precise float pf_1_6;
	precise float pf_10_10;
	precise float pf_10_14;
	precise float pf_11_17;
	precise float pf_12_0;
	precise float pf_12_8;
	precise float pf_12_9;
	precise float pf_13_9;
	precise float pf_14_6;
	precise float pf_15_10;
	precise float pf_15_20;
	precise float pf_15_21;
	precise float pf_15_31;
	precise float pf_15_4;
	precise float pf_16_7;
	precise float pf_17_10;
	precise float pf_18_3;
	precise float pf_18_5;
	precise float pf_18_7;
	precise float pf_18_8;
	precise float pf_2_12;
	precise float pf_2_28;
	precise float pf_2_48;
	precise float pf_2_55;
	precise float pf_2_63;
	precise float pf_2_65;
	precise float pf_2_69;
	precise float pf_2_70;
	precise float pf_2_75;
	precise float pf_2_79;
	precise float pf_2_81;
	precise float pf_20_11;
	precise float pf_20_8;
	precise float pf_21_8;
	precise float pf_22_1;
	precise float pf_24_2;
	precise float pf_24_9;
	precise float pf_25_4;
	precise float pf_26_0;
	precise float pf_3_12;
	precise float pf_3_14;
	precise float pf_3_3;
	precise float pf_3_7;
	precise float pf_4_11;
	precise float pf_4_3;
	precise float pf_4_7;
	precise float pf_4_8;
	precise float pf_5_12;
	precise float pf_5_16;
	precise float pf_5_20;
	precise float pf_5_26;
	precise float pf_5_3;
	precise float pf_5_31;
	precise float pf_5_35;
	precise float pf_5_8;
	precise float pf_6_13;
	precise float pf_6_21;
	precise float pf_6_30;
	precise float pf_6_33;
	precise float pf_6_39;
	precise float pf_6_4;
	precise float pf_6_9;
	precise float pf_7_20;
	precise float pf_7_28;
	precise float pf_7_30;
	precise float pf_7_34;
	precise float pf_7_38;
	precise float pf_8_11;
	precise float pf_8_12;
	precise float pf_8_7;
	precise float pf_9_14;
	uint u_0_1;
	uint u_0_11;
	uint u_0_12;
	uint u_0_13;
	uint u_0_15;
	uint u_0_19;
	uint u_0_2;
	uint u_0_21;
	uint u_0_22;
	uint u_0_24;
	uint u_0_27;
	uint u_0_3;
	uint u_0_31;
	uint u_0_32;
	uint u_0_34;
	uint u_0_4;
	uint u_0_7;
	uint u_0_8;
	uint u_0_phi_22;
	uint u_0_phi_29;
	uint u_0_phi_69;
	uint u_0_phi_71;
	uint u_1_1;
	uint u_1_10;
	uint u_1_2;
	uint u_1_6;
	uint u_1_7;
	uint u_1_8;
	uint u_1_9;
	uint u_1_phi_21;
	uint u_1_phi_39;
	uint u_1_phi_40;
	uint u_10_12;
	uint u_10_13;
	uint u_10_15;
	uint u_10_16;
	uint u_10_19;
	uint u_10_20;
	uint u_10_6;
	uint u_10_7;
	uint u_10_8;
	uint u_10_9;
	uint u_10_phi_51;
	uint u_10_phi_70;
	uint u_10_phi_74;
	uint u_10_phi_79;
	uint u_11_0;
	uint u_11_1;
	uint u_11_10;
	uint u_11_14;
	uint u_11_15;
	uint u_11_3;
	uint u_11_4;
	uint u_11_5;
	uint u_11_6;
	uint u_11_7;
	uint u_11_8;
	uint u_11_phi_19;
	uint u_11_phi_2;
	uint u_11_phi_26;
	uint u_11_phi_31;
	uint u_11_phi_70;
	uint u_12_10;
	uint u_12_2;
	uint u_12_3;
	uint u_12_5;
	uint u_12_6;
	uint u_12_7;
	uint u_12_8;
	uint u_12_9;
	uint u_12_phi_28;
	uint u_12_phi_31;
	uint u_12_phi_35;
	uint u_12_phi_9;
	uint u_13_14;
	uint u_13_16;
	uint u_13_17;
	uint u_13_19;
	uint u_13_20;
	uint u_13_3;
	uint u_13_4;
	uint u_13_7;
	uint u_13_phi_32;
	uint u_13_phi_72;
	uint u_13_phi_77;
	uint u_14_0;
	uint u_14_1;
	uint u_14_10;
	uint u_14_13;
	uint u_14_17;
	uint u_14_19;
	uint u_14_7;
	uint u_14_8;
	uint u_14_9;
	uint u_14_phi_15;
	uint u_14_phi_31;
	uint u_14_phi_35;
	uint u_15_10;
	uint u_15_6;
	uint u_15_7;
	uint u_15_phi_32;
	uint u_16_14;
	uint u_16_15;
	uint u_16_17;
	uint u_16_3;
	uint u_16_4;
	uint u_16_phi_33;
	uint u_17_1;
	uint u_17_16;
	uint u_17_2;
	uint u_17_25;
	uint u_17_26;
	uint u_17_30;
	uint u_17_31;
	uint u_17_39;
	uint u_17_40;
	uint u_17_phi_33;
	uint u_18_1;
	uint u_18_11;
	uint u_18_12;
	uint u_18_2;
	uint u_18_20;
	uint u_18_21;
	uint u_18_22;
	uint u_18_23;
	uint u_18_phi_33;
	uint u_19_1;
	uint u_19_10;
	uint u_19_11;
	uint u_19_12;
	uint u_19_13;
	uint u_19_14;
	uint u_19_16;
	uint u_19_2;
	uint u_19_3;
	uint u_19_4;
	uint u_19_7;
	uint u_19_8;
	uint u_19_9;
	uint u_19_phi_33;
	uint u_19_phi_37;
	uint u_19_phi_47;
	uint u_19_phi_56;
	uint u_19_phi_60;
	uint u_19_phi_66;
	uint u_2_1;
	uint u_2_2;
	uint u_2_4;
	uint u_2_5;
	uint u_2_7;
	uint u_2_8;
	uint u_2_phi_26;
	uint u_2_phi_75;
	uint u_2_phi_78;
	uint u_20_0;
	uint u_20_1;
	uint u_20_2;
	uint u_20_3;
	uint u_20_6;
	uint u_20_7;
	uint u_20_8;
	uint u_20_phi_33;
	uint u_20_phi_37;
	uint u_20_phi_44;
	uint u_21_1;
	uint u_21_12;
	uint u_21_15;
	uint u_21_17;
	uint u_21_18;
	uint u_21_19;
	uint u_21_2;
	uint u_21_20;
	uint u_21_7;
	uint u_21_phi_35;
	uint u_22_0;
	uint u_22_1;
	uint u_22_14;
	uint u_22_15;
	uint u_22_2;
	uint u_22_25;
	uint u_22_3;
	uint u_22_7;
	uint u_22_phi_34;
	uint u_22_phi_36;
	uint u_23_0;
	uint u_23_1;
	uint u_23_14;
	uint u_23_18;
	uint u_23_19;
	uint u_23_2;
	uint u_23_3;
	uint u_23_5;
	uint u_23_phi_34;
	uint u_23_phi_36;
	uint u_24_0;
	uint u_24_1;
	uint u_24_2;
	uint u_24_3;
	uint u_24_7;
	uint u_24_8;
	uint u_24_phi_34;
	uint u_24_phi_36;
	uint u_24_phi_45;
	uint u_25_0;
	uint u_25_1;
	uint u_25_2;
	uint u_25_3;
	uint u_25_6;
	uint u_25_7;
	uint u_25_phi_34;
	uint u_25_phi_36;
	uint u_25_phi_46;
	uint u_26_1;
	uint u_26_2;
	uint u_26_5;
	uint u_26_phi_36;
	uint u_27_14;
	uint u_27_15;
	uint u_27_17;
	uint u_27_18;
	uint u_27_2;
	uint u_27_3;
	uint u_27_6;
	uint u_27_8;
	uint u_27_phi_37;
	uint u_28_5;
	uint u_29_4;
	uint u_29_6;
	uint u_29_7;
	uint u_29_phi_48;
	uint u_3_1;
	uint u_3_10;
	uint u_3_11;
	uint u_3_12;
	uint u_3_17;
	uint u_3_18;
	uint u_3_19;
	uint u_3_2;
	uint u_3_21;
	uint u_3_25;
	uint u_3_27;
	uint u_3_3;
	uint u_3_4;
	uint u_3_6;
	uint u_3_7;
	uint u_3_8;
	uint u_3_phi_25;
	uint u_3_phi_27;
	uint u_3_phi_38;
	uint u_3_phi_41;
	uint u_30_7;
	uint u_30_8;
	uint u_30_phi_49;
	uint u_31_3;
	uint u_32_0;
	uint u_33_0;
	uint u_33_1;
	uint u_33_phi_50;
	uint u_34_1;
	uint u_34_2;
	uint u_34_phi_52;
	uint u_35_0;
	uint u_35_1;
	uint u_35_14;
	uint u_35_4;
	uint u_35_phi_53;
	uint u_36_10;
	uint u_36_12;
	uint u_36_4;
	uint u_36_7;
	uint u_37_3;
	uint u_38_11;
	uint u_38_12;
	uint u_38_13;
	uint u_38_6;
	uint u_38_phi_61;
	uint u_39_10;
	uint u_39_3;
	uint u_39_9;
	uint u_39_phi_62;
	uint u_4_1;
	uint u_4_2;
	uint u_4_4;
	uint u_4_5;
	uint u_4_6;
	uint u_4_7;
	uint u_4_phi_23;
	uint u_4_phi_28;
	uint u_4_phi_30;
	uint u_40_10;
	uint u_40_11;
	uint u_40_2;
	uint u_40_3;
	uint u_40_9;
	uint u_40_phi_54;
	uint u_40_phi_63;
	uint u_41_0;
	uint u_41_1;
	uint u_41_phi_55;
	uint u_42_0;
	uint u_42_1;
	uint u_42_phi_55;
	uint u_43_0;
	uint u_43_1;
	uint u_43_phi_55;
	uint u_44_0;
	uint u_44_1;
	uint u_44_2;
	uint u_44_3;
	uint u_44_4;
	uint u_44_phi_55;
	uint u_44_phi_64;
	uint u_45_0;
	uint u_45_1;
	uint u_45_2;
	uint u_45_3;
	uint u_45_phi_57;
	uint u_45_phi_65;
	uint u_46_0;
	uint u_46_1;
	uint u_46_2;
	uint u_46_3;
	uint u_46_phi_58;
	uint u_46_phi_65;
	uint u_47_0;
	uint u_47_1;
	uint u_47_2;
	uint u_47_3;
	uint u_47_phi_59;
	uint u_47_phi_65;
	uint u_48_0;
	uint u_48_1;
	uint u_48_phi_65;
	uint u_49_0;
	uint u_49_1;
	uint u_49_phi_68;
	uint u_5_1;
	uint u_5_2;
	uint u_5_3;
	uint u_5_4;
	uint u_5_5;
	uint u_5_6;
	uint u_5_phi_24;
	uint u_5_phi_28;
	uint u_5_phi_30;
	uint u_6_1;
	uint u_6_2;
	uint u_6_3;
	uint u_6_4;
	uint u_6_6;
	uint u_6_phi_18;
	uint u_6_phi_30;
	uint u_7_1;
	uint u_7_10;
	uint u_7_13;
	uint u_7_14;
	uint u_7_17;
	uint u_7_19;
	uint u_7_2;
	uint u_7_23;
	uint u_7_24;
	uint u_7_26;
	uint u_7_7;
	uint u_7_8;
	uint u_7_9;
	uint u_7_phi_17;
	uint u_7_phi_26;
	uint u_7_phi_31;
	uint u_7_phi_40;
	uint u_7_phi_71;
	uint u_8_1;
	uint u_8_10;
	uint u_8_12;
	uint u_8_14;
	uint u_8_17;
	uint u_8_18;
	uint u_8_2;
	uint u_8_3;
	uint u_8_4;
	uint u_8_5;
	uint u_8_6;
	uint u_8_9;
	uint u_8_phi_16;
	uint u_8_phi_20;
	uint u_8_phi_31;
	uint u_8_phi_67;
	uint u_8_phi_71;
	uint u_9_1;
	uint u_9_11;
	uint u_9_19;
	uint u_9_2;
	uint u_9_21;
	uint u_9_22;
	uint u_9_23;
	uint u_9_26;
	uint u_9_27;
	uint u_9_29;
	uint u_9_30;
	uint u_9_4;
	uint u_9_5;
	uint u_9_7;
	uint u_9_8;
	uint u_9_phi_11;
	uint u_9_phi_32;
	uint u_9_phi_4;
	uint u_9_phi_70;
	uint u_9_phi_73;
	uint u_9_phi_76;
	// -914.91187  <=>  float(-914.91187)
	o.vertex.x = float(-914.91187);
	// 28.82728  <=>  float(28.82728)
	o.vertex.y = float(28.82728);
	// 540.0519  <=>  float(540.05188)
	o.vertex.z = float(540.05188);
	// 540.2476  <=>  float(540.24756)
	o.vertex.w = float(540.24756);
	// 0.21188  <=>  float(0.21188)
	o.fs_attr0.x = float(0.21188);
	// 0.22817  <=>  float(0.22817)
	o.fs_attr0.y = float(0.22817);
	// 0.22779  <=>  float(0.22779)
	o.fs_attr0.z = float(0.22779);
	// 1.00  <=>  float(1.00)
	o.fs_attr0.w = float(1.00);
	// 0.1241  <=>  float(0.1241)
	o.fs_attr1.x = float(0.1241);
	// 0.13335  <=>  float(0.13335)
	o.fs_attr1.y = float(0.13335);
	// 0.14087  <=>  float(0.14087)
	o.fs_attr1.z = float(0.14087);
	// 1.00  <=>  float(1.00)
	o.fs_attr1.w = float(1.00);
	// 0.9785  <=>  float(0.9785)
	o.fs_attr2.x = float(0.9785);
	// 0.50196  <=>  float(0.50196)
	o.fs_attr2.y = float(0.50196);
	// -0.05795  <=>  float(-0.05795)
	o.fs_attr2.z = float(-0.05795);
	// -0.30511  <=>  float(-0.30511)
	o.fs_attr2.w = float(-0.30511);
	// 0.50196  <=>  float(0.50196)
	o.fs_attr3.x = float(0.50196);
	// -2.49902  <=>  float(-2.49902)
	o.fs_attr3.y = float(-2.49902);
	// 0  <=>  float(0.00)
	o.fs_attr3.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr3.w = float(1.00);
	// -187.33215  <=>  float(-187.33215)
	o.fs_attr4.x = float(-187.33215);
	// 255.7101  <=>  float(255.71014)
	o.fs_attr4.y = float(255.71014);
	// 540.1497  <=>  float(540.14972)
	o.fs_attr4.z = float(540.14972);
	// 540.2476  <=>  float(540.24756)
	o.fs_attr4.w = float(540.24756);
	// 0.3291  <=>  float(0.3291)
	o.fs_attr5.x = float(0.3291);
	// 0  <=>  float(0.00)
	o.fs_attr5.y = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr5.z = float(0.00);
	// 1.00  <=>  float(1.00)
	o.fs_attr5.w = float(1.00);
	// -0.66977  <=>  float(-0.66977)
	o.fs_attr6.x = float(-0.66977);
	// 0  <=>  float(0.00)
	o.fs_attr6.y = float(0.00);
	// -0.74257  <=>  float(-0.74257)
	o.fs_attr6.z = float(-0.74257);
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
	// 0.1812  <=>  float(0.1812)
	o.fs_attr8.y = float(0.1812);
	// 0  <=>  float(0.00)
	o.fs_attr8.z = float(0.00);
	// 0  <=>  float(0.00)
	o.fs_attr8.w = float(0.00);
	// 0.08741  <=>  float(0.08741)
	o.fs_attr9.x = float(0.08741);
	// 0.08741  <=>  float(0.08741)
	o.fs_attr9.y = float(0.08741);
	// 0.08741  <=>  float(0.08741)
	o.fs_attr9.z = float(0.08741);
	// 1.00  <=>  float(1.00)
	o.fs_attr9.w = float(1.00);
	// 0.00111  <=>  float(0.00111)
	o.fs_attr10.x = float(0.00111);
	// 0.00877  <=>  float(0.00877)
	o.fs_attr10.y = float(0.00877);
	// 0.00901  <=>  float(0.00901)
	o.fs_attr10.z = float(0.00901);
	// 0.27842  <=>  float(0.27842)
	o.fs_attr10.w = float(0.27842);
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 0  <=>  0u
	u_11_0 = 0u;
	u_11_phi_2 = u_11_0;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_11_1 = ftou(vs_cbuf8_30.y);
		u_11_phi_2 = u_11_1;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 0  <=>  {u_11_phi_2 : 0}
	u_9_1 = u_11_phi_2;
	u_9_phi_4 = u_9_1;
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_11_phi_2) : 0} * 5.f)) : 0}
		u_9_2 = ftou((utof(u_11_phi_2) * 5.f));
		u_9_phi_4 = u_9_2;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr5.x = 0.f;
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		// 0  <=>  {utof(u_9_phi_4) : 0}
		o.vertex.z = utof(u_9_phi_4);
	}
	// False  <=>  if(((int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	if(((int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))) <= int(0u)) ? true : false))
	{
		return;
	}
	// 163.00  <=>  ((0.f - {i.vao_attr5.w : 836.50}) + {(vs_cbuf10_2.x) : 999.50})
	pf_0_1 = ((0.f - i.vao_attr5.w) + (vs_cbuf10_2.x));
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.x = 0.f;
	}
	// 1146167296  <=>  {ftou(i.vao_attr5.w) : 1146167296}
	u_12_2 = ftou(i.vao_attr5.w);
	u_12_phi_9 = u_12_2;
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1187205120  <=>  ftou({vs_cbuf8_30.y : 25000.00})
		u_12_3 = ftou(vs_cbuf8_30.y);
		u_12_phi_9 = u_12_3;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.vertex.y = 0.f;
	}
	// 1146167296  <=>  {u_12_phi_9 : 1146167296}
	u_9_4 = u_12_phi_9;
	u_9_phi_11 = u_9_4;
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 1166193664  <=>  {ftou(({utof(u_12_phi_9) : 836.50} * 5.f)) : 1166193664}
		u_9_5 = ftou((utof(u_12_phi_9) * 5.f));
		u_9_phi_11 = u_9_5;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 0  <=>  0.f
		o.fs_attr5.x = 0.f;
	}
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		// 836.50  <=>  {utof(u_9_phi_11) : 836.50}
		o.vertex.z = utof(u_9_phi_11);
	}
	// False  <=>  if(((((({i.vao_attr5.w : 836.50} > {(vs_cbuf10_2.x) : 999.50}) && (! myIsNaN({i.vao_attr5.w : 836.50}))) && (! myIsNaN({(vs_cbuf10_2.x) : 999.50}))) || ((({pf_0_1 : 163.00} >= float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN({pf_0_1 : 163.00}))) && (! myIsNaN(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	if((((((i.vao_attr5.w > (vs_cbuf10_2.x)) && (! myIsNaN(i.vao_attr5.w))) && (! myIsNaN((vs_cbuf10_2.x)))) || (((pf_0_1 >= float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f))))))) && (! myIsNaN(pf_0_1))) && (! myIsNaN(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))))) ? true : false))
	{
		return;
	}
	// 164.00  <=>  ({pf_0_1 : 163.00} + {(vs_cbuf10_2.w) : 1.00})
	pf_1_1 = (pf_0_1 + (vs_cbuf10_2.w));
	// 0  <=>  {i.vao_attr11.y : 0}
	f_5_0 = i.vao_attr11.y;
	// 0  <=>  0u
	u_14_0 = 0u;
	u_14_phi_15 = u_14_0;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1188008448  <=>  {ftou(({pf_0_1 : 163.00} * {pf_0_1 : 163.00})) : 1188008448}
		u_14_1 = ftou((pf_0_1 * pf_0_1));
		u_14_phi_15 = u_14_1;
	}
	// 1.00  <=>  {i.vao_attr10.y : 1.00}
	f_7_0 = i.vao_attr10.y;
	// 0  <=>  0u
	u_8_1 = 0u;
	u_8_phi_16 = u_8_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1188175872  <=>  {ftou(({pf_1_1 : 164.00} * {pf_1_1 : 164.00})) : 1188175872}
		u_8_2 = ftou((pf_1_1 * pf_1_1));
		u_8_phi_16 = u_8_2;
	}
	// 0  <=>  0u
	u_7_1 = 0u;
	u_7_phi_17 = u_7_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_14_phi_15) : 26569.00} * {utof(vs_cbuf9[14].w) : 0})) : 0}
		u_7_2 = ftou((utof(u_14_phi_15) * utof(vs_cbuf9[14].w)));
		u_7_phi_17 = u_7_2;
	}
	// 0  <=>  {i.vao_attr9.y : 0}
	f_9_1 = i.vao_attr9.y;
	// 1188008448  <=>  {u_14_phi_15 : 1188008448}
	u_6_1 = u_14_phi_15;
	u_6_phi_18 = u_6_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 1126367232  <=>  {ftou(pf_0_1) : 1126367232}
		u_6_2 = ftou(pf_0_1);
		u_6_phi_18 = u_6_2;
	}
	// 1188175872  <=>  {u_8_phi_16 : 1188175872}
	u_11_3 = u_8_phi_16;
	u_11_phi_19 = u_11_3;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou(({utof(u_8_phi_16) : 26896.00} * {utof(vs_cbuf9[14].w) : 0})) : 0}
		u_11_4 = ftou((utof(u_8_phi_16) * utof(vs_cbuf9[14].w)));
		u_11_phi_19 = u_11_4;
	}
	// 0  <=>  0u
	u_8_3 = 0u;
	u_8_phi_20 = u_8_3;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 2147483648  <=>  {ftou((({utof(u_7_phi_17) : 0} * 0.5f) * {utof(vs_cbuf9[14].y) : -1})) : 2147483648}
		u_8_4 = ftou(((utof(u_7_phi_17) * 0.5f) * utof(vs_cbuf9[14].y)));
		u_8_phi_20 = u_8_4;
	}
	// 0  <=>  0u
	u_1_1 = 0u;
	u_1_phi_21 = u_1_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_7_phi_17) : 0} * 0.5f) * {utof(vs_cbuf9[14].z) : 0})) : 0}
		u_1_2 = ftou(((utof(u_7_phi_17) * 0.5f) * utof(vs_cbuf9[14].z)));
		u_1_phi_21 = u_1_2;
	}
	// 0  <=>  0u
	u_0_1 = 0u;
	u_0_phi_22 = u_0_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_7_phi_17) : 0} * 0.5f) * {utof(vs_cbuf9[14].x) : 0})) : 0}
		u_0_2 = ftou(((utof(u_7_phi_17) * 0.5f) * utof(vs_cbuf9[14].x)));
		u_0_phi_22 = u_0_2;
	}
	// 0  <=>  0u
	u_4_1 = 0u;
	u_4_phi_23 = u_4_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_11_phi_19) : 0} * 0.5f) * {utof(vs_cbuf9[14].x) : 0})) : 0}
		u_4_2 = ftou(((utof(u_11_phi_19) * 0.5f) * utof(vs_cbuf9[14].x)));
		u_4_phi_23 = u_4_2;
	}
	// 0  <=>  0u
	u_5_1 = 0u;
	u_5_phi_24 = u_5_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 2147483648  <=>  {ftou((({utof(u_11_phi_19) : 0} * 0.5f) * {utof(vs_cbuf9[14].y) : -1})) : 2147483648}
		u_5_2 = ftou(((utof(u_11_phi_19) * 0.5f) * utof(vs_cbuf9[14].y)));
		u_5_phi_24 = u_5_2;
	}
	// 0  <=>  0u
	u_3_1 = 0u;
	u_3_phi_25 = u_3_1;
	// True  <=>  if((((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f))) ? true : false))
	if(((((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f))) ? true : false))
	{
		// 0  <=>  {ftou((({utof(u_11_phi_19) : 0} * 0.5f) * {utof(vs_cbuf9[14].z) : 0})) : 0}
		u_3_2 = ftou(((utof(u_11_phi_19) * 0.5f) * utof(vs_cbuf9[14].z)));
		u_3_phi_25 = u_3_2;
	}
	// 0  <=>  {u_3_phi_25 : 0}
	u_2_1 = u_3_phi_25;
	// 2147483648  <=>  {u_5_phi_24 : 2147483648}
	u_7_7 = u_5_phi_24;
	// 0  <=>  {u_4_phi_23 : 0}
	u_11_5 = u_4_phi_23;
	u_2_phi_26 = u_2_1;
	u_7_phi_26 = u_7_7;
	u_11_phi_26 = u_11_5;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 164.00}))
		f_11_15 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// ∞  <=>  ((1.0f / log2({utof(vs_cbuf9[15].x) : 1.00})) * 1.442695f)
		pf_2_12 = ((1.0f / log2(utof(vs_cbuf9[15].x))) * 1.442695f);
		// 4290772992  <=>  {ftou((((((0.f - (({pf_2_12 : ∞} * {f_11_15 : 1.00}) + (0.f - {pf_2_12 : ∞}))) + {pf_1_1 : 164.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].z) : 0})) : 4290772992}
		u_2_2 = ftou((((((0.f - ((pf_2_12 * f_11_15) + (0.f - pf_2_12))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].z)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_2_12 : ∞} * {f_11_15 : 1.00}) + (0.f - {pf_2_12 : ∞}))) + {pf_1_1 : 164.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].y) : -1})) : 4290772992}
		u_7_8 = ftou((((((0.f - ((pf_2_12 * f_11_15) + (0.f - pf_2_12))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].y)));
		// 4290772992  <=>  {ftou((((((0.f - (({pf_2_12 : ∞} * {f_11_15 : 1.00}) + (0.f - {pf_2_12 : ∞}))) + {pf_1_1 : 164.00}) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].x) : 0})) : 4290772992}
		u_11_6 = ftou((((((0.f - ((pf_2_12 * f_11_15) + (0.f - pf_2_12))) + pf_1_1) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].x)));
		u_2_phi_26 = u_2_2;
		u_7_phi_26 = u_7_8;
		u_11_phi_26 = u_11_6;
	}
	// 1126432768  <=>  {ftou(pf_1_1) : 1126432768}
	u_3_3 = ftou(pf_1_1);
	u_3_phi_27 = u_3_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// ∞  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))
		f_10_25 = (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f));
		// 1.00  <=>  exp2((log2(abs({utof(vs_cbuf9[15].x) : 1.00})) * {pf_1_1 : 164.00}))
		f_11_16 = exp2((log2(abs(utof(vs_cbuf9[15].x))) * pf_1_1));
		// 4290772992  <=>  {ftou((({f_11_16 : 1.00} * (0.f - {f_10_25 : ∞})) + {f_10_25 : ∞})) : 4290772992}
		u_3_4 = ftou(((f_11_16 * (0.f - f_10_25)) + f_10_25));
		u_3_phi_27 = u_3_4;
	}
	// 0  <=>  {u_1_phi_21 : 0}
	u_4_4 = u_1_phi_21;
	// 2147483648  <=>  {u_8_phi_20 : 2147483648}
	u_5_3 = u_8_phi_20;
	// 0  <=>  {u_0_phi_22 : 0}
	u_12_5 = u_0_phi_22;
	u_4_phi_28 = u_4_4;
	u_5_phi_28 = u_5_3;
	u_12_phi_28 = u_12_5;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// 1.00  <=>  exp2(({pf_0_1 : 163.00} * log2(abs({utof(vs_cbuf9[15].x) : 1.00}))))
		f_11_19 = exp2((pf_0_1 * log2(abs(utof(vs_cbuf9[15].x)))));
		// ∞  <=>  ((1.0f / log2({utof(vs_cbuf9[15].x) : 1.00})) * 1.442695f)
		pf_1_6 = ((1.0f / log2(utof(vs_cbuf9[15].x))) * 1.442695f);
		// 4290772992  <=>  {ftou((((({pf_0_1 : 163.00} + (0.f - (({pf_1_6 : ∞} * {f_11_19 : 1.00}) + (0.f - {pf_1_6 : ∞})))) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].z) : 0})) : 4290772992}
		u_4_5 = ftou(((((pf_0_1 + (0.f - ((pf_1_6 * f_11_19) + (0.f - pf_1_6)))) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].z)));
		// 4290772992  <=>  {ftou((((({pf_0_1 : 163.00} + (0.f - (({pf_1_6 : ∞} * {f_11_19 : 1.00}) + (0.f - {pf_1_6 : ∞})))) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].y) : -1})) : 4290772992}
		u_5_4 = ftou(((((pf_0_1 + (0.f - ((pf_1_6 * f_11_19) + (0.f - pf_1_6)))) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].y)));
		// 4290772992  <=>  {ftou((((({pf_0_1 : 163.00} + (0.f - (({pf_1_6 : ∞} * {f_11_19 : 1.00}) + (0.f - {pf_1_6 : ∞})))) * (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))) * {utof(vs_cbuf9[14].w) : 0}) * {utof(vs_cbuf9[14].x) : 0})) : 4290772992}
		u_12_6 = ftou(((((pf_0_1 + (0.f - ((pf_1_6 * f_11_19) + (0.f - pf_1_6)))) * (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f))) * utof(vs_cbuf9[14].w)) * utof(vs_cbuf9[14].x)));
		u_4_phi_28 = u_4_5;
		u_5_phi_28 = u_5_4;
		u_12_phi_28 = u_12_6;
	}
	// 1126367232  <=>  {u_6_phi_18 : 1126367232}
	u_0_3 = u_6_phi_18;
	u_0_phi_29 = u_0_3;
	// False  <=>  if(((! ((({utof(vs_cbuf9[15].x) : 1.00} == 1.f) && (! myIsNaN({utof(vs_cbuf9[15].x) : 1.00}))) && (! myIsNaN(1.f)))) ? true : false))
	if(((! (((utof(vs_cbuf9[15].x) == 1.f) && (! myIsNaN(utof(vs_cbuf9[15].x)))) && (! myIsNaN(1.f)))) ? true : false))
	{
		// ∞  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[15].x) : 1.00}) + 1.f))
		f_11_20 = (1.0f / ((0.f - utof(vs_cbuf9[15].x)) + 1.f));
		// 1.00  <=>  exp2(({pf_0_1 : 163.00} * log2(abs({utof(vs_cbuf9[15].x) : 1.00}))))
		f_10_43 = exp2((pf_0_1 * log2(abs(utof(vs_cbuf9[15].x)))));
		// 4290772992  <=>  {ftou((({f_10_43 : 1.00} * (0.f - {f_11_20 : ∞})) + {f_11_20 : ∞})) : 4290772992}
		u_0_4 = ftou(((f_10_43 * (0.f - f_11_20)) + f_11_20));
		u_0_phi_29 = u_0_4;
	}
	// 1.64  <=>  (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) * {i.vao_attr6.w : 1.00}) + {i.vao_attr4.y : 0})
	pf_3_3 = ((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) * i.vao_attr6.w) + i.vao_attr4.y);
	// 108.8271  <=>  (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) * {i.vao_attr6.w : 1.00}) + {i.vao_attr4.x : 108.8271})
	pf_1_16 = ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) * i.vao_attr6.w) + i.vao_attr4.x);
	// 44.27853  <=>  (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) * {i.vao_attr6.w : 1.00}) + {i.vao_attr4.z : 44.27853})
	pf_4_3 = ((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) * i.vao_attr6.w) + i.vao_attr4.z);
	// 0  <=>  {ftou(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * {i.vao_attr11.z : 1.00}) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * {f_5_0 : 0}) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * {i.vao_attr11.x : 0})))) : 0}
	u_4_6 = ftou((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * i.vao_attr11.z) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * f_5_0) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * i.vao_attr11.x))));
	// 1008981760  <=>  {ftou(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * {i.vao_attr10.z : 0}) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * {f_7_0 : 1.00}) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * {i.vao_attr10.x : 0})))) : 1008981760}
	u_5_5 = ftou((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * i.vao_attr10.z) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * f_7_0) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * i.vao_attr10.x))));
	// 0  <=>  {ftou(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * {i.vao_attr9.z : 0}) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * {f_9_1 : 0}) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * {i.vao_attr9.x : 1.00})))) : 0}
	u_6_3 = ftou((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * i.vao_attr9.z) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * f_9_1) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * i.vao_attr9.x))));
	u_4_phi_30 = u_4_6;
	u_5_phi_30 = u_5_5;
	u_6_phi_30 = u_6_3;
	// False  <=>  if(((((sqrt(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * ((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0})))) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * ((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0})))) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * ((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))))))) < float(1e-06)) && (! myIsNaN(sqrt(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * ((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0})))) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * ((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0})))) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * ((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0})))))))))) && (! myIsNaN(float(1e-06)))) ? true : false))
	if(((((sqrt((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * (((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28))))) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * (((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28))))) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * (((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))))))) < float(1e-06)) && (! myIsNaN(sqrt((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * (((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28))))) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * (((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28))))) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * (((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28))))))))))) && (! myIsNaN(float(1e-06)))) ? true : false))
	{
		// 0  <=>  {ftou(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * {i.vao_attr11.z : 1.00}) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * {f_5_0 : 0}) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * {i.vao_attr11.x : 0})))) : 0}
		u_7_9 = ftou((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * i.vao_attr11.z) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * f_5_0) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * i.vao_attr11.x))));
		// 1008981760  <=>  {ftou(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * {i.vao_attr10.z : 0}) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * {f_7_0 : 1.00}) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * {i.vao_attr10.x : 0})))) : 1008981760}
		u_8_5 = ftou((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * i.vao_attr10.z) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * f_7_0) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * i.vao_attr10.x))));
		// 0  <=>  {ftou(((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.z : 0}) + {utof(u_2_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.z : 0}) + {utof(u_4_phi_28) : 0}))) * {i.vao_attr9.z : 0}) + ((((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.y : 0.01}) + {utof(u_7_phi_26) : -0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.y : 0.01}) + {utof(u_5_phi_28) : -0}))) * {f_9_1 : 0}) + (((({utof(u_3_phi_27) : 164.00} * {i.vao_attr5.x : 0}) + {utof(u_11_phi_26) : 0}) + (0.f - (({utof(u_0_phi_29) : 163.00} * {i.vao_attr5.x : 0}) + {utof(u_12_phi_28) : 0}))) * {i.vao_attr9.x : 1.00})))) : 0}
		u_11_7 = ftou((((((utof(u_3_phi_27) * i.vao_attr5.z) + utof(u_2_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.z) + utof(u_4_phi_28)))) * i.vao_attr9.z) + (((((utof(u_3_phi_27) * i.vao_attr5.y) + utof(u_7_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.y) + utof(u_5_phi_28)))) * f_9_1) + ((((utof(u_3_phi_27) * i.vao_attr5.x) + utof(u_11_phi_26)) + (0.f - ((utof(u_0_phi_29) * i.vao_attr5.x) + utof(u_12_phi_28)))) * i.vao_attr9.x))));
		// 1008981770  <=>  {ftou(i.vao_attr5.y) : 1008981770}
		u_12_7 = ftou(i.vao_attr5.y);
		// 0  <=>  {ftou(i.vao_attr5.x) : 0}
		u_14_7 = ftou(i.vao_attr5.x);
		u_7_phi_31 = u_7_9;
		u_8_phi_31 = u_8_5;
		u_11_phi_31 = u_11_7;
		u_12_phi_31 = u_12_7;
		u_14_phi_31 = u_14_7;
		// False  <=>  if(((((0.f < {utof(vs_cbuf9[14].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[14].w) : 0}))) ? true : false))
		if(((((0.f < utof(vs_cbuf9[14].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[14].w)))) ? true : false))
		{
			// 0  <=>  (({i.vao_attr9.z : 0} * {utof(vs_cbuf9[14].z) : 0}) + (({f_9_1 : 0} * {utof(vs_cbuf9[14].y) : -1}) + ({i.vao_attr9.x : 1.00} * {utof(vs_cbuf9[14].x) : 0})))
			pf_2_28 = ((i.vao_attr9.z * utof(vs_cbuf9[14].z)) + ((f_9_1 * utof(vs_cbuf9[14].y)) + (i.vao_attr9.x * utof(vs_cbuf9[14].x))));
			// -1  <=>  (({i.vao_attr10.z : 0} * {utof(vs_cbuf9[14].z) : 0}) + (({f_7_0 : 1.00} * {utof(vs_cbuf9[14].y) : -1}) + ({i.vao_attr10.x : 0} * {utof(vs_cbuf9[14].x) : 0})))
			pf_5_3 = ((i.vao_attr10.z * utof(vs_cbuf9[14].z)) + ((f_7_0 * utof(vs_cbuf9[14].y)) + (i.vao_attr10.x * utof(vs_cbuf9[14].x))));
			// 0  <=>  (({i.vao_attr11.z : 1.00} * {utof(vs_cbuf9[14].z) : 0}) + (({f_5_0 : 0} * {utof(vs_cbuf9[14].y) : -1}) + ({i.vao_attr11.x : 0} * {utof(vs_cbuf9[14].x) : 0})))
			pf_6_4 = ((i.vao_attr11.z * utof(vs_cbuf9[14].z)) + ((f_5_0 * utof(vs_cbuf9[14].y)) + (i.vao_attr11.x * utof(vs_cbuf9[14].x))));
			// 1.00  <=>  inversesqrt((({pf_6_4 : 0} * {pf_6_4 : 0}) + (({pf_5_3 : -1} * {pf_5_3 : -1}) + ({pf_2_28 : 0} * {pf_2_28 : 0}))))
			f_12_18 = inversesqrt(((pf_6_4 * pf_6_4) + ((pf_5_3 * pf_5_3) + (pf_2_28 * pf_2_28))));
			// 0  <=>  {ftou((({pf_6_4 : 0} * {f_12_18 : 1.00}) * float(1e-06))) : 0}
			u_7_10 = ftou(((pf_6_4 * f_12_18) * float(1e-06)));
			// 3045472189  <=>  {ftou((({pf_5_3 : -1} * {f_12_18 : 1.00}) * float(1e-06))) : 3045472189}
			u_8_6 = ftou(((pf_5_3 * f_12_18) * float(1e-06)));
			// 0  <=>  {ftou((({pf_2_28 : 0} * {f_12_18 : 1.00}) * float(1e-06))) : 0}
			u_11_8 = ftou(((pf_2_28 * f_12_18) * float(1e-06)));
			// 0  <=>  {ftou(({pf_2_28 : 0} * {f_12_18 : 1.00})) : 0}
			u_12_8 = ftou((pf_2_28 * f_12_18));
			// 0  <=>  {ftou(({pf_6_4 : 0} * {f_12_18 : 1.00})) : 0}
			u_14_8 = ftou((pf_6_4 * f_12_18));
			u_7_phi_31 = u_7_10;
			u_8_phi_31 = u_8_6;
			u_11_phi_31 = u_11_8;
			u_12_phi_31 = u_12_8;
			u_14_phi_31 = u_14_8;
		}
		// 0  <=>  {u_7_phi_31 : 0}
		u_9_7 = u_7_phi_31;
		// 1008981760  <=>  {u_8_phi_31 : 1008981760}
		u_13_3 = u_8_phi_31;
		// 0  <=>  {u_11_phi_31 : 0}
		u_15_6 = u_11_phi_31;
		u_9_phi_32 = u_9_7;
		u_13_phi_32 = u_13_3;
		u_15_phi_32 = u_15_6;
		// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[14].w) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[14].w) : 0})))) ? true : false))
		if(((! (((0.f < utof(vs_cbuf9[14].w)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[14].w))))) ? true : false))
		{
			// 0  <=>  {u_7_phi_31 : 0}
			u_16_3 = u_7_phi_31;
			// 1008981760  <=>  {u_8_phi_31 : 1008981760}
			u_17_1 = u_8_phi_31;
			// 0  <=>  {u_11_phi_31 : 0}
			u_18_1 = u_11_phi_31;
			// 1008981770  <=>  {u_12_phi_31 : 1008981770}
			u_19_1 = u_12_phi_31;
			// 0  <=>  {u_14_phi_31 : 0}
			u_20_0 = u_14_phi_31;
			// False  <=>  (false ? true : false)
			b_3_0 = (false ? true : false);
			u_16_phi_33 = u_16_3;
			u_17_phi_33 = u_17_1;
			u_18_phi_33 = u_18_1;
			u_19_phi_33 = u_19_1;
			u_20_phi_33 = u_20_0;
			b_3_phi_33 = b_3_0;
			// False  <=>  if(((((sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0})))) < float(1e-06)) && (! myIsNaN(sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0}))))))) && (! myIsNaN(float(1e-06)))) ? true : false))
			if(((((sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31))))) < float(1e-06)) && (! myIsNaN(sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31)))))))) && (! myIsNaN(float(1e-06)))) ? true : false))
			{
				// 117.4901  <=>  sqrt((({i.vao_attr4.z : 44.27853} * {i.vao_attr4.z : 44.27853}) + (({i.vao_attr4.y : 0} * {i.vao_attr4.y : 0}) + ({i.vao_attr4.x : 108.8271} * {i.vao_attr4.x : 108.8271}))))
				f_12_22 = sqrt(((i.vao_attr4.z * i.vao_attr4.z) + ((i.vao_attr4.y * i.vao_attr4.y) + (i.vao_attr4.x * i.vao_attr4.x))));
				// 0  <=>  {u_7_phi_31 : 0}
				u_22_0 = u_7_phi_31;
				// 1008981760  <=>  {u_8_phi_31 : 1008981760}
				u_23_0 = u_8_phi_31;
				// 0  <=>  {u_11_phi_31 : 0}
				u_24_0 = u_11_phi_31;
				// 1008981770  <=>  {u_12_phi_31 : 1008981770}
				u_25_0 = u_12_phi_31;
				u_22_phi_34 = u_22_0;
				u_23_phi_34 = u_23_0;
				u_24_phi_34 = u_24_0;
				u_25_phi_34 = u_25_0;
				// False  <=>  if((((({f_12_22 : 117.4901} < float(1e-06)) && (! myIsNaN({f_12_22 : 117.4901}))) && (! myIsNaN(float(1e-06)))) ? true : false))
				if(((((f_12_22 < float(1e-06)) && (! myIsNaN(f_12_22))) && (! myIsNaN(float(1e-06)))) ? true : false))
				{
					// 0  <=>  {ftou(((inversesqrt((({f_5_0 : 0} * {f_5_0 : 0}) + (({f_7_0 : 1.00} * {f_7_0 : 1.00}) + ({f_9_1 : 0} * {f_9_1 : 0})))) * {f_5_0 : 0}) * float(1e-06))) : 0}
					u_22_1 = ftou(((inversesqrt(((f_5_0 * f_5_0) + ((f_7_0 * f_7_0) + (f_9_1 * f_9_1)))) * f_5_0) * float(1e-06)));
					// 897988541  <=>  {ftou(((inversesqrt((({f_5_0 : 0} * {f_5_0 : 0}) + (({f_7_0 : 1.00} * {f_7_0 : 1.00}) + ({f_9_1 : 0} * {f_9_1 : 0})))) * {f_7_0 : 1.00}) * float(1e-06))) : 897988541}
					u_23_1 = ftou(((inversesqrt(((f_5_0 * f_5_0) + ((f_7_0 * f_7_0) + (f_9_1 * f_9_1)))) * f_7_0) * float(1e-06)));
					// 0  <=>  {ftou(((inversesqrt((({f_5_0 : 0} * {f_5_0 : 0}) + (({f_7_0 : 1.00} * {f_7_0 : 1.00}) + ({f_9_1 : 0} * {f_9_1 : 0})))) * {f_9_1 : 0}) * float(1e-06))) : 0}
					u_24_1 = ftou(((inversesqrt(((f_5_0 * f_5_0) + ((f_7_0 * f_7_0) + (f_9_1 * f_9_1)))) * f_9_1) * float(1e-06)));
					// 1065353216  <=>  {ftou(inversesqrt((({f_5_0 : 0} * {f_5_0 : 0}) + (({f_7_0 : 1.00} * {f_7_0 : 1.00}) + ({f_9_1 : 0} * {f_9_1 : 0}))))) : 1065353216}
					u_25_1 = ftou(inversesqrt(((f_5_0 * f_5_0) + ((f_7_0 * f_7_0) + (f_9_1 * f_9_1)))));
					u_22_phi_34 = u_22_1;
					u_23_phi_34 = u_23_1;
					u_24_phi_34 = u_24_1;
					u_25_phi_34 = u_25_1;
				}
				// 0  <=>  {u_22_phi_34 : 0}
				u_16_4 = u_22_phi_34;
				// 1008981760  <=>  {u_23_phi_34 : 1008981760}
				u_17_2 = u_23_phi_34;
				// 0  <=>  {u_24_phi_34 : 0}
				u_18_2 = u_24_phi_34;
				// 1008981770  <=>  {u_25_phi_34 : 1008981770}
				u_19_2 = u_25_phi_34;
				// 1122695918  <=>  {ftou(f_12_22) : 1122695918}
				u_20_1 = ftou(f_12_22);
				// True  <=>  ((! ((({f_12_22 : 117.4901} < float(1e-06)) && (! myIsNaN({f_12_22 : 117.4901}))) && (! myIsNaN(float(1e-06))))) ? true : false)
				b_3_1 = ((! (((f_12_22 < float(1e-06)) && (! myIsNaN(f_12_22))) && (! myIsNaN(float(1e-06))))) ? true : false);
				u_16_phi_33 = u_16_4;
				u_17_phi_33 = u_17_2;
				u_18_phi_33 = u_18_2;
				u_19_phi_33 = u_19_2;
				u_20_phi_33 = u_20_1;
				b_3_phi_33 = b_3_1;
			}
			// 0  <=>  {u_16_phi_33 : 0}
			u_12_9 = u_16_phi_33;
			// 1008981760  <=>  {u_17_phi_33 : 1008981760}
			u_14_9 = u_17_phi_33;
			// 0  <=>  {u_18_phi_33 : 0}
			u_21_1 = u_18_phi_33;
			u_12_phi_35 = u_12_9;
			u_14_phi_35 = u_14_9;
			u_21_phi_35 = u_21_1;
			// True  <=>  if((((! (((sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0})))) < float(1e-06)) && (! myIsNaN(sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0}))))))) && (! myIsNaN(float(1e-06))))) || {b_3_phi_33 : False}) ? true : false))
			if((((! (((sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31))))) < float(1e-06)) && (! myIsNaN(sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31)))))))) && (! myIsNaN(float(1e-06))))) || b_3_phi_33) ? true : false))
			{
				// 0  <=>  {u_16_phi_33 : 0}
				u_22_2 = u_16_phi_33;
				// 1008981760  <=>  {u_17_phi_33 : 1008981760}
				u_23_2 = u_17_phi_33;
				// 0  <=>  {u_18_phi_33 : 0}
				u_24_2 = u_18_phi_33;
				// 1008981770  <=>  {u_19_phi_33 : 1008981770}
				u_25_2 = u_19_phi_33;
				// 0  <=>  {u_20_phi_33 : 0}
				u_26_1 = u_20_phi_33;
				u_22_phi_36 = u_22_2;
				u_23_phi_36 = u_23_2;
				u_24_phi_36 = u_24_2;
				u_25_phi_36 = u_25_2;
				u_26_phi_36 = u_26_1;
				// False  <=>  if(((((sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0})))) < float(1e-06)) && (! myIsNaN(sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0}))))))) && (! myIsNaN(float(1e-06)))) ? true : false))
				if(((((sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31))))) < float(1e-06)) && (! myIsNaN(sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31)))))))) && (! myIsNaN(float(1e-06)))) ? true : false))
				{
					// 108.8271  <=>  (({i.vao_attr9.z : 0} * {i.vao_attr4.z : 44.27853}) + (({f_9_1 : 0} * {i.vao_attr4.y : 0}) + ({i.vao_attr9.x : 1.00} * {i.vao_attr4.x : 108.8271})))
					pf_2_48 = ((i.vao_attr9.z * i.vao_attr4.z) + ((f_9_1 * i.vao_attr4.y) + (i.vao_attr9.x * i.vao_attr4.x)));
					// 0  <=>  (({i.vao_attr10.z : 0} * {i.vao_attr4.z : 44.27853}) + (({f_7_0 : 1.00} * {i.vao_attr4.y : 0}) + ({i.vao_attr10.x : 0} * {i.vao_attr4.x : 108.8271})))
					pf_5_8 = ((i.vao_attr10.z * i.vao_attr4.z) + ((f_7_0 * i.vao_attr4.y) + (i.vao_attr10.x * i.vao_attr4.x)));
					// 44.27853  <=>  (({i.vao_attr11.z : 1.00} * {i.vao_attr4.z : 44.27853}) + (({f_5_0 : 0} * {i.vao_attr4.y : 0}) + ({i.vao_attr11.x : 0} * {i.vao_attr4.x : 108.8271})))
					pf_6_9 = ((i.vao_attr11.z * i.vao_attr4.z) + ((f_5_0 * i.vao_attr4.y) + (i.vao_attr11.x * i.vao_attr4.x)));
					// 885675178  <=>  {ftou((({pf_6_9 : 44.27853} * inversesqrt((({pf_6_9 : 44.27853} * {pf_6_9 : 44.27853}) + (({pf_5_8 : 0} * {pf_5_8 : 0}) + ({pf_2_48 : 108.8271} * {pf_2_48 : 108.8271}))))) * float(1e-06))) : 885675178}
					u_22_3 = ftou(((pf_6_9 * inversesqrt(((pf_6_9 * pf_6_9) + ((pf_5_8 * pf_5_8) + (pf_2_48 * pf_2_48))))) * float(1e-06)));
					// 0  <=>  {ftou((({pf_5_8 : 0} * inversesqrt((({pf_6_9 : 44.27853} * {pf_6_9 : 44.27853}) + (({pf_5_8 : 0} * {pf_5_8 : 0}) + ({pf_2_48 : 108.8271} * {pf_2_48 : 108.8271}))))) * float(1e-06))) : 0}
					u_23_3 = ftou(((pf_5_8 * inversesqrt(((pf_6_9 * pf_6_9) + ((pf_5_8 * pf_5_8) + (pf_2_48 * pf_2_48))))) * float(1e-06)));
					// 897098885  <=>  {ftou((({pf_2_48 : 108.8271} * inversesqrt((({pf_6_9 : 44.27853} * {pf_6_9 : 44.27853}) + (({pf_5_8 : 0} * {pf_5_8 : 0}) + ({pf_2_48 : 108.8271} * {pf_2_48 : 108.8271}))))) * float(1e-06))) : 897098885}
					u_24_3 = ftou(((pf_2_48 * inversesqrt(((pf_6_9 * pf_6_9) + ((pf_5_8 * pf_5_8) + (pf_2_48 * pf_2_48))))) * float(1e-06)));
					// 1064116166  <=>  {ftou(({pf_2_48 : 108.8271} * inversesqrt((({pf_6_9 : 44.27853} * {pf_6_9 : 44.27853}) + (({pf_5_8 : 0} * {pf_5_8 : 0}) + ({pf_2_48 : 108.8271} * {pf_2_48 : 108.8271})))))) : 1064116166}
					u_25_3 = ftou((pf_2_48 * inversesqrt(((pf_6_9 * pf_6_9) + ((pf_5_8 * pf_5_8) + (pf_2_48 * pf_2_48))))));
					// 1052833062  <=>  {ftou(({pf_6_9 : 44.27853} * inversesqrt((({pf_6_9 : 44.27853} * {pf_6_9 : 44.27853}) + (({pf_5_8 : 0} * {pf_5_8 : 0}) + ({pf_2_48 : 108.8271} * {pf_2_48 : 108.8271})))))) : 1052833062}
					u_26_2 = ftou((pf_6_9 * inversesqrt(((pf_6_9 * pf_6_9) + ((pf_5_8 * pf_5_8) + (pf_2_48 * pf_2_48))))));
					u_22_phi_36 = u_22_3;
					u_23_phi_36 = u_23_3;
					u_24_phi_36 = u_24_3;
					u_25_phi_36 = u_25_3;
					u_26_phi_36 = u_26_2;
				}
				// 0  <=>  {u_22_phi_36 : 0}
				u_19_3 = u_22_phi_36;
				// 1008981760  <=>  {u_23_phi_36 : 1008981760}
				u_20_2 = u_23_phi_36;
				// 0  <=>  {u_24_phi_36 : 0}
				u_27_2 = u_24_phi_36;
				u_19_phi_37 = u_19_3;
				u_20_phi_37 = u_20_2;
				u_27_phi_37 = u_27_2;
				// True  <=>  if(((! (((sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0})))) < float(1e-06)) && (! myIsNaN(sqrt((({i.vao_attr5.z : 0} * {i.vao_attr5.z : 0}) + (({utof(u_12_phi_31) : 0.01} * {utof(u_12_phi_31) : 0.01}) + ({utof(u_14_phi_31) : 0} * {utof(u_14_phi_31) : 0}))))))) && (! myIsNaN(float(1e-06))))) ? true : false))
				if(((! (((sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31))))) < float(1e-06)) && (! myIsNaN(sqrt(((i.vao_attr5.z * i.vao_attr5.z) + ((utof(u_12_phi_31) * utof(u_12_phi_31)) + (utof(u_14_phi_31) * utof(u_14_phi_31)))))))) && (! myIsNaN(float(1e-06))))) ? true : false))
				{
					// 0  <=>  (({i.vao_attr5.z : 0} * {i.vao_attr9.z : 0}) + (({utof(u_25_phi_36) : 0.01} * {f_9_1 : 0}) + ({utof(u_26_phi_36) : 0} * {i.vao_attr9.x : 1.00})))
					pf_2_55 = ((i.vao_attr5.z * i.vao_attr9.z) + ((utof(u_25_phi_36) * f_9_1) + (utof(u_26_phi_36) * i.vao_attr9.x)));
					// 0.01  <=>  (({i.vao_attr5.z : 0} * {i.vao_attr10.z : 0}) + (({utof(u_25_phi_36) : 0.01} * {f_7_0 : 1.00}) + ({utof(u_26_phi_36) : 0} * {i.vao_attr10.x : 0})))
					pf_5_12 = ((i.vao_attr5.z * i.vao_attr10.z) + ((utof(u_25_phi_36) * f_7_0) + (utof(u_26_phi_36) * i.vao_attr10.x)));
					// 0  <=>  (({i.vao_attr5.z : 0} * {i.vao_attr11.z : 1.00}) + (({utof(u_25_phi_36) : 0.01} * {f_5_0 : 0}) + ({utof(u_26_phi_36) : 0} * {i.vao_attr11.x : 0})))
					pf_6_13 = ((i.vao_attr5.z * i.vao_attr11.z) + ((utof(u_25_phi_36) * f_5_0) + (utof(u_26_phi_36) * i.vao_attr11.x)));
					// 0  <=>  {ftou((({pf_6_13 : 0} * inversesqrt((({pf_6_13 : 0} * {pf_6_13 : 0}) + (({pf_5_12 : 0.01} * {pf_5_12 : 0.01}) + ({pf_2_55 : 0} * {pf_2_55 : 0}))))) * float(1e-06))) : 0}
					u_19_4 = ftou(((pf_6_13 * inversesqrt(((pf_6_13 * pf_6_13) + ((pf_5_12 * pf_5_12) + (pf_2_55 * pf_2_55))))) * float(1e-06)));
					// 897988541  <=>  {ftou((({pf_5_12 : 0.01} * inversesqrt((({pf_6_13 : 0} * {pf_6_13 : 0}) + (({pf_5_12 : 0.01} * {pf_5_12 : 0.01}) + ({pf_2_55 : 0} * {pf_2_55 : 0}))))) * float(1e-06))) : 897988541}
					u_20_3 = ftou(((pf_5_12 * inversesqrt(((pf_6_13 * pf_6_13) + ((pf_5_12 * pf_5_12) + (pf_2_55 * pf_2_55))))) * float(1e-06)));
					// 0  <=>  {ftou((({pf_2_55 : 0} * inversesqrt((({pf_6_13 : 0} * {pf_6_13 : 0}) + (({pf_5_12 : 0.01} * {pf_5_12 : 0.01}) + ({pf_2_55 : 0} * {pf_2_55 : 0}))))) * float(1e-06))) : 0}
					u_27_3 = ftou(((pf_2_55 * inversesqrt(((pf_6_13 * pf_6_13) + ((pf_5_12 * pf_5_12) + (pf_2_55 * pf_2_55))))) * float(1e-06)));
					u_19_phi_37 = u_19_4;
					u_20_phi_37 = u_20_3;
					u_27_phi_37 = u_27_3;
				}
				// 0  <=>  {u_19_phi_37 : 0}
				u_12_10 = u_19_phi_37;
				// 897988541  <=>  {u_20_phi_37 : 897988541}
				u_14_10 = u_20_phi_37;
				// 0  <=>  {u_27_phi_37 : 0}
				u_21_2 = u_27_phi_37;
				u_12_phi_35 = u_12_10;
				u_14_phi_35 = u_14_10;
				u_21_phi_35 = u_21_2;
			}
			// 0  <=>  {u_12_phi_35 : 0}
			u_9_8 = u_12_phi_35;
			// 897988541  <=>  {u_14_phi_35 : 897988541}
			u_13_4 = u_14_phi_35;
			// 0  <=>  {u_21_phi_35 : 0}
			u_15_7 = u_21_phi_35;
			u_9_phi_32 = u_9_8;
			u_13_phi_32 = u_13_4;
			u_15_phi_32 = u_15_7;
		}
		// 0  <=>  {u_9_phi_32 : 0}
		u_4_7 = u_9_phi_32;
		// 897988541  <=>  {u_13_phi_32 : 897988541}
		u_5_6 = u_13_phi_32;
		// 0  <=>  {u_15_phi_32 : 0}
		u_6_4 = u_15_phi_32;
		u_4_phi_30 = u_4_7;
		u_5_phi_30 = u_5_6;
		u_6_phi_30 = u_6_4;
	}
	// 0  <=>  {ftou(clamp(min(0.f, {i.vao_attr7.x : 0.81938}), 0.0, 1.0)) : 0}
	u_3_6 = ftou(clamp(min(0.f, i.vao_attr7.x), 0.0, 1.0));
	u_3_phi_38 = u_3_6;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[12].x) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[12].x) : 0}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[12].x)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[12].x)))) ? true : false))
	{
		// ∞  <=>  (((({i.vao_attr7.x : 0.81938} * {utof(vs_cbuf9[13].y) : 0}) * {utof(vs_cbuf9[12].x) : 0}) + {pf_0_1 : 163.00}) * (1.0f / {utof(vs_cbuf9[12].x) : 0}))
		pf_5_16 = ((((i.vao_attr7.x * utof(vs_cbuf9[13].y)) * utof(vs_cbuf9[12].x)) + pf_0_1) * (1.0f / utof(vs_cbuf9[12].x)));
		// 4290772992  <=>  {ftou(({pf_5_16 : ∞} + (0.f - floor({pf_5_16 : ∞})))) : 4290772992}
		u_3_7 = ftou((pf_5_16 + (0.f - floor(pf_5_16))));
		u_3_phi_38 = u_3_7;
	}
	// 0  <=>  {u_3_phi_38 : 0}
	u_1_6 = u_3_phi_38;
	u_1_phi_39 = u_1_6;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[12].x) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[12].x) : 0})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[12].x)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[12].x))))) ? true : false))
	{
		// 1057691621  <=>  {ftou(({pf_0_1 : 163.00} * (1.0f / float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))))) : 1057691621}
		u_1_7 = ftou((pf_0_1 * (1.0f / float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))))));
		u_1_phi_39 = u_1_7;
	}
	// 1.00  <=>  (1.0f / ((0.f - {utof(vs_cbuf9[141].w) : 0}) + {utof(vs_cbuf9[142].w) : 1.00}))
	f_12_28 = (1.0f / ((0.f - utof(vs_cbuf9[141].w)) + utof(vs_cbuf9[142].w)));
	// 0  <=>  {utof(vs_cbuf9[141].w) : 0}
	f_13_5 = utof(vs_cbuf9[141].w);
	// 1065353216  <=>  (((({utof(u_1_phi_39) : 0.5433334} >= {f_13_5 : 0}) && (! myIsNaN({utof(u_1_phi_39) : 0.5433334}))) && (! myIsNaN({f_13_5 : 0}))) ? 1065353216u : 0u)
	u_3_8 = ((((utof(u_1_phi_39) >= f_13_5) && (! myIsNaN(utof(u_1_phi_39)))) && (! myIsNaN(f_13_5))) ? 1065353216u : 0u);
	// 0.5433334  <=>  ({utof(u_1_phi_39) : 0.5433334} + (0.f - {utof(vs_cbuf9[141].w) : 0}))
	pf_5_20 = (utof(u_1_phi_39) + (0.f - utof(vs_cbuf9[141].w)));
	// 1.00  <=>  {utof(vs_cbuf9[142].w) : 1.00}
	f_13_8 = utof(vs_cbuf9[142].w);
	// 0  <=>  (((({utof(u_1_phi_39) : 0.5433334} >= {f_13_8 : 1.00}) && (! myIsNaN({utof(u_1_phi_39) : 0.5433334}))) && (! myIsNaN({f_13_8 : 1.00}))) ? 1065353216u : 0u)
	u_1_8 = ((((utof(u_1_phi_39) >= f_13_8) && (! myIsNaN(utof(u_1_phi_39)))) && (! myIsNaN(f_13_8))) ? 1065353216u : 0u);
	// 1.00  <=>  (({utof(u_3_8) : 1.00} * (0.f - {utof(u_1_8) : 0})) + {utof(u_3_8) : 1.00})
	pf_12_0 = ((utof(u_3_8) * (0.f - utof(u_1_8))) + utof(u_3_8));
	// 1067036250  <=>  {ftou((({pf_5_20 : 0.5433334} * (({utof(vs_cbuf9[142].x) : 1.26} + (0.f - {utof(vs_cbuf9[141].x) : 1.13})) * {f_12_28 : 1.00})) + {utof(vs_cbuf9[141].x) : 1.13})) : 1067036250}
	u_1_9 = ftou(((pf_5_20 * ((utof(vs_cbuf9[142].x) + (0.f - utof(vs_cbuf9[141].x))) * f_12_28)) + utof(vs_cbuf9[141].x)));
	// 1133903872  <=>  {ftou(float(int((myIsNaN({i.vao_attr4.w : 300.00}) ? 0u : int(clamp(trunc({i.vao_attr4.w : 300.00}), float(-2147483600.f), float(2147483600.f))))))) : 1133903872}
	u_7_13 = ftou(float(int((myIsNaN(i.vao_attr4.w) ? 0u : int(clamp(trunc(i.vao_attr4.w), float(-2147483600.f), float(2147483600.f)))))));
	u_1_phi_40 = u_1_9;
	u_7_phi_40 = u_7_13;
	// False  <=>  if(((((0.f < {utof(vs_cbuf9[11].y) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].y) : 0}))) ? true : false))
	if(((((0.f < utof(vs_cbuf9[11].y)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].y)))) ? true : false))
	{
		// ∞  <=>  (((({i.vao_attr7.x : 0.81938} * {utof(vs_cbuf9[12].z) : 0}) * {utof(vs_cbuf9[11].y) : 0}) + {pf_0_1 : 163.00}) * (1.0f / {utof(vs_cbuf9[11].y) : 0}))
		pf_7_20 = ((((i.vao_attr7.x * utof(vs_cbuf9[12].z)) * utof(vs_cbuf9[11].y)) + pf_0_1) * (1.0f / utof(vs_cbuf9[11].y)));
		// 4290772992  <=>  {ftou(({pf_7_20 : ∞} + (0.f - floor({pf_7_20 : ∞})))) : 4290772992}
		u_1_10 = ftou((pf_7_20 + (0.f - floor(pf_7_20))));
		// 2139095040  <=>  {ftou(pf_7_20) : 2139095040}
		u_7_14 = ftou(pf_7_20);
		u_1_phi_40 = u_1_10;
		u_7_phi_40 = u_7_14;
	}
	// 1067036250  <=>  {u_1_phi_40 : 1067036250}
	u_3_10 = u_1_phi_40;
	u_3_phi_41 = u_3_10;
	// True  <=>  if(((! (((0.f < {utof(vs_cbuf9[11].y) : 0}) && (! myIsNaN(0.f))) && (! myIsNaN({utof(vs_cbuf9[11].y) : 0})))) ? true : false))
	if(((! (((0.f < utof(vs_cbuf9[11].y)) && (! myIsNaN(0.f))) && (! myIsNaN(utof(vs_cbuf9[11].y))))) ? true : false))
	{
		// 1057691621  <=>  {ftou(({pf_0_1 : 163.00} * (1.0f / {utof(u_7_phi_40) : 300.00}))) : 1057691621}
		u_3_11 = ftou((pf_0_1 * (1.0f / utof(u_7_phi_40))));
		u_3_phi_41 = u_3_11;
	}
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[78].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[78].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_7_17 = (myIsNaN(utof(vs_cbuf9[78].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[78].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[83].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[83].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_9_11 = (myIsNaN(utof(vs_cbuf9[83].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[83].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  (myIsNaN({utof(vs_cbuf9[88].z) : 1.00}) ? 0u : int(clamp(trunc({utof(vs_cbuf9[88].z) : 1.00}), float(-2147483600.f), float(2147483600.f))))
	u_11_10 = (myIsNaN(utof(vs_cbuf9[88].z)) ? 0u : int(clamp(trunc(utof(vs_cbuf9[88].z)), float(-2147483600.f), float(2147483600.f))));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_7_17 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_13_7 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_7_17), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_9_11 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_14_13 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_9_11), int(0u), int(32u)))))), int(0u), int(32u)));
	// 1  <=>  uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int({u_11_10 : 1}), int(0u), int(32u)))))), int(0u), int(32u)))
	u_15_10 = uint(bitfieldExtract(uint(abs(int(uint(bitfieldExtract(int(u_11_10), int(0u), int(32u)))))), int(0u), int(32u)));
	// 0  <=>  {utof(vs_cbuf9[113].w) : 0}
	f_4_6 = utof(vs_cbuf9[113].w);
	// 1065353216  <=>  (((({utof(u_3_phi_41) : 0.5433334} >= {f_4_6 : 0}) && (! myIsNaN({utof(u_3_phi_41) : 0.5433334}))) && (! myIsNaN({f_4_6 : 0}))) ? 1065353216u : 0u)
	u_17_16 = ((((utof(u_3_phi_41) >= f_4_6) && (! myIsNaN(utof(u_3_phi_41)))) && (! myIsNaN(f_4_6))) ? 1065353216u : 0u);
	// 0.21  <=>  {utof(vs_cbuf9[114].w) : 0.21}
	f_4_8 = utof(vs_cbuf9[114].w);
	// 1065353216  <=>  (((({utof(u_3_phi_41) : 0.5433334} >= {f_4_8 : 0.21}) && (! myIsNaN({utof(u_3_phi_41) : 0.5433334}))) && (! myIsNaN({f_4_8 : 0.21}))) ? 1065353216u : 0u)
	u_20_6 = ((((utof(u_3_phi_41) >= f_4_8) && (! myIsNaN(utof(u_3_phi_41)))) && (! myIsNaN(f_4_8))) ? 1065353216u : 0u);
	// 0.84  <=>  {utof(vs_cbuf9[115].w) : 0.84}
	f_4_14 = utof(vs_cbuf9[115].w);
	// 0  <=>  (((({utof(u_3_phi_41) : 0.5433334} >= {f_4_14 : 0.84}) && (! myIsNaN({utof(u_3_phi_41) : 0.5433334}))) && (! myIsNaN({f_4_14 : 0.84}))) ? 1065353216u : 0u)
	u_18_11 = ((((utof(u_3_phi_41) >= f_4_14) && (! myIsNaN(utof(u_3_phi_41)))) && (! myIsNaN(f_4_14))) ? 1065353216u : 0u);
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 1048576}) == 1u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.x : 0.81938} > 0.5f) && (! myIsNaN({i.vao_attr7.x : 0.81938}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.x > 0.5f) && (! myIsNaN(i.vao_attr7.x))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 1.00  <=>  (1.0f / float({u_15_10 : 1}))
	f_4_16 = (1.0f / float(u_15_10));
	// 1.00  <=>  (1.0f / float({u_14_13 : 1}))
	f_4_18 = (1.0f / float(u_14_13));
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 1048576}) == 4u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.z : 0.03794} > 0.5f) && (! myIsNaN({i.vao_attr7.z : 0.03794}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.z > 0.5f) && (! myIsNaN(i.vao_attr7.z))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
	}
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_7_17 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_23_5 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_7_17))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 1056997491  <=>  {ftou(v.uv.y) : 1056997491}
	u_20_7 = ftou(v.uv.y);
	u_20_phi_44 = u_20_7;
	// False  <=>  if(((! (((~ (((2u & {vs_cbuf9_7_y : 1048576}) == 2u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.y : 0.29926} > 0.5f) && (! myIsNaN({i.vao_attr7.y : 0.29926}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((2u & vs_cbuf9_7_y) == 2u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.y > 0.5f) && (! myIsNaN(i.vao_attr7.y))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.y : 0.50196}) + 1.f)) : 1056898842}
		u_20_8 = ftou(((0.f - v.uv.y) + 1.f));
		u_20_phi_44 = u_20_8;
	}
	// 1056997491  <=>  {ftou(v.uv.y) : 1056997491}
	u_24_7 = ftou(v.uv.y);
	u_24_phi_45 = u_24_7;
	// False  <=>  if(((! (((~ (((8u & {vs_cbuf9_7_y : 1048576}) == 8u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.w : 0.10196} > 0.5f) && (! myIsNaN({i.vao_attr7.w : 0.10196}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((8u & vs_cbuf9_7_y) == 8u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.w > 0.5f) && (! myIsNaN(i.vao_attr7.w))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.y : 0.50196}) + 1.f)) : 1056898842}
		u_24_8 = ftou(((0.f - v.uv.y) + 1.f));
		u_24_phi_45 = u_24_8;
	}
	// 1056997491  <=>  {ftou(v.uv.y) : 1056997491}
	u_25_6 = ftou(v.uv.y);
	u_25_phi_46 = u_25_6;
	// False  <=>  if(((! (((~ (((32u & {vs_cbuf9_7_y : 1048576}) == 32u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.z : 0.03794} > 0.5f) && (! myIsNaN({i.vao_attr7.z : 0.03794}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((32u & vs_cbuf9_7_y) == 32u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.z > 0.5f) && (! myIsNaN(i.vao_attr7.z))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.y : 0.50196}) + 1.f)) : 1056898842}
		u_25_7 = ftou(((0.f - v.uv.y) + 1.f));
		u_25_phi_46 = u_25_7;
	}
	// 1.00  <=>  {utof(vs_cbuf9[116].w) : 1.00}
	f_4_25 = utof(vs_cbuf9[116].w);
	// 0  <=>  (((({utof(u_3_phi_41) : 0.5433334} >= {f_4_25 : 1.00}) && (! myIsNaN({utof(u_3_phi_41) : 0.5433334}))) && (! myIsNaN({f_4_25 : 1.00}))) ? 1065353216u : 0u)
	u_3_12 = ((((utof(u_3_phi_41) >= f_4_25) && (! myIsNaN(utof(u_3_phi_41)))) && (! myIsNaN(f_4_25))) ? 1065353216u : 0u);
	// 0  <=>  0u
	u_19_7 = 0u;
	u_19_phi_47 = u_19_7;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 1  <=>  1u
		u_19_8 = 1u;
		u_19_phi_47 = u_19_8;
	}
	// 1065353214  <=>  ({ftou(f_4_16) : 1065353216} + 4294967294u)
	u_21_7 = (ftou(f_4_16) + 4294967294u);
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_11_10 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_26_5 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_11_10))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 1.00  <=>  (1.0f / float({u_13_7 : 1}))
	f_4_27 = (1.0f / float(u_13_7));
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_23_5 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_23_5 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_29_4 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_23_5), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_23_5), int(0u), int(16u))), int(16u), int(16u));
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(({ftou((1.0f / float({u_9_11 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_18_12 = uint(clamp(trunc((float(0u) * utof((ftou((1.0f / float(u_9_11))) + 4294967294u)))), float(0.f), float(4294967300.f)));
	// 0  <=>  (({u_29_4 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_23_5 : 0}), int(0u), int(16u))))))
	u_27_6 = ((u_29_4 << 16u) + uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_23_5), int(0u), int(16u))))));
	// 1065353214  <=>  ({ftou(f_4_18) : 1065353216} + 4294967294u)
	u_22_7 = (ftou(f_4_18) + 4294967294u);
	// 1056997491  <=>  {ftou(v.uv.x) : 1056997491}
	u_29_6 = ftou(v.uv.x);
	u_29_phi_48 = u_29_6;
	// False  <=>  if(((! (((~ (((1u & {vs_cbuf9_7_y : 1048576}) == 1u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.x : 0.81938} > 0.5f) && (! myIsNaN({i.vao_attr7.x : 0.81938}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((1u & vs_cbuf9_7_y) == 1u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.x > 0.5f) && (! myIsNaN(i.vao_attr7.x))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.x : 0.50196}) + 1.f)) : 1056898842}
		u_29_7 = ftou(((0.f - v.uv.x) + 1.f));
		u_29_phi_48 = u_29_7;
	}
	// 1056997491  <=>  {ftou(v.uv.x) : 1056997491}
	u_30_7 = ftou(v.uv.x);
	u_30_phi_49 = u_30_7;
	// False  <=>  if(((! (((~ (((4u & {vs_cbuf9_7_y : 1048576}) == 4u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.z : 0.03794} > 0.5f) && (! myIsNaN({i.vao_attr7.z : 0.03794}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((4u & vs_cbuf9_7_y) == 4u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.z > 0.5f) && (! myIsNaN(i.vao_attr7.z))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.x : 0.50196}) + 1.f)) : 1056898842}
		u_30_8 = ftou(((0.f - v.uv.x) + 1.f));
		u_30_phi_49 = u_30_8;
	}
	// 1065353214  <=>  ({ftou(f_4_27) : 1065353216} + 4294967294u)
	u_28_5 = (ftou(f_4_27) + 4294967294u);
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(u_22_7) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_31_3 = uint(clamp(trunc((float(0u) * utof(u_22_7))), float(0.f), float(4294967300.f)));
	// 0  <=>  uint(clamp(trunc(({utof(u_28_5) : 0.9999999} * float(0u))), float(0.f), float(4294967300.f)))
	u_32_0 = uint(clamp(trunc((utof(u_28_5) * float(0u))), float(0.f), float(4294967300.f)));
	// 1056997491  <=>  {ftou(v.uv.x) : 1056997491}
	u_33_0 = ftou(v.uv.x);
	u_33_phi_50 = u_33_0;
	// False  <=>  if(((! (((~ (((16u & {vs_cbuf9_7_y : 1048576}) == 16u) ? 4294967295u : 0u)) | (~ (((({i.vao_attr7.y : 0.29926} > 0.5f) && (! myIsNaN({i.vao_attr7.y : 0.29926}))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	if(((! (((~ (((16u & vs_cbuf9_7_y) == 16u) ? 4294967295u : 0u)) | (~ ((((i.vao_attr7.y > 0.5f) && (! myIsNaN(i.vao_attr7.y))) && (! myIsNaN(0.5f))) ? 4294967295u : 0u))) != 0u)) ? true : false))
	{
		// 1056898842  <=>  {ftou(((0.f - {v.uv.x : 0.50196}) + 1.f)) : 1056898842}
		u_33_1 = ftou(((0.f - v.uv.x) + 1.f));
		u_33_phi_50 = u_33_1;
	}
	// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
	u_10_6 = ftou(i.vao_attr7.x);
	u_10_phi_51 = u_10_6;
	// True  <=>  if((({u_19_phi_47 : 1} == 1u) ? true : false))
	if(((u_19_phi_47 == 1u) ? true : false))
	{
		// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
		u_10_7 = ftou(i.vao_attr7.y);
		u_10_phi_51 = u_10_7;
	}
	// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
	u_34_1 = ftou(i.vao_attr7.y);
	u_34_phi_52 = u_34_1;
	// True  <=>  if((({u_19_phi_47 : 1} == 1u) ? true : false))
	if(((u_19_phi_47 == 1u) ? true : false))
	{
		// 1025206009  <=>  {ftou(i.vao_attr7.z) : 1025206009}
		u_34_2 = ftou(i.vao_attr7.z);
		u_34_phi_52 = u_34_2;
	}
	// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
	u_35_0 = ftou(i.vao_attr7.x);
	u_35_phi_53 = u_35_0;
	// True  <=>  if((({u_19_phi_47 : 1} == 1u) ? true : false))
	if(((u_19_phi_47 == 1u) ? true : false))
	{
		// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
		u_35_1 = ftou(i.vao_attr7.y);
		u_35_phi_53 = u_35_1;
	}
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_12 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_18_12 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_37_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_12), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_18_12), int(0u), int(16u))), int(16u), int(16u));
	// 0  <=>  bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_26_5 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_26_5 : 0}), int(0u), int(16u))), int(16u), int(16u))
	u_39_3 = bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_26_5), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_26_5), int(0u), int(16u))), int(16u), int(16u));
	// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
	u_40_2 = ftou(i.vao_attr7.y);
	u_40_phi_54 = u_40_2;
	// True  <=>  if((({u_19_phi_47 : 1} == 1u) ? true : false))
	if(((u_19_phi_47 == 1u) ? true : false))
	{
		// 1025206009  <=>  {ftou(i.vao_attr7.z) : 1025206009}
		u_40_3 = ftou(i.vao_attr7.z);
		u_40_phi_54 = u_40_3;
	}
	// 1025206009  <=>  {u_34_phi_52 : 1025206009}
	u_41_0 = u_34_phi_52;
	// 1050228891  <=>  {u_10_phi_51 : 1050228891}
	u_42_0 = u_10_phi_51;
	// 1050228891  <=>  {u_35_phi_53 : 1050228891}
	u_43_0 = u_35_phi_53;
	// 1025206009  <=>  {u_40_phi_54 : 1025206009}
	u_44_0 = u_40_phi_54;
	u_41_phi_55 = u_41_0;
	u_42_phi_55 = u_42_0;
	u_43_phi_55 = u_43_0;
	u_44_phi_55 = u_44_0;
	// False  <=>  if(((! ({u_19_phi_47 : 1} == 1u)) ? true : false))
	if(((! (u_19_phi_47 == 1u)) ? true : false))
	{
		// 1050228891  <=>  {u_10_phi_51 : 1050228891}
		u_19_9 = u_10_phi_51;
		u_19_phi_56 = u_19_9;
		// False  <=>  if((({u_19_phi_47 : 1} == 2u) ? true : false))
		if(((u_19_phi_47 == 2u) ? true : false))
		{
			// 1025206009  <=>  {ftou(i.vao_attr7.z) : 1025206009}
			u_19_10 = ftou(i.vao_attr7.z);
			u_19_phi_56 = u_19_10;
		}
		// 1025206009  <=>  {u_34_phi_52 : 1025206009}
		u_45_0 = u_34_phi_52;
		u_45_phi_57 = u_45_0;
		// False  <=>  if((({u_19_phi_47 : 1} == 2u) ? true : false))
		if(((u_19_phi_47 == 2u) ? true : false))
		{
			// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
			u_45_1 = ftou(i.vao_attr7.x);
			u_45_phi_57 = u_45_1;
		}
		// 1050228891  <=>  {u_35_phi_53 : 1050228891}
		u_46_0 = u_35_phi_53;
		u_46_phi_58 = u_46_0;
		// False  <=>  if((({u_19_phi_47 : 1} == 2u) ? true : false))
		if(((u_19_phi_47 == 2u) ? true : false))
		{
			// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
			u_46_1 = ftou(i.vao_attr7.x);
			u_46_phi_58 = u_46_1;
		}
		// 1025206009  <=>  {u_40_phi_54 : 1025206009}
		u_47_0 = u_40_phi_54;
		u_47_phi_59 = u_47_0;
		// False  <=>  if((({u_19_phi_47 : 1} == 2u) ? true : false))
		if(((u_19_phi_47 == 2u) ? true : false))
		{
			// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
			u_47_1 = ftou(i.vao_attr7.y);
			u_47_phi_59 = u_47_1;
		}
		// 1025206009  <=>  {u_45_phi_57 : 1025206009}
		u_41_1 = u_45_phi_57;
		// 1050228891  <=>  {u_19_phi_56 : 1050228891}
		u_42_1 = u_19_phi_56;
		// 1050228891  <=>  {u_46_phi_58 : 1050228891}
		u_43_1 = u_46_phi_58;
		// 1025206009  <=>  {u_47_phi_59 : 1025206009}
		u_44_1 = u_47_phi_59;
		u_41_phi_55 = u_41_1;
		u_42_phi_55 = u_42_1;
		u_43_phi_55 = u_43_1;
		u_44_phi_55 = u_44_1;
	}
	// 0  <=>  uint(clamp(trunc((float(0u) * {utof(u_21_7) : 0.9999999})), float(0.f), float(4294967300.f)))
	u_10_8 = uint(clamp(trunc((float(0u) * utof(u_21_7))), float(0.f), float(4294967300.f)));
	// 0  <=>  0u
	u_19_11 = 0u;
	u_19_phi_60 = u_19_11;
	// True  <=>  if((((1u & {vs_cbuf9_7_z : 301056}) != 1u) ? true : false))
	if((((1u & vs_cbuf9_7_z) != 1u) ? true : false))
	{
		// 2  <=>  2u
		u_19_12 = 2u;
		u_19_phi_60 = u_19_12;
	}
	// 0  <=>  (({u_37_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_18_12 : 0}), int(0u), int(16u))))))
	u_35_4 = ((u_37_3 << 16u) + uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_18_12), int(0u), int(16u))))));
	// 0  <=>  (({u_39_3 : 0} << 16u) + uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_26_5 : 0}), int(0u), int(16u))))))
	u_36_4 = ((u_39_3 << 16u) + uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_26_5), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_32_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_7 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_7 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_38_6 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_32_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_7), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_7), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_32_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_7 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_7 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_32_0 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_7 : 1}), int(0u), int(16u))))))
	u_36_7 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_32_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_7), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_7), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_32_0), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_7), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_31_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_13 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_14_13 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_44_2 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_31_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_14_13), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_31_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_13 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_14_13 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_31_3 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_13 : 1}), int(0u), int(16u))))))
	u_36_10 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_31_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_14_13), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_31_3), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_13), int(0u), int(16u))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_8 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_10 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_40_9 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_8), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_10), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_8 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_10 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_10_8 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_10 : 1}), int(0u), int(16u))))))
	u_38_11 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_8), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_10), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_10_8), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_10), int(0u), int(16u))))));
	// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
	u_38_12 = ftou(i.vao_attr7.x);
	u_38_phi_61 = u_38_12;
	// False  <=>  if((({u_19_phi_60 : 2} == 1u) ? true : false))
	if(((u_19_phi_60 == 1u) ? true : false))
	{
		// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
		u_38_13 = ftou(i.vao_attr7.y);
		u_38_phi_61 = u_38_13;
	}
	// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
	u_39_9 = ftou(i.vao_attr7.y);
	u_39_phi_62 = u_39_9;
	// False  <=>  if((({u_19_phi_60 : 2} == 1u) ? true : false))
	if(((u_19_phi_60 == 1u) ? true : false))
	{
		// 1025206009  <=>  {ftou(i.vao_attr7.z) : 1025206009}
		u_39_10 = ftou(i.vao_attr7.z);
		u_39_phi_62 = u_39_10;
	}
	// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
	u_40_10 = ftou(i.vao_attr7.x);
	u_40_phi_63 = u_40_10;
	// False  <=>  if((({u_19_phi_60 : 2} == 1u) ? true : false))
	if(((u_19_phi_60 == 1u) ? true : false))
	{
		// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
		u_40_11 = ftou(i.vao_attr7.y);
		u_40_phi_63 = u_40_11;
	}
	// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
	u_44_3 = ftou(i.vao_attr7.y);
	u_44_phi_64 = u_44_3;
	// False  <=>  if((({u_19_phi_60 : 2} == 1u) ? true : false))
	if(((u_19_phi_60 == 1u) ? true : false))
	{
		// 1025206009  <=>  {ftou(i.vao_attr7.z) : 1025206009}
		u_44_4 = ftou(i.vao_attr7.z);
		u_44_phi_64 = u_44_4;
	}
	// 1062322915  <=>  {u_40_phi_63 : 1062322915}
	u_45_2 = u_40_phi_63;
	// 1050228891  <=>  {u_44_phi_64 : 1050228891}
	u_46_2 = u_44_phi_64;
	// 1050228891  <=>  {u_39_phi_62 : 1050228891}
	u_47_2 = u_39_phi_62;
	// 1062322915  <=>  {u_38_phi_61 : 1062322915}
	u_48_0 = u_38_phi_61;
	u_45_phi_65 = u_45_2;
	u_46_phi_65 = u_46_2;
	u_47_phi_65 = u_47_2;
	u_48_phi_65 = u_48_0;
	// True  <=>  if(((! ({u_19_phi_60 : 2} == 1u)) ? true : false))
	if(((! (u_19_phi_60 == 1u)) ? true : false))
	{
		// 1062322915  <=>  {u_38_phi_61 : 1062322915}
		u_19_13 = u_38_phi_61;
		u_19_phi_66 = u_19_13;
		// True  <=>  if((({u_19_phi_60 : 2} == 2u) ? true : false))
		if(((u_19_phi_60 == 2u) ? true : false))
		{
			// 1025206009  <=>  {ftou(i.vao_attr7.z) : 1025206009}
			u_19_14 = ftou(i.vao_attr7.z);
			u_19_phi_66 = u_19_14;
		}
		// 1050228891  <=>  {u_39_phi_62 : 1050228891}
		u_8_9 = u_39_phi_62;
		u_8_phi_67 = u_8_9;
		// True  <=>  if((({u_19_phi_60 : 2} == 2u) ? true : false))
		if(((u_19_phi_60 == 2u) ? true : false))
		{
			// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
			u_8_10 = ftou(i.vao_attr7.x);
			u_8_phi_67 = u_8_10;
		}
		// 1062322915  <=>  {u_40_phi_63 : 1062322915}
		u_49_0 = u_40_phi_63;
		u_49_phi_68 = u_49_0;
		// True  <=>  if((({u_19_phi_60 : 2} == 2u) ? true : false))
		if(((u_19_phi_60 == 2u) ? true : false))
		{
			// 1062322915  <=>  {ftou(i.vao_attr7.x) : 1062322915}
			u_49_1 = ftou(i.vao_attr7.x);
			u_49_phi_68 = u_49_1;
		}
		// 1050228891  <=>  {u_44_phi_64 : 1050228891}
		u_0_7 = u_44_phi_64;
		u_0_phi_69 = u_0_7;
		// True  <=>  if((({u_19_phi_60 : 2} == 2u) ? true : false))
		if(((u_19_phi_60 == 2u) ? true : false))
		{
			// 1050228891  <=>  {ftou(i.vao_attr7.y) : 1050228891}
			u_0_8 = ftou(i.vao_attr7.y);
			u_0_phi_69 = u_0_8;
		}
		// 1062322915  <=>  {u_49_phi_68 : 1062322915}
		u_45_3 = u_49_phi_68;
		// 1050228891  <=>  {u_0_phi_69 : 1050228891}
		u_46_3 = u_0_phi_69;
		// 1062322915  <=>  {u_8_phi_67 : 1062322915}
		u_47_3 = u_8_phi_67;
		// 1025206009  <=>  {u_19_phi_66 : 1025206009}
		u_48_1 = u_19_phi_66;
		u_45_phi_65 = u_45_3;
		u_46_phi_65 = u_46_3;
		u_47_phi_65 = u_47_3;
		u_48_phi_65 = u_48_1;
	}
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_1_58 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_7_17), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_32_0 : 0}), int(16u), int(16u))) * {u_38_6 : 1})) << 16u) + {u_36_7 : 0}))))
	u_3_17 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_32_0), int(16u), int(16u))) * u_38_6)) << 16u) + u_36_7))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_31_3 : 0}), int(16u), int(16u))) * {u_44_2 : 1})) << 16u) + {u_36_10 : 0}))))
	u_8_12 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_31_3), int(16u), int(16u))) * u_44_2)) << 16u) + u_36_10))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_10_8 : 0}), int(16u), int(16u))) * {u_40_9 : 1})) << 16u) + {u_38_11 : 0}))))
	u_0_11 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_10_8), int(16u), int(16u))) * u_40_9)) << 16u) + u_38_11))));
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_7_17 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_29_4 : 0}), int(16u), int(16u))))) << 16u) + {u_27_6 : 0}))))))), float(0.f), float(4294967300.f)))
	u_0_12 = uint(clamp(trunc((utof((ftou((1.0f / float(u_7_17))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_7_17), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_29_4), int(16u), int(16u))))) << 16u) + u_27_6))))))), float(0.f), float(4294967300.f)));
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_9_11 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_37_3 : 0}), int(16u), int(16u))))) << 16u) + {u_35_4 : 0}))))))), float(0.f), float(4294967300.f)))
	u_3_18 = uint(clamp(trunc((utof((ftou((1.0f / float(u_9_11))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_9_11), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_37_3), int(16u), int(16u))))) << 16u) + u_35_4))))))), float(0.f), float(4294967300.f)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_25 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_7_17), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_9_11 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_2_24 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_9_11), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint(clamp(trunc(({utof(({ftou((1.0f / float({u_11_10 : 1}))) : 1065353216} + 4294967294u)) : 0.9999999} * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(16u), int(16u))) * uint(bitfieldExtract(uint({u_39_3 : 0}), int(16u), int(16u))))) << 16u) + {u_36_4 : 0}))))))), float(0.f), float(4294967300.f)))
	u_16_14 = uint(clamp(trunc((utof((ftou((1.0f / float(u_11_10))) + 4294967294u)) * float(uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_11_10), int(16u), int(16u))) * uint(bitfieldExtract(uint(u_39_3), int(16u), int(16u))))) << 16u) + u_36_4))))))), float(0.f), float(4294967300.f)));
	// 0  <=>  ({u_23_5 : 0} + {u_0_12 : 0})
	u_0_13 = (u_23_5 + u_0_12);
	// 0  <=>  ({u_32_0 : 0} + uint(clamp(trunc(({utof(u_28_5) : 0.9999999} * float({u_3_17 : 0}))), float(0.f), float(4294967300.f))))
	u_8_14 = (u_32_0 + uint(clamp(trunc((utof(u_28_5) * float(u_3_17))), float(0.f), float(4294967300.f))));
	// 0  <=>  ({u_18_12 : 0} + {u_3_18 : 0})
	u_3_19 = (u_18_12 + u_3_18);
	// 0  <=>  ({u_26_5 : 0} + {u_16_14 : 0})
	u_16_15 = (u_26_5 + u_16_14);
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_9_11 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_1_60 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_9_11), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_3_10 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_9_11), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  ({u_31_3 : 0} + uint(clamp(trunc(({utof(u_22_7) : 0.9999999} * float({u_8_12 : 0}))), float(0.f), float(4294967300.f))))
	u_19_16 = (u_31_3 + uint(clamp(trunc((utof(u_22_7) * float(u_8_12))), float(0.f), float(4294967300.f))));
	// 0  <=>  ({u_10_8 : 0} + uint(clamp(trunc(({utof(u_21_7) : 0.9999999} * float({u_0_11 : 0}))), float(0.f), float(4294967300.f))))
	u_10_9 = (u_10_8 + uint(clamp(trunc((utof(u_21_7) * float(u_0_11))), float(0.f), float(4294967300.f))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_14 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_7 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_7 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_27_8 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_14), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_7), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_7), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_8_14 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_7 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_13_7 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
	u_22_14 = (bitfieldInsert(uint((uint(bitfieldExtract(uint(u_8_14), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_7), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_13_7), int(0u), int(16u))), int(16u), int(16u)) << 16u);
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_8_14 : 0}), int(16u), int(16u))) * {u_27_8 : 1})) << 16u) + ({u_22_14 : 0} + uint((uint(bitfieldExtract(uint({u_8_14 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_13_7 : 1}), int(0u), int(16u)))))))
	u_17_25 = ((uint((uint(bitfieldExtract(uint(u_8_14), int(16u), int(16u))) * u_27_8)) << 16u) + (u_22_14 + uint((uint(bitfieldExtract(uint(u_8_14), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_13_7), int(0u), int(16u)))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_0_13 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_0_13 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_22_15 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_0_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_0_13), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_0_13 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_0_13 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_0_13 : 0}), int(0u), int(16u))))))
	u_21_12 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_0_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_0_13), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_0_13), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int({u_17_25 : 0})))
	u_17_26 = uint((int(0) - int(u_17_25)));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_19 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_3_19 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_36_12 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_19), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_3_19), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_19 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_3_19 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_19 : 0}), int(0u), int(16u))))))
	u_21_15 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_19), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_3_19), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_19), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_15 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_35_14 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_15), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_15 : 0}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_16_15 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_15 : 0}), int(0u), int(16u))))))
	u_23_14 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_15), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_16_15), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_15), int(0u), int(16u))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(16u), int(16u))) * {u_22_15 : 0})) << 16u) + {u_21_12 : 0}))))
	u_18_20 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_7_17), int(16u), int(16u))) * u_22_15)) << 16u) + u_21_12))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_19_16 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_13 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_14_13 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_27_14 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_19_16), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_14_13), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_19_16 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_13 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_14_13 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
	u_27_15 = (bitfieldInsert(uint((uint(bitfieldExtract(uint(u_19_16), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_13), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_14_13), int(0u), int(16u))), int(16u), int(16u)) << 16u);
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_19_16 : 0}), int(16u), int(16u))) * {u_27_14 : 1})) << 16u) + ({u_27_15 : 0} + uint((uint(bitfieldExtract(uint({u_19_16 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_14_13 : 1}), int(0u), int(16u)))))))
	u_23_18 = ((uint((uint(bitfieldExtract(uint(u_19_16), int(16u), int(16u))) * u_27_14)) << 16u) + (u_27_15 + uint((uint(bitfieldExtract(uint(u_19_16), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_14_13), int(0u), int(16u)))))));
	// 1  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_9 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_10 : 1}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_27_17 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_10), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  (bitfieldInsert(uint((uint(bitfieldExtract(uint({u_10_9 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_10 : 1}), int(16u), int(16u))))), uint(bitfieldExtract(uint({u_15_10 : 1}), int(0u), int(16u))), int(16u), int(16u)) << 16u)
	u_27_18 = (bitfieldInsert(uint((uint(bitfieldExtract(uint(u_10_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_10), int(16u), int(16u))))), uint(bitfieldExtract(uint(u_15_10), int(0u), int(16u))), int(16u), int(16u)) << 16u);
	// 0  <=>  ((uint((uint(bitfieldExtract(uint({u_10_9 : 0}), int(16u), int(16u))) * {u_27_17 : 1})) << 16u) + ({u_27_18 : 0} + uint((uint(bitfieldExtract(uint({u_10_9 : 0}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_15_10 : 1}), int(0u), int(16u)))))))
	u_17_30 = ((uint((uint(bitfieldExtract(uint(u_10_9), int(16u), int(16u))) * u_27_17)) << 16u) + (u_27_18 + uint((uint(bitfieldExtract(uint(u_10_9), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_15_10), int(0u), int(16u)))))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(16u), int(16u))) * {u_36_12 : 0})) << 16u) + {u_21_15 : 0}))))
	u_21_17 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_9_11), int(16u), int(16u))) * u_36_12)) << 16u) + u_21_15))));
	// 0  <=>  uint((int(0) - int(((uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(16u), int(16u))) * {u_35_14 : 0})) << 16u) + {u_23_14 : 0}))))
	u_22_25 = uint((int(0) - int(((uint((uint(bitfieldExtract(uint(u_11_10), int(16u), int(16u))) * u_35_14)) << 16u) + u_23_14))));
	// 0  <=>  ((uint({u_18_20 : 0}) >= uint({u_7_17 : 1})) ? 4294967295u : 0u)
	u_18_21 = ((uint(u_18_20) >= uint(u_7_17)) ? 4294967295u : 0u);
	// 0  <=>  uint((int(0) - int({u_23_18 : 0})))
	u_23_19 = uint((int(0) - int(u_23_18)));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_2_26 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_9_11), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_11_10 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_4_4 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_11_10), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint((int(0) - int({u_17_30 : 0})))
	u_17_31 = uint((int(0) - int(u_17_30)));
	// 0  <=>  ((uint({u_21_17 : 0}) >= uint({u_9_11 : 1})) ? 4294967295u : 0u)
	u_21_18 = ((uint(u_21_17) >= uint(u_9_11)) ? 4294967295u : 0u);
	// 0  <=>  (uint((int(0) - int({u_0_13 : 0}))) + {u_18_21 : 0})
	u_0_15 = (uint((int(0) - int(u_0_13))) + u_18_21);
	// 0  <=>  ((uint({u_22_25 : 0}) >= uint({u_11_10 : 1})) ? 4294967295u : 0u)
	u_18_22 = ((uint(u_22_25) >= uint(u_11_10)) ? 4294967295u : 0u);
	// 0  <=>  (uint((int(0) - int({u_3_19 : 0}))) + {u_21_18 : 0})
	u_3_21 = (uint((int(0) - int(u_3_19))) + u_21_18);
	// 0  <=>  uint(bitfieldExtract(uint({u_0_15 : 0}), int(16u), int(16u)))
	u_21_19 = uint(bitfieldExtract(uint(u_0_15), int(16u), int(16u)));
	// 0  <=>  (uint((int(0) - int({u_16_15 : 0}))) + {u_18_22 : 0})
	u_16_17 = (uint((int(0) - int(u_16_15))) + u_18_22);
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * {u_21_19 : 0})), uint(bitfieldExtract(uint({u_0_15 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_18_23 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * u_21_19)), uint(bitfieldExtract(uint(u_0_15), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * {u_21_19 : 0})), uint(bitfieldExtract(uint({u_0_15 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_0_15 : 0}), int(0u), int(16u))))))
	u_0_19 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * u_21_19)), uint(bitfieldExtract(uint(u_0_15), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_7_17), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_0_15), int(0u), int(16u))))));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_11_10 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_3_12 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_11_10), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_5_10 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_11_10), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  uint((int(0) - int(({u_9_11 : 1} >> 31u))))
	u_14_17 = uint((int(0) - int((u_9_11 >> 31u))));
	// 0  <=>  uint(bitfieldExtract(uint({u_3_21 : 0}), int(16u), int(16u)))
	u_21_20 = uint(bitfieldExtract(uint(u_3_21), int(16u), int(16u)));
	// 0  <=>  (({b_1_58 : False} || {b_0_25 : True}) ? ((uint((uint(bitfieldExtract(uint({u_7_17 : 1}), int(16u), int(16u))) * {u_18_23 : 0})) << 16u) + {u_0_19 : 0}) : 4294967295u)
	u_0_21 = ((b_1_58 || b_0_25) ? ((uint((uint(bitfieldExtract(uint(u_7_17), int(16u), int(16u))) * u_18_23)) << 16u) + u_0_19) : 4294967295u);
	// 0  <=>  uint((int(0) - int(({u_11_10 : 1} >> 31u))))
	u_0_22 = uint((int(0) - int((u_11_10 >> 31u))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].z) : 1.00}) * {utof(vs_cbuf9[78].x) : 1.00})
	pf_22_1 = ((1.0f / utof(vs_cbuf9[78].z)) * utof(vs_cbuf9[78].x));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * {u_21_20 : 0})), uint(bitfieldExtract(uint({u_3_21 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_17_39 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * u_21_20)), uint(bitfieldExtract(uint(u_3_21), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * {u_21_20 : 0})), uint(bitfieldExtract(uint({u_3_21 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_3_21 : 0}), int(0u), int(16u))))))
	u_3_25 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * u_21_20)), uint(bitfieldExtract(uint(u_3_21), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_9_11), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_3_21), int(0u), int(16u))))));
	// 0  <=>  uint(bitfieldExtract(uint({u_16_17 : 0}), int(16u), int(16u)))
	u_17_40 = uint(bitfieldExtract(uint(u_16_17), int(16u), int(16u)));
	// 0  <=>  (uint((int(0) - int({u_14_17 : 0}))) + (({u_19_16 : 0} + uint((int(0) - int(((uint({u_23_19 : 0}) >= uint({u_14_13 : 1})) ? 4294967295u : 0u))))) ^ {u_14_17 : 0}))
	u_13_14 = (uint((int(0) - int(u_14_17))) + ((u_19_16 + uint((int(0) - int(((uint(u_23_19) >= uint(u_14_13)) ? 4294967295u : 0u))))) ^ u_14_17));
	// 0  <=>  uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * {u_17_40 : 0})), uint(bitfieldExtract(uint({u_16_17 : 0}), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)))
	u_14_19 = uint(bitfieldExtract(uint(bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * u_17_40)), uint(bitfieldExtract(uint(u_16_17), int(0u), int(16u))), int(16u), int(16u))), int(16u), int(16u)));
	// 0  <=>  ((bitfieldInsert(uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * {u_17_40 : 0})), uint(bitfieldExtract(uint({u_16_17 : 0}), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(16u))) * uint(bitfieldExtract(uint({u_16_17 : 0}), int(0u), int(16u))))))
	u_9_19 = ((bitfieldInsert(uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * u_17_40)), uint(bitfieldExtract(uint(u_16_17), int(0u), int(16u))), int(16u), int(16u)) << 16u) + uint((uint(bitfieldExtract(uint(u_11_10), int(0u), int(16u))) * uint(bitfieldExtract(uint(u_16_17), int(0u), int(16u))))));
	// 0  <=>  (({b_3_10 : False} || {b_2_26 : True}) ? ((uint((uint(bitfieldExtract(uint({u_9_11 : 1}), int(16u), int(16u))) * {u_17_39 : 0})) << 16u) + {u_3_25 : 0}) : 4294967295u)
	u_3_27 = ((b_3_10 || b_2_26) ? ((uint((uint(bitfieldExtract(uint(u_9_11), int(16u), int(16u))) * u_17_39)) << 16u) + u_3_25) : 4294967295u);
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint({u_11_10 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_27 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(uint(u_11_10), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// False  <=>  (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_7_17 : 1}), int(0u), int(32u)))), int(0u), int(32u)))) < 0)
	b_4_6 = (int(uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_7_17), int(0u), int(32u)))), int(0u), int(32u)))) < 0);
	// 0  <=>  (uint((int(0) - int({u_0_22 : 0}))) + (({u_10_9 : 0} + uint((int(0) - int(((uint({u_17_31 : 0}) >= uint({u_15_10 : 1})) ? 4294967295u : 0u))))) ^ {u_0_22 : 0}))
	u_0_24 = (uint((int(0) - int(u_0_22))) + ((u_10_9 + uint((int(0) - int(((uint(u_17_31) >= uint(u_15_10)) ? 4294967295u : 0u))))) ^ u_0_22));
	// 0.2118764  <=>  (({utof(vs_cbuf9[105].x) : 0.8475056} * {(vs_cbuf10_0.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.25})
	o.fs_attr0.x = ((utof(vs_cbuf9[105].x) * (vs_cbuf10_0.x)) * utof(vs_cbuf9[104].x));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].z) : 1.00}) * {utof(vs_cbuf9[83].x) : 1.00})
	pf_18_3 = ((1.0f / utof(vs_cbuf9[83].z)) * utof(vs_cbuf9[83].x));
	// 0.2281746  <=>  (({utof(vs_cbuf9[105].y) : 0.9126984} * {(vs_cbuf10_0.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.25})
	o.fs_attr0.y = ((utof(vs_cbuf9[105].y) * (vs_cbuf10_0.y)) * utof(vs_cbuf9[104].x));
	// 0  <=>  (({b_5_10 : False} || {b_0_27 : True}) ? ((uint((uint(bitfieldExtract(uint({u_11_10 : 1}), int(16u), int(16u))) * {u_14_19 : 0})) << 16u) + {u_9_19 : 0}) : 4294967295u)
	u_9_21 = ((b_5_10 || b_0_27) ? ((uint((uint(bitfieldExtract(uint(u_11_10), int(16u), int(16u))) * u_14_19)) << 16u) + u_9_19) : 4294967295u);
	// 0  <=>  uint((int(0) - int(({u_7_17 : 1} >> 31u))))
	u_7_19 = uint((int(0) - int((u_7_17 >> 31u))));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[88].z) : 1.00}) * {utof(vs_cbuf9[88].x) : 1.00})
	pf_26_0 = ((1.0f / utof(vs_cbuf9[88].z)) * utof(vs_cbuf9[88].x));
	// True  <=>  (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int({u_7_17 : 1}), int(0u), int(32u)))), int(0u), int(32u))) == 0))
	b_0_29 = (! (uint(bitfieldExtract(uint(uint(bitfieldExtract(int(u_7_17), int(0u), int(32u)))), int(0u), int(32u))) == 0));
	// 0.1241024  <=>  (({utof(vs_cbuf9[121].x) : 0.4964097} * {(vs_cbuf10_1.x) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.25})
	o.fs_attr1.x = ((utof(vs_cbuf9[121].x) * (vs_cbuf10_1.x)) * utof(vs_cbuf9[104].x));
	// 0.2277897  <=>  (({utof(vs_cbuf9[105].z) : 0.9111589} * {(vs_cbuf10_0.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.25})
	o.fs_attr0.z = ((utof(vs_cbuf9[105].z) * (vs_cbuf10_0.z)) * utof(vs_cbuf9[104].x));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[88].w) : 1.00}) * {utof(vs_cbuf9[88].y) : 1.00})
	pf_24_2 = ((1.0f / utof(vs_cbuf9[88].w)) * utof(vs_cbuf9[88].y));
	// 0  <=>  (uint((int(0) - int({u_7_19 : 0}))) + (({u_8_14 : 0} + uint((int(0) - int(((uint({u_17_26 : 0}) >= uint({u_13_7 : 1})) ? 4294967295u : 0u))))) ^ {u_7_19 : 0}))
	u_0_27 = (uint((int(0) - int(u_7_19))) + ((u_8_14 + uint((int(0) - int(((uint(u_17_26) >= uint(u_13_7)) ? 4294967295u : 0u))))) ^ u_7_19));
	// 0.140873  <=>  (({utof(vs_cbuf9[121].z) : 0.5634921} * {(vs_cbuf10_1.z) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.25})
	o.fs_attr1.z = ((utof(vs_cbuf9[121].z) * (vs_cbuf10_1.z)) * utof(vs_cbuf9[104].x));
	// 1.00  <=>  ((({utof(u_3_12) : 0} * {utof(vs_cbuf9[116].x) : 0}) + (((((({utof(vs_cbuf9[116].x) : 0} + (0.f - {utof(vs_cbuf9[115].x) : 1.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[115].w) : 0.84}) + {utof(vs_cbuf9[116].w) : 1.00}))) * ({utof(u_3_phi_41) : 0.5433334} + (0.f - {utof(vs_cbuf9[115].w) : 0.84}))) + {utof(vs_cbuf9[115].x) : 1.00}) * (({utof(u_18_11) : 0} * (0.f - {utof(u_3_12) : 0})) + {utof(u_18_11) : 0})) + (((((((0.f - {utof(vs_cbuf9[114].x) : 1.00}) + {utof(vs_cbuf9[115].x) : 1.00}) * (1.0f / ({utof(vs_cbuf9[115].w) : 0.84} + (0.f - {utof(vs_cbuf9[114].w) : 0.21})))) * ({utof(u_3_phi_41) : 0.5433334} + (0.f - {utof(vs_cbuf9[114].w) : 0.21}))) + {utof(vs_cbuf9[114].x) : 1.00}) * (({utof(u_20_6) : 1.00} * (0.f - {utof(u_18_11) : 0})) + {utof(u_20_6) : 1.00})) + (((((({utof(vs_cbuf9[114].x) : 1.00} + (0.f - {utof(vs_cbuf9[113].x) : 0})) * (1.0f / ((0.f - {utof(vs_cbuf9[113].w) : 0}) + {utof(vs_cbuf9[114].w) : 0.21}))) * ({utof(u_3_phi_41) : 0.5433334} + (0.f - {utof(vs_cbuf9[113].w) : 0}))) + {utof(vs_cbuf9[113].x) : 0}) * (({utof(u_20_6) : 1.00} * (0.f - {utof(u_17_16) : 1.00})) + {utof(u_17_16) : 1.00})) + (({utof(u_17_16) : 1.00} * (0.f - {utof(vs_cbuf9[113].x) : 0})) + {utof(vs_cbuf9[113].x) : 0}))))) * {(vs_cbuf10_0.w) : 1.00})
	pf_10_10 = (((utof(u_3_12) * utof(vs_cbuf9[116].x)) + ((((((utof(vs_cbuf9[116].x) + (0.f - utof(vs_cbuf9[115].x))) * (1.0f / ((0.f - utof(vs_cbuf9[115].w)) + utof(vs_cbuf9[116].w)))) * (utof(u_3_phi_41) + (0.f - utof(vs_cbuf9[115].w)))) + utof(vs_cbuf9[115].x)) * ((utof(u_18_11) * (0.f - utof(u_3_12))) + utof(u_18_11))) + (((((((0.f - utof(vs_cbuf9[114].x)) + utof(vs_cbuf9[115].x)) * (1.0f / (utof(vs_cbuf9[115].w) + (0.f - utof(vs_cbuf9[114].w))))) * (utof(u_3_phi_41) + (0.f - utof(vs_cbuf9[114].w)))) + utof(vs_cbuf9[114].x)) * ((utof(u_20_6) * (0.f - utof(u_18_11))) + utof(u_20_6))) + ((((((utof(vs_cbuf9[114].x) + (0.f - utof(vs_cbuf9[113].x))) * (1.0f / ((0.f - utof(vs_cbuf9[113].w)) + utof(vs_cbuf9[114].w)))) * (utof(u_3_phi_41) + (0.f - utof(vs_cbuf9[113].w)))) + utof(vs_cbuf9[113].x)) * ((utof(u_20_6) * (0.f - utof(u_17_16))) + utof(u_17_16))) + ((utof(u_17_16) * (0.f - utof(vs_cbuf9[113].x))) + utof(vs_cbuf9[113].x)))))) * (vs_cbuf10_0.w));
	// 1.00  <=>  {pf_10_10 : 1.00}
	o.fs_attr0.w = pf_10_10;
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[83].w) : 1.00}) * {utof(vs_cbuf9[83].y) : 1.00})
	pf_14_6 = ((1.0f / utof(vs_cbuf9[83].w)) * utof(vs_cbuf9[83].y));
	// 1.00  <=>  ((1.0f / {utof(vs_cbuf9[78].w) : 1.00}) * {utof(vs_cbuf9[78].y) : 1.00})
	pf_20_8 = ((1.0f / utof(vs_cbuf9[78].w)) * utof(vs_cbuf9[78].y));
	// 0.1333461  <=>  (({utof(vs_cbuf9[121].y) : 0.5333842} * {(vs_cbuf10_1.y) : 1.00}) * {utof(vs_cbuf9[104].x) : 0.25})
	o.fs_attr1.y = ((utof(vs_cbuf9[121].y) * (vs_cbuf10_1.y)) * utof(vs_cbuf9[104].x));
	// 0.50196  <=>  ((((({pf_0_1 : 163.00} * {utof(vs_cbuf9[85].z) : 0}) + (({utof(u_45_phi_65) : 0.81938} * {utof(vs_cbuf9[86].z) : 0}) + ({utof(vs_cbuf9[86].z) : 0} + {utof(vs_cbuf9[86].x) : 1.00}))) * (({pf_26_0 : 1.00} * {utof(u_33_phi_50) : 0.50196}) + -0.5f)) + (({pf_26_0 : 1.00} * float(int({u_9_21 : 0}))) + (({pf_0_1 : 163.00} * (0.f - {utof(vs_cbuf9[84].x) : 0})) + (0.f - ((({utof(u_48_phi_65) : 0.03794} * {utof(vs_cbuf9[85].x) : 0}) * -2.f) + ({utof(vs_cbuf9[85].x) : 0} + {utof(vs_cbuf9[84].z) : 0})))))) + 0.5f)
	o.fs_attr3.x = (((((pf_0_1 * utof(vs_cbuf9[85].z)) + ((utof(u_45_phi_65) * utof(vs_cbuf9[86].z)) + (utof(vs_cbuf9[86].z) + utof(vs_cbuf9[86].x)))) * ((pf_26_0 * utof(u_33_phi_50)) + -0.5f)) + ((pf_26_0 * float(int(u_9_21))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[84].x))) + (0.f - (((utof(u_48_phi_65) * utof(vs_cbuf9[85].x)) * -2.f) + (utof(vs_cbuf9[85].x) + utof(vs_cbuf9[84].z))))))) + 0.5f);
	// -0.057953417  <=>  ((((({pf_18_3 : 1.00} * {utof(u_30_phi_49) : 0.50196}) + -0.5f) * (({pf_0_1 : 163.00} * {utof(vs_cbuf9[80].z) : 0}) + (({utof(u_43_phi_55) : 0.29926} * {utof(vs_cbuf9[81].z) : 0.10}) + ({utof(vs_cbuf9[81].z) : 0.10} + {utof(vs_cbuf9[81].x) : 3.20})))) + (({pf_18_3 : 1.00} * float(int({u_3_27 : 0}))) + (({pf_0_1 : 163.00} * (0.f - {utof(vs_cbuf9[79].x) : 0.001})) + (0.f - ((({utof(u_42_phi_55) : 0.29926} * {utof(vs_cbuf9[80].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].x) : 1.00} + {utof(vs_cbuf9[79].z) : 0})))))) + 0.5f)
	o.fs_attr2.z = (((((pf_18_3 * utof(u_30_phi_49)) + -0.5f) * ((pf_0_1 * utof(vs_cbuf9[80].z)) + ((utof(u_43_phi_55) * utof(vs_cbuf9[81].z)) + (utof(vs_cbuf9[81].z) + utof(vs_cbuf9[81].x))))) + ((pf_18_3 * float(int(u_3_27))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[79].x))) + (0.f - (((utof(u_42_phi_55) * utof(vs_cbuf9[80].x)) * -2.f) + (utof(vs_cbuf9[80].x) + utof(vs_cbuf9[79].z))))))) + 0.5f);
	// 0.9785039  <=>  ((((({pf_22_1 : 1.00} * {utof(u_29_phi_48) : 0.50196}) + -0.5f) * (({pf_0_1 : 163.00} * {utof(vs_cbuf9[75].z) : 0}) + (({i.vao_attr7.x : 0.81938} * {utof(vs_cbuf9[76].z) : 0}) + ({utof(vs_cbuf9[76].x) : 1.40} + {utof(vs_cbuf9[76].z) : 0})))) + (({pf_22_1 : 1.00} * float(int({u_0_21 : 0}))) + (({pf_0_1 : 163.00} * (0.f - {utof(vs_cbuf9[74].x) : 0.001})) + (0.f - ((({i.vao_attr7.x : 0.81938} * {utof(vs_cbuf9[75].x) : 1.00}) * -2.f) + ({utof(vs_cbuf9[75].x) : 1.00} + {utof(vs_cbuf9[74].z) : 0})))))) + 0.5f)
	o.fs_attr2.x = (((((pf_22_1 * utof(u_29_phi_48)) + -0.5f) * ((pf_0_1 * utof(vs_cbuf9[75].z)) + ((i.vao_attr7.x * utof(vs_cbuf9[76].z)) + (utof(vs_cbuf9[76].x) + utof(vs_cbuf9[76].z))))) + ((pf_22_1 * float(int(u_0_21))) + ((pf_0_1 * (0.f - utof(vs_cbuf9[74].x))) + (0.f - (((i.vao_attr7.x * utof(vs_cbuf9[75].x)) * -2.f) + (utof(vs_cbuf9[75].x) + utof(vs_cbuf9[74].z))))))) + 0.5f);
	// -2.49902  <=>  ((((({pf_0_1 : 163.00} * {utof(vs_cbuf9[85].w) : 0}) + (({utof(u_46_phi_65) : 0.29926} * {utof(vs_cbuf9[86].w) : 0}) + ({utof(vs_cbuf9[86].w) : 0} + {utof(vs_cbuf9[86].y) : 0.50}))) * (({pf_24_2 : 1.00} * {utof(u_25_phi_46) : 0.50196}) + -0.5f)) + (0.f - (({pf_24_2 : 1.00} * (0.f - float(int((({b_4_4 : False} || {b_3_12 : True}) ? {u_0_24 : 0} : 4294967295u))))) + (({pf_0_1 : 163.00} * {utof(vs_cbuf9[84].y) : 0}) + ((({utof(u_47_phi_65) : 0.81938} * {utof(vs_cbuf9[85].y) : 0}) * -2.f) + ({utof(vs_cbuf9[85].y) : 0} + {utof(vs_cbuf9[84].w) : 3.00})))))) + 0.5f)
	o.fs_attr3.y = (((((pf_0_1 * utof(vs_cbuf9[85].w)) + ((utof(u_46_phi_65) * utof(vs_cbuf9[86].w)) + (utof(vs_cbuf9[86].w) + utof(vs_cbuf9[86].y)))) * ((pf_24_2 * utof(u_25_phi_46)) + -0.5f)) + (0.f - ((pf_24_2 * (0.f - float(int(((b_4_4 || b_3_12) ? u_0_24 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[84].y)) + (((utof(u_47_phi_65) * utof(vs_cbuf9[85].y)) * -2.f) + (utof(vs_cbuf9[85].y) + utof(vs_cbuf9[84].w))))))) + 0.5f);
	// -0.30511266  <=>  ((((({pf_0_1 : 163.00} * {utof(vs_cbuf9[80].w) : 0}) + (({utof(u_44_phi_55) : 0.03794} * {utof(vs_cbuf9[81].w) : 0.10}) + ({utof(vs_cbuf9[81].w) : 0.10} + {utof(vs_cbuf9[81].y) : 2.40}))) * (({pf_14_6 : 1.00} * {utof(u_24_phi_45) : 0.50196}) + -0.5f)) + (0.f - (({pf_14_6 : 1.00} * (0.f - float(int((({b_2_24 : False} || {b_1_60 : True}) ? {u_13_14 : 0} : 4294967295u))))) + (({pf_0_1 : 163.00} * {utof(vs_cbuf9[79].y) : -0.0007}) + ((({utof(u_41_phi_55) : 0.03794} * {utof(vs_cbuf9[80].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].y) : 1.00} + {utof(vs_cbuf9[79].w) : 0})))))) + 0.5f)
	o.fs_attr2.w = (((((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_44_phi_55) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y)))) * ((pf_14_6 * utof(u_24_phi_45)) + -0.5f)) + (0.f - ((pf_14_6 * (0.f - float(int(((b_2_24 || b_1_60) ? u_13_14 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_41_phi_55) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f);
	// -3892.4797  <=>  ((({pf_4_3 : 44.27853} * {i.vao_attr11.z : 1.00}) + (({pf_3_3 : 1.64} * {f_5_0 : 0}) + ({pf_1_16 : 108.8271} * {i.vao_attr11.x : 0}))) + {i.vao_attr11.w : -3936.7583})
	pf_4_7 = (((pf_4_3 * i.vao_attr11.z) + ((pf_3_3 * f_5_0) + (pf_1_16 * i.vao_attr11.x))) + i.vao_attr11.w);
	// 0.50196  <=>  ((((({pf_0_1 : 163.00} * {utof(vs_cbuf9[75].w) : 0}) + (({i.vao_attr7.y : 0.29926} * {utof(vs_cbuf9[76].w) : 0}) + ({utof(vs_cbuf9[76].w) : 0} + {utof(vs_cbuf9[76].y) : 1.00}))) * (({pf_20_8 : 1.00} * {utof(u_20_phi_44) : 0.50196}) + -0.5f)) + (0.f - (({pf_20_8 : 1.00} * (0.f - float(int((({b_4_6 : False} || {b_0_29 : True}) ? {u_0_27 : 0} : 4294967295u))))) + (({pf_0_1 : 163.00} * {utof(vs_cbuf9[74].y) : 0}) + ((({i.vao_attr7.y : 0.29926} * {utof(vs_cbuf9[75].y) : 0}) * -2.f) + ({utof(vs_cbuf9[75].y) : 0} + {utof(vs_cbuf9[74].w) : 0})))))) + 0.5f)
	o.fs_attr2.y = (((((pf_0_1 * utof(vs_cbuf9[75].w)) + ((i.vao_attr7.y * utof(vs_cbuf9[76].w)) + (utof(vs_cbuf9[76].w) + utof(vs_cbuf9[76].y)))) * ((pf_20_8 * utof(u_20_phi_44)) + -0.5f)) + (0.f - ((pf_20_8 * (0.f - float(int(((b_4_6 || b_0_29) ? u_0_27 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[74].y)) + (((i.vao_attr7.y * utof(vs_cbuf9[75].y)) * -2.f) + (utof(vs_cbuf9[75].y) + utof(vs_cbuf9[74].w))))))) + 0.5f);
	// 57.22958  <=>  ((({pf_4_3 : 44.27853} * {i.vao_attr10.z : 0}) + (({pf_3_3 : 1.64} * {f_7_0 : 1.00}) + ({pf_1_16 : 108.8271} * {i.vao_attr10.x : 0}))) + {i.vao_attr10.w : 55.58958})
	pf_0_7 = (((pf_4_3 * i.vao_attr10.z) + ((pf_3_3 * f_7_0) + (pf_1_16 * i.vao_attr10.x))) + i.vao_attr10.w);
	// -1025.4431  <=>  ((({pf_4_3 : 44.27853} * {i.vao_attr9.z : 0}) + (({pf_3_3 : 1.64} * {f_9_1 : 0}) + ({pf_1_16 : 108.8271} * {i.vao_attr9.x : 1.00}))) + {i.vao_attr9.w : -1134.2701})
	pf_1_20 = (((pf_4_3 * i.vao_attr9.z) + ((pf_3_3 * f_9_1) + (pf_1_16 * i.vao_attr9.x))) + i.vao_attr9.w);
	// 3197908922  <=>  {ftou(((((({pf_0_1 : 163.00} * {utof(vs_cbuf9[80].w) : 0}) + (({utof(u_44_phi_55) : 0.03794} * {utof(vs_cbuf9[81].w) : 0.10}) + ({utof(vs_cbuf9[81].w) : 0.10} + {utof(vs_cbuf9[81].y) : 2.40}))) * (({pf_14_6 : 1.00} * {utof(u_24_phi_45) : 0.50196}) + -0.5f)) + (0.f - (({pf_14_6 : 1.00} * (0.f - float(int((({b_2_24 : False} || {b_1_60 : True}) ? {u_13_14 : 0} : 4294967295u))))) + (({pf_0_1 : 163.00} * {utof(vs_cbuf9[79].y) : -0.0007}) + ((({utof(u_41_phi_55) : 0.03794} * {utof(vs_cbuf9[80].y) : 1.00}) * -2.f) + ({utof(vs_cbuf9[80].y) : 1.00} + {utof(vs_cbuf9[79].w) : 0})))))) + 0.5f)) : 3197908922}
	u_9_22 = ftou((((((pf_0_1 * utof(vs_cbuf9[80].w)) + ((utof(u_44_phi_55) * utof(vs_cbuf9[81].w)) + (utof(vs_cbuf9[81].w) + utof(vs_cbuf9[81].y)))) * ((pf_14_6 * utof(u_24_phi_45)) + -0.5f)) + (0.f - ((pf_14_6 * (0.f - float(int(((b_2_24 || b_1_60) ? u_13_14 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[79].y)) + (((utof(u_41_phi_55) * utof(vs_cbuf9[80].y)) * -2.f) + (utof(vs_cbuf9[80].y) + utof(vs_cbuf9[79].w))))))) + 0.5f));
	// 1048576  <=>  (1048576u & {vs_cbuf9_7_y : 1048576})
	u_10_12 = (1048576u & vs_cbuf9_7_y);
	// 3223318514  <=>  {ftou(((((({pf_0_1 : 163.00} * {utof(vs_cbuf9[85].w) : 0}) + (({utof(u_46_phi_65) : 0.29926} * {utof(vs_cbuf9[86].w) : 0}) + ({utof(vs_cbuf9[86].w) : 0} + {utof(vs_cbuf9[86].y) : 0.50}))) * (({pf_24_2 : 1.00} * {utof(u_25_phi_46) : 0.50196}) + -0.5f)) + (0.f - (({pf_24_2 : 1.00} * (0.f - float(int((({b_4_4 : False} || {b_3_12 : True}) ? {u_0_24 : 0} : 4294967295u))))) + (({pf_0_1 : 163.00} * {utof(vs_cbuf9[84].y) : 0}) + ((({utof(u_47_phi_65) : 0.81938} * {utof(vs_cbuf9[85].y) : 0}) * -2.f) + ({utof(vs_cbuf9[85].y) : 0} + {utof(vs_cbuf9[84].w) : 3.00})))))) + 0.5f)) : 3223318514}
	u_11_14 = ftou((((((pf_0_1 * utof(vs_cbuf9[85].w)) + ((utof(u_46_phi_65) * utof(vs_cbuf9[86].w)) + (utof(vs_cbuf9[86].w) + utof(vs_cbuf9[86].y)))) * ((pf_24_2 * utof(u_25_phi_46)) + -0.5f)) + (0.f - ((pf_24_2 * (0.f - float(int(((b_4_4 || b_3_12) ? u_0_24 : 4294967295u))))) + ((pf_0_1 * utof(vs_cbuf9[84].y)) + (((utof(u_47_phi_65) * utof(vs_cbuf9[85].y)) * -2.f) + (utof(vs_cbuf9[85].y) + utof(vs_cbuf9[84].w))))))) + 0.5f));
	u_9_phi_70 = u_9_22;
	u_10_phi_70 = u_10_12;
	u_11_phi_70 = u_11_14;
	// True  <=>  if((((1048576u & {vs_cbuf9_7_y : 1048576}) == 1048576u) ? true : false))
	if((((1048576u & vs_cbuf9_7_y) == 1048576u) ? true : false))
	{
		// -893.8191  <=>  ((0.f - {pf_1_20 : -1025.4431}) + {(camera_wpos.x) : -1919.2622})
		pf_6_21 = ((0.f - pf_1_20) + (camera_wpos.x));
		// 308.5077  <=>  ((0.f - {pf_0_7 : 57.22958}) + {(camera_wpos.y) : 365.7373})
		pf_7_28 = ((0.f - pf_0_7) + (camera_wpos.y));
		// 159.4329  <=>  ((0.f - {pf_4_7 : -3892.4797}) + {(camera_wpos.z) : -3733.0469})
		pf_8_7 = ((0.f - pf_4_7) + (camera_wpos.z));
		// 1075813685  <=>  {ftou((({pf_8_7 : 159.4329} * inversesqrt((({pf_8_7 : 159.4329} * {pf_8_7 : 159.4329}) + (({pf_7_28 : 308.5077} * {pf_7_28 : 308.5077}) + ({pf_6_21 : -893.8191} * {pf_6_21 : -893.8191}))))) * {utof(vs_cbuf9[16].w) : 15.00})) : 1075813685}
		u_9_23 = ftou(((pf_8_7 * inversesqrt(((pf_8_7 * pf_8_7) + ((pf_7_28 * pf_7_28) + (pf_6_21 * pf_6_21))))) * utof(vs_cbuf9[16].w)));
		// 1083862496  <=>  {ftou((({pf_7_28 : 308.5077} * inversesqrt((({pf_8_7 : 159.4329} * {pf_8_7 : 159.4329}) + (({pf_7_28 : 308.5077} * {pf_7_28 : 308.5077}) + ({pf_6_21 : -893.8191} * {pf_6_21 : -893.8191}))))) * {utof(vs_cbuf9[16].w) : 15.00})) : 1083862496}
		u_10_13 = ftou(((pf_7_28 * inversesqrt(((pf_8_7 * pf_8_7) + ((pf_7_28 * pf_7_28) + (pf_6_21 * pf_6_21))))) * utof(vs_cbuf9[16].w)));
		// 3244275059  <=>  {ftou((({pf_6_21 : -893.8191} * inversesqrt((({pf_8_7 : 159.4329} * {pf_8_7 : 159.4329}) + (({pf_7_28 : 308.5077} * {pf_7_28 : 308.5077}) + ({pf_6_21 : -893.8191} * {pf_6_21 : -893.8191}))))) * {utof(vs_cbuf9[16].w) : 15.00})) : 3244275059}
		u_11_15 = ftou(((pf_6_21 * inversesqrt(((pf_8_7 * pf_8_7) + ((pf_7_28 * pf_7_28) + (pf_6_21 * pf_6_21))))) * utof(vs_cbuf9[16].w)));
		u_9_phi_70 = u_9_23;
		u_10_phi_70 = u_10_13;
		u_11_phi_70 = u_11_15;
	}
	// 1075813685  <=>  {u_9_phi_70 : 1075813685}
	u_0_31 = u_9_phi_70;
	// 1083862496  <=>  {u_10_phi_70 : 1083862496}
	u_7_23 = u_10_phi_70;
	// 3244275059  <=>  {u_11_phi_70 : 3244275059}
	u_8_17 = u_11_phi_70;
	u_0_phi_71 = u_0_31;
	u_7_phi_71 = u_7_23;
	u_8_phi_71 = u_8_17;
	// False  <=>  if(((! ((1048576u & {vs_cbuf9_7_y : 1048576}) == 1048576u)) ? true : false))
	if(((! ((1048576u & vs_cbuf9_7_y) == 1048576u)) ? true : false))
	{
		// 0  <=>  0u
		u_0_32 = 0u;
		// 0  <=>  0u
		u_7_24 = 0u;
		// 0  <=>  0u
		u_8_18 = 0u;
		u_0_phi_71 = u_0_32;
		u_7_phi_71 = u_7_24;
		u_8_phi_71 = u_8_18;
	}
	// -1039.4249  <=>  ({pf_1_20 : -1025.4431} + {utof(u_8_phi_71) : -13.981799})
	pf_7_30 = (pf_1_20 + utof(u_8_phi_71));
	// 100.0001  <=>  inversesqrt((({utof(u_4_phi_30) : 0} * {utof(u_4_phi_30) : 0}) + (({utof(u_5_phi_30) : 0.01} * {utof(u_5_phi_30) : 0.01}) + ({utof(u_6_phi_30) : 0} * {utof(u_6_phi_30) : 0}))))
	f_3_38 = inversesqrt(((utof(u_4_phi_30) * utof(u_4_phi_30)) + ((utof(u_5_phi_30) * utof(u_5_phi_30)) + (utof(u_6_phi_30) * utof(u_6_phi_30)))));
	// 1.00  <=>  ({f_3_38 : 100.0001} * {utof(u_5_phi_30) : 0.01})
	pf_9_14 = (f_3_38 * utof(u_5_phi_30));
	// 0  <=>  ({f_3_38 : 100.0001} * {utof(u_4_phi_30) : 0})
	pf_10_14 = (f_3_38 * utof(u_4_phi_30));
	// 0  <=>  ({f_3_38 : 100.0001} * {utof(u_6_phi_30) : 0})
	pf_11_17 = (f_3_38 * utof(u_6_phi_30));
	// 0  <=>  ((((clamp(min(0.f, {i.vao_attr7.x : 0.81938}), 0.0, 1.0) + {i.vao_attr6.x : 342.3774}) * (({utof(u_1_8) : 0} * {utof(vs_cbuf9[142].x) : 1.26}) + (((({pf_5_20 : 0.5433334} * (({utof(vs_cbuf9[142].x) : 1.26} + (0.f - {utof(vs_cbuf9[141].x) : 1.13})) * {f_12_28 : 1.00})) + {utof(vs_cbuf9[141].x) : 1.13}) * {pf_12_0 : 1.00}) + (({utof(u_3_8) : 1.00} * (0.f - {utof(vs_cbuf9[141].x) : 1.13})) + {utof(vs_cbuf9[141].x) : 1.13})))) * {(vs_cbuf10_3.y) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].x) : 0}) + {v.vertex.x : 0}))
	pf_2_63 = ((((clamp(min(0.f, i.vao_attr7.x), 0.0, 1.0) + i.vao_attr6.x) * ((utof(u_1_8) * utof(vs_cbuf9[142].x)) + ((((pf_5_20 * ((utof(vs_cbuf9[142].x) + (0.f - utof(vs_cbuf9[141].x))) * f_12_28)) + utof(vs_cbuf9[141].x)) * pf_12_0) + ((utof(u_3_8) * (0.f - utof(vs_cbuf9[141].x))) + utof(vs_cbuf9[141].x))))) * (vs_cbuf10_3.y)) * ((0.5f * utof(vs_cbuf9[16].x)) + v.vertex.x));
	// 41.10697  <=>  ((((({utof(u_1_8) : 0} * {utof(vs_cbuf9[142].y) : 1.26}) + (((({pf_5_20 : 0.5433334} * (({utof(vs_cbuf9[142].y) : 1.26} + (0.f - {utof(vs_cbuf9[141].y) : 1.13})) * {f_12_28 : 1.00})) + {utof(vs_cbuf9[141].y) : 1.13}) * {pf_12_0 : 1.00}) + (({utof(u_3_8) : 1.00} * (0.f - {utof(vs_cbuf9[141].y) : 1.13})) + {utof(vs_cbuf9[141].y) : 1.13}))) * {i.vao_attr6.y : 342.3774}) * {(vs_cbuf10_3.z) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].y) : 0.20}) + {v.vertex.y : 0}))
	pf_3_7 = (((((utof(u_1_8) * utof(vs_cbuf9[142].y)) + ((((pf_5_20 * ((utof(vs_cbuf9[142].y) + (0.f - utof(vs_cbuf9[141].y))) * f_12_28)) + utof(vs_cbuf9[141].y)) * pf_12_0) + ((utof(u_3_8) * (0.f - utof(vs_cbuf9[141].y))) + utof(vs_cbuf9[141].y)))) * i.vao_attr6.y) * (vs_cbuf10_3.z)) * ((0.5f * utof(vs_cbuf9[16].y)) + v.vertex.y));
	// 62.05549  <=>  ({pf_0_7 : 57.22958} + {utof(u_7_phi_71) : 4.825912})
	pf_6_30 = (pf_0_7 + utof(u_7_phi_71));
	// -303.68182  <=>  ({pf_6_30 : 62.05549} + (0.f - {(camera_wpos.y) : 365.7373}))
	pf_15_4 = (pf_6_30 + (0.f - (camera_wpos.y)));
	// -0.6398518  <=>  (({pf_10_14 : 0} * (0.f - {(vs_cbuf8_28.y) : 0.5074672})) + (0.f - ({pf_9_14 : 1.00} * (0.f - {(vs_cbuf8_28.z) : -0.6398518}))))
	pf_8_11 = ((pf_10_14 * (0.f - (vs_cbuf8_28.y))) + (0.f - (pf_9_14 * (0.f - (vs_cbuf8_28.z)))));
	// 0  <=>  (({pf_11_17 : 0} * (0.f - {(vs_cbuf8_28.z) : -0.6398518})) + (0.f - ({pf_10_14 : 0} * (0.f - {(vs_cbuf8_28.x) : -0.57711935}))))
	pf_12_8 = ((pf_11_17 * (0.f - (vs_cbuf8_28.z))) + (0.f - (pf_10_14 * (0.f - (vs_cbuf8_28.x)))));
	// 0.5771194  <=>  (({pf_9_14 : 1.00} * (0.f - {(vs_cbuf8_28.x) : -0.57711935})) + (0.f - ({pf_11_17 : 0} * (0.f - {(vs_cbuf8_28.y) : 0.5074672}))))
	pf_16_7 = ((pf_9_14 * (0.f - (vs_cbuf8_28.x))) + (0.f - (pf_11_17 * (0.f - (vs_cbuf8_28.y)))));
	// 1.160536  <=>  inversesqrt((({pf_16_7 : 0.5771194} * {pf_16_7 : 0.5771194}) + (({pf_12_8 : 0} * {pf_12_8 : 0}) + ({pf_8_11 : -0.6398518} * {pf_8_11 : -0.6398518}))))
	f_3_55 = inversesqrt(((pf_16_7 * pf_16_7) + ((pf_12_8 * pf_12_8) + (pf_8_11 * pf_8_11))));
	// 879.8373  <=>  ({pf_7_30 : -1039.4249} + (0.f - {(camera_wpos.x) : -1919.2622}))
	pf_18_5 = (pf_7_30 + (0.f - (camera_wpos.x)));
	// -0.74257076  <=>  ({pf_8_11 : -0.6398518} * {f_3_55 : 1.160536})
	pf_8_12 = (pf_8_11 * f_3_55);
	// 0  <=>  ({pf_12_8 : 0} * {f_3_55 : 1.160536})
	pf_12_9 = (pf_12_8 * f_3_55);
	// 0.4110697  <=>  ((((({utof(u_1_8) : 0} * {utof(vs_cbuf9[142].z) : 1.26}) + (((({pf_5_20 : 0.5433334} * (({utof(vs_cbuf9[142].z) : 1.26} + (0.f - {utof(vs_cbuf9[141].z) : 1.13})) * {f_12_28 : 1.00})) + {utof(vs_cbuf9[141].z) : 1.13}) * {pf_12_0 : 1.00}) + (({utof(u_3_8) : 1.00} * (0.f - {utof(vs_cbuf9[141].z) : 1.13})) + {utof(vs_cbuf9[141].z) : 1.13}))) * {i.vao_attr6.z : 342.3774}) * {(vs_cbuf10_3.w) : 1.00}) * ((0.5f * {utof(vs_cbuf9[16].z) : 0}) + {v.vertex.z : 0.001}))
	pf_5_26 = (((((utof(u_1_8) * utof(vs_cbuf9[142].z)) + ((((pf_5_20 * ((utof(vs_cbuf9[142].z) + (0.f - utof(vs_cbuf9[141].z))) * f_12_28)) + utof(vs_cbuf9[141].z)) * pf_12_0) + ((utof(u_3_8) * (0.f - utof(vs_cbuf9[141].z))) + utof(vs_cbuf9[141].z)))) * i.vao_attr6.z) * (vs_cbuf10_3.w)) * ((0.5f * utof(vs_cbuf9[16].z)) + v.vertex.z));
	// -3889.9858  <=>  ({pf_4_7 : -3892.4797} + {utof(u_0_phi_71) : 2.49397})
	pf_13_9 = (pf_4_7 + utof(u_0_phi_71));
	// -156.93896  <=>  ({pf_13_9 : -3889.9858} + (0.f - {(camera_wpos.z) : -3733.0469}))
	pf_18_7 = (pf_13_9 + (0.f - (camera_wpos.z)));
	// -0.30524835  <=>  (({pf_5_26 : 0.4110697} * (({pf_9_14 : 1.00} * {pf_8_12 : -0.74257076}) + (0.f - ({pf_11_17 : 0} * {pf_12_9 : 0})))) + (({pf_2_63 : 0} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536})) + ({pf_10_14 : 0} * {pf_3_7 : 41.10697})))
	pf_2_65 = ((pf_5_26 * ((pf_9_14 * pf_8_12) + (0.f - (pf_11_17 * pf_12_9)))) + ((pf_2_63 * (pf_16_7 * f_3_55)) + (pf_10_14 * pf_3_7)));
	// 1.00  <=>  (min(sqrt((({pf_18_7 : -156.93896} * {pf_18_7 : -156.93896}) + (({pf_15_4 : -303.68182} * {pf_15_4 : -303.68182}) + ({pf_18_5 : 879.8373} * {pf_18_5 : 879.8373})))), {utof(vs_cbuf9_197.x) : 400.00}) * (1.0f / {utof(vs_cbuf9_197.x) : 400.00}))
	pf_4_8 = (min(sqrt(((pf_18_7 * pf_18_7) + ((pf_15_4 * pf_15_4) + (pf_18_5 * pf_18_5)))), utof(vs_cbuf9_197.x)) * (1.0f / utof(vs_cbuf9_197.x)));
	// -1039.7002  <=>  (({pf_4_8 : 1.00} * ((0.f - {pf_7_30 : -1039.4249}) + (({pf_1_20 : -1025.4431} + (({pf_5_26 : 0.4110697} * (({pf_10_14 : 0} * {pf_12_9 : 0}) + (0.f - ({pf_9_14 : 1.00} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536}))))) + (({pf_2_63 : 0} * {pf_8_12 : -0.74257076}) + ({pf_11_17 : 0} * {pf_3_7 : 41.10697})))) + {utof(u_8_phi_71) : -13.981799}))) + {pf_7_30 : -1039.4249})
	pf_1_24 = ((pf_4_8 * ((0.f - pf_7_30) + ((pf_1_20 + ((pf_5_26 * ((pf_10_14 * pf_12_9) + (0.f - (pf_9_14 * (pf_16_7 * f_3_55))))) + ((pf_2_63 * pf_8_12) + (pf_11_17 * pf_3_7)))) + utof(u_8_phi_71)))) + pf_7_30);
	// 103.1625  <=>  (({pf_4_8 : 1.00} * ((0.f - {pf_6_30 : 62.05549}) + (({pf_0_7 : 57.22958} + (({pf_5_26 : 0.4110697} * (({pf_11_17 : 0} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536})) + (0.f - ({pf_10_14 : 0} * {pf_8_12 : -0.74257076})))) + (({pf_2_63 : 0} * {pf_12_9 : 0}) + ({pf_9_14 : 1.00} * {pf_3_7 : 41.10697})))) + {utof(u_7_phi_71) : 4.825912}))) + {pf_6_30 : 62.05549})
	pf_0_11 = ((pf_4_8 * ((0.f - pf_6_30) + ((pf_0_7 + ((pf_5_26 * ((pf_11_17 * (pf_16_7 * f_3_55)) + (0.f - (pf_10_14 * pf_8_12)))) + ((pf_2_63 * pf_12_9) + (pf_9_14 * pf_3_7)))) + utof(u_7_phi_71)))) + pf_6_30);
	// -879.562  <=>  ((0.f - {pf_1_24 : -1039.7002}) + {(camera_wpos.x) : -1919.2622})
	pf_17_10 = ((0.f - pf_1_24) + (camera_wpos.x));
	// 262.5748  <=>  ((0.f - {pf_0_11 : 103.1625}) + {(camera_wpos.y) : 365.7373})
	pf_18_8 = ((0.f - pf_0_11) + (camera_wpos.y));
	// -3890.291  <=>  (({pf_4_8 : 1.00} * ((0.f - {pf_13_9 : -3889.9858}) + (({pf_4_7 : -3892.4797} + {pf_2_65 : -0.30524835}) + {utof(u_0_phi_71) : 2.49397}))) + {pf_13_9 : -3889.9858})
	pf_2_69 = ((pf_4_8 * ((0.f - pf_13_9) + ((pf_4_7 + pf_2_65) + utof(u_0_phi_71)))) + pf_13_9);
	// 157.2441  <=>  ((0.f - {pf_2_69 : -3890.291}) + {(camera_wpos.z) : -3733.0469})
	pf_20_11 = ((0.f - pf_2_69) + (camera_wpos.z));
	// -758.45386  <=>  ((({pf_2_69 : -3890.291} * {(view_proj[0].z) : 0.6697676}) + (({pf_0_11 : 103.1625} * {(view_proj[0].y) : 1.493044E-08}) + ({pf_1_24 : -1039.7002} * {(view_proj[0].x) : -0.7425708}))) + {(view_proj[0].w) : 1075.086})
	pf_4_11 = (((pf_2_69 * (view_proj[0].z)) + ((pf_0_11 * (view_proj[0].y)) + (pf_1_24 * (view_proj[0].x)))) + (view_proj[0].w));
	// 13.44202  <=>  ((({pf_2_69 : -3890.291} * {(view_proj[1].z) : 0.3768303}) + (({pf_0_11 : 103.1625} * {(view_proj[1].y) : 0.8616711}) + ({pf_1_24 : -1039.7002} * {(view_proj[1].x) : 0.339885}))) + {(view_proj[1].w) : 1743.908})
	pf_15_10 = (((pf_2_69 * (view_proj[1].z)) + ((pf_0_11 * (view_proj[1].y)) + (pf_1_24 * (view_proj[1].x)))) + (view_proj[1].w));
	// -540.2473  <=>  ((({pf_2_69 : -3890.291} * {(view_proj[2].z) : -0.6398518}) + (({pf_0_11 : 103.1625} * {(view_proj[2].y) : 0.5074672}) + ({pf_1_24 : -1039.7002} * {(view_proj[2].x) : -0.57711935}))) + {(view_proj[2].w) : -3681.8398})
	pf_5_31 = (((pf_2_69 * (view_proj[2].z)) + ((pf_0_11 * (view_proj[2].y)) + (pf_1_24 * (view_proj[2].x)))) + (view_proj[2].w));
	// 1.00  <=>  ((({pf_2_69 : -3890.291} * {(view_proj[3].z) : 0}) + (({pf_0_11 : 103.1625} * {(view_proj[3].y) : 0}) + ({pf_1_24 : -1039.7002} * {(view_proj[3].x) : 0}))) + {(view_proj[3].w) : 1.00})
	pf_21_8 = (((pf_2_69 * (view_proj[3].z)) + ((pf_0_11 * (view_proj[3].y)) + (pf_1_24 * (view_proj[3].x)))) + (view_proj[3].w));
	// 128  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 128u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_0_34 = ((ftou(vs_cbuf0_21.x) + 128u) - ftou(vs_cbuf0_21.x));
	// 112  <=>  ((ftou({vs_cbuf0_21.x : 1.0935697E-14}) + 112u) - ftou({vs_cbuf0_21.x : 1.0935697E-14}))
	u_6_6 = ((ftou(vs_cbuf0_21.x) + 112u) - ftou(vs_cbuf0_21.x));
	// {uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).y : }
	u_7_26 = uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).y;
	// 540.2473  <=>  ((({pf_21_8 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[7].z) : -1}) + (({pf_15_10 : 13.44202} * {(view_proj[7].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[7].x) : 0})))) + ((0.f * (({pf_21_8 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_5_31 : -540.2473} * {(view_proj[6].z) : -1.000008}) + (({pf_15_10 : 13.44202} * {(view_proj[6].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[6].x) : 0}))))) + ((0.f * (({pf_21_8 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[4].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[4].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_21_8 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[5].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[5].y) : 2.144507}) + ({pf_4_11 : -758.45386} * {(view_proj[5].x) : 0}))))))))
	pf_25_4 = (((pf_21_8 * (view_proj[7].w)) + ((pf_5_31 * (view_proj[7].z)) + ((pf_15_10 * (view_proj[7].y)) + (pf_4_11 * (view_proj[7].x))))) + ((0.f * ((pf_21_8 * (view_proj[6].w)) + ((pf_5_31 * (view_proj[6].z)) + ((pf_15_10 * (view_proj[6].y)) + (pf_4_11 * (view_proj[6].x)))))) + ((0.f * ((pf_21_8 * (view_proj[4].w)) + ((pf_5_31 * (view_proj[4].z)) + ((pf_15_10 * (view_proj[4].y)) + (pf_4_11 * (view_proj[4].x)))))) + (0.f * ((pf_21_8 * (view_proj[5].w)) + ((pf_5_31 * (view_proj[5].z)) + ((pf_15_10 * (view_proj[5].y)) + (pf_4_11 * (view_proj[5].x)))))))));
	// 540.1495  <=>  (((({pf_21_8 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[7].z) : -1}) + (({pf_15_10 : 13.44202} * {(view_proj[7].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[7].x) : 0})))) * 0.5f) + (((({pf_21_8 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_5_31 : -540.2473} * {(view_proj[6].z) : -1.000008}) + (({pf_15_10 : 13.44202} * {(view_proj[6].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[6].x) : 0})))) * 0.5f) + ((0.f * (({pf_21_8 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[4].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[4].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[4].x) : 1.206285}))))) + (0.f * (({pf_21_8 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[5].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[5].y) : 2.144507}) + ({pf_4_11 : -758.45386} * {(view_proj[5].x) : 0}))))))))
	pf_24_9 = ((((pf_21_8 * (view_proj[7].w)) + ((pf_5_31 * (view_proj[7].z)) + ((pf_15_10 * (view_proj[7].y)) + (pf_4_11 * (view_proj[7].x))))) * 0.5f) + ((((pf_21_8 * (view_proj[6].w)) + ((pf_5_31 * (view_proj[6].z)) + ((pf_15_10 * (view_proj[6].y)) + (pf_4_11 * (view_proj[6].x))))) * 0.5f) + ((0.f * ((pf_21_8 * (view_proj[4].w)) + ((pf_5_31 * (view_proj[4].z)) + ((pf_15_10 * (view_proj[4].y)) + (pf_4_11 * (view_proj[4].x)))))) + (0.f * ((pf_21_8 * (view_proj[5].w)) + ((pf_5_31 * (view_proj[5].z)) + ((pf_15_10 * (view_proj[5].y)) + (pf_4_11 * (view_proj[5].x)))))))));
	// 0.0010738  <=>  inversesqrt((({pf_20_11 : 157.2441} * {pf_20_11 : 157.2441}) + (({pf_18_8 : 262.5748} * {pf_18_8 : 262.5748}) + ({pf_17_10 : -879.562} * {pf_17_10 : -879.562}))))
	f_4_78 = inversesqrt(((pf_20_11 * pf_20_11) + ((pf_18_8 * pf_18_8) + (pf_17_10 * pf_17_10))));
	// -0.028634146  <=>  log2(((0.f - clamp(((((1.0f / ((({pf_24_9 : 540.1495} * (1.0f / {pf_25_4 : 540.2473})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_22.x) : 0.0000333}) + (0.f - {(vs_cbuf15_22.y) : -0.0016638935})), 0.0, 1.0)) + 1.f))
	f_4_84 = log2(((0.f - clamp(((((1.0f / (((pf_24_9 * (1.0f / pf_25_4)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_22.x)) + (0.f - (vs_cbuf15_22.y))), 0.0, 1.0)) + 1.f));
	// 0.0111901  <=>  ((((({pf_20_11 : 157.2441} * {f_4_78 : 0.0010738}) * {(lightDir.z) : -0.08728968}) + ((({pf_18_8 : 262.5748} * {f_4_78 : 0.0010738}) * {(lightDir.y) : -0.4663191}) + (({pf_17_10 : -879.562} * {f_4_78 : 0.0010738}) * {(lightDir.x) : 0.8802994}))) * 0.5f) + 0.5f)
	pf_15_20 = (((((pf_20_11 * f_4_78) * (lightDir.z)) + (((pf_18_8 * f_4_78) * (lightDir.y)) + ((pf_17_10 * f_4_78) * (lightDir.x)))) * 0.5f) + 0.5f);
	// 1.568365  <=>  (({pf_15_20 : 0.0111901} * (({pf_15_20 : 0.0111901} * (({pf_15_20 : 0.0111901} * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f)
	pf_15_21 = ((pf_15_20 * ((pf_15_20 * ((pf_15_20 * -0.0187293f) + 0.074260995f)) + -0.2121144f)) + 1.5707288f);
	// 0.980348  <=>  exp2(({f_4_84 : -0.028634146} * {(vs_cbuf15_23.y) : 1.00}))
	f_6_38 = exp2((f_4_84 * (vs_cbuf15_23.y)));
	// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex0 : tex0}, float2(((({pf_15_21 : 1.568365} * (0.f - sqrt(((0.f - {pf_15_20 : 0.0111901}) + 1.f)))) * 0.63661975f) + 1.f), (({f_6_38 : 0.980348} * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler)
	f4_0_0 = textureLod(tex0, float2((((pf_15_21 * (0.f - sqrt(((0.f - pf_15_20) + 1.f)))) * 0.63661975f) + 1.f), ((f_6_38 * 0.5f) + 0.5f)), 0.0, s_linear_clamp_sampler);
	// 982302225  <=>  {ftou(f_4_78) : 982302225}
	u_13_16 = ftou(f_4_78);
	u_13_phi_72 = u_13_16;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1149667736  <=>  {ftou(({pf_1_24 : -1039.7002} + (0.f - {(vs_cbuf15_52.x) : -2116}))) : 1149667736}
		u_13_17 = ftou((pf_1_24 + (0.f - (vs_cbuf15_52.x))));
		u_13_phi_72 = u_13_17;
	}
	// 3210008290  <=>  {ftou((({pf_17_10 : -879.562} * {f_4_78 : 0.0010738}) * {(lightDir.x) : 0.8802994})) : 3210008290}
	u_9_26 = ftou(((pf_17_10 * f_4_78) * (lightDir.x)));
	u_9_phi_73 = u_9_26;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 1109841408  <=>  {ftou(({pf_2_69 : -3890.291} + (0.f - {(vs_cbuf15_52.y) : -3932}))) : 1109841408}
		u_9_27 = ftou((pf_2_69 + (0.f - (vs_cbuf15_52.y))));
		u_9_phi_73 = u_9_27;
	}
	// 1110514999  <=>  {ftou(pf_4_3) : 1110514999}
	u_10_15 = ftou(pf_4_3);
	u_10_phi_74 = u_10_15;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 909387388  <=>  {ftou(({utof(u_13_phi_72) : 0.0010738} * {(vs_cbuf15_52.z) : 0.0025})) : 909387388}
		u_10_16 = ftou((utof(u_13_phi_72) * (vs_cbuf15_52.z)));
		u_10_phi_74 = u_10_16;
	}
	// 1065259083  <=>  {ftou(sqrt(((0.f - {pf_15_20 : 0.0111901}) + 1.f))) : 1065259083}
	u_2_4 = ftou(sqrt(((0.f - pf_15_20) + 1.f)));
	u_2_phi_75 = u_2_4;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 3137877915  <=>  {ftou(({utof(u_9_phi_73) : -0.83140385} * {(vs_cbuf15_52.z) : 0.0025})) : 3137877915}
		u_2_5 = ftou((utof(u_9_phi_73) * (vs_cbuf15_52.z)));
		u_2_phi_75 = u_2_5;
	}
	// 1065259083  <=>  {u_2_phi_75 : 1065259083}
	u_9_29 = u_2_phi_75;
	u_9_phi_76 = u_9_29;
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// float4(0.50,0.50,0.50,1.00)  <=>  textureLod({tex1 : tex1}, float2({utof(u_10_phi_74) : 44.27853}, {utof(u_2_phi_75) : 0.9943892}), 0.0, s_linear_clamp_sampler)
		f4_0_1 = textureLod(tex1, float2(utof(u_10_phi_74), utof(u_2_phi_75)), 0.0, s_linear_clamp_sampler);
		// 1065353216  <=>  {ftou(f4_0_1.w) : 1065353216}
		u_9_30 = ftou(f4_0_1.w);
		u_9_phi_76 = u_9_30;
	}
	// 1.00  <=>  1.f
	o.fs_attr7.x = 1.f;
	// 879.562  <=>  ({pf_1_24 : -1039.7002} + (0.f - {(camera_wpos.x) : -1919.2622}))
	pf_1_25 = (pf_1_24 + (0.f - (camera_wpos.x)));
	// -914.9115  <=>  (({pf_21_8 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[4].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[4].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[4].x) : 1.206285}))))
	o.vertex.x = ((pf_21_8 * (view_proj[4].w)) + ((pf_5_31 * (view_proj[4].z)) + ((pf_15_10 * (view_proj[4].y)) + (pf_4_11 * (view_proj[4].x)))));
	// 28.8265  <=>  (({pf_21_8 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[5].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[5].y) : 2.144507}) + ({pf_4_11 : -758.45386} * {(view_proj[5].x) : 0}))))
	o.vertex.y = ((pf_21_8 * (view_proj[5].w)) + ((pf_5_31 * (view_proj[5].z)) + ((pf_15_10 * (view_proj[5].y)) + (pf_4_11 * (view_proj[5].x)))));
	// 540.0516  <=>  (({pf_21_8 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_5_31 : -540.2473} * {(view_proj[6].z) : -1.000008}) + (({pf_15_10 : 13.44202} * {(view_proj[6].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[6].x) : 0}))))
	o.vertex.z = ((pf_21_8 * (view_proj[6].w)) + ((pf_5_31 * (view_proj[6].z)) + ((pf_15_10 * (view_proj[6].y)) + (pf_4_11 * (view_proj[6].x)))));
	// 540.2473  <=>  (({pf_21_8 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[7].z) : -1}) + (({pf_15_10 : 13.44202} * {(view_proj[7].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[7].x) : 0}))))
	o.vertex.w = ((pf_21_8 * (view_proj[7].w)) + ((pf_5_31 * (view_proj[7].z)) + ((pf_15_10 * (view_proj[7].y)) + (pf_4_11 * (view_proj[7].x)))));
	// 0  <=>  {ftou(v.offset.y) : 0}
	u_13_19 = ftou(v.offset.y);
	u_13_phi_77 = u_13_19;
	// True  <=>  if(((! (((({v.vertex.z : 0.001} == 0.f) && (! myIsNaN({v.vertex.z : 0.001}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : 0} == 0.f) && (! myIsNaN({v.vertex.x : 0}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : 0} == 0.f) && (! myIsNaN({v.vertex.y : 0}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.y : 0} * {(vs_cbuf13_0.x) : 0}) + {v.offset.y : 0})) : 0}
		u_13_20 = ftou(((v.vertex.y * (vs_cbuf13_0.x)) + v.offset.y));
		u_13_phi_77 = u_13_20;
	}
	// 540.1495  <=>  {pf_24_9 : 540.1495}
	o.fs_attr4.z = pf_24_9;
	// 0  <=>  {ftou(v.offset.x) : 0}
	u_2_7 = ftou(v.offset.x);
	u_2_phi_78 = u_2_7;
	// True  <=>  if(((! (((({v.vertex.z : 0.001} == 0.f) && (! myIsNaN({v.vertex.z : 0.001}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : 0} == 0.f) && (! myIsNaN({v.vertex.x : 0}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : 0} == 0.f) && (! myIsNaN({v.vertex.y : 0}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 0  <=>  {ftou((({v.vertex.x : 0} * {(vs_cbuf13_0.x) : 0}) + {v.offset.x : 0})) : 0}
		u_2_8 = ftou(((v.vertex.x * (vs_cbuf13_0.x)) + v.offset.x));
		u_2_phi_78 = u_2_8;
	}
	// 540.2473  <=>  {pf_25_4 : 540.2473}
	o.fs_attr4.w = pf_25_4;
	// -262.57483  <=>  ({pf_0_11 : 103.1625} + (0.f - {(camera_wpos.y) : 365.7373}))
	pf_7_34 = (pf_0_11 + (0.f - (camera_wpos.y)));
	// -157.24414  <=>  ({pf_2_69 : -3890.291} + (0.f - {(camera_wpos.z) : -3733.0469}))
	pf_2_70 = (pf_2_69 + (0.f - (camera_wpos.z)));
	// 561.4622  <=>  ((({pf_13_9 : -3889.9858} * {(vs_cbuf8_11.z) : 0.6398518}) + (({pf_6_30 : 62.05549} * {(vs_cbuf8_11.y) : -0.50746715}) + ({pf_7_30 : -1039.4249} * {(vs_cbuf8_11.x) : 0.5771194}))) + {(vs_cbuf8_11.w) : 3681.84})
	pf_15_31 = (((pf_13_9 * (vs_cbuf8_11.z)) + ((pf_6_30 * (vs_cbuf8_11.y)) + (pf_7_30 * (vs_cbuf8_11.x)))) + (vs_cbuf8_11.w));
	// 561.2664  <=>  ((({pf_13_9 : -3889.9858} * {(vs_cbuf8_10.z) : 0.6398569}) + (({pf_6_30 : 62.05549} * {(vs_cbuf8_10.y) : -0.5074712}) + ({pf_7_30 : -1039.4249} * {(vs_cbuf8_10.x) : 0.5771239}))) + {(vs_cbuf8_10.w) : 3681.669})
	pf_6_33 = (((pf_13_9 * (vs_cbuf8_10.z)) + ((pf_6_30 * (vs_cbuf8_10.y)) + (pf_7_30 * (vs_cbuf8_10.x)))) + (vs_cbuf8_10.w));
	// 1065353216  <=>  {ftou(v.offset.z) : 1065353216}
	u_10_19 = ftou(v.offset.z);
	u_10_phi_79 = u_10_19;
	// True  <=>  if(((! (((({v.vertex.z : 0.001} == 0.f) && (! myIsNaN({v.vertex.z : 0.001}))) && (! myIsNaN(0.f))) && (((({v.vertex.x : 0} == 0.f) && (! myIsNaN({v.vertex.x : 0}))) && (! myIsNaN(0.f))) && ((({v.vertex.y : 0} == 0.f) && (! myIsNaN({v.vertex.y : 0}))) && (! myIsNaN(0.f)))))) ? true : false))
	if(((! ((((v.vertex.z == 0.f) && (! myIsNaN(v.vertex.z))) && (! myIsNaN(0.f))) && ((((v.vertex.x == 0.f) && (! myIsNaN(v.vertex.x))) && (! myIsNaN(0.f))) && (((v.vertex.y == 0.f) && (! myIsNaN(v.vertex.y))) && (! myIsNaN(0.f)))))) ? true : false))
	{
		// 1065353216  <=>  {ftou((({v.vertex.z : 0.001} * {(vs_cbuf13_0.x) : 0}) + {v.offset.z : 1.00})) : 1065353216}
		u_10_20 = ftou(((v.vertex.z * (vs_cbuf13_0.x)) + v.offset.z));
		u_10_phi_79 = u_10_20;
	}
	// 0.0010738  <=>  inversesqrt((({pf_2_70 : -157.24414} * {pf_2_70 : -157.24414}) + (({pf_7_34 : -262.57483} * {pf_7_34 : -262.57483}) + ({pf_1_25 : 879.562} * {pf_1_25 : 879.562}))))
	f_0_34 = inversesqrt(((pf_2_70 * pf_2_70) + ((pf_7_34 * pf_7_34) + (pf_1_25 * pf_1_25))));
	// -0.66976756  <=>  (((({pf_10_14 : 0} * {pf_12_9 : 0}) + (0.f - ({pf_9_14 : 1.00} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536})))) * {utof(u_10_phi_79) : 1.00}) + (({pf_8_12 : -0.74257076} * {utof(u_2_phi_78) : 0}) + ({pf_11_17 : 0} * {utof(u_13_phi_77) : 0})))
	o.fs_attr6.x = ((((pf_10_14 * pf_12_9) + (0.f - (pf_9_14 * (pf_16_7 * f_3_55)))) * utof(u_10_phi_79)) + ((pf_8_12 * utof(u_2_phi_78)) + (pf_11_17 * utof(u_13_phi_77))));
	// 0  <=>  (((({pf_11_17 : 0} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536})) + (0.f - ({pf_10_14 : 0} * {pf_8_12 : -0.74257076}))) * {utof(u_10_phi_79) : 1.00}) + (({pf_12_9 : 0} * {utof(u_2_phi_78) : 0}) + ({pf_9_14 : 1.00} * {utof(u_13_phi_77) : 0})))
	o.fs_attr6.y = ((((pf_11_17 * (pf_16_7 * f_3_55)) + (0.f - (pf_10_14 * pf_8_12))) * utof(u_10_phi_79)) + ((pf_12_9 * utof(u_2_phi_78)) + (pf_9_14 * utof(u_13_phi_77))));
	// -0.74257076  <=>  (((({pf_9_14 : 1.00} * {pf_8_12 : -0.74257076}) + (0.f - ({pf_11_17 : 0} * {pf_12_9 : 0}))) * {utof(u_10_phi_79) : 1.00}) + ((({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536}) * {utof(u_2_phi_78) : 0}) + ({pf_10_14 : 0} * {utof(u_13_phi_77) : 0})))
	o.fs_attr6.z = ((((pf_9_14 * pf_8_12) + (0.f - (pf_11_17 * pf_12_9))) * utof(u_10_phi_79)) + (((pf_16_7 * f_3_55) * utof(u_2_phi_78)) + (pf_10_14 * utof(u_13_phi_77))));
	// 0.3839826  <=>  clamp(((abs((((((({pf_9_14 : 1.00} * {pf_8_12 : -0.74257076}) + (0.f - ({pf_11_17 : 0} * {pf_12_9 : 0}))) * {utof(u_10_phi_79) : 1.00}) + ((({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536}) * {utof(u_2_phi_78) : 0}) + ({pf_10_14 : 0} * {utof(u_13_phi_77) : 0}))) * ({pf_2_70 : -157.24414} * {f_0_34 : 0.0010738})) + (((((({pf_11_17 : 0} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536})) + (0.f - ({pf_10_14 : 0} * {pf_8_12 : -0.74257076}))) * {utof(u_10_phi_79) : 1.00}) + (({pf_12_9 : 0} * {utof(u_2_phi_78) : 0}) + ({pf_9_14 : 1.00} * {utof(u_13_phi_77) : 0}))) * ({pf_7_34 : -262.57483} * {f_0_34 : 0.0010738})) + ((((({pf_10_14 : 0} * {pf_12_9 : 0}) + (0.f - ({pf_9_14 : 1.00} * ({pf_16_7 : 0.5771194} * {f_3_55 : 1.160536})))) * {utof(u_10_phi_79) : 1.00}) + (({pf_8_12 : -0.74257076} * {utof(u_2_phi_78) : 0}) + ({pf_11_17 : 0} * {utof(u_13_phi_77) : 0}))) * ({pf_1_25 : 879.562} * {f_0_34 : 0.0010738}))))) + (0.f - {utof(vs_cbuf9[137].z) : 0.20})) * (1.0f / ((0.f - {utof(vs_cbuf9[137].z) : 0.20}) + {utof(vs_cbuf9[137].w) : 1.00}))), 0.0, 1.0)
	f_0_40 = clamp(((abs(((((((pf_9_14 * pf_8_12) + (0.f - (pf_11_17 * pf_12_9))) * utof(u_10_phi_79)) + (((pf_16_7 * f_3_55) * utof(u_2_phi_78)) + (pf_10_14 * utof(u_13_phi_77)))) * (pf_2_70 * f_0_34)) + ((((((pf_11_17 * (pf_16_7 * f_3_55)) + (0.f - (pf_10_14 * pf_8_12))) * utof(u_10_phi_79)) + ((pf_12_9 * utof(u_2_phi_78)) + (pf_9_14 * utof(u_13_phi_77)))) * (pf_7_34 * f_0_34)) + (((((pf_10_14 * pf_12_9) + (0.f - (pf_9_14 * (pf_16_7 * f_3_55)))) * utof(u_10_phi_79)) + ((pf_8_12 * utof(u_2_phi_78)) + (pf_11_17 * utof(u_13_phi_77)))) * (pf_1_25 * f_0_34))))) + (0.f - utof(vs_cbuf9[137].z))) * (1.0f / ((0.f - utof(vs_cbuf9[137].z)) + utof(vs_cbuf9[137].w)))), 0.0, 1.0);
	// -∞  <=>  log2(((0.f - clamp(((((1.0f / ((({pf_24_9 : 540.1495} * (1.0f / {pf_25_4 : 540.2473})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {(vs_cbuf15_24.x) : 0.002381}) + (0.f - {(vs_cbuf15_24.y) : -0.04761905})), 0.0, 1.0)) + 1.f))
	f_9_24 = log2(((0.f - clamp(((((1.0f / (((pf_24_9 * (1.0f / pf_25_4)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) * (vs_cbuf15_24.x)) + (0.f - (vs_cbuf15_24.y))), 0.0, 1.0)) + 1.f));
	// 1.00  <=>  clamp(((((1.0f / ((((({pf_6_33 : 561.2664} * 0.5f) + ({pf_15_31 : 561.4622} * 0.5f)) * (1.0f / ((0.f * {pf_6_33 : 561.2664}) + {pf_15_31 : 561.4622}))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].x) : 100.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[138].x) : 100.00}) + {utof(vs_cbuf9[138].y) : 200.00}))), 0.0, 1.0)
	f_4_85 = clamp(((((1.0f / (((((pf_6_33 * 0.5f) + (pf_15_31 * 0.5f)) * (1.0f / ((0.f * pf_6_33) + pf_15_31))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].x))) * (1.0f / ((0.f - utof(vs_cbuf9[138].x)) + utof(vs_cbuf9[138].y)))), 0.0, 1.0);
	// ((0.f - {utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).x) : })
	pf_3_12 = ((0.f - utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).x));
	// 255.7104  <=>  (((({pf_21_8 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[7].z) : -1}) + (({pf_15_10 : 13.44202} * {(view_proj[7].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[7].x) : 0})))) * 0.5f) + ((0.f * (({pf_21_8 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_5_31 : -540.2473} * {(view_proj[6].z) : -1.000008}) + (({pf_15_10 : 13.44202} * {(view_proj[6].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[6].x) : 0}))))) + ((0.f * (({pf_21_8 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[4].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[4].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[4].x) : 1.206285}))))) + ((({pf_21_8 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[5].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[5].y) : 2.144507}) + ({pf_4_11 : -758.45386} * {(view_proj[5].x) : 0})))) * -0.5f))))
	pf_2_75 = ((((pf_21_8 * (view_proj[7].w)) + ((pf_5_31 * (view_proj[7].z)) + ((pf_15_10 * (view_proj[7].y)) + (pf_4_11 * (view_proj[7].x))))) * 0.5f) + ((0.f * ((pf_21_8 * (view_proj[6].w)) + ((pf_5_31 * (view_proj[6].z)) + ((pf_15_10 * (view_proj[6].y)) + (pf_4_11 * (view_proj[6].x)))))) + ((0.f * ((pf_21_8 * (view_proj[4].w)) + ((pf_5_31 * (view_proj[4].z)) + ((pf_15_10 * (view_proj[4].y)) + (pf_4_11 * (view_proj[4].x)))))) + (((pf_21_8 * (view_proj[5].w)) + ((pf_5_31 * (view_proj[5].z)) + ((pf_15_10 * (view_proj[5].y)) + (pf_4_11 * (view_proj[5].x))))) * -0.5f))));
	// 255.7104  <=>  {pf_2_75 : 255.7104}
	o.fs_attr4.y = pf_2_75;
	// ((0.f - {utof(u_7_26) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).y) : })
	pf_7_38 = ((0.f - utof(u_7_26)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).y));
	// 0.3290971  <=>  (({f_0_40 : 0.3839826} * {f_0_40 : 0.3839826}) * (({f_0_40 : 0.3839826} * -2.f) + 3.f))
	pf_5_35 = ((f_0_40 * f_0_40) * ((f_0_40 * -2.f) + 3.f));
	// ((0.f - {utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z) : }) + {utof(uint4({vs_ssbo_color2.x * vs_ssbo_scale : }, {vs_ssbo_color2.y * vs_ssbo_scale : }, {vs_ssbo_color2.z * vs_ssbo_scale : }, {vs_ssbo_color2.w * vs_ssbo_scale : }).z) : })
	pf_2_79 = ((0.f - utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z)) + utof(uint4(vs_ssbo_color2.x * vs_ssbo_scale, vs_ssbo_color2.y * vs_ssbo_scale, vs_ssbo_color2.z * vs_ssbo_scale, vs_ssbo_color2.w * vs_ssbo_scale).z));
	// ({utof(u_7_26) : } + ((clamp(((({pf_2_75 : 255.7104} * (1.0f / {pf_25_4 : 540.2473})) * -0.7f) + 0.85f), 0.0, 1.0) * (0.f - {pf_7_38 : })) + {pf_7_38 : }))
	pf_6_39 = (utof(u_7_26) + ((clamp((((pf_2_75 * (1.0f / pf_25_4)) * -0.7f) + 0.85f), 0.0, 1.0) * (0.f - pf_7_38)) + pf_7_38));
	// ({utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).x) : } + ((clamp(((({pf_2_75 : 255.7104} * (1.0f / {pf_25_4 : 540.2473})) * -0.7f) + 0.85f), 0.0, 1.0) * (0.f - {pf_3_12 : })) + {pf_3_12 : }))
	pf_3_14 = (utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).x) + ((clamp((((pf_2_75 * (1.0f / pf_25_4)) * -0.7f) + 0.85f), 0.0, 1.0) * (0.f - pf_3_12)) + pf_3_12));
	// 0  <=>  clamp(((((1.0f / ((((({pf_6_33 : 561.2664} * 0.5f) + ({pf_15_31 : 561.4622} * 0.5f)) * (1.0f / ((0.f * {pf_6_33 : 561.2664}) + {pf_15_31 : 561.4622}))) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + (0.f - {utof(vs_cbuf9[138].z) : 800.00})) * (1.0f / ((0.f - {utof(vs_cbuf9[138].z) : 800.00}) + {utof(vs_cbuf9[138].w) : 1000.00}))), 0.0, 1.0)
	f_0_45 = clamp(((((1.0f / (((((pf_6_33 * 0.5f) + (pf_15_31 * 0.5f)) * (1.0f / ((0.f * pf_6_33) + pf_15_31))) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (0.f - utof(vs_cbuf9[138].z))) * (1.0f / ((0.f - utof(vs_cbuf9[138].z)) + utof(vs_cbuf9[138].w)))), 0.0, 1.0);
	// ({utof(uint4({vs_ssbo_color1.x * vs_ssbo_scale : }, {vs_ssbo_color1.y * vs_ssbo_scale : }, {vs_ssbo_color1.z * vs_ssbo_scale : }, {vs_ssbo_color1.w * vs_ssbo_scale : }).z) : } + ((clamp(((({pf_2_75 : 255.7104} * (1.0f / {pf_25_4 : 540.2473})) * -0.7f) + 0.85f), 0.0, 1.0) * (0.f - {pf_2_79 : })) + {pf_2_79 : }))
	pf_2_81 = (utof(uint4(vs_ssbo_color1.x * vs_ssbo_scale, vs_ssbo_color1.y * vs_ssbo_scale, vs_ssbo_color1.z * vs_ssbo_scale, vs_ssbo_color1.w * vs_ssbo_scale).z) + ((clamp((((pf_2_75 * (1.0f / pf_25_4)) * -0.7f) + 0.85f), 0.0, 1.0) * (0.f - pf_2_79)) + pf_2_79));
	// 0.6723652  <=>  exp2(({f_4_84 : -0.028634146} * {(vs_cbuf15_23.x) : 20.00}))
	f_1_71 = exp2((f_4_84 * (vs_cbuf15_23.x)));
	// 0  <=>  exp2(({f_9_24 : -∞} * {(vs_cbuf15_24.w) : 4.00}))
	f_2_19 = exp2((f_9_24 * (vs_cbuf15_24.w)));
	// (({pf_3_14 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.x) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_3_14 : })
	o.fs_attr9.x = ((pf_3_14 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.x)) + (0.f - (vs_cbuf15_58.w)))) + pf_3_14);
	// (({pf_6_39 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.y) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_6_39 : })
	o.fs_attr9.y = ((pf_6_39 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.y)) + (0.f - (vs_cbuf15_58.w)))) + pf_6_39);
	// (({pf_2_81 : } * (({(vs_cbuf15_58.w) : 1.00} * {(vs_cbuf15_58.z) : 1.00}) + (0.f - {(vs_cbuf15_58.w) : 1.00}))) + {pf_2_81 : })
	o.fs_attr9.z = ((pf_2_81 * (((vs_cbuf15_58.w) * (vs_cbuf15_58.z)) + (0.f - (vs_cbuf15_58.w)))) + pf_2_81);
	// -187.33209  <=>  (((({pf_21_8 : 1.00} * {(view_proj[7].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[7].z) : -1}) + (({pf_15_10 : 13.44202} * {(view_proj[7].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[7].x) : 0})))) * 0.5f) + ((0.f * (({pf_21_8 : 1.00} * {(view_proj[6].w) : -0.2000008}) + (({pf_5_31 : -540.2473} * {(view_proj[6].z) : -1.000008}) + (({pf_15_10 : 13.44202} * {(view_proj[6].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[6].x) : 0}))))) + (((({pf_21_8 : 1.00} * {(view_proj[4].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[4].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[4].y) : 0}) + ({pf_4_11 : -758.45386} * {(view_proj[4].x) : 1.206285})))) * 0.5f) + (0.f * (({pf_21_8 : 1.00} * {(view_proj[5].w) : 0}) + (({pf_5_31 : -540.2473} * {(view_proj[5].z) : 0}) + (({pf_15_10 : 13.44202} * {(view_proj[5].y) : 2.144507}) + ({pf_4_11 : -758.45386} * {(view_proj[5].x) : 0}))))))))
	pf_1_35 = ((((pf_21_8 * (view_proj[7].w)) + ((pf_5_31 * (view_proj[7].z)) + ((pf_15_10 * (view_proj[7].y)) + (pf_4_11 * (view_proj[7].x))))) * 0.5f) + ((0.f * ((pf_21_8 * (view_proj[6].w)) + ((pf_5_31 * (view_proj[6].z)) + ((pf_15_10 * (view_proj[6].y)) + (pf_4_11 * (view_proj[6].x)))))) + ((((pf_21_8 * (view_proj[4].w)) + ((pf_5_31 * (view_proj[4].z)) + ((pf_15_10 * (view_proj[4].y)) + (pf_4_11 * (view_proj[4].x))))) * 0.5f) + (0.f * ((pf_21_8 * (view_proj[5].w)) + ((pf_5_31 * (view_proj[5].z)) + ((pf_15_10 * (view_proj[5].y)) + (pf_4_11 * (view_proj[5].x)))))))));
	// 0.1811977  <=>  (clamp(((((0.f - {pf_0_11 : 103.1625}) + min({(camera_wpos.y) : 365.7373}, {(vs_cbuf15_27.z) : 250.00})) * {(vs_cbuf15_27.y) : 0.0071429}) + {(vs_cbuf15_27.x) : -0.14285715}), 0.0, 1.0) * {(vs_cbuf15_26.w) : 0.20})
	o.fs_attr8.y = (clamp(((((0.f - pf_0_11) + min((camera_wpos.y), (vs_cbuf15_27.z))) * (vs_cbuf15_27.y)) + (vs_cbuf15_27.x)), 0.0, 1.0) * (vs_cbuf15_26.w));
	// 0.3290971  <=>  ((({pf_5_35 : 0.3290971} * {f_4_85 : 1.00}) * ((0.f - {f_0_45 : 0}) + 1.f)) * {(vs_cbuf10_3.x) : 1.00})
	pf_0_21 = (((pf_5_35 * f_4_85) * ((0.f - f_0_45) + 1.f)) * (vs_cbuf10_3.x));
	// -187.33209  <=>  {pf_1_35 : -187.33209}
	o.fs_attr4.x = pf_1_35;
	// 0.3290971  <=>  {pf_0_21 : 0.3290971}
	o.fs_attr5.x = pf_0_21;
	// 0.2784896  <=>  clamp((({f_1_71 : 0.6723652} * (0.f - {(vs_cbuf15_23.z) : 0.85})) + {(vs_cbuf15_23.z) : 0.85}), 0.0, 1.0)
	o.fs_attr10.w = clamp(((f_1_71 * (0.f - (vs_cbuf15_23.z))) + (vs_cbuf15_23.z)), 0.0, 1.0);
	// 0.7006614  <=>  (({f_2_19 : 0} * (0.f - {(vs_cbuf15_25.w) : 0.7006614})) + {(vs_cbuf15_25.w) : 0.7006614})
	o.fs_attr8.x = ((f_2_19 * (0.f - (vs_cbuf15_25.w))) + (vs_cbuf15_25.w));
	// 0.50  <=>  (clamp(max((((((1.0f / ((({pf_24_9 : 540.1495} * (1.0f / {pf_25_4 : 540.2473})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.x : 0.50})
	o.fs_attr10.x = (clamp(max((((((1.0f / (((pf_24_9 * (1.0f / pf_25_4)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.x);
	// 0.50  <=>  (clamp(max((((((1.0f / ((({pf_24_9 : 540.1495} * (1.0f / {pf_25_4 : 540.2473})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.y : 0.50})
	o.fs_attr10.y = (clamp(max((((((1.0f / (((pf_24_9 * (1.0f / pf_25_4)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.y);
	// 0.50  <=>  (clamp(max((((((1.0f / ((({pf_24_9 : 540.1495} * (1.0f / {pf_25_4 : 540.2473})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) + {(lightDir.y) : -0.4663191}) * (1.0f / clamp((({(lightDir.y) : -0.4663191} * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * {f4_0_0.z : 0.50})
	o.fs_attr10.z = (clamp(max((((((1.0f / (((pf_24_9 * (1.0f / pf_25_4)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) + (lightDir.y)) * (1.0f / clamp((((lightDir.y) * 1.5f) + 1.5f), 0.0, 1.0))) * 0.06666667f), 0.2f), 0.0, 1.0) * f4_0_0.z);
	// False  <=>  if(((((0.f != {(vs_cbuf15_49.x) : 0}) || myIsNaN(0.f)) || myIsNaN({(vs_cbuf15_49.x) : 0})) ? true : false))
	if(((((0.f != (vs_cbuf15_49.x)) || myIsNaN(0.f)) || myIsNaN((vs_cbuf15_49.x))) ? true : false))
	{
		// 0.0010526  <=>  (1.0f / {(vs_cbuf15_51.x) : 950.00})
		f_0_50 = (1.0f / (vs_cbuf15_51.x));
		// 1.00  <=>  ((({utof(u_9_phi_76) : 0.9943892} * {(vs_cbuf15_49.x) : 0}) + (0.f - {(vs_cbuf15_49.x) : 0})) + 1.f)
		pf_0_25 = (((utof(u_9_phi_76) * (vs_cbuf15_49.x)) + (0.f - (vs_cbuf15_49.x))) + 1.f);
		// 0.515983  <=>  clamp(((((1.0f / ((({pf_24_9 : 540.1495} * (1.0f / {pf_25_4 : 540.2473})) * {(vs_cbuf8_30.w) : 24999.90}) + (0.f - {(vs_cbuf8_30.y) : 25000.00}))) * (0.f - {(vs_cbuf8_30.z) : 2500.00})) * {f_0_50 : 0.0010526}) + (0.f - ({f_0_50 : 0.0010526} * {(vs_cbuf15_51.y) : 50.00}))), 0.0, 1.0)
		f_0_51 = clamp(((((1.0f / (((pf_24_9 * (1.0f / pf_25_4)) * (vs_cbuf8_30.w)) + (0.f - (vs_cbuf8_30.y)))) * (0.f - (vs_cbuf8_30.z))) * f_0_50) + (0.f - (f_0_50 * (vs_cbuf15_51.y)))), 0.0, 1.0);
		// -∞  <=>  log2(abs((({pf_0_25 : 1.00} * (0.f - {f_0_51 : 0.515983})) + {f_0_51 : 0.515983})))
		f_0_53 = log2(abs(((pf_0_25 * (0.f - f_0_51)) + f_0_51)));
		// 0  <=>  exp2(({f_0_53 : -∞} * {(vs_cbuf15_51.z) : 1.50}))
		f_0_54 = exp2((f_0_53 * (vs_cbuf15_51.z)));
		// 1.00  <=>  (({pf_0_25 : 1.00} * (0.f - (({f_0_54 : 0} * {(vs_cbuf15_51.w) : 1.00}) * {(vs_cbuf15_49.x) : 0}))) + {pf_0_25 : 1.00})
		o.fs_attr7.x = ((pf_0_25 * (0.f - ((f_0_54 * (vs_cbuf15_51.w)) * (vs_cbuf15_49.x)))) + pf_0_25);
	}
	// 0  <=>  0.f
	o.fs_attr8.w = 0.f;
	// True  <=>  if(((! ((((((0.f - {f_0_45 : 0}) + 1.f) <= 0.f) && (! myIsNaN(((0.f - {f_0_45 : 0}) + 1.f)))) && (! myIsNaN(0.f))) || ((({f_4_85 : 1.00} <= 0.f) && (! myIsNaN({f_4_85 : 1.00}))) && (! myIsNaN(0.f))))) ? true : false))
	if(((! ((((((0.f - f_0_45) + 1.f) <= 0.f) && (! myIsNaN(((0.f - f_0_45) + 1.f)))) && (! myIsNaN(0.f))) || (((f_4_85 <= 0.f) && (! myIsNaN(f_4_85))) && (! myIsNaN(0.f))))) ? true : false))
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
	o.fs_attr5.x = 0.f;
	return;
}
