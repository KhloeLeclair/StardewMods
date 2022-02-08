using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.Common {
	public class SpriteInfo {
		public Texture2D Texture;

		public Rectangle BaseSource;
		public Color? BaseColor;
		public float BaseScale;

		public Texture2D OverlayTexture;
		public Rectangle? OverlaySource;
		public Color? OverlayColor;
		public float OverlayScale;

		public bool IsPrismatic;

		public SpriteInfo(Texture2D texture, Rectangle baseSource, Color? baseColor = null, float baseScale = 1f, Texture2D overlayTexture = null, Rectangle? overlaySource = null, Color? overlayColor = null, float overlayScale = 1f, bool isPrismatic = false) {
			Texture = texture;
			BaseSource = baseSource;
			BaseColor = baseColor;
			BaseScale = baseScale;
			OverlayTexture = overlayTexture;
			OverlaySource = overlaySource;
			OverlayColor = overlayColor;
			OverlayScale = overlayScale;
			IsPrismatic = isPrismatic;
		}

		public virtual void Draw(SpriteBatch batch, Vector2 location, float scale) {
			float width = BaseSource.Width * BaseScale;
			float height = BaseSource.Height * BaseScale;

			if (OverlaySource.HasValue) {
				width = Math.Max(width, OverlaySource.Value.Width * OverlayScale);
				height = Math.Max(height, OverlaySource.Value.Height * OverlayScale);
			}

			float max = Math.Max(width, height);

			float targetSize = scale * 16;
			float s = Math.Min(scale, targetSize / max);

			// Draw the base.
			float bs = s * BaseScale;
			float offsetX = Math.Max((targetSize - (BaseSource.Width * bs)) / 2, 0);
			float offsetY = Math.Max((targetSize - (BaseSource.Height * bs)) / 2, 0);

			Color color = BaseColor ?? Color.White;
			if (IsPrismatic && OverlaySource == null)
				color = Utility.GetPrismaticColor();

			batch.Draw(Texture, new Vector2(location.X + offsetX, location.Y + offsetY), BaseSource, color, 0f, Vector2.Zero, bs, SpriteEffects.None, 1f);

			if (OverlaySource != null) {
				float os = s * OverlayScale;
				offsetX = Math.Max((targetSize - (OverlaySource.Value.Width * os)) / 2, 0);
				offsetY = Math.Max((targetSize - (OverlaySource.Value.Height * os)) / 2, 0);

				color = OverlayColor ?? Color.White;
				if (IsPrismatic)
					color = Utility.GetPrismaticColor();

				batch.Draw(OverlayTexture ?? Texture, new Vector2(location.X + offsetX, location.Y + offsetY), OverlaySource.Value, color, 0f, Vector2.Zero, os, SpriteEffects.None, 1f);
			}
		}
	}
}
