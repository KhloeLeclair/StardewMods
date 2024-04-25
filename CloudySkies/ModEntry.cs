using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.CloudySkies.Layers;
using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.Common.Integrations.GenericModConfigMenu;
using Leclair.Stardew.Common.Types;
using Leclair.Stardew.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData.LocationContexts;
using StardewValley.Mods;
using StardewValley.TokenizableStrings;

namespace Leclair.Stardew.CloudySkies;


public partial class ModEntry : ModSubscriber {

	public static ModEntry Instance { get; private set; } = null!;

	public const string DATA_ASSET = @"Mods/leclair.cloudyskies/WeatherData";

	private static readonly Func<ModHooks> HookDelegate = AccessTools.Field(typeof(Game1), "hooks").CreateGetter<ModHooks>();

#nullable disable

	public ModConfig Config { get; private set; }

	internal Harmony Harmony;

#nullable enable

	internal GMCMIntegration<ModConfig, ModEntry>? GMCMIntegration;

	private ulong lastLayerId = 0;

	internal readonly Dictionary<ulong, HashSet<string>> AssetsByLayer = new();
	internal readonly Dictionary<string, HashSet<ulong>> AssetsByName = new();

	internal Dictionary<string, WeatherData>? Data;

	internal readonly PerScreen<double> UpdateTiming = new();
	internal readonly PerScreen<double> DrawTiming = new();

	public override void Entry(IModHelper helper) {
		base.Entry(helper);

		Instance = this;

		// Harmony
		Harmony = new Harmony(ModManifest.UniqueID);

		Patches.PatchHelper.Init(this);

		Patches.DayTimeMoneyBox_Patches.Patch(this);
		Patches.Game1_Patches.Patch(this);
		Patches.GameLocation_Patches.Patch(this);
		Patches.LocationWeather_Patches.Patch(this);
		Patches.Music_Patches.Patch(this);
		Patches.TV_Patches.Patch(this);

		// Read Config
		Config = Helper.ReadConfig<ModConfig>();

		// Init
		I18n.Init(Helper.Translation);

	}

	public override object? GetApi(IModInfo mod) {
		return new ModApi(this, mod.Manifest);
	}

	public static bool LocationIgnoreDebrisWeather(string[] query, GameStateQueryContext context) {
		GameLocation location = context.Location;
		if (!GameStateQuery.Helpers.TryGetLocationArg(query, 1, ref location, out string? error))
			return GameStateQuery.Helpers.ErrorResult(query, error);

		return location?.ignoreDebrisWeather.Value ?? false;
	}

	#region Configuration

	private void RegisterSettings() {
		if (GMCMIntegration is null || !GMCMIntegration.IsLoaded)
			return;

		GMCMIntegration.Register(true);

		GMCMIntegration
			.Add(
				I18n.Setting_WeatherTooltip,
				I18n.Setting_WeatherTooltip_About,
				c => c.ShowWeatherTooltip,
				(c, v) => c.ShowWeatherTooltip = v
			)
			.AddLabel("")
			.Add(
				I18n.Setting_Debug,
				I18n.Setting_Debut_About,
				c => c.ShowDebugTiming,
				(c, v) => c.ShowDebugTiming = v
			);

	}

	private void ResetConfig() {
		Config = new();
	}

	private void SaveConfig() {
		Helper.WriteConfig(Config);
	}

	#endregion

	#region Events

