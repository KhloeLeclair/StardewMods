#nullable enable

using System;

namespace Leclair.Stardew.BetterCrafting.Models;

public class Categories {

	public Category[] Cooking { get; set; } = Array.Empty<Category>();

	public Category[] Crafting { get; set; } = Array.Empty<Category>();

	public AppliedDefaults? Applied { get; set; }

}
