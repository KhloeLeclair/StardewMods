#nullable enable

using System;
using System.Collections;

using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using Leclair.Stardew.Common.Integrations;

using JsonAssets;
using System.Linq;

namespace Leclair.Stardew.Almanac.Integrations.JsonAssets;

public class JAIntegration : BaseAPIIntegration<IJSONAssetsAPI, ModEntry> {

	public JAIntegration(ModEntry mod)
	: base(mod, "spacechase0.JsonAssets", "1.10.9") { }

	public bool IsGiantCrop(int id) {

		if (!IsLoaded)
			return false;

		int[] indices = API.GetGiantCropIndexes();
		if (indices is null)
			return false;

		return indices.Contains(id);
	}

	public Texture2D? GetGiantCropTexture(int id) {
		if (!IsLoaded)
			return null;

		if (API.TryGetGiantCropSprite(id, out var tex))
			return tex.Value;

		return null;
	}

}
