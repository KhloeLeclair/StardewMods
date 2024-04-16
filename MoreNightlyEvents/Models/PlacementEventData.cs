
using System.Collections.Generic;

using Leclair.Stardew.Common.Serialization.Converters;

using Microsoft.Xna.Framework;

using StardewValley.GameData;

namespace Leclair.Stardew.MoreNightlyEvents.Models;

[DiscriminatedType("Placement")]
public class PlacementEventData : BaseEventData {

	public string? SoundName { get; set; }

	public int MessageDelay { get; set; } = 7000;

	public string? Message { get; set; }

	public List<PlacementItemData>? Output { get; set; }

}


public class PlacementItemData : GenericSpawnItemDataWithCondition {

	// Used for everything, to determine the bounds that they can spawn within.
	public Rectangle? SpawnArea { get; set; }

	// What type of placeable thing is it?
	public PlaceableType Type { get; set; } = PlaceableType.Item;

	// Item Fields

	public List<GenericSpawnItemDataWithCondition>? Contents { get; set; }


	// Tree Fields

	public int GrowthStage { get; set; } = 4;

	public int InitialFruit { get; set; } = 0;

	public IgnoreSeasonsMode IgnoreSeasons { get; set; } = IgnoreSeasonsMode.Never;

	// Resource Clump Fields
	public int ClumpId { get; set; } = -1;
	public bool ClumpStrictPlacement { get; set; } = false;

	public int ClumpWidth { get; set; } = 2;
	public int ClumpHeight { get; set; } = 2;
	public int? ClumpHealth { get; set; }
	public string? ClumpTexture { get; set; }

}


public enum IgnoreSeasonsMode {
	Never,
	DuringSpawn,
	Always
}


public enum PlaceableType {
	Item,
	WildTree,
	FruitTree,
	GiantCrop,
	ResourceClump
}
