using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Leclair.Stardew.CloudySkies.Effects;
using Leclair.Stardew.CloudySkies.LayerData;
using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.Common.Types;

using StardewModdingAPI;

namespace Leclair.Stardew.CloudySkies;

public class ModApi : ICloudySkiesApi {

	private readonly ModEntry Mod;
	private readonly IManifest Other;

	public ModApi(ModEntry mod, IManifest other) {
		Mod = mod;
		Other = other;
	}

	public string WeatherAssetName => ModEntry.DATA_ASSET;

	public string ContextAssetName => ModEntry.EXTENSION_DATA_ASSET;

	public void RegenerateLayers(string? weatherId = null) {
		Mod.UncacheLayers(weatherId);
	}

	public IEnumerable<IWeatherData> GetAllCustomWeather() {
		Mod.LoadWeatherData();
		foreach (var data in Mod.Data)
			yield return data.Value;
	}

	public IEnumerable<ILocationContextExtensionData> GetAllContextData() {
		Mod.LoadContextData();
		foreach (var data in Mod.ContextData)
			yield return data.Value;
	}

	public bool TryGetContextData(string id, [NotNullWhen(true)] out ILocationContextExtensionData? data) {
		if (Mod.TryGetContextData(id, out var cdata)) {
			data = cdata;
			return data is not null;
		}

		data = null;
		return false;
	}

	public bool TryGetWeather(string id, [NotNullWhen(true)] out IWeatherData? data) {
		if (Mod.TryGetWeather(id, out var weather)) {
			data = weather;
			return data is not null;
		}

		data = null;
		return false;
	}

	private static readonly Dictionary<Type, Func<object?>> WeatherDataEditorTypes = new() {
		{ typeof(IWeatherData), () => new WeatherData() },
		{ typeof(IScreenTintData), () => new ScreenTintData() },

		// When creating our discriminated types, make sure to set Type.
		{ typeof(IColorLayerData), () => new ColorLayerData() {
			Type = "Color"
		} },

		{ typeof(IDebrisLayerData), () => new DebrisLayerData() {
			Type = "Debris"
		} },

		{ typeof(IRainLayerData), () => new RainLayerData() {
			Type = "Rain"
		} },

		{ typeof(ITextureScrollLayerData), () => new TextureScrollLayerData() {
			Type = "TextureScroll"
		} },

		{ typeof(IBuffEffectData), () => new BuffEffectData() {
			Type = "Buff"
		} },

		{ typeof(IModifyHealthEffectData), () => new ModifyHealthEffectData() {
			Type = "ModifyHealth"
		} },

		{ typeof(IModifyStaminaEffectData), () => new ModifyStaminaEffectData() {
			Type = "ModifyStamina"
		} },

		{ typeof(ITriggerEffectData), () => new TriggerEffectData() {
			Type = "Trigger"
		} }
	};

	public IModAssetEditor<IWeatherData> GetWeatherEditor(IAssetData assetData) {
		return new ModAssetEditor<ModEntry, WeatherData, IWeatherData>(
			Mod,
			Other,
			assetData,
			data => data.Id,
			(data, id) => data.Id = id,
			WeatherDataEditorTypes
		);
	}

	public IModAssetEditor<ILocationContextExtensionData> GetContextEditor(IAssetData assetData) {
		return new ModAssetEditor<ModEntry, LocationContextExtensionData, ILocationContextExtensionData>(
			Mod,
			Other,
			assetData,
			data => data.Id,
			(data, id) => data.Id = id
		);
	}
}
