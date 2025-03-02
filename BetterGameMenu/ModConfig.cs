using System.Collections.Generic;

namespace Leclair.Stardew.BetterGameMenu;

public class ModConfig {

	public Dictionary<string, string?> PreferredImplementation { get; set; } = [];

	public bool DeveloperMode { get; set; } = false;

}
