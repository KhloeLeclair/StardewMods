using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.GameData;

namespace Leclair.Stardew.Almanac {
	public static class WeatherHelper {

		public static string GetSeasonName(int season) {
			return season switch {
				0 => "spring",
				1 => "summer",
				2 => "fall",
				3 => "winter",
				_ => throw new ArgumentException(Convert.ToString(season))
			};
		}

		public static string LocalizeWeather(string weatherID) {
			return weatherID switch {
				"Sun" => I18n.Weather_Sunny(),
				"Rain" => I18n.Weather_Rain(),
				"Wind" => I18n.Weather_Debris(),
				"Storm" => I18n.Weather_Lightning(),
				"Festival" => "Festival",
				// 4 = Festival, Sunny
				"Snow" => I18n.Weather_Snow(),
				// 6 = Wedding, Sunny?
				_ => I18n.Weather_Sunny()
			};
		}

		public static string GetWeatherName(int weather) {
			return weather switch {
				0 => "Sun",
				1 => "Rain",
				2 => "Wind",
				3 => "Storm",
				4 => "Festival",
				5 => "Snow",
				6 => "Wedding",
				_ => "unknown"
			};
		}

		public static bool IsRainy(string weatherID) {
			switch (weatherID) {
				case "Rain":
				case "Storm":
					return true;
			}
			return false;
		}

		public static bool IsRainOrSnow(string weatherID) {
			switch(weatherID) {
				case "Rain":
				case "Storm":
				case "Snow":
					return true;
			}
			return false;
		}

		public static Rectangle GetWeatherIcon(string weatherID) {
			int offset = weatherID switch {
				"Sun" => 0,
				"Rain" => 2,
				"Wind" => 1,
				"Storm" => 3,
				"Festival" => 0,
				"Snow" => 4,
				"Wedding" => 0,
				_ => 0
			};

			return new Rectangle(448, 256 + offset * 16, 16, 16);
		}

		public static string GetWeatherForDate(ulong seed, WorldDate date) {
			return GetRawWeatherForDate(seed, date, GameLocation.LocationContext.Default);
		}

		public static Random GetRandom(ulong seed, WorldDate date, string context) {
			int offseed = (int) (((long) seed + date.TotalDays + context.GetHashCode()) % uint.MaxValue - int.MinValue);
			Random result = new(offseed);

			int prewarm = result.Next(0, 50);
			for(int j = 0; j < prewarm; j++)
				result.NextDouble();

			prewarm = result.Next(0, 50);
			for (int j = 0; j < prewarm; j++)
				result.NextDouble();

			return result;
		}

		public static string GetRawWeatherForDate(ulong seed, WorldDate date, LocationContextData context) {
			Random rnd = GetRandom(seed, date, context.Name);

			string result;

			// TODO: Handle CopyWeatherFromLocation

			// Subtract a day from the date. This doesn't make sense until you
			// consider how the vanilla game's state is set when running it's
			// own weather GameStateQuery. Specifically, for festival days.
			WorldDate dt = new(date);
			dt.TotalDays--;

			// Replace state on Game1 so that GameStateQuery will match.
			int oldDayOfMonth = Game1.dayOfMonth;
			string oldSeason = Game1.currentSeason;
			int oldYear = Game1.year;
			uint oldPlayed = Game1.stats.DaysPlayed;

			Game1.dayOfMonth = dt.DayOfMonth;
			Game1.year = dt.Year;
			Game1.currentSeason = dt.Season;
			Game1.stats.DaysPlayed = (uint) (dt.TotalDays + 1);

			ModEntry.Instance.Log($"Getting weather for context {context.Name} for {date.Localize()}:", StardewModdingAPI.LogLevel.Trace);

			try {
				result = ExecuteConditions(context.WeatherConditions, rnd);

			} finally {
				Game1.dayOfMonth = oldDayOfMonth;
				Game1.currentSeason = oldSeason;
				Game1.year = oldYear;
				Game1.stats.DaysPlayed = oldPlayed;
			}

			if (context == GameLocation.LocationContext.Default)
				return Game1.getWeatherModificationsForDate(date, result);

			return result;
		}

		private static string ExecuteConditions(IEnumerable<LocationContextData.WeatherCondition> conditions, Random rnd = null) {
			GameStateQuery.PickRandomValue(rnd);
			foreach(var entry in conditions) {
				if (GameStateQuery.CheckConditions(entry.Condition, seeded_random: rnd)) {
					ModEntry.Instance.Log($"Matched Condition: {entry.Weather} => {entry.Condition}", StardewModdingAPI.LogLevel.Trace);
					return entry.Weather;
				}
			}

			return "Sun";
		}

		public static string GetRawWeatherForDate(ulong seed, int day) {
			return GetRawWeatherForDate(seed, day, GameLocation.LocationContext.Default);
		}

		public static string GetRawWeatherForDate(ulong seed, int day, LocationContextData context) {
			WorldDate date = new();
			date.TotalDays = day;
			return GetRawWeatherForDate(seed, date, context);
		}

	}
}
