using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Leclair.Stardew.BetterGameMenu.Models;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.SimpleLayout;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Menus;

public sealed class BetterGameMenuImpl : IClickableMenu, IBetterGameMenu, IDisposable {

	private readonly ModEntry Mod;

	private Rectangle CurrentScreenSize;

	private readonly Dictionary<string, ClickableComponent> TabComponents = [];
	private readonly Dictionary<string, IClickableMenu> TabPages = [];
	private readonly Dictionary<string, Rectangle> TabLastSize = [];
	private readonly Dictionary<string, (TabDefinition Tab, TabImplementationDefinition Implementation)> TabSources = [];
	private readonly Dictionary<string, (IBetterGameMenuApi.DrawDelegate DrawMethod, bool DrawBackground)> TabDrawing = [];
	private readonly Dictionary<string, IBetterGameMenuApi.DrawDelegate> TabDecorations = [];

	private readonly List<string> Tabs = [];

	private int mCurrent = -1;

	public readonly List<ClickableComponent> TabComponentList = [];
	private bool mInvisible = false;

	private int mNextId = 12340;

	private string? mLastTab;
	public string? LastTab => mLastTab;

	private ISimpleNode? Tooltip;
	private string? LastTooltip;
	private Stopwatch? LastDraw;

	public BetterGameMenuImpl(ModEntry mod, string? startingTab = null, int extra = -1, bool playOpeningSound = false)
		: base(0, 0, 0, 0, false) {

		Mod = mod;

		CurrentScreenSize = new(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);

		// First, load all the tab definitions.
		// TODO: Cache this, possibly?
		foreach (var (Key, Tab, Implementation) in mod.GetTabImplementations())
			TabSources[Key] = (Tab, Implementation);

		// Call UpdateTabs() to determine which tabs are visible and
		// to update the tab components.
		UpdateTabs();

		// Figure out which tab should be focused.
		if (startingTab is null || !Tabs.Contains(startingTab))
			startingTab = Tabs.First();

		// Now, change to the starting tab.
		TryChangeTabImpl(startingTab, performSnap: false);

		// Now, duplicate all the logic of the GameMenu as it finishes initializing.
		if (Game1.activeClickableMenu == null && playOpeningSound)
			Game1.playSound("bigSelect");

		GameMenu.forcePreventClose = false;

		// This is required for InventoryMenu instances (inventory, crafting menus)
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").refreshBundlesIngredientsInfo();

		if (extra != -1 && CurrentPage is OptionsPage op)
			op.currentItemIndex = extra;

		if (Game1.options.SnappyMenus)
			snapToDefaultClickableComponent();

		// Finally, fire off an event because our menu was instantiated.
		Mod.FireMenuInstantiated(this);
	}

	public void Dispose() {
		foreach (var (id, page) in TabPages) {
			if (DisposePage(id, page))
				TabPages.Remove(id);
		}
	}

	private bool DisposePage(string tab, IClickableMenu page) {
		if (page is ErrorMenu ||
			page is null ||
			!TabSources.TryGetValue(tab, out var sources)
		)
			return false;

		bool didSomething = false;

		if (sources.Implementation.OnClose != null) {
			try {
				sources.Implementation.OnClose(page);
			} catch (Exception ex) {
				Mod.Log($"Error calling OnClose for tab '{tab}' using provider '{sources.Implementation.Source}': {ex}", LogLevel.Error);
			}
			didSomething = true;
		}

		if (page is IDisposable disposable && !page.HasDependencies()) {
			disposable.Dispose();
			didSomething = true;
		}

		return didSomething;
	}


	public void AddTabsToClickableComponents(IClickableMenu menu) {
		menu.allClickableComponents.AddRange(TabComponentList);
	}

	public void RemoveTabsFromClickableComponents(IClickableMenu menu) {
		if (menu?.allClickableComponents is not null)
			menu.allClickableComponents.RemoveWhere(TabComponentList.Contains);
	}

	#region Game Menu Event Forwarding

	public override bool readyToClose() {
		return !GameMenu.forcePreventClose && (CurrentPage is null || CurrentPage.readyToClose());
	}

	public override void emergencyShutDown() {
		base.emergencyShutDown();
		CurrentPage?.emergencyShutDown();
	}

	protected override void cleanupBeforeExit() {
		base.cleanupBeforeExit();
		if (Game1.options.optionsDirty)
			Game1.options.SaveDefaultOptions();
	}

