using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Models {
	public class BaseRecipe : IRecipe, IRecipeSprite {

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

		public virtual string SortValue => Recipe.itemToProduce[0];

		public virtual string Name => Recipe.name;

		public virtual string DisplayName => Recipe.DisplayName;

		public virtual string Description => Recipe.description;

		public virtual int GetTimesCrafted(Farmer who) {
			if (Recipe.isCookingRecipe) {
				if (who.recipesCooked.ContainsKey(Name))
					return who.recipesCooked[Name];

			} else if (who.craftingRecipes.ContainsKey(Name))
					return who.craftingRecipes[Name];

			return 0;
		}

		public virtual SpriteInfo Sprite => SpriteHelper.GetSprite(CreateItem());

		public virtual Texture2D Texture {
			get {
				ParsedItemData data;
				if (Recipe.bigCraftable) {
					data = Utility.GetItemTypeFromIdentifier("(BC)").GetItemDataForItemID(Recipe.itemToProduce[0]);
				} else {
					data = Utility.GetItemDataOrErrorObject(Recipe.itemToProduce[0]);
				}

				if (data == null)
					return Game1.objectSpriteSheet;
				return data.texture;
			}
		}

		public virtual Rectangle SourceRectangle {
			get {
				ParsedItemData data;
				if (Recipe.bigCraftable) {
					data = Utility.GetItemTypeFromIdentifier("(BC)").GetItemDataForItemID(Recipe.itemToProduce[0]);
				} else {
					data = Utility.GetItemDataOrErrorObject(Recipe.itemToProduce[0]);
				}

				if (data == null)
					return Rectangle.Empty;
				return data.GetSourceRect(0);
			}
		}

		public virtual int GridHeight => Recipe.bigCraftable ? 2 : 1;

		public virtual int GridWidth => 1;

		public virtual int QuantityPerCraft => Recipe.numberProducedPerCraft;

		public virtual IIngredient[] Ingredients { get; private set; }

		public virtual Item CreateItem() {
			return Recipe.createItem();
		}

		public virtual CraftingRecipe CraftingRecipe => Recipe;


	}
}
