using System.Collections.Generic;

namespace Leclair.Stardew.BetterGameMenu;

public class ModConfig {

	public Dictionary<string, string?> PreferredImplementation { get; set; } = [];

	public bool Enabled { get; set; } = true;

	public bool AllowHotSwap { get; set; } = false;

	public bool DeveloperMode { get; set; } = false;

}
