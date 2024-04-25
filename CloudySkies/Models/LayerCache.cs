using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley;

namespace Leclair.Stardew.CloudySkies.Models;

internal record struct LayerCache {

	public WeatherData Data { get; set; }

	public GameLocation Location { get; set; }

	public int Hour { get; set; }

	public bool EventUp { get; set; }

	public Dictionary<string, IWeatherLayer> LayersById { get; set; }
	public Dictionary<string, BaseLayerData> DataById { get; set; }

	public List<IWeatherLayer>? Layers { get; set; }

}
