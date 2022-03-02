using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley;

using SObject = StardewValley.Object;

namespace Leclair.Stardew.Almanac {
	public static class FishHelper {

		public static IEnumerable<int> GetLocationFish(GameLocation location, int season, int area = -1) {
			return GetLocationFish(location.Name, season, area);
		}

		public static IEnumerable<int> GetLocationFish(string key, int season, int area = -1) {
			if (key == "BeachNightMarket")
				key = "Beach";

			var locations = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");

			if (!locations.ContainsKey(key))
				return null;

			List<int> result = new();

			string[] entries = locations[key].Split('/')[4 + season].Split(' ');

			for (int i = 0; (i + 1) < entries.Length; i += 2) {
				int fish = Convert.ToInt32(entries[i]);
				int zone = Convert.ToInt32(entries[i + 1]);

				if (zone != -1 && zone != area)
					continue;

				result.Add(fish);
			}

			return result;
		}

	}
}
