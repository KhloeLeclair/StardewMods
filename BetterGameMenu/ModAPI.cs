using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Leclair.Stardew.BetterGameMenu.Menus;
using Leclair.Stardew.BetterGameMenu.Models;
using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu;

public class ModAPI : IBetterGameMenuApi {

	private readonly ModEntry Self;
	private readonly IModInfo Other;

	internal ModAPI(ModEntry self, IModInfo other) {
		Self = self;
		Other = other;
	}

	public event EventHandler<IClickableMenu>? OnMenuInstantiated;
	public event EventHandler<(IClickableMenu Menu, string Tab, string OldTab)>? OnTabChanged;
	public event EventHandler<(IClickableMenu Menu, string Tab, string Source, IClickableMenu Page, IClickableMenu? OldPage)>? OnPageInstantiated;

	internal void FireMenuInstantiated(IClickableMenu menu) {
		try {
			OnMenuInstantiated?.Invoke(menu, menu);
		} catch (Exception ex) {
			Self.Log($"Error in OnMenuInstantiated handler for mod '{Other.Manifest.Name}' ({Other.Manifest.UniqueID}): {ex}", LogLevel.Error);
		}
	}

	internal void FireTabChanged(IClickableMenu menu, string tab, string oldTab) {
		try {
			OnTabChanged?.Invoke(menu, (menu, tab, oldTab));
		} catch (Exception ex) {
			Self.Log($"Error in OnTabChanged handler for mod '{Other.Manifest.Name}' ({Other.Manifest.UniqueID}): {ex}", LogLevel.Error);
		}
	}

	internal void FirePageInstantiated(IClickableMenu menu, string tab, string source, IClickableMenu page, IClickableMenu? oldPage) {
		try {
			OnPageInstantiated?.Invoke(menu, (menu, tab, source, page, oldPage));
		} catch (Exception ex) {
			Self.Log($"Error in OnPageInstantiated handler for mod '{Other.Manifest.Name}' ({Other.Manifest.UniqueID}): {ex}", LogLevel.Error);
		}
	}

	public static IBetterGameMenuApi.DrawDelegate CreateDrawImpl(Texture2D texture, Rectangle source, float scale, int frames = 1, int frameTime = 16) {
		void Draw(SpriteBatch batch, Rectangle bounds) {
			var rect = frames > 1
				? SpriteInfo.GetFrame(source, -1, frames, int.MaxValue, frameTime)
				: source;

			float width = rect.Width * scale;
			float height = rect.Height * scale;

			float s = scale; // Math.Min(scale, Math.Min(bounds.Width / width, bounds.Height / height));

			//width *= s;
			//height *= s;

			batch.Draw(
				texture,
				new Vector2(
					bounds.X + (float) Math.Floor((bounds.Width - width) / 2),
					bounds.Y + (float) Math.Floor((bounds.Height - height) / 2)
				),
				rect,
				Color.White,
				0f,
				Vector2.Zero,
				s,
				SpriteEffects.None,
				1f
			);
		}

		return Draw;
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
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null
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
			OnResize: onResize
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
		Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null
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
			OnResize: onResize
		));
	}

	public bool TryGetActiveMenu([NotNullWhen(true)] out IBetterGameMenu? menu) {
		if (Game1.activeClickableMenu is BetterGameMenuImpl bgm) {
			menu = bgm;
			return true;
		}

		menu = null;
		return false;
	}

	public bool TryGetMenu(IClickableMenu menu, [NotNullWhen(true)] out IBetterGameMenu? castMenu) {
		if (menu is BetterGameMenuImpl bgm) {
			castMenu = bgm;
			return true;
		}

		castMenu = null;
		return false;
	}

	public bool TryOpenMenu([NotNullWhen(true)] out IBetterGameMenu? menu, string? defaultTab = null, bool playSound = false, bool closeExistingMenu = false) {
		throw new NotImplementedException();
	}
}
