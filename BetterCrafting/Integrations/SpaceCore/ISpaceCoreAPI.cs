#nullable disable

using System;
using System.Reflection;

using StardewValley;

namespace SpaceCore {
	public interface IApi {
		string[] GetCustomSkills();
		int GetLevelForCustomSkill(Farmer farmer, string skill);
		void AddExperienceForCustomSkill(Farmer farmer, string skill, int amt);
		int GetProfessionId(string skill, string profession);

	}
}
