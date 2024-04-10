using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Delegates;

namespace Leclair.Stardew.BetterCrafting.Managers;

public class DataRecipeManager : BaseManager, IRecipeProvider {

	public readonly static string RECIPE_PATH = @"Mods/leclair.bettercrafting/Recipes";

	public Dictionary<string, DataRecipe>? DataRecipesById;

	public DataRecipeManager(ModEntry mod) : base(mod) {

		Mod.Recipes.AddProvider(this);

	}

	#region Loading

	public void Invalidate() {
		DataRecipesById = null;
	}

	[Subscriber]
	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e) {
		if (e.Name.IsEquivalentTo(RECIPE_PATH))
			e.LoadFrom(() => new Dictionary<string, JsonRecipeData>(), AssetLoadPriority.Exclusive);
	}

	[Subscriber]
	private void OnAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e) {
		foreach(var name in e.Names) {
			if (name.IsEquivalentTo(RECIPE_PATH))
				DataRecipesById = null;
		}
	}

	[MemberNotNull(nameof(DataRecipesById))]
	public void LoadRecipes() {
		if (DataRecipesById != null)
			return;

		var loaded = Mod.Helper.GameContent.Load<Dictionary<string, JsonRecipeData>>(RECIPE_PATH);
		DataRecipesById = new();

		// Time to hydrate our recipes.

		foreach (var pair in loaded) {
			var recipe = pair.Value;
			recipe.Id = pair.Key;

			recipe.Ingredients ??= Array.Empty<JsonIngredientData>();

			if (recipe.Output == null || recipe.Output.Length < 1) {
				Log($"Skipping recipe '{recipe.Id}' with no output.", StardewModdingAPI.LogLevel.Warn);
				continue;
			}

			recipe.Icon ??= new CategoryIcon() {
				Type = CategoryIcon.IconType.Item
			};

			DataRecipesById[recipe.Id] = new DataRecipe(Mod, recipe);
		}
	}

	public bool TryGetRecipeById(string id, [NotNullWhen(true)] out DataRecipe? recipe) {
		LoadRecipes();
		return DataRecipesById.TryGetValue(id, out recipe);
	}

	#endregion

	#region IRecipeProvider

	public int RecipePriority => 0;

	public bool CacheAdditionalRecipes => false;

	public IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking, GameStateQueryContext? context = null) {
		LoadRecipes();

		foreach(var recipe in DataRecipesById.Values) {
			if ( ! string.IsNullOrEmpty(recipe.Data.Condition) ) {
				bool valid = context is null
					? GameStateQuery.CheckConditions(recipe.Data.Condition)
					: GameStateQuery.CheckConditions(recipe.Data.Condition, context.Value);

				if (!valid)
					continue;
			}


			if (cooking == recipe.Data.IsCooking)
				yield return recipe;
		}
	}

	public IRecipe? GetRecipe(CraftingRecipe recipe) {
		LoadRecipes();

		if (DataRecipesById.ContainsKey(recipe.name))
			return new InvalidRecipe(recipe.name);

		return null;
	}

	public IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking) {
		return GetAdditionalRecipes(cooking, null);
	}

	#endregion

}

public class InvalidRecipe : IRecipe {

	public InvalidRecipe(string id) {
		Name = id;
		Ingredients = new IIngredient[] { new ErrorIngredient() };
	}

	public string SortValue => "";

	public string Name { get; }

	public string DisplayName => "";

	public string? Description => null;

	public bool AllowRecycling => false;

	public CraftingRecipe? CraftingRecipe => null;

	public Texture2D Texture => Game1.mouseCursors;

	public Rectangle SourceRectangle => ErrorIngredient.SOURCE;

	public int GridHeight => 1;

	public int GridWidth => 1;

	public int QuantityPerCraft => 0;

	public IIngredient[]? Ingredients { get; }

	public bool Stackable => false;

	public bool CanCraft(Farmer who) {
		return false;
	}

	public Item? CreateItem() {
		return null;
	}

	public int GetTimesCrafted(Farmer who) {
		return 0;
	}

	public string? GetTooltipExtra(Farmer who) {
		return null;
	}

	public bool HasRecipe(Farmer who) {
		return false;
	}
}
