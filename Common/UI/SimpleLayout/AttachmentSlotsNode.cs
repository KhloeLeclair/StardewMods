#if COMMON_SIMPLELAYOUT

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.Common.UI.SimpleLayout;

public class AttachmentSlotsNode : ISimpleNode {

	public readonly Item Item;

	public Alignment Alignment { get; }

	public bool DeferSize => false;

	public AttachmentSlotsNode(Item item, Alignment alignment = Alignment.None) {
		Item = item;
		Alignment = alignment;
	}

	public Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize) {

		int slots = Item?.attachmentSlots() ?? 0;
		if (slots <= 0)
			return Vector2.Zero;

		return new Vector2(17 * 4 * slots, 17 * 4);
	}

	public void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor) {

		Item?.drawAttachments(batch, (int) position.X, (int) position.Y);

	}
}

#endif
