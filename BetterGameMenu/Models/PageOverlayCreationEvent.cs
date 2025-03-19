using System;
using System.Collections.Generic;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewModdingAPI;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed class PageOverlayCreationEvent : IPageOverlayCreationEvent {

	private readonly ModEntry Mod;
	private readonly BetterGameMenuImpl mMenu;

	internal IModInfo? ModSource;

	internal readonly List<IPageOverlay> Overlays = [];

	public PageOverlayCreationEvent(ModEntry mod, BetterGameMenuImpl menu, string tab, string source, IClickableMenu page) {
		Mod = mod;
		mMenu = menu;
		Tab = tab;
		Source = source;
		Page = page;
	}

	public IClickableMenu Menu => mMenu;

	public string Tab { get; }

	public string Source { get; }

	public IClickableMenu Page { get; }

	public void AddOverlay(IDisposable overlay) {
		if (overlay is null)
			throw new ArgumentNullException(nameof(overlay));
		if (ModSource is null)
			throw new ArgumentNullException("Called AddOverlay at bad time");

		// In case Pintail handed us something weird...
		if (Mod.TryUnproxy(overlay, out object? unproxied) && unproxied is IDisposable disp)
			overlay = disp;

		// This may throw an exception, but that's okay.
		IPageOverlay pageOverlay;

		try {
			pageOverlay = new WrappedPageOverlay(overlay, ModSource);
		} catch (Exception ex) {
			throw new InvalidCastException($"Unable to cast overlay from type {overlay.GetType().FullName}", ex);
		}

		Overlays.Add(pageOverlay);
	}
}
