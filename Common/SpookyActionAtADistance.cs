using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#if HARMONY
using HarmonyLib;
#endif

using Leclair.Stardew.Common.Events;
using Leclair.Stardew.Common.UI;

using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

using StardewValley;
using StardewValley.Network;

namespace Leclair.Stardew.Common;

public class SpookyActionAtADistance: EventSubscriber<ModSubscriber> {

	private static SpookyActionAtADistance? Instance;

	public readonly string ModId;

	private readonly Dictionary<string, HashSet<long>> OpenedLocations = new();
	private readonly Dictionary<long, HashSet<string>> PlayerLocations = new();

#if DEBUG
	private bool ShowLocations = false;
	private readonly HashSet<GameLocation> TickedLocations = new();
#endif

	public SpookyActionAtADistance(ModSubscriber mod, string? uniqueId = null) : base(mod) {
		Instance = this;

		ModId = uniqueId ?? mod.ModManifest.UniqueID;
	}

	#region Harmony

#if HARMONY

	public void PatchGame(Harmony harmony) {

		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(NetMutex), nameof(NetMutex.Update), new Type[] { typeof(FarmerCollection) }),
				//prefix: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Mutex_Update_Prefix)),
				transpiler: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Mutex_Update_Transpiler))
			);
		} catch(Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the NetMutex.Update", LogLevel.Warn, ex);
		}

		/*try {
			harmony.Patch(
				original: AccessTools.Method(typeof(NetMutex), "<.ctor>b__9_0"),
				prefix: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Mutex_Delegate_Prefix))
			);
		} catch (Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the NetMutex.ReleaseLock", LogLevel.Warn, ex);
		}

		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(NetMutex), nameof(NetMutex.ReleaseLock)),
				prefix: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Mutex_Release_Prefix))
			);
		} catch (Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the NetMutex.ReleaseLock", LogLevel.Warn, ex);
		}*/

		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(Game1), "_UpdateLocation"),
				transpiler: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Game1_UpdateLocation_Transpiler))
			);
		} catch(Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the Game1._UpdateLocation", LogLevel.Warn, ex);
		}

#if DEBUG
		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation)),
				postfix: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(GameLocation_Update_Postfix))
			);
		} catch(Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the GameLocation.UpdateWhenCurrentLocation", LogLevel.Warn, ex);
		}
#endif

		/*
		try {
			harmony.Patch(
				original: AccessTools.Method(typeof(Multiplayer), nameof(Multiplayer.updateRoots)),
				transpiler: new HarmonyMethod(typeof(SpookyActionAtADistance), nameof(Multiplayer_updateRoots_Transpiler))
			);

		} catch (Exception ex) {
			Mod.Log("An error occurred while registering a harmony patch for the Multiplayer.updateRoots", LogLevel.Warn, ex);
		}*/

	}

#if DEBUG
	[Subscriber]
	private void OnRendered(object? sender, RenderedHudEventArgs e) {

		var maps = TickedLocations.Select(x => x.NameOrUniqueName).ToList();
		TickedLocations.Clear();

		if (ShowLocations && maps.Count > 0)
			SimpleHelper.Builder()
				.Text(string.Join(", ", maps))
				.GetLayout()
				.DrawHover(e.SpriteBatch, Game1.smallFont, overrideX: 0, overrideY: 0);
	}

	[Subscriber]
	private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
		if (!Context.IsWorldReady)
			return;

		if (KeybindList.Parse("F8").JustPressed())
			ShowLocations = !ShowLocations;
	}

	public static void GameLocation_Update_Postfix(GameLocation __instance) {
		Instance?.TickedLocations.Add(__instance);
	}

