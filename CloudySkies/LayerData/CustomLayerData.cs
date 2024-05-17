using System.Collections.Generic;

using Leclair.Stardew.Common.Serialization.Converters;
using Leclair.Stardew.Common.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Leclair.Stardew.CloudySkies.LayerData;

[DiscriminatedType("Custom")]
public record CustomLayerData : BaseLayerData, ICustomLayerData {

	[JsonExtensionData]
	public IDictionary<string, JToken> Fields { get; init; } = new FieldsEqualityDictionary();

}
