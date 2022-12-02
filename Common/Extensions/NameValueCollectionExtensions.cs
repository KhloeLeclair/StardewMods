using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Leclair.Stardew.Common.Extensions;

internal static class NameValueCollectionExtensions {

	internal static int? ReadInt(this NameValueCollection collection, string key) {
		if (collection is not null && collection[key] is string input && int.TryParse(input, out int value))
			return value;
		return null;
	}

	internal static float? ReadFloat(this NameValueCollection collection, string key) {
		if (collection is not null && collection[key] is string input && float.TryParse(input, out float value))
			return value;
		return null;
	}

}
