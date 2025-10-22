// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class DebugScript : ModuleRules
{
    public DebugScript(ReadOnlyTargetRules Target) : base(Target)
    {
        PublicDependencyModuleNames.AddRange(
            new string[] {
                "Core",
                "CoreUObject",
                "Engine"
            }
        );

        PublicDefinitions.AddRange(
            new string[]
            {
                "DEBUGSCRIPT_ENABLED=1",
                "DEBUGSCRIPT_VERBOSE=0"
            });
    }
}