using System.Collections.Generic;

namespace Leclair.Stardew.ThemeManager.Models;

/// <summary>
/// LoadableManifest is a basic class with manifest properties we support
/// loading from theme.json files. These values are used for building a
/// proper <see cref="ThemeManifest"/> instance for a theme.
/// </summary>
internal class LoadableManifest {
	public string? UniqueID { get; set; }
	public string? Name { get; set; }
	public Dictionary<string, string>? LocalizedNames { get; set; }
	public string? TranslationKey { get; set; }
	public string[]? SupportedMods { get; set; }
	public string[]? For { get; set; }
	public string[]? UnsupportedMods { get; set; }
	public string? FallbackTheme { get; set; }
	public string? AssetPrefix { get; set; } = "assets";
	public bool? OverrideRedirection { get; set; }
	public bool NonSelectable { get; set; }
}
