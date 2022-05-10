using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;

using StardewValley.Menus;
using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Patches;

internal static class Billboard_Patches {

	private static ModEntry? Mod;
	private static IMonitor? Monitor;

	internal static void Patch(ModEntry mod) {
		Mod = mod;
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(Billboard), nameof(Billboard.draw), new Type[] {
					typeof(SpriteBatch)
				}),
				transpiler: new HarmonyMethod(typeof(Billboard_Patches), nameof(Draw_Transpiler))
			);

		} catch (Exception ex) {
			mod.Log("Unable to apply Billboard patches due to error.", LogLevel.Error, ex);
		}
	}

	public static Color GetDimColor() {
		return Mod?.BaseTheme?.CalendarDimColor ?? Color.Gray;
	}

	public static Color GetTodayColor() {
		return Mod?.BaseTheme?.CalendarTodayColor ?? Color.Blue;
	}

	public static Color GetHoverColor() {
		return Mod?.BaseTheme?.BillboardHoverColor ?? Mod?.BaseTheme?.ButtonHoverColor ?? Color.LightPink;
	}

	static IEnumerable<CodeInstruction> Draw_Transpiler(IEnumerable<CodeInstruction> instructions) {
		return PatchUtils.ReplaceColors(instructions, typeof(Billboard_Patches), new Dictionary<string, string> {
			{ nameof(Color.Gray), nameof(GetDimColor) },
			{ nameof(Color.Blue), nameof(GetTodayColor) },
			{ nameof(Color.LightPink), nameof(GetHoverColor) },
		});
	}

}
