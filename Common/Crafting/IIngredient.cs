#nullable enable

using System.Collections.Generic;

using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.Common.Crafting;

public interface IIngredient {

	/// <summary>
	/// Whether or not this <c>IIngredient</c> supports quality control
	/// options, including using low quality first and limiting the maximum
	/// quality to use.
	/// </summary>
	bool SupportsQuality { get; }

	// Display
	/// <summary>
	/// The name of this ingredient to be displayed in the menu.
	/// </summary>
	string DisplayName { get; }

	/// <summary>
	/// The texture to use when drawing this ingredient in the menu.
	/// </summary>
	Texture2D Texture { get; }

	/// <summary>
	/// The source rectangle to use when drawing this ingredient in the menu.
	/// </summary>
	Rectangle SourceRectangle { get; }

	#region Quantity

	/// <summary>
	/// The amount of this ingredient required to perform a craft.
	/// </summary>
	int Quantity { get; }

	/// <summary>
	/// Determine how much of this ingredient is available for crafting both
	/// in the player's inventory and in the other inventories.
	/// </summary>
	/// <param name="who">The farmer performing the craft</param>
	/// <param name="items">A list of all available <see cref="Item"/>s across
	/// all available <see cref="IInventory"/> instances. If you only support
	/// consuming ingredients from certain <c>IInventory</c> types, you should
	/// not use this value and instead iterate over the inventories. Please
	/// note that this does <b>not</b> include the player's inventory.</param>
	/// <param name="inventories">All the available inventories.</param>
	/// <param name="maxQuality">The maximum item quality we are allowed to
	/// count. This cannot be ignored unless <see cref="SupportsQuality"/>
	/// returns <c>false</c>.</param>
	int GetAvailableQuantity(Farmer who, IList<Item?> items, IList<IInventory> inventories, int maxQuality);

	#endregion

	#region Consumption

	/// <summary>
	/// Consume this ingredient out of the player's inventory and the other
	/// available inventories.
	/// </summary>
	/// <param name="who">The farmer performing the craft</param>
	/// <param name="inventories">All the available inventories.</param>
	/// <param name="maxQuality">The maximum item quality we are allowed to
	/// count. This cannot be ignored unless <see cref="SupportsQuality"/>
	/// returns <c>false</c>.</param>
	/// <param name="lowQualityFirst">Whether or not we should make an effort
	/// to consume lower quality ingredients before ocnsuming higher quality
	/// ingredients.</param>
	void Consume(Farmer who, IList<IInventory> inventories, int maxQuality, bool lowQualityFirst);

	#endregion

}
