using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common.Events;

using StardewModdingAPI.Events;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Managers;

public class ItemCacheManager : BaseManager {

	private static readonly string FLOORPAPER = @"Data/AdditionalWallpaperFlooring";

	private static readonly Dictionary<string, string> TYPE_MAPS = new() {
		{ ItemRegistry.type_bigCraftable, @"Data/BigCraftables" },
		{ ItemRegistry.type_boots, @"Data/Boots" },
		{ ItemRegistry.type_floorpaper, FLOORPAPER },
		{ ItemRegistry.type_furniture, @"Data/Furniture" },
		{ ItemRegistry.type_hat, @"Data/hats" },
		{ ItemRegistry.type_mannequin, @"Data/Mannequins" },
		{ ItemRegistry.type_object, @"Data/Objects" },
		{ ItemRegistry.type_pants, @"Data/Pants" },
		{ ItemRegistry.type_shirt, @"Data/Shirts" },
		{ ItemRegistry.type_tool, @"Data/Tools" },
		{ ItemRegistry.type_trinket, @"Data/Trinkets" },
		{ ItemRegistry.type_wallpaper, FLOORPAPER },
		{ ItemRegistry.type_weapon, @"Data/Weapons" }
	};

	private static readonly Dictionary<string, string> REVERSE_TYPE_MAPS = TYPE_MAPS
		.Where(pair => pair.Value != FLOORPAPER)
		.ToDictionary(pair => pair.Value, pair => pair.Key);

	private readonly Dictionary<string, List<Item>?> ItemMaps = new();


	public ItemCacheManager(ModEntry mod) : base(mod) { }


	#region Events

	[Subscriber]
	private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e) {
		foreach(var name in e.Names) {
			// This path covers two objects.
			if (name.BaseName == FLOORPAPER) {
				Log($"Clearing floors and wallpapers cache.", StardewModdingAPI.LogLevel.Trace);
				ItemMaps.Remove(ItemRegistry.type_floorpaper);
				ItemMaps.Remove(ItemRegistry.type_wallpaper);

				// And the rest...
			} else if (REVERSE_TYPE_MAPS.TryGetValue(name.BaseName, out string? typekey)) {
				Log($"Clearing {typekey} cache.", StardewModdingAPI.LogLevel.Trace);
				ItemMaps.Remove(typekey);
			}
		}
	}

	#endregion

	private void LoadItems() {
		foreach(string type in TYPE_MAPS.Keys) {
			if (!ItemMaps.ContainsKey(type)) {
				var typedef = ItemRegistry.GetTypeDefinition(type);
				if (typedef is not null) {
					List<Item> result = new();

					foreach (string id in typedef.GetAllIds()) {
						Item? item = ItemRegistry.Create(id, allowNull: true);
						if (item is not null)
							result.Add(item);
					}

					ItemMaps[type] = result;
				} else
					ItemMaps[type] = null;
			}
		}
	}

	private IEnumerable<Item> GetAllUnknownItems() {
		foreach(var typedef in ItemRegistry.ItemTypes) {
			if (!TYPE_MAPS.ContainsKey(typedef.Identifier)) {
				Log($"Unexpected item type: {typedef.Identifier}", StardewModdingAPI.LogLevel.Trace);

				foreach (string id in typedef.GetAllIds()) {
					Item? item = ItemRegistry.Create(id, allowNull: true);
					if (item is not null)
						yield return item;
				}
			}
		}
	}

	public void Invalidate() {
		ItemMaps.Clear();
	}

	public IEnumerable<Item> GetMatchingItems(Func<Item, bool> predicate) {
		// First, make sure we've loaded everything.
		LoadItems();

		foreach(var items in ItemMaps.Values) {
			if (items is not null)
				foreach(var item in items)
					if (predicate(item))
						yield return item;
		}

		foreach(var item in GetAllUnknownItems()) {
			if (predicate(item))
				yield return item;
		}

	}

}
