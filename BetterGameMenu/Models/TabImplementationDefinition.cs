using System;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public record TabImplementationDefinition(
	string Source,
	int Priority,
	Func<IClickableMenu, IClickableMenu> GetPageInstance,
	Func<IBetterGameMenuApi.DrawDelegate?>? GetDecoration = null,
	Func<bool>? GetTabVisible = null,
	Func<bool>? GetMenuInvisible = null,
	Func<int, int>? GetWidth = null,
	Func<int, int>? GetHeight = null,
	Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? OnResize = null,
	Action<IClickableMenu>? OnClose = null
);
