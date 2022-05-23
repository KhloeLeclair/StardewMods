#nullable enable

using Leclair.Stardew.Common.Integrations;

using StackSplitRedux;

using StardewValley;

using Leclair.Stardew.BetterCrafting.Menus;

namespace Leclair.Stardew.BetterCrafting.Integrations.StackSplitRedux {
	public class SSRIntegration : BaseAPIIntegration<IStackSplitAPI, ModEntry> {

		public SSRIntegration(ModEntry mod)
		: base(mod, "pepoluan.StackSplitRedux", "0.14.0") {
			if (!IsLoaded)
				return;

			API.RegisterBasicMenu<BetterCraftingPage>(
				page => page.inventory,
				page => {
					return Self.Helper.Reflection.GetField<Item>(page, "hoverItem");
				},
				page => {
					return Self.Helper.Reflection.GetField<Item>(page, "HeldItem");
				},
				null
			);
		}
	}
}
