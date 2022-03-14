using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leclair.Stardew.Almanac.Models {

	public enum Season {
		All = -1,
		Spring = 0,
		Summer = 1,
		Fall = 2,
		Winter = 3
	}

	public class LocationOverride {
		public string Map;
		public int Zone { get; set; } = -1;
		public Season Season { get; set; } = Season.All;

		public string[] AddFish { get; set; } = null;
		public string[] RemoveFish { get; set; } = null;
	}
}
