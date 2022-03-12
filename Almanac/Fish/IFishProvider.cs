using System.Collections.Generic;

using Leclair.Stardew.Almanac.Fish;

namespace Leclair.Stardew.Almanac.Fish {
	public interface IFishProvider {

		string Name { get; }

		int Priority { get; }

		IEnumerable<FishInfo> GetFish();

	}
}
