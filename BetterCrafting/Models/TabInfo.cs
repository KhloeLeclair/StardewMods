using System.Collections.Generic;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Models {
	public class TabInfo {
		public Category Category;
		public ClickableComponent Component;
		public List<IRecipe> Recipes;
		public List<IRecipe> FilteredRecipes;
		public SpriteInfo Sprite;
	}
}
