using System.Collections.Generic;

using Leclair.Stardew.Common.Types;

namespace Leclair.Stardew.ThemeManager.Models;

public class PatchData {

	public bool WarnIfNotFound { get; set; } = true;

	public CaseInsensitiveDictionary<Dictionary<string, string>>? Colors { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? RawColors { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? ColorFields { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? ColorAlphas { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? FontFields { get; set; }

	public CaseInsensitiveDictionary<Dictionary<string, string>>? TextureFields { get; set; }

	public Dictionary<int, Dictionary<string, string>>? SpriteTextColors { get; set; }

	public Dictionary<string, string[]>? SpriteTextDraw { get; set; }

	public Dictionary<string, string>? DrawTextWithShadow { get; set; }

	public Dictionary<string, string[]>? RedToGreenLerp { get; set; }

}