	[Subscriber]
	private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {

		GMCMIntegration = new(this, () => Config, ResetConfig, SaveConfig);
		RegisterSettings();

		GameStateQuery.Register("leclair.cloudyskies_LOCATION_IGNORE_DEBRIS_WEATHER", LocationIgnoreDebrisWeather);

		if (!GameStateQuery.Exists("LOCATION_IGNORE_DEBRIS_WEATHER"))
			GameStateQuery.RegisterAlias("LOCATION_IGNORE_DEBRIS_WEATHER", "leclair.cloudyskies_LOCATION_IGNORE_DEBRIS_WEATHER");


		Helper.ConsoleCommands.Add("cs_reload", "Force the current weather layers to re-generate.", (_, _) => {
			UncacheLayers();
			Log($"Invalidated layer cache.", LogLevel.Info);
		});

		Helper.ConsoleCommands.Add("cs_list", "List the available custom weather Ids.", (_, _) => {
			LoadWeatherData();

			List<string[]> table = new();

			string? weather = Game1.currentLocation?.GetWeather().Weather;
			string? tomorrow = Game1.weatherForTomorrow;

			foreach(var entry in Data) {
				int count = entry.Value.Layers is null ? 0 : entry.Value.Layers.Count;

				table.Add([
					weather == entry.Key ? "**" : "",
					tomorrow == entry.Key ? "**" : "",
					entry.Key,
					TokenParser.ParseText(entry.Value.DisplayName ?? ""),
					$"{count}"
				]);
			}

			LogTable([
				"Active",
				"Tomorrow",
				"Id",
				"Display Name",
				"Layer Count"
			], table, LogLevel.Info);
		});

		Helper.ConsoleCommands.Add("cs_tomorrow", "Force tomorrow's weather to have a specific type in your current location.", (_, args) => {
			if (!Game1.IsMasterGame) {
				Log($"Only the host can do this.", LogLevel.Error);
				return;
			}

			string input = string.Join(' ', args);
			if (string.IsNullOrWhiteSpace(input)) { 
				Log($"Invalid weather provided.", LogLevel.Error);
				return;
			}

			Game1.currentLocation.GetWeather().WeatherForTomorrow = input;
			if (Game1.currentLocation.GetLocationContextId() == "Default")
				Game1.weatherForTomorrow = input;

			Log($"Changed tomorrow's weather to: {input}", LogLevel.Info);
		});

	}

	[Subscriber]
	private void OnWindowResized(object? sender, WindowResizedEventArgs e) {
		// Send a resize event to all our layers.
		var layers = CachedLayers.Value;
		if (layers.HasValue && layers.Value.Layers is not null)
			foreach (var layer in layers.Value.Layers)
				layer.Resize(e.NewSize, e.OldSize);
	}

