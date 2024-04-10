using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leclair.Stardew.BetterCrafting.Models;

public class CraftingStation : ICraftingStation {

	public string Id { get; set; } = string.Empty;

	public string? Theme { get; set; } = null;

	public string? DisplayName { get; set; } = null;
	public CategoryIcon? Icon { get; set; } = null;

	public bool AreRecipesExclusive { get; set; } = false;
	public bool DisplayUnknownRecipes { get; set; } = false;

	public bool IsCooking { get; set; } = false;

	public string[] Recipes { get; set; } = Array.Empty<string>();

	public Category[]? Categories { get; set; } = null;

}
