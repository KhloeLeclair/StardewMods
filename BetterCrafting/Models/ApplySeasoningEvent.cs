using System;
using System.Collections.Generic;

using Leclair.Stardew.BetterCrafting.Menus;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Inventory;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Models;

public class ApplySeasoningEvent : IApplySeasoningEvent {

	public static readonly IIngredient[] QI_SEASONING_RECIPE = [
		new BaseIngredient(917, 1)
	];

	private readonly ModEntry Mod;
	private readonly BetterCraftingPage _Menu;

	internal IManifest? CurrentMod;
	internal List<IManifest>? SeasoningSources;

	internal List<(string?, List<IIngredient>)>? UsedLastChecks;
	internal bool HasUsedLast;
	internal string? UsedLastMessage;
	internal List<IIngredient>? CurrentIngredients;

	internal int MaxQuality;
	internal IList<Item?>? Items;
	internal IList<IBCInventory>? Inventories;

	internal Dictionary<IIngredient, List<Item>>? _MatchingItems;
	internal List<IIngredient>? AppliedIngredients;

	#region Life Cycle

	public ApplySeasoningEvent(ModEntry mod, BetterCraftingPage menu, IRecipe recipe, Farmer who, Item item, bool isSimulation, bool allowQiSeasoning, int maxQuality, IList<Item?>? items, IList<IBCInventory>? inventories) {
		Mod = mod;
		_Menu = menu;
		Recipe = recipe;
		Player = who;
		_Item = item;
		IsSimulation = isSimulation;
		AllowQiSeasoning = allowQiSeasoning;

		MaxQuality = maxQuality;
		Items = items;
		Inventories = inventories;
	}

	#endregion

	#region Internal Methods

	internal void ApplyQiSeasoning() {
		if (AllowQiSeasoning && Item is SObject sobj && sobj.Quality == 0 && HasIngredients(QI_SEASONING_RECIPE)) {
			sobj.Quality = SObject.highQuality;
			SetUsedLastMessage();
			ApplySeasoning(QI_SEASONING_RECIPE);
			UpdateUsedLast();
		}
	}

	internal void UpdateUsedLast() {
		if (CurrentIngredients != null && HasUsedLast) {
			UsedLastChecks ??= [];
			UsedLastChecks.Add((UsedLastMessage, CurrentIngredients));
		}

		CurrentIngredients = null;
		HasUsedLast = false;
		UsedLastMessage = null;
	}

	#endregion

	#region Public Interface

	public IRecipe Recipe { get; }
	public Farmer Player { get; }

	private Item _Item;

	public Item Item {
		get {
			return _Item;
		}
		set {
			if (value is null)
				throw new ArgumentNullException(nameof(Item));
			_Item = value;
		}
	}

	public IClickableMenu Menu => _Menu;
	public SeasoningMode SeasoningMode => Mod.Config.UseSeasoning;
	public bool IsSimulation { get; }
	public bool AllowQiSeasoning { get; set; }

	public IReadOnlyDictionary<IIngredient, List<Item>> MatchingItems {
		get {
			_MatchingItems ??= [];
			return _MatchingItems;
		}
	}

	public void ApplySeasoning(IEnumerable<IIngredient>? ingredients = null) {
		if (ingredients != null) {
			AppliedIngredients ??= [];
			AppliedIngredients.AddRange(ingredients);

			CurrentIngredients ??= [];
			CurrentIngredients.AddRange(ingredients);
		}

		if (CurrentMod != null) {
			SeasoningSources ??= [];
			SeasoningSources.Add(CurrentMod);
		}
	}

	public void ApplySeasoning(IIngredient? ingredient = null) {
		if (ingredient != null) {
			AppliedIngredients ??= [];
			AppliedIngredients.Add(ingredient);

			CurrentIngredients ??= [];
			CurrentIngredients.Add(ingredient);
		}

		if (CurrentMod != null) {
			SeasoningSources ??= [];
			SeasoningSources.Add(CurrentMod);
		}
	}

	public void SetUsedLastMessage(string? message = null) {
		HasUsedLast = true;
		UsedLastMessage = message;
	}

	public bool HasIngredients(IEnumerable<IIngredient> ingredients) {
		_MatchingItems ??= [];
		return CraftingHelper.HasIngredients(ingredients, Player, Items, Inventories, MaxQuality, _MatchingItems);
	}

	public bool HasIngredients(IIngredient ingredient) {
		_MatchingItems ??= [];
		return CraftingHelper.HasIngredients([ingredient], Player, Items, Inventories, MaxQuality, _MatchingItems);
	}

	#endregion
}
