#nullable enable

using System;
using System.Collections.Generic;

namespace Leclair.Stardew.Common.Types;

public class CaseInsensitiveDictionary<T> : Dictionary<string, T> {

	public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveDictionary(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveDictionary(IEnumerable<KeyValuePair<string, T>> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }

	public CaseInsensitiveDictionary(IDictionary<string, T> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }

}
