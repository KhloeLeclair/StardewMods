using Leclair.Stardew.Common.Serialization.Converters;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.LayerData;


[DiscriminatedType("Color")]
public record ColorLayerData : BaseLayerData, IColorLayerData {

	[JsonConverter(typeof(ColorConverter))]
	public Color? Color { get; set; }

	public float Opacity { get; set; } = 1f;

}
