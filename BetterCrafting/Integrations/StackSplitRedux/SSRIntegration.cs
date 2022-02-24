using Leclair.Stardew.Common.Integrations;

using StackSplitRedux;

using Leclair.Stardew.BetterCrafting.Menus;

namespace Leclair.Stardew.BetterCrafting.Integrations.StackSplitRedux {
	public class SSRIntegration : BaseAPIIntegration<IStackSplitAPI, ModEntry> {

		public SSRIntegration(ModEntry mod)
		: base(mod, "pepoluan.StackSplitRedux", "0.14.3") {
			if (!IsLoaded)
				return;

			API.TryRegisterMenu(typeof(BetterCraftingPage));
		}

	}
}
