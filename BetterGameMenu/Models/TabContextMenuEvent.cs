using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed class TabContextMenuEvent : ITabContextMenuEvent {

	private readonly BetterGameMenuImpl mMenu;

	public TabContextMenuEvent(BetterGameMenuImpl menu, string tab, List<ITabContextMenuEntry> entries) {
		mMenu = menu;
		Tab = tab;
		Entries = entries;
	}

	public IClickableMenu Menu => mMenu;

	public bool IsCurrentTab => mMenu.CurrentTab == Tab;

	public string Tab { get; }

	public IClickableMenu? Page => mMenu.TryGetPage(Tab, out var page) ? page : null;

	public IList<ITabContextMenuEntry> Entries { get; }

	public ITabContextMenuEntry CreateEntry(string label, Action? onSelect, IBetterGameMenuApi.DrawDelegate? icon = null) {
		return new TabContextMenuEntry(label, onSelect, icon);
	}

}


public record TabContextMenuEntry(
	string Label,
	Action? OnSelect,
	IBetterGameMenuApi.DrawDelegate? Icon = null
) : ITabContextMenuEntry;
