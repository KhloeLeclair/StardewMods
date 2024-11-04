using System.Diagnostics.CodeAnalysis;

using Leclair.Stardew.ThemeManager.Serialization;

using Newtonsoft.Json;


namespace Leclair.Stardew.ThemeManager.VariableSets;

[JsonConverter(typeof(RealVariableSetConverter))]
public class FloatVariableSet : BaseVariableSet<float> {

	public override bool TryParseValue(string input, [NotNullWhen(true)] out float result) {
		if (float.TryParse(input, out result))
			return true;

		result = default;
		return false;
	}

	public override bool TryBackupVariable(string key, [NotNullWhen(true)] out float result) {
		bool tryBase = Manager != null && Manager != ModEntry.Instance.GameThemeManager;
		if (tryBase && ModEntry.Instance.GameTheme != null && ModEntry.Instance.GameTheme.TryGetColorAlphaVariable(key, out result))
			return true;

		result = default;
		return false;
	}
}
