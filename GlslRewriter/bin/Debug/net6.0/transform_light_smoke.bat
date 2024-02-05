rem GlslRewriter light_smoke0.vs
rem GlslRewriter light_smoke0.fs

rem GlslRewriter light_smoke.vs
rem GlslRewriter -argcfg 1 -out light_smoke_vs_1.txt light_smoke.vs
rem GlslRewriter -argcfg 2 -out light_smoke_vs_2.txt light_smoke.vs
rem GlslRewriter -argcfg 3 -out light_smoke_vs_3.txt light_smoke.vs
rem GlslRewriter -argcfg 4 -out light_smoke_vs_4.txt light_smoke.vs

rem GlslRewriter light_smoke.fs
rem GlslRewriter -argcfg 1 -out light_smoke_fs_1.txt light_smoke.fs
rem GlslRewriter -argcfg 2 -out light_smoke_fs_2.txt light_smoke.fs
rem GlslRewriter -argcfg 3 -out light_smoke_fs_3.txt light_smoke.fs
rem GlslRewriter -argcfg 4 -out light_smoke_fs_4.txt light_smoke.fs

GlslRewriter -r -out light_smoke0_vs_hlsl.txt light_smoke0.vs
GlslRewriter -r -out light_smoke0_fs_hlsl.txt light_smoke0.fs
GlslRewriter -r -out light_smoke_vs_hlsl.txt light_smoke.vs
GlslRewriter -r -out light_smoke_fs_hlsl.txt light_smoke.fs
