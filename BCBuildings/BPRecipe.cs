using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.BetterCrafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace Leclair.Stardew.BCBuildings; 

public class BPRecipe : IRecipe {

	public readonly ModEntry Mod;
	public readonly BluePrint Blueprint;

	public BPRecipe(BluePrint blueprint, ModEntry mod) {
		Mod = mod;
		Name = $"blueprint:{blueprint.name}";
		Blueprint = blueprint;

		List<IIngredient> ingredients = new();

		if (blueprint.itemsRequired != null)
			foreach (var entry in blueprint.itemsRequired)
				ingredients.Add(mod.API.CreateBaseIngredient(entry.Key, entry.Value));

		if (blueprint.moneyRequired > 0)
			ingredients.Add(mod.API.CreateCurrencyIngredient("Money", blueprint.moneyRequired));

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
		if (who.currentLocation is not BuildableGameLocation bgl)
			return false;

		if (Blueprint.isUpgrade()) {
			return bgl.isBuildingConstructed(Blueprint.nameOfBuildingToUpgrade);
		}

		return true;
	}

	public string? GetTooltipExtra(Farmer who) {
		if (who.currentLocation is not BuildableGameLocation bgl)
			return "@C{red}You cannot build in your current location.";

		if (Blueprint.isUpgrade() && !bgl.isBuildingConstructed(Blueprint.nameOfBuildingToUpgrade)) {
			string other = new BluePrint(Blueprint.nameOfBuildingToUpgrade).displayName;
			return $"@C{{red}}There is not an existing @B{other}@b to upgrade.";
		}

		return null;
	}

	public void PerformCraft(IPerformCraftEvent evt) {

		var menu = new BuildMenu(Blueprint, Blueprint.isUpgrade() ? ActionType.Upgrade : ActionType.Build, evt, Mod);
		var old_menu = Game1.activeClickableMenu;

		Game1.activeClickableMenu = menu;

		menu.exitFunction = () => {
			Game1.activeClickableMenu = old_menu;
			evt.Cancel();
		};
	}

}
