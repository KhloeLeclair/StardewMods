using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed class PageCreatedEvent : IPageCreatedEvent {

	private readonly BetterGameMenuImpl mMenu;

	public PageCreatedEvent(BetterGameMenuImpl menu, string tab, string source, IClickableMenu page, IClickableMenu? oldPage) {
		mMenu = menu;
		Tab = tab;
		Source = source;
		Page = page;
		OldPage = oldPage;
	}

	public IClickableMenu Menu => mMenu;

	public string Tab { get; }

	public string Source { get; }

	public IClickableMenu Page { get; }

	public IClickableMenu? OldPage { get; }

}
