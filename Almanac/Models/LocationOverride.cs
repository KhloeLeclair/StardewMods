using System;

using Leclair.Stardew.Common.Enums;

namespace Leclair.Stardew.Almanac.Models {

	public class LocationOverride {
		public string Map;
		public int Zone { get; set; } = -1;
		public Season Season { get; set; } = Season.All;

		public string[] AddFish { get; set; } = null;
		public string[] RemoveFish { get; set; } = null;
	}
}
