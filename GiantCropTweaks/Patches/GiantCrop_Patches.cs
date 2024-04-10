using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;

using HarmonyLib;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Extensions;
using Leclair.Stardew.GiantCropTweaks.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

using static StardewValley.Debris;

namespace Leclair.Stardew.GiantCropTweaks.Patches;

public static class GiantCrop_Patches {

	private static IMonitor? Monitor;

	private static GiantCrop? CurrentCrop;
	private static Vector2 CurrentPosition;
	private static GameLocation? CurrentLocation;

	public static void Patch(ModEntry mod) {
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.draw)),
				prefix: new HarmonyMethod(AccessTools.Method(typeof(GiantCrop_Patches), nameof(draw_Prefix)), priority: Priority.High)
			);

			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(GiantCrop), nameof(GiantCrop.performToolAction)),
				prefix: new HarmonyMethod(typeof(GiantCrop_Patches), nameof(performToolAction_Prefix)),
				postfix: new HarmonyMethod(typeof(GiantCrop_Patches), nameof(performToolAction_Postfix))
				//transpiler: new HarmonyMethod(typeof(GiantCrop_Patches), nameof(performToolAction_Transpiler))
			);

		} catch(Exception ex) {
			mod.Log($"An error occurred while registering a harmony patch for giant crops.", LogLevel.Error, ex);
		}
	}

	public static bool draw_Prefix(GiantCrop __instance, SpriteBatch spriteBatch, Vector2 tileLocation) {
		try {
			if (__instance.modData.TryGetValue(ModEntry.MD_ID, out string? id)) {
				ModEntry mod = ModEntry.Instance;
				var data = mod.GetGiantCropData(id);
				if (data is not null) {
					float shakeTimer = mod.Helper.Reflection.GetField<float>(__instance, "shakeTimer").GetValue();
					Texture2D? tex = mod.Helper.GameContent.Load<Texture2D>(data.Texture);
					Rectangle source;
					if (tex is null) {
						tex = Game1.mouseCursors;
						source = new(268, 470, 16, 16);

					} else {
						source = new(
							x: data.TexturePosition.X,
							y: data.TexturePosition.Y,
							width: 16 * data.TileSize.X,
							height: 16 * (data.TileSize.Y + 1)
						);
					}

					spriteBatch.Draw(
						texture: tex,
						position: Game1.GlobalToLocal(
							Game1.viewport, tileLocation * 64f - new Vector2(shakeTimer > 0.0 ? (float) (Math.Sin(2.0 * Math.PI / shakeTimer) * 2.0) : 0.0f, 64f)
						),
						sourceRectangle: source,
						color: Color.White,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: 4f,
						effects: SpriteEffects.None,
						layerDepth: (tileLocation.Y + 2.0f) * 64.0f / 10000.0f
					);

					return false;
				}
			}

		} catch(Exception ex) {
			Monitor?.LogOnce($"An error occurred while attempting to draw a GiantCrop instance:\n{ex}", LogLevel.Warn);
		}

		return true;
	}

	/*public static int GetCropAmount(Random r, int min, int max) {
		try {
			if (CurrentCrop is not null && CurrentCrop.modData.TryGetValue(ModEntry.MD_ID, out string? id)) {
				var data = ModEntry.Instance.GetGiantCropData(id);
				if (data is not null)
					return r.Next(data.MinYields, data.MaxYields);
			}

		} catch(Exception ex) {
			Monitor?.Log($"An error occurred while getting the quantity to drop from a GiantCrop:\n{ex}", LogLevel.Error);
		}

		return r.Next(min, max);
	}

	// TODO: Totally replace the debris logic to handle more complex item spawning
	public static void Wrapped_createMultipleObjectDebris(int index, int xTile, int yTile, int number, long who, GameLocation location) {
		try {
			if (CurrentCrop is not null && CurrentCrop.parentSheetIndex.Value == index && CurrentCrop.modData.TryGetValue(ModEntry.MD_ID, out string? id)) {
				var data = ModEntry.Instance.GetGiantCropData(id);
				if (data is not null) {
					if (!string.IsNullOrEmpty(data.HarvestedItemId) && int.TryParse(data.HarvestedItemId, out int hid))
						index = hid;

					xTile += (data.TileSize.X / 2) - 1;
					yTile += (data.TileSize.Y / 2) - 1;
				}
			}

		} catch (Exception ex) {
			Monitor?.Log($"An error occurred while handling drops from a GiantCrop:\n{ex}", LogLevel.Error);
		}

		Game1.createMultipleObjectDebris(index, xTile, yTile, number, who, location);
	}

	public static void Wrapped_createRadialDebris(GameLocation location, int debrisType, int xTile, int yTile, int numberOfChunks, bool resource, int groundLevel = -1, bool item = false, int color = -1) {
		try {
			if (item && CurrentCrop is not null && CurrentCrop.parentSheetIndex.Value == debrisType && CurrentCrop.modData.TryGetValue(ModEntry.MD_ID, out string? id)) {
				var data = ModEntry.Instance.GetGiantCropData(id);
				if (data is not null) {
					if (!string.IsNullOrEmpty(data.HarvestedItemId) && int.TryParse(data.HarvestedItemId, out int hid))
						debrisType = hid;

					xTile += (data.TileSize.X / 2) - 1;
					yTile += (data.TileSize.Y / 2) - 1;
				}
			}

		} catch (Exception ex) {
			Monitor?.Log($"An error occurred while handling drops from a GiantCrop:\n{ex}", LogLevel.Error);
		}

		Game1.createRadialDebris(location, debrisType, xTile, yTile, numberOfChunks, resource, groundLevel, item, color);
	}*/

	public static bool PerformToolAction(GiantCrops data, GiantCrop crop, Tool tool, int damage, Vector2 tileLocation, GameLocation location) {
		if (tool is not Axe taxe)
			return false;

		Farmer who = taxe.getLastFarmerToUse() ?? Game1.player;
		int power = tool.UpgradeLevel / 2 + 1;

		Random rnd = Game1.random;

		location.playSoundAt("axchop", tileLocation);

		int width = crop.width.Value;
		int height = crop.height.Value;

		int centerX = (int) tileLocation.X + width / 2;
		int centerY = (int) tileLocation.Y + height / 2;

		Game1.createRadialDebris(
			location,
			12,
			centerX,
			centerY,
			Game1.random.Next(4, 9),
			resource: false
		);

		IReflectedField<float> shakeField = ModEntry.Instance.Helper.Reflection.GetField<float>(crop, "shakeTimer");
		if (shakeField.GetValue() <= 0f) {
			shakeField.SetValue(100f);
			crop.NeedsUpdate = true;
		}

		if (taxe.hasEnchantmentOfType<ShavingEnchantment>() && rnd.GetChance(power / 5.0) && data.HarvestItems.Count > 0) {
			foreach(IGiantCropHarvestItemData dropData in data.HarvestItems) {
				Item? item = null;
				if (item is not null) {
					Debris d = new Debris(
						item,
						new Vector2(centerX * 64f, centerY * 64f),
						Game1.player.getStandingPosition()
					);

					d.Chunks[0].xVelocity.Value += rnd.Next(-10, 11) / 10f;
					d.chunkFinalYLevel = (int) (tileLocation.Y * 64f + 128f);
					location.debris.Add(d);
				}
			}
		}

		crop.health.Value -= power;
		if (crop.health.Value > 0f)
			return false;

		who.gainExperience(5, 50 * ((who.LuckLevel + 1) / 2));

		if (location.HasUnlockedAreaSecretNotes(who)) {
			SObject obj = location.tryToCreateUnseenSecretNote(who);
			if (obj != null)
				Game1.createItemDebris(obj, tileLocation * 64f, -1, location);
		}

		if (data.HarvestItems.Count > 0) {
			foreach (IGiantCropHarvestItemData dropData in data.HarvestItems) {
				Item? item = null;
				if (item is not null) {
					CreateMultipleItemDebris(
						item,
						new Vector2(centerX * 64f, centerY * 64f),
						-2,
						location
					);

					Game1.setRichPresence("giantcrop", item.Name);
				}
			}
		}

		Game1.createRadialDebris(
			location,
			12,
			centerX,
			centerY,
			Game1.random.Next(4, 9),
			resource: false
		);

		location.playSoundAt("stumpCrack", tileLocation);
		Multiplayer mp = ModEntry.Instance.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

		for (int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				float animationInterval = Utility.RandomFloat(80f, 110f);
				if (width >= 2 && height >= 2 && (x == 0 || x == width - 2) && (y == 0 || y == height - 2))
					mp.broadcastSprites(
						location,
						new TemporaryAnimatedSprite(
							5,
							(tileLocation + new Vector2(x * 0.5f, y * 0.5f)) * 64f,
							Color.White,
							8,
							flipped: false,
							70f
						)
					);

				mp.broadcastSprites(
					location,
					new TemporaryAnimatedSprite(
						5,
						(tileLocation + new Vector2(x, y)) * 64f,
						Color.White,
						8,
						flipped: false,
						animationInterval
					)
				);
			}
		}

		return true;
	}


	public static Item? TryGetDrop(IGiantCropHarvestItemData drop, Random rnd, GameLocation location, Farmer who, bool isShaving) {
		if (!rnd.GetChance(drop.Chance))
			return null;
		if (!string.IsNullOrEmpty(drop.Condition) && ! GameStateQuery.CheckConditions(drop.Condition, location: location, who: who, rnd: rnd, monitor: Monitor))
			return null;
		if (drop.ForShavingEnchantment.HasValue && drop.ForShavingEnchantment != isShaving)
			return null;

		// TODO: The item query resolver.
		return null;
	}


	public static void CreateMultipleItemDebris(Item item, Vector2 origin, int direction, GameLocation? location = null, int groundLevel = -1) {
		location ??= Game1.currentLocation;
		int stack = item.Stack;
		item.Stack = 1;
		Game1.createItemDebris(item, origin, direction, location, groundLevel);
		for(int i = 1; i < stack; i++) {
			Game1.createItemDebris(item.getOne(), origin, direction, location, groundLevel);
		}
	}

	public static bool performToolAction_Prefix(GiantCrop __instance, Tool t, int damage, Vector2 tileLocation, GameLocation location, ref bool __result) {
		CurrentCrop = __instance;
		CurrentPosition = tileLocation;
		CurrentLocation = location;

		try {
			// If this giant crop doesn't have our modData, perform the usual logic.
			if (!__instance.modData.TryGetValue(ModEntry.MD_ID, out string? id))
				return true;

			// Make sure we have data.
			var data = ModEntry.Instance.GetGiantCropData(id);
			if (data is null) {
				__result = false;
				return false;
			}

			__result = PerformToolAction(data, __instance, t, damage, tileLocation, location);
		} catch(Exception ex) {
			Monitor?.Log($"An error occurred while attempting to interact with a GiantCrop:\n{ex}", LogLevel.Warn);
			__result = false;
		}

		return false;
	}

	/*public static IEnumerable<CodeInstruction> performToolAction_Transpiler(IEnumerable<CodeInstruction> instructions) {
		var instrs = instructions.ToArray();

		var method_Next = AccessTools.Method(typeof(Random), nameof(Random.Next), new Type[] {
			typeof(int), typeof(int)
		});

		var method_createMultipleObjectDebris = AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), new Type[] {
			typeof(int), typeof(int), typeof(int), typeof(int), typeof(long), typeof(GameLocation)
		});

		var method_createRadialDebris = AccessTools.Method(typeof(Game1), nameof(Game1.createRadialDebris), new Type[] {
			typeof(GameLocation), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(int), typeof(bool), typeof(int)
		});

		bool handled_random = false;

		for (int i = 0; i < instrs.Length; i++) {
			var in0 = instrs[i];
			var in1 = (i + 1) < instrs.Length ? instrs[i + 1] : null;
			var in2 = (i + 2) < instrs.Length ? instrs[i + 2] : null;

			// Replace the random.Next(15, 22) call with a custom method
			// for determining the amount of items that should drop.
			if (in0.AsInt() == 15 && in1?.AsInt() == 22 && in2 != null && in2.IsCallVirt(method_Next)) {
				yield return in0; i++;
				yield return in1; i++;
				yield return new CodeInstruction(in2) {
					opcode = OpCodes.Call,
					operand = AccessTools.Method(typeof(GiantCrop_Patches), nameof(GetCropAmount))
				};

				handled_random = true;
				continue;
			}

			// Replace the createMultipleObjectDebris call for spawning our drops.
			if (handled_random && in0.IsCall(method_createMultipleObjectDebris)) {
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Call,
					operand = AccessTools.Method(typeof(GiantCrop_Patches), nameof(Wrapped_createMultipleObjectDebris))
				};
				continue;
			}

			// Replace the createRadialDebris call, also for spawning our drops.
			if (handled_random && in0.IsCall(method_createRadialDebris)) {
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Call,
					operand = AccessTools.Method(typeof(GiantCrop_Patches), nameof(Wrapped_createRadialDebris))
				};
				continue;
			}

			yield return in0;
		}
	}*/

	public static void performToolAction_Postfix(GiantCrop __instance, GameLocation location, bool __result) {
		try {
			if (__result)
				ModEntry.Instance.OnGiantCropRemoved(location, __instance);
		} catch(Exception ex) {
			Monitor?.Log($"An error occurred while attempting to interact with a GiantCrop:\n{ex}", LogLevel.Warn);
		}

		CurrentCrop = null;
		CurrentLocation = null;
		CurrentPosition = Vector2.Zero;
	}

}
