using System;

using StardewModdingAPI;

namespace Leclair.Stardew.BreakPintailHost;

public class ModEntry : Mod {

	ModAPI API = new();

	public override void Entry(IModHelper helper) {
		
	}

	public override object? GetApi() {
		return API;
	}

}
