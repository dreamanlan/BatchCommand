// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.IO;

public class DebugScriptXcode : ModuleRules
{
	public DebugScriptXcode(ReadOnlyTargetRules Target) : base(Target)
	{
		// Only build on Apple platforms
		bEnableExceptions = true;

		// Restrict this module to Apple platforms only
		if (Target.Platform != UnrealTargetPlatform.Mac &&
			Target.Platform != UnrealTargetPlatform.IOS &&
			Target.Platform != UnrealTargetPlatform.TVOS)
		{
			// Skip compilation on non-Apple platforms
			Type = ModuleRules.ModuleType.External;
			return;
		}

		PublicDependencyModuleNames.AddRange(
			new string[] {
				"Core"
			}
		);

		PublicIncludePaths.AddRange(
			new string[] {
				Path.Combine(ModuleDirectory, "Public")
			}
		);
		
	// Add Metal framework for Apple platforms
	PublicFrameworks.AddRange(new string[] { "Metal", "MetalKit" });

	// Ensure .mm files are compiled as Objective-C++
	bEnableObjCAutomaticReferenceCounting = true;

}

}


