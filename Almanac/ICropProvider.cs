using System.Collections.Generic;

namespace Leclair.Stardew.Almanac {
	public interface ICropProvider {

		int Priority { get; }

		IEnumerable<CropInfo> GetCrops();

	}
}
