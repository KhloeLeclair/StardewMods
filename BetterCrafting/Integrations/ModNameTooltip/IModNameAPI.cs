using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;

namespace ModNameTooltip;

public interface IModNameInfo {
    /// <summary>The mod's unique id</summary>
    string ModId { get; }

    /// <summary>The mod info, if this entry matches a real mod</summary>
    IModInfo? ModInfo { get; }

    /// <summary>The mod's name, derived from either the manifest or the special translation asset</summary>
    string ModName { get; }

    /// <summary>Display color for this mod's name</summary>
    Color ModNameColor { get; }
}

public interface IModNameAPI {
    /// <summary>
    /// Try and get info about which mod added an item using a real item instance.
    /// Supports all vanilla item types, but not mod added item definitions.
    /// </summary>
    /// <param name="item">Item to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName(Item? item, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a recipe using a real recipe instance.
    /// Supports crafting and cooking recipes.
    /// </summary>
    /// <param name="recipe">The crafting recipe to find mod name for</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns></returns>
    bool TryGetModName(CraftingRecipe? recipe, [NotNullWhen(true)] out IModNameInfo? modName);

    /// <summary>
    /// Try and get info about which mod added a building using the building id
    /// </summary>
    /// <param name="buildingId">The building id</param>
    /// <param name="modName">A <see cref="IModNameInfo"/> record containing info about the mod.</param>
    /// <returns>True if the mod is found</returns>
    bool TryGetModName_FromBuildingId(string buildingId, [NotNullWhen(true)] out IModNameInfo? modName);
}
