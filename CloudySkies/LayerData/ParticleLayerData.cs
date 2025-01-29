using Leclair.Stardew.Common.Serialization.Converters;

using Microsoft.Xna.Framework;

namespace Leclair.Stardew.CloudySkies.LayerData;

[DiscriminatedType("Particle")]
public record ParticleLayerData : BaseLayerData {

	public string? Texture { get; set; }

	public Rectangle? Source { get; set; }

}
