using System.Collections.Generic;

using Leclair.Stardew.BetterCrafting.Menus;
using Leclair.Stardew.BetterGameMenu;
using Leclair.Stardew.Common.Integrations;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Integrations.BetterGameMenu;

public class BGMIntegration : BaseAPIIntegration<IBetterGameMenuApi, ModEntry> {

	internal BetterCraftingPage? pendingPage = null;

	public BGMIntegration(ModEntry mod)
	: base(mod, "leclair.bettergamemenu", "0.1.0") {
		if (!IsLoaded)
			return;

		API.RegisterImplementation(
			nameof(VanillaTabOrders.Crafting),
			100,
			getPageInstance: CreateInstance,
			onResize: OnResize
		);

		API.OnPageCreated(API_OnPageInstantiated);

	}

	private void API_OnPageInstantiated(IPageCreatedEvent e) {
		if (e.Page is OptionsPage page)
			page.options.Add(new OptionsButton("Honk", () => Game1.playSound("Duck")));
	}

	internal IClickableMenu? OnResize((IClickableMenu Menu, IClickableMenu? OldPage) input) {
		var (menu, page) = input;
		if (page is BetterCraftingPage bcp) {
			page.xPositionOnScreen = menu.xPositionOnScreen;
			page.yPositionOnScreen = menu.yPositionOnScreen;
			page.width = menu.width;
			page.height = menu.height;
			bcp.AfterGameWindowSizeChanged();
		}

		return page;
	}

	internal IBetterGameMenu? TryOpenMenu(string? defaultTab = null) {
		if (IsLoaded && API.TryOpenMenu(defaultTab, playSound: false) is IBetterGameMenu menu)
			return menu;
		return null;
	}

	internal IBetterGameMenu? AsMenu(IClickableMenu? input) {
		if (IsLoaded && input != null && API.AsMenu(input) is IBetterGameMenu menu)
			return menu;
		return null;
	}

	internal IBetterGameMenu? ActiveMenu => IsLoaded ? API.ActiveMenu : null;

	IClickableMenu CreateInstance(IClickableMenu container) {
		/*if (Game1.random.Next(2) == 0) {
			throw new System.Exception("testing stuff");
		}*/

		if (pendingPage is not null) {
			var temp = pendingPage;
			pendingPage = null;
			return temp;
		}

		return BetterCraftingPage.Open(
			mod: Self,
			location: Game1.player.currentLocation,
			position: null,
			width: container.width,
			height: container.height,
			cooking: false,
			standalone_menu: false,
			material_containers: (List<object>?) null,
			x: container.xPositionOnScreen,
			y: container.yPositionOnScreen
		);
	}

}
