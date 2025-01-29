using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public class SCRecipe : IRecipe {

	public readonly ICustomCraftingRecipe Recipe;
	public readonly string? ItemId;
	public readonly bool Cooking;
	private readonly CraftingRecipe cRecipe;

	public SCRecipe(string name, CraftingRecipe crecipe, ICustomCraftingRecipe recipe, bool cooking, IEnumerable<IIngredient> ingredients) {
		Name = name;
		cRecipe = crecipe;
		Recipe = recipe;
		Cooking = cooking;
		Ingredients = ingredients.ToArray();

		var item = CreateItem();
		ItemId = item?.ItemId;
		SortValue = $"{item?.ParentSheetIndex ?? 0}";
		QuantityPerCraft = item?.Stack ?? 1;
		Stackable = (item?.maximumStackSize() ?? 1) > 1;

		if (recipe.Name != null)
			DisplayName = recipe.Name;
		else if (item is not null && ItemRegistry.GetData(item.QualifiedItemId) is ParsedItemData data)
			DisplayName = data.DisplayName;
		else
			DisplayName = Name;

		// Ensure we can access things.
		string? test = recipe.Description;
		Texture2D testtwo = recipe.IconTexture;
		Rectangle? testthree = recipe.IconSubrect;
	}

	#region Identity

	public string SortValue { get; }
	public string Name { get; }

	public virtual bool HasRecipe(Farmer who) {
		if (Cooking)
			return who.cookingRecipes.ContainsKey(Name);
		else
			return who.craftingRecipes.ContainsKey(Name);
	}

	public virtual int GetTimesCrafted(Farmer who) {
		if (Cooking) {
			if (who.recipesCooked.TryGetValue(ItemId ?? Name, out int count))
				return count;
			return 0;

		} else if (who.craftingRecipes.ContainsKey(Name))
			return who.craftingRecipes[Name];

		return 0;
	}

	public CraftingRecipe? CraftingRecipe => cRecipe;

	#endregion

	#region Display

	public bool AllowRecycling { get; } = true;

	public string DisplayName { get; }
	public string Description => cRecipe.description ?? string.Empty;

	public Texture2D Texture => Recipe.IconTexture;

	public Rectangle SourceRectangle => Recipe.IconSubrect ?? Texture.Bounds;

	public int GridHeight {
		get {
			Rectangle rect = SourceRectangle;
			if (rect.Height > rect.Width)
				return 2;
			return 1;
		}
	}
	public int GridWidth {
		get {
			Rectangle rect = SourceRectangle;
			if (rect.Width > rect.Height)
				return 2;
			return 1;
		}
	}

	#endregion

	#region Cost

	public int QuantityPerCraft { get; }
	public IIngredient[] Ingredients { get; }

	#endregion

	#region Creation

	public bool Stackable { get; }

	public bool CanCraft(Farmer who) {
		return true;
	}

	public string? GetTooltipExtra(Farmer who) {
		return null;
	}

	public Item? CreateItem() {
		return Recipe.CreateResult();
	}

	public void PerformCraft(IPerformCraftEvent evt) {
		if (evt.Item is null)
			evt.Cancel();
		else
			evt.Complete();
	}

	#endregion

}
