using System;

using Leclair.Stardew.CloudySkies.Models;
using Leclair.Stardew.Common.Serialization.Converters;
using Leclair.Stardew.Common.Types;

using Newtonsoft.Json;

namespace Leclair.Stardew.CloudySkies.Serialization;

public class EffectDataConverter : JsonConverter {

	private static readonly CaseInsensitiveDictionary<Type> Types = new();

	private static readonly DiscriminatingConverter<BaseEffectData> Converter;

	static EffectDataConverter() {
		Converter = new("Type", Types, "Custom");
		Converter.PopulateTypes();
	}

	public static bool RegisterType(string key, Type type) {
		if (!type.IsAssignableFrom(typeof(BaseEffectData)))
			throw new InvalidCastException($"{type} is not a subclass of {typeof(BaseEffectData)}");

		return Types.TryAdd(key, type);
	}

	public override bool CanConvert(Type objectType) {
		return Converter.CanConvert(objectType);
	}

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
		return Converter.ReadJson(reader, objectType, existingValue, serializer);
	}

	public override bool CanWrite => Converter.CanWrite;

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
		Converter.WriteJson(writer, value, serializer);
	}

}
