#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu;


/// <summary>
/// A tab changed event is emitted whenever the currently
/// active tab of a Better Game Menu changes.
/// </summary>
public interface ITabChangedEvent {

	/// <summary>
	/// The Better Game Menu instance involved in the event. You
	/// can use <see cref="IBetterGameMenuApi.AsMenu(IClickableMenu)"/>
	/// to get a more useful interface for this menu.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// The id of the tab the Game Menu was changed to.
	/// </summary>
	string Tab { get; }

	/// <summary>
	/// The id of the previous tab the Game Menu displayed.
	/// </summary>
	string OldTab { get; }

}


/// <summary>
/// A tab context menu event is emitted whenever the user right-clicks
/// on a tab in the game menu.
/// </summary>
public interface ITabContextMenuEvent {

	/// <summary>
	/// The Better Game Menu instance involved in the event. You
	/// can use <see cref="IBetterGameMenuApi.AsMenu(IClickableMenu)"/>
	/// to get a more useful interface for this menu.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// Whether or not the tab is the currently selected tab of
	/// the game menu.
	/// </summary>
	bool IsCurrentTab { get; }

	/// <summary>
	/// The id of the tab the context menu is opening for.
	/// </summary>
	string Tab { get; }

	/// <summary>
	/// The page instance associated with the tab, if one has been
	/// created. This may be <c>null</c> if the user hasn't visited
	/// the tab yet.
	/// </summary>
	IClickableMenu? Page { get; }

	/// <summary>
	/// A list of context menu entries to display. You can freely add
	/// or remove entries. An entry with a null <c>OnSelect</c> will
	/// appear disabled.
	/// </summary>
	IList<ITabContextMenuEntry> Entries { get; }

	/// <summary>
	/// Create a new context menu entry.
	/// </summary>
	/// <param name="label">The string to display.</param>
	/// <param name="onSelect">The action to perform when this entry is selected.</param>
	/// <param name="icon">An icon to display alongside this entry.</param>
	public ITabContextMenuEntry CreateEntry(string label, Action? onSelect, IBetterGameMenuApi.DrawDelegate? icon = null);

}


/// <summary>
/// An entry in a tab context menu.
/// </summary>
public interface ITabContextMenuEntry {

	/// <summary>The string to display for this entry.</summary>
	string Label { get; }

	/// <summary>
	/// An action to perform when this entry is selected. If this is
	/// <c>null</c>, the entry will be disabled.
	/// </summary>
	Action? OnSelect { get; }

	/// <summary>An optional icon to display to the left of this entry.</summary>
	IBetterGameMenuApi.DrawDelegate? Icon { get; }

}


/// <summary>
/// A page created event is emitted whenever a new page
/// is created for a tab by Better Game Menu.
/// </summary>
public interface IPageCreatedEvent {

	/// <summary>
	/// The Better Game Menu instance involved in the event. You
	/// can use <see cref="IBetterGameMenuApi.AsMenu(IClickableMenu)"/>
	/// to get a more useful interface for this menu.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// The id of the tab the page was created for.
	/// </summary>
	string Tab { get; }

	/// <summary>
	/// The id of the provider the page was created with.
	/// </summary>
	string Source { get; }

	/// <summary>
	/// The new page that was just created.
	/// </summary>
	IClickableMenu Page { get; }

	/// <summary>
	/// If the page was previously created and is being replaced,
	/// this will be the old page instance. Otherwise, this will
	/// be <c>null</c>.
	/// </summary>
	IClickableMenu? OldPage { get; }

}


/// <summary>
/// A page ready to close event is emitted whenever Better Game Menu
/// checks if a page can be closed, such as when switching to another
/// tab or when preparing to close the menu. This can be useful for
/// overlays that have rendered their own logic over an existing
/// page implementation.
///
/// This can be used to override the result of <see cref="IClickableMenu.readyToClose"/>
/// to please be careful to not set <see cref="ReadyToClose"/> to <c>true</c>
/// if the menu isn't <em>actually ready to close</em>.
/// </summary>
public interface IPageReadyToCloseEvent {

	/// <summary>
	/// The Better Game Menu instance involved in the event. You
	/// can use <see cref="IBetterGameMenuApi.AsMenu(IClickableMenu)"/>
	/// to get a more useful interface for this menu.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// The id of the tab the Game Menu is checking can be closed.
	/// </summary>
	string Tab { get; }

