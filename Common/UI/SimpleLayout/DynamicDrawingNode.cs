#if COMMON_SIMPLELAYOUT

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.Common.UI.SimpleLayout;

public class DynamicDrawingNode : ISimpleNode {

	public delegate void DrawNodeDelegate(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor);
	public delegate Vector2 GetSizeDelegate(SpriteFont defaultFont, Vector2 containerSize);

	public readonly DrawNodeDelegate _Draw;
	public readonly GetSizeDelegate _GetSize;

	public Alignment Alignment { get; }

	public bool DeferSize => false;

	public DynamicDrawingNode(GetSizeDelegate getSize, DrawNodeDelegate onDraw, Alignment alignment = Alignment.None) {
		_GetSize = getSize;
		_Draw = onDraw;
		Alignment = alignment;
	}

	public Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize) {
		return _GetSize(defaultFont, containerSize);
	}

	public void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor) {

		_Draw(batch, position, size, containerSize, alpha, defaultFont, defaultColor, defaultShadowColor);

	}
}

#endif
