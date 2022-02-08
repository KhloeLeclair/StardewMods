using System;

namespace Leclair.Stardew.Common.UI {
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
		public static bool HasFlag(Alignment alignment, Alignment flag) {
			return (alignment & flag) == flag;
		}
	}
}
