#nullable enable

using System.Collections.Generic;

using Leclair.Stardew.BetterCrafting.DynamicRules;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Models;

public record TabInfo {

	public Category Category { get; set; } = default!;
	public ClickableComponent Component { get; set; } = default!;
	public List<IRecipe> Recipes { get; set; } = default!;
	public List<IRecipe> FilteredRecipes { get; set; } = default!;
	public SpriteInfo? Sprite { get; set; } = default!;

}
