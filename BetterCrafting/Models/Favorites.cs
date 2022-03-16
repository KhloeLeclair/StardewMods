#nullable enable

using System.Collections.Generic;

using Leclair.Stardew.Common.Types;

namespace Leclair.Stardew.BetterCrafting.Models;

public class Favorites {

	public Dictionary<long, CaseInsensitiveHashSet> Cooking { get; set; } = new();
	public Dictionary<long, CaseInsensitiveHashSet> Crafting { get; set; } = new();

}
