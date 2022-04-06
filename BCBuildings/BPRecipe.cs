using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

using Leclair.Stardew.BetterCrafting.Models;

namespace Leclair.Stardew.BCBuildings; 

public class BPRecipe : IRecipe {

	public readonly BluePrint Blueprint;

	public BPRecipe(BluePrint blueprint, ModEntry mod) {
		Name = $"blueprint:{blueprint.name}";
		Blueprint = blueprint;

		List<IIngredient> ingredients = new();

		if (blueprint.itemsRequired != null)
			foreach (var entry in blueprint.itemsRequired)
				ingredients.Add(new BaseIngredient(entry.Key, entry.Value));

		if (blueprint.moneyRequired > 0)
			ingredients.Add(new CurrencyIngredient(CurrencyType.Money, blueprint.moneyRequired));

		Ingredients = ingredients.ToArray();
	}

	// Identity

	public int SortValue { get; }
	public string Name { get; }
	public string DisplayName => Blueprint.displayName;
	public string Description => Blueprint.description;

	public virtual bool HasRecipe(Farmer who) {
		return true;
	}

	public virtual int GetTimesCrafted(Farmer who) {
		return 0;
	}

	public CraftingRecipe CraftingRecipe => null;

	// Display

	public Texture2D Texture => Blueprint.texture;
	public Rectangle SourceRectangle => Blueprint.sourceRectForMenuView;

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

	public int QuantityPerCraft => 1;
	public IIngredient[] Ingredients { get; }

	// Creation

	public bool Stackable => false;

	public Item CreateItem() {
		return null;
	}

	public bool CanCraft(Farmer who) {
		return who.currentLocation is BuildableGameLocation;
	}

	public void PerformCraft(IPerformCraftEvent evt) {

		var menu = new BuildMenu(Blueprint, evt);
		var old_menu = Game1.activeClickableMenu;

		Game1.activeClickableMenu = menu;

		menu.exitFunction = () => {
			Game1.activeClickableMenu = old_menu;
			evt.Cancel();
		};
	}

}
