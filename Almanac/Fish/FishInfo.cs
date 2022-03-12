using System.Linq;

using Leclair.Stardew.Common;

using StardewValley;

using Leclair.Stardew.Almanac.Models;
using System.Collections.Generic;
using System;

namespace Leclair.Stardew.Almanac.Fish {

	public record struct TimeOfDay(
		int Start,
		int End
	) {

		public bool AllDay => Start <= 600 && End >= 2600;

	};

	public enum WaterType {
		Freshwater,
		Ocean
	};

	public enum FishWeather {
		None,
		Rainy,
		Sunny,
		Any
	};

	public enum FishType {
		None,
		Trap,
		Catch
	};

	public enum CaughtStatus {
		None,
		Caught,
		Uncaught
	}

	public record struct FishInfo(
		// Deduplication
		string Id,

		// Main Display
		Item Item,
		string Name,
		string Description,
		SpriteInfo Sprite,

		// Sizes
		int MinSize,
		int MaxSize,

		// Personal Stats
		Func<Farmer, int> NumberCaught,
		Func<Farmer, int> BiggestCatch,

		// Date Range
		int[] Seasons,

		// Extra
		TrapFishInfo? TrapInfo,
		CatchFishInfo? CatchInfo
	);

	public record struct TrapFishInfo(
		WaterType Location
	);

	public record struct CatchFishInfo(
		Dictionary<SubLocation, List<int>> Locations,
		TimeOfDay[] Times,
		FishWeather Weather,

		int Minlevel
	);
}
