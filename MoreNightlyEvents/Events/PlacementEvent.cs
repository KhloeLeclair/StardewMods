using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.MoreNightlyEvents.Models;
using Leclair.Stardew.MoreNightlyEvents.Patches;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace Leclair.Stardew.MoreNightlyEvents.Events;

public class PlacementEvent : BaseFarmEvent<PlacementEventData> { 

	private int timer;

	private bool playedSound;
	private bool showedMessage;
	private bool finished;

	private GameLocation? targetMap;
	private Vector2[]? targetLocations;
	private PlacementItemData? targetData;
	private Point? targetSize;

	public PlacementEvent(string key, PlacementEventData? data = null) : base(key, data) {

	}

	public static bool IsTileOpenBesidesTerrainFeatures(GameLocation location, Vector2 tile) {
		Rectangle bounds = new Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);

		foreach(var building in location.buildings) { 
			if (building.intersects(bounds))
				return false;
		}

		foreach(var clump in location.resourceClumps) {
			if (clump.getBoundingBox().Intersects(bounds))
				return false;
		}

		foreach(var animal in location.animals.Values) {
			if (animal.GetBoundingBox().Intersects(bounds))
				return false;
		}

		foreach(var feature in location.terrainFeatures.Values) {
			if (feature.getBoundingBox().Intersects(bounds))
				return false;
		}

		foreach(var feature in location.largeTerrainFeatures) {
			if (feature.getBoundingBox().Intersects(bounds))
				return false;
		}

		if (location.objects.ContainsKey(tile))
			return false;

