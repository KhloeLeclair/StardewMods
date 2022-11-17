using System.Collections.Generic;

using Leclair.Stardew.Common.Types;

using Newtonsoft.Json;

using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Models;

public class PatchGroupData {

	public string ID { get; set; } = string.Empty;

	[JsonIgnore]
	public bool CanUse { get; set; }

	public RequiredMod[]? RequiredMods { get; set; }

	public CaseInsensitiveDictionary<string>? Variables { get; set; }

	public Dictionary<string, PatchData>? Patches { get; set; }

}

public class RequiredMod {

	public string UniqueID { get; set; } = string.Empty;
	public string? MinimumVersion { get; set; }
	public string? MaximumVersion { get; set; }

}
