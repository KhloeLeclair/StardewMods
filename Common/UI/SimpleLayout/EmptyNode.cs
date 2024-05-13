
using Leclair.Stardew.Common.UI;
using Leclair.Stardew.Common.UI.SimpleLayout;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class EmptyNode : ISimpleNode {

	public static EmptyNode Instance = new EmptyNode();

	public Alignment Alignment => Alignment.None;

	public bool DeferSize => false;

	public void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor) {

	}

	public Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize) {
		return Vector2.Zero;
	}
}
