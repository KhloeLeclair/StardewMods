using System;
using System.Collections.Generic;

using Leclair.Stardew.BetterGameMenu.Menus;
using Leclair.Stardew.BetterGameMenu.Models;
using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu;

public class ModAPI : IBetterGameMenuApi {

	private readonly ModEntry Self;
	private readonly IModInfo Other;

	internal ModAPI(ModEntry self, IModInfo other) {
		Self = self;
		Other = other;
	}

	private readonly List<(IBetterGameMenuApi.MenuCreatedDelegate Handler, EventPriority Priority)> _MenuCreated = [];
	private readonly List<(IBetterGameMenuApi.TabChangedDelegate Handler, EventPriority Priority)> _TabChanged = [];
	private readonly List<(IBetterGameMenuApi.PageCreatedDelegate Handler, EventPriority Priority)> _PageCreated = [];

	private static int SortList<T>((T Handler, EventPriority Priority) first, (T Handler, EventPriority Priority) second) {
		return -first.Priority.CompareTo(second.Priority);
	}

	public void OnMenuCreated(IBetterGameMenuApi.MenuCreatedDelegate handler, EventPriority priority = EventPriority.Normal) {
		_MenuCreated.Add((handler, priority));
		_MenuCreated.Sort(SortList);
	}

	public void OffMenuCreated(IBetterGameMenuApi.MenuCreatedDelegate handler) {
		_MenuCreated.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnTabChanged(IBetterGameMenuApi.TabChangedDelegate handler, EventPriority priority) {
		_TabChanged.Add((handler, priority));
		_TabChanged.Sort(SortList);
	}

	public void OffTabChanged(IBetterGameMenuApi.TabChangedDelegate handler) {
		_TabChanged.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnPageCreated(IBetterGameMenuApi.PageCreatedDelegate handler, EventPriority priority = EventPriority.Normal) {
		_PageCreated.Add((handler, priority));
		_PageCreated.Sort(SortList);
	}

	public void OffPageCreated(IBetterGameMenuApi.PageCreatedDelegate handler) {
		_PageCreated.RemoveWhere(entry => entry.Handler == handler);
	}

	internal void FireMenuCreated(IClickableMenu menu) {
		try {
			foreach (var pair in _MenuCreated)
				pair.Handler(menu);

		} catch (Exception ex) {
			Self.Log($"Error in OnMenuCreated handler for mod '{Other.Manifest.Name}' ({Other.Manifest.UniqueID}): {ex}", LogLevel.Error);
		}
	}

	internal void FireTabChanged(BetterGameMenuImpl menu, string tab, string oldTab) {
		try {
			var evt = new TabChangedEvent(menu, tab, oldTab);
			foreach (var pair in _TabChanged)
				pair.Handler(evt);
		} catch (Exception ex) {
			Self.Log($"Error in OnTabChanged handler for mod '{Other.Manifest.Name}' ({Other.Manifest.UniqueID}): {ex}", LogLevel.Error);
		}
	}

	internal void FirePageCreated(BetterGameMenuImpl menu, string tab, string source, IClickableMenu page, IClickableMenu? oldPage) {
		try {
			var evt = new PageCreatedEvent(menu, tab, source, page, oldPage);
			foreach (var pair in _PageCreated)
				pair.Handler(evt);

		} catch (Exception ex) {
			Self.Log($"Error in OnPageCreated handler for mod '{Other.Manifest.Name}' ({Other.Manifest.UniqueID}): {ex}", LogLevel.Error);
		}
	}

	public static IBetterGameMenuApi.DrawDelegate CreateDrawImpl(Texture2D texture, Rectangle source, float scale, int frames = 1, int frameTime = 16) {
		var inst = new DrawMethod(texture, source, scale, frames, frameTime);
		return inst.Draw;
	}

	public IBetterGameMenuApi.DrawDelegate CreateDraw(Texture2D texture, Rectangle source, float scale, int frames = 1, int frameTime = 16) {
		return CreateDrawImpl(texture, source, scale, frames, frameTime);
	}


	public Type GetMenuType() {
		return typeof(BetterGameMenuImpl);
	}

	public void RegisterImplementation(
		string id,
		int priority,
		Func<IClickableMenu, IClickableMenu> getPageInstance,
		Func<IBetterGameMenuApi.DrawDelegate?>? getDecoration = null,
		Func<bool>? getTabVisible = null,
		Func<bool>? getMenuInvisible = null,
		Func<int, int>? getWidth = null,
		Func<int, int>? getHeight = null,
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null,
		Action<IClickableMenu>? onClose = null
	) {
		if (!Self.Tabs.ContainsKey(id))
			throw new KeyNotFoundException(id);

		Self.AddImplementation(id, new TabImplementationDefinition(
			Source: Other.Manifest.UniqueID,
			Priority: priority,
			GetPageInstance: getPageInstance,
			GetDecoration: getDecoration,
			GetTabVisible: getTabVisible,
			GetMenuInvisible: getMenuInvisible,
			GetWidth: getWidth,
			GetHeight: getHeight,
			OnResize: onResize,
			OnClose: onClose
		));
	}

	public void RegisterTab(
		string id,
		int order,
		Func<string> getDisplayName,
		Func<(IBetterGameMenuApi.DrawDelegate DrawMethod, bool DrawBackground)> getIcon,
		int priority,
		Func<IClickableMenu, IClickableMenu> getPageInstance,
		Func<IBetterGameMenuApi.DrawDelegate?>? getDecoration = null,
		Func<bool>? getTabVisible = null,
		Func<bool>? getMenuInvisible = null,
		Func<int, int>? getWidth = null,
		Func<int, int>? getHeight = null,
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null,
		Action<IClickableMenu>? onClose = null
	) {
		Self.AddTab(id, new TabDefinition(
			Order: order,
			GetDisplayName: getDisplayName,
			GetIcon: getIcon
		), new TabImplementationDefinition(
			Source: Other.Manifest.UniqueID,
			Priority: priority,
			GetPageInstance: getPageInstance,
			GetDecoration: getDecoration,
			GetTabVisible: getTabVisible,
			GetMenuInvisible: getMenuInvisible,
			GetWidth: getWidth,
			GetHeight: getHeight,
			OnResize: onResize,
			OnClose: onClose
		));
	}

	public IBetterGameMenu? ActiveMenu => Game1.activeClickableMenu is BetterGameMenuImpl bgm ? bgm : null;

	public IClickableMenu? ActivePage => Game1.activeClickableMenu is BetterGameMenuImpl bgm ? bgm.CurrentPage : null;

	public IBetterGameMenu? AsMenu(IClickableMenu menu) {
		return menu as BetterGameMenuImpl;
	}

	public IClickableMenu? GetCurrentPage(IClickableMenu menu) {
		return menu is BetterGameMenuImpl bgm ? bgm.CurrentPage : null;
	}

	public IClickableMenu CreateMenu(string? defaultTab = null, bool playSound = false) {
		Game1.PushUIMode();
		var result = new BetterGameMenuImpl(Self, defaultTab, playOpeningSound: playSound);
		Game1.PopUIMode();
		return result;
	}

	public IBetterGameMenu? TryOpenMenu(string? defaultTab = null, bool playSound = false, bool closeExistingMenu = false) {
		if (Game1.activeClickableMenu is not null) {
			if (!closeExistingMenu || !Game1.activeClickableMenu.readyToClose())
				return null;

			CommonHelper.YeetMenu(Game1.activeClickableMenu);
		}

		Game1.PushUIMode();
		var menu = new BetterGameMenuImpl(Self, defaultTab, playOpeningSound: playSound);
		Game1.PopUIMode();
		Game1.activeClickableMenu = menu;
		return menu;
	}
}