		return location.isTilePassable(tile.ToLocation(), Game1.viewport);
	}

	public Vector2? GetTarget(GameLocation location, int width, int height, Rectangle? validArea, HashSet<Vector2> invalid_places, int attempts = 10, Random? rnd = null, bool strict = false) {
		rnd ??= Game1.random;

		var layer = location.Map.Layers[0];
		if (layer is null)
			return null;

		Rectangle area = new(0, 0, layer.LayerWidth, layer.LayerHeight);
		if (validArea.HasValue) {
			int x = Math.Max(0, validArea.Value.X);
			int y = Math.Max(0, validArea.Value.Y);

			if (x >= layer.LayerWidth || y >= layer.LayerHeight)
				return null;

			int areaWidth = Math.Clamp(validArea.Value.Width, 1, layer.LayerWidth - x);
			int areaHeight = Math.Clamp(validArea.Value.Height, 1, layer.LayerHeight - y);

			area = new(x, y, areaWidth, areaHeight);
		}


		while(attempts > 0) {
			int x = rnd.Next(area.X, area.X + area.Width);
			int y = rnd.Next(area.Y, area.Y + area.Height);

			attempts--;

			bool valid = true;

			for(int i=0; i < width; i++) {
				for(int j = 0; j < height; j++) {
					Vector2 v = new(x + i, y + j);

					if (invalid_places.Contains(v)) {
						valid = false;
						break;
					}

					// Loose placing requirements.
					if (! IsTileOpenBesidesTerrainFeatures(location, v) ||
						location.doesTileHaveProperty(x+i, y+j, "Water", "Back") != null
					) {
						valid = false;
						break;
					}

					// Stricter placing requirements.
					if (strict && (
						location.IsNoSpawnTile(v) ||
						location.doesEitherTileOrTileIndexPropertyEqual(x+i, y+j, "Spawnable", "Back", "F") ||
						! location.CanItemBePlacedHere(v) ||
						location.isBehindBush(v)
					)) {
						valid = false;
						break;
					}
				}

				if (!valid)
					break;
			}

			if (valid)
				return new Vector2(x, y);
		}

		return null;
	}

	#region FarmEvent

	public override bool setUp() {
		if (! LoadData())
			return true;

		Random rnd = Utility.CreateDaySaveRandom();

		var loc = GetLocation();
		if (loc is null || Data?.Output is null)
			return true;

		// What are we placing?
		targetData = null;
		GameStateQueryContext ctx = new(loc, null, null, null, rnd);
		foreach(var thing in Data.Output) {
			if (string.IsNullOrEmpty(thing.Condition) || GameStateQuery.CheckConditions(thing.Condition, ctx)) {
				targetData = thing;
				break;
			}
		}

		// If we didn't get a thing, we can't continue
		// so just stop now.
		if (targetData is null)
			return true;

		// How many things are we spawning?
		int min = Math.Max(1, targetData.MinStack);
		int max = Math.Max(min, targetData.MaxStack);

		int toSpawn = rnd.Next(min, max);

		// We need to figure out
		// 1. how big is the thing we're spawning
		// 2. can we place it anywhere
		bool strictPlacement = true;

		switch(targetData.Type) {
			case PlaceableType.ResourceClump:
				// We need a clump Id to process this.
				if (targetData.ClumpId < 0)
					return true;

				strictPlacement = targetData.ClumpStrictPlacement;
				targetSize = new(
					targetData.ClumpWidth,
					targetData.ClumpHeight
				);
				break;

			case PlaceableType.GiantCrop:
				// We need to check every single giant crop we have an ID for,
				// and use the biggest size.
				int x = 1;
				int y = 1;

				var crops = DataLoader.GiantCrops(Game1.content);
				if (targetData.RandomItemId != null && targetData.RandomItemId.Count > 0) {
					foreach (string id in targetData.RandomItemId)
						if (! string.IsNullOrEmpty(id) && crops.TryGetValue(id, out var cropData)) {
							x = Math.Max(x, cropData.TileSize.X);
							y = Math.Max(y, cropData.TileSize.Y);
						}

				} else if (! string.IsNullOrEmpty(targetData.ItemId) && crops.TryGetValue(targetData.ItemId, out var cropData)) {
					x = cropData.TileSize.X;
					y = cropData.TileSize.Y;

				} else {
					// The default size
					x = 3;
					y = 3;
				}

				targetSize = new(x, y);
				break;

			case PlaceableType.WildTree:
			case PlaceableType.FruitTree:
				// Trees are only 1x1, but they need a 3x3 to grow so
				// let's use the 3x3 size.
				targetSize = new(3, 3);
				break;

			default:
				// Items are just 1x1.
				targetSize = new(1, 1);
				break;
		}


		// Figure out where we're putting them.
		HashSet<Vector2> invalidPlaces = new();
		List<Vector2> locations = new();

		for(int i = 0; i < toSpawn; i++) {
			var pos = GetTarget(loc, targetSize.Value.X, targetSize.Value.Y, targetData.SpawnArea, invalidPlaces, attempts: 50, rnd: rnd, strict: strictPlacement);
			if (pos is not null) {
				locations.Add(pos.Value);
				for (int x = 0; x < targetSize.Value.X; x++)
					for (int y = 0; y < targetSize.Value.Y; y++)
						invalidPlaces.Add(new(pos.Value.X + x, pos.Value.Y + y));
			}
		}

		// If we don't have any targets, we can't spawn anything, so
		// we give up.
		targetLocations = locations.Count > 0 ? locations.ToArray() : null;
		if (targetLocations is null)
			return true;

		// But if we got this far, it's go time.
		targetMap = loc;

		Game1.freezeControls = true;
		return false;
	}

	public override void InterruptEvent() {
		finished = true;
	}

	public override bool tickUpdate(GameTime time) {
		timer += time.ElapsedGameTime.Milliseconds;
		if (timer > 1500f && !playedSound) {
			playedSound = true;
			if (! string.IsNullOrEmpty(Data?.SoundName) )
				Game1.playSound(Data.SoundName);
		}

		if (timer > (Data?.MessageDelay ?? 7000) && ! showedMessage) {
			showedMessage = true;
			if (Data?.Message == null)
				finished = true;
			else {
				Game1.pauseThenMessage(10, Translate(Data?.Message, Game1.player));
				Game1.afterDialogues = delegate {
					finished = true;
				};
			}
		}
		if (finished) {
			Game1.freezeControls = false;
			return true;
		}
		return false;
	}

	public override void draw(SpriteBatch b) {
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
	}

	private static bool IsTreeIdValid(string id, bool isFruitTree) {
		if (string.IsNullOrEmpty(id))
			return false;

		return isFruitTree
			? DataLoader.FruitTrees(Game1.content).ContainsKey(id)
			: DataLoader.WildTrees(Game1.content).ContainsKey(id);
	}

	public override void makeChangesToLocation() {
		if (!Game1.IsMasterGame || targetLocations is null || targetMap is null || targetData is null || targetSize is null)
			return;

		var mod = ModEntry.Instance;

		Random rnd = Utility.CreateDaySaveRandom();
		ItemQueryContext ctx = new(targetMap, null, rnd);

		string id;
		Item? targetItem = null;

		// Okay, it's go time.
		foreach(var pos in targetLocations) {
			switch(targetData.Type) {
				case PlaceableType.ResourceClump:
					var clump = new ResourceClump(
						targetData.ClumpId,
						targetData.ClumpWidth,
						targetData.ClumpHeight,
						pos,
						targetData.ClumpHealth,
						targetData.ClumpTexture
					);

					// Copy over any ModData from this spawning condition.
					if (targetData.ModData is not null)
						foreach (var entry in targetData.ModData)
							clump.modData.TryAdd(entry.Key, entry.Value);

					// Remove any features under the clump.
					for(int x = 0; x < targetSize.Value.X; x++) {
						for(int y = 0; y < targetSize.Value.Y; y++) {
							Vector2 target = new(pos.X + x, pos.Y + y);
							targetMap.terrainFeatures.Remove(target);
						}
					}

					// Add to the map.
					targetMap.resourceClumps.Add(clump);
					break;

				case PlaceableType.GiantCrop:
					// Pick a suitable Id.
					id = targetData.ItemId;
					if (targetData.RandomItemId is not null && targetData.RandomItemId.Count > 0)
						id = rnd.ChooseFrom(targetData.RandomItemId);

					var crops = DataLoader.GiantCrops(Game1.content);
					if (string.IsNullOrEmpty(id) || ! crops.TryGetValue(id, out var cropData)) {
						mod.Log($"Error loading giant crop '{id}' for event: unable to find giant crop data", StardewModdingAPI.LogLevel.Error);
						continue;
					}

					// Calculate the offset for this crop within our area.
					int offsetX = (targetSize.Value.X - cropData.TileSize.X) / 2;
					int offsetY = (targetSize.Value.Y - cropData.TileSize.Y) / 2;

					// Create the new crop.
					var crop = new GiantCrop(id, new Vector2(pos.X + offsetX, pos.Y + offsetY));

					// Copy over any ModData from this spawning condition.
					if (targetData.ModData is not null)
						foreach (var entry in targetData.ModData)
							crop.modData.TryAdd(entry.Key, entry.Value);

					// Add to the map.
					targetMap.resourceClumps.Add(crop);
					break;

				case PlaceableType.FruitTree:
				case PlaceableType.WildTree:
					// Pick a suitable Id.
					id = targetData.ItemId;
					if (targetData.RandomItemId is not null && targetData.RandomItemId.Count > 0)
						id = rnd.ChooseFrom(targetData.RandomItemId);

					bool fruitTree = targetData.Type == PlaceableType.FruitTree;

					if (!IsTreeIdValid(id, fruitTree)) { 
						mod.Log($"Error loading tree '{id}' for event: unable to find tree data", StardewModdingAPI.LogLevel.Error);
						continue;
					}

					int growthStage = Math.Clamp(targetData.GrowthStage, 0, 4);
					TerrainFeature tree = fruitTree
						? new FruitTree(id, growthStage)
						: new Tree(id, growthStage);

					switch (targetData.IgnoreSeasons) {
						case IgnoreSeasonsMode.Always:
							tree.modData.TryAdd(ModEntry.FRUIT_TREE_SEASON_DATA, "true");
							break;
						case IgnoreSeasonsMode.DuringSpawn:
							FruitTree_Patches.IsSpawningTree = true;
							break;
					}

					// Perform up to 100 attempts to add fruit, if the tree is in season.
					if (tree is FruitTree ft && ft.IsInSeasonHere()) {
						int attempts = 100;
						int fruit = Math.Clamp(targetData.InitialFruit, 0, 3);

						while (attempts-- > 0 && ft.fruit.Count < fruit)
							ft.TryAddFruit();
					}

					FruitTree_Patches.IsSpawningTree = false;

					// Copy over any ModData from this spawning condition.
					if (targetData.ModData is not null)
						foreach(var entry in targetData.ModData)
							tree.modData.TryAdd(entry.Key, entry.Value);

					// Add at the center of the 3x3 we checked was clear.
					targetMap.terrainFeatures.TryAdd(new Vector2(pos.X + 1, pos.Y + 1), tree);
					break;

				case PlaceableType.Item:
					Item item = ItemQueryResolver.TryResolveRandomItem(targetData, ctx, logError: (query, error) => {
						mod.Log($"Error parsing item query '{query}' for event: {error}", StardewModdingAPI.LogLevel.Error);
					});

					if (item is null)
						continue;

					if (item is not SObject sobj) {
						mod.Log($"Ignoring invalid item query '{targetData}' for placement: the resulting item isn't a placeable item.", StardewModdingAPI.LogLevel.Error);
						continue;
					}

					// TODO: Figure out a good way to just use placementAction
					// for better compatibility. We'll need harmony patches to
					// suppress error messages and sound effects, though.

					if (sobj.HasContextTag("sign_item"))
						sobj = new Sign(pos, sobj.ItemId);

					else if (sobj.HasContextTag("torch_item"))
						sobj = new Torch(sobj.ItemId, bigCraftable: true);

					else
						switch (sobj.QualifiedItemId) {
							case "(BC)62":
								sobj = new IndoorPot(pos);
								break;

							case "(BC)130":
							case "(BC)232":
								sobj = new Chest(playerChest: true, pos, sobj.ItemId) {
									Name = sobj.Name,
									shakeTimer = 50
								};
								break;

							case "(BC)BigChest":
							case "(BC)BigStoneChest":
								sobj = new Chest(playerChest: true, pos, sobj.ItemId) {
									shakeTimer = 50,
									SpecialChestType = Chest.SpecialChestTypes.BigChest
								};
								break;

							case "(BC)163":
								sobj = new Cask(pos);
								break;

							case "(BC)165":
								// Autograbber
								sobj.heldObject.Value = new Chest();
								break;

							case "(BC)208":
								sobj = new Workbench(pos);
								break;

							case "(BC)209":
								sobj = new MiniJukebox(pos);
								break;

							case "(BC)211":
								sobj = new WoodChipper(pos);
								break;

							case "(BC)214":
								sobj = new Phone(pos);
								break;

							case "(BC)216":
								Chest fridge = new Chest("216", pos, 217, 2) {
									shakeTimer = 50,
								};
								fridge.fridge.Value = true;
								sobj = fridge;
								break;

							case "(BC)248":
								sobj = new Chest(playerChest: true, pos, sobj.ItemId) {
									Name = sobj.Name,
									shakeTimer = 50,
									SpecialChestType = Chest.SpecialChestTypes.MiniShippingBin
								};
								break;

							case "(BC)256":
								sobj = new Chest(playerChest: true, pos, sobj.ItemId) {
									Name = sobj.Name,
									shakeTimer = 50,
									SpecialChestType = Chest.SpecialChestTypes.JunimoChest
								};
								break;

							case "(BC)275":
								Chest loader = new Chest(playerChest: true, pos, sobj.ItemId) {
									Name = sobj.Name,
									shakeTimer = 50,
									SpecialChestType = Chest.SpecialChestTypes.AutoLoader
								};
								loader.lidFrameCount.Value = 2;
								sobj = loader;
								break;
						}

					if (targetData.Contents != null && targetData.Contents.Count > 0) {
						List<Item> items = new();
						foreach(var entry in targetData.Contents) {
							Item i = ItemQueryResolver.TryResolveRandomItem(entry, ctx, logError: (query, error) => {
								mod.Log($"Error parsing item query '{query}' for event: {error}", StardewModdingAPI.LogLevel.Error);
							});
							if (i is not null)
								items.Add(i);
						}

						if (items.Count > 0) {
							if (sobj is StorageFurniture sorg) {
								foreach (var i in items)
									sorg.AddItem(i);
							} else if (sobj is Chest chest)
								chest.Items.AddRange(items);
						}
					}

					if (sobj is Furniture furn) {
						furn.TileLocation = pos;
						targetMap.furniture.Add(furn);

					} else if (sobj.bigCraftable.Value) {
						targetMap.Objects.TryAdd(pos, sobj);

						if (sobj is MiniJukebox jbox)
							jbox.RegisterToLocation();

					} else {
						sobj.IsSpawnedObject = true;
						targetMap.dropObject(sobj, pos * 64f, Game1.viewport, initialPlacement: true);
					}

					// Copy over any ModData from this spawning condition.
					if (targetData.ModData is not null)
						foreach (var entry in targetData.ModData)
							sobj.modData.TryAdd(entry.Key, entry.Value);

					if (targetItem is null)
						targetItem = sobj;

					break;
			}
		}

		// Finally, do our stuff.
		PerformSideEffects(targetMap, Game1.player, targetItem);

	}

	#endregion

}
