#nullable enable

using System;
using System.Collections.Generic;

using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Patches;

public static class GameLocation_Patches {

	private static IMonitor? Monitor;

	public static void Patch(ModEntry mod) {
		Monitor = mod.Monitor;

		try {
			mod.Harmony!.Patch(
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.ActivateKitchen)),
				prefix: new HarmonyMethod(typeof(GameLocation_Patches), nameof(ActivateKitchen_Prefix))
			);

		} catch (Exception ex) {
			mod.Log("An error occurred while registering a harmony patch for the FarmHouse.", LogLevel.Warn, ex);
		}
	}

	public static bool ActivateKitchen_Prefix(GameLocation __instance) {

		try {
			ModEntry mod = ModEntry.Instance;

			if (mod.Config.ReplaceCooking && !(mod.Config.SuppressBC?.IsDown() ?? false)) {

				Point? point = __instance.GetFridgePosition();

				Game1.activeClickableMenu = Menus.BetterCraftingPage.Open(
					mod,
					location: __instance,
					position: point.HasValue ? new(point.Value.X, point.Value.Y) : null,
					standalone_menu: true,
					cooking: true,
					material_containers: (IList<object>?) null
				);

				return false;
			}

		} catch(Exception ex) {
			Monitor?.Log($"An error occurred while attempting to perform a location action.", LogLevel.Warn);
			Monitor?.Log($"Details:\n{ex}", LogLevel.Warn);
		}

		return true;
	}

}
