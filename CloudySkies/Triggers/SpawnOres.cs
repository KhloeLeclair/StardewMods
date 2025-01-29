using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;


namespace Leclair.Stardew.CloudySkies;

public static partial class Triggers {

	// Using aliases from Farm Type Manager for user convenience.
	public static readonly Dictionary<string, List<string>> ORE_ALIASES = new() {
		{ "stone", ["668", "670"] },
		{ "geode", ["75"] },
		{ "frozengeode", ["76"] },
		{ "magmageode", ["77"] },
		{ "omnigeode", ["819"] },
		{ "gem", ["2", "4", "6", "8", "10", "12", "14"] },
		{ "copper", ["751"] },
		{ "iron", ["290"] },
		{ "gold", ["764"] },
		{ "iridium", ["765"] },
		{ "mystic", ["46"] },
		{ "radioactive", ["95"] },
		{ "diamond", ["2"] },
		{ "ruby", ["4"] },
		{ "jade", ["6"] },
		{ "amethyst", ["8"] },
		{ "topaz", ["10"] },
		{ "emerald", ["12"] },
		{ "aquamarine", ["14"] },
		{ "mussel", ["25"] },
		{ "fossil", ["816", "817"] },
		{ "clay", ["818"] },
		{ "cindershard", ["843", "844"] },
		{ "coal", ["BasicCoalNode0", "BasicCoalNode1"] },
		{ "volcanocoal", ["VolcanoCoalNode0", "VolcanoCoalNode1"] },
		{ "calicoegg", ["CalicoEggStone_0", "CalicoEggStone_1", "CalicoEggStone_2"] },
		{ "crate", ["922", "923", "924"] }
	};

	public static int? GetOreFragility(string itemId) {
		if (ItemRegistry.IsQualifiedItemId(itemId)) {
			var data = ItemRegistry.GetData(itemId);
			if (!data.HasTypeObject())
				return null;

			itemId = data.ItemId;
		}

		// The following values are all the examples I could find in 1.6.9
		// while searching for values for GetOreHealth.

		switch (itemId) {
			// Beach Crates
			case "922":
			case "923":
			case "924":
				return 2;
		}

		return null;

	}

	public static int? GetOreHealth(string itemId) {
		if (ItemRegistry.IsQualifiedItemId(itemId)) {
			var data = ItemRegistry.GetData(itemId);
			if (!data.HasTypeObject())
				return null;

			itemId = data.ItemId;
		}

		// The following values are all the examples I could find in 1.6.9
		// by browsing the source for calls to the MinutesUntilReady setter.
		// A couple of the methods are kind of nuts though so this is probably
		// not entirely complete / accurate.

		switch (itemId) {
			// Beach Crates
			case "922":
			case "923":
			case "924":
				return 3;

			// Mountain Farm Ores
			case "668":
			case "670":
				return 2;

			case "75":
			case "751":
				return 3;
			case "290":
				return 4;
			case "76":
				return 5;
			case "77":
				return 7;
			case "764":
				return 8;
			case "765":
				return 16;

			// Island Fossils
			case "816":
			case "817":
			case "818": // Clay
				return 4;

			// Island... stones?
			case "32":
			case "38":
			case "40":
			case "42":
				return 2;

			// Island West (Mussels)
			case "25":
				return 8;

			// Mines (getAppropriateOre)
			case "849":
				return 6;
			case "CalicoEggStone_0":
			case "CalicoEggStone_1":
			case "CalicoEggStone_2":
				return 8;

			// Mines (createLitterObject)
			case "95":
				return 25;

			case "31":
			case "33":
			case "34":
			case "35":
			case "36":
			case "37":
			case "39":
			case "41":
				return 1;

			case "55":
			case "56":
			case "57":
			case "58":
			case "762":
			case "760":
				return 4;

			// Gems (still in createLitterObject, which is a total mess)
			case "4":
			case "6":
			case "8":
			case "10":
			case "12":
			case "14":
				return 5;

			// Quarry Day Update
			case "46":
				return 12;
			case "BasicCoalNode0":
			case "BasicCoalNode1":
				return 5;

			// Volcano Dungeon (createStone)
			case "VolcanoCoalNode0":
			case "VolcanoCoalNode1":
				return 10;

			case "845":
			case "846":
			case "847":
				return 6;

			case "843":
			case "844":
				return 12;

			case "VolcanoGoldNode":
			case "819":
				return 8;
		}

		return null;
	}