#endif

	/*
	public static void Mutex_Delegate_Prefix(NetMutex __instance, long playerId) {
		Instance?.Mod?.Log($"Mutex lock request: {__instance.GetHashCode()}; isMaster: {Game1.IsMasterGame}; pId: {playerId}", LogLevel.Debug);
	}*/

	public static void ForEachLocationPatched(Func<GameLocation, bool> action, bool includeInteriors = true, bool includeGenerated = false) {
		if (Instance is null) {
			Utility.ForEachLocation(action, includeInteriors, includeGenerated);
			return;
		}

		HashSet<string> wanted = Instance.OpenedLocations.Keys.ToHashSet();
		bool exited = false;

		Utility.ForEachLocation(loc => {
			wanted.Remove(loc.NameOrUniqueName);
			bool result = action(loc);
			if (!result)
				exited = true;
			return result;
		}, includeInteriors: includeInteriors, includeGenerated: includeGenerated);

		if (exited)
			return;

		foreach(string name in wanted) {
			var loc = Game1.getLocationFromName(name);
			if (loc is null)
				continue;

			if (!action(loc))
				break;
		}
	}

	/*
	public static IEnumerable<CodeInstruction> Multiplayer_updateRoots_Transpiler(IEnumerable<CodeInstruction> instructions) {

		var method = AccessTools.Method(typeof(Utility), nameof(Utility.ForEachLocation));
		var our_method = AccessTools.Method(typeof(SpookyActionAtADistance), nameof(ForEachLocationPatched));

		foreach(var instr in instructions) {
			if (instr.opcode == OpCodes.Call && instr.operand is MethodInfo minfo && minfo == method) {
				yield return new CodeInstruction(instr) {
					operand = our_method
				};

				continue;
			}

			yield return instr;
		}
	}*/

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

	public static IEnumerable<CodeInstruction> Mutex_Update_Transpiler(IEnumerable<CodeInstruction> instructions) {

		var method = AccessTools.Method(typeof(NetMutex), nameof(NetMutex.ReleaseLock));
		var our_method = AccessTools.Method(typeof(SpookyActionAtADistance), nameof(AllowMutexRelease));

		var instrs = instructions.ToArray();

		for(int i = 0; i < instrs.Length; i++) {
			CodeInstruction in0 = instrs[i];

			if ( i + 2 < instrs.Length ) {
				CodeInstruction in1 = instrs[i + 1];
				CodeInstruction in2 = instrs[i + 2];

				if (in0.opcode == OpCodes.Brtrue_S && in1.opcode == OpCodes.Ldarg_0 && in2.opcode == OpCodes.Call && in2.operand is MethodInfo minfo && minfo == method) {
					var copy = new CodeInstruction(in0);

					// Yield the first brtrue.
					yield return in0;

					// Replace the function call with a call to AllowMutexRelease.
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Call, our_method);

					// Now, skip the next two instructions.
					i++;
					i++;

					// Now resume.
					continue;
				}
			}

			yield return in0;
		}

		/*
		return new CodeMatcher(instructions)
			// Find the end if the if ( ) statement around ReleaseLock.
			.MatchStartForward(
				new CodeMatch(OpCodes.Brtrue_S),
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(OpCodes.Call, method)
			)
			// Insert a call to our ShouldLocationUpdate method
			.Insert(
				new CodeInstruction(OpCodes.Ldarg_1),
				new CodeInstruction(OpCodes.Call, our_method),
				new CodeInstruction(OpCodes.And)
			)
			// Return
			.InstructionEnumeration();*/
	}

	/*
	public static void Mutex_Release_Prefix(NetMutex __instance) {
		Instance?.Mod?.Log($"Mutex releasing: {__instance.GetHashCode()}", LogLevel.Debug);
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

	}*/

	public static void AllowMutexRelease(NetMutex mutex, FarmerCollection collection) {
		try {
			var field = Instance?.Mod?.Helper?.Reflection?.GetField<GameLocation>(collection, "_locationFilter", false);
			if (field != null) {
				var value = field.GetValue();
				if (value != null && ShouldLocationUpdate(value)) {
					Instance?.Mod?.Log($"Stopping release for location: {value.NameOrUniqueName}", LogLevel.Trace);
					return;
				}
			}

		} catch (Exception ex) {
			Instance?.Mod?.Log("Error checking AllowMutexRelease.", LogLevel.Error, ex, once: true);
		}

		mutex.ReleaseLock();
	}

	public static bool ShouldLocationUpdate(GameLocation location) {
		return Instance?.OpenedLocations?.ContainsKey(location.NameOrUniqueName) ?? false;
	}

#endif

	#endregion

	#region Events

	[Subscriber]
	private void OnPeerDisconnected(object? sender, PeerDisconnectedEventArgs e) {
		long playerId = e.Peer.PlayerID;

		if (PlayerLocations.TryGetValue(playerId, out var locs))
			RemoveWatches(playerId, locs);
	}

	[Subscriber]
	private void OnLocationListChange(object? sender, LocationListChangedEventArgs e) {
		// Remove any locations that were removed from our watching lists.
		foreach(var loc in e.Removed) {
			string name = loc.NameOrUniqueName;

			if (!OpenedLocations.TryGetValue(name, out var players))
				continue;

			OpenedLocations.Remove(name);

			foreach(long player in players) {
				if (PlayerLocations.TryGetValue(player, out var watched)) {
					watched.Remove(name);
					if (watched.Count == 0)
						PlayerLocations.Remove(player);
				}
			}
		}
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
			watched = new HashSet<string>();
			PlayerLocations[playerId] = watched;
		}

		List<string> added = new();

		foreach(string? location in locations) {
			if (string.IsNullOrEmpty(location))
				continue;

			// Validate the name.
			var loc = Game1.getLocationFromName(location);
			if (loc is null)
				continue;

			if (watched.Add(location)) {
				added.Add(location);

				if (OpenedLocations.TryGetValue(location, out var watchers))
					watchers.Add(playerId);
				else
					OpenedLocations[location] = new HashSet<long>() { playerId };
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
			if (! string.IsNullOrEmpty(location) && watched.Remove(location)) {
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

