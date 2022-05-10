using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using HarmonyLib;

using Leclair.Stardew.Common.Integrations.GenericModConfigMenu;
using Leclair.Stardew.Common.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Toolkit.Serialization;

namespace Leclair.Stardew.ThemeManager;

public class ModEntry : ModSubscriber {

	#nullable disable
	public static ModEntry Instance { get; private set; }
	public static IThemeManagerApi API { get; private set; }

	internal ModConfig Config;
	#nullable enable

	internal Harmony? Harmony;
	internal GMCMIntegration<ModConfig, ModEntry>? intGMCM;

	internal bool ConfigStale = false;

	internal JsonHelper? JsonHelper;

	internal readonly Dictionary<IManifest, IContentPack> ContentPacks = new();
	internal readonly Dictionary<string, Func<object>> LoadingAssets = new();
	internal readonly Dictionary<IManifest, (Type, IThemeManager)> Managers = new();

	internal ThemeManager<Models.BaseTheme>? BaseThemeManager;

	internal Models.BaseTheme? BaseTheme;

	#region Construction

	public override void Entry(IModHelper helper) {
		base.Entry(helper);

		Instance = this;

		// Harmony
		Harmony = new Harmony(ModManifest.UniqueID);

		Patches.Billboard_Patches.Patch(this);
		// TODO: BobberBar
		Patches.CarpenterMenu_Patches.Patch(this);
		Patches.CharacterCustomization_Patches.Patch(this);
		Patches.CoopMenu_Patches.Patch(this);
		Patches.DayTimeMoneyBox_Patches.Patch(this);
		// TODO: DigitEntryMenu
		Patches.ExitPage_Patches.Patch(this);
		Patches.ForgeMenu_Patches.Patch(this);
		Patches.IClickableMenu_Patches.Patch(this);
		Patches.LetterViewerMenu_Patches.Patch(this);
		Patches.LevelUpMenu_Patches.Patch(this);
		Patches.LoadGameMenu_Patches.Patch(this);
		Patches.MineElevatorMenu_Patches.Patch(this);
		// TODO: MuseumMenu
		// TODO: NumberSelectionMenu
		Patches.OptionsDropDown_Patches.Patch(this);
		Patches.QuestLog_Patches.Patch(this);
		Patches.ShopMenu_Patches.Patch(this);
		// TODO: SkillsPage
		// TODO: SliderBar
		Patches.SpriteText_Patches.Patch(this);
		Patches.TutorialMenu_Patches.Patch(this);
		Patches.Utility_Patches.Patch(this);

		// Read Config
		Config = Helper.ReadConfig<ModConfig>();

		// I18n
		I18n.Init(Helper.Translation);

		// Base Theme
		BaseTheme = Models.BaseTheme.GetDefaultTheme();
		BaseThemeManager = new ThemeManager<Models.BaseTheme>(
			mod: this,
			other: ModManifest,
			selectedThemeId: Config.StardewTheme ?? "automatic",
			defaultTheme: BaseTheme
		);

		BaseThemeManager.ThemeChanged += OnStardewThemeChanged;
		BaseThemeManager.Discover();

		// API
		API = new ModAPI(this);
	}

	public override object GetApi() {
		return API;
	}

	#endregion

	#region Configuration

	internal void ResetConfig() {
		Config = new ModConfig();
	}

	internal void SaveConfig() {
		Helper.WriteConfig(Config);
	}

	[MemberNotNullWhen(true, nameof(intGMCM))]
	internal bool HasGMCM() {
		return intGMCM is not null && intGMCM.IsLoaded;
	}

	internal void OpenGMCM() {
		if (!HasGMCM())
			return;

		if (ConfigStale)
			RegisterSettings();

		intGMCM.OpenMenu();
	}

	internal void RegisterSettings() {
		if (intGMCM is null)
			intGMCM = new(this, () => Config, ResetConfig, SaveConfig);

		// Un-register and re-register so we can redo our settings.
		intGMCM.Unregister();
		intGMCM.Register(true);

		var choices = BaseThemeManager!.GetThemeChoiceMethods();

		intGMCM.AddChoice(
			name: () => "Game Theme",
			tooltip: () => "Override the game's color theme.",
			get: c => c.StardewTheme,
			set: (c,v) => {
				c.StardewTheme = v;
				BaseThemeManager!._SelectTheme(v);
			},
			choices: choices
		);

		intGMCM.AddLabel(I18n.Settings_ModThemes);

		foreach (var entry in Managers) {

			string uid = entry.Key.UniqueID;
			var mchoices = entry.Value.Item2.GetThemeChoiceMethods();

			string Getter(ModConfig cfg) {
				if (cfg.SelectedThemes.TryGetValue(uid, out string? value))
					return value;
				return "automatic";
			}

			void Setter(ModConfig cfg, string value) {
				cfg.SelectedThemes[uid] = value;
				if (Managers.TryGetValue(entry.Key, out var mdata) && mdata.Item2 is IThemeSelection tselect)
					tselect._SelectTheme(value);
			}

			intGMCM.AddChoice(
				name: () => entry.Key.Name,
				tooltip: null,
				get: Getter,
				set: Setter,
				choices: mchoices
			);
		}

		ConfigStale = false;
	}