	public override void automaticSnapBehavior(int direction, int oldRegion, int oldID) {
		if (CurrentPage is not null)
			CurrentPage.automaticSnapBehavior(direction, oldRegion, oldID);
		else
			base.automaticSnapBehavior(direction, oldRegion, oldID);
	}

	public override void snapToDefaultClickableComponent() {
		CurrentPage?.snapToDefaultClickableComponent();
	}

	public override ClickableComponent? getCurrentlySnappedComponent() {
		return CurrentPage?.getCurrentlySnappedComponent();
	}

	public override void setCurrentlySnappedComponentTo(int id) {
		CurrentPage?.setCurrentlySnappedComponentTo(id);
	}

	public override void setUpForGamePadMode() {
		base.setUpForGamePadMode();
		CurrentPage?.setUpForGamePadMode();
	}

	public override void receiveGamePadButton(Buttons button) {
		base.receiveGamePadButton(button);
		if (button == Buttons.LeftTrigger) {
			int target = mCurrent - 1;
			if (target < 0)
				target = Tabs.Count - 1;

			TryChangeTab(Tabs[target], playSound: true);

		} else if (button == Buttons.RightTrigger) {
			int target = mCurrent + 1;
			if (target >= Tabs.Count)
				target = 0;

			TryChangeTab(Tabs[target], playSound: true);

		} else
			CurrentPage?.receiveGamePadButton(button);
	}

	public override bool areGamePadControlsImplemented() {
		return false;
	}

	public override void receiveKeyPress(Keys key) {
		if (Game1.options.menuButton.Contains(new InputButton(key)) && readyToClose()) {
			Game1.exitActiveMenu();
			Game1.playSound("bigDeSelect");

		} else
			CurrentPage?.receiveKeyPress(key);
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true) {
		if (CurrentPage is not CollectionsPage cp || cp.letterviewerSubMenu is null)
			base.receiveLeftClick(x, y, playSound);

		if (!mInvisible && !GameMenu.forcePreventClose) {
			foreach (var cmp in TabComponentList) {
				if (cmp.containsPoint(x, y)) {
					TryChangeTab(cmp.name, playSound: true);
					return;
				}
			}
		}

		CurrentPage?.receiveLeftClick(x, y, playSound);
	}

	public override void releaseLeftClick(int x, int y) {
		base.releaseLeftClick(x, y);
		CurrentPage?.releaseLeftClick(x, y);
	}

	public override void leftClickHeld(int x, int y) {
		base.leftClickHeld(x, y);
		CurrentPage?.leftClickHeld(x, y);
	}

	public override void receiveRightClick(int x, int y, bool playSound = true) {
		if (!mInvisible && !GameMenu.forcePreventClose) {
			foreach (var cmp in TabComponentList) {
				if (cmp.containsPoint(x, y)) {
					OpenContextMenu(cmp.name, playSound);
					return;
				}
			}
		}

		CurrentPage?.receiveRightClick(x, y, playSound);
	}

	public override void receiveScrollWheelAction(int direction) {
		base.receiveScrollWheelAction(direction);
		CurrentPage?.receiveScrollWheelAction(direction);
	}

