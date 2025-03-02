using System;

using HarmonyLib;

using StardewValley.Network;

namespace Leclair.Stardew.CloudySkies.Patches;

public static class LocationWeather_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {
		Mod = mod;

		try {

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(LocationWeather), nameof(LocationWeather.UpdateDailyWeather)),
				postfix: new HarmonyMethod(typeof(LocationWeather_Patches), nameof(UpdateDailyWeather__Postfix))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching LocationWeather. Weather may not work correctly.", StardewModdingAPI.LogLevel.Error, ex);
		}

	}

	#region Helpers

	public static void UpdateFromWeatherIncludingVanilla(this LocationWeather __instance, bool includeGreenRain = false) {
		if (Mod is null || __instance.Weather is null || UpdateFromWeatherData(__instance, includeGreenRain))
			return;

		// First, update the various variables
		__instance.IsRaining = false;
		__instance.IsSnowing = false;
		__instance.IsLightning = false;
		__instance.IsDebrisWeather = false;
		__instance.IsGreenRain = false;

		// Now, update based on the weather.
		switch (__instance.Weather) {
			case "Rain":
				__instance.IsRaining = true;
				break;
			case "GreenRain":
				__instance.IsGreenRain = true;
				break;
			case "Storm":
				__instance.IsRaining = true;
				__instance.IsLightning = true;
				break;
			case "Wind":
				__instance.IsDebrisWeather = true;
				break;
			case "Snow":
				__instance.IsSnowing = true;
				break;
		}
	}

	public static bool UpdateFromWeatherData(this LocationWeather __instance, bool includeGreenRain = false) {
		if (Mod is null || __instance.Weather is null || !Mod.TryGetWeather(__instance.Weather, out var weatherData))
			return false;

		__instance.IsRaining = weatherData.IsRaining;
		__instance.IsSnowing = weatherData.IsSnowing;
		__instance.IsLightning = weatherData.IsLightning;
		__instance.IsDebrisWeather = weatherData.IsDebrisWeather;
		if (includeGreenRain)
			__instance.IsGreenRain = weatherData.IsGreenRain;

		return true;
	}

	#endregion

	private static void UpdateDailyWeather__Postfix(LocationWeather __instance) {
		__instance.UpdateFromWeatherData(true);
	}

}
