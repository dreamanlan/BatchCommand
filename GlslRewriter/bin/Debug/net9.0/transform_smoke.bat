rem GlslRewriter -arg_cfg 10 -out smoke_vs_t1_1.txt smoke.vs
rem GlslRewriter -arg_cfg 11 -out smoke_vs_t1_2.txt smoke.vs
rem GlslRewriter -arg_cfg 12 -out smoke_vs_t1_3.txt smoke.vs
rem GlslRewriter -arg_cfg 13 -out smoke_vs_t1_4.txt smoke.vs
rem GlslRewriter -arg_cfg 14 -out smoke_vs_t1_5.txt smoke.vs
rem GlslRewriter -arg_cfg 15 -out smoke_vs_t1_6.txt smoke.vs
rem GlslRewriter -arg_cfg 16 -out smoke_vs_t1_7.txt smoke.vs
rem GlslRewriter -arg_cfg 17 -out smoke_vs_t1_8.txt smoke.vs
rem 
rem GlslRewriter -arg_cfg 10 -out smoke_fs_t1_1.txt smoke.fs
rem GlslRewriter -arg_cfg 11 -out smoke_fs_t1_2.txt smoke.fs
rem GlslRewriter -arg_cfg 12 -out smoke_fs_t1_3.txt smoke.fs
rem GlslRewriter -arg_cfg 13 -out smoke_fs_t1_4.txt smoke.fs
rem GlslRewriter -arg_cfg 14 -out smoke_fs_t1_5.txt smoke.fs
rem GlslRewriter -arg_cfg 15 -out smoke_fs_t1_6.txt smoke.fs
rem GlslRewriter -arg_cfg 16 -out smoke_fs_t1_7.txt smoke.fs
rem GlslRewriter -arg_cfg 17 -out smoke_fs_t1_8.txt smoke.fs
rem 
rem GlslRewriter -arg_cfg 20 -out smoke_vs_t2_1.txt smoke.vs
rem GlslRewriter -arg_cfg 21 -out smoke_vs_t2_2.txt smoke.vs
rem GlslRewriter -arg_cfg 22 -out smoke_vs_t2_3.txt smoke.vs
rem GlslRewriter -arg_cfg 23 -out smoke_vs_t2_4.txt smoke.vs
rem GlslRewriter -arg_cfg 24 -out smoke_vs_t2_5.txt smoke.vs
rem GlslRewriter -arg_cfg 25 -out smoke_vs_t2_6.txt smoke.vs
rem GlslRewriter -arg_cfg 26 -out smoke_vs_t2_7.txt smoke.vs
rem GlslRewriter -arg_cfg 27 -out smoke_vs_t2_8.txt smoke.vs
rem 
rem GlslRewriter -arg_cfg 20 -out smoke_fs_t2_1.txt smoke.fs
rem GlslRewriter -arg_cfg 21 -out smoke_fs_t2_2.txt smoke.fs
rem GlslRewriter -arg_cfg 22 -out smoke_fs_t2_3.txt smoke.fs
rem GlslRewriter -arg_cfg 23 -out smoke_fs_t2_4.txt smoke.fs
rem GlslRewriter -arg_cfg 24 -out smoke_fs_t2_5.txt smoke.fs
rem GlslRewriter -arg_cfg 25 -out smoke_fs_t2_6.txt smoke.fs
rem GlslRewriter -arg_cfg 26 -out smoke_fs_t2_7.txt smoke.fs
rem GlslRewriter -arg_cfg 27 -out smoke_fs_t2_8.txt smoke.fs
rem 
rem GlslRewriter -arg_cfg 30 -out smoke_vs_t3_1.txt smoke.vs
rem GlslRewriter -arg_cfg 31 -out smoke_vs_t3_2.txt smoke.vs
rem GlslRewriter -arg_cfg 32 -out smoke_vs_t3_3.txt smoke.vs
rem GlslRewriter -arg_cfg 33 -out smoke_vs_t3_4.txt smoke.vs
rem GlslRewriter -arg_cfg 34 -out smoke_vs_t3_5.txt smoke.vs
rem GlslRewriter -arg_cfg 35 -out smoke_vs_t3_6.txt smoke.vs
rem GlslRewriter -arg_cfg 36 -out smoke_vs_t3_7.txt smoke.vs
rem GlslRewriter -arg_cfg 37 -out smoke_vs_t3_8.txt smoke.vs
rem 
rem GlslRewriter -arg_cfg 30 -out smoke_fs_t3_1.txt smoke.fs
rem GlslRewriter -arg_cfg 31 -out smoke_fs_t3_2.txt smoke.fs
rem GlslRewriter -arg_cfg 32 -out smoke_fs_t3_3.txt smoke.fs
rem GlslRewriter -arg_cfg 33 -out smoke_fs_t3_4.txt smoke.fs
rem GlslRewriter -arg_cfg 34 -out smoke_fs_t3_5.txt smoke.fs
rem GlslRewriter -arg_cfg 35 -out smoke_fs_t3_6.txt smoke.fs
rem GlslRewriter -arg_cfg 36 -out smoke_fs_t3_7.txt smoke.fs
rem GlslRewriter -arg_cfg 37 -out smoke_fs_t3_8.txt smoke.fs

GlslRewriter smoke.vs
GlslRewriter smoke.fs

GlslRewriter -r -out smoke_vs_hlsl.txt smoke.vs
GlslRewriter -r -out smoke_fs_hlsl.txt smoke.fs

GlslRewriter -arg_cfg 41 -out smoke_vs_fade1.txt smoke.vs
GlslRewriter -arg_cfg 42 -out smoke_vs_fade2.txt smoke.vs
GlslRewriter -arg_cfg 43 -out smoke_vs_fade3.txt smoke.vs

GlslRewriter -arg_cfg 41 -out smoke_fs_fade1.txt smoke.fs
GlslRewriter -arg_cfg 42 -out smoke_fs_fade2.txt smoke.fs
GlslRewriter -arg_cfg 43 -out smoke_fs_fade3.txt smoke.fs
