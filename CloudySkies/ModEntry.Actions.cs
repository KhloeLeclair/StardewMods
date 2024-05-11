using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Internal;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Leclair.Stardew.CloudySkies;

public partial class ModEntry {

	private static IEnumerable<TFeature> EnumerateTerrainFeatures<TFeature>(GameLocation location, Vector2? position = null, int radius = 1) where TFeature : TerrainFeature {
		int radSquared = radius * radius;
		foreach (var feature in location.terrainFeatures.Values) {
			if (feature is TFeature val && (!position.HasValue || Vector2.DistanceSquared(position.Value, feature.Tile) <= radSquared))
				yield return val;
		}
	}

	private static IEnumerable<(HoeDirt, IndoorPot?)> EnumerateHoeDirtAndPots(GameLocation location, Vector2? position = null, int radius = 1) {
		int radSquared = radius * radius;
		foreach (var feature in location.terrainFeatures.Values)
			if (feature is HoeDirt dirt && (!position.HasValue || Vector2.DistanceSquared(position.Value, dirt.Tile) <= radSquared))
				yield return (dirt, null);

		foreach (var sobj in location.Objects.Values)
			if (sobj is IndoorPot pot && pot.hoeDirt.Value != null && (!position.HasValue || Vector2.DistanceSquared(position.Value, pot.TileLocation) <= radSquared))
				yield return (pot.hoeDirt.Value, pot);
	}


	#region Game State Queries

	protected override void RegisterGameStateQueries() {
		List<string> registered = EventHelper.RegisterGameStateQueries(this, [
				$"{ModManifest.UniqueID}_",
				$"CS_"
			], Monitor.Log);

		registered.AddRange(EventHelper.RegisterGameStateQueries(GetType(), [
				$"{ModManifest.UniqueID}_",
				$"CS_"
			], Monitor.Log));

		if (registered.Count > 0)
			Log($"Registered Game State Query conditions: {string.Join(", ", registered)}", LogLevel.Trace);
	}

	[GSQCondition]
	public static bool WEATHER(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGetInt(query, 2, out int offset, out error) ||
			!ArgUtility.TryGet(query, 3, out string? weatherId, out error, allowBlank: false)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		if (offset > 0)
			return GameStateQuery.Helpers.ErrorResult(query, "Cannot get historic data about the future");
		else if (offset < -7)
			return GameStateQuery.Helpers.ErrorResult(query, "Historic data only extends up to 7 days");

		if (Instance.TryGetWeatherHistory(location, Game1.Date.TotalDays + offset, out string? historicId)) {
			if (historicId == weatherId)
				return true;

			int i = 4;
			while (i < query.Length) {
				if (query[i] == historicId)
					return true;
				i++;
			}
		}

		return false;
	}

	private bool TryGetWeatherForGSQ(GameLocation? location, int offset, [NotNullWhen(false)] out string? error, out LocationWeather? weather) {
		if (offset > 0) {
			error = "Cannot get historic data about the future.";
			weather = null;
			return false;
		} else if (offset < -7) {
			error = "Historic data only extends up to 7 days.";
			weather = null;
			return false;
		}

		if (location is null) {
			error = null;
			weather = null;
			return true;
		}

		weather = location.GetWeather();

		if (offset != 0) {
			if (TryGetWeatherHistory(location, Game1.Date.TotalDays + offset, out string? weatherId)) {
				if (weather.Weather != weatherId) {
					// We need to make a new weather object with this data.
					// Unfortunately this is slightly more involved than we'd like.

					if (!Game1.locationContextData.TryGetValue(location.GetLocationContextId(), out var ctxData))
						ctxData = new() {
							WeatherConditions = new()
						};

					weather = new LocationWeather {
						WeatherForTomorrow = weatherId
					};

					weather.UpdateDailyWeather(location.GetLocationContextId(), ctxData, Game1.random);
				}

			} else
				weather = null;
		}

		error = null;
		return true;
	}

	[GSQCondition]
	public static bool WEATHER_IS_RAINING(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGetOptionalInt(query, 2, out int offset, out error) ||
			!Instance.TryGetWeatherForGSQ(location, offset, out error, out var weather)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return weather?.IsRaining ?? false;
	}

	[GSQCondition]
	public static bool WEATHER_IS_SNOWING(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGetOptionalInt(query, 2, out int offset, out error) ||
			!Instance.TryGetWeatherForGSQ(location, offset, out error, out var weather)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return weather?.IsSnowing ?? false;
	}

