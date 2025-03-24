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
	private readonly Dictionary<string, List<IPageOverlay>?> TabOverlays = [];
	private readonly Dictionary<string, Rectangle> TabLastSize = [];
	private readonly Dictionary<string, (TabDefinition Tab, TabImplementationDefinition Implementation)> TabSources;
	private readonly Dictionary<string, (IBetterGameMenuApi.DrawDelegate DrawMethod, bool DrawBackground)> TabDrawing = [];
	private readonly Dictionary<string, IBetterGameMenuApi.DrawDelegate> TabDecorations = [];

	private readonly List<string> Tabs = [];

	private int mCurrent = -1;

	// Tabs
	public ClickableTextureComponent? btnTabsPrev;
	public ClickableTextureComponent? btnTabsNext;
	private int TabScroll = 0;
	private int VisibleTabComponents = 1;

	private readonly List<ClickableComponent> TabComponentList = [];
	public readonly List<ClickableComponent> FirstTabRow = [];
	public readonly List<ClickableComponent> SecondTabRow = [];

	private bool mInvisible = false;

	private Action? PendingContextAction;

	private int mNextId = 12340;

	private string? mLastTab;
	public string? LastTab => mLastTab;

	private ISimpleNode? Tooltip;
	private string? LastTooltip;

	//private readonly PerformanceTracker? EntireDraw;
	private PerformanceTracker? HoverTimer;
	private PerformanceTracker? UpdateTimer;
	private PerformanceTracker? DrawTimer;

	public BetterGameMenuImpl(ModEntry mod, string? startingTab = null, int extra = -1, bool playOpeningSound = false)
		: base(0, 0, 0, 0, false) {

		Mod = mod;

		CurrentScreenSize = new(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);

		// First, load all the tab definitions.
		TabSources = new(mod.GetTabImplementations());

		// Call UpdateTabs() to determine which tabs are visible and
		// to update the tab components.
		UpdateTabs();

		// Figure out which tab should be focused.
		if (startingTab is null || !Tabs.Contains(startingTab))
			startingTab = Tabs.First();

		// Now, change to the starting tab.
		TryChangeTabImpl(startingTab, performSnap: false, playSound: false);
		CenterTab();

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

		//if (Mod.Config.DeveloperMode)
		//	EntireDraw = new();

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

		if (TabOverlays.TryGetValue(tab, out var overlays) && overlays is not null && overlays.Count > 0) {
			foreach (var overlay in overlays)
				overlay.Dispose();
			TabOverlays.Remove(tab);
			didSomething = true;
		}

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


	public void AddTabsToClickableComponents(IClickableMenu menu, string? target = null) {
		if (btnTabsPrev is not null)
			menu.allClickableComponents.Add(btnTabsPrev);
		menu.allClickableComponents.AddRange(FirstTabRow);
		menu.allClickableComponents.AddRange(SecondTabRow);
		if (btnTabsNext is not null)
			menu.allClickableComponents.Add(btnTabsNext);

		if (target is null && CurrentPage == menu)
			target = CurrentTab;

		if (target is not null && TabOverlays.TryGetValue(target, out var overlays) && overlays is not null) {
			foreach (var overlay in overlays)
				overlay.PopulateClickableComponents();
		}
	}

	public void RemoveTabsFromClickableComponents(IClickableMenu menu) {
		if (menu?.allClickableComponents is not null) {
			menu.allClickableComponents.RemoveWhere(x => FirstTabRow.Contains(x) || SecondTabRow.Contains(x));
			if (btnTabsPrev is not null)
				menu.allClickableComponents.Remove(btnTabsPrev);
			if (btnTabsNext is not null)
				menu.allClickableComponents.Remove(btnTabsNext);
		}
	}

	#region Game Menu Event Forwarding

	/// <summary>
	/// Check to see if anything about the page is stopping us from being ready
	/// to close the menu.
	/// </summary>
	/// <param name="reason">The reason we're trying to close the menu.</param>
	/// <param name="target">The target tab we're checking.</param>
	/// <param name="wasOverlay">If this is set to true, we were stopped from being ready by an overlay.</param>
	/// <returns>Whether or not we're ready to close.</returns>
	private bool IsPageReadyToClose(PageReadyToCloseReason reason, string? target, out bool wasOverlay) {
		IClickableMenu? page;
		wasOverlay = false;
		if (target is null) {
			target = CurrentTab;
			page = CurrentPage;
		} else {
			page = TabPages.GetValueOrDefault(target);
		}

		if (page is null)
			return true;

		bool result = page.readyToClose();
		if (TabSources.TryGetValue(target, out var impl))
			result = Mod.FirePageReadyToClose(this, target, impl.Implementation.Source, page, reason, result);

		if (result && TabOverlays.TryGetValue(target, out var overlays) && overlays is not null)
			foreach (var overlay in overlays) {
				result = overlay.ReadyToClose();
				if (!result) {
					wasOverlay = true;
					break;
				}
			}

		return result;
	}

	private bool readyToClose(out bool wasOverlay) {
		if (GameMenu.forcePreventClose) {
			wasOverlay = false;
			return false;
		}

		return IsPageReadyToClose(PageReadyToCloseReason.MenuClosing, null, out wasOverlay);
	}

	public override bool readyToClose() {
		return !GameMenu.forcePreventClose && IsPageReadyToClose(PageReadyToCloseReason.MenuClosing, null, wasOverlay: out _);
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

	public override void gamePadButtonHeld(Buttons button) {
		base.gamePadButtonHeld(button);

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].GamePadButtonHeld(button, out bool suppress);
				if (suppress)
					return;
			}
		}

		CurrentPage?.gamePadButtonHeld(button);
	}

	public override void receiveGamePadButton(Buttons button) {
		base.receiveGamePadButton(button);
		if (button == Buttons.LeftTrigger) {
			int target = mCurrent - 1;
			if (target < 0)
				target = Tabs.Count - 1;

			if (TryChangeTab(Tabs[target], playSound: true))
				CenterTab();

		} else if (button == Buttons.RightTrigger) {
			int target = mCurrent + 1;
			if (target >= Tabs.Count)
				target = 0;

			if (TryChangeTab(Tabs[target], playSound: true))
				CenterTab();

		} else {
			var overlays = CurrentOverlays;
			if (overlays is not null) {
				int c = overlays.Count;
				for (int i = 0; i < c; i++) {
					overlays[i].ReceiveGamePadButton(button, out bool suppress);
					if (suppress)
						return;
				}
			}

			CurrentPage?.receiveGamePadButton(button);
		}
	}

	public override bool areGamePadControlsImplemented() {
		return false;
	}

	public override void receiveKeyPress(Keys key) {
		bool suppressFromClose = false;
		if (Game1.options.menuButton.Contains(new InputButton(key)) && readyToClose(out suppressFromClose)) {
			Game1.exitActiveMenu();
			Game1.playSound(closeSound);
			return;

		} else {
			var overlays = CurrentOverlays;
			if (overlays is not null) {
				int c = overlays.Count;
				for (int i = 0; i < c; i++) {
					overlays[i].ReceiveKeyPress(key, out bool suppress);
					if (suppress)
						return;
				}
			}

			if (!suppressFromClose)
				CurrentPage?.receiveKeyPress(key);
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true) {
		if (CurrentPage is not CollectionsPage cp || cp.letterviewerSubMenu is null) {
			if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y)) {
				if (readyToClose()) {
					if (playSound)
						Game1.playSound(closeSound);

					exitThisMenu();
				}

				return;
			}
		}

		if (!mInvisible && !GameMenu.forcePreventClose) {
			if (btnTabsPrev?.containsPoint(x, y) ?? false) {
				btnTabsPrev.scale = btnTabsPrev.baseScale;
				if (ScrollTabs(-1) && playSound)
					Game1.playSound("shiny4");
				return;
			}

			if (btnTabsNext?.containsPoint(x, y) ?? false) {
				btnTabsNext.scale = btnTabsNext.baseScale;
				if (ScrollTabs(1) && playSound)
					Game1.playSound("shiny4");
				return;
			}

			foreach (var cmp in TabComponentList) {
				if (cmp.visible && cmp.containsPoint(x, y)) {
					TryChangeTab(cmp.name, playSound: true);
					return;
				}
			}
		}

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].ReceiveLeftClick(x, y, playSound, out bool suppress);
				if (suppress)
					return;
			}
		}

		CurrentPage?.receiveLeftClick(x, y, playSound);
	}

	public override void releaseLeftClick(int x, int y) {
		base.releaseLeftClick(x, y);

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].ReleaseLeftClick(x, y);
			}
		}

		CurrentPage?.releaseLeftClick(x, y);
	}

	public override void leftClickHeld(int x, int y) {
		base.leftClickHeld(x, y);

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].LeftClickHeld(x, y);
			}
		}

		CurrentPage?.leftClickHeld(x, y);
	}

	public override void receiveRightClick(int x, int y, bool playSound = true) {
		if (!mInvisible && !GameMenu.forcePreventClose) {
			foreach (var cmp in TabComponentList) {
				if (cmp.visible && cmp.containsPoint(x, y)) {
					OpenContextMenu(cmp.name, playSound);
					return;
				}
			}
		}

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].ReceiveRightClick(x, y, playSound, out bool suppress);
				if (suppress)
					return;
			}
		}

		CurrentPage?.receiveRightClick(x, y, playSound);
	}

	public override void receiveScrollWheelAction(int direction) {
		base.receiveScrollWheelAction(direction);

		if (!mInvisible && !GameMenu.forcePreventClose) {
			int y = Game1.getOldMouseY();
			if (y < (yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64 + 64)) {
				if (ScrollTabs(direction > 0 ? -1 : 1))
					Game1.playSound("shwip");
				return;
			}
		}

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].ReceiveScrollWheelAction(direction, out bool suppress);
				if (suppress)
					return;
			}
		}

		CurrentPage?.receiveScrollWheelAction(direction);
	}

	public override void performHoverAction(int x, int y) {
		base.performHoverAction(x, y);

		var overlays = CurrentOverlays;
		bool suppress = false;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].PerformHoverAction(x, y, out suppress);
				if (suppress)
					break;
			}
		}

		if (!suppress) {
			HoverTimer?.Start();
			CurrentPage?.performHoverAction(x, y);
			HoverTimer?.Stop();
		}

		btnTabsPrev?.tryHover(x, TabScroll > 0 ? y : -1);
		btnTabsNext?.tryHover(x, (TabScroll + VisibleTabComponents) >= TabComponentList.Count ? -1 : y);

		string? tt = null;

		if (!Invisible) {
			foreach (var cmp in TabComponentList) {
				if (cmp.visible && cmp.containsPoint(x, y)) {
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
					else {
						var pageType = page.GetType();
						string? typeName = pageType.Namespace == typeof(GameMenu).Namespace
							? pageType.Name
							: pageType.FullName;

						builder = builder.Text(typeName);
					}

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
		var newSize = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
		if (newSize != CurrentScreenSize) {
			CurrentScreenSize = newSize;
			ResizeMenu(CurrentTabHasErrored ? null : CurrentTab);
			TryGetPage(CurrentTab, out _);
		}
	}

	#endregion

	#region Context Menu

	public void OpenContextMenu(string target, bool playSound = false) {
		if (CurrentPage is not null && !IsPageReadyToClose(PageReadyToCloseReason.TabContextMenu, null, out _))
			return;

		List<ITabContextMenuEntry> options = [];

		if (Mod.Config.DeveloperMode && TabPages.ContainsKey(target))
			options.Add(new TabContextMenuEntry(I18n.Tab_ReloadTab(), () => TryReloadPage(target)));

		if (Mod.Config.DeveloperMode && TabOverlays.ContainsKey(target))
			options.Add(new TabContextMenuEntry(I18n.Tab_ReloadOverlays(), () => ReloadOverlays(target)));

		if (Mod.Config.AllowHotSwap &&
			TabSources.TryGetValue(target, out var sources) &&
			Mod.Implementations.TryGetValue(target, out var impls) &&
			impls.Count > 1
		) {
			if (options.Count > 0)
				options.Add(new TabContextMenuEntry("-", null));

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

				options.Add(new TabContextMenuEntry(label, active ? null : () => TryReloadPage(target, provider: key), icon));
			}
		}

		List<ITabContextMenuEntry> entries = [];
		if (target == nameof(VanillaTabOrders.Options) && Mod.CanOpenGMCM)
			entries.Add(new TabContextMenuEntry(I18n.Tab_OpenSettings(), Mod.OpenGMCM, null));

		Mod.FireTabContextMenu(this, target, entries);

		if (entries.Count > 0) {
			if (options.Count > 0)
				options.Add(new TabContextMenuEntry("-", null));
			options.AddRange(entries);
		}

		if (options.Count == 0)
			return;

		var pos = Game1.getMousePosition(true);

		PendingContextAction = null;

		var menu = new TabContextMenu(Mod, pos.X - 16, pos.Y - 16, options, action => PendingContextAction = action) {
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

		var overlays = CurrentOverlays;
		bool suppress = false;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++) {
				overlays[i].Update(time, out suppress);
				if (suppress)
					break;
			}
		}

		if (!suppress) {
			UpdateTimer?.Start();
			CurrentPage?.update(time);
			UpdateTimer?.Stop();
		}

		if (PendingContextAction != null && GetChildMenu() is null) {
			try {
				PendingContextAction();
			} catch (Exception ex) {
				Mod.Log($"Error executing context menu action: {ex}", LogLevel.Error);
			}

			PendingContextAction = null;
		}
	}

	private void DrawTab(SpriteBatch batch, ClickableComponent cmp, bool secondRow = false) {
		if (!TabDrawing.TryGetValue(cmp.name, out var stuff))
			return;

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

		if (secondRow)
			batch.Draw(
				Game1.mouseCursors,
				position: new Vector2(bounds.X, bounds.Y + 48),
				sourceRectangle: new Rectangle(16, 376, 16, 8),
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

	public override void draw(SpriteBatch batch) {
		//EntireDraw?.Start();
		var page = CurrentPage;

		if (!mInvisible) {
			if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
				batch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

			// Draw Background
			//Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, page?.width ?? width, page?.height ?? height, speaker: false, drawOnlyBox: true);

			// Draw Tabs
			if (SecondTabRow.Count > 0) {
				batch.End();
				batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

				foreach (var cmp in SecondTabRow)
					DrawTab(batch, cmp, true);
			}

			batch.End();
			batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

			foreach (var cmp in FirstTabRow)
				DrawTab(batch, cmp);

			if (btnTabsPrev is not null) {
				if (TabScroll == 0)
					btnTabsPrev.draw(batch, Color.Gray, 0.89f);
				else
					btnTabsPrev.draw(batch);
			}

			if (btnTabsNext is not null) {
				if (TabScroll + VisibleTabComponents >= TabComponentList.Count)
					btnTabsNext.draw(batch, Color.Gray, 0.89f);
				else
					btnTabsNext.draw(batch);
			}

			batch.End();
			batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}

		var overlays = CurrentOverlays;
		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++)
				overlays[i].PreDraw(batch);
		}

		DrawTimer?.Start();
		page?.draw(batch);
		DrawTimer?.Stop();

		if (overlays is not null) {
			int c = overlays.Count;
			for (int i = 0; i < c; i++)
				overlays[i].Draw(batch);
		}

		if (!GameMenu.forcePreventClose && (page?.shouldDrawCloseButton() ?? true))
			base.draw(batch);

		//EntireDraw?.Stop();

		if (Tooltip is not null) {
			var tt = Tooltip;
			if (DrawTimer is not null && LastTooltip == CurrentTab) {
				var builder = SimpleHelper.Builder().Add(tt);
				if (UpdateTimer is not null)
					builder = builder.Group(12)
						.Text("update ticks", Game1.textColor * 0.75f, shadow: false)
						.Text(UpdateTimer.StatString)
					.EndGroup();

				if (HoverTimer is not null)
					builder = builder.Group(12)
						.Text("hover ticks", Game1.textColor * 0.75f, shadow: false)
						.Text(HoverTimer.StatString)
					.EndGroup();

				tt = builder.Group(12)
						.Text("draw ticks", Game1.textColor * 0.75f, shadow: false)
						.Text(DrawTimer.StatString)
					.EndGroup()
					.GetLayout();
			}

			tt.DrawHover(batch, Game1.smallFont);
		}

		if ((!Game1.options.SnappyMenus || (page as CollectionsPage)?.letterviewerSubMenu == null) && !Game1.options.hardwareCursor)
			drawMouse(batch, ignore_transparency: true);

		/*if (EntireDraw is not null) {
			SimpleHelper.Builder()
				.Text(EntireDraw.StatString)
				.GetLayout()
				.DrawHover(batch, Game1.smallFont, overrideX: 0, overrideY: 0);
		}*/
	}

	internal void ResizeMenu(string? tab) {

		int newWidth = 800 + IClickableMenu.borderWidth * 2;
		int newHeight = 600 + IClickableMenu.borderWidth * 2;

		int minWidth = newWidth + 192;
		int minHeight = newHeight;

		bool invis = false;

		if (tab != null && TabSources.TryGetValue(tab, out var source)) {
			newWidth = source.Implementation.GetWidth?.Invoke(newWidth) ?? newWidth;
			newHeight = source.Implementation.GetHeight?.Invoke(newHeight) ?? newHeight;
			invis = source.Implementation.GetMenuInvisible?.Invoke() ?? false;
		}

		// Don't allow the menu to get narrower than 800.
		if (newWidth < 800)
			newWidth = 800;

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

		// Stabilize the position of the upper right close button.
		if (!invis)
			width = 800 + IClickableMenu.borderWidth * 2;

		initializeUpperRightCloseButton();
		width = newWidth;

		if (Game1.options.SnappyMenus)
			snapCursorToCurrentSnappedComponent();
	}

	internal bool ScrollTabs(int direction) {
		int old = TabScroll;
		int maxScroll = TabComponentList.Count - VisibleTabComponents;

		int magnitude = SecondTabRow.Count > 0 ? 2 : 1;

		TabScroll += magnitude * ((direction > 0) ? 1 : -1);
		TabScroll -= TabScroll % magnitude;

		if (TabScroll < 0)
			TabScroll = 0;
		if (TabScroll > maxScroll)
			TabScroll = maxScroll;

		if (old == TabScroll)
			return false;

		RepositionTabs();
		return true;
	}

	internal bool CenterTab() {
		string current = CurrentTab;
		for (int i = 0; i < TabComponentList.Count; i++) {
			var cmp = TabComponentList[i];
			if (cmp.name == current)
				return CenterTab(i);
		}
		return false;
	}

	internal bool CenterTab(int index) {
		int old = TabScroll;
		int maxScroll = TabComponentList.Count - VisibleTabComponents;
		int magnitude = SecondTabRow.Count > 0 ? 2 : 1;

		TabScroll = index - (VisibleTabComponents / 2);
		TabScroll -= TabScroll % magnitude;

		if (TabScroll < 0)
			TabScroll = 0;
		if (TabScroll > maxScroll)
			TabScroll = maxScroll;

		if (old == TabScroll)
			return false;

		RepositionTabs();
		return true;
	}

	/// <summary>
	/// Whether or not to allow two rows of tabs to be displayed. This depends
	/// on the y position of the menu.
	/// </summary>
	internal bool ShouldAllowSecondRow => Mod.Config.AllowSecondRow switch {
		AllowSecondRow.Automatic => yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY >= -16,
		AllowSecondRow.Always => true,
		_ => false
	};

	internal void RepositionTabs() {
		var menu = CurrentPage;
		if (menu is not null)
			RemoveTabsFromClickableComponents(menu);

		FirstTabRow.Clear();
		SecondTabRow.Clear();

		int x = xPositionOnScreen + 48;
		int y = yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64;

		// First, we need to determine how many tabs we can fit in our width.
		int visibleFirstRow = ((800 + IClickableMenu.borderWidth) - (48 * 2)) / 64;

		// Next, do we need a second row? Can we even fit a second row?
		// TODO: y value check
		bool needSecondRow = ShouldAllowSecondRow && visibleFirstRow < TabComponentList.Count;

		// Alright, if we're using a second row, we need to determine how many items
		// are going on the second row vs the first.
		int rowItems = TabComponentList.Count / 2;
		int secondRowItems = Math.Max(0, TabComponentList.Count - Math.Max(rowItems, visibleFirstRow));

		int visibleSecondRow = needSecondRow ? Math.Max(0, Math.Min(secondRowItems, visibleFirstRow - 1)) : 0;

		// Do we have enough space to fit all our tabs?
		bool needPagination = (visibleFirstRow + visibleSecondRow) < TabComponentList.Count;

		// TODO: Math to ensure there's never 

		VisibleTabComponents = Math.Min(TabComponentList.Count, visibleFirstRow + visibleSecondRow);

		//Mod.Log($"Pos: {x},{y} -- visible: {visibleTabs} -- needPagination: {needPagination}", LogLevel.Debug);

		if (needPagination && btnTabsPrev is null) {
			btnTabsPrev = new ClickableTextureComponent(
				bounds: Rectangle.Empty,
				texture: Game1.mouseCursors,
				sourceRect: new Rectangle(352, 495, 12, 11),
				scale: 3.2f
			) {
				myID = 12338,
				rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
			};

			btnTabsNext = new ClickableTextureComponent(
				bounds: Rectangle.Empty,
				texture: Game1.mouseCursors,
				sourceRect: new Rectangle(365, 495, 12, 11),
				scale: 3.2f
			) {
				myID = 12339,
				leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
			};

		} else if (!needPagination && btnTabsPrev is not null) {
			btnTabsPrev = null;
			btnTabsNext = null;
		}

		if (btnTabsPrev is not null) {
			x -= 50;
			btnTabsPrev.bounds = new(x, y + 16, 48, 48);
			x += btnTabsPrev.bounds.Width + 2;
		}

		int scroll = TabScroll;
		int first = 0;
		int second = 0;
		bool isFirst = false;

		foreach (var cmp in TabComponentList) {
			isFirst = !isFirst;
			if (!isFirst && second >= visibleSecondRow)
				isFirst = true;

			if (scroll > 0 || (isFirst ? first >= visibleFirstRow : second >= visibleSecondRow)) {
				scroll--;
				cmp.visible = false;
				continue;
			}

			// Make absolutely sure we aren't drawing the second tab row first.
			if (!isFirst && FirstTabRow.Count == 0) {
				second++;
				cmp.visible = false;
				continue;
			}

			cmp.visible = true;
			// Reset our neighbors while we're at it, in case they were altered.
			cmp.leftNeighborID = ClickableComponent.SNAP_AUTOMATIC;
			cmp.rightNeighborID = ClickableComponent.SNAP_AUTOMATIC;
			cmp.upNeighborID = ClickableComponent.SNAP_AUTOMATIC;
			cmp.downNeighborID = ClickableComponent.SNAP_AUTOMATIC;

			if (isFirst) {
				first++;
				cmp.bounds = new Rectangle(x, y, 64, 64);
				x += 64;

				FirstTabRow.Add(cmp);

			} else {
				second++;
				cmp.bounds = new Rectangle(x - 32, y - 60, 64, 60);
				SecondTabRow.Add(cmp);
			}
		}

		if (btnTabsNext is not null) {
			x += 2;
			btnTabsNext.bounds = new(x, y + 16, 48, 48);
			x += btnTabsNext.bounds.Width;
		}

		if (menu is not null)
			AddTabsToClickableComponents(menu);
	}

	#region IBetterGameMenu

	public IClickableMenu Menu => this;

	public bool Invisible {
		get => mInvisible;
		set {
			mInvisible = value;

			foreach (var cmp in FirstTabRow) {
				cmp.visible = !mInvisible;
			}
			foreach (var cmp in SecondTabRow) {
				cmp.visible = !mInvisible;
			}

			if (btnTabsNext != null)
				btnTabsNext.visible = !mInvisible;

			if (btnTabsPrev != null)
				btnTabsPrev.visible = !mInvisible;
		}
	}

	public IReadOnlyList<string> VisibleTabs => Tabs.AsReadOnly();

	public string CurrentTab => mCurrent >= 0 && mCurrent < Tabs.Count ? Tabs[mCurrent] : string.Empty;

	// TODO: Change this to a field backed property for performance?
	public IClickableMenu? CurrentPage => TabPages.GetValueOrDefault(CurrentTab);

	public List<IPageOverlay>? CurrentOverlays => TabOverlays.GetValueOrDefault(CurrentTab);

	public bool CurrentTabHasErrored => CurrentPage is ErrorMenu;

	public bool TryChangeTab(string target, bool playSound = true) {
		return TryChangeTabImpl(target, playSound: playSound);
	}

	private void ResetPerformanceTracking() {
		bool useTimers = Mod.Config.DeveloperMode;

		UpdateTimer = useTimers ? new() : null;
		HoverTimer = useTimers ? new() : null;
		DrawTimer = useTimers ? new() : null;
	}

	internal bool TryChangeTabImpl(string target, bool performSnap = true, bool playSound = true) {
		int tIndex = Tabs.IndexOf(target);
		if (tIndex == -1)
			return false;
		if (tIndex == mCurrent)
			return true;

		string oldTab = CurrentTab;
		var oldPage = CurrentPage;
		var oldOverlays = CurrentOverlays;
		ClickableComponent? oldSnapped = oldPage?.currentlySnappedComponent;

		if (oldPage is not null) {
			if (!IsPageReadyToClose(PageReadyToCloseReason.TabChanging, null, out _))
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

		if (upperRightCloseButton is not null)
			upperRightCloseButton.visible = true;

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

		// Update the old overlays.
		if (oldOverlays is not null && oldOverlays.Count > 0)
			foreach (var overlay in oldOverlays) {
				overlay.OnDeactivate();
			}

		// Inject our tab components.
		page.populateClickableComponentList();
		AddTabsToClickableComponents(page, target);

		// Clear any existing decoration.
		TabDecorations.Remove(target);

		ResetPerformanceTracking();

		// Update the current tab and fire the event.
		mCurrent = tIndex;
		mLastTab = string.IsNullOrEmpty(oldTab) ? null : oldTab;
		FireTabChanged(target, oldTab);

		// See about new overlays / activating them.
		var overlays = CurrentOverlays;
		if (overlays is null)
			TabOverlays[CurrentTab] = page is ErrorMenu
				? null
				: Mod.FirePageOverlayCreation(this, CurrentTab, sources.Implementation.Source, page);
		else
			foreach (var overlay in overlays)
				overlay.OnActivate();

		if (playSound)
			Game1.playSound("smallSelect");

		// And move the cursor last.
		if (performSnap && Game1.options.SnappyMenus) {
			int extras = page.allClickableComponents.Count - (FirstTabRow.Count + SecondTabRow.Count);
			if (btnTabsPrev is not null) extras--;
			if (btnTabsNext is not null) extras--;

			if (extras > 0)
				snapToDefaultClickableComponent();
			else if (page.allClickableComponents.Contains(oldSnapped))
				page.currentlySnappedComponent = oldSnapped;
		}

		return true;
	}

	internal void ReloadOverlays(string target) {
		// Clear the wrapper cache.
		WrappedPageOverlay._Wrappers.Clear();

		if (!TabOverlays.TryGetValue(target, out var overlays))
			return;

		if (overlays is not null)
			foreach (var overlay in overlays)
				overlay.Dispose();

		TabOverlays.Remove(target);

		if (CurrentTab != target || !TabSources.TryGetValue(target, out var sources))
			return;

		TabOverlays[target] = CurrentPage is ErrorMenu
			? null
			: Mod.FirePageOverlayCreation(this, target, sources.Implementation.Source, CurrentPage);
	}

	internal void TryReloadPage(string target, string? provider = null) {
		if (!TabSources.TryGetValue(target, out var sources))
			return;

		if (TabPages.TryGetValue(target, out var page)) {
			if (!IsPageReadyToClose(PageReadyToCloseReason.TabReloading, target, out _))
				return;

			// On Close
			if (page is not ErrorMenu)
				DisposePage(target, page);
		}

		// Clear state.
		TabLastSize.Remove(target);
		TabPages.Remove(target);
		TabDecorations.Remove(target);

		ResetPerformanceTracking();

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
		AddTabsToClickableComponents(page, target);

		// Create our tab overlays.
		if (!TabOverlays.TryGetValue(target, out var overlays) || overlays is null)
			TabOverlays[target] = page is ErrorMenu
				? null
				: Mod.FirePageOverlayCreation(this, target, sources.Implementation.Source, page);

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
						AddTabsToClickableComponents(page, target);

						// See if we need to recreate our page overlays.
						if (CurrentTab == target) {
							TabOverlays[target] = page is ErrorMenu
								? null
								: Mod.FirePageOverlayCreation(this, target, sources.Implementation.Source, page);
						}

						// Clear the tooltip, just in case.
						if (Mod.Config.DeveloperMode) {
							LastTooltip = null;
							Tooltip = null;
						}
					}

				} else {
					if (TabOverlays.TryGetValue(target, out var overlays) && overlays is not null)
						foreach (var overlay in overlays)
							overlay.PageSizeChanged(oldSize, CurrentScreenSize);

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
		if (Mod.Config.DeveloperMode) {
			LastTooltip = null;
			Tooltip = null;
		}

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
						upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
						downNeighborID = ClickableComponent.SNAP_AUTOMATIC
					};
					TabComponents[id] = component;
				}

				component.visible = !Invisible;

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

		Mod.FirePageCreated(this, tab, source, page, oldPage);
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
