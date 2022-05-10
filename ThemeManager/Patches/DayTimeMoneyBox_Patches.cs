using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;

using StardewValley.Menus;
using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Patches;

internal static class DayTimeMoneyBox_Patches {

	private static ModEntry? Mod;
	private static IMonitor? Monitor;

	internal static void Patch(ModEntry mod) {
		Mod = mod;
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), new Type[] {
					typeof(SpriteBatch)
				}),
				transpiler: new HarmonyMethod(typeof(DayTimeMoneyBox_Patches), nameof(Draw_Transpiler))
			);

		} catch (Exception ex) {
			mod.Log("Unable to apply DayTimeMoneyBox patches due to error.", LogLevel.Error, ex);
		}
	}

	public static Color GetAfterMidnightColor() {
		return Mod?.BaseTheme?.DayTimeAfterMidnightColor ?? Mod?.BaseTheme?.ErrorTextColor ?? Color.Red;
	}

	static IEnumerable<CodeInstruction> Draw_Transpiler(IEnumerable<CodeInstruction> instructions) {
		return PatchUtils.ReplaceColors(
			instructions: instructions,
			type: typeof(DayTimeMoneyBox_Patches),
			replacements: new Dictionary<string, string> {
				{ nameof(Color.Red), nameof(GetAfterMidnightColor) }
			}
		);
	}

}
