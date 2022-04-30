using System;

using StardewModdingAPI;

using Leclair.Stardew.BreakPintail;

namespace Leclair.Stardew.BreakPintailClient;

public class TestData {

	public float SomeValue { get; set; } = 1f;

}

public class ModEntry : Mod {

	public override void Entry(IModHelper helper) {

		Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

	}

	private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {

		var api = Helper.ModRegistry.GetApi<IBreakPintailApi>("leclair.breakpintail.host");

		api!.GetThing<TestData>().Changed += ModEntry_Changed;

	}

	private void ModEntry_Changed(object? sender, IEventData<TestData> e) {
		throw new NotImplementedException();
	}
}
