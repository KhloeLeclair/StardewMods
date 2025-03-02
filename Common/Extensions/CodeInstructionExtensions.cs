#if HARMONY

using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

namespace Leclair.Stardew.Common.Extensions;

internal static class CodeInstructionExtensions {

	private static readonly HashSet<OpCode> BranchCodes = new HashSet<OpCode>
	{
		OpCodes.Br_S,
		OpCodes.Brfalse_S,
		OpCodes.Brtrue_S,
		OpCodes.Beq_S,
		OpCodes.Bge_S,
		OpCodes.Bgt_S,
		OpCodes.Ble_S,
		OpCodes.Blt_S,
		OpCodes.Bne_Un_S,
		OpCodes.Bge_Un_S,
		OpCodes.Bgt_Un_S,
		OpCodes.Ble_Un_S,
		OpCodes.Blt_Un_S,
		OpCodes.Br,
		OpCodes.Brfalse,
		OpCodes.Brtrue,
		OpCodes.Beq,
		OpCodes.Bge,
		OpCodes.Bgt,
		OpCodes.Ble,
		OpCodes.Blt,
		OpCodes.Bne_Un,
		OpCodes.Bge_Un,
		OpCodes.Bgt_Un,
		OpCodes.Ble_Un,
		OpCodes.Blt_Un
	};

	internal static bool IsBranch(this CodeInstruction instr) {
		return BranchCodes.Contains(instr.opcode);
	}

}

#endif
