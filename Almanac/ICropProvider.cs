#nullable enable

using System.Collections.Generic;

namespace Leclair.Stardew.Almanac;

public interface ICropProvider {

	string Name { get; }

	int Priority { get; }

	IEnumerable<CropInfo>? GetCrops();

}
