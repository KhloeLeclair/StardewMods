#nullable enable

using System;
using System.Collections.Generic;

namespace Leclair.Stardew.Common.Types;

public class CaseInsensitiveDictionary<TValue> : Dictionary<string, TValue> {

	public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveDictionary(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveDictionary(IEnumerable<KeyValuePair<string, TValue>> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveDictionary(IDictionary<string, TValue> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }

}
