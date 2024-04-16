using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Leclair.Stardew.MoreNightlyEvents.Patches;

public static class FruitTree_Patches {

	private static ModEntry? Mod;
	private static IMonitor? Monitor;

	internal static bool IsSpawningTree = false;

	internal static void Patch(ModEntry mod) {
		Mod = mod;
		Monitor = mod.Monitor;

		try {

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.IgnoresSeasonsHere)),
				postfix: new HarmonyMethod(typeof(FruitTree_Patches), nameof(IgnoresSeasonsHere__Postfix))
			);

		} catch (Exception ex) {
			mod.Log($"Unable to apply Utility patches due to an error. This mod will not function.", LogLevel.Error, ex);
		}
	}

	internal static void IgnoresSeasonsHere__Postfix(FruitTree __instance, ref bool __result) {
		if (IsSpawningTree)
			__result = true;
		else if (__instance.modData.TryGetValue(ModEntry.FRUIT_TREE_SEASON_DATA, out string? value) && value != null && value.Equals("true", StringComparison.OrdinalIgnoreCase))
			__result = true;
	}

}
