using System;
using System.Collections.Generic;

namespace Leclair.Stardew.Common.Types;

public class ValueEqualityList<TValue> : List<TValue> {

	private readonly IEqualityComparer<TValue>? equalityComparer;

	public IEqualityComparer<TValue> Comparer => equalityComparer ?? EqualityComparer<TValue>.Default;

	public ValueEqualityList() : base() { }

	public ValueEqualityList(int capacity) : base(capacity) { }

	public ValueEqualityList(IEnumerable<TValue> values) : base(values) { }

	public override bool Equals(object? obj) {
		if (obj is not IList<TValue> olist || olist.Count != Count)
			return false;

		IEqualityComparer<TValue> comparer = Comparer;

		for (int i = 0; i < Count; i++) {
			if (!comparer.Equals(this[i], olist[i]))
				return false;
		}

		return true;
	}

	public override int GetHashCode() {
		var hash = new HashCode();
		foreach (var item in this)
			hash.Add(item, Comparer);
		return hash.ToHashCode();
	}

}
