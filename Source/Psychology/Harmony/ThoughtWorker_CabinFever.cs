using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_NeedOutdoors), "CurrentStateInternal")]
    public static class ThoughtWorker_CabinFeverPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OutdoorsyModifier(IEnumerable<CodeInstruction> instrs)
        {
            foreach (CodeInstruction itr in instrs)
            {
                yield return itr;
                float floatOperand;
                if(itr.opcode == OpCodes.Ldc_R4 && float.TryParse(""+itr.operand, out floatOperand) && floatOperand == 2.5f)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ThoughtWorker_CabinFeverPatch), "HasOutdoorsyTraitVal", new Type[] { typeof(Pawn), typeof(float), typeof(float) }));
                    yield return new CodeInstruction(OpCodes.Sub);
                }
                else if (itr.opcode == OpCodes.Ldc_R4 && float.TryParse("" + itr.operand, out floatOperand) && floatOperand == 7.5f)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 2.5f);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ThoughtWorker_CabinFeverPatch), "HasOutdoorsyTraitVal", new Type[] { typeof(Pawn), typeof(float), typeof(float) }));
                    yield return new CodeInstruction(OpCodes.Sub);
                }
            }
        }

        public static float HasOutdoorsyTraitVal(Pawn pawn, float val1, float val2)
        {
            return (pawn.story.traits.HasTrait(TraitDefOfPsychology.Outdoorsy) ? val1 : val2);
        }
    }
}
