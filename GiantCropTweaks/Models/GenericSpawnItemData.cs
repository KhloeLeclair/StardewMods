using System.Collections.Generic;

using Microsoft.Xna.Framework.Content;

namespace Leclair.Stardew.GiantCropTweaks.Models;

public class GenericSpawnItemData : ISpawnItemData {

	private string? IdImpl;

	[ContentSerializer(Optional = true)]
	public string Id {
		get {
			if (IdImpl is not null)
				return IdImpl;

			if (ItemId is not null) {
				if (IsRecipe)
					return $"{ItemId} (Recipe)";
				return ItemId;
			}

			if (RandomItemId is not null && RandomItemId.Count > 0) {
				string result = string.Join("|", RandomItemId);
				if (IsRecipe)
					return $"{result} (Recipe)";
				return result;
			}

			return "???";
		}
		set {
			IdImpl = value;
		}
	}

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string? Condition { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public string? ItemId { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public List<string>? RandomItemId { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int MinStack { get; set; } = -1;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int MaxStack { get; set; } = -1;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int Quality { get; set; } = -1;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public int ToolUpgradeLevel { get; set; } = -1;

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public bool IsRecipe { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public List<IQuantityModifier>? StackModifiers { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public IQuantityModifier.QuantityModifierMode? StackModifierMode { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public List<IQuantityModifier>? QualityModifiers { get; set; }

	/// <inheritdoc />
	[ContentSerializer(Optional = true)]
	public IQuantityModifier.QuantityModifierMode? QualityModifierMode { get; set; }

}
