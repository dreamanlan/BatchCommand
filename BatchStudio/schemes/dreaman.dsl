scheme("dreaman/测试")
{
    add_button("clickonptr","simulate click", "gmscript", "clickonptr()");
    add_input("add_atk","add attack power","500","gmscript","setattr(9,%1)");
    add_button("dev_engine","copy dev engine","copyengine", "dev");
    add_button("rel_engine","copy release engine","copyengine", "rel");
};