
/* Unmerged change from project 'Hydrology'
Before:
using System;

using Microsoft.Xna.Framework;
After:
using System;

using Leclair.Stardew.Common.UI;

using Microsoft.Xna.Framework;
*/

/* Unmerged change from project 'Almanac'
Before:
using System;

using Microsoft.Xna.Framework;
After:
using System;

using Leclair.Stardew.Common.UI;

using Microsoft.Xna.Framework;
*/

/* Unmerged change from project 'SeeMeRollin'
Before:
using System;

using Microsoft.Xna.Framework;
After:
using System;

using Leclair.Stardew.Common.UI;

using Microsoft.Xna.Framework;
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.BellsAndWhistles;

namespace Leclair.Stardew.Common.UI.SimpleLayout {
	public class TextNode : ISimpleNode {

		public Alignment Alignment { get; }
		public string Text { get; }
		public TextStyle Style { get; }

		public TextNode(string text, TextStyle? style = null, Alignment alignment = Alignment.None) {
			Text = text;
			Style = style ?? TextStyle.EMPTY;
			Alignment = alignment;
		}

		public bool DeferSize => false;

		public static Vector2 GetFancySize(string text) {
			return new Vector2(
				SpriteText.getWidthOfString(text),
				SpriteText.getHeightOfString(text)
			);
		}

		public Vector2 GetSize(SpriteFont defaultFont, Vector2 containerSize) {
			if (string.IsNullOrEmpty(Text))
				return Vector2.Zero;

			Vector2 size;
			float scale = Style.Scale ?? 1f;
			SpriteFont font = Style.Font ?? defaultFont ?? Game1.smallFont;

			if (Style.IsFancy())
				size = GetFancySize(Text);
			else
				size = font.MeasureString(Text) * scale;

			return size;
		}

		public void Draw(SpriteBatch batch, Vector2 position, Vector2 size, Vector2 containerSize, float alpha, SpriteFont defaultFont) {
			if (string.IsNullOrEmpty(Text))
				return;

			float scale = Style.Scale ?? 1f;
			SpriteFont font = Style.Font ?? defaultFont ?? Game1.smallFont;
			Color color = Style.Color ?? Game1.textColor;
			if (Style.IsPrismatic())
				color = Utility.GetPrismaticColor();

			if (Style.IsFancy())
				SpriteText.drawString(batch, Text, (int) position.X, (int) position.Y);
			else if (Style.IsBold())
				Utility.drawBoldText(batch, Text, font, position, color, scale);
			else if (Style.HasShadow())
				Utility.drawTextWithShadow(batch, Text, font, position, color, scale);
			else
				batch.DrawString(font, Text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, GUIHelper.GetLayerDepth(position.Y));
		}
	}
}
