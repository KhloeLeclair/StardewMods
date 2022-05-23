#nullable enable

using StardewValley;
using StardewModdingAPI;

namespace Leclair.Stardew.Common;

public static class TimeHelper {

	public static string FormatTime(int time, Translation format) {
		return FormatTime(time, format.HasValue() ? format.ToString() : null);
	}

	public static string FormatTime(int time, string? format) {
		// Limit it to one day.
		time %= 2400;

		if (string.IsNullOrEmpty(format))
			return Game1.getTimeOfDayString(time);

		return LocalizedContentManager.FormatTimeString(time, format).ToString();
	}
}
