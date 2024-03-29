#nullable enable

using System;
using System.Collections.Generic;

using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Inventory;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Models;

public enum CurrencyType {
	Money,
	FestivalPoints,
	ClubCoins,
	QiGems
};

public class CurrencyIngredient : IIngredient, IRecyclable {

	public readonly CurrencyType Type;

	public bool SupportsQuality => true;

	public CurrencyIngredient(CurrencyType type, int quantity) {
		Type = type;
		Quantity = quantity;
	}

	#region IRecyclable

	public Texture2D GetRecycleTexture(Farmer who, Item? recycledItem, bool fuzzyItems) {
		return Texture;
	}

	public Rectangle GetRecycleSourceRect(Farmer who, Item? recycledItem, bool fuzzyItems) {
		return SourceRectangle;
	}

	public string GetRecycleDisplayName(Farmer who, Item? recycledItem, bool fuzzyItems) {
		return DisplayName;
	}

	public int GetRecycleQuantity(Farmer who, Item? recycledItem, bool fuzzyItems) {
		return Quantity;
	}

	public bool CanRecycle(Farmer who, Item? recycledItem, bool fuzzyItems) {
		switch (Type) {
			case CurrencyType.Money:
			case CurrencyType.FestivalPoints:
			case CurrencyType.ClubCoins:
			case CurrencyType.QiGems:
				return true;
		}

		return false;
	}

	public IEnumerable<Item>? Recycle(Farmer who, Item? recycledItem, bool fuzzyItems) {
		switch (Type) {
			case CurrencyType.Money:
				who.Money += Quantity;
				break;
			case CurrencyType.FestivalPoints:
				who.festivalScore += Quantity;
				break;
			case CurrencyType.ClubCoins:
				who.clubCoins += Quantity;
				break;
			case CurrencyType.QiGems:
				who.QiGems += Quantity;
				break;
		}

		return null;
	}

	#endregion

	public string DisplayName {
		get {
			switch (Type) {
				case CurrencyType.Money:
					return I18n.Currency_Gold();
				case CurrencyType.FestivalPoints:
					return I18n.Currency_FestivalPoints();
				case CurrencyType.ClubCoins:
					return I18n.Currency_ClubCoins();
				case CurrencyType.QiGems:
					return I18n.Currency_QiGems();
				default:
					return "???";
			}
		}
	}

	public Texture2D Texture => Game1.mouseCursors;
	public Rectangle SourceRectangle {
		get {
			switch(Type) {
				case CurrencyType.Money:
					return new Rectangle(193, 373, 9, 10);
				case CurrencyType.FestivalPoints:
					return new Rectangle(202, 373, 9, 10);
				case CurrencyType.ClubCoins:
					return new Rectangle(211, 373, 9, 10);
			}

			return Rectangle.Empty;
		}
	}

	public int Quantity { get; }

	public void Consume(Farmer who, IList<IInventory>? inventories, int max_quality, bool low_quality_first) {
		switch (Type) {
			case CurrencyType.Money:
				who.Money -= Quantity;
				break;
			case CurrencyType.FestivalPoints:
				who.festivalScore -= Quantity;
				break;
			case CurrencyType.ClubCoins:
				who.clubCoins -= Quantity;
				break;
			case CurrencyType.QiGems:
				who.QiGems -= Quantity;
				break;
		}
	}

	public int GetAvailableQuantity(Farmer who, IList<Item?>? items, IList<IInventory>? inventories, int max_quality) {
		switch (Type) {
			case CurrencyType.Money:
				return who.Money;
			case CurrencyType.FestivalPoints:
				return who.festivalScore;
			case CurrencyType.ClubCoins:
				return who.clubCoins;
			case CurrencyType.QiGems:
				return who.QiGems;
		}

		return 0;
	}
}
