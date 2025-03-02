using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.Common;

using StardewModdingAPI;

using StardewValley;

namespace Leclair.Stardew.CloudySkies.Patches;

public static class SObject_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {

		Mod = mod;

		try {

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
				postfix: new HarmonyMethod(typeof(SObject_Patches), nameof(checkForAction__Postfix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(SObject), nameof(SObject.performUseAction)),
				prefix: new HarmonyMethod(typeof(SObject_Patches), nameof(performUseAction__Prefix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(SObject), nameof(SObject.performToolAction)),
				transpiler: new HarmonyMethod(typeof(SObject_Patches), nameof(performToolAction__Transpiler))
			);

		} catch (Exception ex) {

			Mod.Log($"Error patching SObject. Weather Totems will not work correctly.", LogLevel.Error, ex);

		}

	}

	private static void checkForAction__Postfix(SObject __instance, Farmer who, bool justCheckingForActivity, ref bool __result) {
		try {
			// If there was already an action, don't do more. Also don't do more if this
			// isn't a big craftable.
			if (__result || !__instance.bigCraftable.Value)
				return;

			// Let's try to find an action! Prioritize modData first.
			if (!__instance.modData.TryGetValue("leclair.cloudyskies/PerformAction", out string? action) &&
				Game1.bigCraftableData.TryGetValue(__instance.ItemId, out var data) && data.CustomFields is not null
			) {
				// Try to find it in CustomFields instead, with backup support for Better Crafting's field.
				if (!data.CustomFields.TryGetValue("leclair.cloudyskies/PerformAction", out action)) {
					if (Mod != null && !Mod.Helper.ModRegistry.IsLoaded("leclair.bettercrafting"))
						data.CustomFields.TryGetValue("leclair.bettercrafting_PerformAction", out action);
				}
			}

			// If we didn't find an action in all that, quit.
			if (string.IsNullOrWhiteSpace(action))
				return;

			// Yes, we have an action.
			__result = true;

			// Don't perform our action if we're just checking.
			if (justCheckingForActivity)
				return;

			// Run the action.
			__instance.Location.performAction(action, who, __instance.TileLocation.ToLocation());

		} catch (Exception ex) {
			Mod?.Log($"Error while attempting to interact with an object: {ex}", LogLevel.Error);
		}
	}

	private static void DigUpArtifactSpot(GameLocation location, int x, int y, Farmer who, SObject sobj) {
		try {
			if (Triggers.DigUpCustomArtifactSpot(location, x, y, who, sobj))
				return;

		} catch (Exception ex) {
			Mod?.Log($"Error handling artifact spot: {ex}", LogLevel.Error);
		}

		location.digUpArtifactSpot(x, y, who);
	}

	private static IEnumerable<CodeInstruction> performToolAction__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_digUpArtifactSpot = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.digUpArtifactSpot));
		var Our_DigUpArtifactSpot = AccessTools.Method(typeof(SObject_Patches), nameof(DigUpArtifactSpot));

		if (GameLocation_digUpArtifactSpot is null)
			throw new Exception("could not find necessary method");

		foreach (var in0 in instructions) {
			if (in0.Calls(GameLocation_digUpArtifactSpot)) {
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Ldarg_0,
					operand = null
				};
				yield return new CodeInstruction(OpCodes.Call, Our_DigUpArtifactSpot);

			} else
				yield return in0;
		}

	}

	private static bool performUseAction__Prefix(SObject __instance, GameLocation location, ref bool __result) {

		try {

			if (Mod is not null) {

				if ((!__instance.modData.TryGetValue(ModEntry.WEATHER_TOTEM_DATA, out string? weatherTotem) &&
					Game1.objectData != null &&
					Game1.objectData.TryGetValue(__instance.ItemId, out var data) &&
					data.CustomFields != null &&
					!data.CustomFields.TryGetValue(ModEntry.WEATHER_TOTEM_DATA, out weatherTotem)) ||
					string.IsNullOrEmpty(weatherTotem)
				)
					return true;

				// Wow, what a mess of an if statement. If we got here, we have a weather totem!

				// Handle the performUseAction logic.
				if (!Game1.player.CanMove || __instance.isTemporarilyInvisible) {
					__result = false;
					return false;
				}

				bool normal_gameplay = !Game1.eventUp &&
					!Game1.isFestival() &&
					!Game1.fadeToBlack &&
					!Game1.player.swimming.Value &&
					!Game1.player.bathingClothes.Value &&
					!Game1.player.onBridge.Value;

				if (normal_gameplay)
					__result = Mod.UseWeatherTotem(Game1.player, weatherTotem, __instance);
				else
					__result = false;

				return false;
			}

		} catch (Exception ex) {
			Mod?.Log($"Error handling weather totem check: {ex}", LogLevel.Error, once: true);
		}

		return true;

	}

}