	#endregion

	#region Content Pack Access

	internal void GetJsonHelper() {
		if (JsonHelper is not null)
			return;

		if (Helper.Data.GetType().GetField("JsonHelper", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(Helper.Data) is JsonHelper helper) {
			JsonHelper = new();
			var converters = JsonHelper.JsonSettings.Converters;
			converters.Clear();
			foreach(var converter in helper.JsonSettings.Converters)
				if (converter.GetType().Name != "ColorConverter")
					converters.Add(converter);

			converters.Add(new Leclair.Stardew.Common.Serialization.Converters.ColorConverter());
		}
	}

	internal TModel? ReadJsonFile<TModel>(string path, IContentPack pack) where TModel : class {
		if (JsonHelper is null)
			GetJsonHelper();

		if (JsonHelper is not null) {
			if (JsonHelper.ReadJsonFileIfExists(Path.Join(pack.DirectoryPath, path), out TModel? result))
				return result;
			return null;
		}
		
		return pack.ReadJsonFile<TModel>(path);
	}

	internal IContentPack? GetContentPackFor(IManifest manifest) {
		if (ContentPacks.TryGetValue(manifest, out IContentPack? cp))
			return cp;

		IModInfo? info = Helper.ModRegistry.Get(manifest.UniqueID);
		if (info is null)
			return null;

		return GetContentPackFor(info);
	}

	internal IContentPack? GetContentPackFor(IModInfo mod) {
		if (ContentPacks.TryGetValue(mod.Manifest, out IContentPack? cp))
			return cp;

		if (mod.IsContentPack && mod.GetType().GetProperty("ContentPack", BindingFlags.Instance | BindingFlags.Public)?.GetValue(mod) is IContentPack pack)
			cp = pack;

		else if (mod.GetType().GetProperty("DirectoryPath", BindingFlags.Instance | BindingFlags.Public)?.GetValue(mod) is string str) {
			cp = Helper.ContentPacks.CreateTemporary(
				directoryPath: str,
				id: $"leclair.theme-loader.${mod.Manifest.UniqueID}",
				name: mod.Manifest.Name,
				description: mod.Manifest.Description,
				author: mod.Manifest.Author,
				version: mod.Manifest.Version
			);

		} else
			return null;

		ContentPacks[mod.Manifest] = cp;
		return cp;
	}

	#endregion

	#region Events

	private void OnStardewThemeChanged(object? sender, IThemeChangedEvent<Models.BaseTheme> e) {
		BaseTheme = e.NewData;

		Game1.textColor = BaseTheme.TextColor ?? BaseThemeManager!.DefaultTheme.TextColor!.Value;
		Game1.textShadowColor = BaseTheme.TextShadowColor ?? BaseThemeManager!.DefaultTheme.TextShadowColor!.Value;
	}

	[Subscriber]
	private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
		// Integrations
		CheckRecommendedIntegrations();

		// Commands
		Helper.ConsoleCommands.Add("theme", "View available themes, reload themes, and change the current themes.", (_, args) => {

		});

		Helper.ConsoleCommands.Add("retheme", "Reload all themes.", (_, _) => {
			BaseThemeManager!.Invalidate();
			BaseThemeManager!.Discover();

			foreach (var entry in Managers) {
				entry.Value.Item2.Invalidate();
				entry.Value.Item2.Discover();
			}

			Log($"Reloaded all themes across {Managers.Count + 1} managers.", LogLevel.Info);
		});

		// Settings
		RegisterSettings();
		Helper.Events.Display.RenderingActiveMenu += OnDrawMenu;
	}

	private void OnDrawMenu(object? sender, RenderingActiveMenuEventArgs e) {
		// Rebuild our settings menu when first drawing the title menu, since
		// the MenuChanged event doesn't handle the TitleMenu.
		Helper.Events.Display.RenderingActiveMenu -= OnDrawMenu;

		if (ConfigStale)
			RegisterSettings();
	}

	[Subscriber]
	private void OnMenuChanged(object? sender, MenuChangedEventArgs e) {
		IClickableMenu? menu = e.NewMenu;
		if (menu is null)
			return;

		Type type = menu.GetType();
		string? name = type.FullName ?? type.Name;

		if (name is not null && name.Equals("GenericModConfigMenu.Framework.ModConfigMenu")) {
			if (ConfigStale)
				RegisterSettings();
		}
	}

	[Subscriber]
	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e) {
		Func<object>? loader;
		lock ((LoadingAssets as ICollection).SyncRoot) {
			if (!LoadingAssets.TryGetValue(e.Name.BaseName, out loader))
				return;
		}

		e.LoadFrom(
			loader,
			priority: AssetLoadPriority.Low
		);
	}

	#endregion

}
