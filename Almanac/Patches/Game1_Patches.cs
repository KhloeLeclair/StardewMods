using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using StardewModdingAPI;

using StardewValley;

namespace Leclair.Stardew.Almanac.Patches;

internal static class Game1_Patches {

	private static IMonitor Monitor;

	internal static void Patch(ModEntry mod) {
		Monitor = mod.Monitor;
		/*
		try {
			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateWeatherForNewDay)),
				postfix: new HarmonyMethod(typeof(Game1_Patches), nameof(UpdateWeatherForNewDay_Postfix))
			);

		} catch(Exception ex) {
			mod.Log("An error occurred while registering a harmony patch for Game1.", LogLevel.Error, ex);
		}*/
	}
	/*
	public static void UpdateWeatherForNewDay_Postfix() {
		try {
			ModEntry.Instance.Weather.UpdateForNewDay();

		} catch (Exception ex) {
			Monitor.Log($"An error occurred while running {typeof(Game1_Patches)}.{nameof(UpdateWeatherForNewDay_Postfix)}", LogLevel.Error);
			Monitor.Log($"Details:\n{ex}", LogLevel.Error);
		}
	}
	*/
}
