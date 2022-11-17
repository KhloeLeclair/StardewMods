using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;

using Leclair.Stardew.ThemeManager.Models;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Leclair.Stardew.Common;
using Leclair.Stardew.Common.Types;

using StardewModdingAPI;
using StardewValley;

namespace Leclair.Stardew.ThemeManager.Patches;

internal class DynamicPatcher : IDisposable {

	internal static bool DidPatch = false;
	internal static Dictionary<MethodBase, DynamicPatcher> LivePatchers = new();

	private static Dictionary<string, Color>? Colors;

	private static Color GetColorPacked(string key, uint @default) {
		return Colors != null && Colors.TryGetValue(key, out Color val) ? val: new Color(@default);
	}

	private static Color GetColor(string key, Color @default) {
		return Colors != null && Colors.TryGetValue(key, out Color val) ? val : @default;
	}

	private static Color GetLerpColorPacked(float power, uint left, uint middle, uint right) {
		if (power <= 0.5f)
			return Color.Lerp(new Color(left), new Color(middle), power * 2f);
		else
			return Color.Lerp(new Color(middle), new Color(right), (power - 0.5f) * 2f);
	}

	private static Color GetLerpColor(float power, string keyLeft, string keyMiddle, string keyRight) {
		if (Colors == null)
			return Utility.getRedToGreenLerpColor(power);

		if (!Colors.TryGetValue(keyLeft, out Color left))
			left = Color.Red;
		if (!Colors.TryGetValue(keyMiddle, out Color middle))
			middle = Color.Yellow;
		if (!Colors.TryGetValue(keyRight, out Color right))
			right = new Color(0, 255, 0);

		if (power <= 0.5f)
			return Color.Lerp(left, middle, power * 2f);
		else
			return Color.Lerp(middle, right, (power - 0.5f) * 2f);
	}

