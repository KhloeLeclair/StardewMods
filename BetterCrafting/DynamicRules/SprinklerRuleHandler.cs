using System;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.DynamicRules;

public class SprinklerRuleHandler : IDynamicRuleHandler {
	public string DisplayName => I18n.Filter_Sprinkler();

	public string Description => I18n.Filter_Sprinkler_About();

	public Texture2D Texture => Game1.objectSpriteSheet;

	public Rectangle Source => Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 621, 16, 16);

	public bool AllowMultiple => false;

	public bool HasEditor => false;

	public bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state) {
		return item.Value is SObject sobj && sobj.IsSprinkler();
	}

	public IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData type) {
		return null;
	}

	public object? ParseState(IDynamicRuleData type) {
		return null;
	}
}