	public override void performHoverAction(int x, int y) {
		base.performHoverAction(x, y);
		CurrentPage?.performHoverAction(x, y);

		string? tt = null;

		if (!Invisible) {
			foreach (var cmp in TabComponentList) {
				if (cmp.containsPoint(x, y)) {
					tt = cmp.name;
					break;
				}
			}
		}

		if (LastTooltip != tt) {
			LastTooltip = tt;
			if (tt != null && TabSources.TryGetValue(tt, out var sources)) {
				var builder = SimpleHelper.Builder()
					.Text(sources.Tab.GetDisplayName());

				if (Mod.Config.DeveloperMode) {
					var page = TabPages.GetValueOrDefault(tt);

					builder = builder
						.Divider()
						.Group(16)
							.Text("key", Game1.textColor * 0.75f, shadow: false)
							.Text(tt)
						.EndGroup()
						.Group(16)
							.Text("provider", Game1.textColor * 0.75f, shadow: false)
							.Text(sources.Implementation.Source)
						.EndGroup()
						.Group(16)
							.Text("class", Game1.textColor * 0.75f, shadow: false);
					if (page is null)
						builder = builder.Text("null", Game1.textColor * 0.75f, shadow: false);
					else
						builder = builder.Text(page.GetType().FullName);

					builder = builder
						.EndGroup();
				}

				Tooltip = builder.GetLayout();
			} else
				Tooltip = null;
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds) {
		//base.gameWindowSizeChanged(oldBounds, newBounds);
		CurrentScreenSize = newBounds;
		ResizeMenu(CurrentTabHasErrored ? null : CurrentTab);
		TryGetPage(CurrentTab, out _);
	}

	#endregion

	#region Context Menu

	public void OpenContextMenu(string target, bool playSound = false) {
		if (CurrentPage is not null && !CurrentPage.readyToClose())
			return;

		List<ContextMenuItem> options = [];

		if (Mod.Config.DeveloperMode)
			options.Add(new("Reload Tab", () => TryReloadPage(target)));

		if (Mod.Config.AllowHotSwap &&
			TabSources.TryGetValue(target, out var sources) &&
			Mod.Implementations.TryGetValue(target, out var impls) &&
			impls.Count > 1
		) {
			if (Mod.Config.DeveloperMode)
				options.Add(new("-", null));

			foreach (var (key, impl) in impls) {
				bool active = sources.Implementation.Source == impl.Source;

				string label;
				if (impl.Source == "stardew")
					label = I18n.Config_Provider_Stardew();
				else if (Mod.Helper.ModRegistry.Get(impl.Source) is IModInfo info)
					label = I18n.Config_Provider_Mod(info.Manifest.Name);
				else
					label = I18n.Config_Provider_Unknown(key);

				IBetterGameMenuApi.DrawDelegate? icon = null;
				if (active)
					icon = ModAPI.CreateDrawImpl(Game1.mouseCursors, new Rectangle(236, 425, 9, 9), 2f);
				else
					icon = ModAPI.CreateDrawImpl(Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 2f);

				options.Add(new(label, active ? null : () => TryReloadPage(target, provider: key), icon));
			}
		}

		if (options.Count == 0)
			return;

		var pos = Game1.getMousePosition(true);

		var menu = new TabContextMenu(Mod, pos.X - 16, pos.Y - 16, options) {
			exitFunction = () => {
				if (Game1.options.SnappyMenus)
					snapToDefaultClickableComponent();
			}
		};

		SetChildMenu(menu);
		performHoverAction(0, 0);

	}

	#endregion

	public override void update(GameTime time) {
		base.update(time);

		if (CurrentTab != null && CurrentPage is null)
			TryReloadPage(CurrentTab);

		CurrentPage?.update(time);
	}

	public override void draw(SpriteBatch batch) {
		var page = CurrentPage;

		if (!mInvisible) {
			if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
				batch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

			// Draw Background
			//Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, page?.width ?? width, page?.height ?? height, speaker: false, drawOnlyBox: true);

			batch.End();
			batch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);

			// Draw Tabs
			foreach (var cmp in TabComponentList) {
				if (!TabDrawing.TryGetValue(cmp.name, out var stuff))
					continue;

				TabDecorations.TryGetValue(cmp.name, out var decoration);

				bool isCurrent = CurrentTab == cmp.name;
				var bounds = isCurrent
					? new Rectangle(cmp.bounds.X, cmp.bounds.Y + 8, cmp.bounds.Width, cmp.bounds.Height)
					: cmp.bounds;

				if (stuff.DrawBackground)
					batch.Draw(
						Game1.mouseCursors,
						position: new Vector2(
							bounds.X,
							bounds.Y
						),
						sourceRectangle: new Rectangle(16, 368, 16, 16),
						color: Color.White,
						rotation: 0f,
						origin: Vector2.Zero,
						scale: 4f,
						effects: SpriteEffects.None,
						layerDepth: 0.0001f
					);

				stuff.DrawMethod(batch, bounds);

				if (decoration is not null)
					decoration(batch, bounds);
			}

			batch.End();
			batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}

		LastDraw = Mod.Config.DeveloperMode ? Stopwatch.StartNew() : null;
		page?.draw(batch);
		LastDraw?.Stop();

		if (Tooltip is not null) {
			var tt = Tooltip;
			if (LastDraw is not null && LastTooltip == CurrentTab)
				tt = SimpleHelper.Builder()
					.Add(tt)
					.Group(12)
						.Text("draw ticks", Game1.textColor * 0.75f, shadow: false)
						.Text(LastDraw.ElapsedTicks.ToString())
					.EndGroup()
					.GetLayout();

			tt.DrawHover(batch, Game1.smallFont);
		}

		if (!GameMenu.forcePreventClose && (page?.shouldDrawCloseButton() ?? true))
			base.draw(batch);

		if ((!Game1.options.SnappyMenus || (page as CollectionsPage)?.letterviewerSubMenu == null) && !Game1.options.hardwareCursor)
			drawMouse(batch, ignore_transparency: true);
	}

