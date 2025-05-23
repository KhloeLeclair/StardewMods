using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

public static class Game1_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			// I love delegate method classes.
			var type = AccessTools.FirstInner(typeof(Game1), t =>
				t.GetField("currentKBState") != null &&
				t.GetField("currentMouseState") != null &&
				t.GetField("currentPadState") != null &&
				t.GetField("time") != null
			) ?? throw new ArgumentNullException("could not find delegate class");

			// Love them so much I do.
			var method = AccessTools.FirstMethod(type, m =>
				m.Name.StartsWith("<UpdateControlInput>")
			) ?? throw new ArgumentNullException("could not find method");

			// Oh hey back to normal, sane code.
			mod.Harmony.Patch(
				original: method,
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(UpdateControlInput__Transpiler))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.CheckGamepadMode)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(CheckGamepadMode__Transpiler))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching Game1. Game Menu will not be replaced correctly.", StardewModdingAPI.LogLevel.Error, ex);
		}

	}

	#region Helper Methods

	internal static IClickableMenu CreateMenuBasic(bool playOpeningSound) {
		//Mod?.Log($"Called CreateMenuBasic {playOpeningSound}", StardewModdingAPI.LogLevel.Warn);
		if (Mod is not null && Mod.IsEnabled)
			return new BetterGameMenuImpl(Mod, playOpeningSound: playOpeningSound);
		return new GameMenu(playOpeningSound);
	}

	internal static IClickableMenu CreateMenuOther(int startingTab, int extra, bool playOpeningSound) {
		//Mod?.Log($"Called CreateMenuOther {startingTab} {extra} {playOpeningSound}", StardewModdingAPI.LogLevel.Warn);
		if (Mod is not null && Mod.IsEnabled)
			return Mod.CreateMenuFromTabId(startingTab, extra, playOpeningSound);

		return new GameMenu(startingTab, extra, playOpeningSound);
	}

	#endregion

	private static IEnumerable<CodeInstruction> CheckGamepadMode__Transpiler(IEnumerable<CodeInstruction> instructions) {
		var GameMenu_Ctor_Basic = AccessTools.Constructor(typeof(GameMenu), [typeof(bool)]);
		var Our_Basic = AccessTools.Method(typeof(Game1_Patches), nameof(CreateMenuBasic));

		foreach (var instr in instructions) {
			if (instr.opcode == OpCodes.Newobj && instr.operand is ConstructorInfo ctor && ctor == GameMenu_Ctor_Basic) {
				// Old Code: new GameMenu(bool)
				// New Code: CreateMenuBasic(bool)
				yield return new CodeInstruction(instr) {
					opcode = OpCodes.Call,
					operand = Our_Basic
				};

			} else
				yield return instr;
		}
	}

	private static IEnumerable<CodeInstruction> UpdateControlInput__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameMenu_Ctor_Basic = AccessTools.Constructor(typeof(GameMenu), [typeof(bool)]);
		var GameMenu_Ctor_Other = AccessTools.Constructor(typeof(GameMenu), [typeof(int), typeof(int), typeof(bool)]);

		var Our_Basic = AccessTools.Method(typeof(Game1_Patches), nameof(CreateMenuBasic));
		var Our_Other = AccessTools.Method(typeof(Game1_Patches), nameof(CreateMenuOther));

		foreach (var instr in instructions) {
			if (instr.opcode != OpCodes.Newobj || instr.operand is not ConstructorInfo ctor) {
				yield return instr;
				continue;
			}

			if (ctor == GameMenu_Ctor_Basic) {
				// Old Code: new GameMenu(bool)
				// New Code: CreateMenuBasic(bool)
				yield return new CodeInstruction(instr) {
					opcode = OpCodes.Call,
					operand = Our_Basic
				};

			} else if (ctor == GameMenu_Ctor_Other) {
				// Old Code: new GameMenu(int, int, bool)
				// New Code: CreateMenuOther(int, int, bool)
				yield return new CodeInstruction(instr) {
					opcode = OpCodes.Call,
					operand = Our_Other
				};

			} else
				yield return instr;
		}
	}

}
