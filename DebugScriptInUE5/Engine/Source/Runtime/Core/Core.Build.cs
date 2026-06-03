// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System;
using System.IO;

public class Core : ModuleRules
{
	public Core(ReadOnlyTargetRules Target) : base(Target)
	{

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