	internal void ResizeMenu(string? tab) {

		int newWidth = 800 + IClickableMenu.borderWidth * 2;
		int newHeight = 600 + IClickableMenu.borderWidth * 2;

		int minWidth = newWidth + 192;
		int minHeight = newHeight;

		if (tab != null && TabSources.TryGetValue(tab, out var source)) {
			newWidth = source.Implementation.GetWidth?.Invoke(newWidth) ?? newWidth;
			newHeight = source.Implementation.GetHeight?.Invoke(newHeight) ?? newHeight;
		}

		// Check that we have enough width for our buttons.
		int realMin = 64 + 64 + (Math.Min(10, TabComponentList.Count) * 64);
		if (newWidth < realMin)
			newWidth = realMin;

		int newX = Game1.uiViewport.Width / 2 - Math.Max(minWidth, newWidth) / 2;
		int newY = Game1.uiViewport.Height / 2 - Math.Max(minHeight, newHeight) / 2;

		if (newWidth == width && newHeight == height && newX == xPositionOnScreen && newY == yPositionOnScreen)
			return;

		width = newWidth;
		height = newHeight;

		xPositionOnScreen = newX;
		yPositionOnScreen = newY;

		// Move all our components.
		RepositionTabs();
		initializeUpperRightCloseButton();

		if (Game1.options.SnappyMenus)
			snapCursorToCurrentSnappedComponent();
	}

	internal void RepositionTabs() {
		int x = xPositionOnScreen + 48;
		int y = yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64;

		int i = 0;

		foreach (var cmp in TabComponentList) {
			cmp.bounds = new Rectangle(x, y, 64, 64);
			x += 64;
			i++;
			if (i > 10) {
				x = xPositionOnScreen + 48 + 32;
				y -= 48;
				i = 0;
			}
		}
	}

	#region IBetterGameMenu

	public IClickableMenu Menu => this;

	public bool Invisible {
		get => mInvisible;
		set {
			mInvisible = value;
			// TODO: Any logic for when the menu changes to invisible?
		}
	}

	public IReadOnlyList<string> VisibleTabs => Tabs.AsReadOnly();

	public string CurrentTab => mCurrent >= 0 && mCurrent < Tabs.Count ? Tabs[mCurrent] : string.Empty;

	// TODO: Change this to a field backed property for performance?
	public IClickableMenu? CurrentPage => TabPages.GetValueOrDefault(CurrentTab);

	public bool CurrentTabHasErrored => CurrentPage is ErrorMenu;

	public bool TryChangeTab(string target, bool playSound = true) {
		return TryChangeTabImpl(target, playSound: playSound);
	}

	internal bool TryChangeTabImpl(string target, bool performSnap = true, bool playSound = true) {
		int tIndex = Tabs.IndexOf(target);
		if (tIndex == -1)
			return false;
		if (tIndex == mCurrent)
			return true;

		string oldTab = CurrentTab;
		var oldPage = CurrentPage;

		if (oldPage is not null) {
			if (!oldPage.readyToClose())
				return false;

			RemoveTabsFromClickableComponents(oldPage);
		}

		// First, check if the page we're switching to is an error.
		// Since we don't resize to the tab's spec if it's an error.
		bool isError = TabPages.TryGetValue(target, out var page) && page is ErrorMenu;

		ResizeMenu(isError ? null : target);

		// Now, actually get the page.
		if (!TryGetPage(target, out page, forceCreation: true))
			return false;

		// If we couldn't get the actual page, then resize again
		// to fit the error message.
		if (!isError && page is ErrorMenu) {
			isError = true;
			ResizeMenu(null);
		}

		// Should always be true, but we're playing things safe.
		if (TabSources.TryGetValue(target, out var sources)) {
			try {
				Invisible = !isError && (sources.Implementation.GetMenuInvisible?.Invoke() ?? false);
			} catch (Exception ex) {
				Mod.Log($"Error calling GetMenuInvisible for tab '{target}' using provider '{sources.Implementation.Source}': {ex}", LogLevel.Error);
				Invisible = false;
			}
		} else
			Invisible = false;

		// Inject our tab components.
		page.populateClickableComponentList();
		AddTabsToClickableComponents(page);

		// Clear any existing decoration.
		TabDecorations.Remove(target);

		// Update the current tab and fire the event.
		mCurrent = tIndex;
		mLastTab = string.IsNullOrEmpty(oldTab) ? null : oldTab;
		FireTabChanged(target, oldTab);

		if (playSound)
			Game1.playSound("smallSelect");

		// And move the cursor last.
		if (performSnap && Game1.options.SnappyMenus)
			snapToDefaultClickableComponent();

		return true;
	}

