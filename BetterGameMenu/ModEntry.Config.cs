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
		if (Config.ShowFakeTabs)
			RegisterFakeTabs();
		else
			UnregisterFakeTabs();
	}

	internal void SaveConfig() {
		Helper.WriteConfig(Config);
		UpdatePreferred();
	}

	[MemberNotNullWhen(true, nameof(intGMCM))]
	internal bool HasGMCM => intGMCM is not null && intGMCM.IsLoaded;

	internal bool CanOpenGMCM => HasGMCM && intGMCM.CanOpenMenu;

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

		intGMCM.Add(
			I18n.Config_Enabled,
			I18n.Config_Enabled_About,
			c => c.Enabled,
			(c, v) => c.Enabled = v
		);

		intGMCM.AddLabel(I18n.Config_Providers, I18n.Config_Providers_About, shortcut: "providers");
		intGMCM.AddLabel(I18n.Config_Advanced, I18n.Config_Advanced_About, shortcut: "advanced");

		// Providers
		intGMCM.StartPage("providers", I18n.Config_Providers);
		intGMCM.AddParagraph(I18n.Config_Providers_About);

		foreach (string tab in SortedTabs) {
			if (!Tabs.TryGetValue(tab, out var info) || !Implementations.TryGetValue(tab, out var impls))
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

		Dictionary<AllowSecondRow, Func<string>> options = new() {
			{ AllowSecondRow.Automatic, I18n.Config_SecondRow_Automatic },
			{ AllowSecondRow.Always, I18n.Config_SecondRow_Always },
			{ AllowSecondRow.Never, I18n.Config_SecondRow_Never },
		};

		intGMCM.AddChoice(
			I18n.Config_SecondRow,
			I18n.Config_SecondRow_About,
			c => c.AllowSecondRow,
			(c, v) => c.AllowSecondRow = v,
			options
		);

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
			I18n.Config_FakeTabs,
			I18n.Config_FakeTabs_About,
			c => c.ShowFakeTabs,
			(c, v) => {
				c.ShowFakeTabs = v;
				if (c.ShowFakeTabs)
					RegisterFakeTabs();
				else
					UnregisterFakeTabs();
			}
		);

		// Done!
		ConfigStale = false;

	}

}
