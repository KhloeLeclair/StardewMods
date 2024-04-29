using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.CloudySkies.Serialization;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.LayerData;

[JsonConverter(typeof(LayerDataConverter))]
public record BaseLayerData : ILayerData {

	public string Id { get; set; } = string.Empty;

	public string Type { get; set; } = string.Empty;

	#region Conditions

	public string? Condition { get; set; }

	public string? Group { get; set; }

	#endregion

	#region Shared Rendering

	public LayerDrawType Mode { get; set; } = LayerDrawType.Normal;

	#endregion

}
