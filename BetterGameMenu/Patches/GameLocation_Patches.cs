using System;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class GameLocation_Patches {

	private static ModEntry? Mod;

	internal static void Patch(ModEntry mod) {
		Mod = mod;

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
		if (Mod is not null && Mod.IsEnabled)
			try {
				var menu = new BetterGameMenuImpl(Mod, nameof(VanillaTabOrders.Crafting));
				Game1.activeClickableMenu = menu;
				return false;

			} catch (Exception ex) {
				Mod?.Log($"Error in openCraftingMenu prefix: {ex}", StardewModdingAPI.LogLevel.Error);
			}

		return true;
	}

}
