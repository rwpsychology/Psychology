using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class PawnComponentsUtility_CreateInitialPatch
    {
        [HarmonyPostfix]
        public static void CreatePsychologyComponents(Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike)
            {
                PsychologyPawn realPawn = pawn as PsychologyPawn;
                if (PsychologyBase.ActivateKinsey())
                {
                    if (realPawn != null)
                    {
                        realPawn.sexuality = new Pawn_SexualityTracker(realPawn);
                    }
                }
                if (realPawn != null)
                {
                    realPawn.psyche = new Pawn_PsycheTracker(realPawn);
                }
            }
        }
    }
}