	[GSQCondition]
	public static bool WEATHER_IS_LIGHTNING(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGetOptionalInt(query, 2, out int offset, out error) ||
			!Instance.TryGetWeatherForGSQ(location, offset, out error, out var weather)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return weather?.IsLightning ?? false;
	}

	[GSQCondition]
	public static bool WEATHER_IS_DEBRIS(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGetOptionalInt(query, 2, out int offset, out error) ||
			!Instance.TryGetWeatherForGSQ(location, offset, out error, out var weather)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return weather?.IsDebrisWeather ?? false;
	}

	[GSQCondition]
	public static bool WEATHER_IS_GREEN_RAIN(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGetOptionalInt(query, 2, out int offset, out error) ||
			!Instance.TryGetWeatherForGSQ(location, offset, out error, out var weather)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return weather?.IsGreenRain ?? false;
	}

	[GSQCondition]
	public static bool LOCATION_IGNORE_DEBRIS_WEATHER(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error))
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return location?.ignoreDebrisWeather.Value ?? false;
	}

	#endregion

	#region Trigger Actions

	public enum LocationOrContext {
		Location,
		Context
	};

	private static readonly Dictionary<string, Item?> CachedItems = new();

	private static Item? GetOrCreateInstance(string? itemId) {
		// TODO: Empty the cache periodically
		if (string.IsNullOrEmpty(itemId))
			return null;

		if (CachedItems.TryGetValue(itemId, out Item? item))
			return item;

		item = ItemRegistry.Create(itemId, allowNull: true);
		CachedItems[itemId] = item;
		return item;
	}

	[TriggerAction]
	public static bool WaterCrops(string[] args, TriggerActionContext context, out string? error) {
		Instance.Log($"Please migrate from WaterCrops to WaterDirt.", LogLevel.Warn, once: true);

		string target = string.Empty;
		string name = string.Empty;
		float chance = 1f;

		var parser = ArgumentParser.New()
			.AddPositional<string>("Target", val => target = val).IsRequired()
			.AddPositional<string>("TargetName", val => name = val).IsRequired()
			.AddPositional<float>("Chance", val => chance = val)
				.WithDescription("The percent chance that any given tile will be changed.")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0");

		if (!parser.TryParse(args[1..], out error)) {
			Instance.Monitor.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Warn);
			return false;
		}

		string[] newArgs = [
			args[0],
			"--chance",
			$"{chance}",
			target,
			name
			];

		return WaterDirt(newArgs, context, out error);
	}

	[TriggerAction]
	public static bool WaterDirt(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		string? fertilizerQuery = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of dirt tiles to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tile will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected.")
			.Add<string>("--fertilizer-query", val => fertilizerQuery = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected based on fertilizer.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and water everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!loc.IsOutdoors && !includeIndoors))
				continue;

			foreach (var (dirt, pot) in EnumerateHoeDirtAndPots(loc, entry.Position, entry.Radius)) {
				if (dirt.state.Value != 0 || !(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!string.IsNullOrEmpty(fertilizerQuery)) {
					var target = GetOrCreateInstance(dirt.fertilizer.Value);
					if (!GameStateQuery.CheckConditions(query, loc, null, inputItem: target))
						continue;
				}

				// Water ALL the dirt! (Assuming random chance.)
				if (pot is not null)
					pot.Water();
				else
					dirt.state.Value = 1;

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool WaterPets(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of pet bowls to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given pet bowl will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var building in loc.buildings) {
				// Distance check.
				if (entry.Position.HasValue) {
					var pos = building.GetBoundingBox().GetNearestPoint(entry.Position.Value);
					if (Vector2.Distance(entry.Position.Value, pos) > entry.Radius)
						continue;
				}

				// Water ALL the pet bowls! (Assuming random chance.)
				if (building is PetBowl bowl && !bowl.watered.Value && (chance >= 1f || Game1.random.NextSingle() <= chance)) {
					bowl.watered.Value = true;

					max--;
					if (max <= 0)
						break;
				}
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool UnWaterDirt(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		string? fertilizerQuery = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of dirt tiles to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tile will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected.")
			.Add<string>("--fertilizer-query", val => fertilizerQuery = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected based on fertilizer.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and un-water everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var (dirt, pot) in EnumerateHoeDirtAndPots(loc, entry.Position, entry.Radius)) {
				if (dirt.state.Value != 1 || !(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!string.IsNullOrEmpty(fertilizerQuery)) {
					var target = GetOrCreateInstance(dirt.fertilizer.Value);
					if (!GameStateQuery.CheckConditions(fertilizerQuery, loc, null, inputItem: target))
						continue;
				}

				// Desiccate ALL the dirt! (Assuming random chance.)
				dirt.state.Value = 0;
				if (pot is not null)
					pot.showNextIndex.Value = dirt.isWatered();

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool UnWaterPets(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of pet bowls to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given pet bowl will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and un-water everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var building in loc.buildings) {
				// Distance check.
				if (entry.Position.HasValue) {
					var pos = building.GetBoundingBox().GetNearestPoint(entry.Position.Value);
					if (Vector2.Distance(entry.Position.Value, pos) > entry.Radius)
						continue;
				}

				// Drain ALL the pet bowls! (Assuming random chance.)
				if (building is PetBowl bowl && bowl.watered.Value && (chance >= 1f || Game1.random.NextSingle() <= chance)) {
					bowl.watered.Value = false;

					max--;
					if (max <= 0)
						break;
				}
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool GrowCrops(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		string? fertilizerQuery = null;
		float chance = 1f;
		int steps = 1;
		int maxSteps = -1;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of crops to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("-d", "--days", val => steps = val)
				.WithDescription("How many days of growth should each crop experience. Default: 1")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("--max-days", val => maxSteps = val)
				.WithDescription("Maximum number of days of growth for each crop. If greater than --days, each crop grows a random number of days within the range.")
				.WithValidation<int>(val => val >= steps, "must be greater than or equal to --days")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given crop will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which crops are affected.")
			.Add<string>("--fertilizer-query", val => fertilizerQuery = val)
				.WithDescription("An optional Game State Query for filtering which crops are affected based on fertilizer.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var (dirt, pot) in EnumerateHoeDirtAndPots(loc, entry.Position, entry.Radius)) {
				if (dirt.crop is null || dirt.crop.dead.Value || !(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!string.IsNullOrEmpty(fertilizerQuery)) {
					var target = GetOrCreateInstance(dirt.fertilizer.Value);

					if (!GameStateQuery.CheckConditions(fertilizerQuery, loc, null, inputItem: target))
						continue;
				}

				int days = steps;
				if (maxSteps > days)
					days = Game1.random.Next(days, maxSteps + 1);

				while (days-- > 0 && dirt.crop != null && !dirt.crop.dead.Value)
					dirt.crop.newDay(dirt.state.Value == 2 ? 2 : 1);

				max--;

				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool GrowGiantCrops(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		string? fertilizerQuery = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;
		bool allowImmature = false;
		bool ignoreSize = false;
		bool ignoreLocationRequirement = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of crops to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.AddFlag("--allow-immature", () => allowImmature = true)
				.WithDescription("Allow crops that aren't yet fully grown to become giant.")
			.AddFlag("--ignore-size", () => ignoreSize = true)
				.WithDescription("Ignore size requirements when growing giant crops.")
			.AddFlag("--allow-anywhere", () => ignoreLocationRequirement = true)
				.WithDescription("Ignore location-specific AllowGiantCrop flags.")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given crop will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which crops are affected.")
			.Add<string>("--fertilizer-query", val => fertilizerQuery = val)
				.WithDescription("An optional Game State Query for filtering which crops are affected based on fertilizer.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			if (!ignoreLocationRequirement && !(loc is Farm || loc.HasMapPropertyWithValue("AllowGiantCrops")))
				continue;

			foreach (var dirt in EnumerateTerrainFeatures<HoeDirt>(loc, entry.Position, entry.Radius)) {
				if (dirt.crop is null || dirt.crop.dead.Value || !(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!allowImmature && (dirt.crop.currentPhase.Value != dirt.crop.phaseDays.Count - 1))
					continue;

				if (!dirt.crop.TryGetGiantCrops(out var crops))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!string.IsNullOrEmpty(fertilizerQuery)) {
					var target = GetOrCreateInstance(dirt.fertilizer.Value);

					if (!GameStateQuery.CheckConditions(fertilizerQuery, loc, null, inputItem: target))
						continue;
				}

				// Find a matching crop.
				foreach (var pair in crops) {
					var giantCrop = pair.Value;
					if (!string.IsNullOrEmpty(giantCrop.Condition) && !GameStateQuery.CheckConditions(giantCrop.Condition, loc))
						continue;

					bool valid = true;
					for (int yOffset = 0; yOffset < giantCrop.TileSize.Y; yOffset++) {
						for (int xOffset = 0; xOffset < giantCrop.TileSize.X; xOffset++) {
							Vector2 pos = new(dirt.Tile.X + xOffset, dirt.Tile.Y + yOffset);
							if (loc.terrainFeatures.TryGetValue(pos, out var feature)) {
								// TODO: Better check so empty HoeDirt won't block with --ignore-size
								if (feature is not HoeDirt dirt2) {
									valid = false;
									break;
								}

								if (dirt2.crop?.indexOfHarvest?.Value == dirt.crop.indexOfHarvest.Value)
									continue;
								else if (dirt2.crop != null || !ignoreSize || loc.IsTileBlockedBy(pos, ~(CollisionMask.Characters | CollisionMask.Farmers | CollisionMask.TerrainFeatures))) {
									valid = false;
									break;
								}

								foreach (var clump in loc.resourceClumps) {
									if (clump.occupiesTile((int) pos.X, (int) pos.Y)) {
										valid = false;
										break;
									}
								}

								if (!valid)
									break;

								if (loc.largeTerrainFeatures is not null)
									foreach (var feat in loc.largeTerrainFeatures) {
										if (feat.getBoundingBox().Contains(pos)) {
											valid = false;
											break;
										}
									}

								if (!valid)
									break;

							} else if (ignoreSize) {
								if (loc.doesTileHaveProperty((int) pos.X, (int) pos.Y, "Diggable", "Back") == null || loc.IsTileBlockedBy(pos, ~(CollisionMask.Characters | CollisionMask.Farmers))) {
									valid = false;
									break;
								}

							} else {
								valid = false;
								break;
							}
						}

						if (!valid)
							break;
					}

					if (!valid)
						continue;

					for (int yOffset = 0; yOffset < giantCrop.TileSize.Y; yOffset++) {
						for (int xOffset = 0; xOffset < giantCrop.TileSize.X; xOffset++) {
							Vector2 pos = new Vector2(dirt.Tile.X + xOffset, dirt.Tile.Y + yOffset);
							if (loc.terrainFeatures.TryGetValue(pos, out var feature) && feature is HoeDirt dirt2)
								dirt2.crop = null;
						}
					}

					loc.resourceClumps.Add(new GiantCrop(pair.Key, dirt.Tile));
					max--;
					break;
				}

				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool GrowTrees(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		float chance = 1f;
		int steps = 1;
		int max = int.MaxValue;
		int maxStage = Tree.treeStage;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of trees to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("-s", "--stages", val => steps = val)
				.WithDescription("How many stages of growth should each tree experience. Default: 1")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("--max-stage", val => maxStage = val)
				.WithDescription("The maximum stage a tree is allowed to reach. Default: 5")
				.WithValidation<int>(val => val >= 0, "must be greater than or equal to 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tree will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which trees are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var tree in EnumerateTerrainFeatures<Tree>(loc, entry.Position, entry.Radius)) {
				int maxSize = Math.Min(tree.GetMaxSizeHere(), maxStage);
				if (maxSize <= tree.growthStage.Value)
					continue;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var data = tree.GetData();
					var input = GetOrCreateInstance(data?.SeedItemId);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: null, inputItem: input))
						continue;
				}

				int newStage = tree.growthStage.Value + steps;
				if (newStage > maxSize)
					newStage = maxSize;

				if (newStage != tree.growthStage.Value) {
					tree.growthStage.Value = newStage;
					max--;
					if (max <= 0)
						break;
				}
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool UnGrowTrees(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		float chance = 1f;
		int steps = 1;
		int max = int.MaxValue;
		int minStage = Tree.seedStage;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of trees to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("-s", "--stages", val => steps = val)
				.WithDescription("How many stages of growth should each tree lose. Default: 1")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("--min-stage", val => minStage = val)
				.WithDescription("The minimum stage a tree is allowed to reach. Default: 0")
				.WithValidation<int>(val => val >= 0, "must be greater than or equal to 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tree will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which trees are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var tree in EnumerateTerrainFeatures<Tree>(loc, entry.Position, entry.Radius)) {
				if (tree.growthStage.Value <= minStage || tree.isTemporaryGreenRainTree.Value)
					continue;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var data = tree.GetData();
					var input = GetOrCreateInstance(data?.SeedItemId);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: null, inputItem: input))
						continue;
				}

				// TODO: Figure out what to do about tappers.

				int stages = steps;
				while (stages-- > 0 && tree.growthStage.Value > minStage)
					tree.growthStage.Value--;

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool GrowFruitTrees(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		float chance = 1f;
		int steps = 1;
		int max = int.MaxValue;
		int maxFruit = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of fruit trees to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("-day", "--days", val => steps = val)
				.WithDescription("How many days of growth should each fruit tree experience. Default: 1")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<int>("--max-fruit", val => maxFruit = val)
				.WithDescription("The maximum number of fruit to grow on any given fruit tree. Default: 3")
				.WithValidation<int>(val => val >= 0 && val <= 3, "must be in range 0 to 3")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given fruit tree will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which fruit trees are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var fruitTree in EnumerateTerrainFeatures<FruitTree>(loc, entry.Position, entry.Radius)) {
				if (fruitTree.stump.Value)
					continue;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					Item? target = fruitTree.fruit.FirstOrDefault();
					Item? input = GetOrCreateInstance(fruitTree.treeId.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				int days = steps;
				bool blocked = FruitTree.IsGrowthBlocked(fruitTree.Tile, fruitTree.Location);
				bool changed = false;

				while (days-- > 0) {
					bool updated = false;
					if (!blocked && fruitTree!.daysUntilMature.Value > 0) {
						fruitTree.daysUntilMature.Value -= fruitTree.growthRate.Value;
						fruitTree.growthStage.Value = FruitTree.DaysUntilMatureToGrowthStage(fruitTree.daysUntilMature.Value);
						updated = true;
					}

					int fruit = fruitTree.fruit.Count;
					if (!fruitTree.stump.Value && fruit < maxFruit) {
						fruitTree.TryAddFruit();
						if (fruitTree.fruit.Count != fruit)
							updated = true;
					}

					if (!updated)
						break;
					else
						changed = true;
				}

				if (changed)
					max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool ConvertTrees(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string treeId = string.Empty;
		string? query = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<string>("TreeId", val => treeId = val)
				.IsRequired()
				.WithDescription("The Id to convert the matching tree(s) to. Must match an entry in Data/WildTrees")
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of trees to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tree will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which trees are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		if (!DataLoader.WildTrees(Game1.content).TryGetValue(treeId, out var treeData)) {
			error = $"Invalid tree Id: {treeId}";
			return false;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var tree in EnumerateTerrainFeatures<Tree>(loc, entry.Position, entry.Radius)) {
				if (tree.treeType.Value == treeId || tree.tapped.Value || tree.isTemporaryGreenRainTree.Value || tree.growthStage.Value < Tree.treeStage)
					continue;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var data = tree.GetData();
					Item? input = GetOrCreateInstance(data?.SeedItemId);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: null, inputItem: input))
						continue;
				}

				tree.treeType.Value = treeId;
				tree.loadSprite();

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool GrowMoss(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of trees to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tree will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which trees are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var tree in EnumerateTerrainFeatures<Tree>(loc, entry.Position, entry.Radius)) {
				if (tree.growthStage.Value < Tree.treeStage || tree.hasMoss.Value)
					continue;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var data = tree.GetData();
					Item? input = GetOrCreateInstance(data?.SeedItemId);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: null, inputItem: input))
						continue;
				}

				tree.hasMoss.Value = true;

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool KillMoss(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of trees to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tree will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which trees are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and grow everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var tree in EnumerateTerrainFeatures<Tree>(loc, entry.Position, entry.Radius)) {
				if (!tree.hasMoss.Value)
					continue;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var data = tree.GetData();
					Item? input = GetOrCreateInstance(data?.SeedItemId);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: null, inputItem: input))
						continue;
				}

				tree.hasMoss.Value = false;

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool KillCrops(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		string? fertilizerQuery = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of crops to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given crops will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which crops are affected.")
			.Add<string>("--fertilizer-query", val => fertilizerQuery = val)
				.WithDescription("An optional Game State Query for filtering which crops are affected based on fertilizer.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and un-alive everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var (dirt, pot) in EnumerateHoeDirtAndPots(loc, entry.Position, entry.Radius)) {
				if (dirt.crop is null || dirt.crop.dead.Value || !(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!string.IsNullOrEmpty(fertilizerQuery)) {
					var target = GetOrCreateInstance(dirt.fertilizer.Value);
					if (!GameStateQuery.CheckConditions(fertilizerQuery, loc, null, inputItem: target))
						continue;
				}

				dirt.crop.Kill();
				dirt.crop.updateDrawMath(dirt.Tile);

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool FertilizeDirt(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? fertilizerId = null;
		string? query = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<string>("FertilizerId", val => fertilizerId = val)
				.IsRequired()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of dirt tiles to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given dirt tiles will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		fertilizerId = ItemRegistry.QualifyItemId(fertilizerId);
		if (ItemRegistry.GetData(fertilizerId) is null) {
			error = $"Invalid item Id for fertilizer: {fertilizerId}";
			return false;
		}

		// Now, loop through all the locations and fertilize everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var (dirt, pot) in EnumerateHoeDirtAndPots(loc, entry.Position, entry.Radius)) {
				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!dirt.CanApplyFertilizer(fertilizerId))
					continue;

				// TODO: Support for mods that allow you to use
				// multiple fertilizers.
				//
				// Currently, that means Ultimate Fertilizer. For now,
				// we just abort if there is an existing fertilizer.
				if (dirt.fertilizer.Value != null)
					continue;

				// Fertilize ALL the crops!
				dirt.fertilizer.Value = fertilizerId;
				dirt.applySpeedIncreases(Game1.player);

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	public static bool UnFertilizeDirt(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		string? query = null;
		string? fertilizerQuery = null;
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of dirt tiles to change.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given dirt tiles will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.Add<string>("-q", "--query", val => query = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected.")
			.Add<string>("--fertilizer-query", val => fertilizerQuery = val)
				.WithDescription("An optional Game State Query for filtering which dirt tiles are affected based on fertilizer.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		// Now, loop through all the locations and un-fertilize everything.
		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			foreach (var (dirt, pot) in EnumerateHoeDirtAndPots(loc, entry.Position, entry.Radius)) {
				if (dirt.fertilizer.Value == null || !(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (!string.IsNullOrEmpty(query)) {
					var target = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.indexOfHarvest.Value);
					var input = dirt.crop is null ? null : GetOrCreateInstance(dirt.crop.netSeedIndex.Value);

					if (!GameStateQuery.CheckConditions(query, loc, null, targetItem: target, inputItem: input))
						continue;
				}

				if (!string.IsNullOrEmpty(fertilizerQuery)) {
					var target = GetOrCreateInstance(dirt.fertilizer.Value);
					if (!GameStateQuery.CheckConditions(fertilizerQuery, loc, null, inputItem: target))
						continue;
				}

				// Remove the fertilizer.
				dirt.fertilizer.Value = null;
				dirt.applySpeedIncreases(Game1.player);

				max--;
				if (max <= 0)
					break;
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

	[TriggerAction]
	private static bool SpawnForage(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;
		bool includeDefault = false;
		bool ignoreSpawnable = false;

		List<SpawnForageData> spawns = [];

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of forage to spawn.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tile spawn forage will be changed. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--include-default", () => includeDefault = true)
				.WithDescription("If this flag is set, include the location's default forage items in the list of potential spawns.")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.AddFlag("--ignore-spawnable", () => ignoreSpawnable = true)
				.WithDescription("If this flag is set, we will ignore the Spawnable flag of tiles and allow spawning anywhere.")
			.Add<string>("-i", "--item", val => {
				var added = new SpawnForageData() {
					ItemId = val
				};
				spawns.Add(added);
			})
				.WithDescription("Add a new item to the list of forage to spawn. Supports item queries.")
			.Add<string>("-iq", "--item-query", val => spawns.Last().PerItemCondition = val)
				.WithDescription("Adds a per-item condition to the previously added item.")
			.Add<int>("-q", "--item-quality", val => spawns.Last().Quality = val)
				.WithDescription("Adds a quality to the previously added item.");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		Dictionary<GameLocation, List<SpawnForageData>>? availableSpawns = includeDefault ? [] : null;

		List<SpawnForageData> GetAvailableSpawns(GameLocation location) {
			if (!includeDefault)
				return spawns;

			if (availableSpawns!.TryGetValue(location, out var result))
				return result;

			result = new(spawns);

			var data = location.GetData();
			if (data != null) {
				Season season = location.GetSeason();
				foreach (var spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage)) {
					if (spawn.Season.HasValue && spawn.Season != season)
						continue;
					if (spawn.Condition is null || GameStateQuery.CheckConditions(spawn.Condition, location))
						result.Add(spawn);
				}
			}

			availableSpawns[location] = result;
			return result;
		}

		if (includeDefault || spawns.Count > 0)
			foreach (var entry in targets.SelectMany(x => x)) {
				var loc = entry.Location;
				if (loc is null || (!includeIndoors && !loc.IsOutdoors))
					continue;

				var forage = GetAvailableSpawns(loc);
				if (forage is null || forage.Count == 0)
					continue;

				IEnumerable<Vector2> tiles;
				if (entry.Position.HasValue)
					tiles = entry.Position.Value.IterArea(entry.Radius, false);
				else
					tiles = EnumerateAllTiles(loc);

				ItemQueryContext ctx = new(loc, Game1.player, Game1.random);
				Dictionary<SpawnForageData, IList<ItemQueryResult>> cachedQueryResults = [];

				foreach (var pos in tiles) {
					int x = (int) pos.X;
					int y = (int) pos.Y;

					if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
						continue;

					if (loc.Objects.ContainsKey(pos) ||
						loc.IsNoSpawnTile(pos) ||
						(!ignoreSpawnable && loc.doesTileHaveProperty(x, y, "Spawnable", "Back") == null) ||
						loc.doesEitherTileOrTileIndexPropertyEqual(x, y, "Spawnable", "Back", "F") ||
						!loc.CanItemBePlacedHere(pos) ||
						loc.getTileIndexAt(x, y, "AlwaysFront") != -1 ||
						loc.getTileIndexAt(x, y, "AlwaysFront2") != -1 ||
						loc.getTileIndexAt(x, y, "AlwaysFront3") != -1 ||
						loc.getTileIndexAt(x, y, "Front") != -1 ||
						loc.isBehindBush(pos)
					)
						continue;

					// TODO: Maybe determine some way of respecting an entry's
					// chance to spawn while also ensuring something spawns?

					var toSpawn = Game1.random.ChooseFrom(forage);

					if (!cachedQueryResults.TryGetValue(toSpawn, out var result)) {
						result = ItemQueryResolver.TryResolve(toSpawn, ctx, ItemQuerySearchMode.AllOfTypeItem, avoidRepeat: false, logError: (query, error) => {
							Instance.Log($"Failed parsing item query '{query}' for forage: {error}", LogLevel.Error);
						});
						cachedQueryResults[toSpawn] = result;
					}

					var itemToSpawn = Game1.random.ChooseFrom(result);
					if (itemToSpawn.Item is not SObject sobj || sobj.getOne() is not SObject copy)
						continue;

					copy.IsSpawnedObject = true;
					if (loc.dropObject(copy, pos * 64f, Game1.viewport, initialPlacement: true)) {
						max--;
						if (max <= 0)
							break;
					}
				}

				if (max <= 0)
					break;
			}

		// Great success!
		error = null;
		return true;
	}

	private static IEnumerable<Vector2> EnumerateAllTiles(GameLocation location) {
		int width = location.map.DisplayWidth / 64;
		int height = location.map.DisplayHeight / 64;

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				yield return new Vector2(x, y);
			}
		}
	}


	#endregion

	#region Custom Weather Totems

	public bool UseWeatherTotem(Farmer who, string weatherId, SObject? item = null) {
		var location = who.currentLocation;

		string contextId = location.GetLocationContextId();
		var context = location.GetLocationContext();

		if (context.RainTotemAffectsContext != null) {
			contextId = context.RainTotemAffectsContext;
			context = LocationContexts.Require(contextId);
		}

		bool allowed = context.AllowRainTotem;

		if (context.CustomFields != null && context.CustomFields.TryGetValue($"{ALLOW_TOTEM_DATA}{weatherId}", out string? val)) {
			if (bool.TryParse(val, out bool result))
				allowed = result;
			else
				allowed = !string.IsNullOrWhiteSpace(val);
		}

		if (!allowed) {
			Game1.showRedMessageUsingLoadString("Strings\\UI:Item_CantBeUsedHere");
			return false;
		}

		bool applied = false;

		if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.season, contextId)) {
			Game1.netWorldState.Value.GetWeatherForLocation(contextId).WeatherForTomorrow = weatherId;
			applied = true;
		}

		if (applied && contextId == "Default") {
			Game1.netWorldState.Value.WeatherForTomorrow = weatherId;
			Game1.weatherForTomorrow = weatherId;
		}

		TryGetWeather(weatherId, out var weatherData);

		if (applied) {
			string message;
			if (weatherData != null && !string.IsNullOrEmpty(weatherData.TotemMessage))
				message = TokenizeText(weatherData.TotemMessage);
			else
				message = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822");

			Game1.pauseThenMessage(2000, message);
		}

		Game1.screenGlow = false;
		string? sound = weatherData is null ? "thunder" : weatherData.TotemSound;
		if (!string.IsNullOrEmpty(sound))
			location.playSound(sound);

		who.CanMove = false;
		Color color = weatherData?.TotemScreenTint ?? Color.SlateBlue;
		if (color != Color.Transparent)
			Game1.screenGlowOnce(color, hold: false);

		Game1.player.faceDirection(2);
		Game1.player.FarmerSprite.animateOnce([
			new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
		]);

		string? texture = weatherData is null ? "LooseSprites\\Cursors" : weatherData.TotemParticleTexture;
		if (!string.IsNullOrEmpty(texture)) {
			Rectangle bounds;
			if (weatherData is null)
				bounds = new Rectangle(648, 1045, 52, 33);
			else if (weatherData.TotemParticleSource.HasValue)
				bounds = weatherData.TotemParticleSource.Value;
			else {
				Texture2D tex = Game1.content.Load<Texture2D>(texture);
				bounds = tex.Bounds;
			}

			for (int i = 0; i < 6; i++) {
				Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(
					texture, bounds,
					9999f, 1, 999,
					who.Position + new Vector2(0f, -128f),
					flicker: false,
					flipped: false,
					1f, 0.01f,
					Color.White * 0.8f,
					2f, 0.01f,
					0f, 0f
				) {
					motion = new Vector2(Game1.random.Next(-10, 11) / 10f, -2f),
					delayBeforeAnimationStart = i * 200
				});

				Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(
					texture, bounds,
					9999f, 1, 999,
					who.Position + new Vector2(0f, -128f),
					flicker: false,
					flipped: false,
					1f, 0.01f,
					Color.White * 0.8f,
					1f, 0.01f,
					0f, 0f
				) {
					motion = new Vector2(Game1.random.Next(-30, -10) / 10f, -1f),
					delayBeforeAnimationStart = 100 + i * 200
				});

				Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(
					texture, bounds,
					9999f, 1, 999,
					who.Position + new Vector2(0f, -128f),
					flicker: false,
					flipped: false,
					1f, 0.01f,
					Color.White * 0.8f,
					1f, 0.01f,
					0f, 0f
				) {
					motion = new Vector2(Game1.random.Next(10, 30) / 10f, -1f),
					delayBeforeAnimationStart = 200 + i * 200
				});
			}
		}

		if (item is not null) {
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(0, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f) {
				motion = new Vector2(0f, -7f),
				acceleration = new Vector2(0f, 0.1f),
				scaleChange = 0.015f,
				alpha = 1f,
				alphaFade = 0.0075f,
				shakeIntensity = 1f,
				initialPosition = Game1.player.Position + new Vector2(0f, -96f),
				xPeriodic = true,
				xPeriodicLoopTime = 1000f,
				xPeriodicRange = 4f,
				layerDepth = 1f
			};
			sprite.CopyAppearanceFromItemId(item.QualifiedItemId);
			Game1.Multiplayer.broadcastSprites(location, sprite);
		}

		sound = weatherData is null ? "rainsound" : weatherData.TotemAfterSound;
		if (!string.IsNullOrEmpty(sound))
			DelayedAction.playSoundAfterDelay(sound, 2000);

		return true;
	}

	#endregion

	#region Tokenized Text Stuff

	[return: NotNullIfNotNull(nameof(input))]
	public string? TokenizeText(string? input, Farmer? who = null, Random? rnd = null) {
		if (string.IsNullOrWhiteSpace(input))
			return input;

		bool ParseToken(string[] query, out string? replacement, Random? random, Farmer? player) {
			if (!ArgUtility.TryGet(query, 0, out string? cmd, out string? error))
				return TokenParser.LogTokenError(query, error, out replacement);

			if (cmd is null || !cmd.Equals("LocalizedText")) {
				replacement = null;
				return false;
			}

			if (!ArgUtility.TryGet(query, 1, out string? key, out error))
				return TokenParser.LogTokenError(query, error, out replacement);

			var tl = Helper.Translation.Get(key);
			if (!tl.HasValue()) {
				replacement = null;
				return false;
			}

			Dictionary<int, string> replacements;
			if (query.Length > 2) {
				replacements = new();
				for (int i = 2; i < query.Length; i++) {
					replacements[i - 2] = query[i];
				}

			} else
				replacements = [];

			replacement = tl.Tokens(replacements).ToString();
			return true;
		}

		return TokenParser.ParseText(input, rnd, ParseToken, who);
	}


	#endregion

}
