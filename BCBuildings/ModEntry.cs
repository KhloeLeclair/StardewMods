#nullable enable

using System;
using System.Collections.Generic;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.BetterCrafting;

using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BCBuildings;

public class ModEntry : ModSubscriber, IRecipeProvider {

	internal IBetterCrafting? API;

	[Subscriber]
	private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {

		API = Helper.ModRegistry.GetApi<IBetterCrafting>("leclair.bettercrafting");
		if (API != null)
			API.AddRecipeProvider(this);
	}

	#region IRecipeProvider

	public int RecipePriority => 0;

	public IRecipe? GetRecipe(CraftingRecipe recipe) {
		return null;
	}

	public bool CacheAdditionalRecipes => false;

	public IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking) {
		Log("Called GetAdditionalRecipes");

		if (cooking)
			return null;

		List<IRecipe> recipes = new();

		recipes.AddRange(GetRecipesFromMenu(false));

		if (Game1.player.mailReceived.Contains("hasPickedUpMagicInk") || Game1.player.hasMagicInk)
			recipes.AddRange(GetRecipesFromMenu(true));

		recipes.Add(new ActionRecipe(ActionType.Move, this));
		recipes.Add(new ActionRecipe(ActionType.Paint, this));
		recipes.Add(new ActionRecipe(ActionType.Demolish, this));

		return recipes;
	}

	public IEnumerable<IRecipe> GetRecipesFromMenu(bool magical) {
		List<IRecipe> result = new();

		CarpenterMenu menu = new CarpenterMenu(magical);
		List<BluePrint>? blueprints = Helper.Reflection.GetField<List<BluePrint>>(menu, "blueprints", false)?.GetValue();

		if (blueprints == null) {
			CommonHelper.YeetMenu(menu);
			return result;
		}

		List<string> seen = new();

		foreach (var bp in blueprints) {
			result.Add(new BPRecipe(bp, this));
			seen.Add(bp.name);
		}

		if (seen.Contains("Coop") && !seen.Contains("Big Coop"))
			result.Add(new BPRecipe(new BluePrint("Big Coop"), this));

		if (seen.Contains("Coop") && !seen.Contains("Deluxe Coop"))
			result.Add(new BPRecipe(new BluePrint("Deluxe Coop"), this));

		if (seen.Contains("Barn") && !seen.Contains("Big Barn"))
			result.Add(new BPRecipe(new BluePrint("Big Barn"), this));

		if (seen.Contains("Barn") && !seen.Contains("Deluxe Barn"))
			result.Add(new BPRecipe(new BluePrint("Deluxe Barn"), this));

		if (seen.Contains("Shed") && !seen.Contains("Big Shed"))
			result.Add(new BPRecipe(new BluePrint("Big Shed"), this));

		CommonHelper.YeetMenu(menu);

		return result;
	}


	#endregion

}
