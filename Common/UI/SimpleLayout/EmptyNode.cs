#if COMMON_SIMPLELAYOUT

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.Common.UI.SimpleLayout;

public class EmptyNode : ISimpleNode {

	public static readonly EmptyNode Instance = new();

	public Alignment Alignment => Alignment.None;

	public bool DeferSize => false;

	public void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor) {

	}

	public Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize) {
		return Vector2.Zero;
	}
}

#endif
