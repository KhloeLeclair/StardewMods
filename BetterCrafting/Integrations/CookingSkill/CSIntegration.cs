#nullable enable

using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;

using Leclair.Stardew.Common.Integrations;

using CookingSkill;

namespace Leclair.Stardew.BetterCrafting.Integrations.CookingSkill;

public class CSIntegration : BaseAPIIntegration<IApi, ModEntry> {

	public CSIntegration(ModEntry mod)
	: base(mod, "spacechase0.CookingSkill", "1.4.4") {

	}

	public bool ModifyCookedItem(CraftingRecipe recipe, Item craftedItem, List<Chest> additionalIngredients) {
		if (!IsLoaded)
			return false;

		return API.ModifyCookedItem(recipe, craftedItem, additionalIngredients);
	}

	public void AddCookingExperience(Farmer who, int amount) {
		if (!IsLoaded || Self.intSCore == null || !Self.intSCore.IsLoaded)
			return;

		Self.intSCore.AddCustomSkillExperience(
			who,
			"spacechase0.Cooking",
			amount
		);
	}
}
