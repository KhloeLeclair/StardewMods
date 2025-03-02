using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed class TabChangedEvent : ITabChangedEvent {

	private readonly BetterGameMenuImpl mMenu;

	public TabChangedEvent(BetterGameMenuImpl menu, string tab, string oldTab) {
		mMenu = menu;
		Tab = tab;
		OldTab = oldTab;
	}

	public IClickableMenu Menu => mMenu;

	public string Tab { get; }

	public string OldTab { get; }

}
