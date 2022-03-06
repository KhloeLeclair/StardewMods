using System;
using System.Collections.Generic;

using Leclair.Stardew.BetterCrafting.Models;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Enums;
using Leclair.Stardew.Common.Events;
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.FlowNode;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.BetterCrafting.Menus {
	public class IconPicker : MenuSubscriber<ModEntry> {


		public readonly Action<CategoryIcon> onPick;

		public List<ClickableComponent> FlowComponents;

		private IEnumerable<IFlowNode> Flow;
		private CachedFlow CachedFlow;

		private int FlowOffset;
		private int MaxFlowOffset;

		private ClickableTextureComponent btnPageUp;
		private ClickableTextureComponent btnPageDown;

		public IconPicker(ModEntry mod, int x, int y, int width, int height, Action<CategoryIcon> onPick)
		: base(mod) {

			FlowComponents = new();
			this.onPick = onPick;

			var builder = FlowHelper.Builder();

			for(int i = 0; i < 17; i++) {
				Rectangle rect = new(10 * i, 428, 10, 10);
				SpriteInfo sprite = new(Game1.mouseCursors, rect);

				builder.Sprite(sprite, scale: 3, onClick: slice => {
					Pick(GameTexture.MouseCursors, rect);
					return true;
				});
			}

			for(int iy = 0; iy < 5; iy++) {
				for(int ix = 0; ix < 6; ix++) {
					Rectangle rect = new(ix * 16, 624 + iy * 16, 16, 16);
					SpriteInfo sprite = new(Game1.mouseCursors, rect);

					builder.Sprite(sprite, scale: 3, onClick: slice => {
						Pick(GameTexture.MouseCursors, rect);
						return true;
					});
				}
			}

			for (int iy = 0; iy < 14; iy++) {
				for (int ix = 0; ix < 14; ix++) {
					Rectangle rect = new(ix * 9, iy * 9, 9, 9);
					SpriteInfo sprite = new(SpriteHelper.GetTexture(GameTexture.Emoji), rect);

					builder.Sprite(sprite, scale: 3, onClick: slice => {
						Pick(GameTexture.Emoji, rect);
						return true;
					});
				}
			}


			Flow = builder.Build();

			CachedFlow = FlowHelper.CalculateFlow(
				Flow,
				width - 64,
				Game1.smallFont
			);

			if (CachedFlow.Height + 32 < height)
				height = (int) CachedFlow.Height + 32;

			MaxFlowOffset = FlowHelper.GetMaximumScrollOffset(
				CachedFlow,
				height - 48
			);

			initialize(x, y, width, height);

			btnPageUp = new ClickableTextureComponent(
				new Rectangle(
					xPositionOnScreen + width - 64,
					yPositionOnScreen + 16,
					64, 64
				),
				Game1.mouseCursors,
				new Rectangle(64, 64, 64, 64),
				.8f
			) {
				myID = 1,
				upNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				downNeighborID = 2,
				rightNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				leftNeighborID = ClickableComponent.SNAP_AUTOMATIC
			};

			btnPageDown = new ClickableTextureComponent(
				new Rectangle(
					xPositionOnScreen + width - 64,
					yPositionOnScreen + height - 64,
					64, 64
				),
				Game1.mouseCursors,
				new Rectangle(0, 64, 64, 64),
				.8f
			) {
				myID = 2,
				upNeighborID = 1,
				downNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				leftNeighborID = ClickableComponent.SNAP_AUTOMATIC,
				rightNeighborID = ClickableComponent.SNAP_AUTOMATIC
			};

			UpdateFlowComponents();

			if (Game1.options.SnappyMenus)
				snapToDefaultClickableComponent();
		}

		private void Pick(GameTexture texture, Rectangle source) {
			onPick(new() {
				Type = CategoryIcon.IconType.Texture,
				Source = texture,
				Rect = source
			});

			exitThisMenu();
		}

		private void UpdateFlowComponents() {
			FlowHelper.UpdateComponentsForFlow(
				CachedFlow,
				FlowComponents,
				xPositionOnScreen + 16,
				yPositionOnScreen + 16,
				lineOffset: FlowOffset,
				maxHeight: height - 48,
				onCreate: cmp => {
					allClickableComponents?.Add(cmp);
				},
				onDestroy: cmp => {
					allClickableComponents?.Remove(cmp);
				}
			);
		}

		private bool Scroll(int direction) {
			int old = FlowOffset;
			FlowOffset += direction;
			if (FlowOffset < 0)
				FlowOffset = 0;
			if (FlowOffset > MaxFlowOffset)
				FlowOffset = MaxFlowOffset;

			if (old == FlowOffset)
				return false;

			UpdateFlowComponents();
			return true;
		}

		public override void snapToDefaultClickableComponent() {
			currentlySnappedComponent = FlowComponents.Count > 0 ? FlowComponents[0] : null;
			if (currentlySnappedComponent != null)
				snapCursorToCurrentSnappedComponent();
		}

		public override void receiveScrollWheelAction(int direction) {
			base.receiveScrollWheelAction(direction);

			if (Scroll(direction > 0 ? -1 : 1))
				Game1.playSound("shwip");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true) {
			base.receiveLeftClick(x, y, playSound);

			if (FlowHelper.ClickFlow(
				CachedFlow,
				x - xPositionOnScreen - 16,
				y - yPositionOnScreen - 16,
				lineOffset: FlowOffset,
				maxHeight: height - 48
			))
				return;

			if (btnPageUp.containsPoint(x, y)) {
				if (Scroll(-1))
					Game1.playSound("smallSelect");
			}

			if (btnPageDown.containsPoint(x, y)) {
				if (Scroll(1))
					Game1.playSound("smallSelect");
			}

			if (x < xPositionOnScreen || x > (xPositionOnScreen + width) || y < yPositionOnScreen || y > (yPositionOnScreen + height))
				exitThisMenu();
		}

		public override void performHoverAction(int x, int y) {
			base.performHoverAction(x, y);

			btnPageDown.tryHover(x, FlowOffset >= MaxFlowOffset ? -1 : y);
			btnPageUp.tryHover(x, FlowOffset > 0 ? y : -1);
		}

		public override void draw(SpriteBatch b) {
			// Dim the Background
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

			// Background
			IClickableMenu.drawTextureBox(
				b,
				texture: Game1.menuTexture,
				sourceRect: new Rectangle(0, 256, 60, 60),
				x: xPositionOnScreen,
				y: yPositionOnScreen,
				width: width,
				height: height,
				color: Color.White,
				scale: 1f
				);

			FlowHelper.RenderFlow(
				b,
				CachedFlow,
				new Vector2(xPositionOnScreen + 16, yPositionOnScreen + 16),
				(Color?) null,
				lineOffset: FlowOffset,
				maxHeight: height - 48
			);

			if (FlowOffset >= MaxFlowOffset)
				btnPageDown.draw(b, Color.Black * 0.35f, 0.89f);
			else
				btnPageDown.draw(b);

			if (FlowOffset == 0)
				btnPageUp.draw(b, Color.Black * 0.35f, 0.89f);
			else
				btnPageUp.draw(b);

			// Base Menu
			base.draw(b);

			// Mouse
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}

	}
}
