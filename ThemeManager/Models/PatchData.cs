using System.Collections.Generic;
using Leclair.Stardew.Common.Types;

namespace Leclair.Stardew.ThemeManager.Models;

public class PatchData {

	public CaseInsensitiveDictionary<Dictionary<string, string>>? Colors { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? RawColors { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? Fields { get; set; }

	public Dictionary<string, string[]>? RedToGreenLerp { get; set; }

}
