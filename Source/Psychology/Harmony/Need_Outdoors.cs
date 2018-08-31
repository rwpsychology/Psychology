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
    [HarmonyPatch(typeof(Need_Outdoors), "NeedInterval")]
    public static class Need_OutdoorsPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OutdoorsyModifier(IEnumerable<CodeInstruction> instrs)
        {
            foreach (CodeInstruction itr in instrs)
            {
                yield return itr;
                float floatOperand;
                if(itr.opcode == OpCodes.Ldc_R4 && float.TryParse(""+itr.operand, out floatOperand)
                    && (floatOperand == -0.32f || floatOperand == -0.45f || floatOperand == -0.4f)) 
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Need_Outdoors), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 0f);
                    if (floatOperand == -0.32f)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.6f);
                    else if (floatOperand == -0.45f)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.7f);
                    else if (floatOperand == -0.4f)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.7f);
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Need_OutdoorsPatch), "HasOutdoorsyTraitVal", new Type[] { typeof(Pawn), typeof(float), typeof(float) }));
                    yield return new CodeInstruction(OpCodes.Sub);
                }
            }
        }

        public static float HasOutdoorsyTraitVal(Pawn pawn, float val1, float val2)
        {
            return (pawn.story.traits.HasTrait(TraitDefOfPsychology.Outdoorsy) ? val2 : val1);
        }
    }
}
