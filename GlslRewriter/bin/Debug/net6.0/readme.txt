﻿0、工具默认在源文件目录输出重新整理后的代码文件（希望是能直接编译的），然后在tmp目录下有与输入和输出文件同名的文件，一个用于DSL解析的源
文件，一个是本文档后续所指的过程文件，是基于输入源文件的过程文件，通过注释描述了代码整理的中间过程，可以用来辅助分析排错
1、首先配置输入变量与uniform变量初始值，目前支持导入从RenderDoc导出的csv文件，可以具体指明导入哪些行（序号），对输入指定一个顶点序号即可
		vs_attr("d:/UC/vs_in.csv", "d:/UC/vs_out.csv", 0);
		uniform("d:/UC/vs_cbuf8.csv", "uvec4"){
			add_range(0,7);
			add(29,30);
		};
		uniform("d:/UC/vs_cbuf9.csv", "uvec4"){
			add(11,12,16,141);
			add_range(113,116);
		};
		uniform("d:/UC/vs_cbuf10.csv", "uvec4"){
			add(0,2,3);
		};
		uniform("d:/UC/vs_cbuf13.csv", "uvec4"){
			add(6);
		};
		uniform("d:/UC/vs_cbuf15.csv", "uvec4"){
			add(1,54,55,57,60,61);
			add_range(22,28);
		};
2、配置拆分赋值表达式，一般采用多行（默认）注释输出层次化的赋值表达式，如不需要自动拆分，此时可生成文件
        split_object_assignment{
            set(frag_color0.x, 64, 2048, true, true);
            set(frag_color0.y, 64, 2048);
            set(frag_color0.z, 64, 2048);
            set(frag_color0.w, 64, 2048);
        };

        split_variable_assignment{
            f_0_12,
            f_2_16,
            f_3_11,
            f_3_22,
            f4_0_0,
            f4_0_1,
            f4_0_2,
            f4_0_3,
            f4_0_4,
            f_5_2,
            f_5_3,
            f_5_4,
            pf_4_4,
            pf_5_2,
            pf_6_1,
        };
3、如需配置自动拆分表达式，则配置后再生成文件
        auto_split(15){
            split_on("exp2");
            split_on("inversesqrt");
            split_on("log2", 12);
        };
4、搜索tmp目录下的过程文件是否有...，有则表明生成的表达式过长或表达嵌套层次过深，此时需要再拆分中间变量，如果使用自动拆分，在过程文件尾有新
的拆分赋值变量列表，复制替换2中拆分赋值表达式，然后再次生成文件
5、如想人工拆分，可观察过程文件中带有...的变量的层次化表达式，找出拆分的中间变量，配置到拆分赋值表达式后再重新生成文件
6、反复进行4或5，直到过程文件中不再有...出现。（通过配置更长的表达式与更深的嵌套层次也可以消除...但这样生成的代码会因为单行过长而无法阅读，
在配置自动拆分时，工具自动迭代直到没有新增拆分变量为止，最多32次，最大迭代次数可以配置里调整理，如果此时过程文件仍有...，表明需要减小配置的
自动拆分层数或者增加表达式长度配置或增加表达式的嵌套层次配置）
		setting
		{
			max_iterations = 32;
		};
7、配置生成表达式列表，再次重新生成文件，此时在源文件所有目录会生成一个代码文件，这是整理后的源代码，应该可以编译通过
        generate_expression_list;
8、尝试编译第7步生成的代码文件(使用GlslangValidator，renderdoc带一个，或者安装vulkan sdk)，通常应该可以通过编译，如果有错解决相应编译错误，
完成后即是可以使用的新shader
		glslangvalidator -G -S vert input.vs
		glslangvalidator -G -S frag input.fs
9、如需要参考在过程文件与输出代码文件里的注释部分的计算值，可以配置计算器对无法计算的函数提供替代值（一般是纹理采样函数），对于普通可计算的
值，工具会执行相应计算（具体api实现可能需要根据反编译文件内容不断扩充）
		calculator
		{
			textureSize(*,*) = vec2(512,128);
			texelFetch(*,*,*) = vec4(0.5,0.5,0.5,0.75);
			textureLod(*,*,*) = vec4(0.5,0.5,0.5,0.75);
			texture(*,*) = vec4(0.5,0.5,0.5,0.75);
			textureQueryLod(*,*) = vec2(4,1);
		};
10、phi变量的赋值没有添加到计算图，所以phi变量不会计算值（因为phi变量会多次赋值，计算也不准确），可以通过配置来指定值
		variable_assignment
		{
			u_1_phi_1 = 1234.5;
		};

配置实例见：test_args.dsl