#nullable enable

using System;
using System.Reflection;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Common;

public static class CraftingPageHelper {

	[Obsolete("No longer required in 1.6")]
	public static bool IsCooking(this CraftingPage menu) {
		return menu.cooking;
	}

	[Obsolete("No longer required in 1.6")]
	public static bool GetHeldItem(this CraftingPage menu, out Item? item) {
		item = menu.heldItem;
		return item is not null;
	}

	[Obsolete("No longer required in 1.6")]
	public static bool SetHeldItem(this CraftingPage menu, Item? item) {
		menu.heldItem = item;
		return true;
	}
}
