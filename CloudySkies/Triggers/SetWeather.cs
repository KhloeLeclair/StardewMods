using System.Collections.Generic;

using Leclair.Stardew.CloudySkies.Patches;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;

namespace Leclair.Stardew.CloudySkies;


public static partial class Triggers {


	[TriggerAction]
	private static bool SetWeather(string[] args, TriggerActionContext context, out string? error) {

		if (!Game1.IsMasterGame) {
			error = $"The SetWeather trigger must only be run for the main player at this time.";
			return false;
		}

		HashSet<TargetLocationContext> targets = [];
		string? weatherId = null;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.Add<IEnumerable<TargetLocationContext>>("-t", "--target", val => targets.AddRange(val))
				.AllowMultiple()
				.IsRequired()
			.AddPositional<string>("WeatherId", val => weatherId = val)
				.WithDescription("The weather Id to change the weather to.")
				.IsRequired()
				.IsFinal();

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		weatherId ??= "Sun";

		if (!ModEntry.VANILLA_WEATHER.Contains(weatherId) && !Instance.TryGetWeather(weatherId, out var weatherData)) {
			error = $"Invalid weather id '{weatherId}'";
			return false;
		}

		if (targets.Count == 0) {
			error = $"No target provided.";
			return false;
		}

		Game1_Patches.SaveGreenRainHistory();
		int total = 0;
		int changed = 0;

		bool wasDebris = ModEntry.IsDebris;

		foreach (var target in targets) {
			if (!target.IsValid) {
				Instance.Log($"Skipping invalid location context '{target.Key}' when processing SetWeather trigger.", LogLevel.Debug);
				continue;
			}

			total++;

			var weather = Game1.netWorldState.Value.GetWeatherForLocation(target.Key);
			if (weather.Weather == weatherId)
				continue;

			changed++;

			weather.Weather = weatherId;
			weather.UpdateFromWeatherIncludingVanilla(true);

			// Is this the default context?
			if (target.Key == "Default") {
				Game1.isRaining = weather.IsRaining;
				Game1.isSnowing = weather.IsSnowing;
				Game1.isLightning = weather.IsLightning;
				Game1.isDebrisWeather = weather.IsDebrisWeather;
				Game1.isGreenRain = weather.IsGreenRain;
			}
		}

		Game1_Patches.UpdateGreenRainLocations();

		if (ModEntry.IsDebris && !wasDebris)
			Game1.populateDebrisWeatherArray();

		Game1.updateWeatherIcon();
		if (Game1.currentLocation != null)
			GameLocation.HandleMusicChange(Game1.currentLocation, Game1.currentLocation);

		Instance.Log($"Changed weather in {changed} of {total} matching location contexts when processing SetWeather trigger.", LogLevel.Debug);
		return true;

	}


	[TriggerAction]
	private static bool SetWeatherTomorrow(string[] args, TriggerActionContext context, out string? error) {

		HashSet<TargetLocationContext> targets = [];
		string? weatherId = null;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.Add<IEnumerable<TargetLocationContext>>("-t", "--target", val => targets.AddRange(val))
				.AllowMultiple()
				.IsRequired()
			.AddPositional<string>("WeatherId", val => weatherId = val)
				.WithDescription("The weather Id to change the weather tomorrow to.")
				.IsRequired()
				.IsFinal();

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		weatherId ??= "Sun";

		if (!ModEntry.VANILLA_WEATHER.Contains(weatherId) && !Instance.TryGetWeather(weatherId, out var weatherData)) {
			error = $"Invalid weather id '{weatherId}'";
			return false;
		}

		if (targets.Count == 0) {
			error = $"No target provided.";
			return false;
		}

		int applied = 0;

		foreach (var target in targets) {
			if (!target.IsValid) {
				Instance.Log($"Skipping invalid location context '{target.Key}' when processing SetWeatherTomorrow trigger.", LogLevel.Debug);
				continue;
			}

			if (Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season, target.Key))
				continue;

			applied++;
			var weather = Game1.netWorldState.Value.GetWeatherForLocation(target.Key);
			weather.WeatherForTomorrow = weatherId;
			if (target.Key == "Default") {
				Game1.netWorldState.Value.WeatherForTomorrow = weatherId;
				Game1.weatherForTomorrow = weatherId;
			}
		}

		if (applied == 0) {
			error = $"No valid location contexts were provided. Is there a festival tomorrow?";
			return false;
		}

		return true;

	}

}
