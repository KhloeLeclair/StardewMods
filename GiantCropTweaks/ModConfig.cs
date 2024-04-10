using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leclair.Stardew.GiantCropTweaks;

public enum AllowedLocations {
	BaseGame,
	BaseAndIsland,
	Anywhere
};

public class ModConfig {

	public AllowedLocations AllowedLocations { get; set; } = AllowedLocations.BaseAndIsland;

}
