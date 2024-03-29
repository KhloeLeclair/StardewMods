using System;

using StardewModdingAPI;

namespace Leclair.Stardew.ThemeManager.Models;

public class VersionedMod {

	public string UniqueID { get; set; } = string.Empty;
	public string? MinimumVersion { get; set; }
	public string? MaximumVersion { get; set; }

	public bool Matches(IModInfo? mod) {
		if (mod?.Manifest?.UniqueID != UniqueID)
			return false;

		try {
			return mod.Manifest.Version.IsBetween(
				string.IsNullOrWhiteSpace(MinimumVersion) ?
					mod.Manifest.Version : new SemanticVersion(MinimumVersion),
				string.IsNullOrWhiteSpace(MaximumVersion) ?
					mod.Manifest.Version : new SemanticVersion(MaximumVersion)
			);

		} catch (Exception ex) {
			ModEntry.Instance.Log($"An error occurred while checking the version of a mod: {ex}", LogLevel.Warn);
			return false;
		}
	}
}
