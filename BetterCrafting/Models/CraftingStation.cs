using Newtonsoft.Json;

namespace Leclair.Stardew.BetterCrafting.Models;

public class CraftingStation : ICraftingStation {

	public string Id { get; set; } = string.Empty;

	public string? Theme { get; set; } = null;

	public string? DisplayName { get; set; } = null;
	public CategoryIcon? Icon { get; set; } = null;

	public bool AreRecipesExclusive { get; set; } = false;

	public bool? IncludeUnknownRecipes { get; set; } = null;

	[JsonProperty(nameof(DisplayUnknownRecipes))]
	public bool? _DisplayUnknownRecipes { get; set; } = null;

	[JsonIgnore]
	public bool DisplayUnknownRecipes => _DisplayUnknownRecipes ?? DisplayAsShop;

	public bool DisplayAsShop { get; set; } = false;

	[JsonProperty(nameof(IncrementCrafted))]
	public bool? _IncrementCrafted { get; set; } = null;

	[JsonIgnore]
	public bool IncrementCrafted => _IncrementCrafted ?? !DisplayAsShop;

	[JsonProperty(nameof(ProcessQuests))]
	public bool? _ProcessQuests { get; set; } = null;

	[JsonIgnore]
	public bool ProcessQuests => _ProcessQuests ?? !DisplayAsShop;

	public bool IsCooking { get; set; } = false;

	public string[] Recipes { get; set; } = [];

	public Category[]? Categories { get; set; } = null;

}
