#nullable enable

using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Inventory;
using Leclair.Stardew.Common.Crafting;

using StardewValley;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.BetterCrafting.Menus;
using StardewValley.Menus;
using Leclair.Stardew.BetterCrafting.DynamicRules;
using StardewModdingAPI;
using System.ComponentModel.Design;

namespace Leclair.Stardew.BetterCrafting;


public class PopulateContainersEventArgs : IPopulateContainersEvent {
	public IBetterCraftingMenu Menu { get; }

	public IList<Tuple<object, GameLocation?>> Containers { get; }

	public bool DisableDiscovery { get; set; } = false;

	public PopulateContainersEventArgs(IBetterCraftingMenu menu, IList<Tuple<object, GameLocation?>> containers) {
		Menu = menu;
		Containers = containers;
	}
}


public class ModAPI : IBetterCrafting {

	private readonly ModEntry Mod;

	private readonly IManifest Other;


	public ModAPI(ModEntry mod, IManifest other) {
		Mod = mod;
		Other = other;
	}

	#region GUI

	/// <inheritdoc />
	public bool OpenCraftingMenu(
		bool cooking,
		bool silent_open = false,
		GameLocation? location = null,
		Vector2? position = null,
		Rectangle? area = null,
		bool discover_containers = true,
		IList<Tuple<object, GameLocation?>>? containers = null,
		IList<string>? listed_recipes = null,
		bool discover_buildings = false
	) {
		var menu = Game1.activeClickableMenu;
		if (menu != null) {
			if (!menu.readyToClose())
				return false;

			CommonHelper.YeetMenu(menu);
			Game1.exitActiveMenu();
		}

		Game1.activeClickableMenu = Menus.BetterCraftingPage.Open(
			Mod,
			location,
			position,
			area,
			cooking: cooking,
			standalone_menu: true,
			material_containers: containers?.Select(val => new LocatedInventory(val.Item1, val.Item2)).ToList(),
			silent_open: silent_open,
			discover_containers: discover_containers,
			discover_buildings: discover_buildings,
			listed_recipes: listed_recipes
		);

		return true;
	}

	/// <inheritdoc />
	public Type GetMenuType() {
		return typeof(Menus.BetterCraftingPage);
	}

	/// <inheritdoc />
	public IBetterCraftingMenu? GetActiveMenu() {
		var menu = Game1.activeClickableMenu;

		while (menu is not null) {
			if (menu is IBetterCraftingMenu bcm)
				return bcm;

			if (menu is GameMenu gm) {
				for (int i = 0; i < gm.pages.Count; i++) {
					if (gm.pages[i] is IBetterCraftingMenu bcm2)
						return bcm2;
				}
			}

			menu = menu.GetChildMenu();
		}

		return null;
	}

	/// <inheritdoc />
	public event Action<IPopulateContainersEvent>? MenuPopulateContainers;

	internal bool EmitMenuPopulate(BetterCraftingPage menu, ref IList<LocatedInventory>? containers) {
		bool disable_discovery = false;

		if (MenuPopulateContainers is not null) {
			List<Tuple<object, GameLocation?>> values = containers == null ? new() :
				containers.Select(x => new Tuple<object, GameLocation?>(x.Source, x.Location)).ToList();

			var evt = new PopulateContainersEventArgs(menu, values) {
				DisableDiscovery = disable_discovery
			};

			MenuPopulateContainers.Invoke(evt);

			disable_discovery = evt.DisableDiscovery;

			if (values.Count == 0)
				containers = null;
			else
				containers = values.Select(x => new LocatedInventory(x.Item1, x.Item2)).ToList();
		}

		return disable_discovery;
	}

	#endregion

	#region Recipes

	/// <inheritdoc />
	public IEnumerable<string> GetExclusiveRecipes(bool cooking) {
		return Mod.Stations.GetExclusiveRecipes(cooking);
	}

	/// <inheritdoc />
	public void AddRecipeProvider(IRecipeProvider provider) {
		Mod.Recipes.AddProvider(provider);
	}

	/// <inheritdoc />
	public void RemoveRecipeProvider(IRecipeProvider provider) {
		Mod.Recipes.RemoveProvider(provider);
	}

	/// <inheritdoc />
	public void InvalidateRecipeCache() {
		Mod.Recipes.Invalidate();
	}

	/// <inheritdoc />
	public IReadOnlyCollection<IRecipe> GetRecipes(bool cooking) {
		return Mod.Recipes.GetRecipes(cooking).AsReadOnly();
	}

	/// <inheritdoc />
	public IRecipeBuilder RecipeBuilder(CraftingRecipe recipe) {
		return new RecipeBuilder(recipe);
	}

