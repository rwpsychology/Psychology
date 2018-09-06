using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_CreepyBreathing), "CurrentSocialStateInternal")]
    public static class ThoughtWorker_CreepyBreathingPatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void Disable(ref ThoughtState __result, Pawn pawn, Pawn other)
        {
            if (PsycheHelper.PsychologyEnabled(pawn) && PsycheHelper.PsychologyEnabled(other))
            {
                __result = false;
            }
        }
    }
}
