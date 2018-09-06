using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using Harmony;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(CharacterCardUtility), nameof(CharacterCardUtility.DrawCharacterCard))]
    public static class CharacterCardUtility_ButtonPatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddPsycheDisplay(IEnumerable<CodeInstruction> instr, ILGenerator gen)
        {
            int doNames = 0;
            foreach(CodeInstruction itr in instr)
            {
                yield return itr;
                if (itr.opcode == OpCodes.Call && itr.operand == typeof(CharacterCardUtility).GetMethod(nameof(CharacterCardUtility.DoNameInputRect)))
                {
                    doNames++;
                    if (doNames == 3)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CharacterCardUtility_ButtonPatch), "PsycheCardButton", new Type[] { typeof(Rect), typeof(Pawn) }));
                    }
                }
            }
        }

        [LogPerformance]
        public static void PsycheCardButton(Rect panelRect, Pawn pawn)
        {
            if(PsycheHelper.PsychologyEnabled(pawn))
            {
                Rect rect = new Rect(panelRect.xMax + 5f, 3f, 22f, 22f);
                Color old = GUI.color;
                if (rect.Contains(Event.current.mousePosition))
                {
                    GUI.color = new Color(0.97647f, 0.97647f, 0.97647f);
                }
                else
                {
                    GUI.color = new Color(0.623529f, 0.623529f, 0.623529f);
                }
                GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get("Buttons/ButtonPsyche", true));
                if (Widgets.ButtonInvisible(rect, false))
                {
                    SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    Find.WindowStack.Add(new Dialog_ViewPsyche(pawn));
                }
                GUI.color = old;
            }
        }
    }
}