	/// <summary>
	/// The id of the provider the page was created with.
	/// </summary>
	string Source { get; }

	/// <summary>
	/// The page instance associated with the tab.
	/// </summary>
	IClickableMenu Page { get; }

	/// <summary>
	/// Whether or not the menu is ready to be closed.
	/// </summary>
	bool ReadyToClose { get; set; }

	/// <summary>
	/// The reason why this event was fired.
	/// </summary>
	PageReadyToCloseReason Reason { get; }

}

/// <summary>
/// The reason why a <see cref="IPageReadyToCloseEvent"/> event is fired.
/// </summary>
public enum PageReadyToCloseReason {
	/// <summary>The reason for this event wasn't tracked.</summary>
	Unknown = 0,
	/// <summary>The Game Menu is attempting to close.</summary>
	MenuClosing = 1,
	/// <summary>The Game Menu is trying to switch to a new tab.</summary>
	TabChanging = 2,
	/// <summary>The Game Menu wants to recreate the current tab instance.</summary>
	TabReloading = 3,
	/// <summary>The user tried to open a tab context menu.</summary>
	TabContextMenu = 4,
}


/// <summary>
/// A page overlay creation event can be used by other mods to add overlays
/// to a menu page. Overlays allow you to wrap various <see cref="IClickableMenu"/>
/// events and potentially prevent them from firing.
/// </summary>
public interface IPageOverlayCreationEvent {

	/// <summary>
	/// The Better Game Menu instance involved in the event. You
	/// can use <see cref="IBetterGameMenuApi.AsMenu(IClickableMenu)"/>
	/// to get a more useful interface for this menu.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// The id of the tab the page belongs to.
	/// </summary>
	string Tab { get; }

	/// <summary>
	/// The id of the provider the page was created with.
	/// </summary>
	string Source { get; }

	/// <summary>
	/// The page to get an overlay for.
	/// </summary>
	IClickableMenu Page { get; }

	/// <summary>
	/// Add an overlay to the menu page.
	/// </summary>
	/// <param name="overlay">The overlay to add. Note that this isn't an <see cref="IPageOverlay"/>
	/// since we do special proxying for performance. All methods are optional.</param>
	/// <exception cref="InvalidCastException">Thrown if the provided instance cannot be proxied correctly.</exception>
	void AddOverlay(IDisposable overlay);

}


/// <summary>
/// A page overlay allows you to encapsulate logic that runs over top of an
/// existing menu in a simple way, rather than needing harmony patches or other
/// events. All of the methods listed here are optional and can be omitted
/// from your overlay. This interface is provided as a guide.
///
/// Overlays are designed to have short lifetimes. They're created whenever
/// a page instance is created and disposed of before the page instance is
/// closed. If a page instance is recreated, overlays are also recreated.
/// </summary>
public interface IPageOverlay : IDisposable {

	#region Life Cycle and Updates

	/// <summary>
	/// This is called whenever the overlayed page becomes the current page
	/// of the menu. This is not called when the overlay is first created.
	/// </summary>
	void OnActivate();

	/// <summary>
	/// This is called whenever the overlayed page becomes inactive, such
	/// as when the menu changes to a different page. This is not called
	/// when the overlay is about to be disposed due to the page instance
	/// being closed.
	/// </summary>
	void OnDeactivate();

	/// <summary>
	/// This is called when checking if the overlayed page is ready to close.
	/// Returning <c>false</c> prevents the page from closing.
	/// </summary>
	bool ReadyToClose(); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.update(GameTime)"/>
	/// </summary>
	/// <param name="time">The current time.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void Update(GameTime time, out bool suppressEvent); // done

	/// <summary>
	/// This is called after the overlayed page changes position or size
	/// due to the game window size changing.
	/// </summary>
	/// <param name="oldSize">The old window size.</param>
	/// <param name="newSize">The new window size.</param>
	void PageSizeChanged(Rectangle oldSize, Rectangle newSize);

	#endregion

	#region Input

