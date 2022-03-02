using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json.Linq;

using Leclair.Stardew.Common.Events;
using Leclair.Stardew.Common.Integrations.GenericModConfigMenu;
using Leclair.Stardew.Common.UI.Overlay;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

using Leclair.Stardew.Almanac.Crops;
using Leclair.Stardew.Almanac.Managers;
using Leclair.Stardew.Almanac.Pages;

namespace Leclair.Stardew.Almanac {
	public class ModEntry : ModSubscriber {

		public static readonly string NPCMapLocationPath = "Mods/Bouhm.NPCMapLocations/NPCs";

		public static readonly string Mail_Prefix = "leclair.almanac";

		public static readonly string Mail_Has_Base = $"{Mail_Prefix}.has_base";
		public static readonly string Mail_Has_Island = $"{Mail_Prefix}.has_island";
		public static readonly string Mail_Has_Magic = $"{Mail_Prefix}.has_magic";

		public static readonly string Mail_Seen_Base = $"{Mail_Prefix}.seen_base";
		public static readonly string Mail_Seen_Island = $"{Mail_Prefix}.seen_island";
		public static readonly string Mail_Seen_Magic = $"{Mail_Prefix}.seen_magic";

		/*public static readonly int Event_Base   = 11022000;
		public static readonly int Event_Island = 11022001;
		public static readonly int Event_Magic  = 11022002;*/

		public static int DaysPerMonth = WorldDate.DaysPerMonth;

		public static ModEntry instance;
		public static ModAPI API;

		private readonly PerScreen<IClickableMenu> CurrentMenu = new();
		private readonly PerScreen<IOverlay> CurrentOverlay = new();

		public ModConfig Config;

		public WeatherManager Weather;
		public LuckManager Luck;
		public NoticesManager Notices;

		internal AssetManager Assets;
		internal CropManager Crops;

		internal readonly List<Func<Menus.AlmanacMenu, ModEntry, IAlmanacPage>> PageBuilders = new();

		internal Dictionary<string, Models.HeadSize> HeadSizes;

		private GMCMIntegration<ModConfig, ModEntry> GMCMIntegration;

		public override void Entry(IModHelper helper) {
			base.Entry(helper);
			instance = this;
			API = new(this);

			Assets = new(this);

			I18n.Init(Helper.Translation);

			// Read Config
			Config = Helper.ReadConfig<ModConfig>();

			Weather = new(this);
			Luck = new(this);
			Notices = new(this);

			// Init
			RegisterBuilder(CoverPage.GetPage);
			RegisterBuilder(CropPage.GetPage);
			RegisterBuilder(WeatherPage.GetPage);
			RegisterBuilder(WeatherPage.GetIslandPage);
			RegisterBuilder(TrainPage.GetPage);
			RegisterBuilder(FortunePage.GetPage);
			RegisterBuilder(MinesPage.GetPage);
			RegisterBuilder(NoticesPage.GetPage);
		}

		public override object GetApi() {
			return API;
		}

		#region Loading

		private void LoadHeads() {
			const string path = "assets/heads.json";
			Dictionary<string, Models.HeadSize> heads = null;

			try {
				heads = Helper.Data.ReadJsonFile<Dictionary<string, Models.HeadSize>>(path);
				if (heads == null)
					Log($"The {path} file is missing or invalid.", LogLevel.Error);
			} catch(Exception ex) {
				Log($"The {path} file is invalid.", LogLevel.Error, ex);
			}

			if (heads == null)
				heads = new();

			// Read any extra data files
			foreach (var cp in Helper.ContentPacks.GetOwned()) {
				if (!cp.HasFile("heads.json"))
					continue;

				Dictionary<string, Models.HeadSize> extra = null;
				try {
					extra = cp.ReadJsonFile<Dictionary<string, Models.HeadSize>>("heads.json");
				} catch (Exception ex) {
					Log($"The heads.json file of {cp.Manifest.Name} is invalid.", LogLevel.Error, ex);
				}

				if (extra != null)
					foreach (var entry in extra)
						if (!string.IsNullOrEmpty(entry.Key))
							heads[entry.Key] = entry.Value;
			}

			// Now, read the data file used by NPC Map Locations. This is
			// convenient because a lot of mods support it.
			Dictionary<string, JObject> content = null;

			try {
				content = Helper.Content.Load<Dictionary<string, JObject>>(NPCMapLocationPath, ContentSource.GameContent);

			} catch (Exception) {
				/* Nothing~ */
			}

			if (content != null) {
				int count = 0;

				foreach (var entry in content) {
					if (heads.ContainsKey(entry.Key))
						continue;

					int offset;
					try {
						offset = entry.Value.Value<int>("MarkerCropOffset");
					} catch (Exception) {
						continue;
					}

					heads[entry.Key] = new() {
						OffsetY = offset
					};
					count++;
				}

				Log($"Loaded {count} head offsets from NPC Map Location data.");
			}

			HeadSizes = heads;
		}

