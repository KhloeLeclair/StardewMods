using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class Farmer_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {
		Mod = mod;

		try {

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Farmer), nameof(Farmer.Update)),
				transpiler: new HarmonyMethod(typeof(Farmer_Patches), nameof(Update__Transpiler))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching Farmer. Game Menu may not close correctly when dying.", StardewModdingAPI.LogLevel.Error, ex);
		}
	}

	private static IEnumerable<CodeInstruction> Update__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameMenu = typeof(GameMenu);
		var get_activeClickableMenu = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.activeClickableMenu));

		var BetterGameMenu = typeof(Menus.BetterGameMenuImpl);

		// Old Code: if (Game1.activeClickableMenu is GameMenu) {
		// New Code: if (Game1.activeClickableMenu is GameMenu || Game1.activeClickableMenu is BetterGameMenu) {
		var matcher = new CodeMatcher(instructions)
			.MatchEndForward(
				new CodeMatch(OpCodes.Call, get_activeClickableMenu),
				new CodeMatch(OpCodes.Isinst, GameMenu),
				new CodeMatch(OpCodes.Brfalse_S)
			)
			.ThrowIfInvalid("could not find Game1.activeClickableMenu is GameMenu")
			.Advance(-1)
			.Insert(
				new CodeInstruction(OpCodes.Call, get_activeClickableMenu),
				new CodeInstruction(OpCodes.Isinst, BetterGameMenu),
				new CodeInstruction(OpCodes.Or)
			);

		return matcher.InstructionEnumeration();

	}

}
