using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.GameData;

using StardewModdingAPI;

using SObject = StardewValley.Object;

namespace Leclair.Stardew.Almanac.Crops {
	public class VanillaProvider : ICropProvider {

		public readonly ModEntry Mod;

		public VanillaProvider(ModEntry mod) {
			Mod = mod;
		}

		public string Name => nameof(VanillaProvider);

		public int Priority => 0;

		public IEnumerable<CropInfo> GetCrops() {
			Dictionary<string, string> crops = Game1.content.Load<Dictionary<string, string>>(@"Data\Crops");
			List<CropInfo> result = new();

			foreach (var entry in crops) {
				try {
					CropInfo? info = GetCropInfo(entry.Key, entry.Value);
					if (info.HasValue)
						result.Add(info.Value);
				} catch (Exception ex) {
					ModEntry.Instance.Log($"Unable to process crop: {entry.Key}", LogLevel.Warn, ex);
				}
			}

			return result;
		}

		private CropInfo? GetCropInfo(string id, string data) {

			// TODO: Create Crop instances so we can
			// lean more on vanilla logic.

			string[] bits = data.Split('/');
			if (bits.Length < 5)
				return null;

			int sprite = Convert.ToInt32(bits[2]);
			string harvestID = bits[3];
			int regrow = Convert.ToInt32(bits[4]);

			bool isTrellisCrop = bits.Length > 7 && bits[7].Equals("true");

			if (!Game1.objectInformation.ContainsKey(harvestID))
				return null;

			string[] seasons = bits[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

			WorldDate startDate = null;
			WorldDate endDate = null;

			// TODO: Handle weird crops with a gap.

			foreach (string season in seasons) {
				WorldDate start;
				WorldDate end;

				try {
					start = new(1, season, 1);
					end = new(1, season, ModEntry.DaysPerMonth);

				} catch (Exception) {
					ModEntry.Instance.Log($"Invalid season for crop {id} (harvest:{harvestID}): {season}", LogLevel.Warn);
					return null;
				}

				if (startDate == null || startDate > start)
					startDate = start;
				if (endDate == null || endDate < end)
					endDate = end;
			}


			// Skip crops that don't have any valid seasons.
			// Who knows what else isn't valid?
			if (startDate == null || endDate == null)
				return null;

			// Skip crops that are always active.
			if (startDate.SeasonIndex == 0 && startDate.DayOfMonth == 1 && endDate.SeasonIndex == (WorldDate.MonthsPerYear - 1) && endDate.DayOfMonth == ModEntry.DaysPerMonth)
				return null;

			// If the sprite is 23, it's a seasonal multi-seed
			// so we want to show that rather than the seed.
			Item item;
			if (sprite == 23)
				item = Utility.CreateItemByID(id, 1);
			else
				item = Utility.CreateItemByID(harvestID, 1);

			// Phases
			int[] phases = bits[0].Split(' ').Select(val => Convert.ToInt32(val)).ToArray();

			// Stupid hard coded bullshit.
			// TODO: Wait for the new context tag / data bit and check that.
			bool paddyCrop = harvestID == "271" || harvestID == "830";

			// Phase Sprites
			// We need an extra sprite for the finished crop,
			// as well as one for regrow if that's enabled.
			SpriteInfo[] sprites = new SpriteInfo[phases.Length + 1 + (regrow > 0 ? 1 : 0)];

			// Are we dealing with colors?
			Color? color = null;
			string[] colorbits = regrow <= 0 && bits.Length > 8 ? bits[8].Split(' ') : null;
			if (colorbits != null && colorbits.Length >= 4 && colorbits[0].Equals("true")) {
				// Technically there could be many colors, but we just use
				// the first one.
				color = new(
					Convert.ToByte(colorbits[1]),
					Convert.ToByte(colorbits[2]),
					Convert.ToByte(colorbits[3])
				);
			}

			for (int i = 0; i < sprites.Length; i++) {
				bool final = i == (sprites.Length - (regrow > 0 ? 2 : 1));

				sprites[i] = new(
					Game1.cropSpriteSheet,
					new Rectangle(
						Math.Min(240, (i + 1) * 16 + (sprite % 2 != 0 ? 128 : 0)),
						sprite / 2 * 16 * 2,
						16, 32
					),
					overlaySource: final && color.HasValue ? new Rectangle(
						Math.Min(240, (i + 2) * 16 + (sprite % 2 != 0 ? 128 : 0)),
						sprite / 2 * 16 * 2,
						16, 32
					) : null,
					overlayColor: final ? color : null
				);
			}

			bool isGiantCrop = false;
			SpriteInfo giantSprite = null;

			// Giant Crops
			var giantCrops = Game1.content.Load<Dictionary<string, GiantCrops>>(@"Data\GiantCrops");
			if (giantCrops != null && giantCrops.TryGetValue(harvestID, out var gcdata) && gcdata != null) { 
				isGiantCrop = true;

				giantSprite = new(
					Game1.content.Load<Texture2D>(gcdata.Texture),
					new Rectangle(
						gcdata.Corner.X, gcdata.Corner.Y,
						gcdata.TileSize.X * 16,
						(gcdata.TileSize.Y + 1) * 16
					)
				);
			}

			return new(
				Convert.ToString(id),
				item,
				item.DisplayName,
				SpriteHelper.GetSprite(item),
				isGiantCrop,
				giantSprite,
				new Item[] {
					new SObject(id, 1)
				},
				isTrellisCrop,
				phases,
				regrow,
				paddyCrop,
				sprites,
				startDate,
				endDate
			);
		}
	}
}
