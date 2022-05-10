#nullable enable

using System.Collections.Generic;

namespace Leclair.Stardew.Almanac.Fish;

public interface IFishProvider {

	string Name { get; }

	int Priority { get; }

	IEnumerable<FishInfo>? GetFish();

}
