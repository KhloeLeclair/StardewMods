using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceCore;

using StardewValley;


namespace Leclair.Stardew.BCSpaceCore {
	class SpaceCoreRecipe : IRecipe {

		public readonly CustomCraftingRecipe Recipe;

		public SpaceCoreRecipe(string name, CustomCraftingRecipe recipe) {
			Recipe = recipe;
			Name = name;

			Ingredients = recipe.Ingredients.Select(val => new SpaceCoreIngredient(val)).ToArray();

			Item example = CreateItem();
			SortValue = example?.ParentSheetIndex ?? 0;
			QuantityPerCraft = example?.Stack ?? 1;
			Stackable = (example?.maximumStackSize() ?? 1) > 1;
		}


		// Identity

		public int SortValue { get; }

		public string Name { get; private set; }
		public string DisplayName => Recipe.Name;
		public string Description => Recipe.Description;


		// Display

		public SpriteInfo Sprite => new(Texture, SourceRectangle);

		public Texture2D Texture => Recipe.IconTexture;
		public Rectangle SourceRectangle => Recipe.IconSubrect ?? Texture.Bounds;

		public int GridHeight {
			get {
				Rectangle rect = SourceRectangle;
				if (rect.Height > rect.Width)
					return 2;
				return 1;
			}
		}
		public int GridWidth {
			get {
				Rectangle rect = SourceRectangle;
				if (rect.Width > rect.Height)
					return 2;
				return 1;
			}
		}


		// Cost

		public int QuantityPerCraft { get; }

		public IIngredient[] Ingredients { get; }

		// Creation

		public bool Stackable { get; }

		public Item CreateItem() {
			return Recipe.CreateResult();
		}

	}
}
