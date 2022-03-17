using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI.FlowNode
{
    public struct DividerNode : IFlowNode {

		public Color? Color { get; }
		public Color? ShadowColor { get; }
		public float Size { get; }
		public float Padding { get; }
		public float ShadowOffset { get; }

		public Alignment Alignment => Alignment.None;
		public object Extra { get; }

		public Func<IFlowNodeSlice, int, int, bool> OnClick => null;
		public Func<IFlowNodeSlice, int, int, bool> OnHover => null;
		public Func<IFlowNodeSlice, int, int, bool> OnRightClick => null;

		public DividerNode(
			Color? color = null,
			Color? shadowColor = null,
			float size = 4f,
			float padding = 14f,
			float shadowOffset = 2f,
			object extra = null
		) {
			Color = color;
			ShadowColor = shadowColor;
			Size = size;
			Padding = padding < 0 ? 0f : padding;
			ShadowOffset = shadowOffset;
			Extra = extra;
		}

		public bool? WantComponent(IFlowNodeSlice slice) {
			return false;
		}

		public ClickableComponent UseComponent(IFlowNodeSlice slice) {
			return null;
		}

		public bool IsEmpty() {
			return Size <= 0;
		}

		public IFlowNodeSlice Slice(IFlowNodeSlice last, SpriteFont font, float maxWidth, float remaining) {
			if (last != null)
				return null;

			return new UnslicedNode(
				this,
				maxWidth,
				Size + Padding + Padding,
				WrapMode.None
			);
		}

		public void Draw(IFlowNodeSlice slice, SpriteBatch batch, Vector2 position, float scale, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor, CachedFlowLine line, CachedFlow flow) {
			if (IsEmpty())
				return;

			int shadowOffset = (int) ShadowOffset;
			int x = (int) position.X;
			int y = (int) position.Y + (int) Padding;
			
			if (shadowOffset != 0)
				batch.Draw(
					Game1.uncoloredMenuTexture,
					new Rectangle(
						x - shadowOffset, y + shadowOffset,
						(int) slice.Width, (int) Size
					),
					new Rectangle(16, 272, 28, 28),
					ShadowColor ?? defaultShadowColor ?? Game1.textShadowColor
				);

			batch.Draw(
				Game1.uncoloredMenuTexture,
				new Rectangle(
					x, y,
					(int) slice.Width, (int) Size
				),
				new Rectangle(16, 272, 28, 28),
				Color ?? defaultColor ?? Game1.textColor
			);
		}
	}
}