	/// <summary>
	/// This is called before <see cref="IClickableMenu.receiveLeftClick(int, int, bool)"/>
	/// </summary>
	/// <param name="x">The mouse's x position.</param>
	/// <param name="y">The mouse's y position.</param>
	/// <param name="playSound">Whether or not sounds should be played.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void ReceiveLeftClick(int x, int y, bool playSound, out bool suppressEvent); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.leftClickHeld(int, int)"/>
	/// </summary>
	/// <param name="x">The mouse's x position.</param>
	/// <param name="y">The mouse's y position.</param>
	void LeftClickHeld(int x, int y); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.releaseLeftClick(int, int)"/>
	/// </summary>
	/// <param name="x">The mouse's x position</param>
	/// <param name="y">The mouse's y position</param>
	void ReleaseLeftClick(int x, int y); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.receiveRightClick(int, int, bool)"/>
	/// </summary>
	/// <param name="x">The mouse's x position.</param>
	/// <param name="y">The mouse's y position.</param>
	/// <param name="playSound">Whether or not sounds should be played.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void ReceiveRightClick(int x, int y, bool playSound, out bool suppressEvent); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.receiveScrollWheelAction(int)"/>
	/// </summary>
	/// <param name="direction">The direction the scroll was performed in.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void ReceiveScrollWheelAction(int direction, out bool suppressEvent); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.performHoverAction(int, int)"/>
	/// </summary>
	/// <param name="x">The mouse's x position.</param>
	/// <param name="y">The mouse's y position.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void PerformHoverAction(int x, int y, out bool suppressEvent); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.receiveKeyPress(Keys)"/>
	/// </summary>
	/// <param name="key">The key(s) that was pressed.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void ReceiveKeyPress(Keys key, out bool suppressEvent); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.receiveGamePadButton(Buttons)"/>
	/// </summary>
	/// <param name="button">The button(s) that was pressed.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void ReceiveGamePadButton(Buttons button, out bool suppressEvent); // done

	/// <summary>
	/// This is called before <see cref="IClickableMenu.gamePadButtonHeld(Buttons)"/>
	/// </summary>
	/// <param name="button">The button(s) that was held.</param>
	/// <param name="suppressEvent">Whether or not to allow the relevant method on <see cref="IClickableMenu"/> to run.</param>
	void GamePadButtonHeld(Buttons button, out bool suppressEvent); // done

	#endregion

	#region Drawing

	/// <summary>
	/// This is called before <see cref="IClickableMenu.draw(SpriteBatch)"/>
	/// </summary>
	/// <param name="batch">The SpriteBatch to draw with.</param>
	void PreDraw(SpriteBatch batch);

	/// <summary>
	/// This is called after <see cref="IClickableMenu.draw(SpriteBatch)"/>
	/// </summary>
	/// <param name="batch">The SpriteBatch to draw with.</param>
	void Draw(SpriteBatch batch);

	#endregion

}


/// <summary>
/// This interface represents a Better Game Menu. 
/// </summary>
public interface IBetterGameMenu {
	/// <summary>
	/// The <see cref="IClickableMenu"/> instance for this game menu. This is
	/// the same object, but with a different type. This property is included
	/// for convenience due to how API proxying works.
	/// </summary>
	IClickableMenu Menu { get; }

	/// <summary>
	/// Whether or not the menu is currently drawing itself. This is typically
	/// always <c>false</c> except when viewing the <c>Map</c> tab.
	/// </summary>
	bool Invisible { get; set; }

	/// <summary>
	/// A list of ids of the currently visible tabs.
	/// </summary>
	IReadOnlyList<string> VisibleTabs { get; }

	/// <summary>
	/// The id of the currently active tab.
	/// </summary>
	string CurrentTab { get; }

	/// <summary>
	/// The <see cref="IClickableMenu"/> instance for the currently active tab.
	/// This may be <c>null</c> if the page instance for the currently active
	/// tab is still being initialized.
	/// </summary>
	IClickableMenu? CurrentPage { get; }

	/// <summary>
	/// Whether or not the currently displayed page is an error page. Error
	/// pages are used when a tab implementation's GetPageInstance method
	/// throws an exception.
	/// </summary>
	bool CurrentTabHasErrored { get; }

