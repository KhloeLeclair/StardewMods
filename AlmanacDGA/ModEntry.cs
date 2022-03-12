using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leclair.Stardew.Common.Events;

using Leclair.Stardew.Almanac;

using DynamicGameAssets;
using DynamicGameAssets.PackData;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

namespace Leclair.Stardew.AlmanacDGA {
	public class ModEntry : ModSubscriber {

		IAlmanacAPI API;

		[Subscriber]
		private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {

			API = Helper.ModRegistry.GetApi<IAlmanacAPI>("leclair.almanac");
			if (API == null)
				Log("Unable to get Almanac's API.", LogLevel.Error);

			API?.SetCropPriority(ModManifest, 10);
			API?.SetCropCallback(ModManifest, UpdateCrops);
		}

		[Subscriber]
		private void OnDayStart(object sender, DayStartedEventArgs e) {
			// Invalidate the crop cache every day because the valid
			// DGA crops could change every day.
			API?.InvalidateCrops();
		}

		private Tuple<WorldDate, WorldDate> ParseConditions(DynamicFieldData[] dynamicFields) {
			// TODO: Actually parse the conditions to see when
			// we can grow this crop.
			bool? spring = null;
			bool? summer = null;
			bool? fall = null;
			bool? winter = null;

			foreach (var data in dynamicFields) {
				// Does this field affect CanGrowNow?
				bool affects = false;
				bool value = false;

				foreach(var field in data.Fields) {
					if (field.Key != "CanGrowNow" || field.Value.Type != JTokenType.Boolean)
						continue;

					affects = true;
					value = (bool) field.Value;
					break;
				}

				// Do we know what this does? No. Do we care? Also no.
				if (!affects)
					continue;

				// If we aren't enabling it, we don't care.
				if (!value)
					return null;

				// We know it affects CanGrowNow. But what *is* the condition?
				foreach(var cond in data.Conditions) {
					if (string.IsNullOrEmpty(cond.Key))
						continue;

					string[] bits = cond.Key.Split('|');
					string token = bits[0]?.Trim()?.ToLowerInvariant();
					string[] choices;

					bool cval;

					if (bits.Length < 2) {
						// No contains=. Expect a list elsewhere.
						cval = true;
						choices = cond.Value.Split(',').Select(x => x.Trim()).ToArray();

					} else {
						// We only support |contains=
						string c = bits[1].Trim().ToLowerInvariant();
						if (!c.StartsWith("contains="))
							return null;

						cval = cond.Value?.Trim()?.ToLowerInvariant() == "true";
						choices = c.Substring(9).Split(',').Select(x => x.Trim()).ToArray();
					}

					switch(token) {
						case "season":
							if (choices.Contains("spring")) {
								if (spring.HasValue && spring.Value != cval)
									return null;
								spring = cval;
							}
							if (choices.Contains("summer")) {
								if (summer.HasValue && summer.Value != cval)
									return null;
								summer = cval;
							}
							if (choices.Contains("fall")) {
								if (fall.HasValue && fall.Value != cval)
									return null;
								fall = cval;
							}
							if (choices.Contains("winter")) {
								if (winter.HasValue && winter.Value != cval)
									return null;
								winter = cval;
							}
							break;

						case "year":
							// Still not supported.
							return null;

						default:
							// Unsupported condition! Abandon ship!
							return null;
					}
				}
			}

			WorldDate start;
			WorldDate end;

			bool _spring = spring ?? false;
			bool _summer = summer ?? false;
			bool _fall = fall ?? false;
			bool _winter = winter ?? false;

			int lastDay = API.DaysPerMonth;

			// Stupid logic we use because the Crops page can't display crops
			// with gaps in their coverage. But what about this method isn't
			// stupid, to be honest?
			if (_spring) {
				start = new(1, "spring", 1);
				if (_summer) {
					if (_fall) {
						if (_winter)
							end = new(1, "winter", lastDay);
						else
							end = new(1, "fall", lastDay);
					} else
						end = new(1, "summer", lastDay);
				} else
					end = new(1, "spring", lastDay);

			} else if (_summer) {
				start = new(1, "summer", 1);
				if (_fall) {
					if (_winter)
						end = new(1, "winter", lastDay);
					else
						end = new(1, "fall", lastDay);
				} else
					end = new(1, "summer", lastDay);

			} else if (_fall) {
				start = new(1, "fall", 1);
				if (_winter)
					end = new(1, "winter", lastDay);
				else
					end = new(1, "fall", lastDay);

			} else if (_winter) {
				start = new(1, "winter", 1);
				end = new(1, "winter", lastDay);

			} else
				return null;

			return new Tuple<WorldDate, WorldDate>(start, end);
		}

