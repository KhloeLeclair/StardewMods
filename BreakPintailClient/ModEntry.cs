using System;

using StardewModdingAPI;

using Leclair.Stardew.BreakPintail;

namespace Leclair.Stardew.BreakPintailClient;

public class ModEntry : Mod {

	public override void Entry(IModHelper helper) {
		helper.Events.GameLoop.GameLaunched += OnGameLaunched;
	}

	private void OnGameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {

		var api = Helper.ModRegistry.GetApi<IBreakPintailApi>("leclair.stardew.breakpintail");
		if (api is null) {
			Monitor.Log($"Could not get BreakPintail API.", LogLevel.Error);
			return;
		}

		var thing = api.GetThingGetter();

		foreach (var entry in thing)
			Monitor.Log($"Thing: {entry.Key}: {entry.Value}", LogLevel.Info);
	}
}
