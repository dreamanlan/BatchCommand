vs
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 512;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;
        remove_duplicate_expression;

        add_utof(1065353216u);

        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt", 9);
            split_on("texture", 3);
            split_on("textureLod", 3);
            split_on("texelFetch", 3);
            split_on("texelQueryLod", 3);
            split_on("texelSize", 3);
            split_on("log2", 12);
        };

        split_object_assignment{
            set(out_attr0.x, 64, 20480, true, true);
            out_attr0.y,
            out_attr0.z,
            out_attr0.w,
            out_attr1.x,
            out_attr1.y,
            out_attr1.z,
            out_attr1.w,
            out_attr2.x,
            out_attr2.y,
            out_attr2.z,
            out_attr2.w,
            out_attr3.x,
            out_attr3.y,
            out_attr3.z,
            out_attr3.w,
            out_attr4.x,
            out_attr4.y,
            out_attr4.z,
            out_attr4.w,
            out_attr5.x,
            out_attr5.y,
            out_attr5.z,
            out_attr5.w,
            out_attr6.x,
            out_attr6.y,
            out_attr6.z,
            out_attr6.w,
            out_attr7.x,
            out_attr7.y,
            out_attr7.z,
            out_attr7.w,
            out_attr8.x,
            out_attr8.y,
            out_attr8.z,
            out_attr8.w,
            out_attr9.x,
            out_attr9.y,
            out_attr9.z,
            out_attr9.w,
            out_attr10.x,
            out_attr10.y,
            out_attr10.z,
            out_attr10.w,
            out_attr11.x,
            out_attr11.y,
            out_attr11.z,
            out_attr11.w,
            out_attr12.x,
            out_attr12.y,
            out_attr12.z,
            out_attr12.w,
            gl_Position.x,
            gl_Position.y,
            gl_Position.z,
            gl_Position.w
        };

        split_variable_assignment{
            b_0_2,
            b_0_20,
            b_0_21,
            b_0_22,
            b_0_23,
            b_0_7,
            b_1_10,
            b_1_11,
            b_1_12,
            b_1_13,
            b_1_14,
            b_1_25,
            b_1_33,
            b_1_8,
            b_1_9,
            b_2_35,
            b_2_36,
            b_2_40,
            b_2_41,
            b_2_43,
            b_2_44,
            b_2_8,
            b_3_12,
            b_3_20,
            b_4_0,
            b_4_12,
            b_5_22,
            b_6_1,
            b_6_18,
            b_7_6,
            b_7_7,
            b_8_8,
            f_0_21,
            f_0_24,
            f_0_26,
            f_0_70,
            f_0_73,
            f_0_78,
            f_0_85,
            f_0_87,
            f_0_88,
            f_1_15,
            f_1_20,
            f_1_5,
            f_10_9,
            f_11_7,
            f_12_1,
            f_12_2,
            f_14_1,
            f_17_0,
            f_19_0,
            f_19_1,
            f_2_16,
            f_2_25,
            f_2_39,
            f_2_43,
            f_2_47,
            f_2_63,
            f_2_64,
            f_2_65,
            f_2_71,
            f_2_74,
            f_2_95,
            f_21_16,
            f_22_0,
            f_22_2,
            f_22_8,
            f_23_14,
            f_23_16,
            f_3_28,
            f_4_19,
            f_6_14,
            f_6_16,
            f_6_21,
            f_6_28,
            f_7_10,
            f_7_26,
            f_7_48,
            f_7_5,
            f_7_76,
            f_8_10,
            f_8_30,
            f_8_36,
            f_8_4,
            f_8_46,
            f_9_25,
            f_9_37,
            f4_0_0,
            f4_0_1,
            pf_0_1,
            pf_0_10,
            pf_0_13,
            pf_0_18,
            pf_0_19,
            pf_0_3,
            pf_0_4,
            pf_1_18,
            pf_1_27,
            pf_1_28,
            pf_1_3,
            pf_1_4,
            pf_1_5,
            pf_1_7,
            pf_10_1,
            pf_10_12,
            pf_10_18,
            pf_10_22,
            pf_10_23,
            pf_10_26,
            pf_10_3,
            pf_10_4,
            pf_10_5,
            pf_10_7,
            pf_11_2,
            pf_11_3,
            pf_11_5,
            pf_11_7,
            pf_11_9,
            pf_12_8,
            pf_13_2,
            pf_13_6,
            pf_14_11,
            pf_14_3,
            pf_14_7,
            pf_15_13,
            pf_15_4,
            pf_15_6,
            pf_15_9,
            pf_16_0,
            pf_16_1,
            pf_16_6,
            pf_17_2,
            pf_17_9,
            pf_18_1,
            pf_2_12,
            pf_2_13,
            pf_2_18,
            pf_2_21,
            pf_2_22,
            pf_2_28,
            pf_2_3,
            pf_2_5,
            pf_21_1,
            pf_22_0,
            pf_23_0,
            pf_26_0,
            pf_26_3,
            pf_29_0,
            pf_3_12,
            pf_3_17,
            pf_3_5,
            pf_3_7,
            pf_31_0,
            pf_4_10,
            pf_4_14,
            pf_4_16,
            pf_4_19,
            pf_4_20,
            pf_4_6,
            pf_4_9,
            pf_5_14,
            pf_5_16,
            pf_5_24,
            pf_5_30,
            pf_5_31,
            pf_5_33,
            pf_5_42,
            pf_6_10,
            pf_6_11,
            pf_6_12,
            pf_6_16,
            pf_6_19,
            pf_6_20,
            pf_6_24,
            pf_6_25,
            pf_6_26,
            pf_6_27,
            pf_6_33,
            pf_6_39,
            pf_6_40,
            pf_6_42,
            pf_6_6,
            pf_7_11,
            pf_7_14,
            pf_7_8,
            pf_8_11,
            pf_8_14,
            pf_8_16,
            pf_8_4,
            pf_8_7,
            pf_9_1,
            pf_9_11,
            pf_9_13,
            pf_9_15,
            pf_9_3,
            pf_9_9,
            u_0_1,
            u_0_10,
            u_0_11,
            u_0_2,
            u_0_3,
            u_0_4,
            u_0_5,
            u_0_7,
            u_0_8,
            u_0_phi_28,
            u_0_phi_44,
            u_0_phi_67,
            u_0_phi_70,
            u_1_1,
            u_1_10,
            u_1_11,
            u_1_12,
            u_1_13,
            u_1_14,
            u_1_15,
            u_1_16,
            u_1_18,
            u_1_19,
            u_1_2,
            u_1_21,
            u_1_22,
            u_1_25,
            u_1_3,
            u_1_32,
            u_1_34,
            u_1_39,
            u_1_4,
            u_1_45,
            u_1_48,
            u_1_49,
            u_1_5,
            u_1_51,
            u_1_54,
            u_1_56,
            u_1_6,
            u_1_61,
            u_1_63,
            u_1_7,
            u_1_8,
            u_1_9,
            u_1_phi_23,
            u_1_phi_25,
            u_1_phi_29,
            u_1_phi_32,
            u_1_phi_34,
            u_1_phi_37,
            u_1_phi_40,
            u_1_phi_42,
            u_1_phi_65,
            u_1_phi_68,
            u_10_0,
            u_10_10,
            u_10_12,
            u_10_16,
            u_10_2,
            u_10_22,
            u_10_23,
            u_10_27,
            u_10_28,
            u_10_3,
            u_10_33,
            u_10_4,
            u_10_5,
            u_10_9,
            u_10_phi_47,
            u_10_phi_56,
            u_10_phi_82,
            u_11_1,
            u_11_10,
            u_11_13,
            u_11_17,
            u_11_2,
            u_11_20,
            u_11_5,
            u_11_6,
            u_11_7,
            u_11_8,
            u_11_9,
            u_11_phi_26,
            u_11_phi_39,
            u_11_phi_48,
            u_11_phi_57,
            u_12_0,
            u_12_1,
            u_12_10,
            u_12_11,
            u_12_2,
            u_12_21,
            u_12_3,
            u_12_4,
            u_12_5,
            u_12_6,
            u_12_7,
            u_12_phi_17,
            u_12_phi_49,
            u_12_phi_58,
            u_13_0,
            u_13_11,
            u_13_13,
            u_13_15,
            u_13_16,
            u_13_2,
            u_13_21,
            u_13_3,
            u_13_4,
            u_13_5,
            u_13_6,
            u_13_7,
            u_13_phi_19,
            u_13_phi_24,
            u_13_phi_50,
            u_14_1,
            u_14_12,
            u_14_2,
            u_14_3,
            u_14_4,
            u_14_5,
            u_14_6,
            u_14_9,
            u_14_phi_16,
            u_14_phi_50,
            u_15_1,
            u_15_12,
            u_15_15,
            u_15_17,
            u_15_2,
            u_15_20,
            u_15_22,
            u_15_25,
            u_15_3,
            u_15_31,
            u_15_32,
            u_15_4,
            u_15_6,
            u_15_7,
            u_15_8,
            u_15_phi_21,
            u_15_phi_50,
            u_16_0,
            u_16_1,
            u_16_2,
            u_16_3,
            u_16_4,
            u_16_6,
            u_16_phi_15,
            u_16_phi_50,
            u_17_11,
            u_17_13,
            u_17_16,
            u_17_19,
            u_17_20,
            u_17_25,
            u_17_27,
            u_17_28,
            u_17_32,
            u_17_33,
            u_17_34,
            u_17_36,
            u_17_4,
            u_17_40,
            u_17_41,
            u_17_45,
            u_17_46,
            u_17_48,
            u_17_5,
            u_17_51,
            u_17_52,
            u_17_56,
            u_17_57,
            u_17_59,
            u_17_6,
            u_17_63,
            u_17_65,
            u_17_7,
            u_17_70,
            u_17_71,
            u_17_8,
            u_17_9,
            u_17_phi_20,
            u_17_phi_52,
            u_17_phi_59,
            u_18_10,
            u_18_11,
            u_18_12,
            u_18_14,
            u_18_16,
            u_18_6,
            u_18_7,
            u_18_8,
            u_18_9,
            u_18_phi_28,
            u_18_phi_53,
            u_18_phi_60,
            u_19_1,
            u_19_10,
            u_19_11,
            u_19_2,
            u_19_3,
            u_19_4,
            u_19_5,
            u_19_7,
            u_19_phi_54,
            u_19_phi_60,
            u_2_10,
            u_2_12,
            u_2_16,
            u_2_19,
            u_2_20,
            u_2_24,
            u_2_27,
            u_2_30,
            u_2_31,
            u_2_32,
            u_2_34,
            u_2_37,
            u_2_39,
            u_2_40,
            u_2_43,
            u_2_44,
            u_2_46,
            u_2_5,
            u_2_6,
            u_20_1,
            u_20_13,
            u_20_15,
            u_20_18,
            u_20_2,
            u_20_21,
            u_20_22,
            u_20_26,
            u_20_27,
            u_20_34,
            u_20_35,
            u_20_6,
            u_20_8,
            u_20_phi_60,
            u_21_1,
            u_21_12,
            u_21_15,
            u_21_17,
            u_21_2,
            u_21_7,
            u_21_8,
            u_21_phi_60,
            u_22_0,
            u_22_1,
            u_22_10,
            u_22_17,
            u_22_20,
            u_22_26,
            u_22_28,
            u_22_30,
            u_22_36,
            u_22_38,
            u_22_42,
            u_22_49,
            u_22_52,
            u_22_53,
            u_22_55,
            u_22_58,
            u_22_59,
            u_22_6,
            u_22_7,
            u_22_8,
            u_22_phi_63,
            u_23_0,
            u_23_1,
            u_23_22,
            u_23_26,
            u_23_27,
            u_23_5,
            u_23_8,
            u_23_phi_81,
            u_24_1,
            u_24_11,
            u_24_14,
            u_24_2,
            u_24_21,
            u_24_5,
            u_24_7,
            u_25_3,
            u_25_5,
            u_25_6,
            u_25_7,
            u_25_8,
            u_25_phi_72,
            u_26_2,
            u_26_6,
            u_26_7,
            u_27_10,
            u_27_11,
            u_27_13,
            u_27_17,
            u_27_2,
            u_27_20,
            u_27_22,
            u_27_25,
            u_27_28,
            u_27_3,
            u_27_32,
            u_27_7,
            u_27_9,
            u_29_0,
            u_29_1,
            u_29_phi_74,
            u_3_1,
            u_3_10,
            u_3_14,
            u_3_18,
            u_3_2,
            u_3_21,
            u_3_25,
            u_3_27,
            u_3_28,
            u_3_32,
            u_3_35,
            u_3_4,
            u_3_5,
            u_3_7,
            u_3_9,
            u_3_phi_11,
            u_3_phi_4,
            u_30_12,
            u_30_17,
            u_30_2,
            u_30_6,
            u_30_9,
            u_31_1,
            u_31_10,
            u_31_13,
            u_31_19,
            u_31_20,
            u_31_23,
            u_31_25,
            u_31_28,
            u_31_29,
            u_31_36,
            u_31_4,
            u_31_5,
            u_31_6,
            u_31_phi_77,
            u_32_0,
            u_32_1,
            u_32_2,
            u_32_phi_76,
            u_33_10,
            u_33_12,
            u_33_13,
            u_33_14,
            u_33_15,
            u_33_17,
            u_33_5,
            u_33_6,
            u_33_7,
            u_33_9,
            u_33_phi_78,
            u_34_0,
            u_34_11,
            u_34_12,
            u_34_13,
            u_34_16,
            u_34_20,
            u_34_21,
            u_34_23,
            u_34_5,
            u_35_10,
            u_35_11,
            u_35_12,
            u_35_2,
            u_35_9,
            u_36_1,
            u_36_11,
            u_36_15,
            u_36_16,
            u_36_21,
            u_36_22,
            u_36_3,
            u_36_4,
            u_36_8,
            u_36_phi_79,
            u_37_0,
            u_37_10,
            u_37_15,
            u_37_18,
            u_37_20,
            u_37_3,
            u_37_7,
            u_38_3,
            u_39_0,
            u_39_2,
            u_39_3,
            u_4_1,
            u_4_10,
            u_4_11,
            u_4_12,
            u_4_13,
            u_4_14,
            u_4_16,
            u_4_17,
            u_4_19,
            u_4_3,
            u_4_4,
            u_4_5,
            u_4_6,
            u_4_7,
            u_4_8,
            u_4_9,
            u_4_phi_45,
            u_4_phi_51,
            u_4_phi_55,
            u_4_phi_61,
            u_4_phi_65,
            u_4_phi_66,
            u_4_phi_69,
            u_40_0,
            u_40_1,
            u_40_2,
            u_5_0,
            u_5_1,
            u_5_11,
            u_5_12,
            u_5_14,
            u_5_15,
            u_5_17,
            u_5_18,
            u_5_19,
            u_5_2,
            u_5_23,
            u_5_27,
            u_5_3,
            u_5_6,
            u_5_7,
            u_5_9,
            u_5_phi_2,
            u_5_phi_22,
            u_5_phi_28,
            u_5_phi_41,
            u_5_phi_9,
            u_6_10,
            u_6_11,
            u_6_13,
            u_6_14,
            u_6_15,
            u_6_16,
            u_6_17,
            u_6_18,
            u_6_19,
            u_6_24,
            u_6_8,
            u_6_9,
            u_6_phi_27,
            u_6_phi_30,
            u_6_phi_33,
            u_6_phi_36,
            u_6_phi_43,
            u_7_1,
            u_7_2,
            u_7_3,
            u_7_4,
            u_7_phi_64,
            u_7_phi_65,
            u_8_0,
            u_8_13,
            u_8_14,
            u_8_16,
            u_8_17,
            u_8_18,
            u_8_19,
            u_8_2,
            u_8_23,
            u_8_26,
            u_8_29,
            u_8_3,
            u_8_31,
            u_8_38,
            u_8_39,
            u_8_45,
            u_8_47,
            u_8_5,
            u_8_51,
            u_8_8,
            u_8_9,
            u_8_phi_31,
            u_8_phi_62,
            u_8_phi_71,
            u_9_10,
            u_9_11,
            u_9_2,
            u_9_21,
            u_9_22,
            u_9_23,
            u_9_24,
            u_9_25,
            u_9_3,
            u_9_31,
            u_9_4,
            u_9_6,
            u_9_7,
            u_9_8,
            u_9_9,
            u_9_phi_18,
            u_9_phi_35,
            u_9_phi_38,
            u_9_phi_46,
            u_9_phi_83,
            u4_0_0,
            u4_0_1,
        };
    };
    shader_arg
    {
        vs_attr("d:/UC/smoke/vs_in.csv", "d:/UC/smoke/vs_out.csv", 0);
        uniform("d:/UC/smoke/vs_cbuf0.csv", "uvec4"){
            add(21);
        };
        uniform("d:/UC/smoke/vs_cbuf8.csv", "uvec4"){
            add_range(0,7);
            add(10,11,24,25,26,28,29,30);
        };
        uniform("d:/UC/smoke/vs_cbuf9.csv", "uvec4"){
            //add(7,14,15,16,17,18,20,74,75,76,78,79,80,81,83,84,85,86,88,104,105,113,121,138,141,157);
            add_range(0, 160);
        };
        uniform("d:/UC/smoke/vs_cbuf10.csv", "uvec4"){
            add_range(0,6);
        };
        uniform("d:/UC/smoke/vs_cbuf13.csv", "uvec4"){
            add(0,1,2,3,5,6);
        };
        uniform("d:/UC/smoke/vs_cbuf15.csv", "uvec4"){
            add(49,51,52,58);
            add_range(22,28);
        };
        uniform("d:/UC/smoke/vs_cbuf16.csv", "uvec4"){
            add(0,1);
        };
        ssbo_attr("private uint[] vs_ssbo_0_array", "uint", "d:/uc/smoke/vs_ssbo_0.csv", "[NonSerialized]");
        vao_attr("private Vector4[] in_attr4_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr4.csv", "[NonSerialized]"){
            add_range(0,65);
        };
        vao_attr("private Vector4[] in_attr5_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr5.csv", "[NonSerialized]"){
            add_range(0,65);
        };
        vao_attr("private Vector4[] in_attr6_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr6.csv", "[NonSerialized]"){
            add_range(0,65);
        };
        /*
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_smoke/ref_data_attr7.csv", "[NonSerialized]"){
            add_range(0,381);
        };
        */
        vao_attr("private Vector4[] in_attr7_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr7.csv", "[NonSerialized]"){
            add_range(0,65);
        };
        vao_attr("private Vector4[] in_attr9_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr9.csv", "[NonSerialized]"){
            add_range(0,65);
        };
        vao_attr("private Vector4[] in_attr10_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr10.csv", "[NonSerialized]"){
            add_range(0,65);
        };
        vao_attr("private Vector4[] in_attr11_array", "vec4", "d:/uc/VAO_smoke/vertex_in_attr11.csv", "[NonSerialized]"){
            add_range(0,65);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/smoke/1", "d:/uc/VAO_smoke/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/smoke/2", "d:/uc/VAO_smoke/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/smoke/3", "d:/uc/VAO_smoke/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/smoke/4", "d:/uc/VAO_smoke/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/smoke/5", "d:/uc/VAO_smoke/5");
    };
    //
    shader_arg(10)
    {
        redirect(0, "d:/UC/smoke/time1/t1", "d:/uc/VAO_smoke/time1/t1");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t2", "d:/uc/VAO_smoke/time1/t2");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t3", "d:/uc/VAO_smoke/time1/t3");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t4", "d:/uc/VAO_smoke/time1/t4");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t5", "d:/uc/VAO_smoke/time1/t5");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t6", "d:/uc/VAO_smoke/time1/t6");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t7", "d:/uc/VAO_smoke/time1/t7");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t8", "d:/uc/VAO_smoke/time1/t8");
    };
    //
    shader_arg(20)
    {
        redirect(0, "d:/UC/smoke/time2/t1", "d:/uc/VAO_smoke/time2/t1");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t2", "d:/uc/VAO_smoke/time2/t2");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t3", "d:/uc/VAO_smoke/time2/t3");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t4", "d:/uc/VAO_smoke/time2/t4");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t5", "d:/uc/VAO_smoke/time2/t5");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t6", "d:/uc/VAO_smoke/time2/t6");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t7", "d:/uc/VAO_smoke/time2/t7");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t8", "d:/uc/VAO_smoke/time2/t8");
    };
    //
    shader_arg(30)
    {
        redirect(0, "d:/UC/smoke/time3/t1", "d:/uc/VAO_smoke/time3/t1");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t2", "d:/uc/VAO_smoke/time3/t2");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t3", "d:/uc/VAO_smoke/time3/t3");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t4", "d:/uc/VAO_smoke/time3/t4");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t5", "d:/uc/VAO_smoke/time3/t5");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t6", "d:/uc/VAO_smoke/time3/t6");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t7", "d:/uc/VAO_smoke/time3/t7");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t8", "d:/uc/VAO_smoke/time3/t8");
    };
    //
    shader_arg(41)
    {
        redirect(0, "d:/UC/smoke/fade1", "d:/uc/VAO_smoke/fade1");
    };
    shader_arg(42)
    {
        redirect(0, "d:/UC/smoke/fade2", "d:/uc/VAO_smoke/fade2");
    };
    shader_arg(43)
    {
        redirect(0, "d:/UC/smoke/fade3", "d:/uc/VAO_smoke/fade3");
    };
    //
    shader_arg(50)
    {
        redirect(0, "d:/UC/smoke/hlsl", "d:/uc/VAO_smoke/hlsl");
        hlsl_merge("d:/UC/smoke/hlsl/vs_cbuf9_vs.csv")
        {
            name_replacement(vs_cbuf9, $iter, vs_cbuf9[@arg(1)])for(0,160);
        };
        hlsl_merge("d:/UC/smoke/hlsl/Globals_vs.csv")
        {
            attr_hlsl_map("in_attr4", "uni_attr4");
            attr_hlsl_map("in_attr5", "uni_attr5");
            attr_hlsl_map("in_attr6", "uni_attr6");
            attr_hlsl_map("in_attr7", "uni_attr7");
            attr_hlsl_map("in_attr9", "uni_attr9");
            attr_hlsl_map("in_attr10", "uni_attr10");
            attr_hlsl_map("in_attr11", "uni_attr11");
            name_replacement("vertex.in_attr4", *, uni_attr4);
            name_replacement("vertex.in_attr5", *, uni_attr5);
            name_replacement("vertex.in_attr6", *, uni_attr6);
            name_replacement("vertex.in_attr7", *, uni_attr7);
            name_replacement("vertex.in_attr9", *, uni_attr9);
            name_replacement("vertex.in_attr10", *, uni_attr10);
            name_replacement("vertex.in_attr11", *, uni_attr11);
            name_replacement(vs_cbuf8, $iter, view_proj[@arg(1)])for(0,7);
            name_replacement(vs_cbuf8, 29, camera_wpos);
            name_replacement(vs_cbuf15, 28, lightDir);
            name_replacement(*, *, @join(@arg(0),"_",@arg(1)));
        };
    };
    type_replacement
    {
        vs_cbuf0 = float;
        vs_cbuf8 = float;
        vs_cbuf10 = float;
        vs_cbuf13 = float;
        vs_cbuf15 = float;
        vs_cbuf16 = float;
    };
    function_replacement
    {
        textureLod = textureLodTest(@arg(0), @arg(1), s_sampler);
        vs_ssbo0[*] = vs_ssbo0[@arg(1)];
        vs_cbuf9[*] = vs_cbuf9[@arg(1)];
        (*)[*] = @join(@arg_and_lvlup(0), _, @arg_and_lvlup(1));
    };
    string_replacement
    {
        string("uvec", "uint");
        string("ivec", "int");
        string("vec2", "float2");
        string("vec3", "float3");
        string("vec4", "float4");
        string("in_attr", "uni_attr");
        string("out_attr", "o.fs_attr");
        string("gl_Position", "o.vertex");
        regex(@"\buni_attr0\b", "v.vertex");
        regex(@"\buni_attr1\b", "v.uv");
        regex(@"\buni_attr2\b", "v.offset");
    };
    calculator
    {
        textureSize(tex0,*) = vec2(8,13);
        textureSize(tex1,*) = vec2(256,256);
        textureSize(tex2,*) = vec2(256,256);
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,1.0);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,1.0);
    };
};
ps
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 512;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;
        remove_duplicate_expression;

        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt");
            split_on("texture", 3);
            split_on("textureLod", 3);
            split_on("texelFetch", 3);
            split_on("texelQueryLod", 3);
            split_on("texelSize", 3);
            split_on("log2", 12);
        };

        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            b_0_0,
            b_1_0,
            f_0_15,
            f_0_16,
            f_0_2,
            f_0_23,
            f_0_24,
            f_0_25,
            f_1_8,
            f_4_12,
            f_4_13,
            f_4_5,
            f_4_6,
            f_6_6,
            f_8_1,
            f2_0_2,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f4_0_5,
            pf_0_10,
            pf_0_12,
            pf_0_14,
            pf_0_17,
            pf_0_21,
            pf_0_4,
            pf_0_6,
            pf_0_9,
            pf_1_13,
            pf_1_17,
            pf_1_4,
            pf_1_5,
            pf_1_9,
            pf_2_1,
            pf_2_10,
            pf_2_6,
            pf_3_1,
            pf_3_5,
            pf_3_6,
            pf_3_9,
            pf_4_2,
            pf_4_3,
            pf_4_5,
            pf_5_2,
            pf_5_3,
            pf_6_0,
        };
    };
    shader_arg
    {
        ps_attr("d:/UC/smoke/vs_out.csv", 0){
            map_in_attr("out_attr0","in_attr0");
            map_in_attr("out_attr1","in_attr1");
            map_in_attr("out_attr2","in_attr2");
            map_in_attr("out_attr3","in_attr3");
            map_in_attr("out_attr4","in_attr4");
            map_in_attr("out_attr5","in_attr5");
            map_in_attr("out_attr6","in_attr6");
            map_in_attr("out_attr7","in_attr7");
            map_in_attr("out_attr8","in_attr8");
            map_in_attr("out_attr9","in_attr9");
            map_in_attr("out_attr10","in_attr10");
            map_in_attr("out_attr11","in_attr11");
            map_in_attr("out_attr12","in_attr12");
            remove_in_attr("gl_Position");
        };
        uniform("d:/UC/smoke/fs_cbuf8.csv", "uvec4"){
            add(30);
        };
        uniform("d:/UC/smoke/fs_cbuf9.csv", "uvec4"){
            add(139,140,189);
        };
        uniform("d:/UC/smoke/fs_cbuf10.csv", "uvec4"){
            add(13);
        };
        uniform("d:/UC/smoke/fs_cbuf13.csv", "uvec4"){
            add(0);
        };
        uniform("d:/UC/smoke/fs_cbuf15.csv", "uvec4"){
            add(1,25,26);
        };
    };
    shader_arg(1)
    {
        redirect(0, "d:/UC/smoke/1", "d:/uc/VAO_smoke/1");
    };
    shader_arg(2)
    {
        redirect(0, "d:/UC/smoke/2", "d:/uc/VAO_smoke/2");
    };
    shader_arg(3)
    {
        redirect(0, "d:/UC/smoke/3", "d:/uc/VAO_smoke/3");
    };
    shader_arg(4)
    {
        redirect(0, "d:/UC/smoke/4", "d:/uc/VAO_smoke/4");
    };
    shader_arg(5)
    {
        redirect(0, "d:/UC/smoke/5", "d:/uc/VAO_smoke/5");
    };
    //
    shader_arg(10)
    {
        redirect(0, "d:/UC/smoke/time1/t1", "d:/uc/VAO_smoke/time1/t1");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t2", "d:/uc/VAO_smoke/time1/t2");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t3", "d:/uc/VAO_smoke/time1/t3");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t4", "d:/uc/VAO_smoke/time1/t4");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t5", "d:/uc/VAO_smoke/time1/t5");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t6", "d:/uc/VAO_smoke/time1/t6");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t7", "d:/uc/VAO_smoke/time1/t7");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time1/t8", "d:/uc/VAO_smoke/time1/t8");
    };
    //
    shader_arg(20)
    {
        redirect(0, "d:/UC/smoke/time2/t1", "d:/uc/VAO_smoke/time2/t1");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t2", "d:/uc/VAO_smoke/time2/t2");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t3", "d:/uc/VAO_smoke/time2/t3");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t4", "d:/uc/VAO_smoke/time2/t4");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t5", "d:/uc/VAO_smoke/time2/t5");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t6", "d:/uc/VAO_smoke/time2/t6");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t7", "d:/uc/VAO_smoke/time2/t7");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time2/t8", "d:/uc/VAO_smoke/time2/t8");
    };
    //
    shader_arg(30)
    {
        redirect(0, "d:/UC/smoke/time3/t1", "d:/uc/VAO_smoke/time3/t1");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t2", "d:/uc/VAO_smoke/time3/t2");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t3", "d:/uc/VAO_smoke/time3/t3");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t4", "d:/uc/VAO_smoke/time3/t4");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t5", "d:/uc/VAO_smoke/time3/t5");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t6", "d:/uc/VAO_smoke/time3/t6");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t7", "d:/uc/VAO_smoke/time3/t7");
    };
    shader_arg
    {
        redirect(0, "d:/UC/smoke/time3/t8", "d:/uc/VAO_smoke/time3/t8");
    };
    //
    shader_arg(41)
    {
        redirect(0, "d:/UC/smoke/fade1", "d:/uc/VAO_smoke/fade1");
    };
    shader_arg(42)
    {
        redirect(0, "d:/UC/smoke/fade2", "d:/uc/VAO_smoke/fade2");
    };
    shader_arg(43)
    {
        redirect(0, "d:/UC/smoke/fade3", "d:/uc/VAO_smoke/fade3");
    };
    //
    shader_arg(50)
    {
        redirect(0, "d:/UC/smoke/hlsl", "d:/uc/VAO_smoke/hlsl");
        hlsl_merge("d:/UC/smoke/hlsl/Globals_fs.csv")
        {
            name_replacement(vs_cbuf8, $iter, view_proj[@arg(1)])for(0,7);
            name_replacement(vs_cbuf8, 29, camera_wpos);
            name_replacement(vs_cbuf15, 28, lightDir);
            name_replacement(*, *, @join(@arg(0),"_",@arg(1)));
        };
    };
    calculator
    {
        textureSize(tex3,*) = vec2(64,64);
        textureSize(tex4,*) = vec2(64,64);
        textureSize(tex5,*) = vec2(512,512);
        textureSize(depthTex,*) = vec2(800,450);
        textureSize(*,*) = vec2(512,128);
        texelFetch(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        textureLod(*,*,*) = vec4(0.5,0.5,0.5,0.75);
        texture(tex5,*) = vec4(0.00020,0.0002,0.0002,0.0002);
        texture(*,*) = vec4(0.5,0.5,0.5,0.75);
        textureQueryLod(*,*) = vec2(4,1);
    };
};
cs
{
    setting
    {
        debug_mode;
        //print_graph;
        //def_multiline;
        //def_expanded_only_once;
        //def_multiline_for_variable = false;
        //def_expanded_only_once_for_variable = false;
        def_max_level = 32;
        def_max_length = 512;
        //def_skip_value;
        def_skip_expression;
        def_max_level_for_variable = 256;
        def_max_length_for_variable = 20480;
        compute_graph_nodes_capacity = 10240;
        shader_variables_capacity = 1024;
        string_buffer_capacity_surplus = 1024;
        generate_expression_list;
        remove_duplicate_expression;
    };
};