	/// <summary>
	/// Try to get the source for the specific tab.
	/// </summary>
	/// <param name="target">The id of the tab to get the source of.</param>
	/// <param name="source">The unique ID of the mod that registered the
	/// implementation being used, or <c>stardew</c> if the base game's
	/// implementation is being used.</param>
	/// <returns>Whether or not the tab is registered with the system.</returns>
	bool TryGetSource(string target, [NotNullWhen(true)] out string? source);

	/// <summary>
	/// Try to get the <see cref="IClickableMenu"/> instance for a specific tab.
	/// </summary>
	/// <param name="target">The id of the tab to get the page for.</param>
	/// <param name="page">The page instance, if one exists.</param>
	/// <param name="forceCreation">If set to true, an instance will attempt to
	/// be created if one has not already been created.</param>
	/// <returns>Whether or not a page instance for that tab exists.</returns>
	bool TryGetPage(string target, [NotNullWhen(true)] out IClickableMenu? page, bool forceCreation = false);

	/// <summary>
	/// Attempt to change the currently active tab to the target tab.
	/// </summary>
	/// <param name="target">The id of the tab to change to.</param>
	/// <param name="playSound">Whether or not to play a sound.</param>
	/// <returns>Whether or not the tab was changed successfully.</returns>
	bool TryChangeTab(string target, bool playSound = true);

	/// <summary>
	/// Force the menu to recalculate the visible tabs. This will not recreate
	/// <see cref="IClickableMenu"/> instances, but can be used to cause an
	/// inactive tab to be removed, or a previously hidden tab to be added.
	/// This can also be used to update tab decorations if necessary.
	/// </summary>
	/// <param name="target">Optionally, a specific tab to update rather than
	/// updating all tabs.</param>
	void UpdateTabs(string? target = null);

}


/// <summary>
/// This enum is included for reference and has the order value for
/// all the default tabs from the base game. These values are intentionally
/// spaced out to allow for modded tabs to be inserted at specific points.
/// </summary>
public enum VanillaTabOrders {
	Inventory = 0,
	Skills = 20,
	Social = 40,
	Map = 60,
	Crafting = 80,
	Animals = 100,
	Powers = 120,
	Collections = 140,
	Options = 160,
	Exit = 200
}


public interface IBetterGameMenuApi {

	/// <summary>
	/// A delegate for drawing something onto the screen.
	/// </summary>
	/// <param name="batch">The <see cref="SpriteBatch"/> to draw with.</param>
	/// <param name="bounds">The region where the thing should be drawn.</param>
	public delegate void DrawDelegate(SpriteBatch batch, Rectangle bounds);

	#region Helpers

	/// <summary>
	/// Create a draw delegate that draws the provided texture to the
	/// screen. This supports basic animations if required.
	/// </summary>
	/// <param name="texture">The texture to draw from.</param>
	/// <param name="source">The source rectangle to draw.</param>
	/// <param name="scale">The scale to draw the source at.</param>
	/// <param name="frames">The number of frames to draw.</param>
	/// <param name="frameTime">The amount of time each frame should be displayed.</param>
	DrawDelegate CreateDraw(Texture2D texture, Rectangle source, float scale = 1f, int frames = 1, int frameTime = 16, Vector2? offset = null);

	#endregion

	#region Tab Registration

