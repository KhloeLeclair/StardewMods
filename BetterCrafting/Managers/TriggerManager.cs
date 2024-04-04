using System;
using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.BetterCrafting.Menus;
using Leclair.Stardew.Common;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Objects;
using StardewValley.Triggers;

using static System.Collections.Specialized.BitVector32;

namespace Leclair.Stardew.BetterCrafting.Managers;

public class TriggerManager: BaseManager {

	public TriggerManager(ModEntry mod) : base(mod) {

		TriggerActionManager.RegisterAction("leclair.bettercrafting_OpenMenu", Trigger_OpenMenu);

		GameLocation.RegisterTileAction("leclair.bettercrafting_OpenMenu", Map_OpenMenu);

		GameStateQuery.Register("leclair.bettercrafting_HAS_WORKBENCH", HAS_WORKBENCH);

	}

	private static bool FindBenchRecursive(GameLocation where) {
		if (where.Objects.Values.Any(obj => obj is Workbench))
			return true;

		foreach(var building in where.buildings) {
			if (building.GetIndoors() is GameLocation interior && FindBenchRecursive(interior))
				return true;
		}

		return false;
	}

	public static bool HAS_WORKBENCH(string[] query, GameStateQueryContext context) {
		Farm farm = Game1.getFarm();
		return FindBenchRecursive(farm);
	}

	public bool Map_OpenMenu(GameLocation location, string[] args, Farmer who, Point pos) {
		if (!ArgUtility.TryGetBool(args, 1, out bool cooking, out string error))
			return false;

		if (!ArgUtility.TryGetBool(args, 2, out bool includeBuildings, out error))
			return false;

		string? station = args.Length >= 4 ? args[3] : null;

		if (Game1.player != who)
			return false;

		if (!string.IsNullOrEmpty(station) && !Mod.Stations.IsStation(station)) {
			Log($"Tried to open invalid station: {station}", StardewModdingAPI.LogLevel.Warn);
			return false;
		}

		//Log($"OpenMenu {who}, {pos}, {location}, {cooking}, {includeBuildings}", StardewModdingAPI.LogLevel.Debug);

		Game1.activeClickableMenu = BetterCraftingPage.Open(
			Mod,
			station: station,
			location: location,
			position: pos.ToVector2(),
			standalone_menu: true,
			cooking: cooking,
			material_containers: (IList<LocatedInventory>?) null,
			discover_buildings: includeBuildings
		);

		return true;
	}

	public bool Trigger_OpenMenu(string[] args, TriggerActionContext ctx, out string? error) {
		if (!ArgUtility.TryGetBool(args, 1, out bool cooking, out error))
			return false;

		if (!ArgUtility.TryGetBool(args, 2, out bool includeBuildings, out error))
			return false;

		string? station = args.Length >= 4 ? args[3] : null;

		if (!string.IsNullOrEmpty(station) && !Mod.Stations.IsStation(station)) {
			Log($"Tried to open invalid station: {station}", StardewModdingAPI.LogLevel.Warn);
			return false;
		}

		Game1.activeClickableMenu = BetterCraftingPage.Open(
			Mod,
			station: station,
			standalone_menu: true,
			cooking: cooking,
			material_containers: (IList<LocatedInventory>?) null,
			discover_buildings: includeBuildings
		);

		return true;
	}

}
