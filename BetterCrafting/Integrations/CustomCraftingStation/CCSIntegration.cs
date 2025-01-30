#nullable enable
using System;
using System.Collections.Generic;

using HarmonyLib;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Integrations;

using StardewModdingAPI;

namespace Leclair.Stardew.BetterCrafting.Integrations.CustomCraftingStation;

public class CCSIntegration : BaseIntegration<ModEntry> {

	// ModEntry
	private static IMod? Entry;
	private static object? ContentManager;

	public CCSIntegration(ModEntry mod)
	: base(mod, "Cherry.CustomCraftingStations", "1.1.3") {
		if (!IsLoaded)
			return;

		mod.Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
	}

	private void LoadEntry() {
		if (!IsLoaded || Entry is not null)
			return;

		try {
			var info = Self.Helper.ModRegistry.Get(ModID);
			if (info is null)
				return;

			var modProp = AccessTools.Property(info.GetType(), "Mod");
			if (modProp is null)
				return;

			var getter = ReflectionHelper.CreateGetter<object, IMod?>(modProp);
			Entry = getter(info);

		} catch (Exception ex) {
			Log($"Unable to grab ModEntry for CustomCraftingStations. Details: {ex}", LogLevel.Debug);
		}
	}

	private void LoadContentManager() {
		LoadEntry();
		if (Entry is null || ContentManager is not null)
			return;

		ContentManager = Self.Helper.Reflection.GetField<object>(Entry, "ContentManager", false)?.GetValue();
	}

	private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e) {
		// CCS creates a new content manager, invalidating our handle.
		ContentManager = null;
	}

	public HashSet<string>? GetRemovedRecipes(bool cooking) {
		LoadContentManager();
		if (ContentManager is null)
			return null;

		return Self.Helper.Reflection.GetField<HashSet<string>>(ContentManager, cooking
			? "ExclusiveCookingRecipes"
			: "ExclusiveCraftingRecipes",
			required: false)?.GetValue();
	}

}
