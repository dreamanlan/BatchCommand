scheme("1、安卓TestUC")
{
    add_button("clickonptr","simulate click", "gmscript", "clickonptr()");
    add_button("build engine","build test engine","buildengine", "dbgscp");
    add_button("copy dev","copy dev engine","copyengine", "dev");
    add_button("copy release","copy release engine","copyengine", "rel");
    add_button("testuc","open testuc","open", "testuc");
    add_button("testucsym","copy testuc symbols","unzipsym", "testuc");
    add_button("android studio","open android studio","open", "androidstudio");
    add_button("quick mirror","open quick mirror","open", "quickmirror");
};
scheme("2、UC跑图")
{
    add_button("clickonptr","simulate click", "gmscript", "clickonptr()");
    add_button("iamgod","iamgod", "gmscript", "iamgod()");
    add_button("invincible","invincible", "gmscript", "invincible()");
    add_button("ironwall","ironwall", "gmscript", "ironwall()");
    add_input("attack","add attack power","500","gmscript","setattr(9,%1)");
};
scheme("3、NF构建")
{

};