using System;

using HarmonyLib;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Integrations;

using StardewModdingAPI;

using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Integrations;
internal class GenericModConfigMenuCompat : BaseIntegration<ModEntry> {

	public const string UNIQUE_ID = "spacechase0.GenericModConfigMenu";

	public static void MaybeAdd(ModEntry self) {
		if (!self.APIInstances.ContainsKey(UNIQUE_ID))
			new GenericModConfigMenuCompat(self);
	}

	public GenericModConfigMenuCompat(ModEntry self) : base(self, UNIQUE_ID, minVersion: null, maxVersion: "1.14.1") {
		try {
			Install();
		} catch (Exception ex) {
			Log($"Unable to register compatibility with Generic Mod Config Menu. Some features may not work.", LogLevel.Warn);
			Log($"Details: {ex}", LogLevel.Debug);
		}
	}

	public void Install() {
		if (!IsLoaded || Self.GetApi(Other) is not ModAPI api)
			return;

		var tMod = AccessTools.TypeByName("GenericModConfigMenu.Mod");
		var fInstance = tMod is null ? null : AccessTools.Field(tMod, "instance");
		var mOpen = tMod is null ? null : AccessTools.Method(tMod, "OpenListMenuNew", [typeof(int?)]);
		var mGetHelper = tMod is null ? null : AccessTools.PropertyGetter(tMod, "Helper");

		if (fInstance is null || mOpen is null || mGetHelper is null)
			throw new ArgumentNullException("Unable to find GMCM.");

		var getInstance = fInstance.CreateGetter<object>();
		if (getInstance() is not object instance)
			throw new ArgumentNullException("Unable to get instance.");

		var getHelper = mGetHelper.CreateFunc<object, IModHelper>();
		if (getHelper(instance) is not IModHelper helper)
			throw new ArgumentNullException("Unable to get helper.");

		var openMenu = mOpen.CreateAction<object, int?>();
		void OpenMenu() {
			openMenu(instance, null);
		}

		api.OnTabContextMenu(evt => {
			if (evt.Tab == nameof(VanillaTabOrders.Options))
				evt.Entries.Add(evt.CreateEntry(helper.Translation.Get("button.mod-options"), OpenMenu));
		});

		api.OnPageCreated(evt => {
			if (evt.Tab == nameof(VanillaTabOrders.Options) && evt.Page is OptionsPage page)
				page.options.Add(new OptionsButton(helper.Translation.Get("button.mod-options"), OpenMenu));
		});

		Log($"Using internal compatibility for Generic Mod Config Menu.", LogLevel.Info);
	}

}
