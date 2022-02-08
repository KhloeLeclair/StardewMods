using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using StardewValley.Menus;


namespace Leclair.Stardew.Common {
	public static class CommonHelper {
		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
			if (value.CompareTo(min) < 0) return min;
			if (value.CompareTo(max) > 0) return max;
			return value;
		}

		public static IEnumerable<T> GetValues<T>() {
			return Enum.GetValues(typeof(T)).Cast<T>();
		}


		private static MethodInfo CleanupMethod;

		public static void YeetMenu(IClickableMenu menu) {
			if (CleanupMethod == null) {
				MethodInfo info = menu.GetType().GetMethod("cleanupBeforeExit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (info.GetParameters().Length == 0)
					CleanupMethod = info;
			}

			menu.behaviorBeforeCleanup?.Invoke(menu);
			CleanupMethod?.Invoke(menu, null);

			if (menu.exitFunction != null) {
				IClickableMenu.onExit exitFunction = menu.exitFunction;
				menu.exitFunction = null;
				exitFunction();
			}
		}

	}
}
