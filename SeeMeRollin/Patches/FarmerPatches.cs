using System.Collections.Generic;

using HarmonyLib;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;


namespace Leclair.Stardew.SeeMeRollin.Patches {
	public class FarmerPatches {

		public static IMonitor Monitor => ModEntry.instance.Monitor;

		[HarmonyPatch(typeof(Farmer), "performBeginUsingTool")]
		public static class Buff_Begin {
			static bool Prefix(Farmer __instance) {
				ModEntry.instance.FixAnimation(__instance);
				return true;
			}
		}

	}
}
