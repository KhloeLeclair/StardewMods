using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Models {
	public class BaseRecipe : IRecipe {

		public readonly ModEntry Mod;
		public readonly CraftingRecipe Recipe;

		public BaseRecipe(ModEntry mod, CraftingRecipe recipe) {
			Mod = mod;
			Recipe = recipe;
			Ingredients = recipe.recipeList
				.Select(val => new BaseIngredient(val.Key, val.Value))
				.ToArray();

			Stackable = CreateItem().maximumStackSize() > 1;
		}

		public virtual bool Stackable { get; }

		public virtual int SortValue => Recipe.itemToProduce[0];

		public virtual string Name => Recipe.name;

		public virtual string DisplayName => Recipe.DisplayName;

		public virtual string Description => Recipe.description;

		public virtual SpriteInfo Sprite => SpriteHelper.GetSprite(CreateItem(), Mod.Helper);

		public virtual Texture2D Texture => Recipe.bigCraftable ?
			Game1.bigCraftableSpriteSheet :
			Game1.objectSpriteSheet;

		public virtual Rectangle SourceRectangle => Recipe.bigCraftable ?
			Game1.getArbitrarySourceRect(Texture, 16, 32, Recipe.getIndexOfMenuView()) :
			Game1.getSourceRectForStandardTileSheet(Texture, Recipe.getIndexOfMenuView(), 16, 16);

		public virtual int GridHeight => Recipe.bigCraftable ? 2 : 1;

		public virtual int GridWidth => 1;

		public virtual int QuantityPerCraft => Recipe.numberProducedPerCraft;

		public virtual IIngredient[] Ingredients { get; private set; }

		public virtual Item CreateItem() {
			return Recipe.createItem();
		}


	}
}
