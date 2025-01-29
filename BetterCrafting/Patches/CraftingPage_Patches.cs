using System;
using System.Collections.Generic;

using HarmonyLib;

using StardewModdingAPI;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Patches;

public static class CraftingPage_Patches {

	private static IMonitor? Monitor;

	public static void Patch(ModEntry mod) {
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(CraftingPage), "GetRecipesToDisplay"),
				postfix: new HarmonyMethod(typeof(CraftingPage_Patches), nameof(GetRecipesToDisplay_Postfix))
			);

		} catch (Exception ex) {
			mod.Log("An error occurred while registering a harmony patch for the vanilla CraftingPage.", LogLevel.Warn, ex);
		}
	}

	public static void GetRecipesToDisplay_Postfix(CraftingPage __instance, ref List<string> __result) {

		try {
			var exclusives = ModEntry.Instance?.Stations?.GetExclusiveRecipes(__instance.cooking);
			if (exclusives != null)
				foreach (string recipe in exclusives)
					__result.Remove(recipe);

		} catch (Exception ex) {
			Monitor?.Log($"An error occurred while attempting to filter recipes from a CraftingPage.", LogLevel.Warn);
			Monitor?.Log($"Details:\n{ex}", LogLevel.Warn);
		}

	}

}
