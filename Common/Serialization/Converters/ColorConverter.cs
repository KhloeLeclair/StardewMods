using System;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using StardewValley;

namespace Leclair.Stardew.Common.Serialization.Converters {
	public class ColorConverter : JsonConverter {
		public override bool CanConvert(Type objectType) {
			// This will get easier in 1.6. For now we only care about SObjects.
			return typeof(Color?).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			string path = reader.Path;
			switch (reader.TokenType) {
				case JsonToken.Null:
					return null;
				case JsonToken.String:
					return ReadString(JToken.Load(reader).Value<string>(), path);
				case JsonToken.StartObject:
					return ReadObject(JObject.Load(reader));
				default:
					throw new JsonReaderException($"Can't parse Color? from {reader.TokenType} node (path: {reader.Path}).");
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
			if (value is not Color color) { 
				writer.WriteNull();
				return;
			}

			var jo = new JObject {
				{"R", color.R },
				{"G", color.G },
				{"B", color.B },
				{"A", color.A }
			};

			jo.WriteTo(writer);
		}

		private Color? ReadString(string value, string path) {
			return CommonHelper.ParseColor(value);
		}

		private Color? ReadObject(JObject obj) {

			if (!obj.TryGetValueIgnoreCase("R", out byte R) ||
				!obj.TryGetValueIgnoreCase("G", out byte G) ||
				!obj.TryGetValueIgnoreCase("B", out byte B)
			)
				return null;

			if (obj.TryGetValueIgnoreCase("A", out byte A))
				return new Color(R, G, B, A);

			return new Color(R, G, B);
		}
	}
}
