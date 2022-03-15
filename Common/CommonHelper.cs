using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Xna.Framework;

using StardewValley.Menus;


namespace Leclair.Stardew.Common {
	public static class CommonHelper {

		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
			if (value.CompareTo(min) < 0) return min;
			if (value.CompareTo(max) > 0) return max;
			return value;
		}

		public static Color? ParseColor(string input) {
			if (string.IsNullOrEmpty(input))
				return null;

			System.Drawing.Color color;
			try {
				color = System.Drawing.ColorTranslator.FromHtml(input);
			} catch (Exception) {
				return null;
			}

			return new Color(color.R, color.G, color.B, color.A);
		}

		public static T Cycle<T>(T current, int direction = 1) {
			var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

			int idx = -1;

			for (int i = 0; i < values.Length; i++) {
				if (current == null || current.Equals(values[i])) {
					idx = i + direction;
					break;
				}
			}

			if (idx < 0)
				return values.Last();

			if (idx >= values.Length)
				return values[0];

			return values[idx];
		}

		public static IEnumerable<T> GetValues<T>() {
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static void YeetMenu(IClickableMenu menu) {
			if (menu == null) return;

			MethodInfo CleanupMethod = menu.GetType().GetMethod("cleanupBeforeExit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			menu.behaviorBeforeCleanup?.Invoke(menu);

			if (CleanupMethod != null && CleanupMethod.GetParameters().Length == 0)
				CleanupMethod.Invoke(menu, null);

			if (menu.exitFunction != null) {
				IClickableMenu.onExit exitFunction = menu.exitFunction;
				menu.exitFunction = null;
				exitFunction();
			}
		}

	}
}