	/// <inheritdoc />
	public IRecipeBuilder RecipeBuilder(string name) {
		return new RecipeBuilder(name);
	}

	#endregion

	#region Ingredients

	/// <inheritdoc />
	public IIngredient CreateBaseIngredient(int item, int quantity) {
		return new BaseIngredient(item, quantity);
	}

	/// <inheritdoc />
	public IIngredient CreateCurrencyIngredient(CurrencyType type, int quantity) {
		return new CurrencyIngredient(type, quantity);
	}

	/// <inheritdoc />
	public IIngredient CreateMatcherIngredient(Func<Item, bool> matcher, int quantity, Func<string> displayName, Func<Texture2D> texture, Rectangle? source = null, Item? recycleTo = null) {
		return new MatcherIngredient(matcher, quantity, displayName, texture, source, recycleTo);
	}

	/// <inheritdoc />
	public IIngredient CreateErrorIngredient() {
		return new ErrorIngredient();
	}

	#endregion

	#region Item Manipulation

	/// <inheritdoc />
	public void WithInventories(
		IEnumerable<Tuple<object, GameLocation?>> inventories,
		Farmer? who,
		Action<IList<IBCInventory>, Action> withLocks
	) {
		// Validate the incoming inventories.
		var located = inventories
			.Select(pair => new LocatedInventory(pair.Item1, pair.Item2))
			.DistinctBy(inv => inv.Source)
			.Where(inv => Mod.GetInventoryProvider(inv.Source) != null)
			.ToList();

		var locations = located
			.Select(inv => inv.Location)
			.Where(inv => inv is not null);

		var for_who = who ?? Game1.player;

		// Make sure events are happening for the location.
		Mod.SpookyAction.WatchLocations(locations, for_who);

		// Call the thing.
		InventoryHelper.WithInventories(located, Mod.GetInventoryProvider, who, (locked, onDone) => withLocks(locked, () => {
			onDone();
			Mod.SpookyAction.UnwatchLocations(locations, for_who);
		}), true);
	}

	/// <inheritdoc />
	public void ConsumeItems(IEnumerable<(Func<Item, bool>, int)> items, Farmer? who, IEnumerable<IBCInventory>? inventories, int maxQuality = int.MaxValue, bool lowQualityFirst = false) {
		InventoryHelper.ConsumeItems(items, who, inventories, maxQuality, lowQualityFirst);
	}

	/// <inheritdoc />
	public int CountItem(Func<Item, bool> predicate, Farmer? who, IEnumerable<Item?>? items, int maxQuality = int.MaxValue) {
		return InventoryHelper.CountItem(predicate, who, items, out bool _, max_quality: maxQuality);
	}

	#endregion

	#region Categories

	/// <inheritdoc />
	public void CreateDefaultCategory(bool cooking, string categoryId, Func<string> Name, IEnumerable<string>? recipeNames = null, string? iconRecipe = null, bool useRules = false, IEnumerable<IDynamicRuleData>? rules = null) {
		Mod.Recipes.CreateDefaultCategory(cooking, categoryId, Name, recipeNames, iconRecipe, useRules, rules);
	}

	/// <inheritdoc />
	public void AddRecipesToDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames) {
		Mod.Recipes.AddRecipesToDefaultCategory(cooking, categoryId, recipeNames);
	}

	/// <inheritdoc />
	public void RemoveRecipesFromDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames) {
		Mod.Recipes.RemoveRecipesFromDefaultCategory(cooking, categoryId, recipeNames);
	}

	#endregion

	#region Dynamic Rules

	/// <inheritdoc />
	public string GetAbsoluteRuleId(string id) {
		return $"{Other.UniqueID}/{id}";
	}

	/// <inheritdoc />
	public bool RegisterRuleHandler(string id, IDynamicRuleHandler handler) {
		string fullId = $"{Other.UniqueID}/{id}";
		return Mod.Recipes.RegisterRuleHandler(fullId, handler);
	}

	/// <inheritdoc />
	public bool RegisterRuleHandler(string id, ISimpleInputRuleHandler handler) {
		string fullId = $"{Other.UniqueID}/{id}";
		return Mod.Recipes.RegisterRuleHandler(fullId, handler);
	}

	/// <inheritdoc />
	public bool UnregisterRuleHandler(string id) {
		string fullId = $"{Other.UniqueID}/{id}";
		return Mod.Recipes.UnregisterRuleHandler(fullId);
	}

	#endregion

	#region Inventories

	/// <inheritdoc />
	public void RegisterInventoryProvider(Type type, IInventoryProvider provider) {
		Mod.RegisterInventoryProvider(type, provider);
	}

	/// <inheritdoc />
	public void UnregisterInventoryProvider(Type type) {
		Mod.UnregisterInventoryProvider(type);
	}

	#endregion
}
