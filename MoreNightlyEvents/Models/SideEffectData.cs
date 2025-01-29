
using System.Collections.Generic;

namespace Leclair.Stardew.MoreNightlyEvents.Models;

public class SideEffectData {

	public string Id { get; set; } = string.Empty;

	public string? Condition { get; set; }

	public bool HostOnly { get; set; }

	public List<string>? Actions { get; set; }

}
