using System;
using System.Collections.Generic;

/* Unmerged change from project 'MoveToConnected'
Before:
using System.Collections.Generic;

using Microsoft.Xna.Framework;
After:
using System.Linq;

using Leclair.Stardew.Common.Enums;
using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;
*/
using System.Linq;

using Leclair.Stardew.Common.Inventory;

using StardewValley;

/* Unmerged change from project 'MoveToConnected'
Before:
using StardewValley.Objects;
using StardewValley.Network;
After:
using StardewValley.Network;
using StardewValley.Objects;
*/
using StardewValley.Network;

using SObject = StardewValley.Object;

namespace Leclair.Stardew.Common {
	public static class InventoryHelper {

		#region Mutex Handling

		public static void WithInventories(IEnumerable<object> inventories, Func<object, IInventoryProvider> getProvider, GameLocation location, Farmer who, Action<IList<WorkingInventory>> withLocks) {
			WithInventories(inventories, getProvider, location, who, (locked, onDone) => {
				try {
					withLocks(locked);
				} catch (Exception) {
					onDone();
					throw;
				}

				onDone();
			});
		}

		public static void WithInventories(IEnumerable<object> inventories, Func<object, IInventoryProvider> getProvider, GameLocation location, Farmer who, Action<IList<WorkingInventory>, Action> withLocks) {
			List<WorkingInventory> locked = new();
			List<WorkingInventory> lockable = new();

			if (inventories != null)
				foreach (object obj in inventories) {
					IInventoryProvider provider = getProvider(obj);
					if (provider == null || !provider.IsValid(obj, location, who))
						continue;

					// If we can't get a mutex, we can't assure safety. Abort.
					NetMutex mutex = provider.GetMutex(obj, location, who);
					if (mutex == null)
						continue;

					// Check the current state of the mutex. If someone else has
					// it locked, then we can't ensure safety. Abort.
					bool mlocked = mutex.IsLocked();
					if (mlocked && !mutex.IsLockHeld())
						continue;

					WorkingInventory entry = new(obj, provider, mutex, location, who);
					if (mlocked)
						locked.Add(entry);
					else
						lockable.Add(entry);
				}

			if (lockable.Count == 0) {
				withLocks(locked, () => { });
				return;
			}

			List<NetMutex> mutexes = lockable.Select(entry => entry.Mutex).ToList();
			MultipleMutexRequest mmr = null;
			mmr = new MultipleMutexRequest(
				mutexes,
				() => {
					locked.AddRange(lockable);
					withLocks(locked, () => {
						mmr?.ReleaseLocks();
						mmr = null;
					});
				},
				() => {
					withLocks(locked, () => { });
				});
		}

		#endregion

		#region Recipes and Crafting

		public static bool DoesPlayerHaveItems(IEnumerable<KeyValuePair<int, int>> items, Farmer who, IList<Item> extra = null) {
			foreach (var pair in items) {
				int num = pair.Value - who.getItemCount(pair.Key, 5);
				if (num > 0 && (extra == null || num - who.getItemCountInList(extra, pair.Key, 5) > 0))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Check if an item matches the provided ID, using the same logic
		/// as the crafting system.
		/// </summary>
		/// <param name="id">the ID to check</param>
		/// <param name="item">the Item to check</param>
		/// <returns></returns>
		public static bool DoesItemMatchID(int id, Item item) {
			if (item is SObject sobj) {
				return
					!sobj.bigCraftable.Value && sobj.ParentSheetIndex == id
					|| item.Category == id
					|| CraftingRecipe.isThereSpecialIngredientRule(sobj, id);
			}

			return false;
		}

		public static void ConsumeIngredients(this CraftingRecipe recipe, Farmer who, IEnumerable<WorkingInventory> inventories) {
			ConsumeItems(recipe.recipeList, who, inventories);
		}

		/// <summary>
		/// Remove a quantity of an item from an inventory.
		/// </summary>
		/// <param name="id">The ID of the item to consume</param>
		/// <param name="amount">The quantity to remove</param>
		/// <param name="items">The inventory we're searching</param>
		/// <param name="nullified">whether or not one or more of the items in the inventory was replaced with null</param>
		/// <returns>the remaining quantity to remove</returns>
		public static int ConsumeItem(int id, int amount, IList<Item> items, out bool nullified) {
			nullified = false;

			for (int idx = items.Count - 1; idx >= 0; --idx) {
				Item item = items[idx];
				if (DoesItemMatchID(id, item)) {
					int count = Math.Min(amount, item.Stack);
					amount -= count;
					item.Stack -= count;

					if (item.Stack <= 0) {
						items[idx] = null;
						nullified = true;
					}

					if (amount <= 0)
						return amount;
				}
			}

			return amount;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="items"></param>
		/// <param name="who"></param>
		/// <param name="inventories"></param>
		public static void ConsumeItems(IEnumerable<KeyValuePair<int, int>> items, Farmer who, IEnumerable<WorkingInventory> inventories) {
			IList<WorkingInventory> working = (inventories as IList<WorkingInventory>) ?? inventories?.ToList();
			bool[] modified = working == null ? null : new bool[working.Count];
			IList<Item>[] invs = working?.Select(val => val.CanExtractItems() ? val.GetItems() : null).ToArray();

			foreach (KeyValuePair<int, int> pair in items) {
				int id = pair.Key;
				int remaining = pair.Value;

				remaining = ConsumeItem(id, remaining, who.Items, out bool m);
				if (remaining <= 0)
					continue;

				if (working != null)
					for (int iidx = 0; iidx < working.Count; iidx++) {
						IList<Item> inv = invs[iidx];
						if (inv == null || inv.Count == 0)
							continue;

						remaining -= ConsumeItem(id, remaining, inv, out bool modded);
						if (modded)
							modified[iidx] = true;

						if (remaining <= 0)
							break;
					}
			}

			if (working != null)
				for (int idx = 0; idx < modified.Length; idx++) {
					if (modified[idx])
						working[idx].CleanInventory();
				}
		}

		#endregion

	}
}
