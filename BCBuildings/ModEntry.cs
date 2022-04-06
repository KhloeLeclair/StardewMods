using System;
using System.Collections.Generic;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.BetterCrafting;

using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace Leclair.Stardew.BCBuildings;

public class ModEntry : ModSubscriber, IRecipeProvider {

	[Subscriber]
	private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {

		var api = Helper.ModRegistry.GetApi<IBetterCrafting>("leclair.bettercrafting");
		api.AddRecipeProvider(this);

	}

	#region IRecipeProvider

	public int RecipePriority => 0;

	public IRecipe GetRecipe(CraftingRecipe recipe) {
		return null;
	}

	public IEnumerable<IRecipe> GetAdditionalRecipes(bool cooking) {
		Log("Called GetAdditionalRecipes");

		if (cooking)
			return null;

		CarpenterMenu menu = new CarpenterMenu(false);
		List<BluePrint> blueprints = Helper.Reflection.GetField<List<BluePrint>>(menu, "blueprints", false)?.GetValue();

		if (blueprints == null) {
			CommonHelper.YeetMenu(menu);
			return null;
		}

		List<IRecipe> recipes = new();

		foreach (var bp in blueprints) {
			recipes.Add(new BPRecipe(bp, this));
		}

		CommonHelper.YeetMenu(menu);
		return recipes;
	}

	#endregion

}
