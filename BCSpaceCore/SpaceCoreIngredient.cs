using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using SpaceCore;

using StardewValley;
using StardewValley.Objects;


namespace Leclair.Stardew.BCSpaceCore {
	public class SpaceCoreIngredient : IIngredient {

		private static List<Chest> GetChests(IList<WorkingInventory> inventories) {
			return inventories
				.Where(val => val.Object is Chest)
				.Select(val => val.Object as Chest)
				.ToList();
		}

		private readonly CustomCraftingRecipe.IngredientMatcher Matcher;

		public SpaceCoreIngredient(CustomCraftingRecipe.IngredientMatcher matcher) {
			Matcher = matcher;
		}

		public string DisplayName => Matcher.DispayName;

		public Texture2D Texture => Matcher.IconTexture;

		public Rectangle SourceRectangle => Matcher.IconSubrect ?? Texture.Bounds;

		public int Quantity => Matcher.Quantity;

		public void Consume(Farmer who, IList<WorkingInventory> inventories) {
			// Unfortunately, we're always going to need chests for this
			// due to how SpaceCore is implemented.
			if (who == Game1.player)
				Matcher.Consume(GetChests(inventories));
		}

		public int GetAvailableQuantity(Farmer who, IList<Item> _, IList<WorkingInventory> inventories) {
			if (who != Game1.player)
				return 0;

			List<Item> items = new();
			items.AddRange(who.Items);

			// Rather than using the provided Item list, we need
			// a list that is only items from chests because
			// SpaceCore ingredient matchers only understand
			// how to consume items from chests.
			if (inventories != null)
				foreach (var inv in inventories) {
					if (inv.Object is Chest chest)
						foreach (var item in chest.items)
							items.Add(item);
				}

			return Matcher.GetAmountInList(items);
		}
	}
}
