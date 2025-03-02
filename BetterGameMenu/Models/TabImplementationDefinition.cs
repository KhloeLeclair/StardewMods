using System;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public record TabImplementationDefinition(
	string Source,
	int Priority,
	Func<IClickableMenu, IClickableMenu> GetPageInstance,
	Func<IBetterGameMenuApi.DrawDelegate?>? GetDecoration,
	Func<bool>? GetTabVisible,
	Func<bool>? GetMenuInvisible,
	Func<int, int>? GetWidth,
	Func<int, int>? GetHeight,
	Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? OnResize,
	Action<IClickableMenu>? OnClose
);
