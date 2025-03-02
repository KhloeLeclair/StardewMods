using System;

using HarmonyLib;

using StardewValley;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class GameLocation_Patches {

	private static ModEntry? ModEntry;

	internal static void Patch(ModEntry mod) {
		ModEntry = mod;

		try {
			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.openCraftingMenu)),
				prefix: new HarmonyMethod(typeof(GameLocation_Patches), nameof(openCraftingMenu_Prefix))
			);
		} catch (Exception ex) {
			mod.Log($"Error patching GameLocation. Game Menu may not be replaced when opening a crafting menu.", StardewModdingAPI.LogLevel.Error, ex);
		}
	}

	private static bool openCraftingMenu_Prefix() {
		return false;
	}

}
