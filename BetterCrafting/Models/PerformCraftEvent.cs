#nullable enable

using System;

using Leclair.Stardew.Common.Crafting;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Models;

public class PerformCraftEvent : IPerformCraftEvent {

	public Farmer Player { get; }
	public Item? Item { get; set; }
	public IClickableMenu Menu { get; }

	public bool IsDone { get; private set; }
	public bool Success { get; private set; }

	public Action? OnDone { get; internal set; }

	public PerformCraftEvent(Farmer who, Item? item, IClickableMenu menu) {
		Player = who;
		Item = item;
		Menu = menu;
	}

	public void Cancel() {
		if (!IsDone) {
			IsDone = true;
			Success = false;
			OnDone?.Invoke();
		}
	}

	public void Complete() {
		if (!IsDone) {
			IsDone = true;
			Success = true;
			OnDone?.Invoke();
		}
	}
}
