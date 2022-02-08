
using Leclair.Stardew.Common.Crafting;

namespace Leclair.Stardew.BetterCrafting {

	public interface IAPI {
		IAPIv1 v1 { get; }
	}

	public interface IAPIv1 {
		void AddRecipeProvider(IRecipeProvider provider);
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

		public void AddRecipeProvider(IRecipeProvider provider) {
			Mod.Recipes.AddProvider(provider);
		}
	}
}
