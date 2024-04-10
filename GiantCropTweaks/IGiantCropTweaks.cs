#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.GiantCropTweaks;

/// <summary>
/// A custom giant crop that may spawn in-game. This implementation is based
/// upon the new custom giant crops being added in 1.6, and once 1.6 is
/// released Giant Crop Tweaks will begin using the base game's data format
/// rather than its own implementation of the same.
/// </summary>
public interface IGiantCropData {

	/// <summary>
	/// The qualified or unqualified item ID from which this giant crop can grow. If multiple giant crops can grow from a single crop, the first one whose <see cref="Chance"/> and <see cref="Condition" /> match will be selected.
	/// </summary>
	string FromItemId { get; set; }

	/// <summary>
	/// The items to produce when this giant crop is broken. All matching items will be produced.
	/// </summary>
	List<IGiantCropHarvestItemData> HarvestItems { get; set; }

	/// <summary>
	/// The asset name for the texture containing the giant crop's sprite.
	/// </summary>
	string Texture { get; set; }

	/// <summary>
	/// The top-left pixel position of the sprite within the <see cref="Texture"/>, specified as a model with X and Y fields. Defaults to (0, 0).
	/// </summary>
	Point TexturePosition { get; set; }

	/// <summary>
	/// The area in tiles occupied by the giant crop, specified as a model with X and Y fields. This affects both its sprite size (which should be 16 pixels per tile) and the grid of crops needed for it to grow. Note that giant crops are drawn with an extra tile's height. Defaults to (3, 3).
	/// </summary>
	Point TileSize { get; set; }

	/// <summary>
	/// The health points that must be depleted to break the giant crop. The number of points depleted per axe chop depends on the axe power level.
	/// </summary>
	int Health { get; set; }

	/// <summary>
	/// The percentage chance a given grid of crops will grow into the giant crop each night, as a value between 0 (never) and 1 (always). Default 0.01.
	/// </summary>
	double Chance { get; set; }

	/// <summary>
	/// A game state query which indicates whether the giant crop can be selected. Defaults to always enabled.
	/// </summary>
	string? Condition { get; set; }

}

public interface IGiantCropHarvestItemData : ISpawnItemData {

	/// <summary>
	/// The probability that the item will be produced, as a value between 0 (never) and 1 (always).
	/// </summary>
	float Chance { get; set; }

	/// <summary>
	/// Whether to drop this item only for the Shaving enchantment (true), only when the giant crop is broken (false), or both (null).
	/// </summary>
	bool? ForShavingEnchantment { get; set; }

}


public interface ISpawnItemData {

	/// <summary>
	/// A game state query which indicates whether the item should be added. Defaults to always enabled.
	/// </summary>
	string? Condition { get; set; }

	/// <summary>
	/// The item(s) to create.
	/// </summary>
	string? ItemId { get; set; }

	/// <summary>
	/// A list of random item IDs to choose from, using the same format as <see cref="ItemId"/>.
	/// </summary>
	List<string>? RandomItemId { get; set; }

	/// <summary>
	/// The minimum stack size for the item to create, or <c>-1</c> to keep the default value.
	/// </summary>
	int MinStack { get; set; }

	/// <summary>
	/// The maximum stack size for the item to create, or <c>-1</c> to match <see cref="MinStack"/>.
	/// </summary>
	int MaxStack { get; set; }

	/// <summary>
	/// The quality of the item to create. One of <c>0</c> (normal), <c>1</c> (silver), <c>2</c> (gold), or <c>4</c> (iridium).
	/// </summary>
	int Quality { get; set; }

	/// <summary>
	/// For tool items only, the initial upgrade level, or <c>-1</c> to keep the default value.
	/// </summary>
	int ToolUpgradeLevel { get; set; }

	/// <summary>
	/// Whether to add the crafting/cooking recipe for the item, instead of the item itself.
	/// </summary>
	bool IsRecipe { get; set; }

	/// <summary>
	/// Changes to apply to the result of <see cref="MinStack"/> and <see cref="MaxStack"/>.
	/// </summary>
	List<IQuantityModifier>? StackModifiers { get; set; }

	/// <summary>
	/// How multiple <see cref="StackModifiers"/> should be combined.
	/// </summary>
	IQuantityModifier.QuantityModifierMode? StackModifierMode { get; set; }

	/// <summary>
	/// Changes to apply to the <see cref="Quality"/>.
	/// </summary>
	List<IQuantityModifier>? QualityModifiers { get; set; }

	/// <summary>
	/// How multiple <see cref="QualityModifiers"/> should be combined.
	/// </summary>
	IQuantityModifier.QuantityModifierMode? QualityModifierMode { get; set; }

}

public interface IQuantityModifier {

	public enum ModificationType {
		Add,
		Subtract,
		Multiply,
		Divide,
		Set
	}

	public enum QuantityModifierMode {
		Stack,
		Minimum,
		Maximum
	}

	/// <summary>
	/// An ID for this modifier. This only needs to be unique within the current modifier list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ModifierName</c>.
	/// </summary>
	string Id { get; set; }

	/// <summary>
	/// A game state query which indicates whether this change should be applied. Item-only tokens are valid for this check, and will check the input (not output) item. Defaults to always true.
	/// </summary>
	string? Condition { get; set; }

	/// <summary>
	/// The type of change to apply.
	/// </summary>
	ModificationType Modification { get; set; }

	/// <summary>
	/// The operand to apply to the target value (e.g. the multiplier if <see cref="Modification"/> is set to <see cref="ModificationType.Multiply"/>).
	/// </summary>
	float? Amount { get; set; }

	/// <summary>
	/// A list of random amounts to choose from, using the same format as <see cref="Amount"/>.
	/// </summary>
	List<float>? RandomAmount { get; set; }

}


public interface IGiantCropTweaks {

	/// <summary>
	/// A dictionary of giant crop data. This is an easy to read version of the
	/// game's <c>"Data\GiantCrops"</c> asset.
	/// </summary>
	IReadOnlyDictionary<string, IGiantCropData> GiantCrops { get; }

	/// <summary>
	/// Try to get a giant crop's texture.
	/// </summary>
	/// <param name="id">The ID of the giant crop.</param>
	bool TryGetTexture(string id, [NotNullWhen(true)] out Texture2D? texture);

	/// <summary>
	/// Try to get a giant crop's source rectangle.
	/// </summary>
	/// <param name="id">The ID of the giant crop.</param>
	bool TryGetSource(string id, [NotNullWhen(true)] out Rectangle? source);

}
