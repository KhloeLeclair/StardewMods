using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Leclair.Stardew.BetterCrafting.Models;

public interface IDynamicType {

	public string Id { get; }

	public IDictionary<string, JToken> Fields { get; }

}

public class DynamicType : IDynamicType {

	public string Id { get; set; } = string.Empty;

	[JsonExtensionData]
	public IDictionary<string, JToken> Fields { get; set; } = new Dictionary<string, JToken>();

}
