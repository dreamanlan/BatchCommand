script(init)
{
    output_log("[dsl] init finish");
    file_echo(true);
    return(0);
};

script(loadsetting)
{
    native_api.AddSettingItem("test_exe","test","just test",0,"test.exe","*.exe","");
    native_api.AddSettingItem("test2_exe","test2","just test",0,"test.exe","*.exe","https://github.com/dreamanlan/BatchCommand");
    native_api.AddSettingItem("test3","test3","just test",1,"123","","");
    native_api.AddSettingItem("test4","test4","just test",2,"234","","");
    native_api.AddSettingItem("test5","test5","just test",3,"345","","");
    native_api.AddSettingItem("test6","test6","just test",4,"456","","");
};

script(loadschememenu)
{
    native_api.LoadSchemeMenu();
};

script(loadscheme)params($path)
{
    native_api.LoadScheme($path);
};

// -1 -- failed 0 -- nothing was done 1 -- finished
script(executecommand)params($cmdType, $cmdArgs)
{
    $javaExe = native_api.GetJavaExe();
    $adbExe = native_api.GetAdbExe();
    $zipalign = native_api.GetZipAlignExe();
    $javaHeap = native_api.GetJavaHeap();
    if (is_null_or_empty($javaExe) || is_null_or_empty($adbExe) || is_null_or_empty($zipalign)) {
        return(-1);
    };
    if ($cmdType=="adb") {
        $args = format("{0}", $cmdArgs);
        $r = native_api.RunCommand($adbExe, $args);
        if (!$r) {
            output_log("adb failed:{0}", native_api.GetResultCode());
            return(-1);
        };
    }
    elseif ($cmdType=="gmscript") {
        $args = format("shell am broadcast -a com.unity3d.command -e cmd '{0}'", $cmdArgs);
        $r = native_api.RunCommand($adbExe, $args);
        if (!$r) {
            output_log("adb failed:{0}", native_api.GetResultCode());
            return(-1);
        };
    }
    elseif ($cmdType=="buildengine") {
        if ($cmdArgs=="dbgscp") {
            file_echo(false);
            command{
                win{:e:/UGit/unity_engine/buildall.bat:};
                nowait(true);
                useshellexecute(true);
            };
            file_echo(true);
        };
    }
    elseif ($cmdType=="copyengine") {
        clear_script_console();
        if ($cmdArgs=="dev") {
            copy_dir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Development/Libs","E:/AndroidPlayerDebug/dev_memdbg/Libs");
            copy_dir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Development/Symbols","E:/AndroidPlayerDebug/dev_memdbg/Symbols");
        }
        elseif ($cmdArgs=="rel") {
            copy_dir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Release/Libs","E:/AndroidPlayerDebug/rel_memdbg/Libs");
            copy_dir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Release/Symbols","E:/AndroidPlayerDebug/rel_memdbg/Symbols");
        };
        log_script_console();
    }
    elseif ($cmdType=="unzipsym") {
        if ($cmdArgs=="testuc") {
            $args = format("x {0}/{1} -o{0} -y", "e:/UGit/TestUC", "testuc-0.1-v1-IL2CPP.symbols.zip");
            $r = native_api.RunCommand("c:/Program Files/7-Zip/7z.exe", $args);
            if (!$r) {
                output_log("unzip sym failed:{0}", native_api.GetResultCode());
                return(-1);
            };
        };
    }
    elseif ($cmdType=="open") {
        if ($cmdArgs=="testuc") {
            process("e:/UGit/unity_engine/build/WindowsEditor/x64/Release/Unity.exe", "-projectPath e:/UGit/TestUC"){
                nowait(true);
            };
        }
        elseif ($cmdArgs=="androidstudio") {
            process("C:/Program Files/Android/Android Studio/bin/studio64.exe", ""){
                nowait(true);
            };
        }
        elseif ($cmdArgs=="quickmirror") {
            process("c:/Users/dreamanlan/AppData/Local/QuickMirror/QuickMirror.exe", ""){
                nowait(true);
            };
        };
    };
    output_log("[dsl] execute command success");
    return(1);
};

script(runprog)
{
    $path = get_directory_name(selInTree);
    $filename = get_file_name(selInTree);

    output_log("[dsl] running program success");
    return(1);
};

script(build)
{
    $path = get_directory_name(selInTree);
    $filename = get_file_name(selInTree);
    if ($filename=="hook.txt") {
        build_dbg_scp($path);
        $adbExe = native_api.GetAdbExe();
        if (is_null_or_empty($adbExe)) {
            return(-1);
        };
        $args = format("push {0}/bytecode.dat /data/local/tmp/", $path);
        $r = native_api.RunCommand($adbExe, $args);
        if (!$r) {
            output_log("build failed:{0}", native_api.GetResultCode());
            return(-1);
        };
        //native_api.ShowWindowsConsole();
    };
    output_log("[dsl] build success");
    return(1);
};

script(install)
{
    $filename = get_file_name(selInTree);
    $ext = get_extension(selInTree);
    if (dir_exist(selInTree)) {
        if ($filename=="AssetsBundle") {
            $adbExe = native_api.GetAdbExe();
            if (is_null_or_empty($adbExe)) {
                return(-1);
            };
            $args = format("push {0} /sdcard/Android/data/com.DefaultCompany.TestUC05/files/", selInTree);
            $r = native_api.RunCommand($adbExe, $args);
            if (!$r) {
                output_log("install failed:{0}", native_api.GetResultCode());
                return(-1);
            };
        };
    }
    else{
        if ($ext==".apk") {
            $adbExe = native_api.GetAdbExe();
            if (is_null_or_empty($adbExe)) {
                return(-1);
            };
            $args = format("install {0}", selInTree);
            $r = native_api.RunCommand($adbExe, $args);
            if (!$r) {
                output_log("install failed:{0}", native_api.GetResultCode());
                return(-1);
            };
        }
        elseif ($ext==".so") {
            $adbExe = native_api.GetAdbExe();
            if (is_null_or_empty($adbExe)) {
                return(-1);
            };
            $args = format("shell mkdir /sdcard/Android/data/com.tencent.uc/files/ExtraFiles");
            $r = native_api.RunCommand($adbExe, $args);
            $args = format("shell mkdir /sdcard/Android/data/com.tencent.uc/files/ExtraFiles/arm64-v8a");
            $r = native_api.RunCommand($adbExe, $args);
            $args = format("push {0} /sdcard/Android/data/com.tencent.uc/files/ExtraFiles/arm64-v8a/", selInTree);
            $r = native_api.RunCommand($adbExe, $args);
            if (!$r) {
                output_log("install failed:{0}", native_api.GetResultCode());
                return(-1);
            };
        }
        else {
            $adbExe = native_api.GetAdbExe();
            if (is_null_or_empty($adbExe)) {
                return(-1);
            };
            $args = format("push {0} /data/local/tmp/", selInTree);
            $r = native_api.RunCommand($adbExe, $args);
            if (!$r) {
                output_log("install failed:{0}", native_api.GetResultCode());
                return(-1);
            };
        };
    };
    output_log("[dsl] install success");
    return(1);
};
