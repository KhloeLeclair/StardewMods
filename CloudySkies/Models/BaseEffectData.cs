using Leclair.Stardew.CloudySkies.Serialization;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.Models;

[JsonConverter(typeof(EffectDataConverter))]
public record BaseEffectData : IEffectData {

	public string Id { get; set; } = string.Empty;

	public string Type { get; set; } = string.Empty;

	public uint Rate { get; set; } = 60;

	#region Conditions

	public string? Condition { get; set; }

	public string? Group { get; set; }

	#endregion

}
