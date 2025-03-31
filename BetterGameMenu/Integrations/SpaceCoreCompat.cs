using System;

using HarmonyLib;

using Leclair.Stardew.Common.Integrations;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Integrations;

internal class SpaceCoreCompat : BaseIntegration<ModEntry> {

	public const string UNIQUE_ID = "spacechase0.SpaceCore";

	public static void MaybeAdd(ModEntry self) {
		if (!self.APIInstances.ContainsKey(UNIQUE_ID))
			new SpaceCoreCompat(self);
	}

	public SpaceCoreCompat(ModEntry self) : base(self, UNIQUE_ID, minVersion: null, maxVersion: "1.27.0") {
		try {
			Install();
		} catch (Exception ex) {
			Log($"Unable to register compatibility with SpaceCore. Some features may not work.", LogLevel.Warn);
			Log($"Details: {ex}", LogLevel.Debug);
		}
	}

	public void Install() {
		if (!IsLoaded || Self.GetApi(Other) is not ModAPI api)
			return;

		var NewSkillsPage = AccessTools.TypeByName("SpaceCore.Interface.NewSkillsPage");
		var ctor = NewSkillsPage is null ? null : AccessTools.DeclaredConstructor(NewSkillsPage, [typeof(int), typeof(int), typeof(int), typeof(int)]);
		if (ctor is null)
			throw new ArgumentNullException("Unable to find NewSkillsPage");

		IClickableMenu CreateInstance(IClickableMenu parent) {
			object? result = ctor.Invoke([parent.xPositionOnScreen, parent.yPositionOnScreen, parent.width, parent.height]);
			if (result is not IClickableMenu menu)
				throw new Exception("Unable to construct menu instance");
			return menu;
		}

		api.RegisterImplementation(
			nameof(VanillaTabOrders.Skills),
			priority: 100,
			getPageInstance: CreateInstance,
			getWidth: width => width + (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru ? 64 : 0),
			onResize: input => CreateInstance(input.Menu)
		);

		Log($"Using internal compatibility for SpaceCore.", LogLevel.Info);
	}
}
