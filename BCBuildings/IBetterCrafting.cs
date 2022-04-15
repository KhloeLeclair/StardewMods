using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

namespace Leclair.Stardew.BetterCrafting;

public enum CurrencyType {
	Money,
	FestivalPoints,
	ClubCoins,
	QiGems
};

public interface IInventory {

	object Object { get; }
	GameLocation Location { get; }
	Farmer Player { get; }
	NetMutex Mutex { get; }


	bool IsLocked();

	bool IsValid();

	bool CanInsertItems();

	bool CanExtractItems();

	Rectangle? GetMultiTileRegion();

	Vector2? GetTilePosition();

	IList<Item> GetItems();

	void CleanInventory();

	int GetActualCapacity();
}

public interface IIngredient {

	// Support Level
	bool SupportsQuality { get; }

	// Display
	string DisplayName { get; }
	Texture2D Texture { get; }
	Rectangle SourceRectangle { get; }

	// Quantity
	int Quantity { get; }

	int GetAvailableQuantity(Farmer who, IList<Item> items, IList<IInventory> inventories, int max_quality);

	void Consume(Farmer who, IList<IInventory> inventories, int max_quality, bool low_quality_first);

}

public interface IPerformCraftEvent {

	Farmer Player { get; }
	Item Item { get; set; }

	IClickableMenu Menu { get; }

	void Cancel();
	void Complete();
}

public interface IRecipe {

	// Identity

	int SortValue { get; }

	string Name { get; }
	string DisplayName { get; }
	string Description { get; }

	bool HasRecipe(Farmer who);

	int GetTimesCrafted(Farmer who);

	CraftingRecipe CraftingRecipe { get; }

	// Display

	Texture2D Texture { get; }
	Rectangle SourceRectangle { get; }

	int GridHeight { get; }
	int GridWidth { get; }

	// Cost

	int QuantityPerCraft { get; }

	IIngredient[] Ingredients { get; }

	// Creation

	bool Stackable { get; }

	bool CanCraft(Farmer who);

	string GetTooltipExtra(Farmer who);

	Item CreateItem();

	void PerformCraft(IPerformCraftEvent evt);
}

public interface IRecipeProvider {

	/// <summary>
	/// The priority of this recipe provider, sort sorting purposes.
	/// When handling CraftingRecipe instances, the first provider
	/// to return a result is used.
	/// </summary>
	int RecipePriority { get; }

	/// <summary>
	/// Get an IRecipe wrapper for a CraftingRecipe.
	/// </summary>
	/// <param name="recipe">The vanilla CraftingRecipe to wrap</param>
	/// <returns>An IRecipe wrapper, or null if this provider does
	/// not handle this recipe.</returns>
	IRecipe GetRecipe(CraftingRecipe recipe);

	/// <summary>
	/// Whether or not additional recipes from this provider should be
	/// cached. If the list should be updated every time the player
	/// opens the menu, this should return false.
	/// </summary>
	bool CacheAdditionalRecipes { get; }

	/// <summary>
	/// Get any additional recipes in IRecipe form. Additional recipes
	/// are those recipes not included in the `CraftingRecipe.cookingRecipes`
	/// and `CraftingRecipe.craftingRecipes` objects.
	/// </summary>
	/// <param name="cooking">Whether we want cooking recipes or crafting recipes.</param>
	/// <returns>An enumeration of this provider's additional recipes, or null.</returns>
	IEnumerable<IRecipe> GetAdditionalRecipes(bool cooking);

}

public interface IInventoryProvider {

