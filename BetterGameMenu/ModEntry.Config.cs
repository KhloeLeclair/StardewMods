using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Leclair.Stardew.BetterGameMenu.Models;
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
	internal bool HasGMCM =>intGMCM is not null && intGMCM.IsLoaded;

	internal bool CanOpenGMCM => HasGMCM && intGMCM.CanOpenMenu;
	internal bool CanOpenGMCMList => HasGMCM && intGMCM.CanOpenListMenu;

	internal void OpenGMCMList() {
		if (!HasGMCM || !intGMCM.CanOpenListMenu)
			return;

		if (ConfigStale)
			RegisterSettings();

		intGMCM.OpenListMenu();
	}

	internal void OpenGMCM() {
		if (!HasGMCM || !intGMCM.CanOpenMenu)
			return;

		if (ConfigStale)
			RegisterSettings();

		intGMCM.OpenMenu();
	}

	internal Func<string> GetDisplayName(TabImplementationDefinition definition, bool allowPriority = true) {
		if (definition.Source == "stardew")
			return I18n.Config_Provider_Stardew;
		else if (Helper.ModRegistry.Get(definition.Source) is IModInfo modInfo)
			return () => I18n.Config_Provider_Mod(allowPriority && Config.DeveloperMode ? $"{modInfo.Manifest.Name} ({definition.Priority})" : modInfo.Manifest.Name);
		return () => I18n.Config_Provider_Unknown(definition.Source);
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

			var impList = impls.Values.ToList();
			impList.Sort(CompareImplementations);

			var first = impList.FirstOrDefault();
			var firstFn = first is null ? () => I18n.Config_Provider_Unknown("?") : GetDisplayName(first, false);

			Dictionary<string, Func<string>> choices = new() {
				{ "", () => I18n.Config_Provider_Automatic(firstFn()) },
				{ "disable", I18n.Config_Provider_Disable },
			};

			foreach (var impl in impList) {
				choices[impl.Source] = GetDisplayName(impl);
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
