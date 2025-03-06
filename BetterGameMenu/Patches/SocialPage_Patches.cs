using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.BetterGameMenu.Menus;
using Leclair.Stardew.Common;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Patches;

internal static class SocialPage_Patches {

	private static ModEntry? Mod;

	internal static void Patch(ModEntry mod) {
		Mod = mod;

		try {
			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(SocialPage), nameof(SocialPage.receiveLeftClick)),
				transpiler: new HarmonyMethod(typeof(SocialPage_Patches), nameof(receiveLeftClick__Transpiler))
			);

		} catch (Exception ex) {
			mod.Log($"Error patching SocialPage: {ex}", LogLevel.Error);
		}
	}

	#region Helper Methods

	private static Action<SocialPage, SocialPage.SocialEntry>? _SelectSlot;

	private static Action<SocialPage, SocialPage.SocialEntry>? GetSelectSlot() {
		if (_SelectSlot is null) {
			var method = AccessTools.Method(typeof(SocialPage), "_SelectSlot", [typeof(SocialPage.SocialEntry)]);
			if (method is not null)
				_SelectSlot = method.CreateAction<SocialPage, SocialPage.SocialEntry>();
		}
		return _SelectSlot;
	}

	private static void AssignExitFunction(ProfileMenu profile, SocialPage page) {
		if (Mod is null)
			return;

		int position = page.slotPosition;
		profile.exitFunction = () => {
			SocialPage? sp;
			if (!Mod.IsEnabled) {
				var gm = new GameMenu(GameMenu.socialTab, -1, playOpeningSound: false);
				Game1.activeClickableMenu = gm;
				sp = gm.GetCurrentPage() as SocialPage;
			} else {
				var bgm = new BetterGameMenuImpl(Mod, nameof(VanillaTabOrders.Social), playOpeningSound: false);
				Game1.activeClickableMenu = bgm;
				sp = bgm.CurrentPage as SocialPage;
			}

			if (sp is not null) {
				sp.slotPosition = position;
				GetSelectSlot()?.Invoke(sp, profile.Current);
			}
		};
	}

	#endregion

	private static IEnumerable<CodeInstruction> receiveLeftClick__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var fExitFunction = AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.exitFunction));
		var mAssignExitFunction = AccessTools.Method(typeof(SocialPage_Patches), nameof(AssignExitFunction));

		var matcher = new CodeMatcher(instructions)
			.MatchStartForward(
				new CodeMatch(in0 => in0.IsLdloc()),
				new CodeMatch(OpCodes.Ldftn),
				new CodeMatch(OpCodes.Newobj),
				new CodeMatch(OpCodes.Stfld, fExitFunction)
			)
			.ThrowIfInvalid("could not find exitFunction assignment")
			.RemoveInstructions(4)
			.Insert(
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Call, mAssignExitFunction)
			);

		return matcher.InstructionEnumeration();

	}

}
