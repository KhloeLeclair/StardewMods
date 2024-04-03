#nullable enable

using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Leclair.Stardew.BetterCrafting.Models;

public class BaseRecipe : IRecipe {

	public readonly CraftingRecipe Recipe;

	public BaseRecipe(CraftingRecipe recipe) {
		Recipe = recipe;

		Ingredients = recipe.recipeList
			.Select(val => new BaseIngredient(val.Key, val.Value))
			.ToArray();

		Stackable = (this.CreateItemSafe()?.maximumStackSize() ?? 0) > 1;
	}

	public virtual string QualifiedItemId {
		get {
			string idx = Recipe.getIndexOfMenuView();
			return Recipe.bigCraftable ? $"(BC){idx}" : idx;
		}
	}

	public virtual bool HasRecipe(Farmer who) {
		if (Recipe.isCookingRecipe)
			return who.cookingRecipes.ContainsKey(Name);
		else
			return who.craftingRecipes.ContainsKey(Name);
	}

	public virtual bool Stackable { get; }

	public virtual string SortValue => Recipe.itemToProduce[0];

	public virtual string Name => Recipe.name;

	public virtual string DisplayName => Recipe.DisplayName;

	public virtual string Description => Recipe.description;

	public virtual int GetTimesCrafted(Farmer who) {
		if (Recipe.isCookingRecipe) {
			string idx = Recipe.getIndexOfMenuView();
			if (who.recipesCooked.ContainsKey(idx))
				return who.recipesCooked[idx];

		} else if (who.craftingRecipes.ContainsKey(Name))
			return who.craftingRecipes[Name];

		return 0;
	}

	public virtual Texture2D Texture => ItemRegistry.GetDataOrErrorItem(QualifiedItemId).GetTexture();

	public virtual Rectangle SourceRectangle => ItemRegistry.GetDataOrErrorItem(QualifiedItemId).GetSourceRect();

	public virtual int GridHeight => Recipe.bigCraftable ? 2 : 1;

	public virtual int GridWidth => 1;

	public virtual int QuantityPerCraft => Recipe.numberProducedPerCraft;

	public virtual IIngredient[] Ingredients { get; protected set; }

	public virtual bool CanCraft(Farmer who) {
		return true;
	}

	public virtual string? GetTooltipExtra(Farmer who) {
		return null;
	}

	public virtual Item? CreateItem() {
		return Recipe.createItem();
	}

	public virtual void PerformCraft(IPerformCraftEvent evt) {
		if (evt.Item is null)
			evt.Cancel();
		else
			evt.Complete();
	}

	public virtual CraftingRecipe CraftingRecipe => Recipe;

}
