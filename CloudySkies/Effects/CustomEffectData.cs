using System.Collections.Generic;

using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.Common.Serialization.Converters;
using Leclair.Stardew.Common.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Leclair.Stardew.CloudySkies.Effects;

[DiscriminatedType("Custom")]
public record CustomEffectData : BaseEffectData, ICustomEffectData {

	[JsonExtensionData]
	public IDictionary<string, JToken> Fields { get; set; } = new ValueEqualityDictionary<string, JToken>();

}
