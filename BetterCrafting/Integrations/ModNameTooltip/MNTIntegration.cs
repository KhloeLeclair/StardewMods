#nullable enable

using StardewValley;

using Leclair.Stardew.Common.Integrations;

using System.Diagnostics.CodeAnalysis;
using ModNameTooltip;

namespace Leclair.Stardew.BetterCrafting.Integrations.ModNameTooltip;

public class MNTIntegration : BaseAPIIntegration<IModNameAPI, ModEntry> {

    public MNTIntegration(ModEntry mod)
    : base(mod, "mushymato.ModNameTooltip", "1.5.2") {

    }

    public bool TryGetModName(CraftingRecipe? recipe, Item? recipeItem, string recipeName, [NotNullWhen(true)] out string? modName) {
        modName = null;

        if (!IsLoaded)
            return false;

        if (!API.TryGetModName(recipe, out IModNameInfo? modNameInfo) &&
            !API.TryGetModName(recipeItem, out modNameInfo) &&
            !API.TryGetModName_FromBuildingId(recipeName, out modNameInfo))
            return false;

        // Null ModInfo means the source is 'Stardew Valley'
        // Following how BC worked previously, do not use this as source mod name
        if (modNameInfo.ModInfo == null)
            return false;

        modName = modNameInfo.ModName;
        return !string.IsNullOrEmpty(modName);
    }
}
