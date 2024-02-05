rem GlslRewriter -argcfg 10 -out smoke_vs_t1_1.txt smoke.vs
rem GlslRewriter -argcfg 11 -out smoke_vs_t1_2.txt smoke.vs
rem GlslRewriter -argcfg 12 -out smoke_vs_t1_3.txt smoke.vs
rem GlslRewriter -argcfg 13 -out smoke_vs_t1_4.txt smoke.vs
rem GlslRewriter -argcfg 14 -out smoke_vs_t1_5.txt smoke.vs
rem GlslRewriter -argcfg 15 -out smoke_vs_t1_6.txt smoke.vs
rem GlslRewriter -argcfg 16 -out smoke_vs_t1_7.txt smoke.vs
rem GlslRewriter -argcfg 17 -out smoke_vs_t1_8.txt smoke.vs
rem 
rem GlslRewriter -argcfg 10 -out smoke_fs_t1_1.txt smoke.fs
rem GlslRewriter -argcfg 11 -out smoke_fs_t1_2.txt smoke.fs
rem GlslRewriter -argcfg 12 -out smoke_fs_t1_3.txt smoke.fs
rem GlslRewriter -argcfg 13 -out smoke_fs_t1_4.txt smoke.fs
rem GlslRewriter -argcfg 14 -out smoke_fs_t1_5.txt smoke.fs
rem GlslRewriter -argcfg 15 -out smoke_fs_t1_6.txt smoke.fs
rem GlslRewriter -argcfg 16 -out smoke_fs_t1_7.txt smoke.fs
rem GlslRewriter -argcfg 17 -out smoke_fs_t1_8.txt smoke.fs
rem 
rem GlslRewriter -argcfg 20 -out smoke_vs_t2_1.txt smoke.vs
rem GlslRewriter -argcfg 21 -out smoke_vs_t2_2.txt smoke.vs
rem GlslRewriter -argcfg 22 -out smoke_vs_t2_3.txt smoke.vs
rem GlslRewriter -argcfg 23 -out smoke_vs_t2_4.txt smoke.vs
rem GlslRewriter -argcfg 24 -out smoke_vs_t2_5.txt smoke.vs
rem GlslRewriter -argcfg 25 -out smoke_vs_t2_6.txt smoke.vs
rem GlslRewriter -argcfg 26 -out smoke_vs_t2_7.txt smoke.vs
rem GlslRewriter -argcfg 27 -out smoke_vs_t2_8.txt smoke.vs
rem 
rem GlslRewriter -argcfg 20 -out smoke_fs_t2_1.txt smoke.fs
rem GlslRewriter -argcfg 21 -out smoke_fs_t2_2.txt smoke.fs
rem GlslRewriter -argcfg 22 -out smoke_fs_t2_3.txt smoke.fs
rem GlslRewriter -argcfg 23 -out smoke_fs_t2_4.txt smoke.fs
rem GlslRewriter -argcfg 24 -out smoke_fs_t2_5.txt smoke.fs
rem GlslRewriter -argcfg 25 -out smoke_fs_t2_6.txt smoke.fs
rem GlslRewriter -argcfg 26 -out smoke_fs_t2_7.txt smoke.fs
rem GlslRewriter -argcfg 27 -out smoke_fs_t2_8.txt smoke.fs
rem 
rem GlslRewriter -argcfg 30 -out smoke_vs_t3_1.txt smoke.vs
rem GlslRewriter -argcfg 31 -out smoke_vs_t3_2.txt smoke.vs
rem GlslRewriter -argcfg 32 -out smoke_vs_t3_3.txt smoke.vs
rem GlslRewriter -argcfg 33 -out smoke_vs_t3_4.txt smoke.vs
rem GlslRewriter -argcfg 34 -out smoke_vs_t3_5.txt smoke.vs
rem GlslRewriter -argcfg 35 -out smoke_vs_t3_6.txt smoke.vs
rem GlslRewriter -argcfg 36 -out smoke_vs_t3_7.txt smoke.vs
rem GlslRewriter -argcfg 37 -out smoke_vs_t3_8.txt smoke.vs
rem 
rem GlslRewriter -argcfg 30 -out smoke_fs_t3_1.txt smoke.fs
rem GlslRewriter -argcfg 31 -out smoke_fs_t3_2.txt smoke.fs
rem GlslRewriter -argcfg 32 -out smoke_fs_t3_3.txt smoke.fs
rem GlslRewriter -argcfg 33 -out smoke_fs_t3_4.txt smoke.fs
rem GlslRewriter -argcfg 34 -out smoke_fs_t3_5.txt smoke.fs
rem GlslRewriter -argcfg 35 -out smoke_fs_t3_6.txt smoke.fs
rem GlslRewriter -argcfg 36 -out smoke_fs_t3_7.txt smoke.fs
rem GlslRewriter -argcfg 37 -out smoke_fs_t3_8.txt smoke.fs

GlslRewriter smoke.vs
GlslRewriter smoke.fs

GlslRewriter -r -out smoke_vs_hlsl.txt smoke.vs
GlslRewriter -r -out smoke_fs_hlsl.txt smoke.fs

GlslRewriter -argcfg 41 -out smoke_vs_fade1.txt smoke.vs
GlslRewriter -argcfg 42 -out smoke_vs_fade2.txt smoke.vs
GlslRewriter -argcfg 43 -out smoke_vs_fade3.txt smoke.vs

GlslRewriter -argcfg 41 -out smoke_fs_fade1.txt smoke.fs
GlslRewriter -argcfg 42 -out smoke_fs_fade2.txt smoke.fs
GlslRewriter -argcfg 43 -out smoke_fs_fade3.txt smoke.fs
