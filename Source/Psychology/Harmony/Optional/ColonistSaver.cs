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
    public static class ColonistSaverPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SavePawnRef(IEnumerable<CodeInstruction> instrs, ILGenerator gen)
        {
            CodeInstruction last = null;
            foreach(CodeInstruction itr in instrs)
            {
                /* Steal the CustomPawn to add its psyche to the dictionary, then load it again. */
                if (last != null && itr.opcode == OpCodes.Newobj && itr.operand == AccessTools.Constructor(typeof(EdB.PrepareCarefully.SaveRecordPawnV4), new Type[] { typeof(EdB.PrepareCarefully.CustomPawn) }))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PresetSaverPatch), nameof(PresetSaverPatch.AddPsycheToDictionary), new Type[] { typeof(EdB.PrepareCarefully.CustomPawn) }));
                    yield return last;
                }
                yield return itr;
                last = itr;
            }
        }
    }
}
