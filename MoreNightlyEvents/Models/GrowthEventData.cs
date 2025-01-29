using Leclair.Stardew.Common.Serialization.Converters;

namespace Leclair.Stardew.MoreNightlyEvents.Models;

[DiscriminatedType("Growth")]
public class GrowthEventData : BaseEventData {

	public bool DrawFaeries { get; set; } = true;

}
