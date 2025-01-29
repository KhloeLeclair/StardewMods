using System;

using Leclair.Stardew.CloudySkies;
using Leclair.Stardew.Common;
using Leclair.Stardew.ThemeManager;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Menus;

namespace ThemeManagerExample;

public class ThemeData {

	public float TextScale { get; set; } = 1;

	public Color? TextColor { get; set; }

}

public class ModEntry : Mod {

	internal IThemeManager<ThemeData>? ThemeManager;
	internal IManagedAsset<Texture2D>? Background;
	internal ThemeData Theme = new();

	internal IGameTheme? GameTheme;

	public override void Entry(IModHelper helper) {
		Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		Helper.Events.Display.RenderedHud += Display_RenderedHud;
	}

	private void Display_RenderedHud(object? sender, StardewModdingAPI.Events.RenderedHudEventArgs e) {
		if (e.SpriteBatch != null)
			return;

		// Read values from our theme!
		float scale = Theme.TextScale;
		Color color = Theme.TextColor ?? GameTheme?.GetColorVariable("Text") ?? Game1.textColor;

		// Set up the text!
		string text = $"Hello!\n\nSelected Theme: {ThemeManager?.SelectedThemeId}\nActive Theme: {ThemeManager?.ActiveThemeId}";
		var size = Game1.smallFont.MeasureString(text) * scale;

		// Draw a box! Not just any box, but a box using our
		// Background texture.
		if (Background?.Value != null)
			IClickableMenu.drawTextureBox(
				e.SpriteBatch,
				texture: Background.Value,
				sourceRect: new Rectangle(0, 0, 15, 15),
				x: 16, y: 16,
				width: 48 + (int) size.X,
				height: 48 + (int) size.Y,
				color: Color.White,
				scale: 4f
			);

		// Now draw our text in the box, using the color
		// and scale from our theme.
		e.SpriteBatch.DrawString(
			Game1.smallFont,
			text,
			new Vector2(40, 40),
			color,
			0f,
			Vector2.Zero,
			scale,
			SpriteEffects.None,
			1f
		);
	}


	public class TestEffect : IWeatherEffect {

		public ulong Id { get; }

		public uint Rate { get; }

		public TestEffect(ulong id, ICustomEffectData data) {
			Id = id;
			Rate = data.Rate;
		}

		public void Update(GameTime time) {
			Game1.player.bathingClothes.Value = time.TotalGameTime.TotalSeconds % 10 >= 5;
		}
	}


	public class TestThing : IWeatherLayer {
		public ulong Id { get; }
		public LayerDrawType DrawType { get; }

		public readonly Color Color;

		public TestThing(ulong id, ICustomLayerData data) {
			Id = id;
			DrawType = data.Mode;

			string? colorInput = null;
			if (data.Fields.TryGetValue("Color", out var token))
				colorInput = ((string?) token);

			Color = CommonHelper.ParseColor(colorInput) ?? Color.White;
		}

		public void Draw(SpriteBatch batch, GameTime time, RenderTarget2D targetScreen) {
			batch.Draw(Game1.mouseCursors, Vector2.Zero, Color);
		}

		// other methods left out for brevity

		public void MoveWithViewport(int offsetX, int offsetY) { }
		public void ReloadAssets() { }
		public void Resize(Point newSize, Point oldSize) { }
		public void Update(GameTime time) { }
	}



	private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e) {
		SetupTheme();

		ICloudySkiesApi? api = null;
		try {
			api = Helper.ModRegistry.GetApi<ICloudySkiesApi>("leclair.cloudyskies");
		} catch (Exception ex) {
			Monitor.Log($"Unable to fetch Cloudy Skies API: {ex}", LogLevel.Error);
		}

		api?.RegisterLayerType("Test", (id, data) => new TestThing(id, data));
		api?.RegisterEffectType("Test", (id, data) => new TestEffect(id, data));

		Background = LoadManaged<Texture2D>("Background.png");
	}

	private IManagedAsset<T> LoadManaged<T>(string path) where T : notnull {
		if (ThemeManager is not null)
			return ThemeManager.GetManagedAsset<T>(path);
		return new FallbackManagedAsset<T>($"assets/{path}", Helper, Monitor);
	}

	private void SetupTheme() {
		if (!Helper.ModRegistry.IsLoaded("leclair.thememanager"))
			return;

		IThemeManagerApi? api;
		try {
			api = Helper.ModRegistry.GetApi<IThemeManagerApi>("leclair.thememanager");
		} catch (Exception ex) {
			Monitor.Log($"Unable to get Theme Manager's API: {ex}", LogLevel.Error);
			return;
		}

		if (api is null)
			return;

		ThemeManager = api.GetOrCreateManager<ThemeData>();

		GameTheme = api.GameTheme;
		api.GameThemeChanged += OnBaseThemeChanged;

		Theme = ThemeManager.Theme;
		ThemeManager.ThemeChanged += OnThemeChanged;
	}

	private void OnBaseThemeChanged(IThemeChangedEvent<IGameTheme> e) {
		GameTheme = e.NewData;
	}

	private void OnThemeChanged(IThemeChangedEvent<ThemeData> e) {
		Theme = e.NewData;
		Background = LoadManaged<Texture2D>("Background.png");
	}
}
