#nullable enable

using StardewModdingAPI;

namespace Leclair.Stardew.Common;

internal static class TranslationHelper {

	internal static bool ContainsKey(this ITranslationHelper helper, string key) {

		return helper.Get(key).HasValue();

		// This is so ugly.
		/*FieldInfo? field = helper.GetType()
			.GetField("Translator", BindingFlags.Instance | BindingFlags.NonPublic);

		if (field == null)
			return false;

		object Translator = field.GetValue(helper);
		if (Translator == null)
			return false;

		FieldInfo flfield = Translator.GetType()
			.GetField("ForLocale", BindingFlags.Instance | BindingFlags.NonPublic);

		if (flfield == null)
			return false;

		return flfield.GetValue(Translator) is
			IDictionary<string, Translation> ForLocale &&
			ForLocale.ContainsKey(key);*/
	}

}
