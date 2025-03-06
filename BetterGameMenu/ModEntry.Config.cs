using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Leclair.Stardew.Common.Integrations.GenericModConfigMenu;

using StardewModdingAPI;

namespace Leclair.Stardew.BetterGameMenu;

public partial class ModEntry {

	internal GMCMIntegration<ModConfig, ModEntry>? intGMCM;
	internal bool ConfigStale;

	internal void ResetConfig() {
		Config = new ModConfig();
		UpdatePreferred();
	}

	internal void SaveConfig() {
		Helper.WriteConfig(Config);
		UpdatePreferred();
	}

	[MemberNotNullWhen(true, nameof(intGMCM))]
	internal bool HasGMCM() {
		return intGMCM is not null && intGMCM.IsLoaded;
	}

	internal void OpenGMCM() {
		if (!HasGMCM())
			return;

		if (ConfigStale)
			RegisterSettings();

		intGMCM.OpenMenu();
	}

	internal void RegisterSettings() {
		intGMCM ??= new(this, () => Config, ResetConfig, SaveConfig);
		if (!intGMCM.IsLoaded)
			return;

		// Un-register and re-register so we can redo our settings.
		intGMCM.Unregister();
		intGMCM.Register(true);

		// Main Menu
		intGMCM.AddLabel(I18n.Config_Providers, I18n.Config_Providers_About, shortcut: "providers");
		intGMCM.AddLabel(I18n.Config_Advanced, I18n.Config_Advanced_About, shortcut: "advanced");

		// Providers
		intGMCM.StartPage("providers", I18n.Config_Providers);
		intGMCM.AddParagraph(I18n.Config_Providers_About);

		foreach (var (tab, info) in Tabs) {
			if (!Implementations.TryGetValue(tab, out var impls))
				continue;

			Dictionary<string, Func<string>> choices = new() {
				{ "", I18n.Config_Provider_Automatic },
				{ "disable", I18n.Config_Provider_Disable },
			};

			var impList = impls.Values.ToList();
			impList.Sort(CompareImplementations);

			foreach (var impl in impList) {
				Func<string> displayName;
				if (impl.Source == "stardew")
					displayName = I18n.Config_Provider_Stardew;

				else if (Helper.ModRegistry.Get(impl.Source) is IModInfo mod)
					displayName = () => I18n.Config_Provider_Mod(Config.DeveloperMode
						? $"{mod.Manifest.Name} ({impl.Priority})"
						: mod.Manifest.Name);

				else
					displayName = () => I18n.Config_Provider_Unknown(impl.Source);

				choices[impl.Source] = displayName;
			}

			intGMCM.AddChoice(
				info.GetDisplayName,
				null,
				c => c.PreferredImplementation.GetValueOrDefault(tab) ?? "",
				(c, v) => {
					if (string.IsNullOrEmpty(v))
						c.PreferredImplementation.Remove(tab);
					else
						c.PreferredImplementation[tab] = v;
				},
				choices: choices
			);

		}

		// Advanced
		intGMCM.StartPage("advanced", I18n.Config_Advanced);
		intGMCM.AddParagraph(I18n.Config_Advanced_About);

		intGMCM.Add(
			I18n.Config_HotSwap,
			I18n.Config_HotSwap_About,
			c => c.AllowHotSwap,
			(c, v) => c.AllowHotSwap = v
		);

		intGMCM.Add(
			I18n.Config_Developer,
			I18n.Config_Developer_About,
			c => c.DeveloperMode,
			(c, v) => c.DeveloperMode = v
		);

		intGMCM.Add(
			I18n.Config_Enabled,
			I18n.Config_Enabled_About,
			c => c.Enabled,
			(c, v) => c.Enabled = v
		);

		// Done!
		ConfigStale = false;

	}

}