		#endregion

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
			if (! HasAlmanac(Game1.player) )
				return;

			if (Game1.activeClickableMenu != null || Game1.CurrentEvent != null)
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
				Game1.player.team.sharedDailyLuck.Value = Luck.GetLuckForDate(seed, Game1.Date);
			}

			if (Config.EnableDeterministicWeather) {
				WorldDate tomorrow = new WorldDate(Game1.Date);
				tomorrow.TotalDays++;

				// Main Weather
				Game1.weatherForTomorrow = Weather.GetWeatherForDate(seed, tomorrow, GameLocation.LocationContext.Default);
				if (Game1.IsMasterGame)
					Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Default).weatherForTomorrow.Value = Game1.weatherForTomorrow;

				// Island Weather
				if (Game1.IsMasterGame && Utility.doesAnyFarmerHaveOrWillReceiveMail("Visited_Island")) {
					var ctx = GameLocation.LocationContext.Island;

					Game1.netWorldState.Value.GetWeatherForLocation(ctx)
						.weatherForTomorrow.Value = Weather
							.GetWeatherForDate(seed, tomorrow, ctx);
				}
			}
		}

		[Subscriber]
		private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
			// Load Data

			// More Init
			Crops = new(this);
			RegisterConfig();

			Helper.ConsoleCommands.Add("al_update", "Invalidate cached data.", (name, args) => {
				Assets.Invalidate();
				Crops.Invalidate();
				Weather.Invalidate();
			});

			Helper.ConsoleCommands.Add("al_forecast", "Get the forecast for the loaded save.", (name, args) => {
				int seed = GetBaseWorldSeed();
				WorldDate date = new WorldDate(Game1.Date);
				for (int i = 0; i < 4 * 28; i++) {
					int weather = Weather.GetWeatherForDate(seed, date, GameLocation.LocationContext.Default);
					Log($"Date: {date.Localize()} -- Weather: {WeatherHelper.GetWeatherName(weather)}");
					date.TotalDays++;
				}
			});

			Helper.ConsoleCommands.Add("al_test", "boop", (name, args) => {
				foreach(var location in Game1.locations) {
					Log($"  {location.Name}: {GetLocationName(location)}");
				}
			});
		}

		[Subscriber]
		private void OnMenuChanged(object sender, MenuChangedEventArgs e) {
			IClickableMenu menu = Game1.activeClickableMenu;
			if (CurrentMenu.Value == menu)
				return;

			CurrentMenu.Value = menu;

			if (CurrentOverlay.Value != null) {
				CurrentOverlay.Value.Dispose();
				CurrentOverlay.Value = null;
			}

			if (!HasAlmanac(Game1.player) || ! Config.ShowAlmanacButton)
				return;

			InventoryPage page = null;

			if (menu is InventoryPage)
				page = menu as InventoryPage;
			else if (menu is GameMenu gm) {
				for(int i = 0; i < gm.pages.Count; i++) {
					if (gm.pages[i] is InventoryPage)
						page = gm.pages[i] as InventoryPage;
				}
			}

			if (page != null)
				CurrentOverlay.Value = new Overlays.InventoryOverlay(page, Game1.player);
		}

		[Subscriber]
		private void OnSaveLoaded(object sender, SaveLoadedEventArgs e) {
			// Load Data
			LoadHeads();

			// Detect Season Length
			WorldDate day = new WorldDate(1, "summer", 1);
			day.TotalDays--;
			DaysPerMonth = day.DayOfMonth;

			if (DaysPerMonth != WorldDate.DaysPerMonth)
				Log($"Using Non-Standard Days Per Month: {DaysPerMonth}");

			// Apply Player Flags
			UpdateFlags();
		}

		#endregion

		#region Configuration

		public void SaveConfig() {
			Helper.WriteConfig(Config);
			UpdateFlags();
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
					I18n.Settings_Button,
					I18n.Settings_ButtonDesc,
					c => c.ShowAlmanacButton,
					(c, v) => c.ShowAlmanacButton = v
				)
				.Add(
					I18n.Settings_RestoreState,
					I18n.Settings_RestoreState_Desc,
					c => c.RestoreAlmanacState,
					(c, v) => c.RestoreAlmanacState = v
				)
				.Add(
					I18n.Settings_CycleTime,
					I18n.Settings_CycleTime_Desc,
					c => (float) c.CycleTime / 1000f,
					(c, v) => c.CycleTime = (int) Math.Floor(v * 1000),
					min: 0.25f,
					max: 10f,
					interval: 0.25f
				)
				.AddLabel("") // Spacer
				.Add(
					I18n.Settings_Available,
					I18n.Settings_AvailableDesc,
					c => c.AlmanacAlwaysAvailable,
					(c, v) => c.AlmanacAlwaysAvailable = v
				)
				.Add(
					I18n.Settings_Island,
					I18n.Settings_IslandDesc,
					c => c.IslandAlwaysAvailable,
					(c, v) => c.IslandAlwaysAvailable = v
				)
				.Add(
					I18n.Settings_Magic,
					I18n.Settings_MagicDesc,
					c => c.MagicAlwaysAvailable,
					(c, v) => c.MagicAlwaysAvailable = v
				);

			GMCMIntegration
				.AddLabel(I18n.Settings_Controls, null, "bindings")
				.AddLabel(I18n.Settings_Crops, null, "page:crop")
				.AddLabel(I18n.Settings_Weather, null, "page:weather")
				.AddLabel(I18n.Settings_Train, null, "page:train")
				.AddLabel(I18n.Settings_Notices, null, "page:notices")
				.AddLabel(I18n.Settings_Fortune, null, "page:fortune")
				.AddLabel(I18n.Settings_Mines, null, "page:mines");

			GMCMIntegration
				.StartPage("bindings", I18n.Settings_Controls)
				.Add(
					I18n.Settings_Controls_Almanac,
					I18n.Settings_Controls_AlmanacDesc,
					c => c.UseKey,
					(c, v) => c.UseKey = v
				);

			GMCMIntegration
				.StartPage("page:crop", I18n.Settings_Crops)
				.Add(
					I18n.Settings_Enable,
					I18n.Settings_EnableDesc,
					c => c.ShowCrops,
					(c, v) => c.ShowCrops = v
				)

				.AddLabel("") // Spacer
				.AddLabel(I18n.Settings_Crops_Preview)
				.AddParagraph(I18n.Settings_Crops_PreviewDesc)

				.Add(
					I18n.Settings_Crops_Preview_Enable,
					null,
					c => c.ShowPreviews,
					(c, v) => c.ShowPreviews = v
				)
				.Add(
					I18n.Settings_Crops_Preview_Sprite,
					I18n.Settings_Crops_Preview_SpriteDesc,
					c => c.PreviewUseHarvestSprite,
					(c, v) => c.PreviewUseHarvestSprite = v
				)
				.Add(
					I18n.Settings_Crops_Preview_Plantonfirst,
					I18n.Settings_Crops_Preview_PlantonfirstDesc,
					c => c.PreviewPlantOnFirst,
					(c, v) => c.PreviewPlantOnFirst = v
				);

			GMCMIntegration
				.StartPage("page:weather", I18n.Settings_Weather)
				.Add(
					I18n.Settings_Enable,
					I18n.Settings_EnableDesc,
					c => c.ShowWeather,
					(c, v) => c.ShowWeather = v
				)

				.SetTitleOnly(true)
				.Add(
					I18n.Settings_Weather_Deterministic,
					I18n.Settings_Weather_DeterministicDesc,
					c => c.EnableDeterministicWeather,
					(c, v) => c.EnableDeterministicWeather = v
				)
				.Add(
					I18n.Settings_Weather_Rules,
					I18n.Settings_Weather_RulesDesc,
					c => c.EnableWeatherRules,
					(c, v) => c.EnableWeatherRules = v
				)
				.SetTitleOnly(false);

			GMCMIntegration
				.StartPage("page:train", I18n.Settings_Train)
				.Add(
					I18n.Settings_Enable,
					I18n.Settings_EnableDesc,
					c => c.ShowTrains,
					(c, v) => c.ShowTrains = v
				);

			GMCMIntegration
				.StartPage("page:fortune", I18n.Settings_Fortune)
				.Add(
					I18n.Settings_Enable,
					I18n.Settings_EnableDesc,
					c => c.ShowFortunes,
					(c, v) => c.ShowFortunes = v
				)
				.Add(
					I18n.Settings_Fortune_Exact,
					I18n.Settings_Fortune_ExactDesc,
					c => c.ShowExactLuck,
					(c, v) => c.ShowExactLuck = v
				)

				.SetTitleOnly(true)
				.Add(
					I18n.Settings_Fortune_Deterministic,
					I18n.Settings_Fortune_DeterministicDesc,
					c => c.EnableDeterministicLuck,
					(c, v) => c.EnableDeterministicLuck = v
				)
				.SetTitleOnly(false);

			GMCMIntegration
				.StartPage("page:mines", I18n.Settings_Mines)
				.Add(
					I18n.Settings_Enable,
					I18n.Settings_EnableDesc,
					c => c.ShowMines,
					(c, v) => c.ShowMines = v
				);

			GMCMIntegration
				.StartPage("page:notices", I18n.Settings_Notices)
				.Add(
					I18n.Settings_Enable,
					I18n.Settings_EnableDesc,
					c => c.ShowNotices,
					(c, v) => c.ShowNotices = v
				);
		}

		#endregion

		#region Flags and Access

		public void UpdateFlags() {
			foreach (var who in Game1.getOnlineFarmers()) {
				// Try checking which players are running on the local machine,
				// and update their mail flags.
				bool local = who.IsLocalPlayer;
				if ( ! local ) {
					var wm = Helper.Multiplayer.GetConnectedPlayer(who.UniqueMultiplayerID);
					if (wm != null)
						local = wm.IsSplitScreen;
				}

				if (local)
					UpdateFlags(who);
			}
		}

		private void ToggleMail(Farmer who, string key, bool has) {
			if (has)
				AddMail(who, key);
			else
				RemoveMail(who, key);
		}

		private void AddMail(Farmer who, string key) {
			if (!who.mailReceived.Contains(key))
				who.mailReceived.Add(key);
		}

		private void RemoveMail(Farmer who, string key) {
			if (who.mailReceived.Contains(key))
				who.mailReceived.Remove(key);
		}

		public void UpdateFlags(Farmer who) {
			bool seen_base = who.mailReceived.Contains(Mail_Seen_Base);
			bool seen_magic = who.mailReceived.Contains(Mail_Seen_Magic);
			bool seen_island = who.mailReceived.Contains(Mail_Seen_Island);

			ToggleMail(who, Mail_Has_Base, Config.AlmanacAlwaysAvailable || seen_base);
			ToggleMail(who, Mail_Has_Magic, Config.MagicAlwaysAvailable || seen_magic);
			ToggleMail(who, Mail_Has_Island, Config.IslandAlwaysAvailable || seen_island);
		}

		public bool HasAlmanac(Farmer who) {
			return Config.AlmanacAlwaysAvailable || who.mailReceived.Contains(Mail_Has_Base);
		}

		public bool HasIsland(Farmer who) {
			return Config.IslandAlwaysAvailable || who.mailReceived.Contains(Mail_Has_Island);
		}

		public bool HasMagic(Farmer who) {
			return Config.MagicAlwaysAvailable || who.mailReceived.Contains(Mail_Has_Magic);
		}

		#endregion

		public int GetBaseWorldSeed() {
			// TODO: Check configuration?
			return (int) Game1.uniqueIDForThisGame;
		}

		public bool DoesTranslationExist(string key) {
			// SMAPI's translation API is very opaque.
			// But SMAPI's reflection helper is here to help with SMAPI.
			// Thank you, SMAPI.
			FieldInfo field = Helper.Translation.GetType()
				.GetField("Translator", BindingFlags.Instance | BindingFlags.NonPublic);

			if (field == null)
				return false;

			object Translator = field.GetValue(Helper.Translation);
			if (Translator == null)
				return false;

			FieldInfo flfield = Translator.GetType()
				.GetField("ForLocale", BindingFlags.Instance | BindingFlags.NonPublic);

			if (flfield == null)
				return false;

			IDictionary<string, Translation> ForLocale = flfield.GetValue(Translator) as IDictionary<string, Translation>;
			return ForLocale != null && ForLocale.ContainsKey(key);
		}

		public string GetLocationName(GameLocation location) {
			string name = location.Name;
			string key = $"location.{name}";
			if (DoesTranslationExist(key))
				return Helper.Translation.Get(key).ToString();

			key = null;

			if (name.StartsWith("UndergroundMine") || name == "Mine") {
				if (location is MineShaft shaft && shaft.mineLevel > 120 && shaft.mineLevel != 77377)
					key = "Strings\\StringsFromCSFiles:MapPage.cs.11062";
				else
					key = "Strings\\StringsFromCSFiles:MapPage.cs.11098";

			} else if (location is IslandLocation)
				key = "Strings\\StringsFromCSFiles:IslandName";

			else {
				switch(name) {
					case "AdventureGuild":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11099";
						break;
					case "AnimalShop":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11068";
						break;
					case "ArchaeologyHouse":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11086";
						break;
					case "BathHouse_Entry":
					case "BathHouse_MensLocker":
					case "BathHouse_Pool":
					case "BathHouse_WomensLocker":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11110";
						break;
					case "Club":
					case "Desert":
					case "SandyHouse":
					case "SandyShop":
					case "SkullCave":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11062";
						break;
					case "CommunityCenter":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11117";
						break;
					case "ElliottHouse":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11088";
						break;
					case "FishShop":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11107";
						break;
					case "HarveyRoom":
					case "Hospital":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11076";
						break;
					case "JoshHouse":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11092";
						break;
					case "ManorHouse":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11085";
						break;
					case "Railroad":
					case "WitchWarpCave":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11119";
						break;
					case "ScienceHouse":
					case "SebastianRoom":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11094";
						break;
					case "SeedShop":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11078";
						break;
					case "Temp":
						if (location.Map.Id.Contains("Town"))
							key = "Strings\\StringsFromCSFiles:MapPage.cs.11190";
						break;
					case "Trailer_Big":
						key = "Strings\\StringsFromCSFiles:MapPage.PamHouse";
						break;
					case "WizardHouse":
					case "WizardHouseBasement":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11067";
						break;
					case "Woods":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11114";
						break;

					case "Backwoods":
					case "Tunnel":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11180";
						break;
					case "Barn":
					case "Big Barn":
					case "Big Coop":
					case "Big Shed":
					case "Cabin":
					case "Coop":
					case "Deluxe Barn":
					case "Deluxe Coop":
					case "Farm":
					case "FarmCave":
					case "FarmHouse":
					case "Greenhouse":
					case "Shed":
					case "Slime Hutch":
						return Game1.content.LoadString(
							"Strings\\StringsFromCSFiles:MapPage.cs.11064",
							Game1.player.farmName.Value
						);
					case "Beach":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11174";
						break;
					case "Forest":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11186";
						break;
					case "Mountain":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11176";
						break;
					case "Saloon":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11172";
						break;
					case "Town":
						key = "Strings\\StringsFromCSFiles:MapPage.cs.11190";
						break;
				}
			}

			if (key != null)
				return Game1.content.LoadString(key);

			return name;
		}

	}
}