	/// <summary>
	/// Register a new tab with the system. 
	/// </summary>
	/// <param name="id">The id of the tab to add.</param>
	/// <param name="order">The order of this tab relative to other tabs.
	/// See <see cref="VanillaTabOrders"/> for an example of existing values.</param>
	/// <param name="getDisplayName">A method that returns the display name of
	/// this tab, to be displayed in a tool-tip to the user.</param>
	/// <param name="getIcon">A method that returns an icon to be displayed
	/// on the tab UI for this tab, expecting both a texture and a Rectangle.</param>
	/// <param name="priority">The priority of the default page instance
	/// provider for this tab. When multiple page instance providers are
	/// registered, and the user hasn't explicitly chosen one, then the
	/// one with the highest priority is used. Please note that a given
	/// mod can only register one provider for any given tab.</param>
	/// <param name="getPageInstance">A method that returns a page instance
	/// for the tab. This should never return a <c>null</c> value.</param>
	/// <param name="getDecoration">A method that returns a decoration for
	/// the tab UI for this tab. This can be used to, for example, add a
	/// sparkle to a tab to indicate that new content is available. The
	/// expected output is either <c>null</c> if no decoration should be
	/// displayed, or a texture, rectangle, number of animation frames
	/// to display, and delay between frame advancements. Please note that
	/// the decoration will be automatically cleared when the user navigates
	/// to the tab.</param>
	/// <param name="getTabVisible">A method that returns whether or not the
	/// tab should be visible in the menu. This is called whenever a menu is
	/// opened, as well as when <see cref="IBetterGameMenu.UpdateTabs(string?)"/>
	/// is called.</param>
	/// <param name="getMenuInvisible">A method that returns the value that the
	/// game menu should set its <see cref="IBetterGameMenu.Invisible"/> flag
	/// to when this is the active tab.</param>
	/// <param name="getWidth">A method that returns a specific width to use when
	/// rendering this tab, in case the page instance requires a different width
	/// than the standard value.</param>
	/// <param name="getHeight">A method that returns a specific height to use
	/// when rendering this tab, in case the page instance requires a different
	/// height than the standard value.</param>
	/// <param name="onResize">A method that is called when the game window is
	/// resized, in addition to the standard <see cref="IClickableMenu.gameWindowSizeChanged(Rectangle, Rectangle)"/>.
	/// This can be used to recreate a menu page if necessary by returning a
	/// new <see cref="IClickableMenu"/> instance. Several menus in the vanilla
	/// game use this logic.</param>
	/// <param name="onClose">A method that is called whenever a page instance
	/// is cleaned up. The standard Game Menu doesn't call <see cref="IClickableMenu.cleanupBeforeExit"/>
	/// of its pages, and only calls <see cref="IClickableMenu.emergencyShutDown"/>
	/// of the currently active tab, and we're keeping that behavior for
	/// compatibility. This method will always be called. This includes calling
	/// it for menus that were replaced by the <c>onResize</c> method.</param>
	void RegisterTab(
		string id,
		int order,
		Func<string> getDisplayName,
		Func<(DrawDelegate DrawMethod, bool DrawBackground)> getIcon,
		int priority,
		Func<IClickableMenu, IClickableMenu> getPageInstance,
		Func<DrawDelegate?>? getDecoration = null,
		Func<bool>? getTabVisible = null,
		Func<bool>? getMenuInvisible = null,
		Func<int, int>? getWidth = null,
		Func<int, int>? getHeight = null,
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null,
		Action<IClickableMenu>? onClose = null
	);

	/// <summary>
	/// Register a tab implementation for an existing tab. This can be used
	/// to override an existing vanilla game tab.
	/// </summary>
	/// <param name="id">The id of the tab to register an implementation for.
	/// The keys for the vanilla game tabs are the same as those in the
	/// <see cref="VanillaTabOrders"/> enum.</param>
	/// <param name="priority">The priority of this page instance provider
	/// for this tab. When multiple page instance providers are
	/// registered, and the user hasn't explicitly chosen one, then the
	/// one with the highest priority is used. Please note that a given
	/// mod can only register one provider for any given tab.</param>
	/// <param name="getPageInstance">A method that returns a page instance
	/// for the tab. This should never return a <c>null</c> value.</param>
	/// <param name="getDecoration">A method that returns a decoration for
	/// the tab UI for this tab. This can be used to, for example, add a
	/// sparkle to a tab to indicate that new content is available. The
	/// expected output is either <c>null</c> if no decoration should be
	/// displayed, or a texture, rectangle, number of animation frames
	/// to display, and delay between frame advancements. Please note that
	/// the decoration will be automatically cleared when the user navigates
	/// to the tab.</param>
	/// <param name="getTabVisible">A method that returns whether or not the
	/// tab should be visible in the menu. This is called whenever a menu is
	/// opened, as well as when <see cref="IBetterGameMenu.UpdateTabs(string?)"/>
	/// is called.</param>
	/// <param name="getMenuInvisible">A method that returns the value that the
	/// game menu should set its <see cref="IBetterGameMenu.Invisible"/> flag
	/// to when this is the active tab.</param>
	/// <param name="getWidth">A method that returns a specific width to use when
	/// rendering this tab, in case the page instance requires a different width
	/// than the standard value.</param>
	/// <param name="getHeight">A method that returns a specific height to use
	/// when rendering this tab, in case the page instance requires a different
	/// height than the standard value.</param>
	/// <param name="onResize">A method that is called when the game window is
	/// resized, in addition to the standard <see cref="IClickableMenu.gameWindowSizeChanged(Rectangle, Rectangle)"/>.
	/// This can be used to recreate a menu page if necessary by returning a
	/// new <see cref="IClickableMenu"/> instance. Several menus in the vanilla
	/// game use this logic.</param>
	/// <param name="onClose">A method that is called whenever a page instance
	/// is cleaned up. The standard Game Menu doesn't call <see cref="IClickableMenu.cleanupBeforeExit"/>
	/// of its pages, and only calls <see cref="IClickableMenu.emergencyShutDown"/>
	/// of the currently active tab, and we're keeping that behavior for
	/// compatibility. This method will always be called. This includes calling
	/// it for menus that were replaced by the <c>onResize</c> method.</param>
	void RegisterImplementation(
		string id,
		int priority,
		Func<IClickableMenu, IClickableMenu> getPageInstance,
		Func<DrawDelegate?>? getDecoration = null,
		Func<bool>? getTabVisible = null,
		Func<bool>? getMenuInvisible = null,
		Func<int, int>? getWidth = null,
		Func<int, int>? getHeight = null,
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null,
		Action<IClickableMenu>? onClose = null
	);

