using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Leclair.Stardew.CloudySkies.Models;

public class LocationContextExtensionData : ILocationContextExtensionData {

	public string Id { get; set; } = string.Empty;

	public string? DisplayName { get; set; }

	public Dictionary<string, bool> AllowWeatherTotem { get; set; } = new();

	public bool IncludeInWeatherChannel { get; set; }

	public string? WeatherChannelCondition { get; set; }

	public string? WeatherForecastPrefix { get; set; }

	public string? WeatherChannelBackgroundTexture { get; set; }

	public Point WeatherChannelBackgroundSource { get; set; } = Point.Zero;

	public int WeatherChannelBackgroundFrames { get; set; } = 1;

	public float WeatherChannelBackgroundSpeed { get; set; } = 150f;

	public string? WeatherChannelOverlayTexture { get; set; }

	public Point WeatherChannelOverlayIntroSource { get; set; } = Point.Zero;

	public int WeatherChannelOverlayIntroFrames { get; set; } = 1;

	public float WeatherChannelOverlayIntroSpeed { get; set; } = 150f;

	public Point? WeatherChannelOverlayWeatherSource { get; set; }

	public int WeatherChannelOverlayWeatherFrames { get; set; } = 1;

	public float WeatherChannelOverlayWeatherSpeed { get; set; } = 150f;
}
