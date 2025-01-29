using Leclair.Stardew.Common.Serialization.Converters;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.Models;


public record CritterSpawnData : ICritterSpawnData {

	public string Id { get; set; } = string.Empty;

	public string? Condition { get; set; }
	public string? Group { get; set; }

	public float Chance { get; set; }

	public string Type { get; set; } = string.Empty;

}
