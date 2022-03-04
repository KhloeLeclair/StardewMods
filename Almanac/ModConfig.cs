using StardewModdingAPI.Utilities;

namespace Leclair.Stardew.Almanac {
	public class ModConfig {

		// General
		public bool AlmanacAlwaysAvailable { get; set; } = false;

		public bool IslandAlwaysAvailable { get; set; } = false;

		public bool MagicAlwaysAvailable { get; set; } = false;

		public bool ShowAlmanacButton { get; set; } = true;

		public bool RestoreAlmanacState { get; set; } = true;

		public int CycleTime { get; set; } = 1000;

		// Bindings
		public KeybindList UseKey { get; set; } = KeybindList.Parse("F7");

		// Crop Page
		public bool ShowCrops { get; set; } = true;

		public bool ShowPreviews { get; set; } = true;
		public bool PreviewPlantOnFirst { get; set; } = false;
		public bool PreviewUseHarvestSprite { get; set; } = true;


		// Weather Page
		public bool ShowWeather { get; set; } = true;
		public bool EnableDeterministicWeather { get; set; } = true;

		public bool EnableWeatherRules { get; set; } = true;


		// Fortune Page
		public bool ShowFortunes { get; set; } = true;
		public bool EnableDeterministicLuck { get; set; } = true;
		public bool ShowExactLuck { get; set; } = false;

		// Train Page
		public bool ShowTrains { get; set; } = true;

		// Mines Page
		public bool ShowMines { get; set; } = true;

		// Notices
		public bool ShowNotices { get; set; } = true;

		public bool NoticesShowAnniversaries { get; set; } = true;
		public bool NoticesShowFestivals { get; set; } = true;
		public bool NoticesShowGathering { get; set; } = true;
	}
}
