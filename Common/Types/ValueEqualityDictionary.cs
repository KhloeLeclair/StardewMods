
using System;
using System.Collections.Generic;

namespace Leclair.Stardew.Common.Types;

/// <summary>
/// This is a <see cref="Dictionary{TKey, TValue}"/> subclass that overrides
/// <see cref="Equals(object?)"/> and <see cref="GetHashCode"/> to use
/// value-based equality checking. This allows you to use this dictionary
/// within record objects and still have robust equality checking behavior.
/// </summary>
public class ValueEqualityDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull {

	public ValueEqualityDictionary() : base() { }
	public ValueEqualityDictionary(IEqualityComparer<TKey>? comparer) : base(comparer) { }
	public ValueEqualityDictionary(int capacity) : base(capacity) { }
	public ValueEqualityDictionary(int capacity, IEqualityComparer<TKey>? comparer) : base(capacity, comparer) { }
	public ValueEqualityDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
	public ValueEqualityDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer) : base(dictionary, comparer) { }
	public ValueEqualityDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }
	public ValueEqualityDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) : base(collection, comparer) { }

	public override bool Equals(object? obj) {

		if (obj is not IDictionary<TKey, TValue> odict || odict.Count != Count)
			return false;

		foreach (var pair in this)
			if (!odict.TryGetValue(pair.Key, out TValue? other) || !EqualityComparer<TValue>.Default.Equals(pair.Value, other))
				return false;

		return true;
	}

	public override int GetHashCode() {
		var hash = new HashCode();
		foreach (var pair in this) {
			hash.Add(pair.Key);
			hash.Add(pair.Value);
		}
		return hash.ToHashCode();
	}
}
