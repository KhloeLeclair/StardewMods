#nullable enable

using System;

namespace Leclair.Stardew.Common.UI;

[Flags]
public enum Alignment {
	None = 0,

	// Horizontal
	Left = 1,
	Center = 2,
	Right = 4,

	// Vertical
	Top = 8,
	Middle = 16,
	Bottom = 32
}

public static class AlignmentHelper {

	public static readonly Alignment HORIZONTAL = Alignment.Left | Alignment.Center | Alignment.Right;
	public static readonly Alignment VERTICAL = Alignment.Top | Alignment.Middle | Alignment.Bottom;

	public static Alignment With(this Alignment self, Alignment other) {
		if ((HORIZONTAL & other) != 0)
			return (self & ~HORIZONTAL) | other;

		if ((VERTICAL & other) != 0)
			return (self & ~VERTICAL) | other;

		return self;
	}

}
