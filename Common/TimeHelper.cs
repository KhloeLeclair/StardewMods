#nullable enable

using StardewValley;

namespace Leclair.Stardew.Common;

public static class TimeHelper {

	public static string FormatTime(int time) {
		// Limit it to one day.
		time %= 2400;

		return Game1.getTimeOfDayString(time);
	}
}
