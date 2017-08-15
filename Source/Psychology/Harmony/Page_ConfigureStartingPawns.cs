using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "DoWindowContents")]
    public static class Page_ConfigureStartingPawns_WindowPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddPsycheDisplay(IEnumerable<CodeInstruction> instr, ILGenerator gen)
        {
            LocalBuilder rect8 = gen.DeclareLocal(typeof(Rect));
            rect8.SetLocalSymInfo("rect8");
            foreach(CodeInstruction itr in instr)
            {
                int intOperand;
                if (itr.opcode == OpCodes.Ldc_R4 && int.TryParse("" + itr.operand, out intOperand) && intOperand == 200)
                {
                    itr.operand = 150f;
                }
                yield return itr;
                if(itr.opcode == OpCodes.Call && itr.operand == typeof(SocialCardUtility).GetMethod("DrawRelationsAndOpinions"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, rect8);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect),"x").GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect), "yMax").GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect), "width").GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 180f);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Constructor(typeof(Rect), new Type[] { typeof(float), typeof(float), typeof(float), typeof(float) }));
                    //Rect rect8 = new Rect(rect6.x, rect6.yMax, rect6.width, 200);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Font").GetSetMethod());
                    //Text.Font = GameFont.Medium;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, rect8);
                    yield return new CodeInstruction(OpCodes.Ldstr, "TabPsyche");
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Translator), "Translate", new Type[] { typeof(string) }));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Widgets), "Label", new Type[] { typeof(Rect), typeof(string) }));
                    //Widgets.Label(rect8, "Psyche".Translate());
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Text), "Font").GetSetMethod());
                    //Text.Font = GameFont.Small;
                    yield return new CodeInstruction(OpCodes.Ldloca_S, rect8);
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect), "yMin").GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Ldc_R4, 20f);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Rect), "yMin").GetSetMethod());
                    //rect7.yMin += 20f;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, rect8);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Page_ConfigureStartingPawns), "curPawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PsycheCardUtility), "DrawPsycheMenuCard", new Type[] { typeof(Rect), typeof(Pawn) }));
                    //PsycheCardUtility.DrawPsycheMenuCard(rect8, this.curPawn);
                }
            }
        }
    }
}
