using System;

namespace Leclair.Stardew.BetterGameMenu.Models;

public record TabDefinition(
	int Order,
	Func<string> GetDisplayName,
	Func<(IBetterGameMenuApi.DrawDelegate DrawMethod, bool DrawBackground)> GetIcon
);
