using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.Common.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.CloudySkies.Patches;

public static class DayTimeMoneyBox_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {
		Mod = mod;

		if (Constants.TargetPlatform == GamePlatform.Android) {
			mod.Log($"Not patching DayTimeMoneyBox while running on Android.", LogLevel.Debug);
			return;
		}

		try {

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.draw), [typeof(SpriteBatch)]),
				postfix: new HarmonyMethod(typeof(DayTimeMoneyBox_Patches), nameof(Draw__Postfix)),
				transpiler: new HarmonyMethod(typeof(DayTimeMoneyBox_Patches), nameof(Draw__Transpiler))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching DayTimeMoneyBox.", LogLevel.Error, ex);
		}

	}


	public static bool DrawWeatherIcon(DayTimeMoneyBox menu, SpriteBatch b) {
		try {
			if (Mod is not null)
				return Mod.DrawWeatherIcon(b, menu.Position + new Vector2(116, 68));
		} catch (Exception ex) {
			Mod?.Log($"Error drawing weather icon: {ex}", LogLevel.Error, once: true);
		}

		return false;
	}


	private static void Draw__Postfix(DayTimeMoneyBox __instance, SpriteBatch b) {
		try {
			if (Mod is not null && Mod.Config.ShowWeatherTooltip) {
				int x = Game1.getOldMouseX();
				int y = Game1.getOldMouseY();

				if (new Rectangle(__instance.xPositionOnScreen + 116, __instance.yPositionOnScreen + 68, 48, 32).Contains(x, y)) {
					string title = Mod.GetWeatherName();
					if (!string.IsNullOrWhiteSpace(title))
						SimpleHelper.Builder()
							.Text(title)
							.GetLayout()
							.DrawHover(b, Game1.dialogueFont);
				}
			}

		} catch (Exception ex) {
			Mod?.Log($"Error drawing weather icon: {ex}", LogLevel.Error, once: true);
		}
	}

	private static IEnumerable<CodeInstruction> Draw__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var first_match = new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Game1), nameof(Game1.weatherIcon)) ?? throw new Exception($"could not find {nameof(Game1.weatherIcon)}"));

		var matcher = new CodeMatcher(instructions)
			.MatchStartForward(first_match)
			.MatchStartForward(new CodeMatch(OpCodes.Br_S))
			.ThrowIfInvalid("could not find weather icon drawing");

		var instr = matcher.Instruction;

		matcher = matcher
			.MatchStartBackwards(first_match)
			.Insert(
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldarg_1),
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DayTimeMoneyBox_Patches), nameof(DrawWeatherIcon))),
				new CodeInstruction(OpCodes.Brtrue_S, instr.operand)
			);

		return matcher.InstructionEnumeration();
	}

}