		private void ProcessCrop(CropPackData crop, ContentPack pack) {
			var dates = ParseConditions(crop.DynamicFields);
			if (dates == null && !crop.CanGrowNow)
				return;

			Item citem = null;

			List<int> phases = new();
			List<Tuple<Texture2D, Rectangle?, Color?, Texture2D, Rectangle?, Color?>> phaseSprites = new();

			IReflectedMethod GetMultiTexture = Helper.Reflection.GetMethod(pack, "GetMultiTexture");

			bool trellis = false;

			int return_idx = -1;

			var last = crop.Phases.Last();

			foreach (var phase in crop.Phases) {

				trellis |= phase.Trellis;

				// Make a note of the last harvest result.
				if (phase.HarvestedDrops.Count > 0) {
					var choices = phase.HarvestedDrops[0].Item;
					if (choices.Count > 0)
						citem = choices[0].Value.Create();
				}

				return_idx = phase.HarvestedNewPhase;

				// Zero length phases need not apply. Except the last one.
				if (phase.Length <= 0 && phase != last)
					continue;

				// Still we don't *add* the last zero length phase. We
				// do want its sprite, though.
				if (phase.Length > 0)
					phases.Add(phase.Length);

				// Add the sprite.
				TexturedRect tex = GetMultiTexture.Invoke<TexturedRect>(phase.TextureChoices, 0, 16, 32);
				TexturedRect colored = phase.TextureColorChoices != null && phase.TextureColorChoices.Length > 0 ? GetMultiTexture.Invoke<TexturedRect>(phase.TextureColorChoices, 0, 16, 32) : null;

				phaseSprites.Add(new(
					tex.Texture,
					tex.Rect,
					null,
					colored?.Texture,
					colored?.Rect,
					null
				));
			}

			int regrow = 0;
			if (return_idx >= 0 && return_idx < phases.Count) {
				for (int i = return_idx; i < phases.Count; i++)
					regrow += phases[i];
			}

			// Because Dynamic Game Assets and Content Patcher
			// are flexible and opaque, we can't query the crop
			// for information like, say, the date range over
			// which it grows.

			// We have a method that tries to parse conditions,
			// but if there's anything beyond the basics, we
			// can't handle them.

			// So if our method fails, we only show crops that
			// are active NOW and assume they're active for
			// this entire season.

			WorldDate earliest = dates?.Item1 ?? new(1, Game1.currentSeason, 1);
			WorldDate latest = dates?.Item2 ?? new(1, Game1.currentSeason, API.DaysPerMonth);

			// Is it a giant crop?
			Tuple<Texture2D, Rectangle?, Color?, Texture2D, Rectangle?, Color?> giantSprite = null;
			bool giant = false;
			if (crop.GiantChance > 0 && crop.GiantTextureChoices != null) {
				TexturedRect tex = GetMultiTexture.Invoke<TexturedRect>(crop.GiantTextureChoices, 0, 48, 64);
				if (tex != null) {
					giant = true;
					giantSprite = new(
						tex.Texture,
						tex.Rect,
						null,
						null,
						null,
						null
					);
				}
			}

			Log($"Registering: {crop.ID}", LogLevel.Trace);

			API.AddCrop(
				ModManifest,
				id: crop.ID,
				item: citem,
				name: citem?.DisplayName ?? "???",
				regrow: regrow,
				isGiantCrop: giant,
				isPaddyCrop: crop.Type == CropPackData.CropType.Paddy,
				isTrellisCrop: trellis,
				start: earliest,
				end: latest,
				sprite: null,
				giantSprite: giantSprite,
				phases,
				phaseSprites
			);
		}

		private void UpdateCrops() {
			Log("Called UpdateCrops", LogLevel.Trace);
			API.ClearCrops(ModManifest);

			foreach (var pack in DynamicGameAssets.Mod.GetPacks()) {
				foreach (var item in pack.GetItems()) {
					if (!item.Enabled)
						continue;

					if (item is not CropPackData crop)
						continue;

					try {
						ProcessCrop(crop, pack);
					} catch (Exception ex) {
						Log($"Encountered an error while loading: {pack.GetManifest().UniqueID}/{crop.ID}", LogLevel.Warn, ex);
					}
				}
			}
		}
	}
}
