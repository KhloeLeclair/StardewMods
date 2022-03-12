using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI.FlowNode
{
    public struct ComponentNode : IFlowNode {

		public ClickableComponent Component { get; }

		public Alignment Alignment { get; }

		public Action<SpriteBatch, Vector2, float, SpriteFont, Color?, Color?> OnDraw;

		public Func<IFlowNodeSlice, int, int, bool> OnClick { get; }
		public Func<IFlowNodeSlice, int, int, bool> OnHover { get; }
		public Func<IFlowNodeSlice, int, int, bool> OnRightClick { get; }

		public WrapMode Wrapping { get; }

		public bool NoComponent => false;
		public ClickableComponent UseComponent => Component;

		public ComponentNode(
			ClickableComponent component,
			Alignment? alignment = null,
			WrapMode? wrapping = null,
			Action<SpriteBatch, Vector2, float, SpriteFont, Color?, Color?> onDraw = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null
		) {
			Component = component;
			Alignment = alignment ?? Alignment.None;
			Wrapping = wrapping ?? WrapMode.None;
			OnDraw = onDraw;
			OnClick = onClick;
			OnHover = onHover;
			OnRightClick = onRightClick;
		}

		private bool HandleHover(IFlowNodeSlice slice, int x, int y) {
			x += Component.bounds.X;
			y += Component.bounds.Y;

			return false;
		}

		public bool IsEmpty() {
			return Component == null;
		}

		public IFlowNodeSlice Slice(IFlowNodeSlice last, SpriteFont font, float maxWidth, float remaining) {
			if (last != null || Component == null)
				return null;

			return new UnslicedNode(
				this,
				Component.bounds.Width,
				Component.bounds.Height,
				Wrapping
			);
		}

		public void Draw(IFlowNodeSlice slice, SpriteBatch batch, Vector2 position, float scale, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor, CachedFlowLine line, CachedFlow flow) {
			if (IsEmpty())
				return;

			Component.visible = true;

			int x = (int) position.X;
			int y = (int) position.Y;

			if (x != Component.bounds.X || y != Component.bounds.Y)
				Component.bounds = new Rectangle(
					x, y,
					Component.bounds.Width,
					Component.bounds.Height
				);

			OnDraw?.Invoke(batch, position, scale, defaultFont, defaultColor, defaultShadowColor);

			if (Component is ClickableTextureComponent cp)
				cp.draw(batch);

			else if (Component is ClickableAnimatedComponent can)
				can.draw(batch);
		}

		public override bool Equals(object obj) {
			return obj is ComponentNode node &&
				   EqualityComparer<ClickableComponent>.Default.Equals(Component, node.Component) &&
				   Alignment == node.Alignment &&
				   EqualityComparer<Action<SpriteBatch, Vector2, float, SpriteFont, Color?, Color?>>.Default.Equals(OnDraw, node.OnDraw) &&
				   EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.Equals(OnClick, node.OnClick) &&
				   EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.Equals(OnHover, node.OnHover) &&
				   EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.Equals(OnRightClick, node.OnRightClick) &&
				   NoComponent == node.NoComponent;
		}

		public override int GetHashCode() {
			return HashCode.Combine(Component, Alignment, OnDraw, OnClick, OnHover, OnRightClick, NoComponent);
		}
	}
}
