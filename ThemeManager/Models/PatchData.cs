using Leclair.Stardew.Common.Types;

namespace Leclair.Stardew.ThemeManager.Models;

public class PatchData {

	public CaseInsensitiveDictionary<string>? Colors { get; set; }

	public CaseInsensitiveDictionary<string>? RawColors { get; set; }

	public CaseInsensitiveDictionary<string>? Fields { get; set; }

	public string[]? RedToGreenLerp { get; set; }

}
