using System;

using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed record DrawMethod(Texture2D Texture, Rectangle Source, float Scale, int Frames = 1, int FrameTime = 16) {

	public Rectangle CurrentSource => Frames > 1
		? SpriteInfo.GetFrame(Source, -1, Frames, int.MaxValue, FrameTime)
		: Source;

	public void Draw(SpriteBatch batch, Rectangle bounds) {
		var rect = CurrentSource;

		float width = rect.Width * Scale;
		float height = rect.Height * Scale;

		batch.Draw(
			Texture,
			new Vector2(
				bounds.X + (float) Math.Floor((bounds.Width - width) / 2),
				bounds.Y + (float) Math.Floor((bounds.Height - height) / 2)
			),
			rect,
			Color.White,
			0f,
			Vector2.Zero,
			Scale,
			SpriteEffects.None,
			1f
		);
	}

}
