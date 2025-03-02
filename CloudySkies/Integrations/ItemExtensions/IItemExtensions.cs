using Microsoft.Xna.Framework;

using StardewValley;
// ReSharper disable UnusedMember.Global

namespace ItemExtensions;

public interface IItemExtensionsApi {
	/// <summary>
	/// Checks for resource data with the Stone type.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	//bool IsStone(string id);

	/// <summary>
	/// Checks for resource data in the mod.
	/// </summary>
	/// <param name="id">Qualified item ID</param>
	/// <param name="health">MinutesUntilReady value</param>
	/// <param name="itemDropped">Item dropped by ore</param>
	/// <returns>Whether the object has resource data.</returns>
	bool IsResource(string id, out int? health, out string itemDropped);

	/// <summary>
	/// Checks for a qualified id in modded clump data (vanilla not included).
	/// </summary>
	/// <param name="qualifiedItemId">Qualified item ID.</param>
	/// <returns>Whether this id is a clump's.</returns>
	bool IsClump(string qualifiedItemId);

	//adding empty in the meantime
	//bool HasBehavior(string qualifiedItemId, string target);

	/// <summary>
	/// Tries to spawn a clump.
	/// </summary>
	/// <param name="itemId">The clump ID.</param>
	/// <param name="position">Tile position.</param>
	/// <param name="locationName">Location name or unique name.</param>
	/// <param name="error">Error string, if applicable.</param>
	/// <param name="avoidOverlap">Avoid overlapping with other clumps.</param>
	/// <returns>Whether spawning succeeded.</returns>
	//bool TrySpawnClump(string itemId, Vector2 position, string locationName, out string error, bool avoidOverlap = false);

	/// <summary>
	/// Tries to spawn a clump.
	/// </summary>
	/// <param name="itemId">The clump ID.</param>
	/// <param name="position">Tile position.</param>
	/// <param name="location">Location to use.</param>
	/// <param name="error">Error string, if applicable.</param>
	/// <param name="avoidOverlap">Avoid overlapping with other clumps.</param>
	/// <returns>Whether spawning succeeded.</returns>
	bool TrySpawnClump(string itemId, Vector2 position, GameLocation location, out string error, bool avoidOverlap = false);

	/// <summary>
	/// Checks custom mixed seeds.
	/// </summary>
	/// <param name="itemId">The 'main seed' ID.</param>
	/// <param name="includeSource">Include the main seed's crop in calculation.</param>
	/// <param name="parseConditions">Whether to pase GSQs before adding to list.</param>
	/// <returns>All possible seeds.</returns>
	//List<string> GetCustomSeeds(string itemId, bool includeSource, bool parseConditions = true);

	/// <summary>
	/// Does checks for a clump's drops, including monster spawning and other behavior.
	/// </summary>
	/// <param name="clump">The clump instance.</param>
	/// <param name="remove">whether to remove the clump from the map.</param>
	//void CheckClumpDrops(ResourceClump clump, bool remove = false);

	/// <summary>
	/// Does checks for a node's drops, including monster spawning and other behavior.
	/// </summary>
	/// <param name="node">The node instance.</param>
	/// <param name="remove">whether to remove the node from the map.</param>
	//void CheckObjectDrops(Object node, bool remove = false);

	/// <summary>
	/// Gets data for a specific resource.
	/// </summary>
	/// <param name="id">The ID if the resource.</param>
	/// <param name="isClump">Whether it's a clump (instead of a node).</param>
	/// <param name="data">The resource data.</param>
	/// <returns>Whether the data was found.</returns>
	//bool GetResourceData(string id, bool isClump, out object data);

	/// <summary>
	/// Gets breaking tool for a specific resource.
	/// </summary>
	/// <param name="id">The ID if the resource.</param>
	/// <param name="isClump">Whether it's a clump (instead of a node).</param>
	/// <param name="tool">The breaking tool.</param>
	/// <returns>Whether the resource data was found.</returns>
	//bool GetBreakingTool(string id, bool isClump, out string tool);
}
