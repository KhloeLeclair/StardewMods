using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public interface IIngredientMatcher {

	string DispayName { get; }

	Texture2D IconTexture { get; }
	Rectangle? IconSubrect { get; }

	int Quantity { get; }

	int GetAmountInList(IList<Item> items);

	void Consume(IList<IInventory> additionalIngredients);

}

public interface ICustomCraftingRecipe {

	string? Name { get; }

	string? Description { get; }

	Texture2D IconTexture { get; }
	Rectangle? IconSubrect { get; }

	//IIngredientMatcher[] Ingredients { get; }

	Item CreateResult();

}