	/// <summary>
	/// Un-register a tab implementation. This does nothing if your mod does
	/// not already have a registered implementation for a tab. If you simply
	/// want to replace your existing implementation with a different one, you
	/// can call <c>RegisterImplementation</c> again without needing to call
	/// this method first.
	/// </summary>
	/// <param name="id">The id of the tab to un-register your implementation for.</param>
	void UnregisterImplementation(string id);

	#endregion

	#region Menu Class Access

	/// <summary>
	/// Return the Better Game Menu menu implementation's type, in case
	/// you want to do spooky stuff to it, I guess.
	/// </summary>
	Type GetMenuType();

	/// <summary>
	/// The current page of the active screen's current Better Game Menu,
	/// if one is open, else <c>null</c>. This exists as a quicker alternative
	/// to <c>ActiveMenu?.CurrentPage</c> with the additional benefit that
	/// you can prune <c>IBetterGameMenu</c> from your copy of the API
	/// file if you're not using anything else from it.
	/// </summary>
	IClickableMenu? ActivePage { get; }

	/// <summary>
	/// The active screen's current Better Game Menu, if one is open,
	/// else <c>null</c>.
	/// </summary>
	IBetterGameMenu? ActiveMenu { get; }

	/// <summary>
	/// Attempt to cast the provided menu into an <see cref="IBetterGameMenu"/>.
	/// This can be useful if you're working with a menu that isn't currently
	/// assigned to <see cref="Game1.activeClickableMenu"/>.
	/// </summary>
	/// <param name="menu">The menu to attempt to cast</param>
	IBetterGameMenu? AsMenu(IClickableMenu menu);

	/// <summary>
	/// Just check to see if the provided menu is a Better Game Menu,
	/// without actually casting it. This can be useful if you want to remove
	/// the menu interface from the API surface you use.
	/// </summary>
	/// <param name="menu">The menu to check</param>
	bool IsMenu(IClickableMenu menu);

	/// <summary>
	/// Get the current page of the provided Better Game Menu instance. If the
	/// provided menu is not a Better Game Menu, or a page is not ready, then
	/// return <c>null</c> instead.
	/// </summary>
	/// <param name="menu">The menu to get the page from.</param>
	IClickableMenu? GetCurrentPage(IClickableMenu menu);

	/// <summary>
	/// Attempt to open a Better Game Menu. This will only work if a game menu can
	/// be opened for the active screen.
	/// </summary>
	/// <param name="defaultTab">The tab that the menu should be opened to.</param>
	/// <param name="playSound">Whether or not a sound should play when the menu is opened.</param>
	/// <param name="closeExistingMenu">If true, attempt to close the <see cref="Game1.activeClickableMenu"/>
	/// if one is set to make room for the game menu.</param>
	/// <returns>The menu that was opened, if one was.</returns>
	IBetterGameMenu? TryOpenMenu(
		string? defaultTab = null,
		bool playSound = false,
		bool closeExistingMenu = false
	);

