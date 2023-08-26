using System.Collections.Generic;

using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.SeeMeRollin.Patches {
	public class BuffPatches {

		public static IMonitor Monitor => ModEntry.Instance.Monitor;

		[HarmonyPatch(typeof(Buff), nameof(Buff.update))]
		public static class Buff_Update {
			static bool Prefix(Buff __instance, GameTime time, ref bool __result) {
				if (__instance is RollinBuff) {
					__result = false;
					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(Buff), nameof(Buff.getTimeLeft))]
		public static class Buff_TimeLeft {
			static bool Prefix(Buff __instance, ref string __result) {
				if (__instance is RollinBuff) {
					__result = "";
					return false;
				}

				return true;
			}
		}

		[HarmonyPatch(typeof(Buff), nameof(Buff.getClickableComponents))]
		public static class Buff_Components {
			static bool Prefix(Buff __instance, ref List<ClickableTextureComponent> __result) {
				if (__instance is RollinBuff && !ModEntry.Instance.Config.ShowBuff) {
					__result = new();
					return false;
				}

				return true;
			}
		}

	}
}
