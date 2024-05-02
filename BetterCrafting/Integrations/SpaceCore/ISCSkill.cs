using System;

using Microsoft.Xna.Framework.Graphics;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public interface ISCSkill {

	string Id { get; }

	string GetName();

	Texture2D Icon { get; }

	Texture2D SkillsPageIcon { get; }

}

public static class ISCSkill_Extensions {

	public static string SafeGetName(this ISCSkill skill) {
		try {
			return skill.GetName() ?? skill.Id;
		} catch (Exception ex) {
			ModEntry.Instance.Log($"Unable to access SpaceCore custom skill. Did the mod author make it internal or private?", StardewModdingAPI.LogLevel.Warn, ex, once: true);
			return skill.Id;
		}
	}

	public static Texture2D? SafeGetTexture(this ISCSkill? skill) {
		try {
			return skill?.SkillsPageIcon;
		} catch (Exception ex) {
			ModEntry.Instance.Log($"Unable to access SpaceCore custom skill. Did the mod author make it internal or private?", StardewModdingAPI.LogLevel.Warn, ex, once: true);
			return null;
		}
	}

}
