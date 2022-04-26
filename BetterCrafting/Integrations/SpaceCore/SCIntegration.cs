#nullable enable

using Leclair.Stardew.Common.Integrations;

using SpaceCore;

using StardewValley;

namespace Leclair.Stardew.BetterCrafting.Integrations.SpaceCore;

public class SCIntegration : BaseAPIIntegration<IApi, ModEntry> {

	public SCIntegration(ModEntry mod)
	: base(mod, "spacechase0.SpaceCore", "1.8.1") { }

	public void AddCustomSkillExperience(Farmer farmer, string skill, int amt) {
		if (!IsLoaded)
			return;

		API.AddExperienceForCustomSkill(farmer, skill, amt);
	}

	public int GetCustomSkillLevel(Farmer farmer, string skill) {
		if (!IsLoaded)
			return 0;

		return API.GetLevelForCustomSkill(farmer, skill);
	}
}
