#nullable enable

using System.Collections.Generic;

using Leclair.Stardew.BetterCrafting.DynamicTypes;
using Leclair.Stardew.Common.Crafting;
using Leclair.Stardew.Common.Types;

using Newtonsoft.Json;

namespace Leclair.Stardew.BetterCrafting.Models;

public class Category {

	public string? Id { get; set; }

	public string? Name { get; set; }
	public string? I18nKey { get; set; }

	public bool UseFilters { get; set; }
	public List<DynamicType>? DynamicFilters { get; set; }

	public CategoryIcon? Icon { get; set; }

	public CaseInsensitiveHashSet? Recipes { get; set; }
	public string[]? UnwantedRecipes { get; set; }

	[JsonIgnore]
	public (IDynamicTypeHandler, object?, DynamicType)[]? CachedTypes { get; set; }

	[JsonIgnore]
	public List<IRecipe>? CachedRecipes { get; set; }

}
