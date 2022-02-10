using System;
using System.Collections.Generic;

using Leclair.Stardew.Almanac.Crops;
using Leclair.Stardew.Almanac.Pages;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.Common.Integrations.GenericModConfigMenu;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

namespace Leclair.Stardew.Almanac {
	public class ModEntry : ModSubscriber {

		public static readonly int Event_Base = 999999999;
		public static readonly int Event_Island = 9999991;

		public static ModEntry instance;
		public static ModAPI API;

		public ModConfig Config;

		internal AssetManager Assets;
		internal CropManager Crops;

		internal readonly List<Func<Menus.AlmanacMenu, ModEntry, IAlmanacPage>> PageBuilders = new();

		private GMCMIntegration<ModConfig, ModEntry> GMCMIntegration;

		public override void Entry(IModHelper helper) {
			base.Entry(helper);
			instance = this;
			API = new(this);

			Assets = new(this);

			I18n.Init(Helper.Translation);

			// Read Config
			Config = Helper.ReadConfig<ModConfig>();

			// Init
			RegisterBuilder(CoverPage.GetPage);
			RegisterBuilder(CropPage.GetPage);
			RegisterBuilder(WeatherPage.GetPage);
			RegisterBuilder(TrainPage.GetPage);
			RegisterBuilder(WeatherPage.GetIslandPage);
			RegisterBuilder(HoroscopePage.GetPage);
		}

		public override object GetApi() {
			return API;
		}

		#region Page Management

		void RegisterBuilder(Func<Menus.AlmanacMenu, ModEntry, IAlmanacPage> builder) {
			PageBuilders.Add(builder);
		}

		#endregion

		#region Events

		[Subscriber]
		private void OnButton(object sender, ButtonPressedEventArgs e) {
			if (Game1.activeClickableMenu != null || !(Config.UseKey?.JustPressed() ?? false))
				return;

			Helper.Input.SuppressActiveKeybinds(Config.UseKey);

			// If the player hasn't seen the event where they receive the Almanac, don't
			// let them use it unless it's always available.
			if (!Game1.player.eventsSeen.Contains(Event_Base) && !Config.AlmanacAlwaysAvailable)
				return;

			Game1.activeClickableMenu = new Menus.AlmanacMenu(Game1.Date.Year);
		}


		// We mark this event as high priority so we can change tomorrow's weather
		// before other mods might rely on it.

		[Subscriber]
		[EventPriority(EventPriority.High)]
		private void OnDayStarted(object sender, DayStartedEventArgs e) {
			int seed = GetBaseWorldSeed();

			if (Config.EnableDeterministicLuck && Game1.IsMasterGame) {
				Game1.player.team.sharedDailyLuck.Value = LuckHelper.GetLuckForDate(seed, Game1.Date);
			}

			if (Config.EnableDeterministicWeather) {
				WorldDate tomorrow = new WorldDate(Game1.Date);
				tomorrow.TotalDays++;

				// Main Weather
				Game1.weatherForTomorrow = WeatherHelper.GetWeatherForDate(seed, tomorrow, GameLocation.LocationContext.Default);
				if (Game1.IsMasterGame)
					Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).weatherForTomorrow.Value = Game1.weatherForTomorrow;

				// Island Weather
				if (Game1.IsMasterGame && Utility.doesAnyFarmerHaveOrWillReceiveMail("Visited_Island")) {
					var ctx = GameLocation.LocationContext.Island;

					Game1.netWorldState.Value.GetWeatherForLocation(ctx)
						.weatherForTomorrow.Value = WeatherHelper
							.GetWeatherForDate(seed, tomorrow, ctx);
				}
			}
		}

		[Subscriber]
		private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
			// More Init
			Crops = new(this);
			RegisterConfig();

			Helper.ConsoleCommands.Add("al_update", "Invalidate cached data.", (name, args) => {
				Crops.Invalidate();
			});

