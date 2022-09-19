using System;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.DynamicTypes;

internal class UncraftedTypeHandler : IDynamicTypeHandler {

	public string DisplayName => I18n.Filter_Uncrafted();
	public string Description => I18n.Filter_Uncrafted_About();

	public Texture2D Texture => Game1.mouseCursors;

	public Rectangle Source => new(32, 672, 16, 16);

	public IFlowNode[]? GetExtraInfo(object? state) => null;

	public bool AllowMultiple => false;

	public bool HasEditor => false;

	public IClickableMenu? GetEditor(IDynamicType type) => null;

	public object? ParseState(IDynamicType type) {
		return null;
	}

	public bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state) {
		return recipe.GetTimesCrafted(Game1.player) <= 0;
	}

}
