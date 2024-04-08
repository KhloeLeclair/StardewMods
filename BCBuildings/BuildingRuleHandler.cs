using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterCrafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BCBuildings;

public class BuildingRuleHandler : IDynamicRuleHandler {

	public readonly ModEntry Mod;

	public BuildingRuleHandler(ModEntry mod) {
		Mod = mod;
	}

	public string DisplayName => I18n.Filter_Name();

	public string Description => I18n.Filter_About();

	public Texture2D Texture => Game1.mouseCursors;

	public Rectangle Source => Texture.Bounds;

	public bool AllowMultiple => false;

	public bool HasEditor => false;

	public bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state) {
		return recipe.Name?.StartsWith("bcbuildings:") ?? false;
	}

	public IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData data) {
		return null;
	}

	public object? ParseState(IDynamicRuleData data) {
		return null;
	}
}