			Helper.ConsoleCommands.Add("al_forecast", "Get the forecast for the loaded save.", (name, args) => {
				int seed = GetBaseWorldSeed();
				WorldDate date = new WorldDate(Game1.Date);
				for (int i = 0; i < 4 * 28; i++) {
					int weather = WeatherHelper.GetWeatherForDate(seed, date, GameLocation.LocationContext.Default);
					Log($"Date: {date.Localize()} -- Weather: {WeatherHelper.GetWeatherName(weather)}");
					date.TotalDays++;
				}
			});
		}

		#endregion

		#region Configuration

		public void SaveConfig() {
			Helper.WriteConfig(Config);
		}

		public void ResetConfig() {
			Config = new();
		}

		public bool HasGMCM() {
			return GMCMIntegration?.IsLoaded ?? false;
		}

		public void OpenGMCM() {
			if (HasGMCM())
				GMCMIntegration.OpenMenu();
		}

		private void RegisterConfig() {
			GMCMIntegration = new(this, () => Config, ResetConfig, SaveConfig);
			if (!GMCMIntegration.IsLoaded)
				return;

			GMCMIntegration.Register(true);

			GMCMIntegration
				.Add(
					I18n.Settings_Available(),
					I18n.Settings_AvailableDesc(),
					c => c.AlmanacAlwaysAvailable,
					(c, v) => c.AlmanacAlwaysAvailable = v
				);

			GMCMIntegration
				.AddLabel(I18n.Settings_Controls(), null, "bindings")
				.AddLabel(I18n.Settings_Crops(), null, "page:crop")
				.AddLabel(I18n.Settings_Weather(), null, "page:weather")
				.AddLabel(I18n.Settings_Train(), null, "page:train");

			GMCMIntegration.StartPage("bindings", I18n.Settings_Controls());
			GMCMIntegration
				.Add(
					I18n.Settings_Controls_Almanac(),
					I18n.Settings_Controls_AlmanacDesc(),
					c => c.UseKey,
					(c, v) => c.UseKey = v
				);

			GMCMIntegration.StartPage("page:crop", I18n.Settings_Crops());

			GMCMIntegration
				.Add(
					I18n.Settings_Enable(),
					I18n.Settings_EnableDesc(),
					c => c.ShowCrops,
					(c, v) => c.ShowCrops = v
				);

			GMCMIntegration.AddLabel(""); // Spacer

			GMCMIntegration.AddLabel(I18n.Settings_Crops_Preview());
			GMCMIntegration.AddParagraph(I18n.Settings_Crops_PreviewDesc());

			GMCMIntegration
				.Add(
					I18n.Settings_Crops_Preview_Enable(),
					null,
					c => c.ShowPreviews,
					(c, v) => c.ShowPreviews = v
				);

			GMCMIntegration
				.Add(
					I18n.Settings_Crops_Preview_Sprite(),
					I18n.Settings_Crops_Preview_SpriteDesc(),
					c => c.PreviewUseHarvestSprite,
					(c, v) => c.PreviewUseHarvestSprite = v
				);

			GMCMIntegration
				.Add(
					I18n.Settings_Crops_Preview_Plantonfirst(),
					I18n.Settings_Crops_Preview_PlantonfirstDesc(),
					c => c.PreviewPlantOnFirst,
					(c, v) => c.PreviewPlantOnFirst = v
				);

			GMCMIntegration.StartPage("page:weather", I18n.Settings_Weather());

			GMCMIntegration
				.Add(
				I18n.Settings_Enable(),
				I18n.Settings_EnableDesc(),
				c => c.ShowWeather,
				(c, v) => c.ShowWeather = v
			);

			GMCMIntegration
				.Add(
					I18n.Settings_Weather_Deterministic(),
					I18n.Settings_Weather_DeterministicDesc(),
					c => c.EnableDeterministicWeather,
					(c, v) => c.EnableDeterministicWeather = v
				);

			GMCMIntegration.StartPage("page:train", I18n.Settings_Train());

			GMCMIntegration
				.Add(
				I18n.Settings_Enable(),
				I18n.Settings_EnableDesc(),
				c => c.ShowTrains,
				(c, v) => c.ShowTrains = v
			);
		}

		#endregion

		public int GetBaseWorldSeed() {
			// TODO: Check configuration.
			return (int) Game1.uniqueIDForThisGame;
		}

	}
}
