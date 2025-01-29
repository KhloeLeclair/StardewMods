using System.Collections.Generic;

using StardewValley;

namespace Leclair.Stardew.CloudySkies.Models;

internal record struct EffectCache {

	public WeatherData Data { get; set; }

	public GameLocation Location { get; set; }

	public int Hour { get; set; }

	public bool EventUp { get; set; }

	public Dictionary<string, IWeatherEffect> EffectsById { get; set; }
	public Dictionary<string, IEffectData> DataById { get; set; }

	public List<IWeatherEffect>? Effects { get; set; }

}
