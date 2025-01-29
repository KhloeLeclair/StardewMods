using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Leclair.Stardew.Common.Types;
using Leclair.Stardew.GiantCropTweaks.Models;

using StardewModdingAPI;


namespace Leclair.Stardew.GiantCropTweaks;

public class ModApi : IGiantCropTweaks {

	internal readonly ModEntry Mod;
	internal readonly IManifest Other;

	public ModApi(ModEntry mod, IManifest other) {
		Mod = mod;
		Other = other;
	}

	public IExtraGiantCropData CreateNew() {
		return new ExtraGiantCropData();
	}

	public IEnumerable<KeyValuePair<string, IExtraGiantCropData>> GetData() {
		Mod.LoadCropData();
		foreach (var pair in Mod.CropData)
			yield return new(pair.Key, pair.Value);
	}

	public IModAssetEditor<IExtraGiantCropData> GetEditor(IAssetData assetData) {
		return new ModAssetEditor<ModEntry, ExtraGiantCropData, IExtraGiantCropData>(
			Mod,
			Other,
			assetData,
			data => data.Id,
			(data, id) => data.Id = id,
			null
		);
	}

	public bool TryGetData(string key, [NotNullWhen(true)] out IExtraGiantCropData? data) {
		Mod.LoadCropData();
		if (Mod.CropData.TryGetValue(key, out var result)) {
			data = result;
			return true;
		}

		data = null;
		return false;
	}
}
