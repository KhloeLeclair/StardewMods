using System;

using HarmonyLib;

using StardewValley;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class StarControl_Patches {

	private static ModEntry? Mod;

	internal static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			var method = AccessTools.Method("StarControl.Menus.BuiltInItems:OpenMenuTab", [typeof(int)]);
			if (method != null)
				mod.Harmony.Patch(
					original: method,
					prefix: new HarmonyMethod(typeof(StarControl_Patches), nameof(OpenMenuTab__Prefix))
				);

		} catch (Exception ex) {
			mod.Log($"Error patching StarControl's OpenMenuTab method: {ex}", StardewModdingAPI.LogLevel.Warn);
		}

	}

	private static bool OpenMenuTab__Prefix(int tab) {
		if (Mod is null || !Mod.Config.Enabled)
			return true;

		Game1.PushUIMode();
		Game1.activeClickableMenu = Game1_Patches.CreateMenuOther(tab, -1, false);
		Game1.PopUIMode();
		return false;
	}

}
