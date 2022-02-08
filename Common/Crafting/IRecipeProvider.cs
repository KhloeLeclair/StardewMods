using StardewValley;

namespace Leclair.Stardew.Common.Crafting {
	public interface IRecipeProvider {

		int RecipePriority { get; }

		IRecipe GetRecipe(CraftingRecipe recipe);

	}
}
