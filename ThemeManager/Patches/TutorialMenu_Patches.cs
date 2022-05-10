using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using HarmonyLib;

using StardewValley.Menus;
using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Patches;

internal static class TutorialMenu_Patches {

	private static ModEntry? Mod;
	private static IMonitor? Monitor;

	internal static void Patch(ModEntry mod) {
		Mod = mod;
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(TutorialMenu), nameof(TutorialMenu.draw), new Type[] {
					typeof(SpriteBatch)
				}),
				transpiler: new HarmonyMethod(typeof(TutorialMenu_Patches), nameof(Draw_Transpiler))
			);

		} catch (Exception ex) {
			mod.Log("Unable to apply TutorialMenu patches due to error.", LogLevel.Error, ex);
		}
	}

	public static Color GetHoverColor() {
		return Mod?.BaseTheme?.TutorialHoverColor ?? Color.Blue;
	}

	static IEnumerable<CodeInstruction> Draw_Transpiler(IEnumerable<CodeInstruction> instructions) {
		return PatchUtils.ReplaceColors(
			instructions: instructions,
			type: typeof(TutorialMenu_Patches),
			replacements: new Dictionary<string, string> {
				{ nameof(Color.Blue), nameof(GetHoverColor) }
			}
		);
	}

}
