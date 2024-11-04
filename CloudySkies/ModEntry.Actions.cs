using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Leclair.Stardew.CloudySkies;

public partial class ModEntry {

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

	private static bool TryParseDarkTime(int darkTime, string? input, out int parsedTime) {
		if (input == null) {
			parsedTime = default;
			return false;
		}

		if (input.StartsWith("Dark", StringComparison.OrdinalIgnoreCase)) {
			if (input.Length == 4)
				parsedTime = darkTime;
			else if (int.TryParse(input[4..], out parsedTime))
				parsedTime += darkTime;
			else
				return false;
		} else if (int.TryParse(input, out parsedTime)) {
			if (parsedTime < 0)
				parsedTime += darkTime;
		} else
			return false;

		return true;
	}


	[GSQCondition]
	public static bool LOCATION_TIME(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGet(query, 2, out string? rawMinTime, out error) ||
			!ArgUtility.TryGetOptional(query, 3, out string? rawMaxTime, out error)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		int time = Game1.timeOfDay;
		int darkTime = Game1.getTrulyDarkTime(location);

		if (!TryParseDarkTime(darkTime, rawMinTime, out int minTime))
			return GameStateQuery.Helpers.ErrorResult(query, $"unable to parse '{rawMinTime}' as minimum time");

		int maxTime;
		if (rawMaxTime == null)
			maxTime = int.MaxValue;
		else if (!TryParseDarkTime(darkTime, rawMaxTime, out maxTime))
			return GameStateQuery.Helpers.ErrorResult(query, $"unable to parse '{rawMaxTime}' as maximum time");

		return time >= minTime && time <= maxTime;
	}

	internal static readonly Dictionary<string, Type?> TypeCache = [];
	internal static bool TryGetType(string name, [NotNullWhen(true)] out Type? type) {
		if (TypeCache.TryGetValue(name, out type))
			return type != null;

		type = AccessTools.TypeByName($"StardewValley.Locations.{name}")
			?? AccessTools.TypeByName(name);

		TypeCache[name] = type;
		return type != null;
	}

