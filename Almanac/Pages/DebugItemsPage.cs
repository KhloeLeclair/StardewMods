using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Types;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using StardewValley;
using StardewValley.Objects;
using StardewValley.GameData;
using StardewValley.Menus;

using SObject = StardewValley.Object;

using Leclair.Stardew.Almanac.Menus;

namespace Leclair.Stardew.Almanac.Pages;

internal class DebugItemsState : BaseState {
	public string Item;
}

internal class DebugItemsPage : BasePage<DebugItemsState> {

	private Item CurrentItem;

	private readonly Cache<IEnumerable<IFlowNode>, Item> ItemInfo;
	private readonly Dictionary<string, SelectableNode> ItemNodes = new();

	#region Life Cycle

	public static DebugItemsPage GetPage(AlmanacMenu menu, ModEntry mod) {
		if (!mod.HasAlmanac(Game1.player) )// || !mod.Config.DebugMode)
			return null;

		return new(menu, mod);
	}

	public DebugItemsPage(AlmanacMenu menu, ModEntry mod) : base(menu, mod) {
		ItemInfo = new(item => BuildRightPage(item), () => CurrentItem);
	}

	public override void ThemeChanged() {
		base.ThemeChanged();

		foreach (var node in ItemNodes.Values) {
			node.SelectedTexture = Menu.background;
			node.HoverTexture = Menu.background;
		}
	}

	#endregion

	#region State Saving

	public override DebugItemsState SaveState() {
		var state = base.SaveState();

		state.Item = CurrentItem?.QualifiedItemID;

		return state;
	}

	public override void LoadState(DebugItemsState state) {
		base.LoadState(state);

		var item = string.IsNullOrEmpty(state.Item) ? null : Utility.CreateItemByID(state.Item, 1, allow_null: true);
		SelectItem(item);
	}

	#endregion

	#region Logic

	public bool SelectItem(Item item) {
		if (item == null || CurrentItem == null || item.QualifiedItemID != CurrentItem.QualifiedItemID) {
			CurrentItem = item;
			SetRightFlow(ItemInfo.Value);

			foreach(var pair in ItemNodes) {
				pair.Value.Selected = pair.Key == CurrentItem?.QualifiedItemID;
			}

			return true;
		}

		return false;
	}

	public IFlowNode[] BuildRightPage(Item item) {
		if (CurrentItem == null)
			return null;

		FlowBuilder builder = FlowHelper.Builder();

		var sprite = SpriteHelper.GetSprite(item);

		builder.Text(item.DisplayName, fancy: true, align: Alignment.Center);
		builder.Text("\n\n");

		builder
			.Sprite(sprite, 1f, Alignment.Bottom | Alignment.Center)
			.Text(" ")
			.Sprite(sprite, 2f, Alignment.Bottom)
			.Text(" ")
			.Sprite(sprite, 4f, Alignment.Bottom)
			.Text(" ")
			.Sprite(sprite, 8f, Alignment.Bottom);

		builder.Text("\n\n");

		builder
			.Text("ID: ", bold: true)
			.Text(item.QualifiedItemID, shadow: false);

		return builder.Build();
	}

	public override void Update() {
		base.Update();

		FlowBuilder builder = new();
		ItemNodes.Clear();

		foreach (var type in ItemDataDefinition.IdentifierLookup.Values) {
			builder.Text("\n").Text(type.Identifier, font: Game1.dialogueFont).Text("\n");
			foreach (string itemID in type.GetAllItemIDs()) {
				var item = Utility.CreateItemByID($"{type.Identifier}{itemID}", 1, allow_null: true);
				if (item == null)
					continue;

				FlowBuilder sb = FlowHelper.Builder()
					.Sprite(SpriteHelper.GetSprite(item), 3f);

				var node = new SelectableNode(
					sb.Build(),
					width: 72,

					onHover: (_, _, _) => {
						Menu.HoveredItem = item;
						Menu.HoverText = $"{item.DisplayName}\n{item.QualifiedItemID}";
						return true;
					},

					onClick: (_, _, _) => {
						if (SelectItem(item))
							Game1.playSound("smallSelect");
						return true;
					}
				) {
					SelectedTexture = Menu.background,
					SelectedSource = new(336, 352, 16, 16),
					HoverTexture = Menu.background,
					HoverSource = new(336, 352, 16, 16),
					HoverColor = Color.White * 0.4f
				};

				ItemNodes.Add(item.QualifiedItemID, node);
				builder.Add(node);
			}
		}

		SetLeftFlow(builder);
	}

	#endregion

	#region ITab

	public override int SortKey => 1000;
	public override string TabSimpleTooltip => "Debug: Items";
	public override Texture2D TabTexture => Game1.mouseCursors;
	public override Rectangle? TabSource => Rectangle.Empty;

	#endregion

	#region IAlmanacPage

	public override PageType Type => PageType.Blank;

	#endregion

}