	/// <summary>
	/// Create a new Better Game Menu instance and return it.
	/// </summary>
	/// <param name="defaultTab">The tab that the menu should be opened to.</param>
	/// <param name="playSound">Whether or not a sound should play.</param>
	IClickableMenu CreateMenu(
		string? defaultTab = null,
		bool playSound = false
	);

	/// <summary>
	/// Create a new Better Game Menu instance and return it.
	/// </summary>
	/// <param name="startingTab">The tab that the menu should be opened to.
	/// This accepts the default tab IDs from <see cref="GameMenu"/>. If you
	/// want to open to a custom tab, you should use the method that takes
	/// a string key instead.</param>
	/// <param name="playSound">Whether or not a sound should play.</param>
	IClickableMenu CreateMenu(
		int startingTab,
		bool playSound = false
	);

	#endregion

	#region Menu Events

	public delegate void MenuCreatedDelegate(IClickableMenu menu);
	public delegate void TabChangedDelegate(ITabChangedEvent evt);
	public delegate void TabContextMenuDelegate(ITabContextMenuEvent evt);
	public delegate void PageCreatedDelegate(IPageCreatedEvent evt);
	public delegate void PageOverlayCreationDelegate(IPageOverlayCreationEvent evt);
	public delegate void PageReadyToCloseDelegate(IPageReadyToCloseEvent evt);

	/// <summary>
	/// This event fires whenever the game menu is created, at the end of
	/// the menu's constructor. As such, this is called before the new <see cref="IBetterGameMenu"/>
	/// instance is assigned to <see cref="Game1.activeClickableMenu"/>.
	/// </summary>
	void OnMenuCreated(MenuCreatedDelegate handler, EventPriority priority = EventPriority.Normal);

	/// <summary>
	/// Unregister a handler for the MenuCreated event.
	/// </summary>
	void OffMenuCreated(MenuCreatedDelegate handler);

	/// <summary>
	/// This event fires whenever the current tab changes, except when a
	/// game menu is first created.
	/// </summary>
	void OnTabChanged(TabChangedDelegate handler, EventPriority priority = EventPriority.Normal);

	/// <summary>
	/// Unregister a handler for the TabChanged event.
	/// </summary>
	void OffTabChanged(TabChangedDelegate handler);

	/// <summary>
	/// This event fires whenever the user opens a context menu for a menu tab
	/// by right-clicking on it.
	/// </summary>
	void OnTabContextMenu(TabContextMenuDelegate handler, EventPriority priority = EventPriority.Normal);

	/// <summary>
	/// Unregister a handler for the TabContextMenu event.
	/// </summary>
	void OffTabContextMenu(TabContextMenuDelegate handler);

	/// <summary>
	/// This event fires whenever a new page instance is created. This can happen
	/// the first time a page is accessed, whenever something calls
	/// <see cref="TryGetPage(string, out IClickableMenu?, bool)"/> with the
	/// <c>forceCreation</c> flag set to true, or when the menu has been resized
	/// and the tab implementation's <c>OnResize</c> method returned a new
	/// page instance.
	/// </summary>
	void OnPageCreated(PageCreatedDelegate handler, EventPriority priority = EventPriority.Normal);

	/// <summary>
	/// Unregister a handler for the PageCreated event.
	/// </summary>
	void OffPageCreated(PageCreatedDelegate handler);

	/// <summary>
	/// This event fires whenever page overlays are being created for a page. This
	/// happens the first time a page is ready to be displayed because it becomes
	/// the current page, or when the current tab's page is recreated.
	/// </summary>
	void OnPageOverlayCreation(PageOverlayCreationDelegate handler, EventPriority priority = EventPriority.Normal);

	/// <summary>
	/// Unregister a handler for the PageCreated event.
	/// </summary>
	void OffPageOverlayCreation(PageOverlayCreationDelegate handler);

	/// <summary>
	/// This event fires whenever Better Game Menu checks if a page is ready to
	/// close, either due to the game menu closing or due to the tab being changed.
	/// </summary>
	void OnPageReadyToClose(PageReadyToCloseDelegate handler, EventPriority priority = EventPriority.Normal);

	/// <summary>
	/// Unregister a handler for the PageReadyToClose event.
	/// </summary>
	void OffPageReadyToClose(PageReadyToCloseDelegate handler);

	#endregion

}
