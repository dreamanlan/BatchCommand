// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class DebugScript : ModuleRules
{
	public DebugScript(ReadOnlyTargetRules Target) : base(Target)
	{
		PublicDependencyModuleNames.AddRange(
			new string[] {
				"Core"
			}
		);

		// On Apple platforms, depend on DebugScriptXcode for Metal capture support
		if (Target.Platform == UnrealTargetPlatform.Mac ||
			Target.Platform == UnrealTargetPlatform.IOS ||
			Target.Platform == UnrealTargetPlatform.TVOS)
		{
			PublicDependencyModuleNames.Add("DebugScriptXcode");
		}

		PublicDefinitions.AddRange(
			new string[]
			{
				"DEBUGSCRIPT_ENABLED=1",
				"DEBUGSCRIPT_VERBOSE=0"
			});
	}
}