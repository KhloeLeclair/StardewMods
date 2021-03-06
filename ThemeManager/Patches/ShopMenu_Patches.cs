using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;

using StardewValley.Menus;
using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Patches;

internal static class ShopMenu_Patches {

	private static ModEntry? Mod;
	private static IMonitor? Monitor;

	internal static void Patch(ModEntry mod) {
		Mod = mod;
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.draw), new Type[] {
					typeof(SpriteBatch)
				}),
				transpiler: new HarmonyMethod(typeof(ShopMenu_Patches), nameof(Draw_Transpiler))
			);

		} catch (Exception ex) {
			mod.Log("Unable to apply ShopMenu patches due to error.", LogLevel.Error, ex);
		}
	}

	public static Color GetSelectedColor() {
		return Mod?.BaseTheme?.ShopSelectedColor ?? Color.Wheat;
	}

	public static Color GetQiSelectedColor() {
		return Mod?.BaseTheme?.ShopQiSelectedColor ?? Color.Blue;
	}

	static IEnumerable<CodeInstruction> Draw_Transpiler(IEnumerable<CodeInstruction> instructions) {
		return PatchUtils.ReplaceColors(instructions, typeof(ShopMenu_Patches), new Dictionary<string, string> {
			{ nameof(Color.Wheat), nameof(GetSelectedColor) },
			{ nameof(Color.Blue), nameof(GetQiSelectedColor) },
		});
	}

}
