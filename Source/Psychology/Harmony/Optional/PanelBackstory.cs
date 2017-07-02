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
    public static class PanelBackstory
    {
        [HarmonyPostfix]
        public static void AddPsycheEditButton(EdB.PrepareCarefully.PanelBackstory __instance, EdB.PrepareCarefully.State state)
        {
            Rect panelRect = __instance.PanelRect;
            PsychologyPawn pawn = state.CurrentPawn.Pawn as PsychologyPawn;
            if (pawn != null)
            {
                if (pawn.psyche == null || pawn.psyche.PersonalityNodes == null)
                {
                    pawn.psyche = new Pawn_PsycheTracker(pawn);
                    pawn.psyche.Initialize();
                }
                foreach (PersonalityNode node in pawn.psyche.PersonalityNodes)
                {
                    if (node.rawRating < 0)
                    {
                        node.Initialize();
                    }
                }
                if (pawn.sexuality == null && PsychologyBase.ActivateKinsey())
                {
                    pawn.sexuality = new Pawn_SexualityTracker(pawn);
                    pawn.sexuality.GenerateSexuality();
                }
                Rect rect = new Rect(panelRect.width - 64f, 9f, 22f, 22f);
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
                    SoundDefOf.TickLow.PlayOneShotOnCamera(null);
                    Find.WindowStack.Add(new Dialog_EditPsyche(pawn));
                }
            }
        }
    }
}
