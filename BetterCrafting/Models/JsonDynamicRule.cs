using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley.GameData;

namespace Leclair.Stardew.BetterCrafting.Models;

public class JsonDynamicRule {

	#region Identity

	public string Id { get; set; } = string.Empty;

	#endregion

	#region Display

	public string? DisplayName { get; set; }

	public string? Description { get; set; }

	public CategoryIcon Icon { get; set; } = new() { Type = CategoryIcon.IconType.Item };

	#endregion

	#region Matching Items

	public GenericSpawnItemData[] Rules { get; set; } = [];

	#endregion

}
