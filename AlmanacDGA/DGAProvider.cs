using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using DynamicGameAssets;
using DynamicGameAssets.PackData;

using Leclair.Stardew.Almanac;
using Leclair.Stardew.Common;

using StardewModdingAPI;

using StardewValley;


namespace Leclair.Stardew.AlmanacDGA {
	public class DGAProvider : ICropProvider {

		private readonly ModEntry Mod;

		public DGAProvider(ModEntry mod) {
			Mod = mod;
		}

		public int Priority => 10;

		public IEnumerable<CropInfo> GetCrops() {
			List<CropInfo> result = new();

			foreach (var pack in DynamicGameAssets.Mod.GetPacks()) {
				foreach (var item in pack.GetItems()) {
					if (!item.Enabled)
						continue;

					if (item is not CropPackData crop || !crop.CanGrowNow)
						continue;

					Item citem = null; // crop.ToItem();

					List<int> phases = new();
					List<SpriteInfo> sprites = new();

					IReflectedMethod GetMultiTexture = Mod.Helper.Reflection.GetMethod(pack, "GetMultiTexture");

					bool trellis = false;

					foreach (var phase in crop.Phases) {

						trellis |= phase.Trellis;

						// Make a note of the last harvest result.
						if (phase.HarvestedDrops.Count > 0) {
							var choices = phase.HarvestedDrops[0].Item;
							if (choices.Count > 0)
								citem = choices[0].Value.Create();
						}

						// Add the phase.
						phases.Add(phase.Length);

						// Add the sprite.
						TexturedRect tex = GetMultiTexture.Invoke<TexturedRect>(phase.TextureChoices, 0, 16, 32);
						TexturedRect colored = phase.TextureColorChoices != null && phase.TextureColorChoices.Length > 0 ? GetMultiTexture.Invoke<TexturedRect>(phase.TextureColorChoices, 0, 16, 32) : null;

						sprites.Add(new(
							tex.Texture,
							tex.Rect ?? tex.Texture.Bounds,
							overlayTexture: colored?.Texture,
							overlaySource: colored?.Rect
						));
					}

					// Because Dynamic Game Assets and Content Patcher
					// are stupid and opaque, we can't query the crop
					// for information like, say, the date range over
					// which it grows.

					// So instead, we only show crops that are active
					// NOW and assume they're active for this entire
					// month.

					// This is stupid shit and I don't like it.

					WorldDate earliest = new(1, Game1.currentSeason, 1);
					WorldDate latest = new(1, Game1.currentSeason, WorldDate.DaysPerMonth);


					result.Add(new CropInfo(
						crop.ID,
						citem,
						citem?.DisplayName ?? "???",
						citem == null ? null : SpriteHelper.GetSprite(citem, Mod.Helper),
						crop.GiantChance > 0,
						trellis,
						phases.ToArray(),
						0,
						crop.Type == CropPackData.CropType.Paddy,
						sprites.ToArray(),
						earliest,
						latest
					));
				}
			}

			return result;
		}
	}
}
