using Leclair.Stardew.Common.Serialization.Converters;

namespace Leclair.Stardew.MoreNightlyEvents.Models;

[DiscriminatedType("Script")]
public class ScriptEventData : BaseEventData {

	public string? Script { get; set; }

}
