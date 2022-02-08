using Leclair.Stardew.Almanac;
using Leclair.Stardew.Common.Events;

using StardewModdingAPI.Events;

namespace Leclair.Stardew.AlmanacDGA {
	public class ModEntry : ModSubscriber {

		[Subscriber]
		private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {

			var api = Helper.ModRegistry.GetApi<IAPI>("leclair.almanac");

			api.v1.AddCropProvider(new DGAProvider(this));

		}

	}
}
