using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed class PageReadyToCloseEvent : IPageReadyToCloseEvent {

	private readonly BetterGameMenuImpl mMenu;

	public PageReadyToCloseEvent(BetterGameMenuImpl menu, string tab, string source, IClickableMenu page, PageReadyToCloseReason reason, bool defaultValue = true) {
		mMenu = menu;
		Tab = tab;
		Source = source;
		Page = page;
		Reason = reason;
		ReadyToClose = defaultValue;
	}

	public IClickableMenu Menu => mMenu;

	public string Tab { get; }

	public string Source { get; }

	public IClickableMenu Page { get; }

	public bool ReadyToClose { get; set; }

	public PageReadyToCloseReason Reason { get; }
}
