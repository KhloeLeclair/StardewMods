using System.Collections.Generic;

using Leclair.Stardew.Common.UI;

namespace Leclair.Stardew.ThemeManager;

public enum ClockAlignMode {
	Default,
	ByTheme,
	Manual
};

internal class ModConfig {

	public bool DebugPatches { get; set; } = false;

	//public bool AlignText { get; set; } = true;

	public ClockAlignMode ClockMode { get; set; } = ClockAlignMode.ByTheme;

	public Alignment? ClockAlignment { get; set; }

	public string StardewTheme { get; set; } = "automatic";

	public Dictionary<string, string> SelectedThemes { get; set; } = new();

}
