using System;

namespace Leclair.Stardew.Common.Types;

internal readonly ref struct BitHelper {

	private readonly Span<int> Span;

	internal BitHelper(Span<int> span, bool clear) {
		if (clear)
			span.Clear();
		Span = span;
	}

	internal void Mark(int position) {
		int idx = position / 32;
		if (idx >= 0 && idx < Span.Length)
			Span[idx] |= 1 << position % 32;
	}

	internal bool IsMarked(int position) {
		int idx = position / 32;
		if (idx >= 0 && idx < Span.Length)
			return (Span[idx] & (1 << position % 32)) != 0;
		return false;
	}

	internal static int ToIntArrayLength(int count) {
		if (count <= 0) return 0;
		return (count - 1) / 32 + 1;
	}

}
