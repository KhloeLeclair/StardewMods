#nullable enable

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Integrations;

using SpaceCore;
using Nanoray.Pintail;

using StardewValley;
using System.Collections.Generic;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public class SCIntegration : BaseAPIIntegration<IApi, ModEntry>, IRecipeProvider {

	private readonly ProxyManager<Nothing>? ProxyMan;

	private readonly Type? CustomCraftingRecipe;
	private readonly IDictionary? CookingRecipes;
	private readonly IDictionary? CraftingRecipes;

	public SCIntegration(ModEntry mod)
	: base(mod, "spacechase0.SpaceCore", "1.8.1") {

		if (!IsLoaded)
			return;

		try {
			CustomCraftingRecipe = Type.GetType("SpaceCore.CustomCraftingRecipe, SpaceCore");

			CookingRecipes = mod.Helper.Reflection.GetField<IDictionary>(CustomCraftingRecipe!, "CookingRecipes", true).GetValue();
			CraftingRecipes = mod.Helper.Reflection.GetField<IDictionary>(CustomCraftingRecipe!, "CraftingRecipes", true).GetValue();

			var builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"Leclair.Stardew.BetterCrafting.Proxies, Version={GetType().Assembly.GetName().Version}, Culture=neutral"), AssemblyBuilderAccess.Run);
			var module = builder.DefineDynamicModule($"Proxies");

			ProxyMan = new ProxyManager<Nothing>(module, new(
				proxyObjectInterfaceMarking: ProxyObjectInterfaceMarking.MarkerWithProperty
			));
		} catch (Exception ex) {
			Log($"Unable to set up Pintail-based proxying of SpaceCore internals.", StardewModdingAPI.LogLevel.Warn, ex);
		}

		mod.Recipes.AddProvider(this);
	}

	public int RecipePriority => 10;

	public bool CacheAdditionalRecipes => true;

	public void AddCustomSkillExperience(Farmer farmer, string skill, int amt) {
		if (!IsLoaded)
			return;

		API.AddExperienceForCustomSkill(farmer, skill, amt);
	}

	public IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking) {
		return null;
	}

	public int GetCustomSkillLevel(Farmer farmer, string skill) {
		if (!IsLoaded)
			return 0;

		return API.GetLevelForCustomSkill(farmer, skill);
	}

	public IRecipe? GetRecipe(CraftingRecipe recipe) {
		if (!IsLoaded || CookingRecipes is null || CraftingRecipes is null || ProxyMan is null)
			return null;

		IDictionary container = recipe.isCookingRecipe ? CookingRecipes : CraftingRecipes;

		if (!container.Contains(recipe.name))
			return null;

		object? value = container[recipe.name];
		if (value is null)
			return null;

		if (!ProxyMan.TryProxy<ICustomCraftingRecipe>(value, out var ccr))
			return null;

		Log($"SpaceCore Recipe: {ccr.Name} - {ccr.Description} - {ccr.Ingredients.Length}");

		/*object[]? ingredients = Self.Helper.Reflection.GetProperty<object[]>(value, "Ingredients", false)?.GetValue();
		if (ingredients is null)
			return null;

		Log($" -- Ingredients: {ingredients.Length}");

		foreach(object ing in ingredients) {
			Type type = ing.GetType();
			string cls = type.FullName ?? type.Name;

			if (!ProxyMan.TryProxy<IIngredientMatcher>(ing, out var matcher)) {
				Log($"    -- Unable to proxy {cls}");
				try {
					ProxyMan.ObtainProxy<IIngredientMatcher>(ing);
				} catch (Exception ex) {
					Log($"    -- Caught Error.", StardewModdingAPI.LogLevel.Warn, ex);
				}
				continue;
			}

			if (cls.Equals("SpaceCore.CustomCraftingRecipe+ObjectIngredientMatcher")) {
				int? index = Self.Helper.Reflection.GetField<int>(value, "objectIndex", false)?.GetValue();
				if (index.HasValue)
					Log($"    -- Basic: {matcher.DisplayName} ({index.Value} - {matcher.Quantity})");

			} else if (cls.Equals("DynamicGameAssets.DGACustomCraftingRecipe+DGAIngredientMatcher")) {
				Log($"    -- DGA Matcher: {matcher.DisplayName}");

			} else
				Log($"    -- Unknown Ingredient: {matcher.DisplayName} ({cls})");

		}*/

		return null;
	}
}
