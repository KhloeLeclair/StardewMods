using System.Diagnostics.CodeAnalysis;

using Microsoft.Xna.Framework;

using Newtonsoft.Json;

using Leclair.Stardew.Common;

using Leclair.Stardew.ThemeManager.Serialization;


namespace Leclair.Stardew.ThemeManager.VariableSets;

[JsonConverter(typeof(RealVariableSetConverter))]
public class ColorVariableSet : BaseVariableSet<Color> {

	#region Functions


	#endregion

	public override bool TryParseValue(string input, [NotNullWhen(true)] out Color result) {
		if (CommonHelper.TryParseColor(input, out var res)) {
			result = res.Value;
			return true;
		}

		result = default;
		return false;
	}

	public override bool TryBackupVariable(string key, [NotNullWhen(true)] out Color result) {
		bool tryBase = Manager != null && Manager != ModEntry.Instance.GameThemeManager;
		if (tryBase && ModEntry.Instance.GameTheme?.GetColorVariable(key) is Color cval) {
			result = cval;
			return true;
		}

		result = default;
		return false;
	}
}