	internal static Lazy<CaseInsensitiveDictionary<(MethodInfo, Color)>> ColorProperties = new(() => {
		var dict = new CaseInsensitiveDictionary<(MethodInfo, Color)>();

		void AddColor(string name, MethodInfo method, Color color) {
			if (!dict.ContainsKey(name))
				dict[name] = (method, color);

			if (name.Contains("Gray")) {
				name = name.Replace("Gray", "Grey");
				if (!dict.ContainsKey(name))
					dict[name] = (method, color);
			}
		}

		foreach(PropertyInfo prop in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public)) {
			string name = prop.Name;
			if (prop.PropertyType != typeof(Color))
				continue;

			if (prop.GetGetMethod() is not MethodInfo method)
				continue;

			Color? color;
			try {
				color = prop.GetValue(null) as Color?;
			} catch {
				continue;
			}

			if (color.HasValue)
				AddColor(name, method, color.Value);
		}

		return dict;
	});

	private readonly ModEntry Mod;

	public readonly MethodInfo Method;
	public readonly string Key;

	private readonly MethodInfo HMethod;

	private PatchData? AppliedChanges;
	private CaseInsensitiveHashSet? UsedColors;

	private readonly List<PatchData> Patches = new();
	//private Dictionary<string, Color>? Colors;

	private bool IsDisposed;
	private bool IsPatched;
	private bool IsDirty;

	public DynamicPatcher(ModEntry mod, MethodInfo method, string key) {
		Mod = mod;
		Method = method;
		Key = key;

		HMethod = AccessTools.Method(typeof(DynamicPatcher), nameof(Transpiler));
	}

	public void ClearPatches() {
		Patches.Clear();
		IsDirty = true;
	}

	public void AddPatch(PatchData data) {
		Patches.Add(data);
		IsDirty = true;
	}

	public void RemovePatch(PatchData data) {
		Patches.Remove(data);
		IsDirty = true;
	}

	public bool Update(Dictionary<string, Color> colors) {
		// Do we need to change our patches?
		if (IsDirty) {
			// Build a new aggregate patch data.
			var applied = new PatchData() {
				Colors = new(),
				RawColors = new(),
				Fields = new()
			};

			foreach (var patch in Patches) {
				if (patch.Colors is not null) {
					foreach (var entry in patch.Colors)
						applied.Colors[entry.Key] = entry.Value;
				}

				if (patch.RawColors is not null) {
					foreach (var entry in patch.RawColors)
						applied.RawColors[entry.Key] = entry.Value;
				}

				if (patch.Fields is not null) {
					foreach (var entry in patch.Fields)
						applied.Fields[entry.Key] = entry.Value;
				}

				if (patch.RedToGreenLerp is not null)
					applied.RedToGreenLerp = patch.RedToGreenLerp;
			}

			// Now, compare it to our existing applied changes.
			if (applied.Colors.Count == 0 && applied.RawColors.Count == 0 && applied.Fields.Count == 0 && applied.RedToGreenLerp == null)
				applied = null;

			if (applied is null) {
				IsDirty = AppliedChanges is not null;
			} else if (AppliedChanges is not null) {
				IsDirty = false;
				if (!applied.Colors!.ShallowEquals(AppliedChanges.Colors!))
					IsDirty = true;
				if (!applied.RawColors!.ShallowEquals(AppliedChanges.RawColors!))
					IsDirty = true;
				if (!applied.Fields!.ShallowEquals(AppliedChanges.Fields!))
					IsDirty = true;
				if (!applied.RedToGreenLerp.ShallowEquals(AppliedChanges.RedToGreenLerp))
					IsDirty = true;
			}

			// If there are changed, recalculate the applied colors and
			// save these new objects.
			if (IsDirty) {
				CaseInsensitiveHashSet used = new();
				if (applied is not null) {
					foreach (string value in applied.Colors!.Values)
						if (value.StartsWith('$'))
							used.Add(value[1..]);

					foreach (string value in applied.RawColors!.Values)
						if (value.StartsWith('$'))
							used.Add(value[1..]);

					foreach (string value in applied.Fields!.Values)
						if (value.StartsWith('$'))
							used.Add(value[1..]);

					if (applied.RedToGreenLerp != null)
						foreach (string value in applied.RedToGreenLerp)
							if (value.StartsWith('$'))
								used.Add(value[1..]);
				}

				AppliedChanges = applied;
				UsedColors = used;
			}
		}

		// If we aren't considered dirty, check the incoming colors and see
		// if any colors we're using have changed.
		/*if (!IsDirty && UsedColors is not null) {
			if (Colors is null)
				IsDirty = true;
			else
				foreach (string color in UsedColors) {
					Colors.TryGetValue(color, out var existing);
					colors.TryGetValue(color, out var incoming);
					if (existing != incoming) {
						IsDirty = true;
						break;
					}
				}
		}*/

		// Always update Colors just to avoid having stuff stick in memory.
		Colors = colors;

		// Re-patch if we're dirty.
		if (IsDirty) {
			Repatch();
			/*if (IsPatched)
				Unpatch();
			Patch();*/
		}

		return AppliedChanges != null;
	}

	public void Repatch() {
		if (IsDisposed || Mod.Harmony is null)
			return;

		if (! IsPatched || LivePatchers.TryGetValue(Method, out var patcher) && patcher != this) {
			Patch();
			return;
		}

		DidPatch = false;
		Mod.Harmony.Patch(Method);

		if (!DidPatch) {
			Unpatch();
			Patch();
			return;
		}

		IsPatched = true;
		IsDirty = false;
	}

	public void Patch() {
		if (IsPatched || IsDisposed || AppliedChanges is null || Mod.Harmony is null || HMethod is null)
			return;

		if (LivePatchers.TryGetValue(Method, out var patcher) && patcher != this) {
			patcher.Unpatch();
			LivePatchers.Remove(Method);
		}

		LivePatchers.Add(Method, this);

		Mod.Harmony.Patch(Method, transpiler: new HarmonyMethod(HMethod, priority: Priority.Last));
		IsPatched = true;
		IsDirty = false;
	}

	public void Unpatch() {
		if (!IsPatched || Mod.Harmony is null)
			return;

		Mod.Harmony.Unpatch(Method, HMethod);

		if (LivePatchers.TryGetValue(Method, out var patcher) && patcher == this) {
			LivePatchers.Remove(Method);
		}

		IsPatched = false;
	}

	protected virtual void Dispose(bool disposing) {
		if (!IsDisposed) {
			if (disposing) {
				Unpatch();

				AppliedChanges = null;
				UsedColors = null;
				Colors = null;
				Patches.Clear();
			}

			IsDisposed = true;
		}
	}

	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#region The Actual Patch

	internal static IEnumerable<CodeInstruction> Transpiler(MethodBase method, IEnumerable<CodeInstruction> instructions) {
		DidPatch = true;

		if (!LivePatchers.TryGetValue(method, out DynamicPatcher? patcher))
			return instructions;

		if (patcher.AppliedChanges is null)
			return instructions;

		int count = 0;

		Dictionary<MethodInfo, (string, Color)> Colors = new();
		Dictionary<Color, string> RawColors = new();
		Dictionary<FieldInfo, string> Fields = new();

		Dictionary<MethodInfo, Color> DirectColors = new();
		Dictionary<Color, Color> DirectRawColors = new();
		Dictionary<FieldInfo, Color> DirectFields = new();

		(string, string, string)? Lerp = null;
		(Color, Color, Color)? DirectLerp = null;

		if (patcher.AppliedChanges.Colors is not null)
			foreach(var entry in patcher.AppliedChanges.Colors) {
				if (!ColorProperties.Value.TryGetValue(entry.Key, out var getter)) {
					patcher.Mod.Log($"Unable to find color named \"{entry.Key}\" processing {patcher.Key}", LogLevel.Warn);
					continue;
				}

				if (entry.Value.StartsWith('$')) {
					Colors[getter.Item1] = (entry.Value[1..], getter.Item2);
				} else if (CommonHelper.TryParseColor(entry.Value, out var c))
					DirectColors[getter.Item1] = c.Value;
				else {
					patcher.Mod.Log($"Unable to parse color \"{entry.Value}\" processing {patcher.Key}", LogLevel.Warn);
					continue;
				}

				count++;
			}

		if (patcher.AppliedChanges.RawColors is not null)
			foreach(var entry in patcher.AppliedChanges.RawColors) {
				if (!CommonHelper.TryParseColor(entry.Key, out var keycolor)) {
					patcher.Mod.Log($"Unable to parse raw color \"{entry.Key}\" processing {patcher.Key}", LogLevel.Warn);
					continue;
				}

				if (entry.Value.StartsWith('$')) {
					RawColors[keycolor.Value] = entry.Value[1..];
				} else if (CommonHelper.TryParseColor(entry.Value, out var c))
					DirectRawColors[keycolor.Value] = c.Value;
				else {
					patcher.Mod.Log($"Unable to parse color \"{entry.Value}\" processing {patcher.Key}", LogLevel.Warn);
					continue;
				}

				count++;
			}

		if (patcher.AppliedChanges.Fields is not null)
			foreach(var entry in patcher.AppliedChanges.Fields) {
				FieldInfo field;
				if (entry.Key == "textColor" || entry.Key == "textShadowColor" || entry.Key == "unselectedOptionColor")
					field = AccessTools.Field(typeof(Game1), entry.Key);
				else {
					patcher.Mod.Log($"Skipping unknown field name \"{entry.Key}\" processing {patcher.Key}", LogLevel.Warn);
					continue;
				}

				if (entry.Value.StartsWith('$')) {
					Fields[field] = entry.Value[1..];
				} else if (CommonHelper.TryParseColor(entry.Value, out var c))
					DirectFields[field] = c.Value;
				else {
					patcher.Mod.Log($"Unable to parse color \"{entry.Value}\" processing {patcher.Key}", LogLevel.Warn);
				}

				count++;
			}

		if (patcher.AppliedChanges.RedToGreenLerp is not null) {
			string[] pair = patcher.AppliedChanges.RedToGreenLerp;
			if (pair.Length == 3 && !string.IsNullOrEmpty(pair[0]) && !string.IsNullOrEmpty(pair[1]) && !string.IsNullOrEmpty(pair[2])) {
				bool is_variable = pair[0].StartsWith('$');
				if (is_variable != pair[1].StartsWith('$') || is_variable != pair[2].StartsWith('$'))
					patcher.Mod.Log($"Unable to combine variable and non-variable colors for lerp processing {patcher.Key}", LogLevel.Warn);
				else if (is_variable) {
					Lerp = (pair[0][1..], pair[1][1..], pair[2][1..]);
					count++;

				} else if (CommonHelper.TryParseColor(pair[0], out var left) && CommonHelper.TryParseColor(pair[1], out var middle) && CommonHelper.TryParseColor(pair[2], out var right)) {
					DirectLerp = (left.Value, middle.Value, right.Value);
					count++;

				} else
					patcher.Mod.Log($"Unable to parse color \"{pair}\" processing {patcher.Key}", LogLevel.Warn);
			}
		}

		if (count == 0)
			return instructions;

		if (patcher.Mod.Config.DebugPatches)
			patcher.Mod.Log($"Patching {patcher.Key} with {count} changes.");

		bool has_raw = RawColors.Count > 0 || DirectRawColors.Count > 0;

		MethodInfo getColorPacked = AccessTools.Method(typeof(DynamicPatcher), nameof(GetColorPacked));
		MethodInfo getColor = AccessTools.Method(typeof(DynamicPatcher), nameof(GetColor));
		MethodInfo getLerpPacked = AccessTools.Method(typeof(DynamicPatcher), nameof(GetLerpColorPacked));
		MethodInfo getLerp = AccessTools.Method(typeof(DynamicPatcher), nameof(GetLerpColor));

		MethodInfo RedGreenLerpInfo = AccessTools.Method(typeof(Utility), nameof(Utility.getRedToGreenLerpColor));

		ConstructorInfo cstruct = AccessTools.Constructor(typeof(Color), new Type[] {
			typeof(uint)
		});

		var instrs = instructions.ToArray();
		Color color;

		List<CodeInstruction> result = new();
		int replaced = 0;

		void AddAndLog(string message, CodeInstruction[] newInstructions, CodeInstruction[] oldInstructions) {
			if (patcher.Mod.Config.DebugPatches) {
				patcher!.Mod.Log(message);
				foreach (var entry in oldInstructions)
					patcher.Mod.Log($"-- {entry}");
				foreach (var entry in newInstructions)
					patcher.Mod.Log($"++ {entry}");
			}

			result.AddRange(newInstructions);
			replaced++;
		}

		for (int i = 0; i < instrs.Length; i++) {
			CodeInstruction in0 = instrs[i];

			// Raw Colors (new Color(r, g, b))
			if (i + 3 < instrs.Length && has_raw) {
				CodeInstruction in1 = instrs[i + 1];
				CodeInstruction in2 = instrs[i + 2];
				CodeInstruction in3 = instrs[i + 3];

				if (in3.IsConstructor<Color>()) {
					int? val0 = in0.AsInt();
					int? val1 = in1.AsInt();
					int? val2 = in2.AsInt();

					if (val0.HasValue && val1.HasValue && val2.HasValue) {
						Color c = new(val0.Value, val1.Value, val2.Value);
						if (RawColors.TryGetValue(c, out string? key)) {
							AddAndLog(
								$"Replacing raw color {c} with: {key}",
								new CodeInstruction[] {
									new CodeInstruction(in0) {
										opcode = OpCodes.Ldstr,
										operand = key
									},
									new CodeInstruction(
										opcode: OpCodes.Ldc_I4,
										operand: unchecked((int) c.PackedValue)
									),
									new CodeInstruction(
										opcode: OpCodes.Call,
										operand: getColorPacked
									)
								},

								oldInstructions: new CodeInstruction[] {
									in0, in1, in2, in3
								}
							);

							i += 3;
							continue;

						} else if (DirectRawColors.TryGetValue(c, out color)) {
							AddAndLog(
								$"Replacing raw color {c} with static: {color}",
								new CodeInstruction[] {
									new CodeInstruction(in0) {
										opcode = OpCodes.Ldc_I4,
										operand = unchecked((int) color.PackedValue)
									},
									new CodeInstruction(
										opcode: OpCodes.Newobj,
										operand: cstruct
									)
								},

								oldInstructions: new CodeInstruction[] {
									in0, in1, in2, in3
								}
							);

							i += 3;
							continue;
						}
					}
				}
			}

			// Color Properties (Color.Red)
			if (in0.opcode == OpCodes.Call && in0.operand is MethodInfo meth) {
				if (Colors.TryGetValue(meth, out (string, Color) key)) {
					AddAndLog(
						$"Replacing color property {meth.Name} with: {key.Item1}",
						new CodeInstruction[] {
							new CodeInstruction(in0) {
								opcode = OpCodes.Ldstr,
								operand = key.Item1
							},
							new CodeInstruction(
								opcode: OpCodes.Ldc_I4,
								operand: unchecked((int) key.Item2.PackedValue)
							),
							new CodeInstruction(
								opcode: OpCodes.Call,
								operand: getColorPacked
							)
						},

						oldInstructions: new CodeInstruction[] {
							in0
						}
					);

					continue;
				}

				if (DirectColors.TryGetValue(meth, out color)) {
					AddAndLog(
						$"Replacing color property {meth.Name} with static: {color}",
						new CodeInstruction[] {
							new CodeInstruction(in0) {
								opcode = OpCodes.Ldc_I4,
								operand = unchecked((int) color.PackedValue)
							},
							new CodeInstruction(
								opcode: OpCodes.Newobj,
								operand: cstruct
							)
						},

						oldInstructions: new CodeInstruction[] {
							in0
						}
					);

					continue;
				}
			}

			// Static Field Access (Game1.textColor)
			if (in0.opcode == OpCodes.Ldsfld && in0.operand is FieldInfo field) {
				if (Fields.TryGetValue(field, out string? key)) {
					AddAndLog(
						$"Replacing static field {field.Name} with: {key}",
						new CodeInstruction[] {
							// Yes, even though we're also emitting in0
							// basically, we need to replace it so that
							// labels and stuff don't get screwed up.
							new CodeInstruction(in0) {
								opcode = OpCodes.Ldstr,
								operand = key
							},
							new CodeInstruction(
								opcode: in0.opcode,
								operand: in0.operand
							),
							new CodeInstruction(
								opcode: OpCodes.Call,
								operand: getColor
							)
						},

						oldInstructions: new CodeInstruction[] {
							in0
						}
					);

					continue;
				}

				if (DirectFields.TryGetValue(field, out color)) {
					AddAndLog(
						$"Replacing static field {field.Name} with static: {color}",
						new CodeInstruction[] {
							new CodeInstruction(in0) {
								opcode = OpCodes.Ldc_I4,
								operand = unchecked((int) color.PackedValue)
							},
							new CodeInstruction(
								opcode: OpCodes.Newobj,
								operand: cstruct
							)
						},

						oldInstructions: new CodeInstruction[] {
							in0
						}
					);

					continue;
				}
			}

			// Red To Green Lerp
			if (in0.opcode == OpCodes.Call && in0.operand is MethodInfo minfo && minfo == RedGreenLerpInfo) {
				if (Lerp is not null) {
					AddAndLog(
						$"Replacing {minfo.Name} call with: {Lerp.Value}",
						new CodeInstruction[] {
							new CodeInstruction(in0) {
								opcode = OpCodes.Ldstr,
								operand = Lerp.Value.Item1
							},
							new CodeInstruction(
								opcode: OpCodes.Ldstr,
								operand: Lerp.Value.Item2
							),
							new CodeInstruction(
								opcode: OpCodes.Ldstr,
								operand: Lerp.Value.Item3
							),
							new CodeInstruction(
								opcode: OpCodes.Call,
								operand: getLerp
							)
						},

						oldInstructions: new CodeInstruction[] {
							in0
						}
					);

					continue;
				}

				if (DirectLerp is not null) {
					AddAndLog(
						$"Replacing {minfo.Name} call with static: {DirectLerp.Value}",
						new CodeInstruction[] {
							new CodeInstruction(in0) {
								opcode = OpCodes.Ldc_I4,
								operand = unchecked((int) DirectLerp.Value.Item1.PackedValue)
							},
							new CodeInstruction(
								opcode: OpCodes.Ldc_I4,
								operand: unchecked((int) DirectLerp.Value.Item2.PackedValue)
							),
							new CodeInstruction(
								opcode: OpCodes.Ldc_I4,
								operand: unchecked((int) DirectLerp.Value.Item3.PackedValue)
							),
							new CodeInstruction(
								opcode: OpCodes.Call,
								operand: getLerpPacked
							)
						},

						oldInstructions: new CodeInstruction[] {
							in0
						}
					);

					continue;
				}
			}

			// Still here? Just push that instruction then.
			result.Add(in0);
		}

		if (patcher.Mod.Config.DebugPatches)
			patcher.Mod.Log($"- Performed {replaced} replacements.");

		return result;
	}

	#endregion
}
