using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_Pretty), "CurrentSocialStateInternal")]
    public static class ThoughtWorker_PrettyPatch
    {
        [HarmonyPostfix]
        public static void Disable(ref ThoughtState __result, Pawn pawn, Pawn other)
        {
            if (pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight) == 0f)
            {
                __result = false;
            }
            if (RelationsUtility.IsDisfigured(other))
            {
                __result = false;
            }
            if (pawn is PsychologyPawn)
            {
                __result = false;
            }
        }
    }
}
