#if COMMON_CRAFTING

using System;
using System.Collections.Generic;
using System.Text;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Common.Crafting;

// Remember to update IBetterCrafting whenever this changes!

/// <summary>
/// This event is dispatched by Better Crafting whenever a
/// craft has been completed, and may be used to modify
/// the finished Item, if there is one, before the item is
/// placed into the player's inventory. At this point
/// the craft has been finalized and cannot be canceled.
/// </summary>
public interface IPostCraftEvent {

	/// <summary>
	/// The recipe being crafted.
	/// </summary>
	IRecipe Recipe { get; }

	/// <summary>
	/// The player performing the craft.
	/// </summary>
	Farmer Player { get; }

	/// <summary>
	/// The item being crafted, may be null depending on the recipe.
	/// Can be changed.
	/// </summary>
	Item? Item { get; set; }

	/// <summary>
	/// The <c>BetterCraftingPage</c> menu instance that the player
	/// is crafting from.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// A list of ingredient items that were consumed during the
	/// crafting process. This may not contain all items.
	/// </summary>
	List<Item> ConsumedItems { get; }

}


#endif
