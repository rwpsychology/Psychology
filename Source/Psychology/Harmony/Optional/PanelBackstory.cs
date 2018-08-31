using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;
using Harmony;

namespace Psychology.Harmony.Optional
{
    public static class PanelBackstoryPatch
    {
        [HarmonyPostfix]
        public static void AddPsycheEditButton(EdB.PrepareCarefully.PanelBackstory __instance, EdB.PrepareCarefully.State state)
        {
            Rect panelRect = __instance.PanelRect;
            Pawn pawn = state.CurrentPawn.Pawn;
            if (PsycheHelper.PsychologyEnabled(pawn))
            {
                Rect rect = new Rect(panelRect.width - 60f, 9f, 22f, 22f);
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
                    Find.WindowStack.Add(new Dialog_EditPsyche(pawn));
                }
            }
        }
    }
}
