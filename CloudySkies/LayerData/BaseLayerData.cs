using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.CloudySkies.Serialization;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.LayerData;

[JsonConverter(typeof(LayerDataConverter))]
public record BaseLayerData {

	/// <summary>
	/// An identifier for this specific layer within its parent weather
	/// condition. This need only be unique within the weather
	/// condition itself, so you can feel free to use Ids like <c>rain</c>
	/// </summary>
	public string Id { get; set; } = string.Empty;

	public string Type { get; set; } = string.Empty;


	#region Conditions

	/// <summary>
	/// A condition that must evaluate to true for this layer to be displayed.
	/// If not set, the layer will always be displayed. This condition is
	/// only reevaluated upon location change or the hour changing.
	/// </summary>
	public string? Condition { get; set; }

	/// <summary>
	/// If you set a group, only the first layer in a group will be
	/// displayed at any given time. This can be used to make layers that
	/// display in some situations, with fall-back layers in other situations.
	/// </summary>
	public string? Group { get; set; }

	#endregion

	#region Shared Rendering

	public LayerDrawType Mode { get; set; } = LayerDrawType.Normal;

	#endregion

}
