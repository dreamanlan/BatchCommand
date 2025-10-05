script(init)
{
    outputlog("[dsl] init finish");
    fileecho(true);
    return(0);
};

script(loadsetting)
{
    nativeapi.AddSettingItem("test_exe","test","just test",0,"test.exe","*.exe","");
    nativeapi.AddSettingItem("test2_exe","test2","just test",0,"test.exe","*.exe","https://github.com/dreamanlan/BatchCommand");
    nativeapi.AddSettingItem("test3","test3","just test",1,"123","","");
    nativeapi.AddSettingItem("test4","test4","just test",2,"234","","");
    nativeapi.AddSettingItem("test5","test5","just test",3,"345","","");
    nativeapi.AddSettingItem("test6","test6","just test",4,"456","","");
};

script(loadschememenu)
{
    nativeapi.LoadSchemeMenu();
};

script(loadscheme)args($path)
{
    nativeapi.LoadScheme($path);
};

// -1 -- failed 0 -- nothing was done 1 -- finished
script(executecommand)args($cmdType, $cmdArgs)
{
    $javaExe = nativeapi.GetJavaExe();
    $adbExe = nativeapi.GetAdbExe();
    $zipalign = nativeapi.GetZipAlignExe();
    $javaHeap = nativeapi.GetJavaHeap();
    if (isnullorempty($javaExe) || isnullorempty($adbExe) || isnullorempty($zipalign)) {
        return(-1);
    };
    if ($cmdType=="adb") {
        $args = format("{0}", $cmdArgs);
        $r = nativeapi.RunCommand($adbExe, $args);
        if (!$r) {
            outputlog("adb failed:{0}", nativeapi.GetResultCode());
            return(-1);
        };
    }
    elseif ($cmdType=="gmscript") {
        $args = format("shell am broadcast -a com.unity3d.command -e cmd '{0}'", $cmdArgs);
        $r = nativeapi.RunCommand($adbExe, $args);
        if (!$r) {
            outputlog("adb failed:{0}", nativeapi.GetResultCode());
            return(-1);
        };
    }
    elseif ($cmdType=="buildengine") {
        if ($cmdArgs=="dbgscp") {
            fileecho(false);
            command{
                win{:e:/UGit/unity_engine/buildall.bat:};
                nowait(true);
                useshellexecute(true);
            };
            fileecho(true);
        };
    }
    elseif ($cmdType=="copyengine") {
        clrscpcon();
        if ($cmdArgs=="dev") {
            copydir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Development/Libs","E:/AndroidPlayerDebug/dev_memdbg/Libs");
            copydir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Development/Symbols","E:/AndroidPlayerDebug/dev_memdbg/Symbols");
        }
        elseif ($cmdArgs=="rel") {
            copydir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Release/Libs","E:/AndroidPlayerDebug/rel_memdbg/Libs");
            copydir("E:/UGit/unity_engine/build/AndroidPlayer/Variations/il2cpp/Release/Symbols","E:/AndroidPlayerDebug/rel_memdbg/Symbols");
        };
        logscpcon();
    }
    elseif ($cmdType=="unzipsym") {
        if ($cmdArgs=="testuc") {
            $args = format("x {0}/{1} -o{0} -y", "e:/UGit/TestUC", "testuc-0.1-v1-IL2CPP.symbols.zip");
            $r = nativeapi.RunCommand("c:/Program Files/7-Zip/7z.exe", $args);
            if (!$r) {
                outputlog("unzip sym failed:{0}", nativeapi.GetResultCode());
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
    outputlog("[dsl] execute command success");
    return(1);
};

script(runprog)
{
    $path = getdirectoryname(selInTree);
    $filename = getfilename(selInTree);

    outputlog("[dsl] running program success");
    return(1);
};

script(build)
{
    $path = getdirectoryname(selInTree);
    $filename = getfilename(selInTree);
    if ($filename=="hook.txt") {
        builddbgscp($path);
        $adbExe = nativeapi.GetAdbExe();
        if (isnullorempty($adbExe)) {
            return(-1);
        };
        $args = format("push {0}/bytecode.dat /data/local/tmp/", $path);
        $r = nativeapi.RunCommand($adbExe, $args);
        if (!$r) {
            outputlog("build failed:{0}", nativeapi.GetResultCode());
            return(-1);
        };
        nativeapi.ShowWindowsConsole();
    };
    outputlog("[dsl] build success");
    return(1);
};

script(install)
{
    $filename = getfilename(selInTree);
    $ext = getextension(selInTree);
    if (direxist(selInTree)) {
        if ($filename=="AssetsBundle") {
            $adbExe = nativeapi.GetAdbExe();
            if (isnullorempty($adbExe)) {
                return(-1);
            };
            $args = format("push {0} /sdcard/Android/data/com.DefaultCompany.TestUC05/files/", selInTree);
            $r = nativeapi.RunCommand($adbExe, $args);
            if (!$r) {
                outputlog("install failed:{0}", nativeapi.GetResultCode());
                return(-1);
            };
        };
    }
    else{
        if ($ext==".apk") {
            $adbExe = nativeapi.GetAdbExe();
            if (isnullorempty($adbExe)) {
                return(-1);
            };
            $args = format("install {0}", selInTree);
            $r = nativeapi.RunCommand($adbExe, $args);
            if (!$r) {
                outputlog("install failed:{0}", nativeapi.GetResultCode());
                return(-1);
            };
        }
        elseif ($ext==".so") {
            $adbExe = nativeapi.GetAdbExe();
            if (isnullorempty($adbExe)) {
                return(-1);
            };
            $args = format("shell mkdir /sdcard/Android/data/com.tencent.uc/files/ExtraFiles");
            $r = nativeapi.RunCommand($adbExe, $args);
            $args = format("shell mkdir /sdcard/Android/data/com.tencent.uc/files/ExtraFiles/arm64-v8a");
            $r = nativeapi.RunCommand($adbExe, $args);
            $args = format("push {0} /sdcard/Android/data/com.tencent.uc/files/ExtraFiles/arm64-v8a/", selInTree);
            $r = nativeapi.RunCommand($adbExe, $args);
            if (!$r) {
                outputlog("install failed:{0}", nativeapi.GetResultCode());
                return(-1);
            };
        }
        else {
            $adbExe = nativeapi.GetAdbExe();
            if (isnullorempty($adbExe)) {
                return(-1);
            };
            $args = format("push {0} /data/local/tmp/", selInTree);
            $r = nativeapi.RunCommand($adbExe, $args);
            if (!$r) {
                outputlog("install failed:{0}", nativeapi.GetResultCode());
                return(-1);
            };
        };
    };
    outputlog("[dsl] install success");
    return(1);
};
