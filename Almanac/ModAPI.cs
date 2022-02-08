using System.Collections.Generic;

namespace Leclair.Stardew.Almanac {

	public interface IAPI {
		IAPIv1 v1 { get; }
	}

	public interface IAPIv1 {
		void AddCropProvider(ICropProvider provider);
	}

	public class ModAPI : IAPI {

		public ModAPI(ModEntry mod) {
			v1 = new APIv1(mod);
		}

		public IAPIv1 v1 { get; private set; }
	}

	public class APIv1 : IAPIv1 {
		private readonly ModEntry Mod;

		public APIv1(ModEntry mod) {
			Mod = mod;
		}

		public void AddCropProvider(ICropProvider provider) {
			Mod.Crops.AddProvider(provider);
		}

		public List<CropInfo> GetSeasonCrops(int season) {
			return Mod.Crops.GetSeasonCrops(season);
		}

		public List<CropInfo> GetSeasonCrops(string season) {
			return Mod.Crops.GetSeasonCrops(season);
		}

		public void InvalidateCrops() {
			Mod.Crops.Invalidate();
		}

	}
}
