#if COMMON_SIMPLELAYOUT

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI.SimpleLayout;

public record struct ComponentSNode : ISimpleNode {

	public delegate void DrawDelegate(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor);

	public ClickableComponent Component { get; }

	public Alignment Alignment { get; }

	public DrawDelegate? OnDraw;

	public ComponentSNode(ClickableComponent component, DrawDelegate? onDraw = null, Alignment align = Alignment.None) {
		Component = component;
		Alignment = align;
		OnDraw = onDraw;
	}

	public readonly bool DeferSize => false;

	public readonly Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize) {
		return new Vector2(Component.bounds.Width, Component.bounds.Height);
	}

	public void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont, Color? defaultColor, Color? defaultShadowColor) {

		Component.visible = true;

		int x = (int) position.X;
		int y = (int) position.Y;

		if (x != Component.bounds.X || y != Component.bounds.Y)
			Component.bounds = new Rectangle(
				x, y,
				Component.bounds.Width,
				Component.bounds.Height
			);

		OnDraw?.Invoke(batch, position, size, containerSize, alpha, defaultFont, defaultColor, defaultShadowColor);

		if (Component is ClickableTextureComponent cp)
			cp.draw(batch);

		else if (Component is ClickableAnimatedComponent can)
			can.draw(batch);
	}

}

#endif