	[TriggerAction]
	private static bool SpawnOres(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		List<TargetTileFilter> filters = [];
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;
		bool ignoreSpawnable = false;

		int? durability = null;
		int? fragility = null;
		List<string> ores = [];

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<TargetTileFilter>("--filter", filters.Add)
				.WithDescription("Add a filter for selecting valid tiles.")
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of ores to spawn.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tile will have an ore spawned. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.AddFlag("--ignore-spawnable", () => ignoreSpawnable = true)
				.WithDescription("If this flag is set, we will ignore the Spawnable flag of tiles and allow spawning anywhere.")
			.Add<string>("-e", "--entry", val => {
				ores.Add(val);
			})
				.WithDescription("Add a new entry to the list of ores to spawn with this id.")
				.AllowMultiple()
				.IsRequired()
			.Add<int>("-hp", "--health", val => durability = val == 0 ? null : val)
				.WithDescription("If this is present, it overrides the amount of health the ores spawn with.")
				.WithValidation<int>(val => val >= 0, "must be greater than zero, set to zero to clear")
			.Add<int>("-f", "--fragility", val => fragility = val < 0 ? null : val)
				.WithDescription("If this is present, it overrides the fragility the ores spawn with.")
				.WithValidation<int>(val => val >= -1, "must be greater than zero, set to -1 to clear");

		if (!parser.TryParse(args[1..], out error))
			return false;

		if (parser.WantsHelp) {
			Instance.Log($"Usage: {args[0]} {parser.Usage}", LogLevel.Info);
			return true;
		}

		foreach (var entry in targets.SelectMany(x => x)) {
			var loc = entry.Location;
			if (loc is null || (!includeIndoors && !loc.IsOutdoors))
				continue;

			IEnumerable<Vector2> tiles;
			if (entry.Position.HasValue)
				tiles = entry.Position.Value.IterArea(entry.Radius, false);
			else
				tiles = EnumerateAllTiles(loc);

			foreach (var pos in FilterEnumeratedTiles(loc, filters, tiles)) {
				int x = (int) pos.X;
				int y = (int) pos.Y;

				if (!(chance >= 1f || Game1.random.NextSingle() <= chance))
					continue;

				if (loc.Objects.ContainsKey(pos) ||
					loc.IsNoSpawnTile(pos) ||
					(!ignoreSpawnable && loc.doesTileHaveProperty(x, y, "Spawnable", "Back") == null) ||
					loc.doesEitherTileOrTileIndexPropertyEqual(x, y, "Spawnable", "Back", "F") ||
					!loc.CanItemBePlacedHere(pos) ||
					loc.getTileIndexAt(x, y, "AlwaysFront") != -1 ||
					loc.getTileIndexAt(x, y, "AlwaysFront2") != -1 ||
					loc.getTileIndexAt(x, y, "AlwaysFront3") != -1 ||
					loc.getTileIndexAt(x, y, "Front") != -1 ||
					loc.isBehindBush(pos)
				)
					continue;

				string toSpawnId = Game1.random.ChooseFrom(ores);
				if (ORE_ALIASES.TryGetValue(toSpawnId, out var aliases))
					toSpawnId = Game1.random.ChooseFrom(aliases);

				if (ItemRegistry.Create(toSpawnId, amount: 1, allowNull: true) is not SObject spawned)
					continue;

				int? health;
				int? frag;

				if (ModEntry.Instance.intIE?.IsResource(toSpawnId, out health) ?? false) {
					frag = null;
				} else {
					health = GetOreHealth(toSpawnId);
					frag = GetOreFragility(toSpawnId);
				}

				if (durability.HasValue)
					spawned.MinutesUntilReady = durability.Value;
				else if (health.HasValue)
					spawned.MinutesUntilReady = health.Value;

				if (fragility.HasValue)
					spawned.Fragility = fragility.Value;
				else if (frag.HasValue)
					spawned.Fragility = frag.Value;

				if (loc.Objects.TryAdd(pos, spawned)) {
					max--;
					if (max <= 0)
						break;
				}
			}

			if (max <= 0)
				break;
		}

		// Great success!
		error = null;
		return true;
	}

}
