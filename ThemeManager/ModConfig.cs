using System;
using System.Collections.Generic;

namespace Leclair.Stardew.ThemeManager;

public enum ClockAlignMode {
	Default,
	ByTheme,
	Manual
};

[Flags]
public enum Alignment {
	None = 0,

	// Horizontal
	Left = 1,
	HCenter = 2,
	Right = 4,

	// Vertical
	Top = 8,
	VCenter = 16,
	Bottom = 32,

	// Absolute Center
	Center = HCenter | VCenter
}

internal class ModConfig {

	public bool DebugPatches { get; set; } = false;

	//public bool AlignText { get; set; } = true;

	public ClockAlignMode ClockMode { get; set; } = ClockAlignMode.ByTheme;

	public bool PatchDropdown { get; set; } = false;

	public Alignment? ClockAlignment { get; set; }

	public string StardewTheme { get; set; } = "automatic";

	public Dictionary<string, string> SelectedThemes { get; set; } = new();

}
