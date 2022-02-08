using Leclair.Stardew.Common.UI.SimpleLayout;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace Leclair.Stardew.Common.UI {

	public static class SimpleHelper {

		public static SimpleBuilder Builder(LayoutDirection dir = LayoutDirection.Vertical, int margin = 0, Vector2? minSize = null, Alignment align = Alignment.None) {
			return new(null, new LayoutNode(dir, null, margin, minSize, align));
		}

		public static void DrawHover(this ISimpleNode node, SpriteBatch batch, SpriteFont defaultFont, int offsetX = 0, int offsetY = 0, int overrideX = -1, int overrideY = -1, float alpha = 1f) {
			// Get the node's size.
			Vector2 size = node.GetSize(defaultFont, Vector2.Zero);

			// If we have no size, we have nothing to draw.
			if (size.X <= 0 || size.Y <= 0)
				return;

			// Add padding around the menu.
			int width = (int) size.X + 32;
			int height = (int) size.Y + 32;

			int x = overrideX < 0 ? Game1.getOldMouseX() + 32 + offsetX : overrideX;
			int y = overrideY < 0 ? Game1.getOldMouseY() + 32 + offsetY : overrideY;

			Rectangle safeArea = Utility.getSafeArea();

			if (x + width > safeArea.Right) {
				x = safeArea.Right - width;
				y += 16;
			}

			if (y + height > safeArea.Bottom) {
				y = safeArea.Bottom - height;
				x += 16;

				if (x + width > safeArea.Right)
					x = safeArea.Right - width;
			}

			if (x < safeArea.Left)
				x = safeArea.Left;

			if (y < safeArea.Top) {
				y = safeArea.Top;
				x += 16 + 32;

				if (x + width > safeArea.Right) {
					x = safeArea.Right - width;
					y += 16 + 32;
				}
			}

			// Draw the background first.
			IClickableMenu.drawTextureBox(batch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White * alpha);

			x += 16;
			y += 16;

			node.Draw(batch, new Vector2(x, y), size, size, alpha, defaultFont);
		}

	}
}
