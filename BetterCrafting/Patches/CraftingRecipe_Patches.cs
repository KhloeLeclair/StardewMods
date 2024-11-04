using System;

using HarmonyLib;

using StardewModdingAPI;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Patches;

public static class CraftingRecipe_Patches {

	private static IMonitor? Monitor;

	public static void Patch(ModEntry mod) {
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.getSpriteIndexFromRawIndex)),
				postfix: new HarmonyMethod(typeof(CraftingRecipe_Patches), nameof(getSpriteIndexFromRawIndex__Postfix))
			);

			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.getNameFromIndex)),
				postfix: new HarmonyMethod(typeof(CraftingRecipe_Patches), nameof(getNameFromIndex__Postfix))
			);

		} catch (Exception ex) {
			mod.Log("An error occurred while registering a harmony patch for the vanilla CraftingPage.", LogLevel.Warn, ex);
		}
	}

	public static void getNameFromIndex__Postfix(CraftingRecipe __instance, ref string __result) {
		try {
			__result = __result switch {
				"-7" => I18n.Item_Category_Cooking(),
				"-8" => I18n.Item_Category_Crafting(),
				"-12" => I18n.Item_Category_Mineral(),
				"-14" => I18n.Item_Category_Meat(),
				"-19" => I18n.Item_Category_Fertilizer(),
				"-20" => I18n.Item_Category_Junk(),
				"-74" => I18n.Item_Category_Seeds(),
				"-75" => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570"),
				"-79" => I18n.Item_Category_Fruit(),
				"-80" => I18n.Item_Category_Flower(),
				"-81" => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568"),
				_ => __result
			};

		} catch (Exception ex) {
			Monitor?.LogOnce($"An error occurred while attempting to fix the display of category ingredients in the vanilla menu. Somehow. Details:\n{ex}", LogLevel.Warn);
		}
	}

	public static void getSpriteIndexFromRawIndex__Postfix(CraftingRecipe __instance, ref string __result) {

		try {
			__result = __result switch {
				//"-7" => "(O)662", // Cooking => unused
				"-8" => "(O)298", // Crafting => Hardwood Fence
				"-12" => "(O)546", // Minerals => Geminite
								   //"-14" => "(O)640", // Meat => unused
				"-19" => "(O)465", // Fertilizer => Speed-Gro
				"-20" => "(O)168", // Junk => Trash
				"-74" => "(O)472", // Seeds => Parsnip Seeds
				"-75" => "(O)24",  // Vegetable => Parsnip
				"-79" => "(O)406", // Fruits => Plum
				"-80" => "(O)591", // Flowers => Tulip
				"-81" => "(O)20",  // Greens => Leek
				_ => __result
			};

		} catch (Exception ex) {
			Monitor?.LogOnce($"An error occurred while attempting to fix the display of category ingredients in the vanilla menu. Somehow. Details:\n{ex}", LogLevel.Warn);
		}

	}

}
