using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;
using Leclair.Stardew.BetterGameMenu.Models;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu;

public partial class ModEntry : ModSubscriber {

	public static ModEntry Instance { get; private set; } = null!;

	public ModConfig Config { get; private set; } = null!;
	internal Harmony Harmony = null!;

	internal readonly Dictionary<string, TabDefinition> Tabs = [];
	internal List<string> SortedTabs = [];
	internal readonly Dictionary<string, TabImplementationDefinition> PreferredImplementation = [];
	internal readonly Dictionary<string, Dictionary<string, TabImplementationDefinition>> Implementations = [];

	internal readonly Dictionary<string, ModAPI> APIInstances = [];

	public override void Entry(IModHelper helper) {
		base.Entry(helper);

		Instance = this;

		// Read Config
		Config = Helper.ReadConfig<ModConfig>();

		// I18n
		I18n.Init(Helper.Translation);

		// Harmony
		Harmony = new Harmony(ModManifest.UniqueID);

		Patches.Farmer_Patches.Patch(this);
		Patches.Game1_Patches.Patch(this);
		Patches.GameLocation_Patches.Patch(this);
		Patches.IClickableMenu_Patches.Patch(this);
		Patches.JunimoNoteMenu_Patches.Patch(this);
		Patches.MapPage_Patches.Patch(this);
		Patches.SocialPage_Patches.Patch(this);
#if DEBUG
		Patches.TestPatches.Patch(this);
#endif

		// Other Initialization
		RegisterDefaultTabs();
	}

	public override object? GetApi(IModInfo mod) {
		if (!APIInstances.TryGetValue(mod.Manifest.UniqueID, out var api)) {
			api = new ModAPI(this, mod);
			APIInstances[mod.Manifest.UniqueID] = api;
		}

		return api;
	}

	internal void FireMenuInstantiated(BetterGameMenuImpl menu) {
		foreach (var api in APIInstances.Values) {
			api.FireMenuCreated(menu);
		}
	}

	internal void FireTabChanged(BetterGameMenuImpl menu, string tab, string oldTab) {
		foreach (var api in APIInstances.Values) {
			api.FireTabChanged(menu, tab, oldTab);
		}
	}

	internal void FirePageInstantiated(BetterGameMenuImpl menu, string tab, string source, IClickableMenu page, IClickableMenu? oldPage) {
		foreach (var api in APIInstances.Values) {
			api.FirePageCreated(menu, tab, source, page, oldPage);
		}
	}

	internal IEnumerable<ModAPI> EnumerateAPIs() {
		return APIInstances.Values;
	}

	internal (TabDefinition Tab, TabImplementationDefinition Implementation)? GetVanillaImplementation(string target) {
		if (!Tabs.TryGetValue(target, out var tab) || !Implementations.TryGetValue(target, out var impls) || !impls.TryGetValue("stardew", out var impl))
			return null;
		return (tab, impl);
	}

	internal IEnumerable<(string Key, TabDefinition Tab, TabImplementationDefinition Implementation)> GetTabImplementations() {
		foreach (string key in SortedTabs) {
			if (!Tabs.TryGetValue(key, out var tab) || !PreferredImplementation.TryGetValue(key, out var impl))
				continue;
			yield return (key, tab, impl);
		}
	}

	internal void AddTab(string key, TabDefinition definition, TabImplementationDefinition? impl = null) {
		Tabs[key] = definition;
		SortedTabs = Tabs.Keys.ToList();
		SortedTabs.Sort((a, b) => {
			Tabs.TryGetValue(a, out var tabA);
			Tabs.TryGetValue(b, out var tabB);

			int orderA = tabA?.Order ?? 0;
			int orderB = tabB?.Order ?? 0;

			int result = orderA.CompareTo(orderB);
			if (result == 0)
				result = a.CompareTo(b);

			return result;
		});

		if (impl is not null)
			AddImplementation(key, impl);

		ConfigStale = true;
	}

	internal void AddImplementation(string key, TabImplementationDefinition definition) {
		if (!Implementations.TryGetValue(key, out var impl)) {
			impl = [];
			Implementations.Add(key, impl);
		}

		impl[definition.Source] = definition;
		UpdatePreferred(key);

		ConfigStale = true;
	}

	internal void RemoveImplementation(string key, string source) {
		if (!Implementations.TryGetValue(key, out var impl) || !impl.ContainsKey(source))
			return;

		impl.Remove(source);
		UpdatePreferred(key);
	}

	internal void UpdatePreferred() {
		foreach (string key in Implementations.Keys) {
			UpdatePreferred(key);
		}
	}

	internal void UpdatePreferred(string key) {
		Implementations.TryGetValue(key, out var impl);

		if (impl != null && Config.PreferredImplementation.TryGetValue(key, out string? source) &&
			!string.IsNullOrEmpty(source) &&
			impl.TryGetValue(source, out var preferred)
		) {
			PreferredImplementation[key] = preferred;

		} else if (impl != null && impl.Count > 1) {
			var sorted = impl.Values.ToList();
			sorted.Sort(CompareImplementations);
			PreferredImplementation[key] = sorted.First();

		} else if (impl != null && impl.Count == 1) {
			PreferredImplementation[key] = impl.First().Value;

		} else if (PreferredImplementation.ContainsKey(key))
			PreferredImplementation.Remove(key);
	}

	internal static int CompareImplementations(TabImplementationDefinition first, TabImplementationDefinition second) {
		int result = -first.Priority.CompareTo(second.Priority);
		if (result == 0)
			result = first.Source.CompareTo(second.Source);

		return result;
	}

	#region Events

	[Subscriber]
	[EventPriority((EventPriority) int.MinValue)]
	private void AfterGameLaunched(object? sender, GameLaunchedEventArgs e) {
		var builder = ReflectionHelper.WhatPatchesMe(this, "  ", false);
		if (builder is not null)
			Log($"Detected Harmony Patches:\n{builder}", LogLevel.Trace);
	}

	[Subscriber]
	private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) {
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

	#endregion

}
