using System;
using System.Collections.Generic;

using Leclair.Stardew.BetterGameMenu.Menus;
using Leclair.Stardew.BetterGameMenu.Models;
using Leclair.Stardew.Common.Integrations;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarControl;

using StardewValley;

namespace Leclair.Stardew.BetterGameMenu.Integrations.StarControl;

internal class StarControlIntegration : BaseAPIIntegration<IStarControlApi, ModEntry> {

	private readonly Dictionary<string, GameMenuTabItem> RegisteredTabs = [];

	public StarControlIntegration(ModEntry self) : base(self, "focustense.StarControl", "1.0.0") {
		if (IsLoaded)
			Patches.StarControl_Patches.Patch(Self);
	}

	public void AddAllTabs() {
		if (!IsLoaded)
			return;

		List<GameMenuTabItem> newTabs = [];

		foreach(var (key, def) in Self.Tabs) {
			bool enabled = Self.PreferredImplementation.ContainsKey(key);
			DrawMethod? method = def.GetIcon().DrawMethod.Target as DrawMethod;

			if (RegisteredTabs.TryGetValue(key, out var tab)) {
				tab.Definition = def;
				tab.Enabled = enabled;
				tab.DrawMethod = method;
			} else {
				tab = new(key, def, enabled, method);
				RegisteredTabs[key] = tab;
				newTabs.Add(tab);
			}
		}

		if (newTabs.Count > 0)
			API.RegisterItems(Self.ModManifest, newTabs);
	}

	public void UpdateTab(string target) {
		if (!IsLoaded || !Self.Tabs.TryGetValue(target, out var def))
			return;

		bool enabled = Self.PreferredImplementation.ContainsKey(target);
		DrawMethod? method = def.GetIcon().DrawMethod.Target as DrawMethod;

		if (RegisteredTabs.TryGetValue(target, out var tab)) {
			tab.Definition = def;
			tab.Enabled = enabled;
			tab.DrawMethod = method;
		} else {
			tab = new(target, def, enabled, method);
			RegisteredTabs[target] = tab;
			API.RegisterItems(Self.ModManifest, [tab]);
		}
	}

}

class GameMenuTabItem(string id, TabDefinition definition, bool enabled, DrawMethod? drawMethod) : IRadialMenuItem {

	internal TabDefinition Definition = definition;
	internal DrawMethod? DrawMethod = drawMethod;

	public string Id => $"leclair.bettergamemenu:tab:{id}";

	public string Title => Definition.GetDisplayName();

	public string Description => string.Empty;

	public bool Enabled { get; internal set; } = enabled;

	public Texture2D? Texture => DrawMethod?.Texture;

	public Rectangle? SourceRectangle => DrawMethod?.CurrentSource;

	public ItemActivationResult Activate(
		Farmer who,
		DelayedActions delayedActions,
		ItemActivationType activationType = ItemActivationType.Primary
	) {
		if (delayedActions != DelayedActions.None)
			return ItemActivationResult.Delayed;

		if (Game1.activeClickableMenu is BetterGameMenuImpl bgm) {
			bgm.TryChangeTab(id);
			return ItemActivationResult.Custom;
		}

		if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu.readyToClose())
			Game1.exitActiveMenu();

		if (Game1.activeClickableMenu is null) {
			Game1.activeClickableMenu = new BetterGameMenuImpl(ModEntry.Instance, startingTab: id);
			return ItemActivationResult.Custom;
		}

		return ItemActivationResult.Ignored;
	}
}
