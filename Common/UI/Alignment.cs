#if (COMMON_SIMPLELAYOUT || COMMON_FLOW)

using System;

namespace Leclair.Stardew.Common.UI;

[Flags]
public enum Alignment {
	None = 0,

	// Horizontal
	Left = 1,
	HCenter = 2,
	Right = 4,

	// Vertical
	Top = 8,
	VCenter = 16,
	Bottom = 32,

	// Absolute Center
	Center = HCenter | VCenter
}

public static class AlignmentHelper {

	public static readonly Alignment HORIZONTAL = Alignment.Left | Alignment.HCenter | Alignment.Right;
	public static readonly Alignment VERTICAL = Alignment.Top | Alignment.VCenter | Alignment.Bottom;

	public static Alignment With(this Alignment self, Alignment other) {
		if ((HORIZONTAL & other) != 0)
			return (self & ~HORIZONTAL) | other;

		if ((VERTICAL & other) != 0)
			return (self & ~VERTICAL) | other;

		return self;
	}

}

#endif
