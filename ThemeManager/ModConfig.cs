using System.Collections.Generic;

namespace Leclair.Stardew.ThemeManager;

internal class ModConfig {

	public string StardewTheme { get; set; } = "automatic";

	public Dictionary<string, string> SelectedThemes { get; set; } = new();

}