	[GSQCondition]
	public static bool LOCATION_IS_TYPE(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error) ||
			!ArgUtility.TryGet(query, 2, out _, out error)
		)
			return GameStateQuery.Helpers.ErrorResult(query, error);

		Type locType = location.GetType();

		for (int i = 2; i < query.Length; i++) {
			if (TryGetType(query[i], out Type? type) && locType.IsAssignableTo(type))
				return true;
		}

		return false;
	}

	#endregion

	#region Trigger Actions

	protected override void RegisterTriggerActions() {
		string key = $"{ModManifest.UniqueID}_";
		List<string> registered = EventHelper.RegisterTriggerActions(typeof(Triggers), key, Monitor.Log);
		if (registered.Count > 0)
			Log($"Registered trigger actions: {string.Join(", ", registered)}", LogLevel.Trace);
	}

	#endregion

	#region Critter Spawning

	private static Action<GameLocation, Vector2, List<Critter>>? AddCrittersStartingAtTile;

	[MemberNotNullWhen(true, nameof(AddCrittersStartingAtTile))]
	private static bool LoadAddCrittersStartingAtTile() {
		if (AddCrittersStartingAtTile != null)
			return true;

		var method = AccessTools.Method(typeof(GameLocation), "addCrittersStartingAtTile");
		if (method is null)
			return false;

		AddCrittersStartingAtTile = method.CreateAction<GameLocation, Vector2, List<Critter>>();
		return true;
	}


	public void SpawnCritters(GameLocation location, IEnumerable<ICritterSpawnData> spawnData, bool onlyIfOnScreen = false) {
		if (spawnData is null || location?.critters is null || location.map?.Layers is null || location.map.Layers.Count == 0 || location.map.Layers[0] is null)
			return;

		double mapArea = location.map.Layers[0].LayerWidth * location.map.Layers[0].LayerHeight;
		double baseChance = Math.Max(0.15, Math.Min(0.5, mapArea / 15_000.0));

		HashSet<string> groups = new();

		bool summer = location.IsSummerHere();
		int critterLimit = summer ? 20 : 10;

		var ctx = new GameStateQueryContext(location, Game1.player, null, null, null);

		int old_count = location.critters.Count;

		foreach (var entry in spawnData) {
			if (entry.Group != null && groups.Contains(entry.Group))
				continue;

			if (entry.Chance <= 0)
				continue;

			if (!string.IsNullOrEmpty(entry.Condition) && !GameStateQuery.CheckConditions(entry.Condition, ctx))
				continue;

			if (entry.Group != null)
				groups.Add(entry.Group);

			if (location.critters.Count > critterLimit)
				break;

			switch (entry.Type.ToLower()) {
				case "cloud":
					SpawnCloud(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "birdie":
					SpawnBirdie(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "firefly":
				case "butterfly":
					SpawnFireOrButterfly(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "rabbit":
					SpawnRabbit(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "squirrel":
					SpawnSquirrel(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "woodpecker":
					SpawnWoodpecker(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "owl":
					SpawnOwl(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				case "opossum":
					SpawnOpossum(location, entry, entry.Chance * baseChance, onlyIfOnScreen);
					break;
				default:
					Log($"Invalid critter type '{entry.Type}' when spawning critters for location '{location.Name}'.", LogLevel.Warn);
					break;
			}
		}

#if DEBUG
		int spawned = location.critters.Count - old_count;
		Log($"Spawned {spawned} critters using custom data.", LogLevel.Debug);
#endif
	}

	private void SpawnCloud(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		double c = Math.Min(0.9, chance);
		while (Game1.random.NextDouble() < c) {
			Vector2 pos;
			if (onlyIfOnScreen) {
				pos = Game1.random.NextBool()
					? new Vector2(
						location.map.Layers[0].LayerWidth,
						Game1.random.Next(location.map.Layers[0].LayerHeight)
					)
					: new Vector2(
						Game1.random.Next(location.map.Layers[0].LayerWidth),
						location.map.Layers[0].LayerHeight
					);

			} else {
				pos = location.getRandomTile();
				if (Utility.isOnScreen(pos * 64f, 1280))
					continue;
			}

			var cloud = new Cloud(pos);
			bool freeToAdd = true;
			foreach (var existing in location.critters) {
				if (existing is Cloud && existing.getBoundingBox(0, 0).Intersects(cloud.getBoundingBox(0, 0))) {
					freeToAdd = false;
					break;
				}
			}

			if (freeToAdd)
				location.addCritter(cloud);
		}
	}

	private void SpawnBirdie(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		var season = location.GetSeason();
		if (!LoadAddCrittersStartingAtTile())
			return;

		while (Game1.random.NextDouble() < chance && location.critters.Count <= 100) {
			int birdiesToAdd = Game1.random.Next(1, 4);
			bool success = false;
			int tries = 0;
			while (!success && tries < 5) {
				Vector2 pos = location.getRandomTile();
				if (!onlyIfOnScreen || !Utility.isOnScreen(pos * 64f, 64)) {
					var area = new Rectangle(
						(int) pos.X - 2,
						(int) pos.Y - 2,
						5, 5);

					if (location.isAreaClear(area)) {
						List<Critter> crittersToAdd = [];

						int whichBird = season == Season.Fall ? 45 : 25;
						if (Game1.random.NextBool() && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
							whichBird = season == Season.Fall ? 135 : 125;
						if (whichBird == 25 && Game1.random.NextDouble() < 0.05)
							whichBird = 165;

						for (int i = 0; i < birdiesToAdd; i++)
							crittersToAdd.Add(new Birdie(-100, -100, whichBird));

						AddCrittersStartingAtTile(location, pos, crittersToAdd);
					}
				}

				tries++;
			}
		}
	}

	private void SpawnFireOrButterfly(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		bool is_firefly = spawnData.Type.Equals("firefly", StringComparison.OrdinalIgnoreCase);
		bool is_island = location.InIslandContext();

		double c = Math.Min(0.8, chance * 1.5);
		while (Game1.random.NextDouble() < c && location.critters.Count <= 100) {
			Vector2 pos = location.getRandomTile();
			if (onlyIfOnScreen && Utility.isOnScreen(pos * 64f, 64))
				continue;

			if (is_firefly)
				location.critters.Add(new Firefly(pos));
			else
				location.critters.Add(new Butterfly(location, pos, is_island));

			while (Game1.random.NextDouble() < 0.4) {
				var p = pos + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3));

				if (is_firefly)
					location.critters.Add(new Firefly(p));
				else
					location.critters.Add(new Butterfly(location, p, is_island));
			}
		}

		if (Game1.timeOfDay < 1700)
			location.tryAddPrismaticButterfly();
	}

	private void SpawnRabbit(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		if (onlyIfOnScreen || !(Game1.random.NextDouble() < chance) || location.largeTerrainFeatures is null || location.largeTerrainFeatures.Count == 0)
			return;

		for (int i = 0; i < 3; i++) {
			int idx = Game1.random.Next(location.largeTerrainFeatures.Count);
			if (location.largeTerrainFeatures[idx] is not Bush bush)
				continue;

			Vector2 pos = bush.Tile;
			int distance = Game1.random.Next(5, 12);
			bool flip = Game1.random.NextBool();
			bool success = true;
			var box = bush.getBoundingBox();

			for (int j = 0; j < distance; j++) {
				pos.X += (flip ? 1 : -1);
				if (!box.Intersects(new Rectangle((int) pos.X * 64, (int) pos.Y * 64, 64, 64)) && !location.CanSpawnCharacterHere(pos)) {
					success = false;
					break;
				}
			}

			if (success)
				location.critters.Add(new Rabbit(location, pos, flip));
		}
	}

	private void SpawnSquirrel(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		if (onlyIfOnScreen || !(Game1.random.NextDouble() < chance) || location.terrainFeatures.Length == 0)
			return;

		for (int i = 0; i < 3; i++) {
			if (!Utility.TryGetRandom(location.terrainFeatures, out var pos, out var feature) ||
				feature is not Tree tree ||
				tree.growthStage.Value < 5 ||
				tree.stump.Value
			)
				continue;

			int distance = Game1.random.Next(4, 7);
			bool flip = Game1.random.NextBool();
			bool success = true;

			for (int j = 0; j < distance; j++) {
				pos.X += (flip ? 1 : -1);
				if (!location.CanSpawnCharacterHere(pos)) {
					success = false;
					break;
				}
			}

			if (success)
				location.critters.Add(new Squirrel(pos, flip));
		}
	}

	private void SpawnWoodpecker(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		if (onlyIfOnScreen || !(Game1.random.NextDouble() < chance) || location.terrainFeatures.Length == 0)
			return;

		for (int i = 0; i < 3; i++) {
			if (!Utility.TryGetRandom(location.terrainFeatures, out var pos, out var feature) ||
				feature is not Tree tree ||
				tree.growthStage.Value < 5 ||
				!(tree.GetData()?.AllowWoodpeckers ?? false)
			)
				continue;

			location.critters.Add(new Woodpecker(tree, pos));
		}
	}

	private void SpawnOwl(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		while (Game1.random.NextDouble() < chance)
			location.addOwl();
	}

	private void SpawnOpossum(GameLocation location, ICritterSpawnData spawnData, double chance, bool onlyIfOnScreen) {
		if (onlyIfOnScreen || !(Game1.random.NextDouble() < chance) || location.largeTerrainFeatures is null || location.largeTerrainFeatures.Count == 0)
			return;

		for (int i = 0; i < 3; i++) {
			int idx = Game1.random.Next(location.largeTerrainFeatures.Count);
			if (location.largeTerrainFeatures[idx] is not Bush bush)
				continue;

			Vector2 pos = bush.Tile;
			int distance = Game1.random.Next(5, 12);
			bool flip = Game1.player.Position.X > (location is BusStop ? 704 : 64);
			bool success = true;
			var box = bush.getBoundingBox();

			for (int j = 0; j < distance; j++) {
				pos.X += (flip ? 1 : -1);
				if (!box.Intersects(new Rectangle((int) pos.X * 64, (int) pos.Y * 64, 64, 64)) && !location.CanSpawnCharacterHere(pos)) {
					success = false;
					break;
				}
			}

			if (success) {
				if (location is BusStop && Game1.random.NextDouble() < 0.5)
					pos = new Vector2(Game1.player.Tile.X < 26 ? 36 : 16, 23 + Game1.random.Next());
				location.critters.Add(new Rabbit(location, pos, flip));
			}
		}
	}

	#endregion

	#region Custom Weather Totems

	public bool UseWeatherTotem(Farmer who, string weatherId, SObject? item = null, bool bypassChecks = false) {
		var location = who.currentLocation;

		string contextId = location.GetLocationContextId();
		var context = location.GetLocationContext();

		if (!bypassChecks && context.RainTotemAffectsContext != null) {
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

		if (!allowed && !bypassChecks) {
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

			replacement = TokenParser.ParseText(tl.Tokens(replacements).ToString(), rnd, ParseToken, who);
			return true;
		}

		return TokenParser.ParseText(input, rnd, ParseToken, who);
	}


	#endregion

}
