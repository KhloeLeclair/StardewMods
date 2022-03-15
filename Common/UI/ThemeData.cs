using System;

using StardewModdingAPI;

namespace Leclair.Stardew.Common.UI
{
    public class ThemeData {

		public string[] For { get; set; }


		public bool HasMatchingMod(IModRegistry registry) {
			if (For != null)
				foreach (string mod in For) {
					if (!string.IsNullOrEmpty(mod) && registry.IsLoaded(mod))
						return true;
				}

			return false;
		}

    }
}
