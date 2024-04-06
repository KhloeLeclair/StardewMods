using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Events;

using StardewModdingAPI.Events;

using StardewValley;

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

		var loaded = Game1.content.Load<Dictionary<string, JsonRecipeData>>(RECIPE_PATH);
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

	#endregion

	#region IRecipeProvider

	public int RecipePriority => 0;

	public bool CacheAdditionalRecipes => false;

	public IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking) {
		LoadRecipes();

		foreach(var recipe in DataRecipesById.Values) {
			if (cooking == recipe.Data.IsCooking)
				yield return recipe;
		}
	}

	public IRecipe? GetRecipe(CraftingRecipe recipe) {
		return null;
	}

	#endregion

}