	internal void TryReloadPage(string target, string? provider = null) {
		if (!TabSources.TryGetValue(target, out var sources))
			return;

		if (TabPages.TryGetValue(target, out var page)) {
			if (!page.readyToClose())
				return;

			// On Close
			if (page is not ErrorMenu)
				DisposePage(target, page);
		}

		// Clear state.
		TabLastSize.Remove(target);
		TabPages.Remove(target);
		TabDecorations.Remove(target);

		// Set the new provider.
		if (!string.IsNullOrEmpty(provider) &&
			sources.Implementation.Source != provider &&
			Mod.Implementations.TryGetValue(target, out var impls) &&
			impls.TryGetValue(provider, out var impl)
		)
			TabSources[target] = sources = (sources.Tab, impl);

		// If this isn't the current page, just handle the decoration and leave.
		if (CurrentTab != target) {
			var deco = sources.Implementation.GetDecoration?.Invoke();
			if (deco != null)
				TabDecorations[target] = deco;

			return;
		}

		// We got here, so it IS the current page. Gotta do some stuff.
		// TODO: Refactor stuff so this doesn't need so much duplicate logic?

		// Now do the change tab logic.
		ResizeMenu(target);

		if (!TryGetPage(target, out page, forceCreation: true))
			return;

		bool isError = page is ErrorMenu;
		if (isError)
			ResizeMenu(null);

		try {
			Invisible = !isError && (sources.Implementation.GetMenuInvisible?.Invoke() ?? false);
		} catch (Exception ex) {
			Mod.Log($"Error calling GetMenuInvisible for tab '{target}' using provider '{sources.Implementation.Source}': {ex}", LogLevel.Error);
			Invisible = false;
		}

		// Inject our tab components.
		page.populateClickableComponentList();
		AddTabsToClickableComponents(page);

		if (Game1.options.SnappyMenus)
			snapToDefaultClickableComponent();
	}

