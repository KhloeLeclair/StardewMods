using Leclair.Stardew.Common.Crafting;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Models;

public class CheckCanCraftEvent : ICheckCanCraftEvent {
	public Farmer Player { get; }

	public IRecipe Recipe { get; }

	public bool IsCooking { get; }

	public IClickableMenu Menu { get; }

	public CheckCanCraftEvent(Farmer player, IRecipe recipe, bool isCooking, IClickableMenu menu) {
		Player = player;
		Recipe = recipe;
		IsCooking = isCooking;
		Menu = menu;
	}

	public bool HasFailed { get; private set; }
	public string? FailReason { get; private set; }

	public void Fail(string? reason = null) {
		HasFailed = true;
		FailReason = reason;
	}

}
