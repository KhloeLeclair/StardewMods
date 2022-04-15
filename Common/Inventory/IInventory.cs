#nullable enable

using System.Collections.Generic;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Network;

namespace Leclair.Stardew.Common.Inventory;

public interface IInventory {

	/// <summary>
	/// The object that has inventory.
	/// </summary>
	object Object { get; }

	/// <summary>
	/// Where this object is located, if a location is relevant.
	/// </summary>
	GameLocation? Location { get; }

	/// <summary>
	/// The player accessing the inventory, if a player is involved.
	/// </summary>
	Farmer? Player { get; }

	/// <summary>
	/// The NetMutex for this object, which should be locked before
	/// using it. If there is no mutex, then we apparently don't
	/// need to worry about that.
	/// </summary>
	NetMutex? Mutex { get; }

	/// <summary>
	/// Whether or not the object is locked and ready for read/write usage.
	/// </summary>
	bool IsLocked();

	/// <summary>
	/// Whether or not the object is a valid inventory.
	/// </summary>
	bool IsValid();

	/// <summary>
	/// Whether or not we can insert items into this inventory.
	/// </summary>
	bool CanInsertItems();

	/// <summary>
	/// Whether or not we can extract items from this inventory.
	/// </summary>
	bool CanExtractItems();

	/// <summary>
	/// For multi-tile inventories, the region that this inventory takes
	/// up in the world. Only rectangular multi-tile inventories are
	/// supported, and this is used primarily for discovering connections.
	/// </summary>
	Rectangle? GetMultiTileRegion();

	/// <summary>
	/// Get the tile position of this object in the world, if it has one.
	/// For multi-tile inventories, this should be the primary tile if
	/// one exists.
	/// </summary>
	Vector2? GetTilePosition();

	/// <summary>
	/// Get this object's inventory as a list of items. May be null if
	/// there is an issue accessing the object's inventory.
	/// </summary>
	IList<Item?>? GetItems();

	/// <summary>
	/// Attempt to clean the object's inventory. This should remove null
	/// entries, and run any other necessary logic.
	/// </summary>
	void CleanInventory();

	/// <summary>
	/// Get the number of item slots in the object's inventory. When adding
	/// items to the inventory, we will never extend the list beyond this
	/// number of entries.
	/// </summary>
	int GetActualCapacity();
}
