using System;

using StardewValley;

namespace Leclair.Stardew.Almanac {
	public static class LuckHelper {

		public static double GetLuckForDate(int seed, WorldDate date) {
			Random rnd = new(seed + date.TotalDays);

			return Math.Min(0.100000001490116, (double) rnd.Next(-100, 101) / 1000.0);
		}

	}
}
