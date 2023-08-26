using System;
using System.Collections.Generic;
using System.Text;

namespace Leclair.Stardew.Common.Extensions;

internal static class RandomExtensions {

	public static bool GetChance(this Random rnd, double chance) {
		if (chance < 0.0) return false;
		if (chance >= 1.0) return true;
		return rnd.NextDouble() < chance;
	}

}
