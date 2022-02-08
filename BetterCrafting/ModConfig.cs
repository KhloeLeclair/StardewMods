using Leclair.Stardew.BetterCrafting.Models;

namespace Leclair.Stardew.BetterCrafting {
	public class ModConfig {

		public bool ReplaceCooking { get; set; } = true;
		public bool ReplaceCrafting { get; set; } = true;

		public bool UseCategories { get; set; } = true;

		// Standard Crafting
		public bool UseUniformGrid { get; set; } = false;
		public bool SortBigLast { get; set; } = false;


		// Cooking
		public SeasoningMode UseSeasoning { get; set; } = SeasoningMode.Enabled;
		public bool HideUnknown { get; set; } = false;

	}
}
