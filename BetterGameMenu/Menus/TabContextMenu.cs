using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterGameMenu.Menus;

public class TabContextMenu : IClickableMenu {

	public const int DIVIDER_HEIGHT = 8;

	private readonly Action<Action> OnSelect;

	public readonly List<ClickableComponent> Components = [];
	public readonly List<ITabContextMenuEntry> Items = [];

	public readonly bool HasIcons;
	public readonly int ItemHeight;

	public TabContextMenu(ModEntry mod, int x, int y, IEnumerable<ITabContextMenuEntry> items, Action<Action> onSelect) : base() {
		OnSelect = onSelect;

		int max_width = 0;
		int max_height = 0;
		bool has_icon = false;

		foreach (var item in items) {
			if (item is null)
				continue;

			Items.Add(item);
			if (item.Label == "-")
				continue;

			has_icon |= item.Icon != null;

			var size = Game1.smallFont.MeasureString(item.Label);
			if (size.X > max_width)
				max_width = (int) size.X;

			if (size.Y > max_height)
				max_height = (int) size.Y;
		}

		HasIcons = has_icon;
		ItemHeight = max_height;

		if (has_icon)
			max_width += 36;

		int total_height = 0;
		int i = 0;

		foreach (var item in Items) {

			bool is_divider = item.Label == "-";

			var cmp = new ClickableComponent(
				bounds: new Rectangle(
					x, y + total_height - 2, max_width,
					is_divider ? DIVIDER_HEIGHT : max_height + 4
				), "", item.Label
			) {
				myID = i,
				upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
			};

			if (is_divider)
				cmp.visible = false;

			Components.Add(cmp);
			i++;

			//if (total_height != 0)
			total_height += 4;

			total_height += is_divider ? DIVIDER_HEIGHT : max_height;
		}

		initialize(x, y, max_width, total_height);

		if (Game1.options.SnappyMenus)
			snapToDefaultClickableComponent();
	}

	public override void snapToDefaultClickableComponent() {
		currentlySnappedComponent = Components.FirstOrDefault(x => x.label != "-");
		if (currentlySnappedComponent != null)
			snapCursorToCurrentSnappedComponent();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true) {
		base.receiveLeftClick(x, y, playSound);

		for (int i = 0; i < Components.Count; i++) {
			var cmp = Components[i];
			if (cmp.visible && cmp.containsPoint(x, y)) {
				var item = Items[i];
				if (item.OnSelect is not null)
					OnSelect(item.OnSelect);

				Game1.playSound("smallSelect");
				exitThisMenu(playSound: false);
				break;
			}
		}

		if (x < xPositionOnScreen || x > (xPositionOnScreen + width) || y < yPositionOnScreen || y > (yPositionOnScreen + height))
			exitThisMenu();
	}

	public override void performHoverAction(int x, int y) {
		base.performHoverAction(x, y);

		foreach (var cmp in Components) {
			if (cmp.containsPoint(x, y)) {
				cmp.scale = 1f;
			} else {
				cmp.scale = 0f;
			}
		}
	}


	public override void draw(SpriteBatch batch) {
		// Dim the Background
		batch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

		// Background
		RenderHelper.DrawBox(
			batch,
			texture: Game1.menuTexture,
			sourceRect: new Rectangle(0, 256, 60, 60),
			x: xPositionOnScreen - 16,
			y: yPositionOnScreen - 16,
			width: width + 32,
			height: height + 32,
			color: Color.White,
			scale: 1f
		);

		// Draw each Item
		int y = yPositionOnScreen;

		for (int i = 0; i < Items.Count; i++) {
			var item = Items[i];
			var cmp = Components[i];
			int x = xPositionOnScreen;

			if (item.Label == "-") {
				y += DIVIDER_HEIGHT + 4;
				continue;
			}

			if (cmp.scale > 0 && item.OnSelect != null)
				batch.Draw(Game1.fadeToBlackRect, cmp.bounds, Color.Wheat * 0.5f);


			if (HasIcons) {
				item.Icon?.Invoke(batch, new Rectangle(x, y, 32, 32));
				x += 36;
			}

			batch.DrawString(Game1.smallFont, item.Label, new Vector2(x, y), Game1.textColor);

			y += ItemHeight + 4;

		}

		// Base Menu
		base.draw(batch);

		drawMouse(batch);
	}

}
