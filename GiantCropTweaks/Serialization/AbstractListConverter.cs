using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Leclair.Stardew.GiantCropTweaks.Serialization;

public class AbstractListConverter<TReal, TAbstract> : JsonConverter where TReal : TAbstract {

	public override bool CanConvert(Type objectType) {
		return objectType == typeof(List<TAbstract>);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
		List<TReal>? values = serializer.Deserialize<List<TReal>>(reader);
		if (values is null)
			return null;

		List<TAbstract> result = new(values.Count);
		foreach (TReal value in values)
			result.Add(value);

		return result;
	}

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
		throw new NotImplementedException();
	}

	public override bool CanWrite => false;

}
