using System;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common.Crafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.DynamicRules;

public class SingleItemRuleHandler : IDynamicRuleHandler {

	public readonly int ItemId;
	public readonly Lazy<Item> Item;

	public SingleItemRuleHandler(int itemId) {
		ItemId = itemId;
		Item = new Lazy<Item>(() => new SObject(ItemId, 1));
	}

	public string DisplayName => I18n.Filter_Buff(Item.Value.DisplayName);
	public string Description => I18n.Filter_Buff_About(Item.Value.DisplayName);

	public Texture2D Texture => Game1.objectSpriteSheet;

	public Rectangle Source => Game1.getSourceRectForStandardTileSheet(Texture, Item.Value.ParentSheetIndex, 16, 16);

	public bool AllowMultiple => false;

	public bool HasEditor => false;

	public IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData type) => null;

	public object? ParseState(IDynamicRuleData type) {
		return null;
	}

	public bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state) {
		return item.Value is not null && Item.Value.canStackWith(item.Value);
	}
}
