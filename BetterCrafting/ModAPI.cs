using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Leclair.Stardew.BetterCrafting {

	public interface IBetterCrafting {

		/// <summary>
		/// Try to open the Better Crafting menu. This may fail if there is another
		/// menu open that cannot be replaced.
		///
		/// If opening the menu from an object in the world, such as a workbench,
		/// its location and tile position can be provided for automatic detection
		/// of nearby chests.
		/// </summary>
		/// <param name="cooking">If true, open the cooking menu. If false, open the crafting menu.</param>
		/// <param name="containers">An optional list of chests to draw extra crafting materials from.</param>
		/// <param name="location">The map the associated object is in, or null if there is no object</param>
		/// <param name="position">The tile position the associated object is at, or null if there is no object</param>
		/// <param name="silent_open">If true, do not make a sound upon opening the menu.</param>
		/// <param name="listed_recipes">An optional list of recipes by name. If provided, only these recipes will be listed in the crafting menu.</param>
		/// <returns>Whether or not the menu was opened successfully</returns>
		bool OpenCraftingMenu(
			bool cooking,
			IList<Chest> containers = null,
			GameLocation location = null,
			Vector2? position = null,
			bool silent_open = false,
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

		/// <summary>
		/// Unregister a recipe provider. Calling this will also invalidate
		/// the recipe cache.
		///
		/// If the recipe provider was not registered, this does nothing.
		/// </summary>
		/// <param name="provider">The recipe provider to remove</param>
		void RemoveRecipeProvider(IRecipeProvider provider);

		/// <summary>
		/// Invalidate the recipe cache. You should call this if your recipe
		/// provider ever adds new recipes after registering it.
		/// </summary>
		void InvalidateRecipeCache();

		/// <summary>
		/// Get all known recipes from all providers.
		/// </summary>
		/// <param name="cooking">If true, return cooking recipes. If false, return crafting recipes.</param>
		/// <returns>A collection of the recipes.</returns>
		IReadOnlyCollection<IRecipe> GetRecipes(bool cooking);
	}


	public class ModAPI : IBetterCrafting {

		private readonly ModEntry Mod;

		public ModAPI(ModEntry mod) {
			Mod = mod;
		}

		public bool OpenCraftingMenu(
			bool cooking,
			IList<Chest> containers = null,
			GameLocation location = null,
			Vector2? position = null,
			bool silent_open = false,
			IList<string> listed_recipes = null
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
				cooking: cooking,
				standalone_menu: true,
				material_containers: containers,
				silent_open: silent_open,
				listed_recipes: listed_recipes
			);

			return true;
		}

		public Type GetMenuType() {
			return typeof(Menus.BetterCraftingPage);
		}

		public void AddRecipeProvider(IRecipeProvider provider) {
			Mod.Recipes.AddProvider(provider);
		}

		public void RemoveRecipeProvider(IRecipeProvider provider) {
			Mod.Recipes.RemoveProvider(provider);
		}

		public void InvalidateRecipeCache() {
			Mod.Recipes.Invalidate();
		}

		public IReadOnlyCollection<IRecipe> GetRecipes(bool cooking) {
			return Mod.Recipes.GetRecipes(cooking).AsReadOnly();
		}
	}
}
