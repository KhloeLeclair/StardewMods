using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.Common.Extensions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Objects;

namespace Leclair.Stardew.CloudySkies.Patches;

public static class Game1_Patches {

	private static ModEntry? Mod;

	public static void Patch(ModEntry mod) {
		Mod = mod;

		try {

			// Drawing

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.ShouldDrawOnBuffer)),
				postfix: new HarmonyMethod(typeof(Game1_Patches), nameof(Game1_ShouldDrawOnBuffer__Postfix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.drawWeather)),
				prefix: new HarmonyMethod(typeof(Game1_Patches), nameof(drawWeather__Prefix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.DrawWorld)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(Game1_DrawWorld__Transpiler))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.DrawLighting)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(DrawLighting__Transpiler))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.DrawLightmapOnScreen)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(DrawLightmapOnScreen__Transpiler))
			);

			// Updating

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.performTenMinuteClockUpdate)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(performTenMinuteClockUpdate__Transpiler))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateGameClock)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(UpdateGameClock__Transpiler))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.updateWeather)),
				prefix: new HarmonyMethod(typeof(Game1_Patches), nameof(updateWeather__Prefix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.updateRaindropPosition)),
				prefix: new HarmonyMethod(typeof(Game1_Patches), nameof(updateRaindropPosition__Prefix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(Game1), nameof(Game1.updateRainDropPositionForPlayerMovement)),
				prefix: new HarmonyMethod(typeof(Game1_Patches), nameof(updateRaindropPositionForPlayerMovement__Prefix))
			);

			mod.Harmony.Patch(
				original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.DayUpdate)),
				transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(IndoorPot_DayUpdate__Transpiler))
			);

			// Green Rain fixes.
			try {
				mod.Harmony.Patch(
					original: AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(Game1), "_newDayAfterFade")),
					transpiler: new HarmonyMethod(typeof(Game1_Patches), nameof(_newDayAfterFade__Transpiler))
				);
			} catch (Exception ex) {
				mod.Log($"Unable to apply the _newDayAfterFade patch to Game1. A weather type introduced in Stardew 1.6 may not work correctly in some locations.", StardewModdingAPI.LogLevel.Warn, ex);
			}

			// Weather nonsense that we can't target by name.
			var newDayRainFix = new HarmonyMethod(typeof(Game1_Patches), nameof(NewDayRain__Transpiler));

			int patched = 0;
			foreach (var type in typeof(Game1).GetNestedTypes(AccessTools.all)) {
				if (type != null && type.IsClass) {
					foreach (var method in AccessTools.GetDeclaredMethods(type)) {
						// We know we want a non-static method, with _newDayAfterFade
						// in its name. The method needs to return a bool, and
						// have one parameter: a GameLocation.

						// Sadly, this does match more than one method, but our
						// transpiler will detect the proper method by inspecting
						// the IL and just not do anything to the methods we
						// don't care about.

						if (method?.Name == null ||
							method.IsStatic ||
							method.ReturnType != typeof(bool) ||
							!method.Name.Contains("_newDayAfterFade")
						)
							continue;

						var parms = method.GetParameters();
						if (parms.Length != 1 || parms[0].ParameterType != typeof(GameLocation))
							continue;

						patched++;
						mod.Harmony.Patch(
							original: method,
							transpiler: newDayRainFix
						);
					}
				}
			}

			if (patched == 0)
				throw new Exception("unable to find _newDayAfterFade delegate to patch");

		} catch (Exception ex) {
			mod.Log($"Error patching Game1. Weather will not work correctly.", StardewModdingAPI.LogLevel.Error, ex);
		}

	}

	#region Helper Methods

	private static readonly Dictionary<string, bool> YesterdayWasGreenRain = [];

	public static void SaveGreenRainHistory() {
		YesterdayWasGreenRain.Clear();

		foreach (string key in Game1.locationContextData.Keys) {
			YesterdayWasGreenRain[key] = Game1.netWorldState.Value.GetWeatherForLocation(key).IsGreenRain;
		}

		Mod?.Log($"Saved green rain history for {YesterdayWasGreenRain.Count} location contexts.", StardewModdingAPI.LogLevel.Trace);
	}

	public static void UpdateGreenRainLocations() {
		HashSet<string> toAdd = [];
		HashSet<string> toRemove = [];

		foreach (string key in Game1.locationContextData.Keys) {
			bool isGreenRain = Game1.netWorldState.Value.GetWeatherForLocation(key).IsGreenRain;
			bool wasGreenRain = YesterdayWasGreenRain.GetValueOrDefault(key);

			if (isGreenRain && !wasGreenRain) {
				toAdd.Add(key);
			} else if (wasGreenRain && !isGreenRain) {
				toRemove.Add(key);
			}
		}

		if (toAdd.Count > 0 || toRemove.Count > 0) {
			int added = 0;
			int removed = 0;

			Utility.ForEachLocation(delegate (GameLocation loc) {
				string id = loc.GetLocationContextId();
				if (toAdd.Contains(id)) {
					loc.performGreenRainUpdate();
					added++;
				} else if (toRemove.Contains(id)) {
					loc.performDayAfterGreenRainUpdate();
					removed++;
				}

				return true;
			});

			if (added != 0 || removed != 0)
				Mod?.Log($"Processed green rain additions for {added} locations across {toAdd.Count} contexts and removed them from {removed} locations across {toRemove.Count} contexts.", StardewModdingAPI.LogLevel.Debug);
		}
	}

	#endregion

	#region Drawing

	private static void Game1_ShouldDrawOnBuffer__Postfix(Game1 __instance, ref bool __result) {
		if (Mod?.HasShaderLayer?.Value ?? false)
			__result = true;
	}

	private static IEnumerable<CodeInstruction> DrawLighting__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_isRainingHere = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsRainingHere));
		var hasAmbientColor = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.HasAmbientColor));

		if (GameLocation_isRainingHere is null)
			throw new Exception("could not find necessary method");

		foreach (var in0 in instructions) {
			if (in0.Calls(GameLocation_isRainingHere))
				// Old Code: Game1.currentLocation.IsRainingHere()
				// New Code: PatchHelper.HasAmbientColor(Game1.currentLocation)
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Call,
					operand = hasAmbientColor
				};

			else
				yield return in0;
		}

	}

	private static IEnumerable<CodeInstruction> DrawLightmapOnScreen__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_isRainingHere = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsRainingHere));
		var hasLightingTint = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.HasLightingTint));

		var getLightingTint = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.GetLightingTint));

		var Color_OrangeRed = AccessTools.PropertyGetter(typeof(Color), nameof(Color.OrangeRed));

		if (GameLocation_isRainingHere is null)
			throw new Exception("could not find necessary method");

		CodeInstruction[] instrs = instructions.ToArray();

		for (int i = 0; i < instrs.Length; i++) {
			var in0 = instrs[i];

			if (i + 2 < instrs.Length) {
				var in1 = instrs[i + 1];
				var in2 = instrs[i + 2];

				if (in0.Calls(Color_OrangeRed) && in1.AsDouble() == 0.45) {
					// Old Code: Color.OrangeRed * 0.45f
					// New Code: Game1_Patches.GetWeatherLightingTint()
					yield return new CodeInstruction(in0) {
						opcode = OpCodes.Ldnull,
						operand = null
					};
					yield return new CodeInstruction(OpCodes.Call, getLightingTint);

					// Skip the color multiplication.
					i += 2;
				}

			}

			if (in0.Calls(GameLocation_isRainingHere))
				// Old Code: Game1.currentLocation.IsRainingHere()
				// New Code: PatchHelper.HasLightingTint(Game1.currentLocation)
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Call,
					operand = hasLightingTint
				};

			else
				yield return in0;
		}
	}

	private static IEnumerable<CodeInstruction> Game1_DrawWorld__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_isRainingHere = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsRainingHere));
		var hasPostLightingTint = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.HasPostLightingTint));

		var getPostLightingTint = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.GetPostLightingTint));

		var Color_Blue = AccessTools.PropertyGetter(typeof(Color), nameof(Color.Blue));

		CodeInstruction[] instrs = instructions.ToArray();

		if (GameLocation_isRainingHere is null)
			throw new Exception("could not find necessary method");

		bool seen_raining = false;

		for (int i = 0; i < instrs.Length; i++) {
			var in0 = instrs[i];

			if (in0.Calls(GameLocation_isRainingHere)) {
				seen_raining = true;
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Call,
					operand = hasPostLightingTint
				};

				continue;
			}

			if (seen_raining && i + 5 < instrs.Length) {
				var in1 = instrs[i + 1];
				var in2 = instrs[i + 2];
				var in3 = instrs[i + 3];
				var in4 = instrs[i + 4];
				var in5 = instrs[i + 5];

				if (in0.Calls(Color_Blue) && in1.opcode == OpCodes.Ldc_R4) {
					// Old Code: Color.Blue * 0.2f
					// New Code: PatchHelper.GetPostLightingTint(null)
					yield return new CodeInstruction(in0) {
						opcode = OpCodes.Ldnull,
						operand = null
					};

					yield return new CodeInstruction(OpCodes.Call, getPostLightingTint);

					// Skip the color multiplication and stuff.
					i += 2;
					continue;
				}

				if (in0.LoadsConstant(0) &&
					in1.LoadsConstant(120) &&
					in2.LoadsConstant(150) &&
					in3.opcode == OpCodes.Newobj && in3.operand is ConstructorInfo cinfo && cinfo.DeclaringType == typeof(Color) &&
					in4.opcode == OpCodes.Ldc_R4
				) {
					// Old Code: new Color(0, 120, 150) * 0.22f
					// New Code: PatchHelper.GetPostLightingTint(null)
					yield return new CodeInstruction(in0) {
						opcode = OpCodes.Ldnull,
						operand = null
					};

					yield return new CodeInstruction(OpCodes.Call, getPostLightingTint);

					// Skip the color multiplication and stuff.
					i += 5;
					continue;
				}

			}

			yield return in0;
		}

	}

	private static bool drawWeather__Prefix(Game1 __instance, GameTime time, RenderTarget2D target_screen) {
		try {
			if (Mod is not null && Mod.DrawWeather(__instance, time, target_screen))
				return false;

		} catch (Exception ex) {
			Mod?.Log($"Error drawing weather: {ex}", StardewModdingAPI.LogLevel.Error, once: true);
		}

		return true;
	}

	#endregion

	#region Updates

	private static IEnumerable<CodeInstruction> IndoorPot_DayUpdate__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_IsRainingHere = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsRainingHere));
		var ShouldWaterCropsAndBowls = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.ShouldWaterCropsAndBowls));

		foreach (var in0 in instructions) {
			if (in0.Calls(GameLocation_IsRainingHere))
				// Old Code: if ((bool)location.isOutdoors && location.IsRainingHere())
				// New Code: if ((bool)location.isOutdoors && PatchHelper.ShouldWaterCropsAndBowls(location))
				yield return new CodeInstruction(in0) {
					opcode = OpCodes.Call,
					operand = ShouldWaterCropsAndBowls
				};

			else
				yield return in0;
		}
	}

	private static IEnumerable<CodeInstruction> _newDayAfterFade__Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method) {

		FieldInfo? yesterdayWasGreenRain = null;

		if (method.DeclaringType != null)
			foreach (var field in AccessTools.GetDeclaredFields(method.DeclaringType)) {
				if (field.Name.StartsWith("<yesterdayWasGreenRain>")) {
					yesterdayWasGreenRain = field;
					break;
				}
			}

		if (yesterdayWasGreenRain == null)
			throw new Exception("could not find yesterdayWasGreenRain field");

		var Game1_UpdateWeatherForNewDay = AccessTools.Method(typeof(Game1), nameof(Game1.UpdateWeatherForNewDay));
		var Game1_IsMasterGame = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.IsMasterGame));
		var Game1_isGreenRain = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.isGreenRain));

		var mSaveGreenRainHistory = AccessTools.Method(typeof(Game1_Patches), nameof(SaveGreenRainHistory));
		var mUpdateGreenRainLocations = AccessTools.Method(typeof(Game1_Patches), nameof(UpdateGreenRainLocations));

		Label? afterYesterday = null;

		var matcher = new CodeMatcher(instructions)
			// Old Code: Game1.UpdateWeatherForNewDay();
			// New Code: SaveGreenRainHistory();Game1.UpdateWeatherForNewDay();
			.MatchStartForward(new CodeMatch(in0 => in0.Calls(Game1_UpdateWeatherForNewDay)))
			.ThrowIfNotMatch("could not find Game1.UpdateWeatherForNewDay()")
			.Insert(new CodeInstruction(OpCodes.Call, mSaveGreenRainHistory))

			// Now, let's make sure we're in the "if (Game1.isGreenRain) {" block
			.MatchStartForward(
				new CodeMatch(in0 => in0.Calls(Game1_isGreenRain)),
				new CodeMatch(in0 => in0.IsBranch())
			)
			.ThrowIfNotMatch("could not find if Game1.isGreenRain")

			// Find the next check for IsMasterGame
			.MatchStartForward(
				new CodeMatch(in0 => in0.Calls(Game1_IsMasterGame)),
				new CodeMatch(in0 => in0.IsBranch())
			)
			.ThrowIfNotMatch("could not find Game1.IsMasterGame")
			// Old Code: "if (Game1.IsMasterGame) {"
			// New Code: "if (Game1.IsMasterGame && false) {"
			.Advance(1)
			.Insert(
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.And)
			)

			// Now, let's go forward until we find "else if (yesterdayWasGreenRain) {"
			.MatchStartForward(
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(in0 => in0.LoadsField(yesterdayWasGreenRain)),
				// We need to store the address it jumps to after this so we can insert
				// some logic to run after
				new CodeMatch(in0 => in0.Branches(out afterYesterday))
			)
			.ThrowIfNotMatch("could not find else if yesterdayWasGreenRain")

			// Find the next check for IsMasterGame
			.MatchStartForward(
				new CodeMatch(in0 => in0.Calls(Game1_IsMasterGame)),
				new CodeMatch(in0 => in0.IsBranch())
			)
			.ThrowIfNotMatch("could not find Game1.IsMasterGame")
			// Old Code: "if (Game1.IsMasterGame) {"
			// New Code: "if (Game1.IsMasterGame && false) {"
			.Advance(1)
			.Insert(
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.And)
			)

			// Now, find our branch.
			.MatchStartForward(
				new CodeMatch(in0 => afterYesterday.HasValue && in0.labels.Contains(afterYesterday.Value))
			)
			.ThrowIfNotMatch("could not find jump point after else if yesterdayWasGreenRain");

		var instr = matcher.Instruction;

		matcher = matcher
			.Insert(
				new CodeInstruction(OpCodes.Call, mUpdateGreenRainLocations).MoveLabelsFrom(instr)
			);

		return matcher.InstructionEnumeration();
	}

	private static IEnumerable<CodeInstruction> NewDayRain__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_IsOutdoors = AccessTools.PropertyGetter(typeof(GameLocation), nameof(GameLocation.IsOutdoors));
		var GameLocation_IsRainingHere = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsRainingHere));

		var ShouldWaterCropsAndBowls = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.ShouldWaterCropsAndBowls));

		var matcher = new CodeMatcher(instructions)
			.MatchEndForward(
				new CodeMatch(OpCodes.Ldarg_1),
				new CodeMatch(in0 => in0.Calls(GameLocation_IsOutdoors)),
				new CodeMatch(OpCodes.Brfalse),
				new CodeMatch(OpCodes.Ldarg_1),
				new CodeMatch(in0 => in0.Calls(GameLocation_IsRainingHere))
			);

		if (matcher.IsValid)
			// We found the expected sequence. Let's replace it!
			// Old Code: if (location.IsOutdoors && location.IsRainingHere())
			// New Code: if (location.IsOutdoors && PatchHelper.ShouldWaterCropsAndBowls(location))
			matcher.SetInstruction(new CodeInstruction(matcher.Instruction) {
				opcode = OpCodes.Call,
				operand = ShouldWaterCropsAndBowls
			});

		return matcher.InstructionEnumeration();
	}

	private static IEnumerable<CodeInstruction> performTenMinuteClockUpdate__Transpiler(IEnumerable<CodeInstruction> instructions) {

		var GameLocation_IsRainingHere = AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsRainingHere));
		var hasAmbientColor = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.HasAmbientColor));
		var hasMusic = AccessTools.Method(typeof(PatchHelper), nameof(PatchHelper.HasMusic));

		var AmbientLight = AccessTools.Field(typeof(Game1), nameof(Game1.ambientLight));

		bool seen_ambient_light = false;

		foreach (var in0 in instructions) {

			if (in0.LoadsField(AmbientLight))
				seen_ambient_light = true;

			if (in0.Calls(GameLocation_IsRainingHere)) {
				if (seen_ambient_light)
					// If we have already seen the Game1.ambientLight access,
					// then we're moving on to checking the music rather
					// than the ambient light, so switch which method we call.
					yield return new CodeInstruction(in0) {
						opcode = OpCodes.Call,
						operand = hasMusic
					};

				else
					yield return new CodeInstruction(in0) {
						opcode = OpCodes.Call,
						operand = hasAmbientColor
					};

			} else
				yield return in0;
		}

	}

	public static bool AssignOutdoorLight() {
		var rawTint = PatchHelper.GetTintData(Game1.currentLocation);
		if (!rawTint.HasValue || !rawTint.Value.HasAmbientColor)
			return false;

		var tint = rawTint.Value;
		float opacity;

		if (tint.AmbientEndTime == int.MaxValue || tint.EndAmbientOutdoorOpacity == tint.StartAmbientOutdoorOpacity)
			// Same value? No end to today? Just return the starting opacity.
			opacity = tint.StartAmbientOutdoorOpacity;
		else if (Game1.timeOfDay >= tint.AmbientEndTime)
			opacity = tint.EndAmbientOutdoorOpacity;
		else {
			// Both values? Lerp between them.
			int minutes = Utility.CalculateMinutesBetweenTimes(tint.AmbientStartTime, Game1.timeOfDay) / 10;
			float progress = (minutes + (Game1.gameTimeInterval / (float) Game1.realMilliSecondsPerGameTenMinutes)) / tint.AmbientDurationInTenMinutes;

			opacity = Utility.Lerp(tint.StartAmbientOutdoorOpacity, tint.EndAmbientOutdoorOpacity, progress);
		}

		Game1.outdoorLight = Game1.ambientLight * opacity;
		return true;
	}


	private static IEnumerable<CodeInstruction> UpdateGameClock__Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator) {

		var assignOutdoorLight = AccessTools.Method(typeof(Game1_Patches), nameof(AssignOutdoorLight));

		var Game1_currentLocation = AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.currentLocation));

		var Game1_timeOfDay = AccessTools.Field(typeof(Game1), nameof(Game1.timeOfDay));
		var Game1_ambientLight = AccessTools.Field(typeof(Game1), nameof(Game1.ambientLight));
		var Game1_outdoorLight = AccessTools.Field(typeof(Game1), nameof(Game1.outdoorLight));

		var matcher = new CodeMatcher(instructions, generator)
			// We want to find Game1.outdoorLight = Game1.ambientLight; so we can jump to the bit after it.
			.MatchEndForward(
				new CodeMatch(in0 => in0.LoadsField(Game1_ambientLight)),
				new CodeMatch(in0 => in0.StoresField(Game1_outdoorLight))
			)
			.ThrowIfInvalid("could not find last branch of lighting code")
			.Advance(1)
			.CreateLabel(out var label) // create a label on the next instruction so we can jump to it???

			// Next, we want to find the start of the lighting block so we can add a branch that skips it
			// all, depending on our own function call's return value.
			.Start()
			.MatchStartForward(
				new CodeMatch(in0 => in0.LoadsField(Game1_timeOfDay)),
				new CodeMatch(in0 => in0.Calls(Game1_currentLocation)),
				new CodeMatch(OpCodes.Call),
				new CodeMatch(OpCodes.Blt)
			)
			.ThrowIfInvalid("could not find initial time of day check");

		var instr = matcher.Instruction;

		// Insert our call + branch instruction
		return matcher.Insert(
				new CodeInstruction(OpCodes.Call, assignOutdoorLight).MoveLabelsFrom(instr).MoveBlocksFrom(instr),
				new CodeInstruction(OpCodes.Brtrue, label)
			)
			.InstructionEnumeration();
	}


	private static bool updateRaindropPosition__Prefix() {
		try {
			if (Mod is not null && Mod.MoveWithViewport())
				return false;

		} catch (Exception ex) {
			Mod?.Log($"Error moving weather: {ex}", StardewModdingAPI.LogLevel.Error, once: true);
		}

		return true;
	}


	private static bool updateRaindropPositionForPlayerMovement__Prefix(int direction, float speed) {
		try {
			if (Mod is not null && Mod.MoveWithPlayer(direction, speed))
				return false;

		} catch (Exception ex) {
			Mod?.Log($"Error moving weather: {ex}", StardewModdingAPI.LogLevel.Error, once: true);
		}

		return true;
	}


	private static bool updateWeather__Prefix(GameTime time) {
		try {
			if (Mod is not null && Mod.UpdateWeather(time))
				return false;

		} catch (Exception ex) {
			Mod?.Log($"Error updating weather: {ex}", StardewModdingAPI.LogLevel.Error, once: true);
		}

		return true;
	}

	#endregion

}