	public bool TryGetPage(string target, [NotNullWhen(true)] out IClickableMenu? page, bool forceCreation = false) {
		if (!TabSources.TryGetValue(target, out var sources)) {
			page = null;
			return false;
		}

		Stopwatch? timer;

		if (TabPages.TryGetValue(target, out page)) {
			// See what we need to do regarding screen size.
			var oldSize = TabLastSize.GetValueOrDefault(target);
			if (oldSize != CurrentScreenSize) {
				TabLastSize[target] = CurrentScreenSize;
				if (page is not ErrorMenu && sources.Implementation.OnResize != null) {
					IClickableMenu? oldPage = page;
					timer = Mod.Config.DeveloperMode ? Stopwatch.StartNew() : null;

					try {
						page = sources.Implementation.OnResize((this, oldPage));
					} catch (Exception ex) {
						Mod.Log($"Error calling OnResize for tab '{target}' using provider '{sources.Implementation.Source}': {ex}", LogLevel.Error);
					}

					if (timer is not null) {
						timer.Stop();
						Mod.Log($"[Timing] OnResize for tab '{target}' using '{sources.Implementation.Source} took {timer.ElapsedTicks} ticks.", LogLevel.Debug);
					}

					if (page is null || page == oldPage) {
						page = oldPage;
						oldPage = null;
					}

					if (oldPage != null) {
						Mod.Log($"Created new instance for tab '{target}' due to resize using provider '{sources.Implementation.Source}'", LogLevel.Trace);
						TabPages[target] = page;
						FirePageInstantiated(target, sources.Implementation.Source, page, oldPage);

						// Remove our components from the old page, just in case.
						RemoveTabsFromClickableComponents(oldPage);

						// Dispose of the old page.
						DisposePage(target, oldPage);

						// Inject our tab components.
						page.populateClickableComponentList();
						AddTabsToClickableComponents(page);

						// Clear the tooltip, just in case.
						LastTooltip = null;
						Tooltip = null;
					}

				} else {
					page.gameWindowSizeChanged(oldSize, CurrentScreenSize);
				}
			}

			return true;
		}

		if (!forceCreation)
			return false;

		timer = Mod.Config.DeveloperMode ? Stopwatch.StartNew() : null;

		try {
			page = sources.Implementation.GetPageInstance(this);
			if (page is null)
				throw new ArgumentNullException("GetPageInstance did not return an IClickableMenu instance");

		} catch (Exception ex) {
			Mod.Log($"Error creating page instance for tab '{target}' using provider '{sources.Implementation.Source}': {ex}", LogLevel.Error);
			bool hasVanilla = sources.Implementation.Source != "stardew" && Mod.GetTabImplementation(target, "stardew") is not null;
			page = new ErrorMenu(
				Mod,
				this,
				I18n.ErrorPage_Message(),
				hasVanilla,
				xPositionOnScreen,
				yPositionOnScreen,
				width,
				height
			);
		}

		if (timer is not null) {
			timer.Stop();
			Mod.Log($"[Timing] GetPageInstance for tab '{target}' using '{sources.Implementation.Source} took {timer.ElapsedTicks} ticks.", LogLevel.Debug);
		}

		Mod.Log($"Created new instance for tab '{target}' using provider '{sources.Implementation.Source}'", LogLevel.Trace);
		TabLastSize[target] = CurrentScreenSize;
		TabPages[target] = page;

		// Clear the tooltip, just in case.
		LastTooltip = null;
		Tooltip = null;

		FirePageInstantiated(target, sources.Implementation.Source, page, null);

		return true;
	}

	public void UpdateTabs(string? target = null) {
		// Remove old tab components, just in case.
		if (CurrentPage is not null)
			RemoveTabsFromClickableComponents(CurrentPage);

		// First, store the current tab so we can restore it later.
		string currentTab = CurrentTab;

		Tabs.Clear();
		TabComponentList.Clear();

		foreach (var (id, source) in TabSources) {
			// The current tab is *always* visible.
			if (currentTab == id || (source.Implementation.GetTabVisible?.Invoke() ?? true)) {
				Tabs.Add(id);

				// Refresh the icon.
				TabDrawing[id] = source.Tab.GetIcon();

				// Check for a decoration
				var deco = currentTab == id ? null : source.Implementation.GetDecoration?.Invoke();
				if (deco != null) {
					TabDecorations[id] = deco;
				} else
					TabDecorations.Remove(id);

				if (!TabComponents.TryGetValue(id, out var component)) {
					component = new ClickableComponent(Rectangle.Empty, id) {
						myID = mNextId++,
						leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
						rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
						downNeighborID = ClickableComponent.SNAP_AUTOMATIC
					};
					TabComponents[id] = component;
				}

				TabComponentList.Add(component);
			}
		}

		// Restore the current tab.
		int idx = string.IsNullOrEmpty(currentTab) ? -1 : Tabs.IndexOf(currentTab);
		mCurrent = idx;

		RepositionTabs();

		// Finally, update our components.
		if (CurrentPage is not null) {
			CurrentPage.populateClickableComponentList();
			AddTabsToClickableComponents(CurrentPage);
		}
	}

	internal void FireTabChanged(string tab, string? oldTab) {
		if (string.IsNullOrEmpty(oldTab))
			return;

		Mod.FireTabChanged(this, tab, oldTab);
	}

	internal void FirePageInstantiated(string tab, string source, IClickableMenu page, IClickableMenu? oldPage) {
		if (page is ErrorMenu || page is null)
			return;

		Mod.FirePageInstantiated(this, tab, source, page, oldPage);
	}

	public bool TryGetSource(string target, [NotNullWhen(true)] out string? source) {
		if (TabSources.TryGetValue(target, out var impl)) {
			source = impl.Implementation.Source;
			return true;
		}

		source = null;
		return false;
	}

	#endregion

}
