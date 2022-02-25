using StardewModdingAPI.Utilities;

using Leclair.Stardew.BetterCrafting.Models;

namespace Leclair.Stardew.BetterCrafting {

	public enum MaxQuality {
		Disabled,
		None,
		Silver,
		Gold,
		Iridium
	};

	public class ModConfig {

		public bool ReplaceCooking { get; set; } = true;
		public bool ReplaceCrafting { get; set; } = true;

		public bool UseCategories { get; set; } = true;

		public bool ShowSettingsButton { get; set; } = true;

		// Quality
		public MaxQuality MaxQuality { get; set; } = MaxQuality.Iridium;
		public bool LowQualityFirst { get; set; } = true;

		// Bindings
		public KeybindList SuppressBC { get; set; } = KeybindList.Parse("LeftShift");

		// Standard Crafting
		public bool UseUniformGrid { get; set; } = false;
		public bool SortBigLast { get; set; } = false;

		// Cooking
		public SeasoningMode UseSeasoning { get; set; } = SeasoningMode.Enabled;
		public bool HideUnknown { get; set; } = false;

	}
}
