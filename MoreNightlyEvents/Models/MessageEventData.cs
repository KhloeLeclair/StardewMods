using Leclair.Stardew.Common.Serialization.Converters;

namespace Leclair.Stardew.MoreNightlyEvents.Models;

[DiscriminatedType("Message")]
public class MessageEventData : BaseEventData {

	public string? SoundName { get; set; }

	public int MessageDelay { get; set; } = 7000;

	public string? Message { get; set; }

}
