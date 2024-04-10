using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.GiantCropTweaks.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json;

namespace Leclair.Stardew.GiantCropTweaks.Models;

/// <inheritdoc />
public class GiantCrops : IGiantCropData {

	/// <inheritdoc />
	public string ID { get; set; } = string.Empty;

	/// <inheritdoc />
	public string FromItemId { get; set; } = string.Empty;

	/// <inheritdoc />
	[JsonConverter(typeof(AbstractListConverter<GiantCropHarvestItemData, IGiantCropHarvestItemData>))]
	public List<IGiantCropHarvestItemData> HarvestItems { get; set; } = new();

	/// <inheritdoc />
	public string Texture { get; set; } = string.Empty;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public Point TexturePosition { get; set; } = Point.Zero;

	[JsonIgnore]
	[Obsolete("Use TexturePosition instead.")]
	public Point Corner {
		get => TexturePosition;
		set => TexturePosition = value;
	}

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public Point TileSize { get; set; } = new Point(3, 3);

	/// <inheritdoc />
	public int Health { get; set; } = 3;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public double Chance { get; set; } = 0.01d;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string? Condition { get; set; }

	[Obsolete("Use HarvestItems instead.")]
	public string? HarvestedItemId {
		get {
			var data = HarvestItems.FirstOrDefault();
			return data?.ItemId;
		}
		set {
			var data = HarvestItems.FirstOrDefault();
			if (data is null) {
				data = new GiantCropHarvestItemData() {
					ItemId = value
				};
				HarvestItems.Add(data);
			} else
				data.ItemId = value;
		}
	}

	public bool ShouldSerializeHarvestedItemId() => false;

	/// <inheritdoc />
	[Obsolete("Use HarvestItems instead.")]
	public int MinYields {
		get {
			var data = HarvestItems.FirstOrDefault();
			return data?.MinStack ?? -1;
		}
		set {
			var data = HarvestItems.FirstOrDefault();
			if (data is null) {
				data = new GiantCropHarvestItemData() {
					MinStack = value
				};
				HarvestItems.Add(data);
			} else
				data.MinStack = value;
		}
	}

	public bool ShouldSerializeMinYields() => false;

	[Obsolete("Use HarvestItems instead.")]
	public int MaxYields {
		get {
			var data = HarvestItems.FirstOrDefault();
			return data?.MaxStack ?? -1;
		}
		set {
			var data = HarvestItems.FirstOrDefault();
			if (data is null) {
				data = new GiantCropHarvestItemData() {
					MaxStack = value
				};
				HarvestItems.Add(data);
			} else
				data.MaxStack = value;
		}
	}

	public bool ShouldSerializeMaxYields() => false;

}
