
using StardewModdingAPI;

namespace Leclair.Stardew.Common.Integrations {
	public abstract class BaseAPIIntegration<T, M> : BaseIntegration<M> where M : Mod where T : class {

		protected T API { get; }

		protected BaseAPIIntegration(M self, string modID, string minVersion, string maxVersion = null) : base(self, modID, minVersion, maxVersion) {
			API = GetAPI();

			if (API == null) {
				Log("Unable to obtain API instance. Disabling integration.", LogLevel.Warn);

				IsLoaded = false;
				return;
			}
		}

		private T GetAPI() {
			return Self.Helper.ModRegistry.GetApi<T>(ModID);
		}
	}
}
