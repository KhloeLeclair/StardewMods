using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.DynamicRules;

public record struct ModFilterInfo(
	string? ModId,
	string? Prefix,
	IModInfo? Info
);

public class SourceModRuleHandler : DynamicTypeHandler<ModFilterInfo>, IOptionInputRuleHandler {

	public readonly ModEntry Mod;

	public SourceModRuleHandler(ModEntry mod) {
		Mod = mod;

		List<KeyValuePair<string, string>> mods = new();

		foreach (var other in Mod.Helper.ModRegistry.GetAll())
			mods.Add(new(other.Manifest.UniqueID, $"{other.Manifest.Name} @>@h({other.Manifest.UniqueID})"));

		mods.Sort((a,b) => a.Value.CompareTo(b.Value));

		Options = new(mods);
	}

	public override string DisplayName => I18n.Filter_Mod();

	public override string Description => I18n.Filter_Mod_About();

	public override Texture2D Texture => Game1.mouseCursors;

	public override Rectangle Source => new(420, 489, 25, 18);

	public override bool AllowMultiple => true;

	public override bool HasEditor => true;

	public Dictionary<string, string> Options { get; }

	public string HelpText => string.Empty;

	private bool isPrefixed(ModFilterInfo state, string? name) {
		return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(state.Prefix) && name.StartsWith(state.Prefix);
	}

	public override bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, ModFilterInfo state) {
		if (isPrefixed(state, recipe.Name))
			return true;

		return isPrefixed(state, item.Value?.ItemId);
	}

	public override IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData data) {
		return null;
	}

	public override IFlowNode[]? GetExtraInfo(ModFilterInfo state) {
		if (state.Info is null) {
			if (string.IsNullOrEmpty(state.ModId))
				return null;

			return FlowHelper.Builder()
				.Text(" ")
				.Text(state.ModId, shadow: false)
				.Text(" (unloaded)", shadow: false, color: Game1.textColor * 0.5f)
				.Build();
		}

		return FlowHelper.Builder()
			.Text(" ")
			.Text(state.Info.Manifest.Name, shadow: false)
			.Build();
	}

	public override ModFilterInfo ParseStateT(IDynamicRuleData type) {
		if (!type.Fields.TryGetValue("Input", out var token))
			return default;

		string? modId = (string?) token;
		if (string.IsNullOrEmpty(modId))
			return default;

		return new ModFilterInfo(
			ModId: modId,
			Prefix: string.IsNullOrEmpty(modId) ? null : $"{modId}_",
			Info: Mod.Helper.ModRegistry.Get(modId)
		);
	}

}
