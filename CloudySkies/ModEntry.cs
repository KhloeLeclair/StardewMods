using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HarmonyLib;

using Leclair.Stardew.CloudySkies.Effects;
using Leclair.Stardew.CloudySkies.Integrations.ContentPatcher;
using Leclair.Stardew.CloudySkies.LayerData;
using Leclair.Stardew.CloudySkies.Layers;
using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.Common.Integrations.GenericModConfigMenu;
using Leclair.Stardew.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Mods;

namespace Leclair.Stardew.CloudySkies;


public partial class ModEntry : PintailModSubscriber {

	public static ModEntry Instance { get; private set; } = null!;

	public const string WEATHER_TOTEM_DATA = @"leclair.cloudyskies/WeatherTotem";

	public const string ALLOW_TOTEM_DATA = @"leclair.cloudyskies/AllowTotem:";

	public const string DATA_ASSET = @"Mods/leclair.cloudyskies/WeatherData";
	public const string EXTENSION_DATA_ASSET = @"Mods/leclair.cloudyskies/LocationContextExtensionData";

	public const string WEATHER_OVERLAY_DEFAULT_ASSET = @"Mods/leclair.cloudyskies/Textures/WeatherOverlay";

	private static readonly Func<ModHooks> HookDelegate = AccessTools.Field(typeof(Game1), "hooks").CreateGetter<ModHooks>();

#nullable disable

	public ModConfig Config { get; private set; }

	internal Harmony Harmony;

#nullable enable

	internal GMCMIntegration<ModConfig, ModEntry>? intGMCM;
	internal CPIntegration? intCP;

	private ulong lastLayerId = 0;

	internal readonly Dictionary<ulong, HashSet<string>> AssetsByLayer = new();
	internal readonly Dictionary<string, HashSet<ulong>> AssetsByName = new();

	internal Dictionary<string, WeatherData>? Data;
	internal Dictionary<string, LocationContextExtensionData>? ContextData;

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
		Patches.SObject_Patches.Patch(this);
		Patches.TV_Patches.Patch(this);

		// Read Config
		Config = Helper.ReadConfig<ModConfig>();

		// Init
		I18n.Init(Helper.Translation);

