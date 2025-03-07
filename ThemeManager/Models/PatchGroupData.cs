using System.Collections.Generic;
using System.Reflection;

using Leclair.Stardew.Common.Types;

using Newtonsoft.Json;

namespace Leclair.Stardew.ThemeManager.Models;

public class PatchGroupData {

	public string ID { get; set; } = string.Empty;

	[JsonIgnore]
	public bool CanUse { get; set; }

	public VersionedMod[]? RequiredMods { get; set; }

	public VersionedMod[]? ForbiddenMods { get; set; }

	public CaseInsensitiveDictionary<string>? ColorVariables { get; set; }

	public CaseInsensitiveDictionary<string>? ColorAlphaVariables { get; set; }

	public CaseInsensitiveDictionary<string>? FontVariables { get; set; }

	public CaseInsensitiveDictionary<string>? BmFontVariables { get; set; }

	public CaseInsensitiveDictionary<string>? TextureVariables { get; set; }

	public Dictionary<string, PatchData>? Patches { get; set; }

	[JsonIgnore]
	public Dictionary<string, MethodBase[]>? Methods { get; set; }

}

