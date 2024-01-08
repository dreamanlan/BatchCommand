
GlslRewriter lighttex.vs
GlslRewriter lighttex.fs

GlslRewriter -argcfg 1 -out lighttex1_vs.txt lighttex.vs
GlslRewriter -argcfg 2 -out lighttex2_vs.txt lighttex.vs
GlslRewriter -argcfg 3 -out lighttex3_vs.txt lighttex.vs

GlslRewriter -argcfg 1 -out lighttex1_fs.txt lighttex.fs
GlslRewriter -argcfg 2 -out lighttex2_fs.txt lighttex.fs
GlslRewriter -argcfg 3 -out lighttex3_fs.txt lighttex.fs

GlslRewriter -r -argcfg 0 -out lighttex_vs_hlsl.txt lighttex.vs
GlslRewriter -r -argcfg 1 -out lighttex1_vs_hlsl.txt lighttex.vs
GlslRewriter -r -argcfg 2 -out lighttex2_vs_hlsl.txt lighttex.vs
GlslRewriter -r -argcfg 3 -out lighttex3_vs_hlsl.txt lighttex.vs

GlslRewriter -r -argcfg 0 -out lighttex_fs_hlsl.txt lighttex.fs
GlslRewriter -r -argcfg 1 -out lighttex1_fs_hlsl.txt lighttex.fs
GlslRewriter -r -argcfg 2 -out lighttex2_fs_hlsl.txt lighttex.fs
GlslRewriter -r -argcfg 3 -out lighttex3_fs_hlsl.txt lighttex.fs

GlslRewriter -argcfg 10 -out lighttex10_vs_after_rain_12.txt lighttex.vs
GlslRewriter -argcfg 11 -out lighttex11_vs_fog.txt lighttex.vs
GlslRewriter -argcfg 12 -out lighttex12_vs_rain_4.txt lighttex.vs
GlslRewriter -argcfg 13 -out lighttex13_vs_rain_6.txt lighttex.vs
GlslRewriter -argcfg 14 -out lighttex14_vs_rain_8.txt lighttex.vs
GlslRewriter -argcfg 15 -out lighttex15_vs_sky_2.txt lighttex.vs
GlslRewriter -argcfg 16 -out lighttex16_vs_sky_4.txt lighttex.vs
GlslRewriter -argcfg 17 -out lighttex17_vs_sky_5.txt lighttex.vs
GlslRewriter -argcfg 18 -out lighttex18_vs_sky_6.txt lighttex.vs
GlslRewriter -argcfg 19 -out lighttex19_vs_sky_8.txt lighttex.vs
GlslRewriter -argcfg 20 -out lighttex20_vs_sky_10.txt lighttex.vs
GlslRewriter -argcfg 21 -out lighttex21_vs_sky_12.txt lighttex.vs
GlslRewriter -argcfg 22 -out lighttex22_vs_sky_14.txt lighttex.vs
GlslRewriter -argcfg 23 -out lighttex23_vs_sky_16.txt lighttex.vs
GlslRewriter -argcfg 24 -out lighttex24_vs_sky_17.txt lighttex.vs
GlslRewriter -argcfg 25 -out lighttex25_vs_sky_18.txt lighttex.vs
GlslRewriter -argcfg 26 -out lighttex26_vs_sky_19.txt lighttex.vs
GlslRewriter -argcfg 27 -out lighttex27_vs_sky_20.txt lighttex.vs
GlslRewriter -argcfg 28 -out lighttex28_vs_sky_22.txt lighttex.vs
GlslRewriter -argcfg 29 -out lighttex29_vs_sky_24.txt lighttex.vs

GlslRewriter -argcfg 10 -out lighttex10_fs_after_rain_12.txt lighttex.fs
GlslRewriter -argcfg 11 -out lighttex11_fs_fog.txt lighttex.fs
GlslRewriter -argcfg 12 -out lighttex12_fs_rain_4.txt lighttex.fs
GlslRewriter -argcfg 13 -out lighttex13_fs_rain_6.txt lighttex.fs
GlslRewriter -argcfg 14 -out lighttex14_fs_rain_8.txt lighttex.fs
GlslRewriter -argcfg 15 -out lighttex15_fs_sky_2.txt lighttex.fs
GlslRewriter -argcfg 16 -out lighttex16_fs_sky_4.txt lighttex.fs
GlslRewriter -argcfg 17 -out lighttex17_fs_sky_5.txt lighttex.fs
GlslRewriter -argcfg 18 -out lighttex18_fs_sky_6.txt lighttex.fs
GlslRewriter -argcfg 19 -out lighttex19_fs_sky_8.txt lighttex.fs
GlslRewriter -argcfg 20 -out lighttex20_fs_sky_10.txt lighttex.fs
GlslRewriter -argcfg 21 -out lighttex21_fs_sky_12.txt lighttex.fs
GlslRewriter -argcfg 22 -out lighttex22_fs_sky_14.txt lighttex.fs
GlslRewriter -argcfg 23 -out lighttex23_fs_sky_16.txt lighttex.fs
GlslRewriter -argcfg 24 -out lighttex24_fs_sky_17.txt lighttex.fs
GlslRewriter -argcfg 25 -out lighttex25_fs_sky_18.txt lighttex.fs
GlslRewriter -argcfg 26 -out lighttex26_fs_sky_19.txt lighttex.fs
GlslRewriter -argcfg 27 -out lighttex27_fs_sky_20.txt lighttex.fs
GlslRewriter -argcfg 28 -out lighttex28_fs_sky_22.txt lighttex.fs
GlslRewriter -argcfg 29 -out lighttex29_fs_sky_24.txt lighttex.fs
