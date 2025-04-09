using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class IClickableMenu_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {
		Mod = mod;

		try {

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.exitThisMenu)),
				transpiler: new HarmonyMethod(typeof(IClickableMenu_Patches), nameof(exitThisMenu__Transpiler))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.populateClickableComponentList)),
				postfix: new HarmonyMethod(typeof(IClickableMenu_Patches), nameof(populateClickableComponentList__Postfix))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching IClickableMenu. Game Menu may not behave correctly.", StardewModdingAPI.LogLevel.Error, ex);
		}
	}

	#region Helper Methods

	private static bool IsCurrentPage(IClickableMenu menu) {
		if (Game1.activeClickableMenu == menu)
			return true;
		if (Game1.activeClickableMenu is GameMenu gm)
			return gm.GetCurrentPage() == menu;
		if (Game1.activeClickableMenu is BetterGameMenuImpl bgm)
			return bgm.CurrentPage == menu;
		return false;
	}

	#endregion

	private static void populateClickableComponentList__Postfix(IClickableMenu __instance) {
		if (Game1.activeClickableMenu is BetterGameMenuImpl bgm && bgm.CurrentPage == __instance)
			bgm.AddTabsToClickableComponents(__instance);
	}

	private static IEnumerable<CodeInstruction> exitThisMenu__Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {
		var tGameMenu = typeof(GameMenu);
		var get_activeClickableMenu = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.activeClickableMenu));
		var mGetCurrentPage = AccessTools.Method(tGameMenu, nameof(GameMenu.GetCurrentPage));

		var mIsCurrentPage = AccessTools.Method(typeof(IClickableMenu_Patches), nameof(IsCurrentPage));

		var matcher = new CodeMatcher(instructions, generator)
			.MatchStartForward(
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(OpCodes.Call, get_activeClickableMenu),
				new CodeMatch(in0 => in0.Branches(out var _))
			)
			.ThrowIfInvalid("could not find check")
			.Advance(1)
			.SetAndAdvance(OpCodes.Call, mIsCurrentPage)
			.SetOpcodeAndAdvance(OpCodes.Brfalse_S);

		return matcher
			.InstructionEnumeration();
	}

}
