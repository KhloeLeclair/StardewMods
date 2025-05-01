using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;

namespace Leclair.Stardew.CloudySkies.Layers;

public class RainLayer : IWeatherLayer {

	public struct DropInfo {
		public int Frame;
		public int Accumulator;
		public Vector2 Position;
		public DropInfo(int x, int y, int frame, int accumulator) {
			Position = new Vector2(x, y);
			Frame = frame;
			Accumulator = accumulator;
		}
	}

	private readonly ModEntry Mod;

	public ulong Id { get; }

	public LayerDrawType DrawType { get; }

	private Texture2D? Texture;
	private string? TextureName;
	private readonly Rectangle? _Source;

	private Rectangle Source => _Source ?? Texture?.Bounds ?? Rectangle.Empty;


	private int Frames;

	private readonly Vector2 Speed;

	private readonly SpriteEffects Effects;
	private readonly float Scale;

	private readonly Color Color;
	private readonly float Opacity;
	private readonly int Vibrancy;

	private bool IsDisposed;

	private readonly DropInfo[] Drops;

	#region Life Cycle

	public RainLayer(ModEntry mod, ulong id, IRainLayerData data) {
		Mod = mod;
		Id = id;
		DrawType = data.Mode;

		TextureName = data.Texture;
		if (TextureName is not null) {
			Mod.MarkLoadsAsset(id, TextureName);
			Texture = Game1.content.Load<Texture2D>(TextureName);
		}

		_Source = Texture is null ? null : data.Source;
		Frames = Texture is null ? 4 : data.Frames - 1;

		Scale = data.Scale;
		Speed = data.Speed;

		Effects = SpriteEffects.None;
		if (data.FlipHorizontal)
			Effects |= SpriteEffects.FlipHorizontally;
		if (data.FlipVertical)
			Effects |= SpriteEffects.FlipVertically;

		Color = data.Color ?? Color.White;
		Opacity = data.Opacity;
		Vibrancy = data.Vibrancy;

		Drops = new DropInfo[data.Count];
		RandomizeDrops();
	}

	public void ReloadAssets() {
		if (TextureName is not null)
			Texture = Game1.content.Load<Texture2D>(TextureName);

		RandomizeDrops();
	}

	protected virtual void Dispose(bool disposing) {
		if (!IsDisposed) {
			Texture = null!;
			TextureName = null!;
			Mod.RemoveLoadsAsset(Id);
			IsDisposed = true;
		}
	}

	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion

	private void RandomizeDrops() {
		int length = Drops.Length;
		for (int i = 0; i < length; i++) {
			Drops[i] = new DropInfo(
				Game1.random.Next(Game1.viewport.Width),
				Game1.random.Next(Game1.viewport.Height),
				Game1.random.Next(Frames),
				Game1.random.Next(70)
			);
		}
	}


	public void MoveWithViewport(int offsetX, int offsetY) {
		int maxY = Game1.viewport.Height + 64;
		int maxX = Game1.viewport.Width + 64;

		for (int i = 0; i < Drops.Length; i++) {
			DropInfo drop = Drops[i];
			drop.Position.X -= offsetX;
			drop.Position.Y -= offsetY;

			if (drop.Position.Y > maxY)
				drop.Position.Y = -64f;
			else if (drop.Position.Y < -64f)
				drop.Position.Y = maxY;

			if (drop.Position.X > maxX)
				drop.Position.X = -64f;
			else if (drop.Position.X < -64f)
				drop.Position.X = maxX;

			Drops[i] = drop;
		}
	}


	public void Draw(SpriteBatch batch, GameTime time, RenderTarget2D targetScreen) {

		int length = Drops.Length;
		Rectangle source = Source;
		Color color = Color * Opacity;
		int white_offset = Color == Color.White ? 0 : 4;

		for (int i = 0; i < length; i++) {
			DropInfo drop = Drops[i];
			Rectangle src = Texture != null
				? new(source.X + (drop.Frame * source.Width), source.Y, source.Width, source.Height)
				: Game1.getSourceRectForStandardTileSheet(Game1.rainTexture, drop.Frame + white_offset, 16, 16);

			for (int v = 0; v < Vibrancy; v++) {
				batch.Draw(
					Texture ?? Game1.rainTexture,
					drop.Position,
					src,
					color,
					0f,
					Vector2.Zero,
					Scale,
					Effects,
					1f
				);
			}
		}

	}

	public void Resize(Point newSize, Point oldSize) {
		RandomizeDrops();
	}

	public void Update(GameTime time) {
		int length = Drops.Length;

		for (int i = 0; i < length; i++) {
			DropInfo drop = Drops[i];
			drop.Accumulator += time.ElapsedGameTime.Milliseconds;

			if (drop.Accumulator > 70) {
				drop.Accumulator = 0;

				if (drop.Frame == 0) {
					drop.Position += new Vector2(Speed.X + i * (Speed.X < 0 ? 8 : -8) / length, Speed.Y + i * (Speed.Y < 0 ? 8 : -8) / length);

					if (drop.Position.Y > (Game1.viewport.Height + 64))
						drop.Position.Y = -64f;

					if (drop.Position.X > (Game1.viewport.Width + 64))
						drop.Position.X = -64f;
					else if (drop.Position.X < -64f)
						drop.Position.X = Game1.viewport.Width + 64f;

					if (Game1.random.NextDouble() < 0.1)
						drop.Frame++;

				} else {
					drop.Frame = (drop.Frame + 1) % Frames;
					if (drop.Frame == 0)
						drop.Position = new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height));
				}
			}

			// Push the modified drop back into the array.
			Drops[i] = drop;
		}
	}
}
