using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Patches;

internal static class IClickableMenu_Patches {

	private static ModEntry? Mod;
	private static IMonitor? Monitor;

	internal static void Patch(ModEntry mod) {
		Mod = mod;
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText), new Type[] {
					typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont),
					typeof(int), typeof(int), typeof(int), typeof(string), typeof(int),
					typeof(string[]), typeof(Item),
					typeof(int), typeof(int), typeof(int), typeof(int), typeof(int),
					typeof(float), typeof(CraftingRecipe), typeof(IList<Item>)
				}),
				transpiler: new HarmonyMethod(typeof(IClickableMenu_Patches), nameof(DrawHoverText_Transpiler))
			);

		} catch (Exception ex) {
			mod.Log("Unable to apply IClickableMenu patches due to error.", LogLevel.Error, ex);
		}
	}

	public static Color GetForgeCountTextColor() {
		return Mod?.BaseTheme?.HoverTextForgeCountTextColor ?? Color.DimGray;
	}

	public static Color GetForgedTextColor() {
		return Mod?.BaseTheme?.HoverTextForgedTextColor ?? Color.DarkRed;
	}

	static IEnumerable<CodeInstruction> DrawHoverText_Transpiler(IEnumerable<CodeInstruction> instructions) {

		return PatchUtils.ReplaceColors(
			instructions: instructions,
			type: typeof(IClickableMenu_Patches),
			replacements: new Dictionary<string, string> {
				{ nameof(Color.DimGray), nameof(GetForgeCountTextColor) },
				{ nameof(Color.DarkRed), nameof(GetForgedTextColor) }
			}
		);

	}

}
