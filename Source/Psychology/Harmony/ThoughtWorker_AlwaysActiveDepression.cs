using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace Psychology.Harmony
{
    [HarmonyPatch(typeof(ThoughtWorker_AlwaysActive), "CurrentStateInternal")]
    public static class ThoughtWorker_AlwaysActivePatch
    {
        [LogPerformance]
        [HarmonyPostfix]
        public static void AlwaysActiveDepression(ref ThoughtState __result, Pawn p)
        {
            if (__result.StageIndex > 1)
            {
                if (p.health.hediffSet.HasHediff(HediffDefOfPsychology.Antidepressants))
                {
                    __result = ThoughtState.ActiveAtStage(1);
                }
            }
        }
    }
}
