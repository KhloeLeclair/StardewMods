#nullable enable

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.Common.UI.SimpleLayout;

public interface ISimpleNode {

	// Alignment
	Alignment Alignment { get; }

	// Size
	bool DeferSize { get; }
	Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize);

	// Rendering
	void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor);

}
