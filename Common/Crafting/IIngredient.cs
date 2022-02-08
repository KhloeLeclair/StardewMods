using System.Collections.Generic;

using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.Common.Crafting {
	public interface IIngredient {

		// Display
		string DisplayName { get; }
		Texture2D Texture { get; }
		Rectangle SourceRectangle { get; }

		// Quantity
		int Quantity { get; }

		int GetAvailableQuantity(Farmer who, IList<Item> items, IList<WorkingInventory> inventories);

		void Consume(Farmer who, IList<WorkingInventory> inventories);

	}
}
