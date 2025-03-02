using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class MapPage_Patches {

	private static ModEntry? Mod;

	internal static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(MapPage), nameof(MapPage.receiveLeftClick)),
				transpiler: new HarmonyMethod(typeof(MapPage_Patches), nameof(receiveLeftClick__Transpiler))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching MapPage's click method.", StardewModdingAPI.LogLevel.Error, ex);
		}

	}

	#region Helper Methods

	private static void RestoreLastTab() {
		if (Game1.activeClickableMenu is BetterGameMenuImpl menu)
			menu.TryChangeTab(menu.LastTab ?? nameof(VanillaTabOrders.Inventory));
	}

	#endregion

	private static IEnumerable<CodeInstruction> receiveLeftClick__Transpiler(IEnumerable<CodeInstruction> instructions) {
		var GameMenu = typeof(GameMenu);
		var Game1_activeClickableMenu = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.activeClickableMenu));
		var mRestoreLastTab = AccessTools.Method(typeof(MapPage_Patches), nameof(RestoreLastTab));

		var matcher = new CodeMatcher(instructions)
			.MatchStartForward(
				new CodeMatch(in0 => in0.Calls(Game1_activeClickableMenu)),
				new CodeMatch(OpCodes.Isinst, GameMenu),
				new CodeMatch(in0 => in0.IsStloc())
			)
			.ThrowIfInvalid("could not find if Game1.activeClickableMenu is GameMenu")
			.SetAndAdvance(OpCodes.Call, mRestoreLastTab)
			.Insert(
				new CodeInstruction(OpCodes.Call, Game1_activeClickableMenu)
			);

		return matcher.InstructionEnumeration();
	}

}