	/// <summary>
	/// Check to see if this object is valid for inventory operations.
	///
	/// If location is null, it should not be considered when determining
	/// the validitiy of the object.
	/// 
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns>whether or not the object is valid</returns>
	bool IsValid(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Check to see if items can be inserted into this object.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	bool CanInsertItems(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Check to see if items can be extracted from this object.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	bool CanExtractItems(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// For objects larger than a single tile on the map, return the rectangle representing
	/// the object. For single tile objects, return null.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	Rectangle? GetMultiTileRegion(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Return the real position of the object. If the object has no position, returns null.
	/// For multi-tile objects, this should return the "main" object if there is one. 
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	Vector2? GetTilePosition(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Get the NetMutex that locks the object for multiplayer synchronization. This method must
	/// return a mutex. If null is returned, the object will be skipped.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	NetMutex GetMutex(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Get a list of items in the object's inventory, for modification or viewing. Assume that
	/// anything using this list will use GetMutex() to lock the inventory before modifying.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	IList<Item> GetItems(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Clean the inventory of the object. This is for removing null entries, organizing, etc.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	void CleanInventory(object obj, GameLocation location, Farmer who);

	/// <summary>
	/// Get the actual inventory capacity of the object's inventory. New items may be added to the
	/// GetItems() list up until this count.
	/// </summary>
	/// <param name="obj">the object</param>
	/// <param name="location">the map where the object is</param>
	/// <param name="who">the player accessing the inventory, or null if no player is involved</param>
	/// <returns></returns>
	int GetActualCapacity(object obj, GameLocation location, Farmer who);

}

public interface IBetterCrafting {



	/// <summary>
	/// Try to open the Better Crafting menu. This may fail if there is another
	/// menu open that cannot be replaced.
	///
	/// If opening the menu from an object in the world, such as a workbench,
	/// its location and tile position can be provided for automatic detection
	/// of nearby chests.
	///
	/// Better Crafting has its own handling of mutexes, so please do not worry
	/// about locking Chests before handing them off to the menu.
	///
	/// When discovering additional containers, Better Crafting scans all tiles
	/// around each of its existing known containers. If a location and position
	/// for the menu source is provided, the tiles around that position will
	/// be scanned as well.
	///
	/// Discovery depends on the user's settings, though at a minimum a 3x3 area
	/// will be scanned to mimic the scanning radius of the vanilla workbench.
	/// </summary>
	/// <param name="cooking">If true, open the cooking menu. If false, open the crafting menu.</param>
	/// <param name="silent_open">If true, do not make a sound upon opening the menu.</param>
	/// <param name="location">The map the associated object is in, or null if there is no object</param>
	/// <param name="position">The tile position the associated object is at, or null if there is no object</param>
	/// <param name="area">The tile area the associated object covers, or null if there is no object or if the object only covers a single tile</param>
	/// <param name="discover_containers">If true, attempt to discover additional material containers.</param>
	/// <param name="containers">An optional list of containers to draw extra crafting materials from.</param>
	/// <param name="listed_recipes">An optional list of recipes by name. If provided, only these recipes will be listed in the crafting menu.</param>
	/// <returns>Whether or not the menu was opened successfully</returns>

	bool OpenCraftingMenu(
		bool cooking,
		bool silent_open = false,
		GameLocation location = null,
		Vector2? position = null,
		Rectangle? area = null,
		bool discover_containers = true,
		IList<Tuple<object, GameLocation>> containers = null,
		IList<string> listed_recipes = null
	);

	/// <summary>
	/// Return the Better Crafting menu's type. In case you want to do
	/// spooky stuff to it, I guess.
	/// </summary>
	/// <returns>The BetterCraftingMenu type.</returns>
	Type GetMenuType();

	/// <summary>
	/// Register a recipe provider with Better Crafting. Calling this
	/// will also invalidate the recipe cache.
	///
	/// If the recipe provider was already registered, this does nothing.
	/// </summary>
	/// <param name="provider">The recipe provider to add</param>
	void AddRecipeProvider(IRecipeProvider provider);

	void InvalidateRecipeCache();

	IIngredient CreateBaseIngredient(int item, int quantity);

	IIngredient CreateCurrencyIngredient(string type, int quantity);
}
