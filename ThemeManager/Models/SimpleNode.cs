using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leclair.Stardew.ThemeManager.Models;

internal record SimpleNode<T> {

	public T Value { get; }

	public SimpleNode<T>? Parent { get; }

	public List<SimpleNode<T>> Children { get; } = new();

	public SimpleNode(T value, SimpleNode<T>? parent) {
		Value = value;
		Parent = parent;
	}

}
