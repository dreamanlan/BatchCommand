
GlslRewriter cubemap.vs
GlslRewriter cubemap.fs

GlslRewriter -arg_cfg 1 -out cubemap1_vs.txt cubemap.vs
GlslRewriter -arg_cfg 2 -out cubemap2_vs.txt cubemap.vs
GlslRewriter -arg_cfg 3 -out cubemap3_vs.txt cubemap.vs
GlslRewriter -arg_cfg 4 -out cubemap4_vs.txt cubemap.vs

GlslRewriter -arg_cfg 1 -out cubemap1_fs.txt cubemap.fs
GlslRewriter -arg_cfg 2 -out cubemap2_fs.txt cubemap.fs
GlslRewriter -arg_cfg 3 -out cubemap3_fs.txt cubemap.fs
GlslRewriter -arg_cfg 4 -out cubemap4_fs.txt cubemap.fs
