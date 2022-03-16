using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.Common.Integrations;

using DynamicGameAssets;
using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Integrations.DynamicGameAssets;

public class DGAIntegration : BaseAPIIntegration<IDynamicGameAssetsApi, ModEntry> {

	public DGAIntegration(ModEntry mod)
	: base(mod, "spacechase0.DynamicGameAssets", "1.4.4") {
		
	}

	public IEnumerable<Item> GetAllItems() {
		if (!IsLoaded)
			yield break;

		foreach(string id in API.GetAllItems()) {
			Item? item = null;
			try {
				item = API.SpawnDGAItem(id) as Item;
			} catch {
				/* no-op */
			}

			if (item is not null)
				yield return item;
		}
	}

}
