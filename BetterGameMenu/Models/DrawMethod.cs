using System;

using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.BetterGameMenu.Models;

public sealed record DrawMethod(Texture2D Texture, Rectangle Source, float Scale, int Frames = 1, int FrameTime = 16, Vector2? offset = null) {

	public Vector2 Offset = offset ?? Vector2.Zero;

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
				bounds.X + (float) Math.Floor((bounds.Width - width) / 2) + Offset.X,
				bounds.Y + (float) Math.Floor((bounds.Height - height) / 2) + Offset.Y
			),
			rect,
			Color.White,
			0f,
			Vector2.Zero,
			Scale,
			SpriteEffects.None,
			0.1f
		);
	}

}
