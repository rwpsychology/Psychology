using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Harmony;
using System.Reflection.Emit;

namespace Psychology.Harmony.Optional
{
    public static class PresetSaverPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SavePawnRef(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            CodeInstruction last = null;
            foreach (CodeInstruction itr in instrs)
            {
                /* Steal the CustomPawn to add its psyche to the dictionary, then load it again. */
                if (last != null && itr.opcode == OpCodes.Newobj && itr.operand == AccessTools.Constructor(typeof(EdB.PrepareCarefully.SaveRecordPawnV4), new Type[] { typeof(EdB.PrepareCarefully.CustomPawn) }))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PresetSaverPatch), "AddPsycheToDictionary", new Type[] { typeof(EdB.PrepareCarefully.CustomPawn) }));
                    if (last.opcode == OpCodes.Call)
                        yield return new CodeInstruction(OpCodes.Ldloca_S, 2);
                    yield return last;
                }
                yield return itr;
                last = itr;
            }
        }

        public static void AddPsycheToDictionary(EdB.PrepareCarefully.CustomPawn pawn)
        {
            if(SaveRecordPawnV4Patch.customPawns.ContainsKey(pawn.Id))
            {
                SaveRecordPawnV4Patch.customPawns.Remove(pawn.Id);
            }
            SaveRecordPawnV4Patch.customPawns.Add(pawn.Id, pawn);
        }
    }
}
