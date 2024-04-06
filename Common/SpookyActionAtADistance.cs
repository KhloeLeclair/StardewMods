using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.Common.Events;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Network;

namespace Leclair.Stardew.Common;

public class SpookyActionAtADistance: EventSubscriber<ModSubscriber> {

	private static SpookyActionAtADistance? Instance;

	public readonly string ModId;

	private readonly Dictionary<string, List<long>> OpenedLocations = new();
	private readonly Dictionary<long, List<string>> PlayerLocations = new();

	public SpookyActionAtADistance(ModSubscriber mod, string? uniqueId = null) : base(mod) {
		Instance = this;
		ModId = uniqueId ?? mod.ModManifest.UniqueID;
	}

	#region Harmony

	public void PatchGame(Harmony harmony) {

		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(NetMutex), nameof(NetMutex.Update), new Type[] { typeof(FarmerCollection) }),
				prefix: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Mutex_Update_Prefix))
			);
		} catch(Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the NetMutex.Update", LogLevel.Warn, ex);
		}

		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(Game1), "_UpdateLocation"),
				transpiler: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Game1_UpdateLocation_Transpiler))
			);
		} catch(Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the Game1._UpdateLocation", LogLevel.Warn, ex);
		}

	}

	public static IEnumerable<CodeInstruction> Game1_UpdateLocation_Transpiler(IEnumerable<CodeInstruction> instructions) {

		var instrs = instructions.ToArray();
		var method = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation));
		var our_method = AccessTools.Method(typeof(SpookyActionAtADistance), nameof(ShouldLocationUpdate));

		bool inserted = false;

		for(int i = 0; i < instrs.Length; i++) {
			CodeInstruction in0 = instrs[i];

			if (! inserted && i + 4 < instrs.Length) {
				CodeInstruction in1 = instrs[i + 1];
				CodeInstruction in2 = instrs[i + 2];
				CodeInstruction in3 = instrs[i + 3];
				CodeInstruction in4 = instrs[i + 4];

				// The longest IF statement.
				// To detect:
				// if ( should_update ) {
				//     location.UpdateWhenCurrentLocation(time);
				// }

				if (   in0.opcode == OpCodes.Ldloc_0
					&& in1.opcode == OpCodes.Brfalse_S
					&& in2.opcode == OpCodes.Ldarg_1
					&& in3.opcode == OpCodes.Ldarg_2
					&& in4.opcode == OpCodes.Callvirt
					&& in4.operand is MethodInfo minfo && minfo == method
				) {
					// We just want to add the following code before that:
					// should_update |= ShouldLocationUpdate(location);
					inserted = true;

					yield return in0; // yield the Ldloc.0 with the label that gets jumped to.

					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, our_method);
					yield return new CodeInstruction(OpCodes.Or);

					// From here it will continue like normal with the Brfalse using or OR'd value.

					//yield return new CodeInstruction(OpCodes.Stloc_0);
					//yield return new CodeInstruction(OpCodes.Ldloc_0); // yield a fresh Ldloc.0 and continue
					continue;
				}
			}

			yield return in0;
		}

		if (!inserted)
			throw new Exception("Failed to insert our method.");

	}

	public static void Mutex_Update_Prefix(NetMutex __instance, ref FarmerCollection farmers) {

		try {
			var field = Instance?.Mod?.Helper?.Reflection?.GetField<GameLocation>(farmers, "_locationFilter", false);
			if (field != null) {
				var value = field.GetValue();
				if (value != null && ShouldLocationUpdate(value))
					farmers = Game1.getOnlineFarmers();
			}

		} catch(Exception ex) {
			Instance?.Mod?.Log("Error inside Mutex.Update prefix.", LogLevel.Error, ex, once: true);
		}

	}

	public static bool ShouldLocationUpdate(GameLocation location) {
		return Instance?.OpenedLocations?.ContainsKey(location.NameOrUniqueName) ?? false;
	}

	#endregion

	#region Events

	[Subscriber]
	private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e) {
		long playerId = e.Peer.PlayerID;

		if (PlayerLocations.TryGetValue(playerId, out var locs))
			RemoveWatches(playerId, locs);
	}

	[Subscriber]
	private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e) {
		if (e.FromModID != ModId)
			return;

		if (e.Type == "SpookyAction:Watch") {
			UpdateWatching msg = e.ReadAs<UpdateWatching>();

			string joined = string.Join(", ", msg.locations);
			Mod.Log($"Got Watch from {e.FromPlayerID}: {joined}", LogLevel.Trace);
			AddWatches(e.FromPlayerID, msg.locations);

			
		} else if ( e.Type == "SpookyAction:Unwatch") {
			UpdateWatching msg = e.ReadAs<UpdateWatching>();

			string joined = string.Join(", ", msg.locations);
			Mod.Log($"Got UnWatch from {e.FromPlayerID}: {joined}", LogLevel.Trace);
			RemoveWatches(e.FromPlayerID, msg.locations);
		}
	}

	#endregion

	#region Internals

	private List<string> AddWatches(long playerId, IEnumerable<GameLocation?> locations) {
		return AddWatches(playerId, locations.Select(loc => loc?.NameOrUniqueName));
	}

	private List<string> AddWatches(long playerId, IEnumerable<string?> locations) {

		if (!PlayerLocations.TryGetValue(playerId, out var watched)) {
			watched = new List<string>();
			PlayerLocations[playerId] = watched;
		}

		List<string> added = new();

		foreach(string? location in locations) {
			if (! string.IsNullOrEmpty(location) && ! watched.Contains(location)) {
				watched.Add(location);
				added.Add(location);

				if (OpenedLocations.TryGetValue(location, out var watchers))
					watchers.Add(playerId);
				else
					OpenedLocations[location] = new List<long>() { playerId };
			}
		}

		return added;
	}

	private List<string> RemoveWatches(long playerId, IEnumerable<GameLocation?> locations) {
		return RemoveWatches(playerId, locations.Select(loc => loc?.NameOrUniqueName));
	}

	private List<string> RemoveWatches(long playerId, IEnumerable<string?> locations) {

		List<string> removed = new();

		if (!PlayerLocations.TryGetValue(playerId, out var watched))
			return removed;

		foreach(string? location in locations) {
			if (! string.IsNullOrEmpty(location) && watched.Contains(location)) { 
				watched.Remove(location);
				removed.Add(location!);

				if (OpenedLocations.TryGetValue(location, out var watchers)) {
					watchers.Remove(playerId);
					if ( watchers.Count == 0 )
						OpenedLocations.Remove(location);
				}
			}
		}

		if (watched.Count == 0)
			PlayerLocations.Remove(playerId);

		return removed;
	}

	private long GetHostId() {
		if (Context.IsMainPlayer)
			return Game1.player.UniqueMultiplayerID;

		foreach (var peer in Mod.Helper.Multiplayer.GetConnectedPlayers())
			if (peer.IsHost)
				return peer.PlayerID;

		throw new IndexOutOfRangeException("Unable to find host.");
	}

	#endregion

	#region API

	public void WatchLocations(IEnumerable<GameLocation?> locations, Farmer? who = null) {
		// First, we need to update our local data structures.
		who ??= Game1.player;
		long playerId = who.UniqueMultiplayerID;

		var added = AddWatches(playerId, locations);
		string joined = string.Join(", ", added);
		Mod.Log($"Self Watch from {playerId}: {joined}", LogLevel.Trace);

		// We also need to update the host.
		if (!Context.IsMainPlayer && added.Count > 0) {
			Mod.Log($"Sending Watch to Host: {joined}", LogLevel.Trace);
			Mod.Helper.Multiplayer.SendMessage(
				new UpdateWatching(added.ToArray()),
				"SpookyAction:Watch",
				null,
				// Only send to the host. No one else needs to know.
				new[] { GetHostId() }
			);
		}
	}

	public void UnwatchLocations(IEnumerable<GameLocation?> locations, Farmer? who = null) {
		// First, we need to update our local data structures.
		who ??= Game1.player;
		long playerId = who.UniqueMultiplayerID;

		var removed = RemoveWatches(playerId, locations);
		string joined = string.Join(", ", removed);
		Mod.Log($"Self UnWatch from {playerId}: {joined}", LogLevel.Trace);

		// We also need to update the host.
		if (!Context.IsMainPlayer && removed.Count > 0) {
			Mod.Log($"Sending UnWatch to Host: {joined}", LogLevel.Trace);
			Mod.Helper.Multiplayer.SendMessage(
				new UpdateWatching(removed.ToArray()),
				"SpookyAction:Unwatch",
				null,
				// Only send to the host. No one else needs to know.
				new[] { GetHostId() }
			);
		}
	}

	#endregion

	public record struct UpdateWatching(
		string[] locations
	);

}

