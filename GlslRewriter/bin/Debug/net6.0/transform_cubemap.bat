
GlslRewriter cubemap.vs
GlslRewriter cubemap.fs

GlslRewriter -argcfg 1 -out cubemap1_vs.txt cubemap.vs
GlslRewriter -argcfg 2 -out cubemap2_vs.txt cubemap.vs
GlslRewriter -argcfg 3 -out cubemap3_vs.txt cubemap.vs
GlslRewriter -argcfg 4 -out cubemap4_vs.txt cubemap.vs

GlslRewriter -argcfg 1 -out cubemap1_fs.txt cubemap.fs
GlslRewriter -argcfg 2 -out cubemap2_fs.txt cubemap.fs
GlslRewriter -argcfg 3 -out cubemap3_fs.txt cubemap.fs
GlslRewriter -argcfg 4 -out cubemap4_fs.txt cubemap.fs
