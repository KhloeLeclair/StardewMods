using System;
using System.Collections.Generic;

using HarmonyLib;

using ItemBags;

using Leclair.Stardew.Common.Integrations;
using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Inventories;
using StardewValley.Network;

namespace Leclair.Stardew.BetterCrafting.Integrations.ItemBags;

public class IBIntegration : BaseAPIIntegration<IItemBagsAPI, ModEntry>, IInventoryProvider {

	public static IBIntegration? Instance { get; private set; }

	private readonly ModAPI? SelfAPI;

	private readonly Type? ItemBagType;
	private readonly Type? ItemBagInventory;
	private readonly PerScreen<Dictionary<object, IInventory?>> BagInventories = new(() => []);
	private readonly PerScreen<Dictionary<object, NetMutex?>> BagMutexes = new(() => []);

	public IBIntegration(ModEntry mod)
	: base(mod, "SlayerDharok.Item_Bags", "3.0.6") {

		Instance = this;

		if (IsLoaded)
			try {
				ItemBagType = Type.GetType("ItemBags.Bags.ItemBag, ItemBags");
				ItemBagInventory = Type.GetType("ItemBags.ItemBagCraftingInventory, ItemBags");

				if (ItemBagType == null || ItemBagInventory == null)
					throw new ArgumentNullException();

			} catch (Exception ex) {
				Log($"Unable to find ItemBag types. Cannot integrate.", LogLevel.Warn, ex);
				IsLoaded = false;
			}

		if (IsLoaded)
			try {
				mod.Harmony!.Patch(
					original: AccessTools.PropertyGetter(ItemBagType, "IsBagInUse"),
					postfix: new HarmonyMethod(typeof(IBIntegration), nameof(IsBagInUse__Postfix))
				);
			} catch (Exception ex) {
				Log($"Unable to patch ItemBag.IsBagInUse. Cannot integrate.", LogLevel.Warn, ex);
				IsLoaded = false;
			}

		if (!IsLoaded) {
			ItemBagType = null;
			ItemBagInventory = null;
			SelfAPI = null;
			return;
		}

		SelfAPI = (ModAPI) Self.GetApi(Other!)!;
		SelfAPI.MenuPopulateContainers += SelfAPI_MenuPopulateContainers;
		SelfAPI.MenuClosing += SelfAPI_MenuClosing;
	}

	private void SelfAPI_MenuClosing(StardewValley.Menus.IClickableMenu rawMenu) {
		foreach (var item in BagInventories.Value) {

		}
	}

	#region Patches

	private static void IsBagInUse__Postfix(object __instance, ref bool __result) {
		if (Instance != null) {
			foreach (var pair in Instance.BagInventories.GetActiveValues()) {
				if (pair.Value.ContainsKey(__instance)) {
					__result = true;
					return;
				}
			}
		}
	}

	#endregion

	#region Populate Menu Containers

	[EventPriority(EventPriority.Low)]
	private void SelfAPI_MenuPopulateContainers(IPopulateContainersEvent evt) {
		if (!IsLoaded)
			return;

		List<object> extras = [];

		foreach (var item in Game1.player.Items) {
			if (item != null && IsValid(item, null, Game1.player)) {
				extras.Add(item);
				BagMutexes.Value[item] = null;
			}
		}

		foreach (var container in evt.Containers) {
			var provider = Self.GetInventoryProvider(container.Item1);
			if (provider != null && provider.IsValid(container.Item1, container.Item2, Game1.player)) {
				var mutex = provider.GetMutex(container.Item1, container.Item2, Game1.player);
				if (mutex != null || !provider.IsMutexRequired(container.Item1, container.Item2, Game1.player)) {
					var items = provider.GetItems(container.Item1, container.Item2, Game1.player);
					if (items != null)
						foreach (var item in items) {
							if (item != null && IsValid(item, null, Game1.player)) {
								extras.Add(item);
								BagMutexes.Value[item] = mutex;
							}
						}
				}
			}
		}

		foreach (object bag in extras)
			evt.Containers.Add(new(bag, null));
	}

	#endregion

	#region IInventoryProvider: ItemBag

	private IInventory? GetBagInventory(object obj) {
		if (!IsLoaded || ItemBagInventory == null || !obj.GetType().IsAssignableTo(ItemBagType))
			return null;

		if (!BagInventories.Value.TryGetValue(obj, out var inventory)) {
			try {
				inventory = Activator.CreateInstance(ItemBagInventory!, obj) as IInventory;
			} catch (Exception ex) {
				Log($"Unable to create ItemBagInventory instance: {ex}", LogLevel.Warn);
				inventory = null;
			}
			BagInventories.Value[obj] = inventory;
		}

		return inventory;
	}

	public bool CanExtractItems(object obj, GameLocation? location, Farmer? who) {
		return IsValid(obj, location, who);
	}

	public bool CanInsertItems(object obj, GameLocation? location, Farmer? who) {
		return false;
	}

	public void CleanInventory(object obj, GameLocation? location, Farmer? who) {
		GetBagInventory(obj)?.RemoveEmptySlots();
	}

	public int GetActualCapacity(object obj, GameLocation? location, Farmer? who) {
		return GetBagInventory(obj)?.CountItemStacks() ?? 0;
	}

	public IInventory? GetInventory(object obj, GameLocation? location, Farmer? who) {
		// We can't return this because several of its methods throw
		// exceptions, which would potentially cause issues when
		// we try using the inventory like a normal inventory.
		return null;
	}

	public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who) {
		return GetBagInventory(obj);
	}

	public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who) {
		return null;
	}

	public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who) {
		// TODO: Check for the container of the bag.
		return null;
	}

	public bool IsMutexRequired(object obj, GameLocation? location, Farmer? who) {
		// TODO: Check for the container of the bag.
		return false;
	}

	public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item) => true;

	public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who) {
		return null;
	}

	public bool IsValid(object obj, GameLocation? location, Farmer? who) {
		return IsLoaded && ItemBagType != null && obj.GetType().IsAssignableTo(ItemBagType);
	}

	#endregion

}
