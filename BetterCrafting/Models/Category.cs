
using Leclair.Stardew.Common.Types;

namespace Leclair.Stardew.BetterCrafting.Models {
	public class Category {

		public string Id { get; set; }

		public string Name { get; set; }
		public string I18nKey { get; set; }

		public CategoryIcon Icon { get; set; }

		public CaseInsensitiveHashSet Recipes { get; set; }
		public string[] UnwantedRecipes { get; set; }

	}
}
