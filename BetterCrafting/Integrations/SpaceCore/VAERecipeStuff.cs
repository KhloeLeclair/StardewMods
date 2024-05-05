using System.Collections.Generic;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public interface IVAECraftingRecipeData {

	List<IVAEIngredientData> Ingredients { get; }

}

public interface IVAEIngredientMatcher : IIngredientMatcher {

	IVAEIngredientData Data { get; }

}

public enum VAEIngredientType {
	Item,
	ContextTag,
}

public interface IVAEIngredientData {

	VAEIngredientType Type { get; }

	string Value { get; }

}