	[Subscriber]
	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e) {

		// Gaze upon my test data in fear.
		/*if (e.Name.IsEquivalentTo(@"Data/LocationContexts"))
			e.Edit(editor => {

				var data = editor.AsDictionary<string, LocationContextData>();

				foreach(var pair in data.Data) {
					var entry = pair.Value;
					entry.WeatherConditions.Clear();

					if (pair.Key == "Default")
						entry.WeatherConditions.Add(new() {
							Id = "KhloeTest",
							Condition = "TRUE",
							Weather = "KhloeTest"
						});
					else
						entry.WeatherConditions.Add(new() {
							Id = "KhloeOtherTest",
							Condition = "TRUE",
							Weather = "KhloeOtherTest"
						});
				}

			}, AssetEditPriority.Late);*/

		if (e.Name.IsEquivalentTo(DATA_ASSET))
			e.LoadFrom(() => new Dictionary<string, WeatherData>(), priority: AssetLoadPriority.Exclusive);
	}


	[Subscriber]
	private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e) {

		HashSet<ulong> MatchedLayers = new();

		foreach(var name in e.Names) {
			if (name.IsEquivalentTo(DATA_ASSET)) {
				Log($"Invalidated our weather data.", LogLevel.Info);
				Data = null;
			}

			if (AssetsByName.TryGetValue(name.BaseName, out var layers))
				foreach (ulong id in layers)
					MatchedLayers.Add(id);
		}

		// TODO: Better invalidation logic to apply certain changes immediately
		// for a better developer experience.

		if (Data is not null && MatchedLayers.Count > 0) {
			foreach(var pair in CachedLayers.GetActiveValues()) { 
				if (pair.Value.HasValue && pair.Value.Value.Layers is not null)
					foreach(var layer in pair.Value.Value.Layers) {
						if (MatchedLayers.Contains(layer.Id))
							layer.ReloadAssets();
					}
			}
		}
	}

	[Subscriber]
	private void OnRenderedHud(object? sender, RenderedHudEventArgs e) {

		double drawing = DrawTiming.Value;
		double updating = UpdateTiming.Value;

		DrawTiming.Value = 0;
		UpdateTiming.Value = 0;

		if (!Config.ShowDebugTiming || (drawing <= 0 && updating <= 0))
			return;

		var builder = SimpleHelper.Builder()
			.Text(string.Format("Update: {0:0.0000} ms", updating))
			.Text(string.Format("  Draw: {0:0.0000} ms", drawing));

		if (Game1.oldKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift)) {
			var weather = Game1.currentLocation.GetWeather();

			builder = builder
				.Divider(false)
				.Text($"Weather: {weather.Weather}")
				.Text($"Tomorrow: {weather.WeatherForTomorrow}")
				.Divider(false)
				.Text($"Raining: {weather.IsRaining}")
				.Text($"Snowing: {weather.IsSnowing}")
				.Text($"Lightning: {weather.IsLightning}")
				.Text($"Debris: {weather.IsDebrisWeather}")
				.Text($"???: {weather.IsGreenRain}");
		}

		builder
			.GetLayout()
			.DrawHover(e.SpriteBatch, Game1.smallFont, overrideX: 0, overrideY: 0);

	}


	#endregion

	#region Loading

	[MemberNotNull(nameof(Data))]
	internal void LoadWeatherData() {
		if (Data is not null)
			return;

		Data = Helper.GameContent.Load<Dictionary<string, WeatherData>>(DATA_ASSET);

		// Normalize all the Id fields, as well as de-duplicating layer Ids.
		foreach (var entry in Data) {
			entry.Value.Id = entry.Key;

			if (entry.Value.Layers is not null) {
				HashSet<string> seen_ids = new();
				int i = 0;

				foreach (var layer in entry.Value.Layers) {
					if (string.IsNullOrEmpty(layer.Id) || ! seen_ids.Add(layer.Id)) {
						layer.Id = $"{i}#{layer.Type}";
						seen_ids.Add(layer.Id);
					}
					i++;
				}
			}
		}
	}


	public bool TryGetWeather(string? key, [NotNullWhen(true)] out WeatherData? weather) {
		if (key is null) {
			weather = null;
			return false;
		}

		if (key == CachedWeatherName.Value) {
			weather = CachedWeather.Value;
			return weather is not null;
		}

		LoadWeatherData();
		return Data.TryGetValue(key, out weather);
	}

	public void MarkLoadsAsset(ulong id, string path) {
		if (!AssetsByLayer.TryGetValue(id, out var layerAssets)) {
			layerAssets = new();
			AssetsByLayer[id] = layerAssets;
		}

		if (!layerAssets.Add(path))
			return;

		if (!AssetsByName.TryGetValue(path, out var assetLayers)) {
			assetLayers = new();
			AssetsByName[path] = assetLayers;
		}

		assetLayers.Add(id);
	}

	public void RemoveLoadsAsset(ulong id) {
		if (!AssetsByLayer.TryGetValue(id, out var layerAssets))
			return;

		AssetsByLayer.Remove(id);

		foreach(string path in layerAssets) {
			if (AssetsByName.TryGetValue(path, out var assetLayers) && assetLayers.Remove(id)) {
				if (assetLayers.Count == 0)
					AssetsByName.Remove(path);
			}
		}
	}

	public void RemoveLoadsAsset(ulong id, string path) {
		if (!AssetsByLayer.TryGetValue(id, out var layerAssets) || ! layerAssets.Remove(path))
			return;

		if (AssetsByName.TryGetValue(path, out var assetLayers))
			assetLayers.Remove(id);
	}

	#endregion

	#region Per-Screen Data Cache

	private readonly PerScreen<string?> CachedWeatherName = new();
	private readonly PerScreen<WeatherData?> CachedWeather = new();
	private readonly PerScreen<LayerCache?> CachedLayers = new();


	internal void UncacheLayers(string? weatherId = null) {
		foreach(var entry in CachedLayers.GetActiveValues()) {
			string? id = CachedWeatherName.GetValueForScreen(entry.Key);
			if (weatherId != null && id != weatherId)
				continue;

			if (entry.Value.HasValue) {
				var thing = entry.Value.Value;
				thing.Hour = -1;
				CachedLayers.SetValueForScreen(entry.Key, thing);
			}
		}
	}


	internal List<IWeatherLayer>? GetCachedWeatherLayers(GameLocation? location = null, int? timeOfDay = null) {
		var data = CachedWeather.Value;
		location ??= Game1.currentLocation;
		if (location is null || data?.Layers is null || data.Layers.Count == 0)
			return null;

		int hour = (timeOfDay ?? Game1.timeOfDay) / 100;

		LayerCache? cache = CachedLayers.Value;
		if (cache.HasValue && cache.Value.Data == data && cache.Value.EventUp == Game1.eventUp && cache.Value.Hour == hour && cache.Value.Location == location)
			return cache.Value.Layers;

		var old_by_id = cache?.LayersById;
		var old_data_by_id = cache?.DataById;

		Dictionary<string, IWeatherLayer> layersById = new();
		Dictionary<string, BaseLayerData> dataById = new();
		List<IWeatherLayer> result = new();
		HashSet<string> groups = new();

		GameStateQueryContext ctx = new(location, Game1.player, null, null, Game1.random);

		int reused = 0;
		int instanced = 0;

		foreach(var layer in data.Layers) {
			if (layer.Group != null && groups.Contains(layer.Group))
				continue;

			if (!string.IsNullOrEmpty(layer.Condition) && !GameStateQuery.CheckConditions(layer.Condition, ctx))
				continue;

			if (layer.Group != null)
				groups.Add(layer.Group);

			IWeatherLayer instance;

			// We rely upon record value equality checks.
			if (old_by_id is not null && old_data_by_id is not null &&
				old_by_id.TryGetValue(layer.Id, out var existing) &&
				old_data_by_id.TryGetValue(layer.Id, out var existingData) &&
				existingData == layer
			) {
				instance = existing;
				reused++;

			} else
				try {
					// TODO: Better way of instantiating based on type.
					if (layer is ColorLayerData colorData)
						instance = new ColorLayer(lastLayerId, colorData);
					else if (layer is SnowLayerData snowData)
						instance = new SnowLayer(this, lastLayerId, snowData);
					else if (layer is RainLayerData rainData)
						instance = new RainLayer(this, lastLayerId, rainData);
					else if (layer is TextureScrollLayerData texScrollData)
						instance = new TextureScrollLayer(this, lastLayerId, texScrollData);
					else if (layer is DebrisLayerData debrisData)
						instance = new DebrisLayer(this, lastLayerId, debrisData);
					else
						throw new ArgumentException($"unknown data type: {layer.Type}");

					instanced++;

				} catch (Exception ex) {
					Log($"Unable to instantiate weather layer '{layer.Id}': {ex}", LogLevel.Warn);
					continue;
				}

			dataById[layer.Id] = layer;
			layersById[layer.Id] = instance;
			result.Add(instance);
		}

		int skipped = data.Layers.Count - (reused + instanced);
#if DEBUG
		LogLevel level = LogLevel.Debug;
#else
		LogLevel level = LogLevel.Trace;
#endif

		Log($"Regenerated weather layers for: {Game1.player.displayName}\n\tReused {reused} layer instances.\n\tCreated {instanced} new layer instances.\n\tSkipped {skipped} layers.", level);

		CachedLayers.Value = new() {
			Data = data,
			DataById = dataById,
			EventUp = Game1.eventUp,
			LayersById = layersById,
			Location = location,
			Hour = hour,
			Layers = result.Count > 0 ? result : null
		};

		return result.Count > 0 ? result : null;
	}

	/// <summary>
	/// Use a per-screen cache to locate our weather data. We cache the
	/// data rather than performing dictionary lookups for optimal
	/// performance in a very hot loop.
	/// </summary>
	/// <param name="name">The current location's weather's name.
	/// If this changes, we re-cache the weather data from the
	/// <see cref="Data"/> dictionary.</param>
	/// <returns>The weather data, if any exists.</returns>
	internal WeatherData? GetCachedWeatherData(string? name) {
		if (Data is not null && CachedWeatherName.Value == name)
			return CachedWeather.Value;

		WeatherData? result;
		if (name != null) {
			LoadWeatherData();
			Data.TryGetValue(name, out result);
		} else
			result = null;

		CachedWeatherName.Value = name;
		CachedWeather.Value = result;
		return result;
	}

#endregion

}
