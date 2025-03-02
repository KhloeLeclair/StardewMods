using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

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
		Mod?.Log($"Checking IsCurrentPage {menu.GetType().FullName}", StardewModdingAPI.LogLevel.Debug);
		return Game1.activeClickableMenu is Menus.BetterGameMenuImpl bgm && bgm.CurrentPage == menu;
	}

	#endregion

	private static void populateClickableComponentList__Postfix(IClickableMenu __instance) {
		if (Game1.activeClickableMenu is Menus.BetterGameMenuImpl bgm && bgm.CurrentPage == __instance)
			bgm.AddTabsToClickableComponents(__instance);
	}

	private static IEnumerable<CodeInstruction> exitThisMenu__Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {

		var tGameMenu = typeof(GameMenu);
		var get_activeClickableMenu = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.activeClickableMenu));
		var mGetCurrentPage = AccessTools.Method(tGameMenu, nameof(GameMenu.GetCurrentPage));

		var mIsCurrentPage = AccessTools.Method(typeof(IClickableMenu_Patches), nameof(IsCurrentPage));

		var BetterGameMenu = typeof(Menus.BetterGameMenuImpl);

		var matcher = new CodeMatcher(instructions, generator)
			.MatchEndForward(
				new CodeMatch(OpCodes.Call, get_activeClickableMenu),
				new CodeMatch(OpCodes.Isinst, tGameMenu),
				new CodeMatch(in0 => in0.IsStloc()),
				new CodeMatch(in0 => in0.IsLdloc()),
				new CodeMatch(in0 => in0.Branches(out var _)),
				new CodeMatch(in0 => in0.IsLdloc()),
				new CodeMatch(in0 => in0.Calls(mGetCurrentPage)),
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(in0 => in0.Branches(out var _))
			)
			.ThrowIfInvalid("could not find GameMenu check")
			.Advance(1)
			.CreateLabel(out var target);

		//Mod?.Log($"Current Instruction: {matcher.Pos}=> {matcher.Instruction}", StardewModdingAPI.LogLevel.Warn);

		return matcher
			.Advance(-9)
			.Insert(
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Call, mIsCurrentPage),
				new CodeInstruction(OpCodes.Brtrue, target)
			)
			.InstructionEnumeration();
	}

}
