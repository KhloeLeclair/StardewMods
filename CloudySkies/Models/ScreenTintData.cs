using Leclair.Stardew.Common.Serialization.Converters;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.Models;


public record ScreenTintData : IScreenTintData {

	public string Id { get; set; } = string.Empty;

	public int TimeOfDay { get; set; } = 600;

	public string? Condition { get; set; }

	public LightingTweenMode TweenMode { get; set; } = LightingTweenMode.Both;

	[JsonConverter(typeof(ColorConverter))]
	public Color? AmbientColor { get; set; }

	public float? AmbientOutdoorOpacity { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? LightingTint { get; set; }

	public float LightingTintOpacity { get; set; }

	[JsonConverter(typeof(ColorConverter))]
	public Color? PostLightingTint { get; set; }

	public float PostLightingTintOpacity { get; set; }

}
