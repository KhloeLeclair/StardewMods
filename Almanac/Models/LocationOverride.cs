#nullable enable

using Leclair.Stardew.Common.Enums;

namespace Leclair.Stardew.Almanac.Models;

public class LocationOverride {
	public string? Map { get; set; }
	public string Zone { get; set; } = "";
	public Season Season { get; set; } = Season.All;

	public string[]? AddFish { get; set; } = null;
	public string[]? RemoveFish { get; set; } = null;
}
