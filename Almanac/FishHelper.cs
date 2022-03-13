using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley;

using SObject = StardewValley.Object;

using Leclair.Stardew.Almanac.Models;

namespace Leclair.Stardew.Almanac {
	public static class FishHelper {

		public static bool SkipLocation(string key) {
			switch(key) {
				case "fishingGame":
				case "Temp":
				case "BeachNightMarket":
				case "IslandSecret":
				case "Backwoods":
					return true;
			}

			return false;
		}

		public static Dictionary<int, Dictionary<SubLocation, List<int>>> GetFishLocations() {
			Dictionary<int, Dictionary<SubLocation, List<int>>> result = new();

			var locations = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			foreach (var lp in locations) {
				if (SkipLocation(lp.Key))
					continue;

				for (int season = 0; season < WorldDate.MonthsPerYear; season++) {
					var data = GetLocationFish(season, lp.Value);
					if (data == null)
						continue;

					foreach (var pair in data) {
						int zone = pair.Key;
						SubLocation sl = new(lp.Key, zone);

						foreach (int fish in pair.Value) {
							Dictionary<SubLocation, List<int>> locs;
							if (!result.TryGetValue(fish, out locs))
								result[fish] = locs = new();

							if (locs.TryGetValue(sl, out var seasons))
								seasons.Add(season);
							else
								locs[sl] = new List<int>() { season };
						}
					}
				}
			}

			return result;
		}

		public static Dictionary<int, List<SubLocation>> GetFishLocations(int season) {
			Dictionary<int, List<SubLocation>> result = new();

			var locations = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");
			foreach (var lp in locations) {
				if (SkipLocation(lp.Key))
					continue;

				var data = GetLocationFish(season, lp.Value);
				if (data == null)
					continue;

				foreach (var pair in data) {
					int zone = pair.Key;
					SubLocation sl = new(lp.Key, zone);

					foreach(int fish in pair.Value) {
						if (result.TryGetValue(fish, out var subs))
							subs.Add(sl);
						else
							result.Add(fish, new List<SubLocation>() { sl });
					}
				}
			}

			return result;
		}

		public static Dictionary<int, List<int>> GetLocationFish(GameLocation location, int season) {
			return GetLocationFish(location.Name, season);
		}

		public static Dictionary<int, List<int>> GetLocationFish(string key, int season) {
			if (key == "BeachNightMarket")
				key = "Beach";

			var locations = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");

			if (!locations.ContainsKey(key))
				return null;

			return GetLocationFish(season, locations[key]);
		}

		public static Dictionary<int, List<int>> GetLocationFish(int season, string data) {
			Dictionary<int, List<int>> result = new();

			string[] entries = data.Split('/')[4 + season].Split(' ', StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; (i + 1) < entries.Length; i += 2) {
				if (int.TryParse(entries[i], out int fish) && int.TryParse(entries[i + 1], out int zone))
					if (result.TryGetValue(zone, out List<int> list))
						list.Add(fish);
					else
						result.Add(zone, new() { fish });
				else
					ModEntry.instance.Log($"Invalid fish data entry for season {season} (Fish ID:{entries[i]}, Zone:{entries[i + 1]})", LogLevel.Warn);
			}

			return result;
		}
	}
}
