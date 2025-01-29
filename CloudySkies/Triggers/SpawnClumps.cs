using System.Collections.Generic;
using System.Linq;

using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Events;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;


namespace Leclair.Stardew.CloudySkies;

public static partial class Triggers {

	public const string OBJECTS2 = @"TileSheets\Objects_2";

	// string -> (parentSheetIndex, health, texture)
	public static readonly Dictionary<string, (int, int?, string?)> VANILLA_CLUMPS = new() {

	};

	static Triggers() {
		foreach (var data in new (string, int)[] {
			("greenRainBush1", ResourceClump.greenRainBush1Index),
			("greenRainBush2", ResourceClump.greenRainBush2Index)
		}) {
			VANILLA_CLUMPS[data.Item1] = (data.Item2, 4, OBJECTS2);
			VANILLA_CLUMPS[$"{data.Item2}"] = (data.Item2, 4, OBJECTS2);
		}

		foreach (var data in new (string, int)[] {
			("stump", ResourceClump.stumpIndex),
			("hollowLog", ResourceClump.hollowLogIndex),
			("meteorite", ResourceClump.meteoriteIndex),
			("boulder", ResourceClump.boulderIndex),
			("mineRock1", ResourceClump.mineRock1Index),
			("mineRock2", ResourceClump.mineRock2Index),
			("mineRock3", ResourceClump.mineRock3Index),
			("mineRock4", ResourceClump.mineRock4Index),
		}) {
			VANILLA_CLUMPS[data.Item1] = (data.Item2, null, null);
			VANILLA_CLUMPS[$"{data.Item2}"] = (data.Item2, null, null);
		}

		foreach (var data in new (string, int)[] {
			("quarryBoulder", ResourceClump.quarryBoulderIndex)
		}) {
			VANILLA_CLUMPS[data.Item1] = (data.Item2, null, OBJECTS2);
			VANILLA_CLUMPS[$"{data.Item2}"] = (data.Item2, null, OBJECTS2);
		}
	}

	private static bool IsValidClumpId(string id) {
		if (VANILLA_CLUMPS.ContainsKey(id))
			return true;

		return ModEntry.Instance.intIE?.IsClump(id) ?? false;
	}

	[TriggerAction]
	private static bool SpawnClumps(string[] args, TriggerActionContext context, out string? error) {
		List<IEnumerable<TargetPosition>> targets = [];
		List<TargetTileFilter> filters = [];
		float chance = 1f;
		int max = int.MaxValue;
		bool includeIndoors = false;
		bool ignoreSpawnable = false;

		List<string> clumps = [];

		var parser = ArgumentParser.New()
			.AddHelpFlag()
			.AddPositional<IEnumerable<TargetPosition>>("Target", targets.Add)
				.IsRequired()
				.AllowMultiple()
			.Add<TargetTileFilter>("--filter", filters.Add)
				.WithDescription("Add a filter for selecting valid tiles.")
			.Add<int>("--max", null, val => max = val)
				.WithDescription("The maximum number of clumps to spawn.")
				.WithValidation<int>(val => val > 0, "must be greater than 0")
			.Add<float>("-c", "--chance", val => chance = val)
				.WithDescription("The percent chance that any given tile will have a clump spawned. Default: 1.0")
				.WithValidation<float>(val => val >= 0 && val <= 1, "must be value in range 0.0 to 1.0")
			.AddFlag("--indoors", () => includeIndoors = true)
				.WithDescription("If this flag is set, indoor locations will not be skipped.")
			.AddFlag("--ignore-spawnable", () => ignoreSpawnable = true)
				.WithDescription("If this flag is set, we will ignore the Spawnable flag of tiles and allow spawning anywhere.")
			.Add<string>("-e", "--entry", val => {
				clumps.Add(val);
			})
				.WithDescription("Add a new entry to the list of clumps to spawn with this id.")
				.WithValidation<string>(IsValidClumpId, "unknown or invalid clump id")
				.AllowMultiple()
				.IsRequired();

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

				// Assume a clump is 2x2.
				bool spawnable = true;
				for (int offX = 0; offX < 2; offX++) {
					for (int offY = 0; offY < 2; offY++) {
						int x2 = x + offX;
						int y2 = y + offY;
						Vector2 p2 = new(x2, y2);

						if (loc.Objects.ContainsKey(p2) ||
							loc.IsNoSpawnTile(p2) ||
							(!ignoreSpawnable && loc.doesTileHaveProperty(x2, y2, "Spawnable", "Back") == null) ||
							loc.doesEitherTileOrTileIndexPropertyEqual(x2, y2, "Spawnable", "Back", "F") ||
							!loc.CanItemBePlacedHere(p2) ||
							loc.getTileIndexAt(x2, y2, "AlwaysFront") != -1 ||
							loc.getTileIndexAt(x2, y2, "AlwaysFront2") != -1 ||
							loc.getTileIndexAt(x2, y2, "AlwaysFront3") != -1 ||
							loc.getTileIndexAt(x2, y2, "Front") != -1 ||
							loc.isBehindBush(p2)
						) {
							spawnable = false;
							break;
						}
					}
					if (!spawnable)
						break;
				}

				if (!spawnable)
					continue;

				string toSpawnId = Game1.random.ChooseFrom(clumps);
				bool spawned = false;

				if (VANILLA_CLUMPS.TryGetValue(toSpawnId, out var data)) {
					var clump = new ResourceClump(data.Item1, 2, 2, pos, data.Item2, data.Item3);
					loc.resourceClumps.Add(clump);

				} else if (ModEntry.Instance.intIE?.TrySpawnClump(toSpawnId, pos, loc, out string? err) ?? false) {
					spawned = true;
				}

				if (spawned) {
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
