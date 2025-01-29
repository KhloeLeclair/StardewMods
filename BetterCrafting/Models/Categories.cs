#nullable enable

using System;
using System.Collections.Generic;

namespace Leclair.Stardew.BetterCrafting.Models;

public class Categories {

	public Category[]? Cooking { get; set; }

	public Category[]? Crafting { get; set; }

	public AppliedDefaults? Applied { get; set; }

}

public class CPCategories {

	public Dictionary<string, Category> Cooking { get; set; } = null!;

	public Dictionary<string, Category> Crafting { get; set; } = null!;

}
