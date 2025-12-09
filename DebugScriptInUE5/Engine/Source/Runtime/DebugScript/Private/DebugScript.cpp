// Copyright Epic Games, Inc. All Rights Reserved.

#include "DebugScript.h"
#include "HAL/IConsoleManager.h"
#include "HAL/PlatformFilemanager.h"
#include "Misc/CoreDelegates.h"
#include "Misc/Paths.h"
#include "DebugScriptEntry.h"
#include <fstream>

#define LOCTEXT_NAMESPACE "FDebugScriptModule"

static FString SavedDir;

static FAutoConsoleCommand GDebugScriptToggleCommand(
    TEXT("DebugScript.Load"),
    TEXT("Load debug script"),
    FConsoleCommandDelegate::CreateStatic([]()
        {
            LoadDbgScp(SavedDir, SavedDir);
        })
);

static FAutoConsoleCommand GDebugScriptEnableCommand(
    TEXT("DebugScript.Pause"),
    TEXT("Pause debug script"),
    FConsoleCommandDelegate::CreateStatic([]()
        {
            PauseDbgScp();
        })
);

static FAutoConsoleCommand GDebugScriptDisableCommand(
    TEXT("DebugScript.Resume"),
    TEXT("Resume debug script"),
    FConsoleCommandDelegate::CreateStatic([]()
        {
            ResumeDbgScp();
        })
);

static FAutoConsoleCommandWithArgsAndOutputDevice GDebugScriptSetCommand(
    TEXT("DebugScript.Set"),
    TEXT("Set debug script parameters: cmd a b c"),
    FConsoleCommandWithArgsAndOutputDeviceDelegate::CreateStatic([](const TArray<FString>& Args, FOutputDevice& Ar)
        {
            int32 cmd = 0;
            int32 a = 0;
            double b = 0.0;
            FString c = TEXT("");
            
            // Parse parameters with default values
            if (Args.Num() > 0)
            {
                cmd = FCString::Atoi(*Args[0]);
            }
            if (Args.Num() > 1)
            {
                a = FCString::Atoi(*Args[1]);
            }
            if (Args.Num() > 2)
            {
                b = FCString::Atod(*Args[2]);
            }
            if (Args.Num() > 3)
            {
                c = Args[3];
            }
            
            DbgScp_Set_Extern(cmd, a, b, TCHAR_TO_UTF8(*c));
            UE_LOG(LogTemp, Log, TEXT("DebugScript.Set called with cmd=%d, a=%d, b=%f, c=%s"), cmd, a, b, *c);
        })
);

static FAutoConsoleCommandWithArgsAndOutputDevice GDebugScriptGetCommand(
    TEXT("DebugScript.Get"),
    TEXT("Get debug script parameters: cmd a b c"),
    FConsoleCommandWithArgsAndOutputDeviceDelegate::CreateStatic([](const TArray<FString>& Args, FOutputDevice& Ar)
        {
            int32 cmd = 0;
            int32 a = 0;
            double b = 0.0;
            FString c = TEXT("");
            
            // Parse parameters with default values
            if (Args.Num() > 0)
            {
                cmd = FCString::Atoi(*Args[0]);
            }
            if (Args.Num() > 1)
            {
                a = FCString::Atoi(*Args[1]);
            }
            if (Args.Num() > 2)
            {
                b = FCString::Atod(*Args[2]);
            }
            if (Args.Num() > 3)
            {
                c = Args[3];
            }
            
            int ret = DbgScp_Get_Extern(cmd, a, b, TCHAR_TO_UTF8(*c));
            // Log the return values
            UE_LOG(LogTemp, Log, TEXT("DebugScript.Get returning %d cmd=%d, a=%d, b=%f, c=%s"), ret, cmd, a, b, *c);
        })
);

void FDebugScriptModule::StartupModule()
{
    UE_LOG(LogTemp, Log, TEXT("DebugScript module starting up"));
    
    SavedDir = FPaths::ProjectSavedDir();
    if (SavedDir.IsEmpty())
    {
        SavedDir = FPaths::Combine(FPaths::LaunchDir(), TEXT("Saved"));
        UE_LOG(LogTemp, Log, TEXT("DebugScript SavedDir %s, based LaunchDir"), *SavedDir);
    }
    else
    {
        UE_LOG(LogTemp, Log, TEXT("DebugScript SavedDir %s, based ProjectSavedDir"), *SavedDir);
    }
    IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();
    if (FPaths::IsRelative(SavedDir))
    {
        SavedDir = PlatformFile.ConvertToAbsolutePathForExternalAppForWrite(*SavedDir);
    }
    UE_LOG(LogTemp, Log, TEXT("DebugScript SavedDir %s, ConvertToAbsolutePathForExternalAppForWrite"), *SavedDir);

#if PLATFORM_ANDROID
    std::ifstream CheckFile("/data/local/tmp/bytecode.dat", std::ios::in | std::ios::binary);
#else
    auto&& dataPath = FPaths::Combine(SavedDir, TEXT("bytecode.dat"));
    std::ifstream CheckFile(TCHAR_TO_UTF8(*dataPath), std::ios::in | std::ios::binary);
#endif
    if (CheckFile.good()) {
        CheckFile.close();

        LoadDbgScp(SavedDir, SavedDir);
    }

    UE_LOG(LogTemp, Log, TEXT("DebugScript module initialized"));

    // Register for engine initialization complete
    FCoreDelegates::OnFEngineLoopInitComplete.AddLambda([]()
        {
            UE_LOG(LogTemp, Log, TEXT("DebugScript module OnFEngineLoopInitComplete, SavedDir=%s"), *SavedDir);
        });
}

void FDebugScriptModule::ShutdownModule()
{
    UE_LOG(LogTemp, Log, TEXT("DebugScript module shutting down"));
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(FDebugScriptModule, DebugScript)
