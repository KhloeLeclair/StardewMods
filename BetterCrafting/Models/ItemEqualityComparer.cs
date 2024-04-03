using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterCrafting.Patches;

using StardewValley;
using StardewValley.Objects;

namespace Leclair.Stardew.BetterCrafting.Models;


public class ItemEqualityComparer : IEqualityComparer<Item> {

	public static readonly ItemEqualityComparer Instance = new();

	public bool Equals(Item? first, Item? second) {
		if (first is null || second is null)
			return first == second;

		try {
			Item_Patches.OverrideStackSize = true;
			if (first.canStackWith(second))
				return true;

		} finally {
			Item_Patches.OverrideStackSize = false;
		}

		// TODO: Figure out how colored items / flavored items work
		// and if we need to do something special.

		return first.QualifiedItemId == second.QualifiedItemId;
	}

	public int GetHashCode(Item obj) {
		return obj.GetHashCode();
	}

}
