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

	private static readonly List<(IBetterGameMenuApi.MenuCreatedDelegate Handler, EventPriority Priority, IModInfo Source)> _MenuCreated = [];
	private static readonly List<(IBetterGameMenuApi.PageCreatedDelegate Handler, EventPriority Priority, IModInfo Source)> _PageCreated = [];
	private static readonly List<(IBetterGameMenuApi.PageOverlayCreationDelegate Handler, EventPriority Priority, IModInfo Source)> _PageOverlayCreation = [];
	private static readonly List<(IBetterGameMenuApi.PageReadyToCloseDelegate Handler, EventPriority Priority, IModInfo Source)> _PageReadyToClose = [];
	private static readonly List<(IBetterGameMenuApi.TabChangedDelegate Handler, EventPriority Priority, IModInfo Source)> _TabChanged = [];
	private static readonly List<(IBetterGameMenuApi.TabContextMenuDelegate Handler, EventPriority Priority, IModInfo Source)> _TabContextMenu = [];

	private static int SortList<T>((T Handler, EventPriority Priority, IModInfo Source) first, (T Handler, EventPriority Priority, IModInfo Source) second) {
		return -first.Priority.CompareTo(second.Priority);
	}

	public void OnMenuCreated(IBetterGameMenuApi.MenuCreatedDelegate handler, EventPriority priority = EventPriority.Normal) {
		_MenuCreated.Add((handler, priority, Other));
		_MenuCreated.Sort(SortList);
	}

	public void OffMenuCreated(IBetterGameMenuApi.MenuCreatedDelegate handler) {
		_MenuCreated.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnPageCreated(IBetterGameMenuApi.PageCreatedDelegate handler, EventPriority priority = EventPriority.Normal) {
		_PageCreated.Add((handler, priority, Other));
		_PageCreated.Sort(SortList);
	}

	public void OffPageCreated(IBetterGameMenuApi.PageCreatedDelegate handler) {
		_PageCreated.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnPageOverlayCreation(IBetterGameMenuApi.PageOverlayCreationDelegate handler, EventPriority priority = EventPriority.Normal) {
		_PageOverlayCreation.Add((handler, priority, Other));
		_PageOverlayCreation.Sort(SortList);
	}

	public void OffPageOverlayCreation(IBetterGameMenuApi.PageOverlayCreationDelegate handler) {
		_PageOverlayCreation.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnPageReadyToClose(IBetterGameMenuApi.PageReadyToCloseDelegate handler, EventPriority priority = EventPriority.Normal) {
		_PageReadyToClose.Add((handler, priority, Other));
		_PageReadyToClose.Sort(SortList);
	}

	public void OffPageReadyToClose(IBetterGameMenuApi.PageReadyToCloseDelegate handler) {
		_PageReadyToClose.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnTabChanged(IBetterGameMenuApi.TabChangedDelegate handler, EventPriority priority) {
		_TabChanged.Add((handler, priority, Other));
		_TabChanged.Sort(SortList);
	}

	public void OffTabChanged(IBetterGameMenuApi.TabChangedDelegate handler) {
		_TabChanged.RemoveWhere(entry => entry.Handler == handler);
	}

	public void OnTabContextMenu(IBetterGameMenuApi.TabContextMenuDelegate handler, EventPriority priority = EventPriority.Normal) {
		_TabContextMenu.Add((handler, priority, Other));
		_TabContextMenu.Sort(SortList);
	}

	public void OffTabContextMenu(IBetterGameMenuApi.TabContextMenuDelegate handler) {
		_TabContextMenu.RemoveWhere(entry => entry.Handler == handler);
	}

	internal static void FireMenuCreated(ModEntry mod, IClickableMenu menu) {
		foreach (var (Handler, Priority, Source) in _MenuCreated) {
			try {
				Handler(menu);
			} catch (Exception ex) {
				mod.Log($"Error in OnMenuCreated handler for mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}): {ex}", LogLevel.Error);
			}
		}
	}

	internal static void FireTabChanged(ModEntry mod, BetterGameMenuImpl menu, string tab, string oldTab) {
		var evt = new TabChangedEvent(menu, tab, oldTab);
		foreach (var (Handler, Priority, Source) in _TabChanged) {
			try {
				Handler(evt);
			} catch (Exception ex) {
				mod.Log($"Error in OnTabChanged handler for mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}): {ex}", LogLevel.Error);
			}
		}
	}

	internal static void FireTabContextMenu(ModEntry mod, BetterGameMenuImpl menu, string tab, List<ITabContextMenuEntry> entries) {
		var evt = new TabContextMenuEvent(menu, tab, entries);
		foreach (var (Handler, Priority, Source) in _TabContextMenu) {
			try {
				Handler(evt);
			} catch (Exception ex) {
				mod.Log($"Error in OnTabContextMenu handler for mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}): {ex}", LogLevel.Error);
			}
		}
	}

	internal static void FirePageCreated(ModEntry mod, BetterGameMenuImpl menu, string tab, string source, IClickableMenu page, IClickableMenu? oldPage) {
		var evt = new PageCreatedEvent(menu, tab, source, page, oldPage);
		foreach (var (Handler, Priority, Source) in _PageCreated) {
			try {
				Handler(evt);
			} catch (Exception ex) {
				mod.Log($"Error in OnPageCreated handler for mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}): {ex}", LogLevel.Error);
			}
		}
	}

	internal static bool FirePageReadyToClose(ModEntry mod, BetterGameMenuImpl menu, string tab, string source, IClickableMenu page, PageReadyToCloseReason reason, bool defaultValue = true) {
		var evt = new PageReadyToCloseEvent(menu, tab, source, page, reason, defaultValue: defaultValue);
		foreach (var (Handler, Priority, Source) in _PageReadyToClose) {
			try {
				Handler(evt);
			} catch (Exception ex) {
				mod.Log($"Error in OnPageReadyToClose handler for mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}): {ex}", LogLevel.Error);
			}
		}
		return evt.ReadyToClose;
	}

	internal static List<IPageOverlay> FirePageOverlayCreation(ModEntry mod, BetterGameMenuImpl menu, string tab, string source, IClickableMenu page) {
		var evt = new PageOverlayCreationEvent(mod, menu, tab, source, page);
		foreach (var (Handler, Priority, Source) in _PageOverlayCreation) {
			try {
				evt.ModSource = Source;
				Handler(evt);
				evt.ModSource = null;
			} catch (Exception ex) {
				mod.Log($"Error in OnPageOverlayCreation handler for mod '{Source.Manifest.Name}' ({Source.Manifest.UniqueID}): {ex}", LogLevel.Error);
			}
		}

		return evt.Overlays;
	}

	public static IBetterGameMenuApi.DrawDelegate CreateDrawImpl(Texture2D texture, Rectangle source, float scale, int frames = 1, int frameTime = 16, Vector2? offset = null) {
		var inst = new DrawMethod(texture, source, scale, frames, frameTime, offset);
		return inst.Draw;
	}

	public IBetterGameMenuApi.DrawDelegate CreateDraw(Texture2D texture, Rectangle source, float scale, int frames = 1, int frameTime = 16, Vector2? offset = null) {
		return CreateDrawImpl(texture, source, scale, frames, frameTime, offset);
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

	public void UnregisterImplementation(string id) {
		Self.RemoveImplementation(id, Other.Manifest.UniqueID);
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

	public bool IsMenu(IClickableMenu menu) {
		return menu is BetterGameMenuImpl;
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

	public IClickableMenu CreateMenu(string? defaultTab = null, int extra = -1, bool playSound = false) {
		Game1.PushUIMode();
		var result = new BetterGameMenuImpl(Self, defaultTab, extra: extra, playOpeningSound: playSound);
		Game1.PopUIMode();
		return result;
	}

	public IClickableMenu CreateMenu(int startingTab, bool playSound = false) {
		Game1.PushUIMode();
		var result = Self.CreateMenuFromTabId(startingTab, playOpeningSound: playSound);
		Game1.PopUIMode();
		return result;
	}

	public IClickableMenu CreateMenu(int startingTab, int extra = -1, bool playSound = false) {
		Game1.PushUIMode();
		var result = Self.CreateMenuFromTabId(startingTab, extra: extra, playOpeningSound: playSound);
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
