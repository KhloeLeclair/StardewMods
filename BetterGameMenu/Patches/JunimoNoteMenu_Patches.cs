using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using StardewModdingAPI;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class JunimoNoteMenu_Patches {

	private static ModEntry? Mod;

	internal static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(JunimoNoteMenu), "cleanupBeforeExit"),
				transpiler: new HarmonyMethod(typeof(JunimoNoteMenu_Patches), nameof(cleanupBeforeExit__Transpiler))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching JunimoNoteMenu: {ex}", LogLevel.Error);
		}

	}

	private static IEnumerable<CodeInstruction> cleanupBeforeExit__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameMenu_Ctor_Other = AccessTools.Constructor(typeof(GameMenu), [typeof(int), typeof(int), typeof(bool)]);
		var Our_Other = AccessTools.Method(typeof(Game1_Patches), nameof(Game1_Patches.CreateMenuOther));

		var matcher = new CodeMatcher(instructions)
			.MatchStartForward(
				new CodeMatch(OpCodes.Newobj, GameMenu_Ctor_Other)
			)
			.ThrowIfInvalid("could not find new GameMenu")
			.Set(OpCodes.Call, Our_Other);

		return matcher.InstructionEnumeration();

	}

}
