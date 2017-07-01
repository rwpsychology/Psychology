using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(InspectPaneUtility), "DoTabs")]
    public static class InspectPaneUtility_ITabPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixTabs(IEnumerable<CodeInstruction> instr, ILGenerator generator)
        {
            var myLabel = generator.DefineLabel();
            int found = 0;
            int skipTwo = 0;
            foreach(CodeInstruction instruct in instr)
            {
                yield return instruct;
                int intOperand;
                if (instruct.opcode == OpCodes.Ldc_R4 && int.TryParse("" +instruct.operand, out intOperand))
                {
                    if (intOperand == 72)
                    {
                        found += 1;
                    }
                }
                else if(found == 2)
                {
                    skipTwo += 1;
                    if(skipTwo == 2)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_1); //ldloc.1 (num)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 0.0); //ldc.r4 0 (num < 0)
                        yield return new CodeInstruction(OpCodes.Bge_Un_S, myLabel); //bge (to nop)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 360f); //ldc.r4 360
                        yield return new CodeInstruction(OpCodes.Stloc_1); //stloc.1 (num = 360)
                        yield return new CodeInstruction(OpCodes.Ldloc_0); //ldloc.0 (y)
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 30f); //ldc.r4 30
                        yield return new CodeInstruction(OpCodes.Sub); //sub (y -= 30)
                        yield return new CodeInstruction(OpCodes.Stloc_0); //stloc.0 (y)
                        yield return new CodeInstruction(OpCodes.Nop) { labels = { myLabel } }; //nop
                    }
                }
            }
        } 
    }
}
