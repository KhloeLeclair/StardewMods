using System.Collections.Generic;

using Leclair.Stardew.Common.Crafting;

using SpaceCore;

using StardewValley;

namespace Leclair.Stardew.BCSpaceCore {
	class SpaceCoreProvider : IRecipeProvider {

		private readonly ModEntry Mod;

		public SpaceCoreProvider(ModEntry mod) {
			Mod = mod;
		}

		public int RecipePriority => 5;

		public IRecipe GetRecipe(CraftingRecipe recipe) {
			Dictionary<string, CustomCraftingRecipe> container;
			if (recipe.isCookingRecipe)
				container = CustomCraftingRecipe.CookingRecipes;
			else
				container = CustomCraftingRecipe.CraftingRecipes;

			if (!container.TryGetValue(recipe.name, out var ccr) || ccr == null)
				return null;

			return new SpaceCoreRecipe(recipe.name, ccr, Mod);
		}

		public IEnumerable<IRecipe> GetAdditionalRecipes(bool cooking) {
			return null;
		}
	}
}
