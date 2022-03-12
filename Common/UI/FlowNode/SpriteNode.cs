using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI.FlowNode {
	public struct SpriteNode : IFlowNode {
		public SpriteInfo Sprite { get; }
		public float Scale { get; }
		public float Size { get; }
		public int Frame { get; set; }
		public Alignment Alignment { get; }

		public bool NoComponent { get; }
		public ClickableComponent UseComponent => null;
		public Func<IFlowNodeSlice, int, int, bool> OnClick { get; }
		public Func<IFlowNodeSlice, int, int, bool> OnHover { get; }
		public Func<IFlowNodeSlice, int, int, bool> OnRightClick { get; }

		public SpriteNode(
			SpriteInfo sprite,
			float scale,
			Alignment? alignment = null,
			Func<IFlowNodeSlice, int, int, bool> onClick = null,
			Func<IFlowNodeSlice, int, int, bool> onHover = null,
			Func<IFlowNodeSlice, int, int, bool> onRightClick = null,
			bool noComponent = false,
			float size = 16,
			int frame = -1
		) {
			Sprite = sprite;
			Scale = scale;
			Size = size;
			Alignment = alignment ?? Alignment.None;
			OnClick = onClick;
			OnHover = onHover;
			OnRightClick = onRightClick;
			NoComponent = noComponent;
			Frame = frame;
		}

		public bool IsEmpty() {
			return Sprite == null || Scale <= 0;
		}

		public IFlowNodeSlice Slice(IFlowNodeSlice last, SpriteFont font, float maxWidth, float remaining) {
			if (last != null)
				return null;

			return new UnslicedNode(this, Size * Scale, Size * Scale, WrapMode.None);
		}

		public void Draw(IFlowNodeSlice slice, SpriteBatch batch, Vector2 position, float scale, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor, CachedFlowLine line, CachedFlow flow) {
			if (IsEmpty())
				return;

			Sprite.Draw(batch, position, scale * Scale, Frame, Size);
		}

		public override bool Equals(object obj) {
			return obj is SpriteNode node &&
				   EqualityComparer<SpriteInfo>.Default.Equals(Sprite, node.Sprite) &&
				   Scale == node.Scale &&
				   Size == node.Size &&
				   Alignment == node.Alignment &&
				   NoComponent == node.NoComponent &&
				   Frame == node.Frame &&
				   EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.Equals(OnClick, node.OnClick) &&
				   EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.Equals(OnHover, node.OnHover) &&
				   EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.Equals(OnRightClick, node.OnRightClick);
		}

		public override int GetHashCode() {
			int hashCode = 2138745294;
			hashCode = hashCode * -1521134295 + EqualityComparer<SpriteInfo>.Default.GetHashCode(Sprite);
			hashCode = hashCode * -1521134295 + Scale.GetHashCode();
			hashCode = hashCode * -1521134295 + Size.GetHashCode();
			hashCode = hashCode * -1521134295 + Alignment.GetHashCode();
			hashCode = hashCode * -1521134295 + NoComponent.GetHashCode();
			hashCode = hashCode * -1521134295 + Frame.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.GetHashCode(OnClick);
			hashCode = hashCode * -1521134295 + EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.GetHashCode(OnHover);
			hashCode = hashCode * -1521134295 + EqualityComparer<Func<IFlowNodeSlice, int, int, bool>>.Default.GetHashCode(OnRightClick);
			return hashCode;
		}
	}
}
