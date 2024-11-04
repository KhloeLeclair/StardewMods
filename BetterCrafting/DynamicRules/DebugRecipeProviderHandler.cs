using System;
using System.Collections.Generic;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.DynamicRules;

public record struct ProviderFilterInfo(
	string? ProviderType
);

public class DebugRecipeProviderHandler : DynamicTypeHandler<ProviderFilterInfo>, IOptionInputRuleHandler {

	public readonly ModEntry Mod;

	public DebugRecipeProviderHandler(ModEntry mod) {
		Mod = mod;
	}

	public override string DisplayName => I18n.Filter_Provider();

	public override string Description => I18n.Filter_Provider_About();

	public override Texture2D Texture => Game1.mouseCursors;

	public override Rectangle Source => new(420, 489, 25, 18);

	public override bool AllowMultiple => true;

	public override bool HasEditor => true;

	public IEnumerable<KeyValuePair<string, string>> GetOptions(bool cooking) {
		List<KeyValuePair<string, string>> providers = new();

		Dictionary<string, int> itemCount = new();

		foreach (var provider in Mod.Recipes.GetRecipeProviders()) {
			string? full = provider.GetType().FullName;
			if (string.IsNullOrEmpty(full))
				continue;

			string name = full; // provider.GetType().Name;
			string? source = Mod.Recipes.GetProviderMod(provider);

			if (!string.IsNullOrEmpty(source) && Mod.TryUnproxy(provider, out object? unproxied, silent: true) && unproxied != null) {
				name = unproxied.GetType().FullName ?? unproxied.GetType().Name;
			}

			providers.Add(new(full, string.IsNullOrEmpty(source) ? name : $"{name} @>@h({source})"));
			itemCount[full] = 0;
		}

		foreach (var other in Mod.Helper.ModRegistry.GetAll())
			itemCount[other.Manifest.UniqueID] = 0;

		bool TryCount(IRecipe recipe) {
			var provider = Mod.Recipes.GetProvider(recipe);
			if (provider == null)
				return false;

			string? full = provider.GetType().FullName;
			if (!string.IsNullOrEmpty(full) && itemCount.TryGetValue(full, out int count)) {
				itemCount[full] = count + 1;
				return true;
			}

			return false;
		}

		foreach (var recipe in Mod.Recipes.GetRecipes(cooking))
			TryCount(recipe);

		for (int i = 0; i < providers.Count; i++) {
			var entry = providers[i];
			int count = itemCount.GetValueOrDefault(entry.Key);
			if (count > 0)
				providers[i] = new(entry.Key, entry.Value + $"\n@<{count}");
		}

		providers.Sort((a, b) => {
			int aCount = itemCount.GetValueOrDefault(a.Key);
			int bCount = itemCount.GetValueOrDefault(b.Key);

			if (aCount != 0 && bCount == 0)
				return -1;
			if (aCount == 0 && bCount != 0)
				return 1;

			return a.Value.CompareTo(b.Value);
		});

		return providers;
	}

	public string HelpText => string.Empty;

	public override bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, ProviderFilterInfo state) {
		return !string.IsNullOrEmpty(state.ProviderType) && Mod.Recipes.GetProvider(recipe)?.GetType()?.FullName == state.ProviderType;
	}

	public override IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData data) {
		return null;
	}

	public override IFlowNode[]? GetExtraInfo(ProviderFilterInfo state) {
		return FlowHelper.Builder()
			.Text(" ")
			.Text(state.ProviderType ?? string.Empty, shadow: false)
			.Build();
	}

	public override ProviderFilterInfo ParseStateT(IDynamicRuleData type) {
		if (!type.Fields.TryGetValue("Input", out var token))
			return default;

		string? providerType = (string?) token;
		if (string.IsNullOrEmpty(providerType))
			return default;

		return new ProviderFilterInfo(
			ProviderType: providerType
		);
	}

}
