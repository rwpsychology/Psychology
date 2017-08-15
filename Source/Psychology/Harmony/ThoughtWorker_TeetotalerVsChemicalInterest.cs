using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_TeetotalerVsChemicalInterest), "CurrentSocialStateInternal")]
    public static class ThoughtWorker_TeetotalerVsChemicalInterestPatch
    {
        [HarmonyPostfix]
        public static void Disable(ref ThoughtState __result, Pawn p, Pawn other)
        {
            if (p is PsychologyPawn && other is PsychologyPawn)
            {
                __result = false;
            }
        }
    }
}
