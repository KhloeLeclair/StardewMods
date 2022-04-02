using System.Collections.Generic;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using SObject = StardewValley.Object;

namespace Leclair.Stardew.BetterCrafting.Models {
	public class BaseIngredient : IIngredient {

		private readonly string Item;
		private readonly KeyValuePair<string, int>[] IngList;

		public bool SupportsQuality => true;

		public BaseIngredient(string item, int quantity) {
			Item = item;
			Quantity = quantity;

			IngList = new KeyValuePair<string, int>[] {
				new(Item, Quantity)
			};
		}

		public string DisplayName {
			get {
				if (Item != null && Item.StartsWith('-'))
					switch (Item) {
						case "-777":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574");
						case "-6":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
						case "-5":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
						case "-4":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
						case "-3":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
						case "-2":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
						case "-1":
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568");
						default:
							return "???";
					}

				ParsedItemData data = Utility.GetItemDataForItemID(Item);
				if (data != null)
					return data.displayName;

				return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
			}
		}

		public string SpriteIndex {
			get {
				switch (Item) {
					case "-777":
						return "(O)495";
					case "-6":
						return "(O)184";
					case "-5":
						return "(O)176";
					case "-4":
						return "(O)145";
					case "-3":
						return "(O)24";
					case "-2":
						return "(O)80";
					case "-1":
						return "(O)20";
					default:
						return Item;
				}
			}
		}

		public Texture2D Texture {
			get {
				ParsedItemData data = Utility.GetItemDataForItemID(SpriteIndex);
				if (data == null)
					return Game1.objectSpriteSheet;
				return data.texture;
			}
		}

		public Rectangle SourceRectangle {
			get {
				var data = Utility.GetItemDataForItemID(SpriteIndex);
				if (data == null)
					return Rectangle.Empty;
				return data.GetSourceRect(0);
			}
		}

		public int Quantity { get; private set; }

		public void Consume(Farmer who, IList<IInventory> inventories, int max_quality, bool low_quality_first) {
			InventoryHelper.ConsumeItems(IngList, who, inventories, max_quality, low_quality_first);
		}

		public int GetAvailableQuantity(Farmer who, IList<Item> items, IList<IInventory> inventories, int max_quality) {
			int amount = 0;

			if (who != null)
				foreach (var item in who.Items) {
					int quality = item is SObject obj ? obj.Quality : 0;
					if (quality <= max_quality && InventoryHelper.DoesItemMatchID(Item, item))
						amount += item.Stack;
				}

			if (items != null)
				foreach (var item in items) {
					int quality = item is SObject obj ? obj.Quality : 0;
					if (quality <= max_quality && InventoryHelper.DoesItemMatchID(Item, item))
						amount += item.Stack;
				}

			return amount;
		}
	}
}
