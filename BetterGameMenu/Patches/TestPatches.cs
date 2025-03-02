#if DEBUG

using System;
using System.Diagnostics;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewModdingAPI.Utilities;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class TestPatches {

	private static ModEntry? Mod;

	private readonly static PerScreen<Stopwatch?> MenuTimer = new();

	internal static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			mod.Harmony.Patch(
				original: AccessTools.Constructor(typeof(GameMenu), [typeof(bool)]),
				prefix: new HarmonyMethod(typeof(TestPatches), nameof(StartTiming)),
				postfix: new HarmonyMethod(typeof(TestPatches), nameof(StopTiming))
			);

			mod.Harmony.Patch(
				original: AccessTools.Constructor(typeof(BetterGameMenuImpl), [typeof(ModEntry), typeof(string), typeof(int), typeof(bool)]),
				prefix: new HarmonyMethod(typeof(TestPatches), nameof(StartTiming)),
				postfix: new HarmonyMethod(typeof(TestPatches), nameof(StopTiming))
			);

		} catch (Exception ex) {
			mod.Log($"Unable to apply test patches to game menu classes.", StardewModdingAPI.LogLevel.Error, ex);
		}

	}

	private static bool StartTiming() {
		Mod?.Log($"Started opening menu.", StardewModdingAPI.LogLevel.Debug);
		MenuTimer.Value = Stopwatch.StartNew();
		return true;
	}

	private static void StopTiming() {
		var timer = MenuTimer.Value;
		MenuTimer.Value = null;
		if (Mod is not null && timer is not null) {
			timer.Stop();
			Mod.Log($"Finished constructing menu in {timer.ElapsedTicks}", StardewModdingAPI.LogLevel.Debug);
		}
	}

}

#endif
