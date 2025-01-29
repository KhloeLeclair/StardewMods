using System;
using System.Collections.Generic;

using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Patches;

internal static class SObject_Patches {

	private static ModEntry? Mod;

	internal static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(SObject), nameof(SObject.GetCategoryColor)),
				postfix: new HarmonyMethod(typeof(SObject_Patches), nameof(GetCategoryColor__Postfix))
			);

		} catch (Exception ex) {
			mod.Log($"Unable to apply SObject patches due to error.", LogLevel.Error, ex);
		}

	}

	internal static readonly Dictionary<int, string> CategoryNames = new() {
		{ -81, "Greens" },
		{ -2, "Gem" },
		{ -75, "Vegetable" },
		{ -4, "Fish" },
		{ -5, "Egg" },
		{ -6, "Milk" },
		{ -7, "Cooking" },
		{ -8, "Crafting" },
		{ -9, "BigCraftable" },
		{ -79, "Fruits" },
		{ -74, "Seeds" },
		{ -12, "Minerals" },
		{ -80, "Flowers" },
		{ -14, "Meat" },
		{ -19, "Fertilizer" },
		{ -20, "Junk" },
		{ -21, "Bait" },
		{ -22, "Tackle" },
		{ -24, "Furniture" },
		{ -25, "Ingredients" },
		{ -26, "ArtisanGoods" },
		{ -27, "Syrup" },
		{ -28, "MonsterLoot" },
		{ -29, "Equipment" },
		{ -94, "Clothing" },
		{ -95, "Hat" },
		{ -96, "Ring" },
		{ -97, "Boots" },
		{ -98, "Weapon" },
		{ -99, "Tool" },
		{ -100, "Clothing" },
		{ -101, "Trinket" },
		{ -102, "Books" },
		{ -103, "SkillBooks" },
		{ -999, "Litter" },

		{ -15, "Metal" },
		{ -16, "Building" },
		{ -17, "SellAtPierres" },
		{ -18, "SellAtPierresAndMarnies" },
		{ -23, "SellAtFishShop" }
	};


	private static void GetCategoryColor__Postfix(int category, ref Color __result) {
		if (Mod?.GameTheme is not null) {
			Color? color;
			if (CategoryNames.TryGetValue(category, out string? name))
				color = Mod.GameTheme.GetColorVariable($"Category:{name}");
			else
				color = Mod.GameTheme.GetColorVariable($"Category:{category}");

			if (color.HasValue)
				__result = color.Value;
		}
	}

}
