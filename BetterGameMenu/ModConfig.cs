using System.Collections.Generic;

namespace Leclair.Stardew.BetterGameMenu;

public class ModConfig {

	public Dictionary<string, string?> PreferredImplementation { get; set; } = [];

	public bool Enabled { get; set; } = true;

	public AllowSecondRow AllowSecondRow { get; set; } = AllowSecondRow.Automatic;

	public bool AllowHotSwap { get; set; } = true;

	public bool DeveloperMode { get; set; } = false;

	public bool ShowFakeTabs { get; set; } = false;

}

public enum AllowSecondRow {
	Automatic = 0,
	Always = 1,
	Never = 2
}