		// Register some early stuff.
		RegisterQueries();

	}

	public override object? GetApi(IModInfo mod) {
		return new ModApi(this, mod.Manifest);
	}

	#region Configuration

	private void RegisterSettings() {
		if (intGMCM is null || !intGMCM.IsLoaded)
			return;

		intGMCM.Register(true);

		intGMCM
			.Add(
				I18n.Setting_ReplaceTvMenu,
				I18n.Setting_ReplaceTvMenu_About,
				c => c.ReplaceTVMenu,
				(c, v) => c.ReplaceTVMenu = v
			)
			.Add(
				I18n.Setting_WeatherTooltip,
				I18n.Setting_WeatherTooltip_About,
				c => c.ShowWeatherTooltip,
				(c, v) => c.ShowWeatherTooltip = v
			)
			.AddLabel("")
			.Add(
				I18n.Setting_Debug,
				I18n.Setting_Debug_About,
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

		intGMCM = new(this, () => Config, ResetConfig, SaveConfig);
		intCP = new(this);

		RegisterSettings();

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

		if (e.Name.IsEquivalentTo(DATA_ASSET))
			e.LoadFrom(() => new Dictionary<string, WeatherData>(), priority: AssetLoadPriority.Exclusive);

		else if (e.Name.IsEquivalentTo(WEATHER_OVERLAY_DEFAULT_ASSET))
			e.LoadFromModFile<Texture2D>("assets/WeatherChannelOverlay.png", AssetLoadPriority.Low);

		else if (e.Name.IsEquivalentTo(EXTENSION_DATA_ASSET))
			e.LoadFrom(() => new Dictionary<string, LocationContextExtensionData>() {
				{ "Default", new() {
					DisplayName = "[LocalizedText location.stardew-valley]",
					IncludeInWeatherChannel = true,
				} },
				{ "Desert", new() {
					DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:Utility.cs.5861]",
					IncludeInWeatherChannel = true,
					WeatherChannelCondition = "PLAYER_HAS_MAIL Host ccVault Any",
					WeatherChannelBackgroundTexture = "LooseSprites\\map",
					WeatherChannelBackgroundSource = Point.Zero,
					WeatherChannelBackgroundFrames = 1
				} },
				{ "Island", new() {
					IncludeInWeatherChannel = true,
					DisplayName = "[LocalizedText Strings\\StringsFromCSFiles:IslandName]",
					WeatherChannelCondition = "PLAYER_HAS_MAIL Current Visited_Island Any",
					WeatherForecastPrefix = "[LocalizedText Strings\\StringsFromCSFiles:TV_IslandWeatherIntro]",
					WeatherChannelOverlayTexture = "LooseSprites\\Cursors2",
					WeatherChannelOverlayIntroSource = new Point(148, 62),
					WeatherChannelOverlayIntroFrames = 1,
					WeatherChannelOverlayWeatherSource = new Point(148, 62),
					WeatherChannelOverlayWeatherFrames = 1
				} }
			}, priority: AssetLoadPriority.Exclusive);

	}

	[Subscriber]
	private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e) {

		HashSet<ulong> MatchedLayers = new();

		foreach (var name in e.Names) {
			if (name.IsEquivalentTo(DATA_ASSET)) {
				Log($"Invalidated our weather data.", LogLevel.Debug);
				Data = null;
				CachedTryGetName.ResetAllScreens();
				CachedTryGetValue.ResetAllScreens();
				CachedWeather.ResetAllScreens();
			}

			if (name.IsEquivalentTo(EXTENSION_DATA_ASSET)) {
				Log($"Invalidated our location context extension data.", LogLevel.Debug);
				ContextData = null;
			}

			if (AssetsByName.TryGetValue(name.BaseName, out var layers))
				foreach (ulong id in layers)
					MatchedLayers.Add(id);
		}

		// TODO: Better invalidation logic to apply certain changes immediately
		// for a better developer experience.

		if (Data is not null && MatchedLayers.Count > 0) {
			foreach (var pair in CachedLayers.GetActiveValues()) {
				if (pair.Value.HasValue && pair.Value.Value.Layers is not null)
					foreach (var layer in pair.Value.Value.Layers) {
						if (MatchedLayers.Contains(layer.Id))
							layer.ReloadAssets();
					}
			}
			foreach (var pair in CachedEffects.GetActiveValues()) {
				if (pair.Value.HasValue && pair.Value.Value.Effects is not null)
					foreach (var effect in pair.Value.Value.Effects) {
						if (MatchedLayers.Contains(effect.Id))
							effect.ReloadAssets();
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
				.Text($"???: {weather.IsGreenRain}")
				.Divider(false)
				.Text($"Draw Lighting: {Game1.drawLighting}")
				.Text($"Ambient: {Game1.ambientLight}")
				.Text($"Outdoor: {Game1.outdoorLight}");
		}

		builder
			.GetLayout()
			.DrawHover(e.SpriteBatch, Game1.smallFont, overrideX: 0, overrideY: 0);

	}

	[Subscriber]
	private void OnUpdating(object? sender, UpdateTickingEventArgs e) {

		// Do not process effects when time is not passing. We need a special
		// check for when the process is out of focus, since this event still
		// fires in that situation. Also don't process effects in events.
		if (!Game1.game1.IsActiveNoOverlay && Game1.options.pauseWhenOutOfFocus && Game1.multiplayerMode == 0)
			return;
		if (Game1.eventUp || !Game1.shouldTimePass())
			return;

		// See if we have effects. If we don't, return.
		var effects = GetCachedWeatherEffects(Game1.player!.currentLocation, Game1.timeOfDay);
		if (effects is null)
			return;

		foreach (var effect in effects) {
			// Only run effects at a multiple of their Rate.
			if (e.IsMultipleOf(effect.Rate))
				effect.Update(Game1.currentGameTime);
		}
	}

	#endregion

	#region Loading

	[MemberNotNull(nameof(ContextData))]
	internal void LoadContextData() {
		if (ContextData is not null)
			return;

		ContextData = Helper.GameContent.Load<Dictionary<string, LocationContextExtensionData>>(EXTENSION_DATA_ASSET);

		// Normalize all the Id fields.
		foreach (var entry in ContextData)
			entry.Value.Id = entry.Key;
	}


	public bool TryGetContextData(string? id, [NotNullWhen(true)] out LocationContextExtensionData? data) {
		if (id is null) {
			data = null;
			return false;
		}

		LoadContextData();
		return ContextData.TryGetValue(id, out data);
	}

	[MemberNotNull(nameof(Data))]
	internal void LoadWeatherData() {
		if (Data is not null)
			return;

		Data = Helper.GameContent.Load<Dictionary<string, WeatherData>>(DATA_ASSET);

		// Normalize all the Id fields, as well as de-duplicating layer Ids.
		foreach (var entry in Data) {
			entry.Value.Id = entry.Key;

			// Un-box all the things we can.
			if (entry.Value.Lighting is not null) {
				for (int i = 0; i < entry.Value.Lighting.Count; i++) {
					var item = entry.Value.Lighting[i];
					if (item is not ScreenTintData && TryUnproxy(item, out object? obj) && obj is ScreenTintData sd)
						entry.Value.Lighting[i] = sd;
				}
			}

			if (entry.Value.Layers is not null) {
				for (int i = 0; i < entry.Value.Layers.Count; i++) {
					var item = entry.Value.Layers[i];
					if (item is not BaseLayerData && TryUnproxy(item, out object? obj) && obj is BaseLayerData ld)
						entry.Value.Layers[i] = ld;
				}
			}

			if (entry.Value.Effects is not null) {
				for (int i = 0; i < entry.Value.Effects.Count; i++) {
					var item = entry.Value.Effects[i];
					if (item is not BaseEffectData && TryUnproxy(item, out object? obj) && obj is BaseEffectData ed)
						entry.Value.Effects[i] = ed;
				}
			}

			// Migrate to the new lighting format.
			if (entry.Value.AmbientColor.HasValue || entry.Value.LightingTint.HasValue || entry.Value.PostLightingTint.HasValue) {
				if (entry.Value.Lighting is null || entry.Value.Lighting.Count == 0) {
					entry.Value.Lighting ??= new List<IScreenTintData>();

					if (entry.Value.AmbientOutdoorOpacity.HasValue && entry.Value.AmbientOutdoorOpacity.Value < 0.93) {
						entry.Value.Lighting.Add(new ScreenTintData() {
							AmbientColor = entry.Value.AmbientColor,
							AmbientOutdoorOpacity = entry.Value.AmbientOutdoorOpacity,
							LightingTint = entry.Value.LightingTint,
							LightingTintOpacity = entry.Value.LightingTintOpacity,
							PostLightingTint = entry.Value.PostLightingTint,
							PostLightingTintOpacity = entry.Value.PostLightingTintOpacity
						});

						entry.Value.Lighting.Add(new ScreenTintData() {
							AmbientColor = entry.Value.AmbientColor,
							TimeOfDay = -200,
							TweenMode = LightingTweenMode.After,
							AmbientOutdoorOpacity = entry.Value.AmbientOutdoorOpacity,
							LightingTint = entry.Value.LightingTint,
							LightingTintOpacity = entry.Value.LightingTintOpacity,
							PostLightingTint = entry.Value.PostLightingTint,
							PostLightingTintOpacity = entry.Value.PostLightingTintOpacity
						});

						entry.Value.Lighting.Add(new ScreenTintData() {
							AmbientColor = entry.Value.AmbientColor,
							TimeOfDay = 0,
							AmbientOutdoorOpacity = 0.93f,
							LightingTint = entry.Value.LightingTint,
							LightingTintOpacity = entry.Value.LightingTintOpacity,
							PostLightingTint = entry.Value.PostLightingTint,
							PostLightingTintOpacity = entry.Value.PostLightingTintOpacity
						});

					} else
						entry.Value.Lighting.Add(new ScreenTintData() {
							AmbientColor = entry.Value.AmbientColor,
							AmbientOutdoorOpacity = entry.Value.AmbientOutdoorOpacity,
							LightingTint = entry.Value.LightingTint,
							LightingTintOpacity = entry.Value.LightingTintOpacity,
							PostLightingTint = entry.Value.PostLightingTint,
							PostLightingTintOpacity = entry.Value.PostLightingTintOpacity
						});

				} else
					Log($"Ignoring legacy lighting data for weather type '{entry.Key}' because Lighting is present and has data.", LogLevel.Warn);
			}

			if (entry.Value.Layers is not null) {
				HashSet<string> seen_ids = new();
				int i = 0;

				foreach (var layer in entry.Value.Layers) {
					if (string.IsNullOrEmpty(layer.Id) || !seen_ids.Add(layer.Id)) {
						layer.Id = $"{i}#{layer.Type}";
						seen_ids.Add(layer.Id);
					}
					i++;
				}
			}
		}
	}

	private readonly PerScreen<string?> CachedTryGetName = new();
	private readonly PerScreen<WeatherData?> CachedTryGetValue = new();

	/// <summary>
	/// Try to get weather data for the weather with the provided key.
	/// This uses an internal lookup cache between calls to improve
	/// performance, only falling back to a dictionary lookup if both
	/// caches aren't set to the value.
	/// </summary>
	/// <param name="key">The Id of the weather condition we want data for.</param>
	/// <param name="weather">The data, if it exists.</param>
	/// <returns>Whether or not the data exists.</returns>
	public bool TryGetWeather(string? key, [NotNullWhen(true)] out WeatherData? weather) {
		if (key is null) {
			weather = null;
			return false;
		}

		if (key == CachedTryGetName.Value) {
			weather = CachedTryGetValue.Value;
			return weather is not null;
		}

		if (key == CachedWeatherName.Value) {
			weather = CachedWeather.Value;
			return weather is not null;
		}

		LoadWeatherData();
		Data.TryGetValue(key, out weather);

		CachedTryGetName.Value = key;
		CachedTryGetValue.Value = weather;

		return weather is not null;
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

		foreach (string path in layerAssets) {
			if (AssetsByName.TryGetValue(path, out var assetLayers) && assetLayers.Remove(id)) {
				if (assetLayers.Count == 0)
					AssetsByName.Remove(path);
			}
		}
	}

	public void RemoveLoadsAsset(ulong id, string path) {
		if (!AssetsByLayer.TryGetValue(id, out var layerAssets) || !layerAssets.Remove(path))
			return;

		if (AssetsByName.TryGetValue(path, out var assetLayers))
			assetLayers.Remove(id);
	}

	#endregion

	#region Per-Screen Data Cache

	private readonly PerScreen<string?> CachedWeatherName = new();
	private readonly PerScreen<WeatherData?> CachedWeather = new();
	private readonly PerScreen<LayerCache?> CachedLayers = new();
	private readonly PerScreen<EffectCache?> CachedEffects = new();
	private readonly PerScreen<CachedTintData?> CachedTint = new();

	internal void UncacheLayers(string? weatherId = null, bool force_reinstance = false) {
		foreach (var entry in CachedLayers.GetActiveValues()) {
			string? id = CachedWeatherName.GetValueForScreen(entry.Key);
			if (weatherId != null && id != weatherId)
				continue;

			if (entry.Value.HasValue) {
				var thing = entry.Value.Value;
				thing.Hour = force_reinstance ? -2 : -1;
				CachedLayers.SetValueForScreen(entry.Key, thing);
			}
		}

		foreach (var entry in CachedEffects.GetActiveValues()) {
			string? id = CachedWeatherName.GetValueForScreen(entry.Key);
			if (weatherId != null && id != weatherId)
				continue;

			if (entry.Value.HasValue) {
				var thing = entry.Value.Value;
				thing.Hour = force_reinstance ? -2 : -1;
				CachedEffects.SetValueForScreen(entry.Key, thing);
			}
		}

		foreach (var entry in CachedTint.GetActiveValues()) {
			if (entry.Value.HasValue) {
				var thing = entry.Value.Value;
				thing.EndTime = 0;
				CachedTint.SetValueForScreen(entry.Key, thing);
			}
		}
	}


	internal CachedTintData? GetCachedTint(GameLocation? location = null) {
		location ??= Game1.player?.currentLocation;
		string? weather = location?.GetWeather()?.Weather;
		TryGetWeather(weather, out var data);
		if (location is null || data?.Lighting is null || data.Lighting.Count == 0)
			return null;

		int time = Game1.timeOfDay;

		CachedTintData? cache = CachedTint.Value;
		if (cache.HasValue &&
			cache.Value.Data == data &&
			cache.Value.EventUp == Game1.eventUp &&
			cache.Value.Location == location &&
			time >= cache.Value.StartTime &&
			time <= cache.Value.EndTime
		)
			return cache;

		GameStateQueryContext ctx = new(location, Game1.player, null, null, Game1.random);

		// We need to collect a start and end value.
		IScreenTintData? start = null;
		IScreenTintData? end = null;

		int darkTime = Game1.getTrulyDarkTime(location);
		int startDarkTime = Game1.getStartingToGetDarkTime(location);

		foreach (var tintData in data.Lighting) {
			if (!string.IsNullOrEmpty(tintData.Condition) && !GameStateQuery.CheckConditions(tintData.Condition, ctx))
				continue;

			int tod = tintData.TimeOfDay;
			if (tod <= 0)
				tod = darkTime - tod;

			if (tod <= time)
				start = tintData;

			else if (start != null && tod > time) {
				end = tintData;
				break;
			}
		}

		// If we have no data, return nothing basically.
		if (start == null)
			cache = new() {
				Data = data,
				Location = location,
				EventUp = Game1.eventUp,
				StartTime = int.MinValue,
				EndTime = int.MaxValue,
				DurationInTenMinutes = 1,
				HasAmbientColor = false,
				HasLightingTint = false,
				HasPostLightingTint = false,
			};

		else {
			bool can_tween = start.TweenMode.HasFlag(LightingTweenMode.After) &&
				end != null && end.TweenMode.HasFlag(LightingTweenMode.Before);

			int start_time = start.TimeOfDay;
			int end_time;

			if (end is null) {
				end_time = int.MaxValue;
				end = start;
			} else {
				end_time = end.TimeOfDay;
				if (!can_tween)
					end = start;
			}

			if (start_time <= 0)
				start_time = darkTime - start_time;
			if (end_time <= 0)
				end_time = darkTime - end_time;

			float start_opacity;
			if (start.AmbientOutdoorOpacity.HasValue)
				start_opacity = start.AmbientOutdoorOpacity.Value;
			else {
				int adjusted = (int) ((start_time - start_time % 100) + (start_time % 100 / 10) * 16.66f);
				if (start_time >= darkTime)
					start_opacity = Math.Min(0.93f, 0.75f + (adjusted - darkTime) * 0.000625f);
				else if (start_time >= startDarkTime) {
					start_opacity = Math.Min(0.93f, 0.3f + (adjusted - startDarkTime) * 0.00225f);
				} else
					start_opacity = 0.3f;
			}

			float end_opacity;

			if (end.AmbientOutdoorOpacity.HasValue)
				end_opacity = end.AmbientOutdoorOpacity.Value;
			else {
				int adjusted = (int) ((end_time - end_time % 100) + (end_time % 100 / 10) * 16.66f);
				if (start_time >= darkTime)
					end_opacity = Math.Min(0.93f, 0.75f + (adjusted - darkTime) * 0.000625f);
				else if (start_time >= startDarkTime) {
					end_opacity = Math.Min(0.93f, 0.3f + (adjusted - startDarkTime) * 0.00225f);
				} else
					end_opacity = 0.3f;
			}

			cache = new() {
				Data = data,
				Location = location,
				EventUp = Game1.eventUp,

				StartTime = start_time,
				EndTime = end_time,

				DurationInTenMinutes = Utility.CalculateMinutesBetweenTimes(start_time, end_time) / 10,

				HasAmbientColor = start.AmbientColor.HasValue || end.AmbientColor.HasValue,
				HasLightingTint = start.LightingTint.HasValue || end.LightingTint.HasValue,
				HasPostLightingTint = start.PostLightingTint.HasValue || end.PostLightingTint.HasValue,

				StartAmbientColor = start.AmbientColor ?? Color.White,
				EndAmbientColor = end.AmbientColor ?? Color.White,

				StartAmbientOutdoorOpacity = start_opacity,
				EndAmbientOutdoorOpacity = end_opacity,

				StartLightingTint = start.LightingTint ?? Color.White,
				EndLightingTint = end.LightingTint ?? Color.White,

				StartLightingTintOpacity = start.LightingTintOpacity,
				EndLightingTintOpacity = end.LightingTintOpacity,

				StartPostLightingTint = start.PostLightingTint ?? Color.White,
				EndPostLightingTint = end.PostLightingTint ?? Color.White,

				StartPostLightingTintOpacity = start.PostLightingTintOpacity,
				EndPostLightingTintOpacity = end.PostLightingTintOpacity
			};
		}

		CachedTint.Value = cache;
		return cache;
	}


	internal List<IEffect>? GetCachedWeatherEffects(GameLocation? location = null, int? timeOfDay = null) {
		var data = CachedWeather.Value;
		location ??= Game1.player?.currentLocation;
		if (location is null || data?.Effects is null || data.Effects.Count == 0)
			return null;

		int hour = (timeOfDay ?? Game1.timeOfDay) / 100;

		EffectCache? cache = CachedEffects.Value;
		if (cache.HasValue && cache.Value.Data == data && cache.Value.EventUp == Game1.eventUp && cache.Value.Hour == hour && cache.Value.Location == location)
			return cache.Value.Effects;

		bool force_reinstance = cache?.Hour == -2;
		var old_by_id = cache?.EffectsById;
		var old_data_by_id = cache?.DataById;

		Dictionary<string, IEffect> effectsById = new();
		Dictionary<string, IEffectData> dataById = new();
		List<IEffect> result = new();
		HashSet<string> groups = new();

		GameStateQueryContext ctx = new(location, Game1.player, null, null, Game1.random);

		int reused = 0;
		int instanced = 0;

		foreach (var effect in data.Effects) {
			if (effect.Group != null && groups.Contains(effect.Group))
				continue;

			if (!string.IsNullOrEmpty(effect.Condition) && !GameStateQuery.CheckConditions(effect.Condition, ctx))
				continue;

			if (effect.Group != null)
				groups.Add(effect.Group);

			IEffect instance;

			// We rely upon record value equality checks.
			if (old_by_id is not null && old_data_by_id is not null &&
				!force_reinstance &&
				old_by_id.TryGetValue(effect.Id, out var existing) &&
				old_data_by_id.TryGetValue(effect.Id, out var existingData) &&
				existingData == effect
			) {
				// Remove the old instance.
				old_by_id.Remove(effect.Id);
				instance = existing;
				reused++;

			} else
				try {
					// TODO: Better way of instantiating based on type.
					if (effect is ModifyHealthEffectData healthData)
						instance = new ModifyHealthEffect(lastLayerId, healthData);
					else if (effect is ModifyStaminaEffectData staminaData)
						instance = new ModifyStaminaEffect(lastLayerId, staminaData);
					else if (effect is BuffEffectData buffData)
						instance = new BuffEffect(this, lastLayerId, buffData);
					else if (effect is TriggerEffectData triggerData)
						instance = new TriggerEffect(this, lastLayerId, triggerData);
					else
						throw new ArgumentException($"unknown data type: {effect.Type}");

					lastLayerId++;
					instanced++;

				} catch (Exception ex) {
					Log($"Unable to instantiate weather layer '{effect.Id}': {ex}", LogLevel.Warn);
					continue;
				}

			dataById[effect.Id] = effect;
			effectsById[effect.Id] = instance;
			result.Add(instance);
		}

		// Clean up the old instances that weren't used.
		if (old_by_id is not null)
			foreach (var effect in old_by_id.Values)
				effect.Remove();

		int skipped = data.Effects.Count - (reused + instanced);
#if DEBUG
		LogLevel level = LogLevel.Debug;
#else
		LogLevel level = LogLevel.Trace;
#endif

		Log($"Regenerated weather effects for: {Game1.player?.displayName}\n\tReused {reused} effect instances.\n\tCreated {instanced} new effect instances.\n\tSkipped {skipped} effects.", level);

		CachedEffects.Value = new() {
			Data = data,
			DataById = dataById,
			EventUp = Game1.eventUp,
			EffectsById = effectsById,
			Location = location,
			Hour = hour,
			Effects = result.Count > 0 ? result : null
		};

		return result.Count > 0 ? result : null;
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

		bool force_reinstance = cache?.Hour == -2;
		var old_by_id = cache?.LayersById;
		var old_data_by_id = cache?.DataById;

		Dictionary<string, IWeatherLayer> layersById = new();
		Dictionary<string, ILayerData> dataById = new();
		List<IWeatherLayer> result = new();
		HashSet<string> groups = new();

		GameStateQueryContext ctx = new(location, Game1.player, null, null, Game1.random);

		int reused = 0;
		int instanced = 0;

		foreach (var layer in data.Layers) {
			if (layer.Group != null && groups.Contains(layer.Group))
				continue;

			if (!string.IsNullOrEmpty(layer.Condition) && !GameStateQuery.CheckConditions(layer.Condition, ctx))
				continue;

			if (layer.Group != null)
				groups.Add(layer.Group);

			IWeatherLayer instance;

			// We rely upon record value equality checks.
			if (old_by_id is not null && old_data_by_id is not null &&
				!force_reinstance &&
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
					else if (layer is TextureScrollLayerData texData)
						instance = new TextureScrollLayer(this, lastLayerId, texData);
					else if (layer is RainLayerData rainData)
						instance = new RainLayer(this, lastLayerId, rainData);
					else if (layer is DebrisLayerData debrisData)
						instance = new DebrisLayer(this, lastLayerId, debrisData);
					else if (layer is ParticleLayerData particleData)
						instance = new ParticleLayer(this, lastLayerId, particleData);
					else
						throw new ArgumentException($"unknown data type: {layer.Type}");

					lastLayerId++;
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